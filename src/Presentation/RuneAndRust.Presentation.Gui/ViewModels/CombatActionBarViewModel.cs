namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.Services;
using Serilog;

/// <summary>
/// View model for the combat action bar.
/// </summary>
/// <remarks>
/// Displays turn info and provides action buttons for combat.
/// Integrates with IGridInteractionService for targeting modes.
/// </remarks>
public partial class CombatActionBarViewModel : ViewModelBase
{
    private IGridInteractionService? _gridInteraction;

    [ObservableProperty]
    private int _currentRound = 1;

    [ObservableProperty]
    private bool _isPlayerTurn = true;

    [ObservableProperty]
    private bool _hasAction = true;

    [ObservableProperty]
    private bool _hasBonus = true;

    [ObservableProperty]
    private int _remainingMovement = 4;

    [ObservableProperty]
    private int _maxMovement = 4;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AttackCommand))]
    private bool _canAttack = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveCommand))]
    private bool _canMove = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AbilityCommand))]
    private bool _canUseAbility = true;

    /// <summary>
    /// Gets the turn indicator text.
    /// </summary>
    public string TurnText => IsPlayerTurn ? "YOUR TURN" : "Enemy Turn";

    /// <summary>
    /// Gets the action point indicator.
    /// </summary>
    public string ActionIndicator => HasAction ? "✓" : "✗";

    /// <summary>
    /// Gets the bonus action indicator.
    /// </summary>
    public string BonusIndicator => HasBonus ? "✓" : "✗";

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public CombatActionBarViewModel()
    {
    }

    /// <summary>
    /// Sets the grid interaction service.
    /// </summary>
    /// <param name="service">The grid interaction service.</param>
    public void SetGridInteractionService(IGridInteractionService service)
    {
        _gridInteraction = service;
        _gridInteraction.OnTargetingComplete += HandleTargetingComplete;
    }

    /// <summary>
    /// Sets the current round number.
    /// </summary>
    /// <param name="round">The round number.</param>
    public void SetRound(int round)
    {
        CurrentRound = round;
    }

    /// <summary>
    /// Updates the view model for a new turn.
    /// </summary>
    /// <param name="isPlayerTurn">Whether it's the player's turn.</param>
    /// <param name="movement">Remaining movement points.</param>
    /// <param name="maxMovement">Maximum movement points.</param>
    public void SetTurnState(bool isPlayerTurn, int movement, int maxMovement)
    {
        IsPlayerTurn = isPlayerTurn;
        HasAction = true;
        HasBonus = true;
        RemainingMovement = movement;
        MaxMovement = maxMovement;

        UpdateButtonStates();

        OnPropertyChanged(nameof(TurnText));
        OnPropertyChanged(nameof(ActionIndicator));
        OnPropertyChanged(nameof(BonusIndicator));
    }

    [RelayCommand(CanExecute = nameof(CanAttack))]
    private void Attack()
    {
        // Would normally get player position and range from combat service
        // For now, just enters attack mode with defaults
        Log.Information("Attack button pressed");
    }

    [RelayCommand(CanExecute = nameof(CanMove))]
    private void Move()
    {
        // Would normally get player position and movement from combat service
        Log.Information("Move button pressed");
    }

    [RelayCommand(CanExecute = nameof(CanUseAbility))]
    private void Ability()
    {
        // Opens ability selection panel (simplified for now)
        Log.Information("Ability button pressed");
    }

    [RelayCommand]
    private void Defend()
    {
        HasAction = false;
        UpdateButtonStates();
        OnPropertyChanged(nameof(ActionIndicator));
        Log.Information("Player took defensive stance");
    }

    [RelayCommand]
    private void UseItem()
    {
        // Opens item selection panel (simplified for now)
        Log.Information("Item button pressed");
    }

    [RelayCommand]
    private void Wait()
    {
        Log.Information("Player chose to wait");
    }

    [RelayCommand]
    private void EndTurn()
    {
        Log.Information("Player ended turn");
    }

    private void HandleTargetingComplete(TargetingResult result)
    {
        if (result.Success)
        {
            switch (result.Mode)
            {
                case GridInteractionMode.Attack:
                    HasAction = false;
                    break;
                case GridInteractionMode.Movement:
                    // Update remaining movement based on path cost
                    break;
            }

            UpdateButtonStates();
            OnPropertyChanged(nameof(ActionIndicator));
        }
    }

    private void UpdateButtonStates()
    {
        CanAttack = IsPlayerTurn && HasAction;
        CanMove = IsPlayerTurn && RemainingMovement > 0;
        CanUseAbility = IsPlayerTurn && HasAction;
    }
}
