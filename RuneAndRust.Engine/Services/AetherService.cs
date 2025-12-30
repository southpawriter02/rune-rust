using System;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Engine.Services
{
    /// <summary>
    /// Singleton service for managing environmental Flux (v0.4.3a).
    /// Tracks magic volatility and publishes events on significant changes.
    /// Thread-safe for concurrent modifications.
    /// </summary>
    public class AetherService : IAetherService
    {
        private readonly ILogger<AetherService> _logger;
        private readonly IEventBus _eventBus;
        private readonly FluxState _state;
        private readonly object _lock = new object();

        public AetherService(ILogger<AetherService> logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
            _state = new FluxState();

            _logger.LogTrace("AetherService initialized. Initial Flux: {InitialFlux}", _state.CurrentValue);
        }

        public void ModifyFlux(int amount, string source, string context = "Unspecified")
        {
            _logger.LogTrace("ModifyFlux called for {Context}: Amount {Amount}, Source {Source}",
                context, amount, source);

            int oldValue;
            int newValue;
            FluxThreshold oldThreshold;
            FluxThreshold newThreshold;

            lock (_lock)
            {
                oldValue = _state.CurrentValue;
                oldThreshold = GetThresholdInternal(oldValue);

                // Apply modification
                _state.CurrentValue += amount;

                // Clamp values
                if (_state.CurrentValue < 0)
                {
                    _logger.LogWarning("Flux dropped below zero ({Value}) for {Context}. Clamping to 0.",
                        _state.CurrentValue, context);
                    _state.CurrentValue = 0;
                }
                else if (_state.CurrentValue > _state.MaxValue)
                {
                    _logger.LogWarning("Flux exceeded max ({Value}) for {Context}. Clamping to {Max}.",
                        _state.CurrentValue, context, _state.MaxValue);
                    _state.CurrentValue = _state.MaxValue;
                }

                newValue = _state.CurrentValue;
                newThreshold = GetThresholdInternal(newValue);
            }

            // Only publish/log if changed
            if (oldValue != newValue)
            {
                _logger.LogDebug("Flux modified for {Context}: {OldValue} -> {NewValue} (Source: {Source})",
                    context, oldValue, newValue, source);

                _eventBus.Publish(new FluxChangedEvent(oldValue, newValue, source, newThreshold.ToString()));

                if (oldThreshold != newThreshold)
                {
                    _logger.LogInformation("Flux threshold changed for {Context}: {OldThreshold} -> {NewThreshold}",
                        context, oldThreshold, newThreshold);
                }
            }
            else
            {
                _logger.LogTrace("Flux modification resulted in no change for {Context}.", context);
            }
        }

        public void DissipateFlux(string context = "Unspecified")
        {
            int rate;
            lock (_lock)
            {
                rate = _state.DissipationRate;
            }

            _logger.LogTrace("DissipateFlux called for {Context}. Rate: {Rate}",
                context, rate);

            // We use ModifyFlux to handle clamping and events safely
            // However, we need to check if we are > 0 first to avoid unnecessary logs,
            // but ModifyFlux handles "no change" logic too.
            // For strict traceability, we'll check current value first.
            int current = GetCurrentFlux();

            if (current > 0)
            {
                ModifyFlux(-rate, "Natural Dissipation", context);
            }
            else
            {
                _logger.LogTrace("Flux already at 0 for {Context}, no dissipation needed.", context);
            }

            _logger.LogDebug("DissipateFlux complete for {Context}.", context);
        }

        public int GetCurrentFlux()
        {
            lock (_lock)
            {
                return _state.CurrentValue;
            }
        }

        public FluxThreshold GetThreshold()
        {
            lock (_lock)
            {
                return GetThresholdInternal(_state.CurrentValue);
            }
        }

        private FluxThreshold GetThresholdInternal(int value)
        {
            if (value >= 75) return FluxThreshold.Overload;
            if (value >= 50) return FluxThreshold.Critical;
            if (value >= 25) return FluxThreshold.Elevated;
            return FluxThreshold.Safe;
        }

        public void Reset(string context = "Unspecified")
        {
            _logger.LogTrace("Reset called for {Context}.", context);

            int oldValue;
            lock (_lock)
            {
                oldValue = _state.CurrentValue;
                _state.CurrentValue = 0;
            }

            if (oldValue != 0)
            {
                _eventBus.Publish(new FluxChangedEvent(oldValue, 0, "System Reset", FluxThreshold.Safe.ToString()));
            }

            _logger.LogDebug("Flux reset to 0 for {Context}.", context);
        }

        public FluxState GetState()
        {
            lock (_lock)
            {
                // Return a copy to prevent external mutation
                return new FluxState
                {
                    CurrentValue = _state.CurrentValue,
                    DissipationRate = _state.DissipationRate,
                    CriticalThreshold = _state.CriticalThreshold,
                    MaxValue = _state.MaxValue
                };
            }
        }
    }
}
