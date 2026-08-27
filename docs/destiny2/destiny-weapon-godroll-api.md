# Destiny 2 武器框架、特性与 God Roll 查询接口设计

## 1. 目标

提供以下三个 Destiny 2 武器查询能力：

1. 查询当前有效武器拥有的武器框架列表。
2. 根据一个或多个武器框架查询可能出现的武器特性（Perk）。
3. 根据武器框架和 Perk Group 查找能够形成目标组合的具体武器。

本文档中的 **Perk** 指 Destiny 2 武器特性。代码和 API 路径统一使用正确术语 `perk`，功能分支名称沿用 `destiny-weapon-peak`。

## 2. 通用约定

### 2.1 路径

所有接口使用统一前缀：

```text
/api/destiny2/weapon
```

### 2.2 多语言

接口支持的语言从 `Destiny2:SyncLanguages` 配置读取。当前默认配置包含：

| 参数值 | 说明 |
| --- | --- |
| `zh-chs` | 简体中文，配置中存在时优先作为默认值 |
| `zh-cht` | 繁体中文 |
| `en` | 英文 |

名称、描述等展示字段从对应语言的 Bungie Manifest 获取。框架、特性和武器之间的 Hash 关系不受语言影响。

请求未传 `language` 时，优先使用配置中的 `zh-chs`；若配置中不存在 `zh-chs`，则使用 `SyncLanguages` 第一项。传入的语言必须存在于配置列表。

如果请求语言的 Manifest 组件尚未同步，接口返回统一的 Manifest 不可用错误，不混用其他语言内容；单条缺少名称的定义不会进入展示结果。

### 2.3 从已缓存清单重建武器缓存

当清单组件已经存在于 Redis，但某种语言的武器派生缓存缺失或数据结构升级后需要刷新时，可调用：

```http
POST /api/Destiny2/weapon/cache/rebuild
Content-Type: application/json

{
  "languages": ["en", "zh-cht", "zh-chs"]
}
```

该接口只提交后台任务，不在 HTTP 请求中执行重建。后台任务读取已缓存的 `InventoryItem`、`PlugSet`、`Season`、`Stat`、`Trait`、`BreakerType` 和 `SandboxPerk` 清单组件，不重新从 Bungie 下载清单。未传 `languages` 或传入空数组时，使用 `Destiny2:SyncLanguages` 配置中的语言：

```json
{}
```

```json
{
  "code": 202,
  "message": "Weapon cache rebuild job queued.",
  "data": {
    "jobId": "12345",
    "languages": ["en", "zh-cht", "zh-chs"]
  }
}
```

任务成功入队时 HTTP 状态和响应 `code` 均为 `202`。接口只表示任务已接受，不表示缓存已经重建完成；各语言的完成、清单缺失或失败信息记录在后台任务日志中。接口沿用 `Destiny2Controller` 的授权与安全校验。

Manifest 同步接口只负责下载并缓存清单组件，不会自动创建或刷新武器派生缓存。清单同步完成后，需要显式调用本接口提交后台重建任务，并等待任务完成后再查询武器数据。

### 2.4 Hash 类型

Destiny Definition Hash 在服务端使用 `uint`。JSON 中以整数返回和接收。

### 2.5 图标

接口返回 Bungie Manifest 中的图标相对路径，不在本接口中拼接站点域名。

## 3. 获取武器框架列表

### 3.1 接口

```http
GET /api/destiny2/weapon/frames?language=zh-chs
```

### 3.2 查询参数

| 参数 | 类型 | 必填 | 默认值 | 说明 |
| --- | --- | --- | --- | --- |
| `language` | `string` | 否 | 配置默认语言 | 返回内容语言 |

### 3.3 业务规则

- 只返回实际被有效武器使用的武器框架。
- 排除被隐藏、被拉黑或无法用于目标查询的定义。
- 相同框架 Hash 只返回一次。
- 返回结果使用当前请求语言的名称和描述。

### 3.4 响应示例

```json
{
  "code": 200,
  "data": {
    "frames": [
      {
        "frameHash": 1234567890,
        "name": "适配框架",
        "description": "一种全能框架，可靠且坚固。",
        "icon": "/common/destiny2_content/icons/frame.png",
        "weaponCount": 12
      }
    ]
  }
}
```

## 4. 获取框架特性列表

### 4.1 接口

```http
GET /api/destiny2/weapon/perks?frameHashes=1234567890&frameHashes=9876543210&language=zh-chs&keyword=爆破
```

### 4.2 查询参数

| 参数 | 类型 | 必填 | 默认值 | 说明 |
| --- | --- | --- | --- | --- |
| `frameHashes` | `uint[]` | 是 | 无 | 武器框架 Hash，允许重复传参查询多个框架 |
| `language` | `string` | 否 | 配置默认语言 | 返回内容及关键词匹配所使用的语言 |
| `keyword` | `string` | 否 | 空 | 按当前语言的特性名称和描述查询 |

请求示例：

```http
GET /api/destiny2/weapon/perks?frameHashes=123&frameHashes=456&language=en
```

### 4.3 业务规则

- `frameHashes` 至少包含一个有效值，服务端对重复值去重。
- 未传递或只传递空白 `keyword` 时，返回所选框架下所有可能出现的特性。
- 传递 `keyword` 时，仅在所选框架范围内按本地化名称和描述进行不区分大小写的包含查询。
- 结果按武器框架和特性插槽组织，不混合不同框架的插槽关系。
- 只返回当前可用于随机组合的特性。
- 特性条目保留适用的武器 Hash，供调用方了解该特性在框架内的适用范围。
- 不分页；无关键词时按需求返回所选框架下的全部可能特性。
- 未匹配到的框架通过 `unmatchedFrameHashes` 返回，不影响其他有效框架的结果。

### 4.4 响应示例

```json
{
  "code": 200,
  "data": {
    "frames": [
      {
        "frame": {
          "frameHash": 1234567890,
          "name": "适配框架",
          "description": "一种全能框架，可靠且坚固。",
          "icon": "/common/destiny2_content/icons/frame.png"
        },
        "perkSockets": [
          {
            "socketIndex": 3,
            "perks": [
              {
                "perkHash": 1111111111,
                "name": "爆破专家",
                "description": "使用此武器完成击杀可生成手雷能量。",
                "icon": "/common/destiny2_content/icons/perk.png",
                "applicableWeaponHashes": [
                  3333333333,
                  4444444444
                ]
              }
            ]
          }
        ]
      }
    ],
    "unmatchedFrameHashes": []
  }
}
```

## 5. God Roll 武器查找

### 5.1 接口

```http
POST /api/destiny2/weapon/godroll/filter
Content-Type: application/json
```

### 5.2 请求体

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "weaponTypes": [6, 24],
  "ammoTypes": [1, 2],
  "damageTypes": [1, 2],
  "weaponSlots": [1, 2],
  "rarities": [5, 6],
  "breakerTypes": [1],
  "isCraftable": true,
  "statFilters": [
    { "statHash": 4284893193, "minimumValue": 600, "maximumValue": 900 }
  ],
  "foundryTraitIds": ["item.weapon.foundry.suros"],
  "currentVersions": [1],
  "powerCapHashes": [1234567890],
  "isAdept": true,
  "isHolofoil": false,
  "classTypes": [0],
  "perkGroups": [
    {
      "perkHashes": [
        1111111111,
        2222222222
      ]
    },
    {
      "perkHashes": [
        3333333333,
        4444444444
      ]
    }
  ],
  "language": "zh-chs"
}
```

请求模型概念：

```csharp
public class FilterDestiny2WeaponGodRollRequest : PageInputDto
{
    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public List<Destiny2ItemSubTypeEnum> WeaponTypes { get; set; } = [];

    public List<Destiny2AmmunitionTypeEnum> AmmoTypes { get; set; } = [];

    public List<Destiny2DamageTypeEnum> DamageTypes { get; set; } = [];

    public List<Destiny2WeaponSlotEnum> WeaponSlots { get; set; } = [];

    public List<Destiny2TierTypeEnum> Rarities { get; set; } = [];

    public List<Destiny2BreakerTypeEnum> BreakerTypes { get; set; } = [];

    public bool? IsCraftable { get; set; }

    public List<Destiny2WeaponStatFilterDto> StatFilters { get; set; } = [];

    public List<string> FoundryTraitIds { get; set; } = [];

    public List<uint> CurrentVersions { get; set; } = [];

    public List<uint> PowerCapHashes { get; set; } = [];

    public bool? IsAdept { get; set; }

    public bool? IsHolofoil { get; set; }

    public List<Destiny2ClassEnum> ClassTypes { get; set; } = [];

    public List<Destiny2WeaponPerkGroupDto> PerkGroups { get; set; } = [];

    public string Language { get; set; } = string.Empty;
}

public class Destiny2WeaponPerkGroupDto
{
    public List<uint> PerkHashes { get; set; } = [];
}
```

筛选值说明：

- `ammoTypes`：`1` 主弹药、`2` 特殊弹药、`3` 重型弹药。
- `damageTypes`：`1` 动能、`2` 电弧、`3` 烈焰、`4` 虚空、`5` 突袭、`6` 冰影、`7` 缚丝。
- `weaponSlots`：`1` 一号位（动能槽）、`2` 二号位（能量槽）、`3` 三号位（威能槽）。
- `rarities`：`3` 普通、`4` 稀有、`5` 传说、`6` 异域。
- `breakerTypes`：`1` 屏障、`2` 过载、`3` 势不可挡。
- `isCraftable`、`isAdept`、`isHolofoil`：不传为不限制，传布尔值时精确筛选。
- `statFilters`：同一面板属性可分别限制最小值和最大值。
- `foundryTraitIds`：按 Manifest Trait ID 筛选制造商/铸造厂。
- `classTypes`：`0` 泰坦、`1` 猎人、`2` 术士、`3` 无职业限制。
- 同一筛选数组内是 OR 关系，不同筛选条件之间是 AND 关系；数组为空时不限制该条件。
- `pageIndex` 从 `1` 开始；`pageSize` 默认为 `20`，允许范围为 `1` 至 `100`。

### 5.3 Perk Group 语义

- 同一个 Perk Group 内的多个 Perk 是 **OR** 关系：命中任意一个即可满足该组。
- 不同 Perk Group 之间是 **AND** 关系：每个组都必须至少命中一个 Perk。

示例：

```text
(Perk A OR Perk B)
AND
(Perk C OR Perk D)
```

### 5.4 匹配规则

一把武器必须同时满足以下条件才会返回：

1. 武器符合请求中所有非空武器属性筛选条件。
2. 每个 Perk Group 至少有一个候选 Perk 存在于该武器的随机特性池中。
3. 不同 Perk Group 最终命中的 Perk 可以分配到不同的特性插槽。
4. 所有命中特性能够在同一把武器上同时出现。
5. 不能使用同一个特性插槽同时满足多个 Perk Group。
6. 只使用当前仍可生成的随机特性进行匹配。

匹配算法的 Perk Group 分配过程属于服务内部实现，不通过 API 响应暴露。

### 5.5 响应约束

响应只返回：

- 匹配到的武器信息；
- 武器外观、稀有度、反勇士属性、制作和版本信息；
- 完整基础面板属性、制造商/铸造厂和职业限制；
- 武器框架信息；
- 命中的特性插槽；
- 每个插槽中匹配到的特性。

响应不返回 `groupId`、`groupIndex` 或其他内部 Perk Group 分配信息。

### 5.6 响应示例

```json
{
  "code": 200,
  "data": {
    "pageIndex": 1,
    "pageSize": 20,
    "total": 1,
    "totalPages": 1,
    "hasPrevPage": false,
    "hasNextPage": false,
    "items": [
      {
        "weaponHash": 5555555555,
        "name": "武器名称",
        "description": "武器描述",
        "icon": "/common/destiny2_content/icons/weapon.png",
        "appearance": {
          "screenshot": "/common/destiny2_content/screenshots/weapon.jpg",
          "iconWatermark": "/common/destiny2_content/icons/watermark.png",
          "iconWatermarkShelved": "",
          "secondaryIcon": "",
          "secondaryOverlay": "",
          "secondarySpecial": "",
          "backgroundColor": { "red": 0, "green": 0, "blue": 0, "alpha": 0 }
        },
        "rarity": 5,
        "rarityName": "传说",
        "breaker": {
          "type": 1,
          "hash": 123456789,
          "name": "屏障勇士",
          "description": "可以穿透屏障勇士的护盾",
          "icon": "/common/destiny2_content/icons/barrier.png"
        },
        "isCraftable": true,
        "recipeItemHash": 987654321,
        "stats": [
          { "statHash": 4284893193, "name": "每分钟发射数", "value": 600 }
        ],
        "foundries": [
          { "traitHash": 123456, "traitId": "item.weapon.foundry.suros", "name": "SUROS" }
        ],
        "version": {
          "currentVersion": 1,
          "powerCapHashes": [1234567890],
          "displayWatermarkIcons": ["/common/destiny2_content/icons/version.png"]
        },
        "isAdept": true,
        "isHolofoil": false,
        "classType": 3,
        "frame": {
          "frameHash": 1234567890,
          "name": "适配框架"
        },
        "matchedPerkSockets": [
          {
            "socketIndex": 3,
            "perks": [
              {
                "perkHash": 1111111111,
                "name": "爆破专家",
                "description": "使用此武器完成击杀可生成手雷能量。",
                "icon": "/common/destiny2_content/icons/perk1.png"
              }
            ]
          },
          {
            "socketIndex": 4,
            "perks": [
              {
                "perkHash": 3333333333,
                "name": "狂乱",
                "description": "长时间处于战斗状态可提高武器性能。",
                "icon": "/common/destiny2_content/icons/perk2.png"
              }
            ]
          }
        ]
      }
    ]
  }
}
```

没有匹配武器时返回空数组：

```json
{
  "code": 200,
  "data": {
    "items": []
  }
}
```

## 6. 参数校验

### 6.1 框架特性查询

- `frameHashes` 不能为空。
- `frameHashes` 去重后不能超过服务端设置的单次查询上限。
- `language` 必须是支持的语言代码。
- `keyword` 在查询前执行首尾空白清理。

### 6.2 God Roll 查询

- `perkGroups` 不能为空。
- 每个 Perk Group 的 `perkHashes` 不能为空。
- 各多选过滤数组及组内 Perk Hash 在匹配前去重。
- 面板属性过滤的 `minimumValue` 不能大于 `maximumValue`。
- Perk Group 数量和每组 Perk 数量设置合理上限，防止组合查询失控。
- 不存在或不属于候选武器的 Perk 不会产生错误匹配。

## 7. 数据来源与缓存

功能依赖以下 Destiny 2 Manifest 组件：

- `InventoryItem`
- `PlugSet`
- `Season`
- `Stat`
- `Trait`
- `BreakerType`
- `SandboxPerk`

同步任务需要为 `Destiny2:SyncLanguages` 中配置的语言缓存以上组件。

服务基于 Manifest 构建按语言隔离的查询索引，至少包含以下关系：

```text
Frame -> Weapons
Weapon -> Randomized Perk Sockets
Perk -> Localized Inventory Item Definition
Weapon Frame -> Sandbox Perk -> Breaker Type
```

索引用于避免每次请求重复反序列化完整 Manifest。Manifest 版本更新后，应使用新版本数据重建或失效旧索引。

`Destiny2WeaponDataProvider` 统一负责 Manifest 缓存读取、解析、内部索引构建、按版本和语言缓存，以及同步完成后的失效处理。索引构建不再拆分独立的 Parser 或 Builder 服务；索引模型仅作为 Data Provider 与业务 Service 之间的内部数据结构。

## 8. 项目分层

实现遵循项目现有设计规范：

```text
Controller
  -> Mediator Request/Command
    -> Request/Command Handler
      -> Bungie Service
        -> Manifest Cache/Data Provider
```

- 请求、响应和公开 DTO 放在 `Void.Bot.Message`。
- Handler、业务匹配、索引和数据读取逻辑放在 `Void.Bot.Core`。
- Destiny 2 武器实现统一放在 `Services/Bungie/Destiny2/Weapon`。
- Controller 只负责接收参数、发送 Mediator 消息并返回统一响应。
- 参数校验使用项目现有 Fluent Message Validator。
- 不在 Controller 中实现 Manifest 解析或组合匹配逻辑。

## 9. 开发步骤

1. 补齐 `InventoryItem`、`PlugSet` 多语言 Manifest 同步配置。
2. 使用功能专用轻量解析模型读取本功能所需的 Manifest 字段，避免修改无关的大型 DTO。
3. 建立武器、框架、随机特性插槽的查询索引。
4. 实现武器框架列表接口。
5. 实现多框架特性列表及关键词查询接口。
6. 实现 Perk Group God Roll 匹配接口。
7. 增加参数校验、日志和 Manifest 缺失处理。
8. 增加单元测试并执行功能验证。
9. 开发完成后统一检查变更范围并提交保存。

## 10. 测试范围

至少覆盖以下场景：

- 三种语言返回对应的本地化内容。
- 框架接口只返回实际被武器使用的有效框架。
- 单框架与多框架特性查询。
- 无关键词时返回框架下所有可能特性。
- 有关键词时只返回名称或描述匹配的特性。
- Perk Group 组内 OR 匹配。
- 多个 Perk Group 组间 AND 匹配。
- 两个组只能命中同一插槽时不返回武器。
- 多个组能分配到不同插槽时正确返回武器。
- 不存在的框架、Perk 和空结果处理。
- 重复 Hash 去重和查询数量上限校验。
- Manifest 缓存缺失时返回明确的统一错误响应。

## 11. 验收标准

- 三个接口符合本文档定义的请求与响应契约。
- 接口支持 `Destiny2:SyncLanguages` 中配置的语言。
- `godroll-find` 不产生同一插槽满足多个 Perk Group 的错误结果。
- API 不暴露 Perk Group 内部匹配编号或分配过程。
- 新增测试通过。
- 实现符合现有 CQRS、Mediator、Service 和缓存设计规范。

## 12. 开发 TODO

以下任务按实施顺序执行。只有完成实现、测试和验证后才勾选对应任务。

### Phase 1：Manifest 数据准备

- [x] 将 `InventoryItem` 和 `PlugSet` 加入默认 `Destiny2:SyncManifests` 配置。
- [x] 确认定时同步任务会为 `Destiny2:SyncLanguages` 中配置的语言同步上述组件。
- [x] 核对 Bungie `InventoryItem`、`PlugSet` 中本功能使用字段的 JSON 结构。
- [x] 使用功能专用轻量解析模型读取武器、框架、插槽和 Plug Set 字段。
- [x] 增加 Manifest 反序列化测试，覆盖武器插槽、框架和随机 Plug Set 数据。
- [x] 定义 Manifest 缺失、语言缓存缺失和无效 JSON 的统一处理方式。

### Phase 2：公开消息与 DTO 契约

- [x] 定义框架、特性、特性插槽和武器摘要公共 DTO。
- [x] 定义 `GetDestiny2WeaponFramesRequest/Response`。
- [x] 定义 `GetDestiny2WeaponPerksRequest/Response`。
- [x] 定义 `FindDestiny2WeaponGodRollRequest/Response`。
- [x] 定义只包含 `PerkHashes` 的 Perk Group 请求 DTO。
- [x] 确保 God Roll 响应不包含 `groupId`、`groupIndex` 或内部匹配状态。
- [x] 为公开模型补充必要的 XML 注释和一致的默认值。

### Phase 3：参数规范化与校验

- [x] 从 `Destiny2:SyncLanguages` 读取支持语言，并按配置确定默认语言。
- [x] 校验并去重 `frameHashes`。
- [x] 清理 `keyword` 首尾空白，并将空白关键词视为未传关键词。
- [x] 校验 `perkGroups` 及每组 `perkHashes` 不为空。
- [x] 去重每个 Perk Group 内的 Perk Hash。
- [x] 设置单次框架数量、Perk Group 数量和每组 Perk 数量上限。
- [x] 使用项目现有 Fluent Message Validator 返回统一校验错误。

### Phase 4：武器查询索引

- [x] 定义内部只读索引模型，区分公开 DTO 与匹配所需内部数据。
- [x] 从 `InventoryItem` 中识别有效武器，排除隐藏或拉黑定义，并保留没有随机特性池的固定词条武器。
- [x] 从武器固有插槽中解析框架 Hash。
- [x] 从武器随机插槽和 `PlugSet` 中解析 Perk Hash、插槽索引及 `CurrentlyCanRoll`。
- [x] 建立 `Frame -> Weapons` 索引。
- [x] 建立 `Weapon -> Perk Sockets` 索引。
- [x] 建立 `Perk Hash -> Localized Definition` 索引。
- [x] 按配置语言隔离查询索引，确保本地化展示数据不串用。
- [x] 使用 Manifest 版本作为索引失效依据。
- [x] 保证索引构建并发安全，避免多个请求重复构建同一版本索引。
- [x] 增加索引构建日志，但不记录完整 Manifest 或大批量 Hash 数据。

### Phase 5：武器框架列表接口

- [x] 实现框架查询 Service 方法。
- [x] 只返回至少关联一把有效武器的框架。
- [x] 按框架 Hash 去重并计算 `weaponCount`。
- [x] 使用请求语言填充名称、描述和图标。
- [x] 实现对应 Request Handler。
- [x] 在 `Destiny2Controller` 增加 `GET weapon/frames` 路由。
- [x] 增加稳定、明确的框架列表排序规则。

### Phase 6：框架特性列表接口

- [x] 实现单框架和多框架 Perk 查询。
- [x] 未传 `keyword` 时返回所选框架下全部当前可用特性。
- [x] 传入 `keyword` 时按当前语言的名称和描述进行包含查询。
- [x] 按框架和插槽组织结果。
- [x] 为每个 Perk 返回去重后的 `applicableWeaponHashes`。
- [x] 返回无法识别的 `unmatchedFrameHashes`。
- [x] 实现对应 Request Handler。
- [x] 在 `Destiny2Controller` 增加 `GET weapon/perks` 路由。
- [x] 增加框架、插槽和 Perk 的稳定排序规则。

### Phase 7：God Roll 匹配

- [x] 实现组内 OR、组间 AND 的 Perk Group 候选匹配。
- [x] 为每把候选武器建立 `Perk Group -> 可用插槽` 关系。
- [x] 使用可验证的插槽分配算法，确保每个 Perk Group 占用不同插槽。
- [x] 排除只能通过同一插槽满足多个组的伪匹配结果。
- [x] 支持多个 `frameHashes`，匹配任意一个指定框架下的武器。
- [x] 只使用 `CurrentlyCanRoll` 的 Perk 参与匹配。
- [x] 只返回武器、框架和 `matchedPerkSockets`，不返回内部组分配信息。
- [x] 对武器、插槽和匹配 Perk 应用稳定排序。
- [x] 实现对应 Request Handler。
- [x] 在 `Destiny2Controller` 增加 `POST weapon/godroll-find` 路由。

### Phase 8：多语言与异常处理

- [ ] 验证三个接口在所有配置语言下返回相同 Hash 关系和对应语言文本。
- [x] 明确目标语言缺失单条展示定义时的回退策略。
- [x] Manifest 组件未同步时返回明确的统一错误响应。
- [x] Manifest 索引构建失败时记录结构化错误日志。
- [x] 无匹配框架、特性或武器时返回成功的空业务结果，而不是服务器异常。

### Phase 9：自动化测试

- [x] 测试有效武器与框架识别。
- [ ] 测试隐藏、拉黑和无随机特性池武器的排除规则。
- [x] 测试 Plug Set 展开和 `CurrentlyCanRoll` 过滤。
- [x] 测试多框架 Perk 合并但仍按框架隔离返回。
- [x] 测试空关键词、空白关键词和本地化关键词查询。
- [x] 测试 Perk Group 组内 OR。
- [x] 测试多个 Perk Group 组间 AND。
- [x] 测试同一插槽冲突导致武器不匹配。
- [x] 测试存在有效不同插槽分配时武器正确匹配。
- [ ] 测试多个可行插槽分配时响应保持稳定。
- [ ] 测试重复 Hash、无效 Hash 和查询数量上限。
- [ ] 测试三种语言及展示定义回退。
- [ ] 测试 Manifest 缓存缺失和反序列化失败。
- [x] 测试响应中不包含 Perk Group 内部字段。

### Phase 10：静态检查、文档与提交

- 按本次开发要求，不加入编译验证环节。
- [x] 执行 `git diff --check`。
- [x] 核对接口实现与本文档请求、响应示例一致。
- [x] 核对变更中不包含无关文件或用户既有修改。
- [x] 更新本文档中因实现细节确认而发生变化的内容。
- [x] 汇总实现结果、测试结果和已知限制。
- [x] 经最终检查后创建一个范围清晰的 Git commit 保存本功能。
