namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Result of a modification operation (apply or remove)
/// </summary>
public class ModificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ModificationId { get; set; }
    public string ModificationName { get; set; } = string.Empty;
    public int? RemainingUses { get; set; }

    /// <summary>
    /// Create a failure result
    /// </summary>
    public static ModificationResult Failure(string message)
    {
        return new ModificationResult
        {
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// Create a success result
    /// </summary>
    public static ModificationResult CreateSuccess(
        int modificationId,
        string name,
        int? remainingUses)
    {
        return new ModificationResult
        {
            Success = true,
            ModificationId = modificationId,
            ModificationName = name,
            RemainingUses = remainingUses,
            Message = remainingUses.HasValue
                ? $"Applied {name} ({remainingUses} uses)"
                : $"Applied {name} (permanent)"
        };
    }
}
