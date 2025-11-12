using RuneAndRust.Core;
using RuneAndRust.Core.Quests;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages quest lifecycle and tracking (v0.8, v0.9)
/// </summary>
public class QuestService
{
    private static readonly ILogger _log = Log.ForContext<QuestService>();
    private readonly Dictionary<string, Quest> _questDatabase = new();
    private readonly string _questDataPath;
    private readonly CurrencyService? _currencyService; // v0.9 - optional for backward compatibility

    public QuestService(string dataPath = "Data/Quests", CurrencyService? currencyService = null)
    {
        _questDataPath = dataPath;
        _currencyService = currencyService; // v0.9
    }

    /// <summary>
    /// Loads all quest definitions from JSON files
    /// </summary>
    public void LoadQuestDatabase()
    {
        _log.Debug("Loading quest database from: {DataPath}", _questDataPath);

        if (!Directory.Exists(_questDataPath))
        {
            _log.Warning("Quest data path not found: {DataPath}", _questDataPath);
            Console.WriteLine($"Warning: Quest data path not found: {_questDataPath}");
            return;
        }

        var questFiles = Directory.GetFiles(_questDataPath, "*.json");
        _log.Debug("Found {FileCount} quest files to load", questFiles.Length);

        foreach (var file in questFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var quest = JsonSerializer.Deserialize<Quest>(json);
                if (quest != null && !string.IsNullOrEmpty(quest.Id))
                {
                    _questDatabase[quest.Id] = quest;
                    _log.Debug("Loaded quest: {QuestId} ({QuestTitle}) from {FileName}",
                        quest.Id, quest.Title, Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading quest from file: {FileName}", Path.GetFileName(file));
                Console.WriteLine($"Error loading quest from {file}: {ex.Message}");
            }
        }

        _log.Information("Loaded {QuestCount} quests from {FileCount} files", _questDatabase.Count, questFiles.Length);
        Console.WriteLine($"Loaded {_questDatabase.Count} quests");
    }

    /// <summary>
    /// v0.14: Offers a quest to the player (makes it available)
    /// </summary>
    public bool OfferQuest(string questId, PlayerCharacter player)
    {
        var questTemplate = _questDatabase.GetValueOrDefault(questId);
        if (questTemplate == null)
        {
            _log.Warning("Quest not found in database: {QuestId}", questId);
            return false;
        }

        // Check eligibility
        if (!CanOfferQuest(player, questTemplate))
        {
            _log.Debug("Player not eligible for quest: {QuestId}", questId);
            return false;
        }

        // Check if already in any quest list
        if (player.ActiveQuests.Any(q => q.Id == questId) ||
            player.CompletedQuests.Any(q => q.Id == questId))
        {
            _log.Debug("Quest already in player quest lists: {QuestId}", questId);
            return false;
        }

        // Create instance and mark as Available
        var quest = CreateQuestInstance(questTemplate);
        quest.Status = QuestStatus.Available;
        player.ActiveQuests.Add(quest); // Store in ActiveQuests list even when Available

        _log.Information("Quest offered: QuestId={QuestId}, Player={PlayerName}",
            questId, player.Name);

        return true;
    }

    /// <summary>
    /// v0.14: Checks if a quest can be offered to the player
    /// </summary>
    public bool CanOfferQuest(PlayerCharacter player, Quest quest)
    {
        // Check minimum Legend requirement
        if (player.CurrentLegend < quest.MinimumLegend)
        {
            _log.Debug("Player Legend too low: Current={Current}, Required={Required}",
                player.CurrentLegend, quest.MinimumLegend);
            return false;
        }

        // Check prerequisite quests
        foreach (var prereqId in quest.PrerequisiteQuests)
        {
            if (!player.CompletedQuests.Any(q => q.Id == prereqId))
            {
                _log.Debug("Missing prerequisite quest: {PrereqId}", prereqId);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Accepts a quest and adds it to the player's active quests
    /// </summary>
    public bool AcceptQuest(string questId, PlayerCharacter player)
    {
        // Find quest in player's available quests
        var quest = player.ActiveQuests.FirstOrDefault(q => q.Id == questId);

        if (quest == null)
        {
            // Try to offer it first
            if (!OfferQuest(questId, player))
            {
                _log.Warning("Cannot accept quest - not available: {QuestId}", questId);
                return false;
            }
            quest = player.ActiveQuests.First(q => q.Id == questId);
        }

        // Check if already active or completed
        if (quest.Status == QuestStatus.Active || quest.Status == QuestStatus.Completed || quest.Status == QuestStatus.TurnedIn)
        {
            _log.Debug("Quest already accepted or completed: {QuestId}", questId);
            return false;
        }

        // Activate the quest
        quest.Status = QuestStatus.Active;
        quest.AcceptedAt = DateTime.UtcNow;

        _log.Information("Quest accepted: {QuestId} ({QuestTitle}), Giver={GiverId}",
            questId, quest.Title, quest.GiverNpcId);

        return true;
    }

    /// <summary>
    /// Updates quest objectives based on game events
    /// </summary>
    public List<string> OnItemCollected(string itemId, PlayerCharacter player)
    {
        var messages = new List<string>();

        foreach (var quest in player.ActiveQuests)
        {
            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.CollectItem &&
                    objective.TargetId == itemId &&
                    !objective.IsComplete)
                {
                    objective.Current++;
                    messages.Add($"[Quest] {quest.Title}: {objective.Description} ({objective.GetProgress()})");

                    _log.Information("Quest objective updated: Quest={QuestId}, Objective={Objective}, Progress={Progress}",
                        quest.Id, objective.Description, objective.GetProgress());

                    if (objective.IsComplete)
                    {
                        messages.Add($"[Quest] Objective complete: {objective.Description}");
                        _log.Information("Quest objective completed: Quest={QuestId}, Objective={Objective}",
                            quest.Id, objective.Description);
                    }
                }
            }

            // Check if quest is complete
            if (quest.IsComplete() && quest.Status == QuestStatus.Active)
            {
                messages.Add($"[Quest] {quest.Title} objectives complete! Return to {quest.GiverNpcId} for reward.");
                _log.Information("All quest objectives completed: Quest={QuestId} ({QuestTitle})",
                    quest.Id, quest.Title);
            }
        }

        return messages;
    }

    /// <summary>
    /// Updates quest objectives when an enemy is killed
    /// </summary>
    public List<string> OnEnemyKilled(string enemyId, PlayerCharacter player)
    {
        var messages = new List<string>();

        foreach (var quest in player.ActiveQuests)
        {
            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.KillEnemy &&
                    objective.TargetId == enemyId &&
                    !objective.IsComplete)
                {
                    objective.Current++;
                    messages.Add($"[Quest] {quest.Title}: {objective.Description} ({objective.GetProgress()})");

                    if (objective.IsComplete)
                    {
                        messages.Add($"[Quest] Objective complete: {objective.Description}");
                    }
                }
            }

            // Check if quest is complete
            if (quest.IsComplete() && quest.Status == QuestStatus.Active)
            {
                messages.Add($"[Quest] {quest.Title} objectives complete! Return to quest giver for reward.");
            }
        }

        return messages;
    }

    /// <summary>
    /// Updates quest objectives when talking to an NPC
    /// </summary>
    public List<string> OnNPCTalk(string npcId, PlayerCharacter player)
    {
        var messages = new List<string>();

        foreach (var quest in player.ActiveQuests)
        {
            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.TalkToNPC &&
                    objective.TargetId == npcId &&
                    !objective.IsComplete)
                {
                    objective.Current++;
                    messages.Add($"[Quest] {quest.Title}: {objective.Description} ({objective.GetProgress()})");

                    if (objective.IsComplete)
                    {
                        messages.Add($"[Quest] Objective complete: {objective.Description}");
                    }
                }
            }

            // Check if quest is complete
            if (quest.IsComplete() && quest.Status == QuestStatus.Active)
            {
                messages.Add($"[Quest] {quest.Title} objectives complete! Return to quest giver for reward.");
            }
        }

        return messages;
    }

    /// <summary>
    /// Updates quest objectives when entering a room
    /// </summary>
    public List<string> OnRoomEntered(int roomId, PlayerCharacter player)
    {
        var messages = new List<string>();

        foreach (var quest in player.ActiveQuests)
        {
            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.ExploreRoom &&
                    objective.TargetId == roomId.ToString() &&
                    !objective.IsComplete)
                {
                    objective.Current++;
                    messages.Add($"[Quest] {quest.Title}: {objective.Description} ({objective.GetProgress()})");

                    if (objective.IsComplete)
                    {
                        messages.Add($"[Quest] Objective complete: {objective.Description}");
                    }
                }
            }

            // Check if quest is complete
            if (quest.IsComplete() && quest.Status == QuestStatus.Active)
            {
                messages.Add($"[Quest] {quest.Title} objectives complete! Return to quest giver for reward.");
            }
        }

        return messages;
    }

    /// <summary>
    /// v0.14: Updates quest progress and checks for completion
    /// </summary>
    public List<string> UpdateQuestProgress(PlayerCharacter player)
    {
        var messages = new List<string>();

        foreach (var quest in player.ActiveQuests.Where(q => q.Status == QuestStatus.Active))
        {
            if (quest.IsComplete() && quest.Status == QuestStatus.Active)
            {
                quest.Status = QuestStatus.Complete;
                messages.Add($"[Quest Complete] {quest.Title} - Return to {quest.GiverNpcName} for reward!");
                _log.Information("Quest objectives completed: QuestId={QuestId}", quest.Id);
            }
        }

        return messages;
    }

    /// <summary>
    /// v0.14: Turns in a completed quest and grants rewards
    /// </summary>
    public List<string> TurnInQuest(string questId, PlayerCharacter player)
    {
        var messages = new List<string>();

        var quest = player.ActiveQuests.FirstOrDefault(q => q.Id == questId);
        if (quest == null || quest.Status != QuestStatus.Complete)
        {
            _log.Debug("Cannot turn in quest: Quest={QuestId}, Found={Found}, Status={Status}",
                questId, quest != null, quest?.Status);
            return messages;
        }

        _log.Information("Turning in quest: {QuestId} ({QuestTitle})", quest.Id, quest.Title);

        // Grant rewards
        if (quest.Reward != null)
        {
            messages.AddRange(GrantReward(quest.Reward, player));
        }

        // Move quest from active to completed
        quest.Status = QuestStatus.TurnedIn;
        quest.CompletedAt = DateTime.UtcNow;
        player.ActiveQuests.Remove(quest);
        player.CompletedQuests.Add(quest);

        messages.Add($"[Quest Turned In] {quest.Title}");

        _log.Information("Quest turned in successfully: {QuestId} ({QuestTitle})", quest.Id, quest.Title);

        return messages;
    }

    /// <summary>
    /// Legacy method: Completes a quest and grants rewards (calls TurnInQuest)
    /// </summary>
    public List<string> CompleteQuest(string questId, PlayerCharacter player)
    {
        // For backward compatibility, auto-complete objectives if possible
        var quest = player.ActiveQuests.FirstOrDefault(q => q.Id == questId);
        if (quest != null && quest.IsComplete() && quest.Status == QuestStatus.Active)
        {
            quest.Status = QuestStatus.Complete;
        }

        return TurnInQuest(questId, player);
    }

    /// <summary>
    /// Grants quest rewards to the player
    /// </summary>
    private List<string> GrantReward(QuestReward reward, PlayerCharacter player)
    {
        var messages = new List<string>();

        // Grant experience
        if (reward.Experience > 0)
        {
            // v0.8: Experience goes to Legend (Aethelgard Saga System)
            player.CurrentLegend += reward.Experience;
            messages.Add($"[Reward] +{reward.Experience} Legend");
            _log.Information("Quest reward granted: Experience={Experience}", reward.Experience);
        }

        // Grant items (v0.8: simplified - just log, actual item granting would need EquipmentService integration)
        foreach (var itemId in reward.ItemIds)
        {
            messages.Add($"[Reward] Received: {itemId}");
            _log.Information("Quest reward granted: Item={ItemId}", itemId);
        }

        // Grant reputation
        if (reward.ReputationChange != 0 && reward.Faction.HasValue)
        {
            var log = new List<string>();
            player.FactionReputations.ModifyReputation(
                reward.Faction.Value,
                reward.ReputationChange,
                "Quest completion",
                log);
            messages.AddRange(log);
            _log.Information("Quest reward granted: Faction={Faction}, ReputationChange={Change}",
                reward.Faction.Value, reward.ReputationChange);
        }

        // Grant currency (v0.9)
        if (reward.Currency > 0 && _currencyService != null)
        {
            _currencyService.AddCurrency(player, reward.Currency, "Quest completion");
            messages.Add($"[Reward] +{_currencyService.GetCurrencyDisplay(reward.Currency)}");
            _log.Information("Quest reward granted: Currency={Currency}", reward.Currency);
        }

        return messages;
    }

    /// <summary>
    /// Creates a new instance of a quest from a template
    /// </summary>
    private Quest CreateQuestInstance(Quest template)
    {
        return new Quest
        {
            Id = template.Id,
            Title = template.Title,
            Description = template.Description,
            GiverNpcId = template.GiverNpcId,
            Status = QuestStatus.NotStarted,
            Objectives = template.Objectives.Select(o => new QuestObjective
            {
                Description = o.Description,
                Type = o.Type,
                TargetId = o.TargetId,
                Required = o.Required,
                Current = 0
            }).ToList(),
            Reward = template.Reward
        };
    }

    /// <summary>
    /// Gets a quest by ID
    /// </summary>
    public Quest? GetQuest(string questId)
    {
        return _questDatabase.GetValueOrDefault(questId);
    }

    /// <summary>
    /// Checks if a quest is ready to be completed
    /// </summary>
    public bool CanCompleteQuest(string questId, PlayerCharacter player)
    {
        var quest = player.ActiveQuests.FirstOrDefault(q => q.Id == questId);
        return quest != null && quest.IsComplete();
    }

    // v0.14: Quest Query Methods

    /// <summary>
    /// Gets all quests with a specific status
    /// </summary>
    public List<Quest> GetQuestsByStatus(PlayerCharacter player, QuestStatus status)
    {
        return player.ActiveQuests.Where(q => q.Status == status).ToList();
    }

    /// <summary>
    /// Gets all available quests (offered but not accepted)
    /// </summary>
    public List<Quest> GetAvailableQuests(PlayerCharacter player)
    {
        return GetQuestsByStatus(player, QuestStatus.Available);
    }

    /// <summary>
    /// Gets all active quests (accepted and in progress)
    /// </summary>
    public List<Quest> GetActiveQuests(PlayerCharacter player)
    {
        return GetQuestsByStatus(player, QuestStatus.Active);
    }

    /// <summary>
    /// Gets all complete quests (ready to turn in)
    /// </summary>
    public List<Quest> GetCompleteQuests(PlayerCharacter player)
    {
        return GetQuestsByStatus(player, QuestStatus.Complete);
    }

    /// <summary>
    /// Gets all turned in quests (from completed list)
    /// </summary>
    public List<Quest> GetTurnedInQuests(PlayerCharacter player)
    {
        return player.CompletedQuests.Where(q =>
            q.Status == QuestStatus.TurnedIn ||
            q.Status == QuestStatus.Completed).ToList();
    }

    /// <summary>
    /// Gets a player's quest by ID (searches both active and completed)
    /// </summary>
    public Quest? GetPlayerQuest(PlayerCharacter player, string questId)
    {
        return player.ActiveQuests.FirstOrDefault(q => q.Id == questId) ??
               player.CompletedQuests.FirstOrDefault(q => q.Id == questId);
    }
}
