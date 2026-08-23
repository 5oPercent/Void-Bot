# 账号管理前端对接说明

## 1. 功能范围

本文说明 Void 主账号以下功能的前端对接方式：

- 普通注册及第三方 OAuth 注册的名称规则；
- 修改当前账号密码；
- 检查目标名称是否可以使用；
- 修改当前账号名称并获取新的随机后缀。

账号管理接口只操作当前登录账号。前端必须携带 Void access token，且不能指定 `userId`。

```http
Authorization: Bearer {accessToken}
```

环境地址：

```text
测试环境：https://testapi.voidbot.cyooops.icu
生产环境：https://api.voidbot.cyooops.icu
```

## 2. 账号名称规则

普通注册 `/api/Identity/register` 和第三方 OAuth 注册 `/api/Identity/oauth/register` 使用相同规则：

- 最少 1 个字符；
- 最多 64 个字符；
- 不再限制名称只能由英文字母和数字组成。

改名使用相同的长度限制，但额外禁止任何空白字符，包括：

- 普通空格；
- Tab；
- 换行；
- 其他 Unicode 空白字符。

完整账号展示名格式为：

```text
{name}#{suffix}
```

其中 `suffix` 是后端生成的四位数字字符串，前端必须按字符串处理并保留前导零。

## 3. 修改密码

### 3.1 请求

```http
POST /api/Identity/change-password
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "oldPassword": "OldPassword1",
  "newPassword": "NewPassword1",
  "newPasswordConfirm": "NewPassword1"
}
```

字段说明：

| 字段 | 必填 | 说明 |
| --- | --- | --- |
| `oldPassword` | 是 | 当前账号密码 |
| `newPassword` | 是 | 新密码，至少 8 个字符，并包含大写字母、小写字母和数字 |
| `newPasswordConfirm` | 是 | 必须与 `newPassword` 完全一致 |

前端不要发送 `userId`。后端始终从 JWT 获取当前账号 ID。

### 3.2 成功响应

```json
{
  "code": 200,
  "message": "ok"
}
```

### 3.3 失败处理

| `message` | 说明 |
| --- | --- |
| `Old password is incorrect.` | 旧密码错误 |
| `New password confirmation does not match.` | 两次输入的新密码不一致 |
| `Account not found.` | 当前 JWT 对应账号不存在 |

密码格式不符合要求时会返回验证错误。前端应同时检查响应体的 `code` 和 `message`，不能只依赖 HTTP 状态码。

## 4. 检查是否可以改名

前端应在用户提交改名前调用可用性接口。`name` 必须使用 URL 查询参数编码。

```http
GET /api/Identity/change-name/availability?name={目标名称}
Authorization: Bearer {accessToken}
```

可以改名：

```json
{
  "code": 200,
  "message": "ok",
  "data": true
}
```

不可以改名：

```json
{
  "code": 200,
  "message": "ok",
  "data": false
}
```

以下情况返回 `data: false`：

- 名称为空或超过 64 个字符；
- 名称包含任何空白字符；
- 当前账号不存在；
- 目标名称下的账号数量已达到上限。

可用性结果只用于提交前提示。它不是名称预留，实际改名接口仍会再次检查。

TypeScript 示例：

```ts
export async function canChangeAccountName(
  apiBaseUrl: string,
  accessToken: string,
  name: string,
): Promise<boolean> {
  const url = new URL(`${apiBaseUrl}/api/Identity/change-name/availability`);
  url.searchParams.set("name", name);

  const response = await fetch(url, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });
  const result = (await response.json()) as ApiResponse<boolean>;

  return result.code === 200 && result.data === true;
}
```

## 5. 修改名称

### 5.1 请求

```http
POST /api/Identity/change-name
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "name": "NewName"
}
```

后端会：

1. 再次校验名称长度、空白字符和目标名称容量；
2. 为账号随机生成新的四位后缀；
3. 保证新后缀与旧后缀不同；
4. 避免新 `name + suffix` 与已有账号冲突；
5. 返回更新后的当前账号信息。

### 5.2 成功响应

```json
{
  "code": 200,
  "message": "ok",
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "name": "NewName",
    "suffix": "0427",
    "displayName": "NewName#0427",
    "createdAt": "2026-08-22T10:00:00+08:00",
    "updatedAt": "2026-08-23T10:00:00+08:00",
    "thirdParties": []
  }
}
```

前端应使用响应中的 `data` 更新当前账号状态，不要继续保留旧名称或旧后缀。也可以在成功后重新调用：

```http
GET /api/Identity/me
Authorization: Bearer {accessToken}
```

### 5.3 失败响应

名称格式错误：

```json
{
  "code": 400,
  "message": "Name must be between 1 and 64 characters long and cannot contain whitespace characters."
}
```

目标名称不可用：

```json
{
  "code": 409,
  "message": "Name cannot be changed."
}
```

前端收到 `409` 时应重新调用可用性接口或提示用户更换名称，不要自动重复提交。

## 6. TypeScript 类型

```ts
export interface ApiResponse<T> {
  code: number;
  message: string;
  data: T | null;
}

export interface CurrentAccount {
  id: string;
  name: string;
  suffix: string;
  displayName: string;
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
```

## 7. 前端检查清单

- [ ] 账号名称输入限制为 1–64 个字符。
- [ ] 改名表单检测并提示空白字符。
- [ ] 提交改名前调用 availability 接口。
- [ ] 不把 availability 的成功结果视为名称预留。
- [ ] 不在修改密码或改名请求中发送 `userId`。
- [ ] 改名成功后同时更新 `name`、`suffix` 和 `displayName`。
- [ ] `suffix` 始终按字符串处理。
- [ ] 同时检查响应体的 `code`、`message` 和 `data`。
