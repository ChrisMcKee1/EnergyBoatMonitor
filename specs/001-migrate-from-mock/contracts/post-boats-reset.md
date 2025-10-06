# API Contract: POST /api/boats/reset

**Feature**: 001-migrate-from-mock  
**Date**: October 3, 2025  
**Phase**: Phase 1 - Design & Contracts

---

## Overview

This document defines the API contract for the boat reset endpoint. This contract **MUST NOT CHANGE** during the PostgreSQL migration.

**Endpoint**: `POST /api/boats/reset`  
**Purpose**: Reset all boats to their initial positions, headings, and energy levels  
**Consumer**: Frontend reset button (future), testing, development  
**Use Cases**: Testing navigation, resetting simulation, debugging

---

## Request Specification

### HTTP Method & Path

```http
POST /api/boats/reset HTTP/1.1
Host: localhost:{port}
Content-Length: 0
```

### Request Body

**None** - This endpoint accepts no request body.

### Headers

| Header | Value | Required |
|--------|-------|----------|
| `Content-Length` | `0` | Yes (HTTP spec) |

---

## Response Specification

### Success Response (200 OK)

**Content-Type**: `application/json`

**Body**:

```json
{
  "success": true,
  "message": "All boats reset to initial state",
  "boatsReset": 4
}
```

### Response Schema

**C# Record Definition**:

```csharp
public record ResetResponse(
    bool Success,
    string Message,
    int BoatsReset
);
```

**TypeScript Interface**:

```typescript
interface ResetResponse {
  success: boolean;
  message: string;
  boatsReset: number;
}
```

### Field Specifications

| Field | Type | Nullable | Description | Example |
|-------|------|----------|-------------|---------|
| `success` | boolean | No | Indicates reset operation completed | `true` |
| `message` | string | No | Human-readable confirmation | `"All boats reset to initial state"` |
| `boatsReset` | number | No | Number of boats reset | `4` |

---

## Behavioral Contracts

### Reset Operations

The reset endpoint performs the following database operations:

#### 1. Reset Boat States

Updates `boat_states` table with initial values:

| Boat ID | Latitude | Longitude | Heading | Speed (knots) | Energy | Status | Waypoint Index |
|---------|----------|-----------|---------|---------------|--------|--------|----------------|
| BOAT-001 | 51.5074 | -0.1278 | 45.0 | 12.0 | 85.5 | Active | 0 |
| BOAT-002 | 51.5154 | -0.1420 | 0.0 | 0.0 | 42.3 | Charging | 0 |
| BOAT-003 | 51.5010 | -0.1200 | 135.0 | 8.0 | 91.2 | Active | 0 |
| BOAT-004 | 51.5090 | -0.1390 | 315.0 | 0.0 | 15.7 | Maintenance | 0 |

#### 2. Reset Derived Fields

- `area_covered` → `0.0` for all boats
- `current_waypoint_index` → `0` for all boats
- `last_updated` → `CURRENT_TIMESTAMP` for all boats

#### 3. Preserve Static Data

**NO changes** to these tables:

- `boats` (vessel metadata remains unchanged)
- `routes` (route definitions remain unchanged)
- `waypoints` (waypoint coordinates remain unchanged)

### Atomicity Guarantee

The reset operation MUST be atomic:

- Either ALL boats reset successfully OR none reset
- Use database transaction with rollback on failure
- Return error if any boat fails to update

---

## Database Implementation

### SQL Pattern (Repository)

```csharp
public async Task<int> ResetAllBoatsAsync()
{
    await using var connection = await _dataSource.OpenConnectionAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    
    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE boat_states
            SET 
                latitude = CASE boat_id
                    WHEN 'BOAT-001' THEN 51.5074
                    WHEN 'BOAT-002' THEN 51.5154
                    WHEN 'BOAT-003' THEN 51.5010
                    WHEN 'BOAT-004' THEN 51.5090
                END,
                longitude = CASE boat_id
                    WHEN 'BOAT-001' THEN -0.1278
                    WHEN 'BOAT-002' THEN -0.1420
                    WHEN 'BOAT-003' THEN -0.1200
                    WHEN 'BOAT-004' THEN -0.1390
                END,
                heading = CASE boat_id
                    WHEN 'BOAT-001' THEN 45.0
                    WHEN 'BOAT-002' THEN 0.0
                    WHEN 'BOAT-003' THEN 135.0
                    WHEN 'BOAT-004' THEN 315.0
                END,
                speed_knots = CASE boat_id
                    WHEN 'BOAT-001' THEN 12.0
                    WHEN 'BOAT-002' THEN 0.0
                    WHEN 'BOAT-003' THEN 8.0
                    WHEN 'BOAT-004' THEN 0.0
                END,
                original_speed_knots = speed_knots,
                energy_level = CASE boat_id
                    WHEN 'BOAT-001' THEN 85.5
                    WHEN 'BOAT-002' THEN 42.3
                    WHEN 'BOAT-003' THEN 91.2
                    WHEN 'BOAT-004' THEN 15.7
                END,
                status = CASE boat_id
                    WHEN 'BOAT-001' THEN 'Active'
                    WHEN 'BOAT-002' THEN 'Charging'
                    WHEN 'BOAT-003' THEN 'Active'
                    WHEN 'BOAT-004' THEN 'Maintenance'
                END,
                speed = CASE boat_id
                    WHEN 'BOAT-001' THEN '12 knots'
                    WHEN 'BOAT-002' THEN 'Station keeping'
                    WHEN 'BOAT-003' THEN '8 knots'
                    WHEN 'BOAT-004' THEN 'Docked'
                END,
                area_covered = 0.0,
                current_waypoint_index = 0,
                last_updated = CURRENT_TIMESTAMP";
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
        
        return rowsAffected;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## Error Responses

### Database Connection Failure (500)

**Scenario**: PostgreSQL unavailable, connection timeout

```json
{
  "success": false,
  "error": "Failed to connect to database",
  "message": "Connection timeout after 30 seconds"
}
```

### Transaction Rollback (500)

**Scenario**: Partial update failed, transaction rolled back

```json
{
  "success": false,
  "error": "Reset operation failed",
  "message": "Transaction rolled back due to constraint violation"
}
```

### Concurrent Modification Conflict (409)

**Scenario**: Another process is updating boat states simultaneously

```json
{
  "success": false,
  "error": "Conflict during reset",
  "message": "Another operation is in progress. Please retry."
}
```

---

## Performance Requirements

| Metric | Target | Rationale |
|--------|--------|-----------|
| Response Time | <100ms | Simple UPDATE query on 4 rows |
| Transaction Isolation | Read Committed | Prevent dirty reads during reset |
| Retry Logic | 3 retries with exponential backoff | Handle transient database errors |

---

## Frontend Integration

### Current Implementation Pattern

**File**: `EnergyBoatApp.Web/src/components/SceneControls.jsx` (future addition)

```javascript
const handleReset = async () => {
  try {
    const response = await fetch('/api/boats/reset', {
      method: 'POST',
      headers: { 'Content-Length': '0' }
    });
    
    if (!response.ok) {
      throw new Error(`Reset failed: ${response.statusText}`);
    }
    
    const result = await response.json();
    console.log(`Reset ${result.boatsReset} boats`);
    
    // Trigger immediate refresh (don't wait for next poll)
    refreshBoats();
  } catch (error) {
    console.error('Failed to reset boats:', error);
    alert('Failed to reset simulation. Please try again.');
  }
};
```

---

## Contract Testing Strategy

### Test Cases

**File**: `EnergyBoatApp.ApiService.Tests/ApiContractTests.cs`

```csharp
[Fact]
public async Task ResetBoats_ReturnsSuccessWithCorrectCount()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.PostAsync("/api/boats/reset", null);
    
    // Assert
    response.EnsureSuccessStatusCode();
    
    var result = await response.Content.ReadFromJsonAsync<ResetResponse>();
    Assert.NotNull(result);
    Assert.True(result.Success);
    Assert.Equal(4, result.BoatsReset);
    Assert.Contains("reset", result.Message, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public async Task ResetBoats_ActuallyResetsBoatStates()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Modify a boat state first
    // ... (implementation depends on test database setup)
    
    // Act - Reset
    var resetResponse = await client.PostAsync("/api/boats/reset", null);
    resetResponse.EnsureSuccessStatusCode();
    
    // Act - Verify reset
    var getResponse = await client.GetAsync("/api/boats");
    var boats = await getResponse.Content.ReadFromJsonAsync<List<BoatStatus>>();
    
    // Assert - Verify BOAT-001 is at initial position
    var boat1 = boats.FirstOrDefault(b => b.Id == "BOAT-001");
    Assert.NotNull(boat1);
    Assert.Equal(51.5074, boat1.Latitude, precision: 4);
    Assert.Equal(-0.1278, boat1.Longitude, precision: 4);
    Assert.Equal(45.0, boat1.Heading, precision: 1);
    Assert.Equal(85.5, boat1.EnergyLevel, precision: 1);
    Assert.Equal("Active", boat1.Status);
}

[Fact]
public async Task ResetBoats_IsAtomic_RollbackOnFailure()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Simulate database constraint violation (test-specific setup)
    // ...
    
    // Act
    var response = await client.PostAsync("/api/boats/reset", null);
    
    // Assert - Expect failure
    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    
    // Assert - Verify NO boats were reset (atomicity)
    var getResponse = await client.GetAsync("/api/boats");
    var boats = await getResponse.Content.ReadFromJsonAsync<List<BoatStatus>>();
    
    // Verify boats are still in pre-reset state
    // ...
}
```

---

## Side Effects

### Simulation State Impact

After calling `/api/boats/reset`:

1. **Simulation continues**: Background timer keeps running
2. **Boats resume from initial positions**: Next simulation tick updates from reset state
3. **Energy tracking resets**: Area covered resets to 0, energy levels restored
4. **Waypoint navigation resets**: All boats restart from waypoint 0

### Observable Changes

The next `GET /api/boats` call after reset will return:

- All boats at initial lat/lon coordinates
- All boats at initial headings (BOAT-001: 45°, BOAT-002: 0°, etc.)
- All boats with initial energy levels (BOAT-001: 85.5%, BOAT-002: 42.3%, etc.)
- All `current_waypoint_index` values reset to 0
- All `area_covered` values reset to 0

---

## Security Considerations

### Authorization (Future)

Currently, the endpoint is **unauthenticated** for development purposes.

**Production considerations**:

- Add authorization header requirement
- Restrict to admin users only
- Log all reset operations for audit trail
- Rate limit to prevent abuse (max 1 reset per minute)

### Input Validation

No input validation required (no request body).

---

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-03 | Migration Team | Initial contract definition |

---

**Contract Status**: ✅ **LOCKED** - No changes allowed during migration
