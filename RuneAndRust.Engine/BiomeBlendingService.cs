using RuneAndRust.Core;
using Serilog;
using System.Text;
using System.Text.RegularExpressions;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.39.2: Service for blending descriptors from multiple biomes
/// Merges names, descriptions, and sensory details based on blend ratios
/// </summary>
public class BiomeBlendingService
{
    private static readonly ILogger _log = Log.ForContext<BiomeBlendingService>();

    public BiomeBlendingService()
    {
        _log.Information("BiomeBlendingService initialized");
    }

    /// <summary>
    /// Blends descriptors from two biomes based on their weights
    /// </summary>
    /// <param name="fromBiome">Source biome</param>
    /// <param name="toBiome">Destination biome</param>
    /// <param name="fromWeight">Weight of source biome (0.0 to 1.0)</param>
    /// <param name="toWeight">Weight of destination biome (0.0 to 1.0)</param>
    /// <param name="rng">Random number generator</param>
    /// <returns>Blended descriptor combining elements from both biomes</returns>
    public BlendedDescriptor BlendBiomeDescriptors(
        BiomeDefinition fromBiome,
        BiomeDefinition toBiome,
        float fromWeight,
        float toWeight,
        Random rng)
    {
        _log.Debug("Blending descriptors: {FromBiome} ({FromWeight:P0}) + {ToBiome} ({ToWeight:P0})",
            fromBiome.BiomeId, fromWeight, toBiome.BiomeId, toWeight);

        var blendedDescriptor = new BlendedDescriptor();

        // Blend name
        blendedDescriptor.Name = BlendNames(fromBiome, toBiome, fromWeight, toWeight, rng);

        // Blend description
        blendedDescriptor.Description = BlendDescriptions(fromBiome, toBiome, fromWeight, toWeight, rng);

        // Blend adjectives
        blendedDescriptor.Adjectives = BlendList(
            GetDescriptorList(fromBiome, "Adjectives"),
            GetDescriptorList(toBiome, "Adjectives"),
            fromWeight, toWeight, rng, maxItems: 3);

        // Blend sounds
        blendedDescriptor.Sounds = BlendList(
            GetDescriptorList(fromBiome, "Sounds"),
            GetDescriptorList(toBiome, "Sounds"),
            fromWeight, toWeight, rng, maxItems: 2);

        // Blend smells
        blendedDescriptor.Smells = BlendList(
            GetDescriptorList(fromBiome, "Smells"),
            GetDescriptorList(toBiome, "Smells"),
            fromWeight, toWeight, rng, maxItems: 2);

        // Blend details
        var fromDetails = GetDescriptorList(fromBiome, "Details");
        var toDetails = GetDescriptorList(toBiome, "Details");
        var blendedDetails = BlendList(fromDetails, toDetails, fromWeight, toWeight, rng, maxItems: 3);

        foreach (var detail in blendedDetails)
        {
            var key = $"detail_{blendedDescriptor.Details.Count + 1}";
            blendedDescriptor.Details[key] = detail;
        }

        _log.Information("Blended descriptor created: {Name}", blendedDescriptor.Name);

        return blendedDescriptor;
    }

    /// <summary>
    /// Blends room names from two biomes
    /// </summary>
    private string BlendNames(
        BiomeDefinition fromBiome,
        BiomeDefinition toBiome,
        float fromWeight,
        float toWeight,
        Random rng)
    {
        // Extract sample nouns and adjectives from biome names
        var fromAdjectives = GetDescriptorList(fromBiome, "Adjectives");
        var toAdjectives = GetDescriptorList(toBiome, "Adjectives");

        // Determine blend category
        var blendCategory = GetBlendCategory(fromWeight, toWeight);

        // Generate name based on blend category
        return blendCategory switch
        {
            BlendCategory.MostlyFrom => GenerateWeightedName(fromBiome, fromAdjectives, rng),
            BlendCategory.Balanced => GenerateTransitionalName(fromBiome, toBiome, fromAdjectives, toAdjectives, rng),
            BlendCategory.MostlyTo => GenerateWeightedName(toBiome, toAdjectives, rng),
            _ => "Unknown Chamber"
        };
    }

    /// <summary>
    /// Blends descriptions from two biomes
    /// </summary>
    private string BlendDescriptions(
        BiomeDefinition fromBiome,
        BiomeDefinition toBiome,
        float fromWeight,
        float toWeight,
        Random rng)
    {
        var sb = new StringBuilder();

        // Get details from both biomes
        var fromDetails = GetDescriptorList(fromBiome, "Details");
        var toDetails = GetDescriptorList(toBiome, "Details");

        // Select details based on weights
        var selectedFromDetails = WeightedSelectMultiple(fromDetails, fromWeight, rng, maxItems: 2);
        var selectedToDetails = WeightedSelectMultiple(toDetails, toWeight, rng, maxItems: 2);

        // Build description with dominant biome first
        if (fromWeight > toWeight)
        {
            foreach (var detail in selectedFromDetails)
            {
                sb.Append(detail);
                sb.Append(" ");
            }

            foreach (var detail in selectedToDetails)
            {
                sb.Append(detail);
                sb.Append(" ");
            }
        }
        else
        {
            foreach (var detail in selectedToDetails)
            {
                sb.Append(detail);
                sb.Append(" ");
            }

            foreach (var detail in selectedFromDetails)
            {
                sb.Append(detail);
                sb.Append(" ");
            }
        }

        // Add sensory details
        var sounds = BlendList(
            GetDescriptorList(fromBiome, "Sounds"),
            GetDescriptorList(toBiome, "Sounds"),
            fromWeight, toWeight, rng, maxItems: 1);

        if (sounds.Any())
        {
            sb.Append($"You hear {sounds[0]}. ");
        }

        var smells = BlendList(
            GetDescriptorList(fromBiome, "Smells"),
            GetDescriptorList(toBiome, "Smells"),
            fromWeight, toWeight, rng, maxItems: 1);

        if (smells.Any())
        {
            sb.Append($"The air smells of {smells[0]}.");
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Blends two lists based on weights, randomly selecting items from each
    /// </summary>
    private List<string> BlendList(
        List<string> fromList,
        List<string> toList,
        float fromWeight,
        float toWeight,
        Random rng,
        int maxItems)
    {
        var result = new List<string>();

        // Calculate how many items to take from each list
        var fromCount = (int)Math.Round(maxItems * fromWeight);
        var toCount = (int)Math.Round(maxItems * toWeight);

        // Ensure we have at least the requested number of items
        if (fromCount + toCount < maxItems && fromList.Any() && toList.Any())
        {
            if (fromWeight > toWeight)
                fromCount++;
            else
                toCount++;
        }

        // Select from fromList
        var selectedFrom = WeightedSelectMultiple(fromList, fromWeight, rng, fromCount);
        result.AddRange(selectedFrom);

        // Select from toList
        var selectedTo = WeightedSelectMultiple(toList, toWeight, rng, toCount);
        result.AddRange(selectedTo);

        // Shuffle and limit
        result = result.OrderBy(x => rng.Next()).Take(maxItems).ToList();

        return result;
    }

    /// <summary>
    /// Weighted selection of multiple items from a list
    /// </summary>
    private List<string> WeightedSelectMultiple(List<string> items, float weight, Random rng, int count)
    {
        if (!items.Any() || count <= 0)
            return new List<string>();

        // Don't select if weight is too low
        if (weight < 0.1f)
            return new List<string>();

        var result = new List<string>();
        var availableItems = new List<string>(items);

        for (int i = 0; i < Math.Min(count, availableItems.Count); i++)
        {
            var selectedIndex = rng.Next(availableItems.Count);
            result.Add(availableItems[selectedIndex]);
            availableItems.RemoveAt(selectedIndex);
        }

        return result;
    }

    /// <summary>
    /// Generates a name weighted toward a specific biome
    /// </summary>
    private string GenerateWeightedName(BiomeDefinition biome, List<string> adjectives, Random rng)
    {
        var adjective = adjectives.Any() ? adjectives[rng.Next(adjectives.Count)] : "Unknown";
        var noun = GetRandomRoomNoun(rng);

        return $"{adjective} {noun}";
    }

    /// <summary>
    /// Generates a transitional name indicating blend state
    /// </summary>
    private string GenerateTransitionalName(
        BiomeDefinition fromBiome,
        BiomeDefinition toBiome,
        List<string> fromAdjectives,
        List<string> toAdjectives,
        Random rng)
    {
        var transitionWords = new[] { "Transitional", "Liminal", "Shifting", "Changing", "Intermediate" };
        var transitionWord = transitionWords[rng.Next(transitionWords.Length)];

        var noun = GetRandomRoomNoun(rng);

        // Optionally add a blended adjective
        if (rng.NextDouble() < 0.5)
        {
            var adjective = rng.NextDouble() < 0.5
                ? (fromAdjectives.Any() ? fromAdjectives[rng.Next(fromAdjectives.Count)] : "")
                : (toAdjectives.Any() ? toAdjectives[rng.Next(toAdjectives.Count)] : "");

            if (!string.IsNullOrEmpty(adjective))
            {
                return $"{transitionWord} {adjective} {noun}";
            }
        }

        return $"{transitionWord} {noun}";
    }

    /// <summary>
    /// Gets a random room noun for naming
    /// </summary>
    private string GetRandomRoomNoun(Random rng)
    {
        var nouns = new[]
        {
            "Chamber", "Corridor", "Hall", "Passage", "Room",
            "Vault", "Gallery", "Junction", "Atrium", "Space"
        };

        return nouns[rng.Next(nouns.Length)];
    }

    /// <summary>
    /// Gets a descriptor list from a biome's descriptor categories
    /// </summary>
    private List<string> GetDescriptorList(BiomeDefinition biome, string category)
    {
        if (biome.DescriptorCategories.TryGetValue(category, out var list))
        {
            return list;
        }

        return new List<string>();
    }

    /// <summary>
    /// Categorizes the blend ratio
    /// </summary>
    private BlendCategory GetBlendCategory(float fromWeight, float toWeight)
    {
        var difference = Math.Abs(fromWeight - toWeight);

        if (difference < 0.2f)
        {
            return BlendCategory.Balanced;
        }

        return fromWeight > toWeight ? BlendCategory.MostlyFrom : BlendCategory.MostlyTo;
    }
}

/// <summary>
/// Categories for blend ratios
/// </summary>
internal enum BlendCategory
{
    MostlyFrom,    // > 60% from primary biome
    Balanced,      // 40-60% blend
    MostlyTo       // > 60% from secondary biome
}
