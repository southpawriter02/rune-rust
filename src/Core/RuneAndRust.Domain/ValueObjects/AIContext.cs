using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides context for AI decision-making during a monster's turn.
/// </summary>
/// <remarks>
/// <para>AIContext aggregates all information needed for the AI to make decisions:</para>
/// <list type="bullet">
/// <item>The monster making the decision (<see cref="Self"/>)</item>
/// <item>The current combat encounter state</item>
/// <item>Lists of active allies and enemies</item>
/// <item>Current round number</item>
/// </list>
/// <para>Computed properties provide convenient access to common decision factors.</para>
/// </remarks>
public readonly record struct AIContext(
    Combatant Self,
    CombatEncounter Encounter,
    IReadOnlyList<Combatant> Allies,
    IReadOnlyList<Combatant> Enemies,
    int RoundNumber)
{
    /// <summary>
    /// Gets the monster's current health percentage (0.0 to 1.0).
    /// </summary>
    public float HealthPercentage => Self.MaxHealth > 0
        ? (float)Self.CurrentHealth / Self.MaxHealth
        : 0f;

    /// <summary>
    /// Gets whether the monster is below 50% health.
    /// </summary>
    public bool IsLowHealth => HealthPercentage < 0.5f;

    /// <summary>
    /// Gets whether the monster is below 30% health (critical).
    /// </summary>
    public bool IsCriticalHealth => HealthPercentage < 0.3f;

    /// <summary>
    /// Gets whether there are any active allies.
    /// </summary>
    public bool HasAllies => Allies.Count > 0;

    /// <summary>
    /// Gets allies that are below 50% health.
    /// </summary>
    public IEnumerable<Combatant> WoundedAllies =>
        Allies.Where(a => a.MaxHealth > 0 && (float)a.CurrentHealth / a.MaxHealth < 0.5f);

    /// <summary>
    /// Gets the enemy with the lowest current health.
    /// </summary>
    public Combatant? WeakestEnemy =>
        Enemies.Where(e => e.IsActive).OrderBy(e => e.CurrentHealth).FirstOrDefault();

    /// <summary>
    /// Gets the enemy with the highest current health.
    /// </summary>
    public Combatant? StrongestEnemy =>
        Enemies.Where(e => e.IsActive).OrderByDescending(e => e.CurrentHealth).FirstOrDefault();

    /// <summary>
    /// Gets a random enemy from the active enemies list.
    /// </summary>
    /// <param name="random">Random number generator.</param>
    /// <returns>A random enemy, or null if none available.</returns>
    public Combatant? GetRandomEnemy(Random random)
    {
        var activeEnemies = Enemies.Where(e => e.IsActive).ToList();
        return activeEnemies.Count > 0
            ? activeEnemies[random.Next(activeEnemies.Count)]
            : null;
    }
}
