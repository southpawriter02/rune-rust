// ═══════════════════════════════════════════════════════════════════════════════
// ApplicationServiceCollectionExtensions.cs
// Extension methods for registering Application layer services with DI container.
// Provides AddTraumaEconomy() for unified trauma economy service registration.
// Version: 0.18.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;

/// <summary>
/// Extension methods for registering Application layer services with the DI container.
/// </summary>
/// <remarks>
/// <para>
/// Provides standardized service registration for Application layer services,
/// ensuring consistent dependency injection patterns.
/// </para>
/// <para>
/// <strong>Service Lifetimes:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Lifetime</term>
///     <description>Service Types</description>
///   </listheader>
///   <item>
///     <term>Singleton</term>
///     <description>Configuration classes - loaded once, shared across requests</description>
///   </item>
///   <item>
///     <term>Scoped</term>
///     <description>Services and handlers - per-request lifecycle for thread safety</description>
///   </item>
/// </list>
/// </remarks>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers all trauma economy services with the DI container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for method chaining.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Registered Services:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <see cref="ITraumaEconomyConfiguration"/> as Singleton
    ///       (TraumaEconomyConfiguration)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="UnifiedDamageHandler"/> as Scoped
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="UnifiedRestHandler"/> as Scoped
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="UnifiedTurnHandler"/> as Scoped
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="ITraumaEconomyService"/> as Scoped (TraumaEconomyService)
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Prerequisites:</strong>
    /// The following services must be registered before calling this method:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="IStressService"/></description></item>
    ///   <item><description><see cref="ICorruptionService"/></description></item>
    ///   <item><description><see cref="ICpsService"/></description></item>
    ///   <item><description><see cref="ITraumaService"/></description></item>
    ///   <item><description><see cref="ISpecializationProvider"/></description></item>
    ///   <item><description>IConfiguration with trauma economy settings</description></item>
    /// </list>
    /// <para>
    /// <strong>Optional Services:</strong>
    /// The following services are optional and will be resolved if available:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="IRageService"/> (Berserker specialization)</description></item>
    ///   <item><description><see cref="IMomentumService"/> (Storm Blade specialization)</description></item>
    ///   <item><description><see cref="ICoherenceService"/> (Arcanist specialization)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// 
    /// // Register prerequisite services first
    /// services.AddSingleton&lt;IStressService, StressService&gt;();
    /// services.AddSingleton&lt;ICorruptionService, CorruptionService&gt;();
    /// // ... other prerequisites ...
    /// 
    /// // Register trauma economy services
    /// services.AddTraumaEconomy();
    /// 
    /// var provider = services.BuildServiceProvider();
    /// var traumaEconomyService = provider.GetRequiredService&lt;ITraumaEconomyService&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddTraumaEconomy(this IServiceCollection services)
    {
        // ═══════════════════════════════════════════════════════════════════════
        // CONFIGURATION (Singleton - loaded once, shared across requests)
        // ═══════════════════════════════════════════════════════════════════════

        services.AddSingleton<ITraumaEconomyConfiguration, TraumaEconomyConfiguration>();

        // ═══════════════════════════════════════════════════════════════════════
        // HANDLERS (Scoped - per-request lifecycle for thread safety)
        // ═══════════════════════════════════════════════════════════════════════

        services.AddScoped<UnifiedDamageHandler>();
        services.AddScoped<UnifiedRestHandler>();
        services.AddScoped<UnifiedTurnHandler>();

        // ═══════════════════════════════════════════════════════════════════════
        // MAIN SERVICE (Scoped - orchestrates handlers and sub-services)
        // ═══════════════════════════════════════════════════════════════════════

        services.AddScoped<ITraumaEconomyService, TraumaEconomyService>();

        return services;
    }
}
