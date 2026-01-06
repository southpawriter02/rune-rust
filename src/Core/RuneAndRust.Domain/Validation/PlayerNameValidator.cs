using System.Text.RegularExpressions;

namespace RuneAndRust.Domain.Validation;

/// <summary>
/// Validates player character names according to game rules.
/// </summary>
public static partial class PlayerNameValidator
{
    private const int MinLength = 2;
    private const int MaxLength = 30;

    [GeneratedRegex(@"^[A-Za-z][A-Za-z\s\-']*$")]
    private static partial Regex ValidNamePattern();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex ConsecutiveSpacesPattern();

    /// <summary>
    /// Validates a player name and returns the result.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns>Validation result with success status and error message if invalid.</returns>
    public static (bool IsValid, string? ErrorMessage) Validate(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Name cannot be empty.");

        var trimmed = name.Trim();

        if (trimmed.Length < MinLength)
            return (false, $"Name must be at least {MinLength} characters.");

        if (trimmed.Length > MaxLength)
            return (false, $"Name cannot exceed {MaxLength} characters.");

        if (!ValidNamePattern().IsMatch(trimmed))
            return (false, "Name can only contain letters, spaces, hyphens, and apostrophes, and must start with a letter.");

        if (ConsecutiveSpacesPattern().IsMatch(trimmed))
            return (false, "Name cannot contain consecutive spaces.");

        return (true, null);
    }

    /// <summary>
    /// Normalizes a valid name (trims whitespace, normalizes internal spacing).
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>The normalized name.</returns>
    public static string Normalize(string name)
    {
        var trimmed = name.Trim();
        return ConsecutiveSpacesPattern().Replace(trimmed, " ");
    }
}
