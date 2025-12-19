namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the encumbrance state of a character based on carried weight.
/// Affects movement speed and attribute penalties.
/// </summary>
public enum BurdenState
{
    /// <summary>
    /// Under 70% capacity. No penalties.
    /// Normal movement and full attribute access.
    /// </summary>
    Light = 0,

    /// <summary>
    /// Between 70% and 90% capacity.
    /// Applies -2 penalty to Finesse attribute.
    /// </summary>
    Heavy = 1,

    /// <summary>
    /// Over 90% capacity.
    /// Cannot move between rooms until weight is reduced.
    /// </summary>
    Overburdened = 2
}
