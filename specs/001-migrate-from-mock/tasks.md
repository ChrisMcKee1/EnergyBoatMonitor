# Tasks: PostgreSQL Migration with Aspire Integration

**Input**: Design documents from `/specs/001-migrate-from-mock/`  
**Prerequisites**: plan.md, research.md, data-model.md, contracts/, quickstart.md

## Execution Flow (main)

```text
1. Load plan.md from feature directory ✅
   → Tech stack: C# 12, .NET 9, Aspire 9.5, PostgreSQL 16, Npgsql
   → Structure: Web app (React frontend + C# backend)
2. Load optional design documents ✅
   → data-model.md: 4 entities (boats, boat_states, routes, waypoints)
   → contracts/: 2 files (get-boats.md, post-boats-reset.md)
   → research.md: Aspire PostgreSQL patterns validated
3. Generate tasks by category ✅
   → Setup: Aspire PostgreSQL integration, NpgsqlDataSource, User Secrets
   → Tests: 2 contract tests, 3 integration tests
   → Core: 4 database tables, repository pattern, simulation updates
   → Integration: DB initialization, seed data, API endpoints
   → Polish: cleanup, performance validation, documentation
4. Apply task rules ✅
   → Different files = [P] for parallel
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001-T034) ✅
6. Tasks ready for execution ✅
```

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions

- **Backend (C#)**: `EnergyBoatApp.ApiService/`
- **AppHost (Aspire)**: `EnergyBoatApp.AppHost/`
- **Tests**: `EnergyBoatApp.Tests/` (to be created)
- **Scripts**: `scripts/` (for SQL migrations)

---

## Phase 3.1: Setup & Infrastructure ✅ COMPLETE

### T001: ✅ Configure Aspire PostgreSQL Hosting

**File**: `EnergyBoatApp.AppHost/AppHost.cs`  
**Description**: Add `AddAzurePostgresFlexibleServer().RunAsContainer()` with `WithPgAdmin()` and `WithDataVolume()` following research.md patterns  
**Dependencies**: None  
**References**: research.md Section 1, plan.md Technical Context  
**Completed**: October 6, 2025 - Added Aspire.Hosting.Azure.PostgreSQL package, configured container with delegate pattern for pgAdmin and data volume

### T002: ✅ Configure User Secrets for PostgreSQL Credentials

**Command**: `dotnet user-secrets set "Parameters:postgres-username" "admin"` and `dotnet user-secrets set "Parameters:postgres-password" "YourSecurePassword123!"`  
**Description**: Initialize User Secrets for local PostgreSQL authentication as documented in quickstart.md  
**Dependencies**: None  
**References**: quickstart.md Step 2, research.md Section 3  
**Completed**: October 6, 2025 - Both secrets configured and verified in AppHost project

### T003: ✅ Add Npgsql Client Integration to ApiService

**File**: `EnergyBoatApp.ApiService/EnergyBoatApp.ApiService.csproj`  
**Description**: Add `Aspire.Npgsql` NuGet package (version 9.5.0) and configure `AddNpgsqlDataSource()` in Program.cs  
**Dependencies**: T001 (requires database reference from AppHost)  
**References**: research.md Section 2, plan.md Technical Context  
**Completed**: October 6, 2025 - Package added, AddNpgsqlDataSource configured with "ContosoSeaDB" connection name, AppHost reference wired

### T004: ✅ Create Database Migration SQL Script

**File**: `EnergyBoatApp.ApiService/Migrations/001-initial-schema.sql` (created in Migrations folder instead of scripts/)  
**Description**: Create SQL script with all 4 tables (boats, boat_states, routes, waypoints) following data-model.md schema exactly, including indexes  
**Dependencies**: None  
**References**: data-model.md Table Definitions, data-model.md Indexes & Performance Optimization  
**Completed**: October 6, 2025 - All 4 tables created with constraints, indexes, and comments

---

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3

**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**

### T005: [P] Create xUnit Test Project

**File**: `EnergyBoatApp.Tests/EnergyBoatApp.Tests.csproj`  
**Description**: Initialize xUnit test project with references to ApiService, Microsoft.AspNetCore.Mvc.Testing, and Aspire.Hosting.Testing packages  
**Dependencies**: None  
**References**: plan.md Constitution Check V

### T006: [P] Contract Test: GET /api/boats

**File**: `EnergyBoatApp.Tests/ContractTests/GetBoatsContractTests.cs`  
**Description**: Write contract test verifying GET /api/boats returns array of BoatStatus with all 13 required fields (Id, Latitude, Longitude, Status, EnergyLevel, VesselName, SurveyType, Project, Equipment, AreaCovered, Speed, CrewCount, Conditions, Heading) matching contracts/get-boats.md schema  
**Dependencies**: T005  
**References**: contracts/get-boats.md Response Specification, contracts/get-boats.md Field Specifications

### T007: [P] Contract Test: POST /api/boats/reset

**File**: `EnergyBoatApp.Tests/ContractTests/PostBoatsResetContractTests.cs`  
**Description**: Write contract test verifying POST /api/boats/reset returns ResetResponse with success=true, message, and boatsReset=4 matching contracts/post-boats-reset.md  
**Dependencies**: T005  
**References**: contracts/post-boats-reset.md Response Specification, contracts/post-boats-reset.md Response Schema

### T008: [P] Integration Test: Database Initialization

**File**: `EnergyBoatApp.Tests/IntegrationTests/DatabaseInitializationTests.cs`  
**Description**: Test that ContosoSeaDB schema is created on startup with all 4 tables, indexes, and constraints from data-model.md  
**Dependencies**: T005  
**References**: data-model.md Table Definitions, quickstart.md Step 4

### T009: [P] Integration Test: Seed Data Population

**File**: `EnergyBoatApp.Tests/IntegrationTests/SeedDataTests.cs`  
**Description**: Test that 4 boats (BOAT-001 to BOAT-004) are seeded with static metadata, initial states, routes, and waypoints from data-model.md sample data  
**Dependencies**: T005  
**References**: data-model.md Sample Data sections, plan.md Migration section

### T010: [P] Integration Test: Navigation Simulation Persistence

**File**: `EnergyBoatApp.Tests/IntegrationTests/NavigationPersistenceTests.cs`  
**Description**: Test that boat position/heading updates persist to database after simulation ticks and waypoint navigation logic works with database-backed routes  
**Dependencies**: T005  
**References**: plan.md Performance Goals, quickstart.md Verification Steps

---

## Phase 3.3: Core Implementation (ONLY after tests are failing)

### T011: [P] Create Boats Model

**File**: `EnergyBoatApp.ApiService/Models/Boat.cs`  
**Description**: Create C# record matching boats table schema (Id, VesselName, CrewCount, Equipment, Project, SurveyType) from data-model.md  
**Dependencies**: T006, T007 (tests must fail first)  
**References**: data-model.md Table 1: boats

### T012: [P] Create BoatState Model

**File**: `EnergyBoatApp.ApiService/Models/BoatState.cs`  
**Description**: Create C# record matching boat_states table (BoatId, Latitude, Longitude, Heading, SpeedKnots, OriginalSpeedKnots, EnergyLevel, Status, Speed, Conditions, AreaCovered, CurrentWaypointIndex, LastUpdated)  
**Dependencies**: T006, T007 (tests must fail first)  
**References**: data-model.md Table 2: boat_states

### T013: [P] Create Route Model

**File**: `EnergyBoatApp.ApiService/Models/Route.cs`  
**Description**: Create C# record matching routes table (BoatId, RouteName, CreatedAt)  
**Dependencies**: T006, T007 (tests must fail first)  
**References**: data-model.md Table 3: routes

### T014: [P] Create Waypoint Model

**File**: `EnergyBoatApp.ApiService/Models/Waypoint.cs`  
**Description**: Create C# record matching waypoints table (Id, BoatId, Latitude, Longitude, Sequence, CreatedAt)  
**Dependencies**: T006, T007 (tests must fail first)  
**References**: data-model.md Table 4: waypoints

### T015: Create IBoatRepository Interface

**File**: `EnergyBoatApp.ApiService/Repositories/IBoatRepository.cs`  
**Description**: Define async repository interface with methods: GetAllBoatsWithStatesAsync(), GetBoatByIdAsync(string id), UpdateBoatStateAsync(BoatState state), GetWaypointsForBoatAsync(string boatId), ResetAllBoatsAsync()  
**Dependencies**: T011, T012, T013, T014  
**References**: data-model.md Query Patterns, contracts/get-boats.md, contracts/post-boats-reset.md

### T016: Implement BoatRepository with NpgsqlDataSource

**File**: `EnergyBoatApp.ApiService/Repositories/BoatRepository.cs`  
**Description**: Implement IBoatRepository using NpgsqlDataSource with direct SQL queries for all CRUD operations, connection pooling, and proper async/await patterns  
**Dependencies**: T015  
**References**: research.md Section 2, data-model.md Query Patterns

### T017: Create Database Initialization Service

**File**: `EnergyBoatApp.ApiService/Services/DatabaseInitializationService.cs`  
**Description**: Create IHostedService that runs 001-initial-schema.sql on startup if tables don't exist, implements idempotent schema creation  
**Dependencies**: T004, T016  
**References**: quickstart.md Step 4, plan.md Migration section

### T018: Create Seed Data Service

**File**: `EnergyBoatApp.ApiService/Services/SeedDataService.cs`  
**Description**: Create service that populates boats, boat_states, routes, and waypoints tables with 4 initial boats if database is empty, using sample data from data-model.md  
**Dependencies**: T017  
**References**: data-model.md Sample Data, plan.md Migration

### T019: Update BoatSimulator to Use Repository

**File**: `EnergyBoatApp.ApiService/Program.cs`  
**Description**: Refactor UpdateBoatPositions() method to read waypoints from database via IBoatRepository, update boat_states table instead of in-memory dictionary, maintain existing Haversine navigation logic  
**Dependencies**: T016, T018  
**References**: plan.md Summary, contracts/get-boats.md Behavioral Contracts

### T020: Implement GET /api/boats with Database Query

**File**: `EnergyBoatApp.ApiService/Program.cs`  
**Description**: Replace in-memory _boatStates.Values with IBoatRepository.GetAllBoatsWithStatesAsync(), maintain exact BoatStatus record contract from contracts/get-boats.md, handle speed multiplier parameter  
**Dependencies**: T019  
**References**: contracts/get-boats.md Request/Response Specification

### T021: Implement POST /api/boats/reset with Database Transaction

**File**: `EnergyBoatApp.ApiService/Program.cs`  
**Description**: Replace in-memory reset logic with IBoatRepository.ResetAllBoatsAsync(), return ResetResponse matching contracts/post-boats-reset.md, use database transaction for atomicity  
**Dependencies**: T019  
**References**: contracts/post-boats-reset.md Behavioral Contracts

### T022: Add Health Checks for PostgreSQL

**File**: `EnergyBoatApp.ApiService/Program.cs`  
**Description**: Configure Aspire health checks for NpgsqlDataSource, add /health endpoint returning database connection status  
**Dependencies**: T003  
**References**: research.md Section 2, plan.md Technical Context (Aspire observability)

---

## Phase 3.4: Integration & Observability

### T023: Configure OpenTelemetry for Database Operations

**File**: `EnergyBoatApp.ApiService/Program.cs`  
**Description**: Verify Aspire auto-instrumentation captures Npgsql traces, add custom spans for repository methods if needed, validate traces appear in Aspire dashboard  
**Dependencies**: T020, T021  
**References**: plan.md Technical Context (OpenTelemetry)

### T024: Add Database Query Logging

**File**: `EnergyBoatApp.ApiService/Repositories/BoatRepository.cs`  
**Description**: Add structured logging for slow queries (>100ms), connection pool exhaustion, and transaction failures using ILogger<BoatRepository>  
**Dependencies**: T016  
**References**: plan.md Performance Goals (<200ms p95)

### T025: Implement Connection Resilience

**File**: `EnergyBoatApp.ApiService/Program.cs`  
**Description**: Configure NpgsqlDataSource with retry policy (Polly), connection timeout (30s), command timeout (60s), max pool size (100)  
**Dependencies**: T003  
**References**: data-model.md Performance Requirements, plan.md Constraints (10x speed support)

### T026: Verify pgAdmin Access

**File**: Manual verification  
**Description**: Start application with `aspire run`, verify pgAdmin accessible at http://localhost:5050, can connect to ContosoSeaDB using credentials from User Secrets, can view all 4 tables with data  
**Dependencies**: T001, T018  
**References**: quickstart.md Step 5, research.md Section 1

---

## Phase 3.5: Polish & Documentation

### T027: [P] Add Unit Tests for BoatRepository

**File**: `EnergyBoatApp.Tests/UnitTests/BoatRepositoryTests.cs`  
**Description**: Write unit tests for each repository method using in-memory PostgreSQL test container (Testcontainers.PostgreSQL), verify SQL correctness, connection handling, and error cases  
**Dependencies**: T016  
**References**: plan.md Constitution Check V (TDD)

### T028: [P] Performance Test: Query Latency

**File**: `EnergyBoatApp.Tests/PerformanceTests/QueryLatencyTests.cs`  
**Description**: Benchmark GET /api/boats under 10x simulation speed (30 req/min), verify p95 latency <200ms, identify slow queries with EXPLAIN ANALYZE  
**Dependencies**: T020  
**References**: plan.md Performance Goals, data-model.md Performance Requirements

### T029: [P] Performance Test: Concurrent Updates

**File**: `EnergyBoatApp.Tests/PerformanceTests/ConcurrentUpdateTests.cs`  
**Description**: Test 4 boats updating simultaneously every 200ms (10x speed), verify no deadlocks, connection pool handles load, all updates committed  
**Dependencies**: T021  
**References**: plan.md Constraints (10x simulation speed), data-model.md Transaction Strategy

### T030: Remove Mock Data Code

**File**: `EnergyBoatApp.ApiService/Program.cs`  
**Description**: Delete in-memory dictionaries (_boatStates, _boatRoutes), remove mock data initialization, clean up unused BoatSimulator fields  
**Dependencies**: T020, T021  
**References**: plan.md Summary (migrate FROM in-memory mock data)

### T031: Update API Documentation

**File**: `EnergyBoatApp.ApiService/EnergyBoatApp.ApiService.http`  
**Description**: Add examples showing database-backed responses, update comments to reference PostgreSQL instead of mock data  
**Dependencies**: T020, T021  
**References**: contracts/get-boats.md, contracts/post-boats-reset.md

### T032: [P] Update Quickstart Guide

**File**: `specs/001-migrate-from-mock/quickstart.md`  
**Description**: Verify all steps in quickstart.md work end-to-end, update screenshots/output samples if needed, add troubleshooting section for common database connection issues  
**Dependencies**: T026  
**References**: quickstart.md all sections

### T033: [P] Update README Documentation

**File**: `EnergyBoatApp/README.md`  
**Description**: Add "Database Architecture" section explaining PostgreSQL schema, update "Running the Application" to mention database seeding, document User Secrets setup  
**Dependencies**: T001, T002  
**References**: plan.md Technical Context, data-model.md Overview

### T034: Run Manual Verification Checklist

**File**: Manual verification following quickstart.md  
**Description**: Execute full quickstart guide from clean state (delete Docker volumes, reset User Secrets), verify: (1) PostgreSQL container starts, (2) Schema created automatically, (3) 4 boats seeded, (4) Navigation works at 1x and 10x speed, (5) Reset endpoint works, (6) pgAdmin shows correct data, (7) Aspire dashboard shows database metrics  
**Dependencies**: T032  
**References**: quickstart.md Verification Steps

---

## Dependencies

### Critical Path

```text
T001 (Aspire PostgreSQL) → T003 (Npgsql Client) → T025 (Resilience)
                                                  ↓
T004 (SQL Schema) → T017 (Init Service) → T018 (Seed Data) → T019 (Update Simulator)
                                                                ↓
T005 (Test Project) → T006-T010 (Tests MUST FAIL) → T011-T014 (Models) → T015-T016 (Repository)
                                                                                      ↓
                                                              T020 (GET) ← ← ← ← ← ←
                                                                 ↓
                                                              T021 (POST)
                                                                 ↓
                                                              T022-T026 (Integration)
                                                                 ↓
                                                              T027-T034 (Polish)
```

### Parallel Groups

**Group 1 (Setup)**:
- T001, T002 (can run together - different tools)

**Group 2 (Tests - MUST RUN BEFORE IMPLEMENTATION)**:
- T006, T007, T008, T009, T010 (different test files)

**Group 3 (Models)**:
- T011, T012, T013, T014 (different model files)

**Group 4 (Polish)**:
- T027, T028, T029, T032, T033 (different files)

---

## Parallel Execution Examples

### Setup Phase

```powershell
# Configure Aspire and User Secrets in parallel (different projects)
Task: "Configure Aspire PostgreSQL in EnergyBoatApp.AppHost/AppHost.cs"
Task: "Set User Secrets for PostgreSQL credentials via dotnet user-secrets"
```

### Test Writing Phase (TDD - BEFORE Implementation)

```powershell
# Write all contract and integration tests in parallel
Task: "Contract test GET /api/boats in EnergyBoatApp.Tests/ContractTests/GetBoatsContractTests.cs"
Task: "Contract test POST /api/boats/reset in EnergyBoatApp.Tests/ContractTests/PostBoatsResetContractTests.cs"
Task: "Integration test database init in EnergyBoatApp.Tests/IntegrationTests/DatabaseInitializationTests.cs"
Task: "Integration test seed data in EnergyBoatApp.Tests/IntegrationTests/SeedDataTests.cs"
Task: "Integration test navigation persistence in EnergyBoatApp.Tests/IntegrationTests/NavigationPersistenceTests.cs"
```

### Model Creation Phase (AFTER Tests Fail)

```powershell
# Create all model files in parallel
Task: "Create Boat model in EnergyBoatApp.ApiService/Models/Boat.cs"
Task: "Create BoatState model in EnergyBoatApp.ApiService/Models/BoatState.cs"
Task: "Create Route model in EnergyBoatApp.ApiService/Models/Route.cs"
Task: "Create Waypoint model in EnergyBoatApp.ApiService/Models/Waypoint.cs"
```

### Polish Phase

```powershell
# Polish tasks that touch different files
Task: "Unit tests for BoatRepository in EnergyBoatApp.Tests/UnitTests/BoatRepositoryTests.cs"
Task: "Performance test query latency in EnergyBoatApp.Tests/PerformanceTests/QueryLatencyTests.cs"
Task: "Performance test concurrent updates in EnergyBoatApp.Tests/PerformanceTests/ConcurrentUpdateTests.cs"
Task: "Update quickstart.md verification steps"
Task: "Update EnergyBoatApp/README.md database architecture section"
```

---

## Notes

### Critical Rules

- **TDD Enforcement**: T006-T010 (tests) MUST be completed and FAILING before starting T011-T021 (implementation)
- **No Manual Builds**: Aspire handles all build/dependency management (no `npm install`, `dotnet build`)
- **User Secrets Required**: T002 must complete before T026 (pgAdmin access) or application won't start
- **Schema First**: T004 (SQL script) must exist before T017 (initialization service)
- **Database-Backed Only**: T030 removes all mock data - no fallback to in-memory after this task

### API Contract Guarantees

- **GET /api/boats**: Response format UNCHANGED from current implementation (frontend sees no difference)
- **POST /api/boats/reset**: Response format UNCHANGED (testing tools see no difference)
- **Speed Multiplier**: Works identically with database backend (dynamic threshold still applied)

### Performance Targets

- **p95 latency**: <200ms for GET /api/boats (validate in T028)
- **10x simulation speed**: No database bottlenecks (validate in T029)
- **Connection pool**: Supports 4 boats updating every 200ms simultaneously

### Validation Gates

- [ ] All 5 tests (T006-T010) written and FAILING before any implementation
- [ ] All 4 models (T011-T014) created matching data-model.md exactly
- [ ] Both endpoints (T020, T021) maintain exact API contracts
- [ ] Performance tests (T028, T029) pass without degradation
- [ ] Manual verification (T034) checklist 100% complete

### Rollback Strategy

If migration fails, revert to mock data by:

1. Keep `Program.cs` backup with in-memory dictionaries
2. Comment out NpgsqlDataSource registration
3. Restore original BoatSimulator initialization
4. Frontend continues working (API contract unchanged)

---

## Constitution Compliance

This task list follows all architectural guardrails from plan.md:

✅ **Separation of Concerns**: All tasks are backend-only (database layer), zero frontend changes required  
✅ **Atomic Design**: No 3D component tasks (migration is backend-only)  
✅ **Research-First**: Tasks reference research.md findings (T001, T003, T016)  
✅ **Aspire Orchestration**: Uses Aspire patterns (T001, T003, T022)  
✅ **Test-Driven Development**: Tests (T006-T010) explicitly before implementation (T011-T021)  
✅ **Architectural Constraints**: Maintains tech stack (C# 12, .NET 9, Aspire 9.5), performance goals, no new frameworks

**Total Tasks**: 34  
**Estimated Duration**: 3-5 days (solo developer) or 1-2 days (team with parallel execution)  
**Risk Level**: Low (API contract unchanged, incremental migration, comprehensive testing)
