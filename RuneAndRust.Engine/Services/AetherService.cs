using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Engine.Services;

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
/// </remarks>
public class AetherService : IAetherService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<AetherService> _logger;
    private readonly FluxState _fluxState;

    /// <summary>
    /// Threshold for significant flux changes that trigger events (≥15 points).
    /// </summary>
    private const int SignificantChangeThreshold = 15;

    public AetherService(IEventBus eventBus, ILogger<AetherService> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
        _fluxState = new FluxState();

        _logger.LogDebug("[Aether] Service initialized. MaxFlux={MaxFlux}, DefaultDissipation={Rate}",
            _fluxState.MaxFlux, _fluxState.DefaultDissipationRate);
    }

    /// <inheritdoc/>
    public int CurrentFlux => _fluxState.CurrentFlux;

    /// <inheritdoc/>
    public int MaxFlux => _fluxState.MaxFlux;

    /// <inheritdoc/>
    public int AddFlux(int amount)
    {
        if (amount < 0)
        {
            _logger.LogWarning("[Aether] AddFlux called with negative amount {Amount}. Treating as 0.", amount);
            amount = 0;
        }

        if (amount == 0)
        {
            _logger.LogDebug("[Aether] AddFlux called with 0. No change.");
            return _fluxState.CurrentFlux;
        }

        var previousFlux = _fluxState.CurrentFlux;
        var previousThreshold = _fluxState.Threshold;

        _fluxState.CurrentFlux = Math.Min(_fluxState.CurrentFlux + amount, _fluxState.MaxFlux);

        var currentThreshold = _fluxState.Threshold;

        _logger.LogDebug("[Aether] Added {Amount} flux. {Previous} -> {Current} ({Threshold})",
            amount, previousFlux, _fluxState.CurrentFlux, currentThreshold);

        PublishEventIfSignificant(previousFlux, previousThreshold, "AddFlux");

        return _fluxState.CurrentFlux;
    }

    /// <inheritdoc/>
    public int RemoveFlux(int amount)
    {
        if (amount < 0)
        {
            _logger.LogWarning("[Aether] RemoveFlux called with negative amount {Amount}. Treating as 0.", amount);
            amount = 0;
        }

        if (amount == 0)
        {
            _logger.LogDebug("[Aether] RemoveFlux called with 0. No change.");
            return _fluxState.CurrentFlux;
        }

        var previousFlux = _fluxState.CurrentFlux;
        var previousThreshold = _fluxState.Threshold;

        _fluxState.CurrentFlux = Math.Max(_fluxState.CurrentFlux - amount, 0);

        var currentThreshold = _fluxState.Threshold;

        _logger.LogDebug("[Aether] Removed {Amount} flux. {Previous} -> {Current} ({Threshold})",
            amount, previousFlux, _fluxState.CurrentFlux, currentThreshold);

        PublishEventIfSignificant(previousFlux, previousThreshold, "RemoveFlux");

        return _fluxState.CurrentFlux;
    }

    /// <inheritdoc/>
    public int DissipateFlux(int? amount = null)
    {
        var dissipationAmount = amount ?? _fluxState.DefaultDissipationRate;

        if (dissipationAmount < 0)
        {
            _logger.LogWarning("[Aether] DissipateFlux called with negative amount {Amount}. Treating as 0.", dissipationAmount);
            dissipationAmount = 0;
        }

        if (_fluxState.CurrentFlux == 0)
        {
            _logger.LogDebug("[Aether] DissipateFlux called but flux is already at 0. No change.");
            return 0;
        }

        var previousFlux = _fluxState.CurrentFlux;
        var previousThreshold = _fluxState.Threshold;

        _fluxState.CurrentFlux = Math.Max(_fluxState.CurrentFlux - dissipationAmount, 0);

        var currentThreshold = _fluxState.Threshold;

        _logger.LogDebug("[Aether] Dissipated {Amount} flux. {Previous} -> {Current} ({Threshold})",
            dissipationAmount, previousFlux, _fluxState.CurrentFlux, currentThreshold);

        PublishEventIfSignificant(previousFlux, previousThreshold, "Dissipation");

        return _fluxState.CurrentFlux;
    }

    /// <inheritdoc/>
    public void ResetFlux()
    {
        var previousFlux = _fluxState.CurrentFlux;
        var previousThreshold = _fluxState.Threshold;

        _fluxState.CurrentFlux = 0;

        _logger.LogDebug("[Aether] Reset flux to 0. Previous={Previous}", previousFlux);

        if (previousFlux > 0)
        {
            PublishEventIfSignificant(previousFlux, previousThreshold, "Reset");
        }
    }

    /// <inheritdoc/>
    public FluxState GetFluxState() => _fluxState.Clone();

    /// <inheritdoc/>
    public FluxThreshold GetThreshold() => _fluxState.Threshold;

    /// <summary>
    /// Publishes a FluxChangedEvent if the change is significant.
    /// Significant changes are:
    /// 1. Threshold changed (e.g., Safe → Elevated)
    /// 2. Flux change ≥ 15 points in single operation
    /// 3. Flux hits boundary (0 or MaxFlux)
    /// Note: Only publishes if an actual change occurred (delta > 0).
    /// </summary>
    private void PublishEventIfSignificant(int previousFlux, FluxThreshold previousThreshold, string reason)
    {
        var delta = Math.Abs(_fluxState.CurrentFlux - previousFlux);

        // No change occurred - don't publish
        if (delta == 0)
        {
            return;
        }

        var thresholdChanged = previousThreshold != _fluxState.Threshold;
        var hitBoundary = _fluxState.CurrentFlux == 0 || _fluxState.CurrentFlux == _fluxState.MaxFlux;
        var significantChange = delta >= SignificantChangeThreshold;

        if (thresholdChanged || hitBoundary || significantChange)
        {
            var fluxEvent = new FluxChangedEvent(
                previousFlux,
                _fluxState.CurrentFlux,
                _fluxState.MaxFlux,
                previousThreshold,
                _fluxState.Threshold,
                reason);

            _logger.LogInformation(
                "[Aether] Publishing FluxChangedEvent: {Previous} -> {Current} ({PrevThreshold} -> {CurrThreshold}). " +
                "Reason={Reason}, ThresholdChanged={ThresholdChanged}, HitBoundary={HitBoundary}, Significant={Significant}",
                previousFlux, _fluxState.CurrentFlux, previousThreshold, _fluxState.Threshold,
                reason, thresholdChanged, hitBoundary, significantChange);

            _eventBus.Publish(fluxEvent);
        }
    }
}
