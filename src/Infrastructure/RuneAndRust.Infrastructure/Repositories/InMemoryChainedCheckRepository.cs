using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of <see cref="IChainedCheckRepository"/>.
/// </summary>
/// <remarks>
/// <para>
/// Provides thread-safe storage for active chained check states during gameplay.
/// States are not persisted across application restarts.
/// </para>
/// </remarks>
public sealed class InMemoryChainedCheckRepository : IChainedCheckRepository
{
    private readonly ConcurrentDictionary<string, ChainedCheckState> _states = new();
    private readonly ILogger<InMemoryChainedCheckRepository> _logger;

    /// <summary>
    /// Creates a new in-memory chained check repository.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public InMemoryChainedCheckRepository(ILogger<InMemoryChainedCheckRepository> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void Add(ChainedCheckState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (_states.TryAdd(state.CheckId, state))
        {
            _logger.LogDebug(
                "Added chained check {CheckId} for character {CharacterId}: {ChainName}",
                state.CheckId, state.CharacterId, state.ChainName);
        }
        else
        {
            _logger.LogWarning(
                "Failed to add chained check {CheckId} - already exists",
                state.CheckId);
        }
    }

    /// <inheritdoc />
    public void Update(ChainedCheckState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _states[state.CheckId] = state;

        _logger.LogDebug(
            "Updated chained check {CheckId}: Step {StepIndex}, Status {Status}",
            state.CheckId, state.CurrentStepIndex, state.Status);
    }

    /// <inheritdoc />
    public ChainedCheckState? GetById(string checkId)
    {
        if (string.IsNullOrWhiteSpace(checkId))
            return null;

        return _states.TryGetValue(checkId, out var state) ? state : null;
    }

    /// <inheritdoc />
    public IReadOnlyList<ChainedCheckState> GetActiveByCharacterId(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return Array.Empty<ChainedCheckState>();

        return _states.Values
            .Where(s => s.CharacterId.Equals(characterId, StringComparison.OrdinalIgnoreCase))
            .Where(s => !s.IsComplete)
            .ToList();
    }

    /// <inheritdoc />
    public bool Remove(string checkId)
    {
        if (string.IsNullOrWhiteSpace(checkId))
            return false;

        var removed = _states.TryRemove(checkId, out var state);

        if (removed)
        {
            _logger.LogDebug(
                "Removed chained check {CheckId} (final status: {Status})",
                checkId, state?.Status);
        }

        return removed;
    }

    /// <inheritdoc />
    public int RemoveAllForCharacter(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return 0;

        var toRemove = _states.Values
            .Where(s => s.CharacterId.Equals(characterId, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.CheckId)
            .ToList();

        var removed = 0;
        foreach (var checkId in toRemove)
        {
            if (_states.TryRemove(checkId, out _))
                removed++;
        }

        if (removed > 0)
        {
            _logger.LogDebug(
                "Removed {Count} chained checks for character {CharacterId}",
                removed, characterId);
        }

        return removed;
    }
}
