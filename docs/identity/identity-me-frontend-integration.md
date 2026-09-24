# `/api/Identity/me` 前端对接说明

## 1. 接口用途

`me` 接口用于获取当前登录用户的 Void 主账号信息及第三方账号绑定列表。

前端应将它作为登录态账号信息的统一来源，不区分用户通过密码、Discord、Bungie 或其他方式登录。

## 2. 请求地址

测试环境：

```http
GET https://testapi.voidbot.cyooops.icu/api/Identity/me
```

生产环境：

```http
GET https://api.voidbot.cyooops.icu/api/Identity/me
```

请求必须携带 Void access token：

```http
Authorization: Bearer {accessToken}
```

接口不接受 `userId`。后端只从 Void JWT 中取得当前账号 ID，前端不能通过该接口查询其他账号。

## 3. 成功响应

```json
{
  "code": 200,
  "message": "ok",
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "name": "VoidName",
    "suffix": "0001",
    "displayName": "VoidName#0001",
    "isTwoFactorEnabled": true,
    "createdAt": "2026-08-22T10:00:00+08:00",
    "updatedAt": "2026-08-22T10:00:00+08:00",
    "thirdParties": [
      {
        "provider": "Discord",
        "thirdPartyUserId": "123456789012345678",
        "createdAt": "2026-08-22T10:05:00+08:00",
        "updatedAt": "2026-08-22T10:05:00+08:00"
      },
      {
        "provider": "Bungie",
        "thirdPartyUserId": "4611686010000000000",
        "createdAt": "2026-08-22T10:06:00+08:00",
        "updatedAt": "2026-08-22T10:06:00+08:00"
      }
    ]
  }
}
```

字段说明：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `data.id` | string | Void 主账号 ID |
| `data.name` | string | Void 主账号名称 |
| `data.suffix` | string | Void 主账号四位后缀 |
| `data.displayName` | string | 用于展示的完整名称，格式为 `name#suffix` |
| `data.isTwoFactorEnabled` | boolean | 当前 Void 账号是否已启用 TOTP 双因素认证 |
| `data.createdAt` | string | Void 主账号创建时间 |
| `data.updatedAt` | string/null | Void 主账号最后更新时间 |
| `data.thirdParties` | array | 当前账号的第三方绑定列表；没有绑定时为空数组 |
| `provider` | string | 平台名称，当前 OAuth 平台为 `Discord` 或 `Bungie` |
| `thirdPartyUserId` | string | 第三方平台用户 ID，必须按字符串处理 |
| `createdAt` | string | 绑定创建时间 |
| `updatedAt` | string | 绑定最后更新时间 |

接口不会返回第三方平台的 access token、refresh token 或过期时间。

## 4. TypeScript 类型

```ts
export interface CurrentAccount {
  id: string;
  name: string;
  suffix: string;
  displayName: string;
  isTwoFactorEnabled: boolean;
  createdAt: string;
  updatedAt: string | null;
  thirdParties: ThirdPartyBinding[];
}

export interface ThirdPartyBinding {
  provider: string;
  thirdPartyUserId: string;
  createdAt: string;
  updatedAt: string;
}

export interface ApiResponse<T> {
  code: number;
  message: string;
  data: T | null;
}
```

不要把 `thirdPartyUserId` 转换为 JavaScript `number`，Discord、Bungie 等平台的 ID 可能超过安全整数范围。

## 5. 调用示例

```ts
export async function getCurrentAccount(
  apiBaseUrl: string,
  accessToken: string,
): Promise<CurrentAccount> {
  const response = await fetch(`${apiBaseUrl}/api/Identity/me`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });

  if (response.status === 401) {
    throw new Error("unauthorized");
  }

  if (!response.ok) {
    throw new Error(`request_failed:${response.status}`);
  }

  const result = (await response.json()) as ApiResponse<CurrentAccount>;
  if (result.code !== 200 || !result.data) {
    throw new Error(result.message || "account_not_found");
  }

  return result.data;
}
```

`apiBaseUrl` 应由前端环境配置提供：

```text
测试环境：https://testapi.voidbot.cyooops.icu
生产环境：https://api.voidbot.cyooops.icu
```

## 6. 调用时机

### 第三方 OAuth 登录成功

第三方身份已经绑定 Void 主账号时：

```text
前端 OAuth 结果页取得一次性 code
  → POST /api/OAuth/exchange
  → 保存返回的 Void accessToken 和 refreshToken
  → GET /api/Identity/me
  → 若 isTwoFactorEnabled=true，调用 POST /api/Identity/two-factor/verify 并替换 Token
  → 保存并展示 Void 主账号和第三方绑定状态
  → 进入已登录页面
```

第三方身份尚未绑定 Void 主账号时：

```text
前端取得 registrationCode
  → 创建新账号调用 /api/OAuth/register
    或登录已有账号调用 /api/OAuth/bind
  → 保存接口返回的 Void accessToken 和 refreshToken
  → GET /api/Identity/me
  → 保存并展示 Void 主账号和第三方绑定状态
```

详细流程见 [OAuth 未绑定身份注册与登录绑定对接说明](../oauth/oauth-registration-frontend-integration.md)。

第三方登录页面不要直接使用 OAuth 平台用户名作为 Void 主账号名称，应以 `me.data.displayName` 为准。

### 密码登录成功

```text
POST /api/Identity/login
  → 保存 Void Token
  → GET /api/Identity/me
  → 初始化当前账号状态
```

### 页面刷新或应用启动

本地存在 access token 时调用 `me` 恢复当前账号状态。若 access token 已过期，先使用 refresh token 刷新，再重新调用 `me`。

### 绑定成功

```text
OAuth callback 返回 flow=link&result=success
  → 不调用 exchange
  → 使用原有 Void access token 调用 GET /api/Identity/me
  → 用最新 thirdParties 刷新绑定状态
```

### 解绑成功

```text
POST /api/OAuth/{platform}/unlink
  → GET /api/Identity/me
  → 用最新 thirdParties 刷新绑定状态
```

## 7. 判断平台是否已绑定

平台名称比较建议忽略大小写：

```ts
export function isPlatformLinked(
  account: CurrentAccount,
  platform: string,
): boolean {
  return account.thirdParties.some(
    (item) => item.provider.toLowerCase() === platform.toLowerCase(),
  );
}
```

不要依赖数组顺序，也不要假设 `thirdParties` 一定包含 Discord 或 Bungie。

## 8. 登录态与异常处理

- HTTP `401`：access token 缺失、无效或过期。可尝试刷新一次 Token；刷新失败则清理登录态并跳转登录页。
- HTTP `403`：不应出现在正常 `me` 请求中。接口只要求有效登录态，不要求后台功能权限；出现时应记录请求信息并排查部署版本。
- HTTP `5xx` 或网络错误：保留已有 Token，展示可重试状态，不要直接判定用户已退出。
- `code` 不是 `200` 或 `data` 为 `null`：不要继续构造空账号，应展示异常并记录响应中的 `message`。

避免并发触发多次 Token 刷新。建议由统一请求层保证同一时间只有一个 refresh 请求，其余失败请求等待刷新完成后再重试。

## 9. 前端状态建议

建议只维护一份当前账号状态：

```ts
interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  account: CurrentAccount | null;
  accountLoading: boolean;
}
```

- Token 表示是否具备认证凭证。
- `account` 表示当前服务端账号资料与绑定状态。
- 登录、绑定、解绑完成后都通过 `me` 更新 `account`，不要在前端手动拼接或删除绑定数据。

## 10. 对接检查清单

- [ ] API Base URL 按测试和生产环境分别配置。
- [ ] 所有 `me` 请求携带 `Authorization: Bearer {accessToken}`。
- [ ] OAuth exchange 完成并保存 Token 后调用 `me`。
- [ ] OAuth register 或 bind 完成并保存 Token 后调用 `me`。
- [ ] 密码登录完成并保存 Token 后调用 `me`。
- [ ] 应用启动或页面刷新时使用 `me` 恢复账号状态。
- [ ] 绑定和解绑成功后重新调用 `me`。
- [ ] 页面显示 `data.displayName` 作为 Void 主账号名称。
- [ ] 使用 `thirdParties` 判断各平台绑定状态。
- [ ] 第三方用户 ID 始终按字符串处理。
- [ ] `401` 时刷新 Token，刷新失败后再清理登录态。
- [ ] 不缓存或展示任何第三方平台 Token。
