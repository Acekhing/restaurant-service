#!/usr/bin/env bash
# Run docker compose against docker-compose.prod.yml with .env.production loaded.
# Example: ./deploy/compose-up.sh up -d
#          ./deploy/compose-up.sh logs -f nginx
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
exec docker compose -f "$ROOT/docker-compose.prod.yml" --env-file "$ROOT/.env.production" "$@"
