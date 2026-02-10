// ═══════════════════════════════════════════════════════════════════════════════
// SkjaldmaerTier3AbilityServiceTests.cs
// Unit tests for the SkjaldmaerTier3AbilityService.
// Version: 0.20.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class SkjaldmaerTier3AbilityServiceTests
{
    private SkjaldmaerTier3AbilityService _service = null!;
    private Mock<ILogger<SkjaldmaerTier3AbilityService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<SkjaldmaerTier3AbilityService>>();
        _service = new SkjaldmaerTier3AbilityService(_mockLogger.Object);
    }

    // ═══════ Unbreakable ═══════

    [Test]
    public void GetDamageReduction_WithUnbreakable_Returns3()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId>
        {
            SkjaldmaerAbilityId.ShieldWall,
            SkjaldmaerAbilityId.Unbreakable
        };

        // Act
        var reduction = _service.GetDamageReduction(abilities);

        // Assert
        reduction.Should().Be(3);
    }

    [Test]
    public void GetDamageReduction_WithoutUnbreakable_Returns0()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId>
        {
            SkjaldmaerAbilityId.ShieldWall,
            SkjaldmaerAbilityId.Intercept
        };

        // Act
        var reduction = _service.GetDamageReduction(abilities);

        // Assert
        reduction.Should().Be(0);
    }

    [Test]
    public void GetDamageReduction_WithEmptyList_Returns0()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId>();

        // Act
        var reduction = _service.GetDamageReduction(abilities);

        // Assert
        reduction.Should().Be(0);
    }

    [Test]
    public void GetDamageReduction_WithNullList_Throws()
    {
        // Arrange & Act
        var act = () => _service.GetDamageReduction(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════ The Wall Lives ═══════

    [Test]
    public void ActivateTheWallLives_CreatesActiveState()
    {
        // Arrange & Act
        var state = _service.ActivateTheWallLives();

        // Assert
        state.IsActive.Should().BeTrue();
        state.TurnsRemaining.Should().Be(3);
        state.ActivatedAt.Should().NotBeNull();
    }

    [Test]
    public void TickTheWallLives_DecrementsAndExpires()
    {
        // Arrange
        var state = _service.ActivateTheWallLives();

        // Act — tick through full lifecycle
        var tick1 = _service.TickTheWallLives(state);
        var tick2 = _service.TickTheWallLives(tick1);
        var tick3 = _service.TickTheWallLives(tick2);

        // Assert
        tick1.TurnsRemaining.Should().Be(2);
        tick1.IsActive.Should().BeTrue();

        tick2.TurnsRemaining.Should().Be(1);
        tick2.IsActive.Should().BeTrue();

        tick3.TurnsRemaining.Should().Be(0);
        tick3.IsActive.Should().BeFalse();
    }

    [Test]
    public void TickTheWallLives_WhenInactive_ReturnsUnchanged()
    {
        // Arrange
        var state = TheWallLivesState.Deactivate();

        // Act
        var ticked = _service.TickTheWallLives(state);

        // Assert
        ticked.Should().Be(state);
    }

    [Test]
    public void TickTheWallLives_WithNull_Throws()
    {
        // Arrange & Act
        var act = () => _service.TickTheWallLives(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════ Capstone Usage ═══════

    [Test]
    public void CanUseCapstone_NotUsed_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUseCapstone(hasUsedCapstoneThisCombat: false).Should().BeTrue();
    }

    [Test]
    public void CanUseCapstone_AlreadyUsed_ReturnsFalse()
    {
        // Act & Assert
        _service.CanUseCapstone(hasUsedCapstoneThisCombat: true).Should().BeFalse();
    }

    // ═══════ PP Threshold Checks ═══════

    [Test]
    public void CanUnlockTier3_With16PP_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUnlockTier3(ppInvested: 16).Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier3_WithMoreThan16PP_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUnlockTier3(ppInvested: 20).Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier3_WithLessThan16PP_ReturnsFalse()
    {
        // Act & Assert
        _service.CanUnlockTier3(ppInvested: 15).Should().BeFalse();
    }

    [Test]
    public void CanUnlockCapstone_With24PP_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUnlockCapstone(ppInvested: 24).Should().BeTrue();
    }

    [Test]
    public void CanUnlockCapstone_WithMoreThan24PP_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUnlockCapstone(ppInvested: 30).Should().BeTrue();
    }

    [Test]
    public void CanUnlockCapstone_WithLessThan24PP_ReturnsFalse()
    {
        // Act & Assert
        _service.CanUnlockCapstone(ppInvested: 23).Should().BeFalse();
    }
}
