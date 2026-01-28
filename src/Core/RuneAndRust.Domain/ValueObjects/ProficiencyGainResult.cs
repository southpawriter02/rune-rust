// ═══════════════════════════════════════════════════════════════════════════════
// ProficiencyGainResult.cs
// Value object representing the outcome of a proficiency acquisition attempt.
// Version: 0.16.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the outcome of a proficiency acquisition or advancement attempt.
/// </summary>
/// <remarks>
/// <para>
/// ProficiencyGainResult is an immutable value object that encapsulates the
/// complete result of attempting to acquire or advance a weapon proficiency.
/// It tracks both successful and failed attempts, including the method used
/// and any costs paid.
/// </para>
/// <para>
/// Use the static factory methods to create instances:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="CreateSuccess"/> - For successful acquisitions</description></item>
///   <item><description><see cref="CreateFailure"/> - For failed attempts</description></item>
/// </list>
/// <para>
/// This result type is returned by all <c>IProficiencyAcquisitionService</c>
/// methods except <c>TrainWithNPCAsync</c> which returns <see cref="TrainingResult"/>.
/// </para>
/// </remarks>
/// <param name="Success">Whether the acquisition succeeded.</param>
/// <param name="Category">The weapon category affected.</param>
/// <param name="OldLevel">The proficiency level before the attempt.</param>
/// <param name="NewLevel">The proficiency level after the attempt (same as OldLevel if failed).</param>
/// <param name="Method">The acquisition method used.</param>
/// <param name="CostPaid">The cost paid for this acquisition.</param>
/// <param name="FailureReason">The reason for failure, if applicable.</param>
/// <seealso cref="AcquisitionMethod"/>
/// <seealso cref="AcquisitionCost"/>
/// <seealso cref="TrainingResult"/>
/// <seealso cref="WeaponCategory"/>
/// <seealso cref="WeaponProficiencyLevel"/>
public readonly record struct ProficiencyGainResult(
    bool Success,
    WeaponCategory Category,
    WeaponProficiencyLevel OldLevel,
    WeaponProficiencyLevel NewLevel,
    AcquisitionMethod Method,
    AcquisitionCost CostPaid,
    string? FailureReason)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the proficiency level changed as a result of this acquisition.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> when <see cref="NewLevel"/> is greater than <see cref="OldLevel"/>.
    /// This can be <c>false</c> even for successful operations, such as when
    /// recording combat experience that doesn't trigger a level-up.
    /// </remarks>
    public bool LevelChanged => NewLevel > OldLevel;

    /// <summary>
    /// Gets whether this acquisition was free (no cost paid).
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> for Archetype, Specialization, and CombatExperience
    /// acquisition methods.
    /// </remarks>
    public bool WasFree => CostPaid.IsFree;

    /// <summary>
    /// Gets whether this acquisition resulted in reaching Master level.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> when the character advanced to <see cref="WeaponProficiencyLevel.Master"/>
    /// as a result of this acquisition.
    /// </remarks>
    public bool ReachedMaster => NewLevel == WeaponProficiencyLevel.Master && LevelChanged;

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful proficiency gain result.
    /// </summary>
    /// <param name="category">The weapon category affected.</param>
    /// <param name="oldLevel">The proficiency level before advancement.</param>
    /// <param name="newLevel">The proficiency level after advancement.</param>
    /// <param name="method">The acquisition method used.</param>
    /// <param name="costPaid">The cost paid for this acquisition.</param>
    /// <returns>A new <see cref="ProficiencyGainResult"/> indicating success.</returns>
    /// <example>
    /// <code>
    /// var result = ProficiencyGainResult.CreateSuccess(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Proficient,
    ///     WeaponProficiencyLevel.Expert,
    ///     AcquisitionMethod.ProgressionPointPurchase,
    ///     AcquisitionCost.FromPP(2));
    /// 
    /// Console.WriteLine(result.LevelChanged); // true
    /// Console.WriteLine(result.FormatResult()); // "Swords: Proficient → Expert (2 PP)"
    /// </code>
    /// </example>
    public static ProficiencyGainResult CreateSuccess(
        WeaponCategory category,
        WeaponProficiencyLevel oldLevel,
        WeaponProficiencyLevel newLevel,
        AcquisitionMethod method,
        AcquisitionCost costPaid)
    {
        return new ProficiencyGainResult(
            Success: true,
            Category: category,
            OldLevel: oldLevel,
            NewLevel: newLevel,
            Method: method,
            CostPaid: costPaid,
            FailureReason: null);
    }

    /// <summary>
    /// Creates a failed proficiency gain result.
    /// </summary>
    /// <param name="category">The weapon category that was targeted.</param>
    /// <param name="currentLevel">The current proficiency level (unchanged).</param>
    /// <param name="method">The acquisition method attempted.</param>
    /// <param name="failureReason">The reason for the failure.</param>
    /// <returns>A new <see cref="ProficiencyGainResult"/> indicating failure.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="failureReason"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var result = ProficiencyGainResult.CreateFailure(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Master,
    ///     AcquisitionMethod.CombatExperience,
    ///     "Already at maximum proficiency level");
    /// 
    /// Console.WriteLine(result.Success); // false
    /// Console.WriteLine(result.LevelChanged); // false
    /// </code>
    /// </example>
    public static ProficiencyGainResult CreateFailure(
        WeaponCategory category,
        WeaponProficiencyLevel currentLevel,
        AcquisitionMethod method,
        string failureReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failureReason, nameof(failureReason));

        return new ProficiencyGainResult(
            Success: false,
            Category: category,
            OldLevel: currentLevel,
            NewLevel: currentLevel,
            Method: method,
            CostPaid: AcquisitionCost.None,
            FailureReason: failureReason);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the result for display in UI or logs.
    /// </summary>
    /// <returns>
    /// A human-readable result description. Examples:
    /// <list type="bullet">
    ///   <item><description>"Swords: Proficient → Expert (2 PP)"</description></item>
    ///   <item><description>"Axes: NonProficient → Proficient (Free)"</description></item>
    ///   <item><description>"FAILED: Swords - Already at maximum proficiency level"</description></item>
    /// </list>
    /// </returns>
    public string FormatResult()
    {
        if (!Success)
        {
            return $"FAILED: {Category} - {FailureReason}";
        }

        if (LevelChanged)
        {
            return $"{Category}: {OldLevel} → {NewLevel} ({CostPaid})";
        }

        // Success but no level change (e.g., experience recorded but threshold not met)
        return $"{Category}: {OldLevel} (experience recorded)";
    }

    /// <summary>
    /// Returns a string representation of this result.
    /// </summary>
    /// <returns>The result of <see cref="FormatResult"/> for convenient logging.</returns>
    public override string ToString() => FormatResult();
}
