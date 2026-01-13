using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using System.Text;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing environmental hazard zones and their effects.
/// </summary>
/// <remarks>
/// <para>
/// HazardZoneService handles all aspects of environmental hazard processing:
/// </para>
/// <list type="bullet">
///   <item><description>Entry damage when players enter rooms with active hazards</description></item>
///   <item><description>Per-turn damage application during turn processing</description></item>
///   <item><description>Saving throw rolls using 1d20 + attribute modifier vs DC</description></item>
///   <item><description>Status effect application on failed saves</description></item>
///   <item><description>Duration countdown for temporary hazards</description></item>
/// </list>
/// <para>
/// This service is distinct from <see cref="HazardService"/> which handles BiomeHazard entities.
/// HazardZoneService specifically handles the v0.4.1c Environmental Hazards system.
/// </para>
/// </remarks>
public class HazardZoneService : IHazardZoneService
{
    private readonly ILogger<HazardZoneService> _logger;
    private readonly IDiceService _diceService;

    /// <summary>
    /// Initializes a new instance of the HazardZoneService.
    /// </summary>
    /// <param name="logger">Logger for hazard operations.</param>
    /// <param name="diceService">Service for dice rolling.</param>
    public HazardZoneService(
        ILogger<HazardZoneService> logger,
        IDiceService diceService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));

        _logger.LogDebug("HazardZoneService initialized");
    }

    /// <inheritdoc/>
    public string GetRoomHazardsDescription(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var activeHazards = room.GetActiveHazards().ToList();
        if (activeHazards.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("[ENVIRONMENTAL HAZARDS]");

        foreach (var hazard in activeHazards)
        {
            sb.AppendLine($"  • {hazard.Name}: {hazard.GetHazardTypeDescription()}");
        }

        _logger.LogDebug("Generated hazard description for room {RoomName} with {Count} active hazards",
            room.Name, activeHazards.Count);

        return sb.ToString().TrimEnd();
    }

    /// <inheritdoc/>
    public HazardZone? FindHazard(Room room, string keyword)
    {
        ArgumentNullException.ThrowIfNull(room);

        if (string.IsNullOrWhiteSpace(keyword))
            return null;

        var hazard = room.GetHazardByKeyword(keyword);

        if (hazard != null)
        {
            _logger.LogDebug("Found hazard {HazardName} by keyword '{Keyword}'",
                hazard.Name, keyword);
        }

        return hazard;
    }

    /// <inheritdoc/>
    public string ExamineHazard(HazardZone hazard)
    {
        ArgumentNullException.ThrowIfNull(hazard);

        var sb = new StringBuilder();
        sb.AppendLine($"=== {hazard.Name} ===");
        sb.AppendLine(hazard.Description);
        sb.AppendLine();
        sb.AppendLine($"Type: {hazard.HazardType}");
        sb.AppendLine($"Status: {(hazard.IsActive ? "Active" : "Inactive")}");

        if (hazard.DamagePerTurn)
        {
            sb.AppendLine($"Damage per turn: {hazard.DamagePerTurnDice} {hazard.DamageType}");
        }

        if (hazard.DamageOnEntry)
        {
            sb.AppendLine($"Entry damage: {hazard.EntryDamageDice} {hazard.DamageType}");
        }

        if (hazard.AppliesStatus)
        {
            sb.AppendLine($"Status effects: {string.Join(", ", hazard.StatusEffects)}");
        }

        if (hazard.HasSave)
        {
            var save = hazard.Save!.Value;
            sb.AppendLine($"Saving throw: {save.Attribute} DC {save.DC} ({(save.Negates ? "negates" : "halves")})");
        }

        if (hazard.IsPermanent)
        {
            sb.AppendLine("Duration: Permanent");
        }
        else
        {
            sb.AppendLine($"Duration: {hazard.Duration} turn(s) remaining");
        }

        return sb.ToString().TrimEnd();
    }

    /// <inheritdoc/>
    public IEnumerable<HazardEffectResult> ProcessEntryHazards(Room room, Player player)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(player);

        var results = new List<HazardEffectResult>();
        var activeHazards = room.GetActiveHazards().Where(h => h.DamageOnEntry).ToList();

        if (activeHazards.Count == 0)
            return results;

        _logger.LogDebug("Processing {Count} entry hazards in room {RoomName}",
            activeHazards.Count, room.Name);

        foreach (var hazard in activeHazards)
        {
            var result = ApplyHazardEffect(hazard, player, isEntry: true);
            if (result.DamageDealt > 0 || result.StatusEffectsApplied.Count > 0 || result.WasNegated)
            {
                results.Add(result);
            }
        }

        return results;
    }

    /// <inheritdoc/>
    public IEnumerable<HazardEffectResult> ProcessTurnHazards(Room room, Player player)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(player);

        var results = new List<HazardEffectResult>();
        var activeHazards = room.GetActiveHazards()
            .Where(h => h.DamagePerTurn || h.AppliesStatus)
            .ToList();

        if (activeHazards.Count == 0)
            return results;

        _logger.LogDebug("Processing {Count} turn hazards in room {RoomName}",
            activeHazards.Count, room.Name);

        foreach (var hazard in activeHazards)
        {
            var result = ApplyHazardEffect(hazard, player, isEntry: false);
            if (result.DamageDealt > 0 || result.StatusEffectsApplied.Count > 0 || result.WasNegated)
            {
                results.Add(result);
            }
        }

        return results;
    }

    /// <inheritdoc/>
    public HazardEffectResult ApplyHazardEffect(HazardZone hazard, Player player, bool isEntry)
    {
        ArgumentNullException.ThrowIfNull(hazard);
        ArgumentNullException.ThrowIfNull(player);

        if (!hazard.IsActive)
        {
            return HazardEffectResult.NoEffect(hazard.Name);
        }

        // Determine which damage dice to use
        var damageDice = isEntry ? hazard.EntryDamageDice : hazard.DamagePerTurnDice;

        // If no damage and no status effects, nothing to do
        if (string.IsNullOrEmpty(damageDice) && !hazard.AppliesStatus)
        {
            return HazardEffectResult.NoEffect(hazard.Name);
        }

        var messageBuilder = new StringBuilder();
        var totalDamage = 0;
        var damageRolls = new List<int>();
        var appliedEffects = new List<string>();
        var saveAttempted = false;
        var saveSucceeded = false;
        int? saveRoll = null;
        int? saveDC = null;
        string? saveAttribute = null;

        // Build effect message header
        messageBuilder.AppendLine($"[ENVIRONMENTAL HAZARD: {hazard.Name}]");
        messageBuilder.AppendLine(hazard.EffectMessage ?? hazard.GetHazardTypeDescription() + "!");

        // Process saving throw if the hazard has one
        if (hazard.HasSave)
        {
            saveAttempted = true;
            var save = hazard.Save!.Value;
            var saveResult = PerformSavingThrow(player, save);
            saveRoll = saveResult.Total;
            saveDC = save.DC;
            saveAttribute = save.Attribute;
            saveSucceeded = saveResult.Success;

            messageBuilder.AppendLine();
            messageBuilder.Append($"Saving Throw ({save.Attribute}): ");
            messageBuilder.Append($"[{string.Join(",", saveResult.Rolls)}] + {saveResult.Modifier} = {saveResult.Total}");
            messageBuilder.AppendLine($" vs DC {save.DC}");

            if (saveSucceeded)
            {
                if (save.Negates)
                {
                    messageBuilder.AppendLine("Save successful! You avoid the hazard's effects.");

                    _logger.LogInformation(
                        "Player saved against {HazardName} (rolled {Roll} vs DC {DC})",
                        hazard.Name, saveResult.Total, save.DC);

                    return HazardEffectResult.Negated(
                        hazard.Name,
                        saveResult.Total,
                        save.DC,
                        save.Attribute,
                        messageBuilder.ToString().TrimEnd());
                }

                messageBuilder.AppendLine("Save successful! Damage halved.");
                _logger.LogInformation(
                    "Player saved against {HazardName} (rolled {Roll} vs DC {DC})",
                    hazard.Name, saveResult.Total, save.DC);
            }
            else
            {
                messageBuilder.AppendLine("Save failed!");
                _logger.LogInformation(
                    "Player failed save against {HazardName} (rolled {Roll} vs DC {DC})",
                    hazard.Name, saveResult.Total, save.DC);
            }
        }

        // Apply damage if present
        if (!string.IsNullOrEmpty(damageDice))
        {
            var damageResult = _diceService.Roll(damageDice);
            damageRolls.AddRange(damageResult.Rolls);
            totalDamage = damageResult.Total;

            // Halve damage if save succeeded and doesn't negate
            if (saveSucceeded && hazard.HasSave && !hazard.Save!.Value.Negates)
            {
                totalDamage = totalDamage / 2;
            }

            // Apply damage to player
            if (totalDamage > 0)
            {
                player.TakeDamage(totalDamage);

                messageBuilder.AppendLine();
                var damageInfo = saveSucceeded && hazard.HasSave && !hazard.Save!.Value.Negates
                    ? $"[{string.Join(",", damageResult.Rolls)}] = {damageResult.Total} / 2 = {totalDamage}"
                    : $"[{string.Join(",", damageResult.Rolls)}] = {totalDamage}";

                messageBuilder.AppendLine($"Damage: {damageInfo} {hazard.DamageType} damage");

                _logger.LogInformation(
                    "Hazard {HazardName} dealt {Damage} {DamageType} damage to {PlayerName}",
                    hazard.Name, totalDamage, hazard.DamageType, player.Name);
            }
        }

        // Apply status effects only on failed saves
        if (hazard.AppliesStatus && !saveSucceeded)
        {
            foreach (var statusEffect in hazard.StatusEffects)
            {
                // Note: Full StatusEffectService integration requires Player to implement IEffectTarget
                // For now, we track applied effects for messaging but don't apply them via service
                // TODO: Implement when Player.ApplyStatusEffect method is added
                appliedEffects.Add(statusEffect);
            }

            if (appliedEffects.Count > 0)
            {
                messageBuilder.AppendLine();
                messageBuilder.AppendLine($"You are afflicted with: {string.Join(", ", appliedEffects)}");

                _logger.LogInformation(
                    "Hazard {HazardName} applied {Effects} to player",
                    hazard.Name, string.Join(", ", appliedEffects));
            }
        }

        return HazardEffectResult.Combined(
            hazard.Name,
            totalDamage,
            hazard.DamageType,
            damageRolls,
            appliedEffects,
            messageBuilder.ToString().TrimEnd(),
            saveAttempted,
            saveSucceeded,
            saveRoll,
            saveDC,
            saveAttribute);
    }

    /// <inheritdoc/>
    public IEnumerable<string> ProcessHazardTurnTicks(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var messages = new List<string>();
        var hazards = room.HazardZones.ToList();

        foreach (var hazard in hazards)
        {
            if (hazard.ProcessTurnTick())
            {
                messages.Add($"The {hazard.Name} dissipates.");

                _logger.LogDebug(
                    "Hazard {HazardName} in room {RoomName} expired after duration countdown",
                    hazard.Name, room.Name);
            }
        }

        // Clean up expired hazards
        var removed = room.RemoveExpiredHazards();
        if (removed > 0)
        {
            _logger.LogDebug("Removed {Count} expired hazards from room {RoomName}",
                removed, room.Name);
        }

        return messages;
    }

    /// <inheritdoc/>
    public HazardRoomSummary GetRoomHazardSummary(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var hazards = room.HazardZones.ToList();
        var active = hazards.Count(h => h.IsActive);
        var permanent = hazards.Count(h => h.IsPermanent);
        var temporary = hazards.Count(h => !h.IsPermanent && h.IsActive);
        var expired = hazards.Count(h => h.IsExpired);

        return new HazardRoomSummary(
            Total: hazards.Count,
            Active: active,
            Permanent: permanent,
            Temporary: temporary,
            Expired: expired);
    }

    /// <inheritdoc/>
    public SavingThrowResult PerformSavingThrow(Player player, SavingThrow save)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Get the attribute modifier from the player
        var modifier = GetAttributeModifier(player, save.Attribute);

        // Roll 1d20
        var roll = _diceService.Roll("1d20");
        var total = roll.Total + modifier;

        _logger.LogDebug(
            "Saving throw {Attribute}: rolled {Roll} + {Modifier} = {Total} vs DC {DC}",
            save.Attribute, roll.Total, modifier, total, save.DC);

        return new SavingThrowResult(
            Total: total,
            Rolls: roll.Rolls.ToList(),
            Modifier: modifier,
            DC: save.DC,
            Success: total >= save.DC,
            Attribute: save.Attribute);
    }

    /// <summary>
    /// Gets the modifier for a saving throw attribute from the player.
    /// </summary>
    /// <param name="player">The player to get the modifier from.</param>
    /// <param name="attribute">The attribute name.</param>
    /// <returns>The modifier value.</returns>
    private static int GetAttributeModifier(Player player, string attribute)
    {
        // Map save attributes to player stats
        // This is a simplified implementation - adjust based on actual Player API
        return attribute.ToLowerInvariant() switch
        {
            "fortitude" => player.Stats.Defense / 2,    // Use defense as a proxy
            "agility" => player.Stats.Attack / 3,       // Use a portion of attack
            "will" => 0,                                 // Default modifier
            _ => 0
        };
    }
}
