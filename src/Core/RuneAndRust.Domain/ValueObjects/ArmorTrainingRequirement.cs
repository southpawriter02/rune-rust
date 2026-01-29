// ═══════════════════════════════════════════════════════════════════════════════
// ArmorTrainingRequirement.cs
// Value object representing requirements for armor proficiency training.
// Version: 0.16.2e
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the requirements for training armor proficiency to a target level.
/// </summary>
/// <remarks>
/// <para>
/// ArmorTrainingRequirement encapsulates all costs and prerequisites needed to
/// advance a character's armor proficiency level. This includes:
/// </para>
/// <list type="bullet">
///   <item><description>Currency costs (in Pieces Silver)</description></item>
///   <item><description>Time costs (in weeks of training)</description></item>
///   <item><description>Minimum character level requirements</description></item>
///   <item><description>Previous proficiency level requirements</description></item>
/// </list>
/// <para>
/// Training costs scale with the target proficiency level:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Transition</term>
///     <description>Typical Cost</description>
///   </listheader>
///   <item>
///     <term>NonProficient → Proficient</term>
///     <description>50 PS, 2 weeks, Level 1</description>
///   </item>
///   <item>
///     <term>Proficient → Expert</term>
///     <description>200 PS, 4 weeks, Level 5</description>
///   </item>
///   <item>
///     <term>Expert → Master</term>
///     <description>500 PS, 8 weeks, Level 10</description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="ArmorProficiencyLevel"/>
/// <seealso cref="ArmorTrainingResult"/>
public readonly record struct ArmorTrainingRequirement
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the target proficiency level after training.
    /// </summary>
    /// <remarks>
    /// The target level must be greater than the character's current proficiency.
    /// Training can only advance one level at a time.
    /// </remarks>
    public ArmorProficiencyLevel TargetLevel { get; init; }

    /// <summary>
    /// Gets the required proficiency level to begin this training.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Characters must already have this proficiency level to start training.
    /// For example, to train to Expert, the character must already be Proficient.
    /// </para>
    /// <para>
    /// The required level is always one level below the target level.
    /// </para>
    /// </remarks>
    public ArmorProficiencyLevel RequiredLevel { get; init; }

    /// <summary>
    /// Gets the armor category being trained.
    /// </summary>
    public ArmorCategory ArmorCategory { get; init; }

    /// <summary>
    /// Gets the currency cost in Pieces Silver.
    /// </summary>
    /// <remarks>
    /// Currency is deducted at the start of training.
    /// Players must have sufficient funds before training can begin.
    /// </remarks>
    public int CurrencyCost { get; init; }

    /// <summary>
    /// Gets the training duration in weeks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Training time represents in-game weeks spent with a trainer.
    /// During this period, the character is assumed to be practicing daily.
    /// </para>
    /// <para>
    /// Time does not pass automatically; the game advances time when training completes.
    /// </para>
    /// </remarks>
    public int TrainingWeeks { get; init; }

    /// <summary>
    /// Gets the minimum character level required to begin training.
    /// </summary>
    /// <remarks>
    /// Characters below this level cannot train to the target proficiency,
    /// even if they have sufficient currency and prior proficiency.
    /// </remarks>
    public int MinimumCharacterLevel { get; init; }

    /// <summary>
    /// Gets a value indicating whether this training requires an NPC trainer.
    /// </summary>
    /// <remarks>
    /// When true, training can only occur at locations with appropriate trainers.
    /// Some proficiency advancements may be available through combat experience
    /// without requiring an NPC trainer.
    /// </remarks>
    public bool RequiresNpcTrainer { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether this requirement has any cost.
    /// </summary>
    public bool HasCost => CurrencyCost > 0 || TrainingWeeks > 0;

    /// <summary>
    /// Gets the level transition description.
    /// </summary>
    /// <example>"NonProficient → Proficient"</example>
    public string TransitionDescription =>
        $"{RequiredLevel} → {TargetLevel}";

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new armor training requirement.
    /// </summary>
    /// <param name="armorCategory">The armor category to train.</param>
    /// <param name="targetLevel">The target proficiency level.</param>
    /// <param name="currencyCost">The cost in Pieces Silver.</param>
    /// <param name="trainingWeeks">The training duration in weeks.</param>
    /// <param name="minimumCharacterLevel">The minimum character level required.</param>
    /// <param name="requiresNpcTrainer">Whether an NPC trainer is required.</param>
    /// <returns>A new <see cref="ArmorTrainingRequirement"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="targetLevel"/> is <see cref="ArmorProficiencyLevel.NonProficient"/>
    /// or when costs/levels are negative.
    /// </exception>
    public static ArmorTrainingRequirement Create(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel targetLevel,
        int currencyCost,
        int trainingWeeks,
        int minimumCharacterLevel = 1,
        bool requiresNpcTrainer = true)
    {
        // Validate target level - cannot train TO NonProficient
        if (targetLevel == ArmorProficiencyLevel.NonProficient)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetLevel),
                "Cannot train to NonProficient level.");
        }

        // Validate costs are non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(currencyCost);
        ArgumentOutOfRangeException.ThrowIfNegative(trainingWeeks);
        ArgumentOutOfRangeException.ThrowIfLessThan(minimumCharacterLevel, 1);

        // Determine the required level (one below target)
        var requiredLevel = targetLevel switch
        {
            ArmorProficiencyLevel.Proficient => ArmorProficiencyLevel.NonProficient,
            ArmorProficiencyLevel.Expert => ArmorProficiencyLevel.Proficient,
            ArmorProficiencyLevel.Master => ArmorProficiencyLevel.Expert,
            _ => throw new ArgumentOutOfRangeException(
                nameof(targetLevel),
                $"Unsupported target level: {targetLevel}")
        };

        return new ArmorTrainingRequirement
        {
            ArmorCategory = armorCategory,
            TargetLevel = targetLevel,
            RequiredLevel = requiredLevel,
            CurrencyCost = currencyCost,
            TrainingWeeks = trainingWeeks,
            MinimumCharacterLevel = minimumCharacterLevel,
            RequiresNpcTrainer = requiresNpcTrainer
        };
    }

    /// <summary>
    /// Creates a requirement for training to Proficient level.
    /// </summary>
    /// <param name="armorCategory">The armor category to train.</param>
    /// <param name="currencyCost">The cost in Pieces Silver (default: 50).</param>
    /// <param name="trainingWeeks">The training duration in weeks (default: 2).</param>
    /// <returns>A requirement for NonProficient → Proficient training.</returns>
    /// <remarks>
    /// Standard training costs:
    /// <list type="bullet">
    ///   <item><description>50 Pieces Silver</description></item>
    ///   <item><description>2 weeks training</description></item>
    ///   <item><description>Minimum Level 1</description></item>
    /// </list>
    /// </remarks>
    public static ArmorTrainingRequirement ForProficient(
        ArmorCategory armorCategory,
        int currencyCost = 50,
        int trainingWeeks = 2)
    {
        return Create(
            armorCategory,
            ArmorProficiencyLevel.Proficient,
            currencyCost,
            trainingWeeks,
            minimumCharacterLevel: 1,
            requiresNpcTrainer: true);
    }

    /// <summary>
    /// Creates a requirement for training to Expert level.
    /// </summary>
    /// <param name="armorCategory">The armor category to train.</param>
    /// <param name="currencyCost">The cost in Pieces Silver (default: 200).</param>
    /// <param name="trainingWeeks">The training duration in weeks (default: 4).</param>
    /// <returns>A requirement for Proficient → Expert training.</returns>
    /// <remarks>
    /// Standard training costs:
    /// <list type="bullet">
    ///   <item><description>200 Pieces Silver</description></item>
    ///   <item><description>4 weeks training</description></item>
    ///   <item><description>Minimum Level 5</description></item>
    /// </list>
    /// </remarks>
    public static ArmorTrainingRequirement ForExpert(
        ArmorCategory armorCategory,
        int currencyCost = 200,
        int trainingWeeks = 4)
    {
        return Create(
            armorCategory,
            ArmorProficiencyLevel.Expert,
            currencyCost,
            trainingWeeks,
            minimumCharacterLevel: 5,
            requiresNpcTrainer: true);
    }

    /// <summary>
    /// Creates a requirement for training to Master level.
    /// </summary>
    /// <param name="armorCategory">The armor category to train.</param>
    /// <param name="currencyCost">The cost in Pieces Silver (default: 500).</param>
    /// <param name="trainingWeeks">The training duration in weeks (default: 8).</param>
    /// <returns>A requirement for Expert → Master training.</returns>
    /// <remarks>
    /// Standard training costs:
    /// <list type="bullet">
    ///   <item><description>500 Pieces Silver</description></item>
    ///   <item><description>8 weeks training</description></item>
    ///   <item><description>Minimum Level 10</description></item>
    /// </list>
    /// </remarks>
    public static ArmorTrainingRequirement ForMaster(
        ArmorCategory armorCategory,
        int currencyCost = 500,
        int trainingWeeks = 8)
    {
        return Create(
            armorCategory,
            ArmorProficiencyLevel.Master,
            currencyCost,
            trainingWeeks,
            minimumCharacterLevel: 10,
            requiresNpcTrainer: true);
    }

    /// <summary>
    /// Creates an empty requirement with no costs (for free training).
    /// </summary>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="targetLevel">The target proficiency level.</param>
    /// <returns>A requirement with zero costs.</returns>
    /// <remarks>
    /// Used for archetype-granted proficiencies or special quest rewards.
    /// </remarks>
    public static ArmorTrainingRequirement Free(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel targetLevel)
    {
        return Create(
            armorCategory,
            targetLevel,
            currencyCost: 0,
            trainingWeeks: 0,
            minimumCharacterLevel: 1,
            requiresNpcTrainer: false);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the requirement as a human-readable description.
    /// </summary>
    /// <returns>A formatted description of all requirements.</returns>
    /// <example>
    /// "Heavy Armor to Proficient: 50 PS, 2 weeks, Level 1+"
    /// </example>
    public string FormatDescription()
    {
        var parts = new List<string>();

        if (CurrencyCost > 0)
        {
            parts.Add($"{CurrencyCost} PS");
        }

        if (TrainingWeeks > 0)
        {
            parts.Add($"{TrainingWeeks} week{(TrainingWeeks != 1 ? "s" : "")}");
        }

        if (MinimumCharacterLevel > 1)
        {
            parts.Add($"Level {MinimumCharacterLevel}+");
        }

        var requirements = parts.Count > 0
            ? string.Join(", ", parts)
            : "Free";

        return $"{ArmorCategory} to {TargetLevel}: {requirements}";
    }

    /// <summary>
    /// Formats short cost summary for UI display.
    /// </summary>
    /// <returns>A short cost string.</returns>
    /// <example>"50 PS / 2wk"</example>
    public string FormatCostSummary()
    {
        if (!HasCost)
        {
            return "Free";
        }

        var parts = new List<string>();

        if (CurrencyCost > 0)
        {
            parts.Add($"{CurrencyCost} PS");
        }

        if (TrainingWeeks > 0)
        {
            parts.Add($"{TrainingWeeks}wk");
        }

        return string.Join(" / ", parts);
    }

    /// <summary>
    /// Returns a debug-friendly string representation.
    /// </summary>
    public override string ToString() =>
        $"ArmorTrainingRequirement({TransitionDescription}, {FormatCostSummary()})";
}
