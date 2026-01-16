namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Entities;
using System.Collections.ObjectModel;

/// <summary>
/// View model for a single combatant entry in the list.
/// </summary>
/// <remarks>
/// Displays name, HP bar, turn indicator, and status effects.
/// </remarks>
public partial class CombatantEntryViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the entity ID.
    /// </summary>
    public Guid EntityId { get; }

    /// <summary>
    /// Gets whether this is the player.
    /// </summary>
    public bool IsPlayer { get; }

    /// <summary>
    /// Gets whether this is an enemy.
    /// </summary>
    public bool IsEnemy { get; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _currentHp;

    [ObservableProperty]
    private int _maxHp;

    [ObservableProperty]
    private bool _isCurrentTurn;

    [ObservableProperty]
    private ObservableCollection<StatusEffectViewModel> _statusEffects = [];

    /// <summary>
    /// Gets the turn indicator character.
    /// </summary>
    public string TurnIndicator => IsCurrentTurn ? "►" : " ";

    /// <summary>
    /// Gets whether there are any status effects.
    /// </summary>
    public bool HasStatusEffects => StatusEffects.Count > 0;

    /// <summary>
    /// Creates a new CombatantEntryViewModel from a combatant.
    /// </summary>
    /// <param name="combatant">The combatant entity.</param>
    public CombatantEntryViewModel(Combatant combatant)
    {
        EntityId = combatant.Id;
        Name = combatant.DisplayName;
        CurrentHp = combatant.CurrentHealth;
        MaxHp = combatant.MaxHealth;
        IsPlayer = combatant.IsPlayer;
        IsEnemy = combatant.IsMonster;

        // Add defending status if active
        if (combatant.IsDefending)
        {
            StatusEffects.Add(new StatusEffectViewModel("🛡", "Defending", "Taking defensive stance"));
        }
    }

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    /// <param name="name">Combatant name.</param>
    /// <param name="hp">Current HP.</param>
    /// <param name="maxHp">Maximum HP.</param>
    /// <param name="isPlayer">Whether this is the player.</param>
    /// <param name="isCurrentTurn">Whether this is the current turn.</param>
    public CombatantEntryViewModel(string name, int hp, int maxHp, bool isPlayer, bool isCurrentTurn)
    {
        EntityId = Guid.NewGuid();
        Name = name;
        CurrentHp = hp;
        MaxHp = maxHp;
        IsPlayer = isPlayer;
        IsEnemy = !isPlayer;
        IsCurrentTurn = isCurrentTurn;

        if (isPlayer)
        {
            StatusEffects.Add(new StatusEffectViewModel("🛡", "Defending", "Taking defensive stance"));
        }
    }

    partial void OnIsCurrentTurnChanged(bool value)
    {
        OnPropertyChanged(nameof(TurnIndicator));
    }
}

/// <summary>
/// View model for a status effect icon.
/// </summary>
/// <remarks>
/// Displays icon, name, and description tooltip.
/// </remarks>
public class StatusEffectViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the icon character.
    /// </summary>
    public string Icon { get; }

    /// <summary>
    /// Gets the effect name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description for tooltip.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Creates a new StatusEffectViewModel.
    /// </summary>
    /// <param name="icon">Icon character.</param>
    /// <param name="name">Effect name.</param>
    /// <param name="description">Description for tooltip.</param>
    public StatusEffectViewModel(string icon, string name, string description)
    {
        Icon = icon;
        Name = name;
        Description = description;
    }
}
