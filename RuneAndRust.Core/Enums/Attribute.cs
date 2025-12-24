using RuneAndRust.Core.Attributes;

namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the five core character attributes in Rune &amp; Rust.
/// These attributes form the foundation of all dice pool calculations.
/// </summary>
/// <remarks>See: SPEC-CHAR-001, Section "Core Attributes"</remarks>
public enum Attribute
{
    /// <summary>
    /// Physical resilience, health, and endurance.
    /// </summary>
    [GameDocument(
        "Sturdiness",
        "The body's capacity to endure punishment and persist through hardship. Sturdiness determines how much damage one can absorb before falling, and influences resistance to physical ailments. A survivor with high Sturdiness stands where others collapse.")]
    Sturdiness = 0,

    /// <summary>
    /// Raw physical power and strength.
    /// </summary>
    [GameDocument(
        "Might",
        "Raw physical power channeled through muscle and bone. Might governs the force behind weapon strikes and the ability to break through obstacles. Those blessed with Might carry heavier burdens and strike with devastating impact.")]
    Might = 1,

    /// <summary>
    /// Mental acuity, perception, and intelligence.
    /// </summary>
    [GameDocument(
        "Wits",
        "The sharpness of mind that separates the vigilant from the ambushed. Wits governs perception, tactical awareness, and the ability to read enemy intentions. A keen-witted survivor sees the attack before it lands.")]
    Wits = 2,

    /// <summary>
    /// Mental fortitude, determination, and resistance to manipulation.
    /// </summary>
    [GameDocument(
        "Will",
        "The fortress of the mind against psychological assault. Will determines resistance to trauma, manipulation, and the horrors of the Blight-touched wastes. Those with iron will maintain their sanity where others break.")]
    Will = 3,

    /// <summary>
    /// Agility, dexterity, and precision.
    /// </summary>
    [GameDocument(
        "Finesse",
        "The harmony of hand and eye, speed and precision. Finesse governs evasion, accuracy with ranged weapons, and delicate manipulations. A survivor of high Finesse moves like water through danger.")]
    Finesse = 4
}
