namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// View model for the player status panel.
/// </summary>
/// <remarks>
/// Manages the display of player information including:
/// - Character name, class, and level
/// - HP, MP, XP resource bars
/// - Six core stats with modifiers
/// - Equipment slots
/// - Gold and current location
/// </remarks>
public partial class PlayerStatusViewModel : ViewModelBase
{
    // Character Info
    /// <summary>
    /// Gets or sets the player name.
    /// </summary>
    [ObservableProperty]
    private string _playerName = "Unknown";

    /// <summary>
    /// Gets or sets the class name.
    /// </summary>
    [ObservableProperty]
    private string _className = "Adventurer";

    /// <summary>
    /// Gets or sets the player level.
    /// </summary>
    [ObservableProperty]
    private int _level = 1;

    // Resource Bars
    /// <summary>
    /// Gets or sets the current HP.
    /// </summary>
    [ObservableProperty]
    private int _currentHp = 100;

    /// <summary>
    /// Gets or sets the maximum HP.
    /// </summary>
    [ObservableProperty]
    private int _maxHp = 100;

    /// <summary>
    /// Gets or sets the current MP.
    /// </summary>
    [ObservableProperty]
    private int _currentMp;

    /// <summary>
    /// Gets or sets the maximum MP.
    /// </summary>
    [ObservableProperty]
    private int _maxMp;

    /// <summary>
    /// Gets or sets the current XP.
    /// </summary>
    [ObservableProperty]
    private int _currentXp;

    /// <summary>
    /// Gets or sets the XP required for next level.
    /// </summary>
    [ObservableProperty]
    private int _xpToNextLevel = 100;

    // Stats
    /// <summary>
    /// Gets or sets the Strength stat value.
    /// </summary>
    [ObservableProperty]
    private int _strength = 10;

    /// <summary>
    /// Gets or sets the Strength modifier.
    /// </summary>
    [ObservableProperty]
    private int _strengthModifier;

    /// <summary>
    /// Gets or sets the Dexterity stat value.
    /// </summary>
    [ObservableProperty]
    private int _dexterity = 10;

    /// <summary>
    /// Gets or sets the Dexterity modifier.
    /// </summary>
    [ObservableProperty]
    private int _dexterityModifier;

    /// <summary>
    /// Gets or sets the Constitution stat value.
    /// </summary>
    [ObservableProperty]
    private int _constitution = 10;

    /// <summary>
    /// Gets or sets the Constitution modifier.
    /// </summary>
    [ObservableProperty]
    private int _constitutionModifier;

    /// <summary>
    /// Gets or sets the Intelligence stat value.
    /// </summary>
    [ObservableProperty]
    private int _intelligence = 10;

    /// <summary>
    /// Gets or sets the Intelligence modifier.
    /// </summary>
    [ObservableProperty]
    private int _intelligenceModifier;

    /// <summary>
    /// Gets or sets the Wisdom stat value.
    /// </summary>
    [ObservableProperty]
    private int _wisdom = 10;

    /// <summary>
    /// Gets or sets the Wisdom modifier.
    /// </summary>
    [ObservableProperty]
    private int _wisdomModifier;

    /// <summary>
    /// Gets or sets the Charisma stat value.
    /// </summary>
    [ObservableProperty]
    private int _charisma = 10;

    /// <summary>
    /// Gets or sets the Charisma modifier.
    /// </summary>
    [ObservableProperty]
    private int _charismaModifier;

    // Equipment
    /// <summary>
    /// Gets or sets the equipment slots.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<EquipmentSlotViewModel> _equipment = [];

    // Footer
    /// <summary>
    /// Gets or sets the gold amount.
    /// </summary>
    [ObservableProperty]
    private int _gold;

    /// <summary>
    /// Gets or sets the current location name.
    /// </summary>
    [ObservableProperty]
    private string _locationName = "Unknown";

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerStatusViewModel"/> class
    /// with design-time sample data.
    /// </summary>
    public PlayerStatusViewModel()
    {
        // Design-time sample data
        PlayerName = "Hero";
        ClassName = "Warrior";
        Level = 5;
        CurrentHp = 65;
        MaxHp = 100;
        CurrentMp = 30;
        MaxMp = 100;
        CurrentXp = 550;
        XpToNextLevel = 1000;
        Strength = 16;
        StrengthModifier = 3;
        Dexterity = 14;
        DexterityModifier = 2;
        Constitution = 15;
        ConstitutionModifier = 2;
        Intelligence = 10;
        IntelligenceModifier = 0;
        Wisdom = 12;
        WisdomModifier = 1;
        Charisma = 11;
        CharismaModifier = 0;
        Gold = 150;
        LocationName = "Dark Cave";

        InitializeEquipmentSlots();
        Equipment[0] = new EquipmentSlotViewModel("Weapon", "⚔", "Iron Sword", "+4 atk");
        Equipment[1] = new EquipmentSlotViewModel("Armor", "🛡", "Leather", "+2 def");
        Equipment[3] = new EquipmentSlotViewModel("Ring", "💍", "Ring of Protection", "+1 def");

        Log.Debug("PlayerStatusViewModel initialized with sample data");
    }

    /// <summary>
    /// Initializes the five standard equipment slots.
    /// </summary>
    private void InitializeEquipmentSlots()
    {
        Equipment.Clear();
        Equipment.Add(new EquipmentSlotViewModel("Weapon", "⚔"));
        Equipment.Add(new EquipmentSlotViewModel("Armor", "🛡"));
        Equipment.Add(new EquipmentSlotViewModel("Shield", "🛡"));
        Equipment.Add(new EquipmentSlotViewModel("Ring", "💍"));
        Equipment.Add(new EquipmentSlotViewModel("Amulet", "📿"));
    }

    /// <summary>
    /// Updates player information from provided values.
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <param name="className">Class name.</param>
    /// <param name="level">Player level.</param>
    public void UpdateCharacterInfo(string name, string className, int level)
    {
        Log.Debug("Updating character info: {Name} ({Class}) Level {Level}", name, className, level);
        PlayerName = name;
        ClassName = className;
        Level = level;
    }

    /// <summary>
    /// Updates the resource bar values.
    /// </summary>
    /// <param name="hp">Current HP.</param>
    /// <param name="maxHp">Maximum HP.</param>
    /// <param name="mp">Current MP.</param>
    /// <param name="maxMp">Maximum MP.</param>
    /// <param name="xp">Current XP.</param>
    /// <param name="xpToNext">XP required for next level.</param>
    public void UpdateResources(int hp, int maxHp, int mp, int maxMp, int xp, int xpToNext)
    {
        CurrentHp = hp;
        MaxHp = maxHp;
        CurrentMp = mp;
        MaxMp = maxMp;
        CurrentXp = xp;
        XpToNextLevel = xpToNext;
        Log.Debug("Resources updated: HP {Hp}/{MaxHp}, MP {Mp}/{MaxMp}, XP {Xp}/{XpToNext}",
            hp, maxHp, mp, maxMp, xp, xpToNext);
    }

    /// <summary>
    /// Updates a single stat value and recalculates its modifier.
    /// </summary>
    /// <param name="statName">The stat name.</param>
    /// <param name="value">The stat value.</param>
    public void UpdateStat(string statName, int value)
    {
        var modifier = CalculateModifier(value);

        switch (statName.ToLowerInvariant())
        {
            case "strength":
            case "str":
                Strength = value;
                StrengthModifier = modifier;
                break;
            case "dexterity":
            case "dex":
                Dexterity = value;
                DexterityModifier = modifier;
                break;
            case "constitution":
            case "con":
                Constitution = value;
                ConstitutionModifier = modifier;
                break;
            case "intelligence":
            case "int":
                Intelligence = value;
                IntelligenceModifier = modifier;
                break;
            case "wisdom":
            case "wis":
                Wisdom = value;
                WisdomModifier = modifier;
                break;
            case "charisma":
            case "cha":
                Charisma = value;
                CharismaModifier = modifier;
                break;
        }
    }

    /// <summary>
    /// Calculates the D&amp;D-style modifier for a stat value.
    /// </summary>
    /// <param name="statValue">The stat value.</param>
    /// <returns>The modifier ((value - 10) / 2).</returns>
    public static int CalculateModifier(int statValue)
    {
        return (statValue - 10) / 2;
    }

    /// <summary>
    /// Updates an equipment slot.
    /// </summary>
    /// <param name="slotIndex">The slot index (0-4).</param>
    /// <param name="itemName">The item name or null if empty.</param>
    /// <param name="itemBonus">The item bonus text.</param>
    public void UpdateEquipmentSlot(int slotIndex, string? itemName, string? itemBonus)
    {
        if (slotIndex < 0 || slotIndex >= Equipment.Count)
            return;

        var slot = Equipment[slotIndex];
        Equipment[slotIndex] = new EquipmentSlotViewModel(slot.SlotName, slot.SlotIcon, itemName, itemBonus);
        Log.Debug("Equipment slot {Index} updated: {Item}", slotIndex, itemName ?? "None");
    }

    /// <summary>
    /// Clears all player status to default values.
    /// </summary>
    public void Clear()
    {
        PlayerName = "Unknown";
        ClassName = "Adventurer";
        Level = 1;
        CurrentHp = 100;
        MaxHp = 100;
        CurrentMp = 0;
        MaxMp = 0;
        CurrentXp = 0;
        XpToNextLevel = 100;
        Strength = 10;
        StrengthModifier = 0;
        Dexterity = 10;
        DexterityModifier = 0;
        Constitution = 10;
        ConstitutionModifier = 0;
        Intelligence = 10;
        IntelligenceModifier = 0;
        Wisdom = 10;
        WisdomModifier = 0;
        Charisma = 10;
        CharismaModifier = 0;
        Gold = 0;
        LocationName = "Unknown";
        InitializeEquipmentSlots();
        Log.Debug("Player status cleared");
    }
}
