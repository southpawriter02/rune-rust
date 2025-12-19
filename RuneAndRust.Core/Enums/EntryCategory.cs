namespace RuneAndRust.Core.Enums;

/// <summary>
/// Categories for organizing Codex entries in the Scavenger's Journal.
/// Determines which tab displays the entry in the Journal UI.
/// </summary>
/// <remarks>
/// The Codex is divided into themed sections for easier navigation.
/// Field Guide entries are tutorial/mechanics explanations unlocked
/// through gameplay triggers. Other categories contain world lore.
/// </remarks>
public enum EntryCategory
{
    /// <summary>
    /// Game mechanics and tutorial explanations.
    /// Unlocked via gameplay triggers when mechanics are first encountered.
    /// </summary>
    FieldGuide = 0,

    /// <summary>
    /// Lore about The Glitch, the Ginnungagap event, and world corruption.
    /// Core mystery content explaining the cataclysm that ruined the world.
    /// </summary>
    BlightOrigin = 1,

    /// <summary>
    /// Creature data, behavioral patterns, and weaknesses.
    /// Bestiary entries compiled from Specimen and observation captures.
    /// </summary>
    Bestiary = 2,

    /// <summary>
    /// Political groups, social structures, and faction information.
    /// Details about surviving civilizations and their relationships.
    /// </summary>
    Factions = 3,

    /// <summary>
    /// Pre-Glitch technology, Aesir artifacts, and engineering records.
    /// Technical specifications and operational data for ancient devices.
    /// </summary>
    Technical = 4,

    /// <summary>
    /// Location descriptions, regional history, and geographic data.
    /// Maps, travel routes, and environmental hazard information.
    /// </summary>
    Geography = 5
}
