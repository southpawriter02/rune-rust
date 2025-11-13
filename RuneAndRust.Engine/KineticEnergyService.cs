using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20: Service responsible for managing Kinetic Energy (KE) resource
/// KE is built by movement and decays when stationary
/// </summary>
public class KineticEnergyService
{
    private static readonly ILogger _log = Log.ForContext<KineticEnergyService>();

    private const int KE_DECAY_AMOUNT = 10; // KE lost per turn if not moving

    /// <summary>
    /// Called at the start of a combatant's turn to decay KE if they didn't move last turn
    /// </summary>
    public void OnTurnStart(object combatant)
    {
        switch (combatant)
        {
            case PlayerCharacter player:
                HandlePlayerTurnStart(player);
                break;

            case Enemy enemy:
                HandleEnemyTurnStart(enemy);
                break;
        }
    }

    private void HandlePlayerTurnStart(PlayerCharacter player)
    {
        // Decay KE if player didn't move last turn
        if (!player.HasMovedThisTurn && player.KineticEnergy > 0)
        {
            int decay = KE_DECAY_AMOUNT;
            int newKE = Math.Max(0, player.KineticEnergy - decay);
            player.KineticEnergy = newKE;

            _log.Information("KE decay: Player={PlayerName}, Decay={Decay}, Remaining={KE}",
                player.Name, decay, newKE);
        }

        // Reset movement tracking for new turn
        player.HasMovedThisTurn = false;
        player.TilesMovedThisTurn = 0;
    }

    private void HandleEnemyTurnStart(Enemy enemy)
    {
        // Decay KE if enemy didn't move last turn
        if (!enemy.HasMovedThisTurn && enemy.KineticEnergy > 0)
        {
            int decay = KE_DECAY_AMOUNT;
            int newKE = Math.Max(0, enemy.KineticEnergy - decay);
            enemy.KineticEnergy = newKE;

            _log.Debug("KE decay: Enemy={EnemyName}, Decay={Decay}, Remaining={KE}",
                enemy.Name, decay, newKE);
        }

        // Reset movement tracking for new turn
        enemy.HasMovedThisTurn = false;
        enemy.TilesMovedThisTurn = 0;
    }

    /// <summary>
    /// Checks if a combatant has enough KE to spend
    /// </summary>
    public bool CanSpendKE(object combatant, int cost)
    {
        var currentKE = combatant switch
        {
            PlayerCharacter player => player.KineticEnergy,
            Enemy enemy => enemy.KineticEnergy,
            _ => 0
        };

        return currentKE >= cost;
    }

    /// <summary>
    /// Spends KE from a combatant (for mobility abilities)
    /// </summary>
    public bool TrySpendKE(object combatant, int cost)
    {
        if (!CanSpendKE(combatant, cost))
            return false;

        switch (combatant)
        {
            case PlayerCharacter player:
                player.KineticEnergy -= cost;
                _log.Information("KE spent: Player={PlayerName}, Cost={Cost}, Remaining={Remaining}",
                    player.Name, cost, player.KineticEnergy);
                return true;

            case Enemy enemy:
                enemy.KineticEnergy -= cost;
                _log.Debug("KE spent: Enemy={EnemyName}, Cost={Cost}, Remaining={Remaining}",
                    enemy.Name, cost, enemy.KineticEnergy);
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Grants KE to a combatant (for abilities that generate momentum)
    /// </summary>
    public void GrantKE(object combatant, int amount)
    {
        switch (combatant)
        {
            case PlayerCharacter player:
                int newPlayerKE = Math.Min(player.KineticEnergy + amount, player.MaxKineticEnergy);
                player.KineticEnergy = newPlayerKE;
                _log.Information("KE granted: Player={PlayerName}, Amount={Amount}, Total={Total}",
                    player.Name, amount, newPlayerKE);
                break;

            case Enemy enemy:
                int newEnemyKE = Math.Min(enemy.KineticEnergy + amount, enemy.MaxKineticEnergy);
                enemy.KineticEnergy = newEnemyKE;
                _log.Debug("KE granted: Enemy={EnemyName}, Amount={Amount}, Total={Total}",
                    enemy.Name, amount, newEnemyKE);
                break;
        }
    }

    /// <summary>
    /// Gets the current KE amount for a combatant
    /// </summary>
    public int GetCurrentKE(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => player.KineticEnergy,
            Enemy enemy => enemy.KineticEnergy,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the maximum KE capacity for a combatant
    /// </summary>
    public int GetMaxKE(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => player.MaxKineticEnergy,
            Enemy enemy => enemy.MaxKineticEnergy,
            _ => 0
        };
    }

    /// <summary>
    /// Checks if a combatant is at maximum KE
    /// </summary>
    public bool IsAtMaxKE(object combatant)
    {
        return GetCurrentKE(combatant) >= GetMaxKE(combatant);
    }

    /// <summary>
    /// Resets KE to 0 (called at end of combat)
    /// </summary>
    public void ResetKE(object combatant)
    {
        switch (combatant)
        {
            case PlayerCharacter player:
                player.KineticEnergy = 0;
                player.HasMovedThisTurn = false;
                player.TilesMovedThisTurn = 0;
                break;

            case Enemy enemy:
                enemy.KineticEnergy = 0;
                enemy.HasMovedThisTurn = false;
                enemy.TilesMovedThisTurn = 0;
                break;
        }
    }
}
