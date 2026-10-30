# ERA 武器配置 Web 管理端对接

更新日期：2026-10-25。面向 Web 后台开发，以当前 Controller、DTO、Validator 和 Service 源码为准；本文未进行部署环境 HTTP 联调。

2026-10-30 后端框架重构已接入源码，最新契约以[武器框架与属性配置接口](武器框架与属性配置接口.md)为准。下文保留上一阶段契约作为历史记录，旧 Behavior、Range、独立映射表及接口、固定八端点及旧五集合示例不适用于新接口。评分端点现与模板属性值同行保存，操控只决定掏出时间。当前已确认只有模板数据、没有装备实例数据；Script0045 保留模板与引用并只移除射程修正项。Web／Godot 尚未同步，迁移与运行验收未执行。

范围：武器模板、槽位类型、整套槽位方案、公共改装定义、共享属性映射配置、预检和发布。玩家实例、背包、装备操作及战斗执行不在本文范围。

槽位相关契约使用 `slotTypes`、`slotConfigurations`、`slotConfigurationId`、`slotTypeId`；Script0040 按空武器业务表升级结构，不执行旧数据转换。完整规则见 [武器改装槽位方案](武器改装槽位方案.md)。

接口及字段统一采用 `weapon-stat-mappings`、`statMappingId`、`statMappings`。已部署数据库升级需执行 `Script0039_rename_era_weapon_stat_mapping.sql`，调整表、列和外键名称，历史脚本保持原样。已确认没有旧业务数据，不增加历史 JSON 数据迁移或旧字段兼容。Web 与 Godot 应同步使用新契约；目标环境的迁移执行状态需部署时确认。

## 1. 当前可接入范围

| 功能 | 当前接口能力 |
| --- | --- |
| 武器模板 | 分页、详情、新建／完整更新、停用、删除 |
| 公共改装 | 分页、详情、新建／完整更新、删除 |
| 槽位类型 | 分页、详情、新建／完整更新、删除 |
| 整套槽位方案 | 分页、详情、新建／完整更新、删除；槽位和候选池独立维护 |
| 共享属性映射配置 | 分页、详情、新建／完整更新、删除；也支持随完整目录预检、发布 |
| 完整武器目录 | `GET /api/era/manifest/equipment` 的 `data.weapons`；支持完整目录预检和原子发布 |
| 草稿 | 无后端草稿；Web 可以维护未提交表单，保存成功就是发布 |

完整目录已有清单接口，包含模板、改装、属性映射、槽位类型及方案五类集合，版本位于 `data.version`。该清单路径目前仍要求有效游戏客户端会话；独立 Admin 管理接口不要求游戏会话，Web 可直接完成五类对象的单项管理。纯 Web 批量发布时，也可以按第 3.4 节从五类管理分页完整读取同一版本目录。

`GET /api/era/admin/configuration` 与 `/configuration/export` 属于技能／BUFF 草稿，不能用于读取武器目录。武器路径中包含 `/equipment/`。

## 2. 通用约定

### 2.1 请求与权限

管理接口前缀：`/api/era/admin/equipment`。

```http
Authorization: Bearer <access_token>
Content-Type: application/json
```

- 使用现有后台登录身份和端点权限体系；超级管理员沿用既有规则，普通管理角色需授权对应方法与路径。
- `/api/era/admin/...` 已在游戏会话过滤器中豁免，**不需要 `X-ERA-Client-Id`，不需要初始化玩家或占用游戏会话**。
- 请求和响应字段使用 camelCase，枚举使用数字，Guid 使用标准 UUID 字符串。
- 新建模板、方案、类型、改装项、属性映射及方案内槽位时，Web 不生成或发送 `id`，正式 Guid 由后端生成。更新沿用已保存的 Guid，名称和表现键不作为身份。
- 路由中的 `{id}` 使用 Guid 约束。新建调用不带 ID 的集合 POST 接口，成功使用 `savedId` 定位对象；更新调用 `/{id}`，路由 ID 覆盖正文 ID，显式空 Guid 由 Validators 拒绝。
- 写入及预检契约拒绝未知 JSON 字段。发送前剔除选中状态、表格序号、校验提示等 UI 字段。
- 管理请求体上限为 1 MiB；完整武器目录序列化后也不得超过 1 MiB。
- `templates`、`modifications`、`statMappings`、`slotTypes`、`slotConfigurations` 五个集合各最多 10000 项；数量上限与目录字节上限分别校验，未到数量上限也可能因体积过大被拒绝。

### 2.2 响应与成功判断

```typescript
interface ApiResponse<T> {
  code: number;
  message: string;
  errorCode?: string;
  data?: T;
}

interface VersionedRead<T> extends ApiResponse<T> {
  version: number; // 模板／改装／属性映射配置列表及详情：版本在响应顶层
}

interface Page<T> {
  pageIndex: number;
  pageSize: number;
  total: number;
  totalPages: number;
  items: T[];
  hasPrevPage: boolean;
  hasNextPage: boolean;
}

interface ConfigurationResult {
  savedId?: Guid | null;
  version: number; // 所有写入：版本在 data 内
  configuration: WeaponCatalog; // 成功后返回完整武器目录
}

interface ConfigurationValidation {
  version: number; // 预检读取到的当前已发布版本
  errors: string[];
}
```

先判断 HTTP 状态，再判断响应体 `code === 200`。业务异常和参数校验失败可能使用 HTTP 200，但响应体 `code` 为 400、404、409、503 等；认证／权限拦截还可能直接返回 HTTP 401／403。失败响应的 `data` 可能不存在，`errorCode` 也可能省略。

预检还需判断 `data.errors.length === 0`。`code=200` 只代表预检请求执行成功，不代表配置有效。

成功时 `message` 可能为空，不应以文字内容判断结果。错误分支优先使用稳定 `errorCode`，`message` 用于展示。

### 2.3 版本字段

所有模板、改装和属性映射配置共用一个装备目录发布版本。Web 将读取到的版本作为 `expectedManifestVersion` 原样提交。

| 操作 | 当前版本读取位置 | 成功后的版本读取位置 |
| --- | --- | --- |
| 模板／改装／属性映射配置分页、详情 | 响应顶层 `version` | 不写入 |
| 装备清单 | `data.version`，完整武器目录为 `data.weapons` | 不写入 |
| 预检 | `data.version` | 不写入、不预留版本 |
| 保存、停用、删除、完整发布 | 请求 `expectedManifestVersion` | `data.version` |

`expectedManifestVersion` 必须大于 0、小于 C# `long.MaxValue`。这是全局发布版本，不是单个模板版本，也不是玩家实例版本。Web 不自行增加版本；每次成功写入增加一次，即使提交内容与当前一致，也会形成一次新发布。

版本在 JSON 中为整数。Web 应保留整数精度并原样回传；不要将可能超过安全整数范围的版本四舍五入。

## 3. 管理接口清单

下表路径均相对于 `/api/era/admin/equipment`。

| 方法 | 路径 | 请求 | 成功响应 |
| --- | --- | --- | --- |
| GET | `/weapon-templates` | Query：`pageIndex`、`pageSize`、`search` | `VersionedRead<Page<WeaponTemplate>>` |
| GET | `/weapon-templates/{id}` | 路由 Guid | `VersionedRead<WeaponTemplate>` |
| POST | `/weapon-templates` | `{ expectedManifestVersion, template }`，对象不含 `id` | `ApiResponse<ConfigurationResult>`，含 `savedId` |
| POST | `/weapon-templates/{id}` | `{ expectedManifestVersion, template }` | `ApiResponse<ConfigurationResult>` |
| POST | `/weapon-templates/{id}/disable` | `{ expectedManifestVersion }` | `ApiResponse<ConfigurationResult>` |
| POST | `/weapon-templates/{id}/delete` | `{ expectedManifestVersion }` | `ApiResponse<ConfigurationResult>` |
| GET | `/weapon-modifications` | Query：`pageIndex`、`pageSize`、`search`、`slotTypeId` | `VersionedRead<Page<WeaponModification>>` |
| GET | `/weapon-modifications/{id}` | 路由 Guid | `VersionedRead<WeaponModification>` |
| POST | `/weapon-modifications` | `{ expectedManifestVersion, modification }`，对象不含 `id` | `ApiResponse<ConfigurationResult>`，含 `savedId` |
| POST | `/weapon-modifications/{id}` | `{ expectedManifestVersion, modification }` | `ApiResponse<ConfigurationResult>` |
| POST | `/weapon-modifications/{id}/delete` | `{ expectedManifestVersion }` | `ApiResponse<ConfigurationResult>` |
| GET | `/weapon-stat-mappings` | Query：`pageIndex`、`pageSize`、`search` | `VersionedRead<Page<WeaponStatMapping>>` |
| GET | `/weapon-stat-mappings/{id}` | 路由 Guid | `VersionedRead<WeaponStatMapping>` |
| POST | `/weapon-stat-mappings` | `{ expectedManifestVersion, statMapping }`，对象不含 `id` | `ApiResponse<ConfigurationResult>`，含 `savedId` |
| POST | `/weapon-stat-mappings/{id}` | `{ expectedManifestVersion, statMapping }` | `ApiResponse<ConfigurationResult>` |
| POST | `/weapon-stat-mappings/{id}/delete` | `{ expectedManifestVersion }` | `ApiResponse<ConfigurationResult>` |
| GET | `/weapon-slot-types` | Query：`pageIndex`、`pageSize`、`search` | `VersionedRead<Page<WeaponSlotType>>` |
| GET | `/weapon-slot-types/{id}` | 路由 Guid | `VersionedRead<WeaponSlotType>` |
| POST | `/weapon-slot-types` | `{ expectedManifestVersion, slotType }`，对象不含 `id` | `ApiResponse<ConfigurationResult>`，含 `savedId` |
| POST | `/weapon-slot-types/{id}` | `{ expectedManifestVersion, slotType }` | `ApiResponse<ConfigurationResult>` |
| POST | `/weapon-slot-types/{id}/delete` | `{ expectedManifestVersion }` | `ApiResponse<ConfigurationResult>` |
| GET | `/weapon-slot-configurations` | Query：`pageIndex`、`pageSize`、`search` | `VersionedRead<Page<WeaponSlotConfiguration>>` |
| GET | `/weapon-slot-configurations/{id}` | 路由 Guid | `VersionedRead<WeaponSlotConfiguration>` |
| POST | `/weapon-slot-configurations` | `{ expectedManifestVersion, slotConfiguration }`，对象不含 `id` | `ApiResponse<ConfigurationResult>`，含 `savedId` |
| POST | `/weapon-slot-configurations/{id}` | `{ expectedManifestVersion, slotConfiguration }` | `ApiResponse<ConfigurationResult>` |
| POST | `/weapon-slot-configurations/{id}/delete` | `{ expectedManifestVersion }` | `ApiResponse<ConfigurationResult>` |
| POST | `/configuration/validate` | `{ configuration }` | `ApiResponse<ConfigurationValidation>` |
| POST | `/configuration/publish` | `{ expectedManifestVersion, configuration }` | `ApiResponse<ConfigurationResult>` |

### 3.1 分页与详情

- `pageIndex` 从 1 开始，默认 1；`pageSize` 默认 20，范围 1～100。
- `search` 可省略，最大 128 字符；按 ID 或名称进行不区分大小写的包含匹配。
- 改装列表的 `slotTypeId` 可省略；指定时使用类型字典的 Guid。
- 当前没有模板启用状态、武器大类或自定义排序查询参数。
- `data.items` 即完整单项 DTO；详情接口返回同一结构。
- 分页响应顶层版本可能在翻页期间变化。需要组成一致快照时，版本变化应重新读取，不能混合不同版本。

请求示例：

```http
GET /api/era/admin/equipment/weapon-templates?pageIndex=1&pageSize=20&search=步枪
GET /api/era/admin/equipment/weapon-modifications?pageIndex=1&pageSize=100&slotTypeId=50000000-0000-0000-0000-000000000004
GET /api/era/admin/equipment/weapon-stat-mappings?pageIndex=1&pageSize=20&search=步枪
```

空列表响应示例：

```json
{
  "code": 200,
  "message": "",
  "version": 7,
  "data": {
    "pageIndex": 1,
    "pageSize": 20,
    "total": 0,
    "totalPages": 0,
    "items": [],
    "hasPrevPage": false,
    "hasNextPage": false
  }
}
```

### 3.2 单项保存、停用和删除

模板、改装和属性映射保存均为 **upsert + 整项替换**。不支持通过省略字段保持原值的 PATCH 语义，应发送完整单项 DTO。

```typescript
// selectedTemplate 为完整 WeaponTemplate；currentVersion 来自最新有效读取。
const templateBody = {
  expectedManifestVersion: currentVersion,
  template: selectedTemplate,
};

const modificationBody = {
  expectedManifestVersion: currentVersion,
  modification: selectedModification,
};

const statMappingBody = {
  expectedManifestVersion: currentVersion,
  statMapping: selectedStatMapping, // 完整 WeaponStatMapping
};
```

停用／删除正文示例：

```json
{ "expectedManifestVersion": 7 }
```

- 停用只将 `isGenerationEnabled` 设为 false，不删除模板。重新启用使用完整模板保存，将该字段设为 true。
- 停用只阻止新生成；已有武器及已接受的固定奖励计划仍可使用模板。
- 单项保存只替换指定模板、改装或共享属性映射配置，保留目录内其他对象；成功响应仍返回完整 `configuration`。
- 单项模板保存引用的属性映射配置、候选池改装必须已经存在于当前目录。首次配置依次保存类型、公共改装、槽位方案与属性映射，每次采用成功返回的新版本，再保存模板；完整目录发布用于统一提交批量关联变更。
- 删除模板时检查装备资产、奖励池、待发奖励计划等引用；改装仍在方案池或固定实例候选中时不能删除。
- 共享属性映射配置被任一模板引用时，单项删除返回 `409 / era.weapon_reference_conflict`，停用的模板也计入引用。先更换模板引用，再删除；也可在一次完整发布中同时调整引用和删除配置。
- 属性映射保存会校验名称、Guid 和八个端点，沿用第 4 节约束；非法端点返回 `400 / era.weapon_invalid_configuration`。不存在的详情或删除目标返回 `404 / era.weapon_not_found`。
- 缩小模板兼容穿戴位置、修改改装槽位、使旧实例引用不再成立等变更也可能被引用保护拒绝。

引用相关失败不能只处理 409：当前服务先校验拟发布目录，再检查资产引用。删除仍在方案候选池中的改装，会因池引用缺失返回 `400 / era.weapon_invalid_configuration`；改装已移出池，但仍被实例候选或待发奖励计划引用时，返回 `409 / era.weapon_reference_conflict`。单项删除被模板引用的属性映射有显式保护，返回 409；完整发布遗漏仍被模板引用的映射则属于目录校验失败，返回 400。

### 3.3 完整目录预检与发布

`configuration` 的类型是 **WeaponCatalog 本身**，只包含 `templates`、`modifications`、`statMappings`、`slotTypes`、`slotConfigurations` 五个集合，不是整个装备 manifest，也没有外层 `weapons` 字段。

预检正文：

```typescript
const validationBody = { configuration: completeCatalog };
```

预检不通过的响应示例：

```json
{
  "code": 200,
  "message": "",
  "data": {
    "version": 7,
    "errors": ["武器配置无效，请检查属性、效果端点、行为、改装槽与候选数量"]
  }
}
```

`errors` 当前是本地化文字数组，没有字段路径或逐项错误编号。Web 可以先做字段级校验，再展示后端预检结果；不要把文案解析成字段标识。

预检通过后，以读取到的当前版本提交完整目录。发布会再次检查版本、配置和引用。**遗漏的对象视为删除，不是保持原值。**尤其不能将某个分页列表、搜索结果或仅本次修改的对象作为完整目录发布。

共享属性映射配置也可以通过第 3.2 节的单项接口维护。完整发布中删除某属性映射配置时，目标目录内所有模板必须已经解除或更换其引用；同一次发布可以保存属性映射配置、公共改装及模板之间的引用。

### 3.4 读取完整武器目录

现有 `GET /api/era/manifest/equipment` 返回装备清单：发布版本在 `data.version`，武器目录为 `data.weapons`，包含 `templates`、`modifications`、`statMappings`、`slotTypes`、`slotConfigurations`。预检／发布时只取 `data.weapons` 作为 `configuration`，不要提交整个装备清单。

该路径沿用游戏端鉴权：需要 Bearer 身份及已获取有效会话的 `X-ERA-Client-Id`；仅传一个随机客户端 Guid 不会自动获取会话。本文新增的四个属性映射配置接口属于 Admin 路径，不需要此会话。

纯 Web 也可无游戏会话读取完整目录：分别完整遍历模板、改装、属性映射、类型及方案五个 Admin 分页接口，不传 `search` 或 `slotTypeId`，取全每类的所有页。所有响应顶层 `version` 必须相同，任意一页版本变化均丢弃此次采集结果并重新读取。只拼模板与改装两类、漏页、混合不同版本或使用筛选结果，都不能作为完整发布目录。

初始建库脚本 Script0029 已创建版本为 1 的固定空装备目录行；没有武器配置时，管理分页应返回当前版本和空列表。Web 必须采用实际返回版本，不能把示例版本或 1 写死。若返回 `503 / era.weapon_config_unavailable`，应检查目标环境迁移及目录状态，不能当作空目录继续发布；武器管理接口不会自动初始化缺失的目录行。

## 4. 数据结构

以下 TypeScript 类型与当前 JSON 字段对应，供 Web 建模使用，不包含额外的 UI 状态。

```typescript
type Guid = string;
type WeaponEquipmentSlot = 101 | 102 | 103;
type WeaponStat = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11;
type ModifierOperation = 1 | 2;

interface WeaponCatalog {
  templates: WeaponTemplate[];
  modifications: WeaponModification[];
  statMappings: WeaponStatMapping[];
  slotTypes: WeaponSlotType[];
  slotConfigurations: WeaponSlotConfiguration[];
}

interface WeaponTemplate {
  id: Guid;
  name: string;
  icon: string;
  note: string;
  isGenerationEnabled: boolean;
  presentationKey: string;
  statMappingId: Guid;
  baseStats: WeaponStats;
  behavior: WeaponBehavior;
  compatibleSlots: WeaponEquipmentSlot[];
  slotConfigurationId: Guid;
}

interface WeaponStats {
  damage: number;
  frequency: number;
  loadedCapacity: number;
  reserveCapacity: number;
  reloadSpeed: number;
  range: number;
  accuracy: number;
  stability: number;
  handling: number;
  projectileSpeed: number | null;
  chargeTime: number | null;
}

interface WeaponBehavior {
  fireMode: 1 | 2 | 3 | 4;
  hitMode: 1 | 2;
  reloadMode: 1 | 2;
  baseReloadSeconds: number;
  burstCount: number;
}

interface WeaponSlotType {
  id: Guid;
  name: string;
}

interface WeaponSlotConfiguration {
  id: Guid;
  name: string;
  note: string;
  slots: WeaponConfigurationSlot[];
}

interface WeaponConfigurationSlot {
  id: Guid;
  name: string;
  slotTypeId: Guid;
  candidateCount: number;
  displayOrder: number;
  pool: Array<{ modDefinitionId: Guid; weight: number }>;
}

interface WeaponModification {
  id: Guid;
  name: string;
  slotTypeId: Guid;
  description: string;
  icon: string;
  modifiers: Array<{
    stat: WeaponStat;
    operation: ModifierOperation;
    value: number;
  }>;
}

interface WeaponStatMapping {
  id: Guid;
  name: string;
  spreadDegreesAtZero: number;
  spreadDegreesAtHundred: number;
  recoilDegreesAtZero: number;
  recoilDegreesAtHundred: number;
  readySecondsAtZero: number;
  readySecondsAtHundred: number;
  aimSecondsAtZero: number;
  aimSecondsAtHundred: number;
}
```

### 4.1 模板公共字段

| 字段 | 填写与约束 |
| --- | --- |
| `id` | 只读的后端生成 Guid；新建请求不发送，更新沿用；目录内不重复 |
| `name` | 非空白，最长 128 字符 |
| `icon` | 字符串，最长 256 字符，可为空；图标引用，不是物品身份 |
| `note` | 字符串，最长 2048 字符，可为空 |
| `isGenerationEnabled` | 是否允许新生成；新建默认 true |
| `presentationKey` | 非空，最长 128 字符，无首尾空白；仅允许 ASCII 字母、数字、`.`、`_`、`-`，如 `scifi_rifle` |
| `statMappingId` | 必须引用同目录中存在的属性映射配置 Guid |
| `compatibleSlots` | 非空、不重复，仅允许 101、102、103 |
| `slotConfigurationId` | 必须引用同目录中的整套槽位方案；模板不内嵌槽位或候选池 |

没有 `kind` 或武器大类字段；也没有模板品阶、玩家当前弹药量、单件武器最终属性等字段。

`presentationKey` 只映射整枪表现，改装不改变模型。Web 可编辑该字符串或提供已有表现键选项，当前没有后端表现资源枚举接口。

### 4.2 十一项基础属性

所有数字必须有限。Web 统一提交 JSON 数字，不发送 NaN、Infinity 或字符串形式的数字。

| 属性编号 | 字段 | 单位／填写含义 | 后端范围 |
| --- | --- | --- | --- |
| 1 | `damage` | 单发基础伤害 | `0 < value ≤ 1000000` |
| 2 | `frequency` | 次／秒；10 表示 600 RPM | `0 < value ≤ 1000000` |
| 3 | `loadedCapacity` | 装载上限，发 | 整数 1～1000000 |
| 4 | `reserveCapacity` | 每把武器独立储备上限，发 | 整数 0～1000000 |
| 5 | `reloadSpeed` | 速度系数；1 为基础速度，1.2 表示提高 20% | `0 < value ≤ 1000000` |
| 6 | `range` | 米；实际命中、衰减规则由游戏执行层解释 | `0 < value ≤ 1000000` |
| 7 | `accuracy` | 精准度评分，非百分比、非角度 | 0～100 |
| 8 | `stability` | 稳定性评分，非百分比、非角度 | 0～100 |
| 9 | `handling` | 操控性评分，非百分比、非秒数 | 0～100 |
| 10 | `projectileSpeed` | 米／秒；仅飞行投射物有值 | null 或 `0 < value ≤ 1000000` |
| 11 | `chargeTime` | 完整蓄力时间，秒 | null 或 `0 < value ≤ 3600` |

射线武器的 `projectileSpeed` 必须为 null；非蓄力武器的 `chargeTime` 必须为 null。Web 切换相关行为时应同步清理或补齐这两个值，不能用 0 代替“不适用”。

评分可以填写小数；容量必须为整数。若 UI 使用 RPM 输入，提交前除以 60，避免把 600 直接作为每秒频率发送。

### 4.3 行为配置

| 字段 | 数值与约束 |
| --- | --- |
| `fireMode` | 1 半自动、2 全自动、3 点射、4 蓄力 |
| `hitMode` | 1 即时射线、2 飞行投射物 |
| `reloadMode` | 1 整弹匣装填、2 逐发装填 |
| `baseReloadSeconds` | 基础装填秒数，有限且 `0 < value ≤ 3600` |
| `burstCount` | 整数 1～64；普通全自动先填 1 |

后端接受上表已定义的模式编号；当前游戏首版计划只执行全自动步枪，配置接口接受不等于对应游戏行为已经实现。

实际装填秒数为 `baseReloadSeconds / 最终 reloadSpeed`。例如基础 2.4 秒、速度系数 0.8 时，实际为 3 秒。

行为中不再有 `baseSpreadDegrees`、`baseRecoilDegrees`、`baseReadySeconds`、`baseAimSeconds`，发送这些字段会被拒绝。

### 4.4 独立槽位方案与候选池

槽位类型由 `slotTypes` 定义，可按需增加。整套方案通过 `slots` 保存零个或多个槽位，同类型允许多个槽位。具体槽位使用独立 UUID；穿戴位置 101／102／103 仍使用原装备枚举。方案与类型的名称必填、最多 128 字符，方案 `note` 最多 2048 字符。

| 字段 | 约束 |
| --- | --- |
| `id` | 已保存槽位的只读 Guid；新增、复制或变更类型时不发送，后端生成；目录内唯一 |
| `name` | 槽位显示名称，非空白，最长 128 字符 |
| `slotTypeId` | 引用同目录的适配类型；同一方案可重复使用同一类型 |
| `candidateCount` | 必填的生成数量，整数 1～16 |
| `displayOrder` | 非负整数；按此值和槽位 UUID 排序 |
| `pool` | 最多 256 项，数量至少等于 `candidateCount` |
| `pool[].modDefinitionId` | 必须引用同目录公共改装，改装的 `slotTypeId` 必须与本槽一致，池内不重复 |
| `pool[].weight` | 正整数；同一池权重总和不得超过 2147483647 |

生成按权重不放回抽取，每个槽首个抽出的候选初始选中。`candidateCount` 不是池总数，也不是可同时安装数量；每个槽同时只选一个候选。

生成数量、池和权重变更只影响新武器，不会补齐、截断或重抽已有实例候选。Web 不编辑玩家实例候选。

方案数量及每方案槽位最多 10000 项，同时受完整目录 1 MiB 限制。编辑槽位类型时应分配新的槽位 ID；排序和改名保留 ID。方案更换、增删或排序只影响新生成，已有实例保留名称、类型、顺序与候选快照。所有关联模板均参与属性兼容校验。

### 4.5 公共改装与修正规则

改装 `id` 为非空 Guid，`name` 非空白且最长 128 字符，`description` 最长 2048 字符，`icon` 最长 256 字符；后两项可使用空字符串。

- `slotTypeId` 必须引用类型字典中的 Guid。
- `modifiers` 最多 22 条，可以为空；空规则可表示原厂候选。
- `stat` 使用十一项属性编号；`operation=1` 表示固定加值，`operation=2` 表示加算百分比。
- `value` 为有限数，绝对值不超过 1000000；百分比必须大于 -1。`0.2` 是 +20%，`-0.2` 是 -20%。
- 同一个改装中，同一 `(stat, operation)` 最多一条；同属性固定值和百分比可以各一条。
- 不支持投射物速度或蓄力时间的模板，不能在候选池中引用修改这些属性的改装。

扩容弹匣规则示例：

```json
[
  { "stat": 3, "operation": 1, "value": 10 },
  { "stat": 5, "operation": 2, "value": -0.2 }
]
```

最终属性按 `(Base + 所有 Flat 之和) × (1 + 所有 AddPercent 之和)` 汇总后限制，容量最后向下取整。Web 预览应从基础值重新计算，不在上次结果上反复加减。

若 Web 展示最终属性，需与当前 Godot 预览使用同一限制：

| 最终属性 | 汇总后的处理 |
| --- | --- |
| 精准度、稳定性、操控性 | 限制在 0～100，再用于双端值映射 |
| 装载容量 | 向下取整后限制在 1～1000000 |
| 储备上限 | 向下取整后限制在 0～1000000 |
| 其余有效属性 | 限制在 0.000001～1000000；蓄力时间上限为 3600 |

最后一行是当前客户端的数值保护，不代表正式平衡下限。不适用的投射物速度／蓄力时间保持 null，不参与修正和计算；非有限计算结果应作为预览错误处理。后端保存的是基础值与修正规则，不落库这些最终值，也不会替 Web 计算最终属性。

### 4.6 共享属性映射配置

`id` 为非空 Guid，`name` 非空白且最长 128 字符。多个模板可以引用同一配置。

| 端点字段组 | 对应评分 | 单位 | 约束 |
| --- | --- | --- | --- |
| `spreadDegreesAtZero` / `spreadDegreesAtHundred` | 精准度 | 度 | `0 ≤ Hundred ≤ Zero ≤ 180` |
| `recoilDegreesAtZero` / `recoilDegreesAtHundred` | 稳定性 | 度 | `0 ≤ Hundred ≤ Zero ≤ 180` |
| `readySecondsAtZero` / `readySecondsAtHundred` | 操控性 | 秒 | `0 < Hundred ≤ Zero ≤ 3600` |
| `aimSecondsAtZero` / `aimSecondsAtHundred` | 操控性 | 秒 | `0 < Hundred ≤ Zero ≤ 3600` |

所有端点必须有限。Zero 对应 0 分，Hundred 对应 100 分：

```text
效果 = Zero + (Hundred - Zero) × 最终评分 / 100
```

例如散布端点 3→1 度，精准度 75 时为 1.5 度，85 时为 1.3 度。端点由后端发布，调整无需更新客户端，也不复制到玩家实例。

新建对象使用集合 POST；保存成功的 `savedId` 和新版本用于后续引用与连续新增。新建对象及方案内新增槽位省略 ID，由 DTO 的 `Guid.NewGuid()` 默认值生成；显式空 Guid 由 Validators 拒绝，不由控制器或保存服务补正。完整目录仍执行统一配置和引用校验。

## 5. 完整发布请求示例

以下示例包含一个模板、一个可复用方案、五个适配类型及公共改装、一个属性映射配置。示例槽位数不构成系统数量限制。已有目录使用时，完整保留仍需存在的对象，并使用实际关联 ID 合并修改。`expectedManifestVersion=7` 仅为示意，必须换为实际读取值。示例不会自动生成玩家武器。

```http
POST /api/era/admin/equipment/configuration/publish
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "expectedManifestVersion": 7,
  "configuration": {
    "statMappings": [
      {
        "id": "40000000-0000-0000-0000-000000000001",
        "name": "步枪初始属性映射",
        "spreadDegreesAtZero": 3,
        "spreadDegreesAtHundred": 1,
        "recoilDegreesAtZero": 2,
        "recoilDegreesAtHundred": 0.5,
        "readySecondsAtZero": 1,
        "readySecondsAtHundred": 0.35,
        "aimSecondsAtZero": 0.5,
        "aimSecondsAtHundred": 0.15
      }
    ],
    "modifications": [
      {
        "id": "30000000-0000-0000-0001-000000000001",
        "name": "标准瞄具",
        "description": "",
        "icon": "",
        "modifiers": [],
        "slotTypeId": "50000000-0000-0000-0000-000000000001"
      },
      {
        "id": "30000000-0000-0000-0002-000000000001",
        "name": "标准枪管",
        "description": "",
        "icon": "",
        "modifiers": [],
        "slotTypeId": "50000000-0000-0000-0000-000000000002"
      },
      {
        "id": "30000000-0000-0000-0003-000000000001",
        "name": "标准握把",
        "description": "",
        "icon": "",
        "modifiers": [],
        "slotTypeId": "50000000-0000-0000-0000-000000000003"
      },
      {
        "id": "30000000-0000-0000-0004-000000000001",
        "name": "扩容弹匣",
        "description": "装载上限增加 10 发，装填速度降低 20%",
        "icon": "",
        "modifiers": [
          {
            "stat": 3,
            "operation": 1,
            "value": 10
          },
          {
            "stat": 5,
            "operation": 2,
            "value": -0.2
          }
        ],
        "slotTypeId": "50000000-0000-0000-0000-000000000004"
      },
      {
        "id": "30000000-0000-0000-0005-000000000001",
        "name": "标准枪托",
        "description": "",
        "icon": "",
        "modifiers": [],
        "slotTypeId": "50000000-0000-0000-0000-000000000005"
      }
    ],
    "templates": [
      {
        "id": "20000000-0000-0000-0000-000000000001",
        "name": "巡逻步枪",
        "icon": "",
        "note": "全自动射线步枪",
        "isGenerationEnabled": true,
        "presentationKey": "scifi_rifle",
        "statMappingId": "40000000-0000-0000-0000-000000000001",
        "baseStats": {
          "damage": 50,
          "frequency": 10,
          "loadedCapacity": 30,
          "reserveCapacity": 120,
          "reloadSpeed": 1,
          "range": 30,
          "accuracy": 75,
          "stability": 50,
          "handling": 50,
          "projectileSpeed": null,
          "chargeTime": null
        },
        "behavior": {
          "fireMode": 2,
          "hitMode": 1,
          "reloadMode": 1,
          "baseReloadSeconds": 2.4,
          "burstCount": 1
        },
        "compatibleSlots": [
          101,
          102,
          103
        ],
        "slotConfigurationId": "70000000-0000-0000-0000-000000000001"
      }
    ],
    "slotTypes": [
      {
        "id": "50000000-0000-0000-0000-000000000001",
        "name": "瞄具"
      },
      {
        "id": "50000000-0000-0000-0000-000000000002",
        "name": "枪管"
      },
      {
        "id": "50000000-0000-0000-0000-000000000003",
        "name": "握把"
      },
      {
        "id": "50000000-0000-0000-0000-000000000004",
        "name": "弹匣"
      },
      {
        "id": "50000000-0000-0000-0000-000000000005",
        "name": "枪托"
      }
    ],
    "slotConfigurations": [
      {
        "id": "70000000-0000-0000-0000-000000000001",
        "name": "巡逻步枪改装方案",
        "note": "",
        "slots": [
          {
            "candidateCount": 1,
            "displayOrder": 1,
            "pool": [
              {
                "modDefinitionId": "30000000-0000-0000-0001-000000000001",
                "weight": 1
              }
            ],
            "id": "60000000-0000-0000-0000-000000000001",
            "name": "瞄具",
            "slotTypeId": "50000000-0000-0000-0000-000000000001"
          },
          {
            "candidateCount": 1,
            "displayOrder": 2,
            "pool": [
              {
                "modDefinitionId": "30000000-0000-0000-0002-000000000001",
                "weight": 1
              }
            ],
            "id": "60000000-0000-0000-0000-000000000002",
            "name": "枪管",
            "slotTypeId": "50000000-0000-0000-0000-000000000002"
          },
          {
            "candidateCount": 1,
            "displayOrder": 3,
            "pool": [
              {
                "modDefinitionId": "30000000-0000-0000-0003-000000000001",
                "weight": 1
              }
            ],
            "id": "60000000-0000-0000-0000-000000000003",
            "name": "握把",
            "slotTypeId": "50000000-0000-0000-0000-000000000003"
          },
          {
            "candidateCount": 1,
            "displayOrder": 4,
            "pool": [
              {
                "modDefinitionId": "30000000-0000-0000-0004-000000000001",
                "weight": 1
              }
            ],
            "id": "60000000-0000-0000-0000-000000000004",
            "name": "弹匣",
            "slotTypeId": "50000000-0000-0000-0000-000000000004"
          },
          {
            "candidateCount": 1,
            "displayOrder": 5,
            "pool": [
              {
                "modDefinitionId": "30000000-0000-0000-0005-000000000001",
                "weight": 1
              }
            ],
            "id": "60000000-0000-0000-0000-000000000005",
            "name": "枪托",
            "slotTypeId": "50000000-0000-0000-0000-000000000005"
          }
        ]
      }
    ]
  }
}
```

用于预检时，删除根对象中的 `expectedManifestVersion`，保留 `{ "configuration": ... }`，请求 `/configuration/validate`。

本例生成后每槽只有一个候选。选中上述扩容弹匣时，最终装载容量为 40 发、装填速度系数为 0.8、装填耗时为 3 秒，射速为 600 RPM、散布为 1.5 度。模板基础容量仍保存 30，不把改装结果写回模板。

## 6. Web 编辑与恢复流程

1. 用管理列表和详情实现模板／方案／类型／改装／属性映射页面。读取版本与表单初始值一起保存，字段修改仅更新 Web 表单。
2. 对单项完整 DTO 做本地校验；现有目录已包含引用对象时，调用单项保存即可。
3. 单项属性映射配置使用独立保存接口。新增关联对象先依次调用创建接口；需要批量调整时，按第 3.4 节取得同一版本完整目录，再构建完整目标目录。
4. 完整目录先预检；`errors` 非空时展示问题并继续编辑。预检通过后仍使用明确的基线版本发布。
5. 成功后从 `data.configuration` 整体更新本地目录，从 `data.version` 更新发布基线，再刷新相关列表和详情。一次只保留一个发布请求，避免不同页签用旧版本覆盖。
6. 遇到版本冲突时保留未提交表单，重新读取并比较差异，让管理员确认后再提交；不自动换成新版本盲目重试。

Admin 写入没有 `requestId` 幂等参数。请求超时可能已经发布成功；此时先查询确认，不直接重复保存。原样重发旧 `expectedManifestVersion` 会在先前已成功时发生版本冲突，但冲突本身不能证明先前请求由谁完成。

新公共数值由游戏按下局采用规则使用。Web 保存成功代表后端当前目录已发布，不代表正在进行的对局已切换。模板基础值、改装规则、效果端点的修改不会增加玩家实例版本，也不会重抽已有实例候选。

## 7. 错误处理

下表 `code` 指响应体中的业务状态码，不能直接等同于 HTTP 状态。

| `errorCode` | `code` | Web 处理 |
| --- | --- | --- |
| `era.weapon_invalid_configuration` | 400 | 提示配置无效，保留表单；检查基础值、端点、槽位方案、候选数量和目录内引用，包括删除造成的引用缺失 |
| `era.weapon_config_version_conflict` | 409 | 目录已更新，重新读取、比较并确认后提交 |
| `era.weapon_reference_conflict` | 409 | 对象仍被引用或修改破坏现有引用；保留配置并处理引用关系 |
| `era.weapon_not_found` | 404 | 目标不存在，刷新列表；不要自动以新建方式覆盖旧编辑上下文 |
| `era.weapon_config_unavailable` | 503 | 当前目录无法读取；不要将读取失败当作空目录发布 |
| `era.weapon_unauthorized` | 401 | 当前管理身份无效，重新登录 |
| 无稳定 `errorCode` 的校验失败 | 400 | 展示 `message`，检查 JSON 字段、类型和请求参数 |
| HTTP 401／403 | 视响应而定 | 处理登录失效或端点权限不足 |

业务错误响应示例：

```json
{
  "code": 409,
  "errorCode": "era.weapon_config_version_conflict",
  "message": "装备目录版本已变化，请刷新配置后重试"
}
```

`{id:guid}` 路由不匹配时还可能直接返回 HTTP 404；请求超过大小限制也可能由 HTTP 层拒绝。Web 网络层应兼容无业务 JSON 的响应。

## 8. 联调检查

- 管理角色可以调用获授权路径，未授权角色被拒绝；无需游戏客户端会话。
- 模板、改装与属性映射配置分页／详情的版本来自响应顶层，写入版本来自 `data.version`。
- 五类对象都支持单项完整保存；关联对象按依赖顺序逐项创建，已有配置可通过完整目录一次更新。
- 删除仍在方案池中的改装按 400 处理；移出池后仍被资产引用按 409 处理。两者均保留表单并展示错误。
- 预检中 `code=200` 且 `errors` 非空时不能进入发布成功状态。
- 更新模板／改装／属性映射配置后，其他编辑页的旧版本请求发生冲突，未提交表单得以保留。
- 示例 JSON 中所有方案池引用和属性映射配置引用均可在同目录解析。
- 已存在的实例候选不因修改方案池或 `candidateCount` 改变；被引用对象的非法删除会被拒绝。
- 只允许发送契约字段，不发送 `kind`、旧四个 Base 字段、玩家实例候选或最终属性。

属性映射接口的保存同样立即发布并增加全局版本，不新增独立映射版本、数据库表或草稿流程。本文已对照源码核对，接口权限及目标环境迁移状态仍需部署联调确认。

实现细节与玩家侧边界见 [武器模板与改装后端接口](武器模板与改装接口.md)。源码入口：`EraAdminController.Equipment.cs`、`EraWeaponService.cs`、`EraWeaponConfigurationValidator.cs`、`EraClientSessionAttribute.cs`。
