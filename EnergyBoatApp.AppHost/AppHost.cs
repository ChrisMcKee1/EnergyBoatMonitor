var builder = DistributedApplication.CreateBuilder(args);

// Configure PostgreSQL with password authentication for local development
var postgresUsername = builder.AddParameter("postgres-username", secret: true);
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(container =>
    {
        container.WithPgAdmin()
                 .WithDataVolume();
    })
    .WithPasswordAuthentication(postgresUsername, postgresPassword);

var db = postgres.AddDatabase("ContosoSeaDB");

// Add the API service
var apiService = builder.AddProject<Projects.EnergyBoatApp_ApiService>("apiservice")
    .WithReference(db)
    .WithExternalHttpEndpoints();

// Add React + Three.js frontend as npm app
// Using Vite 4.5.5 with Rollup 3.29.4 to avoid Windows native module issues
var webFrontend = builder.AddNpmApp("webfrontend", "../EnergyBoatApp.Web", "start")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithEnvironment("BROWSER", "none")  // Prevent Vite from auto-opening browser
    .WithHttpEndpoint(env: "PORT")       // Dynamic port assignment via PORT env var
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

// Note: Aspire automatically provides OTEL_EXPORTER_OTLP_ENDPOINT and OTEL_EXPORTER_OTLP_HEADERS
// For browser telemetry, these need to be exposed as VITE_* environment variables
// This is configured in vite.config.js using define or envPrefix

builder.Build().Run();
