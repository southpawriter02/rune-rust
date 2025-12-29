using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Engine.Helpers;
using RuneAndRust.Core.Models.Input;
using RuneAndRust.Core.Models.Magic;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// The main game service implementation.
/// Handles game initialization and the core game loop.
/// </summary>
/// <remarks>
/// See: SPEC-GAME-001 for Game Orchestration System design.
/// v0.3.23b: Added CancellationToken support, dirty flag rendering, and VFX event subscription.
/// </remarks>
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
    private readonly IScreenTransitionService? _transitionService;
    private readonly IAmbienceService? _ambienceService;
    private readonly IVisualEffectService? _vfxService;
    private readonly ISagaScreenRenderer? _sagaRenderer;
    private readonly ISagaService? _sagaService;
    private readonly IProgressionService? _progressionService;
    private readonly IInputService? _inputService;

    private GamePhase _previousPhase = GamePhase.MainMenu;
    private bool _renderRequired = true;

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
    /// <param name="transitionService">The screen transition service (v0.3.14b, optional for testing).</param>
    /// <param name="ambienceService">The ambient soundscape service (v0.3.19c, optional for testing).</param>
    /// <param name="vfxService">The visual effect service for VFX invalidation (v0.3.23b, optional for testing).</param>
    /// <param name="sagaRenderer">The Saga screen renderer for The Shrine UI (v0.4.0c, optional for testing).</param>
    /// <param name="sagaService">The Saga service for Legend progression (v0.4.0c, optional for testing).</param>
    /// <param name="progressionService">The progression service for attribute upgrades (v0.4.0c, optional for testing).</param>
    /// <param name="inputService">The input service for key-based modal UIs (v0.4.0c, optional for testing).</param>
    public GameService(
        ILogger<GameService> logger,
        IInputHandler inputHandler,
        CommandParser parser,
        GameState state,
        ICombatService combatService,
        IServiceScopeFactory scopeFactory,
        ICombatScreenRenderer? combatRenderer = null,
        IExplorationScreenRenderer? explorationRenderer = null,
        IScreenTransitionService? transitionService = null,
        IAmbienceService? ambienceService = null,
        IVisualEffectService? vfxService = null,
        ISagaScreenRenderer? sagaRenderer = null,
        ISagaService? sagaService = null,
        IProgressionService? progressionService = null,
        IInputService? inputService = null)
    {
        _logger = logger;
        _inputHandler = inputHandler;
        _parser = parser;
        _state = state;
        _combatService = combatService;
        _scopeFactory = scopeFactory;
        _combatRenderer = combatRenderer;
        _explorationRenderer = explorationRenderer;
        _transitionService = transitionService;
        _ambienceService = ambienceService;
        _vfxService = vfxService;
        _sagaRenderer = sagaRenderer;
        _sagaService = sagaService;
        _progressionService = progressionService;
        _inputService = inputService;

        // v0.3.23b: Subscribe to VFX invalidation events
        if (_vfxService != null)
        {
            _vfxService.OnInvalidateVisuals += () => _renderRequired = true;
        }
    }

    /// <inheritdoc/>
    /// <remarks>v0.3.23b: Added CancellationToken support and dirty flag rendering.</remarks>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[GameLoop] Starting game loop (v0.3.23b)");
        _inputHandler.DisplayMessage("Welcome to Rune & Rust!");
        _inputHandler.DisplayMessage("Type 'help' for available commands.");
        _inputHandler.DisplayMessage("");

        _previousPhase = _state.Phase;
        _renderRequired = true;

        while (_state.Phase != GamePhase.Quit && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                // v0.3.23b: 1. Check VFX expiration
                if (_vfxService?.CheckExpiredOverrides() == true)
                {
                    _renderRequired = true;
                }

                // 2. Check for phase transition and play animation (v0.3.14b)
                if (_state.Phase != _previousPhase)
                {
                    await HandlePhaseTransitionAsync(_previousPhase, _state.Phase);
                    _previousPhase = _state.Phase;
                    _renderRequired = true;
                }

                // 3. Render phase-specific UI (v0.3.23b: Only if required)
                if (_renderRequired)
                {
                    await RenderCurrentPhaseAsync();
                    _renderRequired = false;
                }

                // 4. Handle phase-specific input (v0.4.0c: SagaMenu uses key-based input)
                if (_state.Phase == GamePhase.SagaMenu && _inputService != null)
                {
                    // SagaMenu uses key-based modal input (v0.4.0c)
                    var inputEvent = _inputService.ReadNextFiltered();
                    var key = inputEvent switch
                    {
                        RawKeyEvent rawKey => rawKey.KeyInfo.Key,
                        ActionEvent actionEvent when actionEvent.SourceKey.HasValue => actionEvent.SourceKey.Value,
                        _ => ConsoleKey.NoName
                    };

                    _state.Phase = HandleSagaInput(key);
                    _renderRequired = true;
                }
                else
                {
                    // Standard text-based input for other phases
                    string prompt = GetPhasePrompt();

                    // 5. Get input from the user (abstracted for testability)
                    string input = _inputHandler.GetInput(prompt);

                    // 6. Process the input through the command parser
                var parseResult = await _parser.ParseAndExecuteAsync(input, _state);

                // v0.4.4a: Handle Magic Cast Command
                if (parseResult.RequiresCast && parseResult.CastSpell != null)
                {
                    _logger.LogTrace("[Magic] Processing cast command: {Spell}", parseResult.CastSpell);

                    if (_state.CurrentCharacter != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var magicService = scope.ServiceProvider.GetRequiredService<IMagicService>();
                        var spellRegistry = scope.ServiceProvider.GetRequiredService<SpellRegistry>();

                        var spell = spellRegistry.GetSpell(parseResult.CastSpell);
                        if (spell != null)
                        {
                            var result = magicService.Cast(_state.CurrentCharacter, spell, parseResult.CastTarget ?? "Unspecified");

                            // Visual feedback
                            switch (result)
                            {
                                case Core.Models.Magic.CastResult.Success:
                                    _inputHandler.DisplayMessage($"[mediumpurple]You cast {spell.Name}![/]");
                                    break;
                                case Core.Models.Magic.CastResult.InsufficientAether:
                                    _inputHandler.DisplayMessage($"[red]Not enough Aether to cast {spell.Name}.[/]");
                                    break;
                                case Core.Models.Magic.CastResult.StartedChant:
                                    _inputHandler.DisplayMessage($"[mediumpurple]You begin to chant {spell.Name}...[/]");
                                    break;
                            }
                        }
                        else
                        {
                            _logger.LogDebug("Cast aborted: Spell {SpellName} not found or not learned.", parseResult.CastSpell);
                            _inputHandler.DisplayMessage($"[grey]You do not know the Galdr for '{parseResult.CastSpell}'.[/]");
                        }
                    }
                }

                    _renderRequired = true;

                    // 7. Tick ambient soundscape after exploration commands (v0.3.19c)
                    if (_state.Phase == GamePhase.Exploration && _state.CurrentRoomId.HasValue && _ambienceService != null)
                    {
                        await _ambienceService.UpdateAsync(_state.CurrentRoomId.Value);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[GameLoop] Cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GameLoop] Error in game loop iteration");
                // Continue running - don't crash on single iteration errors
            }
        }

        // Final transition before quit (v0.3.14b)
        if (_previousPhase == GamePhase.Combat)
        {
            await HandlePhaseTransitionAsync(_previousPhase, GamePhase.Quit);
        }

        _logger.LogInformation("[GameLoop] Game loop ended. Phase: {Phase}", _state.Phase);
        _inputHandler.DisplayMessage("Thank you for playing Rune & Rust. Farewell!");
    }

    /// <summary>
    /// Renders the appropriate screen based on current game phase.
    /// </summary>
    /// <remarks>v0.3.23b: Extracted from main loop for dirty flag pattern.</remarks>
    private async Task RenderCurrentPhaseAsync()
    {
        _logger.LogTrace("[GameLoop] Rendering phase: {Phase}", _state.Phase);

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
        else if (_state.Phase == GamePhase.SagaMenu && _sagaRenderer != null)
        {
            var viewModel = BuildSagaViewModel();
            if (viewModel != null)
            {
                _sagaRenderer.Render(viewModel);
            }
        }
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
            GamePhase.SagaMenu => "[SHRINE]",
            _ => "[???]"
        };
    }

    /// <summary>
    /// Handles phase transitions by playing appropriate screen animations (v0.3.14b).
    /// </summary>
    /// <param name="from">The phase being transitioned from.</param>
    /// <param name="to">The phase being transitioned to.</param>
    private async Task HandlePhaseTransitionAsync(GamePhase from, GamePhase to)
    {
        if (_transitionService == null)
        {
            return;
        }

        var transitionType = (from, to) switch
        {
            (GamePhase.Exploration, GamePhase.Combat) => TransitionType.Shatter,
            (GamePhase.Combat, GamePhase.Exploration) => TransitionType.Dissolve,
            (GamePhase.Combat, GamePhase.Quit) => TransitionType.GlitchDecay,
            _ => TransitionType.None
        };

        _logger.LogDebug("[Game] Phase transition: {From} -> {To}, Effect: {Effect}",
            from, to, transitionType);

        await _transitionService.PlayAsync(transitionType);
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
            UserNotes: _state.UserNotes,
            VisibleObjects: visibleObjects,
            VisibleEnemies: visibleEnemies,
            Exits: exits,
            BiomeColor: biomeColor
        );
    }

    /// <summary>
    /// The currently selected attribute index in the Saga UI (0-4).
    /// Persisted in GameService to survive scoped controller recreation.
    /// </summary>
    /// <remarks>See: v0.4.0c (The Shrine) for Saga UI implementation.</remarks>
    private int _sagaSelectedIndex;

    /// <summary>
    /// Builds the Saga view model from current game state (v0.4.0c).
    /// Creates attribute rows with upgrade cost and status information.
    /// </summary>
    private SagaViewModel? BuildSagaViewModel()
    {
        if (_state.CurrentCharacter == null)
        {
            _logger.LogWarning("[Saga UI] Cannot build ViewModel: No current character");
            return null;
        }

        if (_sagaService == null || _progressionService == null)
        {
            _logger.LogWarning("[Saga UI] Cannot build ViewModel: Missing Saga/Progression service");
            return null;
        }

        var character = _state.CurrentCharacter;
        var attributes = new List<AttributeRowViewModel>();

        // Build attribute rows in the canonical order: Might, Finesse, Sturdiness, Wits, Will
        CharacterAttribute[] attributeOrder =
        {
            CharacterAttribute.Might,
            CharacterAttribute.Finesse,
            CharacterAttribute.Sturdiness,
            CharacterAttribute.Wits,
            CharacterAttribute.Will
        };

        foreach (var attrType in attributeOrder)
        {
            int currentValue = character.GetAttribute(attrType);
            int upgradeCost = _progressionService.GetUpgradeCost(character, attrType);
            bool canUpgrade = _progressionService.CanUpgrade(character, attrType);

            AttributeStatus status;
            if (upgradeCost == int.MaxValue)
            {
                status = AttributeStatus.Maxed;
            }
            else if (!canUpgrade)
            {
                status = AttributeStatus.Locked;
            }
            else
            {
                status = AttributeStatus.Upgrade;
            }

            attributes.Add(new AttributeRowViewModel(attrType, currentValue, upgradeCost, status));
        }

        int legendForNext = _sagaService.GetLegendForNextLevel(character.Level);

        _logger.LogDebug(
            "[Saga UI] Building ViewModel for {Name}: Level {Level}, Legend {Current}/{Next}, PP {PP}, Selected {Index}",
            character.Name, character.Level, character.Legend, legendForNext, character.ProgressionPoints, _sagaSelectedIndex);

        return new SagaViewModel(
            CharacterName: character.Name,
            Level: character.Level,
            CurrentLegend: character.Legend,
            LegendForNextLevel: legendForNext,
            ProgressionPoints: character.ProgressionPoints,
            SelectedIndex: _sagaSelectedIndex,
            Attributes: attributes
        );
    }

    /// <summary>
    /// Handles input while in the SagaMenu phase (v0.4.0c).
    /// Processes navigation and upgrade commands, returns new phase.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>The game phase to transition to.</returns>
    public GamePhase HandleSagaInput(ConsoleKey key)
    {
        if (_state.CurrentCharacter == null || _progressionService == null)
        {
            return GamePhase.Exploration;
        }

        CharacterAttribute[] attributeOrder =
        {
            CharacterAttribute.Might,
            CharacterAttribute.Finesse,
            CharacterAttribute.Sturdiness,
            CharacterAttribute.Wits,
            CharacterAttribute.Will
        };

        switch (key)
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                if (_sagaSelectedIndex > 0)
                {
                    _sagaSelectedIndex--;
                    _logger.LogTrace("[Saga UI] Selection moved up to index {Index}", _sagaSelectedIndex);
                }
                break;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                if (_sagaSelectedIndex < attributeOrder.Length - 1)
                {
                    _sagaSelectedIndex++;
                    _logger.LogTrace("[Saga UI] Selection moved down to index {Index}", _sagaSelectedIndex);
                }
                break;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                var attribute = attributeOrder[_sagaSelectedIndex];
                var result = _progressionService.UpgradeAttribute(_state.CurrentCharacter, attribute);
                if (result.Success)
                {
                    _logger.LogInformation("[Saga UI] Successfully upgraded {Attribute}", attribute);
                }
                else
                {
                    _logger.LogDebug("[Saga UI] Upgrade failed: {Message}", result.Message);
                }
                break;

            case ConsoleKey.Escape:
            case ConsoleKey.Q:
                _logger.LogInformation("[Saga UI] Exiting Shrine, returning to Exploration");
                _sagaSelectedIndex = 0; // Reset for next entry
                return GamePhase.Exploration;
        }

        return GamePhase.SagaMenu; // Stay in menu
    }
}
