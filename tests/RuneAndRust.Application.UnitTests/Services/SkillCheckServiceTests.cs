using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class SkillCheckServiceTests
{
    private SkillCheckService _service = null!;
    private Mock<ILogger<SkillCheckService>> _mockLogger = null!;
    private Mock<ILogger<DiceService>> _mockDiceLogger = null!;
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private DiceService _diceService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<SkillCheckService>>();
        _mockDiceLogger = new Mock<ILogger<DiceService>>();
        _mockConfig = new Mock<IGameConfigurationProvider>();

        var seededRandom = new Random(42);
        _diceService = new DiceService(_mockDiceLogger.Object, seededRandom);

        SetupDefaultMocks();

        _service = new SkillCheckService(
            _diceService,
            _mockConfig.Object,
            _mockLogger.Object);
    }

    private void SetupDefaultMocks()
    {
        var perception = SkillDefinition.Create(
            "perception", "Perception", "Notice things.",
            "wits", null, "1d10");

        var athletics = SkillDefinition.Create(
            "athletics", "Athletics", "Physical feats.",
            "might", "fortitude", "1d10");

        var lockpicking = SkillDefinition.Create(
            "lockpicking", "Lockpicking", "Open locks.",
            "finesse", null, "1d10", false, 5);

        _mockConfig.Setup(c => c.GetSkillById("perception")).Returns(perception);
        _mockConfig.Setup(c => c.GetSkillById("athletics")).Returns(athletics);
        _mockConfig.Setup(c => c.GetSkillById("lockpicking")).Returns(lockpicking);
        _mockConfig.Setup(c => c.GetSkills())
            .Returns(new List<SkillDefinition> { perception, athletics, lockpicking });

        var moderate = DifficultyClassDefinition.Create(
            "moderate", "Moderate", "Requires effort.", 12);
        var hard = DifficultyClassDefinition.Create(
            "hard", "Hard", "Difficult.", 18);

        _mockConfig.Setup(c => c.GetDifficultyClassById("moderate")).Returns(moderate);
        _mockConfig.Setup(c => c.GetDifficultyClassById("hard")).Returns(hard);
        _mockConfig.Setup(c => c.GetDifficultyClasses())
            .Returns(new List<DifficultyClassDefinition> { moderate, hard });
    }

    private static Player CreateTestPlayer(int wits = 8, int might = 8, int finesse = 8, int fortitude = 8)
    {
        var attributes = new PlayerAttributes(might, fortitude, 8, wits, finesse);
        return new Player("TestPlayer", "human", "soldier", attributes, "Test");
    }

    [Test]
    public void PerformCheck_WithValidParameters_ReturnsResult()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 12);

        // Act
        var result = _service.PerformCheck(player, "perception", "moderate");

        // Assert
        result.SkillId.Should().Be("perception");
        result.SkillName.Should().Be("Perception");
        result.DifficultyClass.Should().Be(12);
        result.AttributeBonus.Should().Be(12);
    }

    [Test]
    public void PerformCheck_WithSecondaryAttribute_CalculatesBonusCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer(might: 10, fortitude: 6);

        // Act
        var result = _service.PerformCheck(player, "athletics", "moderate");

        // Assert
        result.AttributeBonus.Should().Be(13); // 10 (might) + 3 (fortitude/2)
    }

    [Test]
    public void PerformCheck_WithAdvantage_RollsTwice()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.PerformCheck(
            player, "perception", "moderate",
            AdvantageType.Advantage);

        // Assert
        result.DiceResult.AdvantageType.Should().Be(AdvantageType.Advantage);
        result.DiceResult.AllRollTotals.Should().HaveCount(2);
    }

    [Test]
    public void PerformCheck_WithUnknownSkill_ThrowsArgumentException()
    {
        // Arrange
        var player = CreateTestPlayer();
        _mockConfig.Setup(c => c.GetSkillById("unknown")).Returns((SkillDefinition?)null);

        // Act
        var act = () => _service.PerformCheck(player, "unknown", "moderate");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unknown skill: unknown*");
    }

    [Test]
    public void PerformCheckWithDC_UsesExactDCValue()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.PerformCheckWithDC(
            player, "perception", 15, "Custom DC");

        // Assert
        result.DifficultyClass.Should().Be(15);
        result.DifficultyName.Should().Be("Custom DC");
    }

    [Test]
    public void GetAllSkills_ReturnsConfiguredSkills()
    {
        // Act
        var skills = _service.GetAllSkills();

        // Assert
        skills.Should().HaveCount(3);
        skills.Should().Contain(s => s.Id == "perception");
    }

    [Test]
    public void GetDifficultyClasses_ReturnsOrderedList()
    {
        // Act
        var dcs = _service.GetDifficultyClasses();

        // Assert
        dcs.Should().HaveCount(2);
        dcs.First().Id.Should().Be("moderate");
    }
}
