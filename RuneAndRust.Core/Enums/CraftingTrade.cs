namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the crafting trade specializations available in Rune &amp; Rust.
/// Each trade represents a distinct set of skills and recipe types.
/// </summary>
public enum CraftingTrade
{
    /// <summary>
    /// Mechanical repairs, improvised tools, and salvage work.
    /// Uses scraps and found materials to create functional equipment.
    /// </summary>
    Bodging = 0,

    /// <summary>
    /// Potions, salves, and chemical compounds.
    /// Transforms raw ingredients into consumable effects.
    /// </summary>
    Alchemy = 1,

    /// <summary>
    /// Aetheric inscriptions and enchantments.
    /// Channels residual Aesir energy into functional devices.
    /// </summary>
    Runeforging = 2,

    /// <summary>
    /// Bandages, stimulants, and medical kits.
    /// Creates healing supplies from available materials.
    /// </summary>
    FieldMedicine = 3
}
