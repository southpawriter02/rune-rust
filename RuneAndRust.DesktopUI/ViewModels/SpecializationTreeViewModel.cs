using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Controllers;
using RuneAndRust.Engine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// Represents a single ability node in the specialization tree.
/// Displays ability details, unlock status, and rank progression.
/// </summary>
public class AbilityNodeViewModel : ViewModelBase
{
    private readonly PlayerCharacter _character;
    private bool _isUnlocked;
    private int _currentRank;

    /// <summary>
    /// The underlying ability data.
    /// </summary>
    public AbilityData Ability { get; }

    /// <summary>
    /// Ability display name.
    /// </summary>
    public string Name => Ability.Name;

    /// <summary>
    /// Ability description text.
    /// </summary>
    public string Description => Ability.Description;

    /// <summary>
    /// Mechanical summary of the ability's effects.
    /// </summary>
    public string MechanicalSummary => Ability.MechanicalSummary;

    /// <summary>
    /// The tier level (1=Foundation, 2=Core, 3=Advanced, 4=Mastery).
    /// </summary>
    public int TierLevel => Ability.TierLevel;

    /// <summary>
    /// Tier display name.
    /// </summary>
    public string TierName => TierLevel switch
    {
        1 => "Foundation",
        2 => "Core",
        3 => "Advanced",
        4 => "Mastery",
        _ => "Unknown"
    };

    /// <summary>
    /// Whether this ability has been unlocked.
    /// </summary>
    public bool IsUnlocked
    {
        get => _isUnlocked;
        set => this.RaiseAndSetIfChanged(ref _isUnlocked, value);
    }

    /// <summary>
    /// Current rank of the ability (0 if not unlocked, 1-3 if unlocked).
    /// </summary>
    public int CurrentRank
    {
        get => _currentRank;
        set => this.RaiseAndSetIfChanged(ref _currentRank, value);
    }

    /// <summary>
    /// Maximum rank this ability can reach.
    /// </summary>
    public int MaxRank => Ability.MaxRank;

    /// <summary>
    /// PP cost to unlock this ability.
    /// </summary>
    public int UnlockCost => Ability.PPCost;

    /// <summary>
    /// PP cost to rank up to Rank 2.
    /// </summary>
    public int RankUpCostToRank2 => Ability.CostToRank2;

    /// <summary>
    /// PP cost to rank up to Rank 3.
    /// </summary>
    public int RankUpCostToRank3 => Ability.CostToRank3;

    /// <summary>
    /// Current rank up cost (0 if max rank reached).
    /// </summary>
    public int RankUpCost
    {
        get
        {
            if (!IsUnlocked || CurrentRank >= MaxRank) return 0;
            return CurrentRank == 1 ? RankUpCostToRank2 : RankUpCostToRank3;
        }
    }

    /// <summary>
    /// Whether the ability can be unlocked (has PP, meets prerequisites, not already unlocked).
    /// </summary>
    public bool CanUnlock
    {
        get
        {
            if (IsUnlocked) return false;
            if (_character.ProgressionPoints < UnlockCost) return false;

            // Check prerequisites
            foreach (var prereqId in Ability.Prerequisites.RequiredAbilityIDs)
            {
                // In demo mode, check if character has the ability in their list
                if (!_character.Abilities.Any(a => a.Name.GetHashCode() == prereqId))
                    return false;
            }

            // Check tier requirements (PP spent in tree)
            // Simplified: Tier 1 = 0 PP, Tier 2 = 8 PP, Tier 3 = 16 PP, Tier 4 = 24 PP
            int requiredPPInTree = (TierLevel - 1) * 8;
            if (Ability.Prerequisites.RequiredPPInTree > 0)
                requiredPPInTree = Math.Max(requiredPPInTree, Ability.Prerequisites.RequiredPPInTree);

            return true;
        }
    }

    /// <summary>
    /// Whether the ability can be ranked up.
    /// </summary>
    public bool CanRankUp
    {
        get
        {
            if (!IsUnlocked) return false;
            if (CurrentRank >= MaxRank) return false;
            if (RankUpCost == 0) return false; // Rank not available
            if (_character.ProgressionPoints < RankUpCost) return false;
            return true;
        }
    }

    /// <summary>
    /// Display text for prerequisites.
    /// </summary>
    public string PrerequisiteText
    {
        get
        {
            if (Ability.Prerequisites.RequiredAbilityIDs.Count == 0 &&
                Ability.Prerequisites.RequiredPPInTree == 0)
                return "No prerequisites";

            var prereqs = new List<string>();

            if (Ability.Prerequisites.RequiredPPInTree > 0)
                prereqs.Add($"{Ability.Prerequisites.RequiredPPInTree} PP in tree");

            if (Ability.Prerequisites.RequiredAbilityIDs.Count > 0)
                prereqs.Add($"{Ability.Prerequisites.RequiredAbilityIDs.Count} abilities required");

            return $"Requires: {string.Join(", ", prereqs)}";
        }
    }

    /// <summary>
    /// Ability type display (Active/Passive/Reaction).
    /// </summary>
    public string AbilityType => Ability.AbilityType;

    /// <summary>
    /// Action type display (Standard/Bonus/Free/Reaction).
    /// </summary>
    public string ActionType => Ability.ActionType;

    /// <summary>
    /// Target type display.
    /// </summary>
    public string TargetType => Ability.TargetType;

    /// <summary>
    /// Resource cost display.
    /// </summary>
    public string ResourceCostText
    {
        get
        {
            var costs = new List<string>();
            if (Ability.ResourceCost.Stamina > 0)
                costs.Add($"{Ability.ResourceCost.Stamina} Stamina");
            if (Ability.ResourceCost.Stress > 0)
                costs.Add($"{Ability.ResourceCost.Stress} Stress");
            if (Ability.ResourceCost.Corruption > 0)
                costs.Add($"{Ability.ResourceCost.Corruption} Corruption");
            if (Ability.ResourceCost.HP > 0)
                costs.Add($"{Ability.ResourceCost.HP} HP");

            return costs.Count > 0 ? string.Join(", ", costs) : "None";
        }
    }

    /// <summary>
    /// Cooldown display text.
    /// </summary>
    public string CooldownText
    {
        get
        {
            if (Ability.CooldownType == "None" || Ability.CooldownTurns == 0)
                return "None";

            return Ability.CooldownType switch
            {
                "Per Combat" => "Once per combat",
                "Per Expedition" => "Once per expedition",
                "Per Day" => "Once per day",
                _ => $"{Ability.CooldownTurns} turns"
            };
        }
    }

    /// <summary>
    /// Background color based on unlock status.
    /// </summary>
    public string BackgroundColor => IsUnlocked ? "#3C5C3C" : "#2C2C2C";

    /// <summary>
    /// Border color based on unlock/available status.
    /// </summary>
    public string BorderColor
    {
        get
        {
            if (IsUnlocked) return "#4CAF50";
            if (CanUnlock) return "#FFD700";
            return "#444444";
        }
    }

    /// <summary>
    /// Rank display text (e.g., "2/3").
    /// </summary>
    public string RankDisplay => IsUnlocked ? $"{CurrentRank}/{MaxRank}" : $"0/{MaxRank}";

    /// <summary>
    /// Button text for unlock/rank up action.
    /// </summary>
    public string ActionButtonText
    {
        get
        {
            if (!IsUnlocked)
                return $"Unlock ({UnlockCost} PP)";
            if (CanRankUp)
                return $"Rank Up ({RankUpCost} PP)";
            return "Max Rank";
        }
    }

    public AbilityNodeViewModel(AbilityData ability, PlayerCharacter character, bool isUnlocked = false, int currentRank = 0)
    {
        Ability = ability;
        _character = character;
        _isUnlocked = isUnlocked;
        _currentRank = isUnlocked ? Math.Max(1, currentRank) : 0;
    }

    /// <summary>
    /// Refresh the node's computed properties after state changes.
    /// </summary>
    public void RefreshState()
    {
        this.RaisePropertyChanged(nameof(CanUnlock));
        this.RaisePropertyChanged(nameof(CanRankUp));
        this.RaisePropertyChanged(nameof(BackgroundColor));
        this.RaisePropertyChanged(nameof(BorderColor));
        this.RaisePropertyChanged(nameof(RankDisplay));
        this.RaisePropertyChanged(nameof(ActionButtonText));
        this.RaisePropertyChanged(nameof(RankUpCost));
    }
}

/// <summary>
/// ViewModel for the Specialization Tree view.
/// Displays ability tree organized by tiers, handles PP spending for unlocks and rank ups.
/// v0.44.5: Integrates with ProgressionController for milestone/level-up workflow.
/// </summary>
public class SpecializationTreeViewModel : ViewModelBase
{
    private readonly GameStateController? _gameStateController;
    private readonly ProgressionController? _progressionController;
    private PlayerCharacter? _character;
    private SpecializationData? _specializationData;
    private AbilityNodeViewModel? _selectedNode;
    private int _ppSpentInTree;
    private string _statusMessage = string.Empty;
    private bool _isInProgressionMode;

    /// <summary>
    /// Foundation tier abilities (Tier 1).
    /// </summary>
    public ObservableCollection<AbilityNodeViewModel> FoundationTier { get; } = new();

    /// <summary>
    /// Core tier abilities (Tier 2).
    /// </summary>
    public ObservableCollection<AbilityNodeViewModel> CoreTier { get; } = new();

    /// <summary>
    /// Advanced tier abilities (Tier 3).
    /// </summary>
    public ObservableCollection<AbilityNodeViewModel> AdvancedTier { get; } = new();

    /// <summary>
    /// Mastery tier abilities (Tier 4).
    /// </summary>
    public ObservableCollection<AbilityNodeViewModel> MasteryTier { get; } = new();

    /// <summary>
    /// The current character.
    /// </summary>
    public PlayerCharacter? Character
    {
        get => _character;
        private set => this.RaiseAndSetIfChanged(ref _character, value);
    }

    /// <summary>
    /// The specialization data.
    /// </summary>
    public SpecializationData? SpecializationData
    {
        get => _specializationData;
        private set => this.RaiseAndSetIfChanged(ref _specializationData, value);
    }

    /// <summary>
    /// Specialization display name.
    /// </summary>
    public string SpecializationName => SpecializationData?.Name ?? "No Specialization";

    /// <summary>
    /// Specialization description.
    /// </summary>
    public string SpecializationDescription => SpecializationData?.Description ?? "";

    /// <summary>
    /// Specialization tagline.
    /// </summary>
    public string SpecializationTagline => SpecializationData?.Tagline ?? "";

    /// <summary>
    /// Specialization icon emoji.
    /// </summary>
    public string SpecializationIcon => SpecializationData?.IconEmoji ?? "?";

    /// <summary>
    /// Available PP for spending.
    /// </summary>
    public int AvailablePP => Character?.ProgressionPoints ?? 0;

    /// <summary>
    /// PP spent in the current tree.
    /// </summary>
    public int PPSpentInTree
    {
        get => _ppSpentInTree;
        set => this.RaiseAndSetIfChanged(ref _ppSpentInTree, value);
    }

    /// <summary>
    /// Currently selected ability node.
    /// </summary>
    public AbilityNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set => this.RaiseAndSetIfChanged(ref _selectedNode, value);
    }

    /// <summary>
    /// Status message for feedback.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    #region v0.44.5: Progression Mode Properties

    /// <summary>
    /// Whether the view is in progression (level-up) mode.
    /// </summary>
    public bool IsInProgressionMode
    {
        get => _isInProgressionMode;
        private set => this.RaiseAndSetIfChanged(ref _isInProgressionMode, value);
    }

    /// <summary>
    /// Gets the progression summary text.
    /// </summary>
    public string ProgressionSummary => _progressionController?.GetProgressionSummary() ?? string.Empty;

    /// <summary>
    /// Gets whether we can complete progression (always true for now).
    /// </summary>
    public bool CanCompleteProgression => IsInProgressionMode;

    #endregion

    /// <summary>
    /// Command to unlock an ability.
    /// </summary>
    public ICommand UnlockAbilityCommand { get; }

    /// <summary>
    /// Command to rank up an ability.
    /// </summary>
    public ICommand RankUpAbilityCommand { get; }

    /// <summary>
    /// Command to select an ability node.
    /// </summary>
    public ICommand SelectNodeCommand { get; }

    /// <summary>
    /// v0.44.5: Command to complete progression and return to exploration.
    /// </summary>
    public ICommand CompleteProgressionCommand { get; }

    /// <summary>
    /// v0.44.5: Command to skip progression (save points for later).
    /// </summary>
    public ICommand SkipProgressionCommand { get; }

    /// <summary>
    /// v0.44.5: Constructor with optional controller dependencies for game integration.
    /// </summary>
    public SpecializationTreeViewModel(
        GameStateController? gameStateController = null,
        ProgressionController? progressionController = null)
    {
        _gameStateController = gameStateController;
        _progressionController = progressionController;

        UnlockAbilityCommand = ReactiveCommand.Create<AbilityNodeViewModel>(UnlockAbility);
        RankUpAbilityCommand = ReactiveCommand.Create<AbilityNodeViewModel>(RankUpAbility);
        SelectNodeCommand = ReactiveCommand.Create<AbilityNodeViewModel>(node => SelectedNode = node);

        // v0.44.5: Progression mode commands
        CompleteProgressionCommand = ReactiveCommand.CreateFromTask(CompleteProgressionAsync);
        SkipProgressionCommand = ReactiveCommand.CreateFromTask(SkipProgressionAsync);

        // Load character from game state or use demo
        if (_gameStateController?.HasActiveGame == true)
        {
            Character = _gameStateController.CurrentGameState.Player;
            CheckProgressionMode();
            LoadAbilityTree();
        }
        else
        {
            // Load demo data
            LoadDemoCharacter();
        }
    }

    /// <summary>
    /// v0.44.5: Checks if we're in progression mode.
    /// </summary>
    private void CheckProgressionMode()
    {
        if (_gameStateController?.CurrentGameState.CurrentPhase == GamePhase.CharacterProgression)
        {
            IsInProgressionMode = true;
            StatusMessage = "Milestone reached! Spend your Progression Points.";
        }
        else
        {
            IsInProgressionMode = false;
        }
    }

    /// <summary>
    /// v0.44.5: Completes progression and returns to exploration.
    /// </summary>
    private async Task CompleteProgressionAsync()
    {
        if (_progressionController != null)
        {
            await _progressionController.CompleteProgressionAsync();
        }
        IsInProgressionMode = false;
    }

    /// <summary>
    /// v0.44.5: Skips progression (saves points for later).
    /// </summary>
    private async Task SkipProgressionAsync()
    {
        if (_progressionController != null)
        {
            await _progressionController.SkipProgressionAsync();
        }
        IsInProgressionMode = false;
    }

    /// <summary>
    /// Loads a character and their specialization tree.
    /// </summary>
    public void LoadCharacter(PlayerCharacter character)
    {
        Character = character;
        LoadAbilityTree();
        this.RaisePropertyChanged(nameof(AvailablePP));
        this.RaisePropertyChanged(nameof(SpecializationName));
        this.RaisePropertyChanged(nameof(SpecializationDescription));
        this.RaisePropertyChanged(nameof(SpecializationTagline));
        this.RaisePropertyChanged(nameof(SpecializationIcon));
    }

    /// <summary>
    /// Loads demo character data for testing.
    /// </summary>
    private void LoadDemoCharacter()
    {
        // Create a demo Warrior with Iron-Bane specialization
        var demoCharacter = new PlayerCharacter
        {
            Name = "Sigurd Iron-Bane",
            Class = CharacterClass.Warrior,
            Specialization = Specialization.IronBane,
            CurrentLegend = 3,
            ProgressionPoints = 15,
            HP = 45,
            MaxHP = 60,
            Stamina = 80,
            MaxStamina = 100
        };

        // Create demo specialization data
        _specializationData = new SpecializationData
        {
            SpecializationID = 2,
            Name = "Iron-Bane",
            ArchetypeID = 1, // Warrior
            PathType = "Coherent",
            MechanicalRole = "Tank/Anti-Machine",
            PrimaryAttribute = "MIGHT",
            SecondaryAttribute = "WILL",
            Description = "Zealous warriors who have sworn to destroy the corrupted machines that plague Aethelgard. Their righteous fury burns hot against the mechanical abominations.",
            Tagline = "The machine fears nothing. Except you.",
            ResourceSystem = "Righteous Fervor",
            TraumaRisk = "Medium",
            IconEmoji = "???",
            PPCostToUnlock = 3
        };

        // Pre-learn some abilities for the demo
        demoCharacter.Abilities.Add(new Ability
        {
            Name = "Crushing Blow",
            Description = "A devastating overhead strike.",
            CurrentRank = 2,
            MaxRank = 3
        });

        Character = demoCharacter;
        LoadAbilityTree();
    }

    /// <summary>
    /// Loads abilities into tier collections.
    /// </summary>
    private void LoadAbilityTree()
    {
        FoundationTier.Clear();
        CoreTier.Clear();
        AdvancedTier.Clear();
        MasteryTier.Clear();

        if (Character == null) return;

        // Create demo abilities organized by tier
        var abilities = CreateDemoAbilities();

        foreach (var ability in abilities)
        {
            // Check if character has learned this ability
            var learnedAbility = Character.Abilities
                .FirstOrDefault(a => a.Name.Equals(ability.Name, StringComparison.OrdinalIgnoreCase));

            bool isUnlocked = learnedAbility != null;
            int currentRank = learnedAbility?.CurrentRank ?? 0;

            var node = new AbilityNodeViewModel(ability, Character, isUnlocked, currentRank);

            switch (ability.TierLevel)
            {
                case 1:
                    FoundationTier.Add(node);
                    break;
                case 2:
                    CoreTier.Add(node);
                    break;
                case 3:
                    AdvancedTier.Add(node);
                    break;
                case 4:
                    MasteryTier.Add(node);
                    break;
            }
        }

        // Calculate PP spent in tree
        CalculatePPSpentInTree();
    }

    /// <summary>
    /// Creates demo abilities for the Iron-Bane specialization.
    /// </summary>
    private List<AbilityData> CreateDemoAbilities()
    {
        return new List<AbilityData>
        {
            // Foundation Tier (Tier 1)
            new AbilityData
            {
                AbilityID = 1,
                SpecializationID = 2,
                Name = "Crushing Blow",
                Description = "A devastating overhead strike that sends foes crashing to the ground.",
                MechanicalSummary = "2d6+MIGHT damage, knocks target prone",
                TierLevel = 1,
                PPCost = 3,
                AbilityType = "Active",
                ActionType = "Standard Action",
                TargetType = "Single Enemy",
                ResourceCost = new AbilityResourceCost { Stamina = 15 },
                AttributeUsed = "MIGHT",
                DamageDice = 2,
                MaxRank = 3,
                CostToRank2 = 5,
                CostToRank3 = 0
            },
            new AbilityData
            {
                AbilityID = 2,
                SpecializationID = 2,
                Name = "Machine Hunter",
                Description = "Your strikes are especially deadly against mechanical foes.",
                MechanicalSummary = "+2 damage vs mechanical enemies, sense machines within 30ft",
                TierLevel = 1,
                PPCost = 3,
                AbilityType = "Passive",
                ActionType = "Passive",
                TargetType = "Self",
                ResourceCost = new AbilityResourceCost(),
                MaxRank = 3,
                CostToRank2 = 5,
                CostToRank3 = 0
            },
            new AbilityData
            {
                AbilityID = 3,
                SpecializationID = 2,
                Name = "Rally Cry",
                Description = "Your commanding shout inspires your companions to fight on.",
                MechanicalSummary = "All allies within 20ft heal 2d6 HP, +1 to next attack",
                TierLevel = 1,
                PPCost = 3,
                AbilityType = "Active",
                ActionType = "Bonus Action",
                TargetType = "All Allies",
                ResourceCost = new AbilityResourceCost { Stamina = 20 },
                AttributeUsed = "WILL",
                MaxRank = 3,
                CostToRank2 = 5,
                CostToRank3 = 0
            },

            // Core Tier (Tier 2)
            new AbilityData
            {
                AbilityID = 4,
                SpecializationID = 2,
                Name = "Righteous Fervor",
                Description = "Channel your hatred of machines into burning fury.",
                MechanicalSummary = "Build Fervor on attacks, spend for bonus damage",
                TierLevel = 2,
                PPCost = 5,
                AbilityType = "Passive",
                ActionType = "Passive",
                TargetType = "Self",
                ResourceCost = new AbilityResourceCost(),
                Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 6 },
                MaxRank = 3,
                CostToRank2 = 5,
                CostToRank3 = 0
            },
            new AbilityData
            {
                AbilityID = 5,
                SpecializationID = 2,
                Name = "Armor Breaker",
                Description = "Strike at the weak points in your enemy's defenses.",
                MechanicalSummary = "Ignores armor, -3 Defense for 2 turns",
                TierLevel = 2,
                PPCost = 5,
                AbilityType = "Active",
                ActionType = "Standard Action",
                TargetType = "Single Enemy",
                ResourceCost = new AbilityResourceCost { Stamina = 20 },
                AttributeUsed = "MIGHT",
                DamageDice = 2,
                IgnoresArmor = true,
                Prerequisites = new AbilityPrerequisites { RequiredAbilityIDs = new List<int> { 1 } },
                MaxRank = 3,
                CostToRank2 = 5,
                CostToRank3 = 0
            },
            new AbilityData
            {
                AbilityID = 6,
                SpecializationID = 2,
                Name = "Second Wind",
                Description = "Dig deep. You're not done yet.",
                MechanicalSummary = "Heal 2d10+WILL HP, remove one [Wounded]",
                TierLevel = 2,
                PPCost = 5,
                AbilityType = "Active",
                ActionType = "Bonus Action",
                TargetType = "Self",
                ResourceCost = new AbilityResourceCost { Stamina = 20 },
                AttributeUsed = "WILL",
                HealingDice = 2,
                CooldownType = "Per Combat",
                Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 6 },
                MaxRank = 3,
                CostToRank2 = 5,
                CostToRank3 = 0
            },
            new AbilityData
            {
                AbilityID = 7,
                SpecializationID = 2,
                Name = "Intimidating Presence",
                Description = "Your mere presence radiates menace.",
                MechanicalSummary = "Enemies within 10ft must save or become [Frightened]",
                TierLevel = 2,
                PPCost = 5,
                AbilityType = "Active",
                ActionType = "Bonus Action",
                TargetType = "All Enemies (10ft)",
                ResourceCost = new AbilityResourceCost { Stamina = 15 },
                AttributeUsed = "WILL",
                StatusEffectsApplied = new List<string> { "Frightened" },
                Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 6 },
                MaxRank = 3,
                CostToRank2 = 5,
                CostToRank3 = 0
            },

            // Advanced Tier (Tier 3)
            new AbilityData
            {
                AbilityID = 8,
                SpecializationID = 2,
                Name = "Unstoppable",
                Description = "Nothing will stop you. Nothing.",
                MechanicalSummary = "3 turns: immune to CC, half damage, +2 attacks",
                TierLevel = 3,
                PPCost = 8,
                AbilityType = "Active",
                ActionType = "Bonus Action",
                TargetType = "Self",
                ResourceCost = new AbilityResourceCost { Stamina = 30 },
                CooldownType = "Per Combat",
                Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 15 },
                MaxRank = 1
            },
            new AbilityData
            {
                AbilityID = 9,
                SpecializationID = 2,
                Name = "Execute",
                Description = "A killing blow for weakened foes.",
                MechanicalSummary = "If target <30% HP: instant kill. Otherwise: 2d10+MIGHT",
                TierLevel = 3,
                PPCost = 8,
                AbilityType = "Active",
                ActionType = "Standard Action",
                TargetType = "Single Enemy",
                ResourceCost = new AbilityResourceCost { Stamina = 25 },
                AttributeUsed = "MIGHT",
                DamageDice = 2,
                Prerequisites = new AbilityPrerequisites { RequiredAbilityIDs = new List<int> { 5 }, RequiredPPInTree = 15 },
                MaxRank = 3,
                CostToRank2 = 5,
                CostToRank3 = 0
            },
            new AbilityData
            {
                AbilityID = 10,
                SpecializationID = 2,
                Name = "Bulwark",
                Description = "You are the wall. They will break against you.",
                MechanicalSummary = "Stance: All enemies must target you, +5 Defense",
                TierLevel = 3,
                PPCost = 8,
                AbilityType = "Active",
                ActionType = "Free Action",
                TargetType = "Self",
                ResourceCost = new AbilityResourceCost(),
                Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 15 },
                MaxRank = 1
            },

            // Mastery Tier (Tier 4)
            new AbilityData
            {
                AbilityID = 11,
                SpecializationID = 2,
                Name = "Titan's Strength",
                Description = "Channel the strength of the ancient colossi.",
                MechanicalSummary = "4 turns: 2x MIGHT, +10 damage, immovable",
                TierLevel = 4,
                PPCost = 12,
                AbilityType = "Active",
                ActionType = "Bonus Action",
                TargetType = "Self",
                ResourceCost = new AbilityResourceCost { Stamina = 40 },
                CooldownType = "Per Combat",
                Prerequisites = new AbilityPrerequisites { RequiredAbilityIDs = new List<int> { 8 }, RequiredPPInTree = 24 },
                MaxRank = 1
            },
            new AbilityData
            {
                AbilityID = 12,
                SpecializationID = 2,
                Name = "Machine's Bane",
                Description = "Your hatred for the machine is absolute. Final.",
                MechanicalSummary = "Capstone: Double damage vs machines, immune to machine effects",
                TierLevel = 4,
                PPCost = 12,
                AbilityType = "Passive",
                ActionType = "Passive",
                TargetType = "Self",
                ResourceCost = new AbilityResourceCost(),
                Prerequisites = new AbilityPrerequisites { RequiredAbilityIDs = new List<int> { 2, 4 }, RequiredPPInTree = 24 },
                MaxRank = 1
            }
        };
    }

    /// <summary>
    /// Calculates PP spent in the current tree.
    /// </summary>
    private void CalculatePPSpentInTree()
    {
        int ppSpent = 0;

        void CountTier(IEnumerable<AbilityNodeViewModel> tier)
        {
            foreach (var node in tier)
            {
                if (node.IsUnlocked)
                {
                    ppSpent += node.UnlockCost;
                    // Add rank up costs
                    if (node.CurrentRank >= 2)
                        ppSpent += node.RankUpCostToRank2;
                    if (node.CurrentRank >= 3)
                        ppSpent += node.RankUpCostToRank3;
                }
            }
        }

        CountTier(FoundationTier);
        CountTier(CoreTier);
        CountTier(AdvancedTier);
        CountTier(MasteryTier);

        PPSpentInTree = ppSpent;
    }

    /// <summary>
    /// Unlocks an ability for the character.
    /// </summary>
    private void UnlockAbility(AbilityNodeViewModel node)
    {
        if (Character == null || !node.CanUnlock)
        {
            StatusMessage = "Cannot unlock this ability.";
            return;
        }

        // Deduct PP
        Character.ProgressionPoints -= node.UnlockCost;

        // Add ability to character
        Character.Abilities.Add(new Ability
        {
            Name = node.Name,
            Description = node.Description,
            CurrentRank = 1,
            MaxRank = node.MaxRank,
            StaminaCost = node.Ability.ResourceCost.Stamina,
            Type = node.Ability.AbilityType == "Attack" ? AbilityType.Attack :
                   node.Ability.AbilityType == "Defense" ? AbilityType.Defense :
                   node.Ability.AbilityType == "Utility" ? AbilityType.Utility : AbilityType.Control
        });

        // Update node state
        node.IsUnlocked = true;
        node.CurrentRank = 1;

        // Refresh all nodes (prerequisites may have changed)
        RefreshAllNodes();

        StatusMessage = $"Unlocked {node.Name}! ({node.UnlockCost} PP spent)";
        this.RaisePropertyChanged(nameof(AvailablePP));
        CalculatePPSpentInTree();
    }

    /// <summary>
    /// Ranks up an ability.
    /// </summary>
    private void RankUpAbility(AbilityNodeViewModel node)
    {
        if (Character == null || !node.CanRankUp)
        {
            StatusMessage = "Cannot rank up this ability.";
            return;
        }

        int cost = node.RankUpCost;

        // Deduct PP
        Character.ProgressionPoints -= cost;

        // Update character's ability rank
        var charAbility = Character.Abilities
            .FirstOrDefault(a => a.Name.Equals(node.Name, StringComparison.OrdinalIgnoreCase));

        if (charAbility != null)
        {
            charAbility.CurrentRank++;
        }

        // Update node state
        node.CurrentRank++;

        // Refresh
        RefreshAllNodes();

        StatusMessage = $"Ranked up {node.Name} to Rank {node.CurrentRank}! ({cost} PP spent)";
        this.RaisePropertyChanged(nameof(AvailablePP));
        CalculatePPSpentInTree();
    }

    /// <summary>
    /// Refreshes all node states.
    /// </summary>
    private void RefreshAllNodes()
    {
        foreach (var node in FoundationTier) node.RefreshState();
        foreach (var node in CoreTier) node.RefreshState();
        foreach (var node in AdvancedTier) node.RefreshState();
        foreach (var node in MasteryTier) node.RefreshState();
    }
}
