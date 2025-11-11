using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages NPC lifecycle, disposition, and interactions (v0.8, v0.9 - Merchants)
/// </summary>
public class NPCService
{
    private static readonly ILogger _log = Log.ForContext<NPCService>();
    private readonly Dictionary<string, NPC> _npcDatabase = new();
    private readonly string _npcDataPath;

    public NPCService(string dataPath = "Data/NPCs")
    {
        _npcDataPath = dataPath;
    }

    /// <summary>
    /// Loads all NPC definitions from JSON files (v0.9 - supports Merchants)
    /// </summary>
    public void LoadNPCDatabase()
    {
        _log.Debug("Loading NPC database from: {DataPath}", _npcDataPath);

        if (!Directory.Exists(_npcDataPath))
        {
            _log.Warning("NPC data path not found: {DataPath}", _npcDataPath);
            Console.WriteLine($"Warning: NPC data path not found: {_npcDataPath}");
            return;
        }

        var npcFiles = Directory.GetFiles(_npcDataPath, "*.json");
        _log.Debug("Found {FileCount} NPC files to load", npcFiles.Length);

        foreach (var file in npcFiles)
        {
            try
            {
                var json = File.ReadAllText(file);

                // Check if this is a merchant by looking for IsMerchant flag
                var isMerchant = json.Contains("\"IsMerchant\": true");

                NPC? npc;
                if (isMerchant)
                {
                    // Deserialize as Merchant
                    npc = JsonSerializer.Deserialize<Merchant>(json);
                    _log.Debug("Loaded Merchant: {NpcId} ({NpcName}, Type={MerchantType}) from {FileName}",
                        npc?.Id, npc?.Name, (npc as Merchant)?.Type, Path.GetFileName(file));
                }
                else
                {
                    // Deserialize as regular NPC
                    npc = JsonSerializer.Deserialize<NPC>(json);
                    _log.Debug("Loaded NPC: {NpcId} ({NpcName}) from {FileName}",
                        npc?.Id, npc?.Name, Path.GetFileName(file));
                }

                if (npc != null && !string.IsNullOrEmpty(npc.Id))
                {
                    _npcDatabase[npc.Id] = npc;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading NPC from file: {FileName}", Path.GetFileName(file));
                Console.WriteLine($"Error loading NPC from {file}: {ex.Message}");
            }
        }

        _log.Information("Loaded {NpcCount} NPCs from {FileCount} files", _npcDatabase.Count, npcFiles.Length);
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
        int oldDisposition = npc.CurrentDisposition;
        int factionReputation = player.FactionReputations.GetReputation(npc.Faction);
        npc.UpdateDisposition(factionReputation);

        if (oldDisposition != npc.CurrentDisposition)
        {
            _log.Information("NPC disposition updated: {NpcName} ({NpcId}), Old={Old}, New={New}, FactionRep={FactionRep}",
                npc.Name, npc.Id, oldDisposition, npc.CurrentDisposition, factionReputation);
        }
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
    /// Creates a clone of an NPC from the database for placement in a room (v0.9 - supports Merchants)
    /// This allows each room to have its own state for the same NPC
    /// </summary>
    public NPC? CreateNPCInstance(string npcId)
    {
        var template = _npcDatabase.GetValueOrDefault(npcId);
        if (template == null)
        {
            _log.Warning("Failed to create NPC instance: NPC template not found for {NpcId}", npcId);
            return null;
        }

        _log.Debug("Creating NPC instance: {NpcId} ({NpcName})", template.Id, template.Name);

        // Check if this is a Merchant
        if (template is Merchant merchantTemplate)
        {
            // Create a new Merchant instance with the same properties
            return new Merchant
            {
                // Base NPC properties
                Id = merchantTemplate.Id,
                Name = merchantTemplate.Name,
                Description = merchantTemplate.Description,
                InitialGreeting = merchantTemplate.InitialGreeting,
                RoomId = merchantTemplate.RoomId,
                IsHostile = merchantTemplate.IsHostile,
                Faction = merchantTemplate.Faction,
                BaseDisposition = merchantTemplate.BaseDisposition,
                CurrentDisposition = merchantTemplate.CurrentDisposition,
                RootDialogueId = merchantTemplate.RootDialogueId,
                EncounteredTopics = new List<string>(),
                HasBeenMet = false,
                IsAlive = true,
                QuestFlags = new Dictionary<string, bool>(),

                // Merchant-specific properties
                Type = merchantTemplate.Type,
                Inventory = new ShopInventory(), // Fresh inventory (will be initialized by MerchantService)
                RestockIntervalDays = merchantTemplate.RestockIntervalDays,
                LastRestockTime = DateTime.MinValue,
                BaseMarkup = merchantTemplate.BaseMarkup,
                ReputationPriceRange = merchantTemplate.ReputationPriceRange,
                BarterSkillImpact = merchantTemplate.BarterSkillImpact,
                CategoryModifiers = new Dictionary<string, float>(merchantTemplate.CategoryModifiers)
            };
        }

        // Create a regular NPC instance
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

    /// <summary>
    /// Gets a merchant by ID (v0.9)
    /// </summary>
    public Merchant? GetMerchant(string id)
    {
        var npc = _npcDatabase.GetValueOrDefault(id);
        return npc as Merchant;
    }

    /// <summary>
    /// Checks if an NPC is a merchant (v0.9)
    /// </summary>
    public bool IsMerchant(NPC npc)
    {
        return npc is Merchant;
    }

    /// <summary>
    /// Gets all merchants in the database (v0.9)
    /// </summary>
    public List<Merchant> GetAllMerchants()
    {
        return _npcDatabase.Values.OfType<Merchant>().ToList();
    }
}
