using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Conditions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the DialogueConditionEvaluator (v0.4.2b - The Lexicon).
/// Tests all 8 condition types, option evaluation, and node evaluation.
/// </summary>
public class DialogueConditionEvaluatorTests
{
    private readonly Mock<IFactionService> _mockFactionService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<ISpecializationRepository> _mockSpecRepository;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly GameState _gameState;
    private readonly Mock<ILogger<DialogueConditionEvaluator>> _mockLogger;
    private readonly DialogueConditionEvaluator _sut;

    public DialogueConditionEvaluatorTests()
    {
        _mockFactionService = new Mock<IFactionService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _mockSpecRepository = new Mock<ISpecializationRepository>();
        _mockDiceService = new Mock<IDiceService>();
        _gameState = new GameState();
        _mockLogger = new Mock<ILogger<DialogueConditionEvaluator>>();

        _sut = new DialogueConditionEvaluator(
            _mockFactionService.Object,
            _mockInventoryService.Object,
            _mockSpecRepository.Object,
            _mockDiceService.Object,
            _gameState,
            _mockLogger.Object);

        // Default setup: No unlocked nodes
        _mockSpecRepository
            .Setup(r => r.GetUnlockedNodesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<SpecializationNode>());
    }

    private Character CreateTestCharacter(
        string name = "TestChar",
        int wits = 5,
        int might = 5,
        int will = 5,
        int finesse = 5,
        int sturdiness = 5,
        int level = 1)
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = name,
            Wits = wits,
            Might = might,
            Will = will,
            Finesse = finesse,
            Sturdiness = sturdiness,
            Level = level
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // AttributeCondition Tests (8 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateConditionAsync_AttributeGreaterThanOrEqual_WhenMet_ReturnsSuccess()
    {
        var character = CreateTestCharacter(wits: 6);
        var condition = new AttributeCondition
        {
            Attribute = CharacterAttribute.Wits,
            Comparison = ComparisonType.GreaterThanOrEqual,
            Threshold = 6
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.ConditionType.Should().Be(DialogueConditionType.Attribute);
    }

    [Fact]
    public async Task EvaluateConditionAsync_AttributeGreaterThanOrEqual_WhenNotMet_ReturnsFail()
    {
        var character = CreateTestCharacter(wits: 5);
        var condition = new AttributeCondition
        {
            Attribute = CharacterAttribute.Wits,
            Comparison = ComparisonType.GreaterThanOrEqual,
            Threshold = 6
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.FailureReason.Should().Contain("Wits");
    }

    [Fact]
    public async Task EvaluateConditionAsync_AttributeEqual_WhenExact_ReturnsSuccess()
    {
        var character = CreateTestCharacter(might: 7);
        var condition = new AttributeCondition
        {
            Attribute = CharacterAttribute.Might,
            Comparison = ComparisonType.Equal,
            Threshold = 7
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateConditionAsync_AttributeEqual_WhenDifferent_ReturnsFail()
    {
        var character = CreateTestCharacter(might: 8);
        var condition = new AttributeCondition
        {
            Attribute = CharacterAttribute.Might,
            Comparison = ComparisonType.Equal,
            Threshold = 7
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateConditionAsync_AttributeGreaterThan_WhenHigher_ReturnsSuccess()
    {
        var character = CreateTestCharacter(will: 6);
        var condition = new AttributeCondition
        {
            Attribute = CharacterAttribute.Will,
            Comparison = ComparisonType.GreaterThan,
            Threshold = 5
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateConditionAsync_AttributeLessThan_WhenLower_ReturnsSuccess()
    {
        var character = CreateTestCharacter(finesse: 4);
        var condition = new AttributeCondition
        {
            Attribute = CharacterAttribute.Finesse,
            Comparison = ComparisonType.LessThan,
            Threshold = 5
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateConditionAsync_AttributeLessThanOrEqual_WhenEqual_ReturnsSuccess()
    {
        var character = CreateTestCharacter(sturdiness: 5);
        var condition = new AttributeCondition
        {
            Attribute = CharacterAttribute.Sturdiness,
            Comparison = ComparisonType.LessThanOrEqual,
            Threshold = 5
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateConditionAsync_AttributeNotEqual_WhenDifferent_ReturnsSuccess()
    {
        var character = CreateTestCharacter(wits: 4);
        var condition = new AttributeCondition
        {
            Attribute = CharacterAttribute.Wits,
            Comparison = ComparisonType.NotEqual,
            Threshold = 5
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // LevelCondition Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateConditionAsync_Level_WhenMet_ReturnsSuccess()
    {
        var character = CreateTestCharacter(level: 5);
        var condition = new LevelCondition { MinLevel = 5 };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.ConditionType.Should().Be(DialogueConditionType.Level);
    }

    [Fact]
    public async Task EvaluateConditionAsync_Level_WhenExceeded_ReturnsSuccess()
    {
        var character = CreateTestCharacter(level: 10);
        var condition = new LevelCondition { MinLevel = 5 };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateConditionAsync_Level_WhenInsufficient_ReturnsFail()
    {
        var character = CreateTestCharacter(level: 3);
        var condition = new LevelCondition { MinLevel = 5 };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.FailureReason.Should().Contain("level 5");
    }

    [Fact]
    public async Task EvaluateConditionAsync_Level_DisplayHint_ShowsRequirement()
    {
        var character = CreateTestCharacter(level: 1);
        var condition = new LevelCondition { MinLevel = 10 };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.DisplayHint.Should().Be("[Level 10]");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ReputationCondition Tests (6 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateConditionAsync_Reputation_WhenMet_ReturnsSuccess()
    {
        var character = CreateTestCharacter();
        var condition = new ReputationCondition
        {
            Faction = FactionType.Dvergr,
            MinDisposition = Disposition.Friendly
        };

        _mockFactionService
            .Setup(s => s.MeetsDispositionRequirementAsync(character, FactionType.Dvergr, Disposition.Friendly))
            .ReturnsAsync(true);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.ConditionType.Should().Be(DialogueConditionType.Reputation);
    }

    [Fact]
    public async Task EvaluateConditionAsync_Reputation_WhenNotMet_ReturnsFail()
    {
        var character = CreateTestCharacter();
        var condition = new ReputationCondition
        {
            Faction = FactionType.TheBound,
            MinDisposition = Disposition.Friendly
        };

        _mockFactionService
            .Setup(s => s.MeetsDispositionRequirementAsync(character, FactionType.TheBound, Disposition.Friendly))
            .ReturnsAsync(false);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.FailureReason.Should().Contain("Friendly");
    }

    [Fact]
    public async Task EvaluateConditionAsync_Reputation_RequiresNeutral_WhenHostile_ReturnsFail()
    {
        var character = CreateTestCharacter();
        var condition = new ReputationCondition
        {
            Faction = FactionType.IronBanes,
            MinDisposition = Disposition.Neutral
        };

        _mockFactionService
            .Setup(s => s.MeetsDispositionRequirementAsync(character, FactionType.IronBanes, Disposition.Neutral))
            .ReturnsAsync(false);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateConditionAsync_Reputation_RequiresExalted_WhenFriendly_ReturnsFail()
    {
        var character = CreateTestCharacter();
        var condition = new ReputationCondition
        {
            Faction = FactionType.Dvergr,
            MinDisposition = Disposition.Exalted
        };

        _mockFactionService
            .Setup(s => s.MeetsDispositionRequirementAsync(character, FactionType.Dvergr, Disposition.Exalted))
            .ReturnsAsync(false);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateConditionAsync_Reputation_DisplayHint_ShowsFactionAndDisposition()
    {
        var character = CreateTestCharacter();
        var condition = new ReputationCondition
        {
            Faction = FactionType.Dvergr,
            MinDisposition = Disposition.Friendly
        };

        _mockFactionService
            .Setup(s => s.MeetsDispositionRequirementAsync(It.IsAny<Character>(), It.IsAny<FactionType>(), It.IsAny<Disposition>()))
            .ReturnsAsync(false);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.DisplayHint.Should().Be("[Dvergr: Friendly]");
    }

    [Fact]
    public async Task EvaluateConditionAsync_Reputation_WhenExalted_ReturnsSuccess()
    {
        var character = CreateTestCharacter();
        var condition = new ReputationCondition
        {
            Faction = FactionType.TheFaceless,
            MinDisposition = Disposition.Exalted
        };

        _mockFactionService
            .Setup(s => s.MeetsDispositionRequirementAsync(character, FactionType.TheFaceless, Disposition.Exalted))
            .ReturnsAsync(true);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // FlagCondition Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateConditionAsync_Flag_WhenTrue_ReturnsSuccess()
    {
        _gameState.SetFlag("CompletedTutorial", true);
        var character = CreateTestCharacter();
        var condition = new FlagCondition
        {
            FlagKey = "CompletedTutorial",
            RequiredValue = true
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.ConditionType.Should().Be(DialogueConditionType.Flag);
    }

    [Fact]
    public async Task EvaluateConditionAsync_Flag_WhenFalse_ReturnsFail()
    {
        // Flag not set defaults to false
        var character = CreateTestCharacter();
        var condition = new FlagCondition
        {
            FlagKey = "MetTheElders",
            RequiredValue = true
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.FailureReason.Should().Contain("MetTheElders");
    }

    [Fact]
    public async Task EvaluateConditionAsync_Flag_InverseCheck_WhenFlagNotSet_ReturnsSuccess()
    {
        var character = CreateTestCharacter();
        var condition = new FlagCondition
        {
            FlagKey = "BetrayedAllies",
            RequiredValue = false
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateConditionAsync_Flag_InverseCheck_WhenFlagSet_ReturnsFail()
    {
        _gameState.SetFlag("BetrayedAllies", true);
        var character = CreateTestCharacter();
        var condition = new FlagCondition
        {
            FlagKey = "BetrayedAllies",
            RequiredValue = false
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.FailureReason.Should().Contain("Blocked by");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ItemCondition Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateConditionAsync_Item_WhenOwned_ReturnsSuccess()
    {
        var character = CreateTestCharacter();
        var condition = new ItemCondition
        {
            ItemId = "Iron Key",
            MinQuantity = 1
        };

        _mockInventoryService
            .Setup(s => s.HasItemAsync(character.Id, "Iron Key", 1))
            .ReturnsAsync(true);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.ConditionType.Should().Be(DialogueConditionType.Item);
    }

    [Fact]
    public async Task EvaluateConditionAsync_Item_WhenMissing_ReturnsFail()
    {
        var character = CreateTestCharacter();
        var condition = new ItemCondition
        {
            ItemId = "Ancient Tome",
            MinQuantity = 1
        };

        _mockInventoryService
            .Setup(s => s.HasItemAsync(character.Id, "Ancient Tome", 1))
            .ReturnsAsync(false);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.FailureReason.Should().Contain("Ancient Tome");
    }

    [Fact]
    public async Task EvaluateConditionAsync_Item_WithQuantity_WhenEnough_ReturnsSuccess()
    {
        var character = CreateTestCharacter();
        var condition = new ItemCondition
        {
            ItemId = "Gold Coin",
            MinQuantity = 50
        };

        _mockInventoryService
            .Setup(s => s.HasItemAsync(character.Id, "Gold Coin", 50))
            .ReturnsAsync(true);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateConditionAsync_Item_WithQuantity_WhenNotEnough_ReturnsFail()
    {
        var character = CreateTestCharacter();
        var condition = new ItemCondition
        {
            ItemId = "Gold Coin",
            MinQuantity = 100
        };

        _mockInventoryService
            .Setup(s => s.HasItemAsync(character.Id, "Gold Coin", 100))
            .ReturnsAsync(false);

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.DisplayHint.Should().Contain("x100");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SpecializationCondition Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateConditionAsync_Specialization_WhenUnlocked_ReturnsSuccess()
    {
        var character = CreateTestCharacter();
        var specId = Guid.NewGuid();
        var condition = new SpecializationCondition
        {
            SpecializationId = specId,
            SpecializationName = "Berserkr"
        };

        _mockSpecRepository
            .Setup(r => r.GetUnlockedNodesAsync(character.Id))
            .ReturnsAsync(new List<SpecializationNode>
            {
                new() { Id = Guid.NewGuid(), SpecializationId = specId }
            });

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.ConditionType.Should().Be(DialogueConditionType.Specialization);
    }

    [Fact]
    public async Task EvaluateConditionAsync_Specialization_WhenLocked_ReturnsFail()
    {
        var character = CreateTestCharacter();
        var condition = new SpecializationCondition
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Stormcaller"
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.FailureReason.Should().Contain("Stormcaller");
    }

    [Fact]
    public async Task EvaluateConditionAsync_Specialization_DisplayHint_ShowsName()
    {
        var character = CreateTestCharacter();
        var condition = new SpecializationCondition
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Runasmidr"
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.DisplayHint.Should().Be("[Is: Runasmidr]");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // NodeCondition Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateConditionAsync_Node_WhenUnlocked_ReturnsSuccess()
    {
        var character = CreateTestCharacter();
        var nodeId = Guid.NewGuid();
        var condition = new NodeCondition
        {
            NodeId = nodeId,
            NodeName = "Battle Rage"
        };

        _mockSpecRepository
            .Setup(r => r.GetUnlockedNodesAsync(character.Id))
            .ReturnsAsync(new List<SpecializationNode>
            {
                new() { Id = nodeId, SpecializationId = Guid.NewGuid() }
            });

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.ConditionType.Should().Be(DialogueConditionType.Node);
    }

    [Fact]
    public async Task EvaluateConditionAsync_Node_WhenLocked_ReturnsFail()
    {
        var character = CreateTestCharacter();
        var condition = new NodeCondition
        {
            NodeId = Guid.NewGuid(),
            NodeName = "Whirlwind Strike"
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.FailureReason.Should().Contain("Whirlwind Strike");
    }

    [Fact]
    public async Task EvaluateConditionAsync_Node_DisplayHint_ShowsAbilityName()
    {
        var character = CreateTestCharacter();
        var condition = new NodeCondition
        {
            NodeId = Guid.NewGuid(),
            NodeName = "Final Stand"
        };

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.DisplayHint.Should().Be("[Has: Final Stand]");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SkillCheckCondition Tests (6 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateConditionAsync_SkillCheck_WhenPassed_ReturnsSuccess()
    {
        var character = CreateTestCharacter(wits: 5);
        var condition = new SkillCheckCondition
        {
            Attribute = CharacterAttribute.Wits,
            DifficultyClass = 2,
            CheckDescription = "Notice the hidden passage"
        };

        _mockDiceService
            .Setup(d => d.Roll(5, It.IsAny<string>()))
            .Returns(new DiceResult(3, 0, new List<int> { 8, 9, 10, 4, 5 }));

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.ConditionType.Should().Be(DialogueConditionType.SkillCheck);
        result.NetSuccesses.Should().Be(3);
    }

    [Fact]
    public async Task EvaluateConditionAsync_SkillCheck_WhenFailed_ReturnsFail()
    {
        var character = CreateTestCharacter(wits: 3);
        var condition = new SkillCheckCondition
        {
            Attribute = CharacterAttribute.Wits,
            DifficultyClass = 3,
            CheckDescription = "Decipher the inscription"
        };

        _mockDiceService
            .Setup(d => d.Roll(3, It.IsAny<string>()))
            .Returns(new DiceResult(1, 0, new List<int> { 8, 3, 4 }));

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.NetSuccesses.Should().Be(1);
        result.FailureReason.Should().Contain("DC 3");
    }

    [Fact]
    public async Task EvaluateConditionAsync_SkillCheck_WithBotches_CalculatesNetSuccesses()
    {
        var character = CreateTestCharacter(might: 4);
        var condition = new SkillCheckCondition
        {
            Attribute = CharacterAttribute.Might,
            DifficultyClass = 2
        };

        _mockDiceService
            .Setup(d => d.Roll(4, It.IsAny<string>()))
            .Returns(new DiceResult(3, 1, new List<int> { 8, 9, 10, 1 }));

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeTrue();
        result.NetSuccesses.Should().Be(2); // 3 successes - 1 botch
    }

    [Fact]
    public async Task EvaluateConditionAsync_SkillCheck_Botched_ReturnsNegativeSuccesses()
    {
        var character = CreateTestCharacter(finesse: 3);
        var condition = new SkillCheckCondition
        {
            Attribute = CharacterAttribute.Finesse,
            DifficultyClass = 1
        };

        _mockDiceService
            .Setup(d => d.Roll(3, It.IsAny<string>()))
            .Returns(new DiceResult(0, 2, new List<int> { 1, 1, 5 }));

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.Passed.Should().BeFalse();
        result.NetSuccesses.Should().Be(-2);
        result.FailureReason.Should().Contain("Botched");
    }

    [Fact]
    public async Task EvaluateConditionAsync_SkillCheck_IncludesRolls()
    {
        var character = CreateTestCharacter(will: 4);
        var condition = new SkillCheckCondition
        {
            Attribute = CharacterAttribute.Will,
            DifficultyClass = 1
        };

        var rolls = new List<int> { 8, 5, 3, 10 };
        _mockDiceService
            .Setup(d => d.Roll(4, It.IsAny<string>()))
            .Returns(new DiceResult(2, 0, rolls));

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.DiceRolls.Should().BeEquivalentTo(rolls);
    }

    [Fact]
    public async Task EvaluateConditionAsync_SkillCheck_DisplayHint_ShowsDC()
    {
        var character = CreateTestCharacter(wits: 5);
        var condition = new SkillCheckCondition
        {
            Attribute = CharacterAttribute.Wits,
            DifficultyClass = 4
        };

        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(2, 0, new List<int> { 8, 9, 3, 4, 5 }));

        var result = await _sut.EvaluateConditionAsync(character, condition);

        result.DisplayHint.Should().Be("[WITS DC 4]");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // EvaluateOptionAsync Tests (6 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateOptionAsync_NoConditions_ReturnsAvailable()
    {
        var character = CreateTestCharacter();
        var option = new DialogueOption
        {
            Id = Guid.NewGuid(),
            Text = "Hello there",
            Conditions = new List<DialogueCondition>()
        };

        var result = await _sut.EvaluateOptionAsync(character, option);

        result.IsVisible.Should().BeTrue();
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateOptionAsync_AllConditionsPassed_ReturnsAvailable()
    {
        var character = CreateTestCharacter(wits: 6, level: 5);
        var option = new DialogueOption
        {
            Id = Guid.NewGuid(),
            Text = "I understand",
            Conditions = new List<DialogueCondition>
            {
                new AttributeCondition { Attribute = CharacterAttribute.Wits, Threshold = 5 },
                new LevelCondition { MinLevel = 3 }
            }
        };

        var result = await _sut.EvaluateOptionAsync(character, option);

        result.IsVisible.Should().BeTrue();
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateOptionAsync_OneConditionFails_ShowLocked_ReturnsLockedVisible()
    {
        var character = CreateTestCharacter(wits: 4);
        var option = new DialogueOption
        {
            Id = Guid.NewGuid(),
            Text = "I see through your lies",
            VisibilityMode = OptionVisibility.ShowLocked,
            Conditions = new List<DialogueCondition>
            {
                new AttributeCondition { Attribute = CharacterAttribute.Wits, Threshold = 6 }
            }
        };

        var result = await _sut.EvaluateOptionAsync(character, option);

        result.IsVisible.Should().BeTrue();
        result.IsAvailable.Should().BeFalse();
        result.LockHint.Should().Contain("WITS");
    }

    [Fact]
    public async Task EvaluateOptionAsync_ConditionFails_HideWhenFailed_ReturnsHidden()
    {
        var character = CreateTestCharacter(wits: 4);
        var option = new DialogueOption
        {
            Id = Guid.NewGuid(),
            Text = "Secret option",
            VisibilityMode = OptionVisibility.ShowLocked,
            Conditions = new List<DialogueCondition>
            {
                new AttributeCondition
                {
                    Attribute = CharacterAttribute.Wits,
                    Threshold = 6,
                    HideWhenFailed = true
                }
            }
        };

        var result = await _sut.EvaluateOptionAsync(character, option);

        result.IsVisible.Should().BeFalse();
        result.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateOptionAsync_VisibilityModeHidden_WhenFails_ReturnsHidden()
    {
        var character = CreateTestCharacter(wits: 4);
        var option = new DialogueOption
        {
            Id = Guid.NewGuid(),
            Text = "Hidden option",
            VisibilityMode = OptionVisibility.Hidden,
            Conditions = new List<DialogueCondition>
            {
                new AttributeCondition { Attribute = CharacterAttribute.Wits, Threshold = 6 }
            }
        };

        var result = await _sut.EvaluateOptionAsync(character, option);

        result.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateOptionAsync_IncludesConditionResults()
    {
        var character = CreateTestCharacter(wits: 4);
        var option = new DialogueOption
        {
            Id = Guid.NewGuid(),
            Text = "Locked option",
            VisibilityMode = OptionVisibility.ShowLocked,
            Conditions = new List<DialogueCondition>
            {
                new AttributeCondition { Attribute = CharacterAttribute.Wits, Threshold = 6 }
            }
        };

        var result = await _sut.EvaluateOptionAsync(character, option);

        result.ConditionResults.Should().HaveCount(1);
        result.ConditionResults![0].Passed.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // EvaluateNodeOptionsAsync Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EvaluateNodeOptionsAsync_ReturnsAllOptionResults()
    {
        var character = CreateTestCharacter();
        var node = new DialogueNode
        {
            Id = Guid.NewGuid(),
            NodeId = "test_node",
            Text = "Hello",
            Options = new List<DialogueOption>
            {
                new() { Id = Guid.NewGuid(), Text = "Option 1", DisplayOrder = 0 },
                new() { Id = Guid.NewGuid(), Text = "Option 2", DisplayOrder = 1 },
                new() { Id = Guid.NewGuid(), Text = "Option 3", DisplayOrder = 2 }
            }
        };

        var results = await _sut.EvaluateNodeOptionsAsync(character, node);

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task EvaluateNodeOptionsAsync_RespectsDisplayOrder()
    {
        var character = CreateTestCharacter();
        var option1 = new DialogueOption { Id = Guid.NewGuid(), Text = "First", DisplayOrder = 0 };
        var option2 = new DialogueOption { Id = Guid.NewGuid(), Text = "Second", DisplayOrder = 1 };
        var option3 = new DialogueOption { Id = Guid.NewGuid(), Text = "Third", DisplayOrder = 2 };

        var node = new DialogueNode
        {
            Id = Guid.NewGuid(),
            NodeId = "ordered_node",
            Text = "Choose wisely",
            Options = new List<DialogueOption> { option3, option1, option2 } // Out of order
        };

        var results = await _sut.EvaluateNodeOptionsAsync(character, node);

        results[0].OptionId.Should().Be(option1.Id);
        results[1].OptionId.Should().Be(option2.Id);
        results[2].OptionId.Should().Be(option3.Id);
    }

    [Fact]
    public async Task EvaluateNodeOptionsAsync_MixedVisibility_ReturnsCorrectStates()
    {
        var character = CreateTestCharacter(wits: 4);
        var node = new DialogueNode
        {
            Id = Guid.NewGuid(),
            NodeId = "mixed_node",
            Text = "Choose",
            Options = new List<DialogueOption>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Text = "Available",
                    DisplayOrder = 0
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Text = "Locked",
                    DisplayOrder = 1,
                    VisibilityMode = OptionVisibility.ShowLocked,
                    Conditions = new List<DialogueCondition>
                    {
                        new AttributeCondition { Attribute = CharacterAttribute.Wits, Threshold = 6 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Text = "Hidden",
                    DisplayOrder = 2,
                    VisibilityMode = OptionVisibility.Hidden,
                    Conditions = new List<DialogueCondition>
                    {
                        new AttributeCondition { Attribute = CharacterAttribute.Wits, Threshold = 8 }
                    }
                }
            }
        };

        var results = await _sut.EvaluateNodeOptionsAsync(character, node);

        results[0].IsAvailable.Should().BeTrue();
        results[0].IsVisible.Should().BeTrue();

        results[1].IsAvailable.Should().BeFalse();
        results[1].IsVisible.Should().BeTrue();

        results[2].IsAvailable.Should().BeFalse();
        results[2].IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateNodeOptionsAsync_EmptyOptions_ReturnsEmptyList()
    {
        var character = CreateTestCharacter();
        var node = new DialogueNode
        {
            Id = Guid.NewGuid(),
            NodeId = "terminal_node",
            Text = "Farewell",
            IsTerminal = true,
            Options = new List<DialogueOption>()
        };

        var results = await _sut.EvaluateNodeOptionsAsync(character, node);

        results.Should().BeEmpty();
    }
}
