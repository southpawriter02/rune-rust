// ═══════════════════════════════════════════════════════════════════════════════
// ProficiencyProgress.cs
// Value object tracking proficiency level and advancement progress for a weapon category.
// Version: 0.16.1d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks proficiency level and advancement progress for a weapon category.
/// </summary>
/// <remarks>
/// <para>
/// ProficiencyProgress is an immutable value object that encapsulates the current
/// proficiency state for a single weapon category. It tracks both the current level
/// and combat experience progress toward the next level.
/// </para>
/// <para>
/// To advance a proficiency, create a new instance using the factory methods:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="AddExperience"/> - Increment combat experience</description></item>
///   <item><description><see cref="AdvanceToNextLevel"/> - Promote to the next level</description></item>
/// </list>
/// <para>
/// Experience thresholds are stored in <see cref="ProficiencyThresholds"/> and
/// determine when advancement is possible.
/// </para>
/// <para>
/// Progression path:
/// </para>
/// <list type="number">
///   <item><description>NonProficient (0 exp) → 10 combats → Proficient</description></item>
///   <item><description>Proficient (0 exp) → 25 combats → Expert</description></item>
///   <item><description>Expert (0 exp) → 50 combats → Master (MAX)</description></item>
/// </list>
/// </remarks>
/// <param name="Category">The weapon category being tracked.</param>
/// <param name="CurrentLevel">The current proficiency level.</param>
/// <param name="CombatExperience">Total combats using this category at current level.</param>
/// <param name="Thresholds">Experience thresholds configuration.</param>
/// <seealso cref="ProficiencyThresholds"/>
/// <seealso cref="WeaponProficiencyLevel"/>
/// <seealso cref="WeaponCategory"/>
public readonly record struct ProficiencyProgress(
    WeaponCategory Category,
    WeaponProficiencyLevel CurrentLevel,
    int CombatExperience,
    ProficiencyThresholds Thresholds)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties - Level Status
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this proficiency is at maximum level (Master).
    /// </summary>
    /// <remarks>
    /// When at Master level, no further advancement is possible and
    /// <see cref="AdvanceToNextLevel"/> will throw an exception.
    /// </remarks>
    public bool IsAtMaxLevel => CurrentLevel == WeaponProficiencyLevel.Master;

    /// <summary>
    /// Gets whether this proficiency can advance to the next level.
    /// </summary>
    /// <remarks>
    /// This is simply the inverse of <see cref="IsAtMaxLevel"/>. A proficiency
    /// can advance if it is not yet at Master level.
    /// </remarks>
    public bool CanAdvance => !IsAtMaxLevel;

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties - Experience Tracking
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the threshold required to reach the next level.
    /// </summary>
    /// <remarks>
    /// Returns 0 if already at Master level. The threshold represents the
    /// total combats needed at the current level to advance:
    /// <list type="bullet">
    ///   <item><description>NonProficient: 10 combats (default)</description></item>
    ///   <item><description>Proficient: 25 combats (default)</description></item>
    ///   <item><description>Expert: 50 combats (default)</description></item>
    ///   <item><description>Master: 0 (max level)</description></item>
    /// </list>
    /// </remarks>
    public int ThresholdForNextLevel
    {
        get
        {
            if (IsAtMaxLevel) return 0;

            return CurrentLevel switch
            {
                WeaponProficiencyLevel.NonProficient => Thresholds.NonProficientToProficient,
                WeaponProficiencyLevel.Proficient => Thresholds.ProficientToExpert,
                WeaponProficiencyLevel.Expert => Thresholds.ExpertToMaster,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Gets the remaining experience required to reach the next level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns 0 if already at Master level. This is calculated as:
    /// <c>ThresholdForNextLevel - CombatExperience</c>
    /// </para>
    /// <para>
    /// The result is clamped to non-negative values.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // At Proficient level with 20 combats, need 25 for Expert
    /// var progress = ProficiencyProgress.Create(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Proficient,
    ///     combatExperience: 20,
    ///     ProficiencyThresholds.Default);
    /// 
    /// var remaining = progress.ExperienceToNextLevel; // 5
    /// </code>
    /// </example>
    public int ExperienceToNextLevel
    {
        get
        {
            if (IsAtMaxLevel) return 0;

            var requiredForNext = ThresholdForNextLevel;
            return Math.Max(0, requiredForNext - CombatExperience);
        }
    }

    /// <summary>
    /// Gets the progress percentage toward the next level (0-100).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns 100 if already at Master level. The percentage is calculated as:
    /// <c>(CombatExperience / ThresholdForNextLevel) * 100</c>
    /// </para>
    /// <para>
    /// The result is clamped to a maximum of 100% to handle edge cases
    /// where experience exceeds the threshold.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // At Proficient level with 12 combats (48% toward Expert)
    /// var progress = ProficiencyProgress.Create(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Proficient,
    ///     combatExperience: 12,
    ///     ProficiencyThresholds.Default);
    /// 
    /// var percentage = progress.ProgressPercentage; // 48.0
    /// </code>
    /// </example>
    public decimal ProgressPercentage
    {
        get
        {
            if (IsAtMaxLevel) return 100m;

            var threshold = ThresholdForNextLevel;
            if (threshold == 0) return 0m;

            return Math.Min(100m, (decimal)CombatExperience / threshold * 100m);
        }
    }

    /// <summary>
    /// Gets whether the experience threshold for the next level has been reached.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns false if already at Master level. When true, the proficiency
    /// is eligible for advancement via <see cref="AdvanceToNextLevel"/>.
    /// </para>
    /// <para>
    /// Note: Reaching the threshold does not automatically advance the level.
    /// The <see cref="Entities.CharacterProficiencies"/> entity or an
    /// acquisition service (v0.16.1e) must invoke the advancement explicitly.
    /// </para>
    /// </remarks>
    public bool HasReachedNextThreshold
    {
        get
        {
            if (IsAtMaxLevel) return false;
            return CombatExperience >= ThresholdForNextLevel;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new ProficiencyProgress with validation.
    /// </summary>
    /// <param name="category">The weapon category to track.</param>
    /// <param name="currentLevel">The starting proficiency level.</param>
    /// <param name="combatExperience">
    /// Initial combat experience. Must be non-negative. Typically 0 for new proficiencies.
    /// </param>
    /// <param name="thresholds">
    /// Experience thresholds configuration. Use <see cref="ProficiencyThresholds.Default"/>
    /// for standard progression.
    /// </param>
    /// <returns>A new ProficiencyProgress instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when combatExperience is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create progress for a Warrior's proficient category
    /// var swordProgress = ProficiencyProgress.Create(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Proficient,
    ///     combatExperience: 0,
    ///     ProficiencyThresholds.Default);
    /// </code>
    /// </example>
    public static ProficiencyProgress Create(
        WeaponCategory category,
        WeaponProficiencyLevel currentLevel,
        int combatExperience,
        ProficiencyThresholds thresholds)
    {
        // Validate combat experience is non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(
            combatExperience,
            nameof(combatExperience));

        return new ProficiencyProgress(
            category,
            currentLevel,
            combatExperience,
            thresholds);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Mutation Methods (Return New Instances)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new progress instance with incremented experience.
    /// </summary>
    /// <param name="combatsToAdd">
    /// Number of combats to add. Must be positive (default: 1).
    /// </param>
    /// <returns>New progress instance with updated experience.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when combatsToAdd is zero or negative.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method does not automatically advance the level when the threshold
    /// is reached. Use <see cref="HasReachedNextThreshold"/> to check eligibility,
    /// then call <see cref="AdvanceToNextLevel"/> explicitly.
    /// </para>
    /// <para>
    /// Experience can be added even at Master level (for tracking total usage),
    /// though it has no effect on advancement.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var progress = ProficiencyProgress.Create(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Proficient,
    ///     combatExperience: 24,
    ///     ProficiencyThresholds.Default);
    /// 
    /// // Add one combat (default)
    /// var updated = progress.AddExperience();
    /// // updated.CombatExperience == 25
    /// // updated.HasReachedNextThreshold == true (25 >= 25)
    /// </code>
    /// </example>
    public ProficiencyProgress AddExperience(int combatsToAdd = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            combatsToAdd,
            nameof(combatsToAdd));

        return this with { CombatExperience = CombatExperience + combatsToAdd };
    }

    /// <summary>
    /// Creates a new progress instance advanced to the next level.
    /// </summary>
    /// <returns>
    /// New progress instance at the next level with combat experience reset to 0.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called on a Master level progress (cannot advance beyond Master).
    /// </exception>
    /// <remarks>
    /// <para>
    /// Advancing resets combat experience to 0 for the new level. This means
    /// experience does NOT carry over between levels.
    /// </para>
    /// <para>
    /// Advancement path:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>NonProficient → Proficient</description></item>
    ///   <item><description>Proficient → Expert</description></item>
    ///   <item><description>Expert → Master</description></item>
    ///   <item><description>Master → (throws InvalidOperationException)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var progress = ProficiencyProgress.Create(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Proficient,
    ///     combatExperience: 25,
    ///     ProficiencyThresholds.Default);
    /// 
    /// // Advance to Expert
    /// var expert = progress.AdvanceToNextLevel();
    /// // expert.CurrentLevel == WeaponProficiencyLevel.Expert
    /// // expert.CombatExperience == 0
    /// </code>
    /// </example>
    public ProficiencyProgress AdvanceToNextLevel()
    {
        if (IsAtMaxLevel)
        {
            throw new InvalidOperationException(
                $"Cannot advance {Category} beyond Master level.");
        }

        var nextLevel = CurrentLevel switch
        {
            WeaponProficiencyLevel.NonProficient => WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Proficient => WeaponProficiencyLevel.Expert,
            WeaponProficiencyLevel.Expert => WeaponProficiencyLevel.Master,
            _ => throw new InvalidOperationException(
                $"Invalid proficiency level: {CurrentLevel}")
        };

        return new ProficiencyProgress(
            Category,
            nextLevel,
            CombatExperience: 0, // Reset experience on level up
            Thresholds);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the progress for display in UI or logs.
    /// </summary>
    /// <returns>
    /// A formatted string showing level and progress. Examples:
    /// <list type="bullet">
    ///   <item><description>"Proficient (24/25 to Expert)"</description></item>
    ///   <item><description>"Expert (12/50 to Master)"</description></item>
    ///   <item><description>"Master (MAX)"</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// var progress = ProficiencyProgress.Create(
    ///     WeaponCategory.Swords,
    ///     WeaponProficiencyLevel.Proficient,
    ///     combatExperience: 24,
    ///     ProficiencyThresholds.Default);
    /// 
    /// Console.WriteLine(progress.FormatProgress());
    /// // Output: "Proficient (24/25 to Expert)"
    /// </code>
    /// </example>
    public string FormatProgress()
    {
        if (IsAtMaxLevel)
        {
            return $"{CurrentLevel} (MAX)";
        }

        return $"{CurrentLevel} ({CombatExperience}/{ThresholdForNextLevel} to " +
               $"{GetNextLevelName()})";
    }

    /// <summary>
    /// Formats the progress with category name for detailed display.
    /// </summary>
    /// <returns>
    /// A formatted string including the category name. Examples:
    /// <list type="bullet">
    ///   <item><description>"Swords: Proficient (24/25 to Expert)"</description></item>
    ///   <item><description>"Axes: Master (MAX)"</description></item>
    /// </list>
    /// </returns>
    public string FormatWithCategory()
    {
        return $"{Category}: {FormatProgress()}";
    }

    /// <summary>
    /// Gets the name of the next proficiency level.
    /// </summary>
    /// <returns>
    /// The display name of the next level, or "Unknown" for invalid states.
    /// </returns>
    private string GetNextLevelName()
    {
        return CurrentLevel switch
        {
            WeaponProficiencyLevel.NonProficient => "Proficient",
            WeaponProficiencyLevel.Proficient => "Expert",
            WeaponProficiencyLevel.Expert => "Master",
            _ => "Unknown"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Object Overrides
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this progress.
    /// </summary>
    /// <returns>
    /// The result of <see cref="FormatProgress"/> for convenient logging.
    /// </returns>
    public override string ToString() => FormatProgress();
}
