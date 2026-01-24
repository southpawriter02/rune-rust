// ------------------------------------------------------------------------------
// <copyright file="BruteForceResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the outcome of a brute force bypass attempt, including all
// mechanical outcomes, consequences, and retry information.
// Part of v0.15.4h Alternative Bypass Methods implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

// =============================================================================
// NOISE LEVEL ENUM
// =============================================================================

/// <summary>
/// Levels of noise produced by actions, determining alert radius.
/// </summary>
/// <remarks>
/// <para>
/// Noise levels determine how far sound travels and who might be alerted.
/// Higher noise levels increase the chance of combat encounters and
/// reduce stealth effectiveness.
/// </para>
/// <para>
/// Alert Radii by Level:
/// <list type="bullet">
///   <item><description>Silent: No sound produced</description></item>
///   <item><description>Quiet: 5 ft radius (whispers)</description></item>
///   <item><description>Moderate: 30 ft radius (normal activity)</description></item>
///   <item><description>Loud: 60 ft radius (clearly audible)</description></item>
///   <item><description>VeryLoud: 120 ft radius (alerts guards)</description></item>
///   <item><description>Extreme: Entire area (triggers alarms)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum NoiseLevel
{
    /// <summary>
    /// No sound produced.
    /// </summary>
    Silent = 0,

    /// <summary>
    /// Barely audible (5 ft radius).
    /// </summary>
    /// <remarks>
    /// Equivalent to whispers. Only creatures immediately adjacent
    /// have a chance to hear.
    /// </remarks>
    Quiet = 1,

    /// <summary>
    /// Normal activity level (30 ft radius).
    /// </summary>
    /// <remarks>
    /// Equivalent to normal conversation or footsteps.
    /// </remarks>
    Moderate = 2,

    /// <summary>
    /// Clearly audible (60 ft radius).
    /// </summary>
    /// <remarks>
    /// Equivalent to shouting or breaking wood. Guards in adjacent
    /// rooms will likely hear and investigate.
    /// </remarks>
    Loud = 3,

    /// <summary>
    /// Very loud (120 ft radius, alerts guards).
    /// </summary>
    /// <remarks>
    /// Equivalent to crashing metal or explosions. Guards throughout
    /// the building will be alerted.
    /// </remarks>
    VeryLoud = 4,

    /// <summary>
    /// Extremely loud (entire area, triggers alarms).
    /// </summary>
    /// <remarks>
    /// Maximum noise level. Everyone in the vicinity is aware of the
    /// disturbance. Often triggers automated alarm systems.
    /// </remarks>
    Extreme = 5
}

// =============================================================================
// APPLIED CONSEQUENCE VALUE OBJECT
// =============================================================================

/// <summary>
/// A consequence that was actually applied during a brute force attempt.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BruteForceConsequence"/> represents potential consequences
/// with probabilities. <see cref="AppliedConsequence"/> represents consequences
/// that actually occurred after probability checks.
/// </para>
/// </remarks>
/// <param name="Type">Category of consequence that occurred.</param>
/// <param name="Description">Narrative description of what happened.</param>
public readonly record struct AppliedConsequence(
    ConsequenceType Type,
    string Description)
{
    /// <summary>
    /// Creates a display string for the applied consequence.
    /// </summary>
    /// <returns>The description formatted for display.</returns>
    public string ToDisplayString() => Description;
}

// =============================================================================
// BRUTE FORCE RESULT VALUE OBJECT
// =============================================================================

/// <summary>
/// Represents the complete outcome of a brute force bypass attempt.
/// </summary>
/// <remarks>
/// <para>
/// This value object captures every outcome detail from a brute force attempt:
/// <list type="bullet">
///   <item><description>Success/failure/fumble status</description></item>
///   <item><description>Critical success detection (net successes ≥ 5)</description></item>
///   <item><description>Consequences that were applied</description></item>
///   <item><description>Damage to character and contents</description></item>
///   <item><description>Noise level produced</description></item>
///   <item><description>Exhaustion gained</description></item>
///   <item><description>Tool breakage (on fumble)</description></item>
///   <item><description>Retry availability and new DC</description></item>
///   <item><description>Narrative text for display</description></item>
/// </list>
/// </para>
/// <para>
/// Use the static factory methods to create appropriately-configured results
/// for each outcome type.
/// </para>
/// </remarks>
/// <param name="Success">Whether the obstacle was overcome.</param>
/// <param name="IsCritical">True if net successes were 5 or more.</param>
/// <param name="IsFumble">True if 0 successes and at least 1 botch.</param>
/// <param name="ObstacleDestroyed">True if the obstacle is now passable.</param>
/// <param name="ConsequencesApplied">List of consequences that occurred.</param>
/// <param name="DamageToCharacter">Damage taken by the character (fumble).</param>
/// <param name="DamageToContents">Damage to items behind/inside the obstacle.</param>
/// <param name="NoiseLevel">How loud the attempt was.</param>
/// <param name="ExhaustionGained">Stamina or exhaustion cost.</param>
/// <param name="ToolBroken">Whether a tool broke during the attempt.</param>
/// <param name="RetryPossible">Whether another attempt can be made.</param>
/// <param name="NewDc">DC for next attempt if retry is possible.</param>
/// <param name="NarrativeText">Descriptive text for the outcome.</param>
public readonly record struct BruteForceResult(
    bool Success,
    bool IsCritical,
    bool IsFumble,
    bool ObstacleDestroyed,
    IReadOnlyList<AppliedConsequence> ConsequencesApplied,
    int DamageToCharacter,
    int DamageToContents,
    NoiseLevel NoiseLevel,
    int ExhaustionGained,
    bool ToolBroken,
    bool RetryPossible,
    int NewDc,
    string NarrativeText)
{
    // =========================================================================
    // COMPUTED PROPERTIES
    // =========================================================================

    /// <summary>
    /// Gets a value indicating whether the character took damage.
    /// </summary>
    public bool TookDamage => DamageToCharacter > 0;

    /// <summary>
    /// Gets a value indicating whether contents were damaged.
    /// </summary>
    public bool ContentsDamaged => DamageToContents > 0;

    /// <summary>
    /// Gets a value indicating whether the attempt had any consequences.
    /// </summary>
    public bool HasConsequences => ConsequencesApplied.Count > 0;

    /// <summary>
    /// Gets the alert radius in feet based on noise level.
    /// </summary>
    public int AlertRadiusFeet => NoiseLevel switch
    {
        NoiseLevel.Silent => 0,
        NoiseLevel.Quiet => 5,
        NoiseLevel.Moderate => 30,
        NoiseLevel.Loud => 60,
        NoiseLevel.VeryLoud => 120,
        NoiseLevel.Extreme => int.MaxValue,
        _ => 0
    };

    // =========================================================================
    // STATIC FACTORY METHODS
    // =========================================================================

    /// <summary>
    /// Creates a successful brute force result.
    /// </summary>
    /// <param name="isCritical">Whether this was a critical success (net ≥ 5).</param>
    /// <param name="consequences">Consequences that were applied.</param>
    /// <param name="noise">Noise level produced.</param>
    /// <param name="narrative">Narrative description of the success.</param>
    /// <param name="contentDamage">Optional damage to contents.</param>
    /// <param name="exhaustion">Optional exhaustion gained.</param>
    /// <returns>A successful brute force result.</returns>
    /// <remarks>
    /// <para>
    /// Critical success (net successes ≥ 5) provides benefits:
    /// <list type="bullet">
    ///   <item><description>Noise reduced by one level</description></item>
    ///   <item><description>Content damage avoided</description></item>
    ///   <item><description>Exhaustion avoided</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static BruteForceResult CreateSuccess(
        bool isCritical,
        IReadOnlyList<AppliedConsequence> consequences,
        NoiseLevel noise,
        string narrative,
        int contentDamage = 0,
        int exhaustion = 0)
    {
        // Critical success reduces noise by one level
        var effectiveNoise = isCritical ? ReduceNoiseLevel(noise) : noise;

        // Critical success avoids content damage and exhaustion
        var effectiveContentDamage = isCritical ? 0 : contentDamage;
        var effectiveExhaustion = isCritical ? 0 : exhaustion;

        return new BruteForceResult(
            Success: true,
            IsCritical: isCritical,
            IsFumble: false,
            ObstacleDestroyed: true,
            ConsequencesApplied: consequences,
            DamageToCharacter: 0,
            DamageToContents: effectiveContentDamage,
            NoiseLevel: effectiveNoise,
            ExhaustionGained: effectiveExhaustion,
            ToolBroken: false,
            RetryPossible: false,
            NewDc: 0,
            NarrativeText: narrative);
    }

    /// <summary>
    /// Creates a failed brute force result.
    /// </summary>
    /// <param name="attemptNumber">Which attempt this was (1-based).</param>
    /// <param name="maxAttempts">Maximum attempts allowed.</param>
    /// <param name="baseDc">Base DC for the obstacle.</param>
    /// <param name="retryPenalty">DC increase per failed attempt.</param>
    /// <param name="narrative">Narrative description of the failure.</param>
    /// <returns>A failed brute force result.</returns>
    /// <remarks>
    /// <para>
    /// Failed attempts still produce some noise and exhaustion, but the
    /// obstacle remains intact. The character may retry if attempts remain.
    /// </para>
    /// </remarks>
    public static BruteForceResult CreateFailure(
        int attemptNumber,
        int maxAttempts,
        int baseDc,
        int retryPenalty,
        string narrative)
    {
        var canRetry = attemptNumber < maxAttempts;
        var newDc = baseDc + (attemptNumber * retryPenalty);

        return new BruteForceResult(
            Success: false,
            IsCritical: false,
            IsFumble: false,
            ObstacleDestroyed: false,
            ConsequencesApplied: Array.Empty<AppliedConsequence>(),
            DamageToCharacter: 0,
            DamageToContents: 0,
            NoiseLevel: NoiseLevel.Moderate, // Failed attempts still make noise
            ExhaustionGained: 1, // Some effort expended
            ToolBroken: false,
            RetryPossible: canRetry,
            NewDc: canRetry ? newDc : 0,
            NarrativeText: narrative);
    }

    /// <summary>
    /// Creates a fumble brute force result.
    /// </summary>
    /// <param name="attemptNumber">Which attempt this was (1-based).</param>
    /// <param name="maxAttempts">Maximum attempts allowed.</param>
    /// <param name="baseDc">Base DC for the obstacle.</param>
    /// <param name="retryPenalty">DC increase per failed attempt.</param>
    /// <param name="hadTool">Whether a tool was being used.</param>
    /// <param name="fumbleDamage">Damage dealt to the character.</param>
    /// <param name="narrative">Narrative description of the fumble.</param>
    /// <returns>A fumble brute force result.</returns>
    /// <remarks>
    /// <para>
    /// Fumble consequences:
    /// <list type="bullet">
    ///   <item><description>1d6 damage to character</description></item>
    ///   <item><description>Tool breaks if one was used</description></item>
    ///   <item><description>Maximum noise (Extreme)</description></item>
    ///   <item><description>+2 permanent DC penalty for future attempts</description></item>
    ///   <item><description>Extra exhaustion (2 instead of 1)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static BruteForceResult CreateFumble(
        int attemptNumber,
        int maxAttempts,
        int baseDc,
        int retryPenalty,
        bool hadTool,
        int fumbleDamage,
        string narrative)
    {
        var canRetry = attemptNumber < maxAttempts;
        // Fumble adds +2 permanent DC on top of normal retry penalty
        var newDc = baseDc + (attemptNumber * retryPenalty) + 2;

        var consequences = new List<AppliedConsequence>
        {
            new(ConsequenceType.SelfDamage, $"Took {fumbleDamage} damage from the attempt"),
            new(ConsequenceType.MaxNoise, "Made maximum noise")
        };

        if (hadTool)
        {
            consequences.Add(new(ConsequenceType.StructuralDamage, "Tool broken in the attempt"));
        }

        return new BruteForceResult(
            Success: false,
            IsCritical: false,
            IsFumble: true,
            ObstacleDestroyed: false,
            ConsequencesApplied: consequences.AsReadOnly(),
            DamageToCharacter: fumbleDamage,
            DamageToContents: 0,
            NoiseLevel: NoiseLevel.Extreme, // Fumbles are always maximum noise
            ExhaustionGained: 2, // Extra exhaustion from mishap
            ToolBroken: hadTool, // Tool breaks on fumble
            RetryPossible: canRetry,
            NewDc: canRetry ? newDc : 0,
            NarrativeText: narrative);
    }

    // =========================================================================
    // INSTANCE METHODS
    // =========================================================================

    /// <summary>
    /// Creates a display string for the result.
    /// </summary>
    /// <returns>A formatted multi-line string showing all outcome details.</returns>
    /// <example>
    /// <code>
    /// var result = BruteForceResult.CreateSuccess(...);
    /// Console.WriteLine(result.ToDisplayString());
    /// // Output:
    /// // === SUCCESS ===
    /// // The door crashes inward with a resounding bang.
    /// //
    /// // Noise level: Loud
    /// //
    /// // Consequences:
    /// //   - The noise echoes through nearby corridors.
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var lines = new List<string>();

        // Header based on outcome
        if (Success)
        {
            lines.Add(IsCritical
                ? "=== CRITICAL SUCCESS ==="
                : "=== SUCCESS ===");
            lines.Add(NarrativeText);
        }
        else if (IsFumble)
        {
            lines.Add("=== FUMBLE! ===");
            lines.Add(NarrativeText);
        }
        else
        {
            lines.Add("=== FAILED ===");
            lines.Add(NarrativeText);
        }

        lines.Add(string.Empty);

        // Damage information
        if (DamageToCharacter > 0)
        {
            lines.Add($"Damage taken: {DamageToCharacter}");
        }

        if (DamageToContents > 0)
        {
            lines.Add($"Contents damaged: {DamageToContents} HP");
        }

        // Noise and exhaustion
        lines.Add($"Noise level: {NoiseLevel}");

        if (ExhaustionGained > 0)
        {
            lines.Add($"Exhaustion: +{ExhaustionGained}");
        }

        // Tool status
        if (ToolBroken)
        {
            lines.Add("Tool BROKEN!");
        }

        // Retry information
        if (!Success && RetryPossible)
        {
            lines.Add($"May retry at DC {NewDc}");
        }
        else if (!Success && !RetryPossible)
        {
            lines.Add("No more attempts possible.");
        }

        // Consequences
        if (ConsequencesApplied.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Consequences:");
            foreach (var consequence in ConsequencesApplied)
            {
                lines.Add($"  - {consequence.Description}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// Reduces noise level by one step for critical success.
    /// </summary>
    /// <param name="level">Original noise level.</param>
    /// <returns>Reduced noise level.</returns>
    private static NoiseLevel ReduceNoiseLevel(NoiseLevel level) => level switch
    {
        NoiseLevel.Extreme => NoiseLevel.VeryLoud,
        NoiseLevel.VeryLoud => NoiseLevel.Loud,
        NoiseLevel.Loud => NoiseLevel.Moderate,
        NoiseLevel.Moderate => NoiseLevel.Quiet,
        _ => NoiseLevel.Silent
    };
}
