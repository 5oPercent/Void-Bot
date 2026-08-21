# Destiny 2 武器接口变更汇总

本文档面向前端开发，汇总本次 Destiny 2 武器筛选、返回数据和缓存管理相关接口变更。

## 1. 武器筛选接口

```http
POST /api/Destiny2/weapon/godroll/filter
Content-Type: application/json
```

### 1.1 请求结构

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
  isCraftable?: boolean;
  statFilters?: WeaponStatFilter[];
  foundryTraitIds?: string[];
  currentVersions?: number[];
  powerCapHashes?: number[];
  isAdept?: boolean;
  isHolofoil?: boolean;
  classTypes?: number[];
  perkGroups?: PerkGroup[];
  language?: string;
}

interface WeaponStatFilter {
  statHash: number;
  minimumValue?: number;
  maximumValue?: number;
}
```

空数组不限制对应维度；可空布尔参数未传时不参与筛选。

`pageIndex` 从 `1` 开始；`pageSize` 默认为 `20`，允许范围为 `1` 至 `100`。分页在全部筛选和稳定排序完成后执行。

### 1.2 新增筛选能力

| 字段 | 说明 |
| --- | --- |
| `ammoTypes` | 子弹类型 |
| `damageTypes` | 武器元素属性 |
| `weaponSlots` | 动能、能量、威能栏位 |
| `rarities` | 武器稀有度 |
| `breakerTypes` | 屏障、过载、势不可挡等反勇士类型 |
| `isCraftable` | 是否可制作 |
| `statFilters` | 按武器面板属性 Hash 和数值范围筛选 |
| `foundryTraitIds` | 按制造商或铸造厂 Trait ID 筛选 |
| `currentVersions` | 当前版本索引 |
| `powerCapHashes` | 力量上限 Hash |
| `isAdept` | 是否为专家版本 |
| `isHolofoil` | 是否为全息版本 |
| `classTypes` | 职业限制 |

## 2. 武器响应结构

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
  weaponType: WeaponType;
  ammoType: number;
  damageType: number;
  weaponSlot: number;
  rarity: number;
  rarityName: string;
  breaker: WeaponBreaker;
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
```

### 2.1 反勇士属性

反勇士类型和展示信息组合在同一个 `breaker` 对象中。响应不再额外提供顶层 `breakerType`，筛选请求仍使用 `breakerTypes`。

```ts
interface WeaponBreaker {
  type: number;
  hash: number;
  name: string;
  description: string;
  icon: string;
}
```

示例：

```json
{
  "breaker": {
    "type": 1,
    "hash": 123456789,
    "name": "屏障勇士",
    "description": "可以穿透屏障勇士的护盾",
    "icon": "/common/destiny2_content/icons/barrier.png"
  }
}
```

异域武器优先读取武器定义自身的反勇士标识；非异域武器根据固有框架引用的 `SandboxPerk` 获得反勇士标识。两种来源都未命中时，`type` 和 `hash` 为 `0`，名称、描述和图标路径为空。

### 2.2 外观资源

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

### 2.3 完整武器面板

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

### 2.4 制造商或铸造厂

```ts
interface WeaponFoundry {
  traitHash: number;
  traitId: string;
  name: string;
  description: string;
  icon: string;
}
```

### 2.5 版本与强化信息

```ts
interface WeaponVersion {
  currentVersion: number;
  powerCapHashes: number[];
  displayWatermarkIcons: string[];
  infusionCategoryHash: number;
  infusionCategoryName: string;
}
```

所有图标、截图和水印字段均为 Bungie 相对路径，前端需要按通用资源规则拼接 Bungie 资源域名。

## 3. 武器缓存重建接口

```http
POST /api/Destiny2/weapon/cache/rebuild
Content-Type: application/json
```

### 3.1 请求

```json
{
  "languages": ["en", "zh-cht", "zh-chs"]
}
```

未传 `languages` 或传入空数组时，使用服务端 `Destiny2:SyncLanguages` 配置。

### 3.2 响应

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

任务成功入队时 HTTP 状态和响应 `code` 均为 `202`。该响应只表示任务已接受，不表示缓存已经重建完成。后台任务只读取 Redis 中已经缓存的 Manifest，不会访问 Bungie 下载清单；各语言的执行结果通过后台任务日志记录。

## 4. 清单和派生缓存流程

Manifest 同步接口现在只负责下载并缓存清单组件，不会自动创建或刷新武器派生缓存。

上线或 Manifest 更新后的调用顺序：

1. 调用 `POST /api/Sync/destiny2/manifest` 同步清单。
2. 等待后台清单同步任务完成。
3. 调用 `POST /api/Destiny2/weapon/cache/rebuild` 提交武器派生缓存后台任务。
4. 等待后台重建任务完成。
5. 调用武器查询接口。

武器派生缓存依赖以下 Manifest 组件：

```text
InventoryItem
PlugSet
Season
Stat
Trait
BreakerType
SandboxPerk
```

武器派生缓存版本已升级为 `V5`。缺少任一必需组件时，对应语言重建失败，不会混用其他语言的数据。

## 5. 相关接口

| 方法 | 路径 | 说明 |
| --- | --- | --- |
| `GET` | `/api/Destiny2/weapon/types` | 获取武器类型 |
| `GET` | `/api/Destiny2/weapon/perks` | 获取武器可用特性 |
| `POST` | `/api/Destiny2/weapon/godroll/filter` | 筛选符合条件的武器 |
| `POST` | `/api/Sync/destiny2/manifest` | 同步 Manifest 原始清单 |
| `POST` | `/api/Destiny2/weapon/cache/rebuild` | 从已缓存清单重建武器派生缓存 |
