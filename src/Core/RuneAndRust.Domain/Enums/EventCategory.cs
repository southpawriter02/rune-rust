namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of game events for logging and analytics.
/// </summary>
public enum EventCategory
{
    /// <summary>System and infrastructure events.</summary>
    System,

    /// <summary>Combat-related events.</summary>
    Combat,

    /// <summary>Exploration and movement events.</summary>
    Exploration,

    /// <summary>Interactive object events.</summary>
    Interaction,

    /// <summary>Quest state and progress events.</summary>
    Quest,

    /// <summary>Inventory and item events.</summary>
    Inventory,

    /// <summary>Character state events.</summary>
    Character,

    /// <summary>Status effect events.</summary>
    StatusEffect,

    /// <summary>Ability usage events.</summary>
    Ability,

    /// <summary>AI decision and behavior events.</summary>
    AI,

    /// <summary>Dice and random generation events.</summary>
    Dice,

    /// <summary>Environmental and biome events.</summary>
    Environment,

    /// <summary>Session and persistence events.</summary>
    Session
}
