# API Contract: GET /api/boats

**Feature**: 001-migrate-from-mock  
**Date**: October 3, 2025  
**Phase**: Phase 1 - Design & Contracts

---

## Overview

This document defines the API contract for the boat status endpoint. This contract **MUST NOT CHANGE** during the PostgreSQL migration to ensure frontend compatibility.

**Endpoint**: `GET /api/boats`  
**Purpose**: Fetch current state of all boats in the fleet  
**Consumer**: React frontend (BoatScene.jsx)  
**Polling Frequency**: Every 2 seconds

---

## Request Specification

### HTTP Method & Path

```http
GET /api/boats?speed={speedMultiplier} HTTP/1.1
Host: localhost:{port}
```

### Query Parameters

| Parameter | Type | Required | Default | Validation | Description |
|-----------|------|----------|---------|------------|-------------|
| `speed` | `float` | No | `1.0` | 0.1 ≤ speed ≤ 10.0 | Simulation speed multiplier |

**Examples**:
- Normal speed: `GET /api/boats` or `GET /api/boats?speed=1.0`
- 10x speed: `GET /api/boats?speed=10.0`
- Slow motion: `GET /api/boats?speed=0.5`

### Headers

| Header | Value | Required |
|--------|-------|----------|
| `Accept` | `application/json` | No (default) |

---

## Response Specification

### Success Response (200 OK)

**Content-Type**: `application/json`

**Body**: Array of `BoatStatus` objects

```json
[
  {
    "Id": "BOAT-001",
    "Latitude": 51.5074,
    "Longitude": -0.1278,
    "Status": "Active",
    "EnergyLevel": 85.5,
    "VesselName": "Contoso Sea Voyager",
    "SurveyType": "Geophysical Survey",
    "Project": "Dogger Bank Offshore Wind Farm",
    "Equipment": "Multibeam Sonar, Magnetometer",
    "AreaCovered": 12.34,
    "Speed": "12 knots",
    "CrewCount": 24,
    "Conditions": "Good sea state, 2m swell",
    "Heading": 45.0
  },
  {
    "Id": "BOAT-002",
    "Latitude": 51.5154,
    "Longitude": -0.1420,
    "Status": "Charging",
    "EnergyLevel": 42.3,
    "VesselName": "Contoso Sea Pioneer",
    "SurveyType": "ROV Operations",
    "Project": "Subsea Cable Route Survey",
    "Equipment": "ROV, Side-scan Sonar",
    "AreaCovered": 8.67,
    "Speed": "Station keeping",
    "CrewCount": 18,
    "Conditions": "Moderate seas",
    "Heading": 0.0
  },
  {
    "Id": "BOAT-003",
    "Latitude": 51.5010,
    "Longitude": -0.1200,
    "Status": "Active",
    "EnergyLevel": 91.2,
    "VesselName": "Contoso Sea Navigator",
    "SurveyType": "Geotechnical Survey",
    "Project": "North Sea Pipeline Inspection",
    "Equipment": "CPT, Seabed Sampling",
    "AreaCovered": 15.21,
    "Speed": "8 knots",
    "CrewCount": 22,
    "Conditions": "Good conditions",
    "Heading": 135.0
  },
  {
    "Id": "BOAT-004",
    "Latitude": 51.5090,
    "Longitude": -0.1390,
    "Status": "Maintenance",
    "EnergyLevel": 15.7,
    "VesselName": "Contoso Sea Explorer",
    "SurveyType": "Standby",
    "Project": "Scheduled Maintenance",
    "Equipment": "Multibeam, Sub-bottom Profiler",
    "AreaCovered": 0.0,
    "Speed": "Docked",
    "CrewCount": 12,
    "Conditions": "Docked at base",
    "Heading": 315.0
  }
]
```

### BoatStatus Schema

**C# Record Definition** (backend):

```csharp
public record BoatStatus(
    string Id,              // Boat identifier (e.g., "BOAT-001")
    double Latitude,        // Current latitude (-90 to 90)
    double Longitude,       // Current longitude (-180 to 180)
    string Status,          // "Active" | "Charging" | "Maintenance"
    double EnergyLevel,     // Battery percentage (0-100)
    string VesselName,      // Human-readable vessel name
    string SurveyType,      // Type of survey operation
    string Project,         // Current project name
    string Equipment,       // Comma-separated equipment list
    double AreaCovered,     // Square kilometers surveyed
    string Speed,           // Human-readable speed (e.g., "12 knots")
    int CrewCount,          // Number of crew members
    string Conditions,      // Weather/sea conditions description
    double Heading          // Nautical heading in degrees (0-360, 0=North)
);
```

**TypeScript Interface** (frontend):

```typescript
interface BoatStatus {
  Id: string;
  Latitude: number;
  Longitude: number;
  Status: 'Active' | 'Charging' | 'Maintenance';
  EnergyLevel: number;
  VesselName: string;
  SurveyType: string;
  Project: string;
  Equipment: string;
  AreaCovered: number;
  Speed: string;
  CrewCount: number;
  Conditions: string;
  Heading: number;
}
```

### Field Specifications

| Field | Type | Nullable | Validation | Example | Notes |
|-------|------|----------|------------|---------|-------|
| `Id` | string | No | Non-empty, matches `BOAT-\d{3}` | `"BOAT-001"` | Unique identifier |
| `Latitude` | number | No | -90 ≤ lat ≤ 90 | `51.5074` | Decimal degrees |
| `Longitude` | number | No | -180 ≤ lon ≤ 180 | `-0.1278` | Decimal degrees |
| `Status` | string | No | Enum: Active, Charging, Maintenance | `"Active"` | Current operational status |
| `EnergyLevel` | number | No | 0 ≤ energy ≤ 100 | `85.5` | Battery percentage |
| `VesselName` | string | No | Non-empty | `"Contoso Sea Voyager"` | Display name |
| `SurveyType` | string | No | Non-empty | `"Geophysical Survey"` | Operation type |
| `Project` | string | No | Non-empty | `"Dogger Bank Offshore Wind Farm"` | Current project |
| `Equipment` | string | No | Non-empty | `"Multibeam Sonar, Magnetometer"` | Comma-separated list |
| `AreaCovered` | number | No | ≥ 0 | `12.34` | Square kilometers |
| `Speed` | string | No | Non-empty | `"12 knots"` | Human-readable speed |
| `CrewCount` | number | No | > 0 | `24` | Integer count |
| `Conditions` | string | No | Non-empty | `"Good sea state, 2m swell"` | Weather description |
| `Heading` | number | No | 0 ≤ heading < 360 | `45.0` | Nautical degrees (0=North, 90=East) |

---

## Behavioral Contracts

### Status Transition Rules

1. **Active → Charging**: When `EnergyLevel` drops below 30%
2. **Charging → Active**: When `EnergyLevel` reaches 70% during charging
3. **Active → Maintenance**: When `EnergyLevel` drops below 20%
4. **Maintenance → Charging**: When `EnergyLevel` reaches 30% after maintenance
5. **Charging → Maintenance**: When `EnergyLevel` drops below 20% during charging

**Diagram**:

```text
  Active (>30%)
     ↓ (energy < 30%)
  Charging (30-70%)
     ↓ (energy < 20%)
  Maintenance (<20%)
     ↓ (energy reaches 30%)
  Charging (30-70%)
     ↓ (energy reaches 70%)
  Active (>70%)
```

### Speed Multiplier Effects

| Multiplier | Description | Expected Behavior |
|------------|-------------|-------------------|
| 0.1 | Slow motion | Boats move 10% normal speed, energy drains slower |
| 1.0 | Real-time | Normal simulation speed |
| 10.0 | Fast forward | Boats move 10x speed, reach waypoints faster |

**Implementation Detail**: Speed multiplier affects:
- Position update delta (distance traveled per tick)
- Waypoint detection threshold (dynamically adjusted)
- Does NOT affect energy drain rate (drains based on distance, not time)

### Energy Management

- **Drain Rate**: 0.1% per nautical mile traveled
- **Charge Rate**: +10% per simulation tick while Status = "Charging"
- **Maintenance**: Energy fixed at current level until maintenance complete

### Navigation Behavior

- **Waypoint Detection**: Haversine distance < `0.15 + (distanceTraveled * 1.5)` nautical miles
- **Heading Update**: Calculated using bearing formula from current position to next waypoint
- **Route Looping**: After reaching last waypoint, reset to waypoint 0

---

## Error Responses

### Invalid Speed Parameter (400 Bad Request)

**Scenario**: Speed parameter outside valid range (0.1-10.0)

```json
{
  "error": "Invalid speed parameter. Must be between 0.1 and 10.0",
  "parameter": "speed",
  "value": 15.0
}
```

### Internal Server Error (500)

**Scenario**: Database connection failure, query timeout, or unexpected exception

```json
{
  "error": "Failed to retrieve boat data",
  "message": "Connection to database failed"
}
```

---

## Performance Requirements (NFR-001)

| Metric | Target | Rationale |
|--------|--------|-----------|
| Response Time (p50) | <50ms | Maintain smooth 2-second polling |
| Response Time (p95) | <200ms | 95% of requests complete quickly |
| Response Time (p99) | <500ms | Tolerate occasional spikes |
| Payload Size | ~2KB | 4 boats × ~500 bytes each |

---

## Frontend Integration

### Current Implementation Pattern

**File**: `EnergyBoatApp.Web/src/App.jsx`

```javascript
// Polling every 2 seconds
useEffect(() => {
  const interval = setInterval(async () => {
    const response = await fetch(`/api/boats?speed=${speedMultiplier}`);
    const data = await response.json();
    setBoats(data); // Triggers BoatScene.jsx re-render
  }, 2000);
  
  return () => clearInterval(interval);
}, [speedMultiplier]);
```

**File**: `EnergyBoatApp.Web/src/components/BoatScene.jsx`

```javascript
// Handles both PascalCase (C#) and camelCase (JavaScript) field names
useEffect(() => {
  boats.forEach(boat => {
    const latitude = boat.latitude || boat.Latitude;
    const longitude = boat.longitude || boat.Longitude;
    const heading = boat.heading || boat.Heading || 0;
    
    // Convert to scene coordinates
    const boatPos = CoordinateConverter.latLonToScene({ latitude, longitude });
    boatMesh.position.set(boatPos.x, 0, boatPos.z);
    boatMesh.rotation.y = headingToRotation(heading);
  });
}, [boats]);
```

---

## Database-to-API Mapping

### Query Pattern (Repository)

```csharp
public async Task<IEnumerable<BoatStatus>> GetAllBoatStatusesAsync()
{
    await using var connection = await _dataSource.OpenConnectionAsync();
    await using var command = connection.CreateCommand();
    
    command.CommandText = @"
        SELECT 
            b.id,
            bs.latitude,
            bs.longitude,
            bs.status,
            bs.energy_level,
            b.vessel_name,
            b.survey_type,
            b.project,
            b.equipment,
            bs.area_covered,
            bs.speed,
            b.crew_count,
            bs.conditions,
            bs.heading
        FROM boats b
        INNER JOIN boat_states bs ON b.id = bs.boat_id
        ORDER BY b.id";
    
    await using var reader = await command.ExecuteReaderAsync();
    var boats = new List<BoatStatus>();
    
    while (await reader.ReadAsync())
    {
        boats.Add(new BoatStatus(
            Id: reader.GetString(0),
            Latitude: reader.GetDouble(1),
            Longitude: reader.GetDouble(2),
            Status: reader.GetString(3),
            EnergyLevel: reader.GetDouble(4),
            VesselName: reader.GetString(5),
            SurveyType: reader.GetString(6),
            Project: reader.GetString(7),
            Equipment: reader.GetString(8),
            AreaCovered: reader.GetDouble(9),
            Speed: reader.GetString(10),
            CrewCount: reader.GetInt32(11),
            Conditions: reader.GetString(12),
            Heading: reader.GetDouble(13)
        ));
    }
    
    return boats;
}
```

### Column-to-Field Mapping

| Database Column | BoatStatus Field | Table |
|-----------------|------------------|-------|
| `boats.id` | `Id` | boats |
| `boat_states.latitude` | `Latitude` | boat_states |
| `boat_states.longitude` | `Longitude` | boat_states |
| `boat_states.status` | `Status` | boat_states |
| `boat_states.energy_level` | `EnergyLevel` | boat_states |
| `boats.vessel_name` | `VesselName` | boats |
| `boats.survey_type` | `SurveyType` | boats |
| `boats.project` | `Project` | boats |
| `boats.equipment` | `Equipment` | boats |
| `boat_states.area_covered` | `AreaCovered` | boat_states |
| `boat_states.speed` | `Speed` | boat_states |
| `boats.crew_count` | `CrewCount` | boats |
| `boat_states.conditions` | `Conditions` | boat_states |
| `boat_states.heading` | `Heading` | boat_states |

---

## Contract Testing Strategy

### Test Cases

**File**: `EnergyBoatApp.ApiService.Tests/ApiContractTests.cs`

```csharp
[Fact]
public async Task GetBoats_ReturnsExpectedSchemaAndCount()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/boats");
    
    // Assert
    response.EnsureSuccessStatusCode();
    
    var boats = await response.Content.ReadFromJsonAsync<List<BoatStatus>>();
    Assert.NotNull(boats);
    Assert.Equal(4, boats.Count);
    
    var boat = boats.First();
    Assert.NotNull(boat.Id);
    Assert.InRange(boat.Latitude, -90, 90);
    Assert.InRange(boat.Longitude, -180, 180);
    Assert.Contains(boat.Status, new[] { "Active", "Charging", "Maintenance" });
    Assert.InRange(boat.EnergyLevel, 0, 100);
    Assert.InRange(boat.Heading, 0, 360);
}

[Theory]
[InlineData(0.1)]
[InlineData(1.0)]
[InlineData(10.0)]
public async Task GetBoats_WithValidSpeed_ReturnsSuccess(double speed)
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync($"/api/boats?speed={speed}");
    
    // Assert
    response.EnsureSuccessStatusCode();
}

[Fact]
public async Task GetBoats_WithInvalidSpeed_ReturnsBadRequest()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/boats?speed=15.0");
    
    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

---

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-03 | Migration Team | Initial contract definition |

---

**Contract Status**: ✅ **LOCKED** - No changes allowed during migration
