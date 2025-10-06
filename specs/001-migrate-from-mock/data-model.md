# Data Model: PostgreSQL Schema Design

**Feature**: 001-migrate-from-mock  
**Date**: October 3, 2025  
**Phase**: Phase 1 - Design & Contracts

---

## Overview

This document defines the PostgreSQL database schema for persistent boat data storage. The schema directly maps to the existing in-memory `BoatSimulator` data structures, ensuring zero API contract changes.

**Database Name**: `ContosoSeaDB`  
**Schema**: `public` (PostgreSQL default)  
**Character Encoding**: UTF-8  
**Collation**: en_US.UTF-8

---

## Entity Relationship Diagram

```text
┌─────────────────┐         ┌──────────────────┐
│     boats       │────1:1──│   boat_states    │
│                 │         │                  │
│ PK: id          │         │ PK: boat_id (FK) │
│     vessel_name │         │     latitude     │
│     crew_count  │         │     longitude    │
│     equipment   │         │     heading      │
│     project     │         │     ...          │
│     survey_type │         └──────────────────┘
└─────────────────┘
        │
        │ 1:1
        ▼
┌─────────────────┐         ┌──────────────────┐
│     routes      │────1:N──│   waypoints      │
│                 │         │                  │
│ PK: boat_id (FK)│         │ PK: id           │
│     route_name  │         │ FK: boat_id      │
└─────────────────┘         │     latitude     │
                            │     longitude    │
                            │     sequence     │
                            └──────────────────┘
```

**Relationships**:
- `boats` 1:1 `boat_states` (one-to-one via `boat_id` FK)
- `boats` 1:1 `routes` (one-to-one via `boat_id` FK)
- `routes` 1:N `waypoints` (one-to-many via `boat_id` FK)

---

## Table Definitions

### 1. `boats` (Static Vessel Metadata)

Stores immutable vessel information (name, equipment, crew size, project).

```sql
CREATE TABLE boats (
    id              VARCHAR(20) PRIMARY KEY,  -- e.g., "BOAT-001"
    vessel_name     VARCHAR(100) NOT NULL,    -- e.g., "Contoso Sea Voyager"
    crew_count      INT NOT NULL CHECK (crew_count > 0),
    equipment       TEXT NOT NULL,            -- e.g., "Multibeam Sonar, Magnetometer"
    project         VARCHAR(255) NOT NULL,    -- e.g., "Dogger Bank Offshore Wind Farm"
    survey_type     VARCHAR(100) NOT NULL,    -- e.g., "Geophysical Survey"
    
    created_at      TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Indexes
CREATE INDEX idx_boats_vessel_name ON boats(vessel_name);
```

**Sample Data**:
| id       | vessel_name              | crew_count | equipment                          | project                             | survey_type        |
|----------|--------------------------|------------|------------------------------------|------------------------------------|-------------------|
| BOAT-001 | Contoso Sea Voyager      | 24         | Multibeam Sonar, Magnetometer      | Dogger Bank Offshore Wind Farm      | Geophysical Survey |
| BOAT-002 | Contoso Sea Pioneer      | 18         | ROV, Side-scan Sonar               | Subsea Cable Route Survey           | ROV Operations     |
| BOAT-003 | Contoso Sea Navigator    | 22         | CPT, Seabed Sampling               | North Sea Pipeline Inspection       | Geotechnical Survey|
| BOAT-004 | Contoso Sea Explorer     | 12         | Multibeam, Sub-bottom Profiler     | Scheduled Maintenance               | Standby            |

---

### 2. `boat_states` (Dynamic Runtime State)

Stores current position, heading, energy, status. **Updated on every simulation tick** (every 2 seconds at 1x speed).

```sql
CREATE TABLE boat_states (
    boat_id                 VARCHAR(20) PRIMARY KEY REFERENCES boats(id) ON DELETE CASCADE,
    
    -- Geographic position
    latitude                DOUBLE PRECISION NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude               DOUBLE PRECISION NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    heading                 DOUBLE PRECISION NOT NULL CHECK (heading BETWEEN 0 AND 360),
    
    -- Speed and energy
    speed_knots             DOUBLE PRECISION NOT NULL CHECK (speed_knots >= 0),
    original_speed_knots    DOUBLE PRECISION NOT NULL CHECK (original_speed_knots >= 0),
    energy_level            DOUBLE PRECISION NOT NULL CHECK (energy_level BETWEEN 0 AND 100),
    
    -- Status and conditions
    status                  VARCHAR(20) NOT NULL CHECK (status IN ('Active', 'Charging', 'Maintenance')),
    speed                   VARCHAR(50) NOT NULL,      -- Human-readable (e.g., "12 knots", "Station keeping")
    conditions              VARCHAR(255) NOT NULL,     -- e.g., "Moderate seas, 2m swell"
    area_covered            DOUBLE PRECISION DEFAULT 0 CHECK (area_covered >= 0),
    
    -- Navigation state
    current_waypoint_index  INT NOT NULL DEFAULT 0 CHECK (current_waypoint_index >= 0),
    
    -- Audit
    last_updated            TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Indexes
CREATE INDEX idx_boat_states_status ON boat_states(status);
CREATE INDEX idx_boat_states_energy ON boat_states(energy_level);
CREATE INDEX idx_boat_states_updated ON boat_states(last_updated DESC);
```

**Sample Data** (initial state):
| boat_id  | latitude  | longitude | heading | speed_knots | energy_level | status | current_waypoint_index |
|----------|-----------|-----------|---------|-------------|--------------|--------|------------------------|
| BOAT-001 | 51.5074   | -0.1278   | 45.0    | 12.0        | 85.5         | Active | 0                      |
| BOAT-002 | 51.5154   | -0.1420   | 0.0     | 0.0         | 42.3         | Charging | 0                    |
| BOAT-003 | 51.5010   | -0.1200   | 135.0   | 8.0         | 91.2         | Active | 0                      |
| BOAT-004 | 51.5090   | -0.1390   | 315.0   | 0.0         | 15.7         | Maintenance | 0                 |

---

### 3. `routes` (Survey Path Metadata)

Stores route metadata for each boat. One route per boat (1:1 relationship).

```sql
CREATE TABLE routes (
    boat_id     VARCHAR(20) PRIMARY KEY REFERENCES boats(id) ON DELETE CASCADE,
    route_name  VARCHAR(100) NOT NULL,  -- e.g., "Rectangle Pattern - NE Quadrant"
    
    created_at  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- No additional indexes needed (boat_id is PK)
```

**Sample Data**:
| boat_id  | route_name                          |
|----------|-------------------------------------|
| BOAT-001 | Rectangle Pattern - NE Quadrant     |
| BOAT-002 | Zigzag Pattern - NW Quadrant        |
| BOAT-003 | Triangle Pattern - South Quadrant   |
| BOAT-004 | Docked Position (Maintenance)       |

---

### 4. `waypoints` (Geographic Survey Points)

Stores ordered waypoints for each route. Boats navigate through waypoints in `sequence` order (looping back to 0).

```sql
CREATE TABLE waypoints (
    id          SERIAL PRIMARY KEY,
    boat_id     VARCHAR(20) NOT NULL REFERENCES boats(id) ON DELETE CASCADE,
    latitude    DOUBLE PRECISION NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude   DOUBLE PRECISION NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    sequence    INT NOT NULL CHECK (sequence >= 0),
    
    created_at  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE (boat_id, sequence)  -- Ensure no duplicate sequence numbers per boat
);

-- Indexes
CREATE INDEX idx_waypoints_boat_sequence ON waypoints(boat_id, sequence);
```

**Sample Data** (BOAT-001 rectangle pattern):
| id  | boat_id  | latitude  | longitude | sequence |
|-----|----------|-----------|-----------|----------|
| 1   | BOAT-001 | 51.5170   | -0.1278   | 0        |
| 2   | BOAT-001 | 51.5250   | -0.1000   | 1        |
| 3   | BOAT-001 | 51.5250   | -0.1400   | 2        |
| 4   | BOAT-001 | 51.5170   | -0.1400   | 3        |
| 5   | BOAT-001 | 51.5170   | -0.1278   | 4        |

---

## Indexes & Performance Optimization

### Index Strategy

| Index Name | Table | Columns | Purpose |
|------------|-------|---------|---------|
| `idx_boats_vessel_name` | `boats` | `vessel_name` | Fast vessel name lookups (if needed) |
| `idx_boat_states_status` | `boat_states` | `status` | Filter by Active/Charging/Maintenance |
| `idx_boat_states_energy` | `boat_states` | `energy_level` | Find boats with low energy |
| `idx_boat_states_updated` | `boat_states` | `last_updated DESC` | Audit/debugging stale data |
| `idx_waypoints_boat_sequence` | `waypoints` | `(boat_id, sequence)` | Navigate waypoints in order |

### Query Performance Targets

| Query Type | Target Latency | Expected Volume |
|------------|----------------|-----------------|
| Get all boat states | <50ms | Every 2 seconds (frontend polling) |
| Update single boat state | <10ms | Every simulation tick (4 boats × 2 seconds) |
| Get waypoints for boat | <20ms | On status transition (occasional) |
| Seed initial data | <500ms | Once on first run |

### Connection Pooling

- `NpgsqlDataSource` default pool size: 100 connections
- Expected concurrent load: ~30 queries/minute baseline
- Peak load (10x speed, 100 concurrent users): ~3000 queries/minute
- Pool utilization: ~5-10 connections active (well below limit)

---

## Data Migration Strategy

### Initial Seed (First Run)

**Trigger**: Application startup detects empty `boats` table

**Process**:
1. Check if `boats` table is empty: `SELECT COUNT(*) FROM boats`
2. If empty, insert seed data in transaction:
   - 4 boat records (`boats`)
   - 4 boat state records (`boat_states`)
   - 4 route records (`routes`)
   - ~15-20 waypoint records (`waypoints`)

**Seed Data Location**: `EnergyBoatApp.ApiService/Migrations/SeedData.sql`

**Implementation**:
```csharp
// DatabaseInitializer.cs
public async Task SeedInitialDataAsync(NpgsqlDataSource dataSource)
{
    await using var connection = await dataSource.OpenConnectionAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    
    try
    {
        // Check if already seeded
        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM boats";
        var count = (long)(await countCommand.ExecuteScalarAsync() ?? 0);
        
        if (count == 0)
        {
            // Run seed script
            var seedScript = await File.ReadAllTextAsync("Migrations/SeedData.sql");
            var command = connection.CreateCommand();
            command.CommandText = seedScript;
            await command.ExecuteNonQueryAsync();
        }
        
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### Reset to Initial State

**Trigger**: User calls `/api/boats/reset` endpoint

**Process**:
1. Update `boat_states` with initial positions/headings/energy
2. Reset `current_waypoint_index` to 0 for all boats

**No need to reset**:
- `boats` (static data)
- `routes` (static data)
- `waypoints` (static data)

---

## Validation Rules

### Data Integrity Constraints

| Field | Constraint | Rationale |
|-------|------------|-----------|
| `boat_states.latitude` | BETWEEN -90 AND 90 | Geographic validity |
| `boat_states.longitude` | BETWEEN -180 AND 180 | Geographic validity |
| `boat_states.heading` | BETWEEN 0 AND 360 | Nautical degrees (0=North, 90=East) |
| `boat_states.energy_level` | BETWEEN 0 AND 100 | Percentage |
| `boat_states.status` | IN ('Active', 'Charging', 'Maintenance') | Enum validation |
| `boats.crew_count` | > 0 | Cannot have zero crew |
| `waypoints.sequence` | >= 0 | Non-negative index |

### Referential Integrity

- `boat_states.boat_id` → `boats.id` (CASCADE DELETE)
- `routes.boat_id` → `boats.id` (CASCADE DELETE)
- `waypoints.boat_id` → `boats.id` (CASCADE DELETE)

**Rationale for CASCADE**: Deleting a boat should remove all associated state, route, and waypoints.

---

## Schema Evolution (Future)

### Potential Extensions (Out of Scope for v1)

1. **Historical Tracking**: Add `boat_state_history` table for position tracking over time
2. **Multi-Tenant**: Add `organization_id` FK for fleet management
3. **Alarms/Alerts**: Add `alerts` table for low energy notifications
4. **Audit Log**: Add `audit_log` table for tracking manual resets

### Backward Compatibility

Schema changes MUST:
- Preserve existing column names (API contract depends on them)
- Add new columns with defaults (no breaking changes)
- Use migrations (versioned SQL scripts)

---

## SQL Migration Script

**File**: `EnergyBoatApp.ApiService/Migrations/001_InitialSchema.sql`

```sql
-- Migration 001: Initial Schema
-- Date: 2025-10-03
-- Description: Create tables for boat data persistence

BEGIN;

-- 1. Boats (static vessel metadata)
CREATE TABLE boats (
    id              VARCHAR(20) PRIMARY KEY,
    vessel_name     VARCHAR(100) NOT NULL,
    crew_count      INT NOT NULL CHECK (crew_count > 0),
    equipment       TEXT NOT NULL,
    project         VARCHAR(255) NOT NULL,
    survey_type     VARCHAR(100) NOT NULL,
    created_at      TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_boats_vessel_name ON boats(vessel_name);

-- 2. Boat States (dynamic runtime state)
CREATE TABLE boat_states (
    boat_id                 VARCHAR(20) PRIMARY KEY REFERENCES boats(id) ON DELETE CASCADE,
    latitude                DOUBLE PRECISION NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude               DOUBLE PRECISION NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    heading                 DOUBLE PRECISION NOT NULL CHECK (heading BETWEEN 0 AND 360),
    speed_knots             DOUBLE PRECISION NOT NULL CHECK (speed_knots >= 0),
    original_speed_knots    DOUBLE PRECISION NOT NULL CHECK (original_speed_knots >= 0),
    energy_level            DOUBLE PRECISION NOT NULL CHECK (energy_level BETWEEN 0 AND 100),
    status                  VARCHAR(20) NOT NULL CHECK (status IN ('Active', 'Charging', 'Maintenance')),
    speed                   VARCHAR(50) NOT NULL,
    conditions              VARCHAR(255) NOT NULL,
    area_covered            DOUBLE PRECISION DEFAULT 0 CHECK (area_covered >= 0),
    current_waypoint_index  INT NOT NULL DEFAULT 0 CHECK (current_waypoint_index >= 0),
    last_updated            TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_boat_states_status ON boat_states(status);
CREATE INDEX idx_boat_states_energy ON boat_states(energy_level);
CREATE INDEX idx_boat_states_updated ON boat_states(last_updated DESC);

-- 3. Routes (survey path metadata)
CREATE TABLE routes (
    boat_id     VARCHAR(20) PRIMARY KEY REFERENCES boats(id) ON DELETE CASCADE,
    route_name  VARCHAR(100) NOT NULL,
    created_at  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 4. Waypoints (geographic survey points)
CREATE TABLE waypoints (
    id          SERIAL PRIMARY KEY,
    boat_id     VARCHAR(20) NOT NULL REFERENCES boats(id) ON DELETE CASCADE,
    latitude    DOUBLE PRECISION NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude   DOUBLE PRECISION NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    sequence    INT NOT NULL CHECK (sequence >= 0),
    created_at  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (boat_id, sequence)
);

CREATE INDEX idx_waypoints_boat_sequence ON waypoints(boat_id, sequence);

COMMIT;
```

---

## Testing Considerations

### Schema Validation Tests

1. **Constraint Tests**:
   - Insert invalid latitude (91) → expect failure
   - Insert invalid energy (-10) → expect failure
   - Insert duplicate waypoint sequence → expect failure

2. **Referential Integrity Tests**:
   - Delete boat → verify cascading delete of state/route/waypoints
   - Insert boat_state with non-existent boat_id → expect failure

3. **Index Performance Tests**:
   - Query all boat states (should use sequential scan, table is small)
   - Query waypoints by boat_id (should use index)

---

*Data Model Complete - Ready for Contract Generation*
