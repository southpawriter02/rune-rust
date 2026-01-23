// ------------------------------------------------------------------------------
// <copyright file="InMemoryExtendedInfluenceRepository.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// In-memory implementation of the extended influence repository for
// development and testing purposes.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.Repositories;

using System.Collections.Concurrent;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// In-memory implementation of the extended influence repository.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses a ConcurrentDictionary for thread-safe operations
/// and is suitable for development and testing. For production use, consider
/// implementing a persistent storage backend (e.g., SQL, document database).
/// </para>
/// <para>
/// The repository supports efficient queries by:
/// <list type="bullet">
///   <item><description>ID (O(1) lookup)</description></item>
///   <item><description>Character ID (O(n) scan)</description></item>
///   <item><description>Target ID (O(n) scan)</description></item>
///   <item><description>Character + Target + Belief (O(n) scan, but unique)</description></item>
///   <item><description>Status (O(n) scan)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class InMemoryExtendedInfluenceRepository : IExtendedInfluenceRepository
{
    /// <summary>
    /// Thread-safe dictionary storing influence entities by ID.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, ExtendedInfluence> _influences = new();

    /// <inheritdoc/>
    public void Add(ExtendedInfluence influence)
    {
        ArgumentNullException.ThrowIfNull(influence);

        if (!_influences.TryAdd(influence.Id, influence))
        {
            throw new InvalidOperationException(
                $"Influence with ID {influence.Id} already exists.");
        }
    }

    /// <inheritdoc/>
    public ExtendedInfluence? GetById(Guid influenceId)
    {
        _influences.TryGetValue(influenceId, out var influence);
        return influence;
    }

    /// <inheritdoc/>
    public ExtendedInfluence? GetByCharacterTargetAndBelief(
        string characterId,
        string targetId,
        string beliefId)
    {
        return _influences.Values
            .FirstOrDefault(i =>
                i.CharacterId == characterId &&
                i.TargetId == targetId &&
                i.BeliefId == beliefId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetByCharacterAndTarget(
        string characterId,
        string targetId)
    {
        return _influences.Values
            .Where(i => i.CharacterId == characterId && i.TargetId == targetId)
            .OrderByDescending(i => i.LastInteractionAt ?? i.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetByCharacter(string characterId)
    {
        return _influences.Values
            .Where(i => i.CharacterId == characterId)
            .OrderByDescending(i => i.LastInteractionAt ?? i.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetActiveByCharacter(string characterId)
    {
        return _influences.Values
            .Where(i => i.CharacterId == characterId &&
                       (i.Status == InfluenceStatus.Active || i.Status == InfluenceStatus.Stalled))
            .OrderByDescending(i => i.LastInteractionAt ?? i.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetByTarget(string targetId)
    {
        return _influences.Values
            .Where(i => i.TargetId == targetId)
            .OrderByDescending(i => i.LastInteractionAt ?? i.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetByStatus(InfluenceStatus status)
    {
        return _influences.Values
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.LastInteractionAt ?? i.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetStalledByCharacter(string characterId)
    {
        return _influences.Values
            .Where(i => i.CharacterId == characterId && i.Status == InfluenceStatus.Stalled)
            .OrderByDescending(i => i.LastInteractionAt ?? i.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetSuccessfulByCharacter(string characterId)
    {
        return _influences.Values
            .Where(i => i.CharacterId == characterId && i.Status == InfluenceStatus.Successful)
            .OrderByDescending(i => i.ConvictionChangedAt ?? i.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public void Update(ExtendedInfluence influence)
    {
        ArgumentNullException.ThrowIfNull(influence);

        if (!_influences.ContainsKey(influence.Id))
        {
            throw new InvalidOperationException(
                $"Influence with ID {influence.Id} does not exist.");
        }

        _influences[influence.Id] = influence;
    }

    /// <inheritdoc/>
    public void Save(ExtendedInfluence influence)
    {
        ArgumentNullException.ThrowIfNull(influence);

        // AddOrUpdate pattern: adds if new, updates if exists
        _influences[influence.Id] = influence;
    }

    /// <inheritdoc/>
    public bool Delete(Guid influenceId)
    {
        return _influences.TryRemove(influenceId, out _);
    }

    /// <inheritdoc/>
    public int DeleteByCharacter(string characterId)
    {
        var toRemove = _influences.Values
            .Where(i => i.CharacterId == characterId)
            .Select(i => i.Id)
            .ToList();

        var removed = 0;
        foreach (var id in toRemove)
        {
            if (_influences.TryRemove(id, out _))
            {
                removed++;
            }
        }

        return removed;
    }

    /// <inheritdoc/>
    public int DeleteByTarget(string targetId)
    {
        var toRemove = _influences.Values
            .Where(i => i.TargetId == targetId)
            .Select(i => i.Id)
            .ToList();

        var removed = 0;
        foreach (var id in toRemove)
        {
            if (_influences.TryRemove(id, out _))
            {
                removed++;
            }
        }

        return removed;
    }

    /// <inheritdoc/>
    public int Count()
    {
        return _influences.Count;
    }

    /// <inheritdoc/>
    public int CountByStatus(InfluenceStatus status)
    {
        return _influences.Values.Count(i => i.Status == status);
    }

    /// <inheritdoc/>
    public bool Exists(string characterId, string targetId, string beliefId)
    {
        return _influences.Values.Any(i =>
            i.CharacterId == characterId &&
            i.TargetId == targetId &&
            i.BeliefId == beliefId);
    }

    /// <summary>
    /// Clears all influences from the repository.
    /// </summary>
    /// <remarks>
    /// Useful for testing scenarios where a clean state is needed.
    /// </remarks>
    public void Clear()
    {
        _influences.Clear();
    }

    /// <summary>
    /// Gets all influences in the repository.
    /// </summary>
    /// <returns>All stored influences.</returns>
    /// <remarks>
    /// Useful for debugging and administrative purposes.
    /// </remarks>
    public IReadOnlyList<ExtendedInfluence> GetAll()
    {
        return _influences.Values
            .OrderByDescending(i => i.LastInteractionAt ?? i.CreatedAt)
            .ToList();
    }
}
