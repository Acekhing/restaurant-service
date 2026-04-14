Local infrastructure (recommended)
===================================
Run PostgreSQL, Redis, Elasticsearch, and Kafka on the host. Only Kafka Connect (Debezium) runs in Docker.

1. Ensure PostgreSQL has wal_level=logical and create databases:
   restaurant_inventory, shop_inventory, pharmacy_inventory
   (see docker/postgres/init/01-databases.sql for CREATE DATABASE statements.)

2. From repo root:
   bash docker/migrate.sh
   docker compose up -d
   bash docker/register-connectors.sh

3. Point apps at localhost in appsettings (Postgres, Redis, Kafka, Elasticsearch).

register-connectors.sh defaults:
- CONNECT_URL=http://localhost:8083
- POSTGRES_HOST=host.docker.internal (so the Connect container can reach Postgres on the host)

If your broker is not on localhost:9092, set KAFKA_BOOTSTRAP in .env (see .env.example).

Full stack in Docker only
=========================
docker compose -f docker-compose.full-stack.yml up -d
