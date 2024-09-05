HI, Void 是一个命运2查询工具, 立志于做出同等小日向和光影思维同等的查询器~

## 目标方向

1. 集成 Destiny2 Api;
2. 在线 Web 控制面板;
3. 链接聊天平台：QQ、Discord;
4. 未来可能会扩展其它游戏，如果能支持的话 :)

## 项目最佳化部署

项目使用以下服务提供最佳支持，均可使用 Docker 部署。

### 数据库

> 必须安装的，目前没有计划支持其它数据库。

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

> 非必需，但是请注意目前没有计划记录日志到本地文件。

1. 拉取 `Seq` 镜像
```
docker pull datalust/seq
```

2. 运行容器
```
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```
- 访问地址: http://localhost:5341/
