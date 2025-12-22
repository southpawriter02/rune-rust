namespace RuneAndRust.Core.Enums;

/// <summary>
/// Narrative background types that shape the character's prologue and story (v0.3.4c).
/// Each background provides unique flavor text during the prologue sequence.
/// </summary>
public enum BackgroundType
{
    /// <summary>Born among the rust heaps, surviving on salvage.</summary>
    Scavenger = 0,

    /// <summary>Cast out from a settlement for crimes real or imagined.</summary>
    Exile = 1,

    /// <summary>Seeker of forbidden knowledge from the old world.</summary>
    Scholar = 2,

    /// <summary>Former soldier from a fallen outpost.</summary>
    Soldier = 3,

    /// <summary>Survivor of noble lineage, now dispossessed.</summary>
    Noble = 4,

    /// <summary>Once a follower of a corrupted faith.</summary>
    Cultist = 5
}
