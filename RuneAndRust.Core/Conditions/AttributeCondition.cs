using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Condition that checks a character attribute against a threshold.
/// Example: [WITS >= 6] for perception-based dialogue.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class AttributeCondition : DialogueCondition
{
    /// <inheritdoc/>
    public override DialogueConditionType Type => DialogueConditionType.Attribute;

    /// <summary>
    /// The attribute to check (e.g., Wits, Might, Will).
    /// </summary>
    public CharacterAttribute Attribute { get; set; }

    /// <summary>
    /// The comparison operator (default: >=).
    /// </summary>
    public ComparisonType Comparison { get; set; } = ComparisonType.GreaterThanOrEqual;

    /// <summary>
    /// The threshold value to compare against.
    /// </summary>
    public int Threshold { get; set; }

    /// <inheritdoc/>
    public override string GetDisplayHint()
    {
        var op = Comparison switch
        {
            ComparisonType.GreaterThanOrEqual => "",  // Default, no symbol needed
            ComparisonType.Equal => "=",
            ComparisonType.GreaterThan => ">",
            ComparisonType.LessThan => "<",
            ComparisonType.LessThanOrEqual => "<=",
            ComparisonType.NotEqual => "!=",
            _ => ""
        };
        return $"[{Attribute.ToString().ToUpperInvariant()} {op}{Threshold}]";
    }
}
