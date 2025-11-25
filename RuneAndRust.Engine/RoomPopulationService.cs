using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Core.Population;
using RuneAndRust.Persistence;
using Serilog;
using Population = RuneAndRust.Core.Population;
using Descriptors = RuneAndRust.Core.Descriptors;
using DescriptorDynamicHazard = RuneAndRust.Core.Descriptors.DynamicHazard;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38 Integration: Room Population Service
/// Coordinates all descriptor services to procedurally populate rooms
/// Integrates v0.38.1 (Room Descriptions), v0.38.2 (Environmental Features),
/// v0.38.3 (Interactive Objects), v0.38.4 (Atmospheric Descriptors), v0.38.5 (Resource Nodes)
/// </summary>
public class RoomPopulationService
{
    private readonly DescriptorRepository _repository;
    private readonly RoomDescriptorService _roomDescriptorService;
    private readonly EnvironmentalFeatureService _featureService;
    private readonly ObjectInteractionService _objectService;
    private readonly AtmosphericDescriptorService _atmosphericService;
    private readonly ResourceNodeService _resourceNodeService;
    private readonly ILogger _logger;
    private readonly Random _random;

    public RoomPopulationService(
        DescriptorRepository repository,
        RoomDescriptorService roomDescriptorService,
        EnvironmentalFeatureService featureService,
        ObjectInteractionService objectService,
        AtmosphericDescriptorService atmosphericService,
        ResourceNodeService resourceNodeService,
        ILogger logger,
        Random? random = null)
    {
        _repository = repository;
        _roomDescriptorService = roomDescriptorService;
        _featureService = featureService;
        _objectService = objectService;
        _atmosphericService = atmosphericService;
        _resourceNodeService = resourceNodeService;
        _logger = logger;
        _random = random ?? new Random();
    }

    /// <summary>
    /// Fully populates a room with descriptors, features, and interactive objects
    /// </summary>
    public void PopulateRoom(Room room, string biome, Population.RoomArchetype archetype)
    {
        if (room.IsHandcrafted)
        {
            _logger.Debug("Skipping population for handcrafted room: {RoomId}", room.RoomId);
            return;
        }

        _logger.Information(
            "Populating room: {RoomId}, Biome: {Biome}, Archetype: {Archetype}",
            room.RoomId,
            biome,
            archetype);

        try
        {
            // Step 1: Generate room name and description (v0.38.1)
            PopulateRoomDescription(room, biome, archetype);

            // Step 2: Generate atmospheric description (v0.38.4)
            PopulateAtmosphere(room, biome, archetype);

            // Step 3: Generate static terrain and hazards (v0.38.2)
            PopulateEnvironmentalFeatures(room, biome, archetype);

            // Step 4: Generate interactive objects (v0.38.3)
            PopulateInteractiveObjects(room, biome, archetype);

            // Step 5: Generate resource nodes (v0.38.5)
            PopulateResourceNodes(room, biome, archetype);

            // Step 6: Apply coherent glitch rules
            ApplyCoherentGlitchRules(room, biome);

            _logger.Information(
                "Room population complete: {RoomId} - {Features} features, {Objects} objects, {Resources} resources, Atmosphere: {HasAtmosphere}",
                room.RoomId,
                room.StaticTerrainFeatures.Count + room.DynamicHazardFeatures.Count,
                room.InteractiveObjects.Count,
                room.ResourceNodes.Count,
                !string.IsNullOrEmpty(room.AtmosphericDescription));
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error populating room: {RoomId}, Biome: {Biome}",
                room.RoomId,
                biome);
            throw;
        }
    }

    #region Room Description (v0.38.1)

    private void PopulateRoomDescription(Room room, string biome, Population.RoomArchetype archetype)
    {
        try
        {
            // Convert Population.RoomArchetype to Descriptors.RoomArchetype
            var descriptorArchetype = ConvertArchetype(archetype);

            // Generate name and description
            room.Name = _roomDescriptorService.GenerateRoomName(descriptorArchetype, biome);
            room.Description = _roomDescriptorService.GenerateRoomDescription(descriptorArchetype, biome);

            _logger.Debug(
                "Generated room description: {RoomId} - {Name}",
                room.RoomId,
                room.Name);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error generating room description: {RoomId}",
                room.RoomId);
            // Use fallback
            room.Name = $"{biome} {archetype}";
            room.Description = $"A {archetype.ToString().ToLower()} in {biome}.";
        }
    }

    #endregion

    #region Atmospheric Description (v0.38.4)

    private void PopulateAtmosphere(Room room, string biome, Population.RoomArchetype archetype)
    {
        try
        {
            // Determine intensity based on archetype
            var intensity = GetAtmosphericIntensityForArchetype(archetype);

            // Generate atmospheric description
            room.AtmosphericDescription = _atmosphericService.GenerateAtmosphere(biome, intensity);
            room.AtmosphericIntensity = intensity;

            _logger.Debug(
                "Generated atmosphere: {RoomId} - Intensity: {Intensity}",
                room.RoomId,
                intensity);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error generating atmosphere: {RoomId}",
                room.RoomId);
            // Fallback: Leave atmosphere null
            room.AtmosphericDescription = null;
            room.AtmosphericIntensity = null;
        }
    }

    private Descriptors.AtmosphericIntensity GetAtmosphericIntensityForArchetype(Population.RoomArchetype archetype)
    {
        return archetype switch
        {
            Population.RoomArchetype.BossArena => Descriptors.AtmosphericIntensity.Oppressive,  // Oppressive for boss fights
            Population.RoomArchetype.Corridor => Descriptors.AtmosphericIntensity.Subtle,  // Subtle for corridors
            Population.RoomArchetype.SecretRoom => Descriptors.AtmosphericIntensity.Subtle,  // Subtle for secrets
            _ => Descriptors.AtmosphericIntensity.Moderate  // Moderate default
        };
    }

    #endregion

    #region Environmental Features (v0.38.2)

    private void PopulateEnvironmentalFeatures(Room room, string biome, Population.RoomArchetype archetype)
    {
        try
        {
            // Determine number of features based on archetype
            var (staticCount, hazardCount) = GetFeatureCountsForArchetype(archetype);

            // Generate static terrain features (cover, obstacles, elevation)
            for (int i = 0; i < staticCount; i++)
            {
                var feature = GenerateRandomStaticTerrain(biome, archetype);
                if (feature != null)
                {
                    room.StaticTerrainFeatures.Add(feature);
                }
            }

            // Generate dynamic hazards
            for (int i = 0; i < hazardCount; i++)
            {
                var hazard = GenerateRandomDynamicHazard(biome, archetype);
                if (hazard != null)
                {
                    room.DynamicHazardFeatures.Add(hazard);
                }
            }

            _logger.Debug(
                "Generated environmental features: {RoomId} - {Static} static, {Hazards} hazards",
                room.RoomId,
                room.StaticTerrainFeatures.Count,
                room.DynamicHazardFeatures.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error generating environmental features: {RoomId}",
                room.RoomId);
        }
    }

    private StaticTerrainFeature? GenerateRandomStaticTerrain(string biome, Population.RoomArchetype archetype)
    {
        // Get modifier for biome
        var modifier = GetModifierForBiome(biome);
        if (modifier == null)
            return null;

        // Choose random static terrain template based on archetype
        var template = archetype switch
        {
            Population.RoomArchetype.BossArena => _random.Next(3) switch
            {
                0 => "Pillar_Base",  // Cover for tactical combat
                1 => "Elevation_Base",  // Tactical advantage
                _ => "Rubble_Pile_Base"  // Difficult terrain
            },
            Population.RoomArchetype.Corridor => _random.Next(2) switch
            {
                0 => "Crate_Stack_Base",  // Light cover
                _ => "Rubble_Pile_Base"  // Difficult terrain
            },
            _ => _random.Next(4) switch
            {
                0 => "Pillar_Base",
                1 => "Crate_Stack_Base",
                2 => "Elevation_Base",
                _ => "Rubble_Pile_Base"
            }
        };

        try
        {
            return _featureService.GenerateStaticTerrain(template, modifier);
        }
        catch
        {
            return null;
        }
    }

    private DescriptorDynamicHazard? GenerateRandomDynamicHazard(string biome, Population.RoomArchetype archetype)
    {
        // Get modifier for biome
        var modifier = GetModifierForBiome(biome);
        if (modifier == null)
            return null;

        // Boss arenas get more dangerous hazards
        var template = archetype == Population.RoomArchetype.BossArena
            ? _random.Next(3) switch
            {
                0 => "Burning_Ground_Base",
                1 => "Power_Conduit_Base",
                _ => "Steam_Vent_Base"
            }
            : _random.Next(4) switch
            {
                0 => "Steam_Vent_Base",
                1 => "Burning_Ground_Base",
                2 => "Toxic_Haze_Base",
                _ => "Electrified_Floor_Base"
            };

        try
        {
            return _featureService.GenerateDynamicHazard(template, modifier);
        }
        catch
        {
            return null;
        }
    }

    private (int staticCount, int hazardCount) GetFeatureCountsForArchetype(Population.RoomArchetype archetype)
    {
        return archetype switch
        {
            Population.RoomArchetype.BossArena => (3, 2),  // 3 static, 2 hazards
            Population.RoomArchetype.Chamber => (2, 1),  // 2 static, 1 hazard
            Population.RoomArchetype.Corridor => (1, 0),  // 1 static, no hazards
            Population.RoomArchetype.Junction => (1, 1),  // 1 static, 1 hazard
            Population.RoomArchetype.SecretRoom => (0, 0),  // No features, focus on loot
            _ => (1, 0)  // Default: 1 static, no hazards
        };
    }

    #endregion

    #region Interactive Objects (v0.38.3)

    private void PopulateInteractiveObjects(Room room, string biome, Population.RoomArchetype archetype)
    {
        try
        {
            // Determine number of objects based on archetype
            var objectCount = GetObjectCountForArchetype(archetype);

            for (int i = 0; i < objectCount; i++)
            {
                var obj = GenerateRandomInteractiveObject(room, biome, archetype);
                if (obj != null)
                {
                    room.InteractiveObjects.Add(obj);
                }
            }

            _logger.Debug(
                "Generated interactive objects: {RoomId} - {Count} objects",
                room.RoomId,
                room.InteractiveObjects.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error generating interactive objects: {RoomId}",
                room.RoomId);
        }
    }

    private InteractiveObject? GenerateRandomInteractiveObject(Room room, string biome, Population.RoomArchetype archetype)
    {
        var modifier = GetModifierForBiome(biome);
        if (modifier == null)
            return null;

        // Choose object type based on archetype
        var (template, function) = archetype switch
        {
            Population.RoomArchetype.BossArena => ("Corpse_Base", "Warrior"),  // Fallen warrior corpse
            Population.RoomArchetype.SecretRoom => ("Chest_Base", "Weapon_Locker"),  // Valuable chest
            Population.RoomArchetype.Chamber => _random.Next(3) switch
            {
                0 => ("Console_Base", "Hazard_Control"),  // Console to disable hazards
                1 => ("Chest_Base", "Supply_Cache"),  // Loot chest
                _ => ("Corpse_Base", "Scavenger")  // Scavenger corpse
            },
            _ => _random.Next(4) switch
            {
                0 => ("Crate_Base", null),  // Simple crate
                1 => ("Locker_Base", null),  // Personal locker
                2 => ("Data_Slate_Base", "Personal_Log"),  // Data slate
                _ => ("Corpse_Base", "Scavenger")  // Corpse
            }
        };

        try
        {
            var roomId = int.TryParse(room.RoomId, out var id) ? id : room.Id;
            return _objectService.GenerateObject(template, modifier, function, roomId);
        }
        catch
        {
            return null;
        }
    }

    private int GetObjectCountForArchetype(Population.RoomArchetype archetype)
    {
        return archetype switch
        {
            Population.RoomArchetype.BossArena => 1,  // 1 object (usually corpse or console)
            Population.RoomArchetype.SecretRoom => 2,  // 2 objects (focus on loot)
            Population.RoomArchetype.Chamber => _random.Next(1, 3),  // 1-2 objects
            Population.RoomArchetype.Corridor => _random.Next(0, 2),  // 0-1 objects
            _ => _random.Next(0, 2)  // 0-1 objects
        };
    }

    #endregion

    #region Resource Nodes (v0.38.5)

    private void PopulateResourceNodes(Room room, string biome, Population.RoomArchetype archetype)
    {
        try
        {
            // Determine room size based on archetype
            var roomSize = GetRoomSizeForArchetype(archetype);

            // Get room ID
            var roomId = int.TryParse(room.RoomId, out var id) ? id : room.Id;

            // Generate resource nodes
            var nodes = _resourceNodeService.GenerateResourceNodes(roomId, biome, roomSize);

            foreach (var node in nodes)
            {
                room.ResourceNodes.Add(node);
            }

            _logger.Debug(
                "Generated resource nodes: {RoomId} - {Count} nodes",
                room.RoomId,
                room.ResourceNodes.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error generating resource nodes: {RoomId}",
                room.RoomId);
        }
    }

    private string GetRoomSizeForArchetype(Population.RoomArchetype archetype)
    {
        return archetype switch
        {
            Population.RoomArchetype.BossArena => "Large",
            Population.RoomArchetype.Chamber => "Medium",
            Population.RoomArchetype.Corridor => "Small",
            Population.RoomArchetype.Junction => "Medium",
            Population.RoomArchetype.SecretRoom => "Small",
            Population.RoomArchetype.StorageBay => "Large",
            Population.RoomArchetype.PowerStation => "Large",
            Population.RoomArchetype.Laboratory => "Medium",
            _ => "Medium"  // Default
        };
    }

    #endregion

    #region Coherent Glitch Rules

    private void ApplyCoherentGlitchRules(Room room, string biome)
    {
        // Rule 1: Power Conduit + Flooded → Enhanced damage
        var powerConduitHazards = room.DynamicHazardFeatures
            .Where(h => h.Name.Contains("Power Conduit"))
            .ToList();

        if (powerConduitHazards.Any() && room.HasAmbientCondition("Flooded"))
        {
            foreach (var hazard in powerConduitHazards)
            {
                hazard.Damage = "6d6 Lightning";  // Doubled from 3d6
                hazard.ActivationRange = 4;       // Doubled from 2
                hazard.AreaPattern = AreaEffectPattern.RoomWide;

                room.CoherentGlitchRulesFired++;

                _logger.Information(
                    "Coherent Glitch: Enhanced Power Conduit due to Flooded condition in Room {RoomId}",
                    room.RoomId);
            }
        }

        // Rule 2: Unstable Ceiling → Create Rubble Pile
        var unstableCeilings = room.DynamicHazardFeatures
            .Where(h => h.Name.Contains("Unstable Ceiling"))
            .ToList();

        foreach (var hazard in unstableCeilings)
        {
            var modifier = GetModifierForBiome(biome);
            if (modifier != null)
            {
                try
                {
                    var rubble = _featureService.GenerateStaticTerrain("Rubble_Pile_Base", modifier);
                    room.StaticTerrainFeatures.Add(rubble);

                    room.CoherentGlitchRulesFired++;

                    _logger.Information(
                        "Coherent Glitch: Added Rubble Pile beneath Unstable Ceiling in Room {RoomId}",
                        room.RoomId);
                }
                catch
                {
                    // Skip if error
                }
            }
        }

        // Rule 3: Lava hazards create ambient heat (already handled in EnvironmentalFeatureService)
    }

    #endregion

    #region Helper Methods

    private string? GetModifierForBiome(string biome)
    {
        return biome switch
        {
            "Muspelheim" => "Scorched",
            "Niflheim" => "Frozen",
            "The_Roots" => "Rusted",
            "Alfheim" => "Crystalline",
            "Jotunheim" => "Monolithic",
            _ => null
        };
    }

    private Descriptors.RoomArchetype ConvertArchetype(Population.RoomArchetype archetype)
    {
        return archetype switch
        {
            Population.RoomArchetype.EntryHall => Descriptors.RoomArchetype.EntryHall,
            Population.RoomArchetype.Corridor => Descriptors.RoomArchetype.Corridor,
            Population.RoomArchetype.Chamber => Descriptors.RoomArchetype.Chamber,
            Population.RoomArchetype.Junction => Descriptors.RoomArchetype.Junction,
            Population.RoomArchetype.BossArena => Descriptors.RoomArchetype.BossArena,
            Population.RoomArchetype.SecretRoom => Descriptors.RoomArchetype.SecretRoom,
            Population.RoomArchetype.VerticalShaft => Descriptors.RoomArchetype.VerticalShaft,
            Population.RoomArchetype.MaintenanceHub => Descriptors.RoomArchetype.MaintenanceHub,
            Population.RoomArchetype.StorageBay => Descriptors.RoomArchetype.StorageBay,
            Population.RoomArchetype.ObservationPlatform => Descriptors.RoomArchetype.ObservationPlatform,
            Population.RoomArchetype.PowerStation => Descriptors.RoomArchetype.PowerStation,
            Population.RoomArchetype.Laboratory => Descriptors.RoomArchetype.Laboratory,
            Population.RoomArchetype.Barracks => Descriptors.RoomArchetype.Barracks,
            Population.RoomArchetype.ForgeCharnber => Descriptors.RoomArchetype.ForgeCharnber,
            Population.RoomArchetype.CryoVault => Descriptors.RoomArchetype.CryoVault,
            _ => Descriptors.RoomArchetype.Chamber  // Fallback
        };
    }

    #endregion
}
