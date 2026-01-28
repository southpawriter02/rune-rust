// ═══════════════════════════════════════════════════════════════════════════════
// TrainingResult.cs
// Value object representing the outcome of an NPC training attempt.
// Version: 0.16.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the outcome of an NPC training attempt for weapon proficiency.
/// </summary>
/// <remarks>
/// <para>
/// TrainingResult is an immutable value object that encapsulates the complete
/// result of attempting to train a weapon proficiency with an NPC trainer.
/// It includes both the resource costs (time and currency) and the proficiency
/// advancement outcome.
/// </para>
/// <para>
/// NPC training differs from other acquisition methods in that it requires:
/// </para>
/// <list type="bullet">
///   <item><description>Training time (measured in weeks)</description></item>
///   <item><description>Currency payment (Pieces Silver)</description></item>
///   <item><description>A specific trainer (tracked by trainer ID)</description></item>
/// </list>
/// <para>
/// Use the static factory methods to create instances:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="CreateSuccess"/> - For successful training</description></item>
///   <item><description><see cref="CreateFailure"/> - For failed attempts</description></item>
/// </list>
/// <para>
/// Training costs scale with the target proficiency level:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Advancement</term>
///     <description>Time / Cost</description>
///   </listheader>
///   <item>
///     <term>NonProficient → Proficient</term>
///     <description>2 weeks / 50 PS</description>
///   </item>
///   <item>
///     <term>Proficient → Expert</term>
///     <description>4 weeks / 150 PS</description>
///   </item>
///   <item>
///     <term>Expert → Master</term>
///     <description>8 weeks / 400 PS</description>
///   </item>
/// </list>
/// </remarks>
/// <param name="Success">Whether the training succeeded.</param>
/// <param name="Category">The weapon category trained.</param>
/// <param name="PsCost">The Pieces Silver cost paid.</param>
/// <param name="TrainingWeeks">The training duration in weeks.</param>
/// <param name="TrainerId">The unique identifier of the NPC trainer.</param>
/// <param name="OldLevel">The proficiency level before training.</param>
/// <param name="NewLevel">The proficiency level after training.</param>
/// <param name="FailureReason">The reason for failure, if applicable.</param>
/// <seealso cref="AcquisitionMethod"/>
/// <seealso cref="AcquisitionCost"/>
/// <seealso cref="ProficiencyGainResult"/>
/// <seealso cref="WeaponCategory"/>
/// <seealso cref="WeaponProficiencyLevel"/>
public readonly record struct TrainingResult(
    bool Success,
    WeaponCategory Category,
    int PsCost,
    int TrainingWeeks,
    string? TrainerId,
    WeaponProficiencyLevel OldLevel,
    WeaponProficiencyLevel NewLevel,
    string? FailureReason)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the proficiency level changed as a result of this training.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> when <see cref="NewLevel"/> is greater than <see cref="OldLevel"/>.
    /// For successful training, this should always be <c>true</c>.
    /// </remarks>
    public bool LevelChanged => NewLevel > OldLevel;

    /// <summary>
    /// Gets the total cost of this training as an <see cref="AcquisitionCost"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="AcquisitionCost.None"/> for failed training attempts
    /// since no resources should have been consumed.
    /// </remarks>
    public AcquisitionCost TotalCost => Success
        ? AcquisitionCost.FromTraining(PsCost, TrainingWeeks)
        : AcquisitionCost.None;

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful training result.
    /// </summary>
    /// <param name="category">The weapon category trained.</param>
    /// <param name="psCost">The Pieces Silver cost paid.</param>
    /// <param name="weeks">The training duration in weeks.</param>
    /// <param name="trainerId">The unique identifier of the NPC trainer.</param>
    /// <param name="oldLevel">The proficiency level before training.</param>
    /// <param name="newLevel">The proficiency level after training.</param>
    /// <returns>A new <see cref="TrainingResult"/> indicating success.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="psCost"/> or <paramref name="weeks"/> is zero or negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="trainerId"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var result = TrainingResult.CreateSuccess(
    ///     WeaponCategory.Swords,
    ///     psCost: 150,
    ///     weeks: 4,
    ///     trainerId: "master-swordsman",
    ///     WeaponProficiencyLevel.Proficient,
    ///     WeaponProficiencyLevel.Expert);
    /// 
    /// Console.WriteLine(result.FormatResult());
    /// // "Swords: Proficient → Expert (150 PS, 4 weeks with master-swordsman)"
    /// </code>
    /// </example>
    public static TrainingResult CreateSuccess(
        WeaponCategory category,
        int psCost,
        int weeks,
        string trainerId,
        WeaponProficiencyLevel oldLevel,
        WeaponProficiencyLevel newLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(psCost, nameof(psCost));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weeks, nameof(weeks));
        ArgumentException.ThrowIfNullOrWhiteSpace(trainerId, nameof(trainerId));

        return new TrainingResult(
            Success: true,
            Category: category,
            PsCost: psCost,
            TrainingWeeks: weeks,
            TrainerId: trainerId,
            OldLevel: oldLevel,
            NewLevel: newLevel,
            FailureReason: null);
    }

    /// <summary>
    /// Creates a failed training result.
    /// </summary>
    /// <param name="category">The weapon category that was targeted.</param>
    /// <param name="currentLevel">The current proficiency level (unchanged).</param>
    /// <param name="failureReason">The reason for the failure.</param>
    /// <returns>A new <see cref="TrainingResult"/> indicating failure.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="failureReason"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var result = TrainingResult.CreateFailure(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Master,
    ///     "Already at maximum proficiency level");
    /// 
    /// Console.WriteLine(result.Success); // false
    /// Console.WriteLine(result.TotalCost.IsFree); // true (no cost on failure)
    /// </code>
    /// </example>
    public static TrainingResult CreateFailure(
        WeaponCategory category,
        WeaponProficiencyLevel currentLevel,
        string failureReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failureReason, nameof(failureReason));

        return new TrainingResult(
            Success: false,
            Category: category,
            PsCost: 0,
            TrainingWeeks: 0,
            TrainerId: null,
            OldLevel: currentLevel,
            NewLevel: currentLevel,
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
    ///   <item><description>"Swords: Proficient → Expert (150 PS, 4 weeks with master-swordsman)"</description></item>
    ///   <item><description>"FAILED: Swords - Insufficient funds for training"</description></item>
    /// </list>
    /// </returns>
    public string FormatResult()
    {
        if (!Success)
        {
            return $"FAILED: {Category} - {FailureReason}";
        }

        var weekLabel = TrainingWeeks == 1 ? "week" : "weeks";
        return $"{Category}: {OldLevel} → {NewLevel} ({PsCost} PS, {TrainingWeeks} {weekLabel} with {TrainerId})";
    }

    /// <summary>
    /// Returns a string representation of this result.
    /// </summary>
    /// <returns>The result of <see cref="FormatResult"/> for convenient logging.</returns>
    public override string ToString() => FormatResult();
}
