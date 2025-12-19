namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the category of an interactable object in the game world.
/// Determines available interaction options and default behaviors.
/// </summary>
public enum ObjectType
{
    /// <summary>
    /// Decorative objects that provide atmosphere but have minimal interaction.
    /// Examples: collapsed tables, shattered cabinets, debris piles.
    /// </summary>
    Furniture = 0,

    /// <summary>
    /// Objects that can be opened/closed and may contain items.
    /// Examples: chests, lockers, crates, barrels.
    /// </summary>
    Container = 1,

    /// <summary>
    /// Mechanical or technological objects that can be activated.
    /// Examples: terminals, consoles, switches, machinery.
    /// </summary>
    Device = 2,

    /// <summary>
    /// Text, runes, or symbols that can be read or deciphered.
    /// Examples: wall inscriptions, ancient tablets, warning signs.
    /// </summary>
    Inscription = 3,

    /// <summary>
    /// Remains of creatures or people that can be searched.
    /// Examples: fallen rangers, ancient bones, creature remains.
    /// </summary>
    Corpse = 4
}
