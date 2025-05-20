FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

COPY ./src/Void.Bot.Api ./Void.Bot.Api
COPY ./src/Void.Bot.Core ./Void.Bot.Core
COPY ./src/Void.Bot.Message ./Void.Bot.Message

COPY ./src/Void.Bot.Api/appsettings.Docker.json ./Void.Bot.Api/appsettings.json

RUN dotnet publish Void.Bot.Api -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libpng16-16 \
    libharfbuzz0b \
    libicu67 \
    libexpat1 \
    libdl2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build-env /src/out ./

COPY entrypoint.sh ./
RUN chmod +x ./entrypoint.sh

ENV ASPNETCORE_URLS="http://*:3399"
EXPOSE 3399

ENTRYPOINT ["./entrypoint.sh"]