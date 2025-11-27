using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.Engine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// Represents an available exit from the current room.
/// </summary>
public class ExitViewModel : ViewModelBase
{
    /// <summary>
    /// Direction string (north, south, east, west, up, down).
    /// </summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>
    /// Display name with icon.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Target room ID.
    /// </summary>
    public string TargetRoomId { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a vertical connection (stairs, ladder, etc.).
    /// </summary>
    public bool IsVertical { get; set; } = false;

    /// <summary>
    /// For vertical connections, the type description.
    /// </summary>
    public string? VerticalType { get; set; } = null;

    /// <summary>
    /// For vertical connections, traversal requirements.
    /// </summary>
    public string? TraversalRequirements { get; set; } = null;
}

/// <summary>
/// Represents a feature in the current room.
/// </summary>
public class RoomFeatureViewModel : ViewModelBase
{
    /// <summary>
    /// Feature icon.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Feature name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Feature description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this feature is interactable.
    /// </summary>
    public bool IsInteractable { get; set; } = false;

    /// <summary>
    /// Feature type for styling.
    /// </summary>
    public string FeatureType { get; set; } = "Info"; // Info, Hazard, Loot, NPC, Puzzle
}

/// <summary>
/// View model for the dungeon exploration view.
/// Displays room information, available exits, and exploration actions.
/// v0.38 Integration: Uses descriptor services for rich narrative content.
/// </summary>
public class DungeonExplorationViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;
    private Dungeon? _currentDungeon;
    private Room? _currentRoom;
    private string _statusMessage = string.Empty;
    private bool _isSearched = false;
    private PlayerCharacter? _character;
    private MinimapViewModel _minimap = new();
    private bool _isMinimapVisible = true;

    // v0.43.14: Room Interactions & Search
    private bool _isSearching = false;
    private SearchResultViewModel? _searchResult = null;
    private bool _isRestDialogVisible = false;
    private RestConfirmationViewModel? _restConfirmation = null;

    // v0.38: Descriptor Services for Flavor Text
    private readonly RoomDescriptorService? _roomDescriptorService;
    private readonly ExaminationFlavorTextService? _examinationService;
    private readonly AtmosphericDescriptorService? _atmosphericService;
    private readonly SkillUsageFlavorTextService? _skillUsageService;

    // v0.38: Enhanced narrative text
    private string _narrativeText = string.Empty;
    private string _atmosphereText = string.Empty;

    #region Properties

    /// <summary>
    /// The current dungeon being explored.
    /// </summary>
    public Dungeon? CurrentDungeon
    {
        get => _currentDungeon;
        private set => this.RaiseAndSetIfChanged(ref _currentDungeon, value);
    }

    /// <summary>
    /// The current room the player is in.
    /// </summary>
    public Room? CurrentRoom
    {
        get => _currentRoom;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentRoom, value);
            UpdateRoomDisplay();
        }
    }

    /// <summary>
    /// Room name for display.
    /// </summary>
    public string RoomName => CurrentRoom?.Name ?? "Unknown Location";

    /// <summary>
    /// Room description text. Uses RoomDescriptorService when available for rich narrative.
    /// </summary>
    public string RoomDescription => !string.IsNullOrEmpty(_narrativeText)
        ? _narrativeText
        : (CurrentRoom?.Description ?? "You are in an unknown location.");

    /// <summary>
    /// v0.38: Atmospheric description for the room (lighting, sounds, smells, etc.)
    /// </summary>
    public string AtmosphereText
    {
        get => _atmosphereText;
        private set => this.RaiseAndSetIfChanged(ref _atmosphereText, value);
    }

    /// <summary>
    /// v0.38: Whether there is atmospheric description to display.
    /// </summary>
    public bool HasAtmosphere => !string.IsNullOrEmpty(_atmosphereText);

    /// <summary>
    /// Biome name for display.
    /// </summary>
    public string BiomeName => FormatBiomeName(CurrentRoom?.PrimaryBiome ?? "Unknown");

    /// <summary>
    /// Biome color for styling.
    /// </summary>
    public string BiomeColor => GetBiomeColor(CurrentRoom?.PrimaryBiome);

    /// <summary>
    /// Vertical layer display.
    /// </summary>
    public string LayerDisplay => CurrentRoom?.Layer.GetDepthNarrative() ?? "";

    /// <summary>
    /// Available exits from the current room.
    /// </summary>
    public ObservableCollection<ExitViewModel> AvailableExits { get; } = new();

    /// <summary>
    /// Features in the current room.
    /// </summary>
    public ObservableCollection<RoomFeatureViewModel> RoomFeatures { get; } = new();

    /// <summary>
    /// Status message for feedback.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    /// <summary>
    /// Whether the current room has been searched.
    /// </summary>
    public bool IsSearched
    {
        get => _isSearched;
        set => this.RaiseAndSetIfChanged(ref _isSearched, value);
    }

    /// <summary>
    /// Whether the room is cleared of enemies.
    /// </summary>
    public bool IsCleared => CurrentRoom?.HasBeenCleared ?? true;

    /// <summary>
    /// Whether enemies are present.
    /// </summary>
    public bool HasEnemies => CurrentRoom?.Enemies?.Count > 0;

    /// <summary>
    /// Whether the room is a sanctuary (safe rest).
    /// </summary>
    public bool IsSanctuary => CurrentRoom?.IsSanctuary ?? false;

    /// <summary>
    /// Hazard warning if present.
    /// </summary>
    public string? HazardWarning => CurrentRoom?.HasEnvironmentalHazard == true && CurrentRoom.IsHazardActive
        ? CurrentRoom.HazardDescription
        : null;

    /// <summary>
    /// Player character for stats display.
    /// </summary>
    public PlayerCharacter? Character
    {
        get => _character;
        set => this.RaiseAndSetIfChanged(ref _character, value);
    }

    /// <summary>
    /// Current HP display.
    /// </summary>
    public string HPDisplay => Character != null ? $"{Character.HP}/{Character.MaxHP}" : "?/?";

    /// <summary>
    /// Current Stamina display.
    /// </summary>
    public string StaminaDisplay => Character != null ? $"{Character.Stamina}/{Character.MaxStamina}" : "?/?";

    /// <summary>
    /// Psychic Stress display.
    /// </summary>
    public string StressDisplay => Character != null ? $"{Character.PsychicStress}/100" : "?/100";

    /// <summary>
    /// The minimap view model.
    /// </summary>
    public MinimapViewModel Minimap
    {
        get => _minimap;
        private set => this.RaiseAndSetIfChanged(ref _minimap, value);
    }

    /// <summary>
    /// Whether the minimap is visible.
    /// </summary>
    public bool IsMinimapVisible
    {
        get => _isMinimapVisible;
        set => this.RaiseAndSetIfChanged(ref _isMinimapVisible, value);
    }

    #endregion

    #region v0.43.14: Room Interactions & Search Properties

    /// <summary>
    /// Whether a search is currently in progress.
    /// </summary>
    public bool IsSearching
    {
        get => _isSearching;
        set
        {
            this.RaiseAndSetIfChanged(ref _isSearching, value);
            this.RaisePropertyChanged(nameof(CanSearch));
        }
    }

    /// <summary>
    /// The current search result (null if no search has been performed).
    /// </summary>
    public SearchResultViewModel? SearchResult
    {
        get => _searchResult;
        set => this.RaiseAndSetIfChanged(ref _searchResult, value);
    }

    /// <summary>
    /// Whether a search result overlay is visible.
    /// </summary>
    public bool IsSearchResultVisible => SearchResult != null;

    /// <summary>
    /// Whether the player can search (room not already searched, not currently searching).
    /// </summary>
    public bool CanSearch => CurrentRoom != null && !IsSearched && !IsSearching;

    /// <summary>
    /// Whether the rest confirmation dialog is visible.
    /// </summary>
    public bool IsRestDialogVisible
    {
        get => _isRestDialogVisible;
        set => this.RaiseAndSetIfChanged(ref _isRestDialogVisible, value);
    }

    /// <summary>
    /// The rest confirmation view model.
    /// </summary>
    public RestConfirmationViewModel? RestConfirmation
    {
        get => _restConfirmation;
        set => this.RaiseAndSetIfChanged(ref _restConfirmation, value);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to move in a direction.
    /// </summary>
    public ICommand MoveCommand { get; }

    /// <summary>
    /// Command to search the current room.
    /// </summary>
    public ICommand SearchRoomCommand { get; }

    /// <summary>
    /// Command to look at/examine the current room (detailed perception).
    /// </summary>
    public ICommand LookCommand { get; }

    /// <summary>
    /// Command to investigate objects in the room (WITS-based skill check).
    /// </summary>
    public ICommand InvestigateCommand { get; }

    /// <summary>
    /// Command to rest in the current room.
    /// </summary>
    public ICommand RestCommand { get; }

    /// <summary>
    /// Command to view character sheet.
    /// </summary>
    public ICommand ViewCharacterCommand { get; }

    /// <summary>
    /// Command to view inventory.
    /// </summary>
    public ICommand ViewInventoryCommand { get; }

    /// <summary>
    /// Command to interact with a feature.
    /// </summary>
    public ICommand InteractCommand { get; }

    /// <summary>
    /// Command to engage enemies.
    /// </summary>
    public ICommand EngageCommand { get; }

    /// <summary>
    /// Command to toggle minimap visibility.
    /// </summary>
    public ICommand ToggleMinimapCommand { get; }

    // v0.43.14: Room Interactions Commands

    /// <summary>
    /// Command to collect loot from search results.
    /// </summary>
    public ICommand CollectLootCommand { get; }

    /// <summary>
    /// Command to close search results overlay.
    /// </summary>
    public ICommand CloseSearchResultCommand { get; }

    /// <summary>
    /// Command to show rest confirmation dialog.
    /// </summary>
    public ICommand ShowRestDialogCommand { get; }

    /// <summary>
    /// Command to confirm rest.
    /// </summary>
    public ICommand ConfirmRestCommand { get; }

    /// <summary>
    /// Command to cancel rest dialog.
    /// </summary>
    public ICommand CancelRestCommand { get; }

    #endregion

    public DungeonExplorationViewModel()
    {
        // Initialize commands
        MoveCommand = ReactiveCommand.Create<ExitViewModel>(Move);
        SearchRoomCommand = ReactiveCommand.CreateFromTask(SearchRoomAsync);
        LookCommand = ReactiveCommand.CreateFromTask(LookRoomAsync);
        InvestigateCommand = ReactiveCommand.CreateFromTask(InvestigateRoomAsync);
        RestCommand = ReactiveCommand.Create(ShowRestDialog);
        ViewCharacterCommand = ReactiveCommand.Create(ViewCharacter);
        ViewInventoryCommand = ReactiveCommand.Create(ViewInventory);
        InteractCommand = ReactiveCommand.Create<RoomFeatureViewModel>(Interact);
        EngageCommand = ReactiveCommand.Create(EngageEnemies);
        ToggleMinimapCommand = ReactiveCommand.Create(ToggleMinimap);

        // v0.43.14: Room Interactions Commands
        CollectLootCommand = ReactiveCommand.Create(CollectLoot);
        CloseSearchResultCommand = ReactiveCommand.Create(CloseSearchResult);
        ShowRestDialogCommand = ReactiveCommand.Create(ShowRestDialog);
        ConfirmRestCommand = ReactiveCommand.Create(ConfirmRest);
        CancelRestCommand = ReactiveCommand.Create(CancelRest);

        // Load demo dungeon
        LoadDemoDungeon();
    }

    public DungeonExplorationViewModel(INavigationService navigationService) : this()
    {
        _navigationService = navigationService;
    }

    /// <summary>
    /// v0.38: Constructor with descriptor services for rich narrative content.
    /// </summary>
    public DungeonExplorationViewModel(
        INavigationService navigationService,
        RoomDescriptorService? roomDescriptorService,
        ExaminationFlavorTextService? examinationService,
        AtmosphericDescriptorService? atmosphericService,
        SkillUsageFlavorTextService? skillUsageService) : this(navigationService)
    {
        _roomDescriptorService = roomDescriptorService;
        _examinationService = examinationService;
        _atmosphericService = atmosphericService;
        _skillUsageService = skillUsageService;
    }

    #region Public Methods

    /// <summary>
    /// Loads a dungeon and starts at the specified room.
    /// </summary>
    public void LoadDungeon(Dungeon dungeon, string startRoomId)
    {
        CurrentDungeon = dungeon;
        var startRoom = dungeon.GetRoom(startRoomId) ?? dungeon.GetStartRoom();
        CurrentRoom = startRoom;
        StatusMessage = $"Entering {dungeon.Biome}...";

        // Initialize minimap with dungeon data
        if (startRoom != null)
        {
            Minimap.LoadDungeon(dungeon, startRoom);
        }
    }

    /// <summary>
    /// Loads a dungeon and starts at the start room.
    /// </summary>
    public void LoadDungeon(Dungeon dungeon)
    {
        LoadDungeon(dungeon, dungeon.StartRoomId);
    }

    /// <summary>
    /// v0.44.3: Loads the current room (for controller-driven updates).
    /// </summary>
    public void LoadRoom(Room room)
    {
        CurrentRoom = room;
        if (CurrentDungeon != null)
        {
            Minimap.UpdateCurrentRoom(room);
        }
    }

    /// <summary>
    /// v0.44.3: Sets the player character reference.
    /// </summary>
    public void SetCharacter(PlayerCharacter player)
    {
        Character = player;
        this.RaisePropertyChanged(nameof(HPDisplay));
        this.RaisePropertyChanged(nameof(StaminaDisplay));
        this.RaisePropertyChanged(nameof(StressDisplay));
    }

    /// <summary>
    /// v0.44.3: Shows a status message to the player.
    /// Called by ExplorationController for feedback.
    /// </summary>
    public void ShowMessage(string message)
    {
        StatusMessage = message;
    }

    /// <summary>
    /// v0.44.3: Updates the minimap display.
    /// </summary>
    public void UpdateMinimap()
    {
        if (CurrentRoom != null)
        {
            Minimap.UpdateCurrentRoom(CurrentRoom);
        }
    }

    /// <summary>
    /// v0.44.3: Shows the search result overlay.
    /// </summary>
    public void ShowSearchResult(Engine.SearchResult searchResult)
    {
        // Convert engine search result to view model
        var lootItems = searchResult.Items.Select(e => LootItemViewModel.FromEquipment(e)).ToList();

        if (searchResult.ScrapFound > 0)
        {
            lootItems.Add(LootItemViewModel.ForCurrency("Scrap", searchResult.ScrapFound));
        }

        if (searchResult.TriggeredEncounter)
        {
            SearchResult = SearchResultViewModel.WithEncounter(searchResult.Message, null);
        }
        else if (searchResult.FoundItems || lootItems.Any())
        {
            SearchResult = SearchResultViewModel.WithLoot(lootItems, searchResult.Secrets);
        }
        else if (searchResult.Secrets.Any())
        {
            SearchResult = SearchResultViewModel.WithSecrets(searchResult.Secrets);
        }
        else
        {
            SearchResult = SearchResultViewModel.Empty();
        }

        this.RaisePropertyChanged(nameof(IsSearchResultVisible));
    }

    /// <summary>
    /// v0.44.3: Shows the rest confirmation dialog asynchronously.
    /// </summary>
    public Task<bool> ShowRestConfirmationAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        if (CurrentRoom == null || Character == null)
        {
            tcs.SetResult(false);
            return tcs.Task;
        }

        // Set up rest confirmation
        if (CurrentRoom.IsSanctuary)
        {
            RestConfirmation = RestConfirmationViewModel.ForSanctuary(
                Character.HP, Character.MaxHP,
                Character.Stamina, Character.MaxStamina);
        }
        else
        {
            RestConfirmation = RestConfirmationViewModel.ForRegularRest(
                Character.HP, Character.MaxHP,
                Character.Stamina, Character.MaxStamina,
                Character.PsychicStress);
        }

        IsRestDialogVisible = true;

        // For now, return true (in real implementation, would await user confirmation)
        // The actual confirmation is handled by ConfirmRest/CancelRest commands
        tcs.SetResult(true);
        return tcs.Task;
    }

    /// <summary>
    /// v0.44.3: Shows the rest result feedback.
    /// </summary>
    public void ShowRestResult(Engine.RestResult restResult)
    {
        StatusMessage = restResult.Message;
        IsRestDialogVisible = false;
        RestConfirmation = null;

        // Update stats display
        this.RaisePropertyChanged(nameof(HPDisplay));
        this.RaisePropertyChanged(nameof(StaminaDisplay));
        this.RaisePropertyChanged(nameof(StressDisplay));
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads demo dungeon data for testing.
    /// </summary>
    private void LoadDemoDungeon()
    {
        // Create demo character
        Character = new PlayerCharacter
        {
            Name = "Explorer",
            HP = 45,
            MaxHP = 60,
            Stamina = 80,
            MaxStamina = 100,
            PsychicStress = 15
        };

        // Create demo dungeon - The Roots biome
        var dungeon = new Dungeon
        {
            DungeonId = 1,
            Seed = 12345,
            Biome = "The_Roots",
            StartRoomId = "room_01",
            BossRoomId = "room_05"
        };

        // Room 1: Entry Hall (0, 0, 0) - Origin
        dungeon.Rooms["room_01"] = new Room
        {
            RoomId = "room_01",
            Name = "Entry Hall",
            Description = "You stand in a vast entry hall, the ceiling lost in shadows above. Corroded pipes line the walls like metallic veins, occasionally hissing with escaping steam. The air is thick with the smell of rust and ozone. Faded runic glyphs flicker weakly on a crumbling archway to the north.",
            PrimaryBiome = "The_Roots",
            Layer = VerticalLayer.GroundLevel,
            Position = new RoomPosition(0, 0, 0),
            IsStartRoom = true,
            Exits = new Dictionary<string, string>
            {
                ["north"] = "room_02",
                ["east"] = "room_03"
            },
            IsSanctuary = true
        };

        // Room 2: Maintenance Corridor (0, 1, 0) - North of Entry
        dungeon.Rooms["room_02"] = new Room
        {
            RoomId = "room_02",
            Name = "Maintenance Corridor",
            Description = "A narrow maintenance corridor stretches before you, barely wide enough for two people to walk abreast. Exposed conduits spark intermittently overhead, casting dancing shadows on the grime-covered walls. The floor is slick with condensation, and you hear the distant grinding of machinery.",
            PrimaryBiome = "The_Roots",
            Layer = VerticalLayer.GroundLevel,
            Position = new RoomPosition(0, 1, 0),
            Exits = new Dictionary<string, string>
            {
                ["south"] = "room_01",
                ["north"] = "room_04",
                ["west"] = "room_03"
            },
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDescription = "Sparking conduits threaten to discharge electricity.",
            HazardType = HazardType.ElectricalHazard
        };

        // Room 3: Storage Chamber (1, 0, 0) - East of Entry (also connected west to room_02)
        dungeon.Rooms["room_03"] = new Room
        {
            RoomId = "room_03",
            Name = "Storage Chamber",
            Description = "Towering shelves of corroded metal fill this chamber, most collapsed into heaps of rust and debris. Among the wreckage, you spot containers that might still hold useful salvage. Something skitters in the darkness between the shelving units.",
            PrimaryBiome = "The_Roots",
            Layer = VerticalLayer.GroundLevel,
            Position = new RoomPosition(1, 0, 0),
            Exits = new Dictionary<string, string>
            {
                ["west"] = "room_01",
                ["east"] = "room_02"
            },
            ItemsOnGround = new List<Equipment>
            {
                new Equipment { Name = "Scrap Metal", Type = EquipmentType.Accessory }
            }
        };

        // Room 4: Junction with Vertical Access (0, 2, 0) - North of Corridor
        dungeon.Rooms["room_04"] = new Room
        {
            RoomId = "room_04",
            Name = "Central Junction",
            Description = "You've reached a junction where several passages meet. A rusted metal staircase spirals downward into darkness, and you can feel cold air rising from below. The walls here are covered in strange fungal growths that pulse with a faint bioluminescence.",
            PrimaryBiome = "The_Roots",
            SecondaryBiome = "Niflheim",
            BiomeBlendRatio = 0.3f,
            Layer = VerticalLayer.GroundLevel,
            Position = new RoomPosition(0, 2, 0),
            Exits = new Dictionary<string, string>
            {
                ["south"] = "room_02",
                ["north"] = "room_05"
            },
            VerticalConnections = new List<VerticalConnection>
            {
                VerticalConnection.CreateStairs("room_04", "room_06", 1)
            },
            HasPuzzle = true,
            PuzzleDescription = "A control panel with flickering lights. Perhaps it controls something important."
        };

        // Room 5: Boss Chamber (0, 3, 0) - North of Junction
        dungeon.Rooms["room_05"] = new Room
        {
            RoomId = "room_05",
            Name = "Processing Core",
            Description = "A massive chamber opens before you, dominated by the hulking remains of an ancient processing core. Cables hang like dead vines from the ceiling, and the air thrums with residual Aetheric energy. Something massive stirs in the shadows...",
            PrimaryBiome = "The_Roots",
            Layer = VerticalLayer.GroundLevel,
            Position = new RoomPosition(0, 3, 0),
            IsBossRoom = true,
            Exits = new Dictionary<string, string>
            {
                ["south"] = "room_04"
            },
            Enemies = new List<Enemy>
            {
                new Enemy { Type = EnemyType.RuinWarden, HP = 80, MaxHP = 80, Name = "Rust Horror" }
            },
            PsychicResonance = PsychicResonanceLevel.Heavy
        };

        // Room 6: Lower Level (0, 2, -1) - Below Junction (Vertical connection target)
        dungeon.Rooms["room_06"] = new Room
        {
            RoomId = "room_06",
            Name = "Geothermal Access",
            Description = "The stairs have led you down to a lower level. Steam vents hiss from cracks in the floor, and the temperature here is noticeably higher. Ancient geothermal pipes snake along the walls, some still carrying scalding water.",
            PrimaryBiome = "Muspelheim",
            Layer = VerticalLayer.UpperRoots,
            Position = new RoomPosition(0, 2, -1),
            Exits = new Dictionary<string, string>(),
            VerticalConnections = new List<VerticalConnection>
            {
                new VerticalConnection
                {
                    ConnectionId = "vc_room_06_room_04_stairs",
                    FromRoomId = "room_06",
                    ToRoomId = "room_04",
                    Type = VerticalConnectionType.Stairs,
                    IsBidirectional = true,
                    Description = "Metal stairs lead back up to the junction above."
                }
            },
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDescription = "Steam vents release scalding bursts at irregular intervals.",
            HazardType = HazardType.Fire
        };

        // Initialize minimap with dungeon
        var startRoom = dungeon.GetStartRoom();
        if (startRoom != null)
        {
            Minimap.LoadDungeon(dungeon, startRoom);
        }

        CurrentDungeon = dungeon;
        CurrentRoom = startRoom;
        StatusMessage = "You descend into The Roots...";
    }

    /// <summary>
    /// Updates the room display after moving or state changes.
    /// </summary>
    private void UpdateRoomDisplay()
    {
        AvailableExits.Clear();
        RoomFeatures.Clear();

        if (CurrentRoom == null || CurrentDungeon == null) return;

        // Add cardinal direction exits
        foreach (var (direction, targetRoomId) in CurrentRoom.Exits)
        {
            AvailableExits.Add(new ExitViewModel
            {
                Direction = direction,
                DisplayName = GetDirectionDisplayName(direction),
                TargetRoomId = targetRoomId,
                IsVertical = false
            });
        }

        // Add vertical connections
        foreach (var vc in CurrentRoom.VerticalConnections.Where(c => c.CanTraverse()))
        {
            string direction = vc.FromRoomId == CurrentRoom.RoomId ? "down" : "up";
            string targetId = vc.FromRoomId == CurrentRoom.RoomId ? vc.ToRoomId : vc.FromRoomId;

            AvailableExits.Add(new ExitViewModel
            {
                Direction = direction,
                DisplayName = GetVerticalDisplayName(direction, vc.Type),
                TargetRoomId = targetId,
                IsVertical = true,
                VerticalType = vc.Type.ToString(),
                TraversalRequirements = vc.GetTraversalRequirementsDescription()
            });
        }

        // Add room features
        AddRoomFeatures();

        // Update property notifications
        this.RaisePropertyChanged(nameof(RoomName));
        this.RaisePropertyChanged(nameof(RoomDescription));
        this.RaisePropertyChanged(nameof(BiomeName));
        this.RaisePropertyChanged(nameof(BiomeColor));
        this.RaisePropertyChanged(nameof(LayerDisplay));
        this.RaisePropertyChanged(nameof(IsCleared));
        this.RaisePropertyChanged(nameof(HasEnemies));
        this.RaisePropertyChanged(nameof(IsSanctuary));
        this.RaisePropertyChanged(nameof(HazardWarning));

        IsSearched = false;
    }

    /// <summary>
    /// Adds features to the room features collection.
    /// </summary>
    private void AddRoomFeatures()
    {
        if (CurrentRoom == null) return;

        // Enemies
        if (CurrentRoom.Enemies.Count > 0 && !CurrentRoom.HasBeenCleared)
        {
            RoomFeatures.Add(new RoomFeatureViewModel
            {
                Icon = "!",
                Name = "Hostile Presence",
                Description = $"{CurrentRoom.Enemies.Count} enemies detected",
                FeatureType = "Hazard",
                IsInteractable = true
            });
        }

        // Environmental hazard
        if (CurrentRoom.HasEnvironmentalHazard && CurrentRoom.IsHazardActive)
        {
            RoomFeatures.Add(new RoomFeatureViewModel
            {
                Icon = "!",
                Name = GetHazardName(CurrentRoom.HazardType),
                Description = CurrentRoom.HazardDescription,
                FeatureType = "Hazard"
            });
        }

        // Puzzle
        if (CurrentRoom.HasPuzzle && !CurrentRoom.IsPuzzleSolved)
        {
            RoomFeatures.Add(new RoomFeatureViewModel
            {
                Icon = "?",
                Name = "Puzzle",
                Description = CurrentRoom.PuzzleDescription,
                FeatureType = "Puzzle",
                IsInteractable = true
            });
        }

        // Items on ground
        if (CurrentRoom.ItemsOnGround.Count > 0)
        {
            RoomFeatures.Add(new RoomFeatureViewModel
            {
                Icon = "*",
                Name = "Items",
                Description = $"{CurrentRoom.ItemsOnGround.Count} items on the ground",
                FeatureType = "Loot",
                IsInteractable = true
            });
        }

        // NPCs
        if (CurrentRoom.NPCs.Count > 0)
        {
            foreach (var npc in CurrentRoom.NPCs)
            {
                RoomFeatures.Add(new RoomFeatureViewModel
                {
                    Icon = "@",
                    Name = npc.Name,
                    Description = npc.Description ?? "A figure stands here.",
                    FeatureType = "NPC",
                    IsInteractable = true
                });
            }
        }

        // Sanctuary
        if (CurrentRoom.IsSanctuary)
        {
            RoomFeatures.Add(new RoomFeatureViewModel
            {
                Icon = "+",
                Name = "Sanctuary",
                Description = "This area feels safe. You can rest here without risk.",
                FeatureType = "Info"
            });
        }

        // Psychic resonance
        if (CurrentRoom.PsychicResonance != PsychicResonanceLevel.None)
        {
            RoomFeatures.Add(new RoomFeatureViewModel
            {
                Icon = "~",
                Name = "Psychic Resonance",
                Description = $"{CurrentRoom.PsychicResonance} Aetheric presence detected",
                FeatureType = CurrentRoom.PsychicResonance >= PsychicResonanceLevel.Heavy ? "Hazard" : "Info"
            });
        }

        // Biome transition
        if (CurrentRoom.IsTransitionRoom())
        {
            RoomFeatures.Add(new RoomFeatureViewModel
            {
                Icon = "~",
                Name = "Biome Transition",
                Description = $"Transitioning to {FormatBiomeName(CurrentRoom.SecondaryBiome)}",
                FeatureType = "Info"
            });
        }
    }

    /// <summary>
    /// Moves the player in the specified direction.
    /// </summary>
    private void Move(ExitViewModel exit)
    {
        if (CurrentDungeon == null || CurrentRoom == null) return;

        // Check for random encounter before moving
        if (HasEnemies && !CurrentRoom.HasBeenCleared)
        {
            StatusMessage = "Cannot leave - hostile presence blocks your path!";
            return;
        }

        var nextRoom = CurrentDungeon.GetRoom(exit.TargetRoomId);
        if (nextRoom != null)
        {
            string previousRoom = CurrentRoom.Name;
            CurrentRoom = nextRoom;

            // Update minimap with new room
            Minimap.UpdateCurrentRoom(nextRoom);

            // Check for random encounter on arrival
            if (ShouldTriggerEncounter())
            {
                StatusMessage = "Enemies approach!";
                // In full implementation, would trigger combat
            }
            else
            {
                StatusMessage = $"You move from {previousRoom} to {nextRoom.Name}.";
            }
        }
        else
        {
            StatusMessage = "The path is blocked.";
        }
    }

    /// <summary>
    /// Searches the current room asynchronously with visual feedback.
    /// </summary>
    private async Task SearchRoomAsync()
    {
        if (CurrentRoom == null || !CanSearch) return;

        IsSearching = true;
        StatusMessage = "Searching the area...";

        // Simulate search delay for visual feedback
        await Task.Delay(800);

        IsSearched = true;

        // Generate search results
        var rng = new Random();
        var roll = rng.NextDouble();

        if (roll < 0.35) // 35% chance to find loot
        {
            var lootItems = GenerateRandomLoot(rng);
            var secrets = GenerateEnvironmentalSecrets(rng);

            SearchResult = SearchResultViewModel.WithLoot(lootItems, secrets);
            StatusMessage = "Your search reveals something!";
        }
        else if (roll < 0.50) // 15% chance to trigger encounter
        {
            SearchResult = SearchResultViewModel.WithEncounter(
                "Your searching disturbs something lurking in the shadows...",
                null);
            StatusMessage = "Something stirs in the darkness!";
        }
        else if (roll < 0.70) // 20% chance to find secrets only
        {
            var secrets = GenerateEnvironmentalSecrets(rng);
            if (secrets.Any())
            {
                SearchResult = SearchResultViewModel.WithSecrets(secrets);
                StatusMessage = "You discover something interesting...";
            }
            else
            {
                SearchResult = SearchResultViewModel.Empty();
                StatusMessage = "Your search reveals nothing of interest.";
            }
        }
        else // 30% chance to find nothing
        {
            SearchResult = SearchResultViewModel.Empty();
            StatusMessage = "Your search reveals nothing of interest.";
        }

        IsSearching = false;
        this.RaisePropertyChanged(nameof(IsSearchResultVisible));
        this.RaisePropertyChanged(nameof(CanSearch));
    }

    /// <summary>
    /// Examines the current room in detail, showing all visible entities,
    /// exits, hazards, and running passive perception checks.
    /// v0.38: Enhanced with descriptor services for rich narrative content.
    /// </summary>
    private async Task LookRoomAsync()
    {
        if (CurrentRoom == null) return;

        StatusMessage = "You examine your surroundings carefully...";
        await Task.Delay(300);

        var details = new List<string>();
        int wits = Character?.Attributes?.Wits ?? 5;
        var rng = new Random();

        // v0.38: Generate rich room description using RoomDescriptorService
        if (_roomDescriptorService != null)
        {
            try
            {
                var biome = CurrentRoom.PrimaryBiome ?? "The_Roots";
                var roomArchetypeStr = CurrentRoom.Archetype.ToString();
                if (Enum.TryParse<RuneAndRust.Core.Descriptors.RoomArchetype>(roomArchetypeStr, true, out var roomArchetype))
                {
                    var generatedDescription = _roomDescriptorService.GenerateRoomDescription(roomArchetype, biome);
                    if (!string.IsNullOrEmpty(generatedDescription))
                    {
                        _narrativeText = generatedDescription;
                        this.RaisePropertyChanged(nameof(RoomDescription));
                    }
                }
            }
            catch
            {
                // Fall back to basic description if service fails
            }
        }

        // v0.38: Generate atmospheric description
        if (_atmosphericService != null)
        {
            try
            {
                var biome = CurrentRoom.PrimaryBiome ?? "The_Roots";
                var atmosphere = _atmosphericService.GenerateAtmosphere(biome);
                if (!string.IsNullOrEmpty(atmosphere))
                {
                    AtmosphereText = atmosphere;
                    this.RaisePropertyChanged(nameof(HasAtmosphere));
                }
            }
            catch
            {
                // Atmospheric generation failed, continue without
            }
        }

        // Room header
        details.Add($"[{BiomeName}] - {CurrentRoom.Name}");

        // Entities present
        if (CurrentRoom.Enemies?.Any() == true)
        {
            var enemyList = string.Join(", ", CurrentRoom.Enemies.Select(e => e.Name));
            details.Add($"Hostiles: {enemyList}");
        }

        if (CurrentRoom.NPCs?.Any() == true)
        {
            var npcList = string.Join(", ", CurrentRoom.NPCs.Select(n => n.Name));
            details.Add($"Present: {npcList}");
        }

        if (CurrentRoom.ItemsOnGround?.Any() == true)
        {
            var itemList = string.Join(", ", CurrentRoom.ItemsOnGround.Select(i => i.Name));
            details.Add($"Items: {itemList}");
        }

        // Hazards and conditions
        if (CurrentRoom.DynamicHazards?.Any() == true)
        {
            var hazards = string.Join(", ", CurrentRoom.DynamicHazards.Select(h => h.HazardName));
            details.Add($"Hazards: {hazards}");
        }

        if (CurrentRoom.AmbientConditions?.Any() == true)
        {
            var conditions = string.Join(", ", CurrentRoom.AmbientConditions.Select(c => c.ConditionName));
            details.Add($"Conditions: {conditions}");
        }

        // Passive perception check for hidden elements
        int perceptionRoll = rng.Next(1, 21) + (wits - 5);

        // v0.38: Use ExaminationFlavorTextService for perception checks
        if (_examinationService != null)
        {
            try
            {
                if (perceptionRoll >= 12 && CurrentRoom.DynamicHazards?.Any(h => h.IsHidden && !h.HasBeenDiscovered) == true)
                {
                    var perceptionText = _examinationService.GeneratePerceptionCheckText(
                        "HiddenTrap", perceptionRoll, CurrentRoom.PrimaryBiome ?? "The_Roots");
                    if (!string.IsNullOrEmpty(perceptionText))
                    {
                        details.Add($"*{perceptionText}*");
                    }
                    else
                    {
                        details.Add("*You notice subtle signs of danger - a trap may be nearby.*");
                    }
                }

                if (perceptionRoll >= 15 && CurrentRoom.HasSecretExit && !CurrentRoom.SecretExitRevealed)
                {
                    var perceptionText = _examinationService.GeneratePerceptionCheckText(
                        "SecretDoor", perceptionRoll, CurrentRoom.PrimaryBiome ?? "The_Roots");
                    if (!string.IsNullOrEmpty(perceptionText))
                    {
                        details.Add($"*{perceptionText}*");
                    }
                    else
                    {
                        details.Add("*Something about the walls here seems... off. There may be a hidden passage.*");
                    }
                }
            }
            catch
            {
                // Fall back to hardcoded perception text
                if (perceptionRoll >= 12 && CurrentRoom.DynamicHazards?.Any(h => h.IsHidden && !h.HasBeenDiscovered) == true)
                {
                    details.Add("*You notice subtle signs of danger - a trap may be nearby.*");
                }

                if (perceptionRoll >= 15 && CurrentRoom.HasSecretExit && !CurrentRoom.SecretExitRevealed)
                {
                    details.Add("*Something about the walls here seems... off. There may be a hidden passage.*");
                }
            }
        }
        else
        {
            // No service available, use fallback
            if (perceptionRoll >= 12 && CurrentRoom.DynamicHazards?.Any(h => h.IsHidden && !h.HasBeenDiscovered) == true)
            {
                details.Add("*You notice subtle signs of danger - a trap may be nearby.*");
            }

            if (perceptionRoll >= 15 && CurrentRoom.HasSecretExit && !CurrentRoom.SecretExitRevealed)
            {
                details.Add("*Something about the walls here seems... off. There may be a hidden passage.*");
            }
        }

        // Available exits
        var exitList = new List<string>();
        if (CurrentRoom.Exits != null)
        {
            foreach (var exit in CurrentRoom.Exits)
            {
                exitList.Add(exit.Key.ToString());
            }
        }
        if (exitList.Any())
        {
            details.Add($"Exits: {string.Join(", ", exitList)}");
        }

        // Update room features to refresh the display
        UpdateRoomDisplay();

        StatusMessage = string.Join(" | ", details.Take(3)) + (details.Count > 3 ? "..." : "");
    }

    /// <summary>
    /// Investigates objects in the room using a WITS-based skill check
    /// to reveal hidden details, mechanisms, and lore.
    /// v0.38: Enhanced with ExaminationFlavorTextService for rich narrative.
    /// </summary>
    private async Task InvestigateRoomAsync()
    {
        if (CurrentRoom == null) return;

        StatusMessage = "You investigate the area more closely...";
        await Task.Delay(500);

        // Get WITS for skill check
        int wits = Character?.Attributes?.Wits ?? 5;
        var rng = new Random();
        int investigationRoll = rng.Next(1, 21) + (wits - 5); // d20 + WITS modifier

        var findings = new List<string>();
        bool foundSomething = false;
        var biome = CurrentRoom.PrimaryBiome ?? "The_Roots";

        // Determine detail level based on roll (v0.38 tiered system)
        string detailLevel = investigationRoll >= 18 ? "Expert"
            : investigationRoll >= 12 ? "Detailed"
            : "Cursory";

        // Check static terrain
        if (CurrentRoom.StaticTerrain?.Any(t => t.IsInteractive && !t.HasBeenInvestigated) == true)
        {
            var terrain = CurrentRoom.StaticTerrain.First(t => t.IsInteractive && !t.HasBeenInvestigated);
            if (investigationRoll >= 10)
            {
                // v0.38: Use ExaminationFlavorTextService if available
                if (_examinationService != null)
                {
                    try
                    {
                        // GenerateExaminationText(objectCategory, objectType, witsCheck, objectState, biomeName)
                        var examText = _examinationService.GenerateExaminationText(
                            "Terrain", terrain.Name, investigationRoll, null, biome);

                        if (!string.IsNullOrEmpty(examText))
                        {
                            findings.Add(examText);
                        }
                        else
                        {
                            findings.Add($"Examining the {terrain.Name}: {terrain.FlavorText}");
                        }
                    }
                    catch
                    {
                        findings.Add($"Examining the {terrain.Name}: {terrain.FlavorText}");
                    }
                }
                else
                {
                    findings.Add($"Examining the {terrain.Name}: {terrain.FlavorText}");
                }
                terrain.HasBeenInvestigated = true;
                foundSomething = true;
            }
        }

        // Check loot nodes with hidden content
        if (CurrentRoom.LootNodes?.Any(l => l.RequiresInvestigation && !l.HiddenContentRevealed) == true)
        {
            var lootNode = CurrentRoom.LootNodes.First(l => l.RequiresInvestigation && !l.HiddenContentRevealed);
            if (investigationRoll >= 12)
            {
                findings.Add($"You discover a hidden compartment in the {lootNode.NodeType}!");
                lootNode.HiddenContentRevealed = true;
                foundSomething = true;
            }
        }

        // Check hazards to learn disable mechanisms
        if (CurrentRoom.DynamicHazards?.Any(h => !h.HasBeenDiscovered) == true)
        {
            var hazard = CurrentRoom.DynamicHazards.First(h => !h.HasBeenDiscovered);
            if (investigationRoll >= 14)
            {
                findings.Add($"You identify a {hazard.HazardName} and understand how to avoid it.");
                hazard.HasBeenDiscovered = true;
                foundSomething = true;
            }
        }

        // Lore/expert level findings at high rolls
        if (investigationRoll >= 18)
        {
            // v0.38: Use ExaminationFlavorTextService for lore fragments
            string loreText = null!;
            if (_examinationService != null)
            {
                try
                {
                    var fragment = _examinationService.GetLoreFragment(biome);
                    loreText = fragment?.LoreText;
                }
                catch
                {
                    // Fall back to hardcoded lore
                }
            }

            if (string.IsNullOrEmpty(loreText))
            {
                var loreFragments = new[]
                {
                    "Ancient Dvergr runes on the wall speak of a great forge that once burned here.",
                    "The architecture suggests this was a place of importance to the old ones.",
                    "Faded markings indicate this area was once sealed - whatever was kept here has long escaped.",
                    "You find traces of an old battle - rust and bone fragments suggest a desperate last stand."
                };
                loreText = loreFragments[rng.Next(loreFragments.Length)];
            }

            findings.Add($"[Lore] {loreText}");
            foundSomething = true;
        }

        // Report findings
        if (foundSomething)
        {
            StatusMessage = $"[WITS check: {investigationRoll}] " + string.Join(" ", findings);
            UpdateRoomDisplay();
        }
        else if (investigationRoll < 10)
        {
            StatusMessage = $"[WITS check: {investigationRoll}] Your investigation reveals nothing new. Try again?";
        }
        else
        {
            StatusMessage = $"[WITS check: {investigationRoll}] The area has been thoroughly examined. Nothing more to find.";
        }
    }

    /// <summary>
    /// Generates random loot items for search results.
    /// </summary>
    private List<LootItemViewModel> GenerateRandomLoot(Random rng)
    {
        var items = new List<LootItemViewModel>();
        var numItems = rng.Next(1, 4); // 1-3 items

        for (int i = 0; i < numItems; i++)
        {
            var itemType = rng.Next(100);
            if (itemType < 40) // 40% materials
            {
                var materials = new[] { "Scrap Metal", "Corroded Wire", "Crystalline Shard", "Machine Oil", "Rusted Gears" };
                items.Add(LootItemViewModel.ForMaterial(
                    materials[rng.Next(materials.Length)],
                    "Salvageable materials useful for crafting.",
                    rng.Next(1, 4)));
            }
            else if (itemType < 60) // 20% currency
            {
                var currencyType = rng.Next(2) == 0 ? "Scrap" : "Aether Shards";
                items.Add(LootItemViewModel.ForCurrency(currencyType, rng.Next(5, 25)));
            }
            else // 40% equipment
            {
                var equipment = new Equipment
                {
                    Name = GenerateRandomItemName(rng),
                    Type = (EquipmentType)rng.Next(3),
                    Quality = (QualityTier)rng.Next(3),
                    Description = "A piece of salvaged equipment.",
                    DamageDice = rng.Next(1, 3),
                    DamageDieSize = 6,
                    DefenseBonus = rng.Next(1, 5)
                };
                items.Add(LootItemViewModel.FromEquipment(equipment));
            }
        }

        return items;
    }

    /// <summary>
    /// Generates a random item name.
    /// </summary>
    private string GenerateRandomItemName(Random rng)
    {
        var prefixes = new[] { "Corroded", "Salvaged", "Ancient", "Damaged", "Modified" };
        var items = new[] { "Blade", "Shield", "Gauntlet", "Helm", "Ring" };
        return $"{prefixes[rng.Next(prefixes.Length)]} {items[rng.Next(items.Length)]}";
    }

    /// <summary>
    /// Generates environmental secrets/discoveries.
    /// </summary>
    private List<string> GenerateEnvironmentalSecrets(Random rng)
    {
        var secrets = new List<string>();
        var allSecrets = new[]
        {
            "Faded runes on the wall seem to depict an ancient ritual.",
            "You find scratch marks suggesting something large passed through here.",
            "A hidden compartment contains old documents, mostly illegible.",
            "The temperature here is noticeably different than nearby areas.",
            "You notice strange stains on the floor that seem to shimmer faintly.",
            "An old terminal flickers with corrupted data logs.",
            "The walls bear evidence of a past battle.",
            "You discover a hidden maintenance passage (now collapsed)."
        };

        if (rng.NextDouble() < 0.6) // 60% chance to find at least one secret
        {
            var numSecrets = rng.Next(1, 3);
            var shuffled = allSecrets.OrderBy(_ => rng.Next()).Take(numSecrets);
            secrets.AddRange(shuffled);
        }

        return secrets;
    }

    /// <summary>
    /// Collects loot from search results.
    /// </summary>
    private void CollectLoot()
    {
        if (SearchResult == null || !SearchResult.FoundLoot || Character == null) return;

        // Add items to character inventory (simplified for demo)
        foreach (var item in SearchResult.LootItems)
        {
            if (item.Equipment != null)
            {
                Character.Inventory.Add(item.Equipment);
            }
        }

        SearchResult.IsCollected = true;
        StatusMessage = $"Collected {SearchResult.LootItems.Count} item(s).";

        // Auto-close after collection
        CloseSearchResult();
    }

    /// <summary>
    /// Closes the search result overlay.
    /// </summary>
    private void CloseSearchResult()
    {
        // If encounter was triggered, navigate to combat
        if (SearchResult?.TriggeredEncounter == true)
        {
            SearchResult = null;
            this.RaisePropertyChanged(nameof(IsSearchResultVisible));
            _navigationService?.NavigateTo<CombatViewModel>();
            return;
        }

        SearchResult = null;
        this.RaisePropertyChanged(nameof(IsSearchResultVisible));
    }

    /// <summary>
    /// Shows the rest confirmation dialog.
    /// </summary>
    private void ShowRestDialog()
    {
        if (CurrentRoom == null || Character == null) return;

        if (HasEnemies && !CurrentRoom.HasBeenCleared)
        {
            StatusMessage = "Cannot rest with enemies nearby!";
            return;
        }

        if (CurrentRoom.IsSanctuary)
        {
            RestConfirmation = RestConfirmationViewModel.ForSanctuary(
                Character.HP, Character.MaxHP,
                Character.Stamina, Character.MaxStamina);
        }
        else
        {
            RestConfirmation = RestConfirmationViewModel.ForRegularRest(
                Character.HP, Character.MaxHP,
                Character.Stamina, Character.MaxStamina,
                Character.PsychicStress);
        }

        IsRestDialogVisible = true;
    }

    /// <summary>
    /// Confirms and executes rest.
    /// </summary>
    private void ConfirmRest()
    {
        if (CurrentRoom == null || Character == null || RestConfirmation == null) return;

        if (RestConfirmation.IsSanctuary)
        {
            // Sanctuary rest - full recovery, no stress
            Character.HP = Character.MaxHP;
            Character.Stamina = Character.MaxStamina;
            StatusMessage = "You rest peacefully in the sanctuary. Fully restored.";
        }
        else
        {
            // Normal rest - partial recovery, stress risk
            Character.HP = Math.Min(Character.MaxHP, Character.HP + Character.MaxHP / 4);
            Character.Stamina = Math.Min(Character.MaxStamina, Character.Stamina + Character.MaxStamina / 2);
            Character.PsychicStress = Math.Min(100, Character.PsychicStress + 5);
            StatusMessage = "You rest uneasily. Partially restored, but stress increases.";
        }

        this.RaisePropertyChanged(nameof(HPDisplay));
        this.RaisePropertyChanged(nameof(StaminaDisplay));
        this.RaisePropertyChanged(nameof(StressDisplay));

        CancelRest(); // Close dialog
    }

    /// <summary>
    /// Cancels the rest dialog.
    /// </summary>
    private void CancelRest()
    {
        IsRestDialogVisible = false;
        RestConfirmation = null;
    }

    /// <summary>
    /// Navigates to character sheet.
    /// </summary>
    private void ViewCharacter()
    {
        _navigationService?.NavigateTo<CharacterSheetViewModel>();
    }

    /// <summary>
    /// Navigates to inventory.
    /// </summary>
    private void ViewInventory()
    {
        _navigationService?.NavigateTo<InventoryViewModel>();
    }

    /// <summary>
    /// Interacts with a room feature.
    /// </summary>
    private void Interact(RoomFeatureViewModel feature)
    {
        StatusMessage = $"Interacting with {feature.Name}...";
        // In full implementation, would handle feature-specific interactions
    }

    /// <summary>
    /// Engages enemies in combat.
    /// </summary>
    private void EngageEnemies()
    {
        if (!HasEnemies)
        {
            StatusMessage = "No enemies to engage.";
            return;
        }

        StatusMessage = "Engaging hostiles!";
        _navigationService?.NavigateTo<CombatViewModel>();
    }

    /// <summary>
    /// Toggles minimap visibility.
    /// </summary>
    private void ToggleMinimap()
    {
        IsMinimapVisible = !IsMinimapVisible;
        StatusMessage = IsMinimapVisible ? "Map opened." : "Map closed.";
    }

    /// <summary>
    /// Determines if a random encounter should trigger.
    /// </summary>
    private bool ShouldTriggerEncounter()
    {
        return new Random().NextDouble() < 0.15; // 15% chance
    }

    /// <summary>
    /// Gets the display name for a cardinal direction.
    /// </summary>
    private static string GetDirectionDisplayName(string direction)
    {
        return direction.ToLower() switch
        {
            "north" => "North",
            "south" => "South",
            "east" => "East",
            "west" => "West",
            _ => direction
        };
    }

    /// <summary>
    /// Gets the display name for a vertical direction.
    /// </summary>
    private static string GetVerticalDisplayName(string direction, VerticalConnectionType type)
    {
        string typeStr = type switch
        {
            VerticalConnectionType.Stairs => "Stairs",
            VerticalConnectionType.Ladder => "Ladder",
            VerticalConnectionType.Shaft => "Shaft",
            VerticalConnectionType.Elevator => "Elevator",
            _ => "Passage"
        };

        return direction == "up" ? $"Up ({typeStr})" : $"Down ({typeStr})";
    }

    /// <summary>
    /// Formats a biome name for display.
    /// </summary>
    private static string FormatBiomeName(string? biome)
    {
        if (string.IsNullOrEmpty(biome)) return "Unknown";

        return biome.Replace("_", " ") switch
        {
            "The Roots" => "The Roots",
            "Muspelheim" => "Muspelheim (Fire)",
            "Niflheim" => "Niflheim (Ice)",
            "Jotunheim" => "Jotunheim (Machine)",
            "Alfheim" => "Alfheim (Light)",
            _ => biome.Replace("_", " ")
        };
    }

    /// <summary>
    /// Gets the color for a biome.
    /// </summary>
    private static string GetBiomeColor(string? biome)
    {
        return biome?.ToLower() switch
        {
            "the_roots" => "#8B4513",      // Brown
            "muspelheim" => "#FF4500",     // Orange-red
            "niflheim" => "#4169E1",       // Royal blue
            "jotunheim" => "#708090",      // Slate gray
            "alfheim" => "#FFD700",        // Gold
            _ => "#666666"                  // Default gray
        };
    }

    /// <summary>
    /// Gets the hazard type name.
    /// </summary>
    private static string GetHazardName(HazardType type)
    {
        return type switch
        {
            HazardType.Fire => "Fire Hazard",
            HazardType.ElectricalHazard => "Electrical Hazard",
            HazardType.ToxicFumes => "Toxic Hazard",
            HazardType.ToxicSludge => "Toxic Hazard",
            HazardType.Radiation => "Radiation Hazard",
            HazardType.Ice => "Ice Hazard",
            HazardType.Darkness => "Darkness Hazard",
            HazardType.UnstableFlooring => "Unstable Ground",
            _ => "Environmental Hazard"
        };
    }

    #endregion
}
