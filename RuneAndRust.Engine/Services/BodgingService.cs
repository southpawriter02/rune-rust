using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Crafting;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements repair and salvage operations for equipment items.
/// Repair uses WITS-based dice rolls to determine success and quality.
/// Salvage destroys equipment to extract Scrap materials.
/// </summary>
/// <remarks>See: SPEC-REPAIR-001 for Repair & Salvage System design.</remarks>
public class BodgingService : IBodgingService
{
    private readonly IDiceService _diceService;
    private readonly IInventoryService _inventoryService;
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<BodgingService> _logger;

    /// <summary>
    /// Base DC for repair operations before damage modifier.
    /// </summary>
    private const int BaseRepairDc = 8;

    /// <summary>
    /// Divisor for calculating Scrap cost and DC modifier from damage.
    /// </summary>
    private const int DamageDivisor = 5;

    /// <summary>
    /// Threshold above DC for masterwork repair (DC + 5).
    /// </summary>
    private const int MasterworkThreshold = 5;

    /// <summary>
    /// MaxDurability reduction on catastrophic failure.
    /// </summary>
    private const int CatastrophePenalty = 10;

    /// <summary>
    /// Divisor for calculating salvage yield from weight.
    /// </summary>
    private const int SalvageWeightDivisor = 100;

    /// <summary>
    /// The item name for Scrap material.
    /// </summary>
    private const string ScrapItemName = "scrap";

    public BodgingService(
        IDiceService diceService,
        IInventoryService inventoryService,
        IItemRepository itemRepository,
        ILogger<BodgingService> logger)
    {
        _diceService = diceService;
        _inventoryService = inventoryService;
        _itemRepository = itemRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RepairResult> RepairItemAsync(Character character, Guid itemId)
    {
        // Step 1: Find the item in inventory
        var inventoryItem = character.Inventory
            .FirstOrDefault(inv => inv.Item.Id == itemId);

        if (inventoryItem == null)
        {
            return CreateFailedRepairResult("Unknown Item", 0, 0,
                "Item not found in inventory.");
        }

        _logger.LogInformation("Repair attempt: {ItemName} by {CharacterName}",
            inventoryItem.Item.Name, character.Name);

        // Step 2: Verify item is Equipment
        if (inventoryItem.Item is not Equipment equipment)
        {
            return CreateFailedRepairResult(inventoryItem.Item.Name, 0, 0,
                $"Cannot repair {inventoryItem.Item.Name} - it is not equipment.");
        }

        // Step 3: Check if item is damaged
        var damage = equipment.MaxDurability - equipment.CurrentDurability;
        if (damage <= 0)
        {
            return CreateFailedRepairResult(equipment.Name, 0, 0,
                $"{equipment.Name} is already in good condition.");
        }

        // Step 4: Calculate Scrap cost
        var scrapCost = CalculateRepairCost(equipment);
        _logger.LogDebug("Repair cost: {ScrapCost} Scrap for {Damage} damage",
            scrapCost, damage);

        // Step 5: Validate character has enough Scrap
        if (!CanRepair(character, equipment))
        {
            return CreateFailedRepairResult(equipment.Name, 0, scrapCost,
                $"Insufficient Scrap. Need {scrapCost} Scrap to repair {equipment.Name}.");
        }

        // Step 6: Consume Scrap
        var removeResult = await _inventoryService.RemoveItemAsync(character, ScrapItemName, scrapCost);
        if (!removeResult.Success)
        {
            _logger.LogWarning("Failed to consume Scrap for repair: {Message}", removeResult.Message);
            return CreateFailedRepairResult(equipment.Name, 0, scrapCost,
                $"Failed to consume Scrap: {removeResult.Message}");
        }
        _logger.LogDebug("Consumed: {Quantity}x Scrap for repair", scrapCost);

        // Step 7: Calculate DC
        var dc = BaseRepairDc + (damage / DamageDivisor);

        // Step 8: Roll WITS dice pool
        var wits = character.Wits;
        var diceResult = _diceService.Roll(wits, $"Repair {equipment.Name}");
        var netSuccesses = diceResult.Successes - diceResult.Botches;

        _logger.LogDebug(
            "Repair roll: {Wits}d10 = {Successes}S/{Botches}B, Net={Net}, DC={DC}",
            wits, diceResult.Successes, diceResult.Botches, netSuccesses, dc);

        // Step 9: Determine outcome
        var outcome = DetermineOutcome(netSuccesses, dc);

        // Step 10: Apply outcome
        return outcome switch
        {
            CraftingOutcome.Masterwork => HandleMasterworkRepair(equipment, diceResult, netSuccesses, dc, scrapCost, damage),
            CraftingOutcome.Success => HandleSuccessfulRepair(equipment, diceResult, netSuccesses, dc, scrapCost, damage),
            CraftingOutcome.Failure => HandleFailedRepair(equipment, diceResult, netSuccesses, dc, scrapCost),
            CraftingOutcome.Catastrophe => HandleCatastrophicRepair(equipment, diceResult, netSuccesses, dc, scrapCost),
            _ => HandleFailedRepair(equipment, diceResult, netSuccesses, dc, scrapCost)
        };
    }

    /// <inheritdoc />
    public async Task<SalvageResult> SalvageItemAsync(Character character, Guid itemId)
    {
        // Step 1: Find the item in inventory
        var inventoryItem = character.Inventory
            .FirstOrDefault(inv => inv.Item.Id == itemId);

        if (inventoryItem == null)
        {
            return new SalvageResult(
                IsSuccess: false,
                ItemName: "Unknown Item",
                ScrapYield: 0,
                Message: "Item not found in inventory.");
        }

        _logger.LogInformation("Salvage attempt: {ItemName} by {CharacterName}",
            inventoryItem.Item.Name, character.Name);

        // Step 2: Verify item is Equipment
        if (inventoryItem.Item is not Equipment equipment)
        {
            return new SalvageResult(
                IsSuccess: false,
                ItemName: inventoryItem.Item.Name,
                ScrapYield: 0,
                Message: $"Cannot salvage {inventoryItem.Item.Name} - it is not equipment.");
        }

        // Step 3: Calculate yield
        var yield = CalculateSalvageYield(equipment);
        _logger.LogDebug("Salvage yield: {Yield} Scrap from {Weight}g {Quality} item",
            yield, equipment.Weight, equipment.Quality);

        // Step 4: Remove the item from inventory
        var removeResult = await _inventoryService.RemoveItemAsync(character, equipment.Name, 1);
        if (!removeResult.Success)
        {
            _logger.LogWarning("Failed to remove item for salvage: {Message}", removeResult.Message);
            return new SalvageResult(
                IsSuccess: false,
                ItemName: equipment.Name,
                ScrapYield: 0,
                Message: $"Failed to remove item: {removeResult.Message}");
        }

        // Step 5: Add Scrap to inventory
        var scrapItem = await _itemRepository.GetByNameAsync(ScrapItemName);
        if (scrapItem != null)
        {
            await _inventoryService.AddItemAsync(character, scrapItem, yield);
        }
        else
        {
            // Create a basic scrap item if not found in repository
            var basicScrap = new Item
            {
                Id = Guid.NewGuid(),
                Name = ScrapItemName,
                ItemType = ItemType.Material,
                Description = "Salvaged metal fragments useful for repairs.",
                Weight = 100,
                Value = 5,
                IsStackable = true,
                MaxStackSize = 99,
                Quality = QualityTier.JuryRigged,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
            await _inventoryService.AddItemAsync(character, basicScrap, yield);
        }

        _logger.LogInformation("Salvage SUCCESS: {ItemName} -> {Yield}x Scrap",
            equipment.Name, yield);

        return new SalvageResult(
            IsSuccess: true,
            ItemName: equipment.Name,
            ScrapYield: yield,
            Message: $"Salvaged {equipment.Name} for {yield} Scrap.");
    }

    /// <inheritdoc />
    public int CalculateRepairCost(Equipment equipment)
    {
        var damage = equipment.MaxDurability - equipment.CurrentDurability;
        var cost = (int)Math.Ceiling((double)damage / DamageDivisor);
        return Math.Max(1, cost); // Minimum 1 Scrap
    }

    /// <inheritdoc />
    public int CalculateSalvageYield(Equipment equipment)
    {
        var qualityMod = GetQualityModifier(equipment.Quality);
        var yield = (equipment.Weight / SalvageWeightDivisor) * (qualityMod + 1);
        return Math.Max(1, yield); // Minimum 1 Scrap
    }

    /// <inheritdoc />
    public bool CanRepair(Character character, Equipment equipment)
    {
        var requiredScrap = CalculateRepairCost(equipment);
        var scrapInInventory = character.Inventory
            .FirstOrDefault(inv => inv.Item.Name.Equals(ScrapItemName, StringComparison.OrdinalIgnoreCase));

        return (scrapInInventory?.Quantity ?? 0) >= requiredScrap;
    }

    /// <summary>
    /// Gets the quality modifier for salvage yield calculation.
    /// </summary>
    private static int GetQualityModifier(QualityTier tier) => tier switch
    {
        QualityTier.JuryRigged => 0,
        QualityTier.Scavenged => 1,
        QualityTier.ClanForged => 2,
        QualityTier.Optimized => 3,
        QualityTier.MythForged => 4,
        _ => 0
    };

    /// <summary>
    /// Determines the repair outcome based on net successes and DC.
    /// </summary>
    private static CraftingOutcome DetermineOutcome(int netSuccesses, int dc)
    {
        if (netSuccesses < 0)
            return CraftingOutcome.Catastrophe;

        if (netSuccesses >= dc + MasterworkThreshold)
            return CraftingOutcome.Masterwork;

        if (netSuccesses >= dc)
            return CraftingOutcome.Success;

        return CraftingOutcome.Failure;
    }

    /// <summary>
    /// Handles a masterwork repair - fully restores durability.
    /// </summary>
    private RepairResult HandleMasterworkRepair(
        Equipment equipment,
        DiceResult diceResult,
        int netSuccesses,
        int dc,
        int scrapConsumed,
        int damage)
    {
        equipment.CurrentDurability = equipment.MaxDurability;

        _logger.LogInformation("Repair MASTERWORK: {ItemName} fully restored", equipment.Name);

        return new RepairResult(
            IsSuccess: true,
            Outcome: CraftingOutcome.Masterwork,
            ItemName: equipment.Name,
            DiceRolled: diceResult.Rolls.Count,
            Successes: diceResult.Successes,
            Botches: diceResult.Botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: dc,
            DurabilityRestored: damage,
            ScrapConsumed: scrapConsumed,
            MaxDurabilityLost: null,
            Message: $"MASTERWORK! {equipment.Name} has been fully restored to pristine condition.",
            Rolls: diceResult.Rolls);
    }

    /// <summary>
    /// Handles a successful repair - restores partial durability.
    /// </summary>
    private RepairResult HandleSuccessfulRepair(
        Equipment equipment,
        DiceResult diceResult,
        int netSuccesses,
        int dc,
        int scrapConsumed,
        int damage)
    {
        // Restore durability: min(damage, netSuccesses * 5)
        var restored = Math.Min(damage, netSuccesses * DamageDivisor);
        equipment.CurrentDurability = Math.Min(
            equipment.MaxDurability,
            equipment.CurrentDurability + restored);

        _logger.LogInformation("Repair SUCCESS: {ItemName} restored {Durability} points",
            equipment.Name, restored);

        return new RepairResult(
            IsSuccess: true,
            Outcome: CraftingOutcome.Success,
            ItemName: equipment.Name,
            DiceRolled: diceResult.Rolls.Count,
            Successes: diceResult.Successes,
            Botches: diceResult.Botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: dc,
            DurabilityRestored: restored,
            ScrapConsumed: scrapConsumed,
            MaxDurabilityLost: null,
            Message: $"Success! {equipment.Name} has been repaired. Durability restored by {restored}.",
            Rolls: diceResult.Rolls);
    }

    /// <summary>
    /// Handles a failed repair - no durability restored, materials lost.
    /// </summary>
    private RepairResult HandleFailedRepair(
        Equipment equipment,
        DiceResult diceResult,
        int netSuccesses,
        int dc,
        int scrapConsumed)
    {
        _logger.LogInformation("Repair FAILURE: {ItemName} (Net {Net} < DC {DC})",
            equipment.Name, netSuccesses, dc);

        return new RepairResult(
            IsSuccess: false,
            Outcome: CraftingOutcome.Failure,
            ItemName: equipment.Name,
            DiceRolled: diceResult.Rolls.Count,
            Successes: diceResult.Successes,
            Botches: diceResult.Botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: dc,
            DurabilityRestored: 0,
            ScrapConsumed: scrapConsumed,
            MaxDurabilityLost: null,
            Message: $"Failure. Your repair attempt on {equipment.Name} was unsuccessful. Scrap wasted.",
            Rolls: diceResult.Rolls);
    }

    /// <summary>
    /// Handles a catastrophic repair - MaxDurability permanently reduced.
    /// </summary>
    private RepairResult HandleCatastrophicRepair(
        Equipment equipment,
        DiceResult diceResult,
        int netSuccesses,
        int dc,
        int scrapConsumed)
    {
        // Permanently reduce MaxDurability
        equipment.MaxDurability = Math.Max(0, equipment.MaxDurability - CatastrophePenalty);

        // Ensure CurrentDurability doesn't exceed new max
        equipment.CurrentDurability = Math.Min(equipment.CurrentDurability, equipment.MaxDurability);

        _logger.LogWarning("Repair CATASTROPHE: {ItemName} - MaxDurability reduced by {Penalty}!",
            equipment.Name, CatastrophePenalty);

        return new RepairResult(
            IsSuccess: false,
            Outcome: CraftingOutcome.Catastrophe,
            ItemName: equipment.Name,
            DiceRolled: diceResult.Rolls.Count,
            Successes: diceResult.Successes,
            Botches: diceResult.Botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: dc,
            DurabilityRestored: 0,
            ScrapConsumed: scrapConsumed,
            MaxDurabilityLost: CatastrophePenalty,
            Message: $"CATASTROPHE! Your fumbled repair has permanently damaged {equipment.Name}. Maximum durability reduced by {CatastrophePenalty}.",
            Rolls: diceResult.Rolls);
    }

    /// <summary>
    /// Creates a failed repair result for validation errors (before rolling).
    /// </summary>
    private static RepairResult CreateFailedRepairResult(
        string itemName,
        int dc,
        int scrapConsumed,
        string message)
    {
        return new RepairResult(
            IsSuccess: false,
            Outcome: CraftingOutcome.Failure,
            ItemName: itemName,
            DiceRolled: 0,
            Successes: 0,
            Botches: 0,
            NetSuccesses: 0,
            DifficultyClass: dc,
            DurabilityRestored: 0,
            ScrapConsumed: scrapConsumed,
            MaxDurabilityLost: null,
            Message: message,
            Rolls: Array.Empty<int>());
    }
}
