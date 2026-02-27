using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// General-purpose NPC entity for quest givers, merchants, lore sources, and dialogue NPCs.
/// </summary>
/// <remarks>
/// <para>
/// This entity complements <see cref="RiddleNpc"/>, which handles puzzle/gate NPCs specifically.
/// The Npc entity covers the broader narrative needs: quest distribution, faction representation,
/// dialogue interaction, and merchant functionality.
/// </para>
/// <para>
/// NPCs track relationship state through a disposition system (-100 to +100) and maintain
/// a list of quest IDs they can offer. The <see cref="HasBeenMet"/> flag allows first-meeting
/// dialogue to differ from subsequent interactions.
/// </para>
/// <para>
/// Created from <c>config/npcs.json</c> definitions via <c>INpcDefinitionProvider</c>.
/// Placed into <see cref="Room"/> entities during dungeon generation or quest anchor seeding.
/// </para>
/// </remarks>
public class Npc : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // BACKING FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Backing list for available quest IDs this NPC can offer.</summary>
    private readonly List<string> _availableQuestIds = [];

    // ═══════════════════════════════════════════════════════════════
    // IDENTITY PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Gets the unique runtime identifier for this NPC instance.</summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the definition ID from config/npcs.json (e.g., "thorvald_guard").
    /// Used for cross-referencing with quest giver/turn-in IDs.
    /// </summary>
    public string DefinitionId { get; private set; }

    /// <summary>Gets the display name (e.g., "Thorvald the Guard").</summary>
    public string Name { get; private set; }

    /// <summary>Gets the narrative description of this NPC.</summary>
    public string Description { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // DIALOGUE PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the initial greeting text shown when first interacting with this NPC.
    /// Changes after the NPC has been met if a post-meeting greeting exists.
    /// </summary>
    public string InitialGreeting { get; private set; }

    /// <summary>
    /// Gets the root dialogue tree ID that serves as the entry point for conversation.
    /// References a key in config/dialogues.json. Null if NPC has no branching dialogue.
    /// </summary>
    public string? RootDialogueId { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // ROLE & FACTION PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Gets the NPC's archetype, defining their societal role.</summary>
    public NpcArchetype Archetype { get; private set; }

    /// <summary>
    /// Gets the faction this NPC belongs to (e.g., "IronBanes", "MidgardCombine").
    /// Null if the NPC is unaffiliated.
    /// </summary>
    public string? Faction { get; private set; }

    /// <summary>Gets whether this NPC can buy/sell items.</summary>
    public bool IsMerchant { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // RELATIONSHIP PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the starting attitude value from config (-100 to 100).
    /// Negative = hostile, 0 = neutral, positive = friendly.
    /// </summary>
    public int BaseDisposition { get; private set; }

    /// <summary>
    /// Gets the current attitude toward the player, modified by interactions.
    /// Clamped to -100..100.
    /// </summary>
    public int CurrentDisposition { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // STATE PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Gets whether the player has previously interacted with this NPC.</summary>
    public bool HasBeenMet { get; private set; }

    /// <summary>Gets whether this NPC is still alive. Dead NPCs cannot be interacted with.</summary>
    public bool IsAlive { get; private set; } = true;

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Gets whether this NPC has a dialogue tree assigned.</summary>
    public bool HasDialogue => RootDialogueId != null;

    /// <summary>Gets whether this NPC currently has quests available.</summary>
    public bool CanGiveQuests => _availableQuestIds.Count > 0;

    /// <summary>Gets the current greeting, accounting for met status.</summary>
    public string CurrentGreeting => HasBeenMet
        ? $"{Name} nods in recognition."
        : InitialGreeting;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Private constructor for EF Core materialization.</summary>
    private Npc()
    {
        DefinitionId = null!;
        Name = null!;
        Description = null!;
        InitialGreeting = null!;
    }

    /// <summary>
    /// Creates a new NPC instance from definition data.
    /// </summary>
    /// <param name="definitionId">The config definition ID (e.g., "thorvald_guard").</param>
    /// <param name="name">Display name (e.g., "Thorvald the Guard").</param>
    /// <param name="description">Narrative description.</param>
    /// <param name="initialGreeting">First-meeting greeting text.</param>
    /// <param name="archetype">The NPC's societal role.</param>
    /// <param name="baseDisposition">Starting attitude (-100 to 100).</param>
    public Npc(
        string definitionId,
        string name,
        string description,
        string initialGreeting,
        NpcArchetype archetype,
        int baseDisposition = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(initialGreeting);

        Id = Guid.NewGuid();
        DefinitionId = definitionId.ToLowerInvariant();
        Name = name;
        Description = description;
        InitialGreeting = initialGreeting;
        Archetype = archetype;
        BaseDisposition = Math.Clamp(baseDisposition, -100, 100);
        CurrentDisposition = BaseDisposition;
    }

    // ═══════════════════════════════════════════════════════════════
    // CONFIGURATION METHODS (called during setup from provider data)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Sets the root dialogue tree ID for this NPC.</summary>
    /// <param name="dialogueId">The dialogue tree root ID from config/dialogues.json.</param>
    public void SetRootDialogue(string dialogueId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dialogueId);
        RootDialogueId = dialogueId;
    }

    /// <summary>Sets the faction affiliation for this NPC.</summary>
    /// <param name="faction">The faction identifier (e.g., "IronBanes").</param>
    public void SetFaction(string faction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(faction);
        Faction = faction;
    }

    /// <summary>Marks this NPC as a merchant capable of buy/sell transactions.</summary>
    /// <param name="isMerchant">True if the NPC should act as a merchant.</param>
    public void SetMerchant(bool isMerchant)
    {
        IsMerchant = isMerchant;
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERACTION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Marks this NPC as having been met by the player.
    /// Subsequent interactions will use the post-meeting greeting.
    /// </summary>
    public void Meet()
    {
        HasBeenMet = true;
    }

    /// <summary>
    /// Adjusts the NPC's disposition toward the player.
    /// </summary>
    /// <param name="amount">Amount to adjust (positive = friendlier, negative = hostile).</param>
    public void AdjustDisposition(int amount)
    {
        CurrentDisposition = Math.Clamp(CurrentDisposition + amount, -100, 100);
    }

    /// <summary>
    /// Marks this NPC as dead. Dead NPCs cannot be interacted with.
    /// Active quests referencing this NPC may trigger failure conditions.
    /// </summary>
    public void Kill()
    {
        IsAlive = false;
    }

    // ═══════════════════════════════════════════════════════════════
    // QUEST MANAGEMENT METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds a quest ID to this NPC's available quest list.
    /// Duplicate IDs are silently ignored.
    /// </summary>
    /// <param name="questId">The quest definition ID to add.</param>
    public void AddAvailableQuest(string questId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questId);
        var normalized = questId.ToLowerInvariant();

        if (!_availableQuestIds.Contains(normalized))
        {
            _availableQuestIds.Add(normalized);
        }
    }

    /// <summary>
    /// Removes a quest ID from this NPC's available quest list.
    /// Called when a quest is accepted or no longer available.
    /// </summary>
    /// <param name="questId">The quest definition ID to remove.</param>
    /// <returns>True if the quest was found and removed.</returns>
    public bool RemoveAvailableQuest(string questId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questId);
        return _availableQuestIds.Remove(questId.ToLowerInvariant());
    }

    /// <summary>
    /// Returns all quest IDs this NPC can currently offer.
    /// </summary>
    /// <returns>Read-only list of quest definition IDs.</returns>
    public IReadOnlyList<string> GetAvailableQuestIds()
    {
        return _availableQuestIds.AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHOD
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a fully configured NPC from definition data.
    /// </summary>
    /// <param name="definitionId">Config definition ID.</param>
    /// <param name="name">Display name.</param>
    /// <param name="description">Narrative description.</param>
    /// <param name="initialGreeting">First-meeting greeting.</param>
    /// <param name="archetype">Societal role.</param>
    /// <param name="baseDisposition">Starting attitude.</param>
    /// <param name="faction">Optional faction affiliation.</param>
    /// <param name="rootDialogueId">Optional dialogue tree root ID.</param>
    /// <param name="isMerchant">Whether the NPC is a merchant.</param>
    /// <param name="questIds">Optional list of quest IDs this NPC offers.</param>
    /// <returns>A fully configured Npc instance.</returns>
    public static Npc Create(
        string definitionId,
        string name,
        string description,
        string initialGreeting,
        NpcArchetype archetype,
        int baseDisposition = 0,
        string? faction = null,
        string? rootDialogueId = null,
        bool isMerchant = false,
        IEnumerable<string>? questIds = null)
    {
        var npc = new Npc(definitionId, name, description, initialGreeting, archetype, baseDisposition);

        if (!string.IsNullOrWhiteSpace(faction))
            npc.SetFaction(faction);

        if (!string.IsNullOrWhiteSpace(rootDialogueId))
            npc.SetRootDialogue(rootDialogueId);

        npc.SetMerchant(isMerchant);

        if (questIds != null)
        {
            foreach (var questId in questIds)
            {
                npc.AddAvailableQuest(questId);
            }
        }

        return npc;
    }
}
