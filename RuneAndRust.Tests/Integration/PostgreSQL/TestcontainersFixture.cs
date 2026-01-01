using Microsoft.EntityFrameworkCore;
using RuneAndRust.Persistence.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace RuneAndRust.Tests.Integration.PostgreSQL;

/// <summary>
/// Alternative fixture using Testcontainers for fully automated PostgreSQL testing.
/// This fixture automatically manages the Docker container lifecycle - no manual Docker setup required.
/// Use this when you want the container to be automatically created and destroyed per test run.
/// </summary>
/// <remarks>
/// COMPARISON:
/// - PostgreSqlTestFixture: Requires manual Docker container (docker-compose up). Faster for repeated runs.
/// - TestcontainersFixture: Fully automated, no manual setup. Slower startup but more portable.
///
/// USAGE:
/// 1. Ensure Docker Desktop is running
/// 2. No other setup required - container is created automatically
/// 3. Container is destroyed after tests complete
/// </remarks>
public class TestcontainersFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private string? _connectionString;
    private RuneAndRustDbContext? _sharedContext;

    /// <summary>
    /// Gets a shared context instance. Prefer using CreateContext() for test isolation.
    /// This shared context is primarily for setup/teardown operations.
    /// </summary>
    public RuneAndRustDbContext Context => _sharedContext ??= CreateContext();

    public TestcontainersFixture()
    {
        // Build PostgreSQL container configuration
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("RuneAndRust_TestContainer")
            .WithUsername("postgres")
            .WithPassword("password")
            .WithPortBinding(5435, 5432) // Use port 5435 to avoid conflicts
            .WithCleanUp(true) // Automatically clean up container on disposal
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start the container
        await _container.StartAsync();

        // Get the connection string from the container
        _connectionString = _container.GetConnectionString();

        // Apply migrations to the container database
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

        // Stop and remove the container
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a fresh DbContext for the test database.
    /// </summary>
    public RuneAndRustDbContext CreateContext()
    {
        if (_connectionString == null)
        {
            throw new InvalidOperationException(
                "Container not initialized. Ensure InitializeAsync has been called.");
        }

        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new RuneAndRustDbContext(options);
    }

    /// <summary>
    /// Gets the connection string for the test container.
    /// Useful for debugging or manual inspection.
    /// </summary>
    public string GetConnectionString()
    {
        if (_connectionString == null)
        {
            throw new InvalidOperationException(
                "Container not initialized. Ensure InitializeAsync has been called.");
        }

        return _connectionString;
    }
}

/// <summary>
/// Collection definition for Testcontainers-based PostgreSQL tests.
/// Use this when you want automated container management.
/// </summary>
/// <remarks>
/// Example usage:
/// <code>
/// [Collection("Testcontainers")]
/// public class MyDatabaseTests
/// {
///     private readonly TestcontainersFixture _fixture;
///
///     public MyDatabaseTests(TestcontainersFixture fixture)
///     {
///         _fixture = fixture;
///     }
///
///     [Fact]
///     public async Task TestSomething()
///     {
///         using var context = _fixture.CreateContext();
///         // Test code here
///     }
/// }
/// </code>
/// </remarks>
[CollectionDefinition("Testcontainers")]
public class TestcontainersCollection : ICollectionFixture<TestcontainersFixture>
{
}
