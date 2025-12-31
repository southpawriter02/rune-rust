using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Magic;

/// <summary>
/// Represents the current state of environmental flux (aetheric volatility).
/// Flux accumulates with spellcasting and dissipates over time during combat.
/// </summary>
/// <remarks>
/// See: v0.4.3a (The Aether) for Flux State & Service implementation.
///
/// Flux is a per-encounter property ranging from 0 to MaxFlux (default 100).
/// The threshold is computed based on the current flux level:
/// - Safe: 0-24
/// - Elevated: 25-49
/// - Critical: 50-74
/// - Overload: 75-100
/// </remarks>
public class FluxState
{
    /// <summary>
    /// The current flux level (0 to MaxFlux).
    /// </summary>
    public int CurrentFlux { get; set; }

    /// <summary>
    /// The maximum flux level (default 100).
    /// </summary>
    public int MaxFlux { get; set; } = 100;

    /// <summary>
    /// The default dissipation rate per round.
    /// </summary>
    public int DefaultDissipationRate { get; set; } = 5;

    /// <summary>
    /// Gets the current threshold tier based on CurrentFlux.
    /// </summary>
    public FluxThreshold Threshold => CurrentFlux switch
    {
        >= 75 => FluxThreshold.Overload,
        >= 50 => FluxThreshold.Critical,
        >= 25 => FluxThreshold.Elevated,
        _ => FluxThreshold.Safe
    };

    /// <summary>
    /// Gets the flux level as a percentage of MaxFlux (0.0 to 1.0).
    /// </summary>
    public double FluxPercentage => MaxFlux > 0 ? (double)CurrentFlux / MaxFlux : 0;

    /// <summary>
    /// Indicates whether flux is at maximum capacity.
    /// </summary>
    public bool IsAtMax => CurrentFlux >= MaxFlux;

    /// <summary>
    /// Indicates whether flux is at zero (fully dissipated).
    /// </summary>
    public bool IsAtZero => CurrentFlux <= 0;

    /// <summary>
    /// Creates a copy of this FluxState.
    /// </summary>
    public FluxState Clone() => new()
    {
        CurrentFlux = CurrentFlux,
        MaxFlux = MaxFlux,
        DefaultDissipationRate = DefaultDissipationRate
    };
}
