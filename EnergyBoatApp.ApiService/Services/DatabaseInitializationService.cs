using Npgsql;

namespace EnergyBoatApp.ApiService.Services;

/// <summary>
/// Background service that ensures the database schema is created on application startup.
/// Runs the 001-initial-schema.sql migration script idempotently (safe to run multiple times).
/// </summary>
public class DatabaseInitializationService : IHostedService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly IHostEnvironment _environment;

    public DatabaseInitializationService(
        NpgsqlDataSource dataSource,
        ILogger<DatabaseInitializationService> logger,
        IHostEnvironment environment)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database initialization...");

        try
        {
            // Check if tables already exist (idempotent check)
            if (await TablesExistAsync(cancellationToken))
            {
                _logger.LogInformation("Database schema already exists, skipping initialization");
                return;
            }

            // Run the schema creation script
            await RunSchemaMigrationAsync(cancellationToken);

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            
            // In production, we might want to fail fast. In development, we can continue.
            if (!_environment.IsDevelopment())
            {
                throw;
            }
            
            _logger.LogWarning("Continuing despite database initialization failure (Development mode)");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database initialization service stopping");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if the required tables exist in the database.
    /// </summary>
    private async Task<bool> TablesExistAsync(CancellationToken cancellationToken)
    {
        const string checkTablesQuery = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name IN ('boats', 'boat_states', 'routes', 'waypoints')";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = checkTablesQuery;

        var tableCount = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);

        _logger.LogDebug("Found {TableCount} of 4 required tables", tableCount);

        // Return true if all 4 tables exist
        return tableCount == 4;
    }

    /// <summary>
    /// Runs the 001-initial-schema.sql migration script to create the database schema.
    /// </summary>
    private async Task RunSchemaMigrationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running database schema migration...");

        // Read the SQL script from the Migrations folder
        var migrationScriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "Migrations",
            "001-initial-schema.sql");

        if (!File.Exists(migrationScriptPath))
        {
            throw new FileNotFoundException(
                $"Migration script not found at: {migrationScriptPath}. " +
                "Ensure 001-initial-schema.sql is copied to output directory.");
        }

        var migrationSql = await File.ReadAllTextAsync(migrationScriptPath, cancellationToken);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = migrationSql;
            command.CommandTimeout = 60; // 60 seconds for schema creation

            await command.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Database schema created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database schema, rolling back");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
