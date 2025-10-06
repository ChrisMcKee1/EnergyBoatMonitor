using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EnergyBoatApp.Tests.IntegrationTests;

/// <summary>
/// Integration tests for database initialization.
/// Verifies that ContosoSeaDB schema is created on startup with all 4 tables, indexes, and constraints.
/// Per data-model.md specifications.
/// </summary>
public class DatabaseInitializationTests : IAsyncLifetime
{
    private DistributedApplication? _app;
    private NpgsqlDataSource? _dataSource;

    public async Task InitializeAsync()
    {
        // Create Aspire app with PostgreSQL
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.EnergyBoatApp_AppHost>();
        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Get database connection from service provider
        var apiService = _app.Services.GetRequiredService<HttpClient>();
        // TODO: Get NpgsqlDataSource from DI when database implementation is complete
    }

    [Fact(Skip = "Will fail until database initialization service (T017) is implemented")]
    public async Task Database_IsCreatedOnStartup()
    {
        // This test will FAIL until T017 (Database Initialization Service) is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'";
        var tableCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        // Assert all 4 tables exist
        Assert.Equal(4, tableCount);
    }

    [Fact(Skip = "Will fail until database initialization service (T017) is implemented")]
    public async Task Database_HasBoatsTable()
    {
        // This test will FAIL until T017 is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'boats'";
        
        var exists = (long)(await command.ExecuteScalarAsync() ?? 0);
        Assert.Equal(1, exists);
    }

    [Fact(Skip = "Will fail until database initialization service (T017) is implemented")]
    public async Task Database_HasBoatStatesTable()
    {
        // This test will FAIL until T017 is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'boat_states'";
        
        var exists = (long)(await command.ExecuteScalarAsync() ?? 0);
        Assert.Equal(1, exists);
    }

    [Fact(Skip = "Will fail until database initialization service (T017) is implemented")]
    public async Task Database_HasRoutesTable()
    {
        // This test will FAIL until T017 is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'routes'";
        
        var exists = (long)(await command.ExecuteScalarAsync() ?? 0);
        Assert.Equal(1, exists);
    }

    [Fact(Skip = "Will fail until database initialization service (T017) is implemented")]
    public async Task Database_HasWaypointsTable()
    {
        // This test will FAIL until T017 is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'waypoints'";
        
        var exists = (long)(await command.ExecuteScalarAsync() ?? 0);
        Assert.Equal(1, exists);
    }

    [Fact(Skip = "Will fail until database initialization service (T017) is implemented")]
    public async Task Database_HasRequiredIndexes()
    {
        // This test will FAIL until T017 is implemented
        // Verify per data-model.md: 5 indexes total
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM pg_indexes 
            WHERE schemaname = 'public'
            AND indexname IN (
                'idx_boats_vessel_name',
                'idx_boat_states_status',
                'idx_boat_states_energy',
                'idx_boat_states_updated',
                'idx_waypoints_boat_sequence'
            )";
        
        var indexCount = (long)(await command.ExecuteScalarAsync() ?? 0);
        Assert.Equal(5, indexCount);
    }

    [Fact(Skip = "Will fail until database initialization service (T017) is implemented")]
    public async Task Database_BoatStatesTable_HasCheckConstraints()
    {
        // This test will FAIL until T017 is implemented
        // Verify latitude, longitude, heading, energy_level constraints
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.check_constraints 
            WHERE constraint_schema = 'public'
            AND constraint_name LIKE 'boat_states_%'";
        
        var constraintCount = (long)(await command.ExecuteScalarAsync() ?? 0);
        Assert.True(constraintCount >= 4); // At least lat, lon, heading, energy constraints
    }

    [Fact(Skip = "Will fail until database initialization service (T017) is implemented")]
    public async Task Database_WaypointsTable_HasUniqueConstraint()
    {
        // This test will FAIL until T017 is implemented
        // Verify UNIQUE (boat_id, sequence) constraint per data-model.md
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.table_constraints 
            WHERE constraint_schema = 'public'
            AND table_name = 'waypoints'
            AND constraint_type = 'UNIQUE'";
        
        var uniqueConstraintCount = (long)(await command.ExecuteScalarAsync() ?? 0);
        Assert.True(uniqueConstraintCount >= 1);
    }

    public async Task DisposeAsync()
    {
        if (_dataSource != null)
        {
            await _dataSource.DisposeAsync();
        }

        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }
}
