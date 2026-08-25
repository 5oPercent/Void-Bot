# Destiny 2 RaidHub 世界首通竞速数据接入备忘

## 状态

本方案仅作为后续实现备忘，当前不接入 RaidHub，也不修改突袭与地牢统计接口。

## 调研结论

- 未找到由 Bungie 官方或 Raid Report 长期维护、覆盖全部突袭且结构稳定的公开 Google Sheet。
- Raid Report 提供 World's First 排行榜，但明确声明与 Bungie 无关联，且没有发现适合服务端同步的公开数据导出约定。
- RaidHub 提供结构化 API，可按突袭获取完整 Contest 排行榜，也可按 Destiny Membership ID 查找玩家所在队伍。
- 如果未来需要展示 Day One 名次，优先考虑将 RaidHub 数据同步到本地数据库，不建议在玩家接口请求期间实时依赖第三方服务。

相关资料：

- [RaidHub API Introduction](https://raid-hub-api.mintlify.app/introduction)
- [Team Contest Mode Leaderboard](https://raid-hub-api.mintlify.app/api/leaderboard/team-contest)
- [Team First Clears Leaderboard](https://raid-hub-api.mintlify.app/api/leaderboard/team-first)
- [RaidHub Authorization](https://raid-hub-api.mintlify.app/api/authorize)
- [Raid Report](https://raid.report/)

## RaidHub Contest 排行榜

接口：

```http
GET https://api.raidhub.io/leaderboard/team/contest/{raid}
```

该接口返回指定突袭在官方 Contest 开放期间完成的队伍排名，不限于前 1000 名。

常用查询参数：

| 参数 | 说明 |
| --- | --- |
| `count` | 每页数量，范围为 10–100 |
| `page` | 页码，从 1 开始 |
| `search` | Destiny Membership ID；返回包含该玩家的队伍记录 |

主要响应字段：

| 字段 | 说明 |
| --- | --- |
| `position` | 从 1 开始的榜单位置 |
| `rank` | 考虑并列后的排名 |
| `value` | 从 Contest 开始到队伍完成的秒数 |
| `instanceId` | Bungie 活动实例 ID |
| `players` | 完成队伍成员，通常为 6 人 |
| `players[].membershipId` | Destiny Membership ID，检索时的主要关联键 |
| `players[].membershipType` | Bungie 平台类型 |
| `players[].displayName` | Bungie 显示名称，仅用于展示 |
| `players[].cheatLevel` | RaidHub 的异常或作弊等级数据 |

RaidHub 文档列出的 Contest 突袭标识包括：

- `deepstone`
- `vaultofglass`
- `vowofthedisciple`
- `kingsfail`
- `rootofnightmares`
- `crotasend`
- `salvationsedge`

实际可用标识应通过 RaidHub `/manifest` 获取，不应只依赖硬编码列表。

## 鉴权与依赖

- RaidHub 文档说明生产环境需要 API Key，请求示例使用 `X-API-KEY`。
- 本地开发请求可能不要求鉴权，但正式实现不能依赖该行为。
- API Key 必须保存在服务端配置或环境变量中，禁止提交到仓库或下发给前端。
- 实现前需要再次确认 RaidHub 的使用条款、速率限制、数据保留政策和 API Key 申请方式。
- RaidHub 是第三方数据源，其可用性、字段和排名可能发生变化，不能影响现有 Bungie 统计接口的基础功能。

## 推荐数据库设计

### 队伍排名表

```text
destiny2_day_one_team
- id                    char(36) / Guid
- raid_key              varchar(64)
- position              int
- rank                  int
- completion_seconds    bigint
- instance_id           varchar(64)
- source                varchar(32)
- source_minted_at      datetime
- imported_at           datetime
- raw_data              json，可选
```

建议索引：

```text
UNIQUE (raid_key, instance_id)
INDEX  (raid_key, position)
INDEX  (raid_key, rank)
```

`rank` 可能因并列而重复，因此不能作为单独唯一键。

### 队伍成员表

```text
destiny2_day_one_team_member
- id                    char(36) / Guid
- team_id               char(36) / Guid
- membership_type       int
- membership_id         varchar(64)
- display_name          varchar(128)
- cheat_level           int
```

建议索引：

```text
UNIQUE (team_id, membership_id)
INDEX  (membership_id)
```

查询玩家 Day One 数据时，应使用 `membership_id` 关联。显示名称可能修改或重复，不能作为稳定检索键。

## 推荐同步策略

1. 通过 RaidHub `/manifest` 获取支持的 Contest 突袭和标识。
2. 按突袭分页拉取完整 Contest 排行榜。
3. 将响应写入临时集合，完成字段校验和重复实例检查。
4. 在事务内按 `(raid_key, instance_id)` Upsert 队伍和成员。
5. 保存 RaidHub 响应的 `minted` 时间及本次导入时间，便于判断数据新旧。
6. 同步失败时保留上一份有效数据，不清空正式表。
7. Contest 进行期间可以周期同步；活动结束并且排名稳定后降低频率或停止同步。
8. 如果 RaidHub 后续修正作弊记录或排名，允许重新同步并更新已有记录。

历史 Contest 排名基本属于冷数据，导入数据库后由本地接口检索，比每次玩家请求实时调用 RaidHub 更稳定。

## 接口返回建议

Day One 是队伍级排名，建议在具体突袭明细中返回可空对象：

```json
{
  "dayOne": {
    "position": 123,
    "rank": 123,
    "completionSeconds": 45231,
    "instanceId": "12345678901234567",
    "team": [
      {
        "membershipType": 3,
        "membershipId": "4611686018488107374",
        "displayName": "Guardian#1234"
      }
    ]
  }
}
```

没有对应 Contest 排名时，`dayOne` 返回 `null`。不要用 `hasDayOneCompletion` 代替排名对象，否则会丢失队伍、名次和完成时间信息。

## 实现前待确认事项

- RaidHub API Key 的申请和生产使用授权。
- 是否展示 `position`、`rank`，或同时展示两者。
- `cheatLevel` 不同取值的含义及过滤策略。
- RaidHub 突袭标识与 Bungie 活动 Hash/英文原名的映射方式。
- 是否仅导入当前仍可查询的 Contest 排名，还是补齐所有历史 Day One 数据。
- 排名修正后的重新同步与缓存失效策略。
- 第三方服务不可用时，接口应继续返回基础突袭数据，并将 `dayOne` 置为 `null`。

## Google Sheet 备选方案

如果未来获得可信的公开 Google Sheet，可以将其作为一次性历史数据源，但至少需要以下字段：

- 突袭标识
- `position` 或 `rank`
- Bungie 活动 `instanceId`
- 每名成员的 `membershipId` 和 `membershipType`
- 完成时间

只有玩家名称而没有 Membership ID 或 Instance ID 的表格，不适合作为正式关联数据。导入时应保存来源 URL、工作表 `gid`、导入时间和文件校验值，并通过临时表校验后再写入正式表。
