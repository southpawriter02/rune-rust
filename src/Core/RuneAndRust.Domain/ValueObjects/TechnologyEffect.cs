// ═══════════════════════════════════════════════════════════════════════════════
// TechnologyEffect.cs
// Value object representing an effect produced by activated Jötun technology.
// Version: 0.20.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// An effect produced by activated Jötun technology.
/// </summary>
/// <remarks>
/// <para>
/// Each activated technology can produce multiple effects. Effects can be
/// one-time (healing) or continuous (sensor arrays, power restoration).
/// </para>
/// </remarks>
/// <seealso cref="ActivatedTechnology"/>
public sealed record TechnologyEffect
{
    /// <summary>Type of effect (healing, damage, etc.).</summary>
    public string EffectType { get; init; } = string.Empty;

    /// <summary>Radius if area effect (in feet).</summary>
    public int? TargetArea { get; init; }

    /// <summary>Numeric value for the effect (damage, healing, etc.).</summary>
    public int? Value { get; init; }

    /// <summary>Description of the effect.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Whether this effect is continuous (ongoing).</summary>
    public bool IsContinuous { get; init; }
}
