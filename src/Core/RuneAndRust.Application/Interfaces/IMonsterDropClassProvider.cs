using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to monster-to-drop-class mappings.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the source of monster drop class data, allowing
/// it to be loaded from configuration files, databases, or hardcoded defaults.
/// </para>
/// <para>
/// Implementations should cache loaded mappings for performance and provide
/// sensible defaults for unknown monster types.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get the drop class for a specific monster
/// var dropClass = provider.GetDropClass("dragon-boss");
/// Console.WriteLine($"Drop class: {dropClass}"); // Boss
/// 
/// // Get the full mapping including drop count
/// var mapping = provider.GetMapping("elite-warden");
/// Console.WriteLine($"Drops {mapping.DropCount} items at {mapping.DropClass} tier");
/// </code>
/// </example>
public interface IMonsterDropClassProvider
{
    /// <summary>
    /// Gets the drop class for a specific monster type.
    /// </summary>
    /// <param name="monsterTypeId">The monster type identifier (case-insensitive).</param>
    /// <returns>The EnemyDropClass for the monster, or Standard if not mapped.</returns>
    /// <remarks>
    /// Returns <see cref="EnemyDropClass.Standard"/> for unknown monster types.
    /// </remarks>
    EnemyDropClass GetDropClass(string monsterTypeId);

    /// <summary>
    /// Gets the drop count for a specific monster type.
    /// </summary>
    /// <param name="monsterTypeId">The monster type identifier (case-insensitive).</param>
    /// <returns>The number of items the monster drops, or 1 if not mapped.</returns>
    /// <remarks>
    /// Returns 1 for unknown monster types.
    /// </remarks>
    int GetDropCount(string monsterTypeId);

    /// <summary>
    /// Gets the full mapping for a specific monster type.
    /// </summary>
    /// <param name="monsterTypeId">The monster type identifier (case-insensitive).</param>
    /// <returns>
    /// The MonsterDropClassMapping for the monster, or 
    /// <see cref="MonsterDropClassMapping.Default"/> if not mapped.
    /// </returns>
    MonsterDropClassMapping GetMapping(string monsterTypeId);

    /// <summary>
    /// Gets all configured monster drop class mappings.
    /// </summary>
    /// <returns>A read-only list of all monster drop class mappings.</returns>
    IReadOnlyList<MonsterDropClassMapping> GetAllMappings();

    /// <summary>
    /// Checks whether a mapping exists for a specific monster type.
    /// </summary>
    /// <param name="monsterTypeId">The monster type identifier (case-insensitive).</param>
    /// <returns>True if a mapping is configured, false otherwise.</returns>
    bool HasMapping(string monsterTypeId);
}
