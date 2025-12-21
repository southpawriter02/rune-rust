using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Persistence.Data;
using Xunit;

namespace RuneAndRust.Tests.Integration.PostgreSQL;

/// <summary>
/// Base fixture for PostgreSQL integration tests.
/// Provides a real PostgreSQL database connection for testing PostgreSQL-specific features.
/// Tests using this fixture require a running PostgreSQL instance (via Docker).
/// </summary>
public class PostgreSqlTestFixture : IAsyncLifetime
{
    private readonly string _connectionString;
    private readonly string _testDatabaseName;

    private RuneAndRustDbContext? _sharedContext;

    /// <summary>
    /// Gets a shared context instance. Prefer using CreateContext() for test isolation.
    /// This shared context is primarily for setup/teardown operations.
    /// </summary>
    public RuneAndRustDbContext Context => _sharedContext ??= CreateContext();

    public PostgreSqlTestFixture()
    {
        // Use environment variable or default to Docker container on port 5433
        var baseConnectionString = Environment.GetEnvironmentVariable("RUNEANDRUST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5433;Username=postgres;Password=password";

        // Create a unique database name for test isolation
        _testDatabaseName = $"RuneAndRust_Test_{Guid.NewGuid():N}";
        _connectionString = $"{baseConnectionString};Database={_testDatabaseName}";
    }

    public async Task InitializeAsync()
    {
        // Create the test database
        var masterConnectionString = Environment.GetEnvironmentVariable("RUNEANDRUST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5433;Username=postgres;Password=password;Database=postgres";

        var masterOptions = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseNpgsql(masterConnectionString)
            .Options;

        using (var masterContext = new RuneAndRustDbContext(masterOptions))
        {
            await masterContext.Database.ExecuteSqlRawAsync($"CREATE DATABASE \"{_testDatabaseName}\"");
        }

        // Connect to the test database and apply migrations
        using var migrationContext = CreateContext();
        await migrationContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        // Dispose the shared context if it was created
        if (_sharedContext != null)
        {
            await _sharedContext.DisposeAsync();
        }

        // Drop the test database
        var masterConnectionString = Environment.GetEnvironmentVariable("RUNEANDRUST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5433;Username=postgres;Password=password;Database=postgres";

        var masterOptions = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseNpgsql(masterConnectionString)
            .Options;

        using (var masterContext = new RuneAndRustDbContext(masterOptions))
        {
            // Terminate existing connections before dropping
            await masterContext.Database.ExecuteSqlRawAsync($@"
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                FROM pg_stat_activity
                WHERE pg_stat_activity.datname = '{_testDatabaseName}'
                AND pid <> pg_backend_pid()");

            await masterContext.Database.ExecuteSqlRawAsync($"DROP DATABASE IF EXISTS \"{_testDatabaseName}\"");
        }
    }

    /// <summary>
    /// Creates a fresh DbContext for the test database.
    /// </summary>
    public RuneAndRustDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new RuneAndRustDbContext(options);
    }
}

/// <summary>
/// Collection definition for PostgreSQL tests.
/// Ensures tests share the same database fixture for efficiency.
/// </summary>
[CollectionDefinition("PostgreSQL")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlTestFixture>
{
}
