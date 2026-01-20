using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines how and when a hazard triggers.
/// </summary>
public readonly record struct HazardTrigger
{
    /// <summary>
    /// Gets the trigger type.
    /// </summary>
    public HazardTriggerType Type { get; init; }

    /// <summary>
    /// Gets the chance to trigger (0.0 to 1.0).
    /// </summary>
    public float Chance { get; init; }

    /// <summary>
    /// Gets the stat used for avoidance checks.
    /// </summary>
    public string? AvoidanceStat { get; init; }

    /// <summary>
    /// Gets the DC for avoidance checks.
    /// </summary>
    public int AvoidanceDC { get; init; }

    /// <summary>
    /// Creates an OnEnter trigger.
    /// </summary>
    public static HazardTrigger OnEnter(float chance = 1.0f, string? avoidanceStat = null, int avoidanceDC = 0) => new()
    {
        Type = HazardTriggerType.OnEnter,
        Chance = Math.Clamp(chance, 0f, 1f),
        AvoidanceStat = avoidanceStat,
        AvoidanceDC = avoidanceDC
    };

    /// <summary>
    /// Creates a PerTurn trigger.
    /// </summary>
    public static HazardTrigger PerTurn(float chance = 0.5f, string? avoidanceStat = null, int avoidanceDC = 0) => new()
    {
        Type = HazardTriggerType.PerTurn,
        Chance = Math.Clamp(chance, 0f, 1f),
        AvoidanceStat = avoidanceStat,
        AvoidanceDC = avoidanceDC
    };

    /// <summary>
    /// Creates an Ambient trigger (always active).
    /// </summary>
    public static HazardTrigger Ambient() => new()
    {
        Type = HazardTriggerType.Ambient,
        Chance = 1.0f,
        AvoidanceStat = null,
        AvoidanceDC = 0
    };

    /// <summary>
    /// Checks if avoidance is possible.
    /// </summary>
    public bool CanAvoid => !string.IsNullOrEmpty(AvoidanceStat);
}
