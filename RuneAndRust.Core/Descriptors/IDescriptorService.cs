namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38: Service interface for the Descriptor Library Framework
/// Provides access to base templates, thematic modifiers, and composite descriptors
/// </summary>
public interface IDescriptorService
{
    // =====================================================
    // Tier 1: Base Templates
    // =====================================================

    /// <summary>
    /// Gets a base template by name
    /// </summary>
    DescriptorBaseTemplate? GetBaseTemplate(string templateName);

    /// <summary>
    /// Gets a base template by ID
    /// </summary>
    DescriptorBaseTemplate? GetBaseTemplate(int templateId);

    /// <summary>
    /// Gets all base templates for a specific category
    /// </summary>
    List<DescriptorBaseTemplate> GetBaseTemplatesByCategory(string category);

    /// <summary>
    /// Gets all base templates for a specific archetype
    /// </summary>
    List<DescriptorBaseTemplate> GetBaseTemplatesByArchetype(string archetype);

    /// <summary>
    /// Gets all base templates
    /// </summary>
    List<DescriptorBaseTemplate> GetAllBaseTemplates();

    // =====================================================
    // Tier 2: Thematic Modifiers
    // =====================================================

    /// <summary>
    /// Gets a thematic modifier by name
    /// </summary>
    ThematicModifier? GetModifier(string modifierName);

    /// <summary>
    /// Gets a thematic modifier by ID
    /// </summary>
    ThematicModifier? GetModifier(int modifierId);

    /// <summary>
    /// Gets all modifiers for a specific biome
    /// </summary>
    List<ThematicModifier> GetModifiersForBiome(string biomeName);

    /// <summary>
    /// Gets all thematic modifiers
    /// </summary>
    List<ThematicModifier> GetAllModifiers();

    // =====================================================
    // Tier 3: Composite Descriptors
    // =====================================================

    /// <summary>
    /// Gets a composite descriptor by ID
    /// </summary>
    DescriptorComposite? GetComposite(int compositeId);

    /// <summary>
    /// Gets a composite descriptor by final name
    /// </summary>
    DescriptorComposite? GetCompositeByName(string finalName);

    /// <summary>
    /// Composes a descriptor from a base template and modifier
    /// If a matching composite exists in the database, returns it
    /// Otherwise, generates a new composite on-the-fly (not saved)
    /// </summary>
    DescriptorComposite ComposeDescriptor(string baseTemplateName, string? modifierName = null);

    /// <summary>
    /// Gets all composites for a specific base template
    /// </summary>
    List<DescriptorComposite> GetCompositesForBaseTemplate(int baseTemplateId);

    /// <summary>
    /// Gets all composites for a specific modifier
    /// </summary>
    List<DescriptorComposite> GetCompositesForModifier(int modifierId);

    // =====================================================
    // Query System
    // =====================================================

    /// <summary>
    /// Queries descriptors based on filter criteria
    /// Supports category, archetype, biome, tags, and spawn rules
    /// </summary>
    DescriptorQueryResult QueryDescriptors(DescriptorQuery query);

    /// <summary>
    /// Performs weighted random selection from a list of descriptors
    /// Uses SpawnWeight property for probability distribution
    /// </summary>
    DescriptorComposite? WeightedRandomSelection(List<DescriptorComposite> descriptors, Random? random = null);

    // =====================================================
    // Generation & Composition
    // =====================================================

    /// <summary>
    /// Generates a new composite descriptor from base template + modifier
    /// This creates a new composite in memory (does not save to database)
    /// </summary>
    DescriptorComposite GenerateComposite(
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier? modifier = null);

    /// <summary>
    /// Merges base mechanics with modifier stat modifiers
    /// Returns a JSON string of the merged mechanics
    /// </summary>
    string MergeMechanics(string? baseMechanics, string? statModifiers);

    // =====================================================
    // Utility
    // =====================================================

    /// <summary>
    /// Checks if a descriptor can spawn in a given room context
    /// Evaluates spawn rules (room size, tags, etc.)
    /// </summary>
    bool CanSpawnInRoom(DescriptorComposite composite, Room room);

    /// <summary>
    /// Gets statistics about the descriptor library
    /// </summary>
    DescriptorLibraryStats GetLibraryStats();
}

/// <summary>
/// Statistics about the descriptor library
/// </summary>
public class DescriptorLibraryStats
{
    public int TotalBaseTemplates { get; set; }
    public int TotalModifiers { get; set; }
    public int TotalComposites { get; set; }

    public Dictionary<string, int> BaseTemplatesByCategory { get; set; } = new();
    public Dictionary<string, int> ModifiersByBiome { get; set; } = new();
    public Dictionary<string, int> CompositesByBiome { get; set; } = new();

    public int ActiveComposites { get; set; }
    public int InactiveComposites { get; set; }
}
