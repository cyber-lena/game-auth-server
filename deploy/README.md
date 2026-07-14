# GameAuth Deployment & Observability

This folder contains the local container orchestration and observability stack for GameAuthServer.

## Stack

| Component | Purpose | Port |
|-----------|---------|------|
| postgres | Primary relational store (users, credentials, sessions, audit logs) | 5432 |
| redis | Sessions, token revocation, profile cache | 6379 |
| rabbitmq | MassTransit message transport (+ management UI) | 5672 / 15672 |
| otel-collector | Receives OTLP traces/metrics from services | 4317 / 4318 |
| tempo | Distributed trace storage | 3200 |
| prometheus | Metrics scraping (`/metrics` on each service) + alert rules | 9090 |
| grafana | Dashboards & datasources (anonymous access enabled) | 3000 |
| core / profile / audit | The three GameAuth services | 8080 / 8081 / 8082 |

## Run

```powershell
cd deploy
docker compose up --build
```

Grafana: http://localhost:3000 (admin/admin) — the **GameAuth Services Overview** dashboard is auto-provisioned.
Prometheus: http://localhost:9090 — alert rules from `observability/alert-rules.yml`.
RabbitMQ management: http://localhost:15672 (guest/guest).

## Configuration

Services read connection strings and endpoints from environment variables (see `docker-compose.yml`),
overriding the defaults in each project's `appsettings.json`. Set a strong `Jwt__SigningKey`
for the `core` service before any real deployment.

## Database migrations

The initial EF Core migration lives in `src/GameAuth.Infrastructure/Migrations`. Apply it with:

```powershell
dotnet ef database update `
  --project src/GameAuth.Infrastructure `
  --startup-project src/GameAuth.Core
```
