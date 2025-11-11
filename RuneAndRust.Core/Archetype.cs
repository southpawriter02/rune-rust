namespace RuneAndRust.Core;

/// <summary>
/// Base abstraction for all character archetypes (Warrior, Adept, Mystic, Ranger).
/// Defines the formal archetype system introduced in v0.7+.
/// </summary>
public abstract class Archetype
{
    /// <summary>
    /// The type of archetype (Warrior, Adept, Mystic, Ranger)
    /// </summary>
    public abstract CharacterClass ArchetypeType { get; }

    /// <summary>
    /// The primary resource used by this archetype (Stamina or Aether)
    /// </summary>
    public abstract ResourceType PrimaryResource { get; }

    /// <summary>
    /// Get the 3 starting abilities automatically granted to this archetype
    /// </summary>
    public abstract List<Ability> GetStartingAbilities();

    /// <summary>
    /// Get the weapon proficiencies for this archetype
    /// </summary>
    public abstract List<string> GetWeaponProficiencies();

    /// <summary>
    /// Get the armor proficiencies for this archetype
    /// </summary>
    public abstract List<string> GetArmorProficiencies();

    /// <summary>
    /// Get the base statistics for this archetype
    /// </summary>
    public abstract Attributes GetBaseAttributes();
}

/// <summary>
/// Resource types for archetypes
/// </summary>
public enum ResourceType
{
    Stamina,  // Physical resource for Warriors, Adepts
    Aether    // Magical resource for Mystics
}
