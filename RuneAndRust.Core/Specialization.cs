namespace RuneAndRust.Core;

/// <summary>
/// v0.7: Specializations are unlocked with 3 PP during gameplay
/// Each archetype has multiple specializations available
/// </summary>
public enum Specialization
{
    None,  // Character has not chosen a specialization yet

    // Warrior specializations (v0.19+)
    SkarHordeAspirant,  // v0.19.1: Savage berserker with Savagery resource
    IronBane,           // v0.19.2: Anti-mechanical/undying zealot with Righteous Fervor
    AtgeirWielder,      // v0.19.3: Versatile reach weapon specialist

    // Adept specializations (v0.19+)
    BoneSetter,         // v0.19.4: Support/Healer - Non-magical medic
    ScrapTinker,        // v0.19.6: Crafting specialist - Brewmaster & gadgeteer
    JotunReader,        // v0.19.7: Utility/Analyst - System diagnostician

    // Mystic specializations (v0.19.8+)
    VardWarden,         // v0.19.8: Defensive caster - Runic barriers & sanctified ground
    RustWitch           // v0.19.8: Heretical debuffer - Corrosion & entropy magic
}
