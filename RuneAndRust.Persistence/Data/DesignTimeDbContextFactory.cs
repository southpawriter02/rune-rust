using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Design-time factory for creating RuneAndRustDbContext.
/// Used by EF Core tools (migrations, scaffolding) without running the full application.
/// </summary>
/// <remarks>See: SPEC-MIGRATE-001 for Migration System design.</remarks>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RuneAndRustDbContext>
{
    /// <summary>
    /// Creates a new DbContext instance for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments (unused).</param>
    /// <returns>A configured RuneAndRustDbContext instance.</returns>
    public RuneAndRustDbContext CreateDbContext(string[] args)
    {
        // Default connection string for design-time operations
        // Can be overridden via environment variable
        var connectionString = Environment.GetEnvironmentVariable("RUNEANDRUST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password";

        var optionsBuilder = new DbContextOptionsBuilder<RuneAndRustDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new RuneAndRustDbContext(optionsBuilder.Options);
    }
}
