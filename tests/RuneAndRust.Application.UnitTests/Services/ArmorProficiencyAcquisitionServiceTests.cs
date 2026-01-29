// ═══════════════════════════════════════════════════════════════════════════════
// ArmorProficiencyAcquisitionServiceTests.cs
// Unit tests for the ArmorProficiencyAcquisitionService.
// Version: 0.16.2e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="ArmorProficiencyAcquisitionService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover:
/// </para>
/// <list type="bullet">
///   <item><description>Successful training scenarios</description></item>
///   <item><description>Insufficient resource failures</description></item>
///   <item><description>Level requirement failures</description></item>
///   <item><description>Eligibility checking</description></item>
///   <item><description>Training requirements retrieval</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArmorProficiencyAcquisitionServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    private Mock<ILogger<ArmorProficiencyAcquisitionService>> _mockLogger = null!;
    private Mock<IArchetypeArmorProficiencyProvider> _mockArchetypeProvider = null!;
    private ArmorProficiencyAcquisitionService _service = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // Setup
    // ═══════════════════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockLogger = new Mock<ILogger<ArmorProficiencyAcquisitionService>>();
        _mockArchetypeProvider = new Mock<IArchetypeArmorProficiencyProvider>();

        // Default archetype provider behavior: return NonProficient
        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency(It.IsAny<string>(), It.IsAny<ArmorCategory>()))
            .Returns(ArmorProficiencyLevel.NonProficient);

        // Create service
        _service = new ArmorProficiencyAcquisitionService(
            _mockLogger.Object,
            _mockArchetypeProvider.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Training Success Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task TrainArmorProficiencyAsync_WithSufficientResources_Succeeds()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, silverAmount: 100);

        // Act
        var result = await _service.TrainArmorProficiencyAsync(
            player,
            ArmorCategory.Light,
            ArmorProficiencyLevel.Proficient);

        // Assert
        result.Success.Should().BeTrue();
        result.NewLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        result.PreviousLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
        result.CurrencySpent.Should().Be(50);
        result.TimeSpentWeeks.Should().Be(2);

        // Verify currency was deducted
        player.GetCurrency("silver").Should().Be(50); // 100 - 50
    }

    [Test]
    public async Task TrainArmorProficiencyAsync_ToExpert_WithSufficientLevelAndCurrency_Succeeds()
    {
        // Arrange
        var player = CreateTestPlayer(level: 5, silverAmount: 500);

        // Set up archetype provider to return Proficient
        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency(It.IsAny<string>(), ArmorCategory.Heavy))
            .Returns(ArmorProficiencyLevel.Proficient);

        // Act
        var result = await _service.TrainArmorProficiencyAsync(
            player,
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.Expert);

        // Assert
        result.Success.Should().BeTrue();
        result.NewLevel.Should().Be(ArmorProficiencyLevel.Expert);
        result.CurrencySpent.Should().Be(200);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Training Failure Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task TrainArmorProficiencyAsync_InsufficientCurrency_Fails()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, silverAmount: 10); // Not enough

        // Act
        var result = await _service.TrainArmorProficiencyAsync(
            player,
            ArmorCategory.Medium,
            ArmorProficiencyLevel.Proficient);

        // Assert
        result.Success.Should().BeFalse();
        result.PrimaryFailureReason.Should().Contain("Insufficient funds");
        result.CurrencySpent.Should().Be(0);

        // Currency should not be deducted
        player.GetCurrency("silver").Should().Be(10);
    }

    [Test]
    public async Task TrainArmorProficiencyAsync_LevelTooLow_Fails()
    {
        // Arrange
        var player = CreateTestPlayer(level: 3, silverAmount: 1000);

        // Set up archetype provider to return Proficient
        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency(It.IsAny<string>(), ArmorCategory.Heavy))
            .Returns(ArmorProficiencyLevel.Proficient);

        // Act - Try to train to Expert which requires level 5
        var result = await _service.TrainArmorProficiencyAsync(
            player,
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.Expert);

        // Assert
        result.Success.Should().BeFalse();
        result.PrimaryFailureReason.Should().Contain("Character level too low");
    }

    [Test]
    public async Task TrainArmorProficiencyAsync_AlreadyAtMaster_Fails()
    {
        // Arrange
        var player = CreateTestPlayer(level: 15, silverAmount: 10000);

        // Set up archetype provider to return Master
        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency(It.IsAny<string>(), ArmorCategory.Light))
            .Returns(ArmorProficiencyLevel.Master);

        // Act
        var result = await _service.TrainArmorProficiencyAsync(
            player,
            ArmorCategory.Light);

        // Assert
        result.Success.Should().BeFalse();
        result.PrimaryFailureReason.Should().Contain("Already at Master");
    }

    [Test]
    public async Task TrainArmorProficiencyAsync_SkippingLevels_Fails()
    {
        // Arrange
        var player = CreateTestPlayer(level: 10, silverAmount: 10000);

        // Current proficiency is NonProficient (default)
        // Trying to skip directly to Expert

        // Act
        var result = await _service.TrainArmorProficiencyAsync(
            player,
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.Expert);

        // Assert
        result.Success.Should().BeFalse();
        result.PrimaryFailureReason.Should().Contain("Must be at Proficient proficiency");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Eligibility Check Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task CanTrainAsync_WhenEligible_ReturnsEligible()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, silverAmount: 100);

        // Act
        var eligibility = await _service.CanTrainAsync(
            player,
            ArmorCategory.Light,
            ArmorProficiencyLevel.Proficient);

        // Assert
        eligibility.IsEligible.Should().BeTrue();
        eligibility.TargetLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        eligibility.HasBlockingReasons.Should().BeFalse();
    }

    [Test]
    public async Task CanTrainAsync_WithMultipleBlockingReasons_ReturnsAllReasons()
    {
        // Arrange
        var player = CreateTestPlayer(level: 3, silverAmount: 50);

        // Set up archetype provider to return Proficient
        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency(It.IsAny<string>(), ArmorCategory.Heavy))
            .Returns(ArmorProficiencyLevel.Proficient);

        // Act - Try to train to Expert with insufficient level AND currency
        var eligibility = await _service.CanTrainAsync(
            player,
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.Expert);

        // Assert
        eligibility.IsEligible.Should().BeFalse();
        eligibility.BlockingReasons.Should().HaveCountGreaterOrEqualTo(2);
        eligibility.BlockingReasons.Should().Contain(r => r.Contains("level"));
        eligibility.BlockingReasons.Should().Contain(r => r.Contains("funds"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Training Requirements Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetTrainingRequirementsAsync_ForProficient_ReturnsCorrectRequirements()
    {
        // Arrange & Act
        var requirement = await _service.GetTrainingRequirementsAsync(
            ArmorCategory.Light,
            ArmorProficiencyLevel.Proficient);

        // Assert
        requirement.Should().NotBeNull();
        requirement!.Value.TargetLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        requirement.Value.CurrencyCost.Should().Be(50);
        requirement.Value.TrainingWeeks.Should().Be(2);
    }

    [Test]
    public async Task GetTrainingRequirementsAsync_ForNonProficient_ReturnsNull()
    {
        // Arrange & Act
        var requirement = await _service.GetTrainingRequirementsAsync(
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.NonProficient);

        // Assert
        requirement.Should().BeNull();
    }

    [Test]
    public async Task GetAllTrainingRequirementsAsync_ReturnsAllLevels()
    {
        // Arrange & Act
        var requirements = await _service.GetAllTrainingRequirementsAsync(ArmorCategory.Medium);

        // Assert
        requirements.Should().HaveCount(3);
        requirements.Should().ContainKey(ArmorProficiencyLevel.Proficient);
        requirements.Should().ContainKey(ArmorProficiencyLevel.Expert);
        requirements.Should().ContainKey(ArmorProficiencyLevel.Master);
    }

    [Test]
    public async Task GetNextTrainingRequirementAsync_WhenNotAtMax_ReturnsNextLevel()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, silverAmount: 0);

        // Act
        var requirement = await _service.GetNextTrainingRequirementAsync(
            player,
            ArmorCategory.Light);

        // Assert
        requirement.Should().NotBeNull();
        requirement!.Value.TargetLevel.Should().Be(ArmorProficiencyLevel.Proficient);
    }

    [Test]
    public async Task GetNextTrainingRequirementAsync_WhenAtMax_ReturnsNull()
    {
        // Arrange
        var player = CreateTestPlayer(level: 15, silverAmount: 0);

        // Set up archetype provider to return Master
        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency(It.IsAny<string>(), ArmorCategory.Light))
            .Returns(ArmorProficiencyLevel.Master);

        // Act
        var requirement = await _service.GetNextTrainingRequirementAsync(
            player,
            ArmorCategory.Light);

        // Assert
        requirement.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Query Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetCurrentProficiencyAsync_WithArchetypeProficiency_ReturnsArchetypeLevel()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, silverAmount: 0);

        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency("warrior", ArmorCategory.Heavy))
            .Returns(ArmorProficiencyLevel.Proficient);

        // Act
        var level = await _service.GetCurrentProficiencyAsync(player, ArmorCategory.Heavy);

        // Assert
        level.Should().Be(ArmorProficiencyLevel.Proficient);
    }

    [Test]
    public async Task GetCurrentProficiencyAsync_WithoutArchetypeProficiency_ReturnsNonProficient()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, silverAmount: 0);

        // Act
        var level = await _service.GetCurrentProficiencyAsync(player, ArmorCategory.Light);

        // Assert
        level.Should().Be(ArmorProficiencyLevel.NonProficient);
    }

    [Test]
    public async Task GetAllProficienciesAsync_ReturnsAllCategories()
    {
        // Arrange
        var player = CreateTestPlayer(level: 1, silverAmount: 0);

        // Act
        var proficiencies = await _service.GetAllProficienciesAsync(player);

        // Assert
        var categoryCount = Enum.GetValues<ArmorCategory>().Length;
        proficiencies.Should().HaveCount(categoryCount);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a test player with specified level and currency.
    /// </summary>
    private static Player CreateTestPlayer(int level, int silverAmount)
    {
        // Create player with default stats
        var player = new Player("TestPlayer");

        // Add currency
        if (silverAmount > 0)
        {
            player.AddCurrency("silver", silverAmount);
        }

        // Set archetype ID for archetype provider mock
        var archetypeProperty = typeof(Player).GetProperty("ArchetypeId");
        if (archetypeProperty?.CanWrite == true)
        {
            archetypeProperty.SetValue(player, "warrior");
        }

        // Set level using reflection if needed
        var levelProperty = typeof(Player).GetProperty("Level");
        if (levelProperty?.CanWrite == true)
        {
            levelProperty.SetValue(player, level);
        }

        return player;
    }
}
