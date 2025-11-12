using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.11 Ambient Condition Applier
/// Applies environmental effects that modify room behavior
/// Implements Coherent Glitch rule interactions
/// </summary>
public class ConditionApplier
{
    private static readonly ILogger _log = Log.ForContext<ConditionApplier>();

    /// <summary>
    /// Applies ambient conditions to a room based on biome elements
    /// </summary>
    public void ApplyConditions(Room room, BiomeDefinition biome, Random rng)
    {
        // Skip if handcrafted
        if (room.IsHandcrafted)
        {
            _log.Debug("Skipping condition application for handcrafted room {RoomId}", room.RoomId);
            return;
        }

        // Determine condition count (0-1 per room, rarely 2)
        int conditionCount = DetermineConditionCount(room, rng);
        if (conditionCount == 0)
        {
            _log.Debug("Room {RoomId}: No ambient conditions", room.RoomId);
            return;
        }

        _log.Debug("Room {RoomId}: Applying {Count} ambient conditions", room.RoomId, conditionCount);

        // Get eligible conditions
        if (biome.Elements == null)
        {
            _log.Warning("Biome {BiomeName} has no Elements table", biome.Name);
            return;
        }

        var availableConditions = biome.Elements.GetEligibleElements(
            BiomeElementType.AmbientCondition, room, rng);

        if (availableConditions.Count == 0)
        {
            _log.Debug("No eligible conditions for room {RoomId}", room.RoomId);
            return;
        }

        // Apply weight modifiers for boss/narrative rooms
        if (room.IsBossRoom || room.GeneratedNodeType == NodeType.Boss)
        {
            // Psychic Resonance more common in boss rooms
            foreach (var condition in availableConditions.Where(c => c.ElementName.Contains("Psychic")))
            {
                condition.Weight *= 3.0f; // Triple weight for Psychic Resonance
                _log.Debug("Increased weight for {ConditionName} in boss room", condition.ElementName);
            }
        }

        // Apply conditions
        for (int i = 0; i < conditionCount; i++)
        {
            var selected = biome.Elements.WeightedRandomSelection(availableConditions, rng);
            if (selected == null) break;

            var condition = CreateConditionFromElement(selected, room, rng);
            if (condition != null)
            {
                room.AmbientConditions.Add(condition);
                _log.Debug("Applied condition {ConditionName} to room {RoomId}", condition.Name, room.RoomId);

                // Apply Coherent Glitch interactions
                ApplyCoherentGlitchInteractions(room, condition);
            }

            // Remove from pool to avoid duplicates
            availableConditions = availableConditions.Where(c => c.ElementName != selected.ElementName).ToList();
        }

        _log.Information("Room {RoomId}: Applied {Count} ambient conditions", room.RoomId, room.AmbientConditions.Count);
    }

    /// <summary>
    /// Determines how many conditions to apply
    /// </summary>
    private int DetermineConditionCount(Room room, Random rng)
    {
        // Entry halls: 0-1 (usually none)
        if (room.IsStartRoom || room.GeneratedNodeType == NodeType.Start)
        {
            return rng.NextDouble() < 0.2 ? 1 : 0;
        }

        // Boss arenas: 1 (atmospheric)
        if (room.IsBossRoom)
        {
            return 1;
        }

        // Normal rooms: 0-1 (rarely 2)
        double roll = rng.NextDouble();
        if (roll < 0.6) return 0;
        if (roll < 0.95) return 1;
        return 2;
    }

    /// <summary>
    /// Applies Coherent Glitch interactions between conditions and hazards
    /// Example: [Flooded] enhances electrical hazards
    /// </summary>
    private void ApplyCoherentGlitchInteractions(Room room, AmbientCondition condition)
    {
        if (condition.Type == AmbientConditionType.Flooded)
        {
            // Enhance electrical hazards
            foreach (var hazard in room.DynamicHazards.Where(h => h.Type == DynamicHazardType.LivePowerConduit))
            {
                hazard.DamageDice = (int)(hazard.DamageDice * 2);
                hazard.ProximityRange = (int)(hazard.ProximityRange * 1.5);
                _log.Information("Coherent Glitch: [Flooded] enhanced electrical hazard in room {RoomId}", room.RoomId);
            }
        }
    }

    /// <summary>
    /// Creates an AmbientCondition from a BiomeElement
    /// </summary>
    private AmbientCondition? CreateConditionFromElement(BiomeElement element, Room room, Random rng)
    {
        var conditionType = MapElementToConditionType(element.AssociatedDataId);
        if (conditionType == null)
        {
            _log.Warning("Could not map element {ElementName} to condition type", element.ElementName);
            return null;
        }

        return conditionType.Value switch
        {
            AmbientConditionType.PsychicResonance => CreatePsychicResonance(),
            AmbientConditionType.RunicInstability => CreateRunicInstability(),
            AmbientConditionType.Flooded => CreateFlooded(),
            AmbientConditionType.CorrodedAtmosphere => CreateCorrodedAtmosphere(),
            AmbientConditionType.DimLighting => CreateDimLighting(),
            _ => null
        };
    }

    private AmbientConditionType? MapElementToConditionType(string? dataId)
    {
        return dataId switch
        {
            "psychic_resonance" => AmbientConditionType.PsychicResonance,
            "runic_instability" => AmbientConditionType.RunicInstability,
            "flooded" => AmbientConditionType.Flooded,
            "corroded_atmosphere" => AmbientConditionType.CorrodedAtmosphere,
            "dim_lighting" => AmbientConditionType.DimLighting,
            _ => null
        };
    }

    // Condition creation methods
    private AmbientCondition CreatePsychicResonance()
    {
        return new AmbientCondition
        {
            ConditionId = $"psychic_resonance_{Guid.NewGuid():N}",
            Name = "[Psychic Resonance]",
            Description = "The Great Silence echoes loudly here. Something terrible happened in this place.",
            Type = AmbientConditionType.PsychicResonance,
            PsychicStressPerTurn = 2,
            WillResolveModifier = -1,
            EnhancesHazardTypes = new List<string>(), // No direct hazard enhancement
            HazardEnhancementMultiplier = 1.0f
        };
    }

    private AmbientCondition CreateRunicInstability()
    {
        return new AmbientCondition
        {
            ConditionId = $"runic_instability_{Guid.NewGuid():N}",
            Name = "[Runic Instability]",
            Description = "The fabric of Aether feels volatile here. Magic behaves unpredictably.",
            Type = AmbientConditionType.RunicInstability,
            CausesWildMagic = true,
            WildMagicChance = 0.2f,
            EnhancesHazardTypes = new List<string>(),
            HazardEnhancementMultiplier = 1.0f
        };
    }

    private AmbientCondition CreateFlooded()
    {
        return new AmbientCondition
        {
            ConditionId = $"flooded_{Guid.NewGuid():N}",
            Name = "[Flooded]",
            Description = "Stagnant water covers the floor, reeking of rust and chemicals. Knee-deep in places.",
            Type = AmbientConditionType.Flooded,
            MovementCostModifier = 1,
            EnhancesHazardTypes = new List<string> { "LivePowerConduit" },
            HazardEnhancementMultiplier = 2.0f,
            SpawnWeightModifiers = new Dictionary<string, float>
            {
                ["LivePowerConduit"] = 2.0f // Electrical hazards 2x more likely
            }
        };
    }

    private AmbientCondition CreateCorrodedAtmosphere()
    {
        return new AmbientCondition
        {
            ConditionId = $"corroded_atmosphere_{Guid.NewGuid():N}",
            Name = "[Corroded Atmosphere]",
            Description = "The air is thick with rust particles and chemical fumes. Metal corrodes visibly.",
            Type = AmbientConditionType.CorrodedAtmosphere,
            CausesEquipmentDegradation = true,
            DegradationAmount = 1,
            CanApplyStatusEffect = "Corroded",
            StatusEffectChance = 0.1f,
            EnhancesHazardTypes = new List<string>(),
            HazardEnhancementMultiplier = 1.0f
        };
    }

    private AmbientCondition CreateDimLighting()
    {
        return new AmbientCondition
        {
            ConditionId = $"dim_lighting_{Guid.NewGuid():N}",
            Name = "[Dim Lighting]",
            Description = "Failing illumination systems provide only weak, flickering light. Shadows gather in corners.",
            Type = AmbientConditionType.DimLighting,
            AccuracyModifier = -1,
            PsychicStressPerTurn = 1,
            EnhancesHazardTypes = new List<string>(),
            HazardEnhancementMultiplier = 1.0f
        };
    }
}
