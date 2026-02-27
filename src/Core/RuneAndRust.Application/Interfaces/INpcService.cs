using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Manages NPC instances: creation from definitions, lookup, and interaction state.
/// Registered as scoped in DI — one instance per game session.
/// </summary>
public interface INpcService
{
    /// <summary>
    /// Creates an NPC instance from a definition ID.
    /// Returns null if the definition doesn't exist.
    /// </summary>
    Npc? CreateNpcFromDefinition(string npcId);

    /// <summary>Returns a registered NPC by definition ID, or null.</summary>
    Npc? GetNpcByDefinitionId(string npcId);

    /// <summary>Returns all registered NPC instances.</summary>
    IReadOnlyList<Npc> GetAllNpcs();

    /// <summary>Returns all NPCs belonging to a faction.</summary>
    IReadOnlyList<Npc> GetNpcsByFaction(string faction);

    /// <summary>
    /// Registers an NPC instance for tracking. Called when an NPC is placed in a room.
    /// </summary>
    void RegisterNpc(Npc npc);

    /// <summary>
    /// Unregisters an NPC instance. Called when an NPC is removed from the world.
    /// </summary>
    void UnregisterNpc(Guid npcId);
}
