// ═══════════════════════════════════════════════════════════════════════════════
// LineageTraumaBaseline.cs
// Value object representing the Trauma Economy baseline values for a lineage.
// Version: 0.17.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Contains the Trauma Economy baseline values for a lineage.
/// </summary>
/// <remarks>
/// <para>
/// LineageTraumaBaseline defines how a lineage interacts with the Trauma Economy:
/// <list type="bullet">
///   <item><description>StartingCorruption: Permanent Corruption the character begins with</description></item>
///   <item><description>StartingStress: Initial Stress at character creation</description></item>
///   <item><description>CorruptionResistanceModifier: Bonus/penalty to Corruption resistance checks</description></item>
///   <item><description>StressResistanceModifier: Bonus/penalty to Stress resistance checks</description></item>
/// </list>
/// </para>
/// <para>
/// StartingCorruption is considered PERMANENT and cannot be cleansed below
/// this value. It represents the indelible mark of the Runic Blight on
/// certain bloodlines (particularly Rune-Marked).
/// </para>
/// <para>
/// Resistance modifiers affect the difficulty of resisting trauma accumulation.
/// Negative modifiers make the character MORE vulnerable to that trauma type.
/// </para>
/// <para>
/// Lineage trauma baselines by bloodline:
/// <list type="bullet">
///   <item><description>Clan-Born: (0, 0, 0, 0) - Baseline humans with no trauma vulnerability</description></item>
///   <item><description>Rune-Marked: (5, 0, -1, 0) - 5 permanent Corruption, -1 Corruption resistance</description></item>
///   <item><description>Iron-Blooded: (0, 0, 0, -1) - -1 Stress resistance (physical focus weakens mental fortitude)</description></item>
///   <item><description>Vargr-Kin: (0, 0, 0, 0) - Baseline values (Primal Clarity trait handles Stress reduction)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="StartingCorruption">Permanent Corruption at character creation.</param>
/// <param name="StartingStress">Initial Stress at character creation.</param>
/// <param name="CorruptionResistanceModifier">Modifier to Corruption resistance checks.</param>
/// <param name="StressResistanceModifier">Modifier to Stress resistance checks.</param>
/// <seealso cref="Entities.LineageDefinition"/>
public readonly record struct LineageTraumaBaseline(
    int StartingCorruption,
    int StartingStress,
    int CorruptionResistanceModifier,
    int StressResistanceModifier)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during baseline creation.
    /// </summary>
    private static ILogger<LineageTraumaBaseline>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this lineage has permanent starting Corruption.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="StartingCorruption"/> is greater than zero;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Rune-Marked lineage has 5 permanent Corruption that cannot be cleansed.
    /// </remarks>
    public bool HasPermanentCorruption => StartingCorruption > 0;

    /// <summary>
    /// Gets whether this lineage has starting Stress.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="StartingStress"/> is greater than zero;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Currently no lineage starts with Stress, but the design supports it.
    /// </remarks>
    public bool HasStartingStress => StartingStress > 0;

    /// <summary>
    /// Gets whether this lineage has any Corruption resistance penalty.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="CorruptionResistanceModifier"/> is negative;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Rune-Marked has -1 Corruption resistance, making them more susceptible
    /// to gaining Corruption from Aether exposure.
    /// </remarks>
    public bool HasCorruptionVulnerability => CorruptionResistanceModifier < 0;

    /// <summary>
    /// Gets whether this lineage has any Stress resistance penalty.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="StressResistanceModifier"/> is negative;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Iron-Blooded has -1 Stress resistance, as their physical focus
    /// weakens mental fortitude against psychic assault.
    /// </remarks>
    public bool HasStressVulnerability => StressResistanceModifier < 0;

    /// <summary>
    /// Gets whether this lineage has any trauma vulnerabilities.
    /// </summary>
    /// <value>
    /// <c>true</c> if the lineage has either Corruption or Stress resistance penalties;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasAnyVulnerability =>
        HasCorruptionVulnerability || HasStressVulnerability;

    /// <summary>
    /// Gets the permanent Corruption floor (cannot cleanse below this value).
    /// </summary>
    /// <value>
    /// The minimum Corruption value after cleansing attempts.
    /// Equals <see cref="StartingCorruption"/>.
    /// </value>
    /// <remarks>
    /// For Rune-Marked, this is 5. For all other lineages, this is 0.
    /// </remarks>
    public int PermanentCorruptionFloor => StartingCorruption;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="LineageTraumaBaseline"/> with validation.
    /// </summary>
    /// <param name="startingCorruption">Starting Corruption (must be non-negative).</param>
    /// <param name="startingStress">Starting Stress (must be non-negative).</param>
    /// <param name="corruptionResistanceModifier">Corruption resistance modifier.</param>
    /// <param name="stressResistanceModifier">Stress resistance modifier.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A new <see cref="LineageTraumaBaseline"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="startingCorruption"/> or <paramref name="startingStress"/>
    /// is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Rune-Marked: 5 permanent Corruption, -1 Corruption resistance
    /// var runeMarked = LineageTraumaBaseline.Create(
    ///     startingCorruption: 5,
    ///     startingStress: 0,
    ///     corruptionResistanceModifier: -1,
    ///     stressResistanceModifier: 0);
    /// 
    /// // Iron-Blooded: -1 Stress resistance
    /// var ironBlooded = LineageTraumaBaseline.Create(
    ///     startingCorruption: 0,
    ///     startingStress: 0,
    ///     corruptionResistanceModifier: 0,
    ///     stressResistanceModifier: -1);
    /// </code>
    /// </example>
    public static LineageTraumaBaseline Create(
        int startingCorruption,
        int startingStress,
        int corruptionResistanceModifier,
        int stressResistanceModifier,
        ILogger<LineageTraumaBaseline>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating LineageTraumaBaseline. StartingCorruption={StartingCorruption}, " +
            "StartingStress={StartingStress}, CorruptionResistMod={CorruptionResistMod}, " +
            "StressResistMod={StressResistMod}",
            startingCorruption,
            startingStress,
            corruptionResistanceModifier,
            stressResistanceModifier);

        // Validate non-negative starting trauma values
        ArgumentOutOfRangeException.ThrowIfNegative(startingCorruption, nameof(startingCorruption));
        ArgumentOutOfRangeException.ThrowIfNegative(startingStress, nameof(startingStress));

        var baseline = new LineageTraumaBaseline(
            startingCorruption,
            startingStress,
            corruptionResistanceModifier,
            stressResistanceModifier);

        _logger?.LogInformation(
            "Created LineageTraumaBaseline. HasPermanentCorruption={HasPermCorruption}, " +
            "HasCorruptionVulnerability={HasCorruptionVuln}, HasStressVulnerability={HasStressVuln}, " +
            "PermanentFloor={PermFloor}",
            baseline.HasPermanentCorruption,
            baseline.HasCorruptionVulnerability,
            baseline.HasStressVulnerability,
            baseline.PermanentCorruptionFloor);

        return baseline;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the Corruption value after a cleansing attempt, respecting
    /// the permanent floor.
    /// </summary>
    /// <param name="currentCorruption">Current Corruption value before cleansing.</param>
    /// <param name="amountCleansed">Amount of Corruption to cleanse.</param>
    /// <returns>
    /// New Corruption value, respecting the <see cref="PermanentCorruptionFloor"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For lineages with permanent Corruption (e.g., Rune-Marked with 5),
    /// this method ensures the result never drops below the floor.
    /// </para>
    /// <para>
    /// Example for Rune-Marked (permanent floor = 5):
    /// <list type="bullet">
    ///   <item><description>Current: 12, Cleanse: 5 → Result: 7 (above floor)</description></item>
    ///   <item><description>Current: 7, Cleanse: 5 → Result: 5 (at floor)</description></item>
    ///   <item><description>Current: 7, Cleanse: 10 → Result: 5 (clamped to floor)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var runeMarked = LineageTraumaBaseline.RuneMarked;
    /// 
    /// // Attempt to cleanse 10 Corruption from current 12
    /// int result = runeMarked.CalculateCorruptionAfterCleanse(12, 10);
    /// // Result: 5 (not 2, because permanent floor is 5)
    /// </code>
    /// </example>
    public int CalculateCorruptionAfterCleanse(int currentCorruption, int amountCleansed)
    {
        var afterCleanse = currentCorruption - amountCleansed;
        var result = Math.Max(afterCleanse, PermanentCorruptionFloor);

        _logger?.LogDebug(
            "Calculated Corruption after cleanse. Current={Current}, Cleansed={Cleansed}, " +
            "AfterCleanse={After}, Floor={Floor}, Result={Result}",
            currentCorruption,
            amountCleansed,
            afterCleanse,
            PermanentCorruptionFloor,
            result);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC BASELINE PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Default trauma baseline for Clan-Born lineage.
    /// </summary>
    /// <value>
    /// A baseline with all zeros: (0, 0, 0, 0).
    /// </value>
    /// <remarks>
    /// Clan-Born are baseline humans with no inherent trauma or vulnerabilities.
    /// Their stable bloodlines remained untainted by the Runic Blight.
    /// </remarks>
    public static LineageTraumaBaseline ClanBorn => Create(
        startingCorruption: 0,
        startingStress: 0,
        corruptionResistanceModifier: 0,
        stressResistanceModifier: 0);

    /// <summary>
    /// Default trauma baseline for Rune-Marked lineage.
    /// </summary>
    /// <value>
    /// A baseline with: 5 permanent Corruption, -1 Corruption resistance.
    /// </value>
    /// <remarks>
    /// Rune-Marked carry the All-Rune's echo: 5 permanent Corruption and
    /// -1 to Corruption resistance, reflecting their mystical taint from
    /// ancestors exposed to concentrated Aether during the Runic Blight.
    /// </remarks>
    public static LineageTraumaBaseline RuneMarked => Create(
        startingCorruption: 5,
        startingStress: 0,
        corruptionResistanceModifier: -1,
        stressResistanceModifier: 0);

    /// <summary>
    /// Default trauma baseline for Iron-Blooded lineage.
    /// </summary>
    /// <value>
    /// A baseline with: -1 Stress resistance.
    /// </value>
    /// <remarks>
    /// Iron-Blooded have fortified bodies but weaker minds: -1 to Stress
    /// resistance from their physical focus. Their proximity to Blight-metal
    /// in corrupted mines hardened their bodies but left minds vulnerable
    /// to psychic assault.
    /// </remarks>
    public static LineageTraumaBaseline IronBlooded => Create(
        startingCorruption: 0,
        startingStress: 0,
        corruptionResistanceModifier: 0,
        stressResistanceModifier: -1);

    /// <summary>
    /// Default trauma baseline for Vargr-Kin lineage.
    /// </summary>
    /// <value>
    /// A baseline with all zeros: (0, 0, 0, 0).
    /// </value>
    /// <remarks>
    /// Vargr-Kin have no baseline trauma values. Their Stress reduction
    /// comes from the Primal Clarity trait instead, which provides -10%
    /// Psychic Stress from all sources.
    /// </remarks>
    public static LineageTraumaBaseline VargrKin => Create(
        startingCorruption: 0,
        startingStress: 0,
        corruptionResistanceModifier: 0,
        stressResistanceModifier: 0);

    /// <summary>
    /// Empty/neutral trauma baseline (no trauma, no modifiers).
    /// </summary>
    /// <value>
    /// A baseline with all zeros: (0, 0, 0, 0).
    /// </value>
    /// <remarks>
    /// Useful for placeholder or default initialization.
    /// </remarks>
    public static LineageTraumaBaseline None => Create(0, 0, 0, 0);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this trauma baseline.
    /// </summary>
    /// <returns>
    /// A formatted string describing the trauma modifiers, or "No trauma modifiers"
    /// if all values are zero.
    /// </returns>
    /// <example>
    /// <code>
    /// var runeMarked = LineageTraumaBaseline.RuneMarked;
    /// Console.WriteLine(runeMarked.ToString());
    /// // Output: "5 permanent Corruption, Corruption Resist -1"
    /// 
    /// var clanBorn = LineageTraumaBaseline.ClanBorn;
    /// Console.WriteLine(clanBorn.ToString());
    /// // Output: "No trauma modifiers"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var parts = new List<string>();

        if (StartingCorruption > 0)
            parts.Add($"{StartingCorruption} permanent Corruption");
        if (StartingStress > 0)
            parts.Add($"{StartingStress} starting Stress");
        if (CorruptionResistanceModifier != 0)
            parts.Add($"Corruption Resist {CorruptionResistanceModifier:+#;-#;0}");
        if (StressResistanceModifier != 0)
            parts.Add($"Stress Resist {StressResistanceModifier:+#;-#;0}");

        return parts.Count > 0 ? string.Join(", ", parts) : "No trauma modifiers";
    }
}
