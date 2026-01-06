namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the prerequisites required to select a class.
/// </summary>
public readonly record struct ClassRequirements
{
    /// <summary>
    /// Gets the race IDs that can select this class.
    /// </summary>
    /// <remarks>
    /// Empty or null means all races are allowed.
    /// </remarks>
    public IReadOnlyList<string>? AllowedRaceIds { get; init; }

    /// <summary>
    /// Gets the minimum attribute values required.
    /// </summary>
    /// <remarks>
    /// Key is attribute ID (e.g., "might"), value is minimum required.
    /// Empty or null means no attribute requirements.
    /// </remarks>
    public IReadOnlyDictionary<string, int>? MinimumAttributes { get; init; }

    /// <summary>
    /// Gets whether this requirements object has any actual requirements.
    /// </summary>
    public bool HasRequirements =>
        (AllowedRaceIds?.Count > 0) ||
        (MinimumAttributes?.Count > 0);

    /// <summary>
    /// Validates whether a player meets these requirements.
    /// </summary>
    /// <param name="raceId">The player's race ID.</param>
    /// <param name="attributes">The player's attribute values.</param>
    /// <returns>A validation result indicating success or failure reasons.</returns>
    public ClassRequirementValidation Validate(
        string raceId,
        IReadOnlyDictionary<string, int> attributes)
    {
        var failures = new List<string>();

        // Check race requirement
        if (AllowedRaceIds?.Count > 0 &&
            !AllowedRaceIds.Contains(raceId, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add($"Requires race: {string.Join(" or ", AllowedRaceIds)}");
        }

        // Check attribute requirements
        if (MinimumAttributes?.Count > 0)
        {
            foreach (var (attrId, minValue) in MinimumAttributes)
            {
                if (!attributes.TryGetValue(attrId, out var playerValue) ||
                    playerValue < minValue)
                {
                    failures.Add($"Requires {attrId} >= {minValue}");
                }
            }
        }

        return new ClassRequirementValidation(failures.Count == 0, failures);
    }
}

/// <summary>
/// Result of validating class requirements.
/// </summary>
/// <param name="IsValid">Whether all requirements are met.</param>
/// <param name="FailureReasons">List of unmet requirements (empty if valid).</param>
public record ClassRequirementValidation(
    bool IsValid,
    IReadOnlyList<string> FailureReasons);
