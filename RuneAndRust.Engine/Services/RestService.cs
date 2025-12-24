using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for handling rest and recovery mechanics.
/// Manages resource consumption, recovery formulas, and the Exhausted status effect.
/// Integrates with IAmbushService for wilderness rest danger mechanics (v0.3.2b).
/// </summary>
/// <remarks>See: SPEC-REST-001 for Rest & Recovery System design.</remarks>
public class RestService : IRestService
{
    private readonly IInventoryService _inventoryService;
    private readonly IAmbushService _ambushService;
    private readonly ILogger<RestService> _logger;

    /// <summary>
    /// Tag used to identify ration items in inventory.
    /// </summary>
    private const string RationTag = "Ration";

    /// <summary>
    /// Tag used to identify water items in inventory.
    /// </summary>
    private const string WaterTag = "Water";

    /// <summary>
    /// Base HP recovery for wilderness rest (before Sturdiness bonus).
    /// </summary>
    private const int BaseHpRecovery = 10;

    /// <summary>
    /// HP recovery bonus per point of Sturdiness.
    /// </summary>
    private const int HpRecoveryPerSturdiness = 2;

    /// <summary>
    /// Stress reduction per point of Willpower.
    /// </summary>
    private const int StressReductionPerWill = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestService"/> class.
    /// </summary>
    /// <param name="inventoryService">The inventory service for supply management.</param>
    /// <param name="ambushService">The ambush service for danger calculation.</param>
    /// <param name="logger">The logger for traceability.</param>
    public RestService(
        IInventoryService inventoryService,
        IAmbushService ambushService,
        ILogger<RestService> logger)
    {
        _inventoryService = inventoryService;
        _ambushService = ambushService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<RestResult> PerformRestAsync(Character character, RestType type)
    {
        _logger.LogInformation("{Name} initiating {Type} rest.", character.Name, type);

        if (type == RestType.Sanctuary)
        {
            return await PerformSanctuaryRestAsync(character);
        }
        else
        {
            return await PerformWildernessRestAsync(character);
        }
    }

    /// <inheritdoc/>
    public async Task<RestResult> PerformRestAsync(Character character, RestType type, Room room)
    {
        _logger.LogInformation("{Name} initiating {Type} rest in {Room}.", character.Name, type, room.Name);

        // Sanctuary rest - no ambush check, full recovery
        if (type == RestType.Sanctuary)
        {
            return await PerformSanctuaryRestAsync(character);
        }

        // Wilderness rest - perform ambush check first
        var ambushResult = await _ambushService.CalculateAmbushAsync(character, room);

        if (ambushResult.IsAmbush)
        {
            // Ambush triggered - consume supplies but no recovery
            return await PerformAmbushedRestAsync(character, ambushResult);
        }

        // No ambush - proceed with normal wilderness rest
        var restResult = await PerformWildernessRestAsync(character);

        // Return result with ambush details attached
        return restResult with { AmbushDetails = ambushResult };
    }

    /// <inheritdoc/>
    public async Task<bool> HasRequiredSuppliesAsync(Character character)
    {
        var ration = await _inventoryService.FindItemByTagAsync(character, RationTag);
        var water = await _inventoryService.FindItemByTagAsync(character, WaterTag);

        _logger.LogDebug("Supply check: Ration={HasRation}, Water={HasWater}.",
            ration != null, water != null);

        return ration != null && water != null;
    }

    /// <summary>
    /// Performs a sanctuary rest with full recovery.
    /// </summary>
    private async Task<RestResult> PerformSanctuaryRestAsync(Character character)
    {
        // Record starting values
        var oldHp = character.CurrentHP;
        var oldStamina = character.CurrentStamina;
        var oldStress = character.PsychicStress;

        // Full recovery
        character.CurrentHP = character.MaxHP;
        character.CurrentStamina = character.MaxStamina;
        character.PsychicStress = 0;

        // Cure exhaustion
        if (character.HasStatusEffect(StatusEffectType.Exhausted))
        {
            character.RemoveStatusEffect(StatusEffectType.Exhausted);
            _logger.LogInformation("{Name} cured of Exhausted at Sanctuary.", character.Name);
        }

        var hpRecovered = character.CurrentHP - oldHp;
        var staminaRecovered = character.CurrentStamina - oldStamina;
        var stressRecovered = oldStress - character.PsychicStress;

        _logger.LogInformation("{Name} fully restored at Sanctuary.", character.Name);
        _logger.LogInformation("Rest complete. HP+{HP}, Stress-{Stress}. Supplies used: {Used}.",
            hpRecovered, stressRecovered, false);

        return new RestResult(
            HpRecovered: hpRecovered,
            StaminaRecovered: staminaRecovered,
            StressRecovered: stressRecovered,
            SuppliesConsumed: false,
            IsExhausted: false
        );
    }

    /// <summary>
    /// Performs a wilderness rest with partial recovery.
    /// Consumes supplies if available, applies Exhausted if not.
    /// </summary>
    private async Task<RestResult> PerformWildernessRestAsync(Character character)
    {
        // Check for supplies
        var ration = await _inventoryService.FindItemByTagAsync(character, RationTag);
        var water = await _inventoryService.FindItemByTagAsync(character, WaterTag);
        var hasSupplies = ration != null && water != null;
        var applyExhaustion = false;

        _logger.LogDebug("Supply check: Ration={HasRation}, Water={HasWater}.",
            ration != null, water != null);

        // Consume supplies or apply exhaustion
        if (hasSupplies)
        {
            // Consume 1 ration and 1 water
            await _inventoryService.RemoveItemAsync(character, ration!.Item.Name, 1);
            await _inventoryService.RemoveItemAsync(character, water!.Item.Name, 1);

            _logger.LogDebug("Consumed 1x {RationName}, 1x {WaterName}.",
                ration.Item.Name, water.Item.Name);
        }
        else
        {
            applyExhaustion = true;
            character.AddStatusEffect(StatusEffectType.Exhausted);

            _logger.LogWarning("{Name} rests without supplies. Applying [Exhausted].", character.Name);
        }

        // Record starting values
        var oldHp = character.CurrentHP;
        var oldStamina = character.CurrentStamina;
        var oldStress = character.PsychicStress;

        // Calculate recovery
        var isExhausted = character.HasStatusEffect(StatusEffectType.Exhausted);

        // HP Recovery: 10 + (Sturdiness * 2), halved if exhausted
        var hpRecoveryAmount = BaseHpRecovery + (character.Sturdiness * HpRecoveryPerSturdiness);
        if (isExhausted)
        {
            hpRecoveryAmount /= 2;
        }
        character.CurrentHP = Math.Min(character.MaxHP, character.CurrentHP + hpRecoveryAmount);

        // Stamina Recovery: Full, halved if exhausted
        if (isExhausted)
        {
            character.CurrentStamina = character.MaxStamina / 2;
        }
        else
        {
            character.CurrentStamina = character.MaxStamina;
        }

        // Stress Recovery: Will * 5, 0 if exhausted
        if (!isExhausted)
        {
            var stressReduction = character.Will * StressReductionPerWill;
            character.PsychicStress = Math.Max(0, character.PsychicStress - stressReduction);
        }
        // No stress recovery if exhausted

        var hpRecovered = character.CurrentHP - oldHp;
        var staminaRecovered = character.CurrentStamina - oldStamina;
        var stressRecovered = oldStress - character.PsychicStress;

        _logger.LogInformation("Rest complete. HP+{HP}, Stress-{Stress}. Supplies used: {Used}.",
            hpRecovered, stressRecovered, hasSupplies);

        return new RestResult(
            HpRecovered: hpRecovered,
            StaminaRecovered: staminaRecovered,
            StressRecovered: stressRecovered,
            SuppliesConsumed: hasSupplies,
            IsExhausted: applyExhaustion || isExhausted
        );
    }

    /// <summary>
    /// Handles an ambushed rest - consumes supplies but provides no recovery.
    /// Applies Disoriented status to represent being caught off-guard.
    /// </summary>
    /// <param name="character">The character who was ambushed.</param>
    /// <param name="ambushResult">The ambush result with encounter details.</param>
    /// <returns>A RestResult indicating the ambush with no recovery.</returns>
    private async Task<RestResult> PerformAmbushedRestAsync(Character character, AmbushResult ambushResult)
    {
        _logger.LogWarning("{Name} was ambushed during rest!", character.Name);

        // Check for supplies - they are consumed even on ambush (wasted)
        var ration = await _inventoryService.FindItemByTagAsync(character, RationTag);
        var water = await _inventoryService.FindItemByTagAsync(character, WaterTag);
        var hadSupplies = ration != null && water != null;

        if (hadSupplies)
        {
            // Consume supplies - they were used/scattered during the ambush
            await _inventoryService.RemoveItemAsync(character, ration!.Item.Name, 1);
            await _inventoryService.RemoveItemAsync(character, water!.Item.Name, 1);

            _logger.LogDebug("Supplies lost in ambush: 1x {RationName}, 1x {WaterName}.",
                ration.Item.Name, water.Item.Name);
        }

        // Apply Disoriented status - caught off-guard
        character.AddStatusEffect(StatusEffectType.Disoriented);
        _logger.LogInformation("{Name} is [Disoriented] from the ambush.", character.Name);

        // No recovery - rest was interrupted
        return new RestResult(
            HpRecovered: 0,
            StaminaRecovered: 0,
            StressRecovered: 0,
            SuppliesConsumed: hadSupplies,
            IsExhausted: false,
            WasAmbushed: true,
            AmbushDetails: ambushResult
        );
    }
}
