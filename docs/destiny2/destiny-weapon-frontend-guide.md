# Destiny 2 武器查找接口前端对接文档

## 1. 接口

```http
POST /api/destiny2/weapon/godroll/filter
Content-Type: application/json
```

## 2. 请求结构

```ts
interface WeaponFilterRequest {
  pageIndex?: number;
  pageSize?: number;
  weaponTypes?: number[];
  ammoTypes?: number[];
  damageTypes?: number[];
  weaponSlots?: number[];
  rarities?: number[];
  breakerTypes?: number[];
  isCraftable?: boolean | null;
  statFilters?: WeaponStatFilter[];
  foundryTraitIds?: string[];
  currentVersions?: number[];
  powerCapHashes?: number[];
  isAdept?: boolean | null;
  isHolofoil?: boolean | null;
  classTypes?: number[];
  perkGroups: Array<{
    perkHashes: number[];
  }>;
  language: string;
}

interface WeaponStatFilter {
  statHash: number;
  minimumValue?: number | null;
  maximumValue?: number | null;
}
```

请求示例：

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "weaponTypes": [6],
  "ammoTypes": [1, 2],
  "damageTypes": [1, 2],
  "weaponSlots": [1, 2],
  "rarities": [5, 6],
  "breakerTypes": [1],
  "isCraftable": true,
  "statFilters": [
    {
      "statHash": 4284893193,
      "minimumValue": 600,
      "maximumValue": 900
    }
  ],
  "foundryTraitIds": [
    "item.weapon.foundry.suros"
  ],
  "currentVersions": [1],
  "powerCapHashes": [1234567890],
  "isAdept": true,
  "isHolofoil": false,
  "classTypes": [3],
  "perkGroups": [
    {
      "perkHashes": [1111111111, 2222222222]
    }
  ],
  "language": "zh-chs"
}
```

## 3. 筛选语义

- 同一个数组中的值是 OR 关系。
- 不同字段之间是 AND 关系。
- 空数组表示不限制。
- 可空布尔字段不传或传 `null` 表示不限制，传 `true` 或 `false` 表示精确筛选。
- 多个 `statFilters` 之间是 AND 关系。
- `perkGroups` 必填且不能为空。
- 同一个 Perk Group 内是 OR 关系，不同 Perk Group 之间是 AND 关系。

## 4. 枚举值

### 4.1 弹药类型 `ammoTypes`

| 值 | 含义 |
|---:|---|
| 1 | 主弹药 |
| 2 | 特殊弹药 |
| 3 | 重型弹药 |
| 4 | 未知 |

### 4.2 元素属性 `damageTypes`

| 值 | 含义 |
|---:|---|
| 1 | 动能 |
| 2 | 电弧 |
| 3 | 烈焰 |
| 4 | 虚空 |
| 5 | 突袭 |
| 6 | 冰影 |
| 7 | 缚丝 |

### 4.3 武器槽位 `weaponSlots`

| 值 | 含义 |
|---:|---|
| 1 | 一号位／动能槽 |
| 2 | 二号位／能量槽 |
| 3 | 三号位／威能槽 |

### 4.4 稀有度 `rarities`

| 值 | 含义 |
|---:|---|
| 2 | 基础 |
| 3 | 普通 |
| 4 | 稀有 |
| 5 | 传说 |
| 6 | 异域 |

前端通常只需要展示 `3` 到 `6`。

### 4.5 反勇士属性 `breakerTypes`

| 值 | 含义 |
|---:|---|
| 1 | 屏障 |
| 2 | 过载 |
| 3 | 势不可挡 |

### 4.6 职业限制 `classTypes`

| 值 | 含义 |
|---:|---|
| 0 | 泰坦 |
| 1 | 猎人 |
| 2 | 术士 |
| 3 | 无职业限制 |

武器类型枚举通过以下接口获取：

```http
GET /api/destiny2/weapon/types
```

## 5. 响应结构

```ts
interface WeaponFilterResult {
  pageIndex: number;
  pageSize: number;
  total: number;
  totalPages: number;
  hasPrevPage: boolean;
  hasNextPage: boolean;
  items: WeaponResult[];
}

interface WeaponResult {
  weaponHash: number;
  name: string;
  description: string;
  icon: string;
  appearance: WeaponAppearance;
  weaponType: {
    weaponType: number;
    name: string;
  };
  ammoType: number;
  damageType: number;
  weaponSlot: number;
  rarity: number;
  rarityName: string;
  breaker: WeaponBreakerType;
  isCraftable: boolean;
  recipeItemHash: number;
  stats: WeaponStat[];
  foundries: WeaponFoundry[];
  version: WeaponVersion;
  isAdept: boolean;
  isHolofoil: boolean;
  classType: number;
  frame: WeaponFrame;
  roundsPerMinute: number;
  season: WeaponSeason;
  matchedPerkSockets: MatchedPerkSocket[];
}

interface WeaponBreakerType {
  type: number;
  hash: number;
  name: string;
  description: string;
  icon: string;
}

interface WeaponFrame {
  frameHash: number;
  name: string;
  description: string;
}

interface WeaponSeason {
  seasonHash: number;
  seasonNumber: number;
  name: string;
}

interface MatchedPerkSocket {
  socketIndex: number;
  perks: Array<{
    perkHash: number;
    name: string;
    description: string;
    icon: string;
    isEnhanced: boolean;
  }>;
}
```

`pageIndex` 从 `1` 开始；`pageSize` 默认为 `20`，允许范围为 `1` 至 `100`。

## 6. 外观字段

```ts
interface WeaponAppearance {
  screenshot: string;
  iconWatermark: string;
  iconWatermarkShelved: string;
  secondaryIcon: string;
  secondaryOverlay: string;
  secondarySpecial: string;
  backgroundColor: {
    red: number;
    green: number;
    blue: number;
    alpha: number;
  };
}
```

所有图片字段都可能为空。非空路径需要拼接 Bungie 域名：

```ts
const imageUrl = `https://www.bungie.net${imagePath}`;
```

推荐用途：

- `icon`：列表卡片主图。
- `iconWatermark`：覆盖在主图角落的版本或赛季水印。
- `screenshot`：详情页横向大图。
- `backgroundColor`：卡片背景颜色。
- Secondary 系列字段：可选辅助图层，不建议作为核心展示资源。

## 7. 完整武器面板

```ts
interface WeaponStat {
  statHash: number;
  name: string;
  description: string;
  icon: string;
  value: number;
  minimum: number;
  maximum: number;
  displayMaximum: number;
}
```

前端主要展示 `name`、`value` 和 `displayMaximum`。进度条计算示例：

```ts
const percent =
  stat.displayMaximum > 0
    ? Math.min(100, stat.value / stat.displayMaximum * 100)
    : 0;
```

`roundsPerMinute` 仍单独返回，同时也可能出现在 `stats` 中。

## 8. 制造商／铸造厂

```ts
interface WeaponFoundry {
  traitHash: number;
  traitId: string;
  name: string;
  description: string;
  icon: string;
}
```

过滤时传递 `traitId`：

```json
{
  "foundryTraitIds": [
    "item.weapon.foundry.suros"
  ]
}
```

展示时使用本地化的 `name`，不要直接展示 `traitId`。

## 9. 版本信息

```ts
interface WeaponVersion {
  currentVersion: number;
  powerCapHashes: number[];
  displayWatermarkIcons: string[];
  infusionCategoryHash: number;
  infusionCategoryName: string;
}
```

注意事项：

- `currentVersion` 是 Manifest 版本索引，不是赛季号。
- `powerCapHashes` 当前返回原始 Hash。
- `displayWatermarkIcons` 中的路径需要拼接 Bungie 域名。
- `isAdept` 表示专家版本。
- `isHolofoil` 表示 Holofoil 特殊版本。
- `isAdept` 和 `isHolofoil` 不等于通用的“可强化武器”状态。

## 10. 空值约定

- `recipeItemHash = 0`：没有制作图样。
- `foundries = []`：Manifest 没有可识别的铸造厂标签。
- 图片字段为空字符串：没有对应资源。
- `backgroundColor` 分量全部为 `0`：没有有效背景色。
- `classType = 3`：无职业限制。
- `breaker.type = 0`：武器自身和固有框架均没有反勇士属性，此时 `breaker.hash` 为 `0`，文本和图标路径为空。
- 异域武器优先使用自身的固有反勇士属性；非异域武器使用固有框架引用的 `SandboxPerk` 反勇士属性。
- `breaker.icon`：Bungie 图标相对路径，前端按通用图标规则拼接资源域名。
- `version.powerCapHashes = []`：没有版本上限信息。

## 11. 前端选项来源

当前已有独立接口：

- 武器类型：`GET /api/destiny2/weapon/types`
- 特性：`GET /api/destiny2/weapon/perks`

当前没有独立选项接口：

- 制造商列表。
- 面板属性列表。
- PowerCap／版本列表。

第一版建议：

- 弹药、元素、槽位、稀有度、反勇士和职业使用固定枚举。
- 面板属性根据武器响应中的 `stats` 动态整理。
- 制造商根据武器响应中的 `foundries` 动态去重。
- 如果筛选页加载前必须获得完整制造商和属性选项，需要后端补充武器过滤元数据接口。
