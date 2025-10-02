# Energy Boat Monitor - Frontend

React + Three.js frontend for real-time 3D maritime fleet visualization with Vite as the build tool.

## Architecture

### Atomic Design Pattern
This project follows **strict atomic design principles** for Three.js components:

#### Hierarchy
```
src/scene/vessels/
├── atoms/                    # Basic geometry building blocks
│   ├── Hull.js               # ExtrudeGeometry hull with cross-section
│   └── Deck.js               # Main deck platform
├── molecules/                # Composite components
│   ├── Superstructure.js     # Accommodation + bridge assembly
│   └── DeckEquipment.js      # Railings + crane with parent-child relationships
├── organisms/                # Complete functional units
│   └── Mast.js               # Mast + radar + antennas
├── BoatModel.js              # Final assembly (organism-level coordinator)
└── BoatMaterials.js          # PBR materials library
```

#### Critical Rules
1. **NEVER modify geometry in `BoatScene.jsx` or `BoatModel.js`** - they are orchestrators only
2. **Changes flow bottom-up**: Atoms → Molecules → Organisms → Scene
3. **Parent-child relationships**: Use `.add()` for proper rotation/positioning inheritance
4. **Position logic**: Belongs in component definition, not in scene orchestrator

### Coordinate System (Dock-Centered)
**Scene origin (0,0,0) is the DOCK** - critical for all spatial calculations:

```javascript
// Conversion from geographic to scene coordinates
x = (longitude - DOCK_LON) * SCALE_FACTOR_LON    // West(-) to East(+)
z = -(latitude - DOCK_LAT) * SCALE_FACTOR_LAT    // North(-) to South(+)
```

**Dock Location**: `(51.5100, -0.1350)` maps to scene `(0, 0, 0)`  
**Scale**: 2000 units per degree ≈ 55 meters per unit  
**Bounds**: lat 51.48-51.53, lon -0.16 to -0.09

### Heading & Rotation System
**Nautical to Three.js conversion**:
- Nautical: 0° = North, 90° = East, 180° = South, 270° = West
- Scene: -Z = North, +X = East, +Z = South, -X = West
- Boat default: Bow points +X (East)

```javascript
// BoatHelpers.js - headingToRotation()
rotation.y = THREE.MathUtils.degToRad(90 - heading);
```

### Technology Stack
- **React 19.1.1** - UI framework
- **Three.js 0.180.0** - WebGL 3D rendering
- **Vite 4.5.5** - Build tool (pinned to avoid native module issues on Windows)
- **Rollup 3.29.4** - Bundler (pinned for stability)
- **ESLint 9** - Linting with React hooks plugin

## Development

### Running the Frontend
**Do NOT run this manually** - Aspire orchestrates the entire app:

```powershell
cd ../EnergyBoatApp.AppHost
aspire run
```

Aspire will:
1. Install npm dependencies automatically (via `RestoreNpm` MSBuild target)
2. Start Vite dev server on a dynamically assigned port
3. Configure API proxy via `services__apiservice__https__0` environment variable
4. Open dashboard at `http://localhost:15888`

### File Organization
```
src/
├── components/
│   ├── BoatScene.jsx         # Main 3D scene orchestrator (~300 lines)
│   ├── SceneControls.jsx     # UI controls (speed, reset, day/night)
│   └── SceneControls.css
├── scene/                    # Atomic design 3D components
│   ├── vessels/              # Boat components (atoms/molecules/organisms)
│   ├── infrastructure/       # Dock structures
│   ├── environment/          # Ocean, sky, buoys
│   ├── controls/             # Camera and keyboard controls
│   ├── core/                 # Scene setup, lighting, animation loop
│   └── utils/                # Coordinate conversion, constants, helpers
├── App.jsx                   # Root component with dashboard
├── App.css                   # Contoso-Sea branding styles
├── main.jsx                  # Entry point
└── telemetry.js              # OpenTelemetry browser instrumentation
```

## Key Patterns

### Adding/Modifying Boat Components
**Always follow atomic design hierarchy:**

```javascript
// 1. Atom-level (geometry only) - atoms/Hull.js
export function createHull() {
  const hullShape = new THREE.Shape();
  // Define cross-section...
  return new THREE.Mesh(geometry, material);
}

// 2. Molecule-level (composite) - molecules/DeckEquipment.js
export function createDeckEquipment() {
  const railings = createRailingSection(length, orientation);
  const crane = createCraneAssembly();
  return { railings, crane };
}

// 3. Organism-level (assembly) - BoatModel.js
import { createHull } from './atoms/Hull.js';
import { createDeckEquipment } from './molecules/DeckEquipment.js';

const hull = createHull();
const { railings, crane } = createDeckEquipment();
boatModel.add(hull, railings, crane);
```

### Parent-Child Relationships
**Critical for proper rotation inheritance:**

```javascript
// CORRECT - arm inherits base rotation
const base = new THREE.Mesh(baseGeom, material);
base.position.y = baseHeight / 2;

const arm = new THREE.Mesh(armGeom, material);
arm.position.y = (baseHeight / 2) + (armLength / 2); // Relative to parent
base.add(arm); // Parent arm to base

crane.add(base); // Add assembled unit to scene
```

### Coordinate Conversion
**Always use the converter** - never hardcode lat/lon calculations:

```javascript
import { CoordinateConverter } from './scene/utils/CoordinateConverter';
const scenePos = CoordinateConverter.latLonToScene({ latitude, longitude });
```

## Debugging Tools

### Coordinate Verification
```powershell
cd EnergyBoatApp.Web
node coordinate-verification.js    # Shows waypoint/boundary positions in scene units
```

### Common Issues

**Boats oscillating between waypoints**:
- Check dynamic threshold: `0.15 + (distanceTraveled * 1.5)` nautical miles
- Reduce `speedMultiplier` or increase waypoint spacing
- Verify `CalculateDistance()` returns nautical miles (not degrees)

**Boats passing through dock**:
- Routes in `_boatRoutes` must avoid `(51.5100, -0.1350)`
- Keep northern routes above lat 51.5150, southern routes below 51.5010

**Component not positioned correctly**:
- Check position is defined in component file, not in `BoatScene.jsx`
- Verify parent-child relationships for relative positioning
- Confirm all positions are relative to local origin, not world coordinates

## Build Configuration

### Vite Config (`vite.config.js`)
- **API proxy**: `/api/*` routes to backend via `services__apiservice__https__0`
- **Port**: Dynamically assigned by Aspire (stored in `PORT` env var)
- **OpenTelemetry**: Pre-bundled to avoid browser import errors

### Pinned Versions
- Vite 4.5.5 (not 5.x) - avoids Windows native module issues
- Rollup 3.29.4 - bundler stability

## Testing

### Status-Based Rendering Tests
Boats change appearance based on energy level:
- **Active** (>70%): Orange hull (`0xFF4500`), green lights, moving
- **Charging** (20-30%): Orange hull, yellow lights, stationary
- **Maintenance**: Gray hull (`0x888888`), red lights, stationary

### Navigation System Tests
1. **Heading accuracy**: Boats face correct direction (0°=North, 90°=East)
2. **Waypoint following**: No oscillation at 10x speed
3. **Coordinate conversion**: Scene positions match lat/lon accurately

## References

- [Three.js Documentation](https://threejs.org/docs/)
- [Atomic Design Methodology](https://bradfrost.com/blog/post/atomic-web-design/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- Scene Architecture: `src/scene/README.md`
- Copilot Instructions: `../.github/copilot-instructions.md`
