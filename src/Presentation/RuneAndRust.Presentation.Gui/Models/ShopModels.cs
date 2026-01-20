namespace RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Result of comparing two item stat values.
/// </summary>
public enum ComparisonResult
{
    /// <summary>The new item is better.</summary>
    Better,
    /// <summary>The new item is worse.</summary>
    Worse,
    /// <summary>The items are equal.</summary>
    Same
}

/// <summary>
/// A single line in an item comparison.
/// </summary>
/// <param name="Stat">The stat being compared.</param>
/// <param name="NewValue">The value of the new item.</param>
/// <param name="OldValue">The value of the current item.</param>
/// <param name="Result">The comparison result.</param>
public record ComparisonLine(string Stat, string NewValue, string OldValue, ComparisonResult Result)
{
    /// <summary>Gets the formatted comparison text.</summary>
    public string ComparisonText => $"{NewValue} vs {OldValue}";

    /// <summary>Gets the result indicator (▲, ▼, or ═).</summary>
    public string ResultIndicator => Result switch
    {
        ComparisonResult.Better => "▲",
        ComparisonResult.Worse => "▼",
        ComparisonResult.Same => "═",
        _ => ""
    };

    /// <summary>Gets the result text.</summary>
    public string ResultText => Result switch
    {
        ComparisonResult.Better => "Better",
        ComparisonResult.Worse => "Worse",
        ComparisonResult.Same => "Same",
        _ => ""
    };

    /// <summary>Gets the result color name.</summary>
    public string ResultColorName => Result switch
    {
        ComparisonResult.Better => "LimeGreen",
        ComparisonResult.Worse => "IndianRed",
        _ => "Gray"
    };
}

/// <summary>
/// Result of a buy/sell transaction.
/// </summary>
/// <param name="Success">Whether the transaction succeeded.</param>
/// <param name="Message">Result message.</param>
/// <param name="GoldChange">Amount of gold gained (+) or spent (-).</param>
public record TransactionResult(bool Success, string Message, int GoldChange = 0);
