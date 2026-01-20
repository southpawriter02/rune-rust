using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Definition of an item that can cleanse effects.
/// </summary>
/// <param name="Id">Unique item identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">Item description.</param>
/// <param name="CleanseType">What type of effects this removes.</param>
/// <param name="SpecificEffect">Specific effect ID if CleanseType is Specific.</param>
public readonly record struct CleanseItem(
    string Id,
    string Name,
    string Description,
    CleanseType CleanseType,
    string? SpecificEffect = null)
{
    /// <summary>Antidote - removes Poisoned.</summary>
    public static CleanseItem Antidote() =>
        new("antidote", "Antidote", "Cures poison afflictions.",
            CleanseType.Specific, "poisoned");

    /// <summary>Bandage - removes Bleeding.</summary>
    public static CleanseItem Bandage() =>
        new("bandage", "Bandage", "Stops bleeding wounds.",
            CleanseType.Specific, "bleeding");

    /// <summary>Panacea - removes all negative effects.</summary>
    public static CleanseItem Panacea() =>
        new("panacea", "Panacea", "A miracle cure for all ailments.",
            CleanseType.AllNegative);

    /// <summary>Smelling Salts - removes mental effects.</summary>
    public static CleanseItem SmellingSalts() =>
        new("smelling_salts", "Smelling Salts", "Clears the mind of magical influences.",
            CleanseType.Magical);

    /// <summary>Fire Resistance Potion - removes Burning.</summary>
    public static CleanseItem FireResistancePotion() =>
        new("fire_resistance_potion", "Fire Resistance Potion", "Extinguishes flames.",
            CleanseType.Specific, "burning");
}
