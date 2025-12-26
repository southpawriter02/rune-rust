using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Provides inventory management operations including item handling,
/// equipment, and burden calculations.
/// </summary>
/// <remarks>See: SPEC-INV-001 for Inventory & Equipment System design.</remarks>
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<InventoryService> _logger;

    /// <summary>
    /// Threshold for Heavy burden state (70% of capacity).
    /// </summary>
    private const double HeavyBurdenThreshold = 0.7;

    /// <summary>
    /// Threshold for Overburdened state (90% of capacity).
    /// </summary>
    private const double OverburdenedThreshold = 0.9;

    /// <summary>
    /// Grams per MIGHT point for capacity calculation.
    /// </summary>
    private const int GramsPerMight = 10000;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryService"/> class.
    /// </summary>
    /// <param name="inventoryRepository">The inventory repository.</param>
    /// <param name="eventBus">The event bus for publishing loot events (v0.3.19b).</param>
    /// <param name="logger">The logger for traceability.</param>
    public InventoryService(
        IInventoryRepository inventoryRepository,
        IEventBus eventBus,
        ILogger<InventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<InventoryResult> AddItemAsync(Character character, Item item, int quantity = 1)
    {
        _logger.LogInformation("Adding {Quantity}x {ItemName} to {CharacterName}'s inventory",
            quantity, item.Name, character.Name);

        if (quantity <= 0)
        {
            _logger.LogWarning("Invalid quantity {Quantity} for AddItemAsync", quantity);
            return new InventoryResult(false, "Invalid quantity.");
        }

        // Check if item already exists in inventory
        var existingEntry = await _inventoryRepository.GetByCharacterAndItemAsync(character.Id, item.Id);

        if (existingEntry != null)
        {
            if (item.IsStackable)
            {
                var newQuantity = existingEntry.Quantity + quantity;
                if (newQuantity > item.MaxStackSize)
                {
                    _logger.LogWarning("Stack would exceed max size: {NewQuantity} > {MaxStack}",
                        newQuantity, item.MaxStackSize);
                    return new InventoryResult(false, $"Cannot stack more than {item.MaxStackSize} {item.Name}.");
                }

                existingEntry.Quantity = newQuantity;
                existingEntry.LastModified = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(existingEntry);
                await _inventoryRepository.SaveChangesAsync();

                // v0.3.19b: Publish loot event for audio feedback
                _eventBus.Publish(new ItemLootedEvent(
                    character.Id,
                    item.Name,
                    quantity,
                    item.Value * quantity));

                _logger.LogDebug("Updated stack to {Quantity}x {ItemName}", newQuantity, item.Name);
                return new InventoryResult(true, $"Added {quantity}x {item.Name}. Now have {newQuantity}.");
            }
            else
            {
                _logger.LogWarning("Item {ItemName} is not stackable and already in inventory", item.Name);
                return new InventoryResult(false, $"You already have a {item.Name}.");
            }
        }

        // Add new inventory entry
        var nextSlot = await _inventoryRepository.GetItemCountAsync(character.Id);
        var newEntry = new InventoryItem
        {
            CharacterId = character.Id,
            ItemId = item.Id,
            Quantity = quantity,
            SlotPosition = nextSlot,
            IsEquipped = false
        };

        await _inventoryRepository.AddAsync(newEntry);
        await _inventoryRepository.SaveChangesAsync();

        // v0.3.19b: Publish loot event for audio feedback
        _eventBus.Publish(new ItemLootedEvent(
            character.Id,
            item.Name,
            quantity,
            item.Value * quantity));

        _logger.LogInformation("Added {Quantity}x {ItemName} to inventory", quantity, item.Name);
        return new InventoryResult(true, $"Added {quantity}x {item.Name} to your pack.");
    }

    /// <inheritdoc/>
    public async Task<InventoryResult> RemoveItemAsync(Character character, string itemName, int quantity = 1)
    {
        _logger.LogInformation("Removing {Quantity}x {ItemName} from {CharacterName}'s inventory",
            quantity, itemName, character.Name);

        var entry = await _inventoryRepository.FindByItemNameAsync(character.Id, itemName);
        if (entry == null)
        {
            _logger.LogDebug("Item {ItemName} not found in inventory", itemName);
            return new InventoryResult(false, $"You don't have a {itemName}.");
        }

        if (entry.Quantity < quantity)
        {
            _logger.LogDebug("Not enough {ItemName}: have {Have}, need {Need}",
                itemName, entry.Quantity, quantity);
            return new InventoryResult(false, $"You only have {entry.Quantity}x {entry.Item.Name}.");
        }

        if (entry.Quantity == quantity)
        {
            await _inventoryRepository.RemoveAsync(character.Id, entry.ItemId);
        }
        else
        {
            entry.Quantity -= quantity;
            entry.LastModified = DateTime.UtcNow;
            await _inventoryRepository.UpdateAsync(entry);
        }

        await _inventoryRepository.SaveChangesAsync();

        _logger.LogInformation("Removed {Quantity}x {ItemName} from inventory", quantity, entry.Item.Name);
        return new InventoryResult(true, $"Removed {quantity}x {entry.Item.Name}.");
    }

    /// <inheritdoc/>
    public async Task<InventoryResult> DropItemAsync(Character character, string itemName)
    {
        _logger.LogInformation("{CharacterName} dropping {ItemName}", character.Name, itemName);

        var entry = await _inventoryRepository.FindByItemNameAsync(character.Id, itemName);
        if (entry == null)
        {
            return new InventoryResult(false, $"You don't have a {itemName}.");
        }

        if (entry.IsEquipped)
        {
            return new InventoryResult(false, $"You must unequip {entry.Item.Name} before dropping it.");
        }

        if (entry.Item.ItemType == ItemType.KeyItem)
        {
            return new InventoryResult(false, $"You cannot drop {entry.Item.Name}. It seems important.");
        }

        await _inventoryRepository.RemoveAsync(character.Id, entry.ItemId);
        await _inventoryRepository.SaveChangesAsync();

        _logger.LogInformation("Dropped {ItemName}", entry.Item.Name);
        return new InventoryResult(true, $"You drop the {entry.Item.Name}.");
    }

    /// <inheritdoc/>
    public async Task<InventoryResult> EquipItemAsync(Character character, string itemName)
    {
        _logger.LogInformation("{CharacterName} equipping {ItemName}", character.Name, itemName);

        var entry = await _inventoryRepository.FindByItemNameAsync(character.Id, itemName);
        if (entry == null)
        {
            return new InventoryResult(false, $"You don't have a {itemName}.");
        }

        if (entry.Item is not Equipment equipment)
        {
            return new InventoryResult(false, $"{entry.Item.Name} cannot be equipped.");
        }

        if (entry.IsEquipped)
        {
            return new InventoryResult(false, $"{entry.Item.Name} is already equipped.");
        }

        // Check requirements
        if (!equipment.MeetsRequirements(character))
        {
            return new InventoryResult(false, $"You don't meet the requirements to equip {entry.Item.Name}.");
        }

        // Unequip anything currently in that slot
        var currentInSlot = await _inventoryRepository.GetEquippedInSlotAsync(character.Id, equipment.Slot);
        if (currentInSlot != null)
        {
            currentInSlot.IsEquipped = false;
            currentInSlot.LastModified = DateTime.UtcNow;
            await _inventoryRepository.UpdateAsync(currentInSlot);
            _logger.LogDebug("Unequipped {OldItem} from {Slot}", currentInSlot.Item.Name, equipment.Slot);
        }

        // Equip the new item
        entry.IsEquipped = true;
        entry.LastModified = DateTime.UtcNow;
        await _inventoryRepository.UpdateAsync(entry);
        await _inventoryRepository.SaveChangesAsync();

        // Recalculate bonuses
        await RecalculateEquipmentBonusesAsync(character);

        var message = currentInSlot != null
            ? $"You equip {entry.Item.Name}, replacing {currentInSlot.Item.Name}."
            : $"You equip {entry.Item.Name}.";

        _logger.LogInformation("Equipped {ItemName} in {Slot}", entry.Item.Name, equipment.Slot);
        return new InventoryResult(true, message);
    }

    /// <inheritdoc/>
    public async Task<InventoryResult> UnequipSlotAsync(Character character, EquipmentSlot slot)
    {
        _logger.LogInformation("{CharacterName} unequipping slot {Slot}", character.Name, slot);

        var entry = await _inventoryRepository.GetEquippedInSlotAsync(character.Id, slot);
        if (entry == null)
        {
            return new InventoryResult(false, $"Nothing equipped in {slot} slot.");
        }

        entry.IsEquipped = false;
        entry.LastModified = DateTime.UtcNow;
        await _inventoryRepository.UpdateAsync(entry);
        await _inventoryRepository.SaveChangesAsync();

        await RecalculateEquipmentBonusesAsync(character);

        _logger.LogInformation("Unequipped {ItemName} from {Slot}", entry.Item.Name, slot);
        return new InventoryResult(true, $"You unequip {entry.Item.Name}.");
    }

    /// <inheritdoc/>
    public async Task<InventoryResult> UnequipItemAsync(Character character, string itemName)
    {
        _logger.LogInformation("{CharacterName} unequipping {ItemName}", character.Name, itemName);

        var entry = await _inventoryRepository.FindByItemNameAsync(character.Id, itemName);
        if (entry == null)
        {
            return new InventoryResult(false, $"You don't have a {itemName}.");
        }

        if (!entry.IsEquipped)
        {
            return new InventoryResult(false, $"{entry.Item.Name} is not equipped.");
        }

        entry.IsEquipped = false;
        entry.LastModified = DateTime.UtcNow;
        await _inventoryRepository.UpdateAsync(entry);
        await _inventoryRepository.SaveChangesAsync();

        await RecalculateEquipmentBonusesAsync(character);

        _logger.LogInformation("Unequipped {ItemName}", entry.Item.Name);
        return new InventoryResult(true, $"You unequip {entry.Item.Name}.");
    }

    /// <inheritdoc/>
    public async Task<BurdenState> CalculateBurdenAsync(Character character)
    {
        var currentWeight = await GetCurrentWeightAsync(character);
        var maxCapacity = GetMaxCapacity(character);

        if (maxCapacity == 0)
        {
            _logger.LogWarning("Max capacity is zero for {CharacterName}", character.Name);
            return BurdenState.Overburdened;
        }

        var ratio = (double)currentWeight / maxCapacity;

        _logger.LogDebug("Burden calculation: {Current}g / {Max}g = {Ratio:P1}",
            currentWeight, maxCapacity, ratio);

        if (ratio >= OverburdenedThreshold)
        {
            return BurdenState.Overburdened;
        }
        else if (ratio >= HeavyBurdenThreshold)
        {
            return BurdenState.Heavy;
        }
        else
        {
            return BurdenState.Light;
        }
    }

    /// <inheritdoc/>
    public int GetMaxCapacity(Character character)
    {
        // Use effective MIGHT (base + equipment bonuses)
        var effectiveMight = character.GetEffectiveAttribute(CharacterAttribute.Might);
        return effectiveMight * GramsPerMight;
    }

    /// <inheritdoc/>
    public async Task<int> GetCurrentWeightAsync(Character character)
    {
        return await _inventoryRepository.GetTotalWeightAsync(character.Id);
    }

    /// <inheritdoc/>
    public async Task<bool> CanMoveAsync(Character character)
    {
        var burden = await CalculateBurdenAsync(character);
        return burden != BurdenState.Overburdened;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryItem>> GetInventoryAsync(Character character)
    {
        return await _inventoryRepository.GetByCharacterIdAsync(character.Id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryItem>> GetEquippedItemsAsync(Character character)
    {
        return await _inventoryRepository.GetEquippedItemsAsync(character.Id);
    }

    /// <inheritdoc/>
    public async Task RecalculateEquipmentBonusesAsync(Character character)
    {
        _logger.LogDebug("Recalculating equipment bonuses for {CharacterName}", character.Name);

        // Clear existing bonuses
        character.EquipmentBonuses.Clear();

        // Get all equipped items
        var equippedItems = await _inventoryRepository.GetEquippedItemsAsync(character.Id);

        foreach (var entry in equippedItems)
        {
            if (entry.Item is Equipment equipment)
            {
                foreach (var bonus in equipment.AttributeBonuses)
                {
                    if (character.EquipmentBonuses.ContainsKey(bonus.Key))
                    {
                        character.EquipmentBonuses[bonus.Key] += bonus.Value;
                    }
                    else
                    {
                        character.EquipmentBonuses[bonus.Key] = bonus.Value;
                    }
                }
            }
        }

        // Apply Heavy burden penalty if applicable
        var burden = await CalculateBurdenAsync(character);
        if (burden == BurdenState.Heavy)
        {
            if (character.EquipmentBonuses.ContainsKey(CharacterAttribute.Finesse))
            {
                character.EquipmentBonuses[CharacterAttribute.Finesse] -= 2;
            }
            else
            {
                character.EquipmentBonuses[CharacterAttribute.Finesse] = -2;
            }
            _logger.LogDebug("Applied Heavy burden penalty: -2 Finesse");
        }

        _logger.LogInformation("Equipment bonuses recalculated: {BonusCount} attributes modified",
            character.EquipmentBonuses.Count);
    }

    /// <inheritdoc/>
    public async Task<string> FormatInventoryDisplayAsync(Character character)
    {
        var sb = new StringBuilder();
        var inventory = await GetInventoryAsync(character);
        var inventoryList = inventory.ToList();

        var burden = await CalculateBurdenAsync(character);
        var currentWeight = await GetCurrentWeightAsync(character);
        var maxCapacity = GetMaxCapacity(character);

        sb.AppendLine("=== PACK ===");

        if (!inventoryList.Any())
        {
            sb.AppendLine("  (empty)");
        }
        else
        {
            foreach (var entry in inventoryList.Where(e => !e.IsEquipped))
            {
                var quantityStr = entry.Quantity > 1 ? $" x{entry.Quantity}" : "";
                var weightKg = entry.Item.Weight * entry.Quantity / 1000.0;
                sb.AppendLine($"  {entry.Item.Name}{quantityStr} ({weightKg:F1}kg)");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Weight: {currentWeight / 1000.0:F1}kg / {maxCapacity / 1000.0:F1}kg");

        var burdenStr = burden switch
        {
            BurdenState.Light => "Light",
            BurdenState.Heavy => "Heavy (-2 Finesse)",
            BurdenState.Overburdened => "OVERBURDENED (Cannot move!)",
            _ => "Unknown"
        };
        sb.AppendLine($"Burden: {burdenStr}");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public async Task<string> FormatEquipmentDisplayAsync(Character character)
    {
        var sb = new StringBuilder();
        var equipped = await GetEquippedItemsAsync(character);
        var equippedList = equipped.ToList();

        sb.AppendLine("=== EQUIPMENT ===");

        var slots = Enum.GetValues<EquipmentSlot>();
        foreach (var slot in slots)
        {
            var itemInSlot = equippedList
                .FirstOrDefault(e => e.Item is Equipment eq && eq.Slot == slot);

            var slotName = slot.ToString().PadRight(10);
            var itemName = itemInSlot?.Item.Name ?? "(empty)";
            sb.AppendLine($"  {slotName}: {itemName}");
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public async Task<InventoryItem?> FindItemByTagAsync(Character character, string tag)
    {
        _logger.LogDebug("Searching for item with tag '{Tag}' in {CharacterName}'s inventory", tag, character.Name);

        var item = await _inventoryRepository.FindByTagAsync(character.Id, tag);

        if (item == null)
        {
            _logger.LogDebug("No item with tag '{Tag}' found in inventory", tag);
        }
        else
        {
            _logger.LogDebug("Found item '{ItemName}' with tag '{Tag}'", item.Item.Name, tag);
        }

        return item;
    }

    /// <inheritdoc/>
    public async Task<InventoryViewModel> GetViewModelAsync(Character character, int selectedIndex = 0)
    {
        _logger.LogTrace("[GetViewModel] Building inventory snapshot for {CharacterName}", character.Name);

        var allItems = (await _inventoryRepository.GetByCharacterIdAsync(character.Id)).ToList();

        // Build equipped items dictionary
        var equippedDict = new Dictionary<EquipmentSlot, EquippedItemView?>();
        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            var equipped = allItems.FirstOrDefault(i => i.IsEquipped && i.Item is Equipment eq && eq.Slot == slot);
            if (equipped?.Item is Equipment equipment)
            {
                equippedDict[slot] = new EquippedItemView(
                    Name: equipment.Name,
                    Quality: equipment.Quality,
                    DurabilityPercentage: equipment.MaxDurability > 0
                        ? (int)((double)equipment.CurrentDurability / equipment.MaxDurability * 100)
                        : 100,
                    IsBroken: equipment.IsBroken
                );
            }
            else
            {
                equippedDict[slot] = null;
            }
        }

        // Build backpack items list (non-equipped, ordered by slot position)
        var backpackItems = allItems
            .Where(i => !i.IsEquipped)
            .OrderBy(i => i.SlotPosition)
            .Select((item, idx) => new BackpackItemView(
                Index: idx + 1,
                Name: item.Item.Name,
                Quantity: item.Quantity,
                Quality: item.Item.Quality,
                WeightGrams: item.Item.Weight * item.Quantity,
                ItemType: item.Item.ItemType,
                IsEquipable: item.Item is Equipment
            ))
            .ToList();

        // Calculate burden metrics
        var currentWeight = await GetCurrentWeightAsync(character);
        var maxCapacity = GetMaxCapacity(character);
        var burdenPct = maxCapacity > 0 ? (int)((double)currentWeight / maxCapacity * 100) : 0;
        var burdenState = await CalculateBurdenAsync(character);

        _logger.LogDebug("[GetViewModel] Inventory snapshot built: {BackpackCount} backpack items, {Weight}g/{MaxWeight}g ({BurdenPct}%)",
            backpackItems.Count, currentWeight, maxCapacity, burdenPct);

        return new InventoryViewModel(
            CharacterName: character.Name,
            EquippedItems: equippedDict,
            BackpackItems: backpackItems,
            CurrentWeight: currentWeight,
            MaxCapacity: maxCapacity,
            BurdenPercentage: burdenPct,
            BurdenState: burdenState,
            SelectedIndex: Math.Clamp(selectedIndex, 0, Math.Max(0, backpackItems.Count - 1))
        );
    }
}
