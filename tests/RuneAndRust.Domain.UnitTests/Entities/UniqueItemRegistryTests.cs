using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="UniqueItemRegistry"/> entity.
/// </summary>
/// <remarks>
/// Tests cover all CRUD operations, duplicate prevention, run lifecycle,
/// filtering by source type, and edge cases.
/// </remarks>
[TestFixture]
public class UniqueItemRegistryTests
{
    private UniqueItemRegistry _registry = null!;
    private Guid _runId;

    /// <summary>
    /// Sets up a fresh registry before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _runId = Guid.NewGuid();
        _registry = UniqueItemRegistry.Create(_runId);
    }

    /// <summary>
    /// Verifies that Create initializes the registry with correct defaults.
    /// </summary>
    [Test]
    public void Create_WithRunId_InitializesCorrectly()
    {
        // Assert
        _registry.Id.Should().NotBeEmpty();
        _registry.RunId.Should().Be(_runId);
        _registry.GetDroppedCount().Should().Be(0);
        _registry.DropHistory.Should().BeEmpty();
        _registry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that HasDropped returns false for an unregistered item.
    /// </summary>
    [Test]
    public void HasDropped_WithUnregisteredItem_ReturnsFalse()
    {
        // Arrange & Act
        var result = _registry.HasDropped("shadowfang-blade");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasDropped returns true for a registered item.
    /// </summary>
    [Test]
    public void HasDropped_WithRegisteredItem_ReturnsTrue()
    {
        // Arrange
        _registry.RegisterDrop("shadowfang-blade", DropSourceType.Boss, "shadow-lord");

        // Act
        var result = _registry.HasDropped("shadowfang-blade");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasDropped is case-insensitive.
    /// </summary>
    [Test]
    public void HasDropped_IsCaseInsensitive()
    {
        // Arrange
        _registry.RegisterDrop("shadowfang-blade", DropSourceType.Boss, "shadow-lord");

        // Act & Assert
        _registry.HasDropped("SHADOWFANG-BLADE").Should().BeTrue();
        _registry.HasDropped("Shadowfang-Blade").Should().BeTrue();
        _registry.HasDropped("shadowfang-BLADE").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasDropped throws when itemId is null.
    /// </summary>
    [Test]
    public void HasDropped_WithNullItemId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _registry.HasDropped(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("itemId");
    }

    /// <summary>
    /// Verifies that RegisterDrop adds to both history and set.
    /// </summary>
    [Test]
    public void RegisterDrop_AddsToHistoryAndSet()
    {
        // Arrange & Act
        var record = _registry.RegisterDrop(
            "shadowfang-blade",
            DropSourceType.Boss,
            "shadow-lord");

        // Assert
        _registry.GetDroppedCount().Should().Be(1);
        _registry.DropHistory.Should().ContainSingle()
            .Which.ItemId.Should().Be("shadowfang-blade");
        record.ItemId.Should().Be("shadowfang-blade");
        record.SourceType.Should().Be(DropSourceType.Boss);
        record.SourceId.Should().Be("shadow-lord");
    }

    /// <summary>
    /// Verifies that RegisterDrop normalizes item IDs to lowercase.
    /// </summary>
    [Test]
    public void RegisterDrop_NormalizesItemId()
    {
        // Arrange & Act
        var record = _registry.RegisterDrop("SHADOWFANG-BLADE", DropSourceType.Boss, "boss-id");

        // Assert
        record.ItemId.Should().Be("shadowfang-blade");
        _registry.HasDropped("shadowfang-blade").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that RegisterDrop throws when trying to register an already-dropped item.
    /// </summary>
    [Test]
    public void RegisterDrop_WithAlreadyDroppedItem_ThrowsInvalidOperationException()
    {
        // Arrange
        _registry.RegisterDrop("shadowfang-blade", DropSourceType.Boss, "shadow-lord");

        // Act
        var act = () => _registry.RegisterDrop(
            "shadowfang-blade",
            DropSourceType.Container,
            "chest");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already dropped*");
    }

    /// <summary>
    /// Verifies that duplicate detection is case-insensitive.
    /// </summary>
    [Test]
    public void RegisterDrop_WithDifferentCasing_ThrowsInvalidOperationException()
    {
        // Arrange
        _registry.RegisterDrop("shadowfang-blade", DropSourceType.Boss, "shadow-lord");

        // Act
        var act = () => _registry.RegisterDrop(
            "SHADOWFANG-BLADE",
            DropSourceType.Container,
            "chest");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already dropped*");
    }

    /// <summary>
    /// Verifies that RegisterDrop throws when itemId is null.
    /// </summary>
    [Test]
    public void RegisterDrop_WithNullItemId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _registry.RegisterDrop(null!, DropSourceType.Boss, "boss-id");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("itemId");
    }

    /// <summary>
    /// Verifies that RegisterDrop throws when sourceId is null.
    /// </summary>
    [Test]
    public void RegisterDrop_WithNullSourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _registry.RegisterDrop("item-id", DropSourceType.Boss, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("sourceId");
    }

    /// <summary>
    /// Verifies that multiple different items can be registered.
    /// </summary>
    [Test]
    public void RegisterDrop_MultipleDifferentItems_AllTracked()
    {
        // Arrange & Act
        _registry.RegisterDrop("item-1", DropSourceType.Boss, "boss-1");
        _registry.RegisterDrop("item-2", DropSourceType.Container, "chest-1");
        _registry.RegisterDrop("item-3", DropSourceType.Quest, "quest-1");

        // Assert
        _registry.GetDroppedCount().Should().Be(3);
        _registry.HasDropped("item-1").Should().BeTrue();
        _registry.HasDropped("item-2").Should().BeTrue();
        _registry.HasDropped("item-3").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Reset clears all state and sets the new run ID.
    /// </summary>
    [Test]
    public void Reset_ClearsAllStateAndSetsNewRunId()
    {
        // Arrange
        _registry.RegisterDrop("item-1", DropSourceType.Boss, "boss-1");
        _registry.RegisterDrop("item-2", DropSourceType.Container, "chest-1");
        var newRunId = Guid.NewGuid();

        // Act
        _registry.Reset(newRunId);

        // Assert
        _registry.RunId.Should().Be(newRunId);
        _registry.GetDroppedCount().Should().Be(0);
        _registry.DropHistory.Should().BeEmpty();
        _registry.HasDropped("item-1").Should().BeFalse();
        _registry.HasDropped("item-2").Should().BeFalse();
        _registry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that items can be registered after a reset.
    /// </summary>
    [Test]
    public void Reset_AllowsReregistrationOfPreviouslyDroppedItems()
    {
        // Arrange
        _registry.RegisterDrop("item-1", DropSourceType.Boss, "boss-1");
        var newRunId = Guid.NewGuid();
        _registry.Reset(newRunId);

        // Act - should not throw
        var act = () => _registry.RegisterDrop("item-1", DropSourceType.Boss, "boss-1");

        // Assert
        act.Should().NotThrow();
        _registry.GetDroppedCount().Should().Be(1);
    }

    /// <summary>
    /// Verifies that IsCurrentRun returns true for matching run ID.
    /// </summary>
    [Test]
    public void IsCurrentRun_WithMatchingRunId_ReturnsTrue()
    {
        // Assert
        _registry.IsCurrentRun(_runId).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsCurrentRun returns false for non-matching run ID.
    /// </summary>
    [Test]
    public void IsCurrentRun_WithDifferentRunId_ReturnsFalse()
    {
        // Assert
        _registry.IsCurrentRun(Guid.NewGuid()).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetDropsBySourceType filters correctly.
    /// </summary>
    [Test]
    public void GetDropsBySourceType_FiltersCorrectly()
    {
        // Arrange
        _registry.RegisterDrop("item-1", DropSourceType.Boss, "boss-1");
        _registry.RegisterDrop("item-2", DropSourceType.Container, "chest-1");
        _registry.RegisterDrop("item-3", DropSourceType.Boss, "boss-2");

        // Act
        var bossDrops = _registry.GetDropsBySourceType(DropSourceType.Boss).ToList();
        var containerDrops = _registry.GetDropsBySourceType(DropSourceType.Container).ToList();
        var questDrops = _registry.GetDropsBySourceType(DropSourceType.Quest).ToList();

        // Assert
        bossDrops.Should().HaveCount(2);
        bossDrops.Select(d => d.ItemId).Should().Contain("item-1").And.Contain("item-3");
        containerDrops.Should().HaveCount(1);
        containerDrops.Single().ItemId.Should().Be("item-2");
        questDrops.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetLastDrop returns null when no drops exist.
    /// </summary>
    [Test]
    public void GetLastDrop_WithNoDrops_ReturnsNull()
    {
        // Act
        var result = _registry.GetLastDrop();

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetLastDrop returns the most recent drop.
    /// </summary>
    [Test]
    public void GetLastDrop_WithDrops_ReturnsMostRecent()
    {
        // Arrange
        _registry.RegisterDrop("item-1", DropSourceType.Boss, "boss-1");
        _registry.RegisterDrop("item-2", DropSourceType.Container, "chest-1");

        // Act
        var result = _registry.GetLastDrop();

        // Assert
        result.Should().NotBeNull();
        result!.Value.ItemId.Should().Be("item-2");
    }

    /// <summary>
    /// Verifies that GetDroppedItemIds returns a read-only set.
    /// </summary>
    [Test]
    public void GetDroppedItemIds_ReturnsReadOnlySet()
    {
        // Arrange
        _registry.RegisterDrop("item-1", DropSourceType.Boss, "boss-1");
        _registry.RegisterDrop("item-2", DropSourceType.Container, "chest-1");

        // Act
        var droppedIds = _registry.GetDroppedItemIds();

        // Assert
        droppedIds.Should().HaveCount(2);
        droppedIds.Should().Contain("item-1");
        droppedIds.Should().Contain("item-2");
    }

    /// <summary>
    /// Verifies that DropHistory maintains chronological order.
    /// </summary>
    [Test]
    public void DropHistory_MaintainsChronologicalOrder()
    {
        // Arrange
        _registry.RegisterDrop("item-1", DropSourceType.Boss, "boss-1");
        _registry.RegisterDrop("item-2", DropSourceType.Container, "chest-1");
        _registry.RegisterDrop("item-3", DropSourceType.Quest, "quest-1");

        // Act
        var history = _registry.DropHistory;

        // Assert
        history[0].ItemId.Should().Be("item-1");
        history[1].ItemId.Should().Be("item-2");
        history[2].ItemId.Should().Be("item-3");

        // Verify timestamps are in order
        history[0].DroppedAt.Should().BeBefore(history[1].DroppedAt.AddMilliseconds(10));
        history[1].DroppedAt.Should().BeBefore(history[2].DroppedAt.AddMilliseconds(10));
    }
}
