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
        "iconUrl": "https://www.bungie.net/common/destiny2_content/icons/example.png",
        "activityCount": 120,
        "completionCount": 45,
        "mentorCount": 8,
        "playtimeSeconds": 360000
      },
      {
        "activityType": 82,
        "name": "地牢",
        "iconUrl": "https://www.bungie.net/common/destiny2_content/icons/example.png",
        "activityCount": 80,
        "completionCount": 32,
        "mentorCount": 3,
        "playtimeSeconds": 180000
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
| `iconUrl` | 可直接用于前端展示的 Bungie CDN 完整地址 |
| `activityCount` | 跨所有角色累计进入该活动类型的次数 |
| `completionCount` | 跨所有角色累计通关次数 |
| `mentorCount` | 游戏内 Carries 统计追踪器累计值；同一活动的不同阈值追踪器会去重 |
| `playtimeSeconds` | 跨所有角色累计游玩秒数 |

接口结果缓存 5 分钟。前端如需展示小时数，应使用 `playtimeSeconds / 3600` 自行格式化，避免丢失原始精度。
