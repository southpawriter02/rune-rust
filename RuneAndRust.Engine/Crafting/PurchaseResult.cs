namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Result of a recipe purchase operation
/// </summary>
public class PurchaseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public int Cost { get; set; }

    public static PurchaseResult Failure(string message)
    {
        return new PurchaseResult
        {
            Success = false,
            Message = message
        };
    }

    public static PurchaseResult Successful(string recipeName, int cost)
    {
        return new PurchaseResult
        {
            Success = true,
            RecipeName = recipeName,
            Cost = cost,
            Message = $"Learned recipe: {recipeName} ({cost} credits)"
        };
    }
}
