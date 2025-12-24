using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Engine.Helpers;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// The main game service implementation.
/// Handles game initialization and the core game loop.
/// </summary>
/// <remarks>See: SPEC-GAME-001 for Game Orchestration System design.</remarks>
public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly IInputHandler _inputHandler;
    private readonly CommandParser _parser;
    private readonly GameState _state;
    private readonly ICombatService _combatService;
    private readonly ICombatScreenRenderer? _combatRenderer;
    private readonly IExplorationScreenRenderer? _explorationRenderer;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="inputHandler">The input handler for user interaction.</param>
    /// <param name="parser">The command parser for processing input.</param>
    /// <param name="state">The game state singleton.</param>
    /// <param name="combatService">The combat service for ViewModel access.</param>
    /// <param name="scopeFactory">Factory for creating scoped services (v0.3.5a).</param>
    /// <param name="combatRenderer">The combat screen renderer (optional for testing).</param>
    /// <param name="explorationRenderer">The exploration screen renderer (v0.3.5a, optional for testing).</param>
    public GameService(
        ILogger<GameService> logger,
        IInputHandler inputHandler,
        CommandParser parser,
        GameState state,
        ICombatService combatService,
        IServiceScopeFactory scopeFactory,
        ICombatScreenRenderer? combatRenderer = null,
        IExplorationScreenRenderer? explorationRenderer = null)
    {
        _logger = logger;
        _inputHandler = inputHandler;
        _parser = parser;
        _state = state;
        _combatService = combatService;
        _scopeFactory = scopeFactory;
        _combatRenderer = combatRenderer;
        _explorationRenderer = explorationRenderer;
    }

    /// <inheritdoc/>
    public async Task StartAsync()
    {
        _logger.LogInformation("Game Loop Initialized.");
        _inputHandler.DisplayMessage("Welcome to Rune & Rust!");
        _inputHandler.DisplayMessage("Type 'help' for available commands.");
        _inputHandler.DisplayMessage("");

        while (_state.Phase != GamePhase.Quit)
        {
            // 1. Render phase-specific UI
            if (_state.Phase == GamePhase.Combat && _combatRenderer != null)
            {
                var viewModel = _combatService.GetViewModel();
                if (viewModel != null)
                {
                    _combatRenderer.Render(viewModel);
                }
            }
            else if (_state.Phase == GamePhase.Exploration && _explorationRenderer != null)
            {
                var viewModel = await BuildExplorationViewModelAsync();
                if (viewModel != null)
                {
                    _explorationRenderer.Render(viewModel);
                }
            }

            // 2. Determine prompt based on current phase
            string prompt = GetPhasePrompt();

            // 3. Get input from the user (abstracted for testability)
            string input = _inputHandler.GetInput(prompt);

            // 4. Process the input through the command parser
            await _parser.ParseAndExecuteAsync(input, _state);
        }

        _logger.LogInformation("Game Loop Ended. Shutting down.");
        _inputHandler.DisplayMessage("Thank you for playing Rune & Rust. Farewell!");
    }

    /// <summary>
    /// Returns the appropriate prompt string based on the current game phase.
    /// </summary>
    private string GetPhasePrompt()
    {
        return _state.Phase switch
        {
            GamePhase.MainMenu => "[MENU]",
            GamePhase.Exploration => "[EXPLORE]",
            GamePhase.Combat => "[COMBAT]",
            _ => "[???]"
        };
    }

    /// <summary>
    /// Builds the exploration view model from current game state (v0.3.5a).
    /// Uses a scoped service to fetch room data.
    /// Extended with minimap data in v0.3.5b.
    /// Extended with room rendering data in v0.3.5c.
    /// </summary>
    private async Task<ExplorationViewModel?> BuildExplorationViewModelAsync()
    {
        if (_state.CurrentCharacter == null)
        {
            _logger.LogWarning("[HUD] Cannot build exploration ViewModel: No current character");
            return null;
        }

        string roomName = "Unknown";
        string roomDescription = "You are in an unknown location.";
        Coordinate playerPosition = Coordinate.Origin;
        List<Room> localMapRooms = new();
        List<string> visibleObjects = new();
        List<string> visibleEnemies = new();
        string exits = "";
        string biomeColor = "grey";

        // Fetch room data using a scoped service
        if (_state.CurrentRoomId.HasValue)
        {
            using var scope = _scopeFactory.CreateScope();
            var roomRepo = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
            var room = await roomRepo.GetByIdAsync(_state.CurrentRoomId.Value);
            if (room != null)
            {
                roomName = room.Name;
                roomDescription = room.Description;
                playerPosition = room.Position;
                biomeColor = RoomViewHelper.GetBiomeColor(room.BiomeType);

                // Format exits (v0.3.5c)
                if (room.Exits.Any())
                {
                    exits = string.Join(", ", room.Exits.Keys.Select(d => d.ToString().ToLower()));
                }

                // Fetch 3x3 grid around player for minimap (v0.3.5b)
                const int radius = 1;
                localMapRooms = (await roomRepo.GetRoomsInGridAsync(
                    playerPosition.Z,
                    playerPosition.X - radius,
                    playerPosition.X + radius,
                    playerPosition.Y - radius,
                    playerPosition.Y + radius
                )).ToList();

                _logger.LogTrace("[HUD] Fetched {Count} rooms for minimap grid", localMapRooms.Count);

                // Fetch visible objects (v0.3.5c)
                var interactionService = scope.ServiceProvider.GetRequiredService<IInteractionService>();
                var objects = await interactionService.GetVisibleObjectsAsync();
                visibleObjects = objects
                    .Select(o => RoomViewHelper.FormatObjectName(o.Name, o.IsContainer, o.IsLocked))
                    .ToList();

                _logger.LogTrace("[HUD] Found {ObjectCount} visible objects in room", objects.Count());
            }
        }

        // Note: Enemies are only present during Combat phase
        // During Exploration, enemies are spawned by AmbushService when entering rooms
        // VisibleEnemies remains empty during Exploration phase

        var character = _state.CurrentCharacter;

        _logger.LogDebug("[HUD] Building exploration ViewModel for {Character} in {Room}",
            character.Name, roomName);

        return new ExplorationViewModel(
            CharacterName: character.Name,
            CurrentHp: character.CurrentHP,
            MaxHp: character.MaxHP,
            CurrentStamina: character.CurrentStamina,
            MaxStamina: character.MaxStamina,
            CurrentStress: character.PsychicStress,
            MaxStress: character.MaxPsychicStress,
            CurrentCorruption: character.Corruption,
            MaxCorruption: character.MaxCorruption,
            RoomName: roomName,
            RoomDescription: roomDescription,
            TurnCount: _state.TurnCount,
            PlayerPosition: playerPosition,
            LocalMapRooms: localMapRooms,
            VisitedRoomIds: _state.VisitedRoomIds,
            VisibleObjects: visibleObjects,
            VisibleEnemies: visibleEnemies,
            Exits: exits,
            BiomeColor: biomeColor
        );
    }
}
