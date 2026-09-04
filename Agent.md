# Void-Bot 开发规范

本文档是项目新增功能、修复和重构时的默认约定。优先遵循现有实现；如需调整规范，先确认影响范围，再做最小必要修改。

## 1. 架构总览

### 1.1 调用链

```text
入口(Api / Job / Discord)
        ↓
      Mediator
        ↓
     Handler
        ↓
     Service
        ↓
 DataProvider
        ↓
Repository / DbContext
```

- `Void.Bot.Api`：HTTP 边界、路由、模型绑定、认证/权限标记、OpenAPI 和结果转换。
- `Void.Bot.Message`：跨层消息契约（Command、Request、Event、DTO、Enum、Constants）。
- `Void.Bot.Core`：Handler、Service、DataProvider、Domain、Validator、Middleware、Job 和基础设施。
- 依赖只能沿调用链向下；Message 不得依赖 Core 或 Api。

### 1.2 Controller、Handler、Service 职责

- Controller 只能通过 `IMediator.SendAsync` 发送 Command、通过 `IMediator.RequestAsync` 发送 Request。
- Controller 禁止直接调用 Service、DataProvider、Repository 或 `DbContext`，也不实现业务规则、数据库操作或跨 Service 编排。
- 路由参数写入消息、OAuth 重定向等边界适配可以保留在 Controller。
- 一个消息通常对应一个 Handler，实现对应的 Mediator.Net Handler 接口。
- Handler 保持薄：读取 `context.Message`、调用 Service、传递 `CancellationToken`；不要复制业务校验、查询、映射或持久化逻辑。
- Service 负责业务规则、状态判断、用例编排、实体变更、DTO 映射和响应组装；Service 间通过接口协作。
- 命令成功后需要发布事件时，可在 Service 返回成功结果后由 Handler 发布；事件业务放在事件 Handler/Service。
- Service 接口和实现位于 `Core/Services/<Domain>`，用 `IScope`、`ISingleton` 等标记声明生命周期。

## 2. 文件与命名

### 2.1 Service / DataProvider

- 主文件使用 `<Domain>Service.cs`，例如 `AccountService.cs`、`RoleService.cs`。
- 按稳定的业务主题拆分 partial 时使用 `<Domain>Service.<Feature>.cs`，例如 `AccountService.Email.cs`、`RoleService.ConflictMatrix.cs`。
- 同一主题的 `partial interface I<Domain>Service` 与 `partial class <Domain>Service` 放在同名分片文件中；分片只放该主题成员。
- 不按“字段/私有方法”等技术维度拆文件，不因拆分而重复构造函数或 DI 注册；规模较小时保持单一主文件。
- DataProvider 使用同样规则：`<Domain>DataProvider.cs`、`<Domain>DataProvider.<Feature>.cs`。

### 2.2 领域目录

- Entity：`Core/Domain/<Domain>`。
- Service：`Core/Services/<Domain>`。
- Handler：`Core/Handler/CommandHandler/<Domain>`、`RequestHandler/<Domain>`、`EventHandler/<Domain>`。
- Validator：`Core/Validators/<Domain>`。
- 配置：`Core/Settings/<Domain>`；系统配置放 `Core/Settings/System`。
- 本地化模板：`Core/Resources/i18n/<System>`。

## 3. Message 契约

| 类型 | 目录 | 规则 |
| --- | --- | --- |
| Command | `Message/Commands/<Domain>` | 写操作，实现 `ICommand`，配套 Response |
| Request | `Message/Request/<Domain>` | 读操作，实现 `IRequest`，配套 Response |
| Event | `Message/Events/<Domain>` | 领域/集成通知，实现 `IEvent` |
| DTO | `Message/Dto/<Domain>` | 对外或跨模块数据模型，不暴露 Entity |
| Enum | `Message/Enum/<Domain>` | 跨项目共享的状态、平台、类型等固定值 |
| Constants | `Message/Constants/<Domain>` | 权限码、缓存键、外部路径、正则等固定值 |

- 响应继承 `VoidBotResponse` 或 `VoidBotResponse<T>`；分页复用 `PageInputDto`、`PagedListDto<T>`。
- 固定 enum 使用显式数值；已发布值不得随意重排、复用或删除。
- 固定值只有一个来源，不在 Controller、Service、Entity 中重复定义字符串或数字。
- Message 保持纯契约，不引用 EF Core、DbContext、Service 或业务实现；敏感信息不得放入 DTO。

## 4. 数据访问与事务

### 4.1 Unit of Work

- Mediator 全局管道统一执行日志、响应、消息验证、上下文预加载和 Unit of Work。
- 对 `ICommand` 和 `IEvent`，Unit of Work 自动开启最外层事务，成功后执行 `SaveChangesAsync`、提交事务和 after-commit 回调，异常时回滚。
- Repository、DataProvider、Service 在普通 Mediator 调用中不要手动调用 `SaveChanges`/`SaveChangesAsync`，也不要新增 `SaveChange` 方法。
- `IRequest` 应保持只读，不产生写入副作用。DbContext 的保存会统一维护审计字段、创建时间和更新时间。

### 4.2 Hangfire Job 例外

- Job 通过 Mediator 调用 Command/Event 时，仍由 Mediator 管道负责事务和自动保存。
- Job 直接调用 Service、DataProvider 或实体变更逻辑时，不会自动经过 Mediator；必须在明确的应用边界注入 `IUnitOfWork` 并显式调用 `SaveChangesAsync`，必要时自行处理事务、异常和提交边界。
- 后台任务优先转发 Command；只有独立提交、分批处理或特殊 after-commit 需求才使用显式保存，并在代码中注明原因。

### 4.3 Repository / DataProvider 规则

- Repository 只封装通用 EF Core 增删改查；DataProvider 提供按领域命名的数据访问方法。
- 复杂查询优先新增 DataProvider 方法，Service 不直接依赖通用 Repository 处理业务查询。
- 读取默认使用 `QueryNoTracking()`；需要在当前工作单元修改实体时才使用跟踪查询 `Query()`。
- 查询、分页和投影尽量在数据库端完成；所有 I/O 传递 `CancellationToken`。
- DataProvider 不提交事务、不吞异常、不做业务决策；写入方法只表达 Create/Update/Delete 意图。

## 5. 配置 Settings

- 配置类命名为 `<Domain>Setting`，放在 `Core/Settings/<Domain>`；系统级配置放 `Settings/System`，例如 `HangfireSetting`、`JwtSetting`、`ConnectionStringSetting`。
- 实现 `IConfigurationSetting`，通过构造函数注入 `IConfiguration` 读取配置；业务代码不直接读取配置文件或环境变量。
- 配置键使用分层路径并与 `appsettings*.json` 一致，例如 `Discord:Token`、`Destiny2:Cache:PlayerDataMinutes`、`ConnectionStrings:Mysql`。
- `VoidBotModule` 会自动扫描并以单例注册 Setting；业务类通过构造函数注入，不自行 `new` 或重复注册。
- 属性优先只读，在构造函数中完成默认值、规范化和派生值计算；必填配置缺失时应尽早失败或给出清晰错误。
- 密码、token、client secret、连接字符串不得写入日志、响应、异常消息或提交仓库。现有 `AppSetting` 静态值仅作兼容，新功能使用类型化 Setting。

## 6. 校验、响应与映射

- 输入格式校验使用 FluentValidation，Validator 放在 `Core/Validators/<Domain>`，继承 `FluentMessageValidator<T>`，由 Mediator 管道执行。
- Validator 负责必填、长度、范围和格式；Service 负责数据存在性、权限和业务状态。
- 使用统一响应管道和 `VoidBotResponse`，不要在每个 Controller 重新包装错误。
- 异常保留上下文并交给统一中间件/过滤器；禁止无理由吞异常。业务错误使用稳定、可本地化的消息键或既有错误约定。
- Entity 与 DTO 使用 AutoMapper 映射；禁止直接返回 EF Entity。

## 7. 本地化

### 7.1 服务职责

- `ILanguageService`：发现、加载、热更新 `Core/Resources/i18n` 下的 YAML，并生成默认 YAML。
- `ILocalizationService`：面向业务的本地化门面，根据 `ICurrentUser.Language` 选择语言，支持 `LanguageEnum`/i18n 语言码、参数化文案和多语言字典。
- 业务代码依赖 `ILocalizationService`，不直接依赖 `ILanguageService`。

### 7.2 使用约定

- 单条文案使用 `GetValue<T>(x => x.Property, ...)`。
- 多语言 API（如 Discord）使用 `GetValues<T>(...)`，当前返回 `zh-CN`、`zh-TW`、`en-US`。
- Endpoint Action 使用 `[DescriptionLocalization("稳定Key")]`；扫描器收集元数据，`EndpointService` 调用 `GetEndpointDescription(category, key, language)`，缺失时回退默认描述。
- 新增上下文：在 `Core/Resources/i18n/<System>` 创建 `IContextTemplate`，添加 `[Language("<System>", "<ContextTemplateName>", ...)]`；属性名即 YAML 键。
- 不在 Service、Controller、DTO 中直接写重复文案、语言码映射或 `string.Format`。

## 8. 外部依赖与运行规范

- HTTP、Discord、Bungie、邮件等 SDK 调用封装在 Service、DataProvider 或适配器中，不把供应商类型泄漏到 Message。
- 外部 HTTP 请求统一使用注入的 `IVoidBotHttpClientFactory`（实现为 `VoidBotHttpClientFactory`），禁止在业务代码中直接 `new HttpClient()` 或自行重复配置客户端。
- 一般 JSON 请求优先使用工厂提供的 `GetAsync<T>`/`PostAsync<T>`，其结果统一为 `VoidBotResponse<T>`；需要流、特殊请求或自定义处理时使用 `CreateClient(...)`，再通过现有 `HttpClientExtension` 处理响应。
- API token、Authorization、固定请求头和超时时间通过 `TokenModel`、`headers`、`timeout` 参数传入；这些配置应由对应的领域 DataProvider/适配器集中管理，不散落到调用方。
- 只有确实需要隔离生命周期时才设置 `beginScope: true`，并在代码中说明原因；不要让一次外部请求意外创建无界的生命周期范围。
- 所有 HTTP 调用必须传递 `CancellationToken`，检查 `VoidBotResponse.Code` 后再使用 `Data`；对失败、超时、取消和空响应保留清晰的错误处理。
- 日志使用 Serilog `ILogger`，记录领域上下文和关键标识，不记录敏感数据。
- 缓存通过 `ICacheManager`/领域 DataProvider 访问，缓存键使用 Message Constants，并处理缺失、过期和反序列化失败。
- 只有缓存数据结构、序列化格式或兼容性变化才提升缓存版本；单纯修改读写/业务逻辑不得提升版本，同一功能合入大杂烩分支前最多提升一次。
- 数据库结构变更使用 `Core/Dbup/Scripts` 中的 DbUp SQL，并同步更新 Entity/Configuration。
- I/O 使用 async API，方法名以 `Async` 结尾，传递 `CancellationToken`；禁止阻塞式 `.Result`/`.Wait()`。
- 遵循现有 C# 风格（文件范围 namespace、nullable、依赖注入、按领域分目录），避免无意义的小方法和格式改动。

## 9. 分支与 GitHub 协作流程

### 9.1 分支开发

1. 开始工作前检查工作区、当前分支和远程状态，保护已有未提交修改。
2. 从最新的 `master` 创建功能分支；分支名使用常见前缀和简短英文描述，例如 `feature/add-agent-guidelines`、`fix/discord-timeout`、`enhance/cache-read`。
3. 分支名不得使用 `codex/` 前缀，也不得以 `all-in-one` 或 `mixture` 开头；这些名称只用于大杂烩分支，不能作为功能分支来源或 PR 目标。
4. 一个分支聚焦一个功能或问题，避免混入无关重构、格式化或临时文件。
5. Commit message 使用简短、准确的英文描述；除非明确要求，不添加 `Co-authored-by`。
6. 推送前检查 diff 和测试状态，并使用 `git push -u origin <branch>` 建立远程跟踪。
7. 合并到大杂烩分支前，先切换到目标分支并拉取最新代码（例如 `git pull --ff-only origin all-in-one-202501`），确认工作区干净后再合并和推送。
8. 不直接把大杂烩分支作为 PR 源分支或目标分支；面向主线的 PR 应由功能分支指向 `master`。合并大杂烩仅用于集成验证，不能替代主线 PR。

### 9.2 Issue 规范

- 创建 Issue 前先检查 `.github/ISSUE_TEMPLATE/`，选择最匹配的模板（功能请求、错误报告或问题交流）。
- 必须保留模板的 front matter、标题格式、问题标题和顺序；完整填写模板问题，不用自定义正文替代模板结构。
- 功能请求至少说明使用场景和期望结果；错误报告补充版本、操作步骤、实际问题和期望结果；问题交流补充搜索记录、版本、预期/实际行为和复现步骤。
- Issue 标题格式为“emoji + 单语言标题”，中文或英文二选一，不要求也不应同时写成“中文 | English”。例如：`✨ 整理项目开发设计规范` 或 `✨ Add development guidelines`；功能/错误类别通过模板的 `labels` 表达，不必在标题中重复添加 `[Feature]`/`[Bug]`，除非模板明确要求。按模板设置 labels 和 assignees。

### 9.3 Pull Request 规范

- 创建 PR 前先读取 `.github/PULL_REQUEST_TEMPLATE/`（以及根目录 PR 模板文件，如存在），严格保留模板分隔线、Issue 关联区和提交检查清单。
- 用清晰的变更摘要替换模板占位文字；完整说明改动范围、验证方式和已知限制，不删除模板检查项。
- PR 必须由功能分支指向 `master`，标题必须使用简短、准确的英文表达；关联 Issue 使用模板要求的格式。
- 一个 PR 可以正常关联并关闭多个 Issue；在模板的“涉及 `ISSUES`”区域逐条列出，例如 `Close #123`、`Close #124`。只关联本次 PR 实际解决的问题，避免为了关联而添加无关 Issue。
- 按实际情况勾选模板检查项：没有新增测试时明确保持未勾选并在正文说明原因；不要为了通过清单虚假勾选。
- 创建前确认源分支、目标分支、变更 diff 和 Issue 编号正确；PR 创建后再次检查标题、正文、labels、reviewer 和关联关系。

## 10. 测试与交付清单

- Service 业务分支和边界条件优先写 Unit Test；数据库、Mediator 管道、认证和完整用例写 Integration Test。
- 测试优先通过公开 Message 契约调用，不直接测试 Controller 内部实现。
- 新功能完成前确认：Message 契约、Handler、Service、DataProvider、Validator、权限、本地化和测试均已接入。
- 确认普通 Command/Event 未手动保存，直接执行的 Hangfire Job 已明确保存边界，Request 无写入副作用。
- 除非用户明确要求，不主动编译项目、提交 Git commit、合并分支或创建 PR；一旦用户明确要求，仍必须遵循本节分支和模板流程。
