using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Infrastructure.Persistence;
using RuneAndRust.Infrastructure.Repositories;

namespace RuneAndRust.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useInMemoryDatabase = false)
    {
        if (useInMemoryDatabase)
        {
            // Use in-memory repository for development/testing
            services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        }
        else
        {
            // Use PostgreSQL with EF Core
            var connectionString = configuration.GetConnectionString("GameDatabase");
            services.AddDbContext<GameDbContext>(options =>
                options.UseNpgsql(connectionString));

            // TODO: Add EF Core-based repository when full persistence is needed
            services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        }

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<GameSessionService>();
        return services;
    }
}
