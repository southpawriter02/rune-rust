using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Manages NPC instances: creation from definitions, registration, and lookup.
/// Scoped service — one instance per game session.
/// </summary>
public class NpcService : INpcService
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly INpcDefinitionProvider _npcProvider;
    private readonly ILogger<NpcService> _logger;

    /// <summary>Registered NPC instances keyed by runtime GUID.</summary>
    private readonly Dictionary<Guid, Npc> _npcsById = [];

    /// <summary>Index from definition ID to runtime NPC for quick lookup.</summary>
    private readonly Dictionary<string, Npc> _npcsByDefinitionId =
        new(StringComparer.OrdinalIgnoreCase);

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public NpcService(INpcDefinitionProvider npcProvider, ILogger<NpcService> logger)
    {
        _npcProvider = npcProvider ?? throw new ArgumentNullException(nameof(npcProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("NpcService initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Npc? CreateNpcFromDefinition(string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId);

        var definition = _npcProvider.GetDefinition(npcId);
        if (definition == null)
        {
            _logger.LogWarning("NPC definition not found: {NpcId}", npcId);
            return null;
        }

        // Parse archetype
        if (!Enum.TryParse<NpcArchetype>(definition.Archetype, ignoreCase: true, out var archetype))
        {
            _logger.LogWarning(
                "Unknown NPC archetype '{Archetype}' for NPC {NpcId}, defaulting to Citizen",
                definition.Archetype, npcId);
            archetype = NpcArchetype.Citizen;
        }

        // Create NPC using factory method
        var npc = Npc.Create(
            definitionId: definition.NpcId,
            name: definition.Name,
            description: definition.Description,
            initialGreeting: definition.InitialGreeting,
            archetype: archetype,
            baseDisposition: definition.BaseDisposition,
            faction: definition.Faction,
            rootDialogueId: definition.RootDialogueId,
            isMerchant: definition.IsMerchant,
            questIds: definition.QuestIds);

        // Auto-register
        RegisterNpc(npc);

        _logger.LogInformation(
            "Created NPC from definition: {NpcName} ({NpcId}) - {Archetype}",
            npc.Name, npc.DefinitionId, npc.Archetype);

        return npc;
    }

    /// <inheritdoc />
    public Npc? GetNpcByDefinitionId(string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId);

        _npcsByDefinitionId.TryGetValue(npcId, out var npc);
        return npc;
    }

    /// <inheritdoc />
    public IReadOnlyList<Npc> GetAllNpcs()
    {
        return _npcsById.Values.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<Npc> GetNpcsByFaction(string faction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(faction);

        return _npcsById.Values
            .Where(n => n.Faction != null &&
                        n.Faction.Equals(faction, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <inheritdoc />
    public void RegisterNpc(Npc npc)
    {
        ArgumentNullException.ThrowIfNull(npc);

        _npcsById[npc.Id] = npc;
        _npcsByDefinitionId[npc.DefinitionId] = npc;

        _logger.LogDebug("Registered NPC: {NpcName} ({NpcId})", npc.Name, npc.DefinitionId);
    }

    /// <inheritdoc />
    public void UnregisterNpc(Guid npcId)
    {
        if (_npcsById.TryGetValue(npcId, out var npc))
        {
            _npcsById.Remove(npcId);
            _npcsByDefinitionId.Remove(npc.DefinitionId);

            _logger.LogDebug("Unregistered NPC: {NpcName} ({NpcId})", npc.Name, npc.DefinitionId);
        }
    }
}
