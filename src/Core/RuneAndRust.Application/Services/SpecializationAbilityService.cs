namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements specialization ability management and activation.
/// </summary>
/// <remarks>
/// <para>
/// SpecializationAbilityService handles activation of Gantry-Runner and Myrk-gengr
/// abilities, checking prerequisites, managing uses, and applying effects.
/// </para>
/// <para>
/// <b>v0.15.2g:</b> Initial implementation of specialization ability service.
/// </para>
/// </remarks>
public sealed class SpecializationAbilityService : ISpecializationAbilityService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Stage reduction from [Roof-Runner].
    /// </summary>
    public const int RoofRunnerStageReduction = 1;

    /// <summary>
    /// Distance bonus from [Death-Defying Leap].
    /// </summary>
    public const int DeathDefyingLeapBonusFeet = 10;

    /// <summary>
    /// DC for [Wall-Run] Acrobatics check.
    /// </summary>
    public const int WallRunDc = 3;

    /// <summary>
    /// Dice bonus from [Double Jump] reroll.
    /// </summary>
    public const int DoubleJumpDiceBonus = 1;

    /// <summary>
    /// Maximum DC for [Featherfall] auto-success.
    /// </summary>
    public const int FeatherfallDcThreshold = 3;

    /// <summary>
    /// Dice bonus from [Cloak the Party].
    /// </summary>
    public const int CloakThePartyDiceBonus = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly ILogger<SpecializationAbilityService> _logger;

    /// <summary>
    /// Tracks daily uses per character per ability.
    /// Key: "characterId:abilityId", Value: uses remaining.
    /// </summary>
    private readonly Dictionary<string, int> _dailyUses = new();

    /// <summary>
    /// Tracks encounter uses per character per ability.
    /// Key: "characterId:abilityId", Value: uses remaining.
    /// </summary>
    private readonly Dictionary<string, int> _encounterUses = new();

    /// <summary>
    /// Tracks which abilities each character has.
    /// Key: characterId, Value: set of ability IDs.
    /// </summary>
    private readonly Dictionary<string, HashSet<string>> _characterAbilities = new();

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationAbilityService"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public SpecializationAbilityService(ILogger<SpecializationAbilityService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("SpecializationAbilityService initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ABILITY QUERIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool HasAbility(string characterId, GantryRunnerAbility ability)
    {
        var abilityId = ability.GetAbilityId();
        return HasAbilityById(characterId, abilityId);
    }

    /// <inheritdoc/>
    public bool HasAbility(string characterId, MyrkengrAbility ability)
    {
        var abilityId = ability.GetAbilityId();
        return HasAbilityById(characterId, abilityId);
    }

    /// <inheritdoc/>
    public SpecializationAbilityDefinition GetDefinition(GantryRunnerAbility ability)
    {
        return SpecializationAbilityDefinition.FromGantryRunner(ability);
    }

    /// <inheritdoc/>
    public SpecializationAbilityDefinition GetDefinition(MyrkengrAbility ability)
    {
        return SpecializationAbilityDefinition.FromMyrkengr(ability);
    }

    /// <inheritdoc/>
    public int GetRemainingUses(string characterId, string abilityId)
    {
        var key = GetUseKey(characterId, abilityId);

        // Check daily first, then encounter
        if (_dailyUses.TryGetValue(key, out var daily))
        {
            return daily;
        }

        if (_encounterUses.TryGetValue(key, out var encounter))
        {
            return encounter;
        }

        return -1; // Unlimited
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GANTRY-RUNNER ACTIVATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public AbilityActivationResult ApplyRoofRunner(string characterId, int originalStages)
    {
        const string abilityId = "roof-runner";

        if (!HasAbility(characterId, GantryRunnerAbility.RoofRunner))
        {
            _logger.LogDebug("Character {CharacterId} does not have [Roof-Runner]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        // Apply reduction (minimum 1 stage)
        var newStages = Math.Max(1, originalStages - RoofRunnerStageReduction);

        _logger.LogInformation(
            "[Roof-Runner] activated: {CharacterId} climbing stages reduced {Original} → {New}",
            characterId, originalStages, newStages);

        return AbilityActivationResult.StageReduction(
            characterId, abilityId, RoofRunnerStageReduction, originalStages, newStages);
    }

    /// <inheritdoc/>
    public AbilityActivationResult ApplyDeathDefyingLeap(string characterId, int baseMaxDistance)
    {
        const string abilityId = "death-defying-leap";

        if (!HasAbility(characterId, GantryRunnerAbility.DeathDefyingLeap))
        {
            _logger.LogDebug("Character {CharacterId} does not have [Death-Defying Leap]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        var newMax = baseMaxDistance + DeathDefyingLeapBonusFeet;

        _logger.LogInformation(
            "[Death-Defying Leap] activated: {CharacterId} max leap {Base} → {New} ft",
            characterId, baseMaxDistance, newMax);

        return AbilityActivationResult.DistanceBonus(characterId, abilityId, DeathDefyingLeapBonusFeet);
    }

    /// <inheritdoc/>
    public AbilityActivationResult ActivateWallRun(string characterId, int heightFeet, int dicePool)
    {
        const string abilityId = "wall-run";

        if (!HasAbility(characterId, GantryRunnerAbility.WallRun))
        {
            _logger.LogDebug("Character {CharacterId} does not have [Wall-Run]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        _logger.LogInformation(
            "[Wall-Run] activated: {CharacterId} running {Height} ft vertical (DC {Dc})",
            characterId, heightFeet, WallRunDc);

        return AbilityActivationResult.AutoSuccess(
            characterId, abilityId,
            $"Wall-run {heightFeet} ft vertical (DC {WallRunDc})",
            "You sprint up the wall, defying gravity with each stride.");
    }

    /// <inheritdoc/>
    public AbilityActivationResult ActivateDoubleJump(string characterId)
    {
        const string abilityId = "double-jump";

        if (!HasAbility(characterId, GantryRunnerAbility.DoubleJump))
        {
            _logger.LogDebug("Character {CharacterId} does not have [Double Jump]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        // Check daily uses
        var key = GetUseKey(characterId, abilityId);
        if (!_dailyUses.TryGetValue(key, out var remaining))
        {
            // Initialize uses
            remaining = 1;
            _dailyUses[key] = remaining;
        }

        if (remaining <= 0)
        {
            _logger.LogDebug("[Double Jump] exhausted for {CharacterId}", characterId);
            return AbilityActivationResult.NoUsesRemaining(characterId, abilityId);
        }

        // Consume use
        _dailyUses[key] = remaining - 1;

        _logger.LogInformation(
            "[Double Jump] activated: {CharacterId} rerolling with +{Bonus}d10, uses remaining: {Uses}",
            characterId, DoubleJumpDiceBonus, remaining - 1);

        return AbilityActivationResult.Reroll(characterId, abilityId, DoubleJumpDiceBonus, remaining - 1);
    }

    /// <inheritdoc/>
    public AbilityActivationResult CheckFeatherfall(string characterId, int crashLandingDc)
    {
        const string abilityId = "featherfall";

        if (!HasAbility(characterId, GantryRunnerAbility.Featherfall))
        {
            _logger.LogDebug("Character {CharacterId} does not have [Featherfall]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        if (crashLandingDc > FeatherfallDcThreshold)
        {
            _logger.LogDebug(
                "[Featherfall] DC {Dc} exceeds threshold {Threshold} for {CharacterId}",
                crashLandingDc, FeatherfallDcThreshold, characterId);
            return AbilityActivationResult.DcTooHigh(characterId, abilityId, crashLandingDc, FeatherfallDcThreshold);
        }

        _logger.LogInformation(
            "[Featherfall] activated: {CharacterId} auto-success on Crash Landing DC {Dc}",
            characterId, crashLandingDc);

        return AbilityActivationResult.AutoSuccess(
            characterId, abilityId,
            $"Auto-succeeded Crash Landing DC {crashLandingDc}",
            "You land with preternatural grace, absorbing the impact effortlessly.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MYRK-GENGR ACTIVATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public AbilityActivationResult ActivateSlipIntoShadow(string characterId, bool isInShadows, bool isObserved)
    {
        const string abilityId = "slip-into-shadow";

        if (!HasAbility(characterId, MyrkengrAbility.SlipIntoShadow))
        {
            _logger.LogDebug("Character {CharacterId} does not have [Slip into Shadow]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        if (!isInShadows)
        {
            _logger.LogDebug("[Slip into Shadow] failed: {CharacterId} not in shadows", characterId);
            return AbilityActivationResult.ConditionsNotMet(characterId, abilityId, "Not in shadows or dim lighting");
        }

        if (isObserved)
        {
            _logger.LogDebug("[Slip into Shadow] failed: {CharacterId} is observed", characterId);
            return AbilityActivationResult.ConditionsNotMet(characterId, abilityId, "Actively observed by enemies");
        }

        _logger.LogInformation("[Slip into Shadow] activated: {CharacterId} entering [Hidden] without action", characterId);

        return AbilityActivationResult.HiddenEntry(
            characterId, abilityId,
            "You melt into the shadows without breaking stride.");
    }

    /// <inheritdoc/>
    public AbilityActivationResult ActivateGhostlyForm(string characterId)
    {
        const string abilityId = "ghostly-form";

        if (!HasAbility(characterId, MyrkengrAbility.GhostlyForm))
        {
            _logger.LogDebug("Character {CharacterId} does not have [Ghostly Form]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        // Check encounter uses
        var key = GetUseKey(characterId, abilityId);
        if (!_encounterUses.TryGetValue(key, out var remaining))
        {
            // Initialize uses
            remaining = 1;
            _encounterUses[key] = remaining;
        }

        if (remaining <= 0)
        {
            _logger.LogDebug("[Ghostly Form] exhausted for {CharacterId}", characterId);
            return AbilityActivationResult.NoUsesRemaining(characterId, abilityId);
        }

        // Consume use
        _encounterUses[key] = remaining - 1;

        _logger.LogInformation(
            "[Ghostly Form] activated: {CharacterId} maintaining [Hidden] after attack, uses remaining: {Uses}",
            characterId, remaining - 1);

        return AbilityActivationResult.MaintainedHidden(characterId, abilityId, remaining - 1);
    }

    /// <inheritdoc/>
    public AbilityActivationResult ActivateCloakTheParty(string characterId, IReadOnlyList<string> partyMemberIds)
    {
        const string abilityId = "cloak-the-party";

        if (!HasAbility(characterId, MyrkengrAbility.CloakTheParty))
        {
            _logger.LogDebug("Character {CharacterId} does not have [Cloak the Party]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        var memberCount = partyMemberIds.Count;

        _logger.LogInformation(
            "[Cloak the Party] activated: {CharacterId} granting +{Bonus}d10 to {Count} party members",
            characterId, CloakThePartyDiceBonus, memberCount);

        return AbilityActivationResult.PartyDiceBonus(
            characterId, abilityId, CloakThePartyDiceBonus, memberCount);
    }

    /// <inheritdoc/>
    public AbilityActivationResult CheckOneWithTheStatic(string characterId, bool isInPsychicResonance)
    {
        const string abilityId = "one-with-the-static";

        if (!HasAbility(characterId, MyrkengrAbility.OneWithTheStatic))
        {
            _logger.LogDebug("Character {CharacterId} does not have [One with the Static]", characterId);
            return AbilityActivationResult.NotAvailable(characterId, abilityId);
        }

        if (!isInPsychicResonance)
        {
            _logger.LogDebug("[One with the Static] failed: {CharacterId} not in [Psychic Resonance] zone", characterId);
            return AbilityActivationResult.ConditionsNotMet(characterId, abilityId, "Not in [Psychic Resonance] zone");
        }

        _logger.LogInformation("[One with the Static] activated: {CharacterId} auto-entering [Hidden]", characterId);

        return AbilityActivationResult.HiddenEntry(
            characterId, abilityId,
            "The psychic static envelops you, hiding you from perception.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // USE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public void ResetDailyUses(string characterId)
    {
        var keysToReset = _dailyUses.Keys
            .Where(k => k.StartsWith($"{characterId}:"))
            .ToList();

        foreach (var key in keysToReset)
        {
            _dailyUses.Remove(key);
        }

        _logger.LogDebug("Reset daily ability uses for {CharacterId}", characterId);
    }

    /// <inheritdoc/>
    public void ResetEncounterUses(string characterId)
    {
        var keysToReset = _encounterUses.Keys
            .Where(k => k.StartsWith($"{characterId}:"))
            .ToList();

        foreach (var key in keysToReset)
        {
            _encounterUses.Remove(key);
        }

        _logger.LogDebug("Reset encounter ability uses for {CharacterId}", characterId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Grants an ability to a character (for testing).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="abilityId">The ability ID.</param>
    public void GrantAbility(string characterId, string abilityId)
    {
        if (!_characterAbilities.TryGetValue(characterId, out var abilities))
        {
            abilities = new HashSet<string>();
            _characterAbilities[characterId] = abilities;
        }

        abilities.Add(abilityId);
        _logger.LogDebug("Granted ability {AbilityId} to {CharacterId}", abilityId, characterId);
    }

    /// <summary>
    /// Grants a Gantry-Runner ability to a character (for testing).
    /// </summary>
    public void GrantAbility(string characterId, GantryRunnerAbility ability)
    {
        GrantAbility(characterId, ability.GetAbilityId());
    }

    /// <summary>
    /// Grants a Myrk-gengr ability to a character (for testing).
    /// </summary>
    public void GrantAbility(string characterId, MyrkengrAbility ability)
    {
        GrantAbility(characterId, ability.GetAbilityId());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    private bool HasAbilityById(string characterId, string abilityId)
    {
        if (_characterAbilities.TryGetValue(characterId, out var abilities))
        {
            return abilities.Contains(abilityId);
        }

        return false;
    }

    private static string GetUseKey(string characterId, string abilityId)
    {
        return $"{characterId}:{abilityId}";
    }
}
