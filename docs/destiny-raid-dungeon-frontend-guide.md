# Destiny 2 突袭与地牢统计接口

## 接口

```http
GET /api/Destiny2/player/raid-dungeon/stats
```

查询参数：

| 参数 | 类型 | 说明 |
| --- | --- | --- |
| `displayName` | `string` | Bungie 显示名称，不包含 `#` |
| `displayNameCode` | `string` | Bungie 名称后的数字代码 |
| `membershipType` | `number` | Bungie 平台类型 |

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
            "playtimeSeconds": 72000
          },
          {
            "activityHashes": [1661734047],
            "name": "最后一愿",
            "difficulty": 2,
            "imageUrl": "https://www.bungie.net/img/destiny_content/pgcr/raid_beanstalk.jpg",
            "activityCount": 5,
            "completionCount": 3,
            "mentorCount": 0,
            "playtimeSeconds": 24000
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

## 字段说明

| 字段 | 说明 |
| --- | --- |
| `activityType` | Bungie 活动模式：`4` 为突袭，`82` 为地牢 |
| `name` | 根据用户语言返回的活动类型名称 |
| `activityCount` | 跨所有角色累计进入该活动类型的次数 |
| `completionCount` | 跨所有角色累计通关次数 |
| `mentorCount` | 游戏内 Carries 统计追踪器累计值；同一活动的不同阈值追踪器会去重 |
| `playtimeSeconds` | 跨所有角色累计游玩秒数 |
| `details` | 各个具体突袭或地牢的统计列表，普通、大师等难度分开展示 |

明细使用“活动原名 + 难度”作为分组条件，因此同一活动的普通和大师会返回两条相邻记录，次数、通关和时长各自累计，不会合并。`activityHashes` 只合并同一活动、同一难度下的 Bungie 版本哈希；`imageUrl` 是该活动独立的 PGCR 背景图。明细游玩时长优先采用玩家实际参与时长，汇总游玩时长保持 Bungie 历史统计接口的官方数值。

难度枚举：`1` Normal、`2` Master、`3` Expert、`4` Contest、`5` Prestige、`6` Challenge。导师次数仅在 Bungie 提供对应难度的独立 Carries Metric 时归入该难度，不会在普通和大师间重复。

接口结果缓存 5 分钟。前端如需展示小时数，应使用 `playtimeSeconds / 3600` 自行格式化，避免丢失原始精度。

## Manifest 依赖

服务端必须至少同步英文的 `Activity`、`ActivityMode` 和 `Metric` 组件；用户语言的活动清单不存在时，接口会回退到英文。首次增加这些组件时，即使 Bungie Manifest 版本未变化，也需要使用 `forceSync: true` 强制同步一次。
