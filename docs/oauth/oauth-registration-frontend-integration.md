# OAuth 未绑定身份注册与登录绑定对接说明

## 1. 适用场景

用户使用 Discord、Bungie 等第三方平台登录 Void 时，第三方身份可能尚未绑定任何 Void 主账号。

后端不再使用第三方昵称和随机密码自动创建 Void 账号，而是暂存第三方身份及 Token，并让用户在前端选择：

- 创建新的 Void 主账号并绑定；
- 登录已有 Void 主账号并绑定；
- 取消本次操作并返回登录页。

已经绑定 Void 主账号的第三方身份保持原有登录流程，不受影响。历史上已经自动创建的 Void 账号也仍会被识别为已绑定账号。

## 2. 整体流程

```text
用户发起第三方登录
  → 第三方授权并回调后端
  → 后端查询第三方身份绑定
      ├─ 已绑定 Void 主账号
      │    → 生成一次性 exchangeCode
      │    → 前端调用 /oauth/exchange 换取 Void JWT
      │    → 调用 /Identity/me 获取账号资料
      │
      └─ 未绑定 Void 主账号
           → 后端暂存第三方身份和 Token
           → 生成短时 registrationCode
           → 回跳前端 OAuth callback 页面
           → 前端立即清理地址栏参数
           → 用户选择创建新账号或登录已有账号
               ├─ 创建 → POST /oauth/register
               └─ 登录 → POST /oauth/bind
           → 成功返回 Void JWT
           → 调用 /Identity/me 获取主账号和绑定资料
```

## 3. 未绑定身份回跳契约

第三方身份没有绑定 Void 主账号时，后端跳转到固定的前端 OAuth 结果页：

```text
https://test.voidbot.cyooops.icu/oauth/callback
  ?platform=discord
  &flow=register
  &result=registration_required
  &code={registrationCode}
```

查询参数：

| 参数 | 示例 | 说明 |
| --- | --- | --- |
| `platform` | `discord` | 本次第三方平台，按不区分大小写处理 |
| `flow` | `register` | 表示需要用户选择注册或绑定已有账号 |
| `result` | `registration_required` | 表示第三方授权成功，但尚未绑定 Void 账号 |
| `code` | 随机字符串 | 短时、单次使用的 `registrationCode` |

URL 不包含第三方 access token、refresh token、用户 ID 或 Void JWT。

registrationCode 默认 600 秒过期，可通过后端环境变量 `OAuth__RegistrationTtlSeconds` 调整。

## 4. Callback 页面处理

页面识别到以下组合时进入“待绑定身份”状态：

```ts
flow === "register" && result === "registration_required" && code
```

读取参数后应立即移除完整查询参数，避免 code 出现在浏览器历史、日志、埋点或 Referrer 中：

```ts
const registrationCode = route.query.code as string;
await router.replace({ path: route.path });
```

页面展示：

- 标题：`未找到关联的 Void 主账号`
- 主操作：`创建 Void 主账号`
- 次操作：`登录已有 Void 主账号`
- 取消操作：`取消并返回登录`

`registrationCode` 只保存在当前页面内存中，不写入 localStorage、sessionStorage、Cookie 或前端日志。

页面刷新后 code 已从地址栏移除，应提示用户重新发起第三方登录。

## 5. 创建新 Void 主账号并绑定

接口：

```http
POST /api/OAuth/register
Content-Type: application/json
```

请求：

```json
{
  "code": "{registrationCode}",
  "name": "VoidName",
  "password": "Password123"
}
```

字段说明：

| 字段 | 必填 | 说明 |
| --- | --- | --- |
| `code` | 是 | callback 返回的 registrationCode |
| `name` | 是 | 用户主动填写的 Void 用户名 |
| `password` | 是 | 用户主动设置的 Void 密码，遵循普通注册规则 |

后端在同一数据库事务中创建 Void 主账号并写入第三方绑定。成功后 registrationCode 不能再次使用。

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

前端保存 Void Token 后立即调用：

```http
GET /api/Identity/me
Authorization: Bearer {accessToken}
```

页面显示的 Void 账号名称和第三方绑定状态必须以 `/Identity/me` 响应为准。

## 6. 登录已有 Void 主账号并绑定

接口：

```http
POST /api/OAuth/bind
Content-Type: application/json
```

请求：

```json
{
  "code": "{registrationCode}",
  "name": "VoidName",
  "suffix": "0001",
  "password": "Password123"
}
```

字段说明：

| 字段 | 必填 | 说明 |
| --- | --- | --- |
| `code` | 是 | callback 返回的 registrationCode |
| `name` | 是 | 已有 Void 主账号名称 |
| `suffix` | 是 | Void 主账号四位后缀 |
| `password` | 是 | 已有 Void 主账号密码 |

后端先验证 Void 账号密码，再检查第三方身份和目标账号是否存在绑定冲突。绑定成功后直接返回新的 Void JWT：

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

前端保存 Token，然后调用 `/api/Identity/me` 进入登录状态。

密码错误时 registrationCode 不应被消费，用户可以在 code 有效期内修改账号信息后重新提交。前端应限制连续提交频率，不要无限自动重试。

## 7. 与已绑定登录流程的区别

| 场景 | Callback flow | Callback result | 后续接口 |
| --- | --- | --- | --- |
| 第三方身份已绑定 | `login` | `success` | `/oauth/exchange` |
| 第三方身份未绑定 | `register` | `registration_required` | 用户选择 `/oauth/register` 或 `/oauth/bind` |
| 登录状态下新增平台绑定 | `link` | `success` | 不换 Token，重新请求 `/Identity/me` |

前端不要在 `flow=register` 时调用 `/oauth/exchange`。registrationCode 与 exchangeCode 用途不同，不能交叉使用。

## 8. 错误码处理

| message | 建议文案 | 前端处理 |
| --- | --- | --- |
| `invalid_registration_code` | 本次授权信息已失效，请重新使用第三方登录 | 清理当前表单并返回登录页 |
| `oauth_registration_store_unavailable` | 授权服务暂时不可用，请稍后重试 | 不持续重试，返回登录入口 |
| `invalid_credentials` | Void 账号或密码错误 | 保留 registrationCode 和表单，让用户修改后重试 |
| `registration_failed` | Void 账号创建失败，请检查填写内容 | 保留表单，根据普通注册规则提示 |
| `binding_conflict` | 该第三方身份或 Void 账号已绑定其他账号 | 停止提交，提示用户重新登录或更换账号 |
| `provider_failure` | 第三方登录失败，请重新授权 | 返回第三方登录入口 |

同时处理 HTTP 状态码和响应体 `code/message`。网络错误或 `5xx` 不应直接清除已有登录态。

## 9. 取消流程

用户点击“取消并返回登录”时：

1. 清除页面内存中的 registrationCode；
2. 清除注册或登录表单；
3. 返回登录页；
4. 不调用 register、bind 或 exchange 接口。

后端缓存中的待绑定身份会在短时间后自动过期，不需要前端调用取消接口。

## 10. 前端状态建议

```ts
type OAuthCallbackState =
  | { type: "loading" }
  | { type: "login-success"; exchangeCode: string }
  | {
      type: "registration-required";
      platform: string;
      registrationCode: string;
      mode: "choose" | "register" | "bind";
    }
  | { type: "link-success"; platform: string }
  | { type: "failed"; error: string };
```

在 register 或 bind 请求成功前不要写入 Void Token。成功后统一执行：

```text
保存 accessToken 和 refreshToken
  → 清空 registrationCode 和表单
  → GET /api/Identity/me
  → 写入当前账号状态
  → 跳转登录后首页
```

## 11. 安全要求

- registrationCode 必须短时有效且成功后只能使用一次。
- 第三方 Token 只能保存在后端缓存和数据库中，不能返回前端或放进 URL。
- callback 页面设置 `Referrer-Policy: no-referrer`。
- 不把包含 code 的完整 URL 发送到日志、埋点、错误监控或第三方分析服务。
- 不持久化 registrationCode，不把它当作登录凭证。
- 注册或绑定提交期间禁用重复提交按钮。
- 前端不能仅凭 platform、第三方昵称或第三方用户 ID 判定绑定成功。
- 最终登录账号和绑定状态始终以 `/api/Identity/me` 为准。

## 12. 对接检查清单

- [ ] callback 支持 `flow=register`。
- [ ] callback 支持 `result=registration_required`。
- [ ] 读取 registrationCode 后立即移除地址栏参数。
- [ ] 展示创建账号、登录已有账号和取消三个选择。
- [ ] 创建账号调用 `/api/OAuth/register`。
- [ ] 登录已有账号调用 `/api/OAuth/bind`。
- [ ] `flow=register` 时不调用 `/oauth/exchange`。
- [ ] register 或 bind 成功后保存 Void JWT。
- [ ] 保存 JWT 后调用 `/api/Identity/me`。
- [ ] 密码错误时允许用户修改后重新提交。
- [ ] code 失效或绑定冲突时引导用户重新发起第三方登录。
- [ ] 页面刷新时不尝试从持久化存储恢复 registrationCode。
- [ ] 不记录第三方 Token、Void Token、exchangeCode 或 registrationCode。
