# Discord 管理后台前端对接说明

本文档用于 VoidBot 管理平台前端对接 Discord Guild 和自定义语音频道配置功能。

当前版本支持：

- 读取当前 Void 用户有权限管理的 Discord Guild；
- 判断 VoidBot 是否已经加入 Guild；
- 读取已加入 Guild 的语音频道和分类；
- 配置语音频道触发创建自定义频道的规则。

当前版本暂不提供 Bot 邀请链接接口。

## 一、认证前提

所有 Discord 管理接口都需要 Void 登录态，并经过 API 的认证和权限检查。

此外，当前 Void 账号必须绑定 Discord 第三方平台，并且 Discord OAuth 授权范围包含：

```text
identify guilds
```

如果用户是旧用户，之前的 Discord Token 可能只有 `identify` 权限。此时需要重新进行一次 Discord OAuth 授权。重新授权成功后，服务端会更新当前 Discord Binding 中的 Token。

前端不需要也不应该直接使用 Discord Access Token。所有 Discord API 调用由 VoidBot 服务端完成。

## 二、统一响应结构

接口返回结构为：

```json
{
  "code": 200,
  "message": "ok",
  "data": {}
}
```

说明：

- `code` 是业务状态码；
- `message` 是提示信息；
- `data` 是接口数据；
- Discord Guild ID、Channel ID、User ID 均按字符串处理，不要转换为 JavaScript `number`。

前端应优先根据 `code` 判断业务结果，不要只根据 HTTP 状态码判断。

## 三、获取可管理 Guild

```http
GET /api/Discord/managed-guilds
Authorization: Bearer {voidAccessToken}
```

该接口以当前 Void 用户绑定的 Discord 用户为视角，返回该用户拥有或具备 Guild 管理权限的服务器。

返回示例：

```json
{
  "code": 200,
  "message": "ok",
  "data": [
    {
      "id": "123456789012345678",
      "name": "My Server",
      "iconUrl": "https://cdn.discordapp.com/icons/123456789012345678/abc.png",
      "isOwner": true,
      "isBotInstalled": true,
      "canManageGuild": true
    },
    {
      "id": "987654321098765432",
      "name": "Another Server",
      "iconUrl": null,
      "isOwner": false,
      "isBotInstalled": false,
      "canManageGuild": true
    }
  ]
}
```

字段说明：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `id` | `string` | Discord Guild ID |
| `name` | `string` | Guild 名称 |
| `iconUrl` | `string \| null` | Guild 图标地址 |
| `isOwner` | `boolean` | 当前 Discord 用户是否为 Guild Owner |
| `isBotInstalled` | `boolean` | VoidBot 是否已经加入该 Guild |
| `canManageGuild` | `boolean` | 当前用户是否具备管理资格；当前返回列表中的值为 `true` |

前端建议：

- `isBotInstalled = true`：显示“配置”按钮；
- `isBotInstalled = false`：显示“Bot 尚未加入该服务器”，暂时禁用配置按钮；
- 不要把 `isBotInstalled = false` 的 Guild 当成接口异常。

## 四、获取 Guild 频道列表

```http
GET /api/Discord/guilds/channels?guildId={guildId}
Authorization: Bearer {voidAccessToken}
```

该接口要求同时满足：

1. 当前 Void 用户有权限管理该 Guild；
2. VoidBot 已经加入该 Guild。

返回示例：

```json
{
  "code": 200,
  "message": "ok",
  "data": [
    {
      "id": "111111111111111111",
      "name": "Join to Create",
      "type": "Voice",
      "parentId": null,
      "position": 2,
      "usableAsTrigger": true,
      "usableAsCreateCategory": false,
      "configured": true,
      "configId": "4f2c8b64-39ef-4f93-9be4-a64d6d6c90a1"
    },
    {
      "id": "222222222222222222",
      "name": "Temporary Channels",
      "type": "Category",
      "parentId": null,
      "position": 1,
      "usableAsTrigger": false,
      "usableAsCreateCategory": true,
      "configured": true,
      "configId": "4f2c8b64-39ef-4f93-9be4-a64d6d6c90a1"
    }
  ]
}
```

字段说明：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `id` | `string` | Discord Channel ID |
| `name` | `string` | 频道名称 |
| `type` | `string` | 当前主要为 `Voice` 或 `Category` |
| `parentId` | `string \| null` | 父分类 ID；无父分类时为 `null` |
| `position` | `number` | Discord 频道位置 |
| `usableAsTrigger` | `boolean` | 是否可以作为触发频道 |
| `usableAsCreateCategory` | `boolean` | 是否可以作为创建频道的分类 |
| `configured` | `boolean` | 该频道是否已出现在现有配置中 |
| `configId` | `string \| null` | 关联配置 ID，没有关联配置时为 `null` |

前端选择规则：

- 触发频道下拉框只展示 `usableAsTrigger = true` 的项目；
- 创建分类下拉框只展示 `usableAsCreateCategory = true` 的项目；
- `configured = true` 的项目可以显示已配置状态，但不必禁止重新编辑；
- 频道列表为空表示该 Guild 没有可用的 Voice 或 Category 频道。

## 五、读取现有语音频道配置

```http
GET /api/Discord/voice-channel/config/list?guildId={guildId}
Authorization: Bearer {voidAccessToken}
```

`guildId` 可选。不传时返回所有 Guild 的配置；配置页面应传入当前选择的 Guild ID。

返回示例：

```json
{
  "code": 200,
  "message": "ok",
  "data": [
    {
      "id": "4f2c8b64-39ef-4f93-9be4-a64d6d6c90a1",
      "guildId": 123456789012345678,
      "triggerChannelId": 111111111111111111,
      "createCategoryId": 222222222222222222,
      "defaultUserLimit": 3,
      "channelNameTemplate": "{user} 的频道",
      "enabled": true,
      "createdAt": "2026-09-01T08:00:00+00:00",
      "updatedAt": null
    }
  ]
}
```

配置 DTO 字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `id` | `string` | 配置 ID，UUID |
| `guildId` | `number/string` | Discord Guild ID；提交时建议使用字符串语义处理 |
| `triggerChannelId` | `number/string` | 触发用户加入的 Voice Channel ID |
| `createCategoryId` | `number/string` | 创建临时频道所属的 Category ID |
| `defaultUserLimit` | `number` | 默认人数上限，范围为 `3-50` |
| `channelNameTemplate` | `string` | 临时频道名称模板，最大长度 128 |
| `enabled` | `boolean` | 是否启用 |

## 六、新增配置

```http
POST /api/Discord/voice-channel/config/add
Authorization: Bearer {voidAccessToken}
Content-Type: application/json
```

请求示例：

```json
{
  "guildId": "123456789012345678",
  "triggerChannelId": "111111111111111111",
  "createCategoryId": "222222222222222222",
  "defaultUserLimit": 3,
  "channelNameTemplate": "{user} 的频道",
  "enabled": true
}
```

校验规则：

- `guildId`、`triggerChannelId`、`createCategoryId` 不能为空；
- `defaultUserLimit` 必须在 `3-50` 范围内；
- 同一个 Guild 的同一个触发频道不能重复配置；
- 推荐始终使用第四节接口返回的频道 ID，不要由用户手工输入 ID。

成功响应：

```json
{
  "code": 200,
  "message": "ok"
}
```

## 七、修改配置

```http
POST /api/Discord/voice-channel/config/update
Authorization: Bearer {voidAccessToken}
Content-Type: application/json
```

请求示例：

```json
{
  "id": "4f2c8b64-39ef-4f93-9be4-a64d6d6c90a1",
  "guildId": "123456789012345678",
  "triggerChannelId": "111111111111111111",
  "createCategoryId": "222222222222222222",
  "defaultUserLimit": 5,
  "channelNameTemplate": "{user} 的专属频道",
  "enabled": true
}
```

## 八、删除配置

```http
POST /api/Discord/voice-channel/config/delete
Authorization: Bearer {voidAccessToken}
Content-Type: application/json
```

请求示例：

```json
{
  "idList": [
    "4f2c8b64-39ef-4f93-9be4-a64d6d6c90a1"
  ]
}
```

## 九、错误处理

### 未绑定 Discord

```json
{
  "code": 400,
  "message": "A Discord account must be bound first."
}
```

前端应引导用户完成 Discord 绑定，而不是进入 Guild 配置页面。

### Discord Token 授权不足或已失效

```json
{
  "code": 401,
  "message": "Failed to read Discord guilds. Please authorize Discord again."
}
```

前端应让用户重新发起现有 Discord OAuth 流程：

```http
GET /api/OAuth/Discord/authorize
```

成功重新授权后再重新请求 `managed-guilds`。

### 用户无权管理 Guild

```json
{
  "code": 403,
  "message": "You do not have permission to manage this guild."
}
```

### Bot 尚未加入 Guild

```json
{
  "code": 404,
  "message": "Discord bot is not installed in this guild."
}
```

当前版本没有自动邀请入口，前端只需提示用户 Bot 尚未加入即可。

## 十、推荐页面流程

```text
1. 请求 /api/Discord/managed-guilds
2. 检查是否存在 Discord 绑定错误
3. 展示用户可管理的 Guild
4. 用户选择 isBotInstalled = true 的 Guild
5. 请求 /api/Discord/guilds/channels?guildId=...
6. 请求 /api/Discord/voice-channel/config/list?guildId=...
7. 使用 Voice 频道填充触发频道选择
8. 使用 Category 频道填充创建分类选择
9. 编辑时调用 config/update
10. 新增时调用 config/add
```

建议页面状态至少区分：

- Discord 未绑定；
- Discord 已绑定但授权过期或授权范围不足；
- Guild 可管理但 Bot 未加入；
- Guild 可配置；
- Bot 尚未 Ready 或暂时无法读取频道。
