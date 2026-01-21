namespace RuneAndRust.Presentation.Shared.Enums;

/// <summary>
/// Keys for accessing animation timing definitions from the theme.
/// </summary>
/// <remarks>
/// <para>Provides consistent animation durations across TUI and GUI
/// for transitions, effects, and feedback animations.</para>
/// <para>Standard durations:</para>
/// <list type="bullet">
///   <item><description>Short (100ms) - Quick feedback, button presses</description></item>
///   <item><description>Medium (250ms) - Standard transitions</description></item>
///   <item><description>Long (500ms) - Emphasis animations</description></item>
///   <item><description>Extended (1000ms) - Dramatic effects</description></item>
/// </list>
/// </remarks>
public enum AnimationKey
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Standard Durations
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Quick feedback (100ms).</summary>
    Short,

    /// <summary>Standard transitions (250ms).</summary>
    Medium,

    /// <summary>Emphasis animations (500ms).</summary>
    Long,

    /// <summary>Dramatic effects (1000ms).</summary>
    Extended,

    // ═══════════════════════════════════════════════════════════════════════════
    // Specific Animations
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Health bar change animation.</summary>
    HealthChange,

    /// <summary>Damage popup animation.</summary>
    DamagePopup,

    /// <summary>Status effect application.</summary>
    StatusEffect,

    /// <summary>Notification appearance.</summary>
    Notification,

    /// <summary>Panel slide animation.</summary>
    PanelSlide,

    /// <summary>Achievement unlock.</summary>
    AchievementUnlock
}
