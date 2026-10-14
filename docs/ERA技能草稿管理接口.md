# ERA 技能与 BUFF 草稿管理接口

更新日期：2026-10-13。管理操作集中在 `EraAdminController`，根路径 `/api/era/admin`。本文对应源码，迁移与运行验收尚未执行。

## 工作区与数据归属

能力体系、技能和 BUFF 为全局定义；共用一个草稿工作区。保存、删除、恢复、设置、导入和历史恢复只改草稿。显式发布替换完整快照，每个新的发布请求均新增版本，内容不变也新增；相同 RequestId 重试不重复发布。

玩家通过 `GET /api/era/manifest/catalog` 一次读取相同发布版本的能力体系、技能和 BUFF。局内固定配置不变。体系选择、配装和已学习技能按角色独立保存，角色 ID 必须来自路由；普通玩家请求校验角色属于当前账号，不会自动选择其第一个角色。

全局删除定义不更改角色配装与学习记录；客户端同步时清空失效槽位、移除失效体系记忆，不自动替换或选择其他体系。BUFF 局内施加、计时和移除仍由客户端运行时负责，没有普通玩家 BUFF 操作 Controller。

## 认证与权限

管理接口需要 Bearer 登录与对应管理权限，无 SkipPermissionCheck；管理员路径不要求 X-ERA-Client-Id。端点的 Controller 分类和部分路径已变化，应重新扫描并配置实际管理角色授权，不自动扩大权限。

ERA 排序从 400 开始：Manifest 400、玩家 401、角色 402、能力体系 403、技能 404、队伍 405、社交 406、客户端会话 407、管理 499。

## 初始化和草稿状态

先按部署流程执行数据库迁移。旧技能记录已确认尚无数据；Script0028 直接将空表结构改为角色归属，不包含数据检测或归属分配。

- POST `/configuration/initialize`，正文 `{}`：幂等复制已发布配置到共享草稿，不覆盖已有草稿。
- GET `/configuration`：返回 Data.Revision、PublishedVersion、PublishedRevision、HasUnpublishedChanges、SettingsVersion、Settings。
- GET `/configuration/export`：返回完整草稿配置，仅用于备份，可能有未完成引用。

以上为 `/api/era/admin` 下相对路径。普通 GET 不初始化、不写数据。

## 三类独立编辑

集合分别为 `/ability-systems`、`/skills`、`/buffs`。三类采用相同操作：

| 方法／相对路径（以技能为例） | 参数 | 行为 |
| --- | --- | --- |
| GET `/skills` | Skip=0、Take=50、Search、IncludeDeleted=false | 分页列表；Take 为 1～100 |
| GET `/skills/{id}` | 路由 ID | 返回 Version、DraftRevision、Definition、IsDeleted、WasPublished |
| POST `/skills/{id}` | ExpectedVersion、Definition | 创建／修改完整单项定义 |
| POST `/skills/{id}/delete` | ExpectedVersion | 草稿标记删除，保留定义与稳定身份 |
| POST `/skills/{id}/restore` | ExpectedVersion | 恢复原对象，随后正常编辑 |

创建 ExpectedVersion=0；更新使用读取版本。Definition 分别采用 AbilitySystemDefinition、SkillDefinition、BuffDefinitionDto。路由 ID 与 Definition.Id 一致，不可改 ID。删除 ID 不可重新创建，必须显式恢复。

```http
POST /api/era/admin/skills/test.sprint
Content-Type: application/json
```

```json
{
  "ExpectedVersion": 0,
  "Definition": {
    "Id": "test.sprint", "Name": "机动加速", "Description": "自身速度增加20%，持续6秒。",
    "Variants": [{ "Slot": 3, "MaximumCharges": 3, "CooldownSeconds": 3, "RecoveryStart": 0,
      "CarrierId": "self", "Parameters": {}, "BuffIds": ["test.speed_refresh"] }]
  }
}
```

新技能不会自动加入体系池或自动授予角色学习状态。保存自身结构与数值有效即可，允许暂时引用尚不存在的 BUFF；发布前必须全部补齐。

## 设置与引用

GET／POST `/configuration/settings`，保存正文：

```json
{ "ExpectedVersion": 1, "Settings": { "CommonTacticalSkills": [], "DisabledSkills": [] } }
```

GET `/configuration/references?Kind=1&Id=test.sprint&Skip=0&Take=50` 查询反向引用，Kind=0 体系、1 技能、2 BUFF。技能引用包括体系池、通用池和禁用列表；BUFF 引用定位技能方案。不会扫描角色配装与学习记录。

删除不自动清理引用。只从某体系移除技能时编辑该体系池；删除技能定义需清理引用它的池和设置；删除 BUFF 需清理技能 BuffIds。共享 BUFF 修改影响所有引用技能。

## 校验、预览与发布

- POST `/configuration/validate`，正文 `{}`。
- POST `/configuration/publish-preview`，正文 `{}`。

返回 Data.DraftRevision、PublishedVersion、Errors、Changes、HasChanges。预览响应成功不代表可以发布，必须检查 Errors 为空；继续编辑后重新预览。

POST `/configuration/publish`：

```json
{
  "ExpectedDraftRevision": 2,
  "ExpectedPublishedVersion": 0,
  "RequestId": "替换为新的非空GUID"
}
```

两个版本取自预览。每个新的显式发布使用新 GUID；同一次网络超时重试保留原 GUID 和两个版本。样例中的全零 GUID 是占位值。发布整个共享工作区，不是仅发布当前技能／BUFF。

发布复核版本、全部引用、槽位、载体与预算；发布快照、历史与收据在同一事务提交。工作区含删除记录与设置最多 1 MiB，发布配置最多 256 KiB，单项定义最多 256 KiB，每类最多 10000 条。

当前载体契约：self／team 无必需参数；entity distance 默认20，范围0～1000；area radius 默认5、范围(0,1000]，duration 默认8、范围(0,3600]，interval 默认1、范围[0.05,3600]，distance 默认20、范围[0,1000]。未知载体可暂存但不能发布。客户端事件计数与減层扩展不在本次后端 DTO 范围。

## 导入、历史恢复

POST `/configuration/import`：

```json
{
  "ExpectedDraftRevision": 1, "Mode": 0,
  "Configuration": { "Version": 0, "SchemaVersion": 1, "Systems": [], "Skills": [], "Buffs": [],
    "CommonTacticalSkills": [], "DisabledSkills": [] }
}
```

Mode=0 按类别和 ID 合并，设置有序去重合并；Mode=1 标记删除未出现的对象，设置整体替换。导入不发布，删除 ID 需先显式恢复。

- GET `/configuration/history?Skip=0&Take=50`：历史摘要。
- GET `/configuration/history/{version}`：完整不可变配置。
- POST `/configuration/history/{version}/restore`，正文 `{ "ExpectedDraftRevision": 3 }`：整个历史配置恢复到草稿，仍需预览发布，不倒退发布版本号。

## 角色配装与学习

- GET／POST `/api/era/characters/{characterId}/ability-systems/loadouts`：角色体系选择与配装；保存正文仍为 ExpectedVersion 和完整 Profile（CharacterSkillProfile）。
- GET `/api/era/characters/{characterId}/skills/learned`：该角色已学习技能 ID。
- POST `/api/era/admin/characters/{characterId}/learned-skills`：管理员授予该角色技能，正文 `{ "SkillId": "test.sprint" }`，返回角色已学习列表。

普通玩家尚无主动学习命令。玩家档案与仓库仍共享，角色独立的只是体系选择、技能配装与学习记录。

## 错误处理

HTTP 200 不代表业务成功，检查响应业务 Code。常见错误：

- era.draft_unavailable：先初始化或检查部署。
- era.draft_invalid：检查输入与预览诊断。
- era.draft_version_conflict：保留本地修改，重新读取比较，不盲目覆盖。
- era.draft_item_deleted：先恢复对象。
- era.draft_item_not_found／era.publication_not_found：对象或历史不存在。
- era.publish_request_conflict：同一请求 ID 的账号或参数不一致。
- era.character_not_found：角色不存在或不属于当前账号，不泄露他人角色数据。

仅静态检查；本次未编译、未运行测试或数据库迁移。

## 实体条件配置边界

技能槽方案已移除 TargetRelation 字段及对应枚举校验。自身、敌我关系和复合实体筛选由游戏客户端的条件模块定义，并在主机实际生效时判断；后端仍提供技能、BUFF、次数、冷却、载体和参数配置，不保存客户端条件树。JSON 发布数据再次读取及序列化时不再输出旧字段；本次源码调整不自动修改已发布数据或触发发布。客户端和后端应使用配套版本，正式接入目录时需由客户端绑定对应的实体条件。
