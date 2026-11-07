# ERA 管理员模块化活动授权

- `POST /api/era/worlds/{worldInstanceId}/modular-activity/approve`：请求体 `memberId`、`authorityEpoch`、`selection`。服务端校验调用人的有效 SuperAdmin 角色、当前账号玩家、活跃世界成员和实时队伍代次。返回 20 秒内有效的随机授权编号，配置对象最多 64 KiB。
- `POST /api/era/worlds/{worldInstanceId}/modular-activity/claim`：请求体 `memberId`、`authorityEpoch`、`initiatorMemberId`、`approvalId`。只有当前世界的权威房主可以领取；房主无需 SuperAdmin。服务端再次验证发起成员和其有效角色，成功删除共享 Redis 中的授权键后返回原配置，重放不能再次领取。

授权和队伍、世界、代次、发起成员、玩家及角色绑定。不会修改队伍或世界状态，不需要数据库迁移。客户端只通过联机消息向房主传递授权编号；房主取得配置后检查本机打包目录，仍由原活动准备与世界换场流程执行。
