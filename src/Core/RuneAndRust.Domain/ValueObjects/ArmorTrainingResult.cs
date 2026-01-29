// ═══════════════════════════════════════════════════════════════════════════════
// ArmorTrainingResult.cs
// Value object representing the outcome of armor proficiency training.
// Version: 0.16.2e
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of an armor proficiency training attempt.
/// </summary>
/// <remarks>
/// <para>
/// ArmorTrainingResult encapsulates the outcome of training, including:
/// </para>
/// <list type="bullet">
///   <item><description>Whether training succeeded</description></item>
///   <item><description>The previous and new proficiency levels</description></item>
///   <item><description>Resources consumed</description></item>
///   <item><description>Failure reasons if applicable</description></item>
/// </list>
/// <para>
/// Training can fail for several reasons:
/// </para>
/// <list type="bullet">
///   <item><description>Insufficient currency</description></item>
///   <item><description>Character level too low</description></item>
///   <item><description>Current proficiency not at required level</description></item>
///   <item><description>Already at maximum proficiency (Master)</description></item>
///   <item><description>No trainer available</description></item>
/// </list>
/// <para>
/// On success, the character's proficiency level is updated and resources are
/// deducted. Time is also advanced by the training duration.
/// </para>
/// </remarks>
/// <seealso cref="ArmorTrainingRequirement"/>
/// <seealso cref="ArmorProficiencyLevel"/>
public readonly record struct ArmorTrainingResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether the training succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the armor category that was trained.
    /// </summary>
    public ArmorCategory ArmorCategory { get; init; }

    /// <summary>
    /// Gets the proficiency level before training.
    /// </summary>
    public ArmorProficiencyLevel PreviousLevel { get; init; }

    /// <summary>
    /// Gets the proficiency level after training.
    /// </summary>
    /// <remarks>
    /// If training failed, this equals <see cref="PreviousLevel"/>.
    /// </remarks>
    public ArmorProficiencyLevel NewLevel { get; init; }

    /// <summary>
    /// Gets the currency spent on training.
    /// </summary>
    /// <remarks>
    /// Currency is only deducted on successful training.
    /// Returns 0 for failed training attempts.
    /// </remarks>
    public int CurrencySpent { get; init; }

    /// <summary>
    /// Gets the training time spent in weeks.
    /// </summary>
    /// <remarks>
    /// Training time is only consumed on successful training.
    /// Returns 0 for failed training attempts.
    /// </remarks>
    public int TimeSpentWeeks { get; init; }

    /// <summary>
    /// Gets the list of reasons why training failed.
    /// </summary>
    /// <remarks>
    /// Empty for successful training attempts.
    /// May contain multiple reasons if several prerequisites were not met.
    /// </remarks>
    public IReadOnlyList<string> FailureReasons { get; init; }

    /// <summary>
    /// Gets an optional success message describing the training.
    /// </summary>
    public string Message { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether the proficiency level was improved.
    /// </summary>
    public bool LevelImproved => NewLevel > PreviousLevel;

    /// <summary>
    /// Gets a value indicating whether this result has failure reasons.
    /// </summary>
    public bool HasFailureReasons => FailureReasons.Count > 0;

    /// <summary>
    /// Gets the first failure reason, or empty string if none.
    /// </summary>
    public string PrimaryFailureReason =>
        HasFailureReasons ? FailureReasons[0] : string.Empty;

    /// <summary>
    /// Gets a summary of resources consumed.
    /// </summary>
    /// <example>"50 PS, 2 weeks"</example>
    public string ResourceSummary
    {
        get
        {
            if (CurrencySpent == 0 && TimeSpentWeeks == 0)
            {
                return "No resources consumed";
            }

            var parts = new List<string>();

            if (CurrencySpent > 0)
            {
                parts.Add($"{CurrencySpent} PS");
            }

            if (TimeSpentWeeks > 0)
            {
                parts.Add($"{TimeSpentWeeks} week{(TimeSpentWeeks != 1 ? "s" : "")}");
            }

            return string.Join(", ", parts);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods - Success
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful training result.
    /// </summary>
    /// <param name="armorCategory">The armor category trained.</param>
    /// <param name="previousLevel">The proficiency level before training.</param>
    /// <param name="newLevel">The proficiency level after training.</param>
    /// <param name="currencySpent">Currency consumed.</param>
    /// <param name="timeSpentWeeks">Training time consumed in weeks.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful <see cref="ArmorTrainingResult"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when newLevel is not greater than previousLevel.
    /// </exception>
    public static ArmorTrainingResult Successful(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel previousLevel,
        ArmorProficiencyLevel newLevel,
        int currencySpent,
        int timeSpentWeeks,
        string? message = null)
    {
        // Validate that training actually improved the level
        if (newLevel <= previousLevel)
        {
            throw new ArgumentOutOfRangeException(
                nameof(newLevel),
                $"New level ({newLevel}) must be greater than previous level ({previousLevel}).");
        }

        return new ArmorTrainingResult
        {
            Success = true,
            ArmorCategory = armorCategory,
            PreviousLevel = previousLevel,
            NewLevel = newLevel,
            CurrencySpent = currencySpent,
            TimeSpentWeeks = timeSpentWeeks,
            FailureReasons = Array.Empty<string>(),
            Message = message ?? $"Successfully trained {armorCategory} to {newLevel}."
        };
    }

    /// <summary>
    /// Creates a result for successful training to Proficient level.
    /// </summary>
    /// <param name="armorCategory">The armor category trained.</param>
    /// <param name="currencySpent">Currency consumed.</param>
    /// <param name="timeSpentWeeks">Training time consumed.</param>
    /// <returns>A successful result for NonProficient → Proficient.</returns>
    public static ArmorTrainingResult TrainedToProficient(
        ArmorCategory armorCategory,
        int currencySpent,
        int timeSpentWeeks)
    {
        return Successful(
            armorCategory,
            ArmorProficiencyLevel.NonProficient,
            ArmorProficiencyLevel.Proficient,
            currencySpent,
            timeSpentWeeks,
            $"You are now proficient with {armorCategory} armor.");
    }

    /// <summary>
    /// Creates a result for successful training to Expert level.
    /// </summary>
    /// <param name="armorCategory">The armor category trained.</param>
    /// <param name="currencySpent">Currency consumed.</param>
    /// <param name="timeSpentWeeks">Training time consumed.</param>
    /// <returns>A successful result for Proficient → Expert.</returns>
    public static ArmorTrainingResult TrainedToExpert(
        ArmorCategory armorCategory,
        int currencySpent,
        int timeSpentWeeks)
    {
        return Successful(
            armorCategory,
            ArmorProficiencyLevel.Proficient,
            ArmorProficiencyLevel.Expert,
            currencySpent,
            timeSpentWeeks,
            $"You have become an expert with {armorCategory} armor.");
    }

    /// <summary>
    /// Creates a result for successful training to Master level.
    /// </summary>
    /// <param name="armorCategory">The armor category trained.</param>
    /// <param name="currencySpent">Currency consumed.</param>
    /// <param name="timeSpentWeeks">Training time consumed.</param>
    /// <returns>A successful result for Expert → Master.</returns>
    public static ArmorTrainingResult TrainedToMaster(
        ArmorCategory armorCategory,
        int currencySpent,
        int timeSpentWeeks)
    {
        return Successful(
            armorCategory,
            ArmorProficiencyLevel.Expert,
            ArmorProficiencyLevel.Master,
            currencySpent,
            timeSpentWeeks,
            $"You have achieved mastery with {armorCategory} armor!");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods - Failure
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a failed training result.
    /// </summary>
    /// <param name="armorCategory">The armor category that failed to train.</param>
    /// <param name="currentLevel">The character's current proficiency level.</param>
    /// <param name="failureReasons">The reasons training failed.</param>
    /// <returns>A failed <see cref="ArmorTrainingResult"/>.</returns>
    public static ArmorTrainingResult Failed(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel currentLevel,
        IReadOnlyList<string> failureReasons)
    {
        return new ArmorTrainingResult
        {
            Success = false,
            ArmorCategory = armorCategory,
            PreviousLevel = currentLevel,
            NewLevel = currentLevel,
            CurrencySpent = 0,
            TimeSpentWeeks = 0,
            FailureReasons = failureReasons,
            Message = $"Training failed: {string.Join("; ", failureReasons)}"
        };
    }

    /// <summary>
    /// Creates a failed result with a single failure reason.
    /// </summary>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="currentLevel">Current proficiency level.</param>
    /// <param name="reason">The reason training failed.</param>
    /// <returns>A failed result with one reason.</returns>
    public static ArmorTrainingResult Failed(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel currentLevel,
        string reason)
    {
        return Failed(armorCategory, currentLevel, new[] { reason });
    }

    /// <summary>
    /// Creates a result for insufficient currency failure.
    /// </summary>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="currentLevel">Current proficiency level.</param>
    /// <param name="required">The required currency amount.</param>
    /// <param name="available">The available currency amount.</param>
    /// <returns>A failure result for insufficient funds.</returns>
    public static ArmorTrainingResult InsufficientCurrency(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel currentLevel,
        int required,
        int available)
    {
        return Failed(
            armorCategory,
            currentLevel,
            $"Insufficient funds: requires {required} PS, have {available} PS.");
    }

    /// <summary>
    /// Creates a result for character level too low failure.
    /// </summary>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="currentLevel">Current proficiency level.</param>
    /// <param name="requiredLevel">Required character level.</param>
    /// <param name="actualLevel">The character's actual level.</param>
    /// <returns>A failure result for insufficient character level.</returns>
    public static ArmorTrainingResult LevelTooLow(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel currentLevel,
        int requiredLevel,
        int actualLevel)
    {
        return Failed(
            armorCategory,
            currentLevel,
            $"Character level too low: requires Level {requiredLevel}, currently Level {actualLevel}.");
    }

    /// <summary>
    /// Creates a result for already at maximum proficiency.
    /// </summary>
    /// <param name="armorCategory">The armor category.</param>
    /// <returns>A failure result for already at Master level.</returns>
    public static ArmorTrainingResult AlreadyMaster(ArmorCategory armorCategory)
    {
        return Failed(
            armorCategory,
            ArmorProficiencyLevel.Master,
            $"Already at Master proficiency with {armorCategory} armor.");
    }

    /// <summary>
    /// Creates a result for no trainer available failure.
    /// </summary>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="currentLevel">Current proficiency level.</param>
    /// <returns>A failure result for missing trainer.</returns>
    public static ArmorTrainingResult NoTrainerAvailable(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel currentLevel)
    {
        return Failed(
            armorCategory,
            currentLevel,
            $"No trainer available for {armorCategory} armor in this location.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the result for display.
    /// </summary>
    /// <returns>A formatted result string.</returns>
    public string FormatForDisplay()
    {
        if (Success)
        {
            return $"✓ {Message} ({ResourceSummary})";
        }

        return $"✗ {PrimaryFailureReason}";
    }

    /// <summary>
    /// Returns a debug-friendly string representation.
    /// </summary>
    public override string ToString()
    {
        var status = Success ? "Success" : "Failed";
        return $"ArmorTrainingResult({status}, {ArmorCategory}, {PreviousLevel} → {NewLevel})";
    }
}
