// ═══════════════════════════════════════════════════════════════════════════════
// GuiServiceCollectionExtensions.cs
// Extension methods for registering GUI presentation services with DI container.
// Version: 0.13.5c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Gui.Services;
using RuneAndRust.Presentation.Shared.Services;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Gui.Extensions;

/// <summary>
/// Extension methods for registering GUI presentation services with the DI container.
/// </summary>
/// <remarks>
/// <para>
/// Provides standardized service registration for the GUI (Avalonia) presentation layer,
/// ensuring consistent dependency injection patterns across all GUI controls and view models.
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
///     <description>Core services (IThemeService) - shared state, brush caching</description>
///   </item>
///   <item>
///     <term>Transient</term>
///     <description>ViewModels - per-view state, can be scoped if needed</description>
///   </item>
/// </list>
/// <para>
/// <b>Avalonia Integration:</b> Controls in Avalonia are typically created via XAML
/// and receive dependencies through their constructors. Use view locators or
/// design-time data for DI integration.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In App.axaml.cs or Startup.cs
/// var services = new ServiceCollection();
/// services.AddRuneRustGuiServices();
/// services.AddLogging(builder => builder.AddDebug());
/// 
/// var provider = services.BuildServiceProvider();
/// </code>
/// </example>
public static class GuiServiceCollectionExtensions
{
    /// <summary>
    /// Registers all GUI presentation services with standard patterns.
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
    ///   <item><description><see cref="IThemeService"/> as Singleton (GuiThemeAdapter)</description></item>
    /// </list>
    /// <para>
    /// <b>Note:</b> Logging services must be registered separately using
    /// <c>services.AddLogging()</c>.
    /// </para>
    /// <para>
    /// <b>Control Resolution:</b> Avalonia controls are created by the framework
    /// via XAML. To use DI with controls:
    /// </para>
    /// <list type="number">
    ///   <item><description>Create controls with parameterless constructors for XAML</description></item>
    ///   <item><description>Use a custom ViewLocator to inject dependencies</description></item>
    ///   <item><description>Or use ViewModel-first pattern where VMs receive dependencies</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// 
    /// // Register GUI services
    /// services.AddRuneRustGuiServices();
    /// 
    /// // Register logging (required)
    /// services.AddLogging(builder =>
    /// {
    ///     builder.AddDebug();
    ///     builder.SetMinimumLevel(LogLevel.Debug);
    /// });
    /// 
    /// // Build provider
    /// var provider = services.BuildServiceProvider();
    /// </code>
    /// </example>
    public static IServiceCollection AddRuneRustGuiServices(
        this IServiceCollection services)
    {
        // ═══════════════════════════════════════════════════════════════════════
        // CORE SERVICES (Singleton - shared across controls)
        // ═══════════════════════════════════════════════════════════════════════

        // Theme Service: Provides colors, icons, and animation timings
        // Singleton because theme state is shared and brush caching benefits from single instance
        services.AddSingleton<IThemeService>(sp =>
        {
            var logger = sp.GetService<ILogger<GuiThemeAdapter>>();
            var themeDefinition = ThemeDefinition.CreateDefault();

            logger?.LogInformation(
                "Initializing GuiThemeAdapter with theme: {ThemeName}",
                themeDefinition.Name);

            return new GuiThemeAdapter(themeDefinition, logger);
        });

        // ═══════════════════════════════════════════════════════════════════════
        // OPTIONAL SERVICES (Placeholder for future expansion)
        // ═══════════════════════════════════════════════════════════════════════

        // IAccessibilityService will be added in v0.13.5f
        // For now, property injection will receive null

        // ═══════════════════════════════════════════════════════════════════════
        // VIEW MODELS (Transient - per-view state)
        // ═══════════════════════════════════════════════════════════════════════

        // Note: ViewModels would be registered here in a full implementation.
        // They follow the same DI pattern as controls:
        // services.AddTransient<CombatViewModel>();
        // services.AddTransient<CharacterViewModel>();
        // services.AddTransient<InventoryViewModel>();

        return services;
    }

    /// <summary>
    /// Registers GUI presentation services with a custom theme definition.
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
    ///     "Light Mode",
    ///     ColorPalette.CreateLight(),
    ///     IconSet.CreateDefault(),
    ///     AnimationTimings.CreateDefault()
    /// );
    /// 
    /// services.AddRuneRustGuiServices(customTheme);
    /// </code>
    /// </example>
    public static IServiceCollection AddRuneRustGuiServices(
        this IServiceCollection services,
        ThemeDefinition themeDefinition)
    {
        ArgumentNullException.ThrowIfNull(themeDefinition);

        // Theme Service with custom theme
        services.AddSingleton<IThemeService>(sp =>
        {
            var logger = sp.GetService<ILogger<GuiThemeAdapter>>();

            logger?.LogInformation(
                "Initializing GuiThemeAdapter with custom theme: {ThemeName}",
                themeDefinition.Name);

            return new GuiThemeAdapter(themeDefinition, logger);
        });

        return services;
    }
}
