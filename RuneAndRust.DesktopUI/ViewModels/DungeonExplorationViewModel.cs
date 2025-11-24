using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Room description text.
    /// </summary>
    public string RoomDescription => CurrentRoom?.Description ?? "You are in an unknown location.";

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

    #endregion

    public DungeonExplorationViewModel()
    {
        // Initialize commands
        MoveCommand = ReactiveCommand.Create<ExitViewModel>(Move);
        SearchRoomCommand = ReactiveCommand.Create(SearchRoom);
        RestCommand = ReactiveCommand.Create(Rest);
        ViewCharacterCommand = ReactiveCommand.Create(ViewCharacter);
        ViewInventoryCommand = ReactiveCommand.Create(ViewInventory);
        InteractCommand = ReactiveCommand.Create<RoomFeatureViewModel>(Interact);
        EngageCommand = ReactiveCommand.Create(EngageEnemies);
        ToggleMinimapCommand = ReactiveCommand.Create(ToggleMinimap);

        // Load demo dungeon
        LoadDemoDungeon();
    }

    public DungeonExplorationViewModel(INavigationService navigationService) : this()
    {
        _navigationService = navigationService;
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
            HazardType = HazardType.Electrical
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
                new Equipment { Name = "Scrap Metal", EquipmentType = EquipmentType.Material }
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
                new Enemy { Type = EnemyType.RustHorror, CurrentHP = 80, MaxHP = 80 }
            },
            PsychicResonance = PsychicResonanceLevel.High
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
                FeatureType = CurrentRoom.PsychicResonance >= PsychicResonanceLevel.High ? "Hazard" : "Info"
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
    /// Searches the current room.
    /// </summary>
    private void SearchRoom()
    {
        if (CurrentRoom == null) return;

        if (IsSearched)
        {
            StatusMessage = "You've already thoroughly searched this area.";
            return;
        }

        IsSearched = true;

        // Simulate search results
        var rng = new Random();
        if (rng.NextDouble() < 0.3) // 30% chance to find something
        {
            StatusMessage = "Your search reveals a hidden compartment containing salvage!";
            // In full implementation, would add items to inventory
        }
        else if (rng.NextDouble() < 0.15) // 15% chance to trigger encounter
        {
            StatusMessage = "Your searching disturbs something in the shadows...";
            // In full implementation, would trigger combat
        }
        else
        {
            StatusMessage = "Your search reveals nothing of interest.";
        }
    }

    /// <summary>
    /// Rests in the current room.
    /// </summary>
    private void Rest()
    {
        if (CurrentRoom == null || Character == null) return;

        if (HasEnemies && !CurrentRoom.HasBeenCleared)
        {
            StatusMessage = "Cannot rest with enemies nearby!";
            return;
        }

        if (CurrentRoom.IsSanctuary)
        {
            // Sanctuary rest - full recovery, no stress
            Character.HP = Character.MaxHP;
            Character.Stamina = Character.MaxStamina;
            StatusMessage = "You rest in the sanctuary. HP and Stamina fully restored.";
        }
        else
        {
            // Normal rest - partial recovery, stress risk
            Character.HP = Math.Min(Character.MaxHP, Character.HP + Character.MaxHP / 4);
            Character.Stamina = Math.Min(Character.MaxStamina, Character.Stamina + Character.MaxStamina / 2);
            Character.PsychicStress = Math.Min(100, Character.PsychicStress + 5);
            StatusMessage = "You rest uneasily. HP/Stamina partially restored, but stress increases.";
        }

        this.RaisePropertyChanged(nameof(HPDisplay));
        this.RaisePropertyChanged(nameof(StaminaDisplay));
        this.RaisePropertyChanged(nameof(StressDisplay));
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
            HazardType.Electrical => "Electrical Hazard",
            HazardType.Toxic => "Toxic Hazard",
            HazardType.Psychic => "Psychic Hazard",
            HazardType.Physical => "Physical Hazard",
            _ => "Environmental Hazard"
        };
    }

    #endregion
}
