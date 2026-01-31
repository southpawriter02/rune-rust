// ═══════════════════════════════════════════════════════════════════════════════
// InMemoryPlayerRepository.cs
// In-memory implementation of IPlayerRepository for development, testing, and
// scenarios where database persistence is not yet configured. Stores Player
// entities in a thread-safe ConcurrentDictionary keyed by player ID. Tracks
// creation timestamps for ordering in GetMostRecentAsync and GetAllAsync.
// Enforces case-insensitive name uniqueness across all saved characters.
// Data is lost when the application stops.
// Version: 0.17.5g
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Infrastructure.Repositories;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// In-memory implementation of <see cref="IPlayerRepository"/> for development and testing.
/// </summary>
/// <remarks>
/// <para>
/// This repository stores <see cref="Player"/> entities in memory using a thread-safe
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>. Data is lost when the application
/// stops. This implementation is suitable for development, testing, and scenarios where
/// persistence across application restarts is not required.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All operations are thread-safe via
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>. Name uniqueness checks use
/// case-insensitive string comparison.
/// </para>
/// <para>
/// <strong>Ordering:</strong> A separate timestamp dictionary tracks when each player
/// was saved. <see cref="GetMostRecentAsync"/> and <see cref="GetAllAsync"/> order
/// results by this timestamp (most recent first).
/// </para>
/// <para>
/// <strong>Future:</strong> This implementation will be replaced by an EF Core-based
/// <c>PlayerRepository</c> when full database persistence is implemented.
/// </para>
/// </remarks>
/// <seealso cref="IPlayerRepository"/>
/// <seealso cref="InMemoryGameRepository"/>
public class InMemoryPlayerRepository : IPlayerRepository
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Thread-safe storage for player entities, keyed by player ID.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Player> _players = new();

    /// <summary>
    /// Tracks when each player was saved, for ordering in GetMostRecentAsync/GetAllAsync.
    /// Keyed by player ID, value is the UTC timestamp of the most recent save/update.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, DateTime> _timestamps = new();

    /// <summary>
    /// Logger for repository operations and diagnostics.
    /// </summary>
    private readonly ILogger<InMemoryPlayerRepository> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // ERROR MESSAGES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Error when a character with the same name already exists.</summary>
    private const string ErrorNameExists = "A character named '{0}' already exists.";

    /// <summary>Error when an update is attempted on a non-existent player.</summary>
    private const string ErrorPlayerNotFound = "Player with ID '{0}' not found.";

    /// <summary>Error when a generic save failure occurs.</summary>
    private const string ErrorSaveFailed = "Failed to save character. Please try again.";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new in-memory player repository instance.
    /// </summary>
    /// <param name="logger">
    /// Optional logger for diagnostics. If null, a no-op logger is used.
    /// </param>
    public InMemoryPlayerRepository(ILogger<InMemoryPlayerRepository>? logger = null)
    {
        _logger = logger ?? NullLogger<InMemoryPlayerRepository>.Instance;
        _logger.LogDebug("InMemoryPlayerRepository initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IPlayerRepository IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Task<SaveResult> SaveAsync(Player player, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug(
            "Saving new player: {Name} (Id: {Id}). Total players in store: {PlayerCount}",
            player.Name, player.Id, _players.Count);

        try
        {
            // Check name uniqueness (case-insensitive)
            if (NameExists(player.Name))
            {
                var errorMessage = string.Format(ErrorNameExists, player.Name);
                _logger.LogWarning(
                    "Cannot save player: name '{Name}' already exists (case-insensitive). Id: {Id}",
                    player.Name, player.Id);
                return Task.FromResult(SaveResult.Failed(errorMessage));
            }

            // Attempt to add the player
            if (!_players.TryAdd(player.Id, player))
            {
                _logger.LogWarning(
                    "Cannot save player: ID '{Id}' already exists in store. Name: {Name}",
                    player.Id, player.Name);
                return Task.FromResult(SaveResult.Failed(ErrorSaveFailed));
            }

            // Track save timestamp
            _timestamps[player.Id] = DateTime.UtcNow;

            _logger.LogInformation(
                "Player saved successfully: {Name} (Id: {Id}). Total players: {TotalPlayers}",
                player.Name, player.Id, _players.Count);

            return Task.FromResult(SaveResult.Succeeded(player.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error saving player: {Name} (Id: {Id})",
                player.Name, player.Id);
            return Task.FromResult(SaveResult.Failed(ErrorSaveFailed));
        }
    }

    /// <inheritdoc />
    public Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("GetByIdAsync called for player: {PlayerId}", id);

        _players.TryGetValue(id, out var player);

        if (player != null)
        {
            _logger.LogDebug(
                "Player found: {Name} (Id: {Id})",
                player.Name, player.Id);
        }
        else
        {
            _logger.LogDebug("Player not found: {PlayerId}", id);
        }

        return Task.FromResult(player);
    }

    /// <inheritdoc />
    public Task<Player?> GetMostRecentAsync(CancellationToken ct = default)
    {
        _logger.LogDebug(
            "GetMostRecentAsync called. Total players in store: {PlayerCount}",
            _players.Count);

        if (_players.IsEmpty)
        {
            _logger.LogDebug("No players in store — returning null");
            return Task.FromResult<Player?>(null);
        }

        // Find the player with the most recent timestamp
        var mostRecentEntry = _timestamps
            .OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault();

        if (mostRecentEntry.Key == Guid.Empty)
        {
            _logger.LogDebug("No timestamps found — returning null");
            return Task.FromResult<Player?>(null);
        }

        _players.TryGetValue(mostRecentEntry.Key, out var player);

        if (player != null)
        {
            _logger.LogDebug(
                "Most recent player: {Name} (Id: {Id}, SavedAt: {SavedAt})",
                player.Name, player.Id, mostRecentEntry.Value);
        }

        return Task.FromResult(player);
    }

    /// <inheritdoc />
    public Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "ExistsWithNameAsync called for name: '{Name}'",
            name);

        var exists = NameExists(name);

        _logger.LogDebug(
            "Name '{Name}' exists: {Exists}",
            name, exists);

        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public Task<SaveResult> UpdateAsync(Player player, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug(
            "Updating player: {Name} (Id: {Id})",
            player.Name, player.Id);

        if (!_players.ContainsKey(player.Id))
        {
            var errorMessage = string.Format(ErrorPlayerNotFound, player.Id);
            _logger.LogWarning(
                "Cannot update player: ID '{Id}' not found in store. Name: {Name}",
                player.Id, player.Name);
            return Task.FromResult(SaveResult.Failed(errorMessage));
        }

        // Replace the existing player entry
        _players[player.Id] = player;
        _timestamps[player.Id] = DateTime.UtcNow;

        _logger.LogInformation(
            "Player updated: {Name} (Id: {Id})",
            player.Name, player.Id);

        return Task.FromResult(SaveResult.Succeeded(player.Id));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug(
            "GetAllAsync called. Total players in store: {PlayerCount}",
            _players.Count);

        var players = _players.Values
            .OrderByDescending(p => _timestamps.GetValueOrDefault(p.Id, DateTime.MinValue))
            .ToList();

        _logger.LogDebug(
            "Returning {PlayerCount} saved characters",
            players.Count);

        return Task.FromResult<IReadOnlyList<Player>>(players);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if any player in the store has the given name (case-insensitive).
    /// </summary>
    /// <param name="name">The character name to check.</param>
    /// <returns><c>true</c> if a player with the same name exists; <c>false</c> otherwise.</returns>
    private bool NameExists(string name)
    {
        var normalizedName = name.ToUpperInvariant();
        return _players.Values.Any(p =>
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
