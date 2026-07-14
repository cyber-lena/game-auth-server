# GameAuthServer - Multi-Million User Game Authentication Server

## Architecture Overview

Production-grade, microservices-ready authentication system using .NET 10 targeting Kubernetes deployment with complete observability.

### Key Features
- ✅ OAuth2/OpenID Connect with JWT tokens + TOTP MFA
- ✅ Hybrid data layer: PostgreSQL (persistent) + Redis (sessions, revocation, caching)
- ✅ gRPC inter-service communication
- ✅ MassTransit event bus with RabbitMQ
- ✅ OpenTelemetry distributed tracing & metrics
- ✅ Prometheus + Tempo + Grafana observability stack
- ✅ Built-in rate limiting with Redis backend
- ✅ Circuit breakers & resilience patterns

## Solution Structure

```
GameAuthServer/
├── src/
│   ├── GameAuth.Shared/          ✅ COMPLETED - Common DTOs, events, exceptions
│   ├── GameAuth.Protos/          ✅ COMPLETED - gRPC service definitions  
│   ├── GameAuth.Infrastructure/  🔄 IN PROGRESS - Data access, caching, event bus
│   ├── GameAuth.Core/            📋 PENDING - Main authentication service
│   ├── GameAuth.ProfileService/  📋 PENDING - User profile management
│   └── GameAuth.AuditService/    📋 PENDING - Audit logging & compliance
└── deployment/
	├── docker-compose.yml        📋 PENDING - Full local stack
	├── grafana/                  📋 PENDING - Dashboards & data sources
	├── otel-collector/          📋 PENDING - Collector configuration
	└── prometheus/              📋 PENDING - Scrape configuration
```

## Completed Components

### GameAuth.Shared ✅
- **Constants**: Auth, Error codes, gRPC service names
- **DTOs**: Login/Register/Refresh requests & responses
- **Events**: 7 domain events (UserRegistered, UserLoggedIn, MfaChallengeInitiated, TokenGenerated, ProfileUpdated, SecurityEventTriggered, ServiceHealth)
- **Exceptions**: AuthException, ValidationException, UnauthorizedException, ForbiddenException, RateLimitException
- **Interfaces**: IRepository, IUnitOfWork, ICacheService, IEventBus, IServiceClient
- **Models**: Result<T>, PagedResult<T>

### GameAuth.Protos ✅
- **auth/auth_service.proto**: Login, Register, ValidateToken, RefreshToken, Logout, InitiateMfaChallenge
- **profile/profile_service.proto**: GetProfile, UpdateProfile, GetSettings, UpdateSettings
- **audit/audit_service.proto**: LogEvent, QueryLogs, GetSecurityEvents
- **common/common.proto**: Shared message types (UserIdentity, ErrorDetails, Timestamp)

## Technology Stack

### Core Frameworks
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- gRPC (Grpc.AspNetCore 2.67)

### Data & Caching
- PostgreSQL 16 (Npgsql 8.0)
- Redis 7 (StackExchange.Redis 2.8)

### Messaging
- RabbitMQ 3.13
- MassTransit 8.2

### Observability
- OpenTelemetry 1.10
- Prometheus
- Tempo (trace storage)
- Jaeger (trace UI)
- Grafana
- Serilog

### Resilience
- Polly 8.4 (circuit breakers, retries)
- AspNetCoreRateLimit 5.1

## Database Schema

### Users Table
```sql
CREATE TABLE users (
	id BIGSERIAL PRIMARY KEY,
	username VARCHAR(255) UNIQUE NOT NULL,
	email VARCHAR(255) UNIQUE NOT NULL,
	created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Credentials Table
```sql
CREATE TABLE credentials (
	id BIGSERIAL PRIMARY KEY,
	user_id BIGINT REFERENCES users(id) ON DELETE CASCADE,
	password_hash VARCHAR(255) NOT NULL,
	created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### MFA Settings Table
```sql
CREATE TABLE mfa_settings (
	id BIGSERIAL PRIMARY KEY,
	user_id BIGINT UNIQUE REFERENCES users(id) ON DELETE CASCADE,
	mfa_type VARCHAR(50),
	mfa_secret VARCHAR(255),
	backup_codes TEXT[],
	verified BOOLEAN DEFAULT FALSE,
	created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Audit Logs Table
```sql
CREATE TABLE audit_logs (
	id BIGSERIAL PRIMARY KEY,
	user_id BIGINT REFERENCES users(id),
	event_type VARCHAR(100) NOT NULL,
	event_source VARCHAR(50) NOT NULL,
	ip_address INET,
	user_agent TEXT,
	status VARCHAR(50),
	timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_audit_logs_user_id_timestamp ON audit_logs(user_id, timestamp);
CREATE INDEX idx_audit_logs_event_type_timestamp ON audit_logs(event_type, timestamp);
```

## Communication Patterns

### Synchronous (gRPC)
- ProfileService → AuthService.ValidateToken(token)
- External → AuthService.Login(credentials)

### Asynchronous (Event Bus)
```
POST /auth/register
  ↓
AuthService publishes UserRegisteredEvent
  ├→ AuditService consumes → Log to DB
  └→ ProfileService consumes → Create default profile
```

## Observability Flow

```
Services (Auth, Profile, Audit)
  ↓ (OTEL SDK - traces, metrics, logs)
OTEL Collector (4317 gRPC, 4318 HTTP)
  ├→ Prometheus (9090) → Query metrics
  ├→ Tempo (3200) → Store traces
  ├→ Jaeger (16686) → Trace UI
  └→ Grafana (3000) → Unified dashboards
```

### Key Metrics
- `auth_login_attempts_total{status}` - Login success/failure counter
- `auth_login_duration_seconds` - Login latency histogram (p50, p95, p99)
- `auth_active_sessions` - Active session gauge
- `grpc_client_rpc_duration_seconds{service, method}` - gRPC latencies
- `db_query_duration_seconds{statement}` - Database query performance

### Grafana Dashboards (5)
1. **Auth Service Dashboard** - Login metrics, latency, MFA adoption
2. **Trace Explorer** - Distributed traces, dependency graph
3. **Service Dependencies** - Inter-service health, gRPC success rates
4. **Error Rate & SLO** - Error budget, uptime, alert thresholds
5. **Resource Utilization** - CPU, memory, connection pools

## Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
	"PostgreSQL": "Host=localhost;Database=gameauth;...",
	"Redis": "localhost:6379"
  },
  "RabbitMQ": {
	"Host": "localhost",
	"Port": 5672
  },
  "GrpcEndpoints": {
	"AuthService": "https://localhost:7001",
	"ProfileService": "https://localhost:7002",
	"AuditService": "https://localhost:7003"
  },
  "OpenTelemetry": {
	"OtlpExporter": { "Endpoint": "http://localhost:4317" },
	"Jaeger": { "Host": "localhost", "Port": 6831 },
	"Prometheus": { "Port": 9090, "Path": "/metrics" }
  },
  "Authentication": {
	"JwtSecret": "...",
	"AccessTokenExpirationMinutes": 60,
	"RefreshTokenExpirationDays": 7
  }
}
```

## Next Steps

### Remaining Implementation (Steps 5-40)
1. Complete Infrastructure library (DbContext, repositories, cache, event bus)
2. Create Core authentication service with JWT generation
3. Create ProfileService & AuditService
4. Configure OpenTelemetry instrumentation
5. Create Docker Compose with full observability stack
6. Create Grafana dashboards & Prometheus config
7. Integration tests

## Running Locally

```bash
# Start infrastructure (PostgreSQL, Redis, RabbitMQ, Observability)
docker-compose up -d

# Run Auth Service
cd src/GameAuth.Core
dotnet run

# Run Profile Service
cd src/GameAuth.ProfileService
dotnet run

# Run Audit Service
cd src/GameAuth.AuditService
dotnet run
```

### Access Points
- Auth API: https://localhost:7001
- Profile API: https://localhost:7002
- Audit API: https://localhost:7003
- Grafana: http://localhost:3000 (admin/admin)
- Prometheus: http://localhost:9090
- Jaeger UI: http://localhost:16686
- RabbitMQ Management: http://localhost:15672 (guest/guest)

## Deployment (Kubernetes - Phase 2)

Each service will have:
- Deployment (3 replicas, HPA)
- Service (ClusterIP for gRPC, LoadBalancer for external)
- ConfigMap (non-sensitive config)
- Secret (sensitive data)
- NetworkPolicy (restrict inter-service traffic)

## Security Features

- JWT token with RS256 signing
- TOTP MFA with backup codes
- Rate limiting (global + per-user + per-endpoint)
- Distributed token revocation via Redis
- Password hashing with Argon2
- HTTPS/TLS everywhere
- gRPC channel encryption (mTLS in production)

## Scalability Targets

- **Users**: Multi-million concurrent users
- **Throughput**: 10,000+ auth requests/second
- **Latency**: p99 < 200ms for login
- **Availability**: 99.99% uptime (4 nines)
- **Horizontal Scaling**: 3-100 replicas per service

## License

MIT License - See LICENSE file for details
