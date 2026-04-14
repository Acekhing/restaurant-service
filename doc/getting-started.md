# Getting Started

This guide walks you through running the QC Inventory application locally.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)

## Architecture Overview

The solution is made up of four runnable projects and two shared libraries:

| Project | Type | Purpose |
|---|---|---|
| **Inventory.API** | ASP.NET Core Web API | HTTP API with Swagger UI; writes to PostgreSQL with a transactional outbox |
| **Inventory.ElasticProjector** | Background Worker | Consumes Kafka topic `inventory.outbox`, updates a Postgres read model, and indexes into Elasticsearch |
| **Inventory.AuditWorker** | Background Worker | Consumes `inventory.outbox` and writes audit log rows to Postgres |
| *InventoryCore* | Class Library | Shared domain helpers, outbox, idempotency |
| *Inventory.Contracts* | Class Library | Shared DTOs and contracts |

Infrastructure dependencies:

| Service | Default Port | Role |
|---|---|---|
| PostgreSQL 16 | 5432 | Primary data store (requires `wal_level=logical` for Debezium CDC) |
| Kafka | 9092 | Message broker |
| Kafka Connect (Debezium 2.7) | 8083 | Captures outbox table changes and publishes to Kafka |
| Redis 7 | 6379 | Idempotency keys for workers |
| Elasticsearch 8.x | 9200 | Full-text search index |
| Kibana (optional) | 5601 | Elasticsearch UI |

## Option A — Full Stack in Docker (Recommended for First Run)

This is the simplest way to get everything running. A single Compose file starts all
infrastructure, applies database migrations, and registers the Debezium connector automatically.

```bash
docker compose -f docker-compose.full-stack.yml up -d
```

Wait a minute or so for all services to become healthy. You can check status with:

```bash
docker compose -f docker-compose.full-stack.yml ps
```

Once the `migrate` and `connect-register` services show as exited (exit code 0),
infrastructure is ready. Now start the .NET projects.

### Run the API

```bash
dotnet run --project src/Inventory.API
```

The API starts on **http://localhost:5140** with Swagger UI at
[http://localhost:5140/swagger](http://localhost:5140/swagger).

### Run the Workers

Open two additional terminal windows and start each worker:

```bash
# Terminal 2 — Elasticsearch Projector
dotnet run --project src/Inventory.ElasticProjector

# Terminal 3 — Audit Worker
dotnet run --project src/Inventory.AuditWorker
```

> **Note:** The ElasticProjector and AuditWorker default `appsettings.json` files use
> `Username=appuser;Password=apppass` for Postgres. The full-stack Compose creates the
> database with `postgres`/`postgres`. Either create the `appuser` role in Postgres or
> override the connection string:
>
> ```bash
> Database__ConnectionString="Host=localhost;Port=5432;Database=inventory;Username=postgres;Password=postgres" \
>   dotnet run --project src/Inventory.ElasticProjector
> ```
>
> The same override applies to `Inventory.AuditWorker`.

## Option B — Local Infrastructure with Docker Only for Kafka Connect

Use this approach when you already have PostgreSQL, Kafka, Redis, and Elasticsearch
running on your machine.

### 1. Prepare PostgreSQL

Make sure your Postgres instance has logical replication enabled (`wal_level=logical`)
and create the `inventory` database:

```sql
CREATE DATABASE inventory;
```

### 2. Apply Migrations

From the repository root:

```bash
bash docker/migrate.sh
```

The script installs `dotnet-ef` and runs EF Core migrations against the `inventory`
database. Override defaults with environment variables if needed:

| Variable | Default |
|---|---|
| `POSTGRES_HOST` | `localhost` |
| `POSTGRES_PORT` | `5432` |
| `POSTGRES_USER` | `postgres` |
| `POSTGRES_PASSWORD` | `postgres` |

### 3. Start Kafka Connect

```bash
docker compose up -d
```

This starts only the Debezium Connect container. It reaches your host Kafka via
`host.docker.internal:9092` by default. To change the broker address, create a `.env`
file (see `.env.example`):

```bash
cp .env.example .env
# edit .env if your Kafka broker is on a different address
```

### 4. Register the Debezium Connector

```bash
bash docker/register-connectors.sh
```

This POSTs the connector configuration to Kafka Connect. Defaults:

| Variable | Default |
|---|---|
| `CONNECT_URL` | `http://localhost:8083` |
| `POSTGRES_HOST` | `host.docker.internal` |

### 5. Run the .NET Projects

Same as Option A — start the API and three workers with `dotnet run`.

## Building the Solution

To compile everything without running:

```bash
dotnet build QCInventory.sln
```

## Running Tests

```bash
dotnet test src/Inventory.Tests
```

Tests use [Testcontainers](https://dotnet.testcontainers.org/) to spin up PostgreSQL and
Redis automatically — Docker must be running.

## Configuration Reference

### Inventory.API — `src/Inventory.API/appsettings.json`

| Key | Default | Description |
|---|---|---|
| `ConnectionStrings:Inventory` | `Host=localhost;Port=5432;Database=inventory;Username=postgres;Password=postgres` | Npgsql connection string |
| `Elasticsearch:Uri` | `http://localhost:9200` | Elasticsearch node URL |

### Workers — `appsettings.json` in each worker project

| Key | Description |
|---|---|
| `Kafka:BootstrapServers` | Kafka broker address (`localhost:9092`) |
| `Kafka:GroupId` | Consumer group (unique per worker) |
| `Kafka:Topics` | Topics to subscribe to (`inventory.outbox`) |
| `Redis:ConnectionString` | Redis address (`localhost:6379`) |
| `Redis:KeyPrefix` | Prefix for idempotency keys (unique per worker) |
| `Database:ConnectionString` | Npgsql connection string |

All settings can be overridden with environment variables using the standard ASP.NET Core
double-underscore convention, e.g. `Kafka__BootstrapServers=broker:9092`.

## Verifying Everything Works

1. Open Swagger UI at [http://localhost:5140/swagger](http://localhost:5140/swagger).
2. Create an inventory item via the API.
3. Check the Elasticsearch index: `curl http://localhost:9200/inventory/_search?pretty`
4. Verify Kafka Connect status: `curl http://localhost:8083/connectors`
5. Optionally open Kibana at [http://localhost:5601](http://localhost:5601) to browse the
   `inventory` index.

## Stopping Everything

```bash
# Stop .NET processes with Ctrl+C in each terminal, then:

# If using full-stack Compose:
docker compose -f docker-compose.full-stack.yml down

# If using local infrastructure + Connect only:
docker compose down
```

Add `-v` to also remove Docker volumes (database data, Elasticsearch index):

```bash
docker compose -f docker-compose.full-stack.yml down -v
```
