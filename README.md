HI, Void 是一个查询机器人, 正处于早期开发阶段.

## 目标方向

1. 集成 Destiny2 Api.
2. 继承 Steam Api.
3. 在线 Web 控制面板.

## 部署
### 数据库
> 项目所使用账号密码请自行修改 `appsettings.json` 文件.

```
docker pull --platform linux/x86_64 mysql:8.0
docker run -itd --name mysql-8-0 -p 3306:3306 -e MYSQL_ROOT_PASSWORD=123456 mysql:8.0
```

> 请注意, 请勿在生产环境中使用默认密码.

- 端口: `3306`
- 账号: `root`
- 密码: `123456`

### Seq日志
> 项目所使用的日志系统, 请自行修改 `appsettings.json` 文件.

```
docker pull datalust/seq
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

- 访问地址: http://localhost:5341
