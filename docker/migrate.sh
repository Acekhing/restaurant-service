#!/usr/bin/env sh
set -eu
export PATH="$PATH:/root/.dotnet/tools"
export DOTNET_NOLOGO=1

# Pin a concrete version (avoids shell-glob / NuGet resolution issues on some hosts).
EF_VERSION="8.0.11"
if ! dotnet tool list -g | grep -q "dotnet-ef"; then
  echo "Installing dotnet-ef $EF_VERSION..."
  dotnet tool install --global dotnet-ef --version "$EF_VERSION"
fi

echo "Building project for migrations..."
dotnet restore src/Inventory.API/Inventory.API.csproj
dotnet build src/Inventory.API/Inventory.API.csproj -c Release

echo "Applying migrations (inventory DB on ${POSTGRES_HOST:-postgres})..."
dotnet ef database update \
  --project src/Inventory.API/Inventory.API.csproj \
  --startup-project src/Inventory.API/Inventory.API.csproj

echo "Migrations applied."
