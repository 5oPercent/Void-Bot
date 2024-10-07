HI, Void 是一个查询机器人, 正处于早期开发阶段.

## 目标方向

1. 集成 Destiny2 Api.
2. 继承 Steam Api.
3. 在线 Web 控制面板.

## 项目最佳化部署

项目使用以下服务提供最佳支持，均可使用 Docker 部署。

### 数据库 (必须)

1. 拉取 `Mysql 8.0` 镜像
```
docker pull --platform linux/x86_64 mysql:8.0
```

2. 运行容器
```
docker run -itd --name mysql-8-0 -p 3306:3306 -e MYSQL_ROOT_PASSWORD=123456 mysql:8.0
```
- 端口: `3306`
- 账号: `root`
- 密码: `123456`

### 日志服务

1. 拉取 `Seq` 镜像
```
docker pull datalust/seq
```

2. 运行容器
```
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```
- 访问地址: http://localhost:5341/
