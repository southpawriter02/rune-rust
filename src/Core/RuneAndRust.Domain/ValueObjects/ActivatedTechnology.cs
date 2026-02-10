// ═══════════════════════════════════════════════════════════════════════════════
// ActivatedTechnology.cs
// Value object recording the activation of dormant Jötun technology via the
// Voice of the Giants capstone ability.
// Version: 0.20.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using System.Text;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Record of activated Jötun technology via the Voice of the Giants capstone.
/// </summary>
/// <remarks>
/// <para>
/// Tracks the activation of dormant Jötun technology, including the activator,
/// technology type, produced effects, and expiration time. Default duration
/// is 60 minutes (1 hour).
/// </para>
/// </remarks>
/// <seealso cref="JotunTechnologyType"/>
/// <seealso cref="TechnologyEffect"/>
/// <seealso cref="JotunReaderAbilityId"/>
public sealed record ActivatedTechnology
{
    /// <summary>Unique identifier for this activation.</summary>
    public Guid ActivationId { get; init; }

    /// <summary>ID of the activated technology.</summary>
    public Guid TechnologyId { get; init; }

    /// <summary>Type of Jötun technology activated.</summary>
    public JotunTechnologyType TechnologyType { get; init; }

    /// <summary>Name of the technology.</summary>
    public string TechnologyName { get; init; } = string.Empty;

    /// <summary>Character who activated it.</summary>
    public Guid ActivatorId { get; init; }

    /// <summary>When it was activated.</summary>
    public DateTime ActivatedAt { get; init; }

    /// <summary>When the activation expires.</summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>Effects this activation produces.</summary>
    public IReadOnlyList<TechnologyEffect> Effects { get; init; } = new List<TechnologyEffect>();

    /// <summary>Whether this activation is currently active.</summary>
    public bool IsActive => DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Creates a new activation record.
    /// </summary>
    /// <param name="technologyId">ID of the technology being activated.</param>
    /// <param name="type">Type of Jötun technology.</param>
    /// <param name="name">Display name of the technology.</param>
    /// <param name="activatorId">ID of the activating character.</param>
    /// <param name="effects">Effects produced by the activation.</param>
    /// <param name="durationMinutes">Duration in minutes. Defaults to 60.</param>
    /// <returns>A new <see cref="ActivatedTechnology"/> instance.</returns>
    public static ActivatedTechnology Create(
        Guid technologyId,
        JotunTechnologyType type,
        string name,
        Guid activatorId,
        IEnumerable<TechnologyEffect> effects,
        int durationMinutes = 60)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(effects);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationMinutes);

        return new ActivatedTechnology
        {
            ActivationId = Guid.NewGuid(),
            TechnologyId = technologyId,
            TechnologyType = type,
            TechnologyName = name,
            ActivatorId = activatorId,
            ActivatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes),
            Effects = effects.ToList().AsReadOnly()
        };
    }

    /// <summary>
    /// Gets remaining activation duration.
    /// </summary>
    /// <returns>The remaining time, or <see cref="TimeSpan.Zero"/> if expired.</returns>
    public TimeSpan GetRemainingDuration() => IsActive
        ? ExpiresAt - DateTime.UtcNow
        : TimeSpan.Zero;

    /// <summary>
    /// Deactivates the technology by setting expiration to the current time.
    /// </summary>
    /// <returns>A new instance with the expiration set to now.</returns>
    public ActivatedTechnology Deactivate() => this with
    {
        ExpiresAt = DateTime.UtcNow
    };

    /// <summary>
    /// Gets a formatted summary of the activation and its effects.
    /// </summary>
    /// <returns>A multi-line summary string.</returns>
    public string GetEffectSummary()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{TechnologyName} ({TechnologyType})");
        sb.AppendLine($"Duration: {GetRemainingDuration().TotalMinutes:F0} minutes remaining");
        sb.AppendLine("Effects:");
        foreach (var effect in Effects)
        {
            sb.AppendLine($"  • {effect.Description}");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Determines if this activation has expired.
    /// </summary>
    /// <returns><c>true</c> if the current time is at or past expiration.</returns>
    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
}
