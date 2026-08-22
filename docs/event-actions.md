# 内置事件动作配置

事件动作规则由数据库持久化，并通过 API 管理。前端先读取定义元数据，再根据元数据渲染事件、动作和参数控件，不需要硬编码可用类型。

## 命名约定

- `EventAction` 是整个可配置事件动作模块的统一名称，用于目录、命名空间、Controller、Rule 和 Service。
- `IBuiltInEvent` 是主业务发布的内置事件，例如 `AccountRegisteredEvent`。
- `IConfiguredEventActionExecutor` 是配置动作执行器，例如 `GrantRolesEventActionExecutor`。
- `EventActionRule` 将一个内置事件与一组有序动作关联起来。

不再使用 `Automation` 指代该模块，避免与更广义的自动化、调度或工作流概念混淆。

## 当前能力

| 事件 | 触发时机 | 动作 | 参数 |
| --- | --- | --- | --- |
| `Account.Registered` | Void 账号事务提交成功后 | `GrantRoles` | `RoleIds`；可选 `Duration` + `DurationUnit` |

规则中的动作按数组顺序执行；多条规则按 `Sequence`、`CreatedAt` 排序执行。禁用的规则或动作不会执行。

## API

所有接口都需要登录并受现有 `[Security]` 权限体系控制。

| 方法 | 地址 | 用途 |
| --- | --- | --- |
| `GET` | `/api/event-action/definitions` | 获取支持的事件、动作、参数定义和参数数据源 |
| `GET` | `/api/event-action/rules?event=Account.Registered` | 获取规则；`event` 可省略 |
| `POST` | `/api/event-action/rules/add` | 新增规则 |
| `POST` | `/api/event-action/rules/update` | 更新规则 |
| `POST` | `/api/event-action/rules/delete` | 批量删除规则 |

定义接口会返回参数的 `ValueSource`、`DataSourceEndpoint`、`DataSourceValueField` 和 `DataSourceLabelField`。例如角色参数会指向现有角色树接口，前端可以据此加载选项并提交角色 ID。

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
        "description": "Void 账号事务提交成功后执行配置动作。"
      }
    ],
    "actions": [
      {
        "type": "GrantRoles",
        "displayName": "授权角色",
        "description": "为事件关联的用户授予一个或多个角色。",
        "supportedEvents": ["Account.Registered"],
        "arguments": [
          {
            "name": "RoleIds",
            "displayName": "角色",
            "description": "选择需要自动授予的角色。",
            "required": true,
            "multiple": true,
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
            "required": false,
            "multiple": false,
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
            "required": false,
            "multiple": false,
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
    ]
  }
}
```

前端选中事件后，只展示 `supportedEvents` 包含该事件 `name` 的动作。动作参数需根据 `required` 和 `multiple` 生成控件；存在 `dataSourceEndpoint` 时，从该接口加载选项，并使用 value/label 字段定义读取值。

新增注册授权规则：

```json
{
  "name": "新用户默认角色",
  "event": "Account.Registered",
  "enabled": true,
  "sequence": 0,
  "actions": [
    {
      "type": "GrantRoles",
      "enabled": true,
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
    "actions": [
      {
        "type": "GrantRoles",
        "enabled": true,
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

### 前端对接流程

1. 进入配置页时请求 `GET /api/event-action/definitions`，缓存本次页面所需的事件和动作定义。
2. 请求 `GET /api/event-action/rules`；如只编辑某个事件，传递 `?event=Account.Registered`。
3. 用户选择事件后，根据 `supportedEvents` 过滤可用动作。
4. 要求用户填写规则名称；添加动作后，根据 `arguments` 渲染参数控件，并将选中值统一提交为 JSON 对象，每个参数值都是字符串数组。
5. 新增调用 `/rules/add`，编辑调用 `/rules/update`，删除调用 `/rules/delete`。
6. 每次根据响应体 `code` 判断结果；不是 `200` 时展示 `message`，不要仅根据 HTTP 状态判断。

保存规则时会校验事件和动作是否受支持、动作能否用于该事件、参数格式以及角色是否存在。只有当前有效的 `SuperAdmin` 才能保存包含 `SuperAdmin` 自动授权的规则。执行阶段还会再次校验，并跳过用户已经拥有的角色。

普通注册、OAuth 注册和第三方自动建号都遵循相同边界：Service 只执行业务逻辑并返回结果，对应 Command Handler 确认创建成功后，再通过 Mediator.Net 发布 `AccountRegisteredEvent`。事件处理器将配置动作登记为事务提交后任务：账号注册先保存并提交，再执行角色授权。授权失败只记录日志，不会回滚已成功的账号注册。首位用户的 `SuperAdmin` bootstrap 授权仍由原有安全逻辑处理。

## 运行流程与边界

1. Command Handler 调用 Service 执行主业务。
2. Service 返回成功结果及事件所需的业务标识；Service 不引用 Mediator.Net，也不发布事件。
3. Handler 根据结果判断是否触发，并通过 `context.PublishAsync` 发布内置事件。
4. Event Handler 不立即执行动作，而是将动作分发登记到 `IUnitOfWorkExecutionContext` 的提交后回调。
5. 最外层 UnitOfWork 先保存并提交主业务，然后执行提交后回调。
6. `IBuiltInEventDispatcher` 按规则 `Sequence` 和创建时间取出启用规则，再按规则内数组顺序调用已启用的动作执行器。
7. 动作全部完成后单独保存；任一动作失败时清理本次动作的 EF 跟踪状态并记录日志，不影响已提交的主业务。

当前提交后动作仍在原请求内同步等待，属于 best-effort 扩展机制：主业务提交后如果进程退出，动作不保证重试。需要必达、可重试或审计的关键业务事件，应另行引入 Outbox 或可持久化任务，不应直接依赖当前机制。

## 新增内置事件规范

1. 在 Message 项目的 `Events/EventAction` 中新增事件类型，实现 `IBuiltInEvent`。事件只携带动作执行所需的最小业务数据，不携带 Service、DbContext 或回调。
2. 在 `BuiltInEventNames` 中增加稳定且唯一的事件名，命名使用 `<Domain>.<PastTenseAction>`；事件名会持久化到规则表，不可随意改名。
3. 在 `EventActionRuleService.EventDefinitions` 增加前端展示定义，准确说明事件的业务成功条件和触发时机。
4. 只在对应 Command Handler 中发布事件：先调用 Service，再根据响应结果调用 `context.PublishAsync`。不得在 Service 中注入 `IMediator` 或调用 `SendAsync`/`PublishAsync`。
5. 同一业务如果有多个入口，每个会产生该事件的 Handler 都必须发布，并确保只在真正新建业务对象时发布一次。
6. Service 如果需要向 Handler 传递触发条件，优先在原有 Message Response 上增加带用途注释的 `[JsonIgnore]` 字段；新增 DTO 必须放在 Message 项目。
7. 在 Core 的 `Handler/EventHandler/EventAction` 中增加对应 `IEventHandler<TEvent>`，参照 `AccountRegisteredEventHandler` 将动作分发登记为提交后回调，不得让配置动作的失败回滚主业务。
8. 补充触发条件、不触发条件、多入口去重、主业务回滚时不执行、提交后动作失败不影响主业务等测试。

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

1. 在 Core 的 `Services/EventAction` 中实现 `IConfiguredEventActionExecutor`，`Type` 必须稳定且唯一。
2. `Definition` 同时定义前端展示信息、支持的事件、参数形式和数据源；前端通过定义接口联动，不硬编码动作参数。
3. `ValidateAsync` 负责规则保存前校验，`ExecuteAsync` 执行时仍必须重新校验外部对象是否有效。
4. `ExecuteAsync` 需要幂等，不得自行提交 UnitOfWork；动作由提交后回调统一保存。
5. 新动作依赖注入扫描自动注册；定义接口和规则校验会自动识别，无需修改 Controller。
