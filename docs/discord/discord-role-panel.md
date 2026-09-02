# Discord 角色面板功能说明

角色面板允许管理用户在指定 Discord 服务器频道中发布一条带 Button 的消息。用户点击 Button 后，Bot 根据配置为用户添加或移除对应的服务器角色。

## 一、功能边界

- 每个 Guild 只能配置一个角色面板。
- 每个角色面板只能指定一个文字频道。
- 面板内容可自定义，支持 Discord 文本和 Markdown；后端不再拆分标题和正文，也不自动生成 Embed。
- 最多配置 25 个 Button；每个 Button 绑定一个服务器角色。
- Button 只配置文本，文本中可以直接包含 Unicode Emoji；`style` 支持 `Primary`、`Secondary`、`Success`、`Danger`。
- `mode` 支持 `Toggle` 和 `AddOnly`：前者按当前角色状态添加或移除角色，后者只添加角色。
- Button 的 Discord `custom_id` 由服务端生成，格式为 `rp:{panelId}:{itemId}`，前端不需要生成或修改。

## 二、请求处理与权限

管理接口要求当前 Void 用户已绑定 Discord，且 OAuth 授权包含 `identify guilds`。请求还会经过 Guild 中间件检查：Bot 已加入目标 Guild，并且当前 Discord 用户拥有该 Guild 的管理权限。

角色列表中的 `Assignable = false` 表示 Bot 无法管理该角色，通常是因为角色由 Discord 管理或角色层级高于 Bot。保存配置时会再次校验角色属于目标 Guild 且可分配。

发布、刷新和取消发布由后端调用 Discord API 完成。Button 交互事件通过 Component Router 路由到角色面板处理器，再使用 Discord 缓存中的 `SocketGuild`、角色和成员信息执行角色变更。

## 三、配置生命周期

1. 前端读取可管理 Guild、频道和角色。
2. 前端提交保存请求，后端创建或更新该 Guild 唯一的面板配置。
3. 发布请求在指定频道创建 Discord 消息，并保存 `MessageId`。
4. 刷新请求编辑已发布消息；如果尚未发布则返回业务错误。
5. 取消发布删除 Discord 消息，但保留数据库中的面板配置。
6. 删除配置移除数据库配置。删除配置前建议先调用取消发布，避免遗留 Discord 消息。

数据库以 `guild_id` 唯一约束保证“一 Guild 一个面板”，面板项目按 `sort` 排序保存。

## 四、运行要求

Bot 需要连接 Guild 相关 Gateway 事件，并至少具备以下 Discord 权限：

- View Channel
- Send Messages
- Embed Links
- Read Message History
- Manage Roles

Discord 角色层级规则仍然生效：Bot 只能操作低于自身最高角色的目标角色。

## 五、错误处理约定

接口使用项目统一的 `VoidBotResponse` 响应包装。常见业务错误包括：

- Guild 不可用或 Bot 尚未加入 Guild；
- 当前用户没有该 Guild 的管理权限；
- 频道不存在、不是可用文字频道，或不属于目标 Guild；
- 角色不存在、属于其他 Guild，或 Bot 无法分配；
- 面板项目为空、超过 25 个、按钮文本为空或角色重复；
- 刷新/取消发布时 Discord 消息已经不存在。

Discord API 的异常由服务层转换为项目统一的业务错误，不应把 Discord Access Token 暴露给前端。
