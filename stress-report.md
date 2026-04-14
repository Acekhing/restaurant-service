# Infrastructure Stress Simulation Report

**Generated:** 2026-04-10 17:00:41 UTC

## Load Profile

| Metric | Value |
|--------|-------|
| Branches | 30,000 |
| Inventory Items | 10,000,000 |
| Menus | 5,000,000 |
| Concurrent Users | 50,000 |
| Availability Toggles/min | 250,000 |
| Price Updates/min | 120,000 |
| Branch Updates/min | 5,000 |
| Reads/sec | 500,000 |
| **Total Writes/sec** | **6,250** |
| **Total Outbox Events/sec** | **8,125** |

## System Limits

- **Max Sustainable Events/sec:** ~1
- **Peak Backpressure:** 100%
- **Primary Bottleneck:** Elasticsearch (score: 0.60)

### Estimated Breaking Points

- **Normal Production Load:** backpressure reached 100%, top bottleneck: Elasticsearch
- **Black Friday Surge:** backpressure reached 100%, top bottleneck: Elasticsearch
- **Branch Bulk Import:** backpressure reached 100%, top bottleneck: Elasticsearch
- **Kafka Broker Failure:** backpressure reached 100%, top bottleneck: Elasticsearch
- **Debezium Connector Lag:** backpressure reached 100%, top bottleneck: Elasticsearch
- **Elasticsearch Slowdown:** backpressure reached 100%, top bottleneck: Elasticsearch
- **Redis Memory Saturation:** backpressure reached 100%, top bottleneck: Elasticsearch

## Scenario Results

| Scenario | Duration | Writes | Errors | Peak Backpressure | Top Bottleneck |
|----------|----------|--------|--------|-------------------|----------------|
| Normal Production Load | 300s | 1,874,700 | 0 | 100% | Elasticsearch |
| Black Friday Surge | 900s | 28,124,100 | 0 | 100% | Elasticsearch |
| Branch Bulk Import | 600s | 29,999,400 | 0 | 100% | Elasticsearch |
| Kafka Broker Failure | 600s | 3,749,400 | 0 | 100% | Elasticsearch |
| Debezium Connector Lag | 900s | 5,624,100 | 0 | 100% | Elasticsearch |
| Elasticsearch Slowdown | 900s | 5,624,100 | 0 | 100% | Elasticsearch |
| Redis Memory Saturation | 600s | 11,250,000 | 0 | 100% | Elasticsearch |

### Normal Production Load

| Component | Avg P99 Latency | Max P99 Latency | Max Saturation | Max Queue | Errors |
|-----------|----------------|-----------------|----------------|-----------|--------|
| Elasticsearch | 14.7ms | 22.4ms | 100% | 254,568 | 0 |
| Debezium CDC | 0.1ms | 0.1ms | 81% | 0 | 0 |
| Kafka | 2.0ms | 3.0ms | 1% | 19,813 | 0 |
| Redis | 0.5ms | 0.5ms | 1% | 0 | 0 |
| PostgreSQL | 3.0ms | 3.0ms | 1% | 0 | 0 |
| API Gateway | 1747.3ms | 3670.6ms | 0% | 0 | 0 |

**Alerts (422 total):**
- [CascadingLatency] API Gateway P99 latency: 1954.2ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 136,618
- [CascadingLatency] API Gateway P99 latency: 1838.5ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 137,467
- [CascadingLatency] API Gateway P99 latency: 1873.1ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 138,315
- [CascadingLatency] API Gateway P99 latency: 1934.5ms
- ... and 412 more

### Black Friday Surge

| Component | Avg P99 Latency | Max P99 Latency | Max Saturation | Max Queue | Errors |
|-----------|----------------|-----------------|----------------|-----------|--------|
| Debezium CDC | 0.1ms | 0.1ms | 100% | 27,561,330 | 0 |
| Elasticsearch | 15.1ms | 22.5ms | 100% | 1,980,000 | 0 |
| Redis | 0.5ms | 0.5ms | 3% | 0 | 0 |
| Kafka | 2.0ms | 3.0ms | 2% | 24,390 | 0 |
| PostgreSQL | 3.0ms | 3.0ms | 1% | 0 | 0 |
| API Gateway | 13933.5ms | 28765.4ms | 0% | 0 | 0 |

**Alerts (489 total):**
- [CascadingLatency] API Gateway P99 latency: 25304.2ms
- [ServiceSaturation] Debezium CDC saturation at 100%
- [QueueBuildup] Debezium CDC queue depth: 24,621,455
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 1,768,800
- [CascadingLatency] API Gateway P99 latency: 23839.5ms
- [ServiceSaturation] Debezium CDC saturation at 100%
- [QueueBuildup] Debezium CDC queue depth: 24,652,078
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 1,771,000
- ... and 479 more

### Branch Bulk Import

| Component | Avg P99 Latency | Max P99 Latency | Max Saturation | Max Queue | Errors |
|-----------|----------------|-----------------|----------------|-----------|--------|
| Debezium CDC | 0.1ms | 0.1ms | 100% | 32,999,220 | 0 |
| Elasticsearch | 15.2ms | 22.5ms | 100% | 1,320,000 | 0 |
| Redis | 0.5ms | 0.5ms | 2% | 0 | 0 |
| Kafka | 2.0ms | 3.0ms | 2% | 24,390 | 0 |
| PostgreSQL | 3.0ms | 3.0ms | 1% | 0 | 0 |
| API Gateway | 9242.0ms | 19048.0ms | 0% | 0 | 0 |

**Alerts (481 total):**
- [ServiceSaturation] Debezium CDC saturation at 100%
- [QueueBuildup] Debezium CDC queue depth: 27,774,343
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 1,111,000
- [CascadingLatency] API Gateway P99 latency: 15272.4ms
- [ServiceSaturation] Debezium CDC saturation at 100%
- [QueueBuildup] Debezium CDC queue depth: 27,829,342
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 1,113,200
- [CascadingLatency] API Gateway P99 latency: 15886.5ms
- ... and 471 more

### Kafka Broker Failure

| Component | Avg P99 Latency | Max P99 Latency | Max Saturation | Max Queue | Errors |
|-----------|----------------|-----------------|----------------|-----------|--------|
| Elasticsearch | 15.1ms | 22.5ms | 100% | 509,136 | 0 |
| Debezium CDC | 0.1ms | 0.1ms | 81% | 0 | 0 |
| Kafka | 2.1ms | 3.0ms | 4% | 54,208 | 0 |
| Redis | 0.5ms | 0.5ms | 2% | 0 | 0 |
| PostgreSQL | 3.0ms | 3.0ms | 1% | 0 | 0 |
| API Gateway | 3463.6ms | 7447.9ms | 0% | 0 | 0 |

**Alerts (422 total):**
- [CascadingLatency] API Gateway P99 latency: 5512.2ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 391,186
- [CascadingLatency] API Gateway P99 latency: 5778.6ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 392,035
- [CascadingLatency] API Gateway P99 latency: 5278.2ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 392,883
- [CascadingLatency] API Gateway P99 latency: 5363.7ms
- ... and 412 more

### Debezium Connector Lag

| Component | Avg P99 Latency | Max P99 Latency | Max Saturation | Max Queue | Errors |
|-----------|----------------|-----------------|----------------|-----------|--------|
| Debezium CDC | 0.1ms | 0.1ms | 100% | 3,674,220 | 0 |
| Elasticsearch | 11.6ms | 22.5ms | 100% | 396,000 | 0 |
| Redis | 0.5ms | 0.5ms | 3% | 0 | 0 |
| Kafka | 2.0ms | 3.0ms | 2% | 24,390 | 0 |
| PostgreSQL | 3.0ms | 3.0ms | 1% | 0 | 0 |
| API Gateway | 667.2ms | 5594.3ms | 0% | 0 | 0 |

**Alerts (411 total):**
- [ServiceSaturation] Debezium CDC saturation at 100%
- [QueueBuildup] Debezium CDC queue depth: 3,488,466
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 217,800
- [CascadingLatency] API Gateway P99 latency: 2943.4ms
- [ServiceSaturation] Debezium CDC saturation at 100%
- [QueueBuildup] Debezium CDC queue depth: 3,486,590
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 220,000
- [CascadingLatency] API Gateway P99 latency: 3120.9ms
- ... and 401 more

### Elasticsearch Slowdown

| Component | Avg P99 Latency | Max P99 Latency | Max Saturation | Max Queue | Errors |
|-----------|----------------|-----------------|----------------|-----------|--------|
| Elasticsearch | 15.0ms | 22.5ms | 100% | 3,463,704 | 0 |
| Debezium CDC | 0.1ms | 0.1ms | 81% | 0 | 0 |
| Redis | 0.5ms | 0.5ms | 3% | 0 | 0 |
| Kafka | 2.0ms | 3.0ms | 1% | 19,813 | 0 |
| PostgreSQL | 3.0ms | 3.0ms | 1% | 0 | 0 |
| API Gateway | 170099.5ms | 485671.9ms | 0% | 0 | 0 |

**Alerts (435 total):**
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 3,342,360
- [CascadingLatency] API Gateway P99 latency: 46231.0ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 3,343,208
- [CascadingLatency] API Gateway P99 latency: 46435.7ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 3,344,057
- [CascadingLatency] API Gateway P99 latency: 46461.8ms
- [ServiceSaturation] Elasticsearch saturation at 100%
- ... and 425 more

### Redis Memory Saturation

| Component | Avg P99 Latency | Max P99 Latency | Max Saturation | Max Queue | Errors |
|-----------|----------------|-----------------|----------------|-----------|--------|
| Debezium CDC | 0.1ms | 0.1ms | 100% | 8,625,000 | 0 |
| Elasticsearch | 15.1ms | 22.5ms | 100% | 1,320,000 | 0 |
| Redis | 0.8ms | 1.0ms | 2% | 0 | 0 |
| Kafka | 2.0ms | 3.0ms | 2% | 24,390 | 0 |
| PostgreSQL | 3.0ms | 3.0ms | 1% | 0 | 0 |
| API Gateway | 9245.1ms | 18790.7ms | 0% | 0 | 0 |

**Alerts (478 total):**
- [QueueBuildup] Elasticsearch queue depth: 1,111,000
- [CascadingLatency] API Gateway P99 latency: 15781.6ms
- [ServiceSaturation] Debezium CDC saturation at 100%
- [QueueBuildup] Debezium CDC queue depth: 7,273,750
- [ServiceSaturation] Elasticsearch saturation at 100%
- [QueueBuildup] Elasticsearch queue depth: 1,113,200
- [CascadingLatency] API Gateway P99 latency: 15786.9ms
- [ServiceSaturation] Debezium CDC saturation at 100%
- [QueueBuildup] Debezium CDC queue depth: 7,288,125
- [ServiceSaturation] Elasticsearch saturation at 100%
- ... and 468 more

## Scaling Forecast

| Multiplier | Writes/sec | Reads/sec | Stable? | Top Bottleneck | Score | Backpressure |
|------------|-----------|-----------|---------|----------------|-------|-------------|
| 2x | 12,500 | 1,000,000 | Yes | Elasticsearch | 0.60 | 97% |
| 5x | 31,250 | 2,500,000 | Yes | Elasticsearch | 0.60 | 98% |
| 10x | 62,500 | 5,000,000 | Yes | Elasticsearch | 0.60 | 97% |

## Infrastructure Recommendations

| Component | Setting | Current | Recommended | Reason |
|-----------|---------|---------|-------------|--------|
| Kafka | Partition Count | 6 | **6** | At 8,125 events/sec, 1,354 events/sec/partition. Target ~1,500 events/sec/partition for headroom. |
| Kafka | Broker Count | 3 | **3** | ~10 partitions per broker for balanced replication. |
| Debezium | Connector Tasks | 1 | **2** | Need 8,125 events/sec capacity. Each task handles ~10,000 events/sec at 70% target utilization. |
| PostgreSQL | Connection Pool Size | 100 | **29** | At 6,250 writes/sec with ~3.0ms P99 latency, need 29 connections (1.5x headroom). |
| PostgreSQL | max_wal_senders | 16 | **4** | WAL generation rate ~4.0 MB/sec. Need sufficient wal senders for replication. |
| Redis | Max Memory | 512 MB | **7253 MB** | At 16,250 keys/sec across 2 consumers with 60m dedup window (allkeys-lru eviction), peak key count ~58,500,000 consuming ~5,579 MB (1.3x headroom). |
| Elasticsearch | Primary Shards (across all indices) | 5 (default) | **2** | Total estimated data size: 28.7 GB across 15,030,000 docs (inventory + menus + branches). Target ~30 GB per shard. |
| Elasticsearch | Data Nodes | 1 | **3** | At 8,125 index ops/sec, each node handles ~5,000/sec. |
| Elasticsearch | refresh_interval | 1000ms | **5000ms** | High index rate benefits from longer refresh intervals to reduce I/O. |

## Bottleneck Ranking (Normal Load)

| Rank | Component | Score | Max Saturation | Max P99 Latency | Errors |
|------|-----------|-------|----------------|-----------------|--------|
| 1 | Elasticsearch | 0.603 | 100% | 22.4ms | 0 |
| 2 | Debezium CDC | 0.325 | 81% | 0.1ms | 0 |
| 3 | API Gateway | 0.300 | 0% | 3670.6ms | 0 |
| 4 | Kafka | 0.045 | 1% | 3.0ms | 0 |
| 5 | Redis | 0.005 | 1% | 0.5ms | 0 |
| 6 | PostgreSQL | 0.004 | 1% | 3.0ms | 0 |

