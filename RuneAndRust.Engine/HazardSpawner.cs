using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;
using DynamicHazard = RuneAndRust.Core.Population.DynamicHazard;
using AmbientCondition = RuneAndRust.Core.Population.AmbientCondition;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.11 Dynamic Hazard Spawner
/// Places environmental dangers with Coherent Glitch rule support
/// </summary>
public class HazardSpawner
{
    private static readonly ILogger _log = Log.ForContext<HazardSpawner>();

    /// <summary>
    /// Populates a room with dynamic hazards based on biome elements
    /// </summary>
    public void PopulateRoom(Room room, BiomeDefinition biome, Random rng)
    {
        // Skip if handcrafted
        if (room.IsHandcrafted)
        {
            _log.Debug("Skipping hazard population for handcrafted room {RoomId}", room.RoomId);
            return;
        }

        // Determine hazard count (0-2 hazards per room)
        int hazardCount = DetermineHazardCount(room, rng);
        if (hazardCount == 0)
        {
            _log.Debug("Room {RoomId}: No hazards", room.RoomId);
            return;
        }

        _log.Debug("Room {RoomId}: Spawning {Count} hazards", room.RoomId, hazardCount);

        // Get eligible hazards
        if (biome.Elements == null)
        {
            _log.Warning("Biome {BiomeName} has no Elements table", biome.Name);
            return;
        }

        var availableHazards = biome.Elements.GetEligibleElements(
            BiomeElementType.DynamicHazard, room, rng);

        if (availableHazards.Count == 0)
        {
            _log.Debug("No eligible hazards for room {RoomId}", room.RoomId);
            return;
        }

        // Apply weight modifiers based on ambient conditions (Coherent Glitch)
        ApplyCoherentGlitchWeightModifiers(room, availableHazards);

        // Spawn hazards
        for (int i = 0; i < hazardCount; i++)
        {
            var selected = biome.Elements.WeightedRandomSelection(availableHazards, rng);
            if (selected == null) break;

            var hazard = CreateHazardFromElement(selected, room, rng);
            if (hazard != null)
            {
                room.DynamicHazards.Add(hazard);
                _log.Debug("Spawned hazard {HazardName} in room {RoomId}", hazard.HazardName, room.RoomId);
            }

            // Remove from pool to avoid duplicates (optional - could allow multiple)
            availableHazards = availableHazards.Where(h => h.ElementName != selected.ElementName).ToList();
        }

        _log.Information("Room {RoomId}: Spawned {Count} hazards", room.RoomId, room.DynamicHazards.Count);
    }

    /// <summary>
    /// Determines how many hazards to spawn in a room
    /// </summary>
    private int DetermineHazardCount(Room room, Random rng)
    {
        // Entry halls: 0-1 hazards (safe)
        if (room.IsStartRoom || room.GeneratedNodeType == NodeType.Start)
        {
            return rng.NextDouble() < 0.3 ? 1 : 0;
        }

        // Secret rooms: 0-1 hazards (reward rooms)
        if (room.GeneratedNodeType == NodeType.Secret)
        {
            return rng.NextDouble() < 0.4 ? 1 : 0;
        }

        // Boss arenas: 1-2 hazards (challenging)
        if (room.IsBossRoom)
        {
            return rng.NextDouble() < 0.7 ? 2 : 1;
        }

        // Normal rooms: 0-2 hazards
        double roll = rng.NextDouble();
        if (roll < 0.4) return 0;
        if (roll < 0.8) return 1;
        return 2;
    }

    /// <summary>
    /// Applies Coherent Glitch weight modifiers based on ambient conditions
    /// Example: [Flooded] makes electrical hazards more common
    /// </summary>
    private void ApplyCoherentGlitchWeightModifiers(Room room, List<BiomeElement> hazards)
    {
        // Check for Flooded condition
        bool isFlooded = room.AmbientConditions.Cast<AmbientCondition>().Any(c => c.Type == AmbientConditionType.Flooded);
        if (isFlooded)
        {
            // Increase weight of electrical hazards
            foreach (var hazard in hazards.Where(h => h.ElementName.Contains("Power") || h.ElementName.Contains("Electrical")))
            {
                hazard.Weight *= 2.0f; // Double weight for electrical hazards when flooded
            }
        }

        // Check for Rust-Horror enemies (increases Toxic Spore Cloud weight)
        bool hasRustHorror = room.Enemies.Any(e => e.Type == EnemyType.CorruptedServitor);
        if (hasRustHorror)
        {
            foreach (var hazard in hazards.Where(h => h.ElementName.Contains("Spore") || h.ElementName.Contains("Toxic")))
            {
                hazard.Weight *= 1.5f;
            }
        }
    }

    /// <summary>
    /// Creates a DynamicHazard from a BiomeElement
    /// </summary>
    private DynamicHazard? CreateHazardFromElement(BiomeElement element, Room room, Random rng)
    {
        var hazardType = MapElementToHazardType(element.AssociatedDataId);
        if (hazardType == null)
        {
            _log.Warning("Could not map element {ElementName} to hazard type", element.ElementName);
            return null;
        }

        return hazardType.Value switch
        {
            DynamicHazardType.SteamVent => CreateSteamVent(element),
            DynamicHazardType.LivePowerConduit => CreateLivePowerConduit(element, room),
            DynamicHazardType.UnstableCeiling => CreateUnstableCeiling(element),
            DynamicHazardType.ToxicSporeCloud => CreateToxicSporeCloud(element),
            DynamicHazardType.CorrodedGrating => CreateCorrodedGrating(element),
            DynamicHazardType.LeakingCoolant => CreateLeakingCoolant(element),
            _ => null
        };
    }

    private DynamicHazardType? MapElementToHazardType(string? dataId)
    {
        return dataId switch
        {
            "steam_vent" => DynamicHazardType.SteamVent,
            "live_power_conduit" => DynamicHazardType.LivePowerConduit,
            "unstable_ceiling" => DynamicHazardType.UnstableCeiling,
            "toxic_spore_cloud" => DynamicHazardType.ToxicSporeCloud,
            "corroded_grating" => DynamicHazardType.CorrodedGrating,
            "leaking_coolant" => DynamicHazardType.LeakingCoolant,
            _ => null
        };
    }

    // Hazard creation methods
    private DynamicHazard CreateSteamVent(BiomeElement element)
    {
        return new SteamVentHazard
        {
            Id = $"steam_vent_{Guid.NewGuid():N}",
            HazardId = $"steam_vent_{Guid.NewGuid():N}",
            HazardName = "[Steam Vent]",
            Description = "Hissing jets of superheated steam escape from fractured pipes. The geothermal pumping station, once climate-controlled, now vents unpredictably after 800 years of corrosion.",
            IsIntermittent = true,
            TurnsUntilNextVent = 2
        };
    }

    private DynamicHazard CreateLivePowerConduit(BiomeElement element, Room room)
    {
        bool isFlooded = room.AmbientConditions.Cast<AmbientCondition>().Any(c => c.Type == AmbientConditionType.Flooded);

        return new LivePowerConduitHazard
        {
            Id = $"power_conduit_{Guid.NewGuid():N}",
            HazardId = $"power_conduit_{Guid.NewGuid():N}",
            HazardName = "[Live Power Conduit]",
            Description = "Exposed wiring crackles with unstable current. Pre-Glitch safety systems have long since failed." +
                         (isFlooded ? " The standing water conducts electricity across the entire room." : ""),
            DamageDice = isFlooded ? 6 : 3,
            Range = isFlooded ? 5.0f : 1.5f,
            IsContactBased = true,
            IsFloodedEnhanced = isFlooded
        };
    }

    private DynamicHazard CreateUnstableCeiling(BiomeElement element)
    {
        return new UnstableCeilingHazard
        {
            Id = $"unstable_ceiling_{Guid.NewGuid():N}",
            HazardId = $"unstable_ceiling_{Guid.NewGuid():N}",
            HazardName = "[Unstable Ceiling]",
            Description = "Centuries of structural stress have weakened the ceiling. Loud impacts could trigger a collapse.",
            CollapseThreshold = 10,
            AccumulatedDamage = 0
        };
    }

    private DynamicHazard CreateToxicSporeCloud(BiomeElement element)
    {
        return new ToxicSporeCloudHazard
        {
            Id = $"spore_cloud_{Guid.NewGuid():N}",
            HazardId = $"spore_cloud_{Guid.NewGuid():N}",
            HazardName = "[Toxic Spore Cloud]",
            Description = "Fungal growths release clouds of toxic spores into the stagnant air.",
            IsMoving = false
        };
    }

    private DynamicHazard CreateCorrodedGrating(BiomeElement element)
    {
        return new CorrodedGratingHazard
        {
            Id = $"corroded_grating_{Guid.NewGuid():N}",
            HazardId = $"corroded_grating_{Guid.NewGuid():N}",
            HazardName = "[Corroded Grating]",
            Description = "The floor grating is severely weakened. It may break under weight.",
            ActivationChance = 0.3f
        };
    }

    private DynamicHazard CreateLeakingCoolant(BiomeElement element)
    {
        return new LeakingCoolantHazard
        {
            Id = $"leaking_coolant_{Guid.NewGuid():N}",
            HazardId = $"leaking_coolant_{Guid.NewGuid():N}",
            HazardName = "[Leaking Coolant]",
            Description = "Slippery chemical coolant pools across the floor, making movement treacherous.",
            ActivationChance = 0.5f
        };
    }
}
