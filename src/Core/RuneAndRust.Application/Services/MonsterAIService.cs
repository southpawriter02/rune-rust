using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for making AI decisions for monsters during combat.
/// </summary>
/// <remarks>
/// <para>MonsterAIService evaluates the combat situation and determines the optimal action
/// for a monster based on its <see cref="AIBehavior"/>.</para>
/// <para>Each behavior type has distinct priorities and target selection logic.</para>
/// </remarks>
public class MonsterAIService
{
    private readonly ILogger<MonsterAIService> _logger;
    private readonly Random _random;

    /// <summary>
    /// Creates a new MonsterAIService instance.
    /// </summary>
    /// <param name="logger">Logger for AI decision diagnostics.</param>
    /// <param name="random">Optional random source for testability. If null, uses default Random.</param>
    public MonsterAIService(ILogger<MonsterAIService> logger, Random? random = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? new Random();
    }

    /// <summary>
    /// Determines the action a monster should take based on its behavior and context.
    /// </summary>
    /// <param name="context">The AI decision context containing combat state.</param>
    /// <returns>The decided action with target and reasoning.</returns>
    public AIDecision DecideAction(AIContext context)
    {
        var behavior = context.Self.Behavior ?? AIBehavior.Aggressive;

        _logger.LogDebug(
            "AI deciding for {Monster} with behavior {Behavior}, HP: {HP:F0}%",
            context.Self.DisplayName,
            behavior,
            context.HealthPercentage * 100);

        var decision = behavior switch
        {
            AIBehavior.Aggressive => DecideAggressive(context),
            AIBehavior.Defensive => DecideDefensive(context),
            AIBehavior.Cowardly => DecideCowardly(context),
            AIBehavior.Support => DecideSupport(context),
            AIBehavior.Chaotic => DecideChaotic(context),
            _ => DecideAggressive(context)
        };

        _logger.LogInformation(
            "AI {Monster} decided: {Action} - {Reasoning}",
            context.Self.DisplayName,
            decision.Action,
            decision.Reasoning);

        return decision;
    }

    /// <summary>
    /// Builds an AI context for a monster combatant.
    /// </summary>
    /// <param name="monster">The monster making the decision.</param>
    /// <param name="encounter">The current combat encounter.</param>
    /// <returns>The AI context for decision-making.</returns>
    public AIContext BuildContext(Combatant monster, CombatEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(monster);
        ArgumentNullException.ThrowIfNull(encounter);

        var allies = encounter.GetAlliesFor(monster);
        var enemies = encounter.GetEnemiesForMonster();

        return new AIContext(
            monster,
            encounter,
            allies,
            enemies,
            encounter.RoundNumber);
    }

    private AIDecision DecideAggressive(AIContext context)
    {
        // Always attack, target lowest HP enemy
        var target = context.WeakestEnemy;

        if (target == null)
            return AIDecision.Wait("No targets available");

        return AIDecision.Attack(target,
            $"Aggressively targeting weakest enemy ({target.DisplayName})");
    }

    private AIDecision DecideDefensive(AIContext context)
    {
        // If health > 50%, attack normally
        if (!context.IsLowHealth)
        {
            var target = context.StrongestEnemy ?? context.WeakestEnemy;
            if (target != null)
                return AIDecision.Attack(target, "Health good, attacking");
        }

        // If can heal and low health, heal self
        if (context.Self.CanHeal)
        {
            return AIDecision.Heal(context.Self, null, "Low health, healing self");
        }

        // Otherwise defend
        return AIDecision.Defend("Low health, taking defensive stance");
    }

    private AIDecision DecideCowardly(AIContext context)
    {
        // If health < 30%, try to flee (50% chance)
        if (context.IsCriticalHealth && _random.NextDouble() < 0.5)
        {
            return AIDecision.Flee("Critically wounded, attempting to flee");
        }

        // Otherwise attack weakest target
        var target = context.WeakestEnemy;
        if (target != null)
            return AIDecision.Attack(target, "Attacking weakest target");

        return AIDecision.Wait("No targets available");
    }

    private AIDecision DecideSupport(AIContext context)
    {
        // Priority 1: Heal wounded allies
        if (context.Self.CanHeal)
        {
            var woundedAlly = context.WoundedAllies.FirstOrDefault();
            if (woundedAlly != null)
            {
                return AIDecision.Heal(woundedAlly, null,
                    $"Healing wounded ally {woundedAlly.DisplayName}");
            }

            // Heal self if low
            if (context.IsLowHealth)
            {
                return AIDecision.Heal(context.Self, null, "Healing self");
            }
        }

        // Priority 2: Attack if no one needs healing
        var target = context.StrongestEnemy;
        if (target != null)
            return AIDecision.Attack(target, "No healing needed, attacking");

        return AIDecision.Wait("Waiting for opportunity");
    }

    private AIDecision DecideChaotic(AIContext context)
    {
        var roll = _random.NextDouble();

        // 50% chance to attack
        if (roll < 0.5)
        {
            var target = context.GetRandomEnemy(_random);
            if (target != null)
                return AIDecision.Attack(target, "Chaotic attack!");
        }

        // 20% chance to defend
        if (roll < 0.7)
            return AIDecision.Defend("Chaotic defense!");

        // 15% chance to heal if able
        if (roll < 0.85 && context.Self.CanHeal)
            return AIDecision.Heal(context.Self, null, "Chaotic healing!");

        // 15% chance to do nothing
        return AIDecision.Wait("Chaotically doing nothing!");
    }
}
