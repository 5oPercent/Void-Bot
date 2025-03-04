#!/bin/bash
set -e

echo "=============================="
wait_for() {
  local host=$1
  local port=$2
  local service_name=$3
  local post_wait=${4:-5}
  local retries=30
  local wait_seconds=2

  echo "Wait ${service_name} server (host: ${host}, port: ${port})..."
  for ((i=1; i<=retries; i++)); do
    if nc -z ${host} ${port}; then
      sleep ${post_wait}
      return 0
    fi
    echo "${service_name} is not ready (attempt ${i})..."
    sleep ${wait_seconds}
  done

  echo "Wait ${service_name} timeout, Exit."
  exit 1
}

# 注意：这里 host.docker.internal 在 macOS/Windows 下可用，在 Linux 下建议使用 extra_hosts 参数将 host.docker.internal 映射到宿主机 IP

wait_for "host.docker.internal" 3306 "MySQL" 5
wait_for "host.docker.internal" 6379 "Redis" 3
wait_for "host.docker.internal" 5341 "Seq" 3

echo "Deps ready, starting app."
echo "=============================="
exec dotnet Void.Bot.Api.dll
