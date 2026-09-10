# 双因素认证前端接入

当前实现使用标准 TOTP：SHA-1、6 位数字、30 秒周期。允许的时间偏差由 `TwoFactor:AllowedTimeStepDrift` 配置，默认为前后各一个周期。密钥通过 `SecretProtection:EncryptionKey` 加密后保存，接口不会返回已启用账户的密钥。

## 启用流程

以下接口都需要 Bearer 登录态：

1. `POST /api/Identity/two-factor/setup`

   请求体：`{ "password": "当前密码" }`。成功响应的 `data.manualEntryKey` 可供手动录入，`data.otpAuthUri` 可由前端生成二维码，`data.expiresAt` 是本次 setup 的实际过期时间。setup 默认在 10 分钟后过期，再次调用会替换尚未确认的密钥。响应包含敏感信息，前端不要持久化或记录日志。

2. `POST /api/Identity/two-factor/enable`

   请求体：`{ "code": "123456" }`。服务端用 setup 阶段保存的待确认密钥验证动态码；成功后才正式启用 2FA，并仅在本次响应中返回恢复码：

   ```json
   {
     "code": 200,
     "message": "ok",
     "data": {
       "recoveryCodes": [
         "ABCD-EFGH-JK23",
         "NPQR-STUV-WX45"
       ]
     }
   }
   ```

   上述数组仅为格式示例，实际数量由 `TwoFactor:RecoveryCodeCount` 决定，默认为 8 条。

   前端必须要求用户立即下载、复制或打印恢复码，并明确提示关闭页面后无法再次查看。不得写入日志、埋点或浏览器持久化存储。

3. `GET /api/Identity/two-factor/status`

   返回 `data.isEnabled`、`data.enabledAt` 和 `data.recoveryCodesRemaining`，不返回密钥或恢复码明文。

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

无法取得 TOTP 时，可以把一条恢复码作为 `twoFactorCode` 提交，例如 `ABCD-EFGH-JK23`。恢复码不区分大小写，每条只能成功使用一次；成功后 `recoveryCodesRemaining` 会减少。OAuth 账号绑定及 OAuth 登录后的 step-up 同样支持恢复码。

## 重新生成恢复码

```http
POST /api/Identity/two-factor/recovery-codes/regenerate
Authorization: Bearer {twoFactorAuthenticatedAccessToken}
Content-Type: application/json

{
  "password": "当前密码",
  "code": "123456"
}
```

该接口要求 JWT 已完成 2FA，同时再次验证当前密码以及 TOTP 动态码或一条未使用的恢复码。成功响应结构与启用 2FA 时相同；新恢复码生成后，全部旧恢复码立即失效。

## 关闭流程

`POST /api/Identity/two-factor/disable`

请求体：`{ "password": "当前密码", "code": "123456" }`。`code` 可以是 TOTP 动态码或一条未使用的恢复码；密码和验证码都通过后才会清除密钥及剩余恢复码。关闭成功同样会提升 token version。

## 错误标识

- `account_not_found`
- `invalid_password`
- `two_factor_already_enabled`
- `two_factor_not_enabled`
- `two_factor_setup_required`
- `two_factor_setup_expired`
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

动态码只在密码登录、OAuth 账号绑定、启用确认、关闭确认、重新生成恢复码和 OAuth 登录后的 step-up 中提交，不应在每个业务接口中重复传递。refresh token 会保留原 JWT 的 2FA 已认证状态，但启用或关闭 2FA 后 token version 会变化，旧 refresh token 会失效。

## 双因素认证配置

身份验证器及恢复码行为由后端配置控制：

| 配置 | 默认值 | 范围/说明 |
| --- | ---: | --- |
| `TwoFactor:Title` | `Void Bot` | 身份验证器中显示的应用标题，用户不能通过 setup 请求修改 |
| `TwoFactor:AllowedTimeStepDrift` | `1` | 允许的前后时间周期数，服务端限制为 `0`–`2` |
| `TwoFactor:SetupExpireMinutes` | `10` | setup 有效分钟数，服务端限制为 `1`–`60` |
| `TwoFactor:RecoveryCodeCount` | `8` | 每次生成的恢复码数量，服务端限制为 `1`–`20` |

不同部署环境可以分别覆盖配置，例如测试环境将标题设置为 `Void Bot Staging`。环境变量使用双下划线，例如 `TwoFactor__Title`、`TwoFactor__SetupExpireMinutes`。

## 管理端账号列表

`POST /api/Security/user/list` 的分页条目会返回 `email`、`emailVerifiedAt` 和 `isTwoFactorEnabled`，供管理页面展示邮箱及 2FA 状态。邮箱和验证时间可能为 `null`；`isTwoFactorEnabled` 始终为布尔值。接口不会返回 TOTP 密钥。
