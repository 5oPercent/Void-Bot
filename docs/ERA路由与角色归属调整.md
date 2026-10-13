# ERA 路由与角色归属调整

确认方案：Manifest、玩家、角色、能力体系、技能、队伍、社交、客户端会话、管理九类 Controller。管理员统一前缀 /api/era/admin；能力体系、配装和学习记录归角色。没有普通玩家 BUFF Controller。

## 客户端迁移

| 原路径 | 新路径 | 方法 |
| --- | --- | --- |
| /api/era/config | /api/era/manifest | GET |
| /api/era/skills/config | /api/era/manifest/catalog | GET |
| /api/era/me | /api/era/players/me | GET |
| /api/era/me/initialize | /api/era/players/me/initialize | POST |
| /api/era/me/characters | /api/era/characters | GET／POST |
| /api/era/me/characters/{characterId} | /api/era/characters/{characterId} | GET |
| /api/era/me/characters/{characterId}/delete | /api/era/characters/{characterId}/delete | POST |
| /api/era/me/skill-loadout | /api/era/characters/{characterId}/ability-systems/loadouts | GET／POST |
| /api/era/me/learned-skills | /api/era/characters/{characterId}/skills/learned | GET |
| /api/era/admin/players/{playerId}/learned-skills | /api/era/admin/characters/{characterId}/learned-skills | POST |

队伍、社交、会话路径不变；登录后的游戏会话获取入口随玩家初始化迁到 /api/era/players/me/initialize。

## 管理端迁移

旧 /api/era/admin/skills/draft/systems/* 迁到 /api/era/admin/ability-systems/*；skills/* 迁到 /api/era/admin/skills/*；buffs/* 迁到 /api/era/admin/buffs/*。共同草稿状态、initialize、settings、references、export、import、validate、publish-preview、publish、history 统一迁到 /api/era/admin/configuration 下。请求参数及发布流程不变。

基础配置管理、玩家查询和仓库容量管理路径不变，移至 EraAdminController。端点分类和路径已变，需在管理端重新扫描及配置授权。旧路由和旧 Controller 不保留。

## 数据与校验

新增 Script0028，将两张技能数据表、字段、唯一索引和外键改为角色归属。已有脚本保持不变。用户确认暂无旧配装／学习数据；脚本直接调整空表结构，不包含数据检测或归属分配。应用脚本前须先部署确认；本次不执行迁移。

读写必须传 CharacterId，普通操作检查角色属于当前账号。写操作保持玩家行→角色行的锁顺序，角色行与唯一索引保护同角色配装版本及授予幂等。不同角色配装、版本与学习独立；清理无效配装只在客户端同步时执行。角色禁用保留记录，未新增禁用角色的特殊技能规则。

角色选择改变时丢弃过时读取／保存响应，菜单不能用先前角色的配装保存到新角色。离线示例不访问官方角色存储。完整目录由 Manifest 一次返回，同一发布版本，不拆成多次版本不一致的读取。

## 静态验收

- [x] 九类 Controller、唯一排序为普通接口 400～407、管理接口 499 与路由无重复。
- [x] 无旧 API 或玩家级技能存储调用；完整客户端迁移。
- [x] 角色外键、唯一索引、权限和锁顺序核对。
- [x] 更新接口文档及新增角色隔离与越权回归测试源码。
- [ ] 编译、测试、迁移和实际联调未执行。

静态记录：后端 1251 个 C# 文件、客户端 scripts 下 232 个 C# 文件语法诊断为零；70 个 ERA 路由数量与迁移前一致，仅 GET／POST，管理员接口没有 SkipPermissionCheck。旧路径仅在本迁移对照表中记录，不保留调用或兼容入口。
