# Simple Test Script for Energy Boat App
# Tests API and shows instructions for manual testing

Write-Host "⚡ Energy Boat Service Monitor - Test Script" -ForegroundColor Cyan
Write-Host "============================================`n" -ForegroundColor Cyan

# Check if .NET SDK is available
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ .NET SDK $dotnetVersion found" -ForegroundColor Green
} else {
    Write-Host "❌ .NET SDK not found. Please install .NET 9 SDK." -ForegroundColor Red
    exit 1
}

# Check if Node.js is available
Write-Host "Checking Node.js..." -ForegroundColor Yellow
$nodeVersion = node --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Node.js $nodeVersion found" -ForegroundColor Green
} else {
    Write-Host "❌ Node.js not found. Please install Node.js 20+." -ForegroundColor Red
    exit 1
}

# Build the solution
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
cd "$PSScriptRoot"
dotnet build EnergyBoatApp.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build succeeded" -ForegroundColor Green

# Install npm dependencies
Write-Host "`nInstalling npm dependencies..." -ForegroundColor Yellow
cd "$PSScriptRoot\EnergyBoatApp.Web"
if (!(Test-Path "node_modules")) {
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ npm install failed" -ForegroundColor Red
        exit 1
    }
}
Write-Host "✅ npm dependencies installed" -ForegroundColor Green

Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host "✅ All checks passed!" -ForegroundColor Green
Write-Host "`n📋 To run the application manually:" -ForegroundColor Cyan
Write-Host "`nTerminal 1 - API Service:" -ForegroundColor Yellow
Write-Host "  cd EnergyBoatApp.ApiService" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host "`nTerminal 2 - React Frontend:" -ForegroundColor Yellow
Write-Host "  cd EnergyBoatApp.Web" -ForegroundColor White
Write-Host "  npm start" -ForegroundColor White
Write-Host "`nThen open: http://localhost:5173" -ForegroundColor Cyan
Write-Host "`nAPI endpoint: http://localhost:5578/api/boats" -ForegroundColor Cyan

Write-Host "`n🚀 Everything is ready to run!" -ForegroundColor Green
