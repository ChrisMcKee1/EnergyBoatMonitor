# Research: PostgreSQL Migration with Aspire Integration

**Feature**: 001-migrate-from-mock  
**Date**: October 3, 2025  
**Research Phase**: Phase 0 - Technology Validation

---

## Executive Summary

All technical unknowns have been resolved using official Microsoft Learn documentation. The migration path is clear:

1. **Aspire PostgreSQL Hosting**: Use `AddAzurePostgresFlexibleServer().RunAsContainer()` for local dev
2. **Client Integration**: Use `AddNpgsqlDataSource()` for direct SQL (no EF Core overhead)
3. **Secrets Management**: Leverage Aspire's `AddParameter(secret: true)` with User Secrets
4. **Database Tooling**: Include pgAdmin via `WithPgAdmin()` for visual management
5. **Data Persistence**: Aspire supports `WithDataVolume()` for persistent PostgreSQL storage

---

## Research Findings

### 1. Aspire PostgreSQL Hosting Integration

**Decision**: Use `AddAzurePostgresFlexibleServer().RunAsContainer()` for local development

**Rationale**:
- **Hybrid approach**: Same API for local containers and Azure Flexible Server deployment
- **No Docker knowledge required**: Aspire handles container lifecycle
- **pgAdmin included**: `.WithPgAdmin()` adds web-based admin UI automatically
- **Persistent data**: `.WithDataVolume()` preserves database across restarts

**Official Pattern** (from Microsoft Learn):
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Local development with password auth
var username = builder.AddParameter("postgres-username", secret: true);
var password = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
                      .RunAsContainer()
                      .WithPasswordAuthentication(username, password)
                      .WithPgAdmin()
                      .WithDataVolume();

var db = postgres.AddDatabase("ContosoSeaDB");

builder.AddProject<Projects.ApiService>("apiservice")
       .WithReference(db);
```

**Alternatives Considered**:
- `AddPostgres()` - Simpler but doesn't support Azure deployment path
- Entity Framework Core - Rejected (adds unnecessary abstraction, prefer direct SQL)
- Manual Docker Compose - Rejected (defeats Aspire orchestration benefits)

**Source**: [Aspire Azure PostgreSQL Integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/azure-postgresql-integration)

---

### 2. Client Integration with NpgsqlDataSource

**Decision**: Use `AddNpgsqlDataSource()` for direct SQL queries

**Rationale**:
- **Lightweight**: No ORM overhead (EF Core not needed for simple CRUD)
- **Connection pooling**: `NpgsqlDataSource` manages pool automatically
- **Aspire-aware**: Built-in health checks, metrics, tracing
- **Performance**: Direct SQL is faster than EF Core for our read-heavy workload

**Official Pattern** (from Microsoft Learn):
```csharp
// ApiService/Program.cs
builder.AddNpgsqlDataSource(connectionName: "ContosoSeaDB");

// Usage in services
public class BoatRepository(NpgsqlDataSource dataSource)
{
    public async Task<IEnumerable<BoatState>> GetAllBoatStatesAsync()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM boat_states";
        // ... execute and map results
    }
}
```

**Alternatives Considered**:
- Entity Framework Core - Rejected (unnecessary complexity, migrations overhead)
- Dapper - Considered but not needed (Npgsql is sufficient for our scale)
- Repository + UnitOfWork pattern - Kept simple (direct repository pattern only)

**Source**: [Aspire PostgreSQL Client Integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-integration#client-integration)

---

### 3. Secrets Management via Aspire Parameters

**Decision**: Use `AddParameter(secret: true)` with User Secrets for credentials

**Rationale**:
- **Aspire-native**: Built into orchestration (no custom config needed)
- **Developer-friendly**: Stored in User Secrets (not in source control)
- **Environment-aware**: Different secrets for dev/staging/prod
- **Manifest-ready**: Secrets marked for secure provisioning when deploying

**Official Pattern** (from Microsoft Learn):
```csharp
// AppHost.cs
var username = builder.AddParameter("postgres-username", secret: true);
var password = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
                      .WithPasswordAuthentication(username, password);
```

**User Secrets Configuration** (right-click AppHost → Manage User Secrets):
```json
{
  "Parameters:postgres-username": "adminuser",
  "Parameters:postgres-password": "P@ssw0rd123!"
}
```

**Alternatives Considered**:
- Environment variables - Less secure (visible in process listings)
- appsettings.json - Rejected (secrets in source control is bad practice)
- Azure Key Vault - Overkill for local dev (good for prod deployment)

**Source**: [External Parameters - Secret Values](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/external-parameters#secret-values)

---

### 4. pgAdmin Integration

**Decision**: Use `.WithPgAdmin()` to add pgAdmin container

**Rationale**:
- **Zero-config**: Aspire auto-configures connection to PostgreSQL
- **Web-based**: No client install required (runs at http://localhost:5050)
- **Pre-configured**: All databases automatically registered in pgAdmin bookmarks
- **Developer UX**: Visual query editor, schema browser, data inspection

**Official Pattern** (from Microsoft Learn):
```csharp
var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
                      .RunAsContainer()
                      .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));
```

**Access**: Navigate to `http://localhost:5050` after `aspire run`

**Alternatives Considered**:
- pgWeb - Lighter but less feature-complete
- Manual pgAdmin install - Rejected (breaks "everything via Aspire" principle)
- No admin tool - Rejected (hard to debug seed data / inspect schema)

**Source**: [Aspire PostgreSQL Integration - Add pgAdmin Resource](https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-integration#add-postgresql-pgadmin-resource)

---

### 5. Data Persistence with Volumes

**Decision**: Use `.WithDataVolume()` for persistent PostgreSQL storage

**Rationale**:
- **Survives restarts**: Data persists across `aspire run` sessions
- **Fast startup**: No need to re-seed on every launch
- **Named volumes**: Aspire creates `{AppName}-postgres-data` volume automatically
- **Clean reset**: `docker volume rm` to wipe and re-seed if needed

**Official Pattern** (from Microsoft Learn):
```csharp
var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
                      .RunAsContainer()
                      .WithDataVolume();
```

**Volume Lifecycle**:
- First `aspire run`: Creates volume, seeds initial data
- Subsequent runs: Reuses existing volume (no re-seed unless empty)
- Clean slate: `docker volume rm energyboatapp-postgres-data`

**Alternatives Considered**:
- Bind mounts - Rejected (platform-specific paths, less portable)
- No persistence - Rejected (poor DX, re-seed on every restart)
- External PostgreSQL instance - Overkill for local dev

**Source**: [Aspire PostgreSQL Integration - Data Volumes](https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-integration#add-postgresql-server-resource-with-data-volume)

---

## Best Practices Validated

### Performance Considerations

1. **Connection Pooling**: `NpgsqlDataSource` handles pooling (no manual pool management)
   - Default pool size: 100 connections
   - Sufficient for 100 concurrent API requests (NFR-003)

2. **Query Performance**: Use parameterized queries to prevent SQL injection
   ```csharp
   command.CommandText = "SELECT * FROM boats WHERE id = $1";
   command.Parameters.AddWithValue(boatId);
   ```

3. **Read-Heavy Workload**: Boat state queries are read-heavy (90% reads, 10% writes)
   - No need for read replicas at current scale (4 boats → hundreds)
   - Index on `boat_states.boat_id` for fast lookups

4. **Batch Updates**: Update all boat states in single transaction
   ```csharp
   await using var transaction = await connection.BeginTransactionAsync();
   // ... multiple UPDATE statements
   await transaction.CommitAsync();
   ```

### Security Best Practices

1. **Secrets Management**: Never commit `Parameters:*` to source control
   - Use User Secrets (dev)
   - Use Azure Key Vault references (prod)

2. **Least Privilege**: Database user should have minimal permissions
   - `CONNECT` on database
   - `SELECT`, `INSERT`, `UPDATE` on tables (no `DELETE` needed)

3. **Parameterized Queries**: Always use parameters (never string interpolation)

### Observability

Aspire PostgreSQL integration includes:
- **Health checks**: `/health` endpoint verifies database connectivity
- **Metrics**: Connection pool stats, query duration histograms
- **Tracing**: OpenTelemetry spans for each query
- **Logging**: Npgsql logs to standard output (captured by Aspire dashboard)

---

## Open Questions Resolved

| Question | Resolution |
|----------|-----------|
| Use EF Core or raw SQL? | **Raw SQL via NpgsqlDataSource** (simpler, faster, no migrations complexity) |
| How to manage secrets locally? | **Aspire `AddParameter(secret: true)` + User Secrets** |
| How to persist data across restarts? | **`.WithDataVolume()`** (Docker named volume) |
| Include database admin UI? | **Yes - pgAdmin via `.WithPgAdmin()`** |
| Azure deployment path? | **Same code** - `RunAsContainer()` becomes Azure Flexible Server when published |
| Connection pooling? | **Automatic** - `NpgsqlDataSource` manages pool |
| Database name? | **ContosoSeaDB** (per user requirement) |

---

## Technology Stack Finalized

| Component | Technology | Version |
|-----------|-----------|---------|
| Hosting Integration | `Aspire.Hosting.PostgreSQL` | 9.5.0 |
| Client Integration | `Aspire.Npgsql` | 9.5.0 |
| PostgreSQL | Docker image `postgres` | 16 (latest stable) |
| Admin UI | Docker image `dpage/pgadmin4` | Latest |
| Secrets | Aspire Parameters + User Secrets | Built-in |

---

## Next Steps (Phase 1)

With all research complete, Phase 1 will generate:

1. **data-model.md**: Database schema (tables, columns, relationships, indexes)
2. **contracts/**: API contract validation (confirm `BoatStatus` unchanged)
3. **quickstart.md**: Developer onboarding (how to run with PostgreSQL)
4. **Failing tests**: Contract tests for `/api/boats` endpoint

---

*Research Phase Complete - Ready for Phase 1: Design & Contracts*
