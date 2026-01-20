using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for generating branching dungeon paths.
/// </summary>
/// <remarks>
/// Determines how rooms connect to create varied dungeon layouts with
/// main paths, side paths, dead ends, and optional loops.
/// </remarks>
public class BranchGeneratorService : IBranchGeneratorService
{
    private readonly ISeededRandomService _random;
    private readonly BranchRules _rules;
    private readonly ILogger<BranchGeneratorService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public BranchGeneratorService(
        ISeededRandomService random,
        BranchRules? rules = null,
        ILogger<BranchGeneratorService>? logger = null,
        IGameEventLogger? eventLogger = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _rules = rules ?? BranchRules.Default;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<BranchGeneratorService>.Instance;
        _eventLogger = eventLogger;

        _logger.LogDebug(
            "BranchGeneratorService initialized: mainPath={MainPath}, sidePath={SidePath}, deadEnd={DeadEnd}, loop={Loop}",
            _rules.MainPathProbability,
            _rules.SidePathProbability,
            _rules.DeadEndProbability,
            _rules.LoopProbability);
    }

    /// <inheritdoc/>
    public BranchDecision DecideBranching(
        Position3D position,
        IEnumerable<Direction> potentialExits,
        Func<Position3D, bool> hasRoomAt)
    {
        var decisions = new Dictionary<Direction, BranchType>();
        var exits = potentialExits.ToList();
        var hasMainPath = false;

        _logger.LogDebug(
            "Deciding branching at {Position} with {ExitCount} potential exits",
            position, exits.Count);

        foreach (var direction in exits)
        {
            var targetPos = position.Move(direction);
            var contextKey = $"branch_{direction}";
            var roll = _random.NextFloatForPosition(position, contextKey);

            // Check if this would create a loop
            var wouldLoop = hasRoomAt(targetPos);

            if (wouldLoop)
            {
                // Decide whether to allow the loop connection
                if (roll < _rules.LoopProbability)
                {
                    decisions[direction] = BranchType.Loop;
                    _logger.LogDebug(
                        "Loop connection at {Direction} (roll={Roll:F2} < {Prob:F2})",
                        direction, roll, _rules.LoopProbability);
                }
                else
                {
                    decisions[direction] = BranchType.None;
                    _logger.LogDebug(
                        "Blocked loop at {Direction} (roll={Roll:F2} >= {Prob:F2})",
                        direction, roll, _rules.LoopProbability);
                }
                continue;
            }

            // Main path vs side path vs dead end decision
            var branchType = DetermineBranchType(roll, ref hasMainPath);
            decisions[direction] = branchType;

            _logger.LogDebug(
                "Direction {Direction}: {BranchType} (roll={Roll:F2})",
                direction, branchType, roll);
        }

        // Ensure at least one continuation if not at a forced dead end
        if (!decisions.Values.Any(b => b == BranchType.MainPath || b == BranchType.SidePath))
        {
            var fallback = exits.FirstOrDefault();
            if (fallback != default)
            {
                decisions[fallback] = BranchType.MainPath;
                _logger.LogDebug(
                    "Forced MainPath at {Direction} to ensure continuation",
                    fallback);
            }
        }

        var isDeadEnd = decisions.Values.All(b =>
            b == BranchType.None || b == BranchType.DeadEnd);

        return new BranchDecision
        {
            Position = position,
            ExitDecisions = decisions,
            IsDeadEnd = isDeadEnd
        };
    }

    /// <inheritdoc/>
    public DeadEndContent GenerateDeadEndContent(Position3D position, DifficultyRating difficulty)
    {
        var roll = _random.NextFloatForPosition(position, "deadend_type");
        var chances = _rules.DeadEndChances;

        var content = roll switch
        {
            _ when roll < chances.TreasureCache => DeadEndContent.TreasureCache,
            _ when roll < chances.MonsterLair => DeadEndContent.MonsterLair,
            _ when roll < chances.SecretShrine => DeadEndContent.SecretShrine,
            _ when roll < chances.TrapRoom => DeadEndContent.TrapRoom,
            _ => DeadEndContent.Empty
        };

        _logger.LogDebug(
            "Dead end at {Position} contains {Content} (roll={Roll:F2})",
            position, content, roll);

        _eventLogger?.LogSystem("DeadEndGenerated", $"Dead end with {content} at {position}",
            data: new Dictionary<string, object>
            {
                ["position"] = position.ToString(),
                ["content"] = content.ToString(),
                ["difficulty"] = difficulty.ToString()
            });

        return content;
    }

    /// <summary>
    /// Determines the branch type based on roll and main path status.
    /// </summary>
    private BranchType DetermineBranchType(float roll, ref bool hasMainPath)
    {
        // Main path: Ensure at least one main continuation
        if (!hasMainPath && roll < _rules.MainPathProbability)
        {
            hasMainPath = true;
            return BranchType.MainPath;
        }

        // Side path
        if (roll < _rules.MainPathProbability + _rules.SidePathProbability)
        {
            return BranchType.SidePath;
        }

        // Dead end
        if (roll < _rules.MainPathProbability + _rules.SidePathProbability + _rules.DeadEndProbability)
        {
            return BranchType.DeadEnd;
        }

        // No exit
        return BranchType.None;
    }
}
