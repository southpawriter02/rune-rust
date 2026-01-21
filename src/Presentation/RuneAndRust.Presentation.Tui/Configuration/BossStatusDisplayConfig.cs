namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for boss status display elements.
/// </summary>
/// <remarks>
/// <para>Loaded from config/boss-status-display.json.</para>
/// <para>Provides customization for position, size, colors, and timing
/// of phase indicator, enrage timer, and vulnerability window UI elements.</para>
/// </remarks>
public class BossStatusDisplayConfig
{
    /// <summary>
    /// Phase indicator display configuration.
    /// </summary>
    public PhaseIndicatorConfig PhaseIndicator { get; set; } = new();

    /// <summary>
    /// Enrage timer display configuration.
    /// </summary>
    public EnrageTimerConfig EnrageTimer { get; set; } = new();

    /// <summary>
    /// Vulnerability window display configuration.
    /// </summary>
    public VulnerabilityWindowConfig VulnerabilityWindow { get; set; } = new();

    /// <summary>
    /// Phase transition effect configuration.
    /// </summary>
    public TransitionEffectConfig TransitionEffect { get; set; } = new();

    /// <summary>
    /// Color configuration for all status elements.
    /// </summary>
    public BossStatusColors Colors { get; set; } = new();

    /// <summary>
    /// Creates a default configuration with standard values.
    /// </summary>
    /// <returns>Default configuration instance.</returns>
    public static BossStatusDisplayConfig CreateDefault() => new();
}

/// <summary>
/// Phase indicator position and size configuration.
/// </summary>
/// <remarks>
/// The phase indicator displays the current boss phase with name, number,
/// and phase-specific ability hints.
/// </remarks>
public class PhaseIndicatorConfig
{
    /// <summary>
    /// Starting X coordinate for the phase indicator.
    /// </summary>
    public int StartX { get; set; } = 0;

    /// <summary>
    /// Starting Y coordinate for the phase indicator.
    /// </summary>
    /// <remarks>
    /// Default is 7, positioned below the boss health bar (rows 0-6).
    /// </remarks>
    public int StartY { get; set; } = 7;

    /// <summary>
    /// Width of the phase indicator area.
    /// </summary>
    public int Width { get; set; } = 50;

    /// <summary>
    /// Height of the phase indicator area including hints.
    /// </summary>
    public int Height { get; set; } = 5;

    /// <summary>
    /// Maximum number of ability hints to display.
    /// </summary>
    public int MaxHints { get; set; } = 3;
}

/// <summary>
/// Enrage timer position and warning configuration.
/// </summary>
/// <remarks>
/// The enrage timer shows warnings when the boss is approaching an enrage
/// phase threshold and displays stat modifiers when enrage is active.
/// </remarks>
public class EnrageTimerConfig
{
    /// <summary>
    /// Starting X coordinate for the enrage timer.
    /// </summary>
    public int StartX { get; set; } = 0;

    /// <summary>
    /// Starting Y coordinate for the enrage timer.
    /// </summary>
    /// <remarks>
    /// Default is 12, positioned below the phase indicator (rows 7-11).
    /// </remarks>
    public int StartY { get; set; } = 12;

    /// <summary>
    /// Width of the enrage timer area.
    /// </summary>
    public int Width { get; set; } = 60;

    /// <summary>
    /// Height of the enrage timer area.
    /// </summary>
    public int Height { get; set; } = 2;

    /// <summary>
    /// Health percentage threshold at which to show enrage warning.
    /// </summary>
    /// <remarks>
    /// When boss health is within this percentage of an enrage phase threshold,
    /// a warning is displayed. Default is 10%.
    /// </remarks>
    public int WarningThreshold { get; set; } = 10;
}

/// <summary>
/// Vulnerability window display configuration.
/// </summary>
/// <remarks>
/// The vulnerability window shows when the boss takes increased damage
/// and displays the remaining duration and damage bonus.
/// </remarks>
public class VulnerabilityWindowConfig
{
    /// <summary>
    /// Starting X coordinate for the vulnerability window.
    /// </summary>
    public int StartX { get; set; } = 0;

    /// <summary>
    /// Starting Y coordinate for the vulnerability window.
    /// </summary>
    /// <remarks>
    /// Default is 14, positioned below the enrage timer (rows 12-13).
    /// </remarks>
    public int StartY { get; set; } = 14;

    /// <summary>
    /// Width of the vulnerability window area.
    /// </summary>
    public int Width { get; set; } = 70;

    /// <summary>
    /// Damage multiplier during vulnerability (default 1.5 = +50% damage).
    /// </summary>
    public float DamageMultiplier { get; set; } = 1.5f;

    /// <summary>
    /// Duration to display the "window closed" message in milliseconds.
    /// </summary>
    public int ClosedDisplayMs { get; set; } = 1000;

    /// <summary>
    /// Flash interval for urgent warnings in milliseconds.
    /// </summary>
    public int FlashIntervalMs { get; set; } = 200;
}

/// <summary>
/// Phase transition effect display configuration.
/// </summary>
/// <remarks>
/// Controls the centered transition box that appears when the boss
/// transitions to a new phase.
/// </remarks>
public class TransitionEffectConfig
{
    /// <summary>
    /// Center X coordinate for the transition box.
    /// </summary>
    public int CenterX { get; set; } = 40;

    /// <summary>
    /// Starting Y coordinate for the transition box.
    /// </summary>
    public int StartY { get; set; } = 5;

    /// <summary>
    /// Width of the transition effect box.
    /// </summary>
    public int BoxWidth { get; set; } = 55;

    /// <summary>
    /// Duration to display the transition effect in milliseconds.
    /// </summary>
    public int DisplayDurationMs { get; set; } = 2000;
}

/// <summary>
/// Color configuration for boss status elements.
/// </summary>
/// <remarks>
/// Provides customizable colors for phase indicators by behavior type,
/// enrage warnings by severity, and vulnerability window states.
/// </remarks>
public class BossStatusColors
{
    /// <summary>
    /// Color for Aggressive behavior phases.
    /// </summary>
    public ConsoleColor AggressiveColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Color for Tactical behavior phases.
    /// </summary>
    public ConsoleColor TacticalColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Color for Defensive behavior phases.
    /// </summary>
    public ConsoleColor DefensiveColor { get; set; } = ConsoleColor.Blue;

    /// <summary>
    /// Color for Enraged behavior phases and active enrage display.
    /// </summary>
    public ConsoleColor EnragedColor { get; set; } = ConsoleColor.DarkRed;

    /// <summary>
    /// Color for Summoner behavior phases.
    /// </summary>
    public ConsoleColor SummonerColor { get; set; } = ConsoleColor.Magenta;

    /// <summary>
    /// Color for active vulnerability window.
    /// </summary>
    public ConsoleColor VulnerableColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>
    /// Color for urgent warnings (1 turn remaining).
    /// </summary>
    public ConsoleColor UrgentColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Color for "window closed" notification.
    /// </summary>
    public ConsoleColor WindowClosedColor { get; set; } = ConsoleColor.Gray;

    /// <summary>
    /// Color for phase transition effect box.
    /// </summary>
    public ConsoleColor TransitionBoxColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Color for critical enrage warning (&lt;5% to enrage).
    /// </summary>
    public ConsoleColor CriticalWarningColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Color for high enrage warning (5-10% to enrage).
    /// </summary>
    public ConsoleColor HighWarningColor { get; set; } = ConsoleColor.DarkYellow;

    /// <summary>
    /// Color for low enrage warning (>10% to enrage).
    /// </summary>
    public ConsoleColor LowWarningColor { get; set; } = ConsoleColor.Yellow;
}
