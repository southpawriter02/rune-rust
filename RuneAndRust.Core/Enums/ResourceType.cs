using RuneAndRust.Core.Attributes;

namespace RuneAndRust.Core.Enums;

/// <summary>
/// Types of resources that can be spent or regenerated during combat.
/// Resources represent the economy of actions - without resources, actions become limited.
/// </summary>
/// <remarks>See: SPEC-RESOURCE-001, Section "Core Behaviors".</remarks>
public enum ResourceType
{
    /// <summary>
    /// Health points. Spending HP is typically involuntary (damage) or high-risk (Overcast).
    /// </summary>
    [GameDocument(
        "Health",
        "The body's capacity to sustain injury before death. Health loss is typically involuntary through damage, though desperate Mystics may sacrifice Health to fuel abilities. Reaching zero means death.")]
    Health = 0,

    /// <summary>
    /// Physical energy. Used for martial abilities and basic attacks.
    /// Regenerates naturally at the start of each turn.
    /// </summary>
    [GameDocument(
        "Stamina",
        "Physical energy fueling martial abilities and basic attacks. Stamina regenerates naturally at the start of each combat round. Managing stamina economy separates skilled warriors from exhausted corpses.")]
    Stamina = 1,

    /// <summary>
    /// Magical energy (Aether Pool). Used for Mystic abilities.
    /// Does not regenerate naturally in combat. Mystics can Overcast (spend HP) when depleted.
    /// </summary>
    [GameDocument(
        "Aether",
        "Mystical energy channeled for supernatural abilities. Unlike stamina, Aether does not regenerate naturally during combat. Depleted Mystics may Overcast, spending Health to fuel their powers at great risk.")]
    Aether = 2

    // Future resource types:
    // Fury = 3,      // Berserker mechanic - builds on damage taken
    // Momentum = 4,  // Skirmisher mechanic - builds on movement/dodges
    // Focus = 5      // Adept mechanic - builds on successful heals/buffs
}
