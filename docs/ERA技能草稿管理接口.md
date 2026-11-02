# ERA 晶格、谐振技能与效果草稿接口

更新：2026-11-02。当前固定契约 SchemaVersion=3；已完成源码调整与隔离 SQL 迁移验证，未编译、运行真实 HTTP 或修改业务数据库。参数模板、字段库及公开组合载体契约已删除。

## 数据与权限

管理根路径为 /api/era/admin，需 Bearer 及对应管理权限。保存、标记删除、设置、导入、历史恢复均只改共享草稿；显式发布整份配置。普通 GET 不初始化或写数据。

晶格、谐振技能和效果继续使用原配置／草稿／历史表及既有接口 URL。领域名为 Lattice／Resonance，传输集合仍为 Systems／Skills／Buffs，不创建第二套存储。效果的局内计时和结算仍由游戏运行时负责。

## 端点

| 方法／相对路径 | 用途 |
| --- | --- |
| POST /configuration/initialize | 从已发布配置幂等初始化草稿，不覆盖已有草稿 |
| GET /configuration | 修订号、正式版本、设置版本及工作区状态 |
| GET /configuration/export | 完整草稿导出，可含待补齐引用 |
| GET /ability-systems、/skills、/buffs | Skip、Take（1～100）、Search、IncludeDeleted 分页 |
| GET /skills/{id}（另两类同样） | Definition、Version、DraftRevision、IsDeleted、WasPublished |
| POST /skills/{id} | ExpectedVersion＋Definition，创建为版本 0，更新用实际读取版本 |
| POST /skills/{id}/delete、/restore | ExpectedVersion，标记删除／显式恢复 |
| GET／POST /configuration/settings | 管理设置，写入携带 ExpectedVersion＋Settings |
| GET /configuration/references | Kind=0 晶格、1 谐振、2 效果，Id、Skip、Take |
| POST /configuration/validate、/publish-preview | 从已保存草稿校验、生成变更及发布预览 |
| POST /configuration/publish | ExpectedDraftRevision、ExpectedPublishedVersion、RequestId |
| POST /configuration/import | ExpectedDraftRevision、Mode、Configuration，仅导入草稿 |
| GET /configuration/history、/history/{version} | 历史分页／完整配置 |
| POST /configuration/history/{version}/restore | ExpectedDraftRevision，恢复到草稿，之后显式发布 |

路由 Id 必须与 Definition.Id 一致；已删除身份不可重新创建，需显式恢复。单项结构有效即可保存未完成效果引用，发布时全部引用须齐全。新谐振不会自动入池、授予学习记录或装配给角色。

## 固定谐振定义

ResonanceDefinition 仅 Id、Name、Description、Variants。每技能 1～4 个唯一槽位（Special=0、Primary=1、Secondary=2、Tactical=3），战术无特殊必填规则。

| Variants 字段 | 语义与范围 |
| --- | --- |
| MaximumCharges／CooldownSeconds | 次数 1～100；CD 0～86400 秒 |
| RecoveryStart | Released=0 释放后；CarrierEnded=1 载体结束（名称以现有枚举为准） |
| CastMode | Pressed=0、Released=1、Confirm=2；长按由客户端输入绑定实现 |
| MaximumDistance | -1 不限；否则 0～1000000 |
| AllowMovement | 准备、预览、等待出手期间是否可移动，释放后解除 |
| ActionKey | 空字符串为无；非空不超过 128 字符且无首尾空白，客户端关联动作 |
| CarrierKind | Self=0、Entity=1、Construct=2、Area=3 |
| DurationSeconds／TriggerIntervalSeconds | 构造物／区域为 0.001～86400 秒；自身／实体必须为 0 |
| ExecutionDelaySeconds | 0～86400 秒，生成后延迟首次自身逻辑，默认 0 |
| Condition | 默认种族阵营条件树 |
| Effects | 最多 64 项，每项 EffectId 及可空 Condition；空数组表示无给予效果 |

延迟从生成计时，存在时间也从生成计时。默认立即开始自身逻辑，随后按间隔；到期不执行。延迟不小于寿命时不会执行。构造物可能发射携带效果的子弹，公共层不自动替构造物给予；何时命中给予由客户端实现。区域形状、半径、构造物外观与运动不属于服务端配置。

Effects 引用现有 BuffDefinitionDto，不复制治疗／伤害数值或属性持续时间。Condition=null 继承槽位默认；非空完整替换默认，不是附加 AND。比如默认敌对，伤害继承，治疗显式设友方，可在同一区域分别生效。效果的寿命、叠加、刷新和撤销只由效果配置定义，载体结束不截短已给予效果。

## 效果类型与周期叠加

BuffKind 仅 Instant=0、Periodic=2，原 Persistent=1 删除。周期允许混合属性修正、伤害、治疗、防护恢复；即时不持有实例，不能配置需到期撤销的属性修正。

IntervalSeconds=-1 表示不重复，每次给予时结算一次并按规则持有属性。正数范围 0.001～86400，首次在一个间隔后触发。DurationSeconds=0 表示永久，存续时间与执行间隔独立。

周期触发也遵循 StackRule：刷新仅刷新；叠层并刷新增加 1 层至 MaxStacks 并刷新，到上限后仍刷新，再以更新后层数结算。独立和最强实例不由计时触发递归生成新实例。属性修正按当前层数汇总，清理时撤销，不向基础属性永久追加。来源、事件累计和减层规则沿用；周期不伪造累计事件。

类型切换不自动改写效果条目。结构版本仍为本轮未合并的 3，不为同一功能反复升级。数据库无旧业务数据，不新增旧内容转换函数或单独迁移。

## 指向条件

每个节点含 Kind、IdentityId、Relation、Children。Kind：Any=0、Species=1、Faction=2、Relation=3、AllOf=4、AnyOf=5、Not=6。Species／Faction 必须引用 CombatRelations 的实际身份 Id；仅身份节点携带 IdentityId。Relation 只允许自身1、友方2、中立3、敌对4；其他节点 Relation=0。

叶子无子项；AND／OR 至少一个子项；NOT 恰好一个。最多 8 层、64 节点、每节点 16 子项。默认与每条覆盖条件均校验，不再接受自由 Type／Value／Parameters。

## 设置、引用与发布

EraResonanceDraftSettings 仅 CommonTacticalSkills、CombatRelations、DisabledSkills。种族／阵营各最多 256 项，各类关系最多 1024 条；沿用方向、冲突和 Key 校验。保存设置时 null／省略 CombatRelations 保留当前表，显式空目录可清空；其他两个列表按传入完整值保存。

反向效果引用扫描全部槽位 Effects.EffectId；种族阵营引用检查扫描默认及每条覆盖条件。删除不自动移除引用，不修改角色学习／配装；预检报告未解决引用。界面入口为“实体配置 → 种族阵营”。

发布必须匹配预览的草稿修订与正式版本，且 Errors 为空。每个新请求使用新非空 GUID；同一次超时重试保留 RequestId 与两个版本。完整快照、历史与发布收据原子保存，409 冲突需重新读取比较。每个新显式发布递增版本，相同 RequestId 幂等。

Mode=0 按类别和 Id 合并、全局池有序去重；Mode=1 标记删除未出现的对象并替换设置。种族阵营按整表替换，不逐行混合。导入和历史恢复只接受结构 3；旧历史不会自动转换或回写为新配置。

工作区预算 1 MiB、正式快照及单项定义预算 256 KiB、每类最多 10000 项。固定 DTO 严格拒绝未知字段，已删除 EditorDefinitions、ParameterFields、Carriers、CarrierId、Parameters、BuffIds 等旧公开配置。

## 读取、迁移与验证

游戏通过 GET /api/era/manifest/catalog 获取正式快照，任务内冻结。角色配置仍使用 /api/era/characters/{characterId}/ability-systems/loadouts；校验当前账号归属、技能池、槽位、重复装备及配装版本，当前阶段不校验学习状态。主机读取有效成员配装使用 /api/era/worlds/{worldInstanceId}/ability-profiles，保留权威代次及租约校验。

新增 DbUp Script0048_use_fixed_resonance_configuration.sql：直接执行 UPDATE／DELETE，无临时函数或 DELIMITER。空旧目录升结构 3；删除废弃元数据及参数模板菜单／授权绑定；仅有改动的设置递增版本。无旧业务内容，不建立转换器；若意外存在非空旧 Skills，保留其原结构版本，不将其冒充新配置。历史 DbUp 文件及日志不重置，无关晶格、效果和身份数据保留。手动执行应选中整份脚本，并在目标数据库执行；手动执行不登记 DbUp 日志，脚本可重复执行。

已在隔离 MySQL 8 执行全部 48 个脚本，并验证增量清理、幂等、版本、非空旧内容保留及无例程权限执行；无业务库变更。固定契约单元测试源码已更新，未执行 .NET 测试。共享示例见客户端 tools/abilities/catalog-v3.example.json；Web 有同内容夹具。待办以客户端 docs/谐振技能固定配置重构TODO.md 为准。
