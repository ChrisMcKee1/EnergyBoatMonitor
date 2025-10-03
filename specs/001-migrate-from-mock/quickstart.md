# Quickstart: PostgreSQL Migration Development

**Feature**: 001-migrate-from-mock  
**Date**: October 3, 2025  
**Audience**: Developers working on the PostgreSQL migration

---

## Overview

This guide helps developers set up their environment, run the migrated application locally, and verify the PostgreSQL integration works correctly.

**Estimated Time**: 15 minutes  
**Difficulty**: Intermediate

---

## Prerequisites

### Required Software

| Tool | Version | Download | Purpose |
|------|---------|----------|---------|
| **.NET SDK** | 9.0 or later | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) | Backend runtime |
| **Aspire CLI** | 9.5.0 | `dotnet tool install -g aspire` | Orchestration |
| **Docker Desktop** | Latest | [docker.com](https://www.docker.com/products/docker-desktop) | PostgreSQL container |
| **Node.js** | 20.x LTS | [nodejs.org](https://nodejs.org) | Frontend build (auto-managed by Aspire) |
| **Git** | 2.x | [git-scm.com](https://git-scm.com) | Version control |

**Verify Installation**:

```powershell
dotnet --version        # Expected: 9.0.x
aspire --version        # Expected: 9.5.0
docker --version        # Expected: Docker version 24.x or later
node --version          # Expected: v20.x.x
```

### Recommended Tools

- **pgAdmin** (optional): Pre-configured via Aspire `WithPgAdmin()` at `http://localhost:5050`
- **VS Code** or **Visual Studio 2022**: IDE with C# support
- **PowerShell 7+**: Modern shell experience (Windows)

---

## Step 1: Clone Repository & Checkout Branch

```powershell
# Navigate to workspace
cd c:\Users\chrismckee\Documents\GitHub

# Clone repository (if not already cloned)
git clone https://github.com/your-org/Demo2.git
cd Demo2

# Checkout feature branch
git checkout 001-migrate-from-mock

# Verify branch
git branch --show-current  # Should show: 001-migrate-from-mock
```

---

## Step 2: Configure User Secrets

Aspire uses User Secrets for local development credentials. Configure PostgreSQL username/password:

```powershell
# Navigate to AppHost project
cd EnergyBoatApp.AppHost

# Initialize user secrets (if not already initialized)
dotnet user-secrets init

# Set PostgreSQL credentials
dotnet user-secrets set "Parameters:postgres-username" "admin"
dotnet user-secrets set "Parameters:postgres-password" "YourSecurePassword123!"

# Verify secrets
dotnet user-secrets list
```

**Expected Output**:

```text
Parameters:postgres-username = admin
Parameters:postgres-password = YourSecurePassword123!
```

**Why User Secrets?**
- Keeps credentials out of source control
- Works seamlessly with Aspire `AddParameter(secret: true)`
- Automatically injected as environment variables

---

## Step 3: Start Docker Desktop

Ensure Docker Desktop is running before starting Aspire:

```powershell
# Check Docker is running
docker ps
```

**Expected Output**: Table of running containers (may be empty)

**If Docker is not running**:
1. Open Docker Desktop application
2. Wait for "Docker Desktop is running" notification
3. Retry `docker ps`

---

## Step 4: Run Application with Aspire

Aspire handles all orchestration (PostgreSQL container, pgAdmin, API, frontend):

```powershell
# From repository root
cd EnergyBoatApp.AppHost

# Run Aspire
aspire run
```

**What happens automatically:**

1. Aspire reads `AppHost.cs` configuration
2. Pulls PostgreSQL 16 image (first run only, ~5 minutes)
3. Pulls pgAdmin 4 image (first run only, ~2 minutes)
4. Creates Docker volume for persistent data
5. Starts PostgreSQL container with credentials from User Secrets
6. Starts pgAdmin container on port 5050
7. Builds and runs ApiService (.NET backend)
8. Runs `npm install` (first run only, via `RestoreNpm` target)
9. Starts Web frontend (Vite dev server)
10. Opens Aspire dashboard at `http://localhost:15888`

**Expected Output**:

```text
Building...
Aspire.Hosting.DistributedApplication[0]
      Now listening on: http://localhost:15888
Aspire.Hosting.DistributedApplication[0]
      Aspire version: 9.5.0+abc123
info: Aspire.Hosting.PostgresServer[0]
      Resource postgres started with endpoint(s) localhost:5432
info: Aspire.Hosting.PgAdmin[0]
      Resource postgres-pgadmin started with endpoint(s) localhost:5050
info: Aspire.Hosting.ProjectRunnerService[0]
      Resource apiservice started
info: Aspire.Hosting.NpmApp[0]
      Resource webfrontend started with endpoint(s) localhost:5173
```

---

## Step 5: Verify Aspire Dashboard

Open browser to `http://localhost:15888`:

**Expected Resources**:

| Resource | Type | Status | Endpoint |
|----------|------|--------|----------|
| `postgres` | Container | Running | `localhost:5432` |
| `postgres-pgadmin` | Container | Running | `http://localhost:5050` |
| `ContosoSeaDB` | Database | Running | (internal) |
| `apiservice` | Project | Running | `https://localhost:7585` |
| `webfrontend` | NPM App | Running | `http://localhost:5173` |

**Actions in Dashboard**:

- Click **Traces** to view OpenTelemetry traces
- Click **Logs** to view aggregated logs from all services
- Click **Metrics** to view performance metrics

---

## Step 6: Access pgAdmin Web UI

pgAdmin is automatically configured by Aspire's `WithPgAdmin()` method.

### 6.1 Open pgAdmin

Navigate to `http://localhost:5050`

**Default Credentials** (Aspire-configured):

- Email: `admin@admin.com`
- Password: `admin`

### 6.2 Connect to PostgreSQL Server

1. **Right-click "Servers"** → **Register** → **Server**
2. **General Tab**:
   - Name: `ContosoSeaDB`
3. **Connection Tab**:
   - Host: `postgres` (Aspire service name, not `localhost`)
   - Port: `5432`
   - Maintenance database: `postgres`
   - Username: `admin` (from User Secrets)
   - Password: `YourSecurePassword123!` (from User Secrets)
   - Save password: ✅ (optional)
4. Click **Save**

**Why `postgres` instead of `localhost`?**
- pgAdmin runs inside Docker network
- Uses Aspire service discovery (`postgres` resolves to container IP)

### 6.3 Verify Database Schema

1. Expand **Servers** → **ContosoSeaDB** → **Databases** → **ContosoSeaDB**
2. Expand **Schemas** → **public** → **Tables**

**Expected Tables**:

- `boats` (4 rows)
- `boat_states` (4 rows)
- `routes` (4 rows)
- `waypoints` (~20 rows)

**Run Test Query**:

```sql
SELECT b.vessel_name, bs.status, bs.energy_level, bs.latitude, bs.longitude
FROM boats b
INNER JOIN boat_states bs ON b.id = bs.boat_id
ORDER BY b.id;
```

**Expected Results**:

| vessel_name | status | energy_level | latitude | longitude |
|-------------|--------|--------------|----------|-----------|
| Contoso Sea Voyager | Active | 85.5 | 51.5074 | -0.1278 |
| Contoso Sea Pioneer | Charging | 42.3 | 51.5154 | -0.1420 |
| Contoso Sea Navigator | Active | 91.2 | 51.5010 | -0.1200 |
| Contoso Sea Explorer | Maintenance | 15.7 | 51.5090 | -0.1390 |

---

## Step 7: Test Frontend (React + Three.js)

Navigate to `http://localhost:5173` (Aspire automatically opens this)

**Expected Visuals**:

1. **3D Scene**: Four boats positioned on ocean surface
2. **Boat Cards**: Right sidebar showing boat status:
   - BOAT-001: Orange/red (Active)
   - BOAT-002: Blue (Charging)
   - BOAT-003: Orange/red (Active)
   - BOAT-004: Gray (Maintenance)
3. **Scene Controls**: Bottom-left UI with speed slider (1x - 10x)
4. **Real-Time Updates**: Boat positions update every 2 seconds

**Test Interactions**:

- **Drag**: Rotate camera around scene
- **Scroll**: Zoom in/out
- **Speed Slider**: Change to 10x speed, verify boats move faster
- **Keyboard Controls**: Use arrow keys to navigate (if implemented)

---

## Step 8: Verify API Endpoints

### 8.1 Test GET /api/boats

```powershell
# PowerShell
$response = Invoke-RestMethod -Uri "https://localhost:7585/api/boats" -SkipCertificateCheck
$response | ConvertTo-Json -Depth 3
```

**Expected Output**:

```json
[
  {
    "Id": "BOAT-001",
    "Latitude": 51.5074,
    "Longitude": -0.1278,
    "Status": "Active",
    "EnergyLevel": 85.5,
    "VesselName": "Contoso Sea Voyager",
    "SurveyType": "Geophysical Survey",
    "Project": "Dogger Bank Offshore Wind Farm",
    "Equipment": "Multibeam Sonar, Magnetometer",
    "AreaCovered": 0.0,
    "Speed": "12 knots",
    "CrewCount": 24,
    "Conditions": "Good sea state",
    "Heading": 45.0
  },
  ...
]
```

### 8.2 Test POST /api/boats/reset

```powershell
# PowerShell
$response = Invoke-RestMethod -Uri "https://localhost:7585/api/boats/reset" -Method Post -SkipCertificateCheck
$response
```

**Expected Output**:

```json
{
  "success": true,
  "message": "All boats reset to initial state",
  "boatsReset": 4
}
```

### 8.3 Verify Reset in Frontend

After calling reset endpoint:

1. Open browser to `http://localhost:5173`
2. Observe boats jump back to initial positions
3. Verify energy levels reset (BOAT-001: 85.5%, BOAT-002: 42.3%, etc.)

---

## Step 9: Monitor with OpenTelemetry

Aspire automatically configures OpenTelemetry for distributed tracing.

### 9.1 View API Traces

1. Open Aspire dashboard: `http://localhost:15888`
2. Click **Traces** in left sidebar
3. Filter by service: `apiservice`
4. Click a trace for `/api/boats` endpoint

**Expected Trace Details**:

- **Root Span**: HTTP GET /api/boats
- **Child Span**: PostgreSQL query (`SELECT ... FROM boats ...`)
- **Timing**: Total <50ms, query <10ms

### 9.2 View Frontend Traces

1. In Aspire dashboard, filter by service: `browser`
2. Click a trace for `fetch('/api/boats')`

**Expected Trace Details**:

- **Root Span**: Browser fetch
- **Child Span**: API request (linked to backend trace)
- **Timing**: Round-trip <100ms

---

## Step 10: Run Automated Tests (Future)

Once contract tests are implemented:

```powershell
# Navigate to ApiService.Tests project
cd EnergyBoatApp.ApiService.Tests

# Run all tests
dotnet test

# Run only contract tests
dotnet test --filter "Category=Contract"
```

**Expected Output**:

```text
Passed!  - Failed:     0, Passed:    12, Skipped:     0, Total:    12, Duration: 2.3s
```

---

## Troubleshooting

### Issue: PostgreSQL Container Won't Start

**Symptoms**: Aspire logs show "Failed to connect to postgres"

**Solutions**:

1. Check Docker Desktop is running
2. Check port 5432 is not in use:
   ```powershell
   netstat -ano | findstr :5432
   ```
3. Stop any conflicting PostgreSQL services:
   ```powershell
   docker ps -a | findstr postgres
   docker stop <container_id>
   docker rm <container_id>
   ```
4. Re-run `aspire run`

### Issue: User Secrets Not Found

**Symptoms**: Error "Parameter 'postgres-username' not configured"

**Solutions**:

1. Verify User Secrets are set:
   ```powershell
   cd EnergyBoatApp.AppHost
   dotnet user-secrets list
   ```
2. Re-set secrets if missing (see Step 2)
3. Verify `UserSecretsId` in `EnergyBoatApp.AppHost.csproj`

### Issue: npm install Fails

**Symptoms**: Aspire logs show "npm ERR! network request failed"

**Solutions**:

1. Check internet connection
2. Manually run npm install:
   ```powershell
   cd EnergyBoatApp.Web
   npm install
   ```
3. Retry `aspire run`

### Issue: Frontend Shows Empty Scene

**Symptoms**: 3D scene loads but no boats visible

**Solutions**:

1. Open browser console (F12) and check for errors
2. Verify API is reachable:
   ```javascript
   fetch('/api/boats').then(r => r.json()).then(console.log)
   ```
3. Check Aspire logs for backend errors
4. Verify seed data exists in pgAdmin (see Step 6.3)

### Issue: Data Doesn't Persist After Restart

**Symptoms**: After stopping Aspire, data is lost on next run

**Solutions**:

1. Verify `WithDataVolume()` is called in `AppHost.cs`
2. Check Docker volume exists:
   ```powershell
   docker volume ls | findstr postgres
   ```
3. If volume missing, Aspire will recreate on next run (expected first time)

---

## Clean Up

### Stop Application

Press `Ctrl+C` in PowerShell terminal running `aspire run`

Aspire automatically:
- Stops all containers (PostgreSQL, pgAdmin)
- Stops API and frontend processes
- Preserves data volume (data persists)

### Remove All Data

To completely reset (delete database):

```powershell
# Find Docker volume
docker volume ls | findstr postgres

# Remove volume (data will be lost)
docker volume rm <volume_name>

# Next `aspire run` will re-seed database
```

---

## Next Steps

### Development Workflow

1. **Make Code Changes**: Edit C# or React files
2. **Aspire Watch Mode**: Run `aspire run --watch` for auto-restart
3. **View Logs**: Use Aspire dashboard to debug issues
4. **Run Tests**: `dotnet test` before committing

### Contributing

1. **Create Feature Branch**: `git checkout -b feature/your-feature`
2. **Make Changes**: Follow coding standards in `.github/copilot-instructions.md`
3. **Test Locally**: Verify with `aspire run` and `dotnet test`
4. **Commit**: `git commit -m "feat: your feature description"`
5. **Push**: `git push origin feature/your-feature`
6. **Create PR**: Open pull request on GitHub

### Learning Resources

- **.NET Aspire Docs**: [learn.microsoft.com/dotnet/aspire](https://learn.microsoft.com/dotnet/aspire)
- **Npgsql Docs**: [npgsql.org/doc](https://www.npgsql.org/doc/)
- **Three.js Docs**: [threejs.org/docs](https://threejs.org/docs/)
- **React Docs**: [react.dev](https://react.dev/)

---

**Quickstart Complete** ✅

You should now have:
- PostgreSQL running in Docker with seed data
- API serving boat status from database
- Frontend displaying boats in 3D scene
- pgAdmin UI for database management
- Aspire dashboard for monitoring

**Happy coding!** 🚀
