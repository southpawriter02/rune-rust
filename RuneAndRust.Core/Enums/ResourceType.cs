namespace RuneAndRust.Core.Enums;

/// <summary>
/// Types of resources that can be spent or regenerated during combat.
/// Resources represent the economy of actions - without resources, actions become limited.
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// Health points. Spending HP is typically involuntary (damage) or high-risk (Overcast).
    /// </summary>
    Health = 0,

    /// <summary>
    /// Physical energy. Used for martial abilities and basic attacks.
    /// Regenerates naturally at the start of each turn.
    /// </summary>
    Stamina = 1,

    /// <summary>
    /// Magical energy (Aether Pool). Used for Mystic abilities.
    /// Does not regenerate naturally in combat. Mystics can Overcast (spend HP) when depleted.
    /// </summary>
    Aether = 2

    // Future resource types:
    // Fury = 3,      // Berserker mechanic - builds on damage taken
    // Momentum = 4,  // Skirmisher mechanic - builds on movement/dodges
    // Focus = 5      // Adept mechanic - builds on successful heals/buffs
}
