namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a specialization ability activation.
/// </summary>
/// <remarks>
/// <para>
/// AbilityActivationResult captures whether the ability was successfully activated,
/// what effects were applied, and any narrative output.
/// </para>
/// <para>
/// <b>v0.15.2g:</b> Initial implementation of ability activation results.
/// </para>
/// </remarks>
/// <param name="Success">Whether the ability was successfully activated.</param>
/// <param name="AbilityId">The ability that was activated.</param>
/// <param name="CharacterId">The character who activated it.</param>
/// <param name="EffectDescription">Description of the effect applied.</param>
/// <param name="NarrativeText">Narrative text for display.</param>
/// <param name="Modifier">Numeric modifier applied (e.g., stage reduction, distance bonus).</param>
/// <param name="DiceBonus">Dice bonus applied (e.g., +1d10 or +2d10).</param>
/// <param name="UsesRemaining">Uses remaining after activation, or -1 for unlimited.</param>
/// <param name="FailureReason">Reason for failure if Success is false.</param>
public readonly record struct AbilityActivationResult(
    bool Success,
    string AbilityId,
    string CharacterId,
    string EffectDescription,
    string NarrativeText,
    int Modifier = 0,
    int DiceBonus = 0,
    int UsesRemaining = -1,
    string? FailureReason = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Indicates whether a modifier was applied.
    /// </summary>
    public bool HasModifier => Modifier != 0;

    /// <summary>
    /// Indicates whether a dice bonus was applied.
    /// </summary>
    public bool HasDiceBonus => DiceBonus > 0;

    /// <summary>
    /// Indicates whether uses are limited.
    /// </summary>
    public bool HasLimitedUses => UsesRemaining >= 0;

    /// <summary>
    /// Indicates whether no uses remain.
    /// </summary>
    public bool Exhausted => UsesRemaining == 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS - SUCCESS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful result for a stage reduction ability.
    /// </summary>
    public static AbilityActivationResult StageReduction(
        string characterId, string abilityId, int reduction, int originalStages, int newStages)
    {
        return new AbilityActivationResult(
            Success: true,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: $"Climbing stages reduced from {originalStages} to {newStages}",
            NarrativeText: "Your expertise finds an efficient route up.",
            Modifier: -reduction);
    }

    /// <summary>
    /// Creates a successful result for a distance bonus ability.
    /// </summary>
    public static AbilityActivationResult DistanceBonus(
        string characterId, string abilityId, int bonusFeet)
    {
        return new AbilityActivationResult(
            Success: true,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: $"Maximum leap distance increased by {bonusFeet} ft",
            NarrativeText: "You push beyond normal limits.",
            Modifier: bonusFeet);
    }

    /// <summary>
    /// Creates a successful result for an auto-success ability.
    /// </summary>
    public static AbilityActivationResult AutoSuccess(
        string characterId, string abilityId, string effect, string narrative)
    {
        return new AbilityActivationResult(
            Success: true,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: effect,
            NarrativeText: narrative);
    }

    /// <summary>
    /// Creates a successful result for entering [Hidden] without action.
    /// </summary>
    public static AbilityActivationResult HiddenEntry(
        string characterId, string abilityId, string narrative)
    {
        return new AbilityActivationResult(
            Success: true,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: "Entered [Hidden] without using an action",
            NarrativeText: narrative);
    }

    /// <summary>
    /// Creates a successful result for maintaining [Hidden] after attack.
    /// </summary>
    public static AbilityActivationResult MaintainedHidden(
        string characterId, string abilityId, int usesRemaining)
    {
        return new AbilityActivationResult(
            Success: true,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: "Remained [Hidden] after attacking",
            NarrativeText: "You strike from the shadows and vanish once more.",
            UsesRemaining: usesRemaining);
    }

    /// <summary>
    /// Creates a successful result for a dice bonus ability.
    /// </summary>
    public static AbilityActivationResult PartyDiceBonus(
        string characterId, string abilityId, int diceBonus, int partyMembersAffected)
    {
        return new AbilityActivationResult(
            Success: true,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: $"+{diceBonus}d10 to {partyMembersAffected} party members' Passive Stealth",
            NarrativeText: "You extend your shroud of shadows to your companions.",
            DiceBonus: diceBonus);
    }

    /// <summary>
    /// Creates a successful result for a reroll ability.
    /// </summary>
    public static AbilityActivationResult Reroll(
        string characterId, string abilityId, int diceBonus, int usesRemaining)
    {
        return new AbilityActivationResult(
            Success: true,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: $"Reroll granted with +{diceBonus}d10",
            NarrativeText: "You twist mid-air, defying gravity for a precious moment.",
            DiceBonus: diceBonus,
            UsesRemaining: usesRemaining);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS - FAILURE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a failed result due to ability not available.
    /// </summary>
    public static AbilityActivationResult NotAvailable(string characterId, string abilityId)
    {
        return new AbilityActivationResult(
            Success: false,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: "Ability not available",
            NarrativeText: string.Empty,
            FailureReason: "Character does not have this ability");
    }

    /// <summary>
    /// Creates a failed result due to no uses remaining.
    /// </summary>
    public static AbilityActivationResult NoUsesRemaining(string characterId, string abilityId)
    {
        return new AbilityActivationResult(
            Success: false,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: "No uses remaining",
            NarrativeText: string.Empty,
            UsesRemaining: 0,
            FailureReason: "Ability has been used maximum times this day/encounter");
    }

    /// <summary>
    /// Creates a failed result due to conditions not met.
    /// </summary>
    public static AbilityActivationResult ConditionsNotMet(
        string characterId, string abilityId, string reason)
    {
        return new AbilityActivationResult(
            Success: false,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: "Conditions not met",
            NarrativeText: string.Empty,
            FailureReason: reason);
    }

    /// <summary>
    /// Creates a failed result due to DC too high (for auto-success abilities).
    /// </summary>
    public static AbilityActivationResult DcTooHigh(string characterId, string abilityId, int dc, int threshold)
    {
        return new AbilityActivationResult(
            Success: false,
            AbilityId: abilityId,
            CharacterId: characterId,
            EffectDescription: $"DC {dc} exceeds auto-success threshold of {threshold}",
            NarrativeText: string.Empty,
            FailureReason: $"Ability only works for DC ≤ {threshold}");
    }
}
