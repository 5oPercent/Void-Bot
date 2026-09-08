# 内置事件动作配置

事件动作规则由数据库持久化，并通过 API 管理。前端先读取定义元数据，再根据元数据渲染事件、动作和参数控件，不需要硬编码可用类型。

## 本次范围

本次提供即时事件动作配置、条件判断、Action 版本、失败策略和执行审计。不包含延迟执行，也不包含 Action 输出传递、依赖关系、条件分支或并行节点等动作编排能力。

当前执行模型保持为：

```text
内置事件 → 匹配启用规则 → 判断规则条件 → 按数组顺序执行启用 Action
```

## 前端升级要求

旧版新增请求不传新字段时仍可使用默认值：`conditions` 默认为空数组，`version` 默认为 `1`，`failurePolicy` 默认为 `StopAll`。但规则更新采用全量覆盖，前端读取规则后必须保留并完整回传后端返回的新字段，不能在表单转换时丢弃。

| 字段 | 前端处理 | 兼容默认值 |
| --- | --- | --- |
| `conditions` | 规则级条件数组；暂不提供条件 UI 时也应原样回传 | `[]` |
| `actions[].version` | 从动作定义读取；编辑时按 `type + version` 定位定义 | `1` |
| `actions[].failurePolicy` | 使用定义接口的 `failurePolicies` 渲染下拉框 | `StopAll` |
| `events[].fields` | 用于渲染规则条件字段和操作符 | 无条件时可暂不渲染 |
| `actions[].arguments[]` 校验字段 | 用于前端即时校验，后端仍会重复校验 | 可分阶段接入 |
| `executionStatuses` | 用于执行审计筛选和状态展示 | 审计页面可后续接入 |

同一动作类型后续可能同时存在多个版本，前端列表项和缓存键不能只使用 `type`，建议使用 `${type}:${version}`。新增动作时可以默认选择同一 `type` 中版本号最大的定义；编辑已有规则时必须使用规则保存的准确版本。如果对应定义已经不存在，应提示“不支持的 Action 版本”，不得静默替换成最新版本。

最低兼容提交结构：

```json
{
  "name": "新用户默认角色",
  "event": "Account.Registered",
  "enabled": true,
  "sequence": 0,
  "conditions": [],
  "actions": [
    {
      "type": "GrantRoles",
      "version": 1,
      "enabled": true,
      "failurePolicy": "StopAll",
      "arguments": {
        "RoleIds": ["角色 ID"]
      }
    }
  ]
}
```

## 命名约定

- `EventAction` 是整个可配置事件动作模块的统一名称，用于目录、命名空间、Controller、Rule 和 Service。
- `IBuiltInEvent` 是主业务发布的内置事件，例如 `AccountRegisteredEvent`。
- `IBuiltInEventDefinitionProvider` 提供内置事件的前端展示定义，例如 `AccountRegisteredEventDefinitionProvider`。
- `IConfiguredEventActionExecutor` 是配置动作执行器，例如 `GrantRolesEventActionExecutor`。
- `EventActionRule` 将一个内置事件与一组有序动作关联起来。

不再使用 `Automation` 指代该模块，避免与更广义的自动化、调度或工作流概念混淆。

## 当前能力

| 事件 | 触发时机 | 动作 | 参数 |
| --- | --- | --- | --- |
| `Account.Registered` | Void 账号事务提交成功后 | `GrantRoles`、`SendEmail`、`SendAccountRegisteredEmail` | 角色参数、邮件主题与内容，或无参数内置通知 |
| `Account.PasswordForceReset` | 管理人员强制重置账号密码事务提交成功后 | `GrantRoles`、`SendEmail`、`SendPasswordForceResetEmail` | 角色参数、邮件主题与内容，或无参数内置通知 |
| `Account.ThirdPartyForceUnlinked` | 管理人员强制解绑第三方授权事务提交成功后 | `GrantRoles`、`SendEmail`、`SendThirdPartyForceUnlinkedEmail` | 角色参数、邮件主题与内容，或无参数内置通知 |

三个账号事件均提供 `UserId` 条件字段；`Account.ThirdPartyForceUnlinked` 额外提供 `Platform` 条件字段，可按第三方平台筛选规则。失败的密码重置或解绑操作不会发布事件。

`SendEmail` 向事件关联用户的已验证邮箱发送纯文本邮件，不允许在规则中指定任意外部收件地址。动作参数为单值 `Subject` 和 `Content`，分别最多 200 和 10000 个字符。两者都支持 `{UserId}`、`{DisplayName}` 和 `{Platform}` 占位符；只有 `Content` 支持 `{Password}`，邮件标题禁止包含密码。非第三方解绑事件中的 `{Platform}` 会替换为空字符串；`{Password}` 仅在 `Account.PasswordForceReset` 事件正文中有值，其他事件会替换为空字符串。密码只在本次事件的内存执行链中传递，不会进入事件定义字段、规则、执行记录或结构化日志。

`SendAccountRegisteredEmail`、`SendPasswordForceResetEmail` 和 `SendThirdPartyForceUnlinkedEmail` 是无参数内置邮件动作，只支持各自对应的账号事件。它们使用统一 HTML 邮件模板与 `Email/BuiltIn` i18n 文案，分别发送注册成功通知、临时密码或被解绑平台。自定义 `SendEmail` 动作保持不变，可与内置动作独立配置。

规则中的动作按数组顺序执行；多条规则按 `Sequence`、`CreatedAt` 排序执行。禁用的规则或动作不会执行。

## 接口说明

所有接口都需要登录并受现有 `[Security]` 权限体系控制。

| 方法 | 地址 | 用途 |
| --- | --- | --- |
| `GET` | `/api/event-action/definitions` | 获取支持的事件、动作、参数定义和参数数据源 |
| `GET` | `/api/event-action/rules?event=Account.Registered` | 获取规则；`event` 可省略 |
| `GET` | `/api/event-action/executions` | 分页查询动作执行记录 |
| `POST` | `/api/event-action/rules/add` | 新增规则 |
| `POST` | `/api/event-action/rules/update` | 更新规则 |
| `POST` | `/api/event-action/rules/delete` | 批量删除规则 |

定义接口会返回参数的 `ValueSource`、`DataSourceEndpoint`、`DataSourceValueField` 和 `DataSourceLabelField`。例如角色参数会指向现有角色树接口，前端可以据此加载选项并提交角色 ID。

### 参数元数据含义

| 字段 | 含义 |
| --- | --- |
| `valueType` | 值类型，例如 `String`、`Integer`、`Guid` |
| `required` | 参数是否必须配置 |
| `multiple` | 是否允许提交多个值 |
| `minimum` / `maximum` | 数值参数的固定范围 |
| `minLength` / `maxLength` | 参数值数组允许的最少或最多项目数 |
| `requiredWith` | 当前参数与哪些参数必须同时配置 |
| `maximumDependsOn` | 动态最大值依赖的另一个参数 |
| `maximumByOption` | 依赖参数不同选项对应的动态最大值 |
| `valueSource` | 控件或数据来源类型，例如 `Role`、`Number`、`Options` |
| `options` | 可直接渲染的静态 value/label 选项 |
| `dataSourceEndpoint` | 需要远程加载选项时使用的接口 |

无论参数是否单选，规则请求中的 `arguments` 值都统一使用字符串数组。单值参数提交一个元素，多值参数提交多个元素，未配置的可选参数直接省略。

### 统一响应结构

所有接口返回统一响应体：

```json
{
  "code": 200,
  "message": "ok",
  "data": {}
}
```

`code` 是业务结果码，前端必须以它判断业务是否成功。参数或规则校验失败时，HTTP 状态通常仍为 `200`，响应体中的 `code` 为 `400`，`message` 包含原因。未登录或无权限时可能直接返回 HTTP `401`/`403`。无 `data` 的响应只包含 `code` 和 `message`。

### 定义接口响应

`GET /api/event-action/definitions`

```json
{
  "code": 200,
  "message": "ok",
  "data": {
    "events": [
      {
        "name": "Account.Registered",
        "displayName": "账号注册成功",
        "description": "Void 账号事务提交成功后执行配置动作。",
        "fields": [
          {
            "name": "UserId",
            "displayName": "用户 ID",
            "description": "本次注册成功的 Void 用户 ID。",
            "valueType": "Guid",
            "multiple": false,
            "operators": ["Equal", "NotEqual", "In", "NotIn"],
            "options": []
          }
        ]
      }
    ],
    "actions": [
      {
        "type": "GrantRoles",
        "version": 1,
        "displayName": "授权角色",
        "description": "为事件关联的用户授予一个或多个角色。",
        "supportedEvents": [
          "Account.Registered",
          "Account.PasswordForceReset",
          "Account.ThirdPartyForceUnlinked"
        ],
        "arguments": [
          {
            "name": "RoleIds",
            "displayName": "角色",
            "description": "选择需要自动授予的角色。",
            "valueType": "Guid",
            "required": true,
            "multiple": true,
            "minimum": null,
            "maximum": null,
            "maximumDependsOn": null,
            "maximumByOption": {},
            "minLength": 1,
            "maxLength": null,
            "requiredWith": [],
            "valueSource": "Role",
            "dataSourceEndpoint": "/api/security/role/tree",
            "dataSourceValueField": "id",
            "dataSourceLabelField": "name",
            "options": []
          },
          {
            "name": "Duration",
            "displayName": "授权时长",
            "description": "留空表示永久有效；填写时必须同时选择时间单位。",
            "valueType": "Integer",
            "required": false,
            "multiple": false,
            "minimum": 1,
            "maximum": null,
            "maximumDependsOn": "DurationUnit",
            "maximumByOption": {
              "Second": 3153600000,
              "Minute": 52560000,
              "Hour": 876000,
              "Day": 36500
            },
            "minLength": null,
            "maxLength": null,
            "requiredWith": ["DurationUnit"],
            "valueSource": "Number",
            "dataSourceEndpoint": "",
            "dataSourceValueField": "",
            "dataSourceLabelField": "",
            "options": []
          },
          {
            "name": "DurationUnit",
            "displayName": "时间单位",
            "description": "支持秒、分钟、小时和天。",
            "valueType": "String",
            "required": false,
            "multiple": false,
            "minimum": null,
            "maximum": null,
            "maximumDependsOn": null,
            "maximumByOption": {},
            "minLength": null,
            "maxLength": null,
            "requiredWith": ["Duration"],
            "valueSource": "Options",
            "dataSourceEndpoint": "",
            "dataSourceValueField": "",
            "dataSourceLabelField": "",
            "options": [
              { "value": "Second", "label": "秒" },
              { "value": "Minute", "label": "分钟" },
              { "value": "Hour", "label": "小时" },
              { "value": "Day", "label": "天" }
            ]
          }
        ]
      }
    ],
    "failurePolicies": [
      { "value": "StopAll", "label": "停止全部" },
      { "value": "StopRule", "label": "停止当前规则" },
      { "value": "Continue", "label": "继续执行" }
    ],
    "executionStatuses": [
      { "value": "Succeeded", "label": "成功" },
      { "value": "Failed", "label": "失败" }
    ]
  }
}
```

前端选中事件后，只展示 `supportedEvents` 包含该事件 `name` 的动作。动作参数需根据 `valueType`、`required`、`multiple`、数值或长度范围、`requiredWith` 以及动态上限生成控件；存在 `dataSourceEndpoint` 时，从该接口加载选项，并使用 value/label 字段定义读取值。后端仍会执行完整校验。

新增注册授权规则：

```json
{
  "name": "新用户默认角色",
  "event": "Account.Registered",
  "enabled": true,
  "sequence": 0,
  "conditions": [
    {
      "field": "UserId",
      "operator": "NotIn",
      "values": ["00000000-0000-0000-0000-000000000001"]
    }
  ],
  "actions": [
    {
      "type": "GrantRoles",
      "version": 1,
      "enabled": true,
      "failurePolicy": "StopAll",
      "arguments": {
        "RoleIds": [ "角色 ID" ],
        "Duration": [ "2" ],
        "DurationUnit": [ "Hour" ]
      }
    }
  ]
}
```

`name` 是必填的自定义规则名称，最长 128 个字符。`Duration` 和 `DurationUnit` 都可省略，表示永久授权；配置时必须同时传递，`Duration` 是仅包含一个正整数字符串的数组，`DurationUnit` 是仅包含 `Second`、`Minute`、`Hour` 或 `Day` 之一的数组。最长授权时长为 36500 天。同一 `GrantRoles` 动作中的角色共享授权时长；如需不同时长，添加多个动作。如果用户已经拥有某角色，该动作会跳过该角色，不覆盖原有到期时间。

`conditions` 中的条件使用 AND 关系，字段和可用操作符来自事件定义。`Equal`、`NotEqual` 只接受一个值，`In`、`NotIn` 接受一个或多个值。空数组表示不限制事件数据。

`version` 必须与定义接口返回的 Action 版本一致。`failurePolicy` 决定 Action 失败后的行为：`StopAll` 停止本次事件的全部后续动作，`StopRule` 跳过当前规则的剩余动作后继续下一条规则，`Continue` 继续当前规则的下一个动作。省略时默认为 `StopAll`。

更新规则使用相同结构，并增加规则 `id`。删除规则的请求结构为：

```json
{
  "idList": [ "规则 ID" ]
}
```

`POST /api/event-action/rules/add` 和 `POST /api/event-action/rules/update` 成功时的 `data` 是完整规则：

```json
{
  "code": 200,
  "message": "ok",
  "data": {
    "id": "规则 ID",
    "name": "新用户默认角色",
    "event": "Account.Registered",
    "enabled": true,
    "sequence": 0,
    "conditions": [],
    "actions": [
      {
        "type": "GrantRoles",
        "version": 1,
        "enabled": true,
        "failurePolicy": "StopAll",
        "arguments": {
          "RoleIds": ["角色 ID"],
          "Duration": ["2"],
          "DurationUnit": ["Hour"]
        }
      }
    ],
    "createdAt": "2026-08-23T00:00:00Z",
    "updatedAt": null
  }
}
```

`GET /api/event-action/rules` 的 `data` 是上述规则对象数组。`id`、`createdAt` 和 `updatedAt` 由后端管理；新增时不传。`sequence` 越小越先执行，同一规则内的动作按 `actions` 数组顺序执行。

### 执行记录接口

`GET /api/event-action/executions` 支持 `event`、`ruleId`、`status`、`startDate`、`endDate`、`pageIndex`、`pageSize` 查询参数。`status` 使用定义接口返回的 `Succeeded` 或 `Failed`。

```json
{
  "code": 200,
  "message": "ok",
  "data": {
    "pageIndex": 1,
    "pageSize": 20,
    "total": 1,
    "totalPages": 1,
    "items": [
      {
        "id": "执行记录 ID",
        "eventId": "本次事件 ID",
        "event": "Account.Registered",
        "eventOccurredAt": "2026-08-23T00:00:00Z",
        "ruleId": "规则 ID",
        "ruleName": "新用户默认角色",
        "actionIndex": 0,
        "actionType": "GrantRoles",
        "actionVersion": 1,
        "failurePolicy": "StopAll",
        "status": "Succeeded",
        "startedAt": "2026-08-23T00:00:00Z",
        "completedAt": "2026-08-23T00:00:00.025Z",
        "durationMilliseconds": 25,
        "errorMessage": null
      }
    ],
    "hasPrevPage": false,
    "hasNextPage": false
  }
}
```

### 前端对接流程

1. 进入配置页时请求 `GET /api/event-action/definitions`，缓存本次页面所需的事件和动作定义。
2. 请求 `GET /api/event-action/rules`；如只编辑某个事件，传递 `?event=Account.Registered`。
3. 用户选择事件后，根据 `supportedEvents` 过滤可用动作。
4. 要求用户填写规则名称；根据事件 `fields` 渲染条件，根据动作 `arguments` 渲染参数控件，并将条件值和参数值统一提交为字符串数组。
5. 新增调用 `/rules/add`，编辑调用 `/rules/update`，删除调用 `/rules/delete`。
6. 每次根据响应体 `code` 判断结果；不是 `200` 时展示 `message`，不要仅根据 HTTP 状态判断。
7. 审计页面调用 `/executions`，使用定义接口返回的执行状态生成筛选项，并展示规则快照、Action 版本、耗时和错误信息。

### 前端验收清单

- [ ] 页面初始化时先请求定义接口，再请求规则接口。
- [ ] 动作使用 `type + version` 作为唯一标识。
- [ ] 编辑规则时完整回传 `conditions`、`version`、`failurePolicy` 和 `arguments`。
- [ ] 事件切换后只展示 `supportedEvents` 包含该事件的动作。
- [ ] 条件字段和操作符来自事件定义，不在前端硬编码。
- [ ] Action 失败策略选项来自 `failurePolicies`。
- [ ] `Duration` 与 `DurationUnit` 同时填写或同时省略。
- [ ] `Duration` 根据 `DurationUnit` 和 `maximumByOption` 校验最大值。
- [ ] 所有业务结果根据响应体 `code` 判断，而不是只判断 HTTP 状态。
- [ ] 暂不接入条件或审计 UI 时，仍能原样保留后端返回的对应字段。

保存规则时会校验事件和动作是否受支持、动作能否用于该事件、参数格式以及角色是否存在。只有当前有效的 `SuperAdmin` 才能保存包含 `SuperAdmin` 自动授权的规则。执行阶段还会再次校验，并跳过用户已经拥有的角色。

普通注册、OAuth 注册和第三方自动建号都遵循相同边界：Service 只执行业务逻辑并返回结果，对应 Command Handler 确认创建成功后，再通过 Mediator.Net 发布 `AccountRegisteredEvent`。事件处理器将配置动作登记为事务提交后任务：账号注册先保存并提交，再执行角色授权。授权失败只记录日志，不会回滚已成功的账号注册。首位用户的 `SuperAdmin` bootstrap 授权仍由原有安全逻辑处理。

## 运行流程与边界

1. Command Handler 调用 Service 执行主业务。
2. Service 返回成功结果及事件所需的业务标识；Service 不引用 Mediator.Net，也不发布事件。
3. Handler 根据结果判断是否触发，并通过 `context.PublishAsync` 发布内置事件。
4. Event Handler 不立即执行动作，而是将动作分发登记到 `IUnitOfWorkExecutionContext` 的提交后回调。
5. 最外层 UnitOfWork 先保存并提交主业务，然后执行提交后回调。
6. `IBuiltInEventDispatcher` 按规则 `Sequence` 和创建时间取出启用规则，判断全部条件后，再按规则内数组顺序调用已启用的动作执行器。
7. 每个动作的业务变更和成功执行记录一起保存；失败时清理该动作未提交的 EF 跟踪状态，单独保存失败记录，并按 `failurePolicy` 决定后续流程，不影响已提交的主业务。

当前提交后动作仍在原请求内同步等待，属于 best-effort 扩展机制：主业务提交后如果进程退出，动作不保证重试。需要必达或可重试的关键业务事件，应另行引入 Outbox 或可持久化任务，不应直接依赖当前机制。执行审计只记录动作结果，不等同于可靠投递保障，也不会保存 Action 参数或事件敏感数据。

## 新增内置事件规范

1. 在 Message 项目的 `Events/EventAction` 中新增事件类型，实现 `IBuiltInEvent`，为每次事件实例生成唯一 `EventId` 并记录 `OccurredAt`。事件只携带动作执行所需的最小业务数据，不携带 Service、DbContext 或回调。
2. 在 `BuiltInEventNames` 中增加稳定且唯一的事件名，命名使用 `<Domain>.<PastTenseAction>`；事件名会持久化到规则表，不可随意改名。
3. 在 Core 的 `Services/EventAction` 中实现 `IBuiltInEventDefinitionProvider`，通过 `Definition` 提供前端展示定义和可配置条件字段，通过 `GetFieldValues` 提供事件实例的实际字段值。Provider 由依赖注入自动发现，不需要修改 `EventActionRuleService`。
4. 只在对应 Command Handler 中发布事件：先调用 Service，再根据响应结果调用 `context.PublishAsync`。不得在 Service 中注入 `IMediator` 或调用 `SendAsync`/`PublishAsync`。
5. 同一业务如果有多个入口，每个会产生该事件的 Handler 都必须发布，并确保只在真正新建业务对象时发布一次。
6. Service 如果需要向 Handler 传递触发条件，优先在原有 Message Response 上增加带用途注释的 `[JsonIgnore]` 字段；新增 DTO 必须放在 Message 项目。
7. 在 Core 的 `Handler/EventHandler/EventAction` 中增加对应 `IEventHandler<TEvent>`，参照 `AccountRegisteredEventHandler` 将动作分发登记为提交后回调，不得让配置动作的失败回滚主业务。
8. 补充定义自动发现、触发条件、不触发条件、多入口去重、主业务回滚时不执行、提交后动作失败不影响主业务等测试。

Handler 发布模板：

```csharp
var response = await service.ExecuteAsync(context.Message, cancellationToken);
if (response.EventEntityId.HasValue)
{
    await context.PublishAsync(
        new DomainActionCompletedEvent(response.EventEntityId.Value),
        cancellationToken);
}

return response;
```

## 新增动作规范

1. 在 Core 的 `Services/EventAction` 中实现 `IConfiguredEventActionExecutor`，同一 `Type + Version` 必须唯一。参数结构发生不兼容变化时增加版本，不得直接改变已持久化版本的语义。
2. `Definition` 同时定义前端展示信息、版本、支持的事件、参数类型、校验范围、依赖关系和数据源；前端通过定义接口联动，不硬编码动作参数。
3. `ValidateAsync` 负责规则保存前校验，`ExecuteAsync` 执行时仍必须重新校验外部对象是否有效。
4. `ExecuteAsync` 需要幂等，不得自行提交 UnitOfWork；动作由提交后回调统一保存。
5. 新动作依赖注入扫描自动注册；定义接口和规则校验会自动识别，无需修改 Controller。
