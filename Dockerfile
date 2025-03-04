FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

USER root

WORKDIR /app

COPY ./src/Void.Bot.Api ./build/Void.Bot.Api
COPY ./src/Void.Bot.Api/appsettings.Docker.json ./build/Void.Bot.Api/appsettings.json
COPY ./src/Void.Bot.Core ./build/Void.Bot.Core
COPY ./src/Void.Bot.Message ./build/Void.Bot.Message

RUN dotnet publish build/Void.Bot.Api -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

WORKDIR /app

ENV ASPNETCORE_URLS="http://*:3399"
EXPOSE 3399

COPY --from=build-env /app/out .
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]