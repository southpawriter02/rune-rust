using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Services;
using RuneAndRust.Infrastructure.Configuration;
using RuneAndRust.Infrastructure.Persistence;
using RuneAndRust.Infrastructure.Repositories;

namespace RuneAndRust.Infrastructure;

/// <summary>
/// Extension methods for configuring dependency injection in the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration for connection strings.</param>
    /// <param name="useInMemoryDatabase">If true, uses in-memory storage; otherwise, configures PostgreSQL.</param>
    /// <param name="configPath">Path to configuration files directory.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// When <paramref name="useInMemoryDatabase"/> is false, this method configures Entity Framework Core
    /// with PostgreSQL using the "GameDatabase" connection string. However, the actual repository
    /// implementation currently falls back to in-memory storage pending full EF Core repository implementation.
    /// </remarks>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useInMemoryDatabase = false,
        string configPath = "config")
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

        // Register configuration provider
        services.AddSingleton<IGameConfigurationProvider>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<JsonConfigurationProvider>>();
            return new JsonConfigurationProvider(configPath, logger);
        });

        // Register configuration objects from the provider
        services.AddSingleton(sp =>
            sp.GetRequiredService<IGameConfigurationProvider>().GetLexiconConfiguration());
        services.AddSingleton(sp =>
            sp.GetRequiredService<IGameConfigurationProvider>().GetThemeConfiguration());

        return services;
    }

    /// <summary>
    /// Adds application layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ItemEffectService>();
        services.AddScoped<InputValidationService>();
        services.AddScoped<PlayerCreationService>();
        services.AddScoped<GameSessionService>();

        // Text variety services
        services.AddScoped<LexiconService>();

        // Environment coherence service (v0.0.11a)
        services.AddScoped(sp =>
        {
            var configProvider = sp.GetRequiredService<IGameConfigurationProvider>();
            var categoryConfig = configProvider.GetEnvironmentCategories();
            var biomeConfig = configProvider.GetBiomeConfiguration();
            var logger = sp.GetRequiredService<ILogger<EnvironmentCoherenceService>>();
            return new EnvironmentCoherenceService(categoryConfig, biomeConfig, logger);
        });

        // Descriptor service (updated v0.0.11a with optional coherence service)
        services.AddScoped(sp =>
        {
            var configProvider = sp.GetRequiredService<IGameConfigurationProvider>();
            var pools = configProvider.GetAllDescriptorPools();
            var theme = configProvider.GetThemeConfiguration();
            var logger = sp.GetRequiredService<ILogger<DescriptorService>>();
            var coherenceService = sp.GetService<EnvironmentCoherenceService>();
            return new DescriptorService(pools, theme, logger, coherenceService);
        });

        // Resource system service
        services.AddScoped<ResourceService>();

        // Ability system service
        services.AddScoped<AbilityService>();

        // Class system service (depends on AbilityService)
        services.AddScoped<ClassService>();

        // Equipment system service (v0.0.7a)
        services.AddScoped<EquipmentService>();

        // Experience system service (v0.0.8a)
        services.AddScoped<ExperienceService>();

        // Progression system service (v0.0.8b)
        services.AddScoped<ProgressionService>();

        // Tier system service (v0.0.9c)
        services.AddScoped<ITierService, TierService>();

        // Trait system service (v0.0.9c)
        services.AddScoped<ITraitService, TraitService>();

        // Monster system service (v0.0.9a, updated v0.0.9c with tier/trait support)
        services.AddScoped<IMonsterService>(sp =>
        {
            var configProvider = sp.GetRequiredService<IGameConfigurationProvider>();
            var tierService = sp.GetRequiredService<ITierService>();
            var traitService = sp.GetRequiredService<ITraitService>();
            var logger = sp.GetRequiredService<ILogger<MonsterService>>();
            return new MonsterService(
                configProvider.GetMonsters,
                configProvider.GetMonsterById,
                tierService.SelectRandomTier,
                tierService.GetTier,
                tierService.GetDefaultTier,
                traitService.SelectRandomTraits,
                traitService.GetTraits,
                logger);
        });

        // Damage calculation service (v0.0.9b)
        services.AddScoped<IDamageCalculationService, DamageCalculationService>();

        // Loot system service (v0.0.9d)
        services.AddScoped<ILootService, LootService>();

        // Operation scope for logging correlation (v0.0.10a)
        services.AddScoped<IOperationScope, OperationScope>();

        return services;
    }
}

