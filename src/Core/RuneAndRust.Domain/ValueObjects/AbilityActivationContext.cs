namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the context for activating a specialization ability.
/// </summary>
/// <remarks>
/// <para>
/// AbilityActivationContext captures the situation in which an ability is being
/// activated, including trigger conditions, environmental factors, and targets.
/// </para>
/// <para>
/// <b>v0.15.2g:</b> Initial implementation of ability activation context.
/// </para>
/// </remarks>
/// <param name="CharacterId">The character activating the ability.</param>
/// <param name="AbilityId">The ability being activated.</param>
/// <param name="TriggerType">What triggered the activation.</param>
/// <param name="CurrentHeight">Current height for climbing/falling abilities.</param>
/// <param name="LeapDistance">Leap distance for leaping abilities.</param>
/// <param name="IsInShadows">Whether character is in shadows (stealth).</param>
/// <param name="IsInPsychicResonance">Whether in [Psychic Resonance] zone.</param>
/// <param name="PartyMemberIds">Party member IDs for party-wide abilities.</param>
/// <param name="TargetIds">Target IDs for targeted abilities.</param>
public readonly record struct AbilityActivationContext(
    string CharacterId,
    string AbilityId,
    AbilityTriggerType TriggerType,
    int CurrentHeight = 0,
    int LeapDistance = 0,
    bool IsInShadows = false,
    bool IsInPsychicResonance = false,
    IReadOnlyList<string>? PartyMemberIds = null,
    IReadOnlyList<string>? TargetIds = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the number of party members in range.
    /// </summary>
    public int PartyMembersInRange => PartyMemberIds?.Count ?? 0;

    /// <summary>
    /// Gets whether this context involves party members.
    /// </summary>
    public bool HasPartyTargets => PartyMemberIds is { Count: > 0 };

    /// <summary>
    /// Gets whether this context involves specific targets.
    /// </summary>
    public bool HasTargets => TargetIds is { Count: > 0 };

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates context for a climbing ability activation.
    /// </summary>
    public static AbilityActivationContext ForClimbing(string characterId, string abilityId, int height)
    {
        return new AbilityActivationContext(
            characterId, abilityId,
            AbilityTriggerType.Climbing,
            CurrentHeight: height);
    }

    /// <summary>
    /// Creates context for a leaping ability activation.
    /// </summary>
    public static AbilityActivationContext ForLeaping(string characterId, string abilityId, int distance)
    {
        return new AbilityActivationContext(
            characterId, abilityId,
            AbilityTriggerType.Leaping,
            LeapDistance: distance);
    }

    /// <summary>
    /// Creates context for a falling ability activation.
    /// </summary>
    public static AbilityActivationContext ForFalling(string characterId, string abilityId, int height)
    {
        return new AbilityActivationContext(
            characterId, abilityId,
            AbilityTriggerType.Falling,
            CurrentHeight: height);
    }

    /// <summary>
    /// Creates context for entering shadows.
    /// </summary>
    public static AbilityActivationContext ForEnteringShadows(string characterId, string abilityId)
    {
        return new AbilityActivationContext(
            characterId, abilityId,
            AbilityTriggerType.EnteringShadows,
            IsInShadows: true);
    }

    /// <summary>
    /// Creates context for entering a [Psychic Resonance] zone.
    /// </summary>
    public static AbilityActivationContext ForPsychicResonance(string characterId, string abilityId)
    {
        return new AbilityActivationContext(
            characterId, abilityId,
            AbilityTriggerType.EnteringZone,
            IsInPsychicResonance: true);
    }

    /// <summary>
    /// Creates context for after attacking from [Hidden].
    /// </summary>
    public static AbilityActivationContext ForAttackFromHidden(string characterId, string abilityId)
    {
        return new AbilityActivationContext(
            characterId, abilityId,
            AbilityTriggerType.AttackCompleted);
    }

    /// <summary>
    /// Creates context for party-wide ability activation.
    /// </summary>
    public static AbilityActivationContext ForParty(
        string characterId, string abilityId, IReadOnlyList<string> partyMemberIds)
    {
        return new AbilityActivationContext(
            characterId, abilityId,
            AbilityTriggerType.ManualActivation,
            PartyMemberIds: partyMemberIds);
    }
}

/// <summary>
/// Identifies what triggered an ability activation.
/// </summary>
public enum AbilityTriggerType
{
    /// <summary>Manual player activation.</summary>
    ManualActivation = 0,

    /// <summary>Triggered by climbing.</summary>
    Climbing = 1,

    /// <summary>Triggered by leaping.</summary>
    Leaping = 2,

    /// <summary>Triggered by falling.</summary>
    Falling = 3,

    /// <summary>Triggered by entering shadows.</summary>
    EnteringShadows = 4,

    /// <summary>Triggered by entering a zone.</summary>
    EnteringZone = 5,

    /// <summary>Triggered after attack completed.</summary>
    AttackCompleted = 6,

    /// <summary>Triggered by failed check.</summary>
    FailedCheck = 7
}
