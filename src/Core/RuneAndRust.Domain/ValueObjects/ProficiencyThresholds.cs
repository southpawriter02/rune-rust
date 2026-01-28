// ═══════════════════════════════════════════════════════════════════════════════
// ProficiencyThresholds.cs
// Value object defining combat experience thresholds for proficiency advancement.
// Version: 0.16.1d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines combat experience thresholds for proficiency advancement.
/// </summary>
/// <remarks>
/// <para>
/// ProficiencyThresholds encapsulates the number of combats required to advance
/// through each proficiency level. Thresholds are configurable to allow balance
/// adjustments without code changes.
/// </para>
/// <para>
/// Default progression thresholds:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Transition</term>
///     <description>Combats Required</description>
///   </listheader>
///   <item>
///     <term>NonProficient → Proficient</term>
///     <description>10 combats</description>
///   </item>
///   <item>
///     <term>Proficient → Expert</term>
///     <description>25 combats</description>
///   </item>
///   <item>
///     <term>Expert → Master</term>
///     <description>50 combats</description>
///   </item>
/// </list>
/// <para>
/// Total combats to master a weapon category from scratch: 85 (10 + 25 + 50).
/// Characters starting with archetype proficiencies skip the initial 10 combats.
/// </para>
/// </remarks>
/// <param name="NonProficientToProficient">
/// Number of combats required to advance from NonProficient to Proficient.
/// </param>
/// <param name="ProficientToExpert">
/// Number of combats required to advance from Proficient to Expert.
/// </param>
/// <param name="ExpertToMaster">
/// Number of combats required to advance from Expert to Master.
/// </param>
/// <seealso cref="ProficiencyProgress"/>
/// <seealso cref="Enums.WeaponProficiencyLevel"/>
public readonly record struct ProficiencyThresholds(
    int NonProficientToProficient,
    int ProficientToExpert,
    int ExpertToMaster)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total combats needed to reach Master from NonProficient.
    /// </summary>
    /// <remarks>
    /// This is the sum of all three thresholds. A character starting with no
    /// proficiency must complete 85 combats (default) to achieve mastery.
    /// </remarks>
    public int TotalToMaster =>
        NonProficientToProficient + ProficientToExpert + ExpertToMaster;

    /// <summary>
    /// Gets the total combats needed to reach Master from Proficient.
    /// </summary>
    /// <remarks>
    /// This is relevant for characters with archetype proficiencies who start
    /// at Proficient level (75 combats with default thresholds).
    /// </remarks>
    public int TotalToMasterFromProficient =>
        ProficientToExpert + ExpertToMaster;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates default thresholds (10/25/50).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Default values are designed for balanced progression:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>NonProficient → Proficient: 10 combats (learning basics)</description></item>
    ///   <item><description>Proficient → Expert: 25 combats (developing technique)</description></item>
    ///   <item><description>Expert → Master: 50 combats (achieving mastery)</description></item>
    /// </list>
    /// </remarks>
    public static ProficiencyThresholds Default =>
        new(
            NonProficientToProficient: 10,
            ProficientToExpert: 25,
            ExpertToMaster: 50);

    /// <summary>
    /// Creates thresholds with validation.
    /// </summary>
    /// <param name="nonProficientToProficient">
    /// Combats for NonProficient → Proficient. Must be positive.
    /// </param>
    /// <param name="proficientToExpert">
    /// Combats for Proficient → Expert. Must be positive.
    /// </param>
    /// <param name="expertToMaster">
    /// Combats for Expert → Master. Must be positive.
    /// </param>
    /// <returns>Validated ProficiencyThresholds instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any threshold value is zero or negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create custom thresholds for faster progression
    /// var easyMode = ProficiencyThresholds.Create(
    ///     nonProficientToProficient: 5,
    ///     proficientToExpert: 15,
    ///     expertToMaster: 30);
    /// 
    /// // Total to master: 50 combats (vs 85 default)
    /// </code>
    /// </example>
    public static ProficiencyThresholds Create(
        int nonProficientToProficient,
        int proficientToExpert,
        int expertToMaster)
    {
        // Validate all thresholds are positive
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            nonProficientToProficient,
            nameof(nonProficientToProficient));

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            proficientToExpert,
            nameof(proficientToExpert));

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            expertToMaster,
            nameof(expertToMaster));

        return new ProficiencyThresholds(
            nonProficientToProficient,
            proficientToExpert,
            expertToMaster);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Object Overrides
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the thresholds.
    /// </summary>
    /// <returns>
    /// A string in the format "Thresholds: 10/25/50 (Total: 85)" showing
    /// individual thresholds and total combats to master.
    /// </returns>
    /// <example>
    /// <code>
    /// var thresholds = ProficiencyThresholds.Default;
    /// Console.WriteLine(thresholds.ToString());
    /// // Output: "Thresholds: 10/25/50 (Total: 85)"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Thresholds: {NonProficientToProficient}/{ProficientToExpert}/{ExpertToMaster} " +
        $"(Total: {TotalToMaster})";
}
