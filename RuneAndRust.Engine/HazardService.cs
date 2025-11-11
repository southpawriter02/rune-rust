using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// [v0.6] Handles environmental hazard mechanics (damage, stress, attribute checks)
/// </summary>
public class HazardService
{
    private readonly DiceService _diceService;
    private readonly TraumaEconomyService _traumaService;

    public HazardService(DiceService diceService, TraumaEconomyService traumaService)
    {
        _diceService = diceService;
        _traumaService = traumaService;
    }

    /// <summary>
    /// Process automatic hazards that deal damage/stress every turn
    /// Used for: ToxicFumes, ToxicSludge, Radiation, etc.
    /// </summary>
    /// <param name="room">Current room</param>
    /// <param name="character">Character being affected</param>
    /// <returns>Tuple of (damage dealt, stress dealt, log message)</returns>
    public (int damage, int stress, string logMessage) ProcessAutomaticHazard(Room room, PlayerCharacter character)
    {
        if (!room.HasEnvironmentalHazard || !room.IsHazardActive)
        {
            return (0, 0, string.Empty);
        }

        int damage = 0;
        int stress = 0;
        string logMessage = "";

        // Handle dice-based hazards (v0.6 enhanced system)
        if (room.HazardDamageDice > 0)
        {
            damage = _diceService.RollDamage(room.HazardDamageDice, room.HazardDamageDieSize);
            logMessage += $"💀 {room.HazardDescription}\n";
            logMessage += $"   Rolled {room.HazardDamageDice}d{room.HazardDamageDieSize} → {damage} damage!";
        }
        // Fallback to legacy flat damage (v0.4 backward compatibility)
        else if (room.HazardDamagePerTurn > 0)
        {
            damage = room.HazardDamagePerTurn;
            logMessage += $"💀 {room.HazardDescription}\n";
            logMessage += $"   You take {damage} damage!";
        }

        // Apply stress from hazard (if specified)
        if (room.HazardStressPerTurn > 0)
        {
            stress = room.HazardStressPerTurn;
            logMessage += $"\n   💀 The fumes assault your mind! +{stress} Psychic Stress!";
        }

        // Apply damage and stress
        if (damage > 0)
        {
            character.HP = Math.Max(0, character.HP - damage);
        }

        if (stress > 0)
        {
            _traumaService.AddStress(character, stress);
        }

        return (damage, stress, logMessage);
    }

    /// <summary>
    /// Process check-based hazards that require an attribute check to avoid
    /// Used for: UnstableFlooring (FINESSE check), etc.
    /// </summary>
    /// <param name="room">Current room</param>
    /// <param name="character">Character making the check</param>
    /// <returns>Tuple of (success, damage taken if failed, log message)</returns>
    public (bool success, int damage, string logMessage) ProcessCheckBasedHazard(Room room, PlayerCharacter character)
    {
        if (!room.HasEnvironmentalHazard || !room.IsHazardActive || !room.HazardRequiresCheck)
        {
            return (true, 0, string.Empty);
        }

        // Get attribute value for the check
        int attributeValue = character.GetAttributeValue(room.HazardCheckAttribute);
        var rollResult = _diceService.Roll(attributeValue);

        bool success = rollResult.Successes >= room.HazardCheckDC;

        string logMessage = $"⚠️ {room.HazardDescription}\n";
        logMessage += $"   {room.HazardCheckAttribute} Check (DC {room.HazardCheckDC}): ";
        logMessage += $"Rolled {attributeValue} dice → [{string.Join(", ", rollResult.IndividualRolls)}] → {rollResult.Successes} successes\n";

        int damage = 0;

        if (success)
        {
            logMessage += $"   ✓ SUCCESS! You navigate the hazard safely.";
        }
        else
        {
            damage = _diceService.RollDamage(room.HazardCheckFailureDice, room.HazardCheckFailureDieSize);
            logMessage += $"   ✗ FAILURE! You take {room.HazardCheckFailureDice}d{room.HazardCheckFailureDieSize} → {damage} damage!";

            character.HP = Math.Max(0, character.HP - damage);
        }

        return (success, damage, logMessage);
    }

    /// <summary>
    /// Process triggered hazards (called by specific events like knockback attacks)
    /// Used for: ElectricalHazard (knockback into conduits), Fire (spreading), etc.
    /// </summary>
    /// <param name="room">Current room</param>
    /// <param name="character">Character being affected</param>
    /// <param name="triggerContext">Context for what triggered the hazard</param>
    /// <returns>Tuple of (damage dealt, log message)</returns>
    public (int damage, string logMessage) ProcessTriggeredHazard(Room room, PlayerCharacter character, string triggerContext = "")
    {
        if (!room.HasEnvironmentalHazard || !room.IsHazardActive)
        {
            return (0, string.Empty);
        }

        int damage = 0;
        string logMessage = "";

        if (room.HazardType == HazardType.ElectricalHazard)
        {
            damage = _diceService.RollDamage(room.HazardDamageDice, room.HazardDamageDieSize);
            logMessage = $"⚡ {triggerContext}\n";
            logMessage += $"   You're slammed into the live conduits! {room.HazardDamageDice}d{room.HazardDamageDieSize} → {damage} electrical damage!";

            character.HP = Math.Max(0, character.HP - damage);
        }

        return (damage, logMessage);
    }

    /// <summary>
    /// Get hazard type description for UI display
    /// </summary>
    public string GetHazardTypeDescription(HazardType hazardType)
    {
        return hazardType switch
        {
            HazardType.ToxicFumes => "Poisonous gas fills the air",
            HazardType.ToxicSludge => "Corrosive waste covers the ground",
            HazardType.UnstableFlooring => "The floor threatens to collapse",
            HazardType.ElectricalHazard => "Exposed electrical systems crackle with energy",
            HazardType.Radiation => "Radiation permeates the area",
            HazardType.Fire => "Flames rage throughout the room",
            HazardType.Ice => "Freezing conditions make movement treacherous",
            HazardType.Darkness => "Oppressive darkness limits visibility",
            HazardType.Vacuum => "The void of space threatens immediate death",
            HazardType.GenericDamage => "Environmental danger is present",
            _ => "Unknown hazard"
        };
    }

    /// <summary>
    /// Get flavor text for hazard severity based on damage/stress
    /// </summary>
    public string GetHazardSeverityFlavorText(int totalDamage, int totalStress)
    {
        int totalThreat = totalDamage + totalStress;

        return totalThreat switch
        {
            <= 5 => "The environment is mildly threatening.",
            <= 10 => "The hazard poses a moderate danger.",
            <= 15 => "The environment is extremely dangerous.",
            _ => "This place will kill you quickly if you linger."
        };
    }

    /// <summary>
    /// Check if room has active hazard
    /// </summary>
    public bool RoomHasActiveHazard(Room room)
    {
        return room.HasEnvironmentalHazard && room.IsHazardActive;
    }

    /// <summary>
    /// Disable a room's hazard (e.g., draining toxic sludge)
    /// </summary>
    public void DisableHazard(Room room, string disableMessage = "")
    {
        if (room.HasEnvironmentalHazard && room.IsHazardActive)
        {
            room.IsHazardActive = false;
        }
    }
}
