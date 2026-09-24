# 第三方 OAuth 前端对接说明

本文档用于前端对接 Void Bot 的通用 OAuth 登录、账号绑定和解绑流程。目前适用于：

- Bungie
- Discord

接口中的 `{platform}` 建议使用小写：

```text
bungie
discord
```

## 1. 本次流程变化

### 调整前

第三方授权完成后，第三方平台直接跳转后端 callback。后端处理完成后返回包含 Void access token 和 refresh token 的 JSON，浏览器会停留在 API 页面。

绑定流程完成后也会重新生成 Void JWT。

### 调整后

- 第三方平台仍然回调后端，回调地址不设置到前端。
- 后端处理 callback 后返回 HTTP 302，跳转到固定的前端 OAuth 结果页。
- 登录成功时，跳转 URL 只包含一个短时、单次使用的 `exchangeCode`。
- 前端通过 exchange 接口使用 `exchangeCode` 换取 Void JWT。
- 第三方身份未绑定 Void 账号时，不再使用随机密码自动注册；后端返回短时 `registrationCode`。
- 前端使用 `registrationCode` 创建新 Void 账号，或登录已有 Void 账号完成绑定。
- 账号绑定成功时不生成 Void JWT，也不生成 `exchangeCode`。
- callback 失败时同样跳转前端，并携带稳定的错误码。

## 2. 环境地址

### 测试环境

API：

```text
https://testapi.voidbot.cyooops.icu
```

前端 OAuth 结果页：

```text
https://test.voidbot.cyooops.icu/oauth/callback
```

### 生产环境

API：

```text
https://api.voidbot.cyooops.icu
```

前端 OAuth 结果页：

```text
https://voidbot.cyooops.icu/oauth/callback
```

后端通过以下配置区分环境：

```text
OAuth__FrontendCallbackUrl=https://test.voidbot.cyooops.icu/oauth/callback
OAuth__RegistrationTtlSeconds=600
```

前端不需要、也不能通过请求参数指定任意 `returnUrl`。

## 3. 接口汇总

| 用途 | 方法 | 地址 | 是否需要 Void JWT |
| --- | --- | --- | --- |
| 开始登录或绑定 | GET | `/api/OAuth/{platform}/authorize` | 登录不需要；绑定需要 |
| 第三方回调 | GET | `/api/OAuth/{platform}/callback` | 否，由第三方平台访问 |
| 使用一次性 code 换取 JWT | POST | `/api/OAuth/exchange` | 否 |
| 创建 Void 账号并绑定待注册身份 | POST | `/api/OAuth/register` | 否，使用 registrationCode |
| 登录已有 Void 账号并绑定 | POST | `/api/OAuth/bind` | 否，使用 registrationCode 和账号密码 |
| 查询当前 Void 账号及绑定 | GET | `/api/Identity/me` | 是 |
| 解除第三方绑定 | POST | `/api/OAuth/{platform}/unlink` | 是 |

前端不应主动调用 callback 接口。该地址只提供给 Bungie、Discord 等第三方 OAuth 平台。

## 4. 开始 OAuth

```http
GET /api/OAuth/{platform}/authorize
```

同一个 authorize 接口根据请求是否携带有效的 Void access token 自动选择流程：

| Authorization Header | 流程 |
| --- | --- |
| 不携带 | Login，第三方账号登录 Void |
| 携带有效 Bearer Token | Link，将第三方账号绑定到当前 Void 账号 |

登录请求示例：

```http
GET /api/OAuth/bungie/authorize
```

绑定请求示例：

```http
GET /api/OAuth/bungie/authorize
Authorization: Bearer {voidAccessToken}
```

成功响应示例：

```json
{
  "code": 200,
  "message": "ok",
  "data": {
    "platform": "Bungie",
    "redirectUrl": "https://www.bungie.net/..."
  }
}
```

前端收到 `redirectUrl` 后执行整页跳转：

```ts
window.location.assign(response.data.redirectUrl);
```

不要使用 AJAX 请求第三方授权页面，也不要在 iframe 中打开授权页面。

## 5. 第三方登录流程

第三方身份已经绑定 Void 主账号时：

```text
前端请求 authorize（不携带 Void JWT）
  → 后端创建 Login state
  → 前端跳转第三方授权页面
  → 第三方平台回调后端 callback
  → 后端校验并单次消费 state
  → 后端交换第三方 Token
  → 后端找到现有 Void 账号绑定并更新第三方 Token
  → 后端生成 Void JWT
  → 后端暂存 JWT 并生成一次性 exchangeCode
  → 后端 302 跳转前端 OAuth 结果页
  → 前端使用 exchangeCode 调用 exchange
  → 前端取得 Void JWT 并调用 GET /api/Identity/me
  → 若 isTwoFactorEnabled=true，调用 POST /api/Identity/two-factor/verify
  → 前端替换为带 2FA 已认证声明的新 JWT 并进入登录状态
```

登录成功回跳示例：

```text
https://test.voidbot.cyooops.icu/oauth/callback?platform=bungie&flow=login&result=success&code={exchangeCode}
```

回跳参数：

| 参数 | 示例 | 说明 |
| --- | --- | --- |
| `platform` | `bungie` | OAuth 平台 |
| `flow` | `login` | 当前为登录流程 |
| `result` | `success` | OAuth 处理成功 |
| `code` | 随机字符串 | 短时、单次使用的 exchange code |

第三方身份尚未绑定 Void 主账号时，callback 返回：

```text
https://test.voidbot.cyooops.icu/oauth/callback?platform=bungie&flow=register&result=registration_required&code={registrationCode}
```

前端让用户选择创建新 Void 账号或登录已有账号完成绑定，不调用 exchange。完整对接方式见 [OAuth 未绑定身份注册与登录绑定对接说明](oauth-registration-frontend-integration.md)。

### 使用 code 换取 JWT

```http
POST /api/OAuth/exchange
Content-Type: application/json

{
  "code": "{exchangeCode}"
}
```

成功响应：

```json
{
  "code": 200,
  "message": "ok",
  "data": {
    "accessToken": "...",
    "refreshToken": "..."
  }
}
```

`exchangeCode` 默认 120 秒过期，只能成功使用一次。重复请求、过期或格式错误时返回：

```json
{
  "code": 400,
  "message": "invalid_exchange_code",
  "data": null
}
```

前端成功交换 Token 后，应立即使用路由 replace 移除地址栏中的 code：

```ts
window.history.replaceState({}, "", "/oauth/callback");
```

不要对 exchange 请求自动重试。网络状态不明确时，重复消费可能返回 `invalid_exchange_code`，此时应提示用户重新发起登录。

如果随后调用 `/api/Identity/me` 得到 `isTwoFactorEnabled=true`，前端必须使用当前 OAuth access token 和身份验证器动态码调用 `POST /api/Identity/two-factor/verify`，并整体替换响应中的 access token 和 refresh token。否则当前 JWT 可以访问普通接口，但不能访问带有 `IRequireTwoFactorAuthentication` 标识的敏感接口。

### 查询当前 Void 账号

前端取得 Void access token 后，使用该 token 查询当前主账号及第三方绑定：

完整字段、TypeScript 类型、调用时机及异常处理见 [Identity Me 前端对接说明](../identity/identity-me-frontend-integration.md)。

```http
GET /api/Identity/me
Authorization: Bearer {accessToken}
```

响应示例：

```json
{
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "name": "VoidName",
    "suffix": "0001",
    "displayName": "VoidName#0001",
    "createdAt": "2026-08-22T10:00:00+08:00",
    "updatedAt": "2026-08-22T10:00:00+08:00",
    "thirdParties": [
      {
        "provider": "Bungie",
        "thirdPartyUserId": "4611686010000000000",
        "createdAt": "2026-08-22T10:05:00+08:00",
        "updatedAt": "2026-08-22T10:05:00+08:00"
      }
    ]
  }
}
```

账号 ID 由后端从 JWT 获取，前端不需要也不能指定其他用户 ID。响应不会包含第三方平台的 access token 或 refresh token。

## 6. 账号绑定成功流程

绑定流程必须在 authorize 请求中携带当前 Void access token：

```text
前端请求 authorize（携带 Void JWT）
  → 后端创建 Link state，并记录当前 Void AccountId
  → 前端跳转第三方授权页面
  → 第三方平台回调后端 callback
  → 后端校验 state、账号和第三方绑定冲突
  → 后端保存第三方 Token 和绑定信息
  → 后端不生成 Void JWT
  → 后端不生成 exchangeCode
  → 后端 302 跳转前端 OAuth 结果页
```

绑定成功回跳示例：

```text
https://test.voidbot.cyooops.icu/oauth/callback?platform=bungie&flow=link&result=success
```

前端处理：

1. 展示绑定成功。
2. 重新请求当前账号的第三方绑定列表。
3. 不调用 exchange 接口。
4. 跳回发起绑定的账号设置页面。

## 7. 失败回跳

授权被拒绝、state 无效、绑定冲突、第三方服务异常等情况都会跳回前端：

```text
https://test.voidbot.cyooops.icu/oauth/callback?platform=bungie&result=failed&error={errorCode}
```

失败时不保证存在 `flow` 和 `code`，前端必须以 `result=failed` 为判断条件。

建议错误文案映射：

| error | 建议前端文案 |
| --- | --- |
| `invalid_oauth_state` | 授权请求已失效，请重新发起授权 |
| `consent_denied` | 已取消第三方授权 |
| `authentication_required` | 登录状态已失效，请重新登录 |
| `authorization_forbidden` | 当前账号无权执行此操作 |
| `binding_conflict` | 该第三方账号已绑定其他 Void 账号，或当前账号已绑定其他同平台账号 |
| `binding_not_found` | 未找到对应的账号绑定 |
| `provider_failure` | 第三方授权服务暂时不可用，请稍后重试 |
| `oauth_state_store_unavailable` | 授权状态服务暂时不可用，请稍后重试 |
| `oauth_exchange_store_unavailable` | 登录交换服务暂时不可用，请重新登录 |
| `invalid_exchange_code` | 登录凭证已失效，请重新登录 |
| `oauth_registration_store_unavailable` | 待绑定身份服务暂时不可用，请重新登录 |
| `invalid_registration_code` | 本次授权信息已失效，请重新使用第三方登录 |
| `invalid_credentials` | Void 账号或密码错误 |
| `registration_failed` | Void 账号创建失败，请检查填写内容 |

前端只展示本地维护的错误文案，不要直接展示服务端异常内容。

## 8. 解除绑定

```http
POST /api/OAuth/{platform}/unlink
Authorization: Bearer {voidAccessToken}
```

Bungie 示例：

```http
POST /api/OAuth/bungie/unlink
Authorization: Bearer {voidAccessToken}
```

解绑是普通 API 请求，不经过第三方页面，也不会跳转 OAuth 结果页。

成功响应：

```json
{
  "code": 200,
  "message": "ok"
}
```

## 9. Bungie 特别说明

Bungie 开发者后台的 OAuth callback 必须配置到后端，不是前端页面。

测试环境：

```text
https://testapi.voidbot.cyooops.icu/api/OAuth/bungie/callback
```

生产环境：

```text
https://api.voidbot.cyooops.icu/api/OAuth/bungie/callback
```

实际跳转链路：

```text
前端 → Bungie → 后端 callback → 前端 OAuth 结果页
```

以下旧接口已经移除，前端不得继续调用：

```text
GET /api/Identity/generate/url/bungie
GET /api/Identity/link/bungie
```

## 10. 前端安全要求

- OAuth 结果页设置 `Referrer-Policy: no-referrer`。
- 不要把包含 exchange code 的完整 URL 发送到日志、埋点或第三方分析服务。
- 不要把 Void access token、refresh token 放进 URL。
- exchange 完成后立即从地址栏删除 code。
- 不要持久化 exchange code。
- 不要复用或自动重试同一个 exchange code。
- OAuth 结果页加载期间避免加载不必要的第三方脚本。
- 绑定流程必须携带当前有效的 Void access token。

## 11. 前端实现清单

- [ ] 登录入口调用通用 authorize，且不携带 Void JWT。
- [ ] 绑定入口调用通用 authorize，并携带 Void JWT。
- [ ] 收到 authorize 响应后使用 `window.location.assign` 跳转 `redirectUrl`。
- [ ] 新增 `/oauth/callback` 页面。
- [ ] 处理 `result=success/failed/registration_required`。
- [ ] 处理 `flow=login/link/register`。
- [ ] Login 成功时使用 code 调用 exchange。
- [ ] Register flow 展示创建新账号、登录已有账号和取消三个选择。
- [ ] 创建新账号调用 `/api/OAuth/register`。
- [ ] 登录已有账号调用 `/api/OAuth/bind`。
- [ ] exchange 成功后保存 Void access token 和 refresh token。
- [ ] 保存 Token 后调用 `/api/Identity/me` 显示 Void 主账号及绑定状态。
- [ ] exchange 完成后从地址栏移除 code。
- [ ] Link 成功时重新调用 `/api/Identity/me`，不调用 exchange。
- [ ] 根据 error code 显示本地化错误提示。
- [ ] OAuth 结果页禁用 Referrer，并避免第三方埋点采集完整 URL。
- [ ] 移除所有旧 Bungie OAuth 接口调用。
