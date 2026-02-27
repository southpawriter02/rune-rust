namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the archetype/role categories for general-purpose NPCs.
/// </summary>
/// <remarks>
/// <para>
/// Each archetype implies a broad behavioral and narrative role for the NPC
/// within the Aethelgard setting. Archetypes influence default disposition,
/// available dialogue options, and interaction capabilities.
/// </para>
/// <para>
/// This enum is distinct from player character archetypes (Warrior, Adept, Skirmisher).
/// NPC archetypes describe their societal role, not combat capability.
/// </para>
/// </remarks>
public enum NpcArchetype
{
    /// <summary>General inhabitants of Holds and settlements (e.g., Astrid the Jötun-Reader).</summary>
    Citizen = 0,

    /// <summary>Shop NPCs who buy and sell goods (e.g., Kjartan the Merchant).</summary>
    Merchant = 1,

    /// <summary>Protectors and enforcers, often quest givers for combat tasks (e.g., Thorvald).</summary>
    Guard = 2,

    /// <summary>Lore-keepers and researchers, sources of information and knowledge quests (e.g., Jötun-Reader affiliates).</summary>
    Scholar = 3,

    /// <summary>Crafters and makers, can provide crafting services or material quests (e.g., Ragnhild the Smith).</summary>
    Artisan = 4,

    /// <summary>Outsiders and outcasts, often have unique perspectives and hidden quests (e.g., Bjorn the Exile).</summary>
    Exile = 5,

    /// <summary>Isolated NPCs found in remote locations, typically with specialized knowledge (e.g., Rolf the Hermit).</summary>
    Hermit = 6,

    /// <summary>Resource-focused NPCs who trade in salvage and raw materials (e.g., Sigrun, Ulf).</summary>
    Scavenger = 7,

    /// <summary>God-Sleeper faction members with dangerous knowledge and morally ambiguous quests.</summary>
    Cultist = 8
}
