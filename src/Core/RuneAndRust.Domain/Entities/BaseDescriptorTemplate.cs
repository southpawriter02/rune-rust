using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tier 1 base template - biome-agnostic archetype with placeholder tokens.
/// These templates are combined with ThematicModifiers and DescriptorFragments
/// to generate unique room descriptions.
/// </summary>
/// <remarks>
/// Supported placeholder tokens:
/// - {Modifier} - The modifier name (e.g., "Rusted")
/// - {Modifier_Adj} - The modifier adjective (e.g., "corroded")
/// - {Modifier_Detail} - The modifier detail fragment
/// - {Spatial_Descriptor} - Room size/feel description
/// - {Architectural_Feature} - Wall/ceiling/floor feature
/// - {Detail_1}, {Detail_2} - Ambient detail fragments
/// - {Direction_Descriptor} - Direction phrase
/// - {Atmospheric_Detail} - Sensory element
/// - {Function} - Room function detail (if applicable)
/// - {Article} - "a" or "an" based on following word
/// - {Article_Cap} - "A" or "An" capitalized
/// </remarks>
public class BaseDescriptorTemplate : IEntity
{
    public Guid Id { get; private set; }

    /// <summary>Template identifier (e.g., "Corridor_Base").</summary>
    public string TemplateId { get; private set; }

    /// <summary>Name template with placeholders (e.g., "The {Modifier} Corridor").</summary>
    public string NameTemplate { get; private set; }

    /// <summary>Description template with placeholders.</summary>
    public string DescriptionTemplate { get; private set; }

    /// <summary>The room archetype this template applies to.</summary>
    public RoomArchetype Archetype { get; private set; }

    /// <summary>Room size category.</summary>
    public RoomSize Size { get; private set; }

    /// <summary>Minimum number of exits.</summary>
    public int MinExits { get; private set; }

    /// <summary>Maximum number of exits.</summary>
    public int MaxExits { get; private set; }

    /// <summary>Spawn budget multiplier (affects enemy spawning).</summary>
    public double SpawnBudgetMultiplier { get; private set; }

    /// <summary>Selection weight for random template selection.</summary>
    public int Weight { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private BaseDescriptorTemplate()
    {
        TemplateId = null!;
        NameTemplate = null!;
        DescriptionTemplate = null!;
    } // For EF Core

    public BaseDescriptorTemplate(
        string templateId,
        string nameTemplate,
        string descriptionTemplate,
        RoomArchetype archetype,
        RoomSize size,
        int minExits,
        int maxExits,
        double spawnBudgetMultiplier = 1.0,
        int weight = 1)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID cannot be empty", nameof(templateId));
        if (string.IsNullOrWhiteSpace(nameTemplate))
            throw new ArgumentException("Name template cannot be empty", nameof(nameTemplate));
        if (string.IsNullOrWhiteSpace(descriptionTemplate))
            throw new ArgumentException("Description template cannot be empty", nameof(descriptionTemplate));
        if (minExits < 1)
            throw new ArgumentOutOfRangeException(nameof(minExits), "Minimum exits must be at least 1");
        if (maxExits < minExits)
            throw new ArgumentOutOfRangeException(nameof(maxExits), "Maximum exits must be >= minimum exits");
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");

        Id = Guid.NewGuid();
        TemplateId = templateId;
        NameTemplate = nameTemplate;
        DescriptionTemplate = descriptionTemplate;
        Archetype = archetype;
        Size = size;
        MinExits = minExits;
        MaxExits = maxExits;
        SpawnBudgetMultiplier = spawnBudgetMultiplier;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor for seeding with known Id.
    /// </summary>
    public BaseDescriptorTemplate(
        Guid id,
        string templateId,
        string nameTemplate,
        string descriptionTemplate,
        RoomArchetype archetype,
        RoomSize size,
        int minExits,
        int maxExits,
        double spawnBudgetMultiplier = 1.0,
        int weight = 1)
        : this(templateId, nameTemplate, descriptionTemplate, archetype, size, minExits, maxExits, spawnBudgetMultiplier, weight)
    {
        Id = id;
    }

    /// <summary>
    /// Checks if this template is compatible with the given exit count.
    /// </summary>
    public bool IsCompatibleWithExitCount(int exitCount) =>
        exitCount >= MinExits && exitCount <= MaxExits;

    /// <summary>
    /// Gets the article ("a" or "an") appropriate for a word starting with the given character.
    /// </summary>
    public static string GetArticle(char firstChar) =>
        "aeiouAEIOU".Contains(firstChar) ? "an" : "a";

    /// <summary>
    /// Gets the capitalized article ("A" or "An") appropriate for a word starting with the given character.
    /// </summary>
    public static string GetCapitalizedArticle(char firstChar) =>
        "aeiouAEIOU".Contains(firstChar) ? "An" : "A";

    public override string ToString() => $"{TemplateId} ({Archetype}, {Size})";
}
