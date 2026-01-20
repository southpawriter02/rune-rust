namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Definitions;

/// <summary>
/// View model for a single quick-slot in the quick-slot bar.
/// </summary>
/// <remarks>
/// <para>
/// A quick-slot can hold either an ability or an item:
/// <list type="bullet">
///   <item><description>Abilities are defined by <see cref="AbilityDefinition"/></description></item>
///   <item><description>Items are defined by Item (placeholder type)</description></item>
/// </list>
/// </para>
/// </remarks>
public partial class QuickSlotViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the slot number (1-8).
    /// </summary>
    public int SlotNumber { get; }

    /// <summary>
    /// The assigned ability, if any.
    /// </summary>
    [ObservableProperty]
    private AbilityDefinition? _assignedAbility;

    /// <summary>
    /// The assigned item ID, if any.
    /// </summary>
    [ObservableProperty]
    private string? _assignedItemId;

    /// <summary>
    /// The assigned item name, if any.
    /// </summary>
    [ObservableProperty]
    private string? _assignedItemName;

    /// <summary>
    /// The assigned item icon, if any.
    /// </summary>
    [ObservableProperty]
    private string? _assignedItemIcon;

    /// <summary>
    /// Whether the slot is on cooldown.
    /// </summary>
    [ObservableProperty]
    private bool _isOnCooldown;

    /// <summary>
    /// Remaining cooldown time in seconds.
    /// </summary>
    [ObservableProperty]
    private double _cooldownRemaining;

    /// <summary>
    /// Creates a new quick-slot view model.
    /// </summary>
    /// <param name="slotNumber">The slot number (1-8).</param>
    public QuickSlotViewModel(int slotNumber)
    {
        SlotNumber = slotNumber;
    }

    /// <summary>
    /// Gets whether the slot is empty.
    /// </summary>
    public bool IsEmpty => AssignedAbility is null && string.IsNullOrEmpty(AssignedItemId);

    /// <summary>
    /// Gets the icon to display.
    /// </summary>
    public string? Icon => AssignedAbility?.Id ?? AssignedItemIcon;

    /// <summary>
    /// Gets the name to display.
    /// </summary>
    public string? Name => AssignedAbility?.Name ?? AssignedItemName;

    /// <summary>
    /// Gets whether the slot can be used.
    /// </summary>
    public bool CanUse => !IsEmpty && !IsOnCooldown;

    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    public string Tooltip => IsEmpty
        ? $"Empty (Press {SlotNumber} to use)"
        : $"{Name}\nPress {SlotNumber} to use";

    /// <summary>
    /// Assigns an ability to this slot.
    /// </summary>
    /// <param name="ability">The ability to assign.</param>
    public void AssignAbility(AbilityDefinition ability)
    {
        AssignedAbility = ability;
        AssignedItemId = null;
        AssignedItemName = null;
        AssignedItemIcon = null;
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(Icon));
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(CanUse));
        OnPropertyChanged(nameof(Tooltip));
    }

    /// <summary>
    /// Assigns an item to this slot.
    /// </summary>
    /// <param name="itemId">Item unique ID.</param>
    /// <param name="itemName">Item display name.</param>
    /// <param name="itemIcon">Item icon.</param>
    public void AssignItem(string itemId, string itemName, string? itemIcon)
    {
        AssignedAbility = null;
        AssignedItemId = itemId;
        AssignedItemName = itemName;
        AssignedItemIcon = itemIcon;
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(Icon));
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(CanUse));
        OnPropertyChanged(nameof(Tooltip));
    }

    /// <summary>
    /// Clears the slot assignment.
    /// </summary>
    public void Clear()
    {
        AssignedAbility = null;
        AssignedItemId = null;
        AssignedItemName = null;
        AssignedItemIcon = null;
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(Icon));
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(CanUse));
        OnPropertyChanged(nameof(Tooltip));
    }
}
