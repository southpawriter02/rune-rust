using System;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Engine.Services
{
    public class AetherService : IAetherService
    {
        private readonly FluxState _state;
        private readonly IEventBus _eventBus;
        private readonly ILogger<AetherService> _logger;
        private readonly object _lock = new();

        public AetherService(IEventBus eventBus, ILogger<AetherService> logger)
        {
            _state = new FluxState();
            _eventBus = eventBus;
            _logger = logger;
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
                return GetThresholdForValue(_state.CurrentValue);
            }
        }

        public int ModifyFlux(int amount, string source)
        {
            lock (_lock)
            {
                int oldValue = _state.CurrentValue;
                int newValue = Math.Clamp(oldValue + amount, 0, 100);

                if (oldValue != newValue)
                {
                    _state.CurrentValue = newValue;
                    var oldThreshold = GetThresholdForValue(oldValue);
                    var newThreshold = GetThresholdForValue(newValue);

                    // Structured logging per Domain 4 requirements
                    _logger.LogDebug("Flux modified by {Source}: {Old} -> {New}", source, oldValue, newValue);

                    if (oldThreshold != newThreshold)
                    {
                        _logger.LogInformation("Flux threshold changed: {OldThreshold} -> {NewThreshold}", oldThreshold, newThreshold);
                    }

                    _eventBus.Publish(new FluxChangedEvent(oldValue, newValue, source, newThreshold));
                }

                return _state.CurrentValue;
            }
        }

        public int DissipateFlux()
        {
            lock (_lock)
            {
                // Domain 4: "The weave settles, swallowing the turbulence."
                int amount = _state.DissipationRate;
                int oldValue = _state.CurrentValue;

                // Use ModifyFlux to handle clamping, events, and logging
                // Negate amount because we are reducing flux
                ModifyFlux(-amount, "Dissipation");

                int dissipated = oldValue - _state.CurrentValue;
                if (dissipated > 0)
                {
                    _logger.LogTrace("Flux dissipated: {Old} -> {New}", oldValue, _state.CurrentValue);
                }

                return dissipated;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _logger.LogInformation("Resetting local Flux state to zero.");
                ModifyFlux(-100, "Reset");
            }
        }

        private FluxThreshold GetThresholdForValue(int value)
        {
            return value switch
            {
                >= 75 => FluxThreshold.Overload,
                >= 50 => FluxThreshold.Critical,
                >= 25 => FluxThreshold.Elevated,
                _ => FluxThreshold.Safe
            };
        }
    }
}
