// ═══════════════════════════════════════════════════════════════════════════════
// AcquisitionCost.cs
// Value object representing the cost of acquiring or advancing a proficiency.
// Version: 0.16.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the cost of acquiring or advancing a weapon proficiency.
/// </summary>
/// <remarks>
/// <para>
/// AcquisitionCost is an immutable value object that encapsulates the various
/// resources that may be required to acquire or advance a weapon proficiency.
/// Different <see cref="AcquisitionMethod"/> values incur different cost types:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Method</term>
///     <description>Cost Components</description>
///   </listheader>
///   <item>
///     <term>Archetype / Specialization</term>
///     <description>Free (no cost)</description>
///   </item>
///   <item>
///     <term>CombatExperience</term>
///     <description>Free (time investment through gameplay)</description>
///   </item>
///   <item>
///     <term>ProgressionPointPurchase</term>
///     <description>Progression Points (PP) only</description>
///   </item>
///   <item>
///     <term>NpcTraining</term>
///     <description>Pieces Silver (PS) + Training Weeks</description>
///   </item>
/// </list>
/// <para>
/// Use the static factory methods to create instances:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="None"/> - For free acquisitions</description></item>
///   <item><description><see cref="FromPP"/> - For PP purchases</description></item>
///   <item><description><see cref="FromTraining"/> - For NPC training</description></item>
/// </list>
/// </remarks>
/// <param name="ProgressionPoints">The Progression Points cost (0 if not applicable).</param>
/// <param name="PiecesSilver">The Pieces Silver currency cost (0 if not applicable).</param>
/// <param name="TrainingWeeks">The training time in weeks (0 if not applicable).</param>
/// <seealso cref="AcquisitionMethod"/>
/// <seealso cref="ProficiencyGainResult"/>
/// <seealso cref="TrainingResult"/>
public readonly record struct AcquisitionCost(
    int ProgressionPoints,
    int PiecesSilver,
    int TrainingWeeks)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this acquisition has no cost.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> when all cost components are zero. This is the case
    /// for Archetype, Specialization, and CombatExperience acquisition methods.
    /// </remarks>
    public bool IsFree => ProgressionPoints == 0 && PiecesSilver == 0 && TrainingWeeks == 0;

    /// <summary>
    /// Gets whether this acquisition spends Progression Points.
    /// </summary>
    public bool SpentPP => ProgressionPoints > 0;

    /// <summary>
    /// Gets whether this acquisition spends Pieces Silver.
    /// </summary>
    public bool SpentPS => PiecesSilver > 0;

    /// <summary>
    /// Gets whether this acquisition requires training time.
    /// </summary>
    public bool SpentTime => TrainingWeeks > 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an <see cref="AcquisitionCost"/> representing no cost.
    /// </summary>
    /// <remarks>
    /// Use this for free acquisition methods such as Archetype, Specialization,
    /// and CombatExperience.
    /// </remarks>
    /// <example>
    /// <code>
    /// var cost = AcquisitionCost.None;
    /// Console.WriteLine(cost.IsFree); // true
    /// </code>
    /// </example>
    public static AcquisitionCost None => new(0, 0, 0);

    /// <summary>
    /// Creates an <see cref="AcquisitionCost"/> for a Progression Point purchase.
    /// </summary>
    /// <param name="pp">The number of Progression Points required. Must be positive.</param>
    /// <returns>A new <see cref="AcquisitionCost"/> with the specified PP cost.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="pp"/> is zero or negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var cost = AcquisitionCost.FromPP(2);
    /// Console.WriteLine(cost.ProgressionPoints); // 2
    /// Console.WriteLine(cost.SpentPP); // true
    /// </code>
    /// </example>
    public static AcquisitionCost FromPP(int pp)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pp, nameof(pp));
        return new AcquisitionCost(pp, 0, 0);
    }

    /// <summary>
    /// Creates an <see cref="AcquisitionCost"/> for NPC training.
    /// </summary>
    /// <param name="piecesSilver">The Pieces Silver cost. Must be positive.</param>
    /// <param name="weeks">The training duration in weeks. Must be positive.</param>
    /// <returns>A new <see cref="AcquisitionCost"/> with the specified training costs.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="piecesSilver"/> or <paramref name="weeks"/> is zero or negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // NonProficient → Proficient training cost
    /// var cost = AcquisitionCost.FromTraining(piecesSilver: 50, weeks: 2);
    /// Console.WriteLine(cost.PiecesSilver); // 50
    /// Console.WriteLine(cost.TrainingWeeks); // 2
    /// </code>
    /// </example>
    public static AcquisitionCost FromTraining(int piecesSilver, int weeks)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(piecesSilver, nameof(piecesSilver));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weeks, nameof(weeks));
        return new AcquisitionCost(0, piecesSilver, weeks);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this cost.
    /// </summary>
    /// <returns>
    /// A human-readable cost description. Examples:
    /// <list type="bullet">
    ///   <item><description>"Free"</description></item>
    ///   <item><description>"2 PP"</description></item>
    ///   <item><description>"50 PS, 2 weeks"</description></item>
    /// </list>
    /// </returns>
    public override string ToString()
    {
        if (IsFree)
        {
            return "Free";
        }

        var parts = new List<string>();

        if (SpentPP)
        {
            parts.Add($"{ProgressionPoints} PP");
        }

        if (SpentPS)
        {
            parts.Add($"{PiecesSilver} PS");
        }

        if (SpentTime)
        {
            var weekLabel = TrainingWeeks == 1 ? "week" : "weeks";
            parts.Add($"{TrainingWeeks} {weekLabel}");
        }

        return string.Join(", ", parts);
    }
}
