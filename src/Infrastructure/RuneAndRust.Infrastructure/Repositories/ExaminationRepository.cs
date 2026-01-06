using Microsoft.EntityFrameworkCore;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Persistence;

namespace RuneAndRust.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of examination descriptor repository.
/// </summary>
public class ExaminationRepository : IExaminationRepository
{
    private readonly GameDbContext _context;

    public ExaminationRepository(GameDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<ExaminationDescriptor>> GetDescriptorsAsync(
        ObjectCategory category,
        string objectType,
        Biome? biome = null,
        CancellationToken ct = default)
    {
        var query = _context.ExaminationDescriptors
            .Where(d => d.ObjectCategory == category && d.ObjectType == objectType);

        if (biome.HasValue)
        {
            query = query.Where(d => d.BiomeAffinity == null || d.BiomeAffinity == biome.Value);
        }

        return await query
            .OrderBy(d => d.Layer)
            .ThenByDescending(d => d.Weight)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ExaminationDescriptor>> GetDescriptorsByLayerAsync(
        ObjectCategory category,
        string objectType,
        ExaminationLayer maxLayer,
        Biome? biome = null,
        CancellationToken ct = default)
    {
        var query = _context.ExaminationDescriptors
            .Where(d => d.ObjectCategory == category
                && d.ObjectType == objectType
                && d.Layer <= maxLayer);

        if (biome.HasValue)
        {
            query = query.Where(d => d.BiomeAffinity == null || d.BiomeAffinity == biome.Value);
        }

        return await query
            .OrderBy(d => d.Layer)
            .ThenByDescending(d => d.Weight)
            .ToListAsync(ct);
    }

    public async Task AddDescriptorAsync(ExaminationDescriptor descriptor, CancellationToken ct = default)
    {
        await _context.ExaminationDescriptors.AddAsync(descriptor, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddDescriptorsAsync(IEnumerable<ExaminationDescriptor> descriptors, CancellationToken ct = default)
    {
        await _context.ExaminationDescriptors.AddRangeAsync(descriptors, ct);
        await _context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// EF Core implementation of perception descriptor repository.
/// </summary>
public class PerceptionRepository : IPerceptionRepository
{
    private readonly GameDbContext _context;

    public PerceptionRepository(GameDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<PerceptionDescriptor>> GetDescriptorsAsync(
        HiddenElementType elementType,
        PerceptionSuccessLevel? successLevel = null,
        Biome? biome = null,
        CancellationToken ct = default)
    {
        var query = _context.PerceptionDescriptors
            .Where(d => d.PerceptionType == elementType);

        if (successLevel.HasValue)
        {
            query = query.Where(d => d.SuccessLevel == successLevel.Value);
        }

        if (biome.HasValue)
        {
            query = query.Where(d => d.BiomeAffinity == null || d.BiomeAffinity == biome.Value);
        }

        return await query
            .OrderByDescending(d => d.Weight)
            .ToListAsync(ct);
    }

    public async Task AddDescriptorAsync(PerceptionDescriptor descriptor, CancellationToken ct = default)
    {
        await _context.PerceptionDescriptors.AddAsync(descriptor, ct);
        await _context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// EF Core implementation of flora/fauna descriptor repository.
/// </summary>
public class FloraFaunaRepository : IFloraFaunaRepository
{
    private readonly GameDbContext _context;

    public FloraFaunaRepository(GameDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<FloraFaunaDescriptor>> GetByBiomeAsync(
        Biome biome,
        FloraFaunaCategory? category = null,
        CancellationToken ct = default)
    {
        var query = _context.FloraFaunaDescriptors
            .Where(d => d.Biome == biome);

        if (category.HasValue)
        {
            query = query.Where(d => d.Category == category.Value);
        }

        return await query
            .OrderBy(d => d.SpeciesName)
            .ThenBy(d => d.Layer)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FloraFaunaDescriptor>> GetSpeciesDescriptorsAsync(
        string speciesName,
        ExaminationLayer maxLayer,
        CancellationToken ct = default)
    {
        return await _context.FloraFaunaDescriptors
            .Where(d => d.SpeciesName == speciesName && d.Layer <= maxLayer)
            .OrderBy(d => d.Layer)
            .ToListAsync(ct);
    }

    public async Task AddDescriptorAsync(FloraFaunaDescriptor descriptor, CancellationToken ct = default)
    {
        await _context.FloraFaunaDescriptors.AddAsync(descriptor, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddDescriptorsAsync(IEnumerable<FloraFaunaDescriptor> descriptors, CancellationToken ct = default)
    {
        await _context.FloraFaunaDescriptors.AddRangeAsync(descriptors, ct);
        await _context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// EF Core implementation of interaction descriptor repository.
/// </summary>
public class InteractionDescriptorRepository : IInteractionDescriptorRepository
{
    private readonly GameDbContext _context;
    private readonly Random _random = new();

    public InteractionDescriptorRepository(GameDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<InteractionDescriptor>> GetDescriptorsAsync(
        InteractionCategory category,
        string? subCategory = null,
        string? state = null,
        Biome? biome = null,
        CancellationToken ct = default)
    {
        var query = _context.InteractionDescriptors
            .Where(d => d.Category == category);

        if (!string.IsNullOrEmpty(subCategory))
        {
            query = query.Where(d => d.SubCategory == subCategory);
        }

        if (!string.IsNullOrEmpty(state))
        {
            query = query.Where(d => d.State == state);
        }

        if (biome.HasValue)
        {
            query = query.Where(d => d.BiomeAffinity == null || d.BiomeAffinity == biome.Value);
        }

        return await query
            .OrderBy(d => d.SubCategory)
            .ThenBy(d => d.State)
            .ToListAsync(ct);
    }

    public async Task<InteractionDescriptor?> GetRandomDescriptorAsync(
        InteractionCategory category,
        string? subCategory = null,
        string? state = null,
        Biome? biome = null,
        CancellationToken ct = default)
    {
        var descriptors = await GetDescriptorsAsync(category, subCategory, state, biome, ct);
        if (descriptors.Count == 0)
            return null;

        // Weight-based random selection
        var totalWeight = descriptors.Sum(d => d.Weight);
        var roll = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var descriptor in descriptors)
        {
            cumulative += descriptor.Weight;
            if (roll < cumulative)
                return descriptor;
        }

        return descriptors[^1]; // Fallback to last
    }

    public async Task<InteractionDescriptor?> GetWitsSuccessDescriptorAsync(
        WitsCheckMargin margin,
        CancellationToken ct = default)
    {
        return await _context.InteractionDescriptors
            .Where(d => d.Category == InteractionCategory.WitsSuccess
                && d.SubCategory == "WitsCheck"
                && d.State == margin.ToString())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<InteractionDescriptor?> GetWitsFailureDescriptorAsync(
        WitsFailureMargin severity,
        CancellationToken ct = default)
    {
        return await _context.InteractionDescriptors
            .Where(d => d.Category == InteractionCategory.WitsFailure
                && d.SubCategory == "WitsCheck"
                && d.State == severity.ToString())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<InteractionDescriptor>> GetLootQualityDescriptorsAsync(
        LootQuality quality,
        CancellationToken ct = default)
    {
        // Match "Poor", "Poor2", etc.
        var qualityStr = quality.ToString();
        return await _context.InteractionDescriptors
            .Where(d => d.Category == InteractionCategory.ContainerInteraction
                && d.SubCategory == "Loot"
                && d.State.StartsWith(qualityStr))
            .ToListAsync(ct);
    }

    public async Task<InteractionDescriptor?> GetObjectStateDescriptorAsync(
        string objectType,
        string state,
        CancellationToken ct = default)
    {
        // Try mechanical objects first, then containers
        var descriptor = await _context.InteractionDescriptors
            .Where(d => d.Category == InteractionCategory.MechanicalObject
                && d.SubCategory == objectType
                && d.State == state)
            .FirstOrDefaultAsync(ct);

        if (descriptor != null)
            return descriptor;

        return await _context.InteractionDescriptors
            .Where(d => d.Category == InteractionCategory.Container
                && d.SubCategory == objectType
                && d.State == state)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<InteractionDescriptor>> GetDiscoveryDescriptorsAsync(
        string discoveryType,
        CancellationToken ct = default)
    {
        return await _context.InteractionDescriptors
            .Where(d => d.Category == InteractionCategory.Discovery
                && d.SubCategory == discoveryType)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InteractionDescriptor>> GetSkillDescriptorsAsync(
        string skillType,
        string skillName,
        CancellationToken ct = default)
    {
        return await _context.InteractionDescriptors
            .Where(d => d.Category == InteractionCategory.SkillSpecific
                && d.SubCategory == skillType
                && d.State.StartsWith(skillName))
            .ToListAsync(ct);
    }
}
