namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to enemy drop probability tables.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the source of drop probability data, allowing
/// it to be loaded from configuration files, databases, or hardcoded defaults.
/// </para>
/// <para>
/// Implementations should cache loaded tables for performance and validate
/// that all enemy drop classes have corresponding probability entries.
/// </para>
/// </remarks>
public interface IDropTableProvider
{
    /// <summary>
    /// Gets the drop probability entry for a specific enemy class.
    /// </summary>
    /// <param name="enemyClass">The enemy drop class to get probabilities for.</param>
    /// <returns>The DropProbabilityEntry for the specified class.</returns>
    /// <exception cref="ArgumentException">Thrown when entry for class is not found.</exception>
    DropProbabilityEntry GetDropEntry(EnemyDropClass enemyClass);

    /// <summary>
    /// Rolls for a drop from the specified enemy class.
    /// </summary>
    /// <param name="enemyClass">The enemy drop class.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>The drop roll result.</returns>
    /// <exception cref="ArgumentException">Thrown when entry for class is not found.</exception>
    DropRollResult RollDrop(EnemyDropClass enemyClass, Random random);

    /// <summary>
    /// Gets all drop probability entries, ordered by enemy class.
    /// </summary>
    /// <returns>A read-only list of all drop probability entries.</returns>
    IReadOnlyList<DropProbabilityEntry> GetAllEntries();

    /// <summary>
    /// Validates that all required enemy classes have probability entries.
    /// </summary>
    /// <returns>True if all classes are configured, false otherwise.</returns>
    bool ValidateConfiguration();

    /// <summary>
    /// Gets the highest tier that can drop from a specific enemy class.
    /// </summary>
    /// <param name="enemyClass">The enemy drop class.</param>
    /// <returns>The highest quality tier with non-zero probability.</returns>
    QualityTier GetHighestPossibleTier(EnemyDropClass enemyClass);

    /// <summary>
    /// Gets whether an enemy class can result in no drop.
    /// </summary>
    /// <param name="enemyClass">The enemy drop class.</param>
    /// <returns>True if the class has a non-zero no-drop chance.</returns>
    bool CanDropNothing(EnemyDropClass enemyClass);
}
