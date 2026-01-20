using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Repository interface for examination descriptor queries.
/// </summary>
public interface IExaminationRepository
{
    /// <summary>
    /// Gets all examination descriptors for an object type.
    /// </summary>
    Task<IReadOnlyList<ExaminationDescriptor>> GetDescriptorsAsync(
        ObjectCategory category,
        string objectType,
        Biome? biome = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets examination descriptors up to and including the specified layer.
    /// </summary>
    Task<IReadOnlyList<ExaminationDescriptor>> GetDescriptorsByLayerAsync(
        ObjectCategory category,
        string objectType,
        ExaminationLayer maxLayer,
        Biome? biome = null,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new examination descriptor.
    /// </summary>
    Task AddDescriptorAsync(ExaminationDescriptor descriptor, CancellationToken ct = default);

    /// <summary>
    /// Adds multiple examination descriptors.
    /// </summary>
    Task AddDescriptorsAsync(IEnumerable<ExaminationDescriptor> descriptors, CancellationToken ct = default);
}

/// <summary>
/// Repository interface for perception descriptor queries.
/// </summary>
public interface IPerceptionRepository
{
    /// <summary>
    /// Gets perception descriptors for a hidden element type.
    /// </summary>
    Task<IReadOnlyList<PerceptionDescriptor>> GetDescriptorsAsync(
        HiddenElementType elementType,
        PerceptionSuccessLevel? successLevel = null,
        Biome? biome = null,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new perception descriptor.
    /// </summary>
    Task AddDescriptorAsync(PerceptionDescriptor descriptor, CancellationToken ct = default);
}

/// <summary>
/// Repository interface for flora/fauna descriptor queries.
/// </summary>
public interface IFloraFaunaRepository
{
    /// <summary>
    /// Gets all flora/fauna descriptors for a biome.
    /// </summary>
    Task<IReadOnlyList<FloraFaunaDescriptor>> GetByBiomeAsync(
        Biome biome,
        FloraFaunaCategory? category = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets descriptors for a specific species up to the specified layer.
    /// </summary>
    Task<IReadOnlyList<FloraFaunaDescriptor>> GetSpeciesDescriptorsAsync(
        string speciesName,
        ExaminationLayer maxLayer,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new flora/fauna descriptor.
    /// </summary>
    Task AddDescriptorAsync(FloraFaunaDescriptor descriptor, CancellationToken ct = default);

    /// <summary>
    /// Adds multiple flora/fauna descriptors.
    /// </summary>
    Task AddDescriptorsAsync(IEnumerable<FloraFaunaDescriptor> descriptors, CancellationToken ct = default);
}

/// <summary>
/// Repository interface for interaction descriptor queries.
/// </summary>
public interface IInteractionDescriptorRepository
{
    /// <summary>
    /// Gets interaction descriptors by category and subcategory.
    /// </summary>
    Task<IReadOnlyList<InteractionDescriptor>> GetDescriptorsAsync(
        InteractionCategory category,
        string? subCategory = null,
        string? state = null,
        Biome? biome = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a random descriptor for the specified criteria.
    /// </summary>
    Task<InteractionDescriptor?> GetRandomDescriptorAsync(
        InteractionCategory category,
        string? subCategory = null,
        string? state = null,
        Biome? biome = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets WITS success descriptors by margin.
    /// </summary>
    Task<InteractionDescriptor?> GetWitsSuccessDescriptorAsync(
        WitsCheckMargin margin,
        CancellationToken ct = default);

    /// <summary>
    /// Gets WITS failure descriptors by severity.
    /// </summary>
    Task<InteractionDescriptor?> GetWitsFailureDescriptorAsync(
        WitsFailureMargin severity,
        CancellationToken ct = default);

    /// <summary>
    /// Gets loot quality descriptors.
    /// </summary>
    Task<IReadOnlyList<InteractionDescriptor>> GetLootQualityDescriptorsAsync(
        LootQuality quality,
        CancellationToken ct = default);

    /// <summary>
    /// Gets object state descriptor for mechanical objects or containers.
    /// </summary>
    Task<InteractionDescriptor?> GetObjectStateDescriptorAsync(
        string objectType,
        string state,
        CancellationToken ct = default);

    /// <summary>
    /// Gets discovery descriptors by type.
    /// </summary>
    Task<IReadOnlyList<InteractionDescriptor>> GetDiscoveryDescriptorsAsync(
        string discoveryType,
        CancellationToken ct = default);

    /// <summary>
    /// Gets skill-specific descriptors.
    /// </summary>
    Task<IReadOnlyList<InteractionDescriptor>> GetSkillDescriptorsAsync(
        string skillType,
        string skillName,
        CancellationToken ct = default);
}
