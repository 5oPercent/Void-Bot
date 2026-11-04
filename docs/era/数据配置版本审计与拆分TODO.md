# 数据配置版本审计（历史方案）

> 2026-11-04 后续决定：取消配置版本管理，保存即生效，各类数据使用独立业务表。本文保留旧结构的审计记录；下文的独立发布、历史和运行版本清单方案及 TODO 已取消，不再执行。当前实现以[当前配置直接保存](当前配置直接保存.md)为准。

更新：2026-11-04。范围为后端、Web 和客户端的配置存储、编辑并发、发布历史及对局快照。以下现状来自当前工作区源码，拆分方案尚未实现。

效果必须独立于技能发布。敌人、晶格和战斗关系也应拥有自己的发布版本；武器与护甲当前共享装备版本，奖励和全局参数已有独立版本，但缺少完整的发布历史。各类别独立发布后，由明确的版本清单固定一次运行采用的组合。

## 当前配置版本分布

| 数据 | 业务存储 | 当前版本与发布行为 | 调整结论 |
| --- | --- | --- | --- |
| 效果 | `era_skill_draft_record` 的 `kind=2`、JSON `payload` | 单个草稿有对象版本，但发布后进入技能整包 `Buffs`，共享整包版本、预检和历史恢复 | 独立为 `effects` 发布类别，使用效果自己的业务存储 |
| 谐振技能 | 同表 `kind=1` | 发布包同时带效果、晶格、战斗关系、敌人及设置 | 保留 `resonances` 类别，只保存技能定义及技能所属设置；通过 Key 引用其它类别 |
| 晶格 | 同表 `kind=0` | 与技能整包共用发布版本 | 独立为 `lattices` 类别；技能池引用技能 Key |
| 敌人 | 同表 `kind=3` | 与技能整包共用发布版本；游戏配置响应主动移除 `Enemies` | 独立为 `enemies` 类别，业务定义移出技能表；客户端目录仍待接入 |
| 种族、阵营与关系规则 | `era_skill_draft_workspace.settings_payload.CombatRelations` | 与通用战术池、禁用列表共用 `settings_version`，发布时共享技能整包版本 | 三者一起形成 `combat-relations` 类别，独立编辑和发布 |
| 武器 | `era_weapon_*` 关系表 | 单条保存与整目录发布均直接修改当前关系表，递增 `era_equipment_configuration.version` | 建议独立 `weapons` 类别，并增加草稿和不可变发布快照 |
| 护甲 | `era_armor_*` 关系表 | 与武器锁同一装备配置行、使用同一个发布版本；单条保存直接生效 | 建议独立 `armors` 类别，并增加草稿和不可变发布快照 |
| 奖励节点与奖励池 | `era_reward_configuration.payload` | 已独立递增 `version`；完整配置发布覆盖当前 JSON | 保留 `rewards` 类别，补草稿、历史与恢复能力 |
| 全局参数 | `era_configuration` 的各配置项 | 全部配置项写入相同 `config_version`，更新直接替换当前配置 | 保留独立 `runtime-settings` 类别，补发布快照与历史；不按每个参数另建发布版本 |

效果的独立性是本次明确要求；其它类别的具体拆分粒度建议采用下方方案。武器框架、模板、改装、槽位类型及槽位方案可在同一武器目录内原子发布；护甲的模板、改装、槽位类型及槽位方案可在同一护甲目录内原子发布。

依据：[整包 DTO](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Message/Dto/ERA/ResonanceConfiguration.cs:8)、[草稿类别](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Message/Enum/ERA/EraResonanceDraftKind.cs)、[设置保存](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Resonance/EraResonanceService.Draft.cs:429)、[武器保存](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Equipment/EraWeaponService.cs:221)、[护甲保存](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Equipment/EraArmorService.cs:221)、[奖励发布](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Reward/EraInventoryRewardService.cs:60)、[全局参数更新](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Player/EraService.Admin.cs:44)。

## 已确认的问题

### 效果发布和回滚会连带其它类别

`ResonanceConfiguration.Version` 同时代表技能、效果、晶格、战斗关系及管理端敌人集合。效果草稿已有单项 `Version`，该值用于防止覆盖他人的编辑，不能提供独立的效果发布版本。修改一个效果后，需要发布整个配置包；整包历史恢复也会处理其它类别。

应让效果从 v7 发布为 v8 时，技能仍保持原发布版本。恢复效果历史只恢复效果草稿，再校验其引用者；不要同时还原技能、敌人或战斗关系。

依据：[统一发布及历史恢复](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Resonance/EraResonanceService.Draft.Publication.cs)、[统一编译与比较](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Resonance/EraResonanceDraftCompiler.cs)。

### 战斗关系与技能设置共用编辑版本

`CombatRelations`、`CommonTacticalSkills` 和 `DisabledSkills` 都保存在 `EraResonanceDraftSettings` 中。修改战斗关系与修改通用战术池会争用同一个 `SettingsVersion`。这会使互不相关的编辑产生版本冲突，也使战斗关系没有独立的发布历史。

战斗关系独立维护一份完整目录。通用战术池建议跟随晶格类别，因为客户端会把它合并进每个晶格的战术池；禁用技能列表跟随技能类别。跨类别引用仍在发布时校验。

依据：[设置 DTO](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Message/Dto/ERA/EraResonanceDraftSettings.cs)、[晶格池编译](C:/Users/Feng/Documents/GodotProjects/ERA/scripts/lattices/definitions/LatticeDefinition.cs)。

### 武器和护甲的保存会推进共同的正式版本

武器和护甲的 `Save*`、删除及整目录发布最终进入各自的 `WriteAsync`，锁定同一个 `era_equipment_configuration` 固定行，写当前关系表后递增同一个版本。修改护甲会使旧武器编辑基线过期，反之亦然；单条保存已经影响正式目录，不能将它表述为“只保存草稿”。

`era_equipment_configuration.payload` 也不是每个历史版本的完整武器／护甲快照：读取时会从当前关系表重新组装两个目录。`GenerationManifestVersion` 记录来源版本，但当前没有按该版本恢复完整历史目录的存储链路。已有装备保存的生成结果仍然有效，不应因为补快照而重新生成资产。

依据：[目录组装](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Equipment/EraEquipmentService.cs:257)、[装备来源版本](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Domain/ERA/EraEquipmentInstance.cs:15)、[护甲最终生成结果](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Domain/ERA/EraArmorInstance.cs)。

### 奖励和全局参数缺少完整配置历史

奖励已有独立配置版本，发布覆盖 `era_reward_configuration.payload`。奖励事件保留节点、池、抽取结果及来源版本，这保证同一事件重试使用原计划；它不能替代任意奖励配置版本的完整历史查询。

全局参数更新将所有配置项写成相同 `ConfigVersion`，有期望版本校验，但没有与技能发布历史等价的完整配置历史与恢复流程。

依据：[奖励计划快照](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Reward/Models/EraInventoryRewardPlan.cs)、[参数版本一致性检查](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Player/EraService.cs:77)。

### 生成规则与目录版本来自不同来源

装备响应中的 `GenerationRules` 由后端代码生成，护甲生成规则另有 `Version=1`；`InventoryCapacity` 也从代码常量提供。它们不由目录 JSON 决定。

如果部署改变这些规则，仅凭装备目录发布版本无法识别所有响应内容变化。运行版本清单应记录生成规则版本或对应执行契约标识，并将可配置的生成规则纳入所属类别的发布快照。生成算法版本继续单独标识，已有实例不重算。是否把现有代码常量改为后台可编辑数据属于后续功能设计。

依据：[规则注入](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Equipment/EraEquipmentService.cs:233)、[护甲生成规则](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Equipment/EraArmorGenerationRules.cs)、[装备统一规则](C:/Users/Feng/Documents/Projects/Void-Bot/src/Void.Bot.Core/Services/ERA/Equipment/EraEquipmentRules.cs)。

### Web 数据发布仍只管理原技能整包

页面虽然已经归类为“数据发布”，`version-management.vue` 仍只调用 `useEraAbilityApi` 的统一 `configuration/*` 接口，只有一个正式版本、历史列表、导入导出和待核对发布记录。武器、护甲的保存与发布走各自页面；奖励与全局参数也没有进入同一套分类发布面板。

独立发布需要同时调整类别选择、版本显示、权限、预检、历史恢复、导入导出和发布重试记录。重试记录应包含类别、目标草稿修订号和原正式版本，不能在不同类别之间复用请求编号。

依据：[发布页面](C:/Users/Feng/Documents/Projects/Void-Bot-Web/app/pages/(app)/admin/era/version-management.vue)、[效果页面入口](C:/Users/Feng/Documents/Projects/Void-Bot-Web/app/pages/(app)/admin/era/effects.vue)、[管理端发布 API](C:/Users/Feng/Documents/Projects/Void-Bot-Web/app/composables/useEraAbilityApi.ts)。

### 客户端固定目录仍只有一个能力版本

`GetManifestCatalogAsync` 一次读取完整能力包，`AbilityCatalogCompiler` 先编译效果，再解析技能的效果 Key、晶格和战斗关系；编译结果只有一个 `AbilityCatalog.Version`。`ResonanceTaskSnapshot` 保存整份配置，同一任务拒绝版本或内容变化。`CombatWorldSession` 也从这个目录获取关系规则。武器和护甲则通过单个 `EquipmentCatalog.Version` 组合。

因此拆分后需要调整目录获取、编译及网络快照；只改后台表名或只增加一个效果版本字段不够。运行中对局继续保留已经固定的效果和关系规则，新对局采用新的已校验版本组合。断线重连及主机迁移应继续恢复原组合。

依据：[能力编译](C:/Users/Feng/Documents/GodotProjects/ERA/scripts/manifest/AbilityCatalogCompiler.cs:24)、[同局固定检查](C:/Users/Feng/Documents/GodotProjects/ERA/scripts/gameplay/world/AbilityWorldSession.cs:316)、[任务快照](C:/Users/Feng/Documents/GodotProjects/ERA/scripts/networking/gameplay/ResonanceTaskSnapshot.cs)、[装备目录](C:/Users/Feng/Documents/GodotProjects/ERA/scripts/equipment/EquipmentCatalog.cs)。

## 独立发布与运行组合方案

独立发布类别建议为 `effects`、`resonances`、`lattices`、`combat-relations`、`enemies`、`weapons`、`armors`、`rewards` 和 `runtime-settings`。发布单位是一类完整目录，每个业务对象仍有自己的编辑并发版本。

数据库职责划分如下，具体列类型与现有工作单元保持一致：

| 存储 | 职责 |
| --- | --- |
| 各业务定义表 | 保存本类对象、编辑并发版本和草稿状态；效果及敌人不再作为技能表的类别 |
| 按类别的草稿工作区 | 保存该类别的草稿修订号和最后发布修订号；跨类别编辑不会争用同一修订号 |
| `era_data_publication` | 不可变完整类别快照；保存 `DataKind`、`Version`、`SchemaVersion`、`Payload`、`ContentHash`、发布人及时间，唯一键为类别和版本 |
| `era_data_publication_head` | 每类别的当前生效版本；在事务内锁定并校验期望版本 |
| 发布回执 | 持久化请求编号、发布类别、原版本及修订号、结果版本；保证超时重试幂等 |
| 运行版本清单 | 固定一次运行采用的类别版本及哈希组合；可使用独立 `era_data_release` 快照保存 |

类别版本与运行版本清单解决不同问题。示例：清单 A 固定技能 v4、效果 v7、晶格 v2、战斗关系 v3；清单 B 只将效果改为 v8，其它类别版本不变。新清单激活前，校验技能所用效果、敌人的技能及身份、晶格池以及奖励装备引用。已有对局继续使用清单 A，新对局使用清单 B。

同一事务可以发布一类或多类并激活新清单。查询时按已确定的类别版本取不可变快照，不分别读取各类别的“最新”结果再随意拼接。缓存键至少包含类别、数据版本和结构版本；内容哈希用于核对同版本内容一致性，数据库发布快照作为持久化来源。

## 版本含义和已有保障

| 版本 | 用途 |
| --- | --- |
| 对象编辑版本 | 保存／删除的并发校验 |
| 类别草稿修订号 | 固定一次预检及发布的草稿集合 |
| 类别发布版本 | 标识一份不可变业务配置 |
| 运行版本清单 | 固定多类别的兼容组合 |
| `SchemaVersion` | JSON 数据结构兼容性 |
| 生成算法版本 | 标识生成过程；已有资产使用保存结果 |
| 玩家、配装、装备及世界版本 | 对应运行数据和并发状态，继续独立管理 |
| RPC／状态协议版本 | 网络结构兼容性，按实际结构变化决定是否提升 |

应保留现有的期望版本校验、事务锁、精确 Int64 传输、发布请求幂等、分页一致性重读、同局配置冻结和奖励事件重试快照。武器／护甲关系表读取前后复核目录版本的处理当前能够防止跨版本拼接；独立快照应继续提供同等保障。

本轮审计不改变缓存或协议版本。后续实际修改结构时再判断兼容性，同一功能合入大杂烩前遵循一次版本提升的项目约束。

## 拆分 TODO

- [x] 检查后端配置实体、草稿／发布／历史、Web API 和客户端目录及任务快照。
- [x] 确认效果独立于技能版本，列出其它共用版本的类别及缺失的快照能力。
- [ ] 定义独立类别契约、类别版本、草稿修订号和运行版本清单，明确通用战术池与禁用列表的归属。
- [ ] 补 DbUp 建表和初始化脚本，提供独立效果、敌人、晶格及战斗关系的业务存储与发布元数据。
- [ ] 核对迁移时各旧表实际数据；空数据按新结构初始化，非空数据采用明确迁移方案，不引入运行时旧敌人字段转换。
- [ ] 保留旧历史原文及哈希；新类别快照生成自己的版本和哈希，建立来源关系，不把拆分后的 JSON 冒充原历史内容。
- [ ] 从技能包移除效果、晶格、战斗关系与敌人的发布所有权，保留跨类别 Key 引用及反向引用校验。
- [ ] 为武器与护甲增加独立的编辑、发布及历史快照；迁移后单条保存只更新草稿。
- [ ] 为奖励及全局参数补草稿、发布历史和历史恢复，保留既有奖励事件计划。
- [ ] 将生成规则和执行契约标识纳入确定的运行组合，明确哪些参数进入后台配置。
- [ ] 将 Web 数据发布改为按类别查看、预检、发布、导入导出及恢复；隔离权限、版本基线和重试记录。
- [ ] 调整客户端独立目录读取与组合编译，技能按 Key 解析固定效果目录，身份关系读取固定战斗关系目录。
- [ ] 调整对局及网络快照，覆盖重连、主机迁移和运行中发布后的旧局冻结；按实际结构变化判断版本提升。

## 验收条件

- [ ] 只修改效果：效果版本递增，技能、晶格、敌人及战斗关系的类别版本不变；新清单引用新的效果版本。
- [ ] 改战斗关系不会导致技能设置的编辑版本冲突；改护甲不会导致武器草稿基线失效。
- [ ] 保存任意草稿后正式目录和当前运行清单不变；只有成功发布才产生不可变快照。
- [ ] 独立恢复效果历史只影响效果草稿；删除仍被使用的效果、技能或身份时，跨类别预检能阻止无效组合。
- [ ] 按类别和版本能取回完整配置；旧装备来源版本和奖励事件来源版本可追溯，已有生成结果不重算。
- [ ] 发布超时、重复请求、并发发布和分页期间发布不会重复递增版本或拼接出混合目录。
- [ ] 运行中发布新效果：旧局伤害与现有 BUFF 定义保持不变；新局采用新组合，重连和主机迁移保持原组合。
- [ ] 版本号超过 JavaScript 安全整数范围时仍精确读写；结构、业务、资产和协议版本按各自职责校验。
