using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Persistence.Seeders;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// Implements IDescriptorRepository using seeder data.
/// Follows the pattern of SeederBasedRoomTemplateProvider with lazy loading.
/// </summary>
public class SeederBasedDescriptorRepository : IDescriptorRepository
{
    private readonly Lazy<IReadOnlyList<BaseDescriptorTemplate>> _baseTemplates;
    private readonly Lazy<IReadOnlyDictionary<Biome, ThematicModifier>> _modifiers;
    private readonly Lazy<IReadOnlyList<DescriptorFragment>> _fragments;
    private readonly Lazy<IReadOnlyList<RoomFunction>> _functions;

    public SeederBasedDescriptorRepository()
    {
        _baseTemplates = new Lazy<IReadOnlyList<BaseDescriptorTemplate>>(
            () => BaseDescriptorTemplateSeeder.GetAllTemplates().ToList());

        _modifiers = new Lazy<IReadOnlyDictionary<Biome, ThematicModifier>>(
            () => ThematicModifierSeeder.GetAllModifiers());

        _fragments = new Lazy<IReadOnlyList<DescriptorFragment>>(
            () => DescriptorFragmentSeeder.GetAllFragments().ToList());

        _functions = new Lazy<IReadOnlyList<RoomFunction>>(
            () => RoomFunctionSeeder.GetAllFunctions().ToList());
    }

    public BaseDescriptorTemplate? GetBaseTemplate(RoomArchetype archetype) =>
        _baseTemplates.Value.FirstOrDefault(t => t.Archetype == archetype);

    public IReadOnlyList<BaseDescriptorTemplate> GetAllBaseTemplates() =>
        _baseTemplates.Value;

    public ThematicModifier GetModifier(Biome biome) =>
        _modifiers.Value.TryGetValue(biome, out var modifier)
            ? modifier
            : _modifiers.Value[Biome.Citadel]; // Default fallback

    public IReadOnlyList<ThematicModifier> GetAllModifiers() =>
        _modifiers.Value.Values.ToList();

    public IReadOnlyList<DescriptorFragment> GetFragments(
        FragmentCategory category,
        Biome? biomeAffinity = null,
        IEnumerable<string>? tags = null)
    {
        var query = _fragments.Value.Where(f => f.Category == category);

        if (biomeAffinity.HasValue)
        {
            // Include fragments that either match the biome or have no biome affinity
            query = query.Where(f => f.MatchesBiome(biomeAffinity.Value));
        }

        if (tags != null && tags.Any())
        {
            var tagList = tags.ToList();
            query = query.Where(f => f.MatchesTags(tagList));
        }

        return query.ToList();
    }

    public IReadOnlyList<DescriptorFragment> GetFragmentsBySubcategory(
        FragmentCategory category,
        string subcategory,
        Biome? biomeAffinity = null)
    {
        var query = _fragments.Value
            .Where(f => f.Category == category)
            .Where(f => string.Equals(f.Subcategory, subcategory, StringComparison.OrdinalIgnoreCase));

        if (biomeAffinity.HasValue)
        {
            query = query.Where(f => f.MatchesBiome(biomeAffinity.Value));
        }

        return query.ToList();
    }

    public RoomFunction? GetFunction(string functionName) =>
        _functions.Value.FirstOrDefault(f =>
            f.FunctionName.Equals(functionName, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<RoomFunction> GetFunctionsByBiome(Biome biome) =>
        _functions.Value.Where(f => f.HasAffinityFor(biome)).ToList();

    public IReadOnlyList<RoomFunction> GetAllFunctions() =>
        _functions.Value;
}
