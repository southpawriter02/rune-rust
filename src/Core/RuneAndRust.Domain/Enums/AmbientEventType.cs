namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of ambient events.
/// </summary>
public enum AmbientEventType
{
    /// <summary>An auditory event (distant sound, noise).</summary>
    Sound,

    /// <summary>A visual event (shadow movement, light flicker).</summary>
    Visual,

    /// <summary>A small creature event (rats, bats, insects).</summary>
    Creature,

    /// <summary>An environmental event (dust, water drip, wind).</summary>
    Environmental
}
