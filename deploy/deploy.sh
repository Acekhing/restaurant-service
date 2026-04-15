#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
COMPOSE_FILE="$PROJECT_ROOT/docker-compose.prod.yml"
ENV_FILE="$PROJECT_ROOT/.env.production"
ENV_EXAMPLE="$PROJECT_ROOT/.env.production.example"

# ──────────────── Helpers ────────────────

red()   { printf '\033[0;31m%s\033[0m\n' "$*"; }
green() { printf '\033[0;32m%s\033[0m\n' "$*"; }
bold()  { printf '\033[1m%s\033[0m\n' "$*"; }

check_command() {
    if ! command -v "$1" &>/dev/null; then
        red "ERROR: $1 is not installed."
        exit 1
    fi
}

# ──────────────── Prerequisites ────────────────

bold "Checking prerequisites..."
check_command docker
check_command git

if ! docker compose version &>/dev/null; then
    red "ERROR: docker compose plugin is not installed."
    echo "Install it with: sudo apt install docker-compose-plugin"
    exit 1
fi

green "All prerequisites met."

# ──────────────── Environment file ────────────────

if [ ! -f "$ENV_FILE" ]; then
    bold "Creating .env.production from example..."
    cp "$ENV_EXAMPLE" "$ENV_FILE"
    red "IMPORTANT: Edit $ENV_FILE and set a strong POSTGRES_PASSWORD before continuing."
    echo ""
    echo "  nano $ENV_FILE"
    echo ""
    echo "Then re-run this script."
    exit 1
fi

# Validate required vars
set -a
# shellcheck source=/dev/null
source "$ENV_FILE"
set +a

if [ -z "${POSTGRES_PASSWORD:-}" ] || [ "$POSTGRES_PASSWORD" = "change-me-to-a-strong-password" ]; then
    red "ERROR: POSTGRES_PASSWORD in $ENV_FILE is not set or still the default placeholder."
    exit 1
fi

green "Environment file looks good."

# ──────────────── Port Check ────────────────

port_in_use() {
    local p="$1"
    if command -v lsof &>/dev/null; then
        lsof -Pi :"$p" -sTCP:LISTEN -t >/dev/null 2>&1
        return $?
    fi
    if command -v ss &>/dev/null; then
        ss -tln 2>/dev/null | awk 'NR>1 {print $4}' | grep -qE ":${p}$"
        return $?
    fi
    return 1
}

TARGET_PORT="${APP_PORT:-80}"
if port_in_use "$TARGET_PORT"; then
    red "ERROR: Port $TARGET_PORT is already in use."
    echo "Find what is listening: ss -tlnp | grep ':$TARGET_PORT '"
    echo "Or change the port by setting APP_PORT in $ENV_FILE (e.g. APP_PORT=8082)."
    exit 1
fi

# ──────────────── Build & Start ────────────────

bold "Building and starting all services..."
cd "$PROJECT_ROOT"

docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --build

# ──────────────── Wait for one-shot containers ────────────────

bold "Waiting for database migrations..."
docker compose -f "$COMPOSE_FILE" logs -f migrate 2>&1 | while IFS= read -r line; do
    echo "  [migrate] $line"
    echo "$line" | grep -q "Migrations applied" && break
done || true

bold "Waiting for connector registration..."
docker compose -f "$COMPOSE_FILE" logs -f connect-register 2>&1 | while IFS= read -r line; do
    echo "  [connect-register] $line"
    echo "$line" | grep -q "Connector registration finished" && break
done || true

# ──────────────── Status ────────────────

echo ""
bold "Service status:"
docker compose -f "$COMPOSE_FILE" ps
echo ""

SERVER_IP=$(hostname -I 2>/dev/null | awk '{print $1}' || echo "<VPS_IP>")

green "Deployment complete!"
echo ""
bold "Access points:"
echo "  Frontend:       http://$SERVER_IP:${APP_PORT:-80}/"
echo "  Waiter Portal:  http://$SERVER_IP:${APP_PORT:-80}/waiter/"
echo "  Backoffice:     http://$SERVER_IP:${APP_PORT:-80}/backoffice/"
echo "  API / Swagger:  http://$SERVER_IP:${APP_PORT:-80}/swagger"
echo "  Kibana:         http://$SERVER_IP:8081"
echo ""
bold "Useful commands:"
echo "  Logs:     docker compose -f $COMPOSE_FILE logs -f <service>"
echo "  Stop:     docker compose -f $COMPOSE_FILE down"
echo "  Rebuild:  docker compose -f $COMPOSE_FILE up -d --build"
