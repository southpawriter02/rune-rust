namespace RuneAndRust.Infrastructure.Repositories;

using System.Collections.Concurrent;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// In-memory implementation of the fumble consequence repository.
/// </summary>
public sealed class InMemoryFumbleConsequenceRepository : IFumbleConsequenceRepository
{
    private readonly ConcurrentDictionary<string, FumbleConsequence> _consequences = new();

    /// <inheritdoc />
    public void Add(FumbleConsequence consequence)
    {
        if (consequence == null)
            throw new ArgumentNullException(nameof(consequence));

        if (!_consequences.TryAdd(consequence.ConsequenceId, consequence))
        {
            throw new InvalidOperationException(
                $"Consequence with ID {consequence.ConsequenceId} already exists.");
        }
    }

    /// <inheritdoc />
    public void Update(FumbleConsequence consequence)
    {
        if (consequence == null)
            throw new ArgumentNullException(nameof(consequence));

        _consequences[consequence.ConsequenceId] = consequence;
    }

    /// <inheritdoc />
    public FumbleConsequence? GetById(string consequenceId)
    {
        _consequences.TryGetValue(consequenceId, out var consequence);
        return consequence;
    }

    /// <inheritdoc />
    public IReadOnlyList<FumbleConsequence> GetActiveByCharacter(string characterId)
    {
        return _consequences.Values
            .Where(c => c.CharacterId == characterId && c.IsActive)
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<FumbleConsequence> GetAllByCharacter(string characterId)
    {
        return _consequences.Values
            .Where(c => c.CharacterId == characterId)
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<FumbleConsequence> GetAllActive()
    {
        return _consequences.Values
            .Where(c => c.IsActive)
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<FumbleConsequence> GetByTarget(string targetId)
    {
        return _consequences.Values
            .Where(c => c.TargetId == targetId)
            .ToList();
    }

    /// <inheritdoc />
    public void Remove(string consequenceId)
    {
        _consequences.TryRemove(consequenceId, out _);
    }

    /// <inheritdoc />
    public IReadOnlyList<FumbleConsequence> GetExpired(DateTime asOfTime)
    {
        return _consequences.Values
            .Where(c => c.IsActive && c.IsExpired(asOfTime))
            .ToList();
    }
}
