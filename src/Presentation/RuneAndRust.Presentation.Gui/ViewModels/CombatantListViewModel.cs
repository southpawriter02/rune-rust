namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Entities;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// View model for the combatant list panel.
/// </summary>
/// <remarks>
/// Displays all combatants in initiative order with HP bars and status effects.
/// The current turn combatant is highlighted.
/// </remarks>
public partial class CombatantListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<CombatantEntryViewModel> _combatants = [];

    [ObservableProperty]
    private Guid _currentTurnEntityId;

    /// <summary>
    /// Design-time constructor with sample data.
    /// </summary>
    public CombatantListViewModel()
    {
        // Sample data for design-time
        AddSampleCombatants();
    }

    /// <summary>
    /// Refreshes the combatant list from a list of combatants.
    /// </summary>
    /// <param name="combatants">The list of combatants sorted by initiative.</param>
    public void RefreshCombatants(IEnumerable<Combatant> combatants)
    {
        Combatants.Clear();

        foreach (var combatant in combatants.OrderByDescending(c => c.Initiative))
        {
            var entry = new CombatantEntryViewModel(combatant)
            {
                IsCurrentTurn = combatant.Id == CurrentTurnEntityId
            };
            Combatants.Add(entry);
        }

        Log.Debug("Combatant list refreshed: {Count} combatants", Combatants.Count);
    }

    /// <summary>
    /// Handles a turn change.
    /// </summary>
    /// <param name="currentCombatant">The combatant whose turn it is now.</param>
    public void HandleTurnChanged(Combatant currentCombatant)
    {
        CurrentTurnEntityId = currentCombatant.Id;

        foreach (var entry in Combatants)
        {
            entry.IsCurrentTurn = entry.EntityId == CurrentTurnEntityId;
        }

        Log.Debug("Turn changed to {Name}", currentCombatant.DisplayName);
    }

    /// <summary>
    /// Clears the combatant list.
    /// </summary>
    public void Clear()
    {
        Combatants.Clear();
        CurrentTurnEntityId = Guid.Empty;
    }

    private void AddSampleCombatants()
    {
        Combatants.Add(new CombatantEntryViewModel("Hero", 65, 100, true, true));
        Combatants.Add(new CombatantEntryViewModel("Skeleton", 15, 45, false, false));
        Combatants.Add(new CombatantEntryViewModel("Goblin", 25, 25, false, false));
        Combatants.Add(new CombatantEntryViewModel("Goblin Archer", 12, 20, false, false));
    }
}
