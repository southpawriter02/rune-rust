using System;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when the environmental Flux value changes significantly (v0.4.3a).
/// Consumed by UI and Ambience services.
/// </summary>
public record FluxChangedEvent(
    int OldValue,
    int NewValue,
    string Source,
    string ThresholdName);
