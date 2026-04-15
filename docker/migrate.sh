#!/usr/bin/env sh
set -eu
export PATH="$PATH:/root/.dotnet/tools"
dotnet tool install --global dotnet-ef --version 8.0.* >/dev/null

PG_HOST="${POSTGRES_HOST:-localhost}"
PG_PORT="${POSTGRES_PORT:-5432}"
PG_USER="${POSTGRES_USER:-postgres}"
PG_PASS="${POSTGRES_PASSWORD:-postgres}"

CS_BASE="Host=${PG_HOST};Port=${PG_PORT};Username=${PG_USER};Password=${PG_PASS}"

echo "Building project for migrations..."
dotnet build src/Inventory.API/Inventory.API.csproj -c Release

echo "Applying migrations to ${PG_HOST}:${PG_PORT}..."
dotnet ef database update \
  --project src/Inventory.API/Inventory.API.csproj \
  --startup-project src/Inventory.API/Inventory.API.csproj \
  --connection "${CS_BASE};Database=inventory"

echo "Migrations applied."
