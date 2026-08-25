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
            "playtimeSeconds": 72000,
            "fastestCompletionSeconds": 2450,
            "hasLowmanCompletion": true,
            "hasSoloCompletion": false
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
            "fastestCompletionSeconds": null,
            "hasLowmanCompletion": false,
            "hasSoloCompletion": false
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
| `fastestCompletionSeconds` | `number \| null` | 有效通关记录中的最短完整活动时长；没有有效记录时为 `null` |
| `hasLowmanCompletion` | `boolean` | 是否存在非单人的少人通关记录 |
| `hasSoloCompletion` | `boolean` | 是否存在单人通关记录 |

## 统计规则

### 分组与时长

- 明细以“活动原名 + 难度”分组，同一活动的普通、大师等难度不会合并。
- `activityHashes` 仅合并同一活动、同一难度下的 Bungie 版本哈希。
- 明细游玩时长优先采用玩家实际参与时长；汇总游玩时长采用 Bungie 历史统计接口的官方数值。
- 最短通关时长采用 `activityDurationSeconds`，避免玩家中途加入导致记录时长偏短。

### Lowman 与 Solo

标识根据该活动与难度下的全部有效通关记录计算，与最快记录是否满编无关。

| 标识 | 突袭 | 地牢 |
| --- | --- | --- |
| Lowman | 2–5 人 | 2 人 |
| Solo | 1 人 | 1 人 |

同一明细曾分别完成 Solo 和其他 Lowman 挑战时，两个标识可以同时为 `true`。

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
