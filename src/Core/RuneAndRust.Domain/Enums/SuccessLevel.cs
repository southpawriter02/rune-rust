namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the degree of success or failure for a check.
/// </summary>
/// <remarks>
/// Used to categorize the outcome of skill checks, attack rolls, and other
/// dice-based resolutions. Critical results occur on natural maximum or minimum rolls.
/// </remarks>
public enum SuccessLevel
{
    /// <summary>Critical failure - natural 1 rolled.</summary>
    CriticalFailure,

    /// <summary>Normal failure - total below difficulty class.</summary>
    Failure,

    /// <summary>Normal success - total meets or exceeds difficulty class.</summary>
    Success,

    /// <summary>Critical success - natural maximum rolled.</summary>
    CriticalSuccess
}
