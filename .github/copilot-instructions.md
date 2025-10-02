# Copilot Instructions for Energy Boat Monitor

## Project Overview
.NET Aspire 9.5 application with C# Minimal API backend and React+Three.js frontend for real-time 3D maritime fleet visualization with autonomous boat navigation simulation.

## Architecture & Data Flow

### Service Communication Pattern
- **Aspire orchestration** (`EnergyBoatApp.AppHost/AppHost.cs`): Coordinates backend API + npm-based Vite frontend
- **API proxy**: Vite proxies `/api/*` to backend via `services__apiservice__https__0` environment variable (see `vite.config.js`)
- **Backend simulation**: `BoatSimulator` singleton maintains state, calculates waypoint navigation using Haversine formula
- **Frontend polling**: React fetches `/api/boats?speed={multiplier}` every 2 seconds, updates Three.js scene

### Critical Coordinate System (DOCK-CENTERED)
**The scene origin (0,0,0) is the DOCK, not an arbitrary center point.** This is crucial for all coordinate calculations:

```javascript
// Frontend (CoordinateConverter.js):
x = (longitude - DOCK_LON) * SCALE_FACTOR_LON    // West(-) to East(+)
z = -(latitude - DOCK_LAT) * SCALE_FACTOR_LAT    // North(-) to South(+), negative Z for northward

// Backend (Program.cs) uses Haversine and returns nautical miles
```

**Dock location**: `(51.5100, -0.1350)` maps to scene `(0, 0, 0)`  
**Scale**: 2000 units per degree = 1 scene unit ≈ 55 meters  
**Bounds**: lat 51.48-51.53, lon -0.16 to -0.09 (expanded to -0.09 to prevent waypoint boundary collisions)

### Waypoint Navigation System
**Backend simulation** (`Program.cs` line ~290-320):
- Boats follow routes defined in `_boatRoutes` dictionary (must avoid dock at 51.5100, -0.1350)
- **Dynamic threshold**: `0.15 + (distanceTraveled * 1.5)` nautical miles prevents overshooting at high speeds
- `CalculateDistance()` returns **nautical miles**, `CalculateHeading()` returns **degrees**
- Speed multiplier (1-10x) affects `distanceTraveled` and waypoint detection threshold

**Critical**: When adding/modifying waypoints, ensure routes do NOT pass through dock coordinates.

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

**Important**: Aspire handles all building and dependency restoration automatically. Do NOT run:
- ❌ `dotnet build` (Aspire builds for you)
- ❌ `npm install` or `npm start` (Aspire manages npm lifecycle)
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

## Project-Specific Conventions

### Environment Variables & Telemetry
**Vite blocks non-VITE_ prefixed env vars from browser.** Aspire provides `OTEL_EXPORTER_OTLP_ENDPOINT` but Vite can't access it.

**Solution**: API endpoint bridge at `/api/telemetry/config` exposes OTEL vars to frontend (see `VITE_ASPIRE_INTEGRATION.md`)

### Three.js Scene Refactoring Pattern
Following atomic design in `src/scene/`:
- **Atoms**: `scene/vessels/BoatGeometry.js`, `scene/infrastructure/DockPlatform.js`
- **Molecules**: `scene/vessels/BoatEquipment.js`, `scene/infrastructure/DockBuilding.js`
- **Organisms**: `scene/vessels/BoatModel.jsx`, `scene/environment/OceanEnvironment.jsx`
- **Template**: `components/BoatScene.jsx` (orchestrator)

See `src/scene/README.md` for complete refactoring strategy.

### Status-Based Rendering
Boats change color/behavior based on energy level:
- **Active** (>70%): Green lights, full speed
- **Charging** (20-30%): Yellow lights, stationary at dock
- **Maintenance**: Gray, stationary

Status logic in backend `UpdateBoatPositions()` triggers state transitions.

## Common Patterns

### Adding a New Boat
1. Add waypoint route in `Program.cs` `_boatRoutes` dictionary (avoid dock coordinates)
2. Add initial state in `_boatStates` list with `SpeedKnots`, `EnergyLevel`
3. Frontend automatically renders based on API response
4. Test waypoint navigation at 10x speed multiplier to verify no oscillation

### Modifying Scene Constants
All visual constants in `Constants.js`: `SCALE_FACTOR`, `BOUNDS`, `DOCK_LAT/LON`, `BOAT_COLORS`

**Never hardcode coordinates** - always use `latLonToSceneCoords()` for consistency.

### Speed Multiplier Debugging
If boats oscillate between waypoints:
1. Check threshold calculation: `0.15 + (distanceTraveled * 1.5)` must exceed distance per update
2. Verify waypoints aren't too close (<0.3 nautical miles at 10x speed)
3. Confirm `CalculateDistance()` returns nautical miles, not degrees

## Key Files Reference

| File | Purpose | Critical Details |
|------|---------|------------------|
| `AppHost.cs` | Aspire orchestration | Npm app setup, API reference, OTEL config |
| `Program.cs` (API) | Boat simulation logic | Waypoint navigation (line 290-320), Haversine distance (line 354) |
| `BoatScene.jsx` | Main 3D orchestrator | Boat positioning, status updates, coordinate conversion |
| `Constants.js` | Scene configuration | DOCK_LAT/LON (origin), SCALE_FACTOR, BOUNDS |
| `CoordinateConverter.js` | Lat/lon ↔ scene coords | **Dock-centered** conversion (not CENTER_LAT/LON) |
| `vite.config.js` | Dev server config | API proxy, OpenTelemetry pre-bundling |

## Integration Points

### Backend → Frontend
- `/api/boats?speed={1-10}`: Returns boat array with `latitude`, `longitude`, `status`, `energyLevel`, `heading`
- `/api/boats/reset`: POST resets all boats to initial positions
- `/api/telemetry/config`: Exposes OTEL endpoint for browser telemetry

### Frontend Three.js Pipeline
1. Fetch boat data → 2. `latLonToSceneCoords()` → 3. Update mesh positions → 4. Apply animations (bobbing, rotation)

### Aspire Dashboard Integration
- OpenTelemetry browser traces sent to Aspire OTLP collector
- Service names: `apiservice`, `webfrontend`, `browser`
- Logs/traces visible in Aspire dashboard at http://localhost:15888

## Troubleshooting

### Boats passing through dock
Routes in `_boatRoutes` must avoid (51.5100, -0.1350). Keep northern routes above lat 51.5150, southern routes below 51.5010.

### Waypoint oscillation
Reduce `speedMultiplier` or increase waypoint spacing. Dynamic threshold should be > distance traveled per update cycle.

### Vite build errors with native modules
Using Vite 4.5.5 + Rollup 3.29.4 (not latest) to avoid Windows native module issues. See `package.json` pinned versions.
