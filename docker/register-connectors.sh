#!/usr/bin/env sh
set -eu
CONNECT_URL="${CONNECT_URL:-http://localhost:8083}"
POSTGRES_HOST="${POSTGRES_HOST:-host.docker.internal}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DIR="$ROOT/src/Infrastructure.Debezium/connectors"

echo "Waiting for Kafka Connect at $CONNECT_URL ..."
for _ in $(seq 1 60); do
  if curl -fsS "$CONNECT_URL/" >/dev/null 2>&1; then
    break
  fi
  sleep 2
done

post_connector() {
  file="$1"
  tmp="$(mktemp)"
  sed "s/POSTGRES_HOST_PLACEHOLDER/${POSTGRES_HOST}/g" "$file" >"$tmp"
  echo "POST $file (database.hostname=$POSTGRES_HOST)"
  curl -fsS -X POST -H "Content-Type: application/json" --data @"$tmp" "$CONNECT_URL/connectors" \
    || echo "(connector may already exist — OK)"
  rm -f "$tmp"
}

post_connector "$DIR/inventory-outbox-connector.json"

echo "Connector registration finished."
