// ------------------------------------------------------------------------------
// <copyright file="SalvageableComponent.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Value object representing a component that can be salvaged from locks
// on critical success during lockpicking attempts.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a component that can be salvaged from a lock on critical success.
/// </summary>
/// <remarks>
/// <para>
/// Salvageable components are materials recovered from successfully bypassed locks.
/// These can be used in crafting or sold for currency.
/// </para>
/// <para>
/// Component availability depends on lock type:
/// <list type="bullet">
///   <item><description>Improvised Latch: [Wire Bundle], [Small Spring] (Common)</description></item>
///   <item><description>Simple/Standard Lock: [High-Tension Spring], [Pin Set] (Uncommon)</description></item>
///   <item><description>Complex/Master Lock: [Circuit Fragment], [Power Cell Fragment] (Rare)</description></item>
///   <item><description>Jötun-Forged: [Encryption Chip], [Biometric Sensor] (Legendary)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="ComponentId">Unique identifier for the component type.</param>
/// <param name="Name">Display name of the component.</param>
/// <param name="Description">Flavor text describing the component.</param>
/// <param name="Quantity">Number of this component salvaged (default 1).</param>
/// <param name="Rarity">Rarity tier of the component.</param>
/// <param name="BaseValue">Base monetary value of the component.</param>
/// <param name="Weight">Weight of the component in units.</param>
/// <param name="CraftingTags">Tags indicating crafting categories this component belongs to.</param>
public readonly record struct SalvageableComponent(
    string ComponentId,
    string Name,
    string Description,
    int Quantity = 1,
    ItemRarity Rarity = ItemRarity.Common,
    int BaseValue = 1,
    float Weight = 0.1f,
    IReadOnlyList<string>? CraftingTags = null)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the total value of this component stack (base value * quantity * rarity multiplier).
    /// </summary>
    public int TotalValue => (int)(BaseValue * Quantity * GetRarityMultiplier());

    /// <summary>
    /// Gets the total weight of this component stack.
    /// </summary>
    public float TotalWeight => Weight * Quantity;

    /// <summary>
    /// Gets whether this component has any crafting tags.
    /// </summary>
    public bool HasCraftingTags => CraftingTags?.Count > 0;

    /// <summary>
    /// Gets the crafting tags or an empty list if none.
    /// </summary>
    public IReadOnlyList<string> Tags => CraftingTags ?? Array.Empty<string>();

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a formatted string for UI display.
    /// </summary>
    /// <returns>A formatted display string with quantity and name.</returns>
    public string ToDisplayString()
    {
        var qty = Quantity > 1 ? $" x{Quantity}" : "";
        return $"[{Name}]{qty}";
    }

    /// <summary>
    /// Creates a detailed string including rarity and value.
    /// </summary>
    /// <returns>A detailed display string.</returns>
    public string ToDetailedString()
    {
        var qty = Quantity > 1 ? $" x{Quantity}" : "";
        return $"[{Name}]{qty} ({Rarity}) - Value: {TotalValue}";
    }

    /// <summary>
    /// Creates a log-friendly string representation.
    /// </summary>
    /// <returns>A string suitable for logging.</returns>
    public string ToLogString()
    {
        return $"Component[{ComponentId}:{Name}] qty={Quantity} rarity={Rarity} value={TotalValue}";
    }

    /// <inheritdoc/>
    public override string ToString() => ToDisplayString();

    // -------------------------------------------------------------------------
    // Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the value multiplier based on rarity.
    /// </summary>
    private float GetRarityMultiplier()
    {
        return Rarity switch
        {
            ItemRarity.Common => 1.0f,
            ItemRarity.Uncommon => 2.0f,
            ItemRarity.Rare => 5.0f,
            ItemRarity.Epic => 10.0f,
            ItemRarity.Legendary => 25.0f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Determines if this component has a specific crafting tag.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    /// <returns>True if the component has the specified tag.</returns>
    public bool HasTag(string tag)
    {
        return CraftingTags?.Contains(tag, StringComparer.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Creates a new component with a different quantity.
    /// </summary>
    /// <param name="newQuantity">The new quantity.</param>
    /// <returns>A new SalvageableComponent with the updated quantity.</returns>
    public SalvageableComponent WithQuantity(int newQuantity)
    {
        return this with { Quantity = newQuantity };
    }

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a wire bundle component (common, from improvised latches).
    /// </summary>
    public static SalvageableComponent WireBundle(int quantity = 1) => new(
        ComponentId: "wire-bundle",
        Name: "Wire Bundle",
        Description: "A bundle of salvaged wires, useful for improvised tools and repairs.",
        Quantity: quantity,
        Rarity: ItemRarity.Common,
        BaseValue: 2,
        Weight: 0.1f,
        CraftingTags: new[] { "mechanical", "wire", "improvised" });

    /// <summary>
    /// Creates a small spring component (common, from improvised latches).
    /// </summary>
    public static SalvageableComponent SmallSpring(int quantity = 1) => new(
        ComponentId: "spring-small",
        Name: "Small Spring",
        Description: "A basic mechanical spring, a fundamental component for many devices.",
        Quantity: quantity,
        Rarity: ItemRarity.Common,
        BaseValue: 3,
        Weight: 0.05f,
        CraftingTags: new[] { "mechanical", "spring", "basic" });

    /// <summary>
    /// Creates a high-tension spring component (uncommon, from simple/standard locks).
    /// </summary>
    public static SalvageableComponent HighTensionSpring(int quantity = 1) => new(
        ComponentId: "high-tension-spring",
        Name: "High-Tension Spring",
        Description: "A quality mechanical spring under high tension. Valuable for crafting.",
        Quantity: quantity,
        Rarity: ItemRarity.Uncommon,
        BaseValue: 8,
        Weight: 0.1f,
        CraftingTags: new[] { "mechanical", "spring", "quality" });

    /// <summary>
    /// Creates a pin set component (uncommon, from simple/standard locks).
    /// </summary>
    public static SalvageableComponent PinSet(int quantity = 1) => new(
        ComponentId: "pin-set",
        Name: "Pin Set",
        Description: "A set of precision pins from a lock mechanism. Useful for crafting lockpicks.",
        Quantity: quantity,
        Rarity: ItemRarity.Uncommon,
        BaseValue: 10,
        Weight: 0.05f,
        CraftingTags: new[] { "mechanical", "precision", "lockpick-material" });

    /// <summary>
    /// Creates a circuit fragment component (rare, from complex/master locks).
    /// </summary>
    public static SalvageableComponent CircuitFragment(int quantity = 1) => new(
        ComponentId: "circuit-fragment",
        Name: "Circuit Fragment",
        Description: "A fragment of Old World circuitry. Highly valued by Scrap-Tinkers.",
        Quantity: quantity,
        Rarity: ItemRarity.Rare,
        BaseValue: 25,
        Weight: 0.02f,
        CraftingTags: new[] { "electronic", "circuit", "old-world" });

    /// <summary>
    /// Creates a power cell fragment component (rare, from complex/master locks).
    /// </summary>
    public static SalvageableComponent PowerCellFragment(int quantity = 1) => new(
        ComponentId: "power-cell-fragment",
        Name: "Power Cell Fragment",
        Description: "A partially depleted power cell. Can be recharged or used for parts.",
        Quantity: quantity,
        Rarity: ItemRarity.Rare,
        BaseValue: 30,
        Weight: 0.15f,
        CraftingTags: new[] { "electronic", "power", "energy-source" });

    /// <summary>
    /// Creates an encryption chip component (legendary, from Jötun-forged locks).
    /// </summary>
    public static SalvageableComponent EncryptionChip(int quantity = 1) => new(
        ComponentId: "encryption-chip",
        Name: "Encryption Chip",
        Description: "A rare Old World encryption chip. Its secrets are incomprehensible to most.",
        Quantity: quantity,
        Rarity: ItemRarity.Legendary,
        BaseValue: 100,
        Weight: 0.01f,
        CraftingTags: new[] { "electronic", "encryption", "old-world", "jotun" });

    /// <summary>
    /// Creates a biometric sensor component (legendary, from Jötun-forged locks).
    /// </summary>
    public static SalvageableComponent BiometricSensor(int quantity = 1) => new(
        ComponentId: "biometric-sensor",
        Name: "Biometric Sensor",
        Description: "An advanced security component from the World Before. Exceedingly rare.",
        Quantity: quantity,
        Rarity: ItemRarity.Legendary,
        BaseValue: 150,
        Weight: 0.03f,
        CraftingTags: new[] { "electronic", "security", "old-world", "jotun", "biometric" });

    /// <summary>
    /// Creates an empty/null component (no salvage).
    /// </summary>
    public static SalvageableComponent None => new(
        ComponentId: "none",
        Name: "Nothing",
        Description: "No salvageable components.",
        Quantity: 0,
        Rarity: ItemRarity.Common,
        BaseValue: 0,
        Weight: 0f);
}
