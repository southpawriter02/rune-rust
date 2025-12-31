using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing environmental flux (aetheric volatility) during combat.
/// </summary>
/// <remarks>
/// See: v0.4.3a (The Aether) for implementation details.
///
/// Flux is a per-encounter property that:
/// - Accumulates when spells are cast
/// - Dissipates each combat round (default 5 per round)
/// - Resets at combat start and end
///
/// The service publishes FluxChangedEvent when:
/// 1. Threshold changes (e.g., Safe → Elevated)
/// 2. Flux change ≥ 15 points in single operation
/// 3. Flux hits boundary (0 or MaxFlux)
/// </remarks>
public interface IAetherService
{
    /// <summary>
    /// Adds flux to the current pool.
    /// </summary>
    /// <param name="amount">The amount of flux to add (must be non-negative).</param>
    /// <returns>The new flux level after addition.</returns>
    int AddFlux(int amount);

    /// <summary>
    /// Removes flux from the current pool.
    /// </summary>
    /// <param name="amount">The amount of flux to remove (must be non-negative).</param>
    /// <returns>The new flux level after removal.</returns>
    int RemoveFlux(int amount);

    /// <summary>
    /// Dissipates flux at the end of a combat round.
    /// </summary>
    /// <param name="amount">
    /// Optional custom dissipation amount. If null, uses the default rate (5).
    /// </param>
    /// <returns>The new flux level after dissipation.</returns>
    int DissipateFlux(int? amount = null);

    /// <summary>
    /// Resets flux to zero. Called at combat start and end.
    /// </summary>
    void ResetFlux();

    /// <summary>
    /// Gets a copy of the current flux state.
    /// </summary>
    /// <returns>A copy of the current FluxState.</returns>
    FluxState GetFluxState();

    /// <summary>
    /// Gets the current flux threshold tier.
    /// </summary>
    /// <returns>The current FluxThreshold.</returns>
    FluxThreshold GetThreshold();

    /// <summary>
    /// Gets the current flux level.
    /// </summary>
    int CurrentFlux { get; }

    /// <summary>
    /// Gets the maximum flux level.
    /// </summary>
    int MaxFlux { get; }
}
