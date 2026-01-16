namespace RuneAndRust.Presentation.Gui.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

/// <summary>
/// View model for the combat summary window.
/// </summary>
/// <remarks>
/// <para>
/// Displays end-of-combat information including:
/// <list type="bullet">
///   <item><description>Victory/defeat status</description></item>
///   <item><description>Enemies defeated</description></item>
///   <item><description>Rewards (XP, gold, loot)</description></item>
///   <item><description>Level-up notifications</description></item>
///   <item><description>Combat statistics</description></item>
/// </list>
/// </para>
/// </remarks>
public partial class CombatSummaryViewModel : ViewModelBase
{
    private readonly ILogger<CombatSummaryViewModel> _logger;

    /// <summary>
    /// Gets or sets whether the combat was a victory.
    /// </summary>
    [ObservableProperty]
    private bool _isVictory;

    /// <summary>
    /// Gets the collection of defeated enemy names.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _defeatedEnemies = new();

    /// <summary>
    /// Gets or sets the XP earned.
    /// </summary>
    [ObservableProperty]
    private int _xpEarned;

    /// <summary>
    /// Gets or sets the gold earned.
    /// </summary>
    [ObservableProperty]
    private int _goldEarned;

    /// <summary>
    /// Gets the collection of loot items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LootItemViewModel> _loot = new();

    /// <summary>
    /// Gets or sets the combat statistics.
    /// </summary>
    [ObservableProperty]
    private CombatStatisticsViewModel _statistics = new();

    /// <summary>
    /// Gets or sets whether the player leveled up.
    /// </summary>
    [ObservableProperty]
    private bool _didLevelUp;

    /// <summary>
    /// Gets or sets the new level after leveling up.
    /// </summary>
    [ObservableProperty]
    private int _newLevel;

    /// <summary>
    /// Gets the collection of level-up bonuses.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LevelUpBonusViewModel> _levelUpBonuses = new();

    /// <summary>
    /// Gets the header text based on victory/defeat.
    /// </summary>
    public string HeaderText => IsVictory ? "⚔ VICTORY! ⚔" : "💀 DEFEAT 💀";

    /// <summary>
    /// Gets the formatted XP earned text.
    /// </summary>
    public string XpText => $"✨ Experience: +{XpEarned:N0} XP";

    /// <summary>
    /// Gets the formatted gold earned text.
    /// </summary>
    public string GoldText => $"💰 Gold: +{GoldEarned:N0} gold";

    /// <summary>
    /// Gets the level-up notification text.
    /// </summary>
    public string LevelUpText => $"🎉 LEVEL UP! You are now Level {NewLevel}!";

    /// <summary>
    /// Gets whether there is loot to display.
    /// </summary>
    public bool HasLoot => Loot.Count > 0;

    /// <summary>
    /// Event raised when the summary window should close.
    /// </summary>
    public event Action? CloseRequested;

    /// <summary>
    /// Event raised when last save should be loaded.
    /// </summary>
    public event Action? LoadSaveRequested;

    /// <summary>
    /// Event raised when returning to main menu.
    /// </summary>
    public event Action? ReturnToMenuRequested;

    /// <summary>
    /// Event raised when retrying combat.
    /// </summary>
    public event Action? RetryRequested;

    /// <summary>
    /// Event raised when loot is collected.
    /// </summary>
    public event Action<IReadOnlyList<LootItemViewModel>>? LootCollected;

    /// <summary>
    /// Creates a new combat summary view model.
    /// </summary>
    /// <param name="logger">Logger for summary operations.</param>
    public CombatSummaryViewModel(ILogger<CombatSummaryViewModel> logger)
    {
        _logger = logger;
        _logger.LogDebug("CombatSummaryViewModel initialized");
    }

    /// <summary>
    /// Creates a new combat summary view model with default logger.
    /// </summary>
    /// <remarks>For design-time and testing scenarios.</remarks>
    public CombatSummaryViewModel() : this(CreateNullLogger())
    {
    }

    /// <summary>
    /// Loads a victory result into the summary.
    /// </summary>
    /// <param name="defeatedEnemies">List of defeated enemy names.</param>
    /// <param name="xpEarned">XP earned.</param>
    /// <param name="goldEarned">Gold earned.</param>
    /// <param name="statistics">Combat statistics.</param>
    public void LoadVictory(
        IEnumerable<string> defeatedEnemies,
        int xpEarned,
        int goldEarned,
        CombatStatisticsViewModel statistics)
    {
        IsVictory = true;
        XpEarned = xpEarned;
        GoldEarned = goldEarned;
        Statistics = statistics;

        DefeatedEnemies.Clear();
        foreach (var enemy in defeatedEnemies)
        {
            DefeatedEnemies.Add($"☠ {enemy}");
        }

        _logger.LogInformation(
            "Combat victory loaded: XP={Xp}, Gold={Gold}, Enemies={Count}",
            xpEarned,
            goldEarned,
            DefeatedEnemies.Count);

        OnPropertyChanged(nameof(HeaderText));
        OnPropertyChanged(nameof(XpText));
        OnPropertyChanged(nameof(GoldText));
        OnPropertyChanged(nameof(HasLoot));
    }

    /// <summary>
    /// Loads a defeat result into the summary.
    /// </summary>
    /// <param name="statistics">Combat statistics.</param>
    public void LoadDefeat(CombatStatisticsViewModel statistics)
    {
        IsVictory = false;
        Statistics = statistics;

        _logger.LogInformation("Combat defeat loaded");

        OnPropertyChanged(nameof(HeaderText));
    }

    /// <summary>
    /// Adds loot items to the summary.
    /// </summary>
    /// <param name="items">Loot items to add.</param>
    public void AddLoot(IEnumerable<LootItemViewModel> items)
    {
        foreach (var item in items)
        {
            Loot.Add(item);
        }

        _logger.LogDebug("Added {Count} loot items", Loot.Count);
        OnPropertyChanged(nameof(HasLoot));
    }

    /// <summary>
    /// Sets the level-up information.
    /// </summary>
    /// <param name="newLevel">New level reached.</param>
    /// <param name="bonuses">Level-up bonuses.</param>
    public void SetLevelUp(int newLevel, IEnumerable<LevelUpBonusViewModel> bonuses)
    {
        DidLevelUp = true;
        NewLevel = newLevel;

        LevelUpBonuses.Clear();
        foreach (var bonus in bonuses)
        {
            LevelUpBonuses.Add(bonus);
        }

        _logger.LogInformation("Level up to {Level} with {Count} bonuses", newLevel, LevelUpBonuses.Count);
        OnPropertyChanged(nameof(LevelUpText));
    }

    /// <summary>
    /// Collects all loot and adds to inventory.
    /// </summary>
    [RelayCommand]
    public void CollectAll()
    {
        if (Loot.Count == 0) return;

        var collected = Loot.ToList();
        LootCollected?.Invoke(collected);
        Loot.Clear();

        _logger.LogDebug("Collected {Count} loot items", collected.Count);
        OnPropertyChanged(nameof(HasLoot));
    }

    /// <summary>
    /// Continues after victory, closing the summary.
    /// </summary>
    [RelayCommand]
    public void Continue()
    {
        _logger.LogDebug("Continue requested - closing combat summary");
        CloseRequested?.Invoke();
    }

    /// <summary>
    /// Loads the last save after defeat.
    /// </summary>
    [RelayCommand]
    public void LoadSave()
    {
        _logger.LogDebug("Load save requested");
        LoadSaveRequested?.Invoke();
    }

    /// <summary>
    /// Returns to the main menu after defeat.
    /// </summary>
    [RelayCommand]
    public void ReturnToMenu()
    {
        _logger.LogDebug("Return to menu requested");
        ReturnToMenuRequested?.Invoke();
    }

    /// <summary>
    /// Retries the combat encounter.
    /// </summary>
    [RelayCommand]
    public void Retry()
    {
        _logger.LogDebug("Retry combat requested");
        RetryRequested?.Invoke();
    }

    private static ILogger<CombatSummaryViewModel> CreateNullLogger() =>
        Microsoft.Extensions.Logging.Abstractions.NullLogger<CombatSummaryViewModel>.Instance;
}
