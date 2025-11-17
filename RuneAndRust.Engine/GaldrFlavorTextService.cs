// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// GaldrFlavorTextService.cs
// ==============================================================================
// Purpose: Service for generating dynamic Galdr and ability flavor text
// Pattern: Follows CombatFlavorTextService structure
// Integration: Used by AbilityService and MagicService
// ==============================================================================

using RuneAndRust.Core.GaldrFlavor;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for generating dynamic Galdr casting and ability flavor text.
/// Provides contextual, varied descriptions for magical and non-magical abilities.
/// </summary>
public class GaldrFlavorTextService
{
    private static readonly ILogger _log = Log.ForContext<GaldrFlavorTextService>();
    private readonly DescriptorRepository _repository;
    private readonly Random _random;

    // Template variable patterns
    private static readonly Regex _templateVariablePattern = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

    // Rune symbols (Elder Futhark)
    private static readonly Dictionary<string, string> _runeSymbols = new()
    {
        // Fehu's Ætt
        {"Fehu", "ᚠ"},
        {"Uruz", "ᚢ"},
        {"Thurisaz", "ᚦ"},
        {"Ansuz", "ᚨ"},
        {"Raido", "ᚱ"},
        {"Kenaz", "ᚲ"},

        // Hagalaz's Ætt
        {"Hagalaz", "ᚺ"},
        {"Naudiz", "ᚾ"},
        {"Isa", "ᛁ"},
        {"Jera", "ᛃ"},

        // Tiwaz's Ætt
        {"Tiwaz", "ᛏ"},
        {"Berkanan", "ᛒ"},
        {"Ehwaz", "ᛖ"},
        {"Mannaz", "ᛗ"},
        {"Laguz", "ᛚ"}
    };

    // Magic color palette by rune school
    private static readonly Dictionary<string, string> _magicColors = new()
    {
        {"Fehu", "crimson"},
        {"Thurisaz", "frost-white"},
        {"Ansuz", "electric blue"},
        {"Raido", "silver-grey"},
        {"Hagalaz", "icy blue"},
        {"Naudiz", "sickly green"},
        {"Isa", "pale cyan"},
        {"Jera", "verdant green"},
        {"Tiwaz", "golden"},
        {"Berkanan", "soft green"},
        {"Mannaz", "violet"},
        {"Laguz", "aquamarine"}
    };

    public GaldrFlavorTextService(DescriptorRepository repository, Random? random = null)
    {
        _repository = repository;
        _random = random ?? new Random();
    }

    #region Galdr Casting

    /// <summary>
    /// Generates flavor text for Galdr casting (magical abilities).
    /// </summary>
    /// <param name="runeSchool">The rune school (Fehu, Thurisaz, etc.)</param>
    /// <param name="abilityName">The ability name (FlameBolt, FrostLance, etc.)</param>
    /// <param name="successCount">Number of successes rolled (1-2 Minor, 3-4 Solid, 5+ Exceptional)</param>
    /// <param name="targetName">Target of the spell</param>
    /// <param name="casterName">Name of the caster (default: "You")</param>
    /// <param name="biomeName">Current biome (optional)</param>
    /// <param name="casterArchetype">Caster archetype for voice profile (optional)</param>
    /// <returns>Formatted Galdr casting narrative</returns>
    public string GenerateGaldrCastingText(
        string runeSchool,
        string abilityName,
        int successCount,
        string targetName,
        string casterName = "You",
        string? biomeName = null,
        string? casterArchetype = null)
    {
        // Determine success level
        var successLevel = successCount switch
        {
            <= 2 => GaldrActionDescriptor.SuccessLevels.MinorSuccess,
            >= 5 => GaldrActionDescriptor.SuccessLevels.ExceptionalSuccess,
            _ => GaldrActionDescriptor.SuccessLevels.SolidSuccess
        };

        // Get Galdr action descriptors
        var descriptors = _repository.GetGaldrActionDescriptors(
            category: GaldrActionDescriptor.Categories.GaldrCasting,
            runeSchool: runeSchool,
            abilityName: abilityName,
            successLevel: successLevel,
            biomeName: biomeName);

        // If caster archetype specified, filter by voice profile
        if (!string.IsNullOrEmpty(casterArchetype))
        {
            var voiceProfile = _repository.GetGaldrCasterVoiceProfile(casterArchetype);
            if (voiceProfile != null)
            {
                var preferredDescriptorIds = ParseDescriptorIds(voiceProfile.InvocationDescriptors ?? "[]");
                if (preferredDescriptorIds.Count > 0)
                {
                    descriptors = descriptors.Where(d => preferredDescriptorIds.Contains(d.DescriptorId)).ToList();
                }
            }
        }

        if (descriptors.Count == 0)
        {
            _log.Warning("No Galdr casting descriptors found for {Rune}/{Ability}/{SuccessLevel}. Using fallback.",
                runeSchool, abilityName, successLevel);
            return GenerateFallbackGaldrText(runeSchool, abilityName, successLevel, targetName, casterName);
        }

        // Select random descriptor (weighted)
        var descriptor = SelectWeightedDescriptor(descriptors);

        // Fill template variables
        var variables = new Dictionary<string, string>
        {
            {"Caster", casterName},
            {"Target", targetName},
            {"Rune", runeSchool},
            {"RuneSymbol", GetRuneSymbol(runeSchool)},
            {"Ability", FormatAbilityName(abilityName)},
            {"Element", GetElementForRune(runeSchool)},
            {"MagicColor", GetMagicColor(runeSchool)},
            {"SuccessCount", successCount.ToString()},
            {"Biome", biomeName ?? "unknown"}
        };

        return FillTemplate(descriptor.DescriptorText, variables);
    }

    /// <summary>
    /// Generates flavor text for spell manifestation (visual/sensory effects).
    /// </summary>
    public string GenerateGaldrManifestationText(
        string runeSchool,
        string element,
        string powerLevel,
        string? biomeName = null)
    {
        var descriptors = _repository.GetGaldrManifestationDescriptors(
            runeSchool: runeSchool,
            element: element,
            powerLevel: powerLevel,
            biomeName: biomeName);

        if (descriptors.Count == 0)
            return string.Empty;

        var descriptor = SelectWeightedDescriptor(descriptors);

        var variables = new Dictionary<string, string>
        {
            {"Rune", runeSchool},
            {"RuneSymbol", GetRuneSymbol(runeSchool)},
            {"Element", element},
            {"MagicColor", GetMagicColor(runeSchool)},
            {"PowerLevel", powerLevel},
            {"Biome", biomeName ?? "unknown"}
        };

        return FillTemplate(descriptor.DescriptorText, variables);
    }

    /// <summary>
    /// Generates flavor text for ability outcomes (damage, healing, effects).
    /// </summary>
    public string GenerateGaldrOutcomeText(
        string abilityName,
        string outcomeType,
        string targetName,
        int? damageOrHealing = null,
        string? effectCategory = null,
        string? enemyArchetype = null)
    {
        var descriptors = _repository.GetGaldrOutcomeDescriptors(
            abilityName: abilityName,
            outcomeType: outcomeType,
            enemyArchetype: enemyArchetype,
            effectCategory: effectCategory);

        if (descriptors.Count == 0)
        {
            _log.Warning("No outcome descriptors found for {Ability}/{Outcome}. Using fallback.",
                abilityName, outcomeType);
            return GenerateFallbackOutcomeText(abilityName, outcomeType, targetName, damageOrHealing);
        }

        var descriptor = SelectWeightedDescriptor(descriptors);

        var variables = new Dictionary<string, string>
        {
            {"Target", targetName},
            {"Enemy", targetName},
            {"Ability", FormatAbilityName(abilityName)},
            {"Damage", damageOrHealing?.ToString() ?? "0"},
            {"Healing", damageOrHealing?.ToString() ?? "0"},
            {"Rune", ExtractRuneFromAbility(abilityName)},
            {"Element", effectCategory ?? "magic"},
            {"Target_Location", GetRandomTargetLocation()},
            {"Vital_Location", GetRandomVitalLocation()},
            {"Armor_Location", GetRandomArmorLocation()}
        };

        return FillTemplate(descriptor.DescriptorText, variables);
    }

    #endregion

    #region Miscast & Paradox

    /// <summary>
    /// Generates flavor text for magical miscasts and paradox events.
    /// </summary>
    public string GenerateMiscastText(
        string miscastType,
        string severity,
        string? runeSchool = null,
        string? abilityName = null,
        string? biomeName = null,
        string? corruptionSource = null)
    {
        var descriptors = _repository.GetGaldrMiscastDescriptors(
            miscastType: miscastType,
            severity: severity,
            runeSchool: runeSchool,
            abilityName: abilityName,
            biomeName: biomeName,
            corruptionSource: corruptionSource);

        if (descriptors.Count == 0)
        {
            _log.Warning("No miscast descriptors found for {Type}/{Severity}. Using fallback.",
                miscastType, severity);
            return GenerateFallbackMiscastText(miscastType, severity, runeSchool);
        }

        var descriptor = SelectWeightedDescriptor(descriptors);

        var variables = new Dictionary<string, string>
        {
            {"Rune", runeSchool ?? "unknown"},
            {"RuneSymbol", GetRuneSymbol(runeSchool ?? "")},
            {"Ability", FormatAbilityName(abilityName ?? "spell")},
            {"Element", GetElementForRune(runeSchool ?? "")},
            {"CorruptionLevel", severity},
            {"BlightEffect", GetBlightEffect(severity)},
            {"ParadoxManifestation", GetParadoxManifestation()},
            {"Biome", biomeName ?? "unknown"}
        };

        return FillTemplate(descriptor.DescriptorText, variables);
    }

    /// <summary>
    /// Gets mechanical effect from a miscast descriptor.
    /// Returns parsed JSON with effect details (damage, status, duration, etc.)
    /// </summary>
    public Dictionary<string, object>? GetMiscastMechanicalEffect(GaldrMiscastDescriptor descriptor)
    {
        if (string.IsNullOrEmpty(descriptor.MechanicalEffect))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(descriptor.MechanicalEffect);
        }
        catch (JsonException ex)
        {
            _log.Warning(ex, "Failed to parse miscast mechanical effect: {Json}", descriptor.MechanicalEffect);
            return null;
        }
    }

    #endregion

    #region Non-Galdr Abilities

    /// <summary>
    /// Generates flavor text for non-magical abilities (weapon arts, tactics, etc.)
    /// </summary>
    public string GenerateAbilityFlavorText(
        string abilityCategory,
        string abilityName,
        string? weaponName = null,
        string? weaponType = null,
        string? targetName = null,
        string? successLevel = null,
        string? specialization = null)
    {
        var descriptors = _repository.GetAbilityFlavorDescriptors(
            abilityCategory: abilityCategory,
            abilityName: abilityName,
            weaponType: weaponType,
            specialization: specialization,
            successLevel: successLevel);

        if (descriptors.Count == 0)
        {
            _log.Warning("No ability flavor descriptors found for {Category}/{Ability}. Using fallback.",
                abilityCategory, abilityName);
            return GenerateFallbackAbilityText(abilityName, weaponName, targetName);
        }

        var descriptor = SelectWeightedDescriptor(descriptors);

        var variables = new Dictionary<string, string>
        {
            {"Weapon", weaponName ?? "weapon"},
            {"WeaponType", weaponType ?? "weapon"},
            {"Target", targetName ?? "target"},
            {"Enemy", targetName ?? "enemy"},
            {"Ability", FormatAbilityName(abilityName)}
        };

        return FillTemplate(descriptor.DescriptorText, variables);
    }

    #endregion

    #region Environmental Reactions

    /// <summary>
    /// Generates environmental reaction to Galdr casting.
    /// </summary>
    public string GenerateGaldrEnvironmentalReaction(
        string biomeName,
        string? runeSchool = null,
        string? element = null,
        string? reactionType = null)
    {
        var reactions = _repository.GetGaldrEnvironmentalReactions(
            biomeName: biomeName,
            runeSchool: runeSchool,
            element: element,
            reactionType: reactionType);

        if (reactions.Count == 0)
            return string.Empty;

        // Filter by trigger chance
        var triggeredReactions = reactions
            .Where(r => _random.NextDouble() < r.TriggerChance)
            .ToList();

        if (triggeredReactions.Count == 0)
            return string.Empty;

        var reaction = SelectWeightedDescriptor(triggeredReactions);

        var variables = new Dictionary<string, string>
        {
            {"Rune", runeSchool ?? "unknown"},
            {"Element", element ?? "magic"},
            {"Biome", biomeName}
        };

        return FillTemplate(reaction.DescriptorText, variables);
    }

    #endregion

    #region Caster Voice Profiles

    /// <summary>
    /// Gets the caster voice profile for a given archetype.
    /// </summary>
    public GaldrCasterVoiceProfile? GetCasterVoiceProfile(string casterArchetype)
    {
        return _repository.GetGaldrCasterVoiceProfile(casterArchetype);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Fills template variables in descriptor text.
    /// </summary>
    private string FillTemplate(string template, Dictionary<string, string> variables)
    {
        return _templateVariablePattern.Replace(template, match =>
        {
            var variableName = match.Groups[1].Value;
            return variables.ContainsKey(variableName) ? variables[variableName] : match.Value;
        });
    }

    /// <summary>
    /// Selects a weighted random descriptor from a list.
    /// </summary>
    private T SelectWeightedDescriptor<T>(List<T> descriptors) where T : class
    {
        // If only one descriptor, return it
        if (descriptors.Count == 1)
            return descriptors[0];

        // Get weight property via reflection (works for all descriptor types)
        var weightProperty = typeof(T).GetProperty("Weight");
        if (weightProperty == null)
        {
            // No weight property, use uniform random
            return descriptors[_random.Next(descriptors.Count)];
        }

        // Calculate total weight
        var totalWeight = descriptors.Sum(d => (float)(weightProperty.GetValue(d) ?? 1.0f));

        // Random selection based on weight
        var randomValue = (float)(_random.NextDouble() * totalWeight);
        var cumulativeWeight = 0.0f;

        foreach (var descriptor in descriptors)
        {
            cumulativeWeight += (float)(weightProperty.GetValue(descriptor) ?? 1.0f);
            if (randomValue <= cumulativeWeight)
                return descriptor;
        }

        // Fallback (should never reach here)
        return descriptors[^1];
    }

    /// <summary>
    /// Parses JSON array of descriptor IDs.
    /// </summary>
    private List<int> ParseDescriptorIds(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch (JsonException ex)
        {
            _log.Warning(ex, "Failed to parse descriptor IDs from JSON: {Json}", json);
            return new List<int>();
        }
    }

    /// <summary>
    /// Gets rune symbol for a rune school.
    /// </summary>
    private string GetRuneSymbol(string runeSchool)
    {
        return _runeSymbols.GetValueOrDefault(runeSchool, "ᚠ");
    }

    /// <summary>
    /// Gets magic color for a rune school.
    /// </summary>
    private string GetMagicColor(string runeSchool)
    {
        return _magicColors.GetValueOrDefault(runeSchool, "azure");
    }

    /// <summary>
    /// Gets element associated with a rune school.
    /// </summary>
    private string GetElementForRune(string runeSchool)
    {
        return runeSchool switch
        {
            "Fehu" => "Fire",
            "Thurisaz" => "Ice",
            "Ansuz" => "Lightning",
            "Raido" => "Wind",
            "Hagalaz" => "Ice",
            "Naudiz" => "Shadow",
            "Isa" => "Ice",
            "Jera" => "Earth",
            "Tiwaz" => "Light",
            "Berkanan" => "Healing",
            "Mannaz" => "Aether",
            "Laguz" => "Water",
            _ => "Magic"
        };
    }

    /// <summary>
    /// Extracts rune school from ability name (heuristic).
    /// </summary>
    private string ExtractRuneFromAbility(string abilityName)
    {
        // Heuristic mapping (can be enhanced with database lookup)
        if (abilityName.Contains("Flame") || abilityName.Contains("Fire"))
            return "Fehu";
        if (abilityName.Contains("Frost") || abilityName.Contains("Ice"))
            return "Thurisaz";
        if (abilityName.Contains("Lightning") || abilityName.Contains("Thunder"))
            return "Ansuz";
        if (abilityName.Contains("Heal"))
            return "Berkanan";
        if (abilityName.Contains("Ward") || abilityName.Contains("Shield"))
            return "Tiwaz";

        return "Fehu"; // Default
    }

    /// <summary>
    /// Formats ability name for display (camelCase to Title Case).
    /// </summary>
    private string FormatAbilityName(string abilityName)
    {
        return Regex.Replace(abilityName, "([a-z])([A-Z])", "$1 $2");
    }

    /// <summary>
    /// Gets random target location.
    /// </summary>
    private string GetRandomTargetLocation()
    {
        var locations = new[] { "torso", "arm", "leg", "head", "chassis", "limb", "shoulder" };
        return locations[_random.Next(locations.Length)];
    }

    /// <summary>
    /// Gets random vital location.
    /// </summary>
    private string GetRandomVitalLocation()
    {
        var vitals = new[] { "core", "heart", "neck", "power cell", "throat", "spine" };
        return vitals[_random.Next(vitals.Length)];
    }

    /// <summary>
    /// Gets random armor location.
    /// </summary>
    private string GetRandomArmorLocation()
    {
        var armor = new[] { "armor", "plating", "carapace", "scales", "chassis", "hide" };
        return armor[_random.Next(armor.Length)];
    }

    /// <summary>
    /// Gets Blight effect description based on severity.
    /// </summary>
    private string GetBlightEffect(string severity)
    {
        return severity switch
        {
            "Minor" => "a faint corruption",
            "Moderate" => "warping distortion",
            "Severe" => "reality-bending corruption",
            "Catastrophic" => "catastrophic paradox",
            _ => "corruption"
        };
    }

    /// <summary>
    /// Gets random paradox manifestation.
    /// </summary>
    private string GetParadoxManifestation()
    {
        var manifestations = new[]
        {
            "twisted geometries", "impossible colors", "backwards fire", "frozen lightning",
            "solid shadows", "liquid stone", "singing silence"
        };
        return manifestations[_random.Next(manifestations.Length)];
    }

    #endregion

    #region Fallback Text Generators

    private string GenerateFallbackGaldrText(
        string runeSchool,
        string abilityName,
        string successLevel,
        string targetName,
        string casterName)
    {
        var element = GetElementForRune(runeSchool);
        var formattedAbility = FormatAbilityName(abilityName);

        return successLevel switch
        {
            GaldrActionDescriptor.SuccessLevels.MinorSuccess =>
                $"{casterName} chant the {runeSchool} rune. {element} flickers weakly toward {targetName}.",
            GaldrActionDescriptor.SuccessLevels.ExceptionalSuccess =>
                $"{casterName} invoke {runeSchool} with perfect resonance! {element} erupts, engulfing {targetName}!",
            _ =>
                $"{casterName} chant {runeSchool}. {element} streaks toward {targetName}."
        };
    }

    private string GenerateFallbackOutcomeText(
        string abilityName,
        string outcomeType,
        string targetName,
        int? damageOrHealing)
    {
        var formattedAbility = FormatAbilityName(abilityName);

        return outcomeType switch
        {
            "Hit" => $"Your {formattedAbility} strikes {targetName}! [{damageOrHealing ?? 0} damage]",
            "Miss" => $"Your {formattedAbility} misses {targetName}!",
            "CriticalHit" => $"Your {formattedAbility} devastates {targetName}! [{damageOrHealing ?? 0} damage]",
            "FullEffect" => $"Your {formattedAbility} affects {targetName}! [+{damageOrHealing ?? 0} HP]",
            _ => $"{targetName} is affected by your {formattedAbility}."
        };
    }

    private string GenerateFallbackMiscastText(string miscastType, string severity, string? runeSchool)
    {
        var rune = runeSchool ?? "rune";

        return severity switch
        {
            "Catastrophic" => $"The {rune} corrupts catastrophically! Reality warps around you!",
            "Severe" => $"Your chant falters—the Blight warps {rune}!",
            _ => $"The spell fizzles as the {rune} resists your invocation."
        };
    }

    private string GenerateFallbackAbilityText(string abilityName, string? weaponName, string? targetName)
    {
        var formattedAbility = FormatAbilityName(abilityName);

        if (!string.IsNullOrEmpty(weaponName) && !string.IsNullOrEmpty(targetName))
            return $"You use {formattedAbility} with your {weaponName} against {targetName}!";

        if (!string.IsNullOrEmpty(weaponName))
            return $"You use {formattedAbility} with your {weaponName}!";

        return $"You use {formattedAbility}!";
    }

    #endregion
}
