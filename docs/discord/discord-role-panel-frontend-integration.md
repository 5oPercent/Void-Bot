# Discord 角色面板前端对接说明

本文档面向 VoidBot 管理后台前端，用于完成 Discord 角色面板的服务器选择、频道选择、角色选择、配置保存和消息发布。

前端只调用 VoidBot REST API。Discord 消息发送、Button 生成、Button 点击响应和角色分配均由后端 Bot 完成，不需要在 WASM 或其他前端项目中实现。

## 一、认证前提

所有接口都需要 Void 登录态：

```http
Authorization: Bearer {voidAccessToken}
```

当前 Void 用户还必须满足：

1. 已绑定 Discord 账号；
2. Discord OAuth 授权范围包含 `identify guilds`；
3. 当前 Discord 用户是目标 Guild 的 Owner，或者拥有 Administrator / Manage Guild 权限；
4. VoidBot 已加入目标 Guild。

前端不需要也不应该读取或保存 Discord Access Token。

除 `/api/Discord/managed-guilds` 外，本文档中的其他接口都会根据 `guildId` 检查 Bot 是否已加入 Guild，以及当前用户是否有权管理该 Guild。

## 二、统一约定

### 1. 统一响应结构

带业务数据的接口：

```json
{
  "code": 200,
  "message": "",
  "data": {}
}
```

不带业务数据的接口：

```json
{
  "code": 200,
  "message": ""
}
```

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `code` | `number` | 业务状态码，成功时为 `200` |
| `message` | `string` | 错误或提示信息，成功时可能为空字符串 |
| `data` | `object \| array \| null` | 具体业务数据；非泛型响应没有该字段 |

前端应优先根据响应体的 `code` 判断业务是否成功，不要只依赖 HTTP 状态码。

### 2. ID 类型

`guildId`、`channelId`、`roleId`、`messageId` 都是 Discord Snowflake，必须使用 `string`，不能转换为 JavaScript `number`。`panelId` 和项目 `id` 是 UUID 字符串。

### 3. Button 可选值

`style` 只允许以下值（服务端比较时不区分大小写，前端应提交标准写法）：

| 值 | 显示效果 |
| --- | --- |
| `Primary` | 蓝色 |
| `Secondary` | 灰色，默认值 |
| `Success` | 绿色 |
| `Danger` | 红色 |

不支持 `Link` 和 `Premium`。

`mode` 只允许以下值：

| 值 | 行为 |
| --- | --- |
| `Toggle` | 没有角色时添加，已有角色时移除；默认值 |
| `AddOnly` | 只添加角色，已有角色时不移除 |

## 三、接口总览

| 方法 | 路径 | 请求 | 响应数据 | 功能 |
| --- | --- | --- | --- | --- |
| GET | `/api/Discord/managed-guilds` | 无 | `DiscordManagedGuildDto[]` | 获取当前用户可管理的 Guild |
| GET | `/api/Discord/guilds/channels` | Query: `guildId` | `DiscordGuildChannelDto[]` | 获取 Guild 频道 |
| GET | `/api/Discord/guilds/roles` | Query: `guildId` | `DiscordGuildRoleDto[]` | 获取 Guild 角色 |
| GET | `/api/Discord/role-panel/config` | Query: `guildId` | `DiscordRolePanelDto \| null` | 获取面板配置 |
| POST | `/api/Discord/role-panel/config/save` | `SaveDiscordRolePanelCommand` | `DiscordRolePanelDto` | 新建或覆盖保存 |
| POST | `/api/Discord/role-panel/config/delete` | `{ guildId }` | 无 | 删除数据库配置 |
| POST | `/api/Discord/role-panel/publish` | `{ guildId }` | `DiscordRolePanelPublishDto` | 发布 Discord 消息 |
| POST | `/api/Discord/role-panel/refresh` | `{ guildId }` | `DiscordRolePanelPublishDto` | 刷新已发布消息 |
| POST | `/api/Discord/role-panel/unpublish` | `{ guildId }` | 无 | 删除消息并保留配置 |

## 四、GET `/api/Discord/managed-guilds`

无 Query 参数和请求体。返回当前用户拥有或具备管理权限的 Guild。

```json
{
  "code": 200,
  "message": "",
  "data": [
    {
      "id": "123456789012345678",
      "name": "My Server",
      "iconUrl": "https://cdn.discordapp.com/icons/123456789012345678/abc.png",
      "isOwner": true,
      "isBotInstalled": true,
      "canManageGuild": true
    }
  ]
}
```

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `id` | `string` | Guild ID |
| `name` | `string` | Guild 名称 |
| `iconUrl` | `string \| null` | Guild 图标地址 |
| `isOwner` | `boolean` | 当前 Discord 用户是否为 Owner |
| `isBotInstalled` | `boolean` | VoidBot 是否已加入 |
| `canManageGuild` | `boolean` | 是否可管理；当前返回列表中的值为 `true` |

`isBotInstalled = false` 时应禁用角色面板配置入口，而不是当作接口错误。空数组表示没有可管理 Guild。

## 五、GET `/api/Discord/guilds/channels`

请求：

```http
GET /api/Discord/guilds/channels?guildId=123456789012345678
```

| Query 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `guildId` | `string` | 是 | 目标 Guild ID |

成功数据示例：

```json
[
  {
    "id": "222222222222222222",
    "name": "role-selection",
    "type": "Text",
    "parentId": "111111111111111111",
    "position": 3,
    "usableAsTrigger": false,
    "usableAsCreateCategory": false,
    "usableAsRolePanel": true,
    "configured": false,
    "configId": null
  }
]
```

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `id` | `string` | Channel ID |
| `name` | `string` | 频道名称 |
| `type` | `Text \| News \| Voice \| Category` | 频道类型 |
| `parentId` | `string \| null` | 父分类 ID |
| `position` | `number` | Discord 排序位置 |
| `usableAsTrigger` | `boolean` | 是否为语音触发频道 |
| `usableAsCreateCategory` | `boolean` | 是否为创建分类 |
| `usableAsRolePanel` | `boolean` | 是否可发布角色面板 |
| `configured` | `boolean` | 是否被语音频道配置使用，与角色面板无关 |
| `configId` | `string \| null` | 关联语音配置 ID，与角色面板无关 |

角色面板频道下拉框只展示 `usableAsRolePanel = true` 的项目（当前为 `Text`、`News`）。当前选中频道应与面板的 `channelId` 比较，不能使用 `configured` 判断。

## 六、GET `/api/Discord/guilds/roles`

请求：

```http
GET /api/Discord/guilds/roles?guildId=123456789012345678
```

| Query 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `guildId` | `string` | 是 | 目标 Guild ID |

角色数据示例：

```json
[
  {
    "id": "444444444444444444",
    "name": "Player",
    "color": "#5865F2",
    "position": 5,
    "managed": false,
    "assignable": true,
    "unassignableReason": null
  }
]
```

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `id` | `string` | Role ID |
| `name` | `string` | 角色名称 |
| `color` | `string` | 十六进制颜色 |
| `position` | `number` | 角色层级，列表按高到低排列 |
| `managed` | `boolean` | 是否为集成管理角色 |
| `assignable` | `boolean` | Bot 是否可以操作 |
| `unassignableReason` | `string \| null` | 不可操作原因 |

`assignable = false` 的角色必须禁用。当前不可分配原因包括 `@everyone cannot be assigned`、`Managed role cannot be assigned`、`Role is higher than bot's highest role`。

## 七、GET `/api/Discord/role-panel/config`

请求：

```http
GET /api/Discord/role-panel/config?guildId=123456789012345678
```

| Query 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `guildId` | `string` | 是 | 目标 Guild ID |

未配置时 `data` 为 `null`。已配置时：

```json
{
  "id": "4f2c8b64-39ef-4f93-9be4-a64d6d6c90a1",
  "guildId": "123456789012345678",
  "channelId": "222222222222222222",
  "messageId": "333333333333333333",
  "title": "选择你的身份",
  "content": "点击按钮领取或取消角色。",
  "enabled": true,
  "published": true,
  "items": [
    {
      "id": "c5e12547-cff4-43ce-88dd-b7de3bf38b54",
      "label": "🎮 玩家",
      "roleId": "444444444444444444",
      "style": "Primary",
      "mode": "Toggle",
      "sort": 0,
      "enabled": true
    }
  ]
}
```

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `id` | `string` | 面板 UUID，只读 |
| `guildId` | `string` | Guild ID |
| `channelId` | `string` | 发布频道 |
| `messageId` | `string \| null` | 已发布消息 ID |
| `title` | `string` | Embed 标题；空字符串时不生成 Embed |
| `content` | `string` | Discord 消息正文 |
| `enabled` | `boolean` | 是否允许 Button 交互 |
| `published` | `boolean` | 服务端根据 `messageId` 计算，只读 |
| `items` | `array` | Button 项目，按 `sort` 升序 |

项目包含 `id`、`label`、`roleId`、`style`、`mode`、`sort`、`enabled`。按钮文本 `label` 可以直接包含 Unicode Emoji。读取到的项目 `id` 只用于展示和回填，不要依赖它生成 Discord `custom_id`。

## 八、POST `/api/Discord/role-panel/config/save`

请求头：

```http
Content-Type: application/json
```

请求体：

```json
{
  "guildId": "123456789012345678",
  "channelId": "222222222222222222",
  "title": "选择你的身份",
  "content": "点击按钮领取或取消角色。",
  "enabled": true,
  "items": [
    {
      "label": "🎮 玩家",
      "roleId": "444444444444444444",
      "style": "Primary",
      "mode": "Toggle",
      "sort": 0,
      "enabled": true
    }
  ]
}
```

顶层字段：

| 字段 | 类型 | 必填 | 默认值 | 约束 |
| --- | --- | --- | --- | --- |
| `guildId` | `string` | 是 | 无 | 目标 Guild；每个 Guild 一个面板 |
| `channelId` | `string` | 是 | 无 | 必须为可用角色面板的 Text/News 频道 |
| `title` | `string` | 否 | `""` | 最长 256 字符 |
| `content` | `string` | 否 | `""` | 最长 4000 字符 |
| `enabled` | `boolean` | 否 | `true` | false 时点击提示面板失效 |
| `items` | `array` | 是 | 无 | 最少 1 项，最多 25 项 |

项目字段：

| 字段 | 类型 | 必填 | 默认值 | 约束 |
| --- | --- | --- | --- | --- |
| `label` | `string` | 是 | 无 | Button 文本，可以包含 Unicode Emoji，不能为空，最长 80 字符 |
| `roleId` | `string` | 是 | 无 | 必须属于 Guild 且 `assignable = true`，不能重复 |
| `style` | `string` | 否 | `Secondary` | `Primary` / `Secondary` / `Success` / `Danger` |
| `mode` | `string` | 否 | `Toggle` | `Toggle` / `AddOnly` |
| `sort` | `number` | 否 | `0` | 按升序显示，建议从 0 开始连续编号 |
| `enabled` | `boolean` | 否 | `true` | false 的项目不会生成 Button |

保存请求不要提交 `id`、`messageId`、`published` 或 `custom_id`。服务端会重建项目，因此每次保存后项目 UUID 可能变化。

保存成功返回完整 `DiscordRolePanelDto`。保存不会自动发布或刷新消息：

- 新建或未发布配置：保存成功后调用 `publish`；
- 修改已发布配置：保存成功后立即调用 `refresh`；
- 修改发布频道：必须先 `unpublish`，再 `save`，最后 `publish`。

## 九、POST `/api/Discord/role-panel/publish`

请求体：

```json
{
  "guildId": "123456789012345678"
}
```

发布会根据数据库配置在指定频道创建消息，并保存 `messageId`。成功响应：

```json
{
  "code": 200,
  "message": "",
  "data": {
    "guildId": "123456789012345678",
    "channelId": "222222222222222222",
    "messageId": "333333333333333333",
    "created": true
  }
}
```

已发布时返回 `409` 和 `Role panel is already published.`，此时应调用 `refresh`。

## 十、POST `/api/Discord/role-panel/refresh`

请求体与 `publish` 相同：

```json
{
  "guildId": "123456789012345678"
}
```

刷新会使用最新保存的标题、正文和 Button 修改原消息，成功响应中的 `created` 为 `false`。没有 `messageId`、频道不存在或消息无法找到时返回 `404 Published role panel not found.`。

## 十一、POST `/api/Discord/role-panel/unpublish`

请求体与 `publish` 相同。成功响应不包含 `data`：

```json
{
  "code": 200,
  "message": ""
}
```

接口会尝试删除 Discord 消息并清空配置中的 `messageId`；未发布或消息已被手动删除时也可以安全调用。标题、正文、频道和 Button 配置会保留。

## 十二、POST `/api/Discord/role-panel/config/delete`

请求体与 `publish` 相同，成功响应不包含 `data`。该接口只删除数据库配置，不删除已经发送到 Discord 的消息。

前端删除流程必须是：`published = true` 时先 `unpublish`，再 `config/delete`。否则 Discord 中会留下没有对应配置的旧消息，点击后只会提示面板失效。

## 十三、Button 交互

Button `custom_id` 由服务端生成，格式为 `rp:{panelId}:{itemId}`，前端不能生成或修改。点击由 Bot Gateway 事件处理：

| 条件 | 结果 |
| --- | --- |
| `Toggle` 且用户没有角色 | 添加角色 |
| `Toggle` 且用户已有角色 | 移除角色 |
| `AddOnly` 且用户没有角色 | 添加角色 |
| `AddOnly` 且用户已有角色 | 不修改并提示已拥有角色 |
| 面板、项目或消息已失效 | 提示面板已失效 |

这些操作不调用管理后台前端，也不需要前端提供回调接口。

## 十四、推荐调用流程

### 首次进入

1. `GET managed-guilds`；
2. 选择 `isBotInstalled = true` 的 Guild；
3. 并行请求 `guilds/channels`、`guilds/roles`、`role-panel/config`；
4. 根据 `config.data` 是否为 `null` 初始化或回填表单。

### 首次发布

1. 校验频道、角色、按钮文本、数量和枚举值；
2. `POST config/save`；
3. `POST publish`；
4. 用发布响应的 `messageId` 更新页面状态。

### 修改已发布配置

执行 `save → refresh`。如果修改频道，执行 `unpublish → save → publish`。

### 删除

执行 `unpublish（如已发布）→ config/delete`，成功后清空本地表单。

## 十五、常见错误

| `code` | `message` | 处理建议 |
| --- | --- | --- |
| `400` | `A Discord account must be bound first.` | 引导绑定或重新授权 Discord |
| `404` | `Discord guild is not available.` | Bot 未加入、已离开或 Guild 不可用 |
| `403` | `You do not have permission to manage this Discord guild.` | 提示当前用户无 Guild 管理权限 |
| `400` | `ChannelId is invalid.` | 重新选择频道 |
| `404` | `Discord text channel is not available.` | 刷新频道列表 |
| `400` | `Role panel items are invalid.` | 检查按钮文本、Role ID 和项目数量 |
| `400` | `Role panel contains duplicate or unsupported button settings.` | 检查重复角色及 style/mode |
| `400` | `One or more roles cannot be assigned by the bot.` | 刷新角色并检查 Bot 角色层级 |
| `404` | `Role panel or channel not found.` | 先保存配置或重新选择频道 |
| `409` | `Role panel is already published.` | 改调用 `refresh` |
| `404` | `Published role panel not found.` | 重新调用 `publish` |

```ts
type DiscordButtonStyle = "Primary" | "Secondary" | "Success" | "Danger";
type DiscordRolePanelMode = "Toggle" | "AddOnly";

interface VoidBotResponse<T = undefined> {
  code: number;
  message: string;
  data?: T;
}

interface DiscordRolePanelItem {
  id?: string;
  label: string;
  roleId: string;
  style: DiscordButtonStyle;
  mode: DiscordRolePanelMode;
  sort: number;
  enabled: boolean;
}

interface SaveDiscordRolePanelRequest {
  guildId: string;
  channelId: string;
  title: string;
  content: string;
  enabled: boolean;
  items: DiscordRolePanelItem[];
}
```
