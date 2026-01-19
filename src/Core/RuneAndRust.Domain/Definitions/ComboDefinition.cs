using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines an ability combo with steps, timing window, and bonus effects.
/// </summary>
/// <remarks>
/// <para>ComboDefinition is an immutable domain entity that describes a sequence of abilities
/// that, when executed within a time window, grant bonus effects to the player.</para>
/// <para>Combos are loaded from JSON configuration and matched against player actions during combat.</para>
/// <para>Key characteristics:</para>
/// <list type="bullet">
///   <item><description><see cref="ComboId"/> - Unique identifier for configuration and tracking</description></item>
///   <item><description><see cref="Steps"/> - Ordered sequence of abilities (minimum 2)</description></item>
///   <item><description><see cref="WindowTurns"/> - Number of turns to complete the combo</description></item>
///   <item><description><see cref="RequiredClassIds"/> - Class restrictions (empty = all classes)</description></item>
///   <item><description><see cref="BonusEffects"/> - Rewards granted on completion</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Create a simple two-step combo
/// var combo = ComboDefinition.Create(
///     comboId: "quick-strike",
///     name: "Quick Strike",
///     description: "A fast two-hit combo",
///     windowTurns: 2,
///     steps: new[]
///     {
///         new ComboStep { StepNumber = 1, AbilityId = "jab" },
///         new ComboStep { StepNumber = 2, AbilityId = "cross" }
///     },
///     bonusEffects: new[]
///     {
///         new ComboBonusEffect
///         {
///             EffectType = ComboBonusType.ExtraDamage,
///             Value = "2d6",
///             DamageType = "physical"
///         }
///     }
/// );
/// </code>
/// </example>
public class ComboDefinition : IEntity
{
    // ===== Properties =====

    /// <summary>
    /// Gets the unique identifier for this combo definition instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique string identifier for this combo (e.g., "elemental-burst", "warriors-onslaught").
    /// </summary>
    /// <remarks>
    /// <para>Used for configuration loading and combo tracking.</para>
    /// <para>Always stored in lowercase.</para>
    /// </remarks>
    public string ComboId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the combo (e.g., "Elemental Burst").
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the detailed description of the combo's theme and usage.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the number of turns allowed to complete the combo.
    /// </summary>
    /// <remarks>
    /// <para>If the player doesn't complete all steps within this window, the combo resets.</para>
    /// <para>Typical values: 2-3 turns for most combos.</para>
    /// </remarks>
    public int WindowTurns { get; private set; }

    /// <summary>
    /// Gets the class IDs that can use this combo.
    /// </summary>
    /// <remarks>
    /// <para>If empty, the combo is available to all classes.</para>
    /// <para>Class IDs are stored in lowercase.</para>
    /// </remarks>
    public IReadOnlyList<string> RequiredClassIds { get; private set; } = [];

    /// <summary>
    /// Gets the ordered sequence of steps in the combo.
    /// </summary>
    /// <remarks>
    /// <para>Steps are ordered by <see cref="ComboStep.StepNumber"/>.</para>
    /// <para>A combo must have at least 2 steps.</para>
    /// </remarks>
    public IReadOnlyList<ComboStep> Steps { get; private set; } = [];

    /// <summary>
    /// Gets the bonus effects applied when the combo completes.
    /// </summary>
    /// <remarks>
    /// <para>Multiple bonus effects can be applied from a single combo.</para>
    /// <para>Effects are applied in order.</para>
    /// </remarks>
    public IReadOnlyList<ComboBonusEffect> BonusEffects { get; private set; } = [];

    /// <summary>
    /// Gets the path to the combo's icon asset.
    /// </summary>
    /// <remarks>
    /// <para>Used for UI display. May be null if no custom icon is defined.</para>
    /// </remarks>
    public string? IconPath { get; private set; }

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core and serialization.
    /// </summary>
    private ComboDefinition() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a new combo definition with the specified properties.
    /// </summary>
    /// <param name="comboId">Unique string identifier for the combo (will be lowercased).</param>
    /// <param name="name">Display name of the combo.</param>
    /// <param name="description">Detailed description of the combo.</param>
    /// <param name="windowTurns">Number of turns to complete the combo (must be positive).</param>
    /// <param name="steps">Ordered sequence of steps (minimum 2).</param>
    /// <param name="bonusEffects">Optional bonus effects applied on completion.</param>
    /// <param name="requiredClassIds">Optional class restrictions (empty = all classes).</param>
    /// <param name="iconPath">Optional path to the combo icon.</param>
    /// <returns>A new ComboDefinition instance.</returns>
    /// <exception cref="ArgumentException">Thrown when comboId or name is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when windowTurns is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when steps has fewer than 2 elements.</exception>
    public static ComboDefinition Create(
        string comboId,
        string name,
        string description,
        int windowTurns,
        IEnumerable<ComboStep> steps,
        IEnumerable<ComboBonusEffect>? bonusEffects = null,
        IEnumerable<string>? requiredClassIds = null,
        string? iconPath = null)
    {
        // Validate required parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(comboId, nameof(comboId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowTurns, nameof(windowTurns));

        // Order steps by step number
        var stepList = steps.OrderBy(s => s.StepNumber).ToList();

        // Validate minimum step count
        if (stepList.Count < 2)
        {
            throw new ArgumentException("Combo must have at least 2 steps", nameof(steps));
        }

        return new ComboDefinition
        {
            Id = Guid.NewGuid(),
            ComboId = comboId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            WindowTurns = windowTurns,
            Steps = stepList,
            BonusEffects = bonusEffects?.ToList() ?? [],
            RequiredClassIds = requiredClassIds?.Select(c => c.ToLowerInvariant()).ToList() ?? [],
            IconPath = iconPath
        };
    }

    // ===== Computed Properties =====

    /// <summary>
    /// Gets the total number of steps in the combo.
    /// </summary>
    public int StepCount => Steps.Count;

    /// <summary>
    /// Gets the ability ID for the first step of the combo.
    /// </summary>
    /// <remarks>
    /// <para>Used for indexing combos by their starting ability.</para>
    /// <para>Returns empty string if no steps exist (should not happen in valid combos).</para>
    /// </remarks>
    public string FirstAbilityId => Steps.FirstOrDefault()?.AbilityId ?? string.Empty;

    // ===== Query Methods =====

    /// <summary>
    /// Gets the ability ID for a specific step (1-indexed).
    /// </summary>
    /// <param name="stepNumber">The step number (1-indexed).</param>
    /// <returns>The ability ID for the step, or null if not found.</returns>
    public string? GetAbilityForStep(int stepNumber)
    {
        return Steps.FirstOrDefault(s => s.StepNumber == stepNumber)?.AbilityId;
    }

    /// <summary>
    /// Gets the step at a specific position (1-indexed).
    /// </summary>
    /// <param name="stepNumber">The step number (1-indexed).</param>
    /// <returns>The ComboStep, or null if not found.</returns>
    public ComboStep? GetStep(int stepNumber)
    {
        return Steps.FirstOrDefault(s => s.StepNumber == stepNumber);
    }

    /// <summary>
    /// Checks if an ability is part of this combo.
    /// </summary>
    /// <param name="abilityId">The ability identifier to check.</param>
    /// <returns>True if the ability is used in any step of the combo; otherwise, false.</returns>
    public bool ContainsAbility(string abilityId)
    {
        return Steps.Any(s => s.AbilityId.Equals(abilityId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a class can use this combo.
    /// </summary>
    /// <param name="classId">The class identifier to check.</param>
    /// <returns>True if the class can use this combo; otherwise, false.</returns>
    /// <remarks>
    /// <para>If <see cref="RequiredClassIds"/> is empty, the combo is available to all classes.</para>
    /// <para>Comparison is case-insensitive.</para>
    /// </remarks>
    public bool IsAvailableForClass(string classId)
    {
        // No restriction = available to all classes
        if (!RequiredClassIds.Any())
        {
            return true;
        }

        return RequiredClassIds.Any(c => c.Equals(classId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all unique ability IDs used in this combo.
    /// </summary>
    /// <returns>A list of distinct ability IDs.</returns>
    public IReadOnlyList<string> GetAllAbilityIds()
    {
        return Steps.Select(s => s.AbilityId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Gets a summary of all bonus effects for display.
    /// </summary>
    /// <returns>A list of effect descriptions.</returns>
    public IReadOnlyList<string> GetBonusDescriptions()
    {
        return BonusEffects.Select(e => e.GetDescription()).ToList();
    }

    /// <summary>
    /// Checks if the combo has any bonus effects.
    /// </summary>
    /// <returns>True if the combo has at least one bonus effect; otherwise, false.</returns>
    public bool HasBonusEffects() => BonusEffects.Any();

    // ===== Object Overrides =====

    /// <summary>
    /// Returns a string representation of this combo definition.
    /// </summary>
    /// <returns>A string showing the name, ID, and step count.</returns>
    public override string ToString() => $"{Name} ({ComboId}): {StepCount} steps";
}
