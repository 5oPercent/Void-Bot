# ERA 队伍与打洞协调

## 当前范围

账号认证 HTTP API、短期凭证 UDP 登记和官方主机选举已实现。Godot 主菜单和地图内队伍 UI 已接入正式协议 2；官方不转发游戏数据，不处理结算或持久化突袭进度。

队伍直接存入后端共享 Redis，不写玩家数据库，不使用内存回退。客户端只访问 HTTP API 和 UDP 协调地址，不能连接 Redis。

后端实例使用相同 ConnectionStrings:Redis 和 Cache:RedisDatabaseIndex，队伍、成员凭证及端点共享。Cache:UseStorage 可以仍为 Memory，ERA 队伍固定使用 Redis。关闭 ERA 时不会建立此 Redis 连接；启用时必须配置 Redis。

后端重启不会主动删除队伍，但队伍仍遵守心跳有效期，重启超过全部成员心跳窗口后队伍自然失效。Redis 自身重启能否保留数据取决于其 AOF/RDB 持久化配置。

每队伍独立 JSON Key，玩家所属队伍独立 Key，队伍有效期使用有序集合索引。原子 Lua 比较 StorageVersion，同时提交队伍、玩家所属关系及队伍有效期索引；并发冲突重新读取并有限重试，不采用租约锁。关闭或退出只清除仍指向当前队伍的玩家索引，避免误删新队伍归属。所有相关 Key 使用相同 Redis Cluster hash tag，Cluster 数据库索引必须为 0。需要 Redis 6.2 或更新版本（PXAT）。

队伍 Key TTL 取队伍有效期与最晚成员有效期的较早值，玩家归属 TTL 取成员和队伍的较早值；查询与 UDP 登记不会续期成员。队伍有效期索引使用 Redis TIME 清理过期项，多个实例均可执行清理。后端节点应保持系统时间同步；业务时间戳使用 UTC，Redis 提交根据服务器时间防止过期状态复活。

HTTP 可负载均衡到多个实例，所有实例都能查询同一队伍。UDP 对外地址必须固定且指向能够访问同一 Redis 的登记实例或 UDP 负载均衡，不能将 HTTP 代理视为 UDP 代理。每玩家只处于一个队伍的限制由 Redis 原子操作约束，服务不限制总队伍数量，HTTP/UDP 报文限流目前仍按实例计数，网关负责总体限流。

## 配置

appsettings.json 已包含 Era:Networking 默认配置，默认关闭。启用时将 Enabled 设为 true，并调整 PublicHost。Development/Docker 文件可覆盖这些配置，未覆盖时继承基础配置。例如：

```json
{
  "Era": {
    "Networking": {
      "Enabled": true,
      "BindAddress": "0.0.0.0",
      "PublicHost": "127.0.0.1",
      "UdpPort": 3398,
      "MaxMembers": 4,
      "TeamLifetimeSeconds": 120,
      "MemberLifetimeSeconds": 30,
      "TicketLifetimeSeconds": 60,
      "EndpointLifetimeSeconds": 15,
      "PacketsPerSecond": 1000
    }
  }
}
```

PublicHost 填客户端可访问的 DNS 域名或 IPv4（例如 punch.example.com，不带协议或端口），本机可填 127.0.0.1。HTTP 复用主账号认证和现有 Security、Mediator 管道；先初始化 ERA 玩家并创建角色。部署使用 HTTPS，并开放 UDP 3398；容器需单独映射 UDP 端口，HTTP 反向代理不会自动代理 UDP。端口冲突会阻止启动，先停止占用该端口的 Python 协调器。

## HTTP 契约

所有 API 均为 Request/Command + Response，沿用 EraResponse 和 ErrorCode，GET 也提供 Request。带 TeamId 的 POST 从路由覆盖消息 TeamId，正文仍需 JSON 对象，无额外参数时为 {}。

| 方法 | 路径 | 参数 | 结果 |
| --- | --- | --- | --- |
| POST | /api/era/teams | RequestId、CharacterId | 队伍及成员 |
| POST | /api/era/teams/join | TeamCode、CharacterId | 队伍及成员 |
| GET | /api/era/teams/me | GetCurrentEraTeamRequest（空） | 本人当前队伍，无队伍时 Data 为 null |
| GET | /api/era/teams/{teamId} | GetEraTeamRequest | 本人所在队伍 |
| POST | /api/era/teams/{teamId}/heartbeat | MemberId | 续期后队伍 |
| POST | /api/era/teams/{teamId}/leave | MemberId | bool |
| POST | /api/era/teams/{teamId}/punch | MemberId | 本人短期凭证 |
| GET | /api/era/teams/{teamId}/peers | GetEraPunchPeersRequest | 本人协调状态及已登记有效对端 |

新队伍容量固定使用后端 Era:Networking:MaxMembers 配置，客户端不提交人数容量；响应中的 MaxMembers 表示实际队伍容量。现有队伍保留创建时的容量，任务/游玩模式人数限制由对应开始入口单独判断。每玩家只能加入一个有效队伍，角色必须属于本人且未禁用。原队伍有效时，创建使用相同 RequestId、CharacterId 和加入/邀请设置返回原队伍；同成员同角色重复加入返回原结果。过期后的创建是新会话，无持久创建回执。

成员建议每 5 秒 HTTP 心跳，默认 30 秒无心跳过期；任一有效成员的心跳续期队伍。角色在入房后禁用，会拒绝后续心跳及凭证申请。主机退出或超时自动选举下一位已同步候补；没有有效成员或全部候补接管失败时解散。查询只允许有效成员，不能列出他人队伍。不存在关闭整个队伍的接口。

队伍失效不会自动切断已建立的玩家直连，客户端会话层需要决定关闭游戏连接。

## UDP 协议和客户端流程

1. 主机创建，把 TeamId 交给受邀客户端，客户端加入。
2. 每成员调用 punch 获得 TeamId、MemberId、AttemptId、Token、ExpiresAt、UdpHost、UdpPort。
3. 通过用于 ENet 的同一个 Socket，每秒向 UdpHost:UdpPort 发送登记。
4. 每秒查询 peers。客户端连接 IsHost 为 true 的主机；主机探测获准成员，采用星形连接。
5. 在同一个 ENet Socket 握手。正式客户端按官方登记的地址和端口绑定 MemberId，并校验成员列表、TeamId 与 AuthorityEpoch；游戏帧使用手动可靠有序 ENet 通道，不挂载 Godot MultiplayerPeer。UDP 凭证仅用于向官方登记，不交给其他玩家。
6. 凭证过期或新尝试时重新调用 punch。每次调用旋转 AttemptId 和 Token，并清空本人旧端点；丢失响应需重新申请，不假定旧凭证有效。

登记是 UTF-8 前缀 ERA-PUNCH/2 加一个空格，再拼 JSON：

```json
{
  "teamId": "队伍 GUID",
  "memberId": "本人成员 GUID",
  "attemptId": "本次尝试 GUID",
  "token": "本人短期 Token"
}
```

Token 只用于本人成员 UDP 登记，服务器保存其 SHA256 并固定时间比较，不保存明文。端点取自 UDP 来源，不接受报文声明地址。无 UDP 回包，由 peers 观察登记。离开、关闭、过期或凭证轮换使登记失效。成员心跳和 UDP 登记独立，已建连接不依赖登记长期保活。

peers 的 State 固定值：0 未申请、1 等待本人 UDP 登记、2 等待对端、3 可开始打洞、4 本人凭证过期。该状态不代表 ENet 握手成功或游戏身份通过。

协议 2 与 Python 开发协议 1 不兼容，当前验证 UI 仍连接 Python，不能直接更改地址测试正式服务。

## 限制及待验证

HTTP 队伍 API 全局固定窗口 120 请求/秒、无排队，超限返回 HTTP 429。UDP 每秒总量可配置，每来源 IPv4 每秒最多 30 个有效格式报文，最多记 256 个来源，每秒重置，报文不超过 2048 字节。生产网关还需按账号/IP 限流。

Redis 故障时 API 返回 503 和 era.networking_unavailable，不回退本机内存；UDP 登记暂时忽略、清理延后，恢复后继续处理。已提交写入仍可能遇到连接中断导致响应不确定，应通过现有幂等规则或重新查询恢复。

只做静态检查，未编译、启动 Redis 或运行测试。需验证两账号建房加入、越权访问、禁用角色、容量、心跳过期、主机退出、旧凭证、错误 UDP、端点过期、后端重启、跨后端实例查询与并发加入、Redis 故障及恢复、Windows/Linux 及公网 NAT。现有本机成功仅针对 Python 协调器。

Redis 原子脚本语义参考 [官方 Lua 文档](https://redis.io/docs/latest/develop/programmability/eval-intro/)，绝对 TTL 参数参考 [SET 命令](https://redis.io/docs/latest/commands/set/)。


## 标准队伍流程检查

| 流程 | 后端行为 |
| --- | --- |
| 创建队伍 | 校验本人有效角色，原子限制一玩家一队伍，创建者即主机 |
| 加入队伍 | 有效队伍且未满；重复同角色加入返回原 MemberId，跨队伍占用拒绝 |
| 普通成员离开 | 删除该 MemberId，撤销凭证与端点，原子释放玩家归属 |
| 离开后重新加入 | 队伍仍有效且有空位时允许，生成新的 MemberId，旧凭证和退出请求不能影响新成员 |
| 异常断开 | 默认 30 秒没有 HTTP 心跳后逻辑移除，查询及容量立即排除，玩家归属 TTL 释放 |
| 超时后重新加入 | 普通成员可重新加入仍有效队伍，生成新 MemberId 和凭证；不能续期已经过期的旧会话 |
| 主机离开或断开超时 | 自动交接给已同步候补，逐一尝试；无人或全部失败则解散 |
| 后台清理 | 每 5 秒分批扫描，重新读取最新版本后原子清除过期成员；清理不续期存活成员 |
| 重复退出或关闭 | 已不存在的成员/队伍返回成功；若退出请求对应旧 MemberId 而本人已重新加入，返回 era.team_membership_changed |

客户端丢失 TeamId 或创建/加入响应时，可通过 GET /api/era/teams/me 恢复当前队伍和成员 ID；该查询不会续期队伍或成员。没有有效队伍时返回成功且 Data 为 null。

心跳、退出、申请凭证必须携带创建或加入返回的 MemberId，重新加入后替换它。例如：

```json
{
  "memberId": "本次创建或加入返回的成员 GUID"
}
```

后台不能直接感知玩家之间的 ENet 断线。ENet 主机连接丢失时客户端暂停模拟并继续官方心跳，等待重连或官方权威选举。官方心跳失败时继续重试，超过响应中的成员时限仍未确认则关闭游戏连接并返回主界面。运行行为尚待实际联机验证。

以上为静态业务审查及代码修正结果，仍需用真实 Redis 验证并发加入、过期清理、离开重入及旧 MemberId 请求。

## Docker 部署

Compose 已映射 HTTP `3399:3399` 和 UDP `3398:3398/udp`。Dockerfile 使用 appsettings.Docker.json 替换基础配置，因此 Docker 配置中也包含完整的 Era:Networking 默认值。默认仍关闭打洞服务。

启用时直接修改 `src/Void.Bot.Api/appsettings.Docker.json` 中的 `Era:Networking`：将 `Enabled` 设置为 true，`PublicHost` 设置为客户端可访问的服务器域名或 IPv4，保持 `BindAddress` 为 0.0.0.0、`UdpPort` 为 3398。

修改配置后重新构建并创建 app 容器。客户端 HTTP 地址填实际服务器域名或 IPv4；UDP 地址由接口返回 PublicHost 与端口 3398。服务器防火墙及云安全组需要允许 UDP 3398，HTTP 入口按实际部署开放。HTTP 反向代理不会自动转发 UDP。Redis 由后端访问，客户端不连接 Redis。

PublicHost 为 127.0.0.1 只适用于服务器本机测试，跨机器部署需要修改为可访问的域名或 IPv4。当前 Compose 固定内外 UDP 3398。
PublicHost 支持纯域名，例如 `punch.example.com`。后端原样返回到打洞票据的 UdpHost，不在启动时固定解析 IP；客户端建立连接时应解析域名的 IPv4 A 记录。更换服务器时更新 DNS 即可，不需要修改 PublicHost，切换生效受 DNS 缓存和 TTL 影响，已有连接不会自动迁移。BindAddress 仍为监听 IPv4，通常保持 0.0.0.0。正式客户端协议 2 在连接及票据刷新时解析域名 IPv4 A 记录；独立的 Python 协议 1 验证页面仍要求 IPv4。

## 短队伍码

创建队伍时后端自动生成八位队伍码 TeamCode，例如 K7M3RX9A。使用加密随机数，字符不包含 0、1、I、O。活动队伍码由 Redis Lua 原子占用，碰撞时重试生成；码索引 TTL 与队伍实际 TTL 一致，任一有效成员的心跳续期，队伍解散或全员超时释放。查询和心跳返回的队伍 DTO 同样包含 TeamCode。创建幂等重试返回原有队伍及原队伍码。

加入接口改为 `POST /api/era/teams/join`，请求：

```json
{
  "teamCode": "K7M3RX9A",
  "characterId": "角色GUID"
}
```

队伍码支持大小写；服务端规范化为大写后解析并检查队伍有效性。用户只需分享和输入 TeamCode。返回的 TeamId、MemberId 由客户端内部用于心跳、退出和打洞，不需要玩家输入。队伍码不是密码，持有有效账号和角色的玩家知道队伍码即可加入。

Redis 会话结构升级至 v2；更新服务前应结束旧版本活动队伍，升级后客户端重新创建或加入，不混用新旧服务实例。旧 v1 Key 按原 TTL 自然过期。现有 Python 协议 1 验证 UI 尚未接入此正式加入接口。

## 自动主机交接

队伍 DTO 新增 AuthorityEpoch（初始 1）、IsMigrating、MigrationDeadline、HeartbeatIntervalSeconds（5）和 MemberLifetimeSeconds（默认 30）。成员包含 JoinedAt 与 CanHost。队伍实际 TTL 取队伍期限与最晚成员期限的较早值，不再由主机一个人的期限决定。

主机 leave 或成员超时清理触发选举。按 JoinedAt、MemberId 顺序选择已报告 CanHost 的有效候补，每次候选递增 AuthorityEpoch 并清空全部旧打洞票据和端点。候补在 MigrationTimeoutSeconds（默认 15，5～120）内报告接管结果；成功恢复正常状态，失败或超时尝试下一位。无候补、全员离开或全部候补失败时原子删除队伍、码索引与成员归属。失败成员可在后来新权威下重新完成同步并报告资格。

新增接口（均为 Command + Response）：

| POST 路径 | 请求字段 | 返回 |
| --- | --- | --- |
| /api/era/teams/{teamId}/host-readiness | MemberId、AuthorityEpoch、CanHost | 队伍 |
| /api/era/teams/{teamId}/migration-complete | MemberId、AuthorityEpoch、Success | 队伍；全体失败解散时 Data=null |
| /api/era/teams/{teamId}/switch | MemberId、CharacterId、TeamCode | 目标队伍 |

switch 的旧队伍退出、旧主机交接及目标队伍加入使用同一个 Lua 原子脚本；目标满员、无效或迁移中时保留旧队伍。切换成功后响应丢失可重试恢复目标成员。punch 请求新增 AuthorityEpoch，旧权威申请与旧登记凭证均不可继续使用。不存在 close 接口。

接管资格是可信游戏客户端在完整同步后报告的能力状态，不是后端对玩家地图快照内容的校验。官方服务只选举权威，状态快照仍由玩家之间传递。

## 客户端主流程与验证

登录并选择角色后进入多人地图，自动创建或恢复自己的队伍。地图内按 F2 开启队伍 UI，复制八位码邀请、按码加入、离队进入自己的地图或退出多人游戏。主机主动退出自动发送最后快照并最多等待两秒确认，然后调用 leave；不会出现额外交接申请步骤。异常退出由成员 30 秒 TTL 和五秒清理周期触发选举，逻辑成员有效性按实际到期时间判断。

候补恢复最近已应用的地图快照，重新打洞并建立成员连接后确认成功。普通成员在收到新权威版本完整快照前保持暂停。官方持续失联超过成员时限或明确告知会话失效时返回主界面。Windows/Linux 与跨网络打洞仍需实际验证，未提供中继。


## 网卡与 VPN 候选地址

申请打洞票据支持 LocalAddresses（最多 32 个有效网卡 IPv4，包含 VPN 网卡及公网网卡）和实际游戏 Socket 的 LocalPort。过滤未指定、回环、169.254/16 链路本地、组播及保留地址，仍不支持 IPv6。候选由客户端声明；公网 Address/Port 仍取自 UDP 登记的实际来源。

有效队伍成员查询 peers 时返回对端网卡候选，不再要求相同公网出口；对端仍须具有有效成员、票据及 UDP 登记。数据沿用队伍成员的 Redis 临时会话，不增加数据库表或接口。客户端通过原游戏 Socket 逐条握手、多次测量往返延迟，再优先连接低延迟路径；VPN 需允许成员互访和游戏 UDP。

部署时先更新后端，再同步更新联机客户端。只做静态检查，未编译或运行联机测试。
