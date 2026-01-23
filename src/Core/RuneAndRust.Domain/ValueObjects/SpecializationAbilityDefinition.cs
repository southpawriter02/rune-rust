namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the definition of a specialization ability.
/// </summary>
/// <remarks>
/// <para>
/// SpecializationAbilityDefinition encapsulates all metadata about an ability
/// including its name, type, description, usage limits, and effects.
/// </para>
/// <para>
/// <b>v0.15.2g:</b> Initial implementation of ability definitions.
/// </para>
/// </remarks>
/// <param name="AbilityId">The kebab-case ability ID.</param>
/// <param name="DisplayName">The display name with brackets.</param>
/// <param name="SpecializationId">The specialization this ability belongs to.</param>
/// <param name="AbilityType">The activation type.</param>
/// <param name="Description">The effect description.</param>
/// <param name="DailyUses">Daily use limit, or -1 for unlimited.</param>
/// <param name="EncounterUses">Encounter use limit, or -1 for unlimited.</param>
/// <param name="RequiresAction">Whether activation costs an action.</param>
/// <param name="RequiresConcentration">Whether the ability needs concentration.</param>
/// <param name="RangeFeet">Effect range in feet, or 0 for self.</param>
public readonly record struct SpecializationAbilityDefinition(
    string AbilityId,
    string DisplayName,
    string SpecializationId,
    SpecializationAbilityType AbilityType,
    string Description,
    int DailyUses = -1,
    int EncounterUses = -1,
    bool RequiresAction = false,
    bool RequiresConcentration = false,
    int RangeFeet = 0)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Indicates whether this ability has a daily use limit.
    /// </summary>
    public bool HasDailyLimit => DailyUses > 0;

    /// <summary>
    /// Indicates whether this ability has an encounter use limit.
    /// </summary>
    public bool HasEncounterLimit => EncounterUses > 0;

    /// <summary>
    /// Indicates whether this ability has any use limit.
    /// </summary>
    public bool HasUseLimit => HasDailyLimit || HasEncounterLimit;

    /// <summary>
    /// Indicates whether this is a passive ability.
    /// </summary>
    public bool IsPassive => AbilityType == SpecializationAbilityType.Passive;

    /// <summary>
    /// Indicates whether this is a triggered ability.
    /// </summary>
    public bool IsTriggered => AbilityType == SpecializationAbilityType.Triggered;

    /// <summary>
    /// Indicates whether this is a reactive ability.
    /// </summary>
    public bool IsReactive => AbilityType == SpecializationAbilityType.Reactive;

    /// <summary>
    /// Indicates whether this is an active ability.
    /// </summary>
    public bool IsActive => AbilityType == SpecializationAbilityType.Active;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a definition from a GantryRunnerAbility.
    /// </summary>
    public static SpecializationAbilityDefinition FromGantryRunner(GantryRunnerAbility ability)
    {
        return new SpecializationAbilityDefinition(
            AbilityId: ability.ToString().ToLowerInvariant(),
            DisplayName: GetGantryRunnerDisplayName(ability),
            SpecializationId: "gantry-runner",
            AbilityType: GetGantryRunnerType(ability),
            Description: GetGantryRunnerDescription(ability),
            DailyUses: ability == GantryRunnerAbility.DoubleJump ? 1 : -1,
            RequiresAction: ability == GantryRunnerAbility.WallRun);
    }

    /// <summary>
    /// Creates a definition from a MyrkengrAbility.
    /// </summary>
    public static SpecializationAbilityDefinition FromMyrkengr(MyrkengrAbility ability)
    {
        return new SpecializationAbilityDefinition(
            AbilityId: ability.ToString().ToLowerInvariant(),
            DisplayName: GetMyrkengrDisplayName(ability),
            SpecializationId: "myrk-gengr",
            AbilityType: GetMyrkengrType(ability),
            Description: GetMyrkengrDescription(ability),
            EncounterUses: ability == MyrkengrAbility.GhostlyForm ? 1 : -1,
            RequiresAction: ability == MyrkengrAbility.CloakTheParty,
            RequiresConcentration: ability == MyrkengrAbility.CloakTheParty,
            RangeFeet: ability == MyrkengrAbility.CloakTheParty ? 30 : 0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    private static string GetGantryRunnerDisplayName(GantryRunnerAbility ability) => ability switch
    {
        GantryRunnerAbility.RoofRunner => "[Roof-Runner]",
        GantryRunnerAbility.DeathDefyingLeap => "[Death-Defying Leap]",
        GantryRunnerAbility.WallRun => "[Wall-Run]",
        GantryRunnerAbility.DoubleJump => "[Double Jump]",
        GantryRunnerAbility.Featherfall => "[Featherfall]",
        _ => "[Unknown]"
    };

    private static SpecializationAbilityType GetGantryRunnerType(GantryRunnerAbility ability) => ability switch
    {
        GantryRunnerAbility.RoofRunner => SpecializationAbilityType.Passive,
        GantryRunnerAbility.DeathDefyingLeap => SpecializationAbilityType.Passive,
        GantryRunnerAbility.WallRun => SpecializationAbilityType.Active,
        GantryRunnerAbility.DoubleJump => SpecializationAbilityType.Reactive,
        GantryRunnerAbility.Featherfall => SpecializationAbilityType.Passive,
        _ => SpecializationAbilityType.Passive
    };

    private static string GetGantryRunnerDescription(GantryRunnerAbility ability) => ability switch
    {
        GantryRunnerAbility.RoofRunner => "Reduce climbing stages by 1 (minimum 1).",
        GantryRunnerAbility.DeathDefyingLeap => "Add +10 ft to maximum leap distance.",
        GantryRunnerAbility.WallRun => "Run vertically up walls (1 action, DC 3).",
        GantryRunnerAbility.DoubleJump => "Reroll failed leap with +1d10 (1/day).",
        GantryRunnerAbility.Featherfall => "Auto-succeed Crash Landing DC ≤ 3.",
        _ => "Unknown"
    };

    private static string GetMyrkengrDisplayName(MyrkengrAbility ability) => ability switch
    {
        MyrkengrAbility.SlipIntoShadow => "[Slip into Shadow]",
        MyrkengrAbility.GhostlyForm => "[Ghostly Form]",
        MyrkengrAbility.CloakTheParty => "[Cloak the Party]",
        MyrkengrAbility.OneWithTheStatic => "[One with the Static]",
        _ => "[Unknown]"
    };

    private static SpecializationAbilityType GetMyrkengrType(MyrkengrAbility ability) => ability switch
    {
        MyrkengrAbility.SlipIntoShadow => SpecializationAbilityType.Triggered,
        MyrkengrAbility.GhostlyForm => SpecializationAbilityType.Reactive,
        MyrkengrAbility.CloakTheParty => SpecializationAbilityType.Active,
        MyrkengrAbility.OneWithTheStatic => SpecializationAbilityType.Passive,
        _ => SpecializationAbilityType.Passive
    };

    private static string GetMyrkengrDescription(MyrkengrAbility ability) => ability switch
    {
        MyrkengrAbility.SlipIntoShadow => "Enter [Hidden] without action in shadows.",
        MyrkengrAbility.GhostlyForm => "Stay [Hidden] after attacking (1/encounter).",
        MyrkengrAbility.CloakTheParty => "Grant party +2d10 Passive Stealth (30 ft).",
        MyrkengrAbility.OneWithTheStatic => "Auto-[Hidden] in [Psychic Resonance].",
        _ => "Unknown"
    };
}
