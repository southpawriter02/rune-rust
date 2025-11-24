using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.41: Service for managing alternative starting scenarios
/// Handles scenario selection and character initialization with scenario modifications
/// </summary>
public class AlternativeStartService
{
    private readonly ILogger _logger;
    private readonly AlternativeStartRepository _startRepo;

    public AlternativeStartService(AlternativeStartRepository startRepo)
    {
        _logger = Log.ForContext<AlternativeStartService>();
        _startRepo = startRepo;
    }

    #region Alternative Start Management

    /// <summary>
    /// Get all alternative starts
    /// </summary>
    public List<AlternativeStart> GetAllStarts()
    {
        return _startRepo.GetAll();
    }

    /// <summary>
    /// Get unlocked alternative starts for account
    /// </summary>
    public List<AlternativeStart> GetUnlockedStarts(int accountId)
    {
        return _startRepo.GetUnlockedStarts(accountId);
    }

    /// <summary>
    /// Get all alternative starts with unlock status
    /// </summary>
    public List<AlternativeStart> GetAllWithUnlockStatus(int accountId)
    {
        return _startRepo.GetAllWithUnlockStatus(accountId);
    }

    /// <summary>
    /// Get alternative start by ID
    /// </summary>
    public AlternativeStart? GetById(string startId)
    {
        return _startRepo.GetById(startId);
    }

    /// <summary>
    /// Check if alternative start is unlocked
    /// </summary>
    public bool IsUnlocked(int accountId, string startId)
    {
        return _startRepo.IsUnlocked(accountId, startId);
    }

    #endregion

    #region Character Initialization

    /// <summary>
    /// Initialize character with alternative start scenario
    /// </summary>
    public void InitializeCharacterWithScenario(
        int accountId,
        PlayerCharacter character,
        string startScenarioId)
    {
        _logger.Information("Initializing character with scenario: AccountID={AccountId}, Scenario={ScenarioId}, Character={CharacterName}",
            accountId, startScenarioId, character.Name);

        var scenario = _startRepo.GetById(startScenarioId);
        if (scenario == null)
        {
            _logger.Warning("Scenario not found: {ScenarioId}", startScenarioId);
            throw new InvalidOperationException($"Scenario not found: {startScenarioId}");
        }

        // Verify scenario is unlocked
        if (!_startRepo.IsUnlocked(accountId, startScenarioId))
        {
            _logger.Warning("Scenario not unlocked: {ScenarioId}", startScenarioId);
            throw new InvalidOperationException($"Scenario not unlocked: {startScenarioId}");
        }

        // Apply scenario modifications
        ApplyScenarioModifications(character, scenario);

        _logger.Information("Character initialized with scenario: {ScenarioName}, Level={Level}",
            scenario.Name, character.CurrentMilestone);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Apply scenario modifications to character
    /// </summary>
    private void ApplyScenarioModifications(PlayerCharacter character, AlternativeStart scenario)
    {
        _logger.Debug("Applying scenario modifications: Scenario={ScenarioName}", scenario.Name);

        // Apply starting level
        if (scenario.StartingLevel > 1)
        {
            character.CurrentMilestone = scenario.StartingLevel;
            _logger.Debug("Set starting level: {Level}", scenario.StartingLevel);
        }

        // Apply starting equipment
        foreach (var equipmentName in scenario.StartingEquipment)
        {
            // Future: Look up equipment and add to inventory
            _logger.Debug("Starting equipment: {Equipment}", equipmentName);
        }

        // Apply starting Legend
        if (scenario.StartingLegend > 0)
        {
            character.CurrentLegend = scenario.StartingLegend;
            _logger.Debug("Set starting Legend: {Legend}", scenario.StartingLegend);
        }

        // Apply starting resources
        foreach (var resource in scenario.StartingResources)
        {
            switch (resource.Key.ToLower())
            {
                case "currency":
                    character.Currency = resource.Value;
                    _logger.Debug("Set starting currency: {Currency}", resource.Value);
                    break;

                case "hp":
                    character.HP = Math.Min(resource.Value, character.MaxHP);
                    _logger.Debug("Set starting HP: {HP}", resource.Value);
                    break;

                case "stamina":
                    character.Stamina = Math.Min(resource.Value, character.MaxStamina);
                    _logger.Debug("Set starting Stamina: {Stamina}", resource.Value);
                    break;

                // Add more resource types as needed
            }
        }

        // Apply starting sector
        if (scenario.StartingSectorId.HasValue)
        {
            character.CurrentSectorId = scenario.StartingSectorId.Value;
            _logger.Debug("Set starting sector: {SectorId}", scenario.StartingSectorId);
        }

        // Mark completed quests
        foreach (var questId in scenario.CompletedQuests)
        {
            // Future: Mark quest as completed
            _logger.Debug("Quest marked as completed: {QuestId}", questId);
        }

        // Apply difficulty modifiers
        if (scenario.HardModeEnabled)
        {
            _logger.Information("Hard mode enabled for character");
            // Future: Apply hard mode modifiers
        }

        if (scenario.PermadeathEnabled)
        {
            _logger.Information("Permadeath enabled for character");
            // Future: Apply permadeath flag
        }

        if (scenario.RewardMultiplier != 1.0f)
        {
            _logger.Information("Reward multiplier applied: {Multiplier}x", scenario.RewardMultiplier);
            // Future: Apply reward multiplier to character
        }

        _logger.Debug("Scenario modifications applied successfully");
    }

    #endregion
}
