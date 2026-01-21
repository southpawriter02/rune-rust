// ═══════════════════════════════════════════════════════════════════════════════
// TuiServiceCollectionExtensions.cs
// Extension methods for registering TUI presentation services with DI container.
// Version: 0.13.5c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Services;
using RuneAndRust.Presentation.Shared.ValueObjects;
using RuneAndRust.Presentation.Tui.Services;

namespace RuneAndRust.Presentation.Extensions;

/// <summary>
/// Extension methods for registering TUI presentation services with the DI container.
/// </summary>
/// <remarks>
/// <para>
/// Provides standardized service registration for the TUI presentation layer,
/// ensuring consistent dependency injection patterns across all TUI components.
/// </para>
/// <para>
/// <b>Service Lifetimes:</b>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Lifetime</term>
///     <description>Service Types</description>
///   </listheader>
///   <item>
///     <term>Singleton</term>
///     <description>Core services (IThemeService) - shared state, expensive initialization</description>
///   </item>
///   <item>
///     <term>Transient</term>
///     <description>UI components (*Display, *Panel, *Renderer) - per-instance state</description>
///   </item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // In Program.cs or Startup.cs
/// var services = new ServiceCollection();
/// services.AddRuneRustTuiServices();
/// services.AddLogging(builder => builder.AddConsole());
/// 
/// var provider = services.BuildServiceProvider();
/// var themeService = provider.GetRequiredService&lt;IThemeService&gt;();
/// </code>
/// </example>
public static class TuiServiceCollectionExtensions
{
    /// <summary>
    /// Registers all TUI presentation services with standard patterns.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for method chaining.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <b>Registered Services:</b>
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="IThemeService"/> as Singleton (TuiThemeAdapter)</description></item>
    /// </list>
    /// <para>
    /// <b>Note:</b> Logging services must be registered separately using
    /// <c>services.AddLogging()</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// 
    /// // Register TUI services
    /// services.AddRuneRustTuiServices();
    /// 
    /// // Register logging (required)
    /// services.AddLogging(builder => 
    /// {
    ///     builder.AddConsole();
    ///     builder.SetMinimumLevel(LogLevel.Debug);
    /// });
    /// 
    /// // Build provider
    /// var provider = services.BuildServiceProvider();
    /// </code>
    /// </example>
    public static IServiceCollection AddRuneRustTuiServices(
        this IServiceCollection services)
    {
        // ═══════════════════════════════════════════════════════════════════════
        // CORE SERVICES (Singleton - shared across components)
        // ═══════════════════════════════════════════════════════════════════════

        // Theme Service: Provides colors, icons, and animation timings
        // Singleton because theme state is shared and initialization is expensive
        services.AddSingleton<IThemeService>(sp =>
        {
            var logger = sp.GetService<ILogger<TuiThemeAdapter>>();
            var themeDefinition = ThemeDefinition.CreateDefault();

            logger?.LogInformation(
                "Initializing TuiThemeAdapter with theme: {ThemeName}",
                themeDefinition.Name);

            return new TuiThemeAdapter(themeDefinition, logger);
        });

        // ═══════════════════════════════════════════════════════════════════════
        // OPTIONAL SERVICES (Placeholder for future expansion)
        // ═══════════════════════════════════════════════════════════════════════

        // IAccessibilityService will be added in v0.13.5f
        // For now, property injection will receive null

        // ═══════════════════════════════════════════════════════════════════════
        // Note: Display, Panel, and Renderer components are typically created
        // directly rather than through DI, as they often require runtime parameters.
        // They receive IThemeService and ILogger through their constructors.
        // ═══════════════════════════════════════════════════════════════════════

        return services;
    }

    /// <summary>
    /// Registers TUI presentation services with a custom theme definition.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="themeDefinition">The custom theme definition to use.</param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for method chaining.
    /// </returns>
    /// <remarks>
    /// Use this overload when you want to provide a custom theme instead of
    /// the default "Dark Fantasy" theme.
    /// </remarks>
    /// <example>
    /// <code>
    /// var customTheme = ThemeDefinition.Create(
    ///     "High Contrast",
    ///     ColorPalette.CreateHighContrast(),
    ///     IconSet.CreateDefault(),
    ///     AnimationTimings.CreateDefault()
    /// );
    /// 
    /// services.AddRuneRustTuiServices(customTheme);
    /// </code>
    /// </example>
    public static IServiceCollection AddRuneRustTuiServices(
        this IServiceCollection services,
        ThemeDefinition themeDefinition)
    {
        ArgumentNullException.ThrowIfNull(themeDefinition);

        // Theme Service with custom theme
        services.AddSingleton<IThemeService>(sp =>
        {
            var logger = sp.GetService<ILogger<TuiThemeAdapter>>();

            logger?.LogInformation(
                "Initializing TuiThemeAdapter with custom theme: {ThemeName}",
                themeDefinition.Name);

            return new TuiThemeAdapter(themeDefinition, logger);
        });

        return services;
    }
}
