using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="GatheringService"/> (v0.11.0c).
/// </summary>
/// <remarks>
/// <para>
/// These tests verify the gathering service's core functionality:
/// </para>
/// <list type="bullet">
///   <item><description>Feature access and filtering</description></item>
///   <item><description>Gathering validation (depletion, tool requirements)</description></item>
///   <item><description>Skill check integration (2d6 + skill via ISkillService)</description></item>
///   <item><description>Resource quantity and quality determination</description></item>
///   <item><description>Feature depletion and replenishment</description></item>
///   <item><description>Event logging</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class GatheringServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IHarvestableFeatureProvider> _featureProviderMock = null!;
    private Mock<IResourceProvider> _resourceProviderMock = null!;
    private Mock<ISkillService> _skillServiceMock = null!;
    private Mock<IGameEventLogger> _eventLoggerMock = null!;
    private ILogger<GatheringService> _logger = null!;
    private GatheringService _service = null!;

    // Test data - feature definitions
    private HarvestableFeatureDefinition _ironOreVeinDefinition = null!;
    private HarvestableFeatureDefinition _goldOreVeinDefinition = null!;
    private HarvestableFeatureDefinition _herbPatchDefinition = null!;

    // Test data - resource definitions
    private ResourceDefinition _ironOreResource = null!;
    private ResourceDefinition _goldOreResource = null!;
    private ResourceDefinition _healingHerbResource = null!;

    [SetUp]
    public void SetUp()
    {
        // Create mocks
        _featureProviderMock = new Mock<IHarvestableFeatureProvider>();
        _resourceProviderMock = new Mock<IResourceProvider>();
        _skillServiceMock = new Mock<ISkillService>();
        _eventLoggerMock = new Mock<IGameEventLogger>();
        _logger = NullLogger<GatheringService>.Instance;

        // Set up test definitions for harvestable features
        _ironOreVeinDefinition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein", "Iron Ore Vein", "A vein of iron ore",
            "iron-ore", minQuantity: 1, maxQuantity: 5, difficultyClass: 12);

        _goldOreVeinDefinition = HarvestableFeatureDefinition.Create(
            "gold-ore-vein", "Gold Ore Vein", "A vein of gold ore",
            "gold-ore", minQuantity: 1, maxQuantity: 3, difficultyClass: 15,
            requiredToolId: "pickaxe");

        _herbPatchDefinition = HarvestableFeatureDefinition.Create(
            "herb-patch", "Herb Patch", "A patch of healing herbs",
            "healing-herb", minQuantity: 2, maxQuantity: 6, difficultyClass: 10,
            replenishTurns: 100);

        // Set up test definitions for resources
        _ironOreResource = ResourceDefinition.Create(
            "iron-ore", "Iron Ore", "Common iron ore",
            ResourceCategory.Ore, ResourceQuality.Common, baseValue: 5, stackSize: 20);

        _goldOreResource = ResourceDefinition.Create(
            "gold-ore", "Gold Ore", "Valuable gold ore",
            ResourceCategory.Ore, ResourceQuality.Fine, baseValue: 25, stackSize: 20);

        _healingHerbResource = ResourceDefinition.Create(
            "healing-herb", "Healing Herb", "A common healing herb",
            ResourceCategory.Herb, ResourceQuality.Common, baseValue: 3, stackSize: 50);

        // Set up default feature provider mock behavior
        _featureProviderMock.Setup(p => p.GetFeature("iron-ore-vein")).Returns(_ironOreVeinDefinition);
        _featureProviderMock.Setup(p => p.GetFeature("gold-ore-vein")).Returns(_goldOreVeinDefinition);
        _featureProviderMock.Setup(p => p.GetFeature("herb-patch")).Returns(_herbPatchDefinition);
        _featureProviderMock.Setup(p => p.Count).Returns(3);

        // Set up default resource provider mock behavior
        _resourceProviderMock.Setup(p => p.GetResource("iron-ore")).Returns(_ironOreResource);
        _resourceProviderMock.Setup(p => p.GetResource("gold-ore")).Returns(_goldOreResource);
        _resourceProviderMock.Setup(p => p.GetResource("healing-herb")).Returns(_healingHerbResource);
        _resourceProviderMock.Setup(p => p.Count).Returns(3);

        // Set up default skill service mock behavior
        _skillServiceMock.Setup(s => s.GetSkillBonus(It.IsAny<Player>(), "survival")).Returns(3);

        // Create service with seeded random for deterministic tests
        var seededRandom = new Random(42);
        _service = new GatheringService(
            _featureProviderMock.Object,
            _resourceProviderMock.Object,
            _skillServiceMock.Object,
            _eventLoggerMock.Object,
            _logger,
            seededRandom);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a test player for use in gathering tests.
    /// </summary>
    private Player CreateTestPlayer() => new Player("TestPlayer");

    /// <summary>
    /// Creates a test room for use in gathering tests.
    /// </summary>
    private Room CreateTestRoom()
    {
        return new Room(
            "Test Cave",
            "A rocky cave",
            Position3D.Origin);
    }

    /// <summary>
    /// Creates an iron ore feature with the specified quantity.
    /// </summary>
    private HarvestableFeature CreateIronOreFeature(int quantity = 5)
    {
        return HarvestableFeature.Create(_ironOreVeinDefinition, quantity);
    }

    /// <summary>
    /// Creates a gold ore feature with the specified quantity.
    /// </summary>
    private HarvestableFeature CreateGoldOreFeature(int quantity = 3)
    {
        return HarvestableFeature.Create(_goldOreVeinDefinition, quantity);
    }

    /// <summary>
    /// Creates an herb patch feature with the specified quantity.
    /// </summary>
    private HarvestableFeature CreateHerbPatchFeature(int quantity = 4)
    {
        return HarvestableFeature.Create(_herbPatchDefinition, quantity);
    }

    /// <summary>
    /// Sets up the skill service mock to return a specific skill check outcome.
    /// </summary>
    /// <param name="roll">The dice roll (2d6 result).</param>
    /// <param name="skillBonus">The skill bonus.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <remarks>
    /// The success, total, and margin are automatically calculated from the parameters.
    /// </remarks>
    private void SetupSkillCheck(int roll, int skillBonus, int dc)
    {
        var total = roll + skillBonus;
        var success = total >= dc;
        var margin = total - dc;

        _skillServiceMock
            .Setup(s => s.PerformSkillCheck(It.IsAny<Player>(), "survival", dc))
            .Returns(new SkillCheckOutcome(
                SkillId: "survival",
                Success: success,
                Roll: roll,
                SkillBonus: skillBonus,
                Total: total,
                DC: dc,
                Margin: margin,
                Message: success ? "Success!" : "Failed."));
    }

    /// <summary>
    /// Sets up a successful skill check with standard test values.
    /// </summary>
    /// <remarks>
    /// Roll: 9 (2d6), Bonus: 3, Total: 12, vs DC 12 = Success (margin 0).
    /// </remarks>
    private void SetupSuccessfulSkillCheck()
    {
        SetupSkillCheck(roll: 9, skillBonus: 3, dc: 12);
    }

    /// <summary>
    /// Sets up a failed skill check with standard test values.
    /// </summary>
    /// <remarks>
    /// Roll: 5 (2d6), Bonus: 3, Total: 8, vs DC 12 = Failure (margin -4).
    /// </remarks>
    private void SetupFailedSkillCheck()
    {
        SetupSkillCheck(roll: 5, skillBonus: 3, dc: 12);
    }

    /// <summary>
    /// Sets up a high margin skill check that triggers quality upgrade.
    /// </summary>
    /// <remarks>
    /// Roll: 12 (2d6 max), Bonus: 3, Total: 15, vs DC 12 = margin 3.
    /// Note: With 2d6 system, reaching margin >= 10 is impossible at DC 12.
    /// To test quality upgrade, we need lower DC or mock differently.
    /// </remarks>
    private void SetupHighMarginSkillCheck(int dc)
    {
        // With 2d6 (max 12) + bonus 3 = 15 max, we need DC <= 5 for margin >= 10
        var roll = 12; // Max 2d6 roll
        var skillBonus = 3;
        var total = roll + skillBonus;
        var margin = total - dc;

        _skillServiceMock
            .Setup(s => s.PerformSkillCheck(It.IsAny<Player>(), "survival", dc))
            .Returns(new SkillCheckOutcome(
                SkillId: "survival",
                Success: true,
                Roll: roll,
                SkillBonus: skillBonus,
                Total: total,
                DC: dc,
                Margin: margin,
                Message: "Critical success!"));
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullFeatureProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GatheringService(
            null!,
            _resourceProviderMock.Object,
            _skillServiceMock.Object,
            _eventLoggerMock.Object,
            _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("featureProvider");
    }

    [Test]
    public void Constructor_WithNullResourceProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GatheringService(
            _featureProviderMock.Object,
            null!,
            _skillServiceMock.Object,
            _eventLoggerMock.Object,
            _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("resourceProvider");
    }

    [Test]
    public void Constructor_WithNullSkillService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GatheringService(
            _featureProviderMock.Object,
            _resourceProviderMock.Object,
            null!,
            _eventLoggerMock.Object,
            _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("skillService");
    }

    [Test]
    public void Constructor_WithNullEventLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GatheringService(
            _featureProviderMock.Object,
            _resourceProviderMock.Object,
            _skillServiceMock.Object,
            null!,
            _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("eventLogger");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GatheringService(
            _featureProviderMock.Object,
            _resourceProviderMock.Object,
            _skillServiceMock.Object,
            _eventLoggerMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET HARVESTABLE FEATURES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetHarvestableFeatures_WithAvailableFeatures_ReturnsNonDepletedFeatures()
    {
        // Arrange
        var room = CreateTestRoom();
        var feature1 = CreateIronOreFeature();
        var feature2 = CreateGoldOreFeature();
        room.AddHarvestableFeature(feature1);
        room.AddHarvestableFeature(feature2);

        // Act
        var result = _service.GetHarvestableFeatures(room);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(feature1);
        result.Should().Contain(feature2);
    }

    [Test]
    public void GetHarvestableFeatures_WithDepletedFeature_ExcludesIt()
    {
        // Arrange
        var room = CreateTestRoom();
        var availableFeature = CreateIronOreFeature(5);
        var depletedFeature = CreateGoldOreFeature(3);
        depletedFeature.Harvest(3); // Deplete it

        room.AddHarvestableFeature(availableFeature);
        room.AddHarvestableFeature(depletedFeature);

        // Act
        var result = _service.GetHarvestableFeatures(room);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(availableFeature);
        result.Should().NotContain(depletedFeature);
    }

    [Test]
    public void GetHarvestableFeatures_WithNoFeatures_ReturnsEmptyList()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var result = _service.GetHarvestableFeatures(room);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetHarvestableFeatures_WithNullRoom_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.GetHarvestableFeatures(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("room");
    }

    // ═══════════════════════════════════════════════════════════════
    // CAN GATHER VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CanGather_WithValidFeature_ReturnsSuccessWithDC()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature();

        // Act
        var result = _service.CanGather(player, feature);

        // Assert
        result.IsValid.Should().BeTrue();
        result.DifficultyClass.Should().Be(12);
        result.FailureReason.Should().BeNull();
    }

    [Test]
    public void CanGather_WithDepletedFeature_ReturnsFailure()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(3);
        feature.Harvest(3); // Deplete it

        // Act
        var result = _service.CanGather(player, feature);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("depleted");
    }

    [Test]
    public void CanGather_WithUnknownFeatureDefinition_ReturnsFailure()
    {
        // Arrange
        var player = CreateTestPlayer();
        var unknownDefinition = HarvestableFeatureDefinition.Create(
            "unknown-feature", "Unknown", "Unknown", "unknown-resource",
            1, 1, 10);
        var feature = HarvestableFeature.Create(unknownDefinition, 5);

        _featureProviderMock.Setup(p => p.GetFeature("unknown-feature")).Returns((HarvestableFeatureDefinition?)null);

        // Act
        var result = _service.CanGather(player, feature);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("Unknown");
    }

    [Test]
    public void CanGather_WithNullPlayer_ThrowsArgumentNullException()
    {
        // Arrange
        var feature = CreateIronOreFeature();

        // Act
        var act = () => _service.CanGather(null!, feature);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("player");
    }

    [Test]
    public void CanGather_WithNullFeature_ThrowsArgumentNullException()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var act = () => _service.CanGather(player, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("feature");
    }

    // ═══════════════════════════════════════════════════════════════
    // GATHER TESTS - SUCCESS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Gather_WithSuccessfulRoll_ReturnsSuccessResult()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(5);
        SetupSkillCheck(roll: 9, skillBonus: 3, dc: 12); // Total 12 vs DC 12 = success

        // Act
        var result = _service.Gather(player, feature);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Roll.Should().Be(9);
        result.Modifier.Should().Be(3);
        result.Total.Should().Be(12);
        result.DifficultyClass.Should().Be(12);
        result.ResourceId.Should().Be("iron-ore");
        result.ResourceName.Should().Be("Iron Ore");
        result.Quantity.Should().BePositive();
        result.Quality.Should().NotBeNull();
    }

    [Test]
    public void Gather_WithSuccessfulRoll_DepletesFeature()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(3);
        SetupSkillCheck(roll: 10, skillBonus: 3, dc: 12); // Total 13 vs DC 12 = success

        var initialQuantity = feature.RemainingQuantity;

        // Act
        var result = _service.Gather(player, feature);

        // Assert
        result.IsSuccess.Should().BeTrue();
        feature.RemainingQuantity.Should().BeLessThan(initialQuantity);
    }

    [Test]
    public void Gather_WithHighMargin_UpgradesQuality()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Use herb patch which has DC 10 - with max roll 12 + bonus 3 = 15, margin = 5
        // To achieve margin >= 10, we need a very low DC feature
        // Create a custom easy feature for this test
        var easyDefinition = HarvestableFeatureDefinition.Create(
            "easy-ore", "Easy Ore", "Easy to gather ore",
            "iron-ore", minQuantity: 1, maxQuantity: 5, difficultyClass: 5);
        var feature = HarvestableFeature.Create(easyDefinition, 5);

        _featureProviderMock.Setup(p => p.GetFeature("easy-ore")).Returns(easyDefinition);

        // Roll 12 (max 2d6) + 3 bonus = 15 vs DC 5 = margin of 10
        SetupHighMarginSkillCheck(dc: 5);

        // Act
        var result = _service.Gather(player, feature);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Margin.Should().BeGreaterThanOrEqualTo(10);
        // Iron ore is Common, should upgrade to Fine
        result.Quality.Should().Be(ResourceQuality.Fine);
    }

    [Test]
    public void Gather_WithSuccess_PublishesGatherAttemptedEvent()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(5);
        SetupSuccessfulSkillCheck();

        // Act
        _ = _service.Gather(player, feature);

        // Assert
        _eventLoggerMock.Verify(
            e => e.LogInteraction(
                "GatherAttempted",
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // GATHER TESTS - FAILURE
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Gather_WithFailedRoll_ReturnsFailedResult()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(5);
        SetupSkillCheck(roll: 5, skillBonus: 3, dc: 12); // Total 8 vs DC 12 = failure

        // Act
        var result = _service.Gather(player, feature);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Roll.Should().Be(5);
        result.Total.Should().Be(8);
        result.DifficultyClass.Should().Be(12);
        result.FailureReason.Should().NotBeNull();
        result.ResourceId.Should().BeNull();
        result.Quantity.Should().Be(0);
    }

    [Test]
    public void Gather_WithFailedRoll_DoesNotDepleteFeature()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(5);
        SetupFailedSkillCheck();

        var initialQuantity = feature.RemainingQuantity;

        // Act
        _ = _service.Gather(player, feature);

        // Assert
        feature.RemainingQuantity.Should().Be(initialQuantity);
    }

    [Test]
    public void Gather_WithDepletedFeature_ReturnsValidationFailure()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(3);
        feature.Harvest(3); // Deplete it

        // Act
        var result = _service.Gather(player, feature);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsValidationFailure.Should().BeTrue();
        result.FailureReason.Should().Contain("depleted");
    }

    [Test]
    public void Gather_WithFailure_DoesNotPublishResourceGatheredEvent()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(5);
        SetupFailedSkillCheck();

        // Act
        _ = _service.Gather(player, feature);

        // Assert
        _eventLoggerMock.Verify(
            e => e.LogInteraction(
                "ResourceGathered",
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // GATHER INFORMATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetGatherDC_WithValidFeature_ReturnsDC()
    {
        // Arrange
        var feature = CreateIronOreFeature();

        // Act
        var dc = _service.GetGatherDC(feature);

        // Assert
        dc.Should().Be(12);
    }

    [Test]
    public void GetGatherDC_WithUnknownDefinition_ReturnsZero()
    {
        // Arrange
        var unknownDefinition = HarvestableFeatureDefinition.Create(
            "unknown", "Unknown", "Unknown", "unknown",
            1, 1, 10);
        var feature = HarvestableFeature.Create(unknownDefinition, 5);

        _featureProviderMock.Setup(p => p.GetFeature("unknown")).Returns((HarvestableFeatureDefinition?)null);

        // Act
        var dc = _service.GetGatherDC(feature);

        // Assert
        dc.Should().Be(0);
    }

    [Test]
    public void GetGatherModifier_ReturnsSkillBonus()
    {
        // Arrange
        var player = CreateTestPlayer();
        _skillServiceMock.Setup(s => s.GetSkillBonus(player, "survival")).Returns(5);

        // Act
        var modifier = _service.GetGatherModifier(player);

        // Assert
        modifier.Should().Be(5);
    }

    [Test]
    public void GetGatherDC_WithNullFeature_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.GetGatherDC(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("feature");
    }

    [Test]
    public void GetGatherModifier_WithNullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.GetGatherModifier(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("player");
    }

    // ═══════════════════════════════════════════════════════════════
    // REPLENISHMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessReplenishment_WithReadyFeature_ReplenishesIt()
    {
        // Arrange
        var room = CreateTestRoom();
        var feature = CreateHerbPatchFeature(4);
        feature.Harvest(4); // Deplete
        feature.SetReplenishTimer(currentTurn: 0, replenishTurns: 100);
        room.AddHarvestableFeature(feature);

        // Act
        var replenished = _service.ProcessReplenishment(room, currentTurn: 100);

        // Assert
        replenished.Should().HaveCount(1);
        replenished.Should().Contain(feature);
        feature.IsDepleted.Should().BeFalse();
        feature.RemainingQuantity.Should().BePositive();
    }

    [Test]
    public void ProcessReplenishment_WithNotReadyFeature_DoesNotReplenish()
    {
        // Arrange
        var room = CreateTestRoom();
        var feature = CreateHerbPatchFeature(4);
        feature.Harvest(4);
        feature.SetReplenishTimer(currentTurn: 0, replenishTurns: 100);
        room.AddHarvestableFeature(feature);

        // Act
        var replenished = _service.ProcessReplenishment(room, currentTurn: 50);

        // Assert
        replenished.Should().BeEmpty();
        feature.IsDepleted.Should().BeTrue();
    }

    [Test]
    public void ProcessReplenishment_WithNoFeatures_ReturnsEmpty()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var replenished = _service.ProcessReplenishment(room, currentTurn: 100);

        // Assert
        replenished.Should().BeEmpty();
    }

    [Test]
    public void ProcessReplenishment_WithNullRoom_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.ProcessReplenishment(null!, currentTurn: 100);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("room");
    }

    [Test]
    public void ProcessReplenishment_WithReplenishedFeature_PublishesEvent()
    {
        // Arrange
        var room = CreateTestRoom();
        var feature = CreateHerbPatchFeature(4);
        feature.Harvest(4);
        feature.SetReplenishTimer(currentTurn: 0, replenishTurns: 100);
        room.AddHarvestableFeature(feature);

        // Act
        _ = _service.ProcessReplenishment(room, currentTurn: 100);

        // Assert
        _eventLoggerMock.Verify(
            e => e.LogInteraction(
                "FeatureReplenished",
                It.Is<string>(s => s.Contains("Herb Patch")),
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // EVENT LOGGING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Gather_WithSuccess_LogsResourceGatheredEvent()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(5);
        SetupSuccessfulSkillCheck();

        // Act
        _ = _service.Gather(player, feature);

        // Assert
        _eventLoggerMock.Verify(
            e => e.LogInteraction(
                "ResourceGathered",
                It.Is<string>(s => s.Contains("gathered") && s.Contains("Iron Ore")),
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    [Test]
    public void Gather_AlwaysLogsGatherAttemptedEvent()
    {
        // Arrange
        var player = CreateTestPlayer();
        var feature = CreateIronOreFeature(5);
        SetupFailedSkillCheck();

        // Act
        _ = _service.Gather(player, feature);

        // Assert
        _eventLoggerMock.Verify(
            e => e.LogInteraction(
                "GatherAttempted",
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }
}
