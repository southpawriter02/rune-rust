using RuneAndRust.Core.CombatFlavor;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.6: Service for generating dynamic combat flavor text
/// Provides contextual, varied descriptions for all combat actions
/// </summary>
public class CombatFlavorTextService
{
    private static readonly ILogger _log = Log.ForContext<CombatFlavorTextService>();
    private readonly DescriptorRepository _repository;
    private readonly Random _random;

    // Template variable patterns
    private static readonly Regex _templateVariablePattern = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

    // Target location options
    private static readonly string[] _targetLocations = new[]
    {
        "torso", "arm", "leg", "head", "chassis", "limb", "flank", "shoulder", "side"
    };

    private static readonly string[] _vitalLocations = new[]
    {
        "core", "heart", "neck", "power cell", "central processor", "throat", "spine"
    };

    private static readonly string[] _armorLocations = new[]
    {
        "armor", "plating", "carapace", "scales", "chassis", "hide"
    };

    public CombatFlavorTextService(DescriptorRepository repository, Random? random = null)
    {
        _repository = repository;
        _random = random ?? new Random();
    }

    #region Player Actions

    /// <summary>
    /// Generates flavor text for a player attack action
    /// </summary>
    public string GeneratePlayerAttackText(
        WeaponType weaponType,
        string weaponName,
        string enemyName,
        CombatOutcome outcome)
    {
        // Determine category based on weapon type
        var category = weaponType switch
        {
            WeaponType.Bow or WeaponType.Crossbow => CombatActionCategory.PlayerRangedAttack,
            _ => CombatActionCategory.PlayerMeleeAttack
        };

        // Get descriptors matching weapon and outcome
        var descriptors = _repository.GetCombatActionDescriptors(
            category: category,
            weaponType: weaponType.ToString(),
            outcomeType: outcome);

        if (descriptors.Count == 0)
        {
            _log.Warning("No combat descriptors found for weapon {Weapon} and outcome {Outcome}. Using fallback.",
                weaponType, outcome);
            return GenerateFallbackAttackText(weaponName, enemyName, outcome);
        }

        // Select random descriptor
        var descriptor = descriptors[_random.Next(descriptors.Count)];

        // Fill template variables
        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Weapon", weaponName},
            {"Enemy", enemyName},
            {"Target_Location", SelectRandom(_targetLocations)},
            {"Vital_Location", SelectRandom(_vitalLocations)},
            {"Armor_Location", SelectRandom(_armorLocations)},
            {"Damage_Type_Descriptor", GetDamageDescriptor(outcome)},
            {"Enemy_Reaction", GetEnemyReaction()},
            {"Environment_Feature", "wall"} // Can be enhanced with room context
        });
    }

    /// <summary>
    /// Generates flavor text for a player defense action
    /// </summary>
    public string GeneratePlayerDefenseText(
        WeaponType defenseType, // Dodge, Parry, Block
        string? weaponName,
        string? shieldName,
        string enemyName,
        bool successful)
    {
        var outcome = successful ? CombatOutcome.Miss : CombatOutcome.SolidHit;

        var descriptors = _repository.GetCombatActionDescriptors(
            category: CombatActionCategory.PlayerDefense,
            weaponType: defenseType.ToString(),
            outcomeType: outcome);

        if (descriptors.Count == 0)
        {
            return successful
                ? $"You successfully {defenseType.ToString().ToLower()} the {enemyName}'s attack."
                : $"You fail to {defenseType.ToString().ToLower()} the {enemyName}'s attack!";
        }

        var descriptor = descriptors[_random.Next(descriptors.Count)];

        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Weapon", weaponName ?? "weapon"},
            {"Shield", shieldName ?? "shield"},
            {"Enemy", enemyName}
        });
    }

    #endregion

    #region Enemy Actions

    /// <summary>
    /// Generates enemy attack text with archetype-specific voice
    /// </summary>
    public string GenerateEnemyAttackText(
        EnemyArchetype archetype,
        string enemyName,
        bool isSpecialAttack = false)
    {
        var voiceProfile = _repository.GetEnemyVoiceProfile(archetype.ToString());

        if (voiceProfile == null)
        {
            _log.Warning("No voice profile found for enemy archetype: {Archetype}", archetype);
            return $"The {enemyName} attacks!";
        }

        // Parse descriptor ID arrays
        var descriptorIds = isSpecialAttack
            ? ParseDescriptorIds(voiceProfile.SpecialAttacks ?? "[]")
            : ParseDescriptorIds(voiceProfile.AttackDescriptors);

        if (descriptorIds.Count == 0)
        {
            // Fallback to regular attack descriptors from database
            var fallbackDescriptors = _repository.GetCombatActionDescriptors(
                category: CombatActionCategory.EnemyAttack,
                enemyArchetype: archetype.ToString());

            if (fallbackDescriptors.Count > 0)
            {
                var descriptor = fallbackDescriptors[_random.Next(fallbackDescriptors.Count)];
                return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
                {
                    {"Enemy", enemyName}
                });
            }

            return $"The {enemyName} attacks!";
        }

        // Get random descriptor from voice profile
        var descriptorId = descriptorIds[_random.Next(descriptorIds.Count)];
        var selectedDescriptor = _repository.GetCombatActionDescriptorById(descriptorId);

        if (selectedDescriptor == null)
        {
            return $"The {enemyName} attacks!";
        }

        return FillTemplate(selectedDescriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Enemy", enemyName}
        });
    }

    /// <summary>
    /// Generates enemy reaction text when taking damage
    /// </summary>
    public string GenerateEnemyDamageReaction(
        EnemyArchetype archetype,
        string enemyName,
        int damageAmount,
        bool isDying)
    {
        var voiceProfile = _repository.GetEnemyVoiceProfile(archetype.ToString());

        if (voiceProfile == null)
        {
            return isDying
                ? $"The {enemyName} collapses!"
                : $"The {enemyName} reels from the blow.";
        }

        // Parse descriptor ID arrays
        var descriptorIds = isDying
            ? ParseDescriptorIds(voiceProfile.ReactionDeath)
            : ParseDescriptorIds(voiceProfile.ReactionDamage);

        if (descriptorIds.Count == 0)
        {
            // Fallback to database query
            var outcome = isDying ? CombatOutcome.CriticalHit : CombatOutcome.SolidHit;
            var fallbackDescriptors = _repository.GetCombatActionDescriptors(
                category: CombatActionCategory.EnemyDefense,
                enemyArchetype: archetype.ToString(),
                outcomeType: outcome);

            if (fallbackDescriptors.Count > 0)
            {
                var descriptor = fallbackDescriptors[_random.Next(fallbackDescriptors.Count)];
                return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
                {
                    {"Enemy", enemyName},
                    {"DamageAmount", damageAmount.ToString()}
                });
            }

            return isDying
                ? $"The {enemyName} collapses!"
                : $"The {enemyName} reels from the blow.";
        }

        var descriptorId = descriptorIds[_random.Next(descriptorIds.Count)];
        var selectedDescriptor = _repository.GetCombatActionDescriptorById(descriptorId);

        if (selectedDescriptor == null)
        {
            return isDying
                ? $"The {enemyName} collapses!"
                : $"The {enemyName} reels from the blow.";
        }

        return FillTemplate(selectedDescriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Enemy", enemyName},
            {"DamageAmount", damageAmount.ToString()}
        });
    }

    #endregion

    #region Environmental Context

    /// <summary>
    /// Generates environmental reaction to combat
    /// </summary>
    public string GenerateEnvironmentalReaction(
        string biomeName,
        EnvironmentalModifierType? modifierType = null)
    {
        var modifiers = _repository.GetEnvironmentalCombatModifiers(biomeName, modifierType);

        if (modifiers.Count == 0)
            return string.Empty;

        // Filter modifiers by trigger chance
        var triggeredModifiers = modifiers
            .Where(m => _random.NextDouble() < m.TriggerChance)
            .ToList();

        if (triggeredModifiers.Count == 0)
            return string.Empty;

        var modifier = triggeredModifiers[_random.Next(triggeredModifiers.Count)];
        return modifier.DescriptorText;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Fills template variables in descriptor text
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
    /// Parses JSON array of descriptor IDs
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
    /// Selects a random element from an array
    /// </summary>
    private string SelectRandom(string[] options)
    {
        return options[_random.Next(options.Length)];
    }

    /// <summary>
    /// Gets damage descriptor based on outcome
    /// </summary>
    private string GetDamageDescriptor(CombatOutcome outcome)
    {
        return outcome switch
        {
            CombatOutcome.CriticalHit => "blood and viscera",
            CombatOutcome.DevastatingHit => "gore",
            CombatOutcome.SolidHit => "blood",
            CombatOutcome.GlancingHit => "a trickle of blood",
            _ => "sparks"
        };
    }

    /// <summary>
    /// Gets enemy reaction descriptor
    /// </summary>
    private string GetEnemyReaction()
    {
        var reactions = new[] { "a cry of pain", "a howl", "a shriek", "a groan", "a snarl" };
        return SelectRandom(reactions);
    }

    /// <summary>
    /// Generates fallback attack text when no descriptors are found
    /// </summary>
    private string GenerateFallbackText(string weaponName, string enemyName, CombatOutcome outcome)
    {
        return outcome switch
        {
            CombatOutcome.Miss => $"You attack the {enemyName} with your {weaponName} but miss.",
            CombatOutcome.Deflected => $"The {enemyName} deflects your {weaponName}.",
            CombatOutcome.GlancingHit => $"You graze the {enemyName} with your {weaponName}.",
            CombatOutcome.SolidHit => $"You strike the {enemyName} with your {weaponName}.",
            CombatOutcome.DevastatingHit => $"You devastate the {enemyName} with your {weaponName}!",
            CombatOutcome.CriticalHit => $"You critically strike the {enemyName} with your {weaponName}!",
            _ => $"You attack the {enemyName} with your {weaponName}."
        };
    }

    private string GenerateFallbackAttackText(string weaponName, string enemyName, CombatOutcome outcome)
    {
        return GenerateFallbackText(weaponName, enemyName, outcome);
    }

    #endregion
}
