# ERA 官方 API

开发分支：feature/era-backend。仅 HTTP API，游戏客户端通过 HTTPS/JSON 交互，玩家主机继续使用 ENet 直连。没有 Steam、中继、装备库存操作、属性点、突袭或结算功能。

## 认证与权限

复用 POST /api/Identity/login、POST /api/Identity/refresh-token 和现有注册接口，使用现有 Bearer token。首版没有必要增加另一套客户端令牌；以后可扩展专属 audience 和 scope，但不能将客户端令牌当作管理员权限。

玩家接口使用 Authorize，跳过管理端路由权限检查；账号从 ICurrentUser 取得，初始化和查询均不接受外部账号 ID。管理接口沿用项目现有的 Controller 权限与业务权限体系，ERA Service 不再重复检查超级管理员或端点权限。新增路由由既有端点扫描机制发现，部署后按现有权限 API 为运营角色分配对应端点。

## 接口

所有 API 都以对应的 Request 或 Command 作为参数入口，并通过 Mediator 返回配套 Response。即使当前没有业务字段，也保留空 Request/Command，Controller、Handler、Service 全链路传递同一个消息对象，便于后续兼容扩展。

| 方法 | 路径 | 功能 |
| --- | --- | --- |
| GET | /api/era/manifest | 读取当前已发布配置 |
| GET | /api/era/players/me | 读取本人玩家记录，未初始化返回 player_not_found |
| POST | /api/era/players/me/initialize | 幂等初始化本人记录 |
| GET | /api/era/characters | 读取本人档案下的全部角色 |
| GET | /api/era/characters/{characterId} | 读取本人指定角色 |
| POST | /api/era/characters | 幂等创建角色 |
| POST | /api/era/characters/{characterId}/delete | 禁用角色 |
| GET | /api/era/admin/players/{playerId} | 官方查询玩家容量和版本 |
| POST | /api/era/admin/config | 以 ExpectedVersion 校验并发布配置 |
| POST | /api/era/admin/players/{playerId}/warehouse-capacity | 设置容量 |
| POST | /api/era/admin/players/{playerId}/warehouse-capacity-adjustments | 增减容量 |

沿用 VoidBotResponse 响应包装与统一过滤器；客户端应检查 Code，不能只判断 HTTP 状态。ERA 业务失败同时带 ErrorCode，例如 era.version_conflict、era.player_not_found、era.idempotency_conflict。权限过滤器和通用参数校验仍采用既有错误包装。

`era_configuration` 每行保存一个当前配置项，包括 ConfigKey、ValueType、Value、Description 和 ConfigVersion。配置发布请求包含 ExpectedVersion 和完整 Items 集合；ConfigVersion 由后端控制，忽略请求中的配置版本。新增普通配置项只需插入或通过发布接口提交新键，不需要修改表结构。三个基础属性默认 0，范围 0～100，基础生命值 125；满力量攻击加成 50%、满体质生命加成 80%、满敏捷移速加成 10%，默认仓库容量 300。所有值来自数据库，未在玩家记录保存最终属性。

配置百分比以百分数表示：50 对应 50%，不是 0.5。客户端先汇总装备提供的属性并限制到配置范围，再根据 (最终属性 - 下限)/(上限 - 下限) 计算满属性收益比例，随后应用派生值 BUFF 修正。基础规则可能变化，客户端应明确支持并验证配置，不应静默忽略不兼容规则。当前 ERA 游戏工程尚未接入 HTTP 配置，仍需后续客户端适配。

每次发布会锁定 `era_configuration` 的全部当前记录，在同一事务中增删改配置项，并为全部保留项统一写入递增后的 ConfigVersion。表中永远只保留最新配置，不创建状态表、版本表或历史快照。客户端发现服务端 ConfigVersion 变化后重新获取完整配置；多人对局是否立即采用新版本由客户端同步流程决定，同一对局中的各端必须使用相同版本。

默认容量变更只影响新玩家，重复初始化不会覆盖现有容量。官方通过容量接口单独调整已有玩家。请求包含 requestId、expectedVersion、reason，以及 capacity 或 delta；支持容量 0，负数或溢出结果拒绝。管理接口中的 PlayerId 是 ERA 玩家档案 ID，使用路由值覆盖请求体值。重试同一操作必须携带相同请求 ID 和原始参数，成功后返回原始回执；更换参数复用 ID 返回冲突。

ERA 玩家档案使用独立 ID，`era_player` 本身不保存主账号字段。`era_player_account_link` 关联表保存 Void.Bot AccountId 与 ERA PlayerId 的一对一绑定，并分别对两列建立唯一约束。仓库容量以及未来的仓库物品都归属 PlayerId，因此同一账号下的所有角色天然共享仓库。角色记录使用独立 CharacterId，并保存 CharacterDefinitionId、DisplayName 和并发 Version；未来角色装备、技能配置及进度应引用 CharacterId，不应把共享仓库复制到角色记录。

创建角色请求携带 RequestId，同一玩家重复提交相同请求和参数时返回原角色，使用相同 RequestId 提交不同参数时返回 idempotency_conflict。当前只保存已明确的角色标识与显示配置，不提前存储未经验证的任意 JSON；新增外观、技能等字段时继续使用强类型 Command、DTO 和数据库列。

## 持久化与并发

DbUp Script0023 创建 era_configuration、era_player、era_player_account_link、era_character 和 era_capacity_change，初始配置从 SQL 种子进入数据库。没有在开发期间执行迁移；现有 Program 启动会自动运行 DbUp，部署前需审查迁移和备份。

配置更新锁定全部当前配置行并检查整套配置版本，在同一事务中替换为最新完整集合。玩家初始化锁定主账号行，随后通过关联表检查已有玩家，并在同一事务中创建 ERA 玩家与账号绑定；关联表双向唯一约束进一步防重。角色创建锁定所属玩家并检查创建请求 ID。容量变更锁定玩家，当前读取幂等回执，版本检查后同时更新容量和插入审计回执。普通 Command 使用 Mediator 最外层事务统一提交，没有 Service 手动 SaveChanges。相关接口应通过 Mediator 使用，不能直接把 Service 当成独立事务边界。

ERA 玩家档案拥有独立主键，账号绑定只存在于独立关联表中。玩家档案保存共享仓库容量、版本与审计时间；角色通过 PlayerId 归属玩家档案。未来仓库物品独立建表并引用 PlayerId，角色已装备物品或技能配置引用 CharacterId；容量降低不删除物品，未来库存模块决定超限时存入规则。配置值使用明确的 ValueType 和不变格式字符串，不存储未经验证的任意 JSON；后端校验核心配置键，同时允许追加受支持类型的新配置项。

## Blazor

Api 本身没有托管 Blazor 的回退页面，Dockerfile 也仅发布 Api。解决方案保留 Blazor 源码但取消 Debug/Release 默认构建选项；原有 Scalar API 文档、Hangfire 和其他 Bot 功能保持原样。无需删除前端项目及用户配置。

## 验证状态

按当前要求不编译、不运行测试、不启动服务、不执行数据库迁移。新增容量幂等、并发版本、负容量和玩家隔离的单元测试供后续执行。SQL 与 EF 映射以及接口/Handler/DI/Validator 接入做静态检查。MySQL 锁行为、账号认证、实际迁移及 Windows/Linux 客户端接入仍需集成验证。

删除角色会锁定所属玩家和目标角色，校验当前账号归属，将 IsDisabled 设为 true 并递增角色版本，在同一事务中提交。保留角色记录及 PlayerId 关联，不影响共享仓库和主账号绑定。本人角色列表和详情查询一并返回禁用角色，并通过 IsDisabled 标识状态，由客户端决定是否排除；重复删除本人已禁用角色返回成功，不重复递增版本。
