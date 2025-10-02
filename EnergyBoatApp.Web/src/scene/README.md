# 3D Maritime Scene Architecture

## Overview
This document describes the modular architecture for the Energy Boat 3D visualization system using **atomic design principles** to organize Three.js components into maintainable, reusable modules.

## Atomic Design Structure

### Current Implementation

```
src/scene/
├── vessels/                        # Boat components (atomic design)
│   ├── atoms/                      # ✅ Basic geometry building blocks
│   │   ├── Hull.js                 # ExtrudeGeometry hull with cross-section
│   │   └── Deck.js                 # Main deck platform
│   ├── molecules/                  # ✅ Composite components
│   │   ├── Superstructure.js       # Accommodation blocks + bridge assembly
│   │   └── DeckEquipment.js        # Railings + crane with parent-child relationships
│   ├── organisms/                  # ✅ Complete functional units
│   │   └── Mast.js                 # Mast + radar + antennas assembly
│   ├── BoatModel.js                # ✅ Final boat assembly (organism coordinator)
│   ├── BoatMaterials.js            # ✅ PBR materials library (40+ materials)
│   └── BoatEquipment.js            # ✅ Legacy equipment utilities
│
├── infrastructure/                 # ✅ Dock structures (COMPLETE)
│   ├── DockPlatform.js             # Planks, pilings, beams, braces
│   ├── DockEquipment.js            # Cleats, ropes, rings, lights, ladder
│   ├── DockBuilding.js             # Control building, windows, roof
│   └── DockStructure.jsx           # Complete dock assembly
│
├── environment/                    # ✅ Ocean, sky, buoys (COMPLETE)
│   ├── OceanEnvironment.jsx        # Water shader with wave animation
│   ├── SkySystem.jsx               # Preetham sky + day/night toggle
│   └── NavigationBuoys.jsx         # Boundary markers with bobbing
│
├── controls/                       # ✅ Camera & keyboard (COMPLETE)
│   ├── CameraControls.js           # OrbitControls setup & reset
│   ├── KeyboardControls.js         # WASD/arrow key handlers
│   └── ControlsConfig.js           # Control settings & constants
│
├── core/                           # ✅ Scene systems (COMPLETE)
│   ├── SceneSetup.js               # Scene, camera, renderer init
│   ├── LightingSystem.js           # Ambient, sun, fill lights
│   └── AnimationLoop.js            # Render loop, bobbing animations
│
└── utils/                          # ✅ Utilities (COMPLETE)
    ├── CoordinateConverter.js      # Lat/lon ↔ scene coordinates
    ├── Constants.js                # Scene constants (scales, colors)
    └── BoatHelpers.js              # Status colors, heading conversion
```

## Atomic Design Hierarchy

### Atoms (Basic Building Blocks)
**Definition**: Smallest, indivisible components - pure geometry and materials

**Vessels**:
- `Hull.js` - ExtrudeGeometry with boat cross-section (9.5 units length)
- `Deck.js` - Main deck platform (8.5 x 3.2 units)

**Rules**:
- ✅ Export single geometry/mesh creation function
- ✅ Accept material as parameter (or create default)
- ✅ Position relative to local origin (0,0,0)
- ❌ No composite logic or child components

**Example**:
```javascript
// atoms/Hull.js
export function createHull() {
  const hullShape = new THREE.Shape();
  hullShape.moveTo(0, -1.5); // Keel
  // ... define cross-section ...
  
  const geometry = new THREE.ExtrudeGeometry(hullShape, { depth: 9.5 });
  const material = createHullMaterial();
  
  const hull = new THREE.Mesh(geometry, material);
  hull.position.y = -0.5; // Sits IN the water
  return hull;
}
```

### Molecules (Combined Atoms)
**Definition**: Combinations of atoms that form reusable functional units

**Vessels**:
- `Superstructure.js` - Lower accommodation + mid accommodation + bridge
- `DeckEquipment.js` - Railings (all sides) + crane assembly

**Rules**:
- ✅ Combine 2+ atoms or primitives
- ✅ Use Three.js `Group` for assembly
- ✅ Return object with named components: `{ railings, crane }`
- ✅ Implement parent-child relationships (e.g., crane arm → base)
- ❌ No scene-level orchestration

**Example**:
```javascript
// molecules/DeckEquipment.js
export function createDeckEquipment() {
  const railings = new THREE.Group();
  
  // Create railing sections for each side
  const portRailing = createRailingSection(8.5, 'x');
  portRailing.position.set(0, 1.6, -1.6);
  railings.add(portRailing);
  
  // Crane with parent-child relationship
  const crane = new THREE.Group();
  const base = new THREE.Mesh(baseGeom, material);
  base.position.y = baseHeight / 2;
  
  const arm = new THREE.Mesh(armGeom, material);
  arm.position.y = (baseHeight / 2) + (armLength / 2); // Relative to base
  base.add(arm); // Parent arm to base
  
  crane.add(base);
  crane.position.set(-3.5, 1.6, -1.0); // On deck
  
  return { railings, crane };
}
```

### Organisms (Complete Functional Units)
**Definition**: Complex assemblies combining molecules/atoms into self-contained, fully functional components

**Vessels**:
- `Mast.js` - Mast pole + radar dome + antennas + cross-arm
- `BoatModel.js` - **Complete boat assembly** (imports all atoms/molecules)

**Rules**:
- ✅ Combine multiple molecules and atoms
- ✅ Return complete, ready-to-use Three.js `Group`
- ✅ Handle all internal positioning and relationships
- ✅ Accept runtime data (e.g., boat status for color)
- ❌ No scene manipulation or camera control

**Example**:
```javascript
// BoatModel.js (organism-level coordinator)
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
  
  // Assemble
  boatModel.add(hull, deck, superstructure, railings, crane);
  
  // Rotation and world positioning handled by BoatScene.jsx
  return boatModel;
}
```

### Templates (Page Orchestrators)
**Definition**: High-level scene coordinators that position organisms and handle runtime behavior

**Current**:
- `BoatScene.jsx` - Main scene orchestrator (~300 lines)
  - Manages scene setup, camera, lighting
  - Positions boats based on API data (lat/lon → scene coords)
  - Applies heading rotation: `boatMesh.rotation.y = headingToRotation(heading)`
  - Handles user interactions (selection, controls)

**Rules**:
- ✅ Import organisms, not atoms/molecules
- ✅ Handle world positioning and rotation
- ✅ Manage scene lifecycle (mount, unmount, updates)
- ✅ Connect to API data and user interactions
- ❌ NEVER modify component geometry - that's atom/molecule/organism responsibility

## Critical Development Rules

### 1. Modification Hierarchy (STRICT)
## Critical Development Rules

### 1. Modification Hierarchy (STRICT)
**NEVER modify geometry in `BoatScene.jsx` or `BoatModel.js`** - they are orchestrators only.

```
Changes flow BOTTOM-UP:
  Atoms (Hull.js, Deck.js)
    ↓
  Molecules (Superstructure.js, DeckEquipment.js)
    ↓
  Organisms (BoatModel.js, Mast.js)
    ↓
  Templates (BoatScene.jsx)
```

**Example**: To adjust crane position:
1. ❌ **WRONG**: Modify crane position in `BoatScene.jsx`
2. ❌ **WRONG**: Modify crane position in `BoatModel.js`
3. ✅ **CORRECT**: Modify crane assembly in `molecules/DeckEquipment.js`

### 2. Parent-Child Relationships
**Critical for rotation inheritance:**

```javascript
// ❌ WRONG - components float independently
crane.add(base);
crane.add(arm); // arm has NO relationship to base

// ✅ CORRECT - arm inherits base rotation
const base = new THREE.Mesh(baseGeom, material);
base.position.y = baseHeight / 2;

const arm = new THREE.Mesh(armGeom, material);
arm.position.y = (baseHeight / 2) + (armLength / 2); // Relative to parent
base.add(arm); // Parent arm to base - now they rotate together

crane.add(base); // Add assembled unit to scene
```

### 3. Position Logic Location
**Position definitions belong in component files, NOT in scene orchestrator:**

```javascript
// ❌ WRONG - hardcoded in BoatScene.jsx
boatMesh.add(crane);
crane.position.set(-3.5, 1.6, -1.0);

// ✅ CORRECT - defined in molecules/DeckEquipment.js
export function createDeckEquipment() {
  const crane = new THREE.Group();
  // ... assemble crane ...
  crane.position.set(-3.5, 1.6, -1.0); // Defined here
  return { railings, crane };
}
```

### 4. Coordinate System (Dock-Centered)
**Scene origin (0,0,0) is the DOCK** - all spatial calculations are relative to this:

```javascript
// Frontend conversion (CoordinateConverter.js)
x = (longitude - DOCK_LON) * SCALE_FACTOR_LON    // West(-) to East(+)
z = -(latitude - DOCK_LAT) * SCALE_FACTOR_LAT    // North(-) to South(+)

// Dock location: (51.5100, -0.1350) → scene (0, 0, 0)
// Scale: 2000 units/degree ≈ 55 meters/unit
```

### 5. Heading & Rotation System
**Nautical heading (degrees) → Three.js rotation (radians):**

```javascript
// Coordinate mapping:
// Nautical: 0°=North, 90°=East, 180°=South, 270°=West
// Scene: -Z=North, +X=East, +Z=South, -X=West
// Boat default: Bow points +X (East)

// Conversion (BoatHelpers.js)
function headingToRotation(heading) {
  const rotationDegrees = 90 - heading;
  return THREE.MathUtils.degToRad(rotationDegrees);
}

// Application (BoatScene.jsx)
const heading = boat.heading || boat.Heading || 0;
boatMesh.rotation.y = headingToRotation(heading);
```

## Common Development Tasks

### Adding a New Boat Component

**Step 1: Create Atom (if needed)**
```javascript
// atoms/Funnel.js
export function createFunnel() {
  const geometry = new THREE.CylinderGeometry(0.35, 0.42, 1.2, 16);
  const material = createFunnelMaterial();
  const funnel = new THREE.Mesh(geometry, material);
  funnel.position.set(0, 5.95, -0.5); // Behind bridge
  funnel.castShadow = true;
  return funnel;
}
```

**Step 2: Add to Molecule (if composite)**
```javascript
// molecules/Superstructure.js
import { createFunnel } from '../atoms/Funnel.js';

export function createSuperstructure() {
  const superstructure = new THREE.Group();
  // ... existing components ...
  
  const funnel = createFunnel();
  superstructure.add(funnel);
  
  return superstructure;
}
```

**Step 3: Organism automatically includes it**
```javascript
// BoatModel.js - no changes needed if funnel is in Superstructure
const superstructure = createSuperstructure(); // Already includes funnel
boatModel.add(superstructure);
```

### Modifying Existing Component Dimensions

**Wrong approach** (breaks atomic design):
```javascript
// ❌ BoatScene.jsx
boatMesh.traverse(child => {
  if (child.name === 'deck') {
    child.scale.set(1.2, 1, 1); // Hardcoded scale in scene
  }
});
```

**Correct approach** (modify at source):
```javascript
// ✅ atoms/Deck.js
export function createDeck() {
  const geometry = new THREE.BoxGeometry(10.2, 0.2, 3.2); // Changed from 8.5
  const material = new THREE.MeshStandardMaterial({
    color: 0x777777,
    metalness: 0.5,
    roughness: 0.7
  });
  const deck = new THREE.Mesh(geometry, material);
  deck.position.y = 1.5;
  deck.receiveShadow = true;
  return deck;
}
```

### Adding Status-Based Rendering

Status logic belongs in **template** (BoatScene.jsx), materials in **vessel modules**:

```javascript
// BoatMaterials.js - Define materials
export function createHullMaterial(status = 'Active') {
  const color = status === 'Maintenance' ? 0x888888 : 0xFF4500;
  return new THREE.MeshStandardMaterial({ color, metalness: 0.8, roughness: 0.3 });
}

// BoatScene.jsx - Apply at runtime
const hull = boatMesh.getObjectByName('hull');
if (hull && boat.status !== previousStatus) {
  hull.material.dispose();
  hull.material = createHullMaterial(boat.status);
}
```

## Testing & Debugging

### Coordinate Verification
```powershell
cd EnergyBoatApp.Web
node coordinate-verification.js    # Shows waypoint positions in scene units
```

### Common Issues

**Boats oscillating at waypoints**:
- Check dynamic threshold: `0.15 + (distanceTraveled * 1.5)` nautical miles
- Reduce `speedMultiplier` or increase waypoint spacing
- Backend: `Program.cs` line ~310-320

**Components not positioned correctly**:
- Verify position defined in component file, not `BoatScene.jsx`
- Check parent-child relationships for relative positioning
- Use local origin (0,0,0) as reference, not world coordinates

**Boats passing through dock**:
- Routes in `_boatRoutes` must avoid dock coords `(51.5100, -0.1350)`
- Keep northern routes above lat 51.5150, southern below 51.5010
- Backend: `Program.cs` line ~70-100

### Status-Based Rendering Verification
Boats change appearance by energy level:
- **Active** (>70%): Orange hull (`0xFF4500`), green lights, moving
- **Charging** (20-30%): Orange hull, yellow lights, stationary
- **Maintenance**: Gray hull (`0x888888`), red lights, stationary

## Architecture Benefits

### Maintainability
- ✅ Each file < 200 lines (easy to understand)
- ✅ Clear separation of concerns
- ✅ Easy bug localization

### Reusability
- ✅ Atoms can be recombined into different molecules
- ✅ Molecules can form different organisms
- ✅ Organisms can be reused in other maritime projects

### Testability
- ✅ Individual components can be unit tested
- ✅ Easy to mock dependencies
- ✅ Isolated bug reproduction

### Performance
- ✅ Tree-shaking removes unused code
- ✅ Easy to identify bottlenecks
- ✅ Lazy-loading for heavy components

## File Size Metrics

### Before Refactoring
- `BoatScene.jsx`: 1478 lines (monolithic)

### After Refactoring
- `BoatScene.jsx`: 374 lines (75% reduction)
- 20 focused modules (avg 120 lines each)
- Total reduction: ~50% code duplication eliminated

## Migration Status

### ✅ Phase 1: Utilities (COMPLETE)
- `Constants.js` - Scene constants
- `CoordinateConverter.js` - Lat/lon conversions
- `BoatHelpers.js` - Status colors, heading conversion

### ✅ Phase 2: Environment (COMPLETE)
- `OceanEnvironment.jsx` - Water shader + animation
- `SkySystem.jsx` - Sky + day/night toggle
- `NavigationBuoys.jsx` - Boundary markers

### ✅ Phase 3: Vessels (COMPLETE - Atomic Design)
- `atoms/Hull.js` - ExtrudeGeometry hull
- `atoms/Deck.js` - Main deck platform
- `molecules/Superstructure.js` - Accommodation + bridge
- `molecules/DeckEquipment.js` - Railings + crane
- `organisms/Mast.js` - Mast assembly
- `BoatModel.js` - Final boat coordinator
- `BoatMaterials.js` - PBR materials (40+)
- `BoatEquipment.js` - Legacy utilities

### ✅ Phase 4: Infrastructure (COMPLETE)
- `DockPlatform.js` - Planks, pilings, beams
- `DockEquipment.js` - Cleats, ropes, lights
- `DockBuilding.js` - Control building
- `DockStructure.jsx` - Dock assembly

### ✅ Phase 5: Controls (COMPLETE)
- `CameraControls.js` - OrbitControls
- `KeyboardControls.js` - WASD handlers
- `ControlsConfig.js` - Settings

### ✅ Phase 6: Core (COMPLETE)
- `SceneSetup.js` - Scene, camera, renderer
- `LightingSystem.js` - Ambient, sun, fill lights
- `AnimationLoop.js` - Render loop, bobbing

## Next Steps

1. ✅ All phases complete
2. 🔄 Ongoing: Refine atomic design based on new features
3. 🔄 Ongoing: Document new components in this README
4. ⏳ Future: Add TypeScript type definitions for better IntelliSense

---

**Last Updated**: October 2, 2025  
**Status**: ✅ Refactoring Complete - Atomic Design Implemented  
**Maintainer**: Energy Boat Development Team
