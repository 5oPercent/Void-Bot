# Void Bot 文档索引

文档按业务模块归档。接口路径以当前 API 路由为准；同一模块的设计说明、后端约定和前端对接文档放在同一目录中。

## 账号与身份

账号注册、邮箱绑定、密码和名称管理，以及当前登录身份查询。

- [账号邮箱](account/account-email.md)
- [账号管理 API](account/account-management-api.md)
- [账号管理前端对接](account/account-management-frontend-integration.md)
- [当前身份 `/api/Identity/me` 前端对接](identity/identity-me-frontend-integration.md)
- [双因素认证前端对接](identity/two-factor-frontend-integration.md)

## 好友与 ERA 社交通知

- [好友、组队权限与 SignalR 对接](social/social-backend.md)

## OAuth

第三方 OAuth 登录、绑定、注册、兑换和解绑流程。完整流程建议先阅读总览，再阅读未绑定身份流程。

- [OAuth 前端对接总览](oauth/oauth-frontend-integration.md)
- [OAuth 未绑定身份注册与登录绑定](oauth/oauth-registration-frontend-integration.md)

## Discord

Discord Guild 管理、语音频道配置和角色面板。

- [Discord 管理后台前端对接](discord/discord-management-frontend-integration.md)
- [Discord 角色面板功能说明](discord/discord-role-panel.md)
- [Discord 角色面板前端对接](discord/discord-role-panel-frontend-integration.md)

## 邮件服务

- [邮件服务配置与管理流程](email/email-service.md)

## Destiny 2

Destiny 2 清单同步、武器查询和突袭/地牢统计相关文档。

- [Manifest 同步缓存行为](destiny2/destiny2-manifest-sync-cache-behavior.md)
- [武器查找前端对接](destiny2/destiny-weapon-frontend-guide.md)
- [武器接口变更汇总](destiny2/destiny-weapon-api-changes.md)
- [武器框架、特性与 God Roll 接口设计](destiny2/destiny-weapon-godroll-api.md)
- [突袭与地牢统计接口](destiny2/destiny-raid-dungeon-frontend-guide.md)
- [RaidHub 世界首通数据接入备忘](destiny2/destiny-raidhub-integration-notes.md)

## 事件动作

- [内置事件动作配置](event-actions/event-actions.md)

## 文档维护约定

- 新增文档先归入对应模块目录，不直接放在 `docs/` 根目录。
- 同一功能的接口设计、前端对接和变更说明应互相链接，并尽量集中在同一模块。
- `docs/` 根目录仅保留本索引文件。

## ERA 技能编辑与发布

- [技能配置与配装](ERA技能系统.md)
- [草稿编辑与发布接口](ERA技能草稿管理接口.md)
- [实施 TODO](ERA技能编辑与发布TODO.md)
