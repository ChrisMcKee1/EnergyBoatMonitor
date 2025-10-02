# ⚡ Energy Boat Service Monitor

A simple .NET Aspire application for visualizing energy service company boat fleet status using Three.js.

## Features

- **Real-time 3D Visualization**: Interactive Three.js scene showing boats color-coded by energy status
- **C# Minimal API**: Lightweight API providing boat data endpoints
- **React Frontend**: Modern React with Vite for fast development
- **.NET Aspire Orchestration**: Simplified local development and deployment

## Architecture

```
EnergyBoatApp/
├── EnergyBoatApp.AppHost/          # Aspire orchestration
├── EnergyBoatApp.ServiceDefaults/  # Shared configuration
├── EnergyBoatApp.ApiService/       # C# Minimal API (boat data)
└── EnergyBoatApp.Web/              # React + Three.js frontend
```

## Prerequisites

- .NET 9 SDK
- Node.js 20+ and npm
- Docker Desktop (for Aspire orchestration)

## Quick Start

### Option 1: With Aspire Orchestration (Requires Docker)

```bash
cd EnergyBoatApp.AppHost
dotnet run
```

Then open the Aspire dashboard URL shown in the terminal.

### Option 2: Manual Start (No Docker Required)

**Terminal 1 - Start API:**
```bash
cd EnergyBoatApp.ApiService
dotnet run
```

**Terminal 2 - Start React Frontend:**
```bash
cd EnergyBoatApp.Web
npm install
npm start
```

Then open http://localhost:5173 in your browser.

## Boat Status Color Coding

- 🟢 **Green** (>70%): Active with good energy
- 🟡 **Yellow** (30-70%): Warning - energy level moderate
- 🔴 **Red** (<30%): Critical - low energy
- 🟠 **Orange**: Maintenance status

## API Endpoints

### GET /api/boats

Returns current boat fleet status:

```json
[
  {
    "id": "BOAT-001",
    "latitude": 51.5074,
    "longitude": -0.1278,
    "status": "Active",
    "energyLevel": 85.5
  }
]
```

## Technology Stack

- **Backend**: C# 12, .NET 9, ASP.NET Core Minimal API
- **Frontend**: React 19, Three.js, Vite
- **Orchestration**: .NET Aspire 9.5
- **3D Graphics**: Three.js for WebGL rendering

## Development

### Adding New Boat Data

Edit `EnergyBoatApp.ApiService/Program.cs` and modify the boats array:

```csharp
var boats = new[]
{
    new BoatStatus("BOAT-001", lat, lng, "Active", energyLevel),
    // Add more boats here
};
```

### Customizing 3D Visualization

Edit `EnergyBoatApp.Web/src/BoatScene.jsx` to modify:
- Boat geometry and materials
- Camera positioning and movement
- Lighting and scene setup

## Building for Production

```bash
dotnet build EnergyBoatApp.sln
```

## Deployment

The React frontend includes a Dockerfile for containerized deployment:

```bash
cd EnergyBoatApp.Web
docker build -t energy-boat-web .
docker run -p 80:80 energy-boat-web
```

## License

MIT License

## Notes

- Simple, non-over-engineered implementation following best practices
- Uses modern .NET and React patterns
- Researched approach leveraging official Microsoft docs and Three.js documentation
- Color-coded visualization provides instant fleet status awareness
