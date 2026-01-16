namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Defines the type of level-up bonus.
/// </summary>
public enum LevelUpBonusType
{
    /// <summary>
    /// A stat increased (e.g., Max Health +5).
    /// </summary>
    StatIncrease,

    /// <summary>
    /// A new ability was unlocked.
    /// </summary>
    AbilityUnlocked,

    /// <summary>
    /// A generic bonus with custom description.
    /// </summary>
    Other
}

/// <summary>
/// View model for displaying a level-up bonus in the combat summary.
/// </summary>
/// <remarks>
/// Formats different bonus types:
/// <list type="bullet">
///   <item><description>Stat increases: "Max Health: 45 → 50 (+5)"</description></item>
///   <item><description>Ability unlocks: "New Ability Unlocked: Shield Bash"</description></item>
/// </list>
/// </remarks>
public class LevelUpBonusViewModel
{
    /// <summary>
    /// Gets the type of bonus.
    /// </summary>
    public LevelUpBonusType Type { get; }

    /// <summary>
    /// Gets the formatted display text.
    /// </summary>
    public string DisplayText { get; }

    /// <summary>
    /// Creates a stat increase bonus.
    /// </summary>
    /// <param name="statName">Name of the stat.</param>
    /// <param name="oldValue">Previous value.</param>
    /// <param name="newValue">New value after level-up.</param>
    /// <returns>A new LevelUpBonusViewModel.</returns>
    public static LevelUpBonusViewModel StatIncrease(string statName, int oldValue, int newValue)
    {
        var change = newValue - oldValue;
        var text = $"{statName}: {oldValue} → {newValue} (+{change})";
        return new LevelUpBonusViewModel(LevelUpBonusType.StatIncrease, text);
    }

    /// <summary>
    /// Creates an ability unlock bonus.
    /// </summary>
    /// <param name="abilityName">Name of the unlocked ability.</param>
    /// <returns>A new LevelUpBonusViewModel.</returns>
    public static LevelUpBonusViewModel AbilityUnlocked(string abilityName)
    {
        var text = $"New Ability Unlocked: {abilityName}";
        return new LevelUpBonusViewModel(LevelUpBonusType.AbilityUnlocked, text);
    }

    /// <summary>
    /// Creates a custom bonus.
    /// </summary>
    /// <param name="description">Custom description text.</param>
    /// <returns>A new LevelUpBonusViewModel.</returns>
    public static LevelUpBonusViewModel Custom(string description)
    {
        return new LevelUpBonusViewModel(LevelUpBonusType.Other, description);
    }

    private LevelUpBonusViewModel(LevelUpBonusType type, string displayText)
    {
        Type = type;
        DisplayText = displayText;
    }
}
