namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the character lineages available in Rune &amp; Rust.
/// Lineage represents the character's heritage and provides unique traits.
/// </summary>
/// <remarks>See: SPEC-CHAR-001, Section "Attribute Bonuses by Lineage"</remarks>
public enum LineageType
{
    /// <summary>
    /// Standard human survivor. Adaptable and resilient.
    /// Bonus: +1 to all attributes.
    /// </summary>
    Human = 0,

    /// <summary>
    /// Descendants bearing ancient runic inscriptions in their skin.
    /// Bonus: +2 Wits, +2 Will, but -1 Sturdiness.
    /// </summary>
    RuneMarked = 1,

    /// <summary>
    /// Those with machine-integrated bloodlines from the old world.
    /// Bonus: +2 Sturdiness, +2 Might, but -1 Wits.
    /// </summary>
    IronBlooded = 2,

    /// <summary>
    /// Wolf-kin bearing the curse of the northern wastes.
    /// Bonus: +2 Finesse, +2 Wits, but -1 Will.
    /// </summary>
    VargrKin = 3
}
