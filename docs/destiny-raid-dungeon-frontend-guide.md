# Destiny 2 突袭与地牢统计接口

## 接口

```http
POST /api/Destiny2/player/raid-dungeon/stats
Content-Type: application/json
```

请求体：

| 参数 | 类型 | 说明 |
| --- | --- | --- |
| `displayName` | `string` | Bungie 显示名称，不包含 `#` |
| `displayNameCode` | `string` | Bungie 名称后的数字代码 |
| `membershipType` | `number` | Bungie 平台类型 |

```json
{
  "displayName": "Guardian",
  "displayNameCode": "1234",
  "membershipType": 3
}
```

## 响应示例

```json
{
  "code": 200,
  "message": "Success",
  "data": {
    "activities": [
      {
        "activityType": 4,
        "name": "突袭",
        "activityCount": 120,
        "completionCount": 45,
        "mentorCount": 8,
        "playtimeSeconds": 360000,
        "details": [
          {
            "activityHashes": [2122313384, 1661734046],
            "name": "最后一愿",
            "difficulty": 1,
            "imageUrl": "https://www.bungie.net/img/destiny_content/pgcr/raid_beanstalk.jpg",
            "activityCount": 20,
            "completionCount": 10,
            "mentorCount": 2,
            "playtimeSeconds": 72000,
            "fullCompletionCount": 7,
            "fastestCompletionSeconds": 2450,
            "averageFullCompletionSeconds": 4102.57,
            "hasFullTrioCompletion": true,
            "hasFullDuoCompletion": false,
            "hasFullSoloCompletion": false,
            "hasFlawlessCompletion": true
          },
          {
            "activityHashes": [1661734047],
            "name": "最后一愿",
            "difficulty": 2,
            "imageUrl": "https://www.bungie.net/img/destiny_content/pgcr/raid_beanstalk.jpg",
            "activityCount": 5,
            "completionCount": 3,
            "mentorCount": 0,
            "playtimeSeconds": 24000,
            "fullCompletionCount": 0,
            "fastestCompletionSeconds": null,
            "averageFullCompletionSeconds": null,
            "hasFullTrioCompletion": false,
            "hasFullDuoCompletion": false,
            "hasFullSoloCompletion": false,
            "hasFlawlessCompletion": false
          }
        ]
      },
      {
        "activityType": 82,
        "name": "地牢",
        "activityCount": 80,
        "completionCount": 32,
        "mentorCount": 3,
        "playtimeSeconds": 180000,
        "details": []
      }
    ]
  }
}
```

## 响应字段

### 活动类型汇总

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `activityType` | `number` | Bungie 活动模式：`4` 为突袭，`82` 为地牢 |
| `name` | `string` | 根据用户语言返回的活动类型名称 |
| `activityCount` | `number` | 跨所有角色累计进入次数 |
| `completionCount` | `number` | 跨所有角色累计通关次数 |
| `mentorCount` | `number` | 游戏内 Carries 统计追踪器累计值 |
| `playtimeSeconds` | `number` | 跨所有角色累计游玩秒数 |
| `details` | `array` | 具体突袭或地牢的统计，按活动和难度分组 |

### 活动明细

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `activityHashes` | `number[]` | 同一活动、同一难度对应的 Bungie 活动哈希 |
| `name` | `string` | 根据用户语言返回的活动名称 |
| `difficulty` | `number` | 活动难度，取值见下方难度枚举 |
| `imageUrl` | `string` | 该活动的 PGCR 背景图地址 |
| `activityCount` | `number` | 累计进入次数 |
| `completionCount` | `number` | 累计通关次数 |
| `mentorCount` | `number` | 该活动与难度对应的 Carries 次数 |
| `playtimeSeconds` | `number` | 累计游玩秒数 |
| `fullCompletionCount` | `number` | 玩家从活动开始参与至通关的次数 |
| `fastestCompletionSeconds` | `number \| null` | 最短完整活动时长；优先使用 PGCR 确认记录，没有确认记录时回退到推断全程记录 |
| `averageFullCompletionSeconds` | `number \| null` | 平均完整活动时长；优先使用 PGCR 确认记录，没有确认记录时回退到推断全程记录 |
| `hasFullTrioCompletion` | `boolean` | 是否存在三人全程通关记录 |
| `hasFullDuoCompletion` | `boolean` | 是否存在双人全程通关记录 |
| `hasFullSoloCompletion` | `boolean` | 是否存在单人全程通关记录 |
| `hasFlawlessCompletion` | `boolean` | 是否存在死亡次数为 0 的全程无暇通关记录 |

## 统计规则

### 分组与时长

- 明细以“活动原名 + 难度”分组，同一活动的普通、大师等难度不会合并。
- `activityHashes` 仅合并同一活动、同一难度下的 Bungie 版本哈希。
- 明细按照活动发布时间从新到旧排列，同一活动的不同难度保持相邻；缺少发布时间的活动排在最后。
- 明细游玩时长优先采用玩家实际参与时长；汇总游玩时长采用 Bungie 历史统计接口的官方数值。
- 全程通关首先要求记录已通关，服务端会通过 `instanceId` 查询 PGCR，并按角色 ID 找到对应的玩家条目。
- `activityWasStartedFromBeginning == false` 的检查点开局不会计入全程；凌光之刻之前的旧活动使用 `startingPhaseIndex == 0` 判断从头开局。
- 玩家条目的 `startSeconds <= 30` 视为从活动开始参与，以容纳正常加载产生的数秒偏差；超过该值视为中途加入。
- PGCR 或玩家开始时间不可用时，回退使用活动历史的 `startSeconds`，最终再使用 `timePlayedSeconds >= activityDurationSeconds` 推算，避免历史统计因字段缺失而丢失。
- 全程次数可以包含 PGCR 不可用时推算的记录。最短和平均时间优先只使用 PGCR 明确确认从头开始的记录；若该活动完全没有确认记录，则回退使用推断全程记录，避免历史活动的时间字段全部为空。
- 最短和平均全程通关时长均采用 `activityDurationSeconds`。

### PGCR 存储与缓存

- PGCR 只保存全程判定需要的活动级和玩家级摘要，不保存完整响应、武器或击杀明细。
- `destiny2_pgcr` 保存活动实例、开始时间、活动哈希、开始阶段、是否从头开始及数据版本；`destiny2_pgcr_player` 保存该实例中各角色的账号标识、通关状态和加入时间。
- 查询顺序为临时缓存、数据库、Bungie PGCR；数据库中不存在的成功响应会写入数据库。
- 该接口使用 `POST` 接收 Command。PGCR 仓储变更在 Command 执行期间登记，并由 UnitOfWork 管道在 Handler 成功结束后统一调用 `SaveChangesAsync` 和提交事务。
- 成功摘要在临时缓存中保留 15 分钟；请求失败使用 5 分钟负缓存，避免短时间内重复请求 Bungie。
- 活动摘要保存 `data_version`。缓存或数据库版本低于当前代码版本时不会直接使用，而是重新请求 Bungie PGCR，并通过 Upsert 更新活动和玩家摘要。
- 新增需要历史回填的 PGCR 字段或修改解析规则时，应提升 `CurrentPgcrDataVersion`；旧记录会在再次访问时自动回填，`updated_at` 记录更新时间。
- Redis 缓存版本与 `CurrentPgcrDataVersion` 相互独立。只有缓存数据结构、字段含义或序列化格式与旧缓存不兼容时才提升缓存版本；同一功能合并到大杂烩分支前，多轮结构调整只提升一次。
- 数据库读取或仓储登记失败会记录日志，并允许当前请求使用即时摘要继续计算；UnitOfWork 最终保存或事务提交失败时会回滚事务并使请求失败，避免返回成功但数据未落库。

### Trio、Duo、Solo 与 Flawless

标识仅根据该活动与难度下的全程通关记录计算，与最快记录无关。

| 标识 | 判定条件 |
| --- | --- |
| Trio | `playerCount == 3` |
| Duo | `playerCount == 2` |
| Solo | `playerCount == 1` |
| Flawless | `deaths == 0`，缺少死亡数据时不视为无暇 |

Trio、Duo、Solo 仅提供全程通关标识。Flawless 本身即表示全程无暇，因此不额外提供 Full 字段。同一条记录可以同时满足人数状态和 Flawless 状态。

### 难度枚举

| 值 | 名称 |
| --- | --- |
| `1` | Normal |
| `2` | Master |
| `3` | Expert |
| `4` | Contest |
| `5` | Prestige |
| `6` | Challenge |

导师次数仅在 Bungie 提供对应难度的独立 Carries Metric 时归入该难度，不会在普通和大师之间重复。同一活动的不同阈值追踪器会去重。

## 缓存与前端展示

接口结果缓存 5 分钟。前端如需展示小时数，应使用 `playtimeSeconds / 3600` 自行格式化，避免丢失原始精度。

## Manifest 依赖

服务端必须至少同步英文的 `Activity`、`ActivityMode` 和 `Metric` 组件；用户语言的活动清单不存在时，接口会回退到英文。首次增加这些组件时，即使 Bungie Manifest 版本未变化，也需要使用 `forceSync: true` 强制同步一次。
