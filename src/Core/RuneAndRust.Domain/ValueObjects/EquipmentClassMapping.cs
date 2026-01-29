// ═══════════════════════════════════════════════════════════════════════════════
// EquipmentClassMapping.cs
// Value object mapping equipment categories to archetype affinities.
// Version: 0.16.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Maps an equipment category to its archetype affinity.
/// </summary>
/// <remarks>
/// <para>
/// EquipmentClassMapping is loaded from configuration and provides the
/// data needed for smart loot generation to filter drops by class.
/// </para>
/// <para>
/// Each mapping associates a category ID (e.g., "axes", "daggers") with:
/// <list type="bullet">
///   <item><description>An <see cref="EquipmentClassAffinity"/> value (Warrior, Skirmisher, etc.)</description></item>
///   <item><description>A primary attribute that the category scales with (MIGHT, FINESSE, etc.)</description></item>
///   <item><description>An equipment slot (Weapon, Armor, Accessory)</description></item>
///   <item><description>Example items for reference</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="CategoryId">Unique category identifier (e.g., "axes", "daggers"). Always lowercase kebab-case.</param>
/// <param name="DisplayName">Human-readable category name for UI display.</param>
/// <param name="Affinity">The archetype this category is designed for.</param>
/// <param name="PrimaryAttribute">The attribute this category scales with (e.g., "MIGHT", "FINESSE"). Empty for Universal.</param>
/// <param name="EquipmentSlot">The slot type (Weapon, Armor, Accessory).</param>
/// <param name="ExampleItems">Example item names for reference and tooltips.</param>
/// <seealso cref="EquipmentClassAffinity"/>
public readonly record struct EquipmentClassMapping(
    string CategoryId,
    string DisplayName,
    EquipmentClassAffinity Affinity,
    string PrimaryAttribute,
    string EquipmentSlot,
    IReadOnlyList<string> ExampleItems)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this is universal (appropriate for all classes).
    /// </summary>
    /// <returns><c>true</c> if <see cref="Affinity"/> is <see cref="EquipmentClassAffinity.Universal"/>.</returns>
    public bool IsUniversal => Affinity == EquipmentClassAffinity.Universal;

    /// <summary>
    /// Gets whether this is a weapon category.
    /// </summary>
    /// <returns><c>true</c> if <see cref="EquipmentSlot"/> equals "Weapon" (case-insensitive).</returns>
    public bool IsWeapon => EquipmentSlot.Equals("Weapon", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this is an armor category.
    /// </summary>
    /// <returns><c>true</c> if <see cref="EquipmentSlot"/> equals "Armor" (case-insensitive).</returns>
    public bool IsArmor => EquipmentSlot.Equals("Armor", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this is an accessory category.
    /// </summary>
    /// <returns><c>true</c> if <see cref="EquipmentSlot"/> equals "Accessory" (case-insensitive).</returns>
    public bool IsAccessory => EquipmentSlot.Equals("Accessory", StringComparison.OrdinalIgnoreCase);

    // ═══════════════════════════════════════════════════════════════════════════
    // Instance Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if this category is appropriate for an archetype.
    /// </summary>
    /// <param name="archetypeId">The archetype to check (e.g., "warrior", "skirmisher").</param>
    /// <returns>
    /// <c>true</c> if this category is appropriate for the specified archetype,
    /// or if this is a <see cref="EquipmentClassAffinity.Universal"/> category.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Universal items are always appropriate for any archetype.
    /// </para>
    /// <para>
    /// Archetype matching is case-insensitive and normalized to lowercase.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var axesMapping = EquipmentClassMapping.Create("axes", "Axes", EquipmentClassAffinity.Warrior, ...);
    /// axesMapping.IsAppropriateFor("warrior");     // true
    /// axesMapping.IsAppropriateFor("skirmisher");  // false
    /// 
    /// var lightArmorMapping = EquipmentClassMapping.Create("light-armor", "Light Armor", EquipmentClassAffinity.Universal, ...);
    /// lightArmorMapping.IsAppropriateFor("mystic"); // true (Universal)
    /// </code>
    /// </example>
    public bool IsAppropriateFor(string archetypeId)
    {
        // Universal affinity is appropriate for all archetypes
        if (IsUniversal)
        {
            return true;
        }

        // Match archetype to corresponding affinity (case-insensitive)
        return archetypeId.ToLowerInvariant() switch
        {
            "warrior" => Affinity == EquipmentClassAffinity.Warrior,
            "skirmisher" => Affinity == EquipmentClassAffinity.Skirmisher,
            "mystic" => Affinity == EquipmentClassAffinity.Mystic,
            "adept" => Affinity == EquipmentClassAffinity.Adept,
            _ => false
        };
    }

    /// <summary>
    /// Formats example items as a comma-separated string.
    /// </summary>
    /// <returns>A comma-separated list of example items (e.g., "Hand Axe, Bearded Axe, Greataxe").</returns>
    public string FormatExamples() => string.Join(", ", ExampleItems);

    /// <summary>
    /// Creates a display string for debug and logging purposes.
    /// </summary>
    /// <returns>A formatted string showing display name, category ID, affinity, and primary attribute.</returns>
    public override string ToString() =>
        $"{DisplayName} ({CategoryId}): {Affinity} [{PrimaryAttribute}]";

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an EquipmentClassMapping with validation.
    /// </summary>
    /// <param name="categoryId">Unique category identifier (e.g., "axes"). Will be normalized to lowercase.</param>
    /// <param name="displayName">Human-readable category name.</param>
    /// <param name="affinity">The archetype affinity for this category.</param>
    /// <param name="primaryAttribute">The attribute this category scales with. Can be empty for Universal.</param>
    /// <param name="equipmentSlot">The slot type (Weapon, Armor, Accessory).</param>
    /// <param name="exampleItems">Example item names for this category.</param>
    /// <returns>A new <see cref="EquipmentClassMapping"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="categoryId"/>, <paramref name="displayName"/>, or 
    /// <paramref name="equipmentSlot"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exampleItems"/> is null.
    /// </exception>
    public static EquipmentClassMapping Create(
        string categoryId,
        string displayName,
        EquipmentClassAffinity affinity,
        string primaryAttribute,
        string equipmentSlot,
        IReadOnlyList<string> exampleItems)
    {
        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(equipmentSlot);
        ArgumentNullException.ThrowIfNull(exampleItems);

        // Normalize category ID to lowercase for consistent lookups
        return new EquipmentClassMapping(
            categoryId.ToLowerInvariant(),
            displayName,
            affinity,
            primaryAttribute ?? string.Empty,
            equipmentSlot,
            exampleItems);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory - Empty/Default
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an empty mapping used as a fallback for unknown categories.
    /// </summary>
    /// <remarks>
    /// The empty mapping has Universal affinity, meaning it will be appropriate
    /// for all archetypes. This prevents null reference issues when looking up
    /// unmapped categories.
    /// </remarks>
    public static EquipmentClassMapping Empty { get; } = new EquipmentClassMapping(
        string.Empty,
        "Unknown",
        EquipmentClassAffinity.Universal,
        string.Empty,
        "Unknown",
        Array.Empty<string>());
}
