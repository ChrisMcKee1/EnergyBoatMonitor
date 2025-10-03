<!--
Sync Impact Report - Version 1.0.0 (Initial Constitution)
==========================================================
Version Change: NONE → 1.0.0 (Initial ratification)
Date: 2025-10-03

Modified Principles:
- NEW: I. Separation of Concerns (Frontend ↔ Backend)
- NEW: II. Atomic Design for 3D Components
- NEW: III. Research-First Development
- NEW: IV. Aspire Orchestration
- NEW: V. Test-Driven Development

Added Sections:
- Core Principles (5 principles)
- Architectural Constraints
- Development Workflow
- Governance

Template Sync Status:
✅ plan-template.md - Updated Constitution Check section
✅ spec-template.md - Aligned with research-first and testability requirements
✅ tasks-template.md - Updated to reflect TDD and atomic design task categorization
✅ .github/copilot-instructions.md - Already aligned with constitution principles

Follow-up TODOs: None
-->

# Energy Boat Monitor Constitution

## Core Principles

### I. Separation of Concerns (Frontend ↔ Backend)

**NON-NEGOTIABLE**: Clean contract-based integration between frontend and backend.

**Rules**:

- Backend (C# API) MUST own: simulation logic, navigation algorithms, state management, waypoint calculations
- Frontend (React/Three.js) MUST own: visualization, user interactions, coordinate transformations, rendering
- Contract MUST be REST API with strongly-typed records (e.g., `BoatStatus`, `Waypoint`)
- Backend MUST NEVER perform coordinate transformation (scene positioning is frontend responsibility)
- Frontend MUST NEVER calculate waypoints or navigation logic (physics/simulation is backend responsibility)

**Rationale**: Enables independent development, testing, and scaling. Backend can be swapped without frontend changes, and vice versa. Clear boundaries prevent logic duplication and make debugging deterministic.

### II. Atomic Design for 3D Components

**NON-NEGOTIABLE**: All 3D geometry MUST follow atomic design hierarchy.

**Rules**:

- Changes flow BOTTOM-UP only: Atoms → Molecules → Organisms → Templates (Scene)
- Atoms MUST define single geometric primitives (e.g., `Hull.js`, `Deck.js`)
- Molecules MUST assemble atoms into functional units (e.g., `Superstructure.js`, `DeckEquipment.js`)
- Organisms MUST combine molecules into complete models (e.g., `BoatModel.js`)
- Templates MUST orchestrate organisms in world space (e.g., `BoatScene.jsx`)
- Scene templates MUST NEVER modify geometry (positioning/rotation only)
- Component files MUST NEVER use absolute world coordinates (local origin only)

**Rationale**: Prevents "god objects", enables component reusability, makes changes predictable and testable. Bottom-up flow ensures changes propagate correctly through hierarchy.

### III. Research-First Development

**NON-NEGOTIABLE**: All implementation MUST be preceded by authoritative research.

**Rules**:

- Microsoft technologies (.NET, Aspire, C#, Azure) MUST use Microsoft MCP as PRIMARY source
- Third-party libraries (React, Three.js, npm packages) MUST use Context7 for documentation
- Implementation examples (blog posts, Stack Overflow) are TERTIARY sources only
- Every technical decision MUST be backed by official documentation or verified examples
- Unknown APIs MUST be researched before use (no guessing signatures)
- Research findings MUST be documented in feature specs or plan documents

**Rationale**: Prevents outdated patterns, ensures current best practices, reduces debugging time, builds institutional knowledge. Evidence-based development over trial-and-error.

### IV. Aspire Orchestration

**NON-NEGOTIABLE**: `aspire run` is the ONLY way to run the application.

**Rules**:

- ALL services MUST be orchestrated via Aspire AppHost
- Dependencies MUST be managed via Aspire (no manual `npm install` or `dotnet build`)
- Service discovery MUST use Aspire's automatic injection (no hardcoded URLs)
- Frontend MUST use `AddNpmApp()` for npm-based projects
- Backend MUST use `AddProject<T>()` for .NET projects
- Manual execution of `npm start`, `dotnet run`, or Docker MUST be avoided
- RestoreNpm MSBuild target handles npm dependencies automatically

**Rationale**: Single command manages entire stack, eliminates manual dependency management, ensures consistent environment across developers, enables proper service discovery.

### V. Test-Driven Development

**NON-NEGOTIABLE**: Tests MUST be written before implementation.

**Rules**:

- Tests MUST be written first and MUST fail before implementation begins
- Contract tests MUST exist for all API endpoints
- Integration tests MUST cover service communication and data flow
- Unit tests MUST cover business logic and validation
- All tests MUST pass before code review
- New features MUST include tests in the same PR
- Regressions MUST have tests added before fixes

**Rationale**: Prevents regressions, ensures requirements are testable, documents expected behavior, enables confident refactoring, catches issues early.

## Architectural Constraints

### Technology Stack

**MUST USE**:

- **Backend**: C# 12, .NET 9.0, ASP.NET Core Minimal API
- **Frontend**: React 19.1.1, Three.js 0.180.0, Vite 4.5.5 (pinned)
- **Orchestration**: .NET Aspire 9.5.0
- **Runtime**: Node.js 20+

**MUST NOT**:

- Introduce new frameworks without architecture review
- Add Docker/Podman requirements (Aspire handles orchestration)

### Coordinate System Standards

**MUST**:

- Scene origin `(0,0,0)` MUST equal Dock location `(51.5100, -0.1350)`
- Backend MUST use Haversine formula for distance calculations (returns nautical miles)
- Frontend MUST use `CoordinateConverter.latLonToScene()` for geographic → scene conversion
- Frontend MUST use `headingToRotation()` for nautical heading → Three.js rotation
- All waypoint routes MUST avoid dock coordinates
- Dynamic threshold MUST be used: `0.15 + (distanceTraveled * 1.5)` nautical miles

**Rationale**: Ensures real-world spatial relationships map 1:1 to scene, simplifies debugging, prevents navigation oscillation at high speeds.

### Performance Requirements

**MUST**:

- 3D scene MUST maintain 60 FPS on mid-range hardware
- API response time MUST be <200ms p95
- Frontend polling interval MUST be 2 seconds (no faster than 1 second)
- Shadow rendering MUST use selective casting (not all objects)

## Development Workflow

### Feature Development Process

1. **Specification**: Write feature spec using `spec-template.md` (WHAT users need, WHY)
2. **Research**: Use Microsoft MCP / Context7 to validate technical approach
3. **Planning**: Create implementation plan using `plan-template.md` with Constitution Check
4. **Tasks**: Generate tasks using `tasks-template.md` (tests first, then implementation)
5. **Implementation**: Follow TDD (write failing tests → implement → refactor)
6. **Verification**: Run `dotnet build`, `dotnet test`, check `problems` tool
7. **Review**: Ensure constitution compliance before PR

### Prohibited Practices

**MUST NOT**:

- Skip constitution check in implementation plans
- Write implementation before tests
- Hardcode URLs, coordinates, or configuration values
- Bypass Aspire orchestration
- Modify geometry in scene templates
- Add backend logic to frontend (or vice versa)
- Guess API signatures without research

### Code Review Requirements

**MUST**:

- Verify separation of concerns (backend/frontend boundary)
- Check atomic design hierarchy (atoms → molecules → organisms → templates)
- Validate tests exist and pass
- Confirm research sources documented
- Ensure Aspire orchestration patterns followed
- No hardcoded values (use Constants.js, appsettings.json)

## Governance

### Amendment Process

1. Proposed changes MUST be documented with rationale
2. Version bump MUST follow semantic versioning:
   - **MAJOR**: Backward incompatible principle changes
   - **MINOR**: New principles or material expansions
   - **PATCH**: Clarifications, wording, typo fixes
3. Template files MUST be updated to reflect changes
4. Sync Impact Report MUST be generated
5. All developers MUST be notified of changes

### Compliance Review

- Constitution supersedes all other practices
- All PRs MUST pass constitution check
- Violations MUST be justified or corrected
- Templates stay in sync via automated validation

### Runtime Guidance

- Use `.github/copilot-instructions.md` for AI-assisted development
- Use decision frameworks for "where does this logic belong?"
- Consult architecture diagrams for system understanding

**Version**: 1.0.0 | **Ratified**: 2025-10-03 | **Last Amended**: 2025-10-03
