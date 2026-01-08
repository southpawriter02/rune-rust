using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for rolling and managing combat initiative.
/// </summary>
/// <remarks>
/// <para>Initiative determines turn order in combat. Higher initiative acts first.</para>
/// <para>Formula: 1d10 + modifier (Finesse for players, InitiativeModifier for monsters).</para>
/// <para>Ties are broken by highest Finesse/InitiativeModifier value.</para>
/// </remarks>
public class InitiativeService
{
    private readonly DiceService _diceService;
    private readonly ILogger<InitiativeService> _logger;

    /// <summary>
    /// Creates a new InitiativeService instance.
    /// </summary>
    /// <param name="diceService">The dice service for rolling initiative.</param>
    /// <param name="logger">The logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown if diceService or logger is null.</exception>
    public InitiativeService(DiceService diceService, ILogger<InitiativeService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("InitiativeService initialized");
    }

    /// <summary>
    /// Rolls initiative for a player.
    /// </summary>
    /// <param name="player">The player to roll for.</param>
    /// <returns>The initiative roll result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if player is null.</exception>
    public InitiativeRoll RollForPlayer(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var pool = DicePool.D10(); // 1d10 for initiative
        var result = _diceService.Roll(pool);
        var modifier = player.Attributes.Finesse;

        var initiative = new InitiativeRoll(result, modifier);

        _logger.LogDebug(
            "Player {Name} rolls initiative: {Roll}",
            player.Name, initiative.ToDisplayString());

        return initiative;
    }

    /// <summary>
    /// Rolls initiative for a monster.
    /// </summary>
    /// <param name="monster">The monster to roll for.</param>
    /// <returns>The initiative roll result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if monster is null.</exception>
    public InitiativeRoll RollForMonster(Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);

        var pool = DicePool.D10(); // 1d10 for initiative
        var result = _diceService.Roll(pool);
        var modifier = monster.InitiativeModifier;

        var initiative = new InitiativeRoll(result, modifier);

        _logger.LogDebug(
            "Monster {Name} rolls initiative: {Roll}",
            monster.Name, initiative.ToDisplayString());

        return initiative;
    }

    /// <summary>
    /// Creates a fully configured combat encounter with all participants and rolled initiative.
    /// </summary>
    /// <param name="player">The player entering combat.</param>
    /// <param name="monsters">The monsters in the room.</param>
    /// <param name="roomId">The room where combat occurs.</param>
    /// <param name="previousRoomId">The room the player came from (for flee destination).</param>
    /// <returns>A combat encounter with all combatants added and initiative rolled.</returns>
    /// <exception cref="ArgumentNullException">Thrown if player or monsters is null.</exception>
    /// <remarks>
    /// <para>Monsters of the same type are automatically numbered (e.g., "Goblin 1", "Goblin 2").</para>
    /// <para>Unique monster types are not numbered.</para>
    /// <para>The encounter is returned in NotStarted state - call Start() to begin.</para>
    /// </remarks>
    public CombatEncounter CreateEncounter(
        Player player,
        IEnumerable<Monster> monsters,
        Guid roomId,
        Guid? previousRoomId)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(monsters);

        var encounter = CombatEncounter.Create(roomId, previousRoomId);

        // Add player with rolled initiative
        var playerInitiative = RollForPlayer(player);
        encounter.AddCombatant(Combatant.ForPlayer(player, playerInitiative));

        // Count monsters by type for numbering
        var monsterList = monsters.ToList();
        var typeCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var typeOccurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // First pass: count occurrences of each type
        foreach (var monster in monsterList)
        {
            var typeKey = monster.MonsterDefinitionId ?? monster.Name;
            typeCounts.TryGetValue(typeKey, out var count);
            typeCounts[typeKey] = count + 1;
        }

        // Second pass: add monsters with appropriate numbering
        foreach (var monster in monsterList)
        {
            var typeKey = monster.MonsterDefinitionId ?? monster.Name;
            typeOccurrences.TryGetValue(typeKey, out var occurrence);
            occurrence++;
            typeOccurrences[typeKey] = occurrence;

            var monsterInitiative = RollForMonster(monster);

            // Only number if there are multiple of this type
            var displayNumber = typeCounts[typeKey] > 1 ? occurrence : 0;

            encounter.AddCombatant(Combatant.ForMonster(monster, monsterInitiative, displayNumber));
        }

        _logger.LogInformation(
            "Created combat encounter with {PlayerCount} player and {MonsterCount} monsters in room {RoomId}",
            1, monsterList.Count, roomId);

        return encounter;
    }
}
