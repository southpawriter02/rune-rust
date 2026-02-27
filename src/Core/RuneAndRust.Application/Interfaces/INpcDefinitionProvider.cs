using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Loads NPC definitions from JSON configuration (config/npcs.json).
/// Registered as singleton in DI — definitions are immutable after initial load.
/// </summary>
public interface INpcDefinitionProvider
{
    /// <summary>Returns all loaded NPC definitions.</summary>
    IReadOnlyList<NpcDefinitionDto> GetAllDefinitions();

    /// <summary>Returns a specific NPC definition by ID, or null if not found.</summary>
    NpcDefinitionDto? GetDefinition(string npcId);

    /// <summary>Returns all NPCs belonging to a specific faction.</summary>
    IReadOnlyList<NpcDefinitionDto> GetNpcsByFaction(string faction);

    /// <summary>Returns all NPCs matching any of the given tags.</summary>
    IReadOnlyList<NpcDefinitionDto> GetNpcsByTag(string tag);

    /// <summary>Returns true if an NPC definition exists for the given ID.</summary>
    bool NpcExists(string npcId);

    /// <summary>Reloads definitions from disk.</summary>
    void Reload();
}
