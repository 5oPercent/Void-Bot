# ERA 技能配置与配装接口

本模块沿用 Controller → Mediator → Handler → Service → DataProvider 分层。Command 由现有工作单元提交，Request 不创建数据。数据库结构由 `Script0026_init_era_skills.sql` 建立，`Script0028_scope_era_skills_to_characters.sql` 将配装和学习记录改为角色归属，本次未执行迁移或启动服务。新增草稿编辑和统一发布见 [技能草稿管理接口](ERA技能草稿管理接口.md)及[实施清单](ERA技能编辑与发布TODO.md)。

## 数据

- `era_skill_configuration`：固定单行保存游戏已发布快照，初始版本 0；编辑数据位于独立共享草稿，显式发布冻结草稿后原子替换快照。
- `era_character_skill_profile`：每角色一份版本化配装，JSON 内保存当前体系与各体系四槽记忆；锁玩家行、角色行后保存，初始读取版本 0。
- `era_character_learned_skill`：角色与稳定技能 ID 唯一。授予操作锁玩家行、角色行，重复授予不增加记录。

JSON 配装避免当前四槽结构增加大量拆表与单槽接口；服务层完整校验每条体系记录。将来增加独立查询需求时再迁移为分行记录。

## 接口

| 方法与路径 | 用途 |
| --- | --- |
| GET `api/era/manifest/catalog` | 完整体系技能池、方案、BUFF、通用战术池、禁用列表与版本 |
| GET `api/era/characters/{characterId}/ability-systems/loadouts` | 当前体系与全部保存的体系配装 |
| POST `api/era/characters/{characterId}/ability-systems/loadouts` | 使用 `ExpectedVersion` 与完整 `Profile` 保存配装／当前体系 |
| GET `api/era/characters/{characterId}/skills/learned` | 本人已学习技能 ID 列表 |
| POST `api/era/admin/configuration/publish` | 管理员发布已保存草稿，携带两个预期版本与请求 ID |
| `/api/era/admin/ability-systems/*`、`/skills/*`、`/buffs/*` | 三类独立草稿编辑，见专用文档 |
| `/api/era/admin/configuration/*` | 共同设置、导入、预览、发布、历史与恢复 |
| POST `api/era/admin/characters/{characterId}/learned-skills` | 管理员授予一个技能，正文含 `SkillId` |

沿用登录认证、ERA 客户端会话和现有权限检查。管理接口不标记跳过权限检查。

保存校验归属、体系与技能存在、已学习、池与槽位匹配、每体系四槽不重复及配装版本，不校验禁用状态。禁用技能可保存，局内是否释放由主机固定本局配置判定。

后端不保存冷却、载体、BUFF 实例或本局任务快照，不参与每次释放。BUFF 只配置文字与业务参数，不包含图标或表现样式。

## 联调准备

1. 由正常发布流程应用 DbUp 迁移。
2. 应用新的草稿迁移后显式初始化草稿，将 `examples/era-skills-draft-import.json` 导入草稿，再预览并按 `examples/era-skills-draft-publish.json` 结构发布；填写实际版本和新的非空请求 ID。
3. 为测试角色授予拟保存的技能。游戏端当前没有学习 UI，也不进行学习筛选；后端保存校验仍保留。
4. 游戏菜单打开技能配装，加载并保存；本局禁用判定来自主机快照。

示例配置仅用于联调，不会自动发布或自动授予。配置类型与游戏端 `scripts/abilities/contracts` 对应，枚举使用显式数值。

技能 ID 不应随数值或表现调整改变。禁用通过 `DisabledSkills` 表达，避免删除定义破坏学习与历史配装引用。整套配置限制为 256 KiB，当前结构版本为 1。

## 验证状态

新增 `EraSkillServiceFixture`，覆盖禁用已学习技能可保存、未学习拒绝、配装版本冲突、读取不写入和发布冲突。仅进行了静态解析，未运行测试或编译项目。

## 代码组织与写法

- DTO：`Void.Bot.Message/Dto/ERA`，每个 DTO 独立文件，类型及所有属性包含 XML 注释。
- 枚举：`Void.Bot.Message/Enum/ERA`，每个枚举独立文件，类型及所有成员包含 XML 注释，协议数值保持不变。
- Command／Request：分别放在 `Message/Commands/ERA` 和 `Message/Request/ERA`，每个消息文件包含其配套 Response。
- Handler：按 Command／Request 目录拆分，一个消息对应一个独立 Handler 文件。
- Validator：`Core/Validators/ERA`，每个命令校验器独立文件。
- Entity：`Core/Domain/ERA`，每个持久化实体独立文件。
- Service：`EraSkillService.cs` 保留公共依赖与上下文；按配置、配装、学习拆为 `.Configuration`、`.Profile`、`.Learning`，接口和实现同主题放在同一分片。
- DataProvider：`Core/Services/ERA/EraSkillDataProvider.cs`，保持单一数据访问入口，不提交事务。
- 映射：`Core/Mappings/EraSkillMapping.cs`，使用 AutoMapper 将 JSON 存储实体转换为消息 DTO。

技能模块后端使用普通构造函数、完整方法体和带大括号的条件／循环，不使用主构造函数、表达式方法或隐式 `new()`。本次拆分不改变 API 字段、枚举值或数据库结构。
## 通用 BUFF 边界

BUFF 定义使用 `BuffDefinitionDto`、`BuffEffectDto`，生命周期与效果枚举使用 `BuffKind`、`BuffEffectKind`，仍按 DTO／Enum 目录分文件并保留完整 XML 注释。字段、默认值和枚举协议数值未改变，无需迁移已有 JSON 载荷。

`Core/Validators/ERA/BuffDefinitionValidator.cs` 独立校验 BUFF，不依赖技能配置。技能发布校验器调用该通用校验，再检查技能方案的 BUFF ID 引用；装备、物品、环境配置也可复用。当前发布接口仍整批提供技能与相关 BUFF，以保持本局快照一致。

游戏端统一入口为 `scripts/abilities/buffs/BuffRuntime`，载体通过角色装配层调用。新增未执行的 `BuffDefinitionValidatorFixture`，覆盖物品治疗可独立校验、周期间隔无效和即时效果不接受持续属性修正。

## ERA 接口与玩家预加载规范

- 读取技能配置、配装、已学习技能的 GET 接口显式绑定 `[FromQuery] XxxRequest request`，Handler 将完整消息传递给 Service。
- Handler 访问的 ERA Service 方法统一为 `Task<XxxResponse> MethodAsync(XxxRequest request / XxxCommand command, CancellationToken cancellationToken)`。
- 需要当前玩家的消息实现 `IRequireEraPlayer`；管理员指定目标玩家的消息实现 `IRequireEraTargetPlayer`，通过 `PlayerId` 指定目标。中间件按接口判断，不按路由名称判断。
- `EraPlayerSpecification` 在工作单元之后执行：请求只读加载，命令在事务内加载并锁定玩家。未登录和玩家不存在统一提前抛出业务异常，保留 `era.unauthorized` / `era.player_not_found` 错误码。
- `EraPlayerData` 保存当前请求已加载的玩家；`IEraPlayerContext` 统一加载和缓存，同一请求复用结果，读操作升级写操作时重新取得行锁。
- 初始化玩家、公共配置读取和发布、客户端会话关闭与心跳不要求玩家档案已存在。管理员目标玩家操作不要求管理员自身先初始化玩家。
- 技能本地化模板只包含技能错误，玩家不存在使用通用 `EraContextTemplate.PlayerNotFound`。

## 编辑与删除补充规则

体系、技能与 BUFF 保存到共享草稿，不需要手工提交完整配置。显式发布才生成新的完整版本；新请求即使内容未变化也增加版本，相同请求重试返回原结果。删除不改角色配装、保留学习记录，客户端同步后清空失效槽位并取消无效体系选择。没有默认替换技能或自动选中另一个体系。旧整包直发管理接口与消息已移除。
