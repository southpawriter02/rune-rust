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
using RuneAndRust.TestUtilities.Builders;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for the SearchService.
/// </summary>
[TestFixture]
public class SearchServiceTests
{
    private SearchService _service = null!;
    private SkillCheckService _skillCheckService = null!;
    private Mock<IGameConfigurationProvider> _mockConfig = null!;

    [SetUp]
    public void SetUp()
    {
        _mockConfig = new Mock<IGameConfigurationProvider>();
        SetupDefaultMocks();

        var seededRandom = new Random(8);
        var diceService = new DiceService(NullLogger<DiceService>.Instance, seededRandom);
        _skillCheckService = new SkillCheckService(
            diceService,
            _mockConfig.Object,
            NullLogger<SkillCheckService>.Instance);

        _service = new SearchService(
            _skillCheckService,
            NullLogger<SearchService>.Instance);
    }

    private void SetupDefaultMocks()
    {
        var perception = SkillDefinition.Create(
            "perception", "Perception", "Notice things.",
            "wits", null, "1d10");

        var investigation = SkillDefinition.Create(
            "investigation", "Investigation", "Study clues.",
            "wits", null, "1d10");

        _mockConfig.Setup(c => c.GetSkillById("perception")).Returns(perception);
        _mockConfig.Setup(c => c.GetSkillById("investigation")).Returns(investigation);
    }

    [Test]
    public void PerformSearch_NoHiddenContent_ReturnsNothingHidden()
    {
        // Arrange
        var room = new Room("Test Room", "A test room.", Position3D.Origin);
        room.AddExit(Direction.North, Guid.NewGuid());
        var player = PlayerBuilder.Create().Build();

        // Act
        var result = _service.PerformSearch(player, room);

        // Assert
        result.NothingToFind.Should().BeTrue();
        result.SkillCheck.Should().BeNull();
    }

    [Test]
    public void PerformSearch_WithHiddenExit_PerformsSkillCheck()
    {
        // Arrange
        var room = new Room("Test Room", "A test room.", Position3D.Origin);
        room.AddHiddenExit(Direction.East, Guid.NewGuid(), 10);
        var player = CreateTestPlayer(wits: 20); // High wits for likely success

        // Act
        var result = _service.PerformSearch(player, room);

        // Assert
        result.NothingToFind.Should().BeFalse();
        result.SkillCheck.Should().NotBeNull();
        result.SkillCheck!.Value.SkillId.Should().Be("perception");
    }

    [Test]
    public void PerformSearch_SuccessfulCheck_RevealsHiddenExit()
    {
        // Arrange
        var room = new Room("Test Room", "A test room.", Position3D.Origin);
        room.AddHiddenExit(Direction.East, Guid.NewGuid(), 1); // DC 1 = very easy
        var player = CreateTestPlayer(wits: 20);

        // Act
        var result = _service.PerformSearch(player, room);

        // Assert - with DC 1 and high wits, should always succeed
        result.IsSuccess.Should().BeTrue();
        result.DiscoveredExits.Should().Contain(Direction.East);
        room.GetVisibleExits().Should().ContainKey(Direction.East);
    }

    [Test]
    public void PerformSearch_WithHiddenItem_CanRevealItem()
    {
        // Arrange
        var room = new Room("Test Room", "A test room.", Position3D.Origin);
        var item = new Item("Ancient Key", "A rusty key.", ItemType.Quest);
        var hiddenItem = HiddenItem.Create(item, 1); // DC 1 = very easy
        room.AddHiddenItem(hiddenItem);
        var player = CreateTestPlayer(wits: 20);

        // Act
        var result = _service.PerformSearch(player, room);

        // Assert - with DC 1 and high wits, should always succeed
        result.IsSuccess.Should().BeTrue();
        result.DiscoveredItems.Should().HaveCount(1);
        room.Items.Should().Contain(i => i.Name == "Ancient Key");
    }

    [Test]
    public void PerformSearch_ThrowsOnNullPlayer()
    {
        // Arrange
        var room = new Room("Test", "Test", Position3D.Origin);

        // Act
        var act = () => _service.PerformSearch(null!, room);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void PerformSearch_ThrowsOnNullRoom()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();

        // Act
        var act = () => _service.PerformSearch(player, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static Player CreateTestPlayer(int wits = 8)
    {
        var attributes = new PlayerAttributes(8, 8, 8, wits, 8);
        return new Player("TestPlayer", "human", "soldier", attributes, "Test");
    }
}
