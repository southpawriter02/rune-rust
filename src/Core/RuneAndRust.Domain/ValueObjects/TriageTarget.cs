namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a potential target for the Bone-Setter's Triage passive evaluation.
/// Carries ally health data needed to identify the most wounded target within radius.
/// </summary>
/// <remarks>
/// <para>Used as input to <c>EvaluateTriage</c> on the Bone-Setter ability service.
/// The caller provides an array of <see cref="TriageTarget"/> representing all allies
/// within the Triage radius (5 spaces), and the service identifies the most wounded
/// (lowest HP percentage) for bonus healing application.</para>
/// </remarks>
public sealed record TriageTarget
{
    /// <summary>
    /// Unique identifier of the ally being evaluated.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the ally.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Ally's current HP at the time of Triage evaluation.
    /// </summary>
    public int CurrentHp { get; init; }

    /// <summary>
    /// Ally's maximum HP for percentage calculation.
    /// </summary>
    public int MaxHp { get; init; }

    /// <summary>
    /// Computed HP percentage (0.0 to 1.0).
    /// Used to identify the most wounded ally (lowest percentage).
    /// </summary>
    public float HpPercentage => MaxHp > 0 ? (float)CurrentHp / MaxHp : 0f;

    /// <summary>
    /// Creates a new Triage target with validation.
    /// </summary>
    /// <param name="targetId">Unique identifier of the ally.</param>
    /// <param name="targetName">Display name of the ally. Cannot be empty.</param>
    /// <param name="currentHp">Current HP value.</param>
    /// <param name="maxHp">Maximum HP value. Must be greater than 0.</param>
    /// <returns>A new <see cref="TriageTarget"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="targetName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxHp"/> is less than 1.</exception>
    public static TriageTarget Create(Guid targetId, string targetName, int currentHp, int maxHp)
    {
        if (string.IsNullOrWhiteSpace(targetName))
            throw new ArgumentException("Target name cannot be empty.", nameof(targetName));

        if (maxHp < 1)
            throw new ArgumentOutOfRangeException(nameof(maxHp), maxHp,
                "Maximum HP must be at least 1.");

        return new TriageTarget
        {
            TargetId = targetId,
            TargetName = targetName,
            CurrentHp = currentHp,
            MaxHp = maxHp
        };
    }
}
