#!/bin/bash
set -e

echo "=============================="
check_port() {
  local host=$1
  local port=$2
  
  timeout 1 bash -c ">/dev/tcp/$1/$2" 2>/dev/null
  return $?
}

wait_for() {
  local host=$1
  local port=$2
  local service_name=$3
  local post_wait=${4:-5}
  local retries=30
  local wait_seconds=2

  echo "Wait ${service_name} server (host: ${host}, port: ${port})..."
  for ((i=1; i<=retries; i++)); do
    if check_port "${host}" "${port}"; then
      echo "${service_name} is ready."
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

MYSQL_HOST=${MYSQL_HOST:-"host.docker.internal"}
MYSQL_PORT=${MYSQL_PORT:-3306}
REDIS_HOST=${REDIS_HOST:-"host.docker.internal"}
REDIS_PORT=${REDIS_PORT:-6379}
SEQ_HOST=${SEQ_HOST:-"host.docker.internal"}
SEQ_PORT=${SEQ_PORT:-5341}

wait_for "${MYSQL_HOST}" "${MYSQL_PORT}" "MySQL" 5
wait_for "${REDIS_HOST}" "${REDIS_PORT}" "Redis" 3
wait_for "${SEQ_HOST}" "${SEQ_PORT}" "Seq" 3

echo "Deps ready, starting app."
echo "=============================="
exec dotnet Void.Bot.Api.dll
