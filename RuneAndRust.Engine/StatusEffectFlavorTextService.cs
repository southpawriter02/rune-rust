using RuneAndRust.Core.StatusEffectFlavor;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.RegularExpressions;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.8: Service for generating dynamic status effect flavor text
/// Provides contextual, varied descriptions for status effect application, ticks, and expiration
/// </summary>
public class StatusEffectFlavorTextService
{
    private static readonly ILogger _log = Log.ForContext<StatusEffectFlavorTextService>();
    private readonly DescriptorRepository _repository;
    private readonly Random _random;

    // Template variable pattern
    private static readonly Regex _templateVariablePattern = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

    // Body location options
    private static readonly string[] _bodyLocations = new[]
    {
        "torso", "arm", "leg", "head", "shoulder", "side", "chest", "back"
    };

    private static readonly string[] _vitalLocations = new[]
    {
        "core", "heart", "neck", "throat", "spine", "brain"
    };

    public StatusEffectFlavorTextService(DescriptorRepository repository, Random? random = null)
    {
        _repository = repository;
        _random = random ?? new Random();
    }

    #region Status Effect Application (OnApply)

    /// <summary>
    /// Generates flavor text for status effect application
    /// </summary>
    public string GenerateApplicationText(
        string effectType,
        string targetName,
        string? sourceName = null,
        string? sourceType = null,
        string? sourceDetail = null,
        string? biomeName = null,
        bool isPlayer = true)
    {
        // Get descriptor for application
        var descriptor = _repository.GetRandomStatusEffectDescriptor(
            effectType: effectType,
            applicationContext: StatusEffectDescriptor.ApplicationContexts.OnApply,
            severity: null,
            sourceType: sourceType,
            targetType: isPlayer ? StatusEffectDescriptor.TargetTypes.Player : StatusEffectDescriptor.TargetTypes.Enemy);

        if (descriptor == null)
        {
            _log.Warning("No application descriptor found for effect {Effect}, source {Source}. Using fallback.",
                effectType, sourceType);
            return GenerateFallbackApplicationText(effectType, targetName, isPlayer);
        }

        // Check for environmental context
        string? environmentalFlavor = null;
        if (!string.IsNullOrEmpty(biomeName))
        {
            var envContext = _repository.GetRandomEnvironmentalContext(
                effectType, biomeName, StatusEffectDescriptor.ApplicationContexts.OnApply);

            if (envContext != null)
            {
                environmentalFlavor = FillTemplate(envContext.EnvironmentalDescriptor, new Dictionary<string, string>
                {
                    {"Effect", effectType},
                    {"Biome", biomeName},
                    {"Target", targetName}
                });
            }
        }

        // Fill template variables
        var text = FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Target", targetName},
            {"Enemy", sourceName ?? "enemy"},
            {"Source", sourceName ?? sourceType ?? "unknown source"},
            {"SourceDetail", sourceDetail ?? ""},
            {"Location", SelectRandom(_bodyLocations)},
            {"Vital_Location", SelectRandom(_vitalLocations)}
        });

        // Append environmental flavor if present
        if (!string.IsNullOrEmpty(environmentalFlavor))
        {
            text += " " + environmentalFlavor;
        }

        return text;
    }

    #endregion

    #region Status Effect Tick (OnTick)

    /// <summary>
    /// Generates flavor text for status effect tick (ongoing damage/effect)
    /// </summary>
    public string GenerateTickText(
        string effectType,
        string targetName,
        int damageAmount,
        int? stackCount = null,
        string? biomeName = null,
        bool isPlayer = true)
    {
        // Determine severity based on damage or stacks
        string? severity = null;
        if (damageAmount > 0)
        {
            severity = _repository.DetermineSeverityByDamage(effectType, damageAmount);
        }
        else if (stackCount.HasValue)
        {
            severity = _repository.DetermineSeverityByStacks(effectType, stackCount.Value);
        }

        // Get descriptor for tick
        var descriptor = _repository.GetRandomStatusEffectDescriptor(
            effectType: effectType,
            applicationContext: StatusEffectDescriptor.ApplicationContexts.OnTick,
            severity: severity,
            sourceType: null,
            targetType: isPlayer ? StatusEffectDescriptor.TargetTypes.Player : StatusEffectDescriptor.TargetTypes.Enemy);

        if (descriptor == null)
        {
            _log.Warning("No tick descriptor found for effect {Effect}, severity {Severity}. Using fallback.",
                effectType, severity);
            return GenerateFallbackTickText(effectType, targetName, damageAmount, isPlayer);
        }

        // Check for environmental context
        string? environmentalFlavor = null;
        if (!string.IsNullOrEmpty(biomeName))
        {
            var envContext = _repository.GetRandomEnvironmentalContext(
                effectType, biomeName, StatusEffectDescriptor.ApplicationContexts.OnTick);

            if (envContext != null)
            {
                environmentalFlavor = FillTemplate(envContext.EnvironmentalDescriptor, new Dictionary<string, string>
                {
                    {"Effect", effectType},
                    {"Biome", biomeName},
                    {"Target", targetName}
                });
            }
        }

        // Fill template variables
        var text = FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Target", targetName},
            {"Enemy", targetName},
            {"Damage", damageAmount.ToString()},
            {"StackCount", stackCount?.ToString() ?? "0"},
            {"Severity", severity ?? "Unknown"}
        });

        // Append environmental flavor if present
        if (!string.IsNullOrEmpty(environmentalFlavor))
        {
            text += " " + environmentalFlavor;
        }

        return text;
    }

    #endregion

    #region Status Effect End (OnExpire/OnRemove)

    /// <summary>
    /// Generates flavor text for status effect expiration or removal
    /// </summary>
    public string GenerateEndText(
        string effectType,
        string targetName,
        bool wasRemoved = false,
        string? removalMethod = null,
        bool isCatastrophic = false,
        bool isPlayer = true)
    {
        // Determine context and source type
        var context = wasRemoved
            ? StatusEffectDescriptor.ApplicationContexts.OnRemove
            : StatusEffectDescriptor.ApplicationContexts.OnExpire;

        var severity = isCatastrophic ? StatusEffectDescriptor.SeverityLevels.Catastrophic : null;

        // Get descriptor for end
        var descriptor = _repository.GetRandomStatusEffectDescriptor(
            effectType: effectType,
            applicationContext: context,
            severity: severity,
            sourceType: removalMethod,
            targetType: isPlayer ? StatusEffectDescriptor.TargetTypes.Player : StatusEffectDescriptor.TargetTypes.Enemy);

        if (descriptor == null)
        {
            _log.Warning("No end descriptor found for effect {Effect}, method {Method}. Using fallback.",
                effectType, removalMethod);
            return GenerateFallbackEndText(effectType, targetName, wasRemoved, isPlayer);
        }

        // Fill template variables
        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Target", targetName},
            {"Enemy", targetName},
            {"EffectType", effectType}
        });
    }

    #endregion

    #region Status Effect Interactions

    /// <summary>
    /// Generates flavor text for status effect interactions
    /// </summary>
    public string? GenerateInteractionText(
        string effectType1,
        string effectType2,
        string targetName,
        string interactionType)
    {
        var descriptor = _repository.GetRandomInteractionDescriptor(effectType1, effectType2, interactionType);

        if (descriptor == null)
        {
            _log.Debug("No interaction descriptor found for {Effect1} + {Effect2} ({Type})",
                effectType1, effectType2, interactionType);
            return null;
        }

        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Target", targetName},
            {"Effect1", effectType1},
            {"Effect2", effectType2},
            {"ResultEffect", descriptor.ResultEffect ?? "unknown"}
        });
    }

    #endregion

    #region Template Helpers

    /// <summary>
    /// Fills template variables in descriptor text
    /// </summary>
    private string FillTemplate(string template, Dictionary<string, string> variables)
    {
        return _templateVariablePattern.Replace(template, match =>
        {
            var variableName = match.Groups[1].Value;
            return variables.TryGetValue(variableName, out var value) ? value : match.Value;
        });
    }

    /// <summary>
    /// Selects a random element from an array
    /// </summary>
    private string SelectRandom(string[] options)
    {
        return options[_random.Next(options.Length)];
    }

    #endregion

    #region Fallback Text

    /// <summary>
    /// Generates fallback text when no descriptor is found for application
    /// </summary>
    private string GenerateFallbackApplicationText(string effectType, string targetName, bool isPlayer)
    {
        if (isPlayer)
        {
            return effectType switch
            {
                "Burning" => "You're burning!",
                "Bleeding" => "You're bleeding!",
                "Poisoned" => "You're poisoned!",
                "Stunned" => "You're stunned!",
                "Slowed" => "You're slowed!",
                "Weakened" => "You're weakened!",
                "BlightCorruption" => "The Blight corrupts you!",
                _ => $"You're afflicted with {effectType}!"
            };
        }
        else
        {
            return $"The {targetName} is afflicted with {effectType}!";
        }
    }

    /// <summary>
    /// Generates fallback text when no descriptor is found for tick
    /// </summary>
    private string GenerateFallbackTickText(string effectType, string targetName, int damage, bool isPlayer)
    {
        if (damage > 0)
        {
            if (isPlayer)
            {
                return effectType switch
                {
                    "Burning" => $"The flames burn you for {damage} damage!",
                    "Bleeding" => $"You lose {damage} HP from bleeding!",
                    "Poisoned" => $"The poison damages you for {damage} HP!",
                    _ => $"{effectType} deals {damage} damage!"
                };
            }
            else
            {
                return $"The {targetName} takes {damage} damage from {effectType}!";
            }
        }

        if (isPlayer)
        {
            return effectType switch
            {
                "Stunned" => "You're still stunned!",
                "Slowed" => "You're still slowed!",
                "Weakened" => "You remain weakened!",
                _ => $"{effectType} continues to affect you!"
            };
        }
        else
        {
            return $"The {targetName} is still affected by {effectType}!";
        }
    }

    /// <summary>
    /// Generates fallback text when no descriptor is found for end
    /// </summary>
    private string GenerateFallbackEndText(string effectType, string targetName, bool wasRemoved, bool isPlayer)
    {
        if (isPlayer)
        {
            if (wasRemoved)
            {
                return effectType switch
                {
                    "Burning" => "You extinguish the flames!",
                    "Bleeding" => "You bandage the wound!",
                    "Poisoned" => "The antidote takes effect!",
                    _ => $"You remove {effectType}!"
                };
            }
            else
            {
                return effectType switch
                {
                    "Burning" => "The flames die out.",
                    "Bleeding" => "The bleeding stops.",
                    "Poisoned" => "The poison fades.",
                    "Stunned" => "You recover from the stun.",
                    "Slowed" => "Your speed returns.",
                    _ => $"{effectType} fades away."
                };
            }
        }
        else
        {
            return wasRemoved
                ? $"The {targetName} is cured of {effectType}!"
                : $"{effectType} fades from the {targetName}.";
        }
    }

    #endregion
}
