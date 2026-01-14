namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides data for the status bar display.
/// </summary>
/// <remarks>
/// Implementations should fire <see cref="OnDataChanged"/> whenever any property
/// changes to trigger status bar refresh.
/// </remarks>
public interface IStatusBarDataProvider
{
    /// <summary>
    /// Gets current health points.
    /// </summary>
    (int Current, int Max) Health { get; }
    
    /// <summary>
    /// Gets current primary resource (MP, Rage, etc.).
    /// </summary>
    /// <remarks>
    /// Null if the player's class has no resource pool.
    /// </remarks>
    (int Current, int Max, string Name)? PrimaryResource { get; }
    
    /// <summary>
    /// Gets current XP progress.
    /// </summary>
    (int Current, int ToNext) Experience { get; }
    
    /// <summary>
    /// Gets current gold amount.
    /// </summary>
    int Gold { get; }
    
    /// <summary>
    /// Gets current location name.
    /// </summary>
    string Location { get; }
    
    /// <summary>
    /// Gets combat turn info if currently in combat.
    /// </summary>
    /// <remarks>
    /// Null when not in combat.
    /// </remarks>
    (int Round, bool IsPlayerTurn)? CombatInfo { get; }
    
    /// <summary>
    /// Event raised when any status bar data changes.
    /// </summary>
    event Action? OnDataChanged;
}
