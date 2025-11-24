namespace RuneAndRust.Core.ChallengeSectors;

/// <summary>
/// v0.40.2: Challenge modifier categories
/// Organizes modifiers by their primary gameplay impact
/// </summary>
public enum ChallengeModifierCategory
{
    /// <summary>Affects combat mechanics directly (damage, abilities, boss encounters)</summary>
    Combat,

    /// <summary>Affects economy and resources (healing, loot, stamina, aether)</summary>
    Resource,

    /// <summary>Affects hazards and terrain (lava floors, darkness, reality tears)</summary>
    Environmental,

    /// <summary>Affects Trauma Economy (stress, corruption, Breaking Points)</summary>
    Psychological,

    /// <summary>Limits player options (time limits, equipment locks, ability restrictions)</summary>
    Restriction
}
