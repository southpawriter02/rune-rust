namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the age/condition of a scavenger sign, which affects interpretation difficulty.
/// </summary>
/// <remarks>
/// <para>
/// SignAge represents how old or weathered a scavenger sign is. Older signs are harder to interpret
/// due to fading, weathering, or physical damage. Each age category applies a DC modifier:
/// <list type="bullet">
///   <item><description><see cref="Fresh"/> (+0 DC): Just made, marks still wet or clearly defined</description></item>
///   <item><description><see cref="Recent"/> (+0 DC): Made within the last day, still clear</description></item>
///   <item><description><see cref="Old"/> (+1 DC): Made within the last week, slightly worn</description></item>
///   <item><description><see cref="Faded"/> (+2 DC): Made weeks ago, significantly weathered</description></item>
///   <item><description><see cref="Ancient"/> (+4 DC): Very old, barely visible or partially destroyed</description></item>
/// </list>
/// </para>
/// <para>
/// Sign age also affects the reliability of the information contained within:
/// <list type="bullet">
///   <item><description>Fresh/Recent signs contain current, reliable information</description></item>
///   <item><description>Old signs may have outdated information (cache moved, danger passed)</description></item>
///   <item><description>Faded/Ancient signs may reference situations that no longer exist</description></item>
/// </list>
/// </para>
/// </remarks>
public enum SignAge
{
    /// <summary>
    /// Just made, the marks are still wet or freshly cut. No DC modifier.
    /// </summary>
    /// <remarks>
    /// Fresh signs indicate very recent activity—someone was here within hours.
    /// The information is current and highly reliable.
    /// </remarks>
    Fresh = 0,

    /// <summary>
    /// Made within the last day. The marks are clear and easy to read. No DC modifier.
    /// </summary>
    /// <remarks>
    /// Recent signs suggest activity within the past 24 hours.
    /// The information is still current and reliable.
    /// </remarks>
    Recent = 1,

    /// <summary>
    /// Made within the last week. Some wear is visible but still readable. +1 DC.
    /// </summary>
    /// <remarks>
    /// Old signs show some weathering or foot traffic wear.
    /// The information may be slightly outdated—situations can change in a week.
    /// </remarks>
    Old = 2,

    /// <summary>
    /// Made weeks ago, partially worn by time and weather. +2 DC.
    /// </summary>
    /// <remarks>
    /// Faded signs are significantly weathered, with some details obscured.
    /// The information may be outdated—caches might be empty, dangers might have passed.
    /// </remarks>
    Faded = 3,

    /// <summary>
    /// Very old, barely visible or partially destroyed. +4 DC.
    /// </summary>
    /// <remarks>
    /// Ancient signs are extremely difficult to read, with most details lost to time.
    /// The information is likely obsolete—these signs may date from before the current faction
    /// controlled the territory, or reference long-gone threats and resources.
    /// </remarks>
    Ancient = 4
}

/// <summary>
/// Extension methods for <see cref="SignAge"/>.
/// </summary>
/// <remarks>
/// Provides utility methods for working with sign age values including:
/// <list type="bullet">
///   <item><description>DC modifier calculation</description></item>
///   <item><description>Display string conversion</description></item>
///   <item><description>Reliability assessment</description></item>
/// </list>
/// </remarks>
public static class SignAgeExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DC MODIFIER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the DC modifier applied based on sign age.
    /// </summary>
    /// <param name="age">The sign age.</param>
    /// <returns>The DC modifier to add to the base interpretation DC.</returns>
    /// <remarks>
    /// DC modifiers by age:
    /// <list type="bullet">
    ///   <item><description>Fresh: +0</description></item>
    ///   <item><description>Recent: +0</description></item>
    ///   <item><description>Old: +1</description></item>
    ///   <item><description>Faded: +2</description></item>
    ///   <item><description>Ancient: +4</description></item>
    /// </list>
    /// </remarks>
    public static int GetDcModifier(this SignAge age)
    {
        return age switch
        {
            SignAge.Fresh => 0,
            SignAge.Recent => 0,
            SignAge.Old => 1,
            SignAge.Faded => 2,
            SignAge.Ancient => 4,
            _ => 0
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts the sign age to a narrative display string.
    /// </summary>
    /// <param name="age">The sign age.</param>
    /// <returns>A narrative description of the sign's age suitable for player display.</returns>
    public static string ToDisplayString(this SignAge age)
    {
        return age switch
        {
            SignAge.Fresh => "freshly made, the marks still wet",
            SignAge.Recent => "recent, made within the last day",
            SignAge.Old => "old, perhaps a week or more",
            SignAge.Faded => "faded, worn by time and weather",
            SignAge.Ancient => "ancient, barely visible",
            _ => "of indeterminate age"
        };
    }

    /// <summary>
    /// Gets a short display name for the sign age.
    /// </summary>
    /// <param name="age">The sign age.</param>
    /// <returns>A short display name suitable for UI labels.</returns>
    public static string GetDisplayName(this SignAge age)
    {
        return age switch
        {
            SignAge.Fresh => "Fresh",
            SignAge.Recent => "Recent",
            SignAge.Old => "Old",
            SignAge.Faded => "Faded",
            SignAge.Ancient => "Ancient",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a detailed description of what this age indicates.
    /// </summary>
    /// <param name="age">The sign age.</param>
    /// <returns>A detailed description of the sign's condition and implications.</returns>
    public static string GetDescription(this SignAge age)
    {
        return age switch
        {
            SignAge.Fresh =>
                "This sign was made very recently—within the last few hours. " +
                "The marks are still wet or freshly cut, indicating someone was here moments ago.",

            SignAge.Recent =>
                "This sign was made within the past day. " +
                "The marks are clear and easy to read, with no weathering yet.",

            SignAge.Old =>
                "This sign was made perhaps a week ago. " +
                "Some wear is visible from foot traffic or weather, but it remains readable.",

            SignAge.Faded =>
                "This sign was made weeks ago and has weathered significantly. " +
                "Some details are obscured, making interpretation more difficult.",

            SignAge.Ancient =>
                "This sign is very old—possibly months or even years. " +
                "Most details have been lost to time, weather, and decay.",

            _ => "The age of this sign is unclear."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RELIABILITY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether information from a sign of this age is likely still reliable.
    /// </summary>
    /// <param name="age">The sign age.</param>
    /// <returns>True if the sign's information is likely still current and accurate.</returns>
    /// <remarks>
    /// Fresh and Recent signs are considered reliable.
    /// Old signs may have outdated information.
    /// Faded and Ancient signs likely contain obsolete information.
    /// </remarks>
    public static bool IsReliable(this SignAge age)
    {
        return age is SignAge.Fresh or SignAge.Recent;
    }

    /// <summary>
    /// Determines whether this sign age indicates recent faction activity.
    /// </summary>
    /// <param name="age">The sign age.</param>
    /// <returns>True if the sign indicates the faction has been active recently.</returns>
    public static bool IndicatesRecentActivity(this SignAge age)
    {
        return age is SignAge.Fresh or SignAge.Recent or SignAge.Old;
    }

    /// <summary>
    /// Gets the approximate time since the sign was made.
    /// </summary>
    /// <param name="age">The sign age.</param>
    /// <returns>A string describing the approximate time since creation.</returns>
    public static string GetApproximateTime(this SignAge age)
    {
        return age switch
        {
            SignAge.Fresh => "within the last few hours",
            SignAge.Recent => "within the last day",
            SignAge.Old => "about a week ago",
            SignAge.Faded => "several weeks ago",
            SignAge.Ancient => "months or years ago",
            _ => "at an unknown time"
        };
    }

    /// <summary>
    /// Gets a warning about potential information reliability issues.
    /// </summary>
    /// <param name="age">The sign age.</param>
    /// <returns>A warning string if the sign is old enough to potentially contain outdated information, or null if reliable.</returns>
    public static string? GetReliabilityWarning(this SignAge age)
    {
        return age switch
        {
            SignAge.Fresh => null,
            SignAge.Recent => null,
            SignAge.Old => "Note: This sign is old. Situations may have changed since it was made.",
            SignAge.Faded => "Warning: This sign is significantly aged. The information may be outdated.",
            SignAge.Ancient => "Caution: This sign is ancient. The information is likely obsolete.",
            _ => null
        };
    }
}
