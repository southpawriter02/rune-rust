namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Outcome of attempting to apply a status effect.
/// </summary>
public enum EffectApplicationOutcome
{
    /// <summary>Effect was newly applied to the target.</summary>
    Applied,

    /// <summary>Existing effect duration was refreshed.</summary>
    Refreshed,

    /// <summary>Existing effect stacks were increased.</summary>
    Stacked,

    /// <summary>Effect was blocked due to stacking rules (already active).</summary>
    Blocked,

    /// <summary>Target is immune to this effect.</summary>
    Immune,

    /// <summary>Effect definition was not found.</summary>
    NotFound
}
