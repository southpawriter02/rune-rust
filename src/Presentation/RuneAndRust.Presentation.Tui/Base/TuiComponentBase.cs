// ═══════════════════════════════════════════════════════════════════════════════
// TuiComponentBase.cs
// Abstract base class for TUI presentation components with standardized DI patterns.
// Version: 0.13.5c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Interfaces;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Services;
using RuneAndRust.Presentation.Shared.ValueObjects;
using RuneAndRust.Presentation.Tui.Services;
using Spectre.Console;

namespace RuneAndRust.Presentation.Base;

/// <summary>
/// Abstract base class for TUI presentation components providing standardized
/// dependency injection, lifecycle management, and theme integration.
/// </summary>
/// <remarks>
/// <para>
/// <b>Purpose:</b> Provides a consistent foundation for all TUI components,
/// ensuring they follow standard DI patterns and have access to theme services,
/// logging, and lifecycle management.
/// </para>
/// <para>
/// <b>Required Dependencies (Constructor Injection):</b>
/// <list type="bullet">
///   <item><description><see cref="IThemeService"/> - Colors, icons, animations</description></item>
///   <item><description><see cref="ILogger"/> - Diagnostic logging</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Optional Dependencies (Property Injection):</b>
/// <list type="bullet">
///   <item><description><see cref="Accessibility"/> - Screen reader support (when registered)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Lifecycle:</b>
/// <code>
/// Constructor → Initialize() → Activate() ↔ Deactivate() → Dispose()
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class StatusBarDisplay : TuiComponentBase
/// {
///     protected override string ComponentName => "StatusBar";
///     
///     public StatusBarDisplay(IThemeService theme, ILogger&lt;StatusBarDisplay&gt; logger)
///         : base(theme, logger)
///     { }
///     
///     public void Render(int health, int maxHealth)
///     {
///         var color = GetHealthColor((double)health / maxHealth);
///         // Render with standardized theming...
///     }
/// }
/// </code>
/// </example>
public abstract class TuiComponentBase : IComponentLifecycle
{
    // ═══════════════════════════════════════════════════════════════════════════
    // REQUIRED DEPENDENCIES (Constructor Injection)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Theme service for consistent color, icon, and animation access.
    /// </summary>
    /// <remarks>
    /// Use <see cref="GetColor"/>, <see cref="GetSpectreColor"/>, 
    /// <see cref="GetIcon"/>, and <see cref="GetHealthColor"/> helper methods
    /// rather than accessing this directly for most use cases.
    /// </remarks>
    protected readonly IThemeService Theme;

    /// <summary>
    /// Logger for diagnostic output and debugging.
    /// </summary>
    /// <remarks>
    /// All lifecycle transitions are logged at Debug level automatically.
    /// Use this logger for component-specific logging needs.
    /// </remarks>
    protected readonly ILogger Logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // OPTIONAL DEPENDENCIES (Property Injection)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Optional accessibility service for screen reader support.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property is set via property injection if <c>IAccessibilityService</c>
    /// is registered in the DI container. Check for null before using.
    /// </para>
    /// <para>
    /// <b>Usage Example:</b>
    /// <code>
    /// if (Accessibility?.IsScreenReaderActive == true)
    /// {
    ///     Accessibility.Announce("Health is critically low");
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public IAccessibilityService? Accessibility { get; set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT METADATA
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable name of this component for logging and diagnostics.
    /// </summary>
    /// <remarks>
    /// This should be a short, descriptive name without namespace or suffix.
    /// Examples: "HealthBar", "CombatGrid", "StatusEffect".
    /// </remarks>
    protected abstract string ComponentName { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE STATE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public bool IsActive { get; private set; }

    /// <summary>
    /// Tracks whether <see cref="Dispose"/> has been called.
    /// </summary>
    private bool _isDisposed;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="TuiComponentBase"/> class.
    /// </summary>
    /// <param name="theme">
    /// The theme service for color, icon, and animation access.
    /// Cannot be <c>null</c>.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output.
    /// Cannot be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="theme"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    protected TuiComponentBase(IThemeService theme, ILogger logger)
    {
        // Validate required dependencies - these are essential for component operation
        Theme = theme ?? throw new ArgumentNullException(nameof(theme),
            "Theme service is required for TUI components.");
        Logger = logger ?? throw new ArgumentNullException(nameof(logger),
            "Logger is required for TUI components.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// This method is idempotent - multiple calls have no additional effect.
    /// Subclasses should override <see cref="OnInitialize"/> for custom initialization.
    /// </remarks>
    public void Initialize()
    {
        // Guard: Already initialized - no-op
        if (IsInitialized)
        {
            Logger.LogDebug("{Component} already initialized, skipping", ComponentName);
            return;
        }

        Logger.LogDebug("Initializing {Component}", ComponentName);

        // Call subclass initialization hook
        OnInitialize();

        // Mark as initialized
        IsInitialized = true;

        Logger.LogDebug("Initialized {Component}", ComponentName);
    }

    /// <inheritdoc />
    /// <remarks>
    /// If not initialized, <see cref="Initialize"/> is called automatically.
    /// This method is idempotent when already active.
    /// Subclasses should override <see cref="OnActivate"/> for custom activation.
    /// </remarks>
    public void Activate()
    {
        // Ensure initialization before activation
        if (!IsInitialized)
        {
            Initialize();
        }

        // Guard: Already active - no-op
        if (IsActive)
        {
            Logger.LogDebug("{Component} already active, skipping", ComponentName);
            return;
        }

        Logger.LogDebug("Activating {Component}", ComponentName);

        // Call subclass activation hook
        OnActivate();

        // Mark as active
        IsActive = true;

        Logger.LogDebug("Activated {Component}", ComponentName);
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method is idempotent when already inactive.
    /// Subclasses should override <see cref="OnDeactivate"/> for custom deactivation.
    /// </remarks>
    public void Deactivate()
    {
        // Guard: Not active - no-op
        if (!IsActive)
        {
            Logger.LogDebug("{Component} already inactive, skipping", ComponentName);
            return;
        }

        Logger.LogDebug("Deactivating {Component}", ComponentName);

        // Call subclass deactivation hook
        OnDeactivate();

        // Mark as inactive
        IsActive = false;

        Logger.LogDebug("Deactivated {Component}", ComponentName);
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method is idempotent - multiple calls have no additional effect.
    /// Deactivates the component if active before disposing.
    /// Subclasses should override <see cref="OnDispose"/> for custom cleanup.
    /// </remarks>
    public void Dispose()
    {
        // Guard: Already disposed - no-op
        if (_isDisposed)
        {
            return;
        }

        // Ensure deactivation before disposal
        if (IsActive)
        {
            Deactivate();
        }

        Logger.LogDebug("Disposing {Component}", ComponentName);

        // Call subclass disposal hook
        OnDispose();

        // Mark as disposed
        _isDisposed = true;

        Logger.LogDebug("Disposed {Component}", ComponentName);

        // Suppress finalization (standard dispose pattern)
        GC.SuppressFinalize(this);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OVERRIDABLE LIFECYCLE HOOKS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Called during <see cref="Initialize"/> for subclass-specific initialization.
    /// </summary>
    /// <remarks>
    /// Override this method to perform one-time setup operations such as:
    /// <list type="bullet">
    ///   <item><description>Loading configuration data</description></item>
    ///   <item><description>Creating reusable resources</description></item>
    ///   <item><description>Validating component state</description></item>
    /// </list>
    /// The base implementation does nothing.
    /// </remarks>
    protected virtual void OnInitialize() { }

    /// <summary>
    /// Called during <see cref="Activate"/> for subclass-specific activation.
    /// </summary>
    /// <remarks>
    /// Override this method to perform activation operations such as:
    /// <list type="bullet">
    ///   <item><description>Starting animations</description></item>
    ///   <item><description>Subscribing to events</description></item>
    ///   <item><description>Refreshing displayed data</description></item>
    /// </list>
    /// The base implementation does nothing.
    /// </remarks>
    protected virtual void OnActivate() { }

    /// <summary>
    /// Called during <see cref="Deactivate"/> for subclass-specific deactivation.
    /// </summary>
    /// <remarks>
    /// Override this method to perform deactivation operations such as:
    /// <list type="bullet">
    ///   <item><description>Pausing animations</description></item>
    ///   <item><description>Unsubscribing from events</description></item>
    ///   <item><description>Saving temporary state</description></item>
    /// </list>
    /// The base implementation does nothing.
    /// </remarks>
    protected virtual void OnDeactivate() { }

    /// <summary>
    /// Called during <see cref="Dispose"/> for subclass-specific cleanup.
    /// </summary>
    /// <remarks>
    /// Override this method to release resources such as:
    /// <list type="bullet">
    ///   <item><description>Unmanaged resources</description></item>
    ///   <item><description>Event handlers</description></item>
    ///   <item><description>Cached data</description></item>
    /// </list>
    /// The base implementation does nothing.
    /// </remarks>
    protected virtual void OnDispose() { }

    // ═══════════════════════════════════════════════════════════════════════════
    // THEME HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a <see cref="ConsoleColor"/> for the specified color key.
    /// </summary>
    /// <param name="key">The color key to look up.</param>
    /// <returns>
    /// The nearest <see cref="ConsoleColor"/> matching the theme color.
    /// </returns>
    /// <remarks>
    /// Uses the TUI theme adapter to convert the platform-agnostic theme color
    /// to the nearest available console color.
    /// </remarks>
    /// <example>
    /// <code>
    /// var color = GetColor(ColorKey.HealthCritical);
    /// Console.ForegroundColor = color;
    /// </code>
    /// </example>
    protected ConsoleColor GetColor(ColorKey key)
    {
        // Get the theme color from the service
        var themeColor = Theme.GetColor(key);

        // Convert to console color via the TUI adapter
        // Note: We know Theme is TuiThemeAdapter for TUI components
        if (Theme is TuiThemeAdapter tuiAdapter)
        {
            return tuiAdapter.ToConsoleColor(themeColor);
        }

        // Fallback: Use a simple mapping if adapter cast fails (shouldn't happen in practice)
        Logger.LogWarning("Theme is not TuiThemeAdapter, using fallback color mapping");
        return ConsoleColor.Gray;
    }

    /// <summary>
    /// Gets a Spectre.Console <see cref="Color"/> for the specified color key.
    /// </summary>
    /// <param name="key">The color key to look up.</param>
    /// <returns>
    /// A Spectre.Console <see cref="Color"/> with the exact RGB values from the theme.
    /// </returns>
    /// <remarks>
    /// Spectre.Console supports true color output on modern terminals,
    /// providing better color accuracy than <see cref="GetColor"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var color = GetSpectreColor(ColorKey.Mana);
    /// AnsiConsole.MarkupLine($"[rgb({color.R},{color.G},{color.B})]Mana Bar[/]");
    /// </code>
    /// </example>
    protected Color GetSpectreColor(ColorKey key)
    {
        // Get the theme color from the service
        var themeColor = Theme.GetColor(key);

        // Create Spectre color directly from RGB values
        return new Color(themeColor.R, themeColor.G, themeColor.B);
    }

    /// <summary>
    /// Gets the icon string for the specified icon key.
    /// </summary>
    /// <param name="key">The icon key to look up.</param>
    /// <returns>
    /// The Unicode icon string, or ASCII fallback if Unicode is not supported.
    /// </returns>
    /// <remarks>
    /// The theme service automatically handles ASCII fallback based on the
    /// <see cref="IThemeService.UseAsciiIcons"/> setting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var healthIcon = GetIcon(IconKey.Health);   // Returns "♥" or "[HP]"
    /// var manaIcon = GetIcon(IconKey.Mana);       // Returns "◆" or "[MP]"
    /// </code>
    /// </example>
    protected string GetIcon(IconKey key)
    {
        return Theme.GetIcon(key);
    }

    /// <summary>
    /// Gets the health color based on a percentage value.
    /// </summary>
    /// <param name="percentage">
    /// The health percentage as a value from 0.0 to 1.0.
    /// </param>
    /// <returns>
    /// A <see cref="ConsoleColor"/> appropriate for the health percentage:
    /// <list type="bullet">
    ///   <item><description>76-100%: Green (full health)</description></item>
    ///   <item><description>51-75%: Light green (good health)</description></item>
    ///   <item><description>26-50%: Yellow (low health)</description></item>
    ///   <item><description>0-25%: Red (critical health)</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// var percentage = (double)currentHealth / maxHealth;
    /// var color = GetHealthColor(percentage);
    /// Console.ForegroundColor = color;
    /// Console.Write(healthBar);
    /// </code>
    /// </example>
    protected ConsoleColor GetHealthColor(double percentage)
    {
        // Get the theme color based on health percentage
        var themeColor = Theme.GetHealthColor(percentage);

        // Convert to console color via the TUI adapter
        if (Theme is TuiThemeAdapter tuiAdapter)
        {
            return tuiAdapter.ToConsoleColor(themeColor);
        }

        // Fallback mapping based on percentage thresholds
        return percentage switch
        {
            > 0.75 => ConsoleColor.Green,
            > 0.50 => ConsoleColor.DarkGreen,
            > 0.25 => ConsoleColor.Yellow,
            _ => ConsoleColor.Red
        };
    }

    /// <summary>
    /// Gets a Spectre.Console <see cref="Color"/> for health based on percentage.
    /// </summary>
    /// <param name="percentage">
    /// The health percentage as a value from 0.0 to 1.0.
    /// </param>
    /// <returns>
    /// A Spectre.Console <see cref="Color"/> with the exact RGB values for the health state.
    /// </returns>
    protected Color GetHealthSpectreColor(double percentage)
    {
        var themeColor = Theme.GetHealthColor(percentage);
        return new Color(themeColor.R, themeColor.G, themeColor.B);
    }

    /// <summary>
    /// Gets the animation duration for the specified animation key.
    /// </summary>
    /// <param name="key">The animation key to look up.</param>
    /// <returns>The duration as a <see cref="TimeSpan"/>.</returns>
    /// <example>
    /// <code>
    /// var delay = GetAnimationDuration(AnimationKey.DamagePopup);
    /// await Task.Delay(delay);
    /// </code>
    /// </example>
    protected TimeSpan GetAnimationDuration(AnimationKey key)
    {
        return Theme.GetAnimationDuration(key);
    }
}

/// <summary>
/// Placeholder interface for accessibility services.
/// </summary>
/// <remarks>
/// This will be fully defined in v0.13.5f (Accessibility Features).
/// For now, it serves as a contract for optional accessibility injection.
/// </remarks>
public interface IAccessibilityService
{
    /// <summary>
    /// Gets a value indicating whether a screen reader is active.
    /// </summary>
    bool IsScreenReaderActive { get; }

    /// <summary>
    /// Announces text to the screen reader.
    /// </summary>
    /// <param name="message">The message to announce.</param>
    void Announce(string message);
}
