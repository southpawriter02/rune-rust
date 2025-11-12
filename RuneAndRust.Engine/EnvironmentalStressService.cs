using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Applies environmental stress based on room conditions and traumas
/// v0.15: Trauma Economy
/// </summary>
public class EnvironmentalStressService
{
    private static readonly ILogger _log = Log.ForContext<EnvironmentalStressService>();
    private readonly TraumaEconomyService _traumaEconomy;

    public EnvironmentalStressService(TraumaEconomyService traumaEconomy)
    {
        _traumaEconomy = traumaEconomy;
    }

    /// <summary>
    /// Applies all environmental stress for entering/being in a room
    /// </summary>
    /// <param name="character">The character</param>
    /// <param name="room">The room they're in</param>
    /// <returns>Total stress gained</returns>
    public int ApplyEnvironmentalStress(PlayerCharacter character, Room room)
    {
        int totalStress = 0;

        // Base environmental stress from ambient conditions
        totalStress += ApplyAmbientConditionStress(character, room);

        // Room size-based stress (agoraphobia/claustrophobia)
        totalStress += ApplyRoomSizeStress(character, room);

        // Lighting condition stress (nyctophobia)
        totalStress += ApplyLightingStress(character, room);

        // Enemy presence stress (paranoia)
        totalStress += ApplyEnemyPresenceStress(character, room);

        // Isolation stress (isolophobia)
        totalStress += ApplyIsolationStress(character);

        // Trauma-specific passive stress
        totalStress += ApplyTraumaPassiveStress(character, room);

        if (totalStress > 0)
        {
            _log.Debug("Environmental stress applied: Character={CharacterName}, Room={RoomName}, TotalStress={Stress}",
                character.Name, room.Name, totalStress);
        }

        return totalStress;
    }

    /// <summary>
    /// Applies stress from ambient conditions (v0.11 integration)
    /// </summary>
    private int ApplyAmbientConditionStress(PlayerCharacter character, Room room)
    {
        int stress = 0;

        // Check for Psychic Resonance condition (from v0.11)
        if (room.AmbientConditions?.Contains(AmbientCondition.PsychicResonance) ?? false)
        {
            stress += 2;
            _log.Debug("Psychic Resonance stress: +2");
        }

        // Flooded rooms (claustrophobia amplifier)
        if (room.AmbientConditions?.Contains(AmbientCondition.Flooded) ?? false)
        {
            if (character.HasTrauma("claustrophobia"))
            {
                stress += 2;
                _log.Debug("Flooded + Claustrophobia stress: +2");
            }
        }

        return stress;
    }

    /// <summary>
    /// Applies stress based on room size
    /// </summary>
    private int ApplyRoomSizeStress(PlayerCharacter character, Room room)
    {
        int stress = 0;

        // Agoraphobia: Large rooms cause stress
        if (character.HasTrauma("agoraphobia"))
        {
            var trauma = character.GetTrauma("agoraphobia");
            if (trauma != null)
            {
                // Check if room is "large" - we'll use a heuristic based on description or explicit size
                if (IsLargeRoom(room))
                {
                    int passiveStress = trauma.GetPassiveStress("large_room");
                    stress += passiveStress;
                    _log.Debug("Agoraphobia (large room) stress: +{Stress}", passiveStress);
                }
            }
        }

        // Claustrophobia: Small rooms cause stress
        if (character.HasTrauma("claustrophobia"))
        {
            var trauma = character.GetTrauma("claustrophobia");
            if (trauma != null)
            {
                if (IsSmallRoom(room))
                {
                    int passiveStress = trauma.GetPassiveStress("small_room");
                    stress += passiveStress;
                    _log.Debug("Claustrophobia (small room) stress: +{Stress}", passiveStress);
                }
            }
        }

        return stress;
    }

    /// <summary>
    /// Applies stress from lighting conditions
    /// </summary>
    private int ApplyLightingStress(PlayerCharacter character, Room room)
    {
        int stress = 0;

        // Nyctophobia: Dim lighting causes increased stress
        if (character.HasTrauma("nyctophobia"))
        {
            if (room.AmbientConditions?.Contains(AmbientCondition.DimLighting) ?? false)
            {
                var trauma = character.GetTrauma("nyctophobia");
                if (trauma != null)
                {
                    // Apply stress multiplier
                    float multiplier = trauma.GetStressMultiplier("dim_lighting");
                    // Base darkness stress
                    int baseStress = 3;
                    stress += (int)(baseStress * multiplier);
                    _log.Debug("Nyctophobia (dim lighting) stress: +{Stress} (multiplier: {Mult})",
                        stress, multiplier);
                }
            }
        }

        return stress;
    }

    /// <summary>
    /// Applies stress from enemy presence
    /// </summary>
    private int ApplyEnemyPresenceStress(PlayerCharacter character, Room room)
    {
        int stress = 0;

        // Check for enemies in the room
        bool hasEnemies = (room.DormantProcesses?.Count ?? 0) > 0;

        if (hasEnemies)
        {
            // Base stress from enemy presence
            stress += 1;

            // Paranoia: Extra stress from enemies
            if (character.HasTrauma("paranoia"))
            {
                stress += 2;
                _log.Debug("Paranoia (enemy present) stress: +2");
            }

            // Hypervigilance: Always on edge
            if (character.HasTrauma("hypervigilance"))
            {
                stress += 1;
                _log.Debug("Hypervigilance (enemy present) stress: +1");
            }
        }

        return stress;
    }

    /// <summary>
    /// Applies stress from being alone
    /// </summary>
    private int ApplyIsolationStress(PlayerCharacter character)
    {
        int stress = 0;

        // Isolophobia: Stress when alone (no companions)
        if (character.HasTrauma("isolophobia"))
        {
            var trauma = character.GetTrauma("isolophobia");
            if (trauma != null)
            {
                // Check if character is alone (no companions)
                // Note: This would need integration with companion system
                bool isAlone = true; // TODO: Check for companions

                if (isAlone)
                {
                    int passiveStress = trauma.GetPassiveStress("alone");
                    stress += passiveStress;
                    _log.Debug("Isolophobia (alone) stress: +{Stress}", passiveStress);
                }
            }
        }

        return stress;
    }

    /// <summary>
    /// Applies passive stress from traumas based on current conditions
    /// </summary>
    private int ApplyTraumaPassiveStress(PlayerCharacter character, Room room)
    {
        int stress = 0;

        // Build condition string based on room state
        var conditions = new List<string>();

        if (IsLargeRoom(room)) conditions.Add("large_room");
        if (IsSmallRoom(room)) conditions.Add("small_room");
        if (room.AmbientConditions?.Contains(AmbientCondition.DimLighting) ?? false) conditions.Add("dim_lighting");
        if ((room.DormantProcesses?.Count ?? 0) > 0) conditions.Add("enemy_nearby");

        // Apply passive stress for each condition
        foreach (var condition in conditions)
        {
            int conditionStress = _traumaEconomy.ApplyPassiveTraumaStress(character, condition);
            stress += conditionStress;
        }

        return stress;
    }

    /// <summary>
    /// Determines if a room is "large" (for agoraphobia)
    /// </summary>
    private bool IsLargeRoom(Room room)
    {
        // Heuristic: Check for keywords in name/description or explicit size markers
        string checkText = $"{room.Name} {room.Description}".ToLower();

        var largeKeywords = new[] { "vast", "sprawling", "enormous", "cavernous", "hall", "atrium", "plaza" };

        return largeKeywords.Any(keyword => checkText.Contains(keyword));
    }

    /// <summary>
    /// Determines if a room is "small" (for claustrophobia)
    /// </summary>
    private bool IsSmallRoom(Room room)
    {
        // Heuristic: Check for keywords in name/description
        string checkText = $"{room.Name} {room.Description}".ToLower();

        var smallKeywords = new[] { "cramped", "narrow", "tight", "confined", "closet", "alcove", "crawlspace" };

        return smallKeywords.Any(keyword => checkText.Contains(keyword));
    }

    /// <summary>
    /// Gets a description of current environmental stress sources
    /// </summary>
    public string GetEnvironmentalStressReport(PlayerCharacter character, Room room)
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("Environmental Stress Sources:");

        bool anyStress = false;

        // Check each source
        if (room.AmbientConditions?.Contains(AmbientCondition.PsychicResonance) ?? false)
        {
            report.AppendLine("  • Psychic Resonance: +2 Stress/turn");
            anyStress = true;
        }

        if (character.HasTrauma("agoraphobia") && IsLargeRoom(room))
        {
            report.AppendLine("  • [AGORAPHOBIA] Large open space: +2 Stress/turn");
            anyStress = true;
        }

        if (character.HasTrauma("claustrophobia") && IsSmallRoom(room))
        {
            report.AppendLine("  • [CLAUSTROPHOBIA] Cramped quarters: +2 Stress/turn");
            anyStress = true;
        }

        if (character.HasTrauma("nyctophobia") && (room.AmbientConditions?.Contains(AmbientCondition.DimLighting) ?? false))
        {
            report.AppendLine("  • [NYCTOPHOBIA] Darkness: +50% Stress");
            anyStress = true;
        }

        if ((room.DormantProcesses?.Count ?? 0) > 0)
        {
            report.AppendLine("  • Enemy presence: +1 Stress/turn");
            if (character.HasTrauma("paranoia"))
            {
                report.AppendLine("  • [PARANOIA] Heightened threat: +2 Stress/turn");
            }
            anyStress = true;
        }

        if (character.HasTrauma("isolophobia"))
        {
            report.AppendLine("  • [ISOLOPHOBIA] Alone: +1 Stress/turn");
            anyStress = true;
        }

        if (!anyStress)
        {
            report.AppendLine("  None");
        }

        return report.ToString();
    }
}
