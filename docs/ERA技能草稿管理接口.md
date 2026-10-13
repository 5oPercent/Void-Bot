# ERA 技能草稿管理接口

更新日期：2026-10-13。对应 `EraSkillDraftController`，根路径 `/api/era/admin/skills/draft`。本文记录源码实现，不表示已应用 `Script0027_init_era_skill_drafts.sql` 或通过运行验收。

## 编辑与发布约定

所有体系共用一个草稿工作区。体系、技能、BUFF 分别一项一行，独立版本控制；通用战术池与禁用列表属于工作区设置。保存、删除、恢复、导入和历史恢复只改草稿。只有显式发布才替换游戏配置，每次新发布请求都加一版本，即使内容没有变化；同一个请求 ID 重试不重复加版本。

游戏仍通过 `GET /api/era/skills/config` 读取已发布完整快照，局内固定配置不变。后端删除技能、移出技能池或删除体系时不改玩家配装；客户端按本次同步的目录清空失效槽位／移除无效体系记忆，不自动替换或选中另一体系。学习记录保留，删除记录不物理清除，旧 ID 不能作为新对象重新创建；恢复必须操作原对象。

本模块不更改学习保存校验，不保存图标、冷却或局内 BUFF 实例。客户端本地事件计数与减层字段不在本次后端 DTO 扩展范围内。

## 认证和权限

所有草稿接口需要 Bearer 登录与对应管理接口权限，不使用 `SkipPermissionCheck`；管理员路径不要求 `X-ERA-Client-Id`。现有端点扫描器发现新增接口，通过已有权限配置分别授权读取、编辑、发布和历史操作；不会自动给所有管理员角色授予新权限。超级管理员沿用现有机制。

ERA 文档标签排序：基础 200、会话 201、队伍 202、社交 203、技能 204、草稿 205。路由保持原用途；旧整包直发接口 `PUT /api/era/admin/skills/config` 已移除。

## 初始化与读取

先按项目部署流程应用新增迁移，再显式调用：

```http
POST /api/era/admin/skills/draft/initialize
Content-Type: application/json

{}
```

该命令幂等，从当前已发布配置复制 ID、顺序、效果、池和禁用设置，建立初始历史；不覆盖已经存在的已初始化草稿。普通 GET 不会初始化或写数据。

- `GET /draft` 返回 `Revision`、`PublishedVersion`、`PublishedRevision`、`HasUnpublishedChanges`、设置和设置版本；这里的 `/draft` 表示完整根路径。
- `GET /draft/export` 返回草稿完整配置，可能有未完成引用，仅用于备份；不能把导出当作已经发布。
- 未初始化／未应用迁移不能编辑；`era.draft_unavailable` 提示先初始化。缺少数据库表属于部署错误，需应用迁移，不通过 GET 自动建表。

## 单项编辑

三个对象集合路径为 `systems`、`skills`、`buffs`。均提供以下操作：

| 方法／相对路径 | 正文或查询 | 行为 |
| --- | --- | --- |
| `GET /systems` | `Skip=0&Take=50&Search=...&IncludeDeleted=false` | 返回草稿修订号、总数和当前页；Take 为 1～100 |
| `GET /systems/{id}` | 无正文 | 单项版本、定义、删除标记与曾发布标记 |
| `POST /systems/{id}` | `ExpectedVersion`、`Definition` | 单项创建／保存；创建版本为 0，更新使用读取版本 |
| `POST /systems/{id}/delete` | `ExpectedVersion` | 标记删除，保留原定义和身份，不自动删引用 |
| `POST /systems/{id}/restore` | `ExpectedVersion` | 恢复原定义，随后可以正常编辑 |

`skills` 与 `buffs` 使用完全相同的路由形态。`Definition` 分别是现有 `AbilitySystemDefinition`、`SkillDefinition`、`BuffDefinitionDto`。路由 ID 写入消息，必须与定义 ID 一致，保存时不能改 ID。

单技能修改示例：

```http
POST /api/era/admin/skills/draft/skills/test.sprint
Content-Type: application/json

{
  "ExpectedVersion": 1,
  "Definition": {
    "Id": "test.sprint",
    "Name": "机动加速",
    "Description": "自身速度增加20%，持续6秒。",
    "Variants": [{
      "Slot": 3, "MaximumCharges": 3, "CooldownSeconds": 3,
      "RecoveryStart": 0, "CarrierId": "self", "TargetRelation": 0,
      "Parameters": {}, "BuffIds": ["test.speed_refresh"]
    }]
  }
}
```

设置接口为 `GET /draft/settings` 和 `POST /draft/settings`。保存正文：

```json
{
  "ExpectedVersion": 1,
  "Settings": { "CommonTacticalSkills": [], "DisabledSkills": ["test.sprint"] }
}
```

单项保存校验自身结构、枚举、次数／时间、效果与数值范围。允许暂时引用不存在的其他草稿对象，例如先创建技能后补 BUFF；这些引用在发布前必须全部修复。

当前客户端载体参数契约：self／team 无必需参数；entity 的 distance 默认为 20，范围 0～1000；area 的 radius 默认 5、范围大于 0 至 1000，duration 默认 8、范围大于 0 至 3600，interval 默认 1、范围 0.05～3600，distance 默认 20、范围 0～1000。未知载体可以暂存草稿，但不能通过当前发布校验；增加载体时同步扩展客户端工厂和后端契约校验。

## 引用、校验与预览

- `GET /draft/references?Kind=Skill&Id=test.sprint&Skip=0&Take=50`：列出体系池、通用池、禁用列表中对应引用；Kind 取 System／Skill／Buff（数值 0／1／2）。BUFF 引用定位到技能方案。不扫描玩家配装和学习记录。
- `POST /draft/validate`，正文 `{}`：返回冻结版本、具体诊断与差异。
- `POST /draft/publish-preview`，正文 `{}`：与发布共用同一汇总器和校验规则，返回 `DraftRevision`、`PublishedVersion`、`Errors`、`Changes` 和 `HasChanges`。

预览诊断位于 `Data.Errors`，不是仅看响应 Code 判断可以发布。移除定义后应编辑对应技能池或 BUFF 引用；如果仅从某体系下架技能，只改该体系的池，保留其他体系使用的全局定义。共享 BUFF 修改影响所有引用它的技能。

发布配置限制 256 KiB，草稿所有对象（含删除记录）与设置的 JSON 总量限制 1 MiB；每类对象最多 10000 条。分页、搜索、预览均不修改版本。

## 导入与发布

```http
POST /api/era/admin/skills/draft/import
Content-Type: application/json
```

```json
{
  "ExpectedDraftRevision": 1,
  "Mode": 0,
  "Configuration": { "Version": 0, "SchemaVersion": 1, "Systems": [], "Skills": [], "Buffs": [], "CommonTacticalSkills": [], "DisabledSkills": [] }
}
```

上面是空配置结构示例。Mode=0 合并：按类别和 ID 更新定义，通用池／禁用列表有序去重合并；Mode=1 替换：正文中未出现的原草稿对象标记删除，设置整体替换。导入不会发布。若正文包含已经标记删除的 ID，先显式恢复原对象；不能通过导入静默复用旧身份。

完成编辑后预览，取得两个版本，再发布：

```http
POST /api/era/admin/skills/draft/publish
Content-Type: application/json
```

```json
{
  "ExpectedDraftRevision": 2,
  "ExpectedPublishedVersion": 0,
  "RequestId": "替换为新的非空GUID"
}
```

RequestId 用于一次发布的幂等；重试原请求时保留原 ID 与版本。下一次显式发布生成新 ID 并使用新读取版本。示例文件中的全零 GUID 是占位值，不能直接发布。

发布在同一工作单元中冻结草稿，重校验两个版本、全部引用、槽位、载体和预算；已发布快照、历史与幂等收据一起提交。后端不自动清理玩家数据。旧管理直发 Command／Handler／Validator 已删除，没有绕过草稿的发布分支。

## 历史恢复

- `GET /draft/history?Skip=0&Take=50` 返回不可变历史摘要。
- `GET /draft/history/{version}` 返回完整历史配置。
- `POST /draft/history/{version}/restore`，正文 `{ "ExpectedDraftRevision": 3 }`：历史恢复到草稿，仍需预览和显式发布。

回滚不降低已发布版本号。历史恢复是明确恢复原对象身份，可以恢复曾被删除的定义；学习记录仍保留，后端不调整配装。

## 常见响应

响应沿用业务 Code，HTTP 200 不代表业务成功：

- `era.draft_unavailable`：草稿尚未初始化。
- `era.draft_invalid`：参数、定义或完整发布校验失败，先查看 validate／preview 诊断。
- `era.draft_version_conflict`：对象、草稿或发布版本已变化，重新读取后再修改，不能盲目强制覆盖。
- `era.draft_item_deleted`：旧 ID 已删除，需恢复原对象。
- `era.draft_item_not_found`／`era.publication_not_found`：对象或历史不存在。
- `era.publish_request_conflict`：同一请求 ID 已被其他账号或版本参数使用。

测试用例源码已覆盖草稿不改变发布、未完成引用阻断、两个版本冲突、幂等、新请求无变化仍加版本、删除保留玩家数据和导入替换。没有编译、运行测试或执行数据库迁移。数据库锁、失败回滚及真实权限／多管理员并发仍需运行验收。
