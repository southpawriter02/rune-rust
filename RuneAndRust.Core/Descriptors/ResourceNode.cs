using System.Text.Json;

namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.5: Resource node model
/// Represents a harvestable/extractable resource in a room
/// </summary>
public class ResourceNode
{
    /// <summary>
    /// Unique identifier for this resource node instance
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Room ID where this node is located
    /// </summary>
    public int RoomId { get; set; }

    /// <summary>
    /// Composite descriptor ID (if generated from composite)
    /// </summary>
    public int? CompositeDescriptorId { get; set; }

    /// <summary>
    /// Node name (e.g., "Corroded Iron Vein", "Scorched Star-Metal Vein")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive text for the resource node
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Resource node type
    /// </summary>
    public ResourceNodeType NodeType { get; set; }

    #region Extraction Mechanics

    /// <summary>
    /// Extraction method required
    /// </summary>
    public ExtractionType ExtractionType { get; set; }

    /// <summary>
    /// Difficulty class for extraction check
    /// </summary>
    public int ExtractionDC { get; set; }

    /// <summary>
    /// Time cost in turns
    /// </summary>
    public int ExtractionTime { get; set; }

    /// <summary>
    /// Whether a tool is required for extraction
    /// </summary>
    public bool RequiresTool { get; set; }

    /// <summary>
    /// Required tool name (e.g., "Mining_Tool", "Salvage_Kit", "Aether_Siphon")
    /// </summary>
    public string? RequiredTool { get; set; }

    #endregion

    #region Yield Properties

    /// <summary>
    /// Minimum yield per extraction
    /// </summary>
    public int YieldMin { get; set; }

    /// <summary>
    /// Maximum yield per extraction
    /// </summary>
    public int YieldMax { get; set; }

    /// <summary>
    /// Resource type yielded (e.g., "Iron_Ore", "Star_Metal", "Luminous_Fungus")
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Rarity tier
    /// </summary>
    public RarityTier RarityTier { get; set; }

    #endregion

    #region State Management

    /// <summary>
    /// Whether this node has been fully depleted
    /// </summary>
    public bool Depleted { get; set; }

    /// <summary>
    /// Number of extraction attempts remaining
    /// </summary>
    public int UsesRemaining { get; set; }

    /// <summary>
    /// Maximum number of uses
    /// </summary>
    public int MaxUses { get; set; }

    #endregion

    #region Hazard Properties

    /// <summary>
    /// Whether this node is hazardous to extract
    /// </summary>
    public bool Hazardous { get; set; }

    /// <summary>
    /// Hazard description (if applicable)
    /// </summary>
    public string? HazardDescription { get; set; }

    /// <summary>
    /// Chance of triggering a trap (0.0-1.0)
    /// </summary>
    public float TrapChance { get; set; }

    #endregion

    #region Special Properties

    /// <summary>
    /// Whether this node is hidden and requires detection
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Detection difficulty class (for hidden nodes)
    /// </summary>
    public int DetectionDC { get; set; }

    /// <summary>
    /// Whether this node is unstable (Aetheric anomalies)
    /// </summary>
    public bool Unstable { get; set; }

    /// <summary>
    /// Whether Galdr-casting is required for extraction
    /// </summary>
    public bool RequiresGaldr { get; set; }

    /// <summary>
    /// Biome restriction (null = any biome)
    /// </summary>
    public string? BiomeRestriction { get; set; }

    /// <summary>
    /// Tags for filtering and classification (JSON array)
    /// </summary>
    public string? Tags { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Gets tags as a list
    /// </summary>
    public List<string> GetTags()
    {
        if (string.IsNullOrEmpty(Tags))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Checks if this node can be extracted
    /// </summary>
    public bool CanExtract()
    {
        if (Depleted)
            return false;

        if (UsesRemaining <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Attempts extraction, returns yield amount
    /// </summary>
    public int Extract()
    {
        if (!CanExtract())
            return 0;

        UsesRemaining--;

        if (UsesRemaining <= 0)
        {
            Depleted = true;
        }

        // Calculate yield
        var random = new Random();
        return random.Next(YieldMin, YieldMax + 1);
    }

    /// <summary>
    /// Gets a summary of this node for display
    /// </summary>
    public string GetDisplaySummary()
    {
        var summary = new List<string>();

        summary.Add($"Type: {NodeType}");
        summary.Add($"Extraction: {ExtractionType} (DC {ExtractionDC}, {ExtractionTime} turns)");
        summary.Add($"Yield: {YieldMin}-{YieldMax} {ResourceType}");
        summary.Add($"Rarity: {RarityTier}");

        if (RequiresTool && !string.IsNullOrEmpty(RequiredTool))
        {
            summary.Add($"Requires: {RequiredTool}");
        }

        if (Hidden)
        {
            summary.Add($"Hidden (Detection DC {DetectionDC})");
        }

        if (Hazardous)
        {
            summary.Add("Hazardous");
        }

        if (TrapChance > 0)
        {
            summary.Add($"Trap Chance: {TrapChance * 100:F0}%");
        }

        if (Unstable)
        {
            summary.Add("Unstable");
        }

        if (RequiresGaldr)
        {
            summary.Add("Requires Galdr");
        }

        if (Depleted)
        {
            summary.Add("DEPLETED");
        }
        else
        {
            summary.Add($"Uses: {UsesRemaining}/{MaxUses}");
        }

        return string.Join(" | ", summary);
    }

    /// <summary>
    /// Validates that this node has required properties
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Name))
            return false;

        if (RoomId <= 0)
            return false;

        if (string.IsNullOrEmpty(ResourceType))
            return false;

        if (YieldMin < 0 || YieldMax < YieldMin)
            return false;

        if (ExtractionTime < 1)
            return false;

        return true;
    }

    #endregion
}
