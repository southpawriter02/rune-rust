namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Severity levels for game events.
/// </summary>
public enum EventSeverity
{
    /// <summary>Fine-grained tracing for debugging.</summary>
    Trace,

    /// <summary>Debug information for development.</summary>
    Debug,

    /// <summary>Standard game events.</summary>
    Info,

    /// <summary>Notable events that may need attention.</summary>
    Warning,

    /// <summary>Error events that affect gameplay.</summary>
    Error,

    /// <summary>Critical failures.</summary>
    Critical
}
