# 双因素认证前端接入

当前实现使用标准 TOTP：SHA-1、6 位数字、30 秒周期，并允许前后各一个周期的时钟偏差。密钥通过 `SecretProtection:EncryptionKey` 加密后保存，接口不会返回已启用账户的密钥。

## 启用流程

以下接口都需要 Bearer 登录态：

1. `POST /api/Identity/two-factor/setup`

   请求体：`{ "password": "当前密码" }`。成功响应的 `data.manualEntryKey` 可供手动录入，`data.otpAuthUri` 可由前端生成二维码。响应包含敏感信息，前端不要持久化或记录日志。

2. `POST /api/Identity/two-factor/enable`

   请求体：`{ "code": "123456" }`。服务端用 setup 阶段保存的待确认密钥验证动态码；成功后才正式启用 2FA。再次调用 setup 会替换尚未确认的密钥。

3. `GET /api/Identity/two-factor/status`

   返回 `data.isEnabled` 和 `data.enabledAt`，不返回密钥。

启用成功会提升账户的 token version，使已有 refresh token 失效。
启用后需要使用密码和动态码重新登录；新 JWT 会包含服务端使用的 2FA 已认证声明。

## 登录验证

登录接口改为 `POST /api/Identity/login`，凭据通过 JSON 请求体传递：

```json
{
  "name": "user",
  "suffix": "0001",
  "password": "Password1",
  "twoFactorCode": "123456"
}
```

未启用 2FA 时可以省略 `twoFactorCode`。已启用时，如果省略动态码，响应体的 `code` 为 `401`、`message` 为 `two_factor_code_required`，并且 `requiresTwoFactor` 为 `true`；动态码错误时 `message` 为 `invalid_two_factor_code`。这两种情况都不会签发 JWT。

## 关闭流程

`POST /api/Identity/two-factor/disable`

请求体：`{ "password": "当前密码", "code": "123456" }`。密码和动态码都通过后才会清除密钥。关闭成功同样会提升 token version。

## 错误标识

- `account_not_found`
- `invalid_password`
- `two_factor_already_enabled`
- `two_factor_not_enabled`
- `two_factor_setup_required`
- `two_factor_code_required`
- `invalid_two_factor_code`
- `two_factor_authentication_required`

第三方 OAuth 登录仍以第三方身份提供方的认证策略为准；本流程强制保护 Void 账号密码登录。
使用 Void 账号密码完成 OAuth 账户绑定时，也需要按同样规则传递可选的 `twoFactorCode`。

第三方 OAuth 直接登录取得的 JWT 不包含 2FA 已认证声明。若 `/api/Identity/me` 返回 `isTwoFactorEnabled=true`，前端应继续执行 step-up：

```http
POST /api/Identity/two-factor/verify
Authorization: Bearer {oauthAccessToken}
Content-Type: application/json

{
  "code": "123456"
}
```

成功后接口返回新的 access token 和 refresh token，二者都包含 2FA 已认证声明。前端必须整体替换 OAuth 登录阶段取得的旧 Token，之后才能访问受 `IRequireTwoFactorAuthentication` 保护的接口。该接口不要求密码，因为第三方 OAuth JWT 已经确认当前账户身份。

## 受保护接口

后端通过 `IRequireTwoFactorAuthentication` 消息标识和 Mediator 中间件统一保护敏感操作。账号未启用 2FA 时按原流程执行；账号已启用时，JWT 必须来自一次成功的 2FA 登录，否则响应 `two_factor_authentication_required`。

当前标记的操作包括：

- 修改密码；
- 关闭 2FA；
- 创建或删除 API Key；
- 解绑第三方 OAuth 账号。

动态码只在密码登录、OAuth 账号绑定、启用确认、关闭确认和 OAuth 登录后的 step-up 中提交，不应在每个业务接口中重复传递。refresh token 会保留原 JWT 的 2FA 已认证状态，但启用或关闭 2FA 后 token version 会变化，旧 refresh token 会失效。
