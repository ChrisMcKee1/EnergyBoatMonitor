
# Implementation Plan: PostgreSQL Migration with Aspire Integration

**Branch**: `001-migrate-from-mock` | **Date**: October 3, 2025 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/001-migrate-from-mock/spec.md`  
**Database Name**: `ContosoSeaDB`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary

**Primary Requirement**: Migrate Energy Boat Monitor from in-memory mock data (stored in `BoatSimulator` class) to persistent PostgreSQL storage using .NET Aspire integration.

**Technical Approach**:
- **Hosting**: Use `AddAzurePostgresFlexibleServer()` with `RunAsContainer()` for local development
- **Client**: Use `AddNpgsqlDataSource()` for direct SQL queries (no EF Core - maintains current architecture)
- **Database**: `ContosoSeaDB` with tables for Boats, BoatStates, Routes, and Waypoints
- **Security**: Secrets managed via Aspire `AddParameter()` with `secret: true` flag and User Secrets
- **Tooling**: Include pgAdmin via `WithPgAdmin()` for database management
- **Migration**: Seed 4 existing boats with their routes on first run

## Technical Context

**Language/Version**: C# 12, .NET 9.0  
**Primary Dependencies**: 
- `Aspire.Hosting.PostgreSQL 9.5.0` (AppHost)
- `Aspire.Npgsql 9.5.0` (ApiService)
- `Npgsql 9.x` (PostgreSQL client)

**Storage**: PostgreSQL 16 (containerized locally, Azure Flexible Server in cloud)  
**Testing**: xUnit for contract/integration tests, NpgsqlDataSource for test fixtures  
**Target Platform**: Cross-platform (Windows/Linux/macOS via Docker)  
**Project Type**: Web application (React frontend + C# backend)  
**Performance Goals**: <200ms p95 for boat state queries, 60 FPS 3D rendering  
**Constraints**: 
- No EF Core (keep lightweight, direct SQL approach)
- Backward compatible API contract (frontend unaffected)
- Must support 10x simulation speed without database bottlenecks

**Scale/Scope**:
- 4 boats initially (can scale to hundreds)
- ~50-100 waypoints total across all routes
- 2-second polling interval = ~30 queries/minute baseline

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Separation of Concerns (Frontend ↔ Backend)

- [x] Backend owns: simulation logic, navigation algorithms, state management
- [x] Frontend owns: visualization, user interactions, coordinate transformations
- [x] Clean REST API contract with strongly-typed records (`BoatStatus` unchanged)
- [x] No coordinate transformation in backend (Haversine distance only)
- [x] No waypoint/navigation logic in frontend (retrieves from API)

**Status**: ✅ PASS - Database migration is purely backend concern. API contract (`BoatStatus` record) remains unchanged, so frontend sees no difference between mock data and database-backed data.

### II. Atomic Design for 3D Components

- [x] 3D components follow hierarchy: Atoms → Molecules → Organisms → Templates
- [x] Scene templates only position/rotate, never modify geometry
- [x] Components use local coordinates, not absolute world coordinates
- [x] Bottom-up changes (atoms modified, molecules/organisms inherit)

**Status**: ✅ PASS - No 3D component changes required. Database migration affects only backend data layer.

### III. Research-First Development

- [x] Microsoft technologies researched via Microsoft MCP ✅ Completed
  - Aspire PostgreSQL integration patterns
  - `AddParameter()` for secrets management
  - `WithPgAdmin()` for pgAdmin container
- [x] Third-party libraries researched via Context7 (N/A - using Microsoft stack)
- [x] All technical decisions backed by official documentation
- [x] No guessed API signatures or patterns

**Status**: ✅ PASS - Research completed using Microsoft Learn docs for Aspire PostgreSQL, NpgsqlDataSource, and parameter management.

### IV. Aspire Orchestration

- [x] All services orchestrated via Aspire AppHost
- [x] Using `AddNpmApp()` for frontend, `AddProject<T>()` for backend
- [x] Service discovery via Aspire (no hardcoded URLs)
- [x] No manual `npm install`, `dotnet build`, or Docker commands
- [x] PostgreSQL added via `AddAzurePostgresFlexibleServer().RunAsContainer()`
- [x] Secrets managed via `AddParameter("username", secret: true)`

**Status**: ✅ PASS - Using Aspire's built-in PostgreSQL hosting integration with proper parameter/secret management.

### V. Test-Driven Development

- [ ] Tests written before implementation
- [ ] Contract tests for all API endpoints
- [ ] Integration tests for service communication
- [ ] All tests pass before code review

**Status**: ⚠️ PENDING - Tests will be defined in Phase 1 (Design & Contracts) and implemented in Phase 2 (Task Execution).

### Architectural Constraints

- [x] Tech stack: C# 12, .NET 9, React 19, Three.js 0.180, Aspire 9.5
- [x] Coordinate system: Scene origin = Dock location (51.5100, -0.1350) - unchanged
- [x] Performance: 60 FPS (3D), <200ms p95 (API) - database queries must meet p95 target
- [x] No new frameworks without architecture review - using Aspire + Npgsql (approved)

**Status**: ✅ PASS - All constraints respected.

**Constitution Compliance**: PASS (with TDD pending Phase 1) | **Violations**: None

## Project Structure

### Documentation (this feature)

```text
specs/001-migrate-from-mock/
├── spec.md              # Feature specification (complete)
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0 output (research findings)
├── data-model.md        # Phase 1 output (database schema)
├── quickstart.md        # Phase 1 output (developer onboarding)
├── contracts/           # Phase 1 output (API contracts - unchanged from spec)
│   └── api-boats-contract.md
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)

```text
EnergyBoatApp/
├── EnergyBoatApp.ApiService/          # C# API (MODIFIED)
│   ├── Program.cs                     # MODIFY: Replace BoatSimulator with database queries
│   ├── Data/                          # NEW: Database layer
│   │   ├── BoatRepository.cs         # NEW: Data access for boats
│   │   ├── WaypointRepository.cs     # NEW: Data access for waypoints
│   │   └── DatabaseInitializer.cs    # NEW: Seed data on first run
│   ├── Models/                        # NEW: Database models
│   │   ├── Boat.cs                   # NEW: Entity model
│   │   ├── BoatState.cs              # NEW: Entity model
│   │   ├── Route.cs                  # NEW: Entity model
│   │   └── Waypoint.cs               # NEW: Entity model
│   └── Migrations/                    # NEW: SQL migration scripts
│       └── 001_InitialSchema.sql     # NEW: Create tables
│
├── EnergyBoatApp.AppHost/             # Aspire orchestration (MODIFIED)
│   └── AppHost.cs                     # MODIFY: Add PostgreSQL + pgAdmin
│
├── EnergyBoatApp.ServiceDefaults/     # Unchanged
│
└── EnergyBoatApp.Web/                 # React frontend (UNCHANGED)
    └── [No changes - API contract preserved]
```

**Structure Decision**: **Web application** with React frontend + C# backend. Database migration affects only backend:

- **Modified**: `EnergyBoatApp.ApiService/Program.cs` - Replace `BoatSimulator` with repository pattern
- **Modified**: `EnergyBoatApp.AppHost/AppHost.cs` - Add PostgreSQL resource with pgAdmin
- **New**: Database layer (`Data/`, `Models/`, `Migrations/`) in ApiService project
- **Unchanged**: Frontend (React/Three.js) - no code changes required

## Phase 0: Outline & Research

**Status**: ✅ COMPLETE

**Research Completed**:
- Aspire PostgreSQL hosting integration (`AddAzurePostgresFlexibleServer`, `RunAsContainer`)
- NpgsqlDataSource client integration patterns
- Aspire secrets management (`AddParameter(secret: true)`, User Secrets)
- pgAdmin container integration (`WithPgAdmin()`)
- Data persistence with Docker volumes (`WithDataVolume()`)

**Output**: 
- ✅ `research.md` created with 5 technology decisions documented
- ✅ All technology choices backed by Microsoft Learn official documentation
- ✅ No NEEDS CLARIFICATION markers remain

## Phase 1: Design & Contracts

**Status**: ✅ COMPLETE

**Design Artifacts Created**:

1. **Data Model** (`data-model.md`):
   - 4 PostgreSQL tables: `boats`, `boat_states`, `routes`, `waypoints`
   - Entity relationships: boats 1:1 boat_states, boats 1:1 routes, routes 1:N waypoints
   - Schema with validation constraints (lat/lon ranges, energy 0-100, status enum)
   - Indexes for performance: boat_id lookups, status filtering, waypoint sequencing
   - Migration script `001_InitialSchema.sql` ready for execution
   - Seed data strategy: detect empty `boats` table, run `SeedData.sql` in transaction

2. **API Contracts** (`contracts/`):
   - `get-boats.md`: GET /api/boats endpoint contract (LOCKED - no changes allowed)
   - `post-boats-reset.md`: POST /api/boats/reset endpoint contract (LOCKED)
   - Schema validation: `BoatStatus` record with 14 fields
   - Column-to-field mapping documented for database queries
   - Performance requirements: <200ms p95, <50ms p50

3. **Developer Onboarding** (`quickstart.md`):
   - 10-step setup guide: prerequisites, user secrets, Aspire run, pgAdmin access, API testing
   - Troubleshooting section: PostgreSQL container issues, npm install failures, data persistence
   - Verification steps: seed data queries, API endpoint testing, frontend visualization
   - Clean up instructions: stop Aspire, remove volumes

**Contract Tests to Implement** (Phase 2 tasks):

```csharp
// ApiService.Tests/ApiContractTests.cs

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
    var client = _factory.CreateClient();
    var response = await client.GetAsync($"/api/boats?speed={speed}");
    response.EnsureSuccessStatusCode();
}

[Fact]
public async Task ResetBoats_ReturnsSuccessWithCorrectCount()
{
    var client = _factory.CreateClient();
    var response = await client.PostAsync("/api/boats/reset", null);
    response.EnsureSuccessStatusCode();
    
    var result = await response.Content.ReadFromJsonAsync<ResetResponse>();
    Assert.NotNull(result);
    Assert.True(result.Success);
    Assert.Equal(4, result.BoatsReset);
}

[Fact]
public async Task ResetBoats_ActuallyResetsBoatStates()
{
    var client = _factory.CreateClient();
    
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
```

**Output**:
- ✅ `data-model.md` (database schema with 4 tables, indexes, constraints, migrations)
- ✅ `contracts/get-boats.md` (GET /api/boats contract - LOCKED)
- ✅ `contracts/post-boats-reset.md` (POST /api/boats/reset contract - LOCKED)
- ✅ `quickstart.md` (10-step developer onboarding guide)
- ✅ Contract test specifications (to be implemented in Phase 2)

## Phase 2: Task Planning Approach

**Note**: This section describes what the /tasks command will do. DO NOT execute during /plan.

### Task Generation Strategy

The `/tasks` command will:

1. **Load Template**: Use `.specify/templates/tasks-template.md` as base structure

2. **Generate Test Tasks** (from Phase 1 contracts):
   - Task 1-3: Write failing contract tests for GET /api/boats (schema, count, speed parameter)
   - Task 4-5: Write failing contract tests for POST /api/boats/reset (success response, actual state reset)
   - **Mark [P]**: All test tasks can run in parallel (independent test files)

3. **Generate Model Tasks** (from data-model.md):
   - Task 6-9: Create entity models (`Boat.cs`, `BoatState.cs`, `Route.cs`, `Waypoint.cs`)
   - **Mark [P]**: All model tasks can run in parallel (no dependencies)

4. **Generate Infrastructure Tasks** (from research.md):
   - Task 10: Add `Aspire.Hosting.PostgreSQL` NuGet package to AppHost
   - Task 11: Add `Aspire.Npgsql` NuGet package to ApiService
   - Task 12: Configure PostgreSQL resource in `AppHost.cs` with `RunAsContainer()`, `WithPgAdmin()`, `WithDataVolume()`
   - Task 13: Configure User Secrets for postgres username/password
   - Task 14: Add `AddNpgsqlDataSource()` to ApiService `Program.cs`

5. **Generate Database Tasks** (from data-model.md):
   - Task 15: Create `Migrations/001_InitialSchema.sql` with table DDL
   - Task 16: Create `Migrations/SeedData.sql` with initial 4 boats
   - Task 17: Implement `DatabaseInitializer.cs` to run migrations on startup

6. **Generate Repository Tasks** (from data-model.md):
   - Task 18: Create `BoatRepository.cs` with `GetAllBoatStatusesAsync()` method
   - Task 19: Create `WaypointRepository.cs` with `GetWaypointsForBoatAsync()` method
   - Task 20: Implement `ResetAllBoatsAsync()` method in `BoatRepository.cs`

7. **Generate Integration Tasks** (from Program.cs analysis):
   - Task 21: Replace `BoatSimulator` class with repository pattern in `Program.cs`
   - Task 22: Update `/api/boats` endpoint to call `BoatRepository.GetAllBoatStatusesAsync()`
   - Task 23: Update `/api/boats/reset` endpoint to call `BoatRepository.ResetAllBoatsAsync()`
   - Task 24: Register repositories in DI container

8. **Generate Verification Tasks** (from quickstart.md):
   - Task 25: Run contract tests - verify all pass
   - Task 26: Execute quickstart.md steps - verify seed data
   - Task 27: Test simulation at 10x speed - verify no database bottlenecks
   - Task 28: Verify pgAdmin connection and query tables

### Ordering Strategy

1. **Tests First (TDD)**: Tasks 1-5 (contract tests) run before implementation
2. **Foundation**: Tasks 6-14 (models, infrastructure) before data access
3. **Data Layer**: Tasks 15-20 (database, repositories) before API integration
4. **Integration**: Tasks 21-24 (replace BoatSimulator) after repositories ready
5. **Validation**: Tasks 25-28 (tests, quickstart) last

### Parallelization Strategy

- **[P] Test tasks** (1-5): All can run in parallel (independent test files)
- **[P] Model tasks** (6-9): All can run in parallel (no dependencies)
- **[P] NuGet tasks** (10-11): Can run in parallel (different projects)
- **Sequential infrastructure** (12-14): Must run in order (depends on packages)
- **Sequential database** (15-17): Must run in order (schema → seed → initializer)
- **[P] Repository tasks** (18-20): Can run in parallel (independent files)
- **Sequential integration** (21-24): Must run in order (repository → endpoint → DI)
- **Sequential verification** (25-28): Must run in order (tests → quickstart → performance)

### Estimated Task Count

**Total**: 28 tasks
- **Test tasks**: 5 (TDD contract validation)
- **Model tasks**: 4 (entity POCOs)
- **Infrastructure**: 5 (Aspire, NuGet, DI)
- **Database**: 3 (migrations, seed, initializer)
- **Repository**: 3 (data access layer)
- **Integration**: 4 (replace BoatSimulator)
- **Verification**: 4 (tests, quickstart, performance)

### Task Dependencies

```text
┌─────────────────────────────────────────────────────────────┐
│ Phase 1: Tests (TDD) [P]                                    │
│ 1. Contract test: GET /api/boats schema                     │
│ 2. Contract test: GET /api/boats count                      │
│ 3. Contract test: GET /api/boats speed parameter            │
│ 4. Contract test: POST /api/boats/reset success             │
│ 5. Contract test: POST /api/boats/reset state verification  │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Phase 2: Foundation [P where noted]                         │
│ 6. [P] Create Boat.cs model                                 │
│ 7. [P] Create BoatState.cs model                            │
│ 8. [P] Create Route.cs model                                │
│ 9. [P] Create Waypoint.cs model                             │
│ 10. [P] Add Aspire.Hosting.PostgreSQL to AppHost            │
│ 11. [P] Add Aspire.Npgsql to ApiService                     │
│ 12. Configure PostgreSQL in AppHost.cs                      │
│ 13. Configure User Secrets                                  │
│ 14. Add AddNpgsqlDataSource() to ApiService                 │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Phase 3: Database                                           │
│ 15. Create 001_InitialSchema.sql                            │
│ 16. Create SeedData.sql                                     │
│ 17. Implement DatabaseInitializer.cs                        │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Phase 4: Data Access [P]                                    │
│ 18. [P] Create BoatRepository.cs                            │
│ 19. [P] Create WaypointRepository.cs                        │
│ 20. [P] Implement ResetAllBoatsAsync()                      │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Phase 5: Integration (Sequential)                           │
│ 21. Replace BoatSimulator in Program.cs                     │
│ 22. Update GET /api/boats endpoint                          │
│ 23. Update POST /api/boats/reset endpoint                   │
│ 24. Register repositories in DI                             │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Phase 6: Verification (Sequential)                          │
│ 25. Run contract tests (all must pass)                      │
│ 26. Execute quickstart.md (verify seed data)                │
│ 27. Performance test (10x speed, no bottlenecks)            │
│ 28. Verify pgAdmin access and queries                       │
└─────────────────────────────────────────────────────────────┘
```

**IMPORTANT**: This phase describes the approach. The actual `tasks.md` file will be generated by the `/tasks` command.

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking

**Phase Status**:

- [x] Phase 0: Research complete (/plan command) ✅
  - research.md created with 5 technology decisions
  - All decisions backed by Microsoft Learn documentation
  - No NEEDS CLARIFICATION markers remain

- [x] Phase 1: Design complete (/plan command) ✅
  - data-model.md: 4 PostgreSQL tables with indexes, constraints, migrations
  - contracts/get-boats.md: GET /api/boats contract (LOCKED)
  - contracts/post-boats-reset.md: POST /api/boats/reset contract (LOCKED)
  - quickstart.md: 10-step developer onboarding guide
  - Contract test specifications ready for Phase 2 implementation

- [x] Phase 2: Task planning approach described (/plan command) ✅
  - 28 tasks estimated (5 tests, 4 models, 5 infrastructure, 3 database, 3 repository, 4 integration, 4 verification)
  - Ordering strategy: TDD → Foundation → Database → Data Access → Integration → Verification
  - Parallelization identified: [P] for tests, models, NuGet packages, repositories
  - Dependency graph documented
  - **Ready for /tasks command to generate tasks.md**

- [ ] Phase 3: Tasks generated (/tasks command - NOT YET RUN)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:

- [x] Initial Constitution Check: PASS ✅
  - All 5 constitutional principles evaluated
  - Separation of Concerns: PASS (backend-only change)
  - Atomic Design: PASS (no 3D changes)
  - Research-First: PASS (Microsoft MCP used)
  - Aspire Orchestration: PASS (using AddAzurePostgresFlexibleServer)
  - TDD: PENDING (contract tests specified, to be implemented in Phase 2)

- [x] Post-Design Constitution Check: PASS ✅
  - Verified API contract preservation (`BoatStatus` record unchanged)
  - Verified frontend remains unaffected (no React/Three.js changes)
  - Verified separation: database layer isolated in `Data/` and `Models/` folders
  - Verified Aspire patterns: secrets via AddParameter, pgAdmin via WithPgAdmin, persistence via WithDataVolume
  - No new constitutional violations introduced

- [x] All NEEDS CLARIFICATION resolved ✅
  - Technical Context has no pending clarifications
  - All technology choices finalized (NpgsqlDataSource, AddParameter, WithPgAdmin, WithDataVolume)
  - Database schema fully designed (4 tables, indexes, constraints)

- [x] Complexity deviations documented ✅
  - No complexity deviations required
  - Repository pattern justified (lightweight, no EF Core overhead)
  - No additional projects needed (using existing ApiService structure)

---
*Based on Constitution v2.1.1 - See `/memory/constitution.md`*
