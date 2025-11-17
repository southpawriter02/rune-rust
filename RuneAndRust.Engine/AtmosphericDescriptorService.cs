using RuneAndRust.Core.Descriptors;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.4: Atmospheric Descriptor Service
/// Generates multi-sensory atmospheric descriptions for rooms
/// </summary>
public class AtmosphericDescriptorService
{
    private static readonly ILogger _log = Log.ForContext<AtmosphericDescriptorService>();
    private readonly DescriptorRepository _repository;
    private readonly Random _random;

    public AtmosphericDescriptorService(DescriptorRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _random = new Random();
        _log.Information("AtmosphericDescriptorService initialized");
    }

    /// <summary>
    /// Generates full atmospheric description for a room based on biome and intensity
    /// </summary>
    /// <param name="biomeName">Biome name (e.g., "The_Roots", "Muspelheim")</param>
    /// <param name="intensity">Atmospheric intensity level (Subtle, Moderate, Oppressive)</param>
    /// <returns>Composed atmospheric description</returns>
    public string GenerateAtmosphere(string biomeName, AtmosphericIntensity intensity)
    {
        _log.Debug("Generating atmosphere for biome {Biome} with intensity {Intensity}",
            biomeName, intensity);

        // Get biome atmosphere profile
        var profile = _repository.GetBiomeAtmosphereProfile(biomeName);

        if (profile == null)
        {
            _log.Warning("No atmosphere profile found for biome {Biome}, using generic atmosphere",
                biomeName);
            return GenerateGenericAtmosphere(intensity);
        }

        // Select descriptors from each category
        var lighting = SelectDescriptorFromCategory(
            profile.GetLightingDescriptorIds(),
            AtmosphericCategory.Lighting,
            intensity,
            biomeName);

        var sound = SelectDescriptorFromCategory(
            profile.GetSoundDescriptorIds(),
            AtmosphericCategory.Sound,
            intensity,
            biomeName);

        var smell = SelectDescriptorFromCategory(
            profile.GetSmellDescriptorIds(),
            AtmosphericCategory.Smell,
            intensity,
            biomeName);

        var temperature = SelectDescriptorFromCategory(
            profile.GetTemperatureDescriptorIds(),
            AtmosphericCategory.Temperature,
            intensity,
            biomeName);

        var psychic = SelectDescriptorFromCategory(
            profile.GetPsychicDescriptorIds(),
            AtmosphericCategory.PsychicPresence,
            intensity,
            biomeName);

        // Compose using template
        var result = profile.CompositeTemplate
            .Replace("{Lighting}", lighting)
            .Replace("{Sound}", sound)
            .Replace("{Smell}", smell)
            .Replace("{Temperature}", temperature)
            .Replace("{Psychic}", psychic);

        _log.Debug("Generated atmosphere: {Atmosphere}", result);
        return result;
    }

    /// <summary>
    /// Generates full atmospheric description using biome's default intensity
    /// </summary>
    public string GenerateAtmosphere(string biomeName)
    {
        var profile = _repository.GetBiomeAtmosphereProfile(biomeName);
        var intensity = profile?.DefaultIntensity ?? AtmosphericIntensity.Moderate;
        return GenerateAtmosphere(biomeName, intensity);
    }

    /// <summary>
    /// Generates atmospheric description for a single category
    /// </summary>
    public string GenerateCategoryAtmosphere(
        string biomeName,
        AtmosphericCategory category,
        AtmosphericIntensity intensity)
    {
        _log.Debug("Generating {Category} atmosphere for biome {Biome}",
            category, biomeName);

        var profile = _repository.GetBiomeAtmosphereProfile(biomeName);

        if (profile == null)
        {
            // Fall back to generic descriptors
            var genericDescriptors = _repository.GetAtmosphericDescriptorsByCategory(
                category,
                intensity,
                null);

            if (genericDescriptors.Count == 0)
            {
                _log.Warning("No generic descriptors found for category {Category}", category);
                return string.Empty;
            }

            return genericDescriptors[_random.Next(genericDescriptors.Count)].DescriptorText;
        }

        var descriptorIds = category switch
        {
            AtmosphericCategory.Lighting => profile.GetLightingDescriptorIds(),
            AtmosphericCategory.Sound => profile.GetSoundDescriptorIds(),
            AtmosphericCategory.Smell => profile.GetSmellDescriptorIds(),
            AtmosphericCategory.Temperature => profile.GetTemperatureDescriptorIds(),
            AtmosphericCategory.PsychicPresence => profile.GetPsychicDescriptorIds(),
            _ => new List<int>()
        };

        return SelectDescriptorFromCategory(descriptorIds, category, intensity, biomeName);
    }

    #region Helper Methods

    /// <summary>
    /// Selects a descriptor from a list of descriptor IDs based on intensity preference
    /// </summary>
    private string SelectDescriptorFromCategory(
        List<int> descriptorIds,
        AtmosphericCategory category,
        AtmosphericIntensity preferredIntensity,
        string biomeName)
    {
        if (descriptorIds.Count == 0)
        {
            _log.Warning("No descriptor IDs provided for category {Category}", category);
            return string.Empty;
        }

        // Load descriptors
        var descriptors = _repository.GetAtmosphericDescriptorsByIds(descriptorIds);

        if (descriptors.Count == 0)
        {
            _log.Warning("No descriptors found for IDs in category {Category}", category);
            return string.Empty;
        }

        // Filter by intensity and biome suitability
        var suitableDescriptors = descriptors
            .Where(d => d.IsSuitableForBiome(biomeName))
            .ToList();

        if (suitableDescriptors.Count == 0)
        {
            _log.Warning("No suitable descriptors found for biome {Biome}, category {Category}",
                biomeName, category);
            suitableDescriptors = descriptors; // Fall back to all
        }

        // Try to match intensity
        var preferredDescriptors = suitableDescriptors
            .Where(d => d.Intensity == preferredIntensity)
            .ToList();

        if (preferredDescriptors.Count > 0)
        {
            var selected = preferredDescriptors[_random.Next(preferredDescriptors.Count)];
            _log.Verbose("Selected {Category} descriptor (ID {Id}): {Text}",
                category, selected.DescriptorId, selected.DescriptorText);
            return selected.DescriptorText;
        }

        // Fallback: Select any suitable descriptor
        var fallback = suitableDescriptors[_random.Next(suitableDescriptors.Count)];
        _log.Verbose("Selected fallback {Category} descriptor (ID {Id}): {Text}",
            category, fallback.DescriptorId, fallback.DescriptorText);
        return fallback.DescriptorText;
    }

    /// <summary>
    /// Generates a generic atmospheric description when no biome profile is available
    /// </summary>
    private string GenerateGenericAtmosphere(AtmosphericIntensity intensity)
    {
        _log.Debug("Generating generic atmosphere with intensity {Intensity}", intensity);

        var lighting = GetGenericDescriptor(AtmosphericCategory.Lighting, intensity);
        var sound = GetGenericDescriptor(AtmosphericCategory.Sound, intensity);
        var smell = GetGenericDescriptor(AtmosphericCategory.Smell, intensity);
        var temperature = GetGenericDescriptor(AtmosphericCategory.Temperature, intensity);
        var psychic = GetGenericDescriptor(AtmosphericCategory.PsychicPresence, intensity);

        return $"{lighting}. {sound}. {smell}. {temperature}. {psychic}.";
    }

    /// <summary>
    /// Gets a random generic descriptor for a category
    /// </summary>
    private string GetGenericDescriptor(AtmosphericCategory category, AtmosphericIntensity intensity)
    {
        var descriptors = _repository.GetAtmosphericDescriptorsByCategory(
            category,
            intensity,
            null); // null = generic only

        if (descriptors.Count == 0)
        {
            // Fallback to any intensity
            descriptors = _repository.GetAtmosphericDescriptorsByCategory(
                category,
                null,
                null);
        }

        if (descriptors.Count == 0)
        {
            _log.Warning("No generic descriptors found for category {Category}", category);
            return string.Empty;
        }

        return descriptors[_random.Next(descriptors.Count)].DescriptorText;
    }

    #endregion

    #region Validation & Utility

    /// <summary>
    /// Validates that atmospheric descriptor system is properly configured
    /// </summary>
    public bool ValidateAtmosphericSystem()
    {
        _log.Information("Validating atmospheric descriptor system...");

        var profiles = _repository.GetAllBiomeAtmosphereProfiles();
        if (profiles.Count == 0)
        {
            _log.Error("No biome atmosphere profiles found");
            return false;
        }

        var stats = _repository.GetAtmosphericDescriptorStats();
        if (stats.Count == 0)
        {
            _log.Error("No atmospheric descriptors found");
            return false;
        }

        _log.Information("Atmospheric system validation passed: {ProfileCount} profiles, {DescriptorCount} descriptors",
            profiles.Count, stats.Values.Sum());

        foreach (var kvp in stats)
        {
            _log.Information("  {Category}: {Count} descriptors", kvp.Key, kvp.Value);
        }

        return true;
    }

    /// <summary>
    /// Gets all available biome names with atmosphere profiles
    /// </summary>
    public List<string> GetAvailableBiomes()
    {
        return _repository.GetAllBiomeAtmosphereProfiles()
            .Select(p => p.BiomeName)
            .ToList();
    }

    /// <summary>
    /// Gets the default intensity for a biome
    /// </summary>
    public AtmosphericIntensity GetDefaultIntensity(string biomeName)
    {
        var profile = _repository.GetBiomeAtmosphereProfile(biomeName);
        return profile?.DefaultIntensity ?? AtmosphericIntensity.Moderate;
    }

    #endregion
}
