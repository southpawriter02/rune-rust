namespace RuneAndRust.Core.Enums;

/// <summary>
/// Comparison operators for numeric condition checks.
/// Used by AttributeCondition and other numeric conditions.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public enum ComparisonType
{
    /// <summary>Greater than or equal to (>=).</summary>
    GreaterThanOrEqual = 0,

    /// <summary>Exactly equal to (==).</summary>
    Equal = 1,

    /// <summary>Greater than (>).</summary>
    GreaterThan = 2,

    /// <summary>Less than (&lt;).</summary>
    LessThan = 3,

    /// <summary>Less than or equal to (&lt;=).</summary>
    LessThanOrEqual = 4,

    /// <summary>Not equal to (!=).</summary>
    NotEqual = 5
}
