// ------------------------------------------------------------------------------
// <copyright file="SpecializationAbilityEffect.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the effect of a triggered specialization ability on social checks.
// Part of v0.15.3i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the effect of a triggered specialization ability.
/// </summary>
/// <remarks>
/// <para>
/// This value object captures what modifications should be applied to a social check
/// or interaction based on specialization abilities. It serves as the output of the
/// rhetoric specialization service methods.
/// </para>
/// <para>
/// Effects can include:
/// <list type="bullet">
///   <item><description>Dice pool modifications (bonus dice)</description></item>
///   <item><description>Auto-success conditions (bypass check)</description></item>
///   <item><description>Outcome modifications (prevent option locking, fumble downgrade)</description></item>
///   <item><description>Stress reduction (for self or party)</description></item>
///   <item><description>Asset creation (forged documents)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Ability">The specialization ability that triggered this effect.</param>
/// <param name="DiceBonus">Bonus dice to add to the pool (0 if none).</param>
/// <param name="AutoSuccess">If true, bypass the check entirely.</param>
/// <param name="AutoSuccessThreshold">DC threshold for auto-success abilities.</param>
/// <param name="PreventOptionLock">If true, failed check doesn't lock dialogue options.</param>
/// <param name="StressReduction">Amount of stress reduction applied.</param>
/// <param name="IsPartyWide">If true, effect applies to all party members.</param>
/// <param name="FumbleDowngrade">If true, fumble is treated as normal failure.</param>
/// <param name="CreatesAsset">If true, ability creates a usable asset.</param>
/// <param name="AssetType">Type of asset created (e.g., "ForgedDocument").</param>
/// <param name="AssetQuality">Quality tier of created asset (1-5 scale).</param>
/// <param name="Duration">How long the effect lasts (null = immediate).</param>
/// <param name="Description">Human-readable description of the effect.</param>
public readonly record struct SpecializationAbilityEffect(
    RhetoricSpecializationAbility Ability,
    int DiceBonus,
    bool AutoSuccess,
    int AutoSuccessThreshold,
    bool PreventOptionLock,
    int StressReduction,
    bool IsPartyWide,
    bool FumbleDowngrade,
    bool CreatesAsset,
    string? AssetType,
    int AssetQuality,
    TimeSpan? Duration,
    string Description)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets whether this effect modifies the dice pool.
    /// </summary>
    /// <value>True if <see cref="DiceBonus"/> is greater than 0.</value>
    public bool ModifiesDicePool => DiceBonus > 0;

    /// <summary>
    /// Gets whether this effect modifies the check outcome.
    /// </summary>
    /// <value>True if the effect prevents option locking or downgrades fumbles.</value>
    public bool ModifiesOutcome => PreventOptionLock || FumbleDowngrade;

    /// <summary>
    /// Gets whether this effect bypasses the check entirely.
    /// </summary>
    /// <value>True if <see cref="AutoSuccess"/> is set.</value>
    public bool BypassesCheck => AutoSuccess;

    /// <summary>
    /// Gets whether this effect affects party members.
    /// </summary>
    /// <value>True if <see cref="IsPartyWide"/> is set.</value>
    public bool AffectsParty => IsPartyWide;

    /// <summary>
    /// Gets whether this effect reduces stress.
    /// </summary>
    /// <value>True if <see cref="StressReduction"/> is greater than 0.</value>
    public bool ReducesStress => StressReduction > 0;

    /// <summary>
    /// Gets whether this effect creates an asset.
    /// </summary>
    /// <value>True if <see cref="CreatesAsset"/> is set.</value>
    public bool ProducesAsset => CreatesAsset;

    /// <summary>
    /// Gets whether this effect has any impact.
    /// </summary>
    /// <value>True if any effect property is active; false for <see cref="None"/>.</value>
    public bool HasEffect =>
        ModifiesDicePool ||
        ModifiesOutcome ||
        BypassesCheck ||
        ReducesStress ||
        ProducesAsset;

    /// <summary>
    /// Gets whether this effect has a duration (is not immediate).
    /// </summary>
    /// <value>True if <see cref="Duration"/> is set.</value>
    public bool HasDuration => Duration.HasValue;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates an effect with only a dice bonus.
    /// </summary>
    /// <param name="ability">The ability that triggered.</param>
    /// <param name="bonus">Number of bonus dice to add.</param>
    /// <param name="description">Human-readable description.</param>
    /// <returns>A new effect with the specified dice bonus.</returns>
    public static SpecializationAbilityEffect DiceBonusEffect(
        RhetoricSpecializationAbility ability,
        int bonus,
        string description) => new(
            Ability: ability,
            DiceBonus: bonus,
            AutoSuccess: false,
            AutoSuccessThreshold: 0,
            PreventOptionLock: false,
            StressReduction: 0,
            IsPartyWide: false,
            FumbleDowngrade: false,
            CreatesAsset: false,
            AssetType: null,
            AssetQuality: 0,
            Duration: null,
            Description: description);

    /// <summary>
    /// Creates an effect that auto-succeeds below a DC threshold.
    /// </summary>
    /// <param name="ability">The ability that triggered.</param>
    /// <param name="threshold">The DC at or below which auto-success applies.</param>
    /// <param name="description">Human-readable description.</param>
    /// <returns>A new effect with auto-success enabled.</returns>
    public static SpecializationAbilityEffect AutoSucceedEffect(
        RhetoricSpecializationAbility ability,
        int threshold,
        string description) => new(
            Ability: ability,
            DiceBonus: 0,
            AutoSuccess: true,
            AutoSuccessThreshold: threshold,
            PreventOptionLock: false,
            StressReduction: 0,
            IsPartyWide: false,
            FumbleDowngrade: false,
            CreatesAsset: false,
            AssetType: null,
            AssetQuality: 0,
            Duration: null,
            Description: description);

    /// <summary>
    /// Creates an effect that prevents option locking on failure.
    /// </summary>
    /// <param name="ability">The ability that triggered.</param>
    /// <param name="description">Human-readable description.</param>
    /// <returns>A new effect that prevents dialogue option locking.</returns>
    public static SpecializationAbilityEffect PreventLockEffect(
        RhetoricSpecializationAbility ability,
        string description) => new(
            Ability: ability,
            DiceBonus: 0,
            AutoSuccess: false,
            AutoSuccessThreshold: 0,
            PreventOptionLock: true,
            StressReduction: 0,
            IsPartyWide: false,
            FumbleDowngrade: false,
            CreatesAsset: false,
            AssetType: null,
            AssetQuality: 0,
            Duration: null,
            Description: description);

    /// <summary>
    /// Creates a party-wide stress reduction effect.
    /// </summary>
    /// <param name="ability">The ability that triggered.</param>
    /// <param name="reduction">Amount of stress to reduce.</param>
    /// <param name="description">Human-readable description.</param>
    /// <returns>A new effect that reduces stress for all party members.</returns>
    public static SpecializationAbilityEffect StressReliefEffect(
        RhetoricSpecializationAbility ability,
        int reduction,
        string description) => new(
            Ability: ability,
            DiceBonus: 0,
            AutoSuccess: false,
            AutoSuccessThreshold: 0,
            PreventOptionLock: false,
            StressReduction: reduction,
            IsPartyWide: true,
            FumbleDowngrade: false,
            CreatesAsset: false,
            AssetType: null,
            AssetQuality: 0,
            Duration: null,
            Description: description);

    /// <summary>
    /// Creates an inspiration effect that grants allies bonus dice.
    /// </summary>
    /// <param name="bonus">Number of bonus dice to grant.</param>
    /// <param name="isPartyWide">Whether the effect applies to all party members.</param>
    /// <param name="description">Human-readable description.</param>
    /// <returns>A new effect that grants allies bonus dice.</returns>
    public static SpecializationAbilityEffect InspirationEffect(
        int bonus,
        bool isPartyWide,
        string description) => new(
            Ability: RhetoricSpecializationAbility.InspiringWords,
            DiceBonus: bonus,
            AutoSuccess: false,
            AutoSuccessThreshold: 0,
            PreventOptionLock: false,
            StressReduction: 0,
            IsPartyWide: isPartyWide,
            FumbleDowngrade: false,
            CreatesAsset: false,
            AssetType: null,
            AssetQuality: 0,
            Duration: TimeSpan.FromMinutes(10),
            Description: description);

    /// <summary>
    /// Creates an effect for the Myrk-gengr's [Maintain Cover] ability.
    /// </summary>
    /// <param name="description">Human-readable description.</param>
    /// <returns>A new effect with dice bonus, stress reduction, and fumble downgrade.</returns>
    /// <remarks>
    /// This is a composite effect that includes:
    /// <list type="bullet">
    ///   <item><description>+2d10 to cover maintenance checks</description></item>
    ///   <item><description>-1 Stress from cover challenges</description></item>
    ///   <item><description>Fumble consequences downgraded one tier</description></item>
    /// </list>
    /// </remarks>
    public static SpecializationAbilityEffect MaintainCoverEffect(
        string description) => new(
            Ability: RhetoricSpecializationAbility.MaintainCover,
            DiceBonus: 2,
            AutoSuccess: false,
            AutoSuccessThreshold: 0,
            PreventOptionLock: false,
            StressReduction: 1,
            IsPartyWide: false,
            FumbleDowngrade: true,
            CreatesAsset: false,
            AssetType: null,
            AssetQuality: 0,
            Duration: null,
            Description: description);

    /// <summary>
    /// Creates an effect for document forgery.
    /// </summary>
    /// <param name="quality">Quality tier of the forgery (1-5).</param>
    /// <param name="description">Human-readable description.</param>
    /// <returns>A new effect representing a created forged document.</returns>
    /// <remarks>
    /// Quality tiers:
    /// <list type="bullet">
    ///   <item><description>2 - Passable: -2 DC to detect</description></item>
    ///   <item><description>3 - Good: -4 DC to detect</description></item>
    ///   <item><description>4 - Excellent: -6 DC to detect</description></item>
    ///   <item><description>5 - Masterwork: Requires expertise to detect</description></item>
    /// </list>
    /// </remarks>
    public static SpecializationAbilityEffect ForgeryEffect(
        int quality,
        string description) => new(
            Ability: RhetoricSpecializationAbility.ForgeDocuments,
            DiceBonus: 0,
            AutoSuccess: false,
            AutoSuccessThreshold: 0,
            PreventOptionLock: false,
            StressReduction: 0,
            IsPartyWide: false,
            FumbleDowngrade: false,
            CreatesAsset: true,
            AssetType: "ForgedDocument",
            AssetQuality: quality,
            Duration: null,
            Description: description);

    /// <summary>
    /// Represents no effect (ability did not trigger or is not applicable).
    /// </summary>
    /// <value>An effect with all properties set to their default values.</value>
    public static SpecializationAbilityEffect None => new(
        Ability: default,
        DiceBonus: 0,
        AutoSuccess: false,
        AutoSuccessThreshold: 0,
        PreventOptionLock: false,
        StressReduction: 0,
        IsPartyWide: false,
        FumbleDowngrade: false,
        CreatesAsset: false,
        AssetType: null,
        AssetQuality: 0,
        Duration: null,
        Description: string.Empty);

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a formatted display string for the effect.
    /// </summary>
    /// <returns>A human-readable summary of the effect.</returns>
    public string ToDisplayString()
    {
        if (!HasEffect)
        {
            return "No effect";
        }

        var parts = new List<string>();

        if (ModifiesDicePool)
        {
            parts.Add($"+{DiceBonus}d10");
        }

        if (AutoSuccess)
        {
            parts.Add($"Auto-success (DC ≤ {AutoSuccessThreshold})");
        }

        if (PreventOptionLock)
        {
            parts.Add("Options remain unlocked");
        }

        if (FumbleDowngrade)
        {
            parts.Add("Fumble downgraded");
        }

        if (ReducesStress)
        {
            parts.Add($"-{StressReduction} Stress");
        }

        if (ProducesAsset)
        {
            parts.Add($"Created {AssetType} (Quality {AssetQuality}/5)");
        }

        if (HasDuration)
        {
            parts.Add($"Duration: {Duration!.Value.TotalMinutes}min");
        }

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Gets a short summary suitable for logs.
    /// </summary>
    /// <returns>A concise log-friendly string.</returns>
    public string ToLogString()
    {
        if (!HasEffect)
        {
            return "None";
        }

        return $"[{Ability}] {ToDisplayString()}";
    }
}
