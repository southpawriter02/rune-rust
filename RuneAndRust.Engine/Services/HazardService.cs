using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements dynamic hazard processing (v0.3.3a).
/// Handles hazard triggers, effect execution via EffectScriptExecutor, and lifecycle state management.
/// </summary>
/// <remarks>See: SPEC-HAZARD-001 for Dynamic Hazard System design.</remarks>
public class HazardService : IHazardService
{
    private readonly IInteractableObjectRepository _objectRepository;
    private readonly EffectScriptExecutor _scriptExecutor;
    private readonly ILogger<HazardService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HazardService"/> class.
    /// </summary>
    /// <param name="objectRepository">Repository for interactable objects (including hazards).</param>
    /// <param name="scriptExecutor">Shared utility for effect script execution.</param>
    /// <param name="logger">Logger for traceability.</param>
    public HazardService(
        IInteractableObjectRepository objectRepository,
        EffectScriptExecutor scriptExecutor,
        ILogger<HazardService> logger)
    {
        _objectRepository = objectRepository;
        _scriptExecutor = scriptExecutor;
        _logger = logger;

        _logger.LogInformation("HazardService initialized (v0.3.3a)");
    }

    /// <inheritdoc/>
    public async Task<List<HazardResult>> TriggerOnRoomEnterAsync(Room room, Combatant? entrant = null)
    {
        _logger.LogDebug("[Hazard] Checking room {Room} for movement-triggered hazards", room.Name);

        var results = new List<HazardResult>();
        var hazards = await GetActiveHazardsAsync(room.Id);

        foreach (var hazard in hazards.Where(h => h.Trigger == TriggerType.Movement))
        {
            if (hazard.State != HazardState.Dormant)
            {
                _logger.LogTrace("[Hazard] {Hazard} skipped: state is {State}", hazard.Name, hazard.State);
                continue;
            }

            _logger.LogInformation(
                "[Hazard] [{Hazard}] triggered by movement in {Room}",
                hazard.Name, room.Name);

            var result = await ActivateHazardAsync(hazard, entrant);
            results.Add(result);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<List<HazardResult>> TriggerOnDamageAsync(
        Room room,
        DamageType damageType,
        int amount,
        Combatant? target = null)
    {
        _logger.LogDebug(
            "[Hazard] Checking room {Room} for damage-triggered hazards ({Type}, {Amount})",
            room.Name, damageType, amount);

        var results = new List<HazardResult>();
        var hazards = await GetActiveHazardsAsync(room.Id);

        foreach (var hazard in hazards.Where(h => h.Trigger == TriggerType.DamageTaken))
        {
            if (hazard.State != HazardState.Dormant)
            {
                _logger.LogTrace("[Hazard] {Hazard} skipped: state is {State}", hazard.Name, hazard.State);
                continue;
            }

            // Check damage type filter
            if (hazard.RequiredDamageType.HasValue && hazard.RequiredDamageType != damageType)
            {
                _logger.LogTrace(
                    "[Hazard] {Hazard} skipped: requires {Required}, got {Actual}",
                    hazard.Name, hazard.RequiredDamageType, damageType);
                continue;
            }

            // Check damage threshold
            if (amount < hazard.DamageThreshold)
            {
                _logger.LogTrace(
                    "[Hazard] {Hazard} skipped: damage {Amount} below threshold {Threshold}",
                    hazard.Name, amount, hazard.DamageThreshold);
                continue;
            }

            _logger.LogInformation(
                "[Hazard] [{Hazard}] triggered by {DamageType} damage ({Amount})",
                hazard.Name, damageType, amount);

            var result = await ActivateHazardAsync(hazard, target);
            results.Add(result);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<List<HazardResult>> ProcessTurnStartHazardsAsync(Room room, List<Combatant> combatants)
    {
        _logger.LogDebug("[Hazard] Processing turn-start hazards in {Room}", room.Name);

        var results = new List<HazardResult>();
        var hazards = await GetActiveHazardsAsync(room.Id);

        foreach (var hazard in hazards.Where(h => h.Trigger == TriggerType.TurnStart))
        {
            if (hazard.State != HazardState.Dormant)
            {
                _logger.LogTrace("[Hazard] {Hazard} skipped: state is {State}", hazard.Name, hazard.State);
                continue;
            }

            _logger.LogInformation("[Hazard] [{Hazard}] triggered at turn start", hazard.Name);

            // Apply to all combatants in the room
            foreach (var combatant in combatants)
            {
                var result = await ActivateHazardAsync(hazard, combatant);
                results.Add(result);
            }
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task TickCooldownsAsync(Room room)
    {
        var hazards = await GetActiveHazardsAsync(room.Id);
        var cooldownHazards = hazards.Where(h => h.State == HazardState.Cooldown).ToList();

        if (cooldownHazards.Count == 0)
        {
            return;
        }

        _logger.LogDebug(
            "[Hazard] Ticking cooldowns for {Count} hazards in {Room}",
            cooldownHazards.Count, room.Name);

        foreach (var hazard in cooldownHazards)
        {
            hazard.CooldownRemaining--;

            if (hazard.CooldownRemaining <= 0)
            {
                hazard.State = HazardState.Dormant;
                hazard.CooldownRemaining = 0;
                _logger.LogTrace(
                    "[Hazard] [{Hazard}] cooldown complete, returning to Dormant",
                    hazard.Name);
            }
            else
            {
                _logger.LogTrace(
                    "[Hazard] [{Hazard}] cooldown: {Remaining} turns remaining",
                    hazard.Name, hazard.CooldownRemaining);
            }

            await _objectRepository.UpdateAsync(hazard);
        }
    }

    /// <inheritdoc/>
    public async Task<List<DynamicHazard>> GetActiveHazardsAsync(Guid roomId)
    {
        var allObjects = await _objectRepository.GetByRoomIdAsync(roomId);

        var hazards = allObjects
            .OfType<DynamicHazard>()
            .Where(h => h.State != HazardState.Destroyed)
            .ToList();

        _logger.LogTrace("[Hazard] Found {Count} active hazards in room {RoomId}", hazards.Count, roomId);

        return hazards;
    }

    /// <inheritdoc/>
    public async Task<HazardResult> ManualActivateAsync(DynamicHazard hazard, Combatant activator)
    {
        if (hazard.Trigger != TriggerType.ManualInteraction)
        {
            _logger.LogWarning(
                "[Hazard] {Hazard} cannot be manually activated (trigger type: {Type})",
                hazard.Name, hazard.Trigger);
            return HazardResult.None;
        }

        if (hazard.State != HazardState.Dormant)
        {
            _logger.LogDebug(
                "[Hazard] {Hazard} cannot be activated: state is {State}",
                hazard.Name, hazard.State);
            return HazardResult.None;
        }

        _logger.LogInformation(
            "[Hazard] [{Hazard}] manually activated by {Activator}",
            hazard.Name, activator.Name);

        return await ActivateHazardAsync(hazard, activator);
    }

    #region Private Methods

    /// <summary>
    /// Activates a hazard, executing its effect script and updating its state.
    /// </summary>
    private async Task<HazardResult> ActivateHazardAsync(DynamicHazard hazard, Combatant? target)
    {
        hazard.State = HazardState.Triggered;

        // Build trigger message
        var message = !string.IsNullOrEmpty(hazard.TriggerMessage)
            ? hazard.TriggerMessage
            : GetDefaultTriggerMessage(hazard);

        // Execute effect script if we have a target
        int totalDamage = 0;
        int totalHealing = 0;
        var statusesApplied = new List<string>();

        if (target != null && !string.IsNullOrWhiteSpace(hazard.EffectScript))
        {
            _logger.LogDebug(
                "[Hazard] [{Hazard}] executing script: {Script}",
                hazard.Name, hazard.EffectScript);

            var scriptResult = _scriptExecutor.Execute(
                hazard.EffectScript,
                target,
                hazard.Name,
                hazard.Id);

            totalDamage = scriptResult.TotalDamage;
            totalHealing = scriptResult.TotalHealing;
            statusesApplied = scriptResult.StatusesApplied;

            if (!string.IsNullOrEmpty(scriptResult.Narrative))
            {
                message = $"{message} {scriptResult.Narrative}";
            }

            _logger.LogDebug(
                "[Hazard] [{Hazard}] executed. Damage: {Damage}, Healing: {Healing}, Statuses: [{Statuses}]",
                hazard.Name, totalDamage, totalHealing, string.Join(", ", statusesApplied));
        }

        // Update state based on one-time use
        if (hazard.OneTimeUse)
        {
            hazard.State = HazardState.Destroyed;
            _logger.LogDebug("[Hazard] [{Hazard}] destroyed after one-time trigger", hazard.Name);
        }
        else
        {
            hazard.State = HazardState.Cooldown;
            hazard.CooldownRemaining = hazard.MaxCooldown;
            _logger.LogDebug(
                "[Hazard] [{Hazard}] entering cooldown: {Cooldown} turns",
                hazard.Name, hazard.MaxCooldown);
        }

        await _objectRepository.UpdateAsync(hazard);

        return new HazardResult(
            WasTriggered: true,
            HazardName: hazard.Name,
            Message: message,
            TotalDamage: totalDamage,
            TotalHealing: totalHealing,
            StatusesApplied: statusesApplied,
            NewState: hazard.State);
    }

    /// <summary>
    /// Gets a default trigger message based on hazard type.
    /// </summary>
    private static string GetDefaultTriggerMessage(DynamicHazard hazard)
    {
        return hazard.HazardType switch
        {
            HazardType.Mechanical => $"The {hazard.Name} activates with a grinding sound!",
            HazardType.Environmental => $"The {hazard.Name} erupts violently!",
            HazardType.Biological => $"The {hazard.Name} releases a noxious burst!",
            _ => $"The {hazard.Name} triggers!"
        };
    }

    #endregion
}
