// ------------------------------------------------------------------------------
// <copyright file="BypassSpecializationAbilityService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Implements the bypass specialization ability service, managing passive effects,
// triggered abilities, and unique actions for System Bypass skill specializations.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implements bypass specialization ability management and activation.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all specialization abilities related to the System Bypass
/// skill system. It processes passive bonuses, triggered effects, and unique actions
/// for the four bypass specializations:
/// <list type="bullet">
///   <item><description><b>Scrap-Tinker:</b> [Master Craftsman], [Relock]</description></item>
///   <item><description><b>Ruin-Stalker:</b> [Trap Artist], [Sixth Sense]</description></item>
///   <item><description><b>Jötun-Reader:</b> [Deep Access], [Pattern Recognition]</description></item>
///   <item><description><b>Gantry-Runner:</b> [Fast Pick], [Bypass Under Fire]</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.4i:</b> Initial implementation of bypass specialization ability service.
/// </para>
/// </remarks>
public sealed class BypassSpecializationAbilityService : IBypassSpecializationAbilityService
{
    // =========================================================================
    // CONSTANTS
    // =========================================================================

    /// <summary>
    /// Time reduction from [Fast Pick] in rounds.
    /// </summary>
    public const int FastPickTimeReduction = 1;

    /// <summary>
    /// DC reduction from [Pattern Recognition] for [Glitched] targets.
    /// </summary>
    public const int PatternRecognitionDcReduction = 2;

    /// <summary>
    /// Detection radius for [Sixth Sense] in feet.
    /// </summary>
    public const int SixthSenseRadiusFeet = 10;

    /// <summary>
    /// DC for [Relock] unique action.
    /// </summary>
    public const int RelockDc = 12;

    /// <summary>
    /// DC for [Trap Artist] unique action.
    /// </summary>
    public const int TrapArtistDc = 14;

    // =========================================================================
    // FIELDS
    // =========================================================================

    private readonly ILogger<BypassSpecializationAbilityService> _logger;
    private readonly IDiceService _diceService;

    /// <summary>
    /// Tracks bypass specialization per character.
    /// Key: characterId, Value: bypass specialization.
    /// </summary>
    private readonly Dictionary<string, BypassSpecialization> _characterSpecializations = new();

    /// <summary>
    /// Tracks locks that have been picked (for [Relock] validation).
    /// Key: lockId, Value: characterId who picked it.
    /// </summary>
    private readonly Dictionary<string, string> _pickedLocks = new();

    /// <summary>
    /// Tracks traps that have been disabled (for [Trap Artist] validation).
    /// Key: trapId, Value: characterId who disabled it.
    /// </summary>
    private readonly Dictionary<string, string> _disabledTraps = new();

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================

    /// <summary>
    /// Initializes a new instance of the <see cref="BypassSpecializationAbilityService"/> class.
    /// </summary>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="diceService"/> or <paramref name="logger"/> is null.
    /// </exception>
    public BypassSpecializationAbilityService(
        IDiceService diceService,
        ILogger<BypassSpecializationAbilityService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("BypassSpecializationAbilityService initialized");
    }

    // =========================================================================
    // ABILITY QUERIES
    // =========================================================================

    /// <inheritdoc/>
    public BypassSpecialization GetCharacterSpecialization(string characterId)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));

        if (_characterSpecializations.TryGetValue(characterId, out var specialization))
        {
            _logger.LogDebug(
                "GetCharacterSpecialization: Character {CharacterId} has specialization {Specialization}",
                characterId, specialization);
            return specialization;
        }

        _logger.LogDebug(
            "GetCharacterSpecialization: Character {CharacterId} has no bypass specialization",
            characterId);
        return BypassSpecialization.None;
    }

    /// <inheritdoc/>
    public IReadOnlyList<BypassSpecializationAbility> GetCharacterAbilities(string characterId)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));

        var specialization = GetCharacterSpecialization(characterId);
        var abilities = BypassSpecializationAbility.GetAbilitiesFor(specialization);

        _logger.LogDebug(
            "GetCharacterAbilities: Character {CharacterId} ({Specialization}) has {Count} abilities",
            characterId, specialization, abilities.Count);

        return abilities;
    }

    /// <inheritdoc/>
    public bool HasAbility(string characterId, string abilityId)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrEmpty(abilityId, nameof(abilityId));

        var abilities = GetCharacterAbilities(characterId);
        var hasAbility = abilities.Any(a =>
            a.AbilityId.Equals(abilityId, StringComparison.OrdinalIgnoreCase));

        _logger.LogDebug(
            "HasAbility: Character {CharacterId} {Has} ability {AbilityId}",
            characterId, hasAbility ? "has" : "does not have", abilityId);

        return hasAbility;
    }

    /// <inheritdoc/>
    public BypassSpecializationAbility? GetAbilityDefinition(string abilityId)
    {
        ArgumentException.ThrowIfNullOrEmpty(abilityId, nameof(abilityId));

        var ability = BypassSpecializationAbility.GetById(abilityId);

        if (ability.HasValue)
        {
            _logger.LogDebug(
                "GetAbilityDefinition: Found ability {AbilityId} ({Name})",
                abilityId, ability.Value.Name);
        }
        else
        {
            _logger.LogWarning(
                "GetAbilityDefinition: Ability {AbilityId} not found",
                abilityId);
        }

        return ability;
    }

    // =========================================================================
    // PASSIVE ABILITY APPLICATION
    // =========================================================================

    /// <inheritdoc/>
    public BypassAbilityContext GetPassiveModifiers(
        string characterId,
        BypassType bypassType,
        bool isInDanger,
        bool isTargetGlitched)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));

        var specialization = GetCharacterSpecialization(characterId);
        if (specialization == BypassSpecialization.None)
        {
            _logger.LogDebug(
                "GetPassiveModifiers: Character {CharacterId} has no specialization, returning empty context",
                characterId);
            return BypassAbilityContext.Empty();
        }

        var timeReduction = 0;
        var dcReduction = 0;
        var negateCombatPenalties = false;
        var appliedAbilities = new List<string>();

        // Check [Fast Pick] - Gantry-Runner passive
        if (specialization == BypassSpecialization.GantryRunner)
        {
            timeReduction = FastPickTimeReduction;
            appliedAbilities.Add("fast-pick");
            _logger.LogInformation(
                "[Fast Pick] active for {CharacterId}: -{Reduction} rounds on {BypassType}",
                characterId, FastPickTimeReduction, bypassType);
        }

        // Check [Bypass Under Fire] - Gantry-Runner passive when in danger
        if (specialization == BypassSpecialization.GantryRunner && isInDanger)
        {
            negateCombatPenalties = true;
            appliedAbilities.Add("bypass-under-fire");
            _logger.LogInformation(
                "[Bypass Under Fire] active for {CharacterId}: combat penalties negated on {BypassType}",
                characterId, bypassType);
        }

        // Check [Pattern Recognition] - Jötun-Reader passive when target is glitched
        if (specialization == BypassSpecialization.JotunReader && isTargetGlitched)
        {
            dcReduction = PatternRecognitionDcReduction;
            appliedAbilities.Add("pattern-recognition");
            _logger.LogInformation(
                "[Pattern Recognition] active for {CharacterId}: -{Reduction} DC on [Glitched] target",
                characterId, PatternRecognitionDcReduction);
        }

        var context = new BypassAbilityContext(
            TimeReductionRounds: timeReduction,
            DcReduction: dcReduction,
            NegateCombatPenalties: negateCombatPenalties,
            AppliedAbilities: appliedAbilities);

        _logger.LogDebug(
            "GetPassiveModifiers: Character {CharacterId} context: Time-{Time} DC-{Dc} NegateP={Neg}",
            characterId, timeReduction, dcReduction, negateCombatPenalties);

        return context;
    }

    /// <inheritdoc/>
    public TrapDetectionResult CheckTrapDetection(
        string characterId,
        int characterX,
        int characterY,
        IEnumerable<TrapInfo> nearbyTraps)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));
        ArgumentNullException.ThrowIfNull(nearbyTraps, nameof(nearbyTraps));

        var specialization = GetCharacterSpecialization(characterId);

        // [Sixth Sense] only available to Ruin-Stalkers
        if (specialization != BypassSpecialization.RuinStalker)
        {
            _logger.LogDebug(
                "CheckTrapDetection: Character {CharacterId} is not a Ruin-Stalker, skipping",
                characterId);
            return TrapDetectionResult.AreaClear(characterId, characterX, characterY);
        }

        _logger.LogInformation(
            "[Sixth Sense] checking for traps around {CharacterId} at ({X}, {Y})",
            characterId, characterX, characterY);

        // Find traps within detection radius
        var detectedTraps = new List<DetectedTrap>();
        foreach (var trap in nearbyTraps)
        {
            // Skip already-detected or non-hidden traps
            if (!trap.IsArmed || !trap.IsHidden)
            {
                continue;
            }

            var dx = trap.PositionX - characterX;
            var dy = trap.PositionY - characterY;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= SixthSenseRadiusFeet)
            {
                var detected = TrapDetectionResult.CreateDetectedTrap(
                    trap.TrapId,
                    trap.TrapName,
                    trap.PositionX,
                    trap.PositionY,
                    characterX,
                    characterY);

                detectedTraps.Add(detected);

                _logger.LogInformation(
                    "[Sixth Sense] detected trap {TrapId} at ({X}, {Y}), {Distance} ft from {CharacterId}",
                    trap.TrapId, trap.PositionX, trap.PositionY, (int)distance, characterId);
            }
        }

        if (detectedTraps.Count > 0)
        {
            _logger.LogInformation(
                "[Sixth Sense] result for {CharacterId}: {Count} traps detected",
                characterId, detectedTraps.Count);
            return TrapDetectionResult.TrapsDetected(
                characterId, characterX, characterY, detectedTraps);
        }

        _logger.LogDebug(
            "[Sixth Sense] result for {CharacterId}: area clear",
            characterId);
        return TrapDetectionResult.AreaClear(characterId, characterX, characterY);
    }

    // =========================================================================
    // TRIGGERED ABILITY APPLICATION
    // =========================================================================

    /// <inheritdoc/>
    public BypassAbilityActivationResult? TryTriggerAbility(
        string characterId,
        BypassType bypassType,
        bool wasSuccessful,
        string targetId)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrEmpty(targetId, nameof(targetId));

        var specialization = GetCharacterSpecialization(characterId);

        // Track successful bypasses for unique action validation
        if (wasSuccessful)
        {
            TrackSuccessfulBypass(characterId, bypassType, targetId);
        }

        // Check [Deep Access] - Jötun-Reader triggered ability on terminal hack success
        if (specialization == BypassSpecialization.JotunReader &&
            bypassType == BypassType.TerminalHacking &&
            wasSuccessful)
        {
            _logger.LogInformation(
                "[Deep Access] triggered for {CharacterId} on terminal {TargetId}: upgrading to Admin access",
                characterId, targetId);

            return BypassAbilityActivationResult.DeepAccessUpgrade(
                terminalId: targetId,
                previousAccessLevel: 1); // Assume User level before upgrade
        }

        // Check [Master Craftsman] - Scrap-Tinker triggered ability on craft
        // Note: This is handled differently as crafting is a separate system
        // The service returns the unlocked recipes instead

        _logger.LogDebug(
            "TryTriggerAbility: No triggered abilities for {CharacterId} on {BypassType} (Success={Success})",
            characterId, bypassType, wasSuccessful);

        return null;
    }

    // =========================================================================
    // UNIQUE ACTION EXECUTION
    // =========================================================================

    /// <inheritdoc/>
    public BypassAbilityActivationResult ExecuteRelock(
        string characterId,
        string lockId,
        int witsScore)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrEmpty(lockId, nameof(lockId));

        var ability = BypassSpecializationAbility.Relock();

        // Verify character has the ability
        if (!HasAbility(characterId, "relock"))
        {
            _logger.LogWarning(
                "ExecuteRelock: Character {CharacterId} does not have [Relock] ability",
                characterId);
            return BypassAbilityActivationResult.MissingSpecialization(
                "relock", BypassSpecialization.ScrapTinker);
        }

        // Verify lock was previously picked
        if (!_pickedLocks.ContainsKey(lockId))
        {
            _logger.LogWarning(
                "ExecuteRelock: Lock {LockId} was not previously picked",
                lockId);
            return BypassAbilityActivationResult.ConditionsNotMet(
                ability, "Lock must have been picked first");
        }

        // Perform WITS check
        var dicePool = DicePool.D10(witsScore);
        var rollResult = _diceService.Roll(dicePool, AdvantageType.Normal);
        var netSuccesses = rollResult.TotalSuccesses - rollResult.TotalBotches;

        _logger.LogInformation(
            "[Relock] attempt by {CharacterId} on {LockId}: WITS {Score} rolled {Net} vs DC {Dc}",
            characterId, lockId, witsScore, netSuccesses, RelockDc);

        if (netSuccesses >= RelockDc)
        {
            // Remove from picked locks tracking
            _pickedLocks.Remove(lockId);

            _logger.LogInformation(
                "[Relock] SUCCESS: {CharacterId} relocked {LockId}",
                characterId, lockId);

            var data = new Dictionary<string, object>
            {
                ["lockId"] = lockId,
                ["netSuccesses"] = netSuccesses
            };

            return BypassAbilityActivationResult.UniqueActionSuccess(
                ability,
                "With practiced fingers, you re-engage the lock's mechanism. " +
                "A satisfying click confirms the lock is secured once more.",
                data);
        }

        _logger.LogInformation(
            "[Relock] FAILED: {CharacterId} failed to relock {LockId}",
            characterId, lockId);

        return BypassAbilityActivationResult.UniqueActionFailed(
            ability,
            netSuccesses,
            "The mechanism resists your efforts. Perhaps the lock was damaged when you picked it, " +
            "or it requires a more delicate touch.");
    }

    /// <inheritdoc/>
    public BypassAbilityActivationResult ExecuteTrapArtist(
        string characterId,
        string trapId,
        int witsScore)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrEmpty(trapId, nameof(trapId));

        var ability = BypassSpecializationAbility.TrapArtist();

        // Verify character has the ability
        if (!HasAbility(characterId, "trap-artist"))
        {
            _logger.LogWarning(
                "ExecuteTrapArtist: Character {CharacterId} does not have [Trap Artist] ability",
                characterId);
            return BypassAbilityActivationResult.MissingSpecialization(
                "trap-artist", BypassSpecialization.RuinStalker);
        }

        // Verify trap was previously disabled
        if (!_disabledTraps.ContainsKey(trapId))
        {
            _logger.LogWarning(
                "ExecuteTrapArtist: Trap {TrapId} was not previously disabled",
                trapId);
            return BypassAbilityActivationResult.ConditionsNotMet(
                ability, "Trap must have been disabled first");
        }

        // Perform WITS check
        var dicePool = DicePool.D10(witsScore);
        var rollResult = _diceService.Roll(dicePool, AdvantageType.Normal);
        var netSuccesses = rollResult.TotalSuccesses - rollResult.TotalBotches;

        _logger.LogInformation(
            "[Trap Artist] attempt by {CharacterId} on {TrapId}: WITS {Score} rolled {Net} vs DC {Dc}",
            characterId, trapId, witsScore, netSuccesses, TrapArtistDc);

        if (netSuccesses >= TrapArtistDc)
        {
            // Remove from disabled traps tracking (now re-armed)
            _disabledTraps.Remove(trapId);

            _logger.LogInformation(
                "[Trap Artist] SUCCESS: {CharacterId} re-armed {TrapId}",
                characterId, trapId);

            var data = new Dictionary<string, object>
            {
                ["trapId"] = trapId,
                ["netSuccesses"] = netSuccesses,
                ["isControlledByParty"] = true
            };

            return BypassAbilityActivationResult.UniqueActionSuccess(
                ability,
                "You carefully re-arm the trap, adjusting its trigger mechanism. " +
                "The trap now serves your purposes, ready to catch any who follow.",
                data);
        }

        _logger.LogInformation(
            "[Trap Artist] FAILED: {CharacterId} failed to re-arm {TrapId}",
            characterId, trapId);

        return BypassAbilityActivationResult.UniqueActionFailed(
            ability,
            netSuccesses,
            "The trap's mechanism proves too damaged or complex to re-arm. " +
            "Perhaps it can only be triggered once.");
    }

    // =========================================================================
    // MASTERWORK CRAFTING
    // =========================================================================

    /// <inheritdoc/>
    public IReadOnlyList<MasterworkRecipe> GetAvailableMasterworkRecipes(string characterId)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));

        // Only Scrap-Tinkers have access to masterwork recipes
        if (!HasAbility(characterId, "master-craftsman"))
        {
            _logger.LogDebug(
                "GetAvailableMasterworkRecipes: Character {CharacterId} does not have [Master Craftsman]",
                characterId);
            return Array.Empty<MasterworkRecipe>();
        }

        var recipes = MasterworkRecipe.GetAll();

        _logger.LogDebug(
            "GetAvailableMasterworkRecipes: Character {CharacterId} has access to {Count} masterwork recipes",
            characterId, recipes.Count);

        return recipes;
    }

    /// <inheritdoc/>
    public bool CanCraftMasterwork(
        string characterId,
        string recipeId,
        IEnumerable<string> availableComponents)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrEmpty(recipeId, nameof(recipeId));
        ArgumentNullException.ThrowIfNull(availableComponents, nameof(availableComponents));

        // Check ability
        if (!HasAbility(characterId, "master-craftsman"))
        {
            _logger.LogDebug(
                "CanCraftMasterwork: Character {CharacterId} does not have [Master Craftsman]",
                characterId);
            return false;
        }

        // Check recipe exists
        var recipe = MasterworkRecipe.GetById(recipeId);
        if (!recipe.HasValue)
        {
            _logger.LogWarning(
                "CanCraftMasterwork: Recipe {RecipeId} not found",
                recipeId);
            return false;
        }

        // Check components
        var componentsList = availableComponents.ToList();
        var hasAllComponents = recipe.Value.RequiredComponents.All(
            required => componentsList.Any(
                available => available.Equals(required, StringComparison.OrdinalIgnoreCase)));

        _logger.LogDebug(
            "CanCraftMasterwork: Character {CharacterId} {Can} craft {RecipeId} (has components: {Has})",
            characterId, hasAllComponents ? "can" : "cannot", recipeId, hasAllComponents);

        return hasAllComponents;
    }

    // =========================================================================
    // CHARACTER REGISTRATION
    // =========================================================================

    /// <inheritdoc/>
    public void RegisterCharacterSpecialization(string characterId, BypassSpecialization specialization)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));

        _characterSpecializations[characterId] = specialization;

        _logger.LogInformation(
            "RegisterCharacterSpecialization: Character {CharacterId} registered as {Specialization}",
            characterId, specialization);
    }

    /// <inheritdoc/>
    public void UnregisterCharacter(string characterId)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterId, nameof(characterId));

        var removed = _characterSpecializations.Remove(characterId);

        if (removed)
        {
            _logger.LogInformation(
                "UnregisterCharacter: Character {CharacterId} unregistered",
                characterId);
        }
        else
        {
            _logger.LogDebug(
                "UnregisterCharacter: Character {CharacterId} was not registered",
                characterId);
        }
    }

    // =========================================================================
    // TEST HELPER METHODS
    // =========================================================================

    /// <summary>
    /// Marks a lock as picked for [Relock] validation (for testing).
    /// </summary>
    /// <param name="lockId">The lock's unique identifier.</param>
    /// <param name="characterId">The character who picked it.</param>
    public void MarkLockAsPicked(string lockId, string characterId)
    {
        _pickedLocks[lockId] = characterId;
        _logger.LogDebug(
            "MarkLockAsPicked: Lock {LockId} marked as picked by {CharacterId}",
            lockId, characterId);
    }

    /// <summary>
    /// Marks a trap as disabled for [Trap Artist] validation (for testing).
    /// </summary>
    /// <param name="trapId">The trap's unique identifier.</param>
    /// <param name="characterId">The character who disabled it.</param>
    public void MarkTrapAsDisabled(string trapId, string characterId)
    {
        _disabledTraps[trapId] = characterId;
        _logger.LogDebug(
            "MarkTrapAsDisabled: Trap {TrapId} marked as disabled by {CharacterId}",
            trapId, characterId);
    }

    // =========================================================================
    // PRIVATE METHODS
    // =========================================================================

    /// <summary>
    /// Tracks a successful bypass for unique action validation.
    /// </summary>
    private void TrackSuccessfulBypass(string characterId, BypassType bypassType, string targetId)
    {
        switch (bypassType)
        {
            case BypassType.Lockpicking:
                _pickedLocks[targetId] = characterId;
                _logger.LogDebug(
                    "TrackSuccessfulBypass: Lock {TargetId} picked by {CharacterId}",
                    targetId, characterId);
                break;

            case BypassType.TrapDisarmament:
                _disabledTraps[targetId] = characterId;
                _logger.LogDebug(
                    "TrackSuccessfulBypass: Trap {TargetId} disabled by {CharacterId}",
                    targetId, characterId);
                break;
        }
    }
}
