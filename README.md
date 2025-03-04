HI, Void 是一个查询机器人, 正处于早期开发阶段.

## 目标方向

1. 链接平台: Discord、Steam、Bungie
2. 集成 Destiny2 Api.
3. 在线 Web 控制面板.

## 部署
- 请注意, 请勿在生产环境中使用默认密码.

### Local
- 维护 `appsettings.json` 配置文件, 如: `ConnectionStrings`、`Token`...

### Docker
- 维护 `appsettings.Docker.json` 配置文件.

#### 仅构建项目和运行
```shell
docker build -t voidbot .
docker run -d --name void_bot_app -p 3399:3399 voidbot
```

#### 一键部署环境和运行项目
```shell
docker-compose up -d
```
