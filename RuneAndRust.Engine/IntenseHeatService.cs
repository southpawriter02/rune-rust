using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.29.2: Intense Heat Service
/// Handles [Intense Heat] ambient condition for Muspelheim biome.
/// Processes STURDINESS Resolve checks and Fire damage application.
///
/// Responsibilities:
/// - End-of-turn STURDINESS checks (DC 12)
/// - Fire damage calculation (2d6 on failure)
/// - Fire Resistance application
/// - Biome statistics tracking
/// </summary>
public class IntenseHeatService
{
    private static readonly ILogger _log = Log.ForContext<IntenseHeatService>();
    private readonly DiceService _diceService;
    private readonly ResolveCheckService _resolveCheckService;
    private readonly TraumaEconomyService _traumaService;

    /// <summary>
    /// v2.0 canonical: STURDINESS check DC for [Intense Heat]
    /// </summary>
    private const int INTENSE_HEAT_DC = 12;

    /// <summary>
    /// v2.0 canonical: Damage dice on check failure (2d6)
    /// </summary>
    private const int DAMAGE_DICE_COUNT = 2;
    private const int DAMAGE_DIE_SIZE = 6;

    public IntenseHeatService(
        DiceService diceService,
        ResolveCheckService resolveCheckService,
        TraumaEconomyService traumaService)
    {
        _diceService = diceService;
        _resolveCheckService = resolveCheckService;
        _traumaService = traumaService;
    }

    /// <summary>
    /// Process [Intense Heat] checks for all combatants at end of turn.
    /// Called by combat system when in Muspelheim biome.
    /// </summary>
    public List<HazardResult> ProcessEndOfTurnHeat(List<PlayerCharacter> characters, Room room)
    {
        if (!room.HasAmbientCondition("[Intense Heat]") && !room.HasAmbientCondition("Intense Heat"))
        {
            return new List<HazardResult>();
        }

        _log.Information("[Intense Heat] processing for {Count} characters in {Room}",
            characters.Count, room.Name);

        var results = new List<HazardResult>();

        foreach (var character in characters)
        {
            var result = ProcessHeatCheckForCharacter(character);
            if (result.WasTriggered)
            {
                results.Add(result);
            }
        }

        return results;
    }

    /// <summary>
    /// Process heat check for a single character.
    /// </summary>
    private HazardResult ProcessHeatCheckForCharacter(PlayerCharacter character)
    {
        var result = new HazardResult
        {
            WasTriggered = true,
            AffectedCharacters = new List<int> { character.CharacterId },
            HazardName = "[Intense Heat]"
        };

        // Make STURDINESS Resolve check
        var checkResult = MakeSturdynessCheck(character, INTENSE_HEAT_DC);

        if (checkResult.success)
        {
            // Success: No damage
            result.LogMessage = $"✓ {character.Name} resists [Intense Heat] (STURDINESS check passed)";
            result.DamageDealt = 0;

            _log.Information("{Character} passed [Intense Heat] check: {Successes} successes",
                character.Name, checkResult.successes);

            return result;
        }

        // Failure: Roll Fire damage
        int rawDamage = _diceService.RollDamage(DAMAGE_DICE_COUNT, DAMAGE_DIE_SIZE);

        _log.Information("{Character} failed [Intense Heat] check, raw damage: {RawDamage}",
            character.Name, rawDamage);

        // Apply Fire Resistance
        int finalDamage = ApplyFireResistance(character, rawDamage);

        // Apply damage to character
        character.HP = Math.Max(0, character.HP - finalDamage);

        result.DamageDealt = finalDamage;
        result.LogMessage = BuildHeatDamageMessage(character, checkResult.successes, rawDamage, finalDamage);

        // Track death if applicable
        if (character.HP <= 0)
        {
            _log.Warning("{Character} died from [Intense Heat]", character.Name);
            result.LogMessage += $"\n💀 {character.Name} has succumbed to the [Intense Heat]!";
        }

        _log.Information("{Character} takes {FinalDamage} Fire damage from [Intense Heat] (HP: {HP}/{MaxHP})",
            character.Name, finalDamage, character.HP, character.MaxHP);

        return result;
    }

    /// <summary>
    /// Make a STURDINESS Resolve check.
    /// Reuses existing WILL check pattern but with STURDINESS attribute.
    /// </summary>
    private (bool success, int successes, string rollDetails) MakeSturdynessCheck(PlayerCharacter character, int dc)
    {
        int sturdiness = character.GetAttributeValue("STURDINESS");

        // Roll STURDINESS dice
        var rollResult = _diceService.Roll(sturdiness);

        bool success = rollResult.Successes >= dc;

        string rollDetails = $"STURDINESS Resolve Check (DC {dc}): " +
                             $"Rolled {sturdiness} dice → [{string.Join(", ", rollResult.Rolls)}] " +
                             $"→ {rollResult.Successes} successes";

        return (success, rollResult.Successes, rollDetails);
    }

    /// <summary>
    /// Apply Fire Resistance to raw damage.
    /// </summary>
    public int ApplyFireResistance(PlayerCharacter character, int rawDamage)
    {
        // Get Fire Resistance percentage (0-100)
        // Note: This assumes there's a resistance system in place
        // For now, we'll use a placeholder that checks if character has fire resistance abilities
        int resistancePercent = GetFireResistancePercent(character);

        if (resistancePercent <= 0)
        {
            return rawDamage;
        }

        if (resistancePercent >= 100)
        {
            _log.Information("{Character} is immune to Fire damage (100% resistance)", character.Name);
            return 0;
        }

        // Calculate reduced damage
        int reducedDamage = rawDamage - (rawDamage * resistancePercent / 100);

        _log.Information("Fire Resistance {Percent}%: {RawDamage} reduced to {FinalDamage}",
            resistancePercent, rawDamage, reducedDamage);

        return reducedDamage;
    }

    /// <summary>
    /// Get Fire Resistance percentage for a character.
    /// TODO: Implement proper resistance system in future version.
    /// </summary>
    private int GetFireResistancePercent(PlayerCharacter character)
    {
        // Placeholder: Check for fire resistance abilities or equipment
        // This would integrate with equipment/ability system when implemented

        // Check if character has any fire resistance from abilities
        // (This is a simplified check - full implementation would check equipment, buffs, etc.)
        int resistance = 0;

        // Example: Check if character has certain abilities that grant fire resistance
        // This would be expanded based on the actual ability/equipment system

        return resistance;
    }

    /// <summary>
    /// Build flavor message for heat damage.
    /// </summary>
    private string BuildHeatDamageMessage(PlayerCharacter character, int successes, int rawDamage, int finalDamage)
    {
        string message = $"✗ {character.Name} fails [Intense Heat] check ({successes} successes)\n";
        message += $"   🔥 The searing heat overwhelms your defenses!\n";
        message += $"   Damage: {DAMAGE_DICE_COUNT}d{DAMAGE_DIE_SIZE} = {rawDamage}";

        if (rawDamage != finalDamage)
        {
            int resistancePercent = GetFireResistancePercent(character);
            message += $" → {finalDamage} (Fire Resistance {resistancePercent}%)";
        }

        message += $"\n   HP: {Math.Max(0, character.HP)}/{character.MaxHP}";

        return message;
    }

    /// <summary>
    /// Check if character has sufficient Fire Resistance for Muspelheim.
    /// Recommendation: 50%+ for comfortable exploration.
    /// </summary>
    public bool HasSufficientFireResistance(PlayerCharacter character, int threshold = 50)
    {
        int resistancePercent = GetFireResistancePercent(character);
        bool sufficient = resistancePercent >= threshold;

        _log.Information("{Character} Fire Resistance: {Percent}% ({Status})",
            character.Name, resistancePercent, sufficient ? "Sufficient" : "Insufficient");

        return sufficient;
    }

    /// <summary>
    /// Get flavor text for heat resistance status.
    /// </summary>
    public string GetFireResistanceStatusMessage(PlayerCharacter character)
    {
        int resistance = GetFireResistancePercent(character);

        return resistance switch
        {
            >= 75 => $"{character.Name} is well-protected against the heat (Fire Resistance: {resistance}%)",
            >= 50 => $"{character.Name} has adequate heat protection (Fire Resistance: {resistance}%)",
            >= 25 => $"{character.Name} has minimal heat protection (Fire Resistance: {resistance}%)",
            _ => $"⚠️ {character.Name} has NO fire resistance! Muspelheim will be extremely dangerous!"
        };
    }
}
