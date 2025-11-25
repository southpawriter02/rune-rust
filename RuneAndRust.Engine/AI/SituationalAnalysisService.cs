using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for analyzing the current battlefield situation.
/// Provides tactical context for AI decision-making.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public class SituationalAnalysisService : ISituationalAnalysisService
{
    private readonly ILogger<SituationalAnalysisService> _logger;
    private readonly CoverService? _coverService;

    public SituationalAnalysisService(
        ILogger<SituationalAnalysisService> logger,
        CoverService? coverService = null)
    {
        _logger = logger;
        _coverService = coverService;
    }

    /// <inheritdoc/>
    public SituationalContext AnalyzeSituation(Enemy actor, BattlefieldState state)
    {
        var context = new SituationalContext();

        // ===== NUMERICAL ADVANTAGE =====
        var livingAllies = state.Enemies.Count(e => e.IsAlive);
        var livingEnemies = state.PlayerCharacters.Count(c => c.IsAlive);

        context.AllyCount = livingAllies;
        context.EnemyCount = livingEnemies;
        context.IsOutnumbered = livingEnemies > livingAllies;

        // ===== HEALTH STATUS =====
        context.SelfHPPercent = actor.MaxHP > 0 ? (float)actor.HP / actor.MaxHP : 0f;
        context.IsLowHP = context.SelfHPPercent < 0.3f;
        context.IsCriticalHP = context.SelfHPPercent < 0.15f;

        // Ally HP analysis
        var livingAlliesList = state.Enemies.Where(e => e.IsAlive).ToList();
        if (livingAlliesList.Any())
        {
            var allyHPPercentages = livingAlliesList
                .Select(e => e.MaxHP > 0 ? (float)e.HP / e.MaxHP : 0f)
                .ToList();

            context.AverageAllyHP = allyHPPercentages.Average();
            context.HasCriticalAllies = allyHPPercentages.Any(hp => hp < 0.2f);
        }

        // ===== POSITIONING =====
        if (state.Grid != null)
        {
            context.IsFlanked = IsFlanked(actor, state.Grid, state.PlayerCharacters);
            context.HasHighGround = HasHighGround(actor, state.Grid, state.PlayerCharacters);
            context.IsInCover = actor.Position.HasValue &&
                state.Grid.GetTile(actor.Position.Value)?.Cover != CoverType.None;
            context.IsIsolated = IsIsolated(actor, state.Grid, state.Enemies);
        }

        // ===== COMBAT PHASE =====
        context.TurnNumber = state.CurrentTurn;
        context.IsEarlyGame = context.TurnNumber <= 3;
        context.IsMidGame = context.TurnNumber > 3 && context.TurnNumber <= 8;
        context.IsLateGame = context.TurnNumber > 8;

        // ===== OVERALL ASSESSMENT =====
        context.Advantage = CalculateAdvantage(context);
        context.Summary = GenerateSummary(context);

        _logger.LogDebug(
            "Situation Analysis: Enemy {EnemyId} | Allies={Allies} Enemies={Enemies} | " +
            "HP={HP:P0} | Flanked={Flanked} HighGround={HighGround} | Advantage={Advantage}",
            actor.Id, context.AllyCount, context.EnemyCount,
            context.SelfHPPercent, context.IsFlanked, context.HasHighGround, context.Advantage);

        return context;
    }

    /// <inheritdoc/>
    public TacticalAdvantage CalculateAdvantage(SituationalContext context)
    {
        int advantageScore = 0;

        // Numerical advantage (+2/-2)
        if (!context.IsOutnumbered)
            advantageScore += 2;
        else
            advantageScore -= 2;

        // HP advantage (+1/-2)
        if (context.SelfHPPercent > 0.7f)
            advantageScore += 1;
        if (context.SelfHPPercent < 0.3f)
            advantageScore -= 2;

        // Positional advantage (+1/-2/+1)
        if (context.HasHighGround)
            advantageScore += 1;
        if (context.IsFlanked)
            advantageScore -= 2;
        if (context.IsInCover)
            advantageScore += 1;

        // Ally status (-1 if allies in trouble)
        if (context.HasCriticalAllies)
            advantageScore -= 1;

        // Determine advantage tier
        if (advantageScore >= 3)
            return TacticalAdvantage.Strong;
        if (advantageScore >= 1)
            return TacticalAdvantage.Slight;
        if (advantageScore >= -1)
            return TacticalAdvantage.Neutral;

        return TacticalAdvantage.Disadvantaged;
    }

    /// <inheritdoc/>
    public bool IsFlanked(Enemy actor, BattlefieldGrid? grid, List<PlayerCharacter> playerCharacters)
    {
        if (grid == null || !actor.Position.HasValue)
            return false;

        var actorPos = actor.Position.Value;
        var livingPlayers = playerCharacters.Where(p => p.IsAlive && p.Position.HasValue).ToList();

        if (livingPlayers.Count < 2)
            return false; // Need at least 2 enemies to flank

        // Get directions to each player
        var directions = new List<string>();
        foreach (var player in livingPlayers)
        {
            var direction = GetDirection(actorPos, player.Position!.Value);
            if (direction != null)
                directions.Add(direction);
        }

        // Check if players are on opposite sides
        // Simplified: check if we have players in opposite columns or rows
        var hasLeft = directions.Contains("Left");
        var hasRight = directions.Contains("Right");
        var hasFront = directions.Contains("Front");
        var hasBack = directions.Contains("Back");

        return (hasLeft && hasRight) || (hasFront && hasBack);
    }

    /// <inheritdoc/>
    public bool HasHighGround(Enemy actor, BattlefieldGrid? grid, List<PlayerCharacter> playerCharacters)
    {
        if (grid == null || !actor.Position.HasValue)
            return false;

        var actorElevation = actor.Position.Value.Elevation;
        var livingPlayers = playerCharacters
            .Where(p => p.IsAlive && p.Position.HasValue)
            .ToList();

        if (!livingPlayers.Any())
            return false;

        // Has high ground if elevated above ALL enemies
        return livingPlayers.All(p => actorElevation > p.Position!.Value.Elevation);
    }

    /// <inheritdoc/>
    public bool IsIsolated(Enemy actor, BattlefieldGrid? grid, List<Enemy> allies)
    {
        if (grid == null || !actor.Position.HasValue)
            return false;

        var livingAllies = allies
            .Where(e => e.IsAlive && e.Id != actor.Id && e.Position.HasValue)
            .ToList();

        if (!livingAllies.Any())
            return true; // No allies = isolated

        // Check if any ally is within 2 tiles (Manhattan distance)
        var actorPos = actor.Position.Value;
        foreach (var ally in livingAllies)
        {
            var distance = GetManhattanDistance(actorPos, ally.Position!.Value);
            if (distance <= 2)
                return false; // Ally nearby, not isolated
        }

        return true;
    }

    // ===== HELPER METHODS =====

    private string? GetDirection(GridPosition from, GridPosition to)
    {
        // Simplified direction calculation
        if (from.Zone != to.Zone)
            return from.Zone == Zone.Player ? "Front" : "Back";

        if (from.Column < to.Column)
            return "Right";
        if (from.Column > to.Column)
            return "Left";

        return null;
    }

    private int GetManhattanDistance(GridPosition a, GridPosition b)
    {
        // Simple Manhattan distance (row + column differences)
        int rowDist = a.Row == b.Row ? 0 : 1;
        int colDist = Math.Abs(a.Column - b.Column);
        int zoneDist = a.Zone == b.Zone ? 0 : 1;

        return rowDist + colDist + zoneDist;
    }

    private string GenerateSummary(SituationalContext context)
    {
        var parts = new List<string>();

        // Numerical status
        if (context.IsOutnumbered)
            parts.Add("outnumbered");
        else if (context.AllyCount > context.EnemyCount)
            parts.Add("numerical advantage");

        // HP status
        if (context.IsCriticalHP)
            parts.Add("critically wounded");
        else if (context.IsLowHP)
            parts.Add("wounded");

        // Positioning
        if (context.IsFlanked)
            parts.Add("flanked");
        if (context.HasHighGround)
            parts.Add("high ground");
        if (context.IsInCover)
            parts.Add("in cover");

        if (!parts.Any())
            return "Standard combat situation";

        return string.Join(", ", parts);
    }
}
