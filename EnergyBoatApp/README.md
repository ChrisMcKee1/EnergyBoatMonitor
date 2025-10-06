# ⚡ Energy Boat Service Monitor

A .NET Aspire application for real-time 3D visualization of energy service company boat fleets, backed by PostgreSQL database.

## Features

- **Real-time 3D Visualization**: Interactive Three.js scene showing boats color-coded by energy status
- **PostgreSQL Database**: Persistent storage for boat states, waypoints, and simulation data
- **C# Minimal API**: Lightweight API providing boat data from database
- **React Frontend**: Modern React with Vite for fast development
- **.NET Aspire Orchestration**: Simplified local development with automated PostgreSQL and pgAdmin setup
- **OpenTelemetry Integration**: Distributed tracing and observability built-in

## Architecture

```
EnergyBoatApp/
├── EnergyBoatApp.AppHost/          # Aspire orchestration (PostgreSQL, pgAdmin, API, Web)
├── EnergyBoatApp.ServiceDefaults/  # Shared configuration (OTEL, health checks)
├── EnergyBoatApp.ApiService/       # C# Minimal API (boat simulation & data)
│   ├── Models/                     # Boat, BoatState, Waypoint models
│   ├── Repositories/               # BoatRepository (Npgsql data access)
│   └── Services/                   # DatabaseInitializationService, SeedDataService
├── EnergyBoatApp.Web/              # React + Three.js frontend
└── EnergyBoatApp.Tests/            # Unit, integration, contract, and performance tests
    ├── UnitTests/                  # Repository unit tests (mocked)
    ├── IntegrationTests/           # PostgreSQL integration tests (Testcontainers)
    ├── ContractTests/              # API contract validation
    └── PerformanceTests/           # Query latency and concurrency tests
```

## Database Architecture

### Schema Design

EnergyBoatApp uses **PostgreSQL 16** with a relational schema optimized for real-time boat tracking:

```
┌─────────────────┐       ┌──────────────────┐       ┌─────────────────┐
│     boats       │       │   boat_states    │       │   waypoints     │
├─────────────────┤       ├──────────────────┤       ├─────────────────┤
│ id (PK)         │──────<│ boat_id (FK)     │       │ id (PK)         │
│ vessel_name     │       │ latitude         │       │ boat_id (FK)    │──┐
│ survey_type     │       │ longitude        │       │ latitude        │  │
│ project         │       │ heading          │       │ longitude       │  │
│ crew_count      │       │ status           │       │ sequence_order  │  │
│ equipment       │       │ energy_level     │       │ created_at      │  │
│ created_at      │       │ speed_knots      │       └─────────────────┘  │
└─────────────────┘       │ area_covered     │                            │
                          │ speed            │                            │
                          │ conditions       │                            │
                          │ waypoint_index   │                            │
                          │ last_updated     │                            │
                          └──────────────────┘                            │
                                      │                                   │
                                      └───────────────────────────────────┘
```

### Tables

#### `boats` - Vessel Metadata
Stores static vessel information (crew, equipment, project assignments).

**Key Fields**:
- `id` (VARCHAR PRIMARY KEY): Boat identifier (e.g., "BOAT-001")
- `vessel_name`: Human-readable name (e.g., "Contoso Sea Voyager")
- `survey_type`: Mission type (e.g., "Geophysical Survey")
- `project`: Current project assignment
- `crew_count`: Number of crew members

#### `boat_states` - Real-Time State
Tracks current position, energy, heading, and status. **Updated every 200ms-1s** during simulation.

**Key Fields**:
- `boat_id` (FK to `boats.id`): One-to-one relationship
- `latitude`, `longitude`: Current position (WGS84 coordinates)
- `heading`: Nautical heading in degrees (0-360, 0=North)
- `status`: "Active" | "Charging" | "Maintenance"
- `energy_level`: Battery percentage (0-100)
- `current_waypoint_index`: Active waypoint in route (foreign key to `waypoints.sequence_order`)

#### `waypoints` - Navigation Routes
Defines survey routes for each boat. Boats navigate between waypoints in sequence.

**Key Fields**:
- `boat_id` (FK to `boats.id`): Route owner
- `sequence_order`: Waypoint order (0-based index)
- `latitude`, `longitude`: Waypoint coordinates

### Repository Pattern

`BoatRepository` (in `EnergyBoatApp.ApiService/Repositories/`) provides clean data access:

```csharp
public interface IBoatRepository
{
    Task<IEnumerable<(Boat boat, BoatState state)>> GetAllBoatsWithStatesAsync();
    Task UpdateBoatStateAsync(BoatState state);
    Task<IEnumerable<Waypoint>> GetWaypointsForBoatAsync(string boatId);
    Task ResetAllBoatsAsync();
}
```

**Key Features**:
- **Npgsql** for high-performance PostgreSQL access
- **Connection Pooling**: MaxPoolSize=100, MinPoolSize=10 (handles 10x simulation speed)
- **OpenTelemetry Tracing**: All queries traced with boat_id tags
- **Error Handling**: Structured logging for slow queries (>100ms) and connection failures

### Performance Characteristics

Based on T028 performance benchmarks with Testcontainers:

| Operation | p95 Latency | Target SLA | Performance |
|-----------|-------------|------------|-------------|
| `GetAllBoatsWithStatesAsync()` | **3ms** | <200ms | **66x better** |
| `UpdateBoatStateAsync()` | **1ms** | <10ms | **34x better** |
| `GetWaypointsForBoatAsync()` | **2ms** | <20ms | **55x better** |
| **10x Load Simulation** (300 req, 10 concurrent) | **7ms** | <200ms | **28x better** |

**Concurrency Testing** (T029):
- ✅ No deadlocks under 5-second sustained 10x speed load (100 updates, 4 boats)
- ✅ Connection pool handles 100 concurrent updates without exhaustion
- ✅ 526 reads + 60 writes mixed workload with no conflicts

### Database Initialization

Handled automatically by `DatabaseInitializationService` and `SeedDataService` on startup:

1. **Schema Creation**: Tables, indexes, foreign keys created if missing
2. **Seed Data**: 4 production boats inserted if database empty:
   - **BOAT-001**: Contoso Sea Voyager (Dogger Bank geophysical survey)
   - **BOAT-002**: Contoso Sea Explorer (North Sea environmental monitoring)
   - **BOAT-003**: Contoso Sea Surveyor (Irish Sea cable route survey)
   - **BOAT-004**: Contoso Sea Monitor (German Bight renewable energy)
3. **Waypoints**: ~5 waypoints per boat auto-generated for route navigation

## Prerequisites

- **.NET 9 SDK**: Backend runtime
- **Aspire CLI 9.5.0**: `dotnet tool install -g aspire`
- **Docker Desktop**: PostgreSQL and pgAdmin containers
- **Node.js 20+**: Frontend build (auto-managed by Aspire)

## Quick Start

### With Aspire Orchestration (Recommended)

```powershell
# Configure PostgreSQL credentials (first time only)
cd EnergyBoatApp.AppHost
dotnet user-secrets set "Parameters:postgres-username" "admin"
dotnet user-secrets set "Parameters:postgres-password" "YourSecurePassword123!"

# Start all services (PostgreSQL, pgAdmin, API, Web)
aspire run
```

**Aspire automatically:**
- Pulls PostgreSQL 16 and pgAdmin 4 images (first run only)
- Creates persistent Docker volume for data
- Initializes schema and seeds 4 boats
- Opens dashboard at `http://localhost:15888`

**Endpoints:**
- **Frontend**: `http://localhost:5173` (React + Three.js)
- **API**: `https://localhost:7585/api/boats`
- **pgAdmin**: `http://localhost:5050` (Database UI)
- **Health Check**: `https://localhost:7585/health`

### Access Database via pgAdmin

1. Navigate to `http://localhost:5050`
2. Login: `admin@admin.com` / `admin`
3. Register Server:
   - Name: `ContosoSeaDB`
   - Host: `postgres` (Aspire service name)
   - Port: `5432`
   - Username: `admin` (from User Secrets)
   - Password: `YourSecurePassword123!`

### Manual Start (No Docker)

**Not recommended** - Aspire orchestration is the primary development workflow.

## Boat Status Color Coding

- 🟢 **Green** (>70%): Active with good energy
- � **Blue** (30-70%): Charging via solar panels
- 🔴 **Red** (<30%): Critical - switching to maintenance
- ⚪ **Gray**: Maintenance status (docked)

## API Endpoints

### GET /api/boats

Returns current boat fleet status from PostgreSQL database:

**Query Parameters:**
- `speed` (optional): Simulation speed multiplier (0.1-10.0, default 1.0)

**Example Request:**
```bash
curl https://localhost:7585/api/boats?speed=10.0
```

**Example Response:**
```json
[
  {
    "id": "BOAT-001",
    "latitude": 51.5074,
    "longitude": -0.1278,
    "status": "Active",
    "energyLevel": 85.5,
    "vesselName": "Contoso Sea Voyager",
    "surveyType": "Geophysical Survey",
    "project": "Dogger Bank Offshore Wind Farm",
    "equipment": "Multibeam Sonar, Magnetometer",
    "areaCovered": 125.3,
    "speed": "10 knots",
    "crewCount": 12,
    "conditions": "Good sea state",
    "heading": 245.8
  }
]
```

### POST /api/boats/reset

Resets all boats to initial waypoint positions in database.

**Example Request:**
```bash
curl -X POST https://localhost:7585/api/boats/reset
```

**Response:**
```json
{
  "message": "Boats reset to initial positions"
}
```

## Technology Stack

- **Backend**: C# 12, .NET 9, ASP.NET Core Minimal API
- **Database**: PostgreSQL 16 with Npgsql driver
- **Frontend**: React 19, Three.js 0.180, Vite 4.5
- **Orchestration**: .NET Aspire 9.5
- **Testing**: xUnit 2.9, Testcontainers.PostgreSql 4.1
- **Observability**: OpenTelemetry (traces, logs, metrics)
- **3D Graphics**: Three.js for WebGL rendering

## Development

### Database Schema Changes

Edit `EnergyBoatApp.ApiService/Services/DatabaseInitializationService.cs`:

```csharp
private async Task CreateSchemaAsync(NpgsqlConnection connection)
{
    var createTablesCommand = new NpgsqlCommand(@"
        CREATE TABLE IF NOT EXISTS boats (
            id VARCHAR(50) PRIMARY KEY,
            vessel_name VARCHAR(200) NOT NULL,
            -- Add new fields here
        );
    ", connection);
    
    await createTablesCommand.ExecuteNonQueryAsync();
}
```

### Adding New Boats

Edit `EnergyBoatApp.ApiService/Services/SeedDataService.cs`:

```csharp
private static readonly Boat[] ProductionBoats = [
    new("BOAT-001", "Contoso Sea Voyager", "Geophysical Survey", ...),
    new("BOAT-005", "Your New Boat", "Survey Type", ...), // Add here
];
```

### Customizing 3D Visualization

Edit `EnergyBoatApp.Web/src/components/BoatScene.jsx` to modify:
- Boat geometry and materials
- Camera positioning and movement
- Lighting and scene setup

## Testing

Run all tests (unit, integration, contract, performance):

```powershell
dotnet test
```

**Test Suites**:
- **Unit Tests**: Repository logic with mocked data source (15 tests)
- **Integration Tests**: Schema validation and CRUD operations with Testcontainers (6 tests)
- **Contract Tests**: API endpoint validation (10 tests)
- **Performance Tests**: Query latency under load (7 tests)
- **Concurrency Tests**: Deadlock prevention and connection pooling (4 tests)

**Total: 42 tests** covering database, API, and simulation logic.

## Building for Production

```bash
dotnet build EnergyBoatApp.sln --configuration Release
```

## Deployment

### Publish with Aspire (Recommended)

```powershell
# Generate deployment artifacts (Azure Container Apps, Kubernetes, etc.)
aspire publish

# Follow generated deployment guide in .aspire/manifest.json
```

### Manual Docker Deployment

**Frontend Container:**
```bash
cd EnergyBoatApp.Web
docker build -t energy-boat-web .
docker run -p 80:80 energy-boat-web
```

**Backend Container:**
```bash
cd EnergyBoatApp.ApiService
dotnet publish -c Release
docker build -t energy-boat-api .
docker run -p 7585:7585 energy-boat-api
```

## Observability

### OpenTelemetry Tracing

All database operations are traced with distributed tracing:

- **Traces**: View in Aspire dashboard (`http://localhost:15888` → Traces tab)
- **Tags**: Each query tagged with `boat_id` and `operation` for filtering
- **Slow Query Detection**: Warnings logged for queries >100ms

### Health Checks

Built-in health check endpoint verifies PostgreSQL connectivity:

```bash
curl https://localhost:7585/health
```

**Response:**
```json
{
  "status": "Healthy",
  "duration": "00:00:00.0123456",
  "entries": {
    "npgsql": {
      "status": "Healthy"
    }
  }
}
```

## Documentation

- **Quickstart Guide**: [`specs/001-migrate-from-mock/quickstart.md`](../specs/001-migrate-from-mock/quickstart.md)
- **API Contracts**: [`specs/001-migrate-from-mock/contracts/`](../specs/001-migrate-from-mock/contracts/)
- **Data Model**: [`specs/001-migrate-from-mock/data-model.md`](../specs/001-migrate-from-mock/data-model.md)
- **Migration Plan**: [`specs/001-migrate-from-mock/plan.md`](../specs/001-migrate-from-mock/plan.md)

## License

MIT License

## Notes

- **PostgreSQL-backed**: Persistent storage for boat state, waypoints, and simulation history
- **Research-driven**: Leverages official Microsoft docs, Npgsql best practices, and Three.js patterns
- **Performance-optimized**: Sub-10ms query latency with connection pooling (MaxPoolSize=100)
- **Test-driven**: 42 tests covering unit, integration, contract, performance, and concurrency
- **Observable**: OpenTelemetry tracing, structured logging, health checks built-in
- **Production-ready**: Aspire orchestration for simplified deployment to Azure/Kubernetes
