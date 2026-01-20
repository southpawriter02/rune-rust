using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for displaying initiative roll results.
/// </summary>
/// <param name="Name">The combatant's display name.</param>
/// <param name="RollValue">The dice roll value (before modifier).</param>
/// <param name="Modifier">The initiative modifier applied.</param>
/// <param name="Total">The total initiative value.</param>
/// <param name="IsPlayer">Whether this is the player.</param>
public record CombatantInitiativeDto(
    string Name,
    int RollValue,
    int Modifier,
    int Total,
    bool IsPlayer)
{
    /// <summary>
    /// Creates a DTO from a Combatant.
    /// </summary>
    public static CombatantInitiativeDto FromCombatant(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        return new CombatantInitiativeDto(
            combatant.DisplayName,
            combatant.InitiativeRoll.RollValue,
            combatant.InitiativeRoll.Modifier,
            combatant.Initiative,
            combatant.IsPlayer);
    }
}

/// <summary>
/// DTO for turn order display.
/// </summary>
/// <param name="Name">The combatant's display name.</param>
/// <param name="Initiative">The combatant's total initiative.</param>
/// <param name="IsCurrentTurn">Whether this combatant has the current turn.</param>
/// <param name="IsPlayer">Whether this is the player.</param>
/// <param name="IsDefeated">Whether this combatant is defeated.</param>
public record TurnOrderEntryDto(
    string Name,
    int Initiative,
    bool IsCurrentTurn,
    bool IsPlayer,
    bool IsDefeated)
{
    /// <summary>
    /// Creates a DTO from a Combatant.
    /// </summary>
    public static TurnOrderEntryDto FromCombatant(Combatant combatant, bool isCurrentTurn)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        return new TurnOrderEntryDto(
            combatant.DisplayName,
            combatant.Initiative,
            isCurrentTurn,
            combatant.IsPlayer,
            !combatant.IsActive);
    }
}

/// <summary>
/// DTO for enemy status display in targeting.
/// </summary>
/// <param name="Number">The 1-based targeting number.</param>
/// <param name="DisplayName">The enemy's display name.</param>
/// <param name="CurrentHealth">Current health points.</param>
/// <param name="MaxHealth">Maximum health points.</param>
/// <param name="IsDefeated">Whether the enemy is defeated.</param>
public record EnemyStatusDto(
    int Number,
    string DisplayName,
    int CurrentHealth,
    int MaxHealth,
    bool IsDefeated)
{
    /// <summary>
    /// Creates a DTO from a Combatant.
    /// </summary>
    public static EnemyStatusDto FromCombatant(Combatant combatant, int number)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        return new EnemyStatusDto(
            number,
            combatant.DisplayName,
            combatant.CurrentHealth,
            combatant.MaxHealth,
            !combatant.IsActive);
    }
}

/// <summary>
/// DTO for current combat state display.
/// </summary>
/// <param name="RoundNumber">The current round number.</param>
/// <param name="TurnOrder">Ordered list of combatants by initiative.</param>
/// <param name="CurrentTurnIndex">Index of current turn in TurnOrder.</param>
/// <param name="Enemies">List of enemies with targeting info.</param>
/// <param name="IsPlayerTurn">Whether it is the player's turn.</param>
public record CombatStateDisplayDto(
    int RoundNumber,
    IReadOnlyList<TurnOrderEntryDto> TurnOrder,
    int CurrentTurnIndex,
    IReadOnlyList<EnemyStatusDto> Enemies,
    bool IsPlayerTurn)
{
    /// <summary>
    /// Creates a DTO from a CombatEncounter.
    /// </summary>
    public static CombatStateDisplayDto FromEncounter(CombatEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);

        var turnOrder = encounter.Combatants
            .Select((c, i) => TurnOrderEntryDto.FromCombatant(c, i == encounter.CurrentTurnIndex))
            .ToList();

        var enemies = encounter.GetActiveMonsters()
            .Select((c, i) => EnemyStatusDto.FromCombatant(c, i + 1))
            .ToList();

        return new CombatStateDisplayDto(
            encounter.RoundNumber,
            turnOrder,
            encounter.CurrentTurnIndex,
            enemies,
            encounter.IsPlayerTurn);
    }
}

/// <summary>
/// DTO for combat end result display.
/// </summary>
/// <param name="EndState">The final combat state.</param>
/// <param name="RoundsElapsed">Total rounds of combat.</param>
/// <param name="MonstersDefeated">Number of monsters defeated.</param>
public record CombatEndResultDto(
    CombatState EndState,
    int RoundsElapsed,
    int MonstersDefeated)
{
    /// <summary>
    /// Creates a DTO from a CombatEncounter.
    /// </summary>
    public static CombatEndResultDto FromEncounter(CombatEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        return new CombatEndResultDto(
            encounter.State,
            encounter.RoundNumber,
            encounter.Combatants.Count(c => c.IsMonster && !c.IsActive));
    }
}
