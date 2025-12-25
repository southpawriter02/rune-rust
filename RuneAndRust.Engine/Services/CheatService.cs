using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Executes debug cheat commands (v0.3.17b).
/// Part of "The Toolbox" debug tools milestone.
/// </summary>
/// <remarks>See: SPEC-CHEAT-001 for Cheat Command System design.</remarks>
public class CheatService : ICheatService
{
    private readonly GameState _state;
    private readonly IRoomRepository _roomRepo;
    private readonly ILogger<CheatService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheatService"/> class.
    /// </summary>
    /// <param name="state">The game state.</param>
    /// <param name="roomRepo">The room repository.</param>
    /// <param name="logger">The logger instance.</param>
    public CheatService(
        GameState state,
        IRoomRepository roomRepo,
        ILogger<CheatService> logger)
    {
        _state = state;
        _roomRepo = roomRepo;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool ToggleGodMode()
    {
        _logger.LogDebug("[Cheat] ToggleGodMode invoked, current state: {State}", _state.IsGodMode);

        _state.IsGodMode = !_state.IsGodMode;

        _logger.LogWarning("[Cheat] God Mode set to: {State}", _state.IsGodMode);

        return _state.IsGodMode;
    }

    /// <inheritdoc />
    public bool FullHeal()
    {
        _logger.LogDebug("[Cheat] FullHeal invoked");

        var character = _state.CurrentCharacter;
        if (character == null)
        {
            _logger.LogWarning("[Cheat] FullHeal failed: no active character");
            return false;
        }

        character.CurrentHP = character.MaxHP;
        character.CurrentStamina = character.MaxStamina;
        character.CurrentAp = character.MaxAp;
        character.PsychicStress = 0;
        character.ActiveStatusEffects.Clear();

        _logger.LogWarning("[Cheat] Character fully healed: HP={HP}, Stamina={Stamina}, AP={AP}",
            character.CurrentHP, character.CurrentStamina, character.CurrentAp);

        return true;
    }

    /// <inheritdoc />
    public async Task<string?> TeleportAsync(string roomIdOrName)
    {
        _logger.LogDebug("[Cheat] TeleportAsync invoked with: {RoomIdOrName}", roomIdOrName);

        // Try GUID first
        if (Guid.TryParse(roomIdOrName, out var roomId))
        {
            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room != null)
            {
                _state.CurrentRoomId = room.Id;
                _state.VisitedRoomIds.Add(room.Id);
                _logger.LogWarning("[Cheat] Teleported to: {RoomName} (GUID match)", room.Name);
                return room.Name;
            }
        }

        // Fallback: search by name (partial match)
        var allRooms = await _roomRepo.GetAllRoomsAsync();
        var match = allRooms.FirstOrDefault(r =>
            r.Name.Contains(roomIdOrName, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            _state.CurrentRoomId = match.Id;
            _state.VisitedRoomIds.Add(match.Id);
            _logger.LogWarning("[Cheat] Teleported to: {RoomName} (name match)", match.Name);
            return match.Name;
        }

        _logger.LogWarning("[Cheat] Teleport failed: room not found for '{RoomIdOrName}'", roomIdOrName);
        return null;
    }

    /// <inheritdoc />
    public Task<bool> SpawnItemAsync(string itemId, int quantity = 1)
    {
        // TODO: Implement when ItemRepository lookup is available
        _logger.LogWarning("[Cheat] Spawn not yet implemented: {ItemId} x{Qty}", itemId, quantity);
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<int> RevealMapAsync()
    {
        _logger.LogDebug("[Cheat] RevealMapAsync invoked");

        var allRooms = await _roomRepo.GetAllRoomsAsync();
        var count = 0;

        foreach (var room in allRooms)
        {
            if (_state.VisitedRoomIds.Add(room.Id))
            {
                count++;
            }
        }

        _logger.LogWarning("[Cheat] Revealed {Count} rooms", count);

        return count;
    }
}
