using System.Text.Json;

namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.3: Object mechanics definition for interactive objects
/// Contains all mechanical properties for interactions, checks, and consequences
/// </summary>
public class ObjectMechanics
{
    // Interaction Properties
    public string InteractionType { get; set; } = string.Empty;
    public bool RequiresCheck { get; set; }
    public string? CheckType { get; set; }
    public int CheckDC { get; set; }
    public int AttemptsAllowed { get; set; }

    // State Properties
    public List<string>? States { get; set; }
    public bool Reversible { get; set; }
    public string? Range { get; set; }

    // Lock Properties
    public bool Locked { get; set; }
    public string? Requires { get; set; }  // "Keycard", "Console_Override", etc.

    // Destructible Properties
    public bool Destructible { get; set; }
    public int HP { get; set; }
    public int Soak { get; set; }
    public bool BlocksLoS { get; set; }

    // Container Properties
    public string? LootTier { get; set; }
    public float TrapChance { get; set; }
    public int SearchTime { get; set; }
    public bool PersonalEffects { get; set; }

    // Investigatable Properties
    public bool NarrativeClue { get; set; }
    public string? DecayState { get; set; }
    public string? TextLength { get; set; }
    public string? CorruptionLevel { get; set; }
    public bool QuestHook { get; set; }
    public string? LoreValue { get; set; }

    // Trap Properties
    public string? Trigger { get; set; }
    public int DetectionDC { get; set; }
    public int DisarmDC { get; set; }
    public bool OneTime { get; set; }

    // Consequence Properties
    public string? ConsequenceType { get; set; }
    public Dictionary<string, object>? Consequence { get; set; }
    public string? FailureConsequence { get; set; }

    /// <summary>
    /// Creates a deep copy of the mechanics
    /// </summary>
    public ObjectMechanics Clone()
    {
        return new ObjectMechanics
        {
            InteractionType = this.InteractionType,
            RequiresCheck = this.RequiresCheck,
            CheckType = this.CheckType,
            CheckDC = this.CheckDC,
            AttemptsAllowed = this.AttemptsAllowed,
            States = this.States != null ? new List<string>(this.States) : null,
            Reversible = this.Reversible,
            Range = this.Range,
            Locked = this.Locked,
            Requires = this.Requires,
            Destructible = this.Destructible,
            HP = this.HP,
            Soak = this.Soak,
            BlocksLoS = this.BlocksLoS,
            LootTier = this.LootTier,
            TrapChance = this.TrapChance,
            SearchTime = this.SearchTime,
            PersonalEffects = this.PersonalEffects,
            NarrativeClue = this.NarrativeClue,
            DecayState = this.DecayState,
            TextLength = this.TextLength,
            CorruptionLevel = this.CorruptionLevel,
            QuestHook = this.QuestHook,
            LoreValue = this.LoreValue,
            Trigger = this.Trigger,
            DetectionDC = this.DetectionDC,
            DisarmDC = this.DisarmDC,
            OneTime = this.OneTime,
            ConsequenceType = this.ConsequenceType,
            Consequence = this.Consequence != null ? new Dictionary<string, object>(this.Consequence) : null,
            FailureConsequence = this.FailureConsequence
        };
    }

    /// <summary>
    /// Serializes mechanics to JSON string
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Deserializes mechanics from JSON string
    /// </summary>
    public static ObjectMechanics? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<ObjectMechanics>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });
    }
}

/// <summary>
/// v0.38.3: Object function variant model
/// Defines function-specific overrides for object templates
/// </summary>
public class ObjectFunctionVariant
{
    /// <summary>
    /// Unique identifier for this variant
    /// </summary>
    public int VariantId { get; set; }

    /// <summary>
    /// Base template name this variant applies to
    /// </summary>
    public string BaseTemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Function name (e.g., "Door_Control", "Supply_Cache")
    /// </summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>
    /// Function description
    /// </summary>
    public string FunctionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Mechanic overrides (JSON)
    /// </summary>
    public string? MechanicOverrides { get; set; }

    /// <summary>
    /// Consequence type
    /// </summary>
    public string? ConsequenceType { get; set; }

    /// <summary>
    /// Consequence data (JSON)
    /// </summary>
    public string? ConsequenceData { get; set; }

    /// <summary>
    /// Biome affinity (JSON array)
    /// </summary>
    public string? BiomeAffinity { get; set; }

    /// <summary>
    /// Tags (JSON array)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets biome affinity as list
    /// </summary>
    public List<string> GetBiomeAffinity()
    {
        if (string.IsNullOrEmpty(BiomeAffinity))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(BiomeAffinity) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Gets tags as list
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
    /// Checks if this variant is suitable for a specific biome
    /// </summary>
    public bool IsSuitableForBiome(string biome)
    {
        var affinity = GetBiomeAffinity();
        if (affinity.Count == 0)
            return true;  // No restriction

        return affinity.Contains(biome);
    }
}
