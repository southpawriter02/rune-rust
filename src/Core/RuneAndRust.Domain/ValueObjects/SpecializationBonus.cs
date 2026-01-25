namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a bonus granted by a Wasteland Survival specialization ability.
/// </summary>
/// <remarks>
/// <para>
/// Specialization bonuses are applied to skill checks when their trigger conditions
/// are met. Bonuses can add dice to the pool, grant advantage, or provide special
/// effects that modify the check in other ways.
/// </para>
/// <para>
/// <b>Bonus Types:</b>
/// <list type="bullet">
///   <item><description><see cref="SpecializationBonusType.DicePool"/>: Adds extra d10s to the skill check</description></item>
///   <item><description><see cref="SpecializationBonusType.Advantage"/>: Grants advantage (roll twice, keep best)</description></item>
///   <item><description><see cref="SpecializationBonusType.SpecialEffect"/>: Provides a unique effect defined in AbilityActivation</description></item>
/// </list>
/// </para>
/// <para>
/// Multiple bonuses can apply to the same check if their conditions are all met.
/// Dice bonuses stack additively, while advantage does not stack with itself.
/// </para>
/// </remarks>
/// <param name="AbilityId">The unique identifier of the ability granting this bonus.</param>
/// <param name="BonusDice">The number of extra d10s to add (0 if not a dice bonus).</param>
/// <param name="Type">The type of bonus being applied.</param>
/// <param name="Description">A human-readable description of the bonus.</param>
/// <seealso cref="RuneAndRust.Domain.Enums.WastelandSurvivalSpecializationType"/>
/// <seealso cref="AbilityActivation"/>
public readonly record struct SpecializationBonus(
    string AbilityId,
    int BonusDice,
    SpecializationBonusType Type,
    string Description)
{
    // =========================================================================
    // COMPUTED PROPERTIES
    // =========================================================================

    /// <summary>
    /// Gets a value indicating whether this bonus adds dice to the pool.
    /// </summary>
    /// <value>True if <see cref="BonusDice"/> is greater than zero.</value>
    public bool AddsDice => BonusDice > 0;

    /// <summary>
    /// Gets a value indicating whether this bonus grants advantage.
    /// </summary>
    /// <value>True if <see cref="Type"/> is <see cref="SpecializationBonusType.Advantage"/>.</value>
    public bool GrantsAdvantage => Type == SpecializationBonusType.Advantage;

    /// <summary>
    /// Gets a value indicating whether this bonus provides a special effect.
    /// </summary>
    /// <value>True if <see cref="Type"/> is <see cref="SpecializationBonusType.SpecialEffect"/>.</value>
    public bool HasSpecialEffect => Type == SpecializationBonusType.SpecialEffect;

    /// <summary>
    /// Gets a value indicating whether this is a valid, non-empty bonus.
    /// </summary>
    /// <value>True if the ability ID is not null or whitespace.</value>
    public bool IsValid => !string.IsNullOrWhiteSpace(AbilityId);

    // =========================================================================
    // FACTORY METHODS
    // =========================================================================

    /// <summary>
    /// Creates a dice pool bonus that adds extra d10s to a skill check.
    /// </summary>
    /// <param name="abilityId">The unique identifier of the ability.</param>
    /// <param name="bonusDice">The number of extra d10s to add.</param>
    /// <param name="description">A human-readable description of the bonus.</param>
    /// <returns>A new <see cref="SpecializationBonus"/> with dice pool type.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="abilityId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="bonusDice"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// // Beast Tracker grants +2d10 when tracking living creatures
    /// var beastTracker = SpecializationBonus.DiceBonus(
    ///     "beast-tracker",
    ///     2,
    ///     "+2d10 to tracking checks against living creatures");
    /// </code>
    /// </example>
    public static SpecializationBonus DiceBonus(string abilityId, int bonusDice, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId, nameof(abilityId));
        ArgumentOutOfRangeException.ThrowIfLessThan(bonusDice, 1, nameof(bonusDice));

        return new SpecializationBonus(
            AbilityId: abilityId,
            BonusDice: bonusDice,
            Type: SpecializationBonusType.DicePool,
            Description: description ?? string.Empty);
    }

    /// <summary>
    /// Creates an advantage bonus that grants advantage on a skill check.
    /// </summary>
    /// <param name="abilityId">The unique identifier of the ability.</param>
    /// <param name="description">A human-readable description of the bonus.</param>
    /// <returns>A new <see cref="SpecializationBonus"/> with advantage type.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="abilityId"/> is null, empty, or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Toxin Resistance grants advantage on poison saves
    /// var toxinResistance = SpecializationBonus.Advantage(
    ///     "toxin-resistance",
    ///     "Advantage on saves against poison gas hazards");
    /// </code>
    /// </example>
    public static SpecializationBonus Advantage(string abilityId, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId, nameof(abilityId));

        return new SpecializationBonus(
            AbilityId: abilityId,
            BonusDice: 0,
            Type: SpecializationBonusType.Advantage,
            Description: description ?? string.Empty);
    }

    /// <summary>
    /// Creates a special effect bonus that provides a unique effect.
    /// </summary>
    /// <param name="abilityId">The unique identifier of the ability.</param>
    /// <param name="description">A human-readable description of the effect.</param>
    /// <returns>A new <see cref="SpecializationBonus"/> with special effect type.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="abilityId"/> is null, empty, or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Predator's Eye reveals creature weakness after successful tracking
    /// var predatorsEye = SpecializationBonus.SpecialEffect(
    ///     "predators-eye",
    ///     "Reveal creature weakness and behavior patterns");
    /// </code>
    /// </example>
    public static SpecializationBonus SpecialEffect(string abilityId, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId, nameof(abilityId));

        return new SpecializationBonus(
            AbilityId: abilityId,
            BonusDice: 0,
            Type: SpecializationBonusType.SpecialEffect,
            Description: description ?? string.Empty);
    }

    /// <summary>
    /// Creates an empty, invalid bonus.
    /// </summary>
    /// <returns>A new empty <see cref="SpecializationBonus"/>.</returns>
    /// <remarks>
    /// Used as a default return value when no bonus applies.
    /// Check <see cref="IsValid"/> before using the bonus.
    /// </remarks>
    public static SpecializationBonus Empty() => new(
        AbilityId: string.Empty,
        BonusDice: 0,
        Type: SpecializationBonusType.DicePool,
        Description: string.Empty);

    // =========================================================================
    // DISPLAY METHODS
    // =========================================================================

    /// <summary>
    /// Returns a formatted display string for this bonus.
    /// </summary>
    /// <returns>A string suitable for display in the UI.</returns>
    /// <example>
    /// <code>
    /// var bonus = SpecializationBonus.DiceBonus("beast-tracker", 2, "Beast Tracker");
    /// Console.WriteLine(bonus.ToDisplayString());
    /// // Output: "+2d10 (Beast Tracker)"
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        if (!IsValid)
        {
            return "No bonus";
        }

        return Type switch
        {
            SpecializationBonusType.DicePool => $"+{BonusDice}d10 ({Description})",
            SpecializationBonusType.Advantage => $"Advantage ({Description})",
            SpecializationBonusType.SpecialEffect => Description,
            _ => Description
        };
    }

    /// <summary>
    /// Returns a string representation of this bonus.
    /// </summary>
    /// <returns>A detailed string representation.</returns>
    public override string ToString() =>
        $"SpecializationBonus {{ AbilityId = {AbilityId}, Type = {Type}, " +
        $"BonusDice = {BonusDice}, Description = {Description} }}";
}

/// <summary>
/// Types of bonuses that can be applied by specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// Each bonus type has different stacking rules:
/// <list type="bullet">
///   <item><description><see cref="DicePool"/>: Stacks additively with other dice bonuses</description></item>
///   <item><description><see cref="Advantage"/>: Does not stack (multiple advantages = one advantage)</description></item>
///   <item><description><see cref="SpecialEffect"/>: Effects are evaluated individually by the service</description></item>
/// </list>
/// </para>
/// </remarks>
public enum SpecializationBonusType
{
    /// <summary>
    /// Adds extra dice to the skill check pool.
    /// </summary>
    /// <remarks>
    /// The number of dice is specified in <see cref="SpecializationBonus.BonusDice"/>.
    /// Multiple dice bonuses stack additively.
    /// </remarks>
    DicePool = 0,

    /// <summary>
    /// Grants advantage on the skill check (roll twice, keep best).
    /// </summary>
    /// <remarks>
    /// Advantage does not stack with itself. Having multiple sources of
    /// advantage still only grants one additional roll.
    /// </remarks>
    Advantage = 1,

    /// <summary>
    /// Provides a special effect that doesn't fit the other categories.
    /// </summary>
    /// <remarks>
    /// Special effects are typically handled by <see cref="AbilityActivation"/>
    /// and may include revealing information, marking areas, or other unique effects.
    /// </remarks>
    SpecialEffect = 2
}
