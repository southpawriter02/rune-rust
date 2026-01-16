namespace RuneAndRust.Presentation.Gui.ViewModels;

using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Definitions;

/// <summary>
/// View model for the quick-slot bar containing 8 slots.
/// </summary>
/// <remarks>
/// <para>
/// Manages 8 quick-slots that can be activated via:
/// <list type="bullet">
///   <item><description>Keys 1-8 on the main keyboard</description></item>
///   <item><description>Numpad keys 1-8</description></item>
/// </list>
/// </para>
/// </remarks>
public partial class QuickSlotBarViewModel : ViewModelBase
{
    private readonly ILogger<QuickSlotBarViewModel> _logger;

    /// <summary>
    /// The collection of quick-slots.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<QuickSlotViewModel> _slots = new();

    /// <summary>
    /// Creates a new quick-slot bar view model.
    /// </summary>
    /// <param name="logger">Logger for slot operations.</param>
    public QuickSlotBarViewModel(ILogger<QuickSlotBarViewModel> logger)
    {
        _logger = logger;

        // Initialize 8 slots
        for (var i = 1; i <= 8; i++)
        {
            Slots.Add(new QuickSlotViewModel(i));
        }

        _logger.LogDebug("QuickSlotBarViewModel initialized with 8 slots");
    }

    /// <summary>
    /// Creates a new quick-slot bar view model with default logger.
    /// </summary>
    /// <remarks>For design-time and testing scenarios.</remarks>
    public QuickSlotBarViewModel() : this(CreateNullLogger())
    {
    }

    /// <summary>
    /// Uses the ability or item in the specified slot.
    /// </summary>
    /// <param name="slotNumber">Slot number (1-8).</param>
    [RelayCommand]
    public void UseSlot(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > 8)
        {
            _logger.LogWarning("Invalid slot number: {SlotNumber}", slotNumber);
            return;
        }

        var slot = Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        if (slot is null || slot.IsEmpty)
        {
            _logger.LogDebug("Slot {SlotNumber} is empty", slotNumber);
            return;
        }

        if (!slot.CanUse)
        {
            _logger.LogDebug("Slot {SlotNumber} is on cooldown", slotNumber);
            return;
        }

        if (slot.AssignedAbility is not null)
        {
            _logger.LogInformation(
                "Using ability from slot {SlotNumber}: {AbilityName}",
                slotNumber,
                slot.AssignedAbility.Name);

            // Trigger ability use event (to be wired to AbilityService)
            OnAbilityUsed(slot.AssignedAbility);
        }
        else if (!string.IsNullOrEmpty(slot.AssignedItemId))
        {
            _logger.LogInformation(
                "Using item from slot {SlotNumber}: {ItemName}",
                slotNumber,
                slot.AssignedItemName);

            // Trigger item use event (to be wired to InventoryService)
            OnItemUsed(slot.AssignedItemId);
        }
    }

    /// <summary>
    /// Assigns an ability to a specific slot.
    /// </summary>
    /// <param name="slotNumber">Slot number (1-8).</param>
    /// <param name="ability">The ability to assign.</param>
    public void AssignAbilityToSlot(int slotNumber, AbilityDefinition ability)
    {
        var slot = Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        if (slot is null) return;

        slot.AssignAbility(ability);
        _logger.LogDebug("Assigned {Ability} to slot {SlotNumber}", ability.Name, slotNumber);
    }

    /// <summary>
    /// Assigns an item to a specific slot.
    /// </summary>
    /// <param name="slotNumber">Slot number (1-8).</param>
    /// <param name="itemId">Item ID.</param>
    /// <param name="itemName">Item name.</param>
    /// <param name="itemIcon">Item icon.</param>
    public void AssignItemToSlot(int slotNumber, string itemId, string itemName, string? itemIcon)
    {
        var slot = Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        if (slot is null) return;

        slot.AssignItem(itemId, itemName, itemIcon);
        _logger.LogDebug("Assigned {Item} to slot {SlotNumber}", itemName, slotNumber);
    }

    /// <summary>
    /// Handles keyboard input for quick-slot activation.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    public void HandleKeyPress(Key key)
    {
        int? slotNumber = key switch
        {
            Key.D1 => 1,
            Key.D2 => 2,
            Key.D3 => 3,
            Key.D4 => 4,
            Key.D5 => 5,
            Key.D6 => 6,
            Key.D7 => 7,
            Key.D8 => 8,
            Key.NumPad1 => 1,
            Key.NumPad2 => 2,
            Key.NumPad3 => 3,
            Key.NumPad4 => 4,
            Key.NumPad5 => 5,
            Key.NumPad6 => 6,
            Key.NumPad7 => 7,
            Key.NumPad8 => 8,
            _ => null
        };

        if (slotNumber.HasValue)
        {
            _logger.LogDebug("Key press activated slot {SlotNumber}", slotNumber.Value);
            UseSlot(slotNumber.Value);
        }
    }

    /// <summary>
    /// Gets the slot by number.
    /// </summary>
    /// <param name="slotNumber">Slot number (1-8).</param>
    /// <returns>The slot view model, or null if not found.</returns>
    public QuickSlotViewModel? GetSlot(int slotNumber) =>
        Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);

    /// <summary>
    /// Event raised when an ability is used from a slot.
    /// </summary>
    public event Action<AbilityDefinition>? AbilityUsed;

    /// <summary>
    /// Event raised when an item is used from a slot.
    /// </summary>
    public event Action<string>? ItemUsed;

    private void OnAbilityUsed(AbilityDefinition ability) => AbilityUsed?.Invoke(ability);
    private void OnItemUsed(string itemId) => ItemUsed?.Invoke(itemId);

    private static ILogger<QuickSlotBarViewModel> CreateNullLogger() =>
        Microsoft.Extensions.Logging.Abstractions.NullLogger<QuickSlotBarViewModel>.Instance;
}
