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
using RuneAndRust.Infrastructure.Providers;
using RuneAndRust.Infrastructure.Repositories;
using RuneAndRust.Infrastructure.Services;

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

        // Stance provider (v0.10.1b) - loads stance definitions from JSON config
        services.AddSingleton<IStanceProvider>(sp =>
        {
            var stancesPath = Path.Combine(configPath, "stances.json");
            var logger = sp.GetRequiredService<ILogger<JsonStanceProvider>>();
            return new JsonStanceProvider(stancesPath, logger);
        });

        // Environmental hazard provider (v0.10.1c) - loads hazard definitions from JSON config
        services.AddSingleton<IEnvironmentalHazardProvider>(sp =>
        {
            var hazardsPath = Path.Combine(configPath, "environmental-hazards.json");
            var logger = sp.GetRequiredService<ILogger<JsonEnvironmentalHazardProvider>>();
            return new JsonEnvironmentalHazardProvider(hazardsPath, logger);
        });

        // Ability tree provider (v0.10.2a) - loads ability tree definitions from JSON config
        services.AddSingleton<IAbilityTreeProvider>(sp =>
        {
            var treesPath = Path.Combine(configPath, "ability-trees.json");
            var logger = sp.GetRequiredService<ILogger<JsonAbilityTreeProvider>>();
            return new JsonAbilityTreeProvider(treesPath, logger);
        });

        // Combo provider (v0.10.3a) - loads combo definitions from JSON config
        services.AddSingleton<IComboProvider>(sp =>
        {
            var combosPath = Path.Combine(configPath, "combos.json");
            var logger = sp.GetRequiredService<ILogger<JsonComboProvider>>();
            return new JsonComboProvider(combosPath, logger);
        });

        // Resource provider (v0.11.0a) - loads resource definitions from JSON config
        services.AddSingleton<IResourceProvider>(sp =>
        {
            var resourcesPath = Path.Combine(configPath, "resources.json");
            var logger = sp.GetRequiredService<ILogger<JsonResourceProvider>>();
            return new JsonResourceProvider(resourcesPath, logger);
        });

        // Recipe provider (v0.11.1a) - loads recipe definitions from JSON config
        services.AddSingleton<IRecipeProvider>(sp =>
        {
            var recipesPath = Path.Combine(configPath, "recipes.json");
            var logger = sp.GetRequiredService<ILogger<RecipeProvider>>();
            return new RecipeProvider(recipesPath, logger);
        });

        // Recipe scroll provider (v0.11.1c) - loads recipe scroll configurations from JSON config
        // Note: Uses default configuration; override via RecipeScrollProvider constructor if needed
        services.AddSingleton<RecipeScrollSettings>(new RecipeScrollSettings());
        services.AddSingleton<IRecipeScrollProvider, RecipeScrollProvider>();

        // Crafting station provider (v0.11.2a) - loads station definitions from config
        // Note: Uses default configuration; override via CraftingStationProvider constructor if needed
        services.AddSingleton<CraftingStationSettings>(new CraftingStationSettings());
        services.AddSingleton<ICraftingStationProvider, CraftingStationProvider>();

        // Quality tier provider (v0.11.2c) - loads quality tier definitions from JSON config
        services.AddSingleton<IQualityTierProvider>(sp =>
        {
            var tiersPath = Path.Combine(configPath, "quality-tiers.json");
            var logger = sp.GetRequiredService<ILogger<QualityTierProvider>>();
            return new QualityTierProvider(tiersPath, logger);
        });

        // Achievement provider (v0.12.1a) - loads achievement definitions from JSON config
        // Note: Uses default configuration; override via AchievementProvider constructor if needed
        services.AddSingleton<AchievementOptions>(new AchievementOptions());
        services.AddSingleton<IAchievementProvider, AchievementProvider>();

        // Lineage provider (v0.17.0e) - loads lineage definitions from JSON config
        services.AddSingleton<ILineageProvider>(sp =>
        {
            var lineagesPath = Path.Combine(configPath, "lineages.json");
            var logger = sp.GetRequiredService<ILogger<LineageProvider>>();
            return new LineageProvider(logger, lineagesPath);
        });

        // Background provider (v0.17.1d) - loads background definitions from JSON config
        services.AddSingleton<IBackgroundProvider>(sp =>
        {
            var backgroundsPath = Path.Combine(configPath, "backgrounds.json");
            var logger = sp.GetRequiredService<ILogger<BackgroundProvider>>();
            return new BackgroundProvider(logger, backgroundsPath);
        });

        // Attribute provider (v0.17.2e) - loads attribute definitions from JSON config
        services.AddSingleton<IAttributeProvider>(sp =>
        {
            var attributesPath = Path.Combine(configPath, "attributes.json");
            var logger = sp.GetRequiredService<ILogger<AttributeProvider>>();
            return new AttributeProvider(logger, attributesPath);
        });

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

        // Combat descriptor service (v0.0.11b)
        services.AddScoped(sp =>
        {
            var descriptorService = sp.GetRequiredService<DescriptorService>();
            var logger = sp.GetRequiredService<ILogger<CombatDescriptorService>>();
            return new CombatDescriptorService(descriptorService, logger);
        });

        // Ability descriptor service (v0.0.11b)
        services.AddScoped(sp =>
        {
            var descriptorService = sp.GetRequiredService<DescriptorService>();
            var logger = sp.GetRequiredService<ILogger<AbilityDescriptorService>>();
            return new AbilityDescriptorService(descriptorService, logger);
        });

        // Sensory descriptor service (v0.0.11c)
        services.AddScoped(sp =>
        {
            var descriptorService = sp.GetRequiredService<DescriptorService>();
            var configProvider = sp.GetRequiredService<IGameConfigurationProvider>();
            var sensoryConfig = configProvider.GetSensoryConfiguration();
            var logger = sp.GetRequiredService<ILogger<SensoryDescriptorService>>();
            return new SensoryDescriptorService(descriptorService, sensoryConfig, logger);
        });

        // Object descriptor service (v0.0.11d)
        services.AddScoped(sp =>
        {
            var descriptorService = sp.GetRequiredService<DescriptorService>();
            var configProvider = sp.GetRequiredService<IGameConfigurationProvider>();
            var objectConfig = configProvider.GetObjectDescriptorConfiguration();
            var logger = sp.GetRequiredService<ILogger<ObjectDescriptorService>>();
            return new ObjectDescriptorService(descriptorService, objectConfig, logger);
        });

        // Ambient event service (v0.0.11d)
        services.AddScoped(sp =>
        {
            var descriptorService = sp.GetRequiredService<DescriptorService>();
            var configProvider = sp.GetRequiredService<IGameConfigurationProvider>();
            var ambientConfig = configProvider.GetAmbientEventConfiguration();
            var logger = sp.GetRequiredService<ILogger<AmbientEventService>>();
            return new AmbientEventService(descriptorService, ambientConfig, logger);
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

        // Game event logger (used by various services for combat logging)
        services.AddScoped<IGameEventLogger, GameEventLogger>();

        // Stance system (v0.10.1b)
        // Note: IStanceProvider is registered in AddInfrastructure as it loads from JSON config
        services.AddScoped<IStanceService, StanceService>();

        // Environmental combat system (v0.10.1c)
        // Note: IEnvironmentalHazardProvider is registered in AddInfrastructure as it loads from JSON config
        services.AddScoped<IEnvironmentalCombatService, EnvironmentalCombatService>();

        // Talent point system (v0.10.2b)
        // Note: IAbilityTreeProvider is registered in AddInfrastructure as it loads from JSON config
        services.AddScoped<IPrerequisiteValidator, PrerequisiteValidator>();
        services.AddScoped<ITalentPointService, TalentPointService>();

        // Respec system (v0.10.2c)
        // Configuration for respec costs and eligibility
        services.AddSingleton<IRespecConfiguration, RespecConfiguration>();
        // Service for handling talent point reallocation operations
        services.AddScoped<IRespecService, RespecService>();

        // Combo detection system (v0.10.3b)
        // Note: IComboProvider is registered in AddInfrastructure as it loads from JSON config
        services.AddScoped<IComboService, ComboService>();

        // Recipe book system (v0.11.1b)
        // Note: IRecipeProvider and IResourceProvider are registered in AddInfrastructure as they load from JSON config
        services.AddScoped<IRecipeService, RecipeService>();

        // Recipe scroll use handler (v0.11.1c)
        // Handler for processing recipe scroll items when used by players
        // Note: IRecipeScrollProvider is registered in AddInfrastructure as it loads from config
        services.AddScoped<IItemUseHandler, Application.Handlers.RecipeScrollUseHandler>();

        // Crafting service (v0.11.2b)
        // Handles crafting mechanics: validation, dice checks, resource consumption, item creation
        // Dependencies: IRecipeProvider, IRecipeService, ICraftingStationProvider, IResourceProvider,
        //               IDiceService, IGameEventLogger
        services.AddScoped<ICraftingService, CraftingService>();

        // Quality determination service (v0.11.2c)
        // Determines crafted item quality based on roll results and margin thresholds
        // Note: IQualityTierProvider is registered in AddInfrastructure as it loads from JSON config
        services.AddScoped<IQualityDeterminationService, QualityDeterminationService>();

        // Statistics service (v0.12.0a)
        // Tracks and queries player statistics across combat, exploration, progression, and time categories
        // Calculates derived metrics including combat rating, critical hit rate, and trap avoidance rate
        services.AddScoped<IStatisticsService, StatisticsService>();

        // Dice history service (v0.12.0b)
        // Tracks and queries dice roll history including streaks and luck ratings
        // Provides statistics aggregation for the dice statistics view
        services.AddScoped<IDiceHistoryService, DiceHistoryService>();

        // Lineage application service (v0.17.0f)
        // Orchestrates applying lineage bonuses, traits, and trauma baselines to characters
        // during character creation. Depends on ILineageProvider for lineage definitions.
        services.AddScoped<ILineageApplicationService, LineageApplicationService>();

        // Background application service (v0.17.1e)
        // Orchestrates applying background grants (skills, equipment) to characters
        // during character creation. Depends on IBackgroundProvider for background definitions.
        services.AddScoped<IBackgroundApplicationService, BackgroundApplicationService>();

        // Derived stat calculator (v0.17.2g)
        // Calculates derived statistics (HP, Stamina, Aether Pool, Initiative, Soak,
        // Movement Speed, Carrying Capacity) from core attributes during character creation.
        // Uses default formulas built into the calculator.
        services.AddScoped<IDerivedStatCalculator>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DerivedStatCalculator>>();
            return DerivedStatCalculator.CreateWithDefaultFormulas(logger);
        });

        return services;
    }
}

