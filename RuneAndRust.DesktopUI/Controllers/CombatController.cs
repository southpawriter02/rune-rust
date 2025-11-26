using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.5: Combat Loop Controller
/// Orchestrates combat encounters from initiation to resolution.
/// Coordinates turn management, action execution, and phase transitions.
///
/// Architecture Note: CombatViewModel handles the real-time combat UI and turn loop.
/// This controller manages the higher-level flow: initialization, phase transitions,
/// rewards, and integration with the exploration loop.
///
/// v0.44.5: Integrates with LootController and ProgressionController for
/// post-combat reward workflows.
/// v0.44.6: Integrates with DeathController for player death handling.
/// v0.44.7: Integrates with VictoryController for dungeon completion detection.
/// </summary>
public class CombatController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly CombatEngine _combatEngine;
    private readonly EnemyAI _enemyAI;
    private readonly SagaService _sagaService;
    private readonly LootService _lootService;
    private LootController? _lootController;
    private ProgressionController? _progressionController;
    private DeathController? _deathController;
    private VictoryController? _victoryController; // v0.44.7: Victory handling
    private CombatViewModel? _viewModel;
    private bool _isProcessingCombat = false;
    private string? _lastDamageSource = null; // v0.44.6: Track last enemy that dealt damage
    private bool _wasBossRoomVictory = false; // v0.44.7: Track if this was a boss room victory

    /// <summary>
    /// Event raised when combat ends (victory or defeat).
    /// </summary>
    public event EventHandler<CombatEndedEventArgs>? CombatEnded;

    /// <summary>
    /// Event raised when the player flees combat.
    /// </summary>
    public event EventHandler? PlayerFled;

    /// <summary>
    /// Event raised when loot is ready to be collected.
    /// </summary>
    public event EventHandler<LootCollectionEventArgs>? LootReady;

    public CombatController(
        ILogger logger,
        GameStateController gameStateController,
        INavigationService navigationService,
        CombatEngine combatEngine,
        EnemyAI enemyAI,
        SagaService sagaService,
        LootService lootService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
        _enemyAI = enemyAI ?? throw new ArgumentNullException(nameof(enemyAI));
        _sagaService = sagaService ?? throw new ArgumentNullException(nameof(sagaService));
        _lootService = lootService ?? throw new ArgumentNullException(nameof(lootService));
    }

    /// <summary>
    /// Initializes the controller with the combat view model.
    /// Should be called when CombatViewModel is created/activated.
    /// </summary>
    public void Initialize(CombatViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _logger.Debug("CombatController initialized with ViewModel");
    }

    /// <summary>
    /// v0.44.5: Sets the loot and progression controllers for reward workflows.
    /// v0.44.6: Also accepts DeathController for death handling.
    /// v0.44.7: Also accepts VictoryController for dungeon completion.
    /// Should be called during application startup.
    /// </summary>
    public void SetRewardControllers(
        LootController lootController,
        ProgressionController progressionController,
        DeathController? deathController = null,
        VictoryController? victoryController = null)
    {
        _lootController = lootController;
        _progressionController = progressionController;
        _deathController = deathController;
        _victoryController = victoryController;

        // Wire up events
        if (_lootController != null)
        {
            _lootController.MilestoneReached += OnMilestoneReached;
            _lootController.LootCollectionComplete += OnLootCollectionComplete; // v0.44.7
        }

        _logger.Debug("Reward controllers configured for CombatController (Death: {HasDeath}, Victory: {HasVictory})",
            deathController != null, victoryController != null);
    }

    /// <summary>
    /// v0.44.6: Sets the death controller separately (for backwards compatibility).
    /// </summary>
    public void SetDeathController(DeathController deathController)
    {
        _deathController = deathController;
        _logger.Debug("DeathController configured for CombatController");
    }

    /// <summary>
    /// v0.44.7: Sets the victory controller separately.
    /// </summary>
    public void SetVictoryController(VictoryController victoryController)
    {
        _victoryController = victoryController;
        _logger.Debug("VictoryController configured for CombatController");
    }

    /// <summary>
    /// v0.44.7: Called when loot collection is complete.
    /// Checks if this was a boss room victory triggering dungeon completion.
    /// </summary>
    private async void OnLootCollectionComplete(object? sender, EventArgs e)
    {
        if (_wasBossRoomVictory && _victoryController != null)
        {
            _logger.Information("Boss room loot collected - checking for dungeon victory");

            if (_victoryController.CheckVictoryCondition())
            {
                await _victoryController.HandleVictoryAsync();
            }
        }

        // Reset boss room flag
        _wasBossRoomVictory = false;
    }

    private async void OnMilestoneReached(object? sender, EventArgs e)
    {
        if (_progressionController != null)
        {
            await _progressionController.InitializeMilestoneProgressionAsync();
        }
    }

    /// <summary>
    /// Starts combat with the given combat state (from ExplorationController).
    /// </summary>
    public async Task StartCombatAsync(CombatState combatState)
    {
        if (combatState == null)
            throw new ArgumentNullException(nameof(combatState));

        if (_isProcessingCombat)
        {
            _logger.Warning("Combat already in progress, ignoring StartCombatAsync");
            return;
        }

        _isProcessingCombat = true;

        try
        {
            _logger.Information("Combat starting: {EnemyCount} enemies, CanFlee={CanFlee}",
                combatState.Enemies.Count, combatState.CanFlee);

            // Store in game state
            _gameStateController.CurrentGameState.CurrentCombat = combatState;

            // Navigate to combat view
            _navigationService.NavigateTo<CombatViewModel>();

            // The ViewModel should now be available via the navigation service
            // LoadCombatState will be called by the view when it initializes
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error starting combat");
            _isProcessingCombat = false;
            throw;
        }
    }

    /// <summary>
    /// Starts combat with a list of enemies (creates CombatState internally).
    /// </summary>
    public async Task StartCombatAsync(List<Enemy> enemies, bool canFlee = true)
    {
        if (!_gameStateController.HasActiveGame)
        {
            _logger.Error("Cannot start combat without active game");
            return;
        }

        var player = _gameStateController.CurrentGameState.Player;
        if (player == null)
        {
            _logger.Error("Cannot start combat without player");
            return;
        }

        var room = _gameStateController.CurrentGameState.CurrentRoom;

        _logger.Information("Initializing combat with {EnemyCount} enemies", enemies.Count);

        // Use CombatEngine to initialize combat with proper initiative and grid
        var combatState = _combatEngine.InitializeCombat(
            player,
            enemies,
            currentRoom: room,
            canFlee: canFlee,
            characterId: 0);

        await StartCombatAsync(combatState);
    }

    /// <summary>
    /// Called by ViewModel when combat state is loaded and ready.
    /// </summary>
    public void OnCombatLoaded()
    {
        _logger.Information("Combat loaded in ViewModel, beginning encounter");
    }

    /// <summary>
    /// Called by ViewModel when player's turn begins.
    /// </summary>
    public void OnPlayerTurnStarted()
    {
        _logger.Debug("Player turn started");
        // Controller can add effects like turn-start buffs here
    }

    /// <summary>
    /// Called by ViewModel when a player action is executed.
    /// </summary>
    public void OnPlayerActionExecuted(string actionType, string? targetName, int damage)
    {
        _logger.Information("Player action: {Action} on {Target} for {Damage} damage",
            actionType, targetName ?? "N/A", damage);

        // Track performance for Saga system
        if (damage > 0)
        {
            // High damage actions contribute to Legend
            var player = _gameStateController.CurrentGameState.Player;
            if (player != null && damage >= player.MaxHP / 4)
            {
                _logger.Debug("Significant damage dealt, contributing to Saga");
            }
        }
    }

    /// <summary>
    /// Called by ViewModel when an enemy's turn is processed.
    /// v0.44.6: Also tracks damage source for death cause reporting.
    /// </summary>
    public void OnEnemyTurnProcessed(string enemyName, string actionType, int damageDealt)
    {
        _logger.Debug("Enemy turn: {Enemy} used {Action}, dealt {Damage} damage",
            enemyName, actionType, damageDealt);

        // v0.44.6: Track damage source for death cause
        if (damageDealt > 0)
        {
            _lastDamageSource = enemyName;
        }

        // Check for player danger state
        var player = _gameStateController.CurrentGameState.Player;
        if (player != null && player.HP <= player.MaxHP / 4)
        {
            _logger.Warning("Survivor is critically wounded! HP: {HP}/{MaxHP}",
                player.HP, player.MaxHP);
        }
    }

    /// <summary>
    /// Handles victory in combat. Called by ViewModel when all enemies are defeated.
    /// v0.44.5: Integrates with LootController for post-combat rewards.
    /// v0.44.7: Tracks boss room victories for dungeon completion detection.
    /// </summary>
    public async Task OnCombatVictoryAsync()
    {
        if (!_gameStateController.HasActiveGame) return;

        _logger.Information("Combat victory!");

        var gameState = _gameStateController.CurrentGameState;
        var combatState = gameState.CurrentCombat;

        if (combatState == null)
        {
            _logger.Warning("No combat state found for victory handling");
            return;
        }

        // Calculate Legend (XP) reward - don't award yet, let LootController handle it
        var legendAwarded = CalculateLegendReward(combatState);
        _logger.Information("Calculated {Legend} Legend for victory", legendAwarded);

        // Generate loot
        var loot = GenerateCombatLoot(combatState);
        _logger.Information("Generated {ItemCount} loot items", loot.Count);

        // Calculate currency from enemies
        int currency = 0;
        foreach (var enemy in combatState.Enemies)
        {
            currency += _lootService.GenerateCurrencyDrop(enemy);
        }

        // Mark room as cleared
        var room = gameState.CurrentRoom;
        if (room != null)
        {
            room.HasBeenCleared = true;
            room.Enemies.Clear();

            // v0.44.7: Track if this was a boss room victory
            _wasBossRoomVictory = room.IsBossRoom;
            if (_wasBossRoomVictory)
            {
                _logger.Information("Boss room cleared! Sector completion will be checked after loot collection.");
            }
        }

        // End combat state
        _gameStateController.EndCombat();
        _isProcessingCombat = false;

        // Transition to loot collection phase
        await _gameStateController.UpdatePhaseAsync(Core.GamePhase.LootCollection, "Combat victory - collecting loot");

        // Raise legacy events for any listeners
        CombatEnded?.Invoke(this, new CombatEndedEventArgs(true, false));
        LootReady?.Invoke(this, new LootCollectionEventArgs(loot, legendAwarded));

        // v0.44.5: Use LootController if available for the full reward workflow
        if (_lootController != null)
        {
            await _lootController.InitializeLootCollectionAsync(loot, legendAwarded, currency);
        }
        else
        {
            // Fallback: just navigate to inventory
            _navigationService.NavigateTo<InventoryViewModel>();
        }
    }

    /// <summary>
    /// Handles player defeat in combat. Called by ViewModel when player HP reaches 0.
    /// v0.44.6: Integrates with DeathController for full death handling workflow.
    /// </summary>
    public async Task OnCombatDefeatAsync()
    {
        if (!_gameStateController.HasActiveGame) return;

        _logger.Warning("Combat defeat - Survivor has fallen!");

        var gameState = _gameStateController.CurrentGameState;
        var player = gameState.Player;

        // End combat state
        _gameStateController.EndCombat();
        _isProcessingCombat = false;

        CombatEnded?.Invoke(this, new CombatEndedEventArgs(false, false));

        // v0.44.6: Use DeathController for death handling if available
        if (_deathController != null && player != null)
        {
            string causeOfDeath = _lastDamageSource != null
                ? $"Slain by {_lastDamageSource}"
                : "Slain in combat";

            await _deathController.HandlePlayerDeathAsync(player, causeOfDeath);
        }
        else
        {
            // Fallback: just transition to death phase
            await _gameStateController.UpdatePhaseAsync(Core.GamePhase.Death, "Survivor has fallen in combat");
        }
    }

    /// <summary>
    /// v0.44.6: Records the last enemy that dealt damage for death cause tracking.
    /// </summary>
    public void RecordDamageSource(string enemyName)
    {
        _lastDamageSource = enemyName;
    }

    /// <summary>
    /// Handles the player fleeing combat. Uses CombatEngine flee mechanics.
    /// </summary>
    public async Task OnFleeAttemptAsync()
    {
        if (!_gameStateController.HasActiveGame) return;

        var combatState = _gameStateController.CurrentGameState.CurrentCombat;
        if (combatState == null)
        {
            _logger.Warning("No combat state for flee attempt");
            return;
        }

        if (!combatState.CanFlee)
        {
            _logger.Warning("Flee attempt blocked - combat does not allow fleeing (boss fight?)");
            _viewModel?.AddToCombatLog("Cannot flee from this battle!");
            return;
        }

        _logger.Information("Survivor attempting to flee");

        // Use CombatEngine's flee mechanics
        bool fleeSuccess = _combatEngine.PlayerFlee(combatState);

        if (fleeSuccess)
        {
            _logger.Information("Survivor fled combat successfully");

            // End combat state
            _gameStateController.EndCombat();
            _isProcessingCombat = false;

            // Transition back to exploration
            await _gameStateController.UpdatePhaseAsync(Core.GamePhase.DungeonExploration, "Fled combat");

            PlayerFled?.Invoke(this, EventArgs.Empty);
            CombatEnded?.Invoke(this, new CombatEndedEventArgs(false, true));

            // Navigate back to exploration
            _navigationService.NavigateTo<DungeonExplorationViewModel>();
        }
        else
        {
            _logger.Information("Flee attempt failed!");
            _viewModel?.AddToCombatLog("Failed to escape! Enemies close in!");

            // Flee failure - enemies may get opportunity attacks
            // This is handled by the CombatEngine's PlayerFlee method
            // which adds appropriate log entries
        }
    }

    /// <summary>
    /// Returns to exploration after combat ends (called after loot collection).
    /// </summary>
    public async Task ReturnToExplorationAsync()
    {
        if (!_gameStateController.HasActiveGame) return;

        await _gameStateController.UpdatePhaseAsync(Core.GamePhase.DungeonExploration, "Returning to exploration");
        _navigationService.NavigateTo<DungeonExplorationViewModel>();

        _logger.Information("Returned to exploration after combat");
    }

    /// <summary>
    /// Returns to exploration after defeat (for continue/restart flow).
    /// </summary>
    public void ReturnToMainMenu()
    {
        _navigationService.NavigateTo<MenuViewModel>();
        _logger.Information("Returned to main menu after combat");
    }

    #region Reward Calculations

    /// <summary>
    /// Calculates Legend (XP) reward for combat victory.
    /// </summary>
    private int CalculateLegendReward(CombatState combatState)
    {
        // Base Legend from enemies
        int baseLegend = combatState.Enemies.Sum(e => e.BaseLegendValue > 0 ? e.BaseLegendValue : CalculateEnemyLegendValue(e));

        // Bonus for clean victory (player above 50% HP)
        var player = combatState.Player;
        float healthBonus = player.HP > player.MaxHP / 2 ? 1.25f : 1.0f;

        // Bonus for low Psychic Stress
        float stressBonus = player.PsychicStress < 25 ? 1.1f : 1.0f;

        // Boss bonus
        bool hadBoss = combatState.Enemies.Any(e => e.IsBoss);
        float bossMultiplier = hadBoss ? 2.0f : 1.0f;

        int totalLegend = (int)(baseLegend * healthBonus * stressBonus * bossMultiplier);

        _logger.Debug("Legend calculation: base={Base}, health={Health}x, stress={Stress}x, boss={Boss}x, total={Total}",
            baseLegend, healthBonus, stressBonus, bossMultiplier, totalLegend);

        return totalLegend;
    }

    /// <summary>
    /// Calculates Legend value for an enemy based on their threat level.
    /// </summary>
    private int CalculateEnemyLegendValue(Enemy enemy)
    {
        // Base value from threat level
        int baseValue = enemy.ThreatLevel switch
        {
            Core.Population.ThreatLevel.Minimal => 10,
            Core.Population.ThreatLevel.Low => 25,
            Core.Population.ThreatLevel.Medium => 50,
            Core.Population.ThreatLevel.High => 100,
            Core.Population.ThreatLevel.Boss => 200,
            _ => 25
        };

        // Bonus for champion enemies
        if (enemy.IsChampion)
        {
            baseValue = (int)(baseValue * 1.5f);
        }

        // Bonus for bosses
        if (enemy.IsBoss)
        {
            baseValue = (int)(baseValue * 3.0f);
        }

        return baseValue;
    }

    /// <summary>
    /// Generates loot from defeated enemies.
    /// </summary>
    private List<Equipment> GenerateCombatLoot(CombatState combatState)
    {
        var loot = new List<Equipment>();

        // Use LootService if available, otherwise generate basic loot
        if (combatState.CurrentRoom != null)
        {
            try
            {
                _combatEngine.GenerateLoot(combatState, combatState.CurrentRoom);

                // Collect items from room
                if (combatState.CurrentRoom.ItemsOnGround.Count > 0)
                {
                    loot.AddRange(combatState.CurrentRoom.ItemsOnGround);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error generating loot via CombatEngine, using fallback");
                loot.AddRange(GenerateFallbackLoot(combatState));
            }
        }
        else
        {
            loot.AddRange(GenerateFallbackLoot(combatState));
        }

        return loot;
    }

    /// <summary>
    /// Fallback loot generation if LootService is unavailable.
    /// </summary>
    private List<Equipment> GenerateFallbackLoot(CombatState combatState)
    {
        var loot = new List<Equipment>();
        var random = new Random();

        foreach (var enemy in combatState.Enemies)
        {
            // 30% base drop chance per enemy
            if (random.NextDouble() < 0.3)
            {
                var item = new Equipment
                {
                    Name = $"Salvaged {enemy.Type} Part",
                    Type = EquipmentType.Accessory,
                    Quality = QualityTier.Scavenged,
                    Description = "A component salvaged from a fallen foe."
                };
                loot.Add(item);
            }

            // Boss guaranteed drop
            if (enemy.IsBoss)
            {
                var bossLoot = new Equipment
                {
                    Name = $"{enemy.Name}'s Trophy",
                    Type = EquipmentType.Accessory,
                    Quality = QualityTier.ClanForged,
                    Description = $"A trophy taken from the fallen {enemy.Name}."
                };
                loot.Add(bossLoot);
            }
        }

        return loot;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Adds a message to the combat log.
    /// </summary>
    public void AddToCombatLog(string message)
    {
        _viewModel?.AddToCombatLog(message);
        _logger.Debug("Combat log: {Message}", message);
    }

    /// <summary>
    /// Gets whether combat is currently in progress.
    /// </summary>
    public bool IsCombatActive => _isProcessingCombat &&
        _gameStateController.CurrentGameState.CurrentCombat?.IsActive == true;

    /// <summary>
    /// Gets the current combat state.
    /// </summary>
    public CombatState? CurrentCombatState => _gameStateController.CurrentGameState.CurrentCombat;

    #endregion
}

/// <summary>
/// Event args for combat ending.
/// </summary>
public class CombatEndedEventArgs : EventArgs
{
    /// <summary>Whether the player won.</summary>
    public bool WasVictory { get; }

    /// <summary>Whether the player fled.</summary>
    public bool PlayerFled { get; }

    public CombatEndedEventArgs(bool wasVictory, bool playerFled)
    {
        WasVictory = wasVictory;
        PlayerFled = playerFled;
    }
}

/// <summary>
/// Event args for loot collection ready.
/// </summary>
public class LootCollectionEventArgs : EventArgs
{
    /// <summary>Items available for collection.</summary>
    public List<Equipment> LootItems { get; }

    /// <summary>Legend (XP) awarded.</summary>
    public int LegendAwarded { get; }

    public LootCollectionEventArgs(List<Equipment> lootItems, int legendAwarded)
    {
        LootItems = lootItems;
        LegendAwarded = legendAwarded;
    }
}
