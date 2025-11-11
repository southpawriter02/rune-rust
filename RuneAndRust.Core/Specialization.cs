namespace RuneAndRust.Core;

/// <summary>
/// v0.7: Specializations are unlocked with 10 PP during gameplay
/// Each archetype has 3 specializations available
/// </summary>
public enum Specialization
{
    None,  // Character has not chosen a specialization yet

    // Warrior specializations (future - not yet implemented)
    Berserker,
    ShieldBearer,
    WeaponMaster,

    // Adept specializations (v0.7)
    BoneSetter,     // Support/Healer - Non-magical medic
    JotunReader,    // Utility/Analyst - System diagnostician
    Skald           // Buffer/Debuffer - Morale officer
}
