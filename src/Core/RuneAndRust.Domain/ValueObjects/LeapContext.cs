using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates all factors affecting a leap attempt.
/// </summary>
/// <remarks>
/// <para>
/// The leap context aggregates distance, modifiers, and environmental factors
/// to calculate the final DC for the leap skill check.
/// </para>
/// <para>
/// DC modifiers are applied in the following order:
/// <list type="number">
///   <item><description>Base DC from distance</description></item>
///   <item><description>Running start modifier (-1 DC)</description></item>
///   <item><description>Landing type modifier</description></item>
///   <item><description>Encumbrance modifier (+1 DC)</description></item>
///   <item><description>Gravity modifier ([Low Gravity] -1 DC)</description></item>
///   <item><description>Standing jump modifier (+1 DC if no run-up)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="DistanceFeet">The horizontal distance to leap in feet.</param>
/// <param name="HasRunningStart">Whether the character has a 10ft run-up.</param>
/// <param name="LandingType">The type of landing surface/target.</param>
/// <param name="IsEncumbered">Whether the character is over carrying capacity.</param>
/// <param name="HasLowGravity">Whether the area has [Low Gravity] modifier.</param>
/// <param name="FallDepth">The depth of fall if the leap fails (in feet).</param>
/// <param name="IsStandingJump">Whether forced to jump without run-up space.</param>
public readonly record struct LeapContext(
    int DistanceFeet,
    bool HasRunningStart = false,
    LandingType LandingType = LandingType.Normal,
    bool IsEncumbered = false,
    bool HasLowGravity = false,
    int FallDepth = 20,
    bool IsStandingJump = false)
{
    /// <summary>
    /// Gets the leap distance category for this context.
    /// </summary>
    public LeapDistance DistanceCategory => LeapDistanceExtensions.FromFeet(
        Math.Min(25, Math.Max(1, DistanceFeet)));

    /// <summary>
    /// Gets the base DC from distance before modifiers.
    /// </summary>
    public int BaseDc => DistanceCategory.GetBaseDc();

    /// <summary>
    /// Gets the running start DC modifier.
    /// </summary>
    /// <remarks>
    /// A 10ft clear run-up reduces DC by 1.
    /// </remarks>
    public int RunningStartModifier => HasRunningStart ? -1 : 0;

    /// <summary>
    /// Gets the standing jump DC modifier (penalty for no run-up space).
    /// </summary>
    /// <remarks>
    /// When no run-up space is available, DC increases by 1.
    /// </remarks>
    public int StandingJumpModifier => IsStandingJump ? 1 : 0;

    /// <summary>
    /// Gets the landing type DC modifier.
    /// </summary>
    public int LandingModifier => LandingType.GetDcModifier();

    /// <summary>
    /// Gets the encumbrance DC modifier.
    /// </summary>
    /// <remarks>
    /// Being over carrying capacity increases DC by 1.
    /// </remarks>
    public int EncumbranceModifier => IsEncumbered ? 1 : 0;

    /// <summary>
    /// Gets the low gravity DC modifier.
    /// </summary>
    /// <remarks>
    /// [Low Gravity] environmental modifier reduces DC by 1.
    /// </remarks>
    public int GravityModifier => HasLowGravity ? -1 : 0;

    /// <summary>
    /// Gets the total DC modifier from all factors.
    /// </summary>
    public int TotalDcModifier =>
        RunningStartModifier +
        StandingJumpModifier +
        LandingModifier +
        EncumbranceModifier +
        GravityModifier;

    /// <summary>
    /// Gets the final DC for the leap (minimum 1).
    /// </summary>
    public int FinalDc => Math.Max(1, BaseDc + TotalDcModifier);

    /// <summary>
    /// Gets the base stamina cost for this leap.
    /// </summary>
    public int BaseStaminaCost => DistanceCategory.GetBaseStaminaCost();

    /// <summary>
    /// Gets whether this leap requires Master rank (Heroic distance with high DC).
    /// </summary>
    public bool RequiresMasterRank =>
        DistanceCategory == LeapDistance.Heroic && FinalDc >= 5;

    /// <summary>
    /// Creates a simple leap context with just distance.
    /// </summary>
    /// <param name="distanceFeet">Distance to leap in feet.</param>
    /// <param name="fallDepth">Depth of fall if failed (default 20ft).</param>
    /// <returns>A new LeapContext with default modifiers.</returns>
    /// <example>
    /// <code>
    /// var context = LeapContext.Simple(15); // 15ft leap, 20ft fall depth
    /// </code>
    /// </example>
    public static LeapContext Simple(int distanceFeet, int fallDepth = 20)
    {
        return new LeapContext(
            DistanceFeet: distanceFeet,
            FallDepth: fallDepth);
    }

    /// <summary>
    /// Creates a leap context with running start.
    /// </summary>
    /// <param name="distanceFeet">Distance to leap in feet.</param>
    /// <param name="fallDepth">Depth of fall if failed.</param>
    /// <returns>A new LeapContext with running start enabled.</returns>
    /// <example>
    /// <code>
    /// var context = LeapContext.WithRunningStart(20, 30); // 20ft with running start, 30ft fall
    /// </code>
    /// </example>
    public static LeapContext WithRunningStart(int distanceFeet, int fallDepth = 20)
    {
        return new LeapContext(
            DistanceFeet: distanceFeet,
            HasRunningStart: true,
            FallDepth: fallDepth);
    }

    /// <summary>
    /// Creates a leap context for a precision landing.
    /// </summary>
    /// <param name="distanceFeet">Distance to leap in feet.</param>
    /// <param name="hasRunningStart">Whether a running start is available.</param>
    /// <param name="fallDepth">Depth of fall if failed.</param>
    /// <returns>A new LeapContext for precision landing.</returns>
    /// <example>
    /// <code>
    /// var context = LeapContext.PrecisionLanding(10, hasRunningStart: true); // Narrow ledge
    /// </code>
    /// </example>
    public static LeapContext PrecisionLanding(
        int distanceFeet,
        bool hasRunningStart = false,
        int fallDepth = 20)
    {
        return new LeapContext(
            DistanceFeet: distanceFeet,
            HasRunningStart: hasRunningStart,
            LandingType: LandingType.Precision,
            FallDepth: fallDepth);
    }

    /// <summary>
    /// Creates a leap context for a glitched landing zone.
    /// </summary>
    /// <param name="distanceFeet">Distance to leap in feet.</param>
    /// <param name="hasRunningStart">Whether a running start is available.</param>
    /// <param name="fallDepth">Depth of fall if failed.</param>
    /// <returns>A new LeapContext for glitched landing.</returns>
    public static LeapContext GlitchedLanding(
        int distanceFeet,
        bool hasRunningStart = false,
        int fallDepth = 20)
    {
        return new LeapContext(
            DistanceFeet: distanceFeet,
            HasRunningStart: hasRunningStart,
            LandingType: LandingType.Glitched,
            FallDepth: fallDepth);
    }

    /// <summary>
    /// Creates a leap context for jumping downward.
    /// </summary>
    /// <param name="distanceFeet">Horizontal distance to leap in feet.</param>
    /// <param name="fallDepth">Depth of fall if missed.</param>
    /// <returns>A new LeapContext for downward leap.</returns>
    public static LeapContext Downward(int distanceFeet, int fallDepth = 20)
    {
        return new LeapContext(
            DistanceFeet: distanceFeet,
            LandingType: LandingType.Downward,
            FallDepth: fallDepth);
    }

    /// <summary>
    /// Gets a detailed description of the leap context for display.
    /// </summary>
    /// <returns>A multi-line string describing all leap factors.</returns>
    public string ToDescription()
    {
        var lines = new List<string>
        {
            $"Leap {DistanceFeet}ft ({DistanceCategory})",
            $"Base DC: {BaseDc}"
        };

        var modifiers = new List<string>();
        if (HasRunningStart) modifiers.Add($"Running Start ({RunningStartModifier:+#;-#;+0})");
        if (IsStandingJump) modifiers.Add($"Standing Jump ({StandingJumpModifier:+#;-#;+0})");
        if (LandingModifier != 0) modifiers.Add($"{LandingType} ({LandingModifier:+#;-#;+0})");
        if (IsEncumbered) modifiers.Add($"Encumbered ({EncumbranceModifier:+#;-#;+0})");
        if (HasLowGravity) modifiers.Add($"[Low Gravity] ({GravityModifier:+#;-#;+0})");

        if (modifiers.Count > 0)
        {
            lines.Add($"Modifiers: {string.Join(", ", modifiers)}");
        }

        lines.Add($"Final DC: {FinalDc}");
        lines.Add($"Fall Depth (on failure): {FallDepth}ft");
        lines.Add($"Stamina Cost: {BaseStaminaCost}");

        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Leap {DistanceFeet}ft (DC {FinalDc}, fall {FallDepth}ft)";
}
