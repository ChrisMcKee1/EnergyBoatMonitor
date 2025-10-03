# Feature Specification: PostgreSQL Migration with Aspire Integration

**Feature Branch**: `001-migrate-from-mock`  
**Created**: October 3, 2025  
**Status**: Draft  
**Input**: User description: "migrate from mock data to PostgreSQL with Aspire integration for persistent boat data storage"

## Execution Flow (main)

```text
1. Parse user description from Input
   → SUCCESS: Clear requirement to migrate from in-memory mock data to PostgreSQL
2. Extract key concepts from description
   → Identified: Aspire integration, PostgreSQL persistent storage, boat data migration
3. For each unclear aspect:
   → ✅ Clear: Use .NET Aspire PostgreSQL integration
   → ✅ Clear: Replace mock data in BoatSimulator with database queries
   → ✅ Clear: Persist boat state, waypoints, routes
4. Fill User Scenarios & Testing section
   → SUCCESS: User flows identified for data persistence and retrieval
5. Generate Functional Requirements
   → SUCCESS: All requirements testable
6. Identify Key Entities
   → SUCCESS: Boat, BoatState, Waypoint, Route entities identified
7. Run Review Checklist
   → ✅ No [NEEDS CLARIFICATION] markers
   → ✅ No implementation details
8. Return: SUCCESS (spec ready for planning)
```

---

## ⚡ Quick Guidelines

- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers

---

## User Scenarios & Testing

### Primary User Story

As an **energy service company operator**, I need the boat monitoring system to **persist boat data across application restarts** so that I can **maintain continuity of operations, track historical positions, and not lose survey progress** when the system is restarted or updated.

### Acceptance Scenarios

1. **Given** the system is running with boats at various positions and statuses, **When** the application is stopped and restarted, **Then** all boats resume from their last known positions, energy levels, waypoint indices, and statuses without data loss.

2. **Given** a boat completes a survey waypoint and moves to the next waypoint, **When** the boat state is updated, **Then** the new position, heading, energy level, and waypoint index are immediately persisted to the database.

3. **Given** an operator resets boats to initial positions via the API, **When** the reset operation completes, **Then** all boats return to their starting positions and the database reflects these initial states.

4. **Given** boats are navigating predefined survey routes with waypoints, **When** a boat reaches a waypoint and transitions to the next one, **Then** the current waypoint index is updated in the database to maintain route continuity.

5. **Given** a boat's energy level drops below 20% and enters "Charging" status, **When** the status change occurs, **Then** the database reflects the new status, zero speed, and "Station keeping" speed description.

6. **Given** the application starts with an empty database, **When** the system initializes for the first time, **Then** initial boat data (4 boats with positions, routes, and metadata) is seeded into the database.

7. **Given** an operator queries the `/api/boats` endpoint, **When** the request is processed, **Then** the latest boat data is retrieved from the database, not from in-memory mock data.

### Edge Cases

- **What happens when the database connection fails?** System should log errors and return appropriate HTTP error responses (e.g., 503 Service Unavailable) rather than crashing or returning stale mock data.

- **How does the system handle concurrent updates to the same boat?** Database transactions should ensure consistency when multiple requests attempt to update boat positions simultaneously.

- **What happens when a boat's route references waypoints that don't exist in the database?** System should validate route integrity during initialization and log errors for missing waypoints, preventing boats from navigating invalid routes.

- **How does the system handle database schema migrations?** Application should detect missing tables/columns on startup and either auto-migrate or fail gracefully with clear error messages.

- **What happens when querying boat data with high speed multipliers (e.g., 10x)?** Database queries should remain performant and boat position updates should batch correctly to avoid overwhelming the database with writes.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST persist all boat state data (ID, latitude, longitude, status, energy level, heading, speed, crew count, area covered, equipment, project, survey type, conditions) to a PostgreSQL database.

- **FR-002**: System MUST persist boat route waypoints (latitude, longitude, sequence order) and associate them with specific boats.

- **FR-003**: System MUST update boat positions, headings, energy levels, and waypoint indices in the database each time the simulation step executes.

- **FR-004**: System MUST retrieve boat state and route data from the PostgreSQL database when serving API requests, replacing in-memory mock data.

- **FR-005**: System MUST seed initial boat data (4 boats: Contoso Sea Voyager, Contoso Sea Pioneer, Contoso Sea Navigator, Contoso Sea Explorer) with predefined routes and metadata when the database is empty on first run.

- **FR-006**: System MUST update boat status transitions (Active → Charging → Maintenance) in the database when energy level thresholds are crossed (below 20% for Charging, above 75% for resuming Active).

- **FR-007**: System MUST reset all boats to their initial positions, headings, energy levels, and statuses in the database when the `/api/boats/reset` endpoint is called.

- **FR-008**: System MUST use .NET Aspire Azure PostgreSQL Flexible Server integration for local development (container-based) and Azure-deployed environments.

- **FR-009**: System MUST authenticate to Azure PostgreSQL Flexible Server using Microsoft Entra ID (Azure Active Directory) when deployed to Azure, and use container-based PostgreSQL with password authentication for local development.

- **FR-010**: System MUST validate database connection health on application startup and fail gracefully with clear error messages if the database is unreachable.

- **FR-011**: System MUST maintain referential integrity between boats and their waypoint routes, ensuring routes cannot reference non-existent boats and waypoints cannot be orphaned.

- **FR-012**: System MUST support querying boat data with speed multipliers (0.1 to 10.0) without degrading database query performance or data consistency.

### Non-Functional Requirements

- **NFR-001**: Database queries for boat state retrieval MUST complete in less than 200ms at the 95th percentile.

- **NFR-002**: Boat position updates MUST be atomic (all-or-nothing) to prevent partial state corruption.

- **NFR-003**: System MUST handle up to 100 concurrent API requests for boat data without database connection pool exhaustion.

- **NFR-004**: Database schema MUST be version-controlled and support automated migrations during application startup.

- **NFR-005**: System MUST log all database errors with sufficient detail for troubleshooting (connection failures, query errors, transaction rollbacks).

### Key Entities

- **Boat**: Represents a survey vessel with metadata (ID, name, crew count, equipment, project, survey type).
  - Attributes: ID (string), VesselName (string), CrewCount (integer), Equipment (string), Project (string), SurveyType (string)
  - Relationships: Has one BoatState (current runtime state), has one Route (survey path)

- **BoatState**: Represents the current dynamic state of a boat.
  - Attributes: BoatId (foreign key), Latitude (double), Longitude (double), Heading (double), SpeedKnots (double), EnergyLevel (double), Status (string: Active/Charging/Maintenance), AreaCovered (double), Conditions (string), CurrentWaypointIndex (integer), OriginalSpeedKnots (double), LastUpdated (timestamp)
  - Relationships: Belongs to one Boat

- **Route**: Represents a predefined survey path for a boat.
  - Attributes: RouteId (unique identifier), BoatId (foreign key), RouteName (string)
  - Relationships: Belongs to one Boat, has many Waypoints (ordered sequence)

- **Waypoint**: Represents a geographic point in a survey route.
  - Attributes: WaypointId (unique identifier), RouteId (foreign key), Latitude (double), Longitude (double), SequenceOrder (integer)
  - Relationships: Belongs to one Route

---

## Dependencies & Assumptions

### Dependencies

- **.NET Aspire Azure PostgreSQL Integration**: Requires `Aspire.Hosting.Azure.PostgreSQL` NuGet package in AppHost and `Aspire.Azure.Npgsql` NuGet package in ApiService.

- **Npgsql Data Source**: Client integration uses `NpgsqlDataSource` for connection pooling and query execution.

- **Azure Provisioning (Optional)**: For Azure-deployed environments, requires Azure subscription configuration for automatic resource provisioning.

- **Database Schema Initialization**: Requires database migration strategy (e.g., EF Core Migrations, FluentMigrator, or SQL scripts).

### Assumptions

- **Aspire Orchestration**: Application will continue to use `aspire run` for local development and Aspire hosting integration patterns.

- **Backward Compatibility**: API contract (`BoatStatus` record) remains unchanged; frontend clients are not impacted by backend storage migration.

- **Development Environment**: Developers have Node.js 20+, .NET 9.0 SDK, and can run Docker containers (for local PostgreSQL via Aspire's `RunAsContainer()`).

- **Azure Entra ID**: Azure-deployed environments will have Azure AD authentication configured for PostgreSQL Flexible Server.

- **Seed Data**: Initial boat data (4 boats with routes) is considered application configuration, not user-generated data, and can be reset/reseeded without data loss concerns.

- **Single Database**: All boat data, routes, and waypoints reside in a single PostgreSQL database (not sharded or distributed).

---

## Success Criteria

- ✅ Boats persist positions, energy levels, and statuses across application restarts without data loss.
- ✅ API endpoints `/api/boats` and `/api/boats/reset` retrieve and update data from PostgreSQL, not in-memory mock data.
- ✅ Database connection uses Aspire's Azure PostgreSQL integration with proper service discovery.
- ✅ Local development uses containerized PostgreSQL via Aspire's `RunAsContainer()`.
- ✅ Azure-deployed environments use Azure PostgreSQL Flexible Server with Entra ID authentication.
- ✅ Database schema is version-controlled and auto-migrates on application startup.
- ✅ All existing frontend functionality (3D visualization, polling, boat movement simulation) continues to work without modification.

---

## Review & Acceptance Checklist

### Content Quality

- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

### Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous  
- [x] Success criteria are measurable
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

---

## Execution Status

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked (none found)
- [x] User scenarios defined
- [x] Requirements generated
- [x] Entities identified
- [x] Review checklist passed

---

## Out of Scope

The following items are explicitly **NOT** part of this feature:

- **Historical Data Analytics**: This feature does not include time-series historical position tracking, analytics dashboards, or reporting on past survey operations.

- **Multi-Tenant Support**: The database schema is single-tenant; no support for multiple organizations or isolated data partitions.

- **Real-Time Collaboration**: No support for multiple users viewing/editing boat data simultaneously with real-time conflict resolution.

- **External System Integration**: No integration with external maritime tracking systems (AIS, NMEA, etc.) or third-party APIs.

- **User Authentication/Authorization**: No user login system; API endpoints remain open (authentication is out of scope).

- **Database Replication/Sharding**: Single-instance PostgreSQL database; high availability, read replicas, or sharding are not included.

- **Frontend UI Changes**: No modifications to the React/Three.js frontend; API contract remains unchanged.

- **Performance Optimization Beyond Requirements**: Database indexing and query optimization will meet NFR-001 (200ms p95), but advanced tuning (caching, query profiling) is deferred.
