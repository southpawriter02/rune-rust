using RuneAndRust.Core;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages NPC lifecycle, disposition, and interactions (v0.8)
/// </summary>
public class NPCService
{
    private readonly Dictionary<string, NPC> _npcDatabase = new();
    private readonly string _npcDataPath;

    public NPCService(string dataPath = "Data/NPCs")
    {
        _npcDataPath = dataPath;
    }

    /// <summary>
    /// Loads all NPC definitions from JSON files
    /// </summary>
    public void LoadNPCDatabase()
    {
        if (!Directory.Exists(_npcDataPath))
        {
            Console.WriteLine($"Warning: NPC data path not found: {_npcDataPath}");
            return;
        }

        var npcFiles = Directory.GetFiles(_npcDataPath, "*.json");
        foreach (var file in npcFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var npc = JsonSerializer.Deserialize<NPC>(json);
                if (npc != null && !string.IsNullOrEmpty(npc.Id))
                {
                    _npcDatabase[npc.Id] = npc;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading NPC from {file}: {ex.Message}");
            }
        }

        Console.WriteLine($"Loaded {_npcDatabase.Count} NPCs");
    }

    /// <summary>
    /// Gets an NPC by ID
    /// </summary>
    public NPC? GetNPC(string id)
    {
        return _npcDatabase.GetValueOrDefault(id);
    }

    /// <summary>
    /// Gets all NPCs in a specific room
    /// </summary>
    public List<NPC> GetNPCsInRoom(Room room)
    {
        return room.NPCs;
    }

    /// <summary>
    /// Updates NPC disposition based on faction reputation
    /// </summary>
    public void UpdateDisposition(NPC npc, PlayerCharacter player)
    {
        int factionReputation = player.FactionReputations.GetReputation(npc.Faction);
        npc.UpdateDisposition(factionReputation);
    }

    /// <summary>
    /// Updates all NPCs in a room based on player's faction reputation
    /// </summary>
    public void UpdateRoomNPCDispositions(Room room, PlayerCharacter player)
    {
        foreach (var npc in room.NPCs)
        {
            UpdateDisposition(npc, player);
        }
    }

    /// <summary>
    /// Checks if an NPC is hostile towards the player
    /// </summary>
    public bool IsHostile(NPC npc)
    {
        // Check base hostility flag
        if (npc.IsHostile)
            return true;

        // Check if disposition is very negative (Hostile tier: -50 to -100)
        if (npc.CurrentDisposition <= -50)
            return true;

        return false;
    }

    /// <summary>
    /// Finds an NPC in a room by name (case-insensitive partial match)
    /// </summary>
    public NPC? FindNPCByName(Room room, string name)
    {
        name = name.ToLower().Trim();

        // First try exact match
        var exact = room.NPCs.FirstOrDefault(n =>
            n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        // Then try partial match
        return room.NPCs.FirstOrDefault(n =>
            n.Name.ToLower().Contains(name));
    }

    /// <summary>
    /// Marks an NPC as met
    /// </summary>
    public void MarkAsMet(NPC npc)
    {
        npc.HasBeenMet = true;
    }

    /// <summary>
    /// Adds a topic to NPC's encountered topics
    /// </summary>
    public void RecordTopic(NPC npc, string topic)
    {
        if (!npc.EncounteredTopics.Contains(topic))
        {
            npc.EncounteredTopics.Add(topic);
        }
    }

    /// <summary>
    /// Creates a clone of an NPC from the database for placement in a room
    /// This allows each room to have its own state for the same NPC
    /// </summary>
    public NPC? CreateNPCInstance(string npcId)
    {
        var template = _npcDatabase.GetValueOrDefault(npcId);
        if (template == null) return null;

        // Create a new instance with the same base properties
        return new NPC
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            InitialGreeting = template.InitialGreeting,
            RoomId = template.RoomId,
            IsHostile = template.IsHostile,
            Faction = template.Faction,
            BaseDisposition = template.BaseDisposition,
            CurrentDisposition = template.CurrentDisposition,
            RootDialogueId = template.RootDialogueId,
            EncounteredTopics = new List<string>(),
            HasBeenMet = false,
            IsAlive = true,
            QuestFlags = new Dictionary<string, bool>()
        };
    }
}
