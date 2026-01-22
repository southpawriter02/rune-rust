using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="RngContextService"/>.
/// </summary>
/// <remarks>
/// Tests cover context stack management, seed locking/unlocking,
/// and context transitions.
/// </remarks>
[TestFixture]
public class RngContextServiceTests
{
    private Mock<IRandomProvider> _mockProvider = null!;
    private Mock<ILogger<RngContextService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockProvider = new Mock<IRandomProvider>();
        _mockLogger = new Mock<ILogger<RngContextService>>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTEXT STACK TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetCurrentContext_WithNoContext_ReturnsDefault()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);

        // Act
        var context = service.GetCurrentContext();

        // Assert
        context.Should().Be(RngContext.Default);
    }

    [Test]
    public void EnterContext_PushesContextOntoStack()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);

        // Act
        service.EnterContext(RngContext.Combat);

        // Assert
        service.GetCurrentContext().Should().Be(RngContext.Combat);
    }

    [Test]
    public void EnterContext_SupportsNestedContexts()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);

        // Act
        service.EnterContext(RngContext.Exploration);
        service.EnterContext(RngContext.Combat);
        service.EnterContext(RngContext.Dialogue);

        // Assert
        service.GetCurrentContext().Should().Be(RngContext.Dialogue);
    }

    [Test]
    public void ExitContext_PopsContextFromStack()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);
        service.EnterContext(RngContext.Exploration);
        service.EnterContext(RngContext.Combat);

        // Act
        service.ExitContext();

        // Assert
        service.GetCurrentContext().Should().Be(RngContext.Exploration);
    }

    [Test]
    public void ExitContext_WithEmptyStack_DoesNotThrow()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);

        // Act
        var act = () => service.ExitContext();

        // Assert
        act.Should().NotThrow();
        service.GetCurrentContext().Should().Be(RngContext.Default);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOCKED SEED TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void EnterContext_WithLockedSeed_AppliesSeedToProvider()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);
        service.LockSeedForContext(RngContext.Combat, 12345);

        // Act
        service.EnterContext(RngContext.Combat);

        // Assert
        _mockProvider.Verify(p => p.SetSeed(12345), Times.Once);
    }

    [Test]
    public void EnterContext_WithExploration_GeneratesFreshSeed()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);

        // Act
        service.EnterContext(RngContext.Exploration);

        // Assert - SetSeed should be called with some value
        _mockProvider.Verify(p => p.SetSeed(It.IsAny<int>()), Times.Once);
    }

    [Test]
    public void EnterContext_CombatWithNoLock_KeepsCurrentState()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);

        // Act
        service.EnterContext(RngContext.Combat);

        // Assert - SetSeed should NOT be called for unlocked Combat
        _mockProvider.Verify(p => p.SetSeed(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ExitContext_RestoresPreviousContextSeed()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);
        service.LockSeedForContext(RngContext.Exploration, 111);
        service.LockSeedForContext(RngContext.Combat, 222);

        service.EnterContext(RngContext.Exploration);
        service.EnterContext(RngContext.Combat);

        // Reset mock to track only exit behavior
        _mockProvider.Invocations.Clear();

        // Act
        service.ExitContext();

        // Assert - Should restore exploration seed
        _mockProvider.Verify(p => p.SetSeed(111), Times.Once);
        service.GetCurrentContext().Should().Be(RngContext.Exploration);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SEED LOCK MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsContextSeedLocked_AfterLock_ReturnsTrue()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);

        // Act
        service.LockSeedForContext(RngContext.Combat, 42);

        // Assert
        service.IsContextSeedLocked(RngContext.Combat).Should().BeTrue();
        service.IsContextSeedLocked(RngContext.Exploration).Should().BeFalse();
    }

    [Test]
    public void GetLockedSeed_AfterLock_ReturnsCorrectSeed()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);

        // Act
        service.LockSeedForContext(RngContext.Combat, 42);

        // Assert
        service.GetLockedSeed(RngContext.Combat).Should().Be(42);
        service.GetLockedSeed(RngContext.Exploration).Should().BeNull();
    }

    [Test]
    public void ReleaseSeedLock_RemovesLock()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);
        service.LockSeedForContext(RngContext.Combat, 42);

        // Act
        service.ReleaseSeedLock(RngContext.Combat);

        // Assert
        service.IsContextSeedLocked(RngContext.Combat).Should().BeFalse();
        service.GetLockedSeed(RngContext.Combat).Should().BeNull();
    }

    [Test]
    public void LockSeedForContext_WhenContextActive_AppliesSeedImmediately()
    {
        // Arrange
        var service = new RngContextService(_mockProvider.Object, _mockLogger.Object);
        service.EnterContext(RngContext.Combat);
        _mockProvider.Invocations.Clear();

        // Act
        service.LockSeedForContext(RngContext.Combat, 999);

        // Assert
        _mockProvider.Verify(p => p.SetSeed(999), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RngContextService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("randomProvider");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RngContextService(_mockProvider.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
