using RuneAndRust.Core.Descriptors;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.2: Service interface for environmental feature generation
/// </summary>
public interface IEnvironmentalFeatureService
{
    /// <summary>
    /// Generates a static terrain feature from base template + modifier
    /// </summary>
    StaticTerrainFeature GenerateStaticTerrain(string baseTemplateName, string? modifierName = null);

    /// <summary>
    /// Generates a dynamic hazard from base template + modifier
    /// </summary>
    DynamicHazard GenerateDynamicHazard(string baseTemplateName, string? modifierName = null);

    /// <summary>
    /// Gets all static terrain features for a biome
    /// </summary>
    List<StaticTerrainFeature> GetStaticTerrainForBiome(string biome, List<string>? tags = null, int limit = 10);

    /// <summary>
    /// Gets all dynamic hazards for a biome
    /// </summary>
    List<DynamicHazard> GetDynamicHazardsForBiome(string biome, List<string>? tags = null, int limit = 5);
}

/// <summary>
/// v0.38.2: Environmental Feature Service
/// Generates environmental features from descriptor templates
/// Integrates with v0.11 Dynamic Room Engine population system
/// </summary>
public class EnvironmentalFeatureService : IEnvironmentalFeatureService
{
    private readonly DescriptorRepository _repository;
    private readonly ILogger _logger;

    public EnvironmentalFeatureService(
        DescriptorRepository repository,
        ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Generate static terrain feature from base template + modifier.
    /// </summary>
    public StaticTerrainFeature GenerateStaticTerrain(
        string baseTemplateName,
        string? modifierName = null)
    {
        try
        {
            var baseTemplate = _repository.GetBaseTemplate(baseTemplateName);
            if (baseTemplate == null)
            {
                _logger.Error(
                    "Base template not found: {BaseTemplateName}",
                    baseTemplateName);
                throw new ArgumentException($"Base template not found: {baseTemplateName}");
            }

            ThematicModifier? modifier = null;
            if (!string.IsNullOrEmpty(modifierName))
            {
                modifier = _repository.GetModifier(modifierName);
                if (modifier == null)
                {
                    _logger.Warning(
                        "Modifier not found, using unmodified template: {ModifierName}",
                        modifierName);
                }
            }

            // Parse base mechanics
            var baseMechanics = FeatureMechanics.FromJson(baseTemplate.BaseMechanics);
            if (baseMechanics == null)
            {
                _logger.Error(
                    "Failed to parse base mechanics for template: {TemplateName}",
                    baseTemplateName);
                throw new InvalidOperationException($"Invalid base mechanics for template: {baseTemplateName}");
            }

            // Apply modifier stat adjustments
            var finalMechanics = modifier != null
                ? ApplyModifierToMechanics(baseMechanics, modifier)
                : baseMechanics;

            // Generate name and description
            var name = GenerateName(baseTemplate.NameTemplate, modifier);
            var description = GenerateDescription(
                baseTemplate.DescriptionTemplate,
                baseTemplate,
                modifier);

            _logger.Debug(
                "Generated static terrain: {Name} (Base: {Base}, Modifier: {Modifier})",
                name,
                baseTemplateName,
                modifierName ?? "None");

            return new StaticTerrainFeature
            {
                Name = name,
                Description = description,
                BaseTemplateName = baseTemplateName,
                ModifierName = modifierName,
                Archetype = baseTemplate.Archetype,
                HP = finalMechanics.HP,
                Soak = finalMechanics.Soak,
                IsDestructible = finalMechanics.Destructible,
                CoverQuality = ParseCoverQuality(finalMechanics.CoverQuality),
                CoverBonus = finalMechanics.CoverBonus,
                BlocksLoS = finalMechanics.BlocksLoS,
                IsImpassable = finalMechanics.Impassable,
                FallDamage = finalMechanics.FallDamage,
                DamageType = finalMechanics.DamageType,
                ElevationBonus = finalMechanics.ElevationBonus,
                ClimbCost = finalMechanics.ClimbCost,
                MovementCostModifier = finalMechanics.MovementCostModifier,
                TilesOccupied = finalMechanics.TilesOccupied,
                TilesWidth = finalMechanics.TilesWidth,
                IsTacticalDivider = finalMechanics.TacticalDivider,
                BiomeRestriction = modifier?.PrimaryBiome,
                Tags = baseTemplate.GetTags()
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error generating static terrain: Base={Base}, Modifier={Modifier}",
                baseTemplateName,
                modifierName);
            throw;
        }
    }

    /// <summary>
    /// Generate dynamic hazard from base template + modifier.
    /// </summary>
    public DynamicHazard GenerateDynamicHazard(
        string baseTemplateName,
        string? modifierName = null)
    {
        try
        {
            var baseTemplate = _repository.GetBaseTemplate(baseTemplateName);
            if (baseTemplate == null)
            {
                _logger.Error(
                    "Base template not found: {BaseTemplateName}",
                    baseTemplateName);
                throw new ArgumentException($"Base template not found: {baseTemplateName}");
            }

            ThematicModifier? modifier = null;
            if (!string.IsNullOrEmpty(modifierName))
            {
                modifier = _repository.GetModifier(modifierName);
                if (modifier == null)
                {
                    _logger.Warning(
                        "Modifier not found, using unmodified template: {ModifierName}",
                        modifierName);
                }
            }

            // Parse base mechanics
            var baseMechanics = HazardMechanics.FromJson(baseTemplate.BaseMechanics);
            if (baseMechanics == null)
            {
                _logger.Error(
                    "Failed to parse hazard mechanics for template: {TemplateName}",
                    baseTemplateName);
                throw new InvalidOperationException($"Invalid hazard mechanics for template: {baseTemplateName}");
            }

            // Apply modifier stat adjustments
            var finalMechanics = modifier != null
                ? ApplyModifierToHazard(baseMechanics, modifier)
                : baseMechanics;

            // Generate name and description
            var name = GenerateName(baseTemplate.NameTemplate, modifier);
            var description = GenerateDescription(
                baseTemplate.DescriptionTemplate,
                baseTemplate,
                modifier);

            _logger.Debug(
                "Generated dynamic hazard: {Name}, Type={Type} (Base: {Base}, Modifier: {Modifier})",
                name,
                finalMechanics.DamageType,
                baseTemplateName,
                modifierName ?? "None");

            return new DynamicHazard
            {
                Name = name,
                Description = description,
                BaseTemplateName = baseTemplateName,
                ModifierName = modifierName,
                Archetype = baseTemplate.Archetype,
                Damage = finalMechanics.Damage,
                DamageType = finalMechanics.DamageType,
                ActivationType = ParseActivationType(finalMechanics.ActivationType),
                ActivationFrequency = finalMechanics.ActivationFrequency,
                ActivationRange = finalMechanics.ActivationRange,
                ActivationTiming = ParseActivationTiming(finalMechanics.ActivationTiming),
                WarningTurn = finalMechanics.WarningTurn,
                AreaPattern = ParseAreaPattern(finalMechanics.AreaPattern),
                TilesAffected = finalMechanics.TilesAffected,
                StatusEffect = finalMechanics.StatusEffect,
                StatusEffectChance = finalMechanics.StatusEffectChance,
                IsOneTime = finalMechanics.OneTime,
                CreatesTerrain = finalMechanics.CreatesTerrain,
                Triggers = finalMechanics.Triggers ?? new List<string>(),
                EnhancedBy = finalMechanics.EnhancedBy ?? new List<string>(),
                SpreadChance = finalMechanics.SpreadChance,
                AccuracyPenalty = finalMechanics.AccuracyPenalty,
                Stacks = finalMechanics.Stacks,
                AmbientHeatRange = finalMechanics.AmbientHeatRange,
                AmbientHeatDamage = finalMechanics.AmbientHeatDamage,
                ProximityStress = finalMechanics.ProximityStress,
                ProximityRange = finalMechanics.ProximityRange,
                IsUnstable = finalMechanics.Unstable,
                BiomeRestriction = modifier?.PrimaryBiome,
                Tags = baseTemplate.GetTags()
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Error generating dynamic hazard: Base={Base}, Modifier={Modifier}",
                baseTemplateName,
                modifierName);
            throw;
        }
    }

    /// <summary>
    /// Gets all static terrain features for a biome
    /// </summary>
    public List<StaticTerrainFeature> GetStaticTerrainForBiome(
        string biome,
        List<string>? tags = null,
        int limit = 10)
    {
        var query = new DescriptorQuery
        {
            Category = "Feature",
            Archetype = "Cover,Obstacle,Tactical",
            Biome = biome,
            RequiredTags = tags,
            Limit = limit
        };

        var composites = _repository.QueryComposites(query);
        var features = new List<StaticTerrainFeature>();

        foreach (var composite in composites)
        {
            try
            {
                var feature = GenerateStaticTerrain(
                    composite.BaseTemplate?.TemplateName ?? "",
                    composite.Modifier?.ModifierName);
                features.Add(feature);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex,
                    "Failed to generate static terrain from composite: {CompositeName}",
                    composite.FinalName);
            }
        }

        return features;
    }

    /// <summary>
    /// Gets all dynamic hazards for a biome
    /// </summary>
    public List<DynamicHazard> GetDynamicHazardsForBiome(
        string biome,
        List<string>? tags = null,
        int limit = 5)
    {
        var query = new DescriptorQuery
        {
            Category = "Feature",
            Archetype = "DynamicHazard",
            Biome = biome,
            RequiredTags = tags,
            Limit = limit
        };

        var composites = _repository.QueryComposites(query);
        var hazards = new List<DynamicHazard>();

        foreach (var composite in composites)
        {
            try
            {
                var hazard = GenerateDynamicHazard(
                    composite.BaseTemplate?.TemplateName ?? "",
                    composite.Modifier?.ModifierName);
                hazards.Add(hazard);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex,
                    "Failed to generate dynamic hazard from composite: {CompositeName}",
                    composite.FinalName);
            }
        }

        return hazards;
    }

    #region Private Helper Methods

    private FeatureMechanics ApplyModifierToMechanics(
        FeatureMechanics baseMechanics,
        ThematicModifier modifier)
    {
        var result = baseMechanics.Clone();

        // Parse modifier stat adjustments
        if (!string.IsNullOrEmpty(modifier.StatModifiers))
        {
            try
            {
                var modifiers = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    modifier.StatModifiers);

                if (modifiers == null)
                    return result;

                // Apply HP modifier if present
                if (modifiers.ContainsKey("hp_multiplier"))
                {
                    var multiplier = modifiers["hp_multiplier"].GetDouble();
                    result.HP = (int)(result.HP * multiplier);
                }

                // Apply Soak modifier if present
                if (modifiers.ContainsKey("soak_multiplier"))
                {
                    var multiplier = modifiers["soak_multiplier"].GetDouble();
                    result.Soak = (int)(result.Soak * multiplier);
                }

                // Apply damage type override if present
                if (modifiers.ContainsKey("damage_type"))
                {
                    result.DamageType = modifiers["damage_type"].GetString();
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex,
                    "Failed to parse modifier stat adjustments for: {ModifierName}",
                    modifier.ModifierName);
            }
        }

        return result;
    }

    private HazardMechanics ApplyModifierToHazard(
        HazardMechanics baseMechanics,
        ThematicModifier modifier)
    {
        var result = baseMechanics.Clone();

        // Parse modifier hazard adjustments
        if (!string.IsNullOrEmpty(modifier.StatModifiers))
        {
            try
            {
                var modifiers = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    modifier.StatModifiers);

                if (modifiers == null)
                    return result;

                // Damage type override
                if (modifiers.ContainsKey("damage_type"))
                {
                    result.DamageType = modifiers["damage_type"].GetString() ?? result.DamageType;
                }

                // Damage bonus
                if (modifiers.ContainsKey("fall_damage_bonus"))
                {
                    var bonus = modifiers["fall_damage_bonus"].GetString();
                    result.Damage += " + " + bonus;
                }

                // Activation range modifier
                if (modifiers.ContainsKey("activation_range_multiplier"))
                {
                    var multiplier = modifiers["activation_range_multiplier"].GetDouble();
                    result.ActivationRange = (int)(result.ActivationRange * multiplier);
                }

                // Ambient heat properties
                if (modifiers.ContainsKey("ambient_heat_range"))
                {
                    result.AmbientHeatRange = modifiers["ambient_heat_range"].GetInt32();
                }

                if (modifiers.ContainsKey("ambient_heat_damage"))
                {
                    result.AmbientHeatDamage = modifiers["ambient_heat_damage"].GetString();
                }

                // Proximity stress properties (for Void modifier)
                if (modifiers.ContainsKey("proximity_stress"))
                {
                    result.ProximityStress = modifiers["proximity_stress"].GetInt32();
                }

                if (modifiers.ContainsKey("proximity_range"))
                {
                    result.ProximityRange = modifiers["proximity_range"].GetInt32();
                }

                if (modifiers.ContainsKey("unstable"))
                {
                    result.Unstable = modifiers["unstable"].GetBoolean();
                }

                // Damage multiplier
                if (modifiers.ContainsKey("damage_multiplier"))
                {
                    var multiplier = modifiers["damage_multiplier"].GetDouble();
                    // This would require parsing and modifying damage dice, simplified for now
                    _logger.Debug("Damage multiplier applied: {Multiplier}", multiplier);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex,
                    "Failed to parse modifier hazard adjustments for: {ModifierName}",
                    modifier.ModifierName);
            }
        }

        return result;
    }

    private string GenerateName(string template, ThematicModifier? modifier)
    {
        if (modifier == null)
        {
            // Return unmodified name (remove placeholder text)
            return template
                .Replace("{Modifier} ", "")
                .Replace(" {Modifier}", "")
                .Replace("{Modifier}", "Standard");
        }

        return template
            .Replace("{Modifier}", modifier.ModifierName.Replace("_", " "))
            .Replace("{Modifier_Adj}", modifier.Adjective);
    }

    private string GenerateDescription(
        string template,
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier? modifier)
    {
        if (modifier == null)
        {
            return template
                .Replace("{Modifier_Adj}", "standard")
                .Replace("{Modifier_Detail}", "shows typical construction");
        }

        return template
            .Replace("{Modifier_Adj}", modifier.Adjective)
            .Replace("{Modifier_Detail}", modifier.DetailFragment);
    }

    private CoverQuality ParseCoverQuality(string? quality)
    {
        if (string.IsNullOrEmpty(quality))
            return CoverQuality.None;

        return quality.ToLower() switch
        {
            "light" => CoverQuality.Light,
            "heavy" => CoverQuality.Heavy,
            _ => CoverQuality.None
        };
    }

    private HazardActivationType ParseActivationType(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return HazardActivationType.Persistent;

        return type switch
        {
            "Periodic" => HazardActivationType.Periodic,
            "Proximity" => HazardActivationType.Proximity,
            "Triggered" => HazardActivationType.Triggered,
            "Persistent" => HazardActivationType.Persistent,
            "Movement" => HazardActivationType.Movement,
            _ => HazardActivationType.Persistent
        };
    }

    private HazardActivationTiming? ParseActivationTiming(string? timing)
    {
        if (string.IsNullOrEmpty(timing))
            return null;

        return timing switch
        {
            "Start_Of_Turn" => HazardActivationTiming.StartOfTurn,
            "End_Of_Turn" => HazardActivationTiming.EndOfTurn,
            "Immediate" => HazardActivationTiming.Immediate,
            _ => null
        };
    }

    private AreaEffectPattern ParseAreaPattern(string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return AreaEffectPattern.Single;

        return pattern switch
        {
            "3x3" => AreaEffectPattern.ThreeByThree,
            "5x5" => AreaEffectPattern.FiveByFive,
            "Line" => AreaEffectPattern.Line,
            "Cone" => AreaEffectPattern.Cone,
            "Room_Wide" => AreaEffectPattern.RoomWide,
            "All_Combatants" => AreaEffectPattern.AllCombatants,
            "Single" => AreaEffectPattern.Single,
            _ => AreaEffectPattern.Custom
        };
    }

    #endregion
}
