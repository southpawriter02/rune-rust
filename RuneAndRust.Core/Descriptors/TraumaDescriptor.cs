namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.14: Trauma Descriptor Library
/// Provides narrative descriptors for trauma acquisition, manifestation, triggers, and recovery
/// Integrates with Trauma Economy System (v0.15)
/// </summary>
public class TraumaDescriptor
{
    // Identity
    public int DescriptorId { get; set; }
    public string DescriptorName { get; set; } = string.Empty;

    // Trauma linkage
    public string TraumaId { get; set; } = string.Empty;  // Links to Trauma.TraumaId
    public string TraumaName { get; set; } = string.Empty;  // Display name (e.g., "[FLASHBACKS]")

    // Descriptor type and context
    public TraumaDescriptorType DescriptorType { get; set; }  // Acquisition, Manifestation, Trigger, BreakingPoint, Recovery
    public string? ContextTag { get; set; }  // "Combat", "Environmental", "Social", etc.

    // Narrative content
    public string DescriptorText { get; set; } = string.Empty;  // The actual flavor text
    public int? ProgressionLevel { get; set; }  // 1-3, for progression-specific descriptors

    // Mechanical context (JSON)
    public string? MechanicalContext { get; set; }  // {"stress_threshold": 75, "trigger_condition": "low_health"}

    // Display/selection rules
    public float SpawnWeight { get; set; } = 1.0f;  // Probability weight for random selection
    public string? DisplayConditions { get; set; }  // JSON: {"min_stress": 50, "has_trauma": "paranoia"}

    // Metadata
    public string? Tags { get; set; }  // JSON array: ["Intrusive", "Combat", "Visceral"]
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Parses mechanical context JSON into a dictionary
    /// </summary>
    public Dictionary<string, object>? GetMechanicalContext()
    {
        if (string.IsNullOrEmpty(MechanicalContext))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(MechanicalContext);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses display conditions JSON into a dictionary
    /// </summary>
    public Dictionary<string, object>? GetDisplayConditions()
    {
        if (string.IsNullOrEmpty(DisplayConditions))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(DisplayConditions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses tags JSON into a list
    /// </summary>
    public List<string> GetTags()
    {
        if (string.IsNullOrEmpty(Tags))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Validates that the descriptor has required fields
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(DescriptorName)) return false;
        if (string.IsNullOrEmpty(TraumaId)) return false;
        if (string.IsNullOrEmpty(DescriptorText)) return false;

        return true;
    }

    /// <summary>
    /// Checks if this descriptor should display given current conditions
    /// </summary>
    public bool ShouldDisplay(int currentStress, int? currentProgression = null, List<string>? activeTraumas = null)
    {
        var conditions = GetDisplayConditions();
        if (conditions == null) return true;

        // Check stress threshold
        if (conditions.ContainsKey("min_stress"))
        {
            var minStress = Convert.ToInt32(conditions["min_stress"]);
            if (currentStress < minStress) return false;
        }

        if (conditions.ContainsKey("max_stress"))
        {
            var maxStress = Convert.ToInt32(conditions["max_stress"]);
            if (currentStress > maxStress) return false;
        }

        // Check progression level
        if (ProgressionLevel.HasValue && currentProgression.HasValue)
        {
            if (ProgressionLevel.Value != currentProgression.Value) return false;
        }

        // Check for required trauma
        if (conditions.ContainsKey("has_trauma") && activeTraumas != null)
        {
            var requiredTrauma = conditions["has_trauma"]?.ToString();
            if (!string.IsNullOrEmpty(requiredTrauma) && !activeTraumas.Contains(requiredTrauma))
                return false;
        }

        return true;
    }
}

/// <summary>
/// Types of trauma descriptors
/// </summary>
public enum TraumaDescriptorType
{
    /// <summary>
    /// Describes the moment of trauma acquisition (Breaking Point)
    /// </summary>
    Acquisition,

    /// <summary>
    /// Describes ongoing trauma effects during normal gameplay
    /// </summary>
    Manifestation,

    /// <summary>
    /// Describes trauma triggering in specific contexts
    /// </summary>
    Trigger,

    /// <summary>
    /// Describes approaching/reaching breaking point
    /// </summary>
    BreakingPoint,

    /// <summary>
    /// Describes trauma suppression (Cognitive Stabilizer)
    /// </summary>
    Suppression,

    /// <summary>
    /// Describes trauma removal (Saga Quest)
    /// </summary>
    Recovery
}

/// <summary>
/// v0.38.14: Breaking Point Descriptor
/// Specialized descriptor for stress threshold warnings
/// </summary>
public class BreakingPointDescriptor
{
    public int DescriptorId { get; set; }
    public string DescriptorName { get; set; } = string.Empty;

    // Stress thresholds
    public int StressThresholdMin { get; set; }  // e.g., 75 for "approaching"
    public int StressThresholdMax { get; set; }  // e.g., 99 for "approaching", 100 for "breaking"

    // Descriptor type
    public BreakingPointPhase Phase { get; set; }  // Warning, Breaking, ResolveSuccess, ResolveFailure

    // Narrative content
    public string DescriptorText { get; set; } = string.Empty;

    // Display rules
    public float SpawnWeight { get; set; } = 1.0f;
    public string? Tags { get; set; }  // JSON array

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Parses tags JSON into a list
    /// </summary>
    public List<string> GetTags()
    {
        if (string.IsNullOrEmpty(Tags))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}

/// <summary>
/// Phases of breaking point experience
/// </summary>
public enum BreakingPointPhase
{
    /// <summary>
    /// Warning signs (75-99% stress)
    /// </summary>
    Warning,

    /// <summary>
    /// Breaking point moment (100% stress)
    /// </summary>
    Breaking,

    /// <summary>
    /// System message prompt for resolve check
    /// </summary>
    SystemMessage,

    /// <summary>
    /// Successful resolve check (barely holding)
    /// </summary>
    ResolveSuccess,

    /// <summary>
    /// Failed resolve check (trauma acquisition)
    /// </summary>
    ResolveFailure,

    /// <summary>
    /// Specific trauma reveal message
    /// </summary>
    TraumaReveal
}
