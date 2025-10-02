# ⚡ Energy Boat Monitor

A real-time 3D maritime fleet visualization application built with .NET Aspire 9.5, featuring autonomous boat navigation simulation with C# backend and React + Three.js frontend.

![Maritime Fleet Visualization](Fugro-Blue-Essence-ship-and-Fugro-Blue-Volta-ROV.jpg)

## 🚀 Features

- **Real-time 3D Visualization**: Interactive Three.js scene with WebGL rendering
- **Autonomous Navigation**: Boats follow predefined waypoint routes using Haversine formula
- **Dynamic Threshold System**: Adaptive waypoint detection prevents oscillation at high speeds
- **Status-based Rendering**: Color-coded boats based on energy levels (Active, Charging, Maintenance)
- **Speed Multiplier**: 1x-10x simulation speed for testing navigation algorithms
- **Dock-Centered Coordinates**: Scene origin at dock position for realistic spatial relationships
- **OpenTelemetry Integration**: Browser and backend telemetry sent to Aspire dashboard

## 🏗️ Architecture

```
EnergyBoatApp/
├── EnergyBoatApp.AppHost/          # .NET Aspire orchestration
├── EnergyBoatApp.ServiceDefaults/  # Shared Aspire configuration
├── EnergyBoatApp.ApiService/       # C# Minimal API (boat simulation)
└── EnergyBoatApp.Web/              # React + Three.js frontend (Vite)
```

### Technology Stack

- **Backend**: C# 12, .NET 9, ASP.NET Core Minimal API
- **Frontend**: React 19, Three.js, Vite 4.5.5
- **Orchestration**: .NET Aspire 9.5 (no Docker required)
- **3D Graphics**: Three.js with custom shaders (ocean, sky)
- **Navigation**: Haversine distance calculation, dynamic waypoint threshold

## 🎯 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling): `dotnet workload install aspire`
- Node.js 20+ and npm (for frontend)

### Running the Application

```powershell
cd EnergyBoatApp.AppHost
aspire run
```

The Aspire dashboard will open at `http://localhost:15888` showing:
- **webfrontend** - React app at `http://localhost:5173`
- **apiservice** - C# API at `https://localhost:7585`
- **Logs, Traces, Metrics** - OpenTelemetry data from both services

### Aspire CLI Options

```powershell
aspire run           # Standard run
aspire run --watch   # Auto-restart on file changes
aspire run --debug   # Enable debug logging
```

**Important**: Aspire handles all building and dependency restoration. Do NOT run `dotnet build` or `npm install` manually.

## 🗺️ Key Concepts

### Coordinate System (Dock-Centered)

The 3D scene origin `(0, 0, 0)` is positioned at the **dock**, not an arbitrary center:

```javascript
// Frontend coordinate conversion
x = (longitude - DOCK_LON) * 2000    // 2000 units per degree
z = -(latitude - DOCK_LAT) * 2000    // Negative Z = north
```

- **Dock Location**: `(51.5100°N, -0.1350°W)` → Scene `(0, 0, 0)`
- **Scale**: 1 scene unit ≈ 55 meters
- **Bounds**: lat 51.48-51.53, lon -0.16 to -0.09

### Waypoint Navigation

Boats follow routes defined in `Program.cs` using:
- **Haversine formula** for distance (returns nautical miles)
- **Dynamic threshold**: `0.15 + (distanceTraveled × 1.5)` nautical miles
- Routes must **avoid dock coordinates** `(51.5100, -0.1350)`

### Status-Based Rendering

| Status | Energy Level | Color | Behavior |
|--------|--------------|-------|----------|
| **Active** | >70% | 🟢 Green lights | Full speed navigation |
| **Charging** | 20-30% | 🟡 Yellow lights | Stationary at dock |
| **Maintenance** | Any | ⚪ Gray | Stationary |

## 📡 API Endpoints

### GET `/api/boats?speed={1-10}`

Returns current fleet status with optional speed multiplier:

```json
[
  {
    "id": "BOAT-001",
    "latitude": 51.5170,
    "longitude": -0.1278,
    "status": "Active",
    "energyLevel": 85.5,
    "heading": 45.2,
    "speedKnots": 12.0
  }
]
```

### POST `/api/boats/reset`

Resets all boats to initial positions.

### GET `/api/telemetry/config`

Exposes OpenTelemetry endpoint for browser telemetry (bypasses Vite env var restrictions).

## 🎨 Three.js Scene Architecture

Following **atomic design principles**:

```
src/scene/
├── vessels/          # BoatGeometry, BoatEquipment, BoatModel
├── infrastructure/   # DockPlatform, DockBuilding, DockEquipment
├── environment/      # OceanEnvironment, SkySystem, NavigationBuoys
├── controls/         # CameraControls, KeyboardControls
└── utils/            # CoordinateConverter, Constants, Helpers
```

See [`EnergyBoatApp.Web/src/scene/README.md`](EnergyBoatApp.Web/src/scene/README.md) for refactoring strategy.

## 🔧 Development

### Project Structure

- **`AppHost.cs`**: Aspire orchestration with npm app configuration
- **`Program.cs`** (API): Boat simulation logic, waypoint navigation (lines 290-320)
- **`BoatScene.jsx`**: Main Three.js orchestrator
- **`Constants.js`**: Scene configuration (DOCK_LAT/LON, SCALE_FACTOR, BOUNDS)
- **`CoordinateConverter.js`**: Dock-centered lat/lon ↔ scene coordinate conversion

### Adding a New Boat

1. Add waypoint route in `Program.cs` `_boatRoutes` dictionary:
   ```csharp
   ["BOAT-005"] = new List<Waypoint>
   {
       new Waypoint(51.5200, -0.1450), // Must avoid dock at 51.5100, -0.1350
       new Waypoint(51.5250, -0.1300),
       // ...
   }
   ```

2. Add initial state in `_boatStates` list with `SpeedKnots`, `EnergyLevel`

3. Frontend automatically renders based on API response

### Debugging Coordinate Issues

```powershell
cd EnergyBoatApp.Web
node coordinate-verification.js
```

Shows waypoint/boundary positions in scene units to verify spatial relationships.

## 📚 Documentation

- **[`.github/copilot-instructions.md`](.github/copilot-instructions.md)** - AI agent instructions
- **[`VITE_ASPIRE_INTEGRATION.md`](VITE_ASPIRE_INTEGRATION.md)** - Vite environment variable workaround
- **[`BROWSER_TELEMETRY_SETUP.md`](BROWSER_TELEMETRY_SETUP.md)** - OpenTelemetry browser setup
- **[`INTERACTIVE_FEATURES.md`](INTERACTIVE_FEATURES.md)** - Scene controls and interactions

## 🐛 Troubleshooting

### Boats passing through dock
Routes in `_boatRoutes` must avoid `(51.5100, -0.1350)`. Keep northern routes above lat 51.5150, southern routes below 51.5010.

### Waypoint oscillation
Dynamic threshold `0.15 + (distanceTraveled × 1.5)` must exceed distance per update cycle. Verify waypoints aren't too close (<0.3 nautical miles at 10x speed).

### Vite build errors
Using pinned Vite 4.5.5 + Rollup 3.29.4 to avoid Windows native module issues. See `package.json`.

## 🤝 Contributing

This project demonstrates:
- ✅ .NET Aspire multi-service orchestration without Docker
- ✅ Vite + React SPA with Aspire integration
- ✅ OpenTelemetry browser → backend → dashboard pipeline
- ✅ Three.js atomic design refactoring pattern
- ✅ Dynamic navigation algorithms with real-time simulation

## 📝 License

This project is for demonstration purposes.

---

**Built with** [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) • [Three.js](https://threejs.org/) • [Vite](https://vitejs.dev/)
