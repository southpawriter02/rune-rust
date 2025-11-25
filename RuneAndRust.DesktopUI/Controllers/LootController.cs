using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.5: Loot Collection Controller
/// Manages post-combat loot collection workflow.
/// Handles item pickup, currency collection, and transition to progression or exploration.
/// </summary>
public class LootController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly SagaService _sagaService;
    private readonly LootService _lootService;
    private List<Equipment> _pendingLoot = new();
    private int _pendingCurrency;
    private int _pendingLegend;
    private bool _hasPendingMilestone;

    /// <summary>
    /// Event raised when loot collection is complete.
    /// </summary>
    public event EventHandler? LootCollectionComplete;

    /// <summary>
    /// Event raised when a milestone is reached during loot collection.
    /// </summary>
    public event EventHandler? MilestoneReached;

    /// <summary>
    /// Event raised when an item is collected.
    /// </summary>
    public event EventHandler<Equipment>? ItemCollected;

    public LootController(
        ILogger logger,
        GameStateController gameStateController,
        INavigationService navigationService,
        SagaService sagaService,
        LootService lootService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _sagaService = sagaService ?? throw new ArgumentNullException(nameof(sagaService));
        _lootService = lootService ?? throw new ArgumentNullException(nameof(lootService));
    }

    /// <summary>
    /// Gets the pending loot items.
    /// </summary>
    public IReadOnlyList<Equipment> PendingLoot => _pendingLoot;

    /// <summary>
    /// Gets the pending currency amount.
    /// </summary>
    public int PendingCurrency => _pendingCurrency;

    /// <summary>
    /// Gets the Legend (XP) awarded.
    /// </summary>
    public int PendingLegend => _pendingLegend;

    /// <summary>
    /// Gets whether a milestone was reached.
    /// </summary>
    public bool HasPendingMilestone => _hasPendingMilestone;

    /// <summary>
    /// Initializes loot collection with the given items and rewards.
    /// Called by CombatController after combat victory.
    /// </summary>
    public async Task InitializeLootCollectionAsync(List<Equipment> loot, int legendAwarded, int currency = 0)
    {
        if (!_gameStateController.HasActiveGame)
        {
            _logger.Error("Cannot initialize loot collection without active game");
            return;
        }

        _pendingLoot = loot ?? new List<Equipment>();
        _pendingLegend = legendAwarded;
        _pendingCurrency = currency;

        var player = _gameStateController.CurrentGameState.Player;
        if (player != null)
        {
            // Award Legend and check for milestone
            if (legendAwarded > 0)
            {
                _sagaService.AwardLegend(player, legendAwarded);
                _hasPendingMilestone = _sagaService.CanReachMilestone(player);
            }

            // Award currency
            if (currency > 0)
            {
                player.Currency += currency;
            }
        }

        _logger.Information(
            "Loot collection initialized: {ItemCount} items, {Legend} Legend, {Currency} currency, Milestone={Milestone}",
            _pendingLoot.Count, _pendingLegend, _pendingCurrency, _hasPendingMilestone);

        // Phase is already LootCollection from CombatController
        // Navigate to inventory view for loot management
        _navigationService.NavigateTo<InventoryViewModel>();
    }

    /// <summary>
    /// Collects a single item from the pending loot.
    /// </summary>
    public bool CollectItem(Equipment item)
    {
        if (!_gameStateController.HasActiveGame) return false;

        var player = _gameStateController.CurrentGameState.Player;
        if (player == null) return false;

        if (!_pendingLoot.Contains(item))
        {
            _logger.Warning("Item not in pending loot: {ItemName}", item.Name);
            return false;
        }

        // Check inventory capacity
        if (player.Inventory.Count >= player.MaxInventorySize)
        {
            _logger.Warning("Cannot collect item - inventory full: {ItemName}", item.Name);
            return false;
        }

        // Add to inventory and remove from pending
        player.Inventory.Add(item);
        _pendingLoot.Remove(item);

        _logger.Information("Item collected: {ItemName}, {Remaining} items remaining",
            item.Name, _pendingLoot.Count);

        ItemCollected?.Invoke(this, item);
        return true;
    }

    /// <summary>
    /// Collects all pending loot items (up to inventory capacity).
    /// </summary>
    public int CollectAllItems()
    {
        if (!_gameStateController.HasActiveGame) return 0;

        var player = _gameStateController.CurrentGameState.Player;
        if (player == null) return 0;

        int collected = 0;
        var itemsToCollect = _pendingLoot.ToList();

        foreach (var item in itemsToCollect)
        {
            if (player.Inventory.Count >= player.MaxInventorySize)
            {
                _logger.Information("Inventory full after collecting {Count} items", collected);
                break;
            }

            player.Inventory.Add(item);
            _pendingLoot.Remove(item);
            collected++;

            ItemCollected?.Invoke(this, item);
        }

        _logger.Information("Collected {Count} items, {Remaining} items remaining",
            collected, _pendingLoot.Count);

        return collected;
    }

    /// <summary>
    /// Drops an item from pending loot (leaves it on the ground).
    /// </summary>
    public void DropItem(Equipment item)
    {
        if (_pendingLoot.Remove(item))
        {
            // Optionally add to room's ground items
            var room = _gameStateController.CurrentGameState.CurrentRoom;
            if (room != null)
            {
                room.ItemsOnGround.Add(item);
            }

            _logger.Debug("Item dropped: {ItemName}", item.Name);
        }
    }

    /// <summary>
    /// Completes loot collection and proceeds to next phase.
    /// Transitions to CharacterProgression if milestone reached, otherwise to DungeonExploration.
    /// </summary>
    public async Task CompleteLootCollectionAsync()
    {
        if (!_gameStateController.HasActiveGame)
        {
            _logger.Error("Cannot complete loot collection without active game");
            return;
        }

        _logger.Information("Loot collection complete. Milestone pending: {Milestone}", _hasPendingMilestone);

        // Clear remaining pending loot (dropped)
        if (_pendingLoot.Count > 0)
        {
            var room = _gameStateController.CurrentGameState.CurrentRoom;
            if (room != null)
            {
                foreach (var item in _pendingLoot)
                {
                    room.ItemsOnGround.Add(item);
                }
            }
            _pendingLoot.Clear();
        }

        LootCollectionComplete?.Invoke(this, EventArgs.Empty);

        if (_hasPendingMilestone)
        {
            MilestoneReached?.Invoke(this, EventArgs.Empty);
            // ProgressionController will handle the transition
        }
        else
        {
            // Return to exploration
            await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, "Loot collected");
            _navigationService.NavigateTo<DungeonExplorationViewModel>();
        }

        // Reset state
        _hasPendingMilestone = false;
        _pendingLegend = 0;
        _pendingCurrency = 0;
    }

    /// <summary>
    /// Generates additional loot from defeated enemies.
    /// Called by CombatController during loot generation.
    /// </summary>
    public List<Equipment> GenerateCombatLoot(List<Enemy> enemies, PlayerCharacter player)
    {
        var loot = new List<Equipment>();
        int totalCurrency = 0;

        foreach (var enemy in enemies)
        {
            // Generate equipment loot
            var item = _lootService.GenerateLoot(enemy, player);
            if (item != null)
            {
                loot.Add(item);
            }

            // Generate currency
            totalCurrency += _lootService.GenerateCurrencyDrop(enemy);
        }

        _pendingCurrency += totalCurrency;
        player.Currency += totalCurrency;

        _logger.Information("Generated combat loot: {ItemCount} items, {Currency} currency from {EnemyCount} enemies",
            loot.Count, totalCurrency, enemies.Count);

        return loot;
    }

    /// <summary>
    /// Gets a summary of the loot collection for display.
    /// </summary>
    public string GetLootSummary()
    {
        var parts = new List<string>();

        if (_pendingLoot.Count > 0)
        {
            parts.Add($"{_pendingLoot.Count} item(s)");
        }

        if (_pendingCurrency > 0)
        {
            parts.Add($"{_pendingCurrency} Scrap");
        }

        if (_pendingLegend > 0)
        {
            parts.Add($"+{_pendingLegend} Legend");
        }

        if (_hasPendingMilestone)
        {
            parts.Add("MILESTONE REACHED!");
        }

        return parts.Count > 0 ? string.Join(" | ", parts) : "No loot";
    }
}
