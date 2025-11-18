using RuneAndRust.Core.ExaminationFlavor;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.RegularExpressions;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.9: Service for generating dynamic examination and perception flavor text
/// Provides layered examination detail, perception check results, flora/fauna observations
/// </summary>
public class ExaminationFlavorTextService
{
    private static readonly ILogger _log = Log.ForContext<ExaminationFlavorTextService>();
    private readonly DescriptorRepository _repository;
    private readonly Random _random;

    // Template variable pattern
    private static readonly Regex _templateVariablePattern = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

    public ExaminationFlavorTextService(DescriptorRepository repository, Random? random = null)
    {
        _repository = repository;
        _random = random ?? new Random();
    }

    #region Object Examination

    /// <summary>
    /// Generates examination text based on WITS check result
    /// </summary>
    public string GenerateExaminationText(
        string objectCategory,
        string? objectType = null,
        int witsCheck = 0,
        string? objectState = null,
        string? biomeName = null)
    {
        // Determine detail level based on WITS check
        string detailLevel = DetermineDetailLevel(witsCheck);

        // Get descriptor
        var descriptor = _repository.GetRandomExaminationDescriptor(
            objectCategory,
            objectType,
            detailLevel,
            biomeName,
            objectState);

        if (descriptor == null)
        {
            _log.Warning("No examination descriptor found for {Category} {Type} {Level}",
                objectCategory, objectType, detailLevel);
            return GenerateFallbackExaminationText(objectCategory, objectType, detailLevel);
        }

        // Fill template variables
        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Object", objectType ?? objectCategory},
            {"ObjectState", objectState ?? "intact"},
            {"Biome", biomeName ?? "unknown"},
            {"Material", "iron"}, // Could be parameterized
            {"Age", "800 years"}, // Could be calculated
            {"Faction", "Jötun"}, // Could be parameterized
            {"Era", "Pre-Blight"} // Could be parameterized
        });
    }

    /// <summary>
    /// Determines examination detail level based on WITS check
    /// </summary>
    private string DetermineDetailLevel(int witsCheck)
    {
        if (witsCheck >= 18)
            return ExaminationDescriptor.DetailLevels.Expert;
        else if (witsCheck >= 12)
            return ExaminationDescriptor.DetailLevels.Detailed;
        else
            return ExaminationDescriptor.DetailLevels.Cursory;
    }

    /// <summary>
    /// Gets layered examination text (all levels) for progressive revelation
    /// </summary>
    public List<string> GetLayeredExamination(
        string objectCategory,
        string? objectType = null,
        string? objectState = null,
        string? biomeName = null)
    {
        var layers = new List<string>();

        // Cursory
        var cursory = _repository.GetRandomExaminationDescriptor(
            objectCategory, objectType, ExaminationDescriptor.DetailLevels.Cursory, biomeName, objectState);
        if (cursory != null)
            layers.Add(FillExaminationTemplate(cursory.DescriptorText));

        // Detailed
        var detailed = _repository.GetRandomExaminationDescriptor(
            objectCategory, objectType, ExaminationDescriptor.DetailLevels.Detailed, biomeName, objectState);
        if (detailed != null)
            layers.Add(FillExaminationTemplate(detailed.DescriptorText));

        // Expert
        var expert = _repository.GetRandomExaminationDescriptor(
            objectCategory, objectType, ExaminationDescriptor.DetailLevels.Expert, biomeName, objectState);
        if (expert != null)
            layers.Add(FillExaminationTemplate(expert.DescriptorText));

        return layers;
    }

    private string FillExaminationTemplate(string template)
    {
        return FillTemplate(template, new Dictionary<string, string>
        {
            {"Object", "object"},
            {"ObjectState", "intact"},
            {"Biome", "unknown"},
            {"Material", "iron"},
            {"Age", "ancient"},
            {"Faction", "Jötun"},
            {"Era", "Pre-Blight"}
        });
    }

    #endregion

    #region Perception Checks

    /// <summary>
    /// Generates perception check success text
    /// </summary>
    public string GeneratePerceptionCheckText(
        string detectionType,
        int checkResult,
        string? biomeName = null)
    {
        var descriptor = _repository.GetRandomPerceptionCheckDescriptor(
            detectionType,
            checkResult,
            biomeName);

        if (descriptor == null)
        {
            _log.Warning("No perception descriptor found for {Type} DC {DC}",
                detectionType, checkResult);
            return GenerateFallbackPerceptionText(detectionType, checkResult);
        }

        // Main description
        var text = FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"HiddenElement", detectionType},
            {"DetectionCue", "visual discrepancy"},
            {"Biome", biomeName ?? "unknown"}
        });

        // Add expert insight if high DC success
        if (!string.IsNullOrEmpty(descriptor.ExpertInsight) && checkResult >= 20)
        {
            text += " " + FillTemplate(descriptor.ExpertInsight, new Dictionary<string, string>
            {
                {"HiddenElement", detectionType},
                {"Biome", biomeName ?? "unknown"}
            });
        }

        return text;
    }

    /// <summary>
    /// Checks if a perception roll succeeds for a given DC
    /// </summary>
    public bool CheckPerception(int perceptionRoll, int difficultyClass)
    {
        return perceptionRoll >= difficultyClass;
    }

    #endregion

    #region Flora Observation

    /// <summary>
    /// Generates flora observation text
    /// </summary>
    public string GenerateFloraObservation(
        string biomeName,
        string? floraName = null,
        int witsCheck = 0)
    {
        string detailLevel = DetermineDetailLevel(witsCheck);

        List<FloraDescriptor> descriptors;

        if (!string.IsNullOrEmpty(floraName))
        {
            // Specific flora requested
            descriptors = _repository.GetFloraDescriptors(
                floraName: floraName,
                detailLevel: detailLevel,
                biomeName: biomeName);
        }
        else
        {
            // Random flora for biome
            var flora = _repository.GetRandomFloraDescriptor(biomeName, detailLevel);
            descriptors = flora != null ? new List<FloraDescriptor> { flora } : new List<FloraDescriptor>();
        }

        if (descriptors.Count == 0)
        {
            _log.Warning("No flora descriptors found for {Biome} {Flora} {Level}",
                biomeName, floraName, detailLevel);
            return GenerateFallbackFloraText(biomeName);
        }

        var descriptor = descriptors[_random.Next(descriptors.Count)];

        var text = FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Species", descriptor.FloraName},
            {"Biome", biomeName},
            {"AlchemyUse", descriptor.AlchemyUse ?? "unknown"}
        });

        // Add harvest/danger warnings
        if (descriptor.IsHarvestable && descriptor.IsDangerous)
        {
            text += " [Harvestable - Dangerous]";
        }
        else if (descriptor.IsHarvestable)
        {
            text += " [Harvestable]";
        }
        else if (descriptor.IsDangerous)
        {
            text += " [Dangerous]";
        }

        return text;
    }

    /// <summary>
    /// Gets harvestable flora in a biome
    /// </summary>
    public List<FloraDescriptor> GetHarvestableFlora(string biomeName)
    {
        return _repository.GetFloraDescriptors(
            biomeName: biomeName,
            harvestableOnly: true);
    }

    #endregion

    #region Fauna Observation

    /// <summary>
    /// Generates fauna observation text
    /// </summary>
    public string GenerateFaunaObservation(
        string? biomeName = null,
        string? observationType = null,
        bool includeExpertInsight = false)
    {
        var descriptor = _repository.GetRandomFaunaDescriptor(biomeName, observationType);

        if (descriptor == null)
        {
            _log.Warning("No fauna descriptors found for biome {Biome}",
                biomeName ?? "any");
            return "The area seems devoid of animal life.";
        }

        var text = FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Species", descriptor.CreatureName},
            {"Behavior", descriptor.EcologicalRole ?? "unknown"},
            {"Biome", biomeName ?? "unknown"}
        });

        // Add expert insight if requested and available
        if (includeExpertInsight && !string.IsNullOrEmpty(descriptor.ExpertInsight))
        {
            text += "\n\n" + FillTemplate(descriptor.ExpertInsight, new Dictionary<string, string>
            {
                {"Species", descriptor.CreatureName},
                {"Biome", biomeName ?? "unknown"}
            });
        }

        return text;
    }

    /// <summary>
    /// Gets random ambient creature sighting for atmosphere
    /// </summary>
    public string? GetRandomAmbientCreature(string? biomeName = null)
    {
        var descriptor = _repository.GetRandomFaunaDescriptor(
            biomeName,
            FaunaDescriptor.ObservationTypes.Sighting);

        if (descriptor == null)
            return null;

        return FillTemplate(descriptor.DescriptorText, new Dictionary<string, string>
        {
            {"Species", descriptor.CreatureName},
            {"Biome", biomeName ?? "unknown"}
        });
    }

    #endregion

    #region Lore Fragments

    /// <summary>
    /// Gets lore fragment revealed by expert examination
    /// </summary>
    public ExaminationLoreFragment? GetLoreFragment(
        string? objectType = null,
        string detailLevel = "Expert",
        string? biomeName = null)
    {
        var fragments = _repository.GetExaminationLoreFragments(
            relatedObjectType: objectType,
            requiredDetailLevel: detailLevel,
            biomeName: biomeName);

        if (fragments.Count == 0)
            return null;

        return fragments[_random.Next(fragments.Count)];
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

    #endregion

    #region Fallback Text

    private string GenerateFallbackExaminationText(string category, string? type, string level)
    {
        return level switch
        {
            "Expert" => $"Upon expert examination, you discern intricate details about this {type ?? category}.",
            "Detailed" => $"A closer look reveals more about this {type ?? category}.",
            _ => $"You see a {type ?? category}."
        };
    }

    private string GenerateFallbackPerceptionText(string detectionType, int checkResult)
    {
        var success = checkResult >= 20 ? "expertly" : "successfully";
        return detectionType switch
        {
            "HiddenTrap" => $"You {success} spot a hidden trap!",
            "SecretDoor" => $"You {success} discover a secret passage!",
            "HiddenCache" => $"You {success} find a hidden cache!",
            _ => $"You {success} notice something hidden: {detectionType}!"
        };
    }

    private string GenerateFallbackFloraText(string biomeName)
    {
        return biomeName switch
        {
            "Muspelheim" => "Heat-adapted vegetation grows here, surviving in extreme temperatures.",
            "Niflheim" => "Hardy frost-resistant plants cling to life in the frozen wastes.",
            "Alfheim" => "Strange, reality-warped growths defy natural classification.",
            "The_Roots" => "Bioluminescent fungi provide eerie illumination.",
            _ => "Various plants and fungi grow in this environment."
        };
    }

    #endregion
}
