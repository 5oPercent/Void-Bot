# Destiny 2 Manifest 同步缓存行为

## 当前行为

Manifest 同步不会在执行前清空已有的清单缓存。

同步任务只处理本次请求中 `components` 指定的清单组件：

1. 根据 `components` 筛选需要下载的组件。
2. 从 Bungie 下载对应语言的组件数据。
3. 下载成功后，通过相同的 Redis Key 覆盖该组件的旧缓存。
4. 未包含在 `components` 中的组件不会被读取、更新或删除。
5. 下载失败的组件也不会删除已有缓存。

## 示例

假设 Redis 中已经存在以下缓存：

```text
Destiny2:Manifest:Season:zh-chs
Destiny2:Manifest:InventoryItem:zh-chs
```

最新一次同步只指定：

```json
{
  "languages": ["zh-chs"],
  "components": ["InventoryItem"],
  "forceSync": true
}
```

同步完成后：

- `InventoryItem` 会在下载成功后更新为最新内容。
- `Season` 不在本次同步范围内，因此原缓存继续保留。
- 如果 `InventoryItem` 下载失败，其原缓存也会继续保留。

## 版本一致性风险

当前 Manifest 总版本号和各组件缓存并不是原子更新。

发现新版本或执行强制同步时，系统会先更新 Manifest 版本缓存，再把组件下载任务放入后台队列。因此可能出现以下情况：

- Manifest 版本号已经更新，但后台组件尚未下载完成。
- 本次没有选择的组件仍保留上一版本的数据。
- 下载失败的组件仍保留上一版本的数据。
- 不同组件在短时间内可能分别来自新旧 Manifest 版本。

因此，Manifest 版本缓存表示系统已经发现并开始处理该版本，不代表所有已有组件都已成功更新到该版本。

## 当前配置的同步范围

同步选项接口直接读取 `Destiny2Setting` 中的以下配置：

- `Destiny2:SyncLanguages`
- `Destiny2:SyncManifests`

当前默认使用的清单组件为：

```text
Season
InventoryItem
PlugSet
Stat
Trait
BreakerType
SandboxPerk
```

如果从 `SyncManifests` 中移除某个组件，后续定时同步将不再更新该组件，但已经存在的缓存仍会保留。

## 相关代码

- `src/Void.Bot.Core/Services/Sync/SyncService.Destiny2.cs`
- `src/Void.Bot.Core/Settings/Bungie/Destiny2Setting.cs`
- `src/Void.Bot.Core/Jobs/RecurringJobs/SynchronizeDestiny2ManifestJob.cs`
- `src/Void.Bot.Message/Constants/Bungie/Destiny2/Destiny2ManifestConstants.cs`
