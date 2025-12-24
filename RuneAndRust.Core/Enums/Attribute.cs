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
    Sturdiness = 0,

    /// <summary>
    /// Raw physical power and strength.
    /// </summary>
    Might = 1,

    /// <summary>
    /// Mental acuity, perception, and intelligence.
    /// </summary>
    Wits = 2,

    /// <summary>
    /// Mental fortitude, determination, and resistance to manipulation.
    /// </summary>
    Will = 3,

    /// <summary>
    /// Agility, dexterity, and precision.
    /// </summary>
    Finesse = 4
}
