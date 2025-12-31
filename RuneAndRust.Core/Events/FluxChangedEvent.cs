using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Events
{
    public record FluxChangedEvent(
        int OldValue,
        int NewValue,
        string Source,
        FluxThreshold Threshold
    );
}
