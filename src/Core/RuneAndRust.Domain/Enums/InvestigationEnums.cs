namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the type of target that can be investigated.
/// Each target type has different clue discovery mechanics and DC modifiers.
/// </summary>
public enum InvestigationTarget
{
    /// <summary>
    /// A crime scene such as a murder site, theft location, or sabotage area.
    /// Clues include: blood patterns, tool marks, footprints, struggle signs.
    /// </summary>
    CrimeScene,

    /// <summary>
    /// Remains of a creature or person, including corpses and skeletons.
    /// Clues include: cause of death, identity markers, last actions, equipment.
    /// </summary>
    Remains,

    /// <summary>
    /// Destroyed or damaged equipment, vehicles, or structures.
    /// Clues include: cause of destruction, salvageable parts, sabotage signs.
    /// </summary>
    Wreckage,

    /// <summary>
    /// Physical trails such as footprints, blood trails, or drag marks.
    /// Clues include: direction, creature type, time elapsed, number of travelers.
    /// </summary>
    Trail,

    /// <summary>
    /// Written documents, logs, or inscriptions.
    /// Clues include: author identity, hidden meanings, referenced locations.
    /// </summary>
    Document
}

/// <summary>
/// Defines categories for organizing and matching clues for deductions.
/// </summary>
public enum ClueCategory
{
    /// <summary>
    /// Clues about who was involved (names, descriptions, affiliations).
    /// </summary>
    Identity,

    /// <summary>
    /// Clues about what caused an event (murder weapon, explosion source).
    /// </summary>
    Cause,

    /// <summary>
    /// Clues about how something was done (method, technique, tools).
    /// </summary>
    Method,

    /// <summary>
    /// Clues about why something occurred (motivation, goals, grudges).
    /// </summary>
    Motive,

    /// <summary>
    /// Clues about where something happened or leads to.
    /// </summary>
    Location,

    /// <summary>
    /// Clues about when something occurred or timeline ordering.
    /// </summary>
    Time,

    /// <summary>
    /// Clues about creatures involved (species, behavior, tracks).
    /// </summary>
    Creature,

    /// <summary>
    /// Clues about objects or artifacts involved.
    /// </summary>
    Object,

    /// <summary>
    /// Clues about historical context or past events.
    /// </summary>
    History
}
