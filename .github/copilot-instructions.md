# Copilot Instructions for Energy Boat Monitor

## Core Architectural Principles

### 1. Separation of Concerns (Frontend ↔ Backend)
**WHY**: Clean contract-based integration enables independent development and testing.

- **Backend (C# API)**: Owns simulation logic, navigation algorithms, state management
- **Frontend (React/Three.js)**: Owns visualization, user interactions, coordinate transformations
- **Contract**: REST API with strongly-typed records (`BoatStatus`, `Waypoint`)

**Decision Framework**:
- Does it involve physics/navigation/waypoints? → Backend (`Program.cs`)
- Does it involve rendering/materials/user input? → Frontend (`src/scene/`)
- Does it involve coordinate transformation? → Frontend (`CoordinateConverter.js`)

### 2. Atomic Design for 3D Components (Frontend Only)
**WHY**: Prevents "god objects", enables reusability, makes changes predictable and testable.

**Hierarchy** (changes flow BOTTOM-UP only):
```
Atoms → Molecules → Organisms → Templates (Scene)
```

**Decision Framework**:
- Modifying a single geometric primitive? → Edit atom (`Hull.js`, `Deck.js`)
- Changing how components assemble? → Edit molecule (`DeckEquipment.js`, `Superstructure.js`)
- Adding new complete unit? → Create organism (`Mast.js`, `BoatModel.js`)
- Changing world positioning/rotation? → Edit template (`BoatScene.jsx`)

### 3. Coordinate System (Dock-Centered)
**WHY**: Real-world spatial relationships map 1:1 to scene relationships, simplifying debugging and ensuring accuracy.

**Invariant**: Scene origin `(0,0,0)` = Dock location `(51.5100, -0.1350)`

**Decision Framework**:
- Need to position something relative to real-world location? → Use `CoordinateConverter.latLonToScene()`
- Need to define position within a boat/building? → Use local coordinates relative to component origin
- Need to check if boat is at waypoint? → Backend uses Haversine (nautical miles), frontend uses scene units

### 4. Aspire Orchestration (Build & Run)
**WHY**: Single command manages entire multi-service stack, eliminates manual dependency management.

**Invariant**: `aspire run` is the ONLY way to run the application (no `npm start`, `dotnet run`, or Docker).

**Decision Framework**:
- Need to run app? → `cd EnergyBoatApp.AppHost && aspire run`
- Need to install dependencies? → Don't - Aspire's `RestoreNpm` MSBuild target handles it
- Need to configure service discovery? → Aspire injects `services__apiservice__https__0` automatically

## Project Overview
.NET Aspire 9.5 application with C# Minimal API backend and React+Three.js frontend for real-time 3D maritime fleet visualization with autonomous boat navigation simulation.

## Tech Stack

### Backend
- **.NET 9.0** (`net9.0` target framework)
- **C# 12** with implicit usings and nullable reference types enabled
- **ASP.NET Core 9.0** Minimal API
- **Aspire SDK 9.5.0** (via `Aspire.AppHost.Sdk` and NuGet packages)
- **OpenTelemetry** for distributed tracing

### Frontend
- **React 19.1.1** with React DOM 19.1.1
- **Three.js 0.180.0** for WebGL 3D rendering
- **Vite 4.5.5** (pinned to avoid native module issues on Windows)
- **Rollup 3.29.4** (bundler, pinned for stability)
- **Node.js 20+** runtime
- **ESLint 9** with React hooks and refresh plugins

### Orchestration & Build
- **Aspire.Hosting.AppHost 9.5.0** - orchestrates multi-service application
- **Aspire.Hosting.NodeJs 9.5.0** - manages npm-based frontend via `AddNpmApp()`
- **Aspire ServiceDefaults** - shared configuration for OpenTelemetry and service discovery
- **MSBuild `RestoreNpm` target** - automatically runs `npm install` before AppHost builds (if `node_modules` missing)

## Navigation Guide: Where to Make Changes

### Backend Changes (C# API)

| Task | File | Location | Pattern |
|------|------|----------|---------|
| Add new boat | `Program.cs` | `_boatStates` list | Add `BoatState` record with initial lat/lon/heading |
| Add waypoint route | `Program.cs` | `_boatRoutes` dictionary | Add `List<Waypoint>`, avoid dock coords |
| Modify navigation logic | `Program.cs` | `UpdateBoatPositions()` | Haversine distance, heading calculation |
| Change status transitions | `Program.cs` | `UpdateBoatPositions()` | Energy level thresholds (70%, 30%, 20%) |
| Add API endpoint | `Program.cs` | After `app.MapGet/Post` | Use Minimal API pattern, add to OpenAPI |
| Modify boat data contract | `Program.cs` | `BoatStatus` record | Add property (will auto-include in JSON) |

**Guardrails**:
- ✅ ALWAYS use Haversine formula for distance (returns nautical miles)
- ✅ ALWAYS use dynamic threshold: `0.15 + (distanceTraveled * 1.5)` to prevent oscillation
- ❌ NEVER hardcode coordinates - use `Waypoint` records
- ❌ NEVER add frontend logic to backend (coordinate transformation belongs in frontend)

### Frontend Changes (React/Three.js)

#### 3D Geometry/Visual Changes

| Task | File(s) | Hierarchy Level | Pattern |
|------|---------|-----------------|---------|
| Change hull shape | `atoms/Hull.js` | Atom | Modify `THREE.Shape` cross-section or extrude depth |
| Change deck size | `atoms/Deck.js` | Atom | Modify `BoxGeometry` dimensions |
| Reposition crane | `molecules/DeckEquipment.js` | Molecule | Adjust `crane.position.set()` in assembly |
| Add new superstructure level | `molecules/Superstructure.js` | Molecule | Add new block, position relative to others |
| Add new mast equipment | `organisms/Mast.js` | Organism | Add component to mast assembly |
| Change boat material | `BoatMaterials.js` | Utility | Modify material properties (color, metalness, roughness) |
| Add new boat component | 1. Create atom → 2. Add to molecule → 3. Auto-included in organism | Bottom-up | Follow atomic hierarchy |

**Guardrails**:
- ✅ ALWAYS define positions in component file, not in `BoatScene.jsx`
- ✅ ALWAYS use parent-child relationships (`.add()`) for rotation inheritance
- ✅ ALWAYS set `castShadow = true` for solid objects, `receiveShadow = true` for large surfaces
- ❌ NEVER modify geometry in `BoatScene.jsx` or `BoatModel.js` (they are orchestrators)
- ❌ NEVER use absolute world coordinates in atoms/molecules (use local origin)

#### Scene/Runtime Changes

| Task | File | Location | Pattern |
|------|------|----------|---------|
| Change boat positioning | `BoatScene.jsx` | `useEffect([boats])` | Use `CoordinateConverter.latLonToScene()` |
| Apply rotation/heading | `BoatScene.jsx` | `useEffect([boats])` | Use `headingToRotation(boat.heading)` |
| Change camera behavior | `controls/CameraControls.js` | `setupCameraControls()` | Modify OrbitControls settings |
| Add keyboard shortcut | `controls/KeyboardControls.js` | `handleKeyDown()` | Add key case, update state |
| Change ocean wave speed | `environment/OceanEnvironment.jsx` | `updateOceanAnimation()` | Modify `water.material.uniforms.time` |
| Toggle day/night | `environment/SkySystem.jsx` | `toggleDayNight()` | Switch between `SKY_PRESETS` |
| Change status colors | `utils/Constants.js` | `BOAT_COLORS` object | Update hex values |

**Guardrails**:
- ✅ ALWAYS use `CoordinateConverter` for lat/lon → scene conversion
- ✅ ALWAYS use `headingToRotation()` for nautical heading → Three.js rotation
- ✅ ALWAYS apply status-based changes in `BoatScene.jsx`, not in component files
- ❌ NEVER hardcode coordinate conversions (use `SCALE_FACTOR_LAT/LON` from `Constants.js`)
- ❌ NEVER modify scene constants in component files (centralize in `Constants.js`)

#### Dashboard/UI Changes

| Task | File | Location | Pattern |
|------|------|----------|---------|
| Add boat card field | `App.jsx` | `<div className="boat-card">` JSX | Add `<div className="boat-info-row">` |
| Change card styling | `App.css` | `.boat-card` selector | Update CSS properties |
| Add control button | `SceneControls.jsx` | Button group JSX | Add button with event handler |
| Modify speed slider | `SceneControls.jsx` | `<input type="range">` | Change min/max/step |

**Guardrails**:
- ✅ ALWAYS normalize API data (handle both `boat.latitude` and `boat.Latitude`)
- ✅ ALWAYS use Contoso-Sea branding colors (CSS variables in `App.css`)
- ❌ NEVER put business logic in `App.jsx` (it's a presentation component)

## Decision-Making Frameworks

### Framework 1: "Where Does This Logic Belong?"

```
START: What are you implementing?

├─ Simulating boat movement/physics?
│  └─ Backend: Program.cs → UpdateBoatPositions()
│
├─ Converting geographic coordinates?
│  └─ Frontend: utils/CoordinateConverter.js
│
├─ Changing 3D geometry shape?
│  └─ Frontend: scene/vessels/atoms/ or molecules/
│
├─ Positioning objects in the world?
│  ├─ Real-world location (lat/lon)? → Frontend: BoatScene.jsx (use CoordinateConverter)
│  └─ Relative to parent component? → Frontend: Component definition file
│
├─ Status-based visual changes (colors, lights)?
│  ├─ Material definitions? → Frontend: BoatMaterials.js
│  └─ Runtime application? → Frontend: BoatScene.jsx
│
└─ User interaction (clicks, keyboard)?
   └─ Frontend: controls/ or App.jsx
```

### Framework 2: "How Should I Modify Boat Geometry?"

```
START: What component needs to change?

├─ Single primitive (hull shape, deck size)?
│  └─ 1. Edit atom file (Hull.js, Deck.js)
│     2. Test in isolation (verify dimensions)
│     3. Changes propagate up automatically
│
├─ Assembly of primitives (superstructure levels, equipment)?
│  └─ 1. Edit molecule file (Superstructure.js, DeckEquipment.js)
│     2. Verify parent-child relationships
│     3. Test rotation inheritance
│
├─ Complete new functional unit (new vessel type)?
│  └─ 1. Create atoms (if needed)
│     2. Create/modify molecules
│     3. Create organism (new model assembly)
│     4. Add to scene in BoatScene.jsx
│
└─ World position/rotation?
   └─ Edit BoatScene.jsx ONLY (never in components)
```

### Framework 3: "How Should I Add a New Boat?"

```
STEP 1: Backend (Program.cs)
├─ Add to _boatRoutes dictionary
│  ├─ Define waypoint list (List<Waypoint>)
│  ├─ AVOID dock coordinates (51.5100, -0.1350)
│  └─ Ensure waypoints are >0.3 nautical miles apart
│
└─ Add to _boatStates list
   ├─ Set initial Latitude, Longitude
   ├─ Set SpeedKnots (0 for maintenance, 8-12 for active)
   ├─ Set Heading (0-360 degrees)
   └─ Set EnergyLevel (>70 = Active, 20-30 = Charging, <20 = Maintenance)

STEP 2: Frontend (Automatic)
├─ BoatScene.jsx fetches from /api/boats
├─ Creates boat mesh using BoatModel.js
├─ Positions using CoordinateConverter
└─ Applies rotation using headingToRotation()

STEP 3: Testing
├─ Verify boat appears at correct location
├─ Verify boat faces correct direction (heading)
├─ Test navigation at 10x speed (no oscillation)
└─ Verify status colors match energy level
```

### Framework 4: "How Should I Debug Coordinate Issues?"

```
PROBLEM: Boat not appearing where expected

├─ Check backend coordinates
│  ├─ Run: console.log(boat.Latitude, boat.Longitude) in Program.cs
│  └─ Verify within BOUNDS (lat 51.48-51.53, lon -0.16 to -0.09)
│
├─ Check coordinate conversion
│  ├─ Run: node coordinate-verification.js
│  ├─ Verify scene position matches expected location
│  └─ Check DOCK_LAT/LON in Constants.js (51.5100, -0.1350)
│
├─ Check scene positioning
│  ├─ In BoatScene.jsx: console.log(boatMesh.position)
│  ├─ Verify using CoordinateConverter, not hardcoded math
│  └─ Verify position.y = 0 (model origin at water level)
│
└─ Check heading/rotation
   ├─ In BoatScene.jsx: console.log(boat.heading, boatMesh.rotation.y)
   ├─ Verify using headingToRotation(), not direct conversion
   └─ Expected: 0°=North faces -Z, 90°=East faces +X
```

## Integration Points

### Backend → Frontend Contract

**API Endpoint**: `/api/boats?speed={multiplier}`

**Request**:
- Query parameter `speed`: 0.1 to 10.0 (default 1.0)
- Multiplies simulation speed and waypoint detection threshold

**Response**: `IEnumerable<BoatStatus>`
```csharp
record BoatStatus(
    string Id,              // "BOAT-001"
    double Latitude,        // 51.5074
    double Longitude,       // -0.1278
    string Status,          // "Active" | "Charging" | "Maintenance"
    double EnergyLevel,     // 0-100
    string VesselName,      // "Contoso Sea Voyager"
    string SurveyType,      // "Geophysical Survey"
    string Project,         // "Dogger Bank Offshore Wind Farm"
    string Equipment,       // "Multibeam Sonar, Magnetometer"
    double AreaCovered,     // Square kilometers
    string Speed,           // "8 knots"
    int CrewCount,          // 12
    string Conditions,      // "Good sea state"
    double Heading          // 0-360 degrees (0=North)
);
```

**Frontend Consumption**:
```javascript
// App.jsx - Polling
useEffect(() => {
  const interval = setInterval(async () => {
    const response = await fetch(`/api/boats?speed=${speedMultiplier}`);
    const data = await response.json();
    setBoats(data);
  }, 2000); // Poll every 2 seconds
  
  return () => clearInterval(interval);
}, [speedMultiplier]);
```

**Guardrails**:
- ✅ Backend returns nautical miles (distance) and degrees (heading)
- ✅ Frontend converts lat/lon → scene units and heading → rotation
- ✅ Handle both PascalCase and camelCase (API may vary)
- ❌ Backend NEVER does coordinate transformation (frontend's job)
- ❌ Frontend NEVER calculates waypoints (backend's job)

### Frontend Three.js Pipeline

**Data Flow**:
```
API Response (BoatStatus[])
    ↓
CoordinateConverter.latLonToScene({ latitude, longitude })
    ↓
Scene Position { x, y, z }
    ↓
BoatHelpers.headingToRotation(heading)
    ↓
Rotation (radians)
    ↓
boatMesh.position.set(x, y, z)
boatMesh.rotation.y = rotation
```

**Example** (`BoatScene.jsx`):
```javascript
const boatPos = CoordinateConverter.latLonToScene({ 
  latitude: boat.latitude || boat.Latitude, 
  longitude: boat.longitude || boat.Longitude 
});

boatMesh.position.x = boatPos.x;
boatMesh.position.y = 0; // Model origin at water level
boatMesh.position.z = boatPos.z;

const heading = boat.heading || boat.Heading || 0;
boatMesh.rotation.y = headingToRotation(heading);
```

### Aspire Service Discovery

**Configuration** (`EnergyBoatApp.AppHost/AppHost.cs`):
```csharp
var api = builder.AddProject<Projects.EnergyBoatApp_ApiService>("apiservice");

var web = builder.AddNpmApp("webfrontend", "../EnergyBoatApp.Web", "start")
    .WithReference(api)  // Injects services__apiservice__https__0
    .WithHttpEndpoint(env: "PORT")
    .PublishAsDockerFile();
```

**Vite Proxy** (`vite.config.js`):
```javascript
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: env.services__apiservice__https__0 || 'http://localhost:7585',
        changeOrigin: true,
        secure: false
      }
    }
  }
});
```

**Result**: Frontend calls `/api/boats` → Vite proxies to backend → No CORS issues

### OpenTelemetry Integration

**Problem**: Vite blocks non-`VITE_` prefixed env vars from browser.

**Solution**: API bridge endpoint exposes OTEL config:
```csharp
// Program.cs
app.MapGet("/api/telemetry/config", (IConfiguration config) => new
{
    OtlpEndpoint = config["OTEL_EXPORTER_OTLP_ENDPOINT"],
    ServiceName = "browser"
});
```

```javascript
// telemetry.js
const response = await fetch('/api/telemetry/config');
const { OtlpEndpoint } = await response.json();

const provider = new WebTracerProvider({
  resource: new Resource({
    [SemanticResourceAttributes.SERVICE_NAME]: 'browser'
  })
});

provider.addSpanProcessor(new BatchSpanProcessor(new OTLPTraceExporter({
  url: `${OtlpEndpoint}/v1/traces`
})));
```

**Result**: Browser traces appear in Aspire dashboard alongside API traces

## Development Workflow

### Running the Application
**This project uses Aspire CLI exclusively - no Docker/Podman required, no manual build commands.**

```powershell
cd EnergyBoatApp.AppHost
aspire run                  # Standard run (opens dashboard at http://localhost:15888)
aspire run --watch          # Watch mode - auto-restart on file changes
aspire run --debug          # Enable debug logging to console
aspire run --project <path> # Specify AppHost project if not in directory
```

**What happens automatically when you run `aspire run`:**
1. **AppHost's `RestoreNpm` MSBuild target** checks if `node_modules` exists in `../EnergyBoatApp.Web/`
2. If `node_modules` is missing, runs `npm install` in the Web project directory
3. Aspire builds the AppHost project (which triggers the RestoreNpm target)
4. Aspire builds the ApiService project
5. Aspire launches the dashboard at `http://localhost:15888`
6. Aspire starts the ApiService (C# Minimal API)
7. Aspire starts the Web frontend via `AddNpmApp("webfrontend", "../EnergyBoatApp.Web", "start")`
   - This executes `npm run start` (which runs `vite` dev server)
   - Vite starts on the port specified by the `PORT` environment variable (set by Aspire)
8. Aspire configures service discovery so frontend can call backend via `services__apiservice__https__0`

**Important**: Aspire handles all building and dependency restoration automatically:
- ✅ `npm install` runs automatically via AppHost's `RestoreNpm` MSBuild target (if `node_modules` missing)
- ✅ `dotnet build` handled by Aspire for all .NET projects
- ✅ `npm start` launched by Aspire via `AddNpmApp()` 
- ✅ Service discovery environment variables injected automatically

**Do NOT run manually:**
- ❌ `npm install` (auto-runs during AppHost build if needed)
- ❌ `dotnet build` (Aspire builds all projects)
- ❌ `npm start` or `vite` (Aspire manages npm lifecycle via `AddNpmApp`)
- ❌ `dotnet run` in ApiService (Aspire orchestrates this)

### Other Aspire CLI Commands
```powershell
aspire --help              # Show all available commands
aspire new                 # Create new Aspire project
aspire add <integration>   # Add hosting integration
aspire update              # Update integrations (Preview)
aspire publish             # Generate deployment artifacts (Preview)
```

### Debugging Coordinate Issues
Use `coordinate-verification.js` to validate scene positioning:
```powershell
cd EnergyBoatApp.Web
node coordinate-verification.js    # Shows waypoint/boundary positions in scene units
```

## Architectural Guardrails

### Backend Guardrails (C# API)

**ALWAYS**:
- ✅ Use Haversine formula for distance calculations (returns nautical miles)
- ✅ Use dynamic waypoint threshold: `0.15 + (distanceTraveled * 1.5)`
- ✅ Include `Heading` property in `BoatStatus` record (frontend depends on it)
- ✅ Return `IEnumerable<BoatStatus>` from API endpoints (supports LINQ)
- ✅ Keep waypoints >0.3 nautical miles apart (prevents oscillation at 10x speed)

**NEVER**:
- ❌ Hardcode coordinates in navigation logic (use `Waypoint` records)
- ❌ Add frontend-specific logic (coordinate transformation, rendering)
- ❌ Use degrees for distance (always nautical miles from Haversine)
- ❌ Create waypoints at dock coordinates `(51.5100, -0.1350)`
- ❌ Return different casing (`latitude` vs `Latitude`) - be consistent

### Frontend Guardrails (React/Three.js)

#### Atomic Design (3D Components)

**ALWAYS**:
- ✅ Define geometry in atoms (`Hull.js`, `Deck.js`)
- ✅ Assemble components in molecules (`Superstructure.js`, `DeckEquipment.js`)
- ✅ Use `THREE.Group` and `.add()` for parent-child relationships
- ✅ Position relative to component's local origin `(0,0,0)`
- ✅ Set `castShadow = true` on meshes, `receiveShadow = true` on large surfaces

**NEVER**:
- ❌ Modify geometry in `BoatScene.jsx` or `BoatModel.js` (orchestrators only)
- ❌ Define positions in scene template (belongs in component definition)
- ❌ Use absolute world coordinates in atoms/molecules
- ❌ Bypass parent-child relationships (breaks rotation inheritance)
- ❌ Hardcode scale/position in scene (define in component file)

#### Coordinate System

**ALWAYS**:
- ✅ Use `CoordinateConverter.latLonToScene()` for geographic → scene conversion
- ✅ Use dock as origin: `(51.5100, -0.1350)` → scene `(0, 0, 0)`
- ✅ Use `headingToRotation()` for nautical heading → Three.js rotation
- ✅ Keep SCALE_FACTOR constants in `Constants.js` (2000 units/degree)
- ✅ Verify positions are within BOUNDS (lat 51.48-51.53, lon -0.16 to -0.09)

**NEVER**:
- ❌ Hardcode coordinate conversion math in components
- ❌ Use arbitrary center point (CENTER_LAT/LON is for reference only)
- ❌ Apply heading directly as rotation (use `headingToRotation()` conversion)
- ❌ Modify DOCK_LAT/LON without updating backend waypoints
- ❌ Use scene units for distance checks (backend uses nautical miles)

#### Status-Based Rendering

**ALWAYS**:
- ✅ Define materials in `BoatMaterials.js` (centralized)
- ✅ Apply status changes in `BoatScene.jsx` (runtime orchestrator)
- ✅ Support both camelCase and PascalCase API responses (`boat.status` and `boat.Status`)
- ✅ Use status colors from `Constants.js` (`BOAT_COLORS`, `STATUS_LIGHT_COLORS`)

**NEVER**:
- ❌ Put status logic in atoms/molecules (they're status-agnostic)
- ❌ Hardcode colors in component files (use Constants)
- ❌ Assume API casing (normalize in `BoatHelpers.js`)

### Integration Guardrails (API ↔ Frontend)

**ALWAYS**:
- ✅ Use `/api/boats?speed={multiplier}` endpoint for polling
- ✅ Poll every 2 seconds (React `useEffect` with interval)
- ✅ Handle null/undefined gracefully (`boat.heading || boat.Heading || 0`)
- ✅ Verify API contract: `BoatStatus` record = frontend boat data shape
- ✅ Use Aspire service discovery (`services__apiservice__https__0`)

**NEVER**:
- ❌ Poll faster than 1 second (overloads backend)
- ❌ Hardcode API URLs (use Vite proxy `/api/*`)
- ❌ Skip normalization (API may return PascalCase or camelCase)
- ❌ Bypass Aspire orchestration (`npm start` directly)

## Common Patterns & Examples

### Pattern 1: Adding a New Atom

## Common Patterns & Examples

### Pattern 1: Adding a New Atom

**When**: Need a new primitive geometry component (e.g., funnel, lifeboat)

```javascript
// atoms/Funnel.js
import * as THREE from 'three';
import { createFunnelMaterial } from '../BoatMaterials.js';

export function createFunnel() {
  const geometry = new THREE.CylinderGeometry(0.35, 0.42, 1.2, 16);
  const material = createFunnelMaterial();
  
  const funnel = new THREE.Mesh(geometry, material);
  funnel.position.set(0, 5.95, -0.5); // Position relative to local origin
  funnel.castShadow = true;
  
  return funnel;
}
```

**Rules**:
- Export single function returning `THREE.Mesh` or `THREE.Group`
- Accept material as parameter OR create default
- Position relative to `(0,0,0)` - never use world coordinates
- Set shadow properties (`castShadow`, `receiveShadow`)

### Pattern 2: Composing a Molecule

**When**: Need to assemble multiple atoms into a functional unit

```javascript
// molecules/DeckEquipment.js
import { createRailingSection } from './helpers.js';
import { createCraneAssembly } from './helpers.js';

export function createDeckEquipment() {
  const railings = new THREE.Group();
  
  // Port side railing
  const portRailing = createRailingSection(8.5, 'x');
  portRailing.position.set(0, 1.6, -1.6); // Relative to deck origin
  railings.add(portRailing);
  
  // Crane with parent-child relationship
  const crane = new THREE.Group();
  
  const base = new THREE.Mesh(baseGeom, baseMaterial);
  base.position.y = baseHeight / 2; // Bottom of base at y=0
  
  const arm = new THREE.Mesh(armGeom, armMaterial);
  arm.position.y = (baseHeight / 2) + (armLength / 2); // Relative to base
  arm.rotation.z = Math.PI / 5; // Tilt from attachment point
  base.add(arm); // Parent arm to base - they rotate together
  
  crane.add(base);
  crane.position.set(-3.5, 1.6, -1.0); // On deck surface
  
  return { railings, crane }; // Return named components
}
```

**Rules**:
- Use `THREE.Group` for assembly
- Implement parent-child relationships with `.add()`
- Position children relative to parent's origin
- Return object with named components: `{ railings, crane }`
- Test rotation inheritance (child should rotate with parent)

### Pattern 3: Creating an Organism

**When**: Need complete functional unit combining molecules/atoms

```javascript
// BoatModel.js
import { createHull } from './atoms/Hull.js';
import { createDeck } from './atoms/Deck.js';
import { createSuperstructure } from './molecules/Superstructure.js';
import { createDeckEquipment } from './molecules/DeckEquipment.js';

export function createBoatModel(boat) {
  const boatModel = new THREE.Group();
  boatModel.name = `boat-${boat.id}`;
  
  // Atoms
  const hull = createHull();
  const deck = createDeck();
  
  // Molecules
  const superstructure = createSuperstructure();
  const { railings, crane } = createDeckEquipment();
  
  // Assemble (internal positioning already defined in components)
  boatModel.add(hull, deck, superstructure, railings, crane);
  
  // Store data for runtime updates
  boatModel.userData.status = boat.status || boat.Status;
  
  // World position/rotation applied in BoatScene.jsx, not here
  return boatModel;
}
```

**Rules**:
- Import from atoms/molecules (never skip hierarchy)
- Use `THREE.Group` as container
- Components already have internal positions - just add them
- Store runtime data in `userData` (accessible in scene)
- DO NOT apply world position/rotation (that's scene's job)

### Pattern 4: Positioning in Scene Template

**When**: Need to place organisms in world space based on API data

```javascript
// BoatScene.jsx
import { CoordinateConverter } from '../scene/utils/CoordinateConverter';
import { headingToRotation } from '../scene/utils/BoatHelpers';
import { createBoatModel } from '../scene/vessels/BoatModel.js';

useEffect(() => {
  boats.forEach(boat => {
    const boatId = boat.id || boat.Id;
    let boatMesh = boatMeshesRef.current[boatId];
    
    if (!boatMesh) {
      // Create organism
      boatMesh = createBoatModel(boat);
      boatMesh.name = `boat-${boatId}`;
      sceneRef.current.add(boatMesh);
      boatMeshesRef.current[boatId] = boatMesh;
    }
    
    // Apply world position (geographic → scene coordinates)
    const latitude = boat.latitude || boat.Latitude;
    const longitude = boat.longitude || boat.Longitude;
    const boatPos = CoordinateConverter.latLonToScene({ latitude, longitude });
    
    boatMesh.position.x = boatPos.x;
    boatMesh.position.y = 0; // Model origin at water level
    boatMesh.position.z = boatPos.z;
    
    // Apply rotation (nautical heading → Three.js rotation)
    const heading = boat.heading || boat.Heading || 0;
    boatMesh.rotation.y = headingToRotation(heading);
  });
}, [boats]);
```

**Rules**:
- ONLY scene template positions organisms in world space
- ALWAYS use `CoordinateConverter` for lat/lon → scene
- ALWAYS use `headingToRotation()` for nautical → Three.js rotation
- ALWAYS normalize API data (`boat.latitude || boat.Latitude`)
- Store references in `useRef` for updates (not state - causes re-renders)

### Pattern 5: Parent-Child Rotation Inheritance

**When**: Need component to rotate with its parent (e.g., crane arm follows base)

```javascript
// WRONG - components don't rotate together
const crane = new THREE.Group();
crane.add(base);
crane.add(arm); // arm is sibling to base, not child

// CORRECT - arm inherits base rotation
const crane = new THREE.Group();

const base = new THREE.Mesh(baseGeom, material);
base.position.y = baseHeight / 2;

const arm = new THREE.Mesh(armGeom, material);
arm.position.y = (baseHeight / 2) + (armLength / 2); // Relative to base top
base.add(arm); // Parent arm to base

crane.add(base); // Add assembled unit to crane
crane.position.set(-3.5, 1.6, -1.0); // Position crane in deck space
```

**Rules**:
- Child position is RELATIVE to parent's origin
- Rotating parent automatically rotates children
- Use `.add()` to establish parent-child relationship
- Test by rotating parent: `base.rotation.y = Math.PI / 4`

### Pattern 6: Status-Based Material Updates

**When**: Need to change boat appearance based on runtime status

```javascript
// BoatMaterials.js - Define materials
export function createHullMaterial(status = 'Active') {
  const color = status === 'Maintenance' ? 0x888888 : 0xFF4500;
  return new THREE.MeshStandardMaterial({
    color,
    metalness: 0.8,
    roughness: 0.3,
    flatShading: true
  });
}

// BoatScene.jsx - Apply at runtime
useEffect(() => {
  boats.forEach(boat => {
    const boatMesh = boatMeshesRef.current[boat.id];
    const currentStatus = boat.status || boat.Status;
    
    if (boatMesh.userData.status !== currentStatus) {
      // Traverse to find hull mesh
      boatMesh.traverse(child => {
        if (child.isMesh && child.geometry.type === 'ExtrudeGeometry') {
          child.material.dispose(); // Clean up old material
          child.material = createHullMaterial(currentStatus);
        }
      });
      
      boatMesh.userData.status = currentStatus; // Update cache
    }
  });
}, [boats]);
```

**Rules**:
- Define materials in `BoatMaterials.js` (centralized)
- Apply changes in scene template (runtime orchestrator)
- ALWAYS dispose old material before replacing
- Cache status in `userData` to avoid redundant updates
- Use `traverse()` to find specific meshes in hierarchy

## Key Files Reference

| File | Purpose | Critical Details |
|------|---------|------------------|
| `AppHost.cs` | Aspire orchestration | Npm app setup, API reference, OTEL config |
| `Program.cs` (API) | Boat simulation logic | Waypoint navigation (line 290-320), Haversine distance (line 354) |
| `BoatScene.jsx` | Main 3D orchestrator | Boat positioning, status updates, coordinate conversion |
| `Constants.js` | Scene configuration | DOCK_LAT/LON (origin), SCALE_FACTOR, BOUNDS |
| `CoordinateConverter.js` | Lat/lon ↔ scene coords | **Dock-centered** conversion (not CENTER_LAT/LON) |
| `BoatHelpers.js` | Navigation utilities | `headingToRotation()` - nautical degrees → Three.js radians |
| `BoatModel.js` | Organism assembly | Imports atoms/molecules, returns complete boat |
| `atoms/Hull.js` | Hull geometry | ExtrudeGeometry with cross-section, 9.5 units length |
| `atoms/Deck.js` | Deck geometry | BoxGeometry 8.5 x 0.2 x 3.2 units |
| `molecules/DeckEquipment.js` | Equipment assembly | Railings + crane with parent-child relationships |
| `molecules/Superstructure.js` | Building assembly | Accommodation + bridge stacked vertically |
| `BoatMaterials.js` | PBR materials | 40+ material functions, status-based colors |
| `vite.config.js` | Dev server config | API proxy, OpenTelemetry pre-bundling |

## Tech Stack

### Backend
- **.NET 9.0** (`net9.0` target framework)
- **C# 12** with implicit usings and nullable reference types enabled
- **ASP.NET Core 9.0** Minimal API
- **Aspire SDK 9.5.0** (via `Aspire.AppHost.Sdk` and NuGet packages)
- **OpenTelemetry** for distributed tracing

### Frontend
- **React 19.1.1** with React DOM 19.1.1
- **Three.js 0.180.0** for WebGL 3D rendering
- **Vite 4.5.5** (pinned to avoid native module issues on Windows)
- **Rollup 3.29.4** (bundler, pinned for stability)
- **Node.js 20+** runtime
- **ESLint 9** with React hooks and refresh plugins

### Orchestration & Build
- **Aspire.Hosting.AppHost 9.5.0** - orchestrates multi-service application
- **Aspire.Hosting.NodeJs 9.5.0** - manages npm-based frontend via `AddNpmApp()`
- **Aspire ServiceDefaults** - shared configuration for OpenTelemetry and service discovery
- **MSBuild `RestoreNpm` target** - automatically runs `npm install` before AppHost builds (if `node_modules` missing)

## Development Workflow

### Running the Application
**This project uses Aspire CLI exclusively - no Docker required, no manual dotnet/npm commands.**

```powershell
cd EnergyBoatApp.AppHost
aspire run                  # Standard run (opens dashboard at http://localhost:15888)
aspire run --watch          # Watch mode - auto-restart on file changes
aspire run --debug          # Enable debug logging to console
aspire run --project <path> # Specify AppHost project if not in directory
```

**Aspire handles all building and dependency restoration automatically:**
- ✅ `npm install` runs via AppHost's `RestoreNpm` MSBuild target (if `node_modules` missing)
- ✅ `dotnet build` handled by Aspire for all .NET projects
- ✅ `npm start` launched by Aspire via `AddNpmApp()` 
- ✅ Service discovery environment variables injected automatically

**Do NOT run manually:**
- ❌ `npm install`, `dotnet build`, `npm start`, or `dotnet run`
- ❌ All dependency management is automated

### Debugging Tools
```powershell
cd EnergyBoatApp.Web
node coordinate-verification.js    # Validates scene positioning from lat/lon
```
