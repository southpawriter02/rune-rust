// ═══════════════════════════════════════════════════════════════════════════════
// GuiControlBase.cs
// Abstract base class for GUI presentation controls with standardized DI patterns.
// Version: 0.13.5c
// ═══════════════════════════════════════════════════════════════════════════════

using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Interfaces;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Services;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Gui.Base;

/// <summary>
/// Placeholder interface for accessibility services.
/// </summary>
/// <remarks>
/// This will be fully defined in v0.13.5f (Accessibility Features).
/// For now, it serves as a contract for optional accessibility injection.
/// </remarks>
public interface IGuiAccessibilityService
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

/// <summary>
/// Abstract base class for GUI presentation controls providing standardized
/// dependency injection, lifecycle management, and theme integration.
/// </summary>
/// <remarks>
/// <para>
/// <b>Purpose:</b> Provides a consistent foundation for all Avalonia GUI controls,
/// ensuring they follow standard DI patterns and have access to theme services,
/// logging, and lifecycle management.
/// </para>
/// <para>
/// <b>Inheritance:</b> Extends <see cref="UserControl"/> to integrate with
/// Avalonia's visual tree while implementing <see cref="IComponentLifecycle"/>
/// for standardized lifecycle management.
/// </para>
/// <para>
/// <b>Required Dependencies (Constructor Injection):</b>
/// <list type="bullet">
///   <item><description><see cref="IThemeService"/> - Colors, icons, animations</description></item>
///   <item><description><see cref="ILogger"/> - Diagnostic logging</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Performance:</b> Includes brush caching via <see cref="GetBrush"/> to avoid
/// creating new brush instances on each render.
/// </para>
/// <para>
/// <b>Avalonia Integration:</b> Lifecycle methods are automatically called during
/// visual tree attachment/detachment:
/// <list type="bullet">
///   <item><description><see cref="OnAttachedToVisualTree"/> → InitializeControl() + Activate()</description></item>
///   <item><description><see cref="OnDetachedFromVisualTree"/> → Deactivate()</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Note:</b> The <see cref="ThemeService"/> and <see cref="ControlIsInitialized"/>
/// properties use different names than TuiComponentBase to avoid conflicts with
/// Avalonia's <see cref="Avalonia.StyledElement.Theme"/> and
/// <see cref="Avalonia.StyledElement.IsInitialized"/> properties.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class StatusEffectControl : GuiControlBase
/// {
///     protected override string ControlName => "StatusEffect";
///     
///     public StatusEffectControl(IThemeService theme, ILogger&lt;StatusEffectControl&gt; logger)
///         : base(theme, logger)
///     {
///         InitializeComponent();
///     }
///     
///     private void UpdateEffectDisplay(string effectType)
///     {
///         var brush = GetBrush(ColorKey.Buff);
///         effectBorder.Background = brush;
///     }
/// }
/// </code>
/// </example>
public abstract class GuiControlBase : UserControl, IComponentLifecycle
{
    // ═══════════════════════════════════════════════════════════════════════════
    // REQUIRED DEPENDENCIES (Constructor Injection)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Theme service for consistent color, icon, and animation access.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Named <c>ThemeService</c> to avoid conflict with Avalonia's
    /// <see cref="Avalonia.StyledElement.Theme"/> property.
    /// </para>
    /// <para>
    /// Use <see cref="GetBrush"/>, <see cref="GetHealthBrush"/>, and 
    /// <see cref="GetIcon"/> helper methods rather than accessing this directly
    /// for most use cases.
    /// </para>
    /// </remarks>
    protected readonly IThemeService ThemeService;

    /// <summary>
    /// Logger for diagnostic output and debugging.
    /// </summary>
    /// <remarks>
    /// All lifecycle transitions are logged at Debug level automatically.
    /// Use this logger for control-specific logging needs.
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
    /// This property is set via property injection if <c>IGuiAccessibilityService</c>
    /// is registered in the DI container. Check for null before using.
    /// </para>
    /// </remarks>
    public IGuiAccessibilityService? Accessibility { get; set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTROL METADATA
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable name of this control for logging and diagnostics.
    /// </summary>
    /// <remarks>
    /// This should be a short, descriptive name without namespace or suffix.
    /// Examples: "HealthBar", "GridCell", "EntityToken".
    /// </remarks>
    protected abstract string ControlName { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE STATE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether <see cref="Initialize"/> has been called.
    /// </summary>
    /// <remarks>
    /// Named <c>ControlIsInitialized</c> to avoid conflict with Avalonia's
    /// <see cref="Avalonia.StyledElement.IsInitialized"/> property.
    /// </remarks>
    bool IComponentLifecycle.IsInitialized => ControlIsInitialized;

    /// <summary>
    /// Gets a value indicating whether the control lifecycle has been initialized.
    /// </summary>
    /// <remarks>
    /// This is distinct from Avalonia's <see cref="Avalonia.StyledElement.IsInitialized"/>
    /// which tracks XAML initialization state.
    /// </remarks>
    protected bool ControlIsInitialized { get; private set; }

    /// <inheritdoc />
    public bool IsActive { get; private set; }

    /// <summary>
    /// Tracks whether <see cref="DisposeControl"/> has been called.
    /// </summary>
    private bool _isDisposed;

    // ═══════════════════════════════════════════════════════════════════════════
    // BRUSH CACHE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cache for brushes created from theme colors, keyed by <see cref="ColorKey"/>.
    /// </summary>
    /// <remarks>
    /// Caching brushes improves performance by avoiding repeated brush creation.
    /// Use <see cref="InvalidateBrushCache"/> when the theme changes.
    /// </remarks>
    private readonly Dictionary<ColorKey, IBrush> _brushCache = new();

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiControlBase"/> class.
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
    protected GuiControlBase(IThemeService theme, ILogger logger)
    {
        // Validate required dependencies - these are essential for control operation
        ThemeService = theme ?? throw new ArgumentNullException(nameof(theme),
            "Theme service is required for GUI controls.");
        Logger = logger ?? throw new ArgumentNullException(nameof(logger),
            "Logger is required for GUI controls.");
    }

    /// <summary>
    /// Default constructor for XAML designer support.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This constructor exists to support the Avalonia XAML designer and should
    /// not be used in production code. Controls created with this constructor
    /// will have null theme and logger services.
    /// </para>
    /// <para>
    /// In production, always use the DI-enabled constructor.
    /// </para>
    /// </remarks>
    protected GuiControlBase()
    {
        // Designer support - theme and logger will be null
        // This allows XAML preview to work without full DI setup
        ThemeService = null!;
        Logger = null!;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AVALONIA LIFECYCLE INTEGRATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Called when the control is attached to the visual tree.
    /// </summary>
    /// <param name="e">Event arguments containing the visual parent.</param>
    /// <remarks>
    /// Automatically triggers <see cref="Initialize"/> (if not already initialized)
    /// and <see cref="Activate"/> to synchronize with Avalonia's visual tree lifecycle.
    /// </remarks>
    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Skip lifecycle if dependencies not set (designer mode)
        if (ThemeService == null || Logger == null)
        {
            return;
        }

        // Initialize if not already done
        if (!ControlIsInitialized)
        {
            Initialize();
        }

        // Activate when attached to visual tree
        Activate();
    }

    /// <summary>
    /// Called when the control is detached from the visual tree.
    /// </summary>
    /// <param name="e">Event arguments containing the former visual parent.</param>
    /// <remarks>
    /// Automatically triggers <see cref="Deactivate"/> to clean up when the control
    /// is removed from the visual tree.
    /// </remarks>
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        // Skip lifecycle if dependencies not set (designer mode)
        if (ThemeService != null && Logger != null)
        {
            Deactivate();
        }

        base.OnDetachedFromVisualTree(e);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// This method is idempotent - multiple calls have no additional effect.
    /// Subclasses should override <see cref="OnControlInitialize"/> for custom initialization.
    /// </remarks>
    public void Initialize()
    {
        // Guard: Already initialized - no-op
        if (ControlIsInitialized)
        {
            Logger?.LogDebug("{Control} already initialized, skipping", ControlName);
            return;
        }

        Logger?.LogDebug("Initializing {Control}", ControlName);

        // Call subclass initialization hook
        OnControlInitialize();

        // Mark as initialized
        ControlIsInitialized = true;

        Logger?.LogDebug("Initialized {Control}", ControlName);
    }

    /// <inheritdoc />
    /// <remarks>
    /// If not initialized, <see cref="Initialize"/> is called automatically.
    /// This method is idempotent when already active.
    /// Subclasses should override <see cref="OnControlActivate"/> for custom activation.
    /// </remarks>
    public void Activate()
    {
        // Ensure initialization before activation
        if (!ControlIsInitialized)
        {
            Initialize();
        }

        // Guard: Already active - no-op
        if (IsActive)
        {
            Logger?.LogDebug("{Control} already active, skipping", ControlName);
            return;
        }

        Logger?.LogDebug("Activating {Control}", ControlName);

        // Call subclass activation hook
        OnControlActivate();

        // Mark as active
        IsActive = true;

        Logger?.LogDebug("Activated {Control}", ControlName);
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method is idempotent when already inactive.
    /// Subclasses should override <see cref="OnControlDeactivate"/> for custom deactivation.
    /// </remarks>
    public void Deactivate()
    {
        // Guard: Not active - no-op
        if (!IsActive)
        {
            Logger?.LogDebug("{Control} already inactive, skipping", ControlName);
            return;
        }

        Logger?.LogDebug("Deactivating {Control}", ControlName);

        // Call subclass deactivation hook
        OnControlDeactivate();

        // Mark as inactive
        IsActive = false;

        Logger?.LogDebug("Deactivated {Control}", ControlName);
    }

    /// <summary>
    /// Disposes the control and releases all resources.
    /// </summary>
    /// <remarks>
    /// This method is idempotent - multiple calls have no additional effect.
    /// Deactivates the control if active before disposing.
    /// Subclasses should override <see cref="OnControlDispose"/> for custom cleanup.
    /// </remarks>
    void IDisposable.Dispose()
    {
        DisposeControl();
    }

    /// <summary>
    /// Internal disposal implementation for the control.
    /// </summary>
    protected void DisposeControl()
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

        Logger?.LogDebug("Disposing {Control}", ControlName);

        // Clear the brush cache
        _brushCache.Clear();

        // Call subclass disposal hook
        OnControlDispose();

        // Mark as disposed
        _isDisposed = true;

        Logger?.LogDebug("Disposed {Control}", ControlName);

        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OVERRIDABLE LIFECYCLE HOOKS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Called during <see cref="Initialize"/> for subclass-specific initialization.
    /// </summary>
    /// <remarks>
    /// Override this method to perform one-time setup operations.
    /// The base implementation does nothing.
    /// </remarks>
    protected virtual void OnControlInitialize() { }

    /// <summary>
    /// Called during <see cref="Activate"/> for subclass-specific activation.
    /// </summary>
    /// <remarks>
    /// Override this method to perform activation operations such as
    /// starting animations or subscribing to events.
    /// The base implementation does nothing.
    /// </remarks>
    protected virtual void OnControlActivate() { }

    /// <summary>
    /// Called during <see cref="Deactivate"/> for subclass-specific deactivation.
    /// </summary>
    /// <remarks>
    /// Override this method to perform deactivation operations such as
    /// pausing animations or unsubscribing from events.
    /// The base implementation does nothing.
    /// </remarks>
    protected virtual void OnControlDeactivate() { }

    /// <summary>
    /// Called during <see cref="DisposeControl"/> for subclass-specific cleanup.
    /// </summary>
    /// <remarks>
    /// Override this method to release resources.
    /// The base implementation does nothing.
    /// </remarks>
    protected virtual void OnControlDispose() { }

    // ═══════════════════════════════════════════════════════════════════════════
    // THEME HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a cached <see cref="IBrush"/> for the specified color key.
    /// </summary>
    /// <param name="key">The color key to look up.</param>
    /// <returns>
    /// An <see cref="IBrush"/> instance for the theme color.
    /// Cached for performance - same instance returned on subsequent calls.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Brushes are cached to avoid creating new instances on each call.
    /// Use <see cref="InvalidateBrushCache"/> when the theme changes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// healthBar.Background = GetBrush(ColorKey.HealthFull);
    /// statusBorder.BorderBrush = GetBrush(ColorKey.Border);
    /// </code>
    /// </example>
    protected IBrush GetBrush(ColorKey key)
    {
        // Check cache first
        if (_brushCache.TryGetValue(key, out var cachedBrush))
        {
            return cachedBrush;
        }

        // Get theme color and create brush
        var themeColor = ThemeService.GetColor(key);
        var avaloniaColor = Color.FromRgb(themeColor.R, themeColor.G, themeColor.B);
        var brush = new SolidColorBrush(avaloniaColor);

        // Cache the brush
        _brushCache[key] = brush;

        Logger?.LogDebug("Created and cached brush for {ColorKey}: {Hex}",
            key, themeColor.Hex);

        return brush;
    }

    /// <summary>
    /// Gets an <see cref="IBrush"/> for health color based on percentage.
    /// </summary>
    /// <param name="percentage">
    /// The health percentage as a value from 0.0 to 1.0.
    /// </param>
    /// <returns>
    /// An <see cref="IBrush"/> appropriate for the health percentage.
    /// Not cached due to the dynamic nature of health values.
    /// </returns>
    /// <example>
    /// <code>
    /// var percentage = (double)currentHealth / maxHealth;
    /// healthBar.Background = GetHealthBrush(percentage);
    /// </code>
    /// </example>
    protected IBrush GetHealthBrush(double percentage)
    {
        var themeColor = ThemeService.GetHealthColor(percentage);
        var avaloniaColor = Color.FromRgb(themeColor.R, themeColor.G, themeColor.B);
        return new SolidColorBrush(avaloniaColor);
    }

    /// <summary>
    /// Converts a <see cref="ThemeColor"/> to an Avalonia <see cref="Color"/>.
    /// </summary>
    /// <param name="themeColor">The theme color to convert.</param>
    /// <returns>An Avalonia <see cref="Color"/> with the same RGB values.</returns>
    protected static Color ToAvaloniaColor(ThemeColor themeColor)
    {
        return Color.FromRgb(themeColor.R, themeColor.G, themeColor.B);
    }

    /// <summary>
    /// Gets the icon string for the specified icon key.
    /// </summary>
    /// <param name="key">The icon key to look up.</param>
    /// <returns>
    /// The Unicode icon string (GUI typically supports full Unicode).
    /// </returns>
    protected string GetIcon(IconKey key)
    {
        return ThemeService.GetIcon(key);
    }

    /// <summary>
    /// Gets the animation duration for the specified animation key.
    /// </summary>
    /// <param name="key">The animation key to look up.</param>
    /// <returns>The duration as a <see cref="TimeSpan"/>.</returns>
    protected TimeSpan GetAnimationDuration(AnimationKey key)
    {
        return ThemeService.GetAnimationDuration(key);
    }

    /// <summary>
    /// Invalidates the brush cache, forcing colors to be re-fetched from the theme.
    /// </summary>
    /// <remarks>
    /// Call this method when the theme changes to ensure controls
    /// display the updated colors.
    /// </remarks>
    protected void InvalidateBrushCache()
    {
        var count = _brushCache.Count;
        _brushCache.Clear();
        Logger?.LogDebug("Invalidated brush cache for {Control}: {Count} brushes cleared",
            ControlName, count);
    }
}
