// ═══════════════════════════════════════════════════════════════════════════════
// WeaknessAnalysis.cs
// Immutable value objects representing an enemy vulnerability analysis produced
// by the Jötun-Reader's Exploit Weakness ability. Includes the WeakPoint record.
// Version: 0.20.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using System.Text;
using RuneAndRust.Domain.Enums;

/// <summary>
/// A specific weak point on an enemy's body that grants a hit bonus when targeted.
/// </summary>
/// <remarks>
/// Discovered via <see cref="WeaknessAnalysis"/>. Each weak point grants a
/// configurable hit bonus (default +2) when deliberately targeted.
/// </remarks>
public sealed record WeakPoint
{
    /// <summary>Default hit bonus for targeting a weak point.</summary>
    public const int DefaultHitBonus = 2;

    /// <summary>Gets the name of the weak point location (e.g., "Head", "Left arm").</summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>Gets the bonus to hit when targeting this location.</summary>
    public int HitBonus { get; init; } = DefaultHitBonus;

    /// <summary>Gets the description of why this location is vulnerable.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Creates a new weak point.
    /// </summary>
    /// <param name="location">Body location name.</param>
    /// <param name="description">Reason the location is vulnerable.</param>
    /// <param name="hitBonus">Hit bonus (defaults to +2).</param>
    /// <returns>A new <see cref="WeakPoint"/> instance.</returns>
    public static WeakPoint Create(string location, string description, int hitBonus = DefaultHitBonus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        return new WeakPoint
        {
            Location = location,
            HitBonus = hitBonus,
            Description = description
        };
    }

    /// <summary>Returns a human-readable representation.</summary>
    public override string ToString() =>
        $"{Location} (+{HitBonus} to hit): {Description}";
}

/// <summary>
/// Analysis of an enemy's vulnerabilities, resistances, immunities, and weak points.
/// </summary>
/// <remarks>
/// <para>
/// Produced by the Jötun-Reader's <see cref="JotunReaderAbilityId.ExploitWeakness"/>
/// ability (Tier 2). The analysis is shared with all combat participants and persists
/// until the end of combat.
/// </para>
/// <para>
/// Key mechanics:
/// </para>
/// <list type="bullet">
///   <item><description>Attacks targeting a vulnerability deal +1d6 bonus damage</description></item>
///   <item><description>Weak points grant +2 to hit when deliberately targeted</description></item>
///   <item><description>Analysis is visible to all allies in combat</description></item>
///   <item><description>Can be used once per enemy per combat</description></item>
/// </list>
/// <para>
/// <b>Cost:</b> 2 AP, 3 Lore Insight.
/// <b>Tier:</b> 2 (requires 8 PP invested in Jötun-Reader tree).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var analysis = WeaknessAnalysis.Create(
///     targetId, "Goblin Shaman",
///     new[] { DamageType.Fire, DamageType.Psychic },
///     new[] { DamageType.Cold },
///     new[] { DamageType.Necrotic },
///     new[] { WeakPoint.Create("Head", "Exposed thin skull") },
///     new[] { "Moves to cover after casting" });
///
/// analysis.IsVulnerableTo(DamageType.Fire); // true
/// </code>
/// </example>
/// <seealso cref="WeakPoint"/>
/// <seealso cref="DamageType"/>
public sealed record WeaknessAnalysis
{
    /// <summary>
    /// Bonus dice expression for attacks exploiting a vulnerability.
    /// </summary>
    public const string ExploitBonusDice = "1d6";

    /// <summary>
    /// Number of sides on the exploit bonus die.
    /// </summary>
    public const int ExploitBonusDieSides = 6;

    /// <summary>Gets the unique identifier for this analysis.</summary>
    public Guid AnalysisId { get; init; }

    /// <summary>Gets the ID of the analyzed enemy.</summary>
    public Guid TargetId { get; init; }

    /// <summary>Gets the display name of the analyzed enemy.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Gets damage types the enemy is vulnerable to (takes extra damage).</summary>
    public IReadOnlyList<DamageType> Vulnerabilities { get; init; } = [];

    /// <summary>Gets damage types the enemy resists (takes reduced damage).</summary>
    public IReadOnlyList<DamageType> Resistances { get; init; } = [];

    /// <summary>Gets damage types the enemy is immune to (takes no damage).</summary>
    public IReadOnlyList<DamageType> Immunities { get; init; } = [];

    /// <summary>Gets specific weak points on the enemy body.</summary>
    public IReadOnlyList<WeakPoint> WeakPoints { get; init; } = [];

    /// <summary>Gets observed behavioral patterns.</summary>
    public IReadOnlyList<string> BehavioralPatterns { get; init; } = [];

    /// <summary>Gets the timestamp when the analysis was performed.</summary>
    public DateTime AnalyzedAt { get; init; }

    /// <summary>Gets the timestamp when the analysis expires (end of combat).</summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Creates a new weakness analysis for an enemy.
    /// </summary>
    /// <param name="targetId">ID of the enemy being analyzed.</param>
    /// <param name="targetName">Display name of the enemy.</param>
    /// <param name="vulnerabilities">Damage types the enemy is vulnerable to.</param>
    /// <param name="resistances">Damage types the enemy resists.</param>
    /// <param name="immunities">Damage types the enemy is immune to.</param>
    /// <param name="weakPoints">Specific weak points on the enemy.</param>
    /// <param name="behaviors">Observed behavioral patterns.</param>
    /// <param name="expiresAt">Optional explicit expiration timestamp.</param>
    /// <returns>A new <see cref="WeaknessAnalysis"/> instance.</returns>
    public static WeaknessAnalysis Create(
        Guid targetId,
        string targetName,
        IEnumerable<DamageType> vulnerabilities,
        IEnumerable<DamageType> resistances,
        IEnumerable<DamageType> immunities,
        IEnumerable<WeakPoint> weakPoints,
        IEnumerable<string> behaviors,
        DateTime? expiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        ArgumentNullException.ThrowIfNull(vulnerabilities);
        ArgumentNullException.ThrowIfNull(resistances);
        ArgumentNullException.ThrowIfNull(immunities);
        ArgumentNullException.ThrowIfNull(weakPoints);
        ArgumentNullException.ThrowIfNull(behaviors);

        return new WeaknessAnalysis
        {
            AnalysisId = Guid.NewGuid(),
            TargetId = targetId,
            TargetName = targetName,
            Vulnerabilities = vulnerabilities.ToList().AsReadOnly(),
            Resistances = resistances.ToList().AsReadOnly(),
            Immunities = immunities.ToList().AsReadOnly(),
            WeakPoints = weakPoints.ToList().AsReadOnly(),
            BehavioralPatterns = behaviors.ToList().AsReadOnly(),
            AnalyzedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Determines if the enemy is vulnerable to a specific damage type.
    /// </summary>
    /// <param name="type">The damage type to check.</param>
    /// <returns><c>true</c> if the enemy is vulnerable to the specified type.</returns>
    public bool IsVulnerableTo(DamageType type) => Vulnerabilities.Contains(type);

    /// <summary>
    /// Determines if the enemy resists a specific damage type.
    /// </summary>
    /// <param name="type">The damage type to check.</param>
    /// <returns><c>true</c> if the enemy resists the specified type.</returns>
    public bool IsResistantTo(DamageType type) => Resistances.Contains(type);

    /// <summary>
    /// Determines if the enemy is immune to a specific damage type.
    /// </summary>
    /// <param name="type">The damage type to check.</param>
    /// <returns><c>true</c> if the enemy is immune to the specified type.</returns>
    public bool IsImmuneTo(DamageType type) => Immunities.Contains(type);

    /// <summary>
    /// Gets the hit bonus for attacking a specific weak point location.
    /// </summary>
    /// <param name="location">The body location to target.</param>
    /// <returns>The hit bonus, or 0 if no weak point exists at that location.</returns>
    public int GetWeakPointBonus(string location)
    {
        var weakPoint = WeakPoints.FirstOrDefault(wp =>
            wp.Location.Equals(location, StringComparison.OrdinalIgnoreCase));
        return weakPoint?.HitBonus ?? 0;
    }

    /// <summary>
    /// Formats the analysis as a human-readable combat report.
    /// </summary>
    /// <returns>A multi-line report string.</returns>
    public string GetFormattedReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"═══ {TargetName} Analysis ═══");

        if (Vulnerabilities.Count > 0)
            sb.AppendLine("Vulnerabilities: " + string.Join(", ", Vulnerabilities));

        if (Resistances.Count > 0)
            sb.AppendLine("Resistances: " + string.Join(", ", Resistances));

        if (Immunities.Count > 0)
            sb.AppendLine("Immunities: " + string.Join(", ", Immunities));

        if (WeakPoints.Count > 0)
        {
            sb.AppendLine("Weak Points:");
            foreach (var point in WeakPoints)
            {
                sb.AppendLine($"  • {point.Location} (+{point.HitBonus} to hit)");
            }
        }

        if (BehavioralPatterns.Count > 0)
        {
            sb.AppendLine("Behavioral Patterns:");
            foreach (var pattern in BehavioralPatterns)
            {
                sb.AppendLine($"  • {pattern}");
            }
        }

        return sb.ToString();
    }

    /// <summary>Returns a human-readable summary of the analysis.</summary>
    public override string ToString() =>
        $"Weakness Analysis [{TargetName}]: {Vulnerabilities.Count} vuln, " +
        $"{Resistances.Count} resist, {Immunities.Count} immune, " +
        $"{WeakPoints.Count} weak points";
}
