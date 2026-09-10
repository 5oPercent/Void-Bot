# 账号安全管理接口前端对接

本文档说明账号列表安全状态、管理员密码重置、密码重置链接、第三方授权解绑和邮箱验证码解绑能力，以及密码重置页面和 EventSystem 配置页面需要配合的内容。

## 接口概览

| 方法 | 地址 | 权限 | 用途 |
| --- | --- | --- | --- |
| `POST` | `/api/Security/user/list` | 登录并拥有接口权限 | 分页查询账号，并返回邮箱验证和双因素认证状态 |
| `POST` | `/api/Account/password/reset-link` | 登录并拥有接口权限 | 向指定账号的已验证邮箱发送一次性密码重置链接 |
| `POST` | `/api/Account/password/reset` | 登录并拥有接口权限 | 管理人员直接为指定账号设置新密码 |
| `POST` | `/api/Account/oauth/force-unlink` | 登录并拥有接口权限 | 强制解绑指定账号的第三方授权平台 |
| `POST` | `/api/Identity/password-reset/link` | 匿名 | 用户使用邮件链接中的令牌设置新密码 |
| `POST` | `/api/Identity/email/unbind/send-code` | 登录 | 向当前账号的已验证邮箱发送解绑验证码 |
| `POST` | `/api/Identity/email/unbind` | 登录 | 验证邮件验证码并解绑当前账号邮箱 |

账号列表接口位于 `SecurityController`，三个账号管理接口位于 `AccountController`；它们都受登录验证和现有接口权限体系控制。未登录时返回 `401`，没有对应接口权限时返回 `403`。密码重置链接完成接口允许匿名访问，两个邮箱解绑接口需要登录但跳过接口权限检查。

接口使用统一响应结构：

```json
{
  "code": 200,
  "message": "ok"
}
```

管理接口的业务失败可能仍使用 HTTP `200` 返回，前端应以响应体中的 `code` 判断业务结果，不能只判断 HTTP 状态。

## 账号列表安全状态

`POST /api/Security/user/list` 的分页条目在原有账号字段基础上增加以下字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `email` | `string \| null` | 账号绑定的完整邮箱，仅管理端账号列表返回 |
| `emailVerifiedAt` | `string \| null` | 邮箱验证时间；为空表示邮箱尚未验证或未绑定 |
| `isTwoFactorEnabled` | `boolean` | 是否已启用 TOTP 双因素认证 |

示例条目：

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "name": "example",
  "suffix": "1234",
  "displayName": "example#1234",
  "email": "user@example.com",
  "emailVerifiedAt": "2026-09-09T10:00:00+08:00",
  "isTwoFactorEnabled": true,
  "createdAt": "2026-09-01T10:00:00+08:00",
  "updatedAt": "2026-09-09T10:00:00+08:00",
  "thirdParties": []
}
```

该接口只返回是否启用 2FA，不返回 TOTP 密钥。普通用户身份接口的邮箱可见范围不受此次调整影响。

## 推荐交互流程

账号管理页面可以同时提供两种密码重置方式：

1. 默认使用“发送重置链接”，不在管理页面接触用户的新密码。
2. 只有明确需要代用户设置密码时，才使用“直接设置新密码”。
3. 两种方式成功完成密码修改后，账号的 `TokenVersion` 都会递增，使已有登录令牌按现有版本校验机制失效。

## 发送密码重置链接

### 请求

```http
POST /api/Account/password/reset-link
Authorization: Bearer <token>
Content-Type: application/json

{
  "userId": "00000000-0000-0000-0000-000000000000"
}
```

`userId` 必填且不能是空 GUID。目标账号必须已经绑定并验证邮箱。

### 响应

发送成功：

```json
{
  "code": 200,
  "message": "password_reset_link_sent"
}
```

| `code` | `message` | 前端建议 |
| --- | --- | --- |
| `404` | `account_not_found` | 提示账号不存在并刷新账号数据 |
| `409` | `verified_email_required` | 提示该账号需要先绑定并验证邮箱 |
| `502` | `email_send_failed` | 提示邮件发送失败，允许稍后重试 |
| `503` | `password_reset_url_not_configured` | 提示服务配置异常 |
| `503` | `password_reset_link_unavailable` | 提示重置链接服务暂不可用 |

接口会实时返回邮件发送结果，不会额外发布“重置链接发送成功”事件。

每次成功生成新链接都会使该账号之前的链接失效。邮件发送失败时，本次生成的令牌也会被删除。

## 密码重置页面

后端发送的链接格式如下：

```text
https://voidbot.cyooops.icu/password-reset#token=<43-character-token>
```

前端需要提供 `/password-reset` 页面，并从 URL fragment 中读取 `token`。fragment 不会随页面请求发送给服务器，前端读取后再调用完成接口。

示例：

```ts
const params = new URLSearchParams(window.location.hash.slice(1));
const token = params.get("token");
```

读取令牌后建议立即通过 `history.replaceState` 清理地址栏，避免令牌出现在截图、复制链接或前端埋点中：

```ts
window.history.replaceState(null, "", window.location.pathname + window.location.search);
```

不要把令牌写入日志、埋点、Local Storage 或错误上报上下文。

### 提交新密码

```http
POST /api/Identity/password-reset/link
Content-Type: application/json

{
  "token": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
  "newPassword": "NewPassword1",
  "newPasswordConfirm": "NewPassword1"
}
```

字段规则：

| 字段 | 规则 |
| --- | --- |
| `token` | 必填，固定 43 个字符 |
| `newPassword` | 至少 8 个字符，并同时包含大写字母、小写字母和数字 |
| `newPasswordConfirm` | 必须与 `newPassword` 完全一致 |

成功：

```json
{
  "code": 200,
  "message": "ok"
}
```

| `code` | `message` | 前端建议 |
| --- | --- | --- |
| `400` | `invalid_or_expired_password_reset_link` | 统一提示链接无效或已过期，并引导联系管理员重新发送 |
| `503` | `password_reset_link_unavailable` | 提示服务暂不可用，避免引导用户反复提交 |

令牌默认有效期为 15 分钟且只能使用一次。成功后跳转登录页，不应继续保留密码或令牌表单状态。

## 管理人员直接重置密码

### 请求

```http
POST /api/Account/password/reset
Authorization: Bearer <token>
Content-Type: application/json

{
  "userId": "00000000-0000-0000-0000-000000000000",
  "newPassword": "NewPassword1",
  "newPasswordConfirm": "NewPassword1"
}
```

密码规则与密码重置页面一致。成功响应为：

```json
{
  "code": 200,
  "message": "ok"
}
```

账号不存在时：

```json
{
  "code": 404,
  "message": "Account not found."
}
```

直接重置成功后会触发 EventSystem 事件 `Account.PasswordForceReset`。后台可为该事件配置无参数的 `SendPasswordForceResetEmail` 内置动作，使用统一 HTML 模板和 i18n 文案将本次设置的密码发送到用户的已验证邮箱。原有 `SendEmail` 自定义动作仍可使用 `{Password}` 正文占位符；邮件主题禁止使用该占位符。

前端不得记录、回显或持久化密码。提交完成后应立即清空两个密码输入框。

## 强制解绑第三方授权

### 请求

```http
POST /api/Account/oauth/force-unlink
Authorization: Bearer <token>
Content-Type: application/json

{
  "userId": "00000000-0000-0000-0000-000000000000",
  "platform": 4
}
```

平台值：

| 值 | 平台 |
| --- | --- |
| `1` | Steam |
| `2` | Bungie |
| `4` | Discord |
| `8` | Tencent |

`Void` 的值为 `0`，不是第三方平台，禁止提交。建议前端使用平台枚举选择器，不允许输入任意数字。

成功响应为：

```json
{
  "code": 200,
  "message": "ok"
}
```

| `code` | `message` | 前端建议 |
| --- | --- | --- |
| `404` | `Account not found.` | 提示账号不存在并刷新账号数据 |
| `404` | `Third-party binding not found.` | 提示该平台已经解绑或不存在绑定 |

解绑成功后触发 EventSystem 事件 `Account.ThirdPartyForceUnlinked`。

## 当前用户解绑邮箱

邮箱解绑不再校验账号密码，改为向当前已验证邮箱发送验证码。两个接口都需要登录，但跳过接口权限检查。

第一步发送验证码，请求不需要 Body：

```http
POST /api/Identity/email/unbind/send-code
Authorization: Bearer <token>
```

成功时返回挑战信息：

```json
{
  "code": 200,
  "message": "ok",
  "data": {
    "challengeId": "00000000-0000-0000-0000-000000000000",
    "expiresAt": "2026-08-26T12:00:00+00:00"
  }
}
```

账号未绑定已验证邮箱时返回 `409 / account_email_not_bound`。发送频率受现有邮件验证码策略限制，可能返回 `429 / email_verification_rate_limited`、`502 / email_send_failed` 或 `503 / email_verification_unavailable`。

第二步提交验证码：

```http
POST /api/Identity/email/unbind
Authorization: Bearer <token>
Content-Type: application/json

{
  "challengeId": "00000000-0000-0000-0000-000000000000",
  "code": "123456"
}
```

验证码固定为 6 位数字。验证码与当前登录用户、当前绑定邮箱及解绑用途绑定，不能与注册、绑定或密码重置验证码混用。验证码过期、错误或已使用时返回 `400 / invalid_or_expired_email_verification`。

解绑成功后返回更新后的邮箱状态，`isBound` 为 `false`。前端不再展示或提交账号密码字段。

## EventSystem 前端配合

EventSystem 定义接口会返回以下账号事件和邮件动作：

| 类型 | 名称 | 说明 |
| --- | --- | --- |
| Event | `Account.PasswordForceReset` | 管理人员直接重置密码并成功提交事务后触发 |
| Event | `Account.ThirdPartyForceUnlinked` | 管理人员强制解绑第三方平台并成功提交事务后触发 |
| Action | `SendEmail`，版本 `1` | 向事件关联账号的已验证邮箱发送纯文本邮件 |
| Action | `SendAccountRegisteredEmail`，版本 `1` | 无参数，发送内置账号注册成功通知 |
| Action | `SendPasswordForceResetEmail`，版本 `1` | 无参数，发送内置密码强制重置通知 |
| Action | `SendThirdPartyForceUnlinkedEmail`，版本 `1` | 无参数，发送内置第三方授权强制解绑通知 |

`SendEmail` 参数：

| 参数 | 必填 | 限制 |
| --- | --- | --- |
| `Subject` | 是 | 单值，替换占位符后最多 200 个字符；禁止使用 `{Password}` |
| `Content` | 是 | 单值，替换占位符后最多 10000 个字符；支持 `{Password}` |

主题和正文都支持 `{UserId}`、`{DisplayName}` 和 `{Platform}`。只有正文支持 `{Password}`，且仅在 `Account.PasswordForceReset` 事件中有值；其他事件会替换为空字符串。前端应阻止在主题中选择或输入 `{Password}`。

三个内置邮件动作不接受参数，且仅支持各自对应的事件。事件和动作定义会由 EventSystem 定义接口返回。前端应根据定义元数据动态渲染，不要硬编码事件、动作或参数列表。发送重置链接和用户通过链接完成重置都不会触发 EventSystem 事件。

## 前端验收清单

- 账号管理页能发送重置链接，并根据响应体 `code` 显示准确结果。
- `/password-reset` 能读取并清理 hash 中的令牌。
- 重置页面能校验密码复杂度和两次输入一致性。
- 链接无效、过期或已使用时显示统一提示，不泄露账号状态。
- 管理页可直接设置密码，提交结束后清空密码字段。
- 第三方解绑只展示允许的平台枚举，并在成功后刷新绑定列表。
- 邮箱解绑先发送验证码，再提交 `challengeId + code`，不再要求用户输入账号密码。
- 所有敏感字段均不进入日志、埋点、浏览器持久化或错误上报。
- EventSystem 页面能通过定义接口识别新增事件、三个内置邮件动作及原有 `SendEmail` 动作。
