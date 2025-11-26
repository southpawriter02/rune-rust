using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.5: Progression Controller
/// Manages milestone (level-up) workflow including attribute allocation and ability unlocks.
/// Coordinates with SagaService for progression mechanics.
/// </summary>
public class ProgressionController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly SagaService _sagaService;
    private int _startingPP;
    private int _attributePointsSpent;
    private int _abilityPointsSpent;

    /// <summary>
    /// Event raised when progression is complete.
    /// </summary>
    public event EventHandler? ProgressionComplete;

    /// <summary>
    /// Event raised when an attribute is increased.
    /// </summary>
    public event EventHandler<string>? AttributeIncreased;

    /// <summary>
    /// Event raised when an ability is unlocked or ranked up.
    /// </summary>
    public event EventHandler<string>? AbilityAdvanced;

    public ProgressionController(
        ILogger logger,
        GameStateController gameStateController,
        INavigationService navigationService,
        SagaService sagaService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _sagaService = sagaService ?? throw new ArgumentNullException(nameof(sagaService));
    }

    /// <summary>
    /// Gets the player's current Progression Points.
    /// </summary>
    public int CurrentPP => _gameStateController.HasActiveGame
        ? _gameStateController.CurrentGameState.Player?.ProgressionPoints ?? 0
        : 0;

    /// <summary>
    /// Gets the current milestone level.
    /// </summary>
    public int CurrentMilestone => _gameStateController.HasActiveGame
        ? _gameStateController.CurrentGameState.Player?.CurrentMilestone ?? 0
        : 0;

    /// <summary>
    /// Gets the Legend required for next milestone.
    /// </summary>
    public int LegendToNextMilestone => _gameStateController.HasActiveGame
        ? _gameStateController.CurrentGameState.Player?.LegendToNextMilestone ?? 0
        : 0;

    /// <summary>
    /// Gets whether a milestone can be reached.
    /// </summary>
    public bool CanReachMilestone => _gameStateController.HasActiveGame
        && _gameStateController.CurrentGameState.Player != null
        && _sagaService.CanReachMilestone(_gameStateController.CurrentGameState.Player);

    /// <summary>
    /// Initializes the progression screen for milestone advancement.
    /// Called by LootController when milestone is reached.
    /// </summary>
    public async Task InitializeMilestoneProgressionAsync()
    {
        if (!_gameStateController.HasActiveGame)
        {
            _logger.Error("Cannot initialize progression without active game");
            return;
        }

        var player = _gameStateController.CurrentGameState.Player;
        if (player == null)
        {
            _logger.Error("Cannot initialize progression without player");
            return;
        }

        if (!_sagaService.CanReachMilestone(player))
        {
            _logger.Warning("Player cannot reach milestone yet");
            return;
        }

        // Reach the milestone (grants PP, HP, Stamina)
        _sagaService.ReachMilestone(player);
        _startingPP = player.ProgressionPoints;
        _attributePointsSpent = 0;
        _abilityPointsSpent = 0;

        _logger.Information(
            "Milestone reached! New Milestone={Milestone}, PP={PP}, MaxHP={MaxHP}, MaxStamina={MaxStamina}",
            player.CurrentMilestone, player.ProgressionPoints, player.MaxHP, player.MaxStamina);

        // Transition to progression phase
        await _gameStateController.UpdatePhaseAsync(Core.GamePhase.CharacterProgression, "Milestone reached");

        // Navigate to character sheet/specialization view for point allocation
        _navigationService.NavigateTo<SpecializationTreeViewModel>();
    }

    /// <summary>
    /// Spends a Progression Point to increase an attribute.
    /// </summary>
    public bool SpendPPOnAttribute(string attributeName)
    {
        if (!_gameStateController.HasActiveGame) return false;

        var player = _gameStateController.CurrentGameState.Player;
        if (player == null) return false;

        if (_sagaService.SpendPPOnAttribute(player, attributeName))
        {
            _attributePointsSpent++;
            _logger.Information("PP spent on {Attribute}, remaining PP={PP}",
                attributeName, player.ProgressionPoints);

            AttributeIncreased?.Invoke(this, attributeName);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Advances an ability to the next rank.
    /// </summary>
    public bool AdvanceAbilityRank(Ability ability)
    {
        if (!_gameStateController.HasActiveGame) return false;

        var player = _gameStateController.CurrentGameState.Player;
        if (player == null) return false;

        if (_sagaService.AdvanceAbilityRank(player, ability))
        {
            _abilityPointsSpent += ability.CostToRank2;
            _logger.Information("Ability ranked up: {AbilityName} to Rank {Rank}, remaining PP={PP}",
                ability.Name, ability.CurrentRank, player.ProgressionPoints);

            AbilityAdvanced?.Invoke(this, ability.Name);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Unlocks a specialization for the player.
    /// </summary>
    public bool UnlockSpecialization(Specialization specialization)
    {
        if (!_gameStateController.HasActiveGame) return false;

        var player = _gameStateController.CurrentGameState.Player;
        if (player == null) return false;

        if (_sagaService.UnlockSpecialization(player, specialization))
        {
            _logger.Information("Specialization unlocked: {Specialization}, remaining PP={PP}",
                specialization, player.ProgressionPoints);

            AbilityAdvanced?.Invoke(this, $"Specialization: {specialization}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the available attributes that can be increased (not at cap).
    /// </summary>
    public List<(string Name, int CurrentValue, bool CanIncrease)> GetAttributeOptions()
    {
        var player = _gameStateController.CurrentGameState.Player;
        if (player == null) return new List<(string, int, bool)>();

        const int AttributeCap = 6;
        var attributes = new List<(string, int, bool)>
        {
            ("MIGHT", player.Attributes.Might, player.Attributes.Might < AttributeCap),
            ("FINESSE", player.Attributes.Finesse, player.Attributes.Finesse < AttributeCap),
            ("WITS", player.Attributes.Wits, player.Attributes.Wits < AttributeCap),
            ("WILL", player.Attributes.Will, player.Attributes.Will < AttributeCap),
            ("STURDINESS", player.Attributes.Sturdiness, player.Attributes.Sturdiness < AttributeCap)
        };

        return attributes;
    }

    /// <summary>
    /// Gets the abilities that can be ranked up.
    /// </summary>
    public List<Ability> GetRankableAbilities()
    {
        var player = _gameStateController.CurrentGameState.Player;
        if (player == null) return new List<Ability>();

        // Return abilities that can still be ranked up (rank 1 to rank 2)
        return player.Abilities
            .Where(a => a.CurrentRank < 2 && player.ProgressionPoints >= a.CostToRank2)
            .ToList();
    }

    /// <summary>
    /// Gets the available specializations for the player's class.
    /// </summary>
    public List<Specialization> GetAvailableSpecializations()
    {
        var player = _gameStateController.CurrentGameState.Player;
        if (player == null || player.Specialization != Specialization.None)
        {
            return new List<Specialization>();
        }

        // Return specializations valid for the player's class
        return player.Class switch
        {
            CharacterClass.Warrior => new List<Specialization>
            {
                Specialization.SkarHordeAspirant,
                Specialization.IronBane,
                Specialization.AtgeirWielder
            },
            CharacterClass.Mystic => new List<Specialization>
            {
                Specialization.VardWarden,
                Specialization.RustWitch
            },
            _ => new List<Specialization>()
        };
    }

    /// <summary>
    /// Completes progression and returns to exploration.
    /// </summary>
    public async Task CompleteProgressionAsync()
    {
        if (!_gameStateController.HasActiveGame)
        {
            _logger.Error("Cannot complete progression without active game");
            return;
        }

        var player = _gameStateController.CurrentGameState.Player;
        _logger.Information(
            "Progression complete. PP spent: Attributes={AttrPP}, Abilities={AbilityPP}, Remaining={PP}",
            _attributePointsSpent, _abilityPointsSpent, player?.ProgressionPoints ?? 0);

        // Recalculate derived stats
        if (player != null)
        {
            RecalculateDerivedStats(player);
        }

        // Transition back to exploration
        await _gameStateController.UpdatePhaseAsync(Core.GamePhase.DungeonExploration, "Progression complete");
        _navigationService.NavigateTo<DungeonExplorationViewModel>();

        ProgressionComplete?.Invoke(this, EventArgs.Empty);

        // Reset tracking
        _startingPP = 0;
        _attributePointsSpent = 0;
        _abilityPointsSpent = 0;
    }

    /// <summary>
    /// Skips progression without spending points.
    /// </summary>
    public async Task SkipProgressionAsync()
    {
        _logger.Information("Progression skipped - points saved for later");
        await CompleteProgressionAsync();
    }

    /// <summary>
    /// Recalculates derived stats after attribute changes.
    /// </summary>
    private void RecalculateDerivedStats(PlayerCharacter player)
    {
        // HP is based on Sturdiness: 20 + (STURDINESS * 5) + milestone bonuses
        int baseSturdinessHP = 20 + (player.Attributes.Sturdiness * 5);
        int milestoneHP = player.CurrentMilestone * 10;
        player.MaxHP = baseSturdinessHP + milestoneHP;

        // Stamina is based on Finesse: 20 + (FINESSE * 3) + milestone bonuses
        int baseFinesseStamina = 20 + (player.Attributes.Finesse * 3);
        int milestoneStamina = player.CurrentMilestone * 5;
        player.MaxStamina = baseFinesseStamina + milestoneStamina;

        // Aether Pool for Mystics is based on Will
        if (player.Class == CharacterClass.Mystic)
        {
            player.MaxAP = 10 + (player.Attributes.Will * 2);
        }

        // Ensure current values don't exceed new maximums
        player.HP = Math.Min(player.HP, player.MaxHP);
        player.Stamina = Math.Min(player.Stamina, player.MaxStamina);
        player.AP = Math.Min(player.AP, player.MaxAP);

        _logger.Debug("Derived stats recalculated: MaxHP={MaxHP}, MaxStamina={MaxStamina}, MaxAP={MaxAP}",
            player.MaxHP, player.MaxStamina, player.MaxAP);
    }

    /// <summary>
    /// Gets a summary of the progression session.
    /// </summary>
    public string GetProgressionSummary()
    {
        var player = _gameStateController.CurrentGameState.Player;
        if (player == null) return "No active character";

        var parts = new List<string>
        {
            $"Milestone {player.CurrentMilestone}",
            $"{player.ProgressionPoints} PP available"
        };

        if (_attributePointsSpent > 0)
        {
            parts.Add($"+{_attributePointsSpent} attributes");
        }

        if (_abilityPointsSpent > 0)
        {
            parts.Add($"+{_abilityPointsSpent} ability ranks");
        }

        return string.Join(" | ", parts);
    }
}
