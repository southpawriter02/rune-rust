using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Event published when flux changes significantly.
/// </summary>
/// <remarks>
/// See: v0.4.3a (The Aether) for Flux State & Service implementation.
///
/// This event is published when ANY of the following occur:
/// 1. Threshold changed (e.g., Safe → Elevated)
/// 2. Flux change ≥ 15 points in single operation
/// 3. Flux hits boundary (0 or MaxFlux)
/// </remarks>
/// <param name="PreviousFlux">The flux level before the change.</param>
/// <param name="CurrentFlux">The flux level after the change.</param>
/// <param name="MaxFlux">The maximum flux capacity.</param>
/// <param name="PreviousThreshold">The threshold tier before the change.</param>
/// <param name="CurrentThreshold">The threshold tier after the change.</param>
/// <param name="ChangeReason">Description of what caused the flux change.</param>
public record FluxChangedEvent(
    int PreviousFlux,
    int CurrentFlux,
    int MaxFlux,
    FluxThreshold PreviousThreshold,
    FluxThreshold CurrentThreshold,
    string ChangeReason)
{
    /// <summary>
    /// The absolute change in flux level.
    /// </summary>
    public int Delta => Math.Abs(CurrentFlux - PreviousFlux);

    /// <summary>
    /// Whether this change crossed a threshold boundary.
    /// </summary>
    public bool CrossedThreshold => PreviousThreshold != CurrentThreshold;

    /// <summary>
    /// Whether flux hit a boundary (0 or MaxFlux).
    /// </summary>
    public bool HitBoundary => CurrentFlux == 0 || CurrentFlux == MaxFlux;

    /// <summary>
    /// Whether the change was significant (≥15 points).
    /// </summary>
    public bool IsSignificantChange => Delta >= 15;

    /// <summary>
    /// Whether flux increased (positive change).
    /// </summary>
    public bool IsIncrease => CurrentFlux > PreviousFlux;

    /// <summary>
    /// Whether flux decreased (negative change).
    /// </summary>
    public bool IsDecrease => CurrentFlux < PreviousFlux;
}
