using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.TestUtilities.Builders;

/// <summary>
/// Fluent builder for creating test GameSession instances.
/// </summary>
public class GameSessionBuilder
{
    private Player? _player;
    private PlayerBuilder? _playerBuilder;
    private Dungeon? _dungeon;
    private readonly List<Room> _rooms = [];
    private GameState _state = GameState.Playing;
    private Guid? _startingRoomId;

    /// <summary>
    /// Creates a new GameSessionBuilder with default values.
    /// </summary>
    public static GameSessionBuilder Create() => new();

    /// <summary>
    /// Sets the player for this session.
    /// </summary>
    public GameSessionBuilder WithPlayer(Player player)
    {
        _player = player;
        _playerBuilder = null;
        return this;
    }

    /// <summary>
    /// Sets the player using a PlayerBuilder.
    /// </summary>
    public GameSessionBuilder WithPlayer(PlayerBuilder builder)
    {
        _playerBuilder = builder;
        _player = null;
        return this;
    }

    /// <summary>
    /// Sets the player using a configuration action on a new PlayerBuilder.
    /// </summary>
    public GameSessionBuilder WithPlayer(Action<PlayerBuilder> configure)
    {
        var builder = PlayerBuilder.Create();
        configure(builder);
        _playerBuilder = builder;
        _player = null;
        return this;
    }

    /// <summary>
    /// Sets the dungeon for this session.
    /// </summary>
    public GameSessionBuilder WithDungeon(Dungeon dungeon)
    {
        _dungeon = dungeon;
        return this;
    }

    /// <summary>
    /// Adds a room to be used when building a dungeon.
    /// </summary>
    public GameSessionBuilder WithRoom(Room room)
    {
        _rooms.Add(room);
        return this;
    }

    /// <summary>
    /// Sets the starting room ID for the session.
    /// </summary>
    public GameSessionBuilder InRoom(Guid roomId)
    {
        _startingRoomId = roomId;
        return this;
    }

    /// <summary>
    /// Sets the game state.
    /// </summary>
    public GameSessionBuilder WithState(GameState state)
    {
        _state = state;
        return this;
    }

    /// <summary>
    /// Builds the GameSession instance.
    /// </summary>
    public GameSession Build()
    {
        // Build or use provided player
        var player = _player ?? _playerBuilder?.Build() ?? PlayerBuilder.Create().Build();

        // Use the GameSession's static factory method which creates a starter dungeon
        var session = GameSession.CreateNew(player.Name);

        // Set the game state
        if (_state != GameState.Playing)
        {
            session.SetState(_state);
        }

        return session;
    }

    /// <summary>
    /// Builds the GameSession with a custom player name.
    /// </summary>
    public GameSession Build(string playerName)
    {
        var session = GameSession.CreateNew(playerName);

        if (_state != GameState.Playing)
        {
            session.SetState(_state);
        }

        return session;
    }
}
