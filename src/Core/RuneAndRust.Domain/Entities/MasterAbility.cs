namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a master-rank (Rank 5) ability that provides powerful bonuses to skilled characters.
/// </summary>
/// <remarks>
/// <para>
/// Master abilities are the pinnacle of skill mastery. Each of the four core skills
/// (Acrobatics, Rhetoric, System Bypass, Wasteland Survival) has 4-5 master abilities
/// that characters unlock at Rank 5 proficiency.
/// </para>
/// <para>
/// Abilities can be passive (always active) or active (require explicit invocation).
/// Most abilities in v0.15.1c are passive, with active abilities planned for future versions.
/// </para>
/// </remarks>
public sealed class MasterAbility
{
    /// <summary>
    /// Unique identifier for this master ability.
    /// </summary>
    /// <remarks>
    /// Uses kebab-case format matching the configuration file.
    /// Example: "spider-climb", "fearsome-reputation".
    /// </remarks>
    public string AbilityId { get; private set; } = string.Empty;

    /// <summary>
    /// The skill this ability is associated with.
    /// </summary>
    /// <remarks>
    /// Must match a valid skill ID from the skill definitions.
    /// Example: "acrobatics", "rhetoric", "system-bypass", "wasteland-survival".
    /// </remarks>
    public string SkillId { get; private set; } = string.Empty;

    /// <summary>
    /// Display name of the ability.
    /// </summary>
    /// <example>"Spider Climb", "Fearsome Reputation"</example>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Full description of the ability's effect.
    /// </summary>
    /// <example>"Automatically succeed on climbing checks with DC 12 or lower. You climb at full speed."</example>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// The type of effect this ability provides.
    /// </summary>
    /// <seealso cref="MasterAbilityType"/>
    public MasterAbilityType AbilityType { get; private set; }

    /// <summary>
    /// The effect parameters for this ability.
    /// </summary>
    /// <remarks>
    /// Contains type-specific values like <c>AutoSucceedDc</c>, <c>DiceBonus</c>, etc.
    /// </remarks>
    public MasterAbilityEffect Effect { get; private set; } = MasterAbilityEffect.Empty;

    /// <summary>
    /// Skill subtypes this ability applies to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If empty, the ability applies to all checks for the associated skill.
    /// If specified, the ability only applies when the check's subtype matches.
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    ///   <item><description>Spider Climb: ["climbing"]</description></item>
    ///   <item><description>Ghost Walk: ["stealth"]</description></item>
    ///   <item><description>Fearsome Reputation: ["intimidation"]</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> SubTypes { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Whether this ability is always active (passive) or requires activation (active).
    /// </summary>
    /// <remarks>
    /// Passive abilities are automatically evaluated during skill checks.
    /// Active abilities require explicit player invocation (future feature).
    /// </remarks>
    public bool IsPassive { get; private set; } = true;

    /// <summary>
    /// Optional flavor text for when the ability triggers.
    /// </summary>
    /// <example>"Your mastery allows you to scale the surface effortlessly."</example>
    public string? TriggerMessage { get; private set; }

    // Private constructor for deserialization
    private MasterAbility() { }

    /// <summary>
    /// Creates a new master ability.
    /// </summary>
    /// <param name="abilityId">Unique identifier for the ability.</param>
    /// <param name="skillId">Associated skill ID.</param>
    /// <param name="name">Display name.</param>
    /// <param name="description">Full description.</param>
    /// <param name="abilityType">Type of effect.</param>
    /// <param name="effect">Effect parameters.</param>
    /// <param name="subTypes">Optional skill subtypes.</param>
    /// <param name="isPassive">Whether passive (default: true).</param>
    /// <param name="triggerMessage">Optional trigger flavor text.</param>
    /// <returns>A new <see cref="MasterAbility"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="abilityId"/>, <paramref name="skillId"/>,
    /// or <paramref name="name"/> is null or whitespace.
    /// </exception>
    public static MasterAbility Create(
        string abilityId,
        string skillId,
        string name,
        string description,
        MasterAbilityType abilityType,
        MasterAbilityEffect effect,
        IReadOnlyList<string>? subTypes = null,
        bool isPassive = true,
        string? triggerMessage = null)
    {
        if (string.IsNullOrWhiteSpace(abilityId))
            throw new ArgumentException("Ability ID is required.", nameof(abilityId));
        if (string.IsNullOrWhiteSpace(skillId))
            throw new ArgumentException("Skill ID is required.", nameof(skillId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new MasterAbility
        {
            AbilityId = abilityId,
            SkillId = skillId,
            Name = name,
            Description = description ?? string.Empty,
            AbilityType = abilityType,
            Effect = effect,
            SubTypes = subTypes ?? Array.Empty<string>(),
            IsPassive = isPassive,
            TriggerMessage = triggerMessage
        };
    }

    /// <summary>
    /// Checks if this ability applies to a specific skill check subtype.
    /// </summary>
    /// <param name="subType">The subtype to check, or null for general checks.</param>
    /// <returns>
    /// <c>true</c> if the ability has no subtype restrictions, or if the specified
    /// subtype is in the ability's <see cref="SubTypes"/> list; otherwise <c>false</c>.
    /// </returns>
    public bool AppliesToSubType(string? subType)
    {
        // No restrictions means applies to all
        if (SubTypes.Count == 0)
            return true;

        // Must match one of the specified subtypes
        if (string.IsNullOrWhiteSpace(subType))
            return false;

        return SubTypes.Contains(subType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if this ability would auto-succeed a check at the given DC.
    /// </summary>
    /// <param name="difficultyClass">The DC of the check.</param>
    /// <returns>
    /// <c>true</c> if this is an <see cref="MasterAbilityType.AutoSucceed"/> ability
    /// and the DC is at or below the threshold; otherwise <c>false</c>.
    /// </returns>
    public bool WouldAutoSucceed(int difficultyClass)
    {
        if (AbilityType != MasterAbilityType.AutoSucceed)
            return false;

        return Effect.AutoSucceedDc.HasValue && difficultyClass <= Effect.AutoSucceedDc.Value;
    }

    /// <summary>
    /// Gets the dice bonus provided by this ability, if any.
    /// </summary>
    /// <returns>
    /// The dice bonus for <see cref="MasterAbilityType.DiceBonus"/> abilities,
    /// or 0 if not applicable.
    /// </returns>
    public int GetDiceBonus()
    {
        if (AbilityType != MasterAbilityType.DiceBonus)
            return 0;

        return Effect.DiceBonus ?? 0;
    }

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    public override string ToString() => $"[{AbilityId}] {Name} ({AbilityType})";
}
