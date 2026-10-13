# VoidBot 好友与 ERA 组队后端

## 范围与验证

好友归属 VoidBot AccountId；ERA PlayerId 和 CharacterId 仍是独立身份。实现 HTTP 查询与业务操作、SignalR 实时失效通知和 ERA 临时邀请。没有公开队伍大厅，没有 ERA UI 改动，也没有新增 Python 项目依赖。

此实现仅做静态检查，不编译、不运行测试、不启动服务、不执行数据库迁移。语法解析不等同于完整编译；数据库锁行为、Redis Lua、SignalR 浏览器/.NET 客户端及容量需要后续验证。

## 分层与事务

Controller → Mediator → Handler → Service → DataProvider；每项操作有独立 Request/Command、Response、Handler 和 Validator。服务遵循现有 IScope/ISingleton 自动注册。Web/ERA Hub、通知传输与后台任务在 API 服务注册。

好友 Command 复用 Mediator 事务，主账号按规范化 ID 顺序加锁。申请处理锁后读取当前状态，接受申请与创建关系、写通知发件箱同事务完成。涉及关系的社交和入队 Command 使用 ReadCommitted，不依赖锁前快照。好友关系规范化为 FirstAccountId/SecondAccountId，唯一索引防重。

Script0025_init_social.sql 新建 friend_request、friendship、account_block、social_notification_outbox。没有执行脚本；项目既有启动流程会运行 DbUp，部署前审查脚本并备份。

## 通用 HTTP

全部需要有效 Bearer 账号身份；新社交接口额外核验 token_version。响应保持现有 Code/Message 包装，客户端必须检查响应 Code，不能只检查 HTTP 200。稳定 ErrorCode 定义在公共 VoidBotResponse 基类中，社交响应直接继承 VoidBotResponse<T>；ErrorCode 为空时不输出该字段。

| 方法 | 路径 | 参数/用途 |
| --- | --- | --- |
| GET | /api/social/lookup | displayName，完整名称#四位后缀；只返回公开资料 |
| GET | /api/social/friends | pageIndex、pageSize |
| GET | /api/social/requests | pageIndex、pageSize、outgoing；含申请历史和状态 |
| GET | /api/social/blocks | pageIndex、pageSize |
| POST | /api/social/requests | targetAccountId、requestId |
| POST | /api/social/requests/{friendRequestId}/accept | 接受申请，JSON 空对象 |
| POST | /api/social/requests/{friendRequestId}/reject | 拒绝申请，JSON 空对象 |
| POST | /api/social/requests/{friendRequestId}/withdraw | 撤回本人申请，JSON 空对象 |
| POST | /api/social/friends/{targetAccountId}/remove | 删除好友，JSON 空对象 |
| POST | /api/social/blocks | targetAccountId |
| POST | /api/social/blocks/{targetAccountId}/remove | 解除拉黑，JSON 空对象 |

路由覆盖请求体中的目标 ID。分页上限 100，默认 20；好友上限 1000，单账号参与的待处理申请上限 100。数据库执行分页并批量读取账号资料，不返回密码、邮箱、安全字段或第三方凭证。双方互相申请返回 incoming_request，不自动接受。拉黑删除现有关系并取消待处理申请；解除拉黑不自动恢复好友。

申请状态：Pending=0、Accepted=1、Rejected=2、Withdrawn=3、Cancelled=4。相同申请处理重复提交幂等，不允许将已处理申请改为其他状态。发送申请的 requestId 对发送者唯一，复用到不同目标返回 idempotency_conflict。

## ERA 权限

JoinMode：Public=0、HostFriends=1、InvitationOnly=2、Solo=3。

| 模式 | 主动加入 | 邀请加入 |
| --- | --- | --- |
| Public | 通过队伍码或好友入口尝试加入 | 允许 |
| HostFriends | 当前主机的好友 | 允许 |
| InvitationOnly | 禁止 | 允许 |
| Solo | 禁止 | 禁止 |

InvitationPermission：AnyMember=0、HostOnly=1，AnyMember 仅指本队有效成员。修改设置仅限当前主机，队伍迁移期间不可修改。多人时禁止切换 Solo。主机转让后按新主机账号判断主动加入资格。

旧队伍缺少字段时默认 Public、HostOnly、SocialVersion=0。现有创建队伍接口也支持 joinMode、invitationPermission，省略时使用同样默认值。队伍码 lookup/join/switch 全部执行加入策略，不允许绕过 InvitationOnly/Solo。

## ERA HTTP

下列接口均需要 Bearer 和有效 X-ERA-Client-Id，遵循现有独占游戏会话机制。

| 方法 | 路径（前缀 /api/era/social） | 参数/用途 |
| --- | --- | --- |
| GET | /friends/statuses | accountIds，可重复 query 参数，最多 100 个；仅返回当前好友 |
| GET | /friends/{friendAccountId}/team | 好友在线和队伍摘要 |
| POST | /friends/join | friendAccountId、expectedTeamId、characterId，可选 sourceTeamId/sourceMemberId |
| POST | /teams/{teamId}/settings | memberId、expectedVersion、joinMode、invitationPermission |
| POST | /invitations | teamId、memberId、targetAccountId、requestId |
| GET | /invitations | outgoing，默认接收到的有效邀请；上限 100 |
| GET | /invitations/{invitationId} | query teamId，查询本人发送/接收的指定邀请 |
| POST | /invitations/{invitationId}/withdraw | teamId |
| POST | /invitations/{invitationId}/reject | teamId |
| POST | /invitations/{invitationId}/accept | teamId、characterId，可选 sourceTeamId/sourceMemberId |

已有队伍时 sourceTeamId/sourceMemberId 必须成对提供，使用现有双队伍原子切换；无队伍时省略。expectedTeamId 固定目标队伍，好友换队或队伍码被复用时返回冲突。只读查询不离队、不刷新队伍 TTL、不暴露队伍码或打洞信息。真正入队后通过原有准备/对端查询接口获取联机凭证。

邀请必须发给好友。有效期 120 秒，不预占名额；每队最多保留 128 条邀请/回执，单成员每分钟最多创建 10 条。处理回执保留到邀请过期后 10 分钟，但队伍解散/过期会更早删除。发送 requestId 的幂等范围为队伍＋发送者。

邀请状态：Pending=0、Accepted=1、Rejected=2、Withdrawn=3、Expired=4、Invalidated=5。查询返回有效状态；接受重新校验邀请、好友关系 ID、主机拉黑关系、发送者成员会话、邀请权限、队伍模式、容量、迁移状态、角色归属。删除后重新加好友不会恢复旧邀请。接受回执绑定新 MemberId 与 CharacterId；玩家离队后旧邀请不能再次用于入队。

邀请与队伍保存在同一 Redis JSON，通过原有 CAS/Lua 提交。入队/切队与邀请消费、邀请索引和通知记录一起提交；Lua 使用 Redis TIME 再次检查邀请到期。数据库好友关系与 Redis 队伍不能构成跨存储 ACID 事务：入队期间通过账号行锁保护关系，Redis 回执支持响应丢失后的恢复。后续集成验证需覆盖数据库提交异常和 Redis 超时。

## SignalR

| 路径 | 身份 | 范围 |
| --- | --- | --- |
| /hubs/web/social | Bearer＋账号 token_version | 通用好友通知 |
| /hubs/era/social | 上述身份＋有效 ERA clientId | 通用好友通知＋ERA 状态/邀请通知 |

ERA 每个客户端只连接 ERA Hub。游戏 clientId 优先读取 X-ERA-Client-Id，也支持 query client_id。浏览器 access_token query 仅在这两个 Hub 路径接受；其他 HTTP 路径仍使用 Authorization 请求头。WebSocket 代理需转发 Upgrade，部署日志不能记录含 access_token 的完整 URL。

两端接收事件名 SocialChanged。事件包含 eventId、eventType、resourceId、parentResourceId、resourceVersion、occurredAt、correlationId。parentResourceId 对邀请为 TeamId，可直接通过 HTTP 查询指定邀请；不包含完整好友列表、队伍成员网络端点或凭证。

| eventType | resourceId | 客户端处理 |
| --- | --- | --- |
| social.friend_request_changed | 申请 ID | 合并刷新申请分页 |
| social.friendship_changed | 关系变化标识 | 刷新好友，并刷新相关 ERA 状态/邀请 |
| social.block_list_changed | 被屏蔽账号 AccountId | 刷新自己的屏蔽列表 |
| era.friend_presence_changed | 好友 AccountId | 合并批量刷新对应好友状态 |
| era.team_invitation_changed | 邀请 ID；parentResourceId=TeamId | 查询邀请或刷新邀请列表 |

首次连接/重连：先连接并缓冲通知，再 HTTP 查询快照，最后处理缓冲通知。通知仅作失效提示，不能作为业务授权或完整事件历史。客户端按 eventId 去重、合并查询，并用请求代次避免旧 HTTP 响应覆盖新结果；resourceVersion 只在同一资源/事件类型中比较，不是好友列表的全局快照版本。列表同步需要按分页取全，或标记未加载分页失效。

客户端显式配置自动重连，首次 StartAsync 失败也需退避重试；加入随机抖动。凭证刷新后重新连接。服务端凭证到期关闭连接，每 10 秒核验账号版本和 ERA 会话；检查从不续期游戏会话。客户端登出立即主动关闭连接；无法联系客户端时由凭证或会话失效终止连接。

## 通知可靠性与负载

数据库发件箱每秒读取最多 128 条，最多尝试 8 次，指数退避上限 300 秒。已完成记录保留 1 天，所有记录最多保留 7 天。后台发送成功不代表客户端已收到或已读；离线数据通过 HTTP 恢复。

Redis 邀请通知每秒读取最多 128 条，最多尝试 8 次；失败重试期间保留。队列上限 10000，达到上限时淘汰最早的待发失效通知；邀请业务状态仍保留，客户端通过查询恢复。因此不承诺离线消息或无限历史可靠投递。

游戏在线索引由账号会话心跳续期，主动退出立即移除，失联在现有 30 秒租约到期后离线。后台每 5 秒按 128 个账号批量读取公开状态，只在上线、离线、成员/主机/模式/权限/迁移状态变化时通知好友。通知通道不是在线依据，不比较 TTL、打洞端点或令牌变化。

配置：Social:MaximumConnections 默认 5000，每账号最多 4 条连接；这是资源保护上限，不是已验证的服务器容量。Social:AllowedWebOrigins 配置可信 Web 来源数组，未配置只使用同源 Web。HTTP 社交限流按账号 10 秒 30 次，与客户端查询合并配合使用。

通过 Meter Void.Bot.Social 暴露 social.connections.active、social.notifications.sent、social.notifications.failed、social.notifications.delay；结合应用进程内存、HTTP 延迟、Redis/MySQL 指标和失败日志评估负载。当前单实例，不配置 Redis backplane，也不声称已经验证 1000 人承载能力。

## 后续验证清单

- 编译与依赖注入启动验证（需另行授权）。
- 数据库迁移、好友双向申请、重复接受、并发删除/加入及锁顺序。
- Redis 邀请消费与双队切换、满员、迁移、队伍解散和到期。
- 接受响应丢失后重试、离队后旧邀请重试、删除再加好友。
- Web/.NET 客户端认证、跨域、代理 Upgrade、凭证刷新和会话失效。
- 首次同步期间收到通知、重复通知、断线重连和旧 HTTP 响应。
- 100/200/500/1000 人：连接＋既有心跳＋通知＋HTTP 查询，另测集中重连。
- 接口验证完成后再讨论 ERA 界面设计。

## 静态复查记录

已核对 21 个新增 HTTP 操作的消息、处理器、验证器、服务与路由对应关系，以及两个 SignalR 端点的注册和鉴权。事务隔离级别通过 TransactionIsolationAttribute 在命令上声明，公共事务中间件不引用好友或 ERA 命令类型。单个好友与批量好友状态查询使用同一实现；主机已失效时摘要不标记可加入。屏蔽与解除屏蔽会通知账号自己的其他连接刷新屏蔽列表。

上述为静态检查范围，尚未完成编译、启动、SQL/Redis 执行及客户端联调，不能视为已验证上线可用。


## 对端变化通知

队伍保存独立的 `PeersVersion` 与对端状态摘要，旧 Redis 数据默认版本 0，首次有效提交初始化。摘要包含成员、连接尝试、主机代次、迁移状态、地址候选和票据/端点有效性，不比较续期时间或凭证明文。UDP 登记、队伍变化及每 5 秒的过期清理发现业务变化时才增加版本；peers 查询仍只读，不续期或写 Redis。

成功 CAS/Lua 提交将 `era.team_peers_changed` 通知与队伍状态原子写入独立发件箱。事件沿用 `SocialChanged`，`resourceId=TeamId`、`resourceVersion=PeersVersion`、`authorityEpoch` 为主机代次；HTTP peers 快照增加 `peersVersion`。通知不包含地址或凭证。每队只保留最新通知，250ms 合并，队列上限 10000，最长保留 2 分钟，失败最多重试 8 次；旧发送确认不能删除并发写入的新通知。

后台将发件箱发布到 Redis 频道 `era:peers:notifications:v1:{RedisDatabaseIndex}`，所有 API 实例订阅后分别投递本机 SignalR 连接，避免单实例连接索引漏掉其他实例。投递时重新检查当前队伍成员和有效 ERA 客户端会话。退出、移出或解散后的会话由既有队伍心跳处理成员失效，不向非成员投递端点通知。Redis 指标 `era:peers:metrics:v1` 记录排队、合并及容量淘汰，SignalR 沿用发送数量、失败和延迟指标。

Redis 发布成功不代表客户端收到；本机队列上限 1024，过载可丢弃旧失效提示。首次连接、重连、低频保底、端点到期及连接异常通过 HTTP 补查，不能把通知作为业务授权或离线判断。各实例使用相同 Redis 与数据库设置，当前原子脚本沿用单 Redis 部署限制。

本次只做静态检查，未启动服务、执行测试或部署；多实例分发及流量收益仍需运行验证。
