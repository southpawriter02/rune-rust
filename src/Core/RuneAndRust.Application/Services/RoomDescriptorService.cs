using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implements three-tier descriptor composition for procedural room description generation.
/// </summary>
/// <remarks>
/// Composition process:
/// 1. Start with base template (Tier 1)
/// 2. Apply thematic modifier tokens (Tier 2)
/// 3. Fill fragment placeholders via weighted selection (Tier 3)
/// </remarks>
public class RoomDescriptorService : IRoomDescriptorService
{
    private readonly IDescriptorRepository _repository;

    public RoomDescriptorService(IDescriptorRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public string GenerateRoomName(
        BaseDescriptorTemplate template,
        ThematicModifier modifier,
        RoomFunction? function = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(modifier);

        var name = template.NameTemplate;

        // Apply modifier tokens
        name = name
            .Replace("{Modifier}", modifier.Name)
            .Replace("{Modifier_Adj}", modifier.Adjective);

        // Apply function if present
        if (function != null)
        {
            name = name.Replace("{Function}", function.FunctionName);
        }

        return name;
    }

    public string GenerateRoomDescription(
        BaseDescriptorTemplate template,
        ThematicModifier modifier,
        IReadOnlyList<string> roomTags,
        Random random,
        RoomFunction? function = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(modifier);
        ArgumentNullException.ThrowIfNull(random);

        var description = template.DescriptionTemplate;
        var biome = modifier.PrimaryBiome;

        // Tier 2: Apply modifier tokens
        description = ApplyModifierTokens(description, modifier);

        // Tier 3: Fill fragment placeholders via weighted selection
        description = FillFragmentPlaceholders(description, biome, roomTags, random);

        // Apply article tokens based on following word
        description = ApplyArticleTokens(description);

        // Apply function if present
        if (function != null)
        {
            description = description.Replace("{Function}", function.FunctionDetail);
        }
        else
        {
            // Remove unfilled function placeholder
            description = description.Replace("{Function}", "");
        }

        // Clean up any double spaces from removed placeholders
        description = CleanupDescription(description);

        return description;
    }

    public string GenerateFeatureDescription(
        RoomFeature feature,
        Biome biome,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(feature);
        ArgumentNullException.ThrowIfNull(random);

        // Use the feature's descriptor override if available
        if (!string.IsNullOrEmpty(feature.DescriptorOverride))
            return feature.DescriptorOverride;

        // Otherwise, generate based on feature type
        var category = feature.Type switch
        {
            RoomFeatureType.Hazard => FragmentCategory.Detail,
            RoomFeatureType.Decoration => FragmentCategory.Architectural,
            RoomFeatureType.Interactable => FragmentCategory.Detail,
            RoomFeatureType.LightSource => FragmentCategory.Atmospheric,
            _ => FragmentCategory.Detail
        };

        var fragment = SelectFragment(category, biome, null, random);
        return fragment ?? $"A {feature.FeatureId} is here.";
    }

    private string ApplyModifierTokens(string description, ThematicModifier modifier)
    {
        return description
            .Replace("{Modifier}", modifier.Name)
            .Replace("{Modifier_Adj}", modifier.Adjective)
            .Replace("{Modifier_Detail}", modifier.DetailFragment);
    }

    private string FillFragmentPlaceholders(
        string description,
        Biome biome,
        IReadOnlyList<string> tags,
        Random random)
    {
        // Select fragments for each placeholder type
        var spatialFragment = SelectFragment(FragmentCategory.Spatial, biome, tags, random);
        var architecturalFragment = SelectFragment(FragmentCategory.Architectural, biome, tags, random);
        var detail1Fragment = SelectFragment(FragmentCategory.Detail, biome, tags, random);
        var detail2Fragment = SelectFragment(FragmentCategory.Detail, biome, tags, random);
        var directionFragment = SelectFragment(FragmentCategory.Direction, biome, tags, random);
        var atmosphericFragment = SelectFragment(FragmentCategory.Atmospheric, biome, tags, random);

        return description
            .Replace("{Spatial_Descriptor}", spatialFragment ?? "")
            .Replace("{Architectural_Feature}", architecturalFragment ?? "")
            .Replace("{Detail_1}", detail1Fragment ?? "")
            .Replace("{Detail_2}", detail2Fragment ?? "")
            .Replace("{Direction_Descriptor}", directionFragment ?? "")
            .Replace("{Atmospheric_Detail}", atmosphericFragment ?? "");
    }

    private string? SelectFragment(
        FragmentCategory category,
        Biome biome,
        IReadOnlyList<string>? tags,
        Random random)
    {
        // Try to find fragments matching biome and tags
        var candidates = _repository.GetFragments(category, biome, tags);

        if (candidates.Count == 0)
        {
            // Fallback: try without biome filter
            candidates = _repository.GetFragments(category, null, tags);
        }

        if (candidates.Count == 0)
        {
            // Fallback: try without any filters
            candidates = _repository.GetFragments(category);
        }

        if (candidates.Count == 0)
            return null;

        return SelectWeighted(candidates, random);
    }

    private static string SelectWeighted(IReadOnlyList<DescriptorFragment> fragments, Random random)
    {
        var totalWeight = fragments.Sum(f => f.Weight);
        var roll = random.NextDouble() * totalWeight;

        foreach (var fragment in fragments)
        {
            roll -= fragment.Weight;
            if (roll <= 0)
                return fragment.Text;
        }

        return fragments[^1].Text;
    }

    private static string ApplyArticleTokens(string description)
    {
        // Handle {Article} and {Article_Cap} tokens
        // These need to look ahead to the next word to determine a/an

        // Find {Article} followed by text and determine article
        var result = description;

        // Process {Article_Cap} first (capitalized version)
        while (result.Contains("{Article_Cap}"))
        {
            var index = result.IndexOf("{Article_Cap}", StringComparison.Ordinal);
            var afterToken = result[(index + 13)..].TrimStart();

            if (afterToken.Length > 0)
            {
                var article = BaseDescriptorTemplate.GetCapitalizedArticle(afterToken[0]);
                result = result[..index] + article + result[(index + 13)..];
            }
            else
            {
                result = result[..index] + "A" + result[(index + 13)..];
            }
        }

        // Process {Article} (lowercase version)
        while (result.Contains("{Article}"))
        {
            var index = result.IndexOf("{Article}", StringComparison.Ordinal);
            var afterToken = result[(index + 9)..].TrimStart();

            if (afterToken.Length > 0)
            {
                var article = BaseDescriptorTemplate.GetArticle(afterToken[0]);
                result = result[..index] + article + result[(index + 9)..];
            }
            else
            {
                result = result[..index] + "a" + result[(index + 9)..];
            }
        }

        return result;
    }

    private static string CleanupDescription(string description)
    {
        // Remove double spaces
        while (description.Contains("  "))
        {
            description = description.Replace("  ", " ");
        }

        // Remove space before punctuation
        description = description
            .Replace(" .", ".")
            .Replace(" ,", ",")
            .Replace(" ;", ";")
            .Replace(" :", ":");

        // Remove empty sentences (just periods)
        description = description
            .Replace(". .", ".")
            .Replace("..", ".");

        return description.Trim();
    }
}
