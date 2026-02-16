using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="AvatarState"/> value object.
/// </summary>
[TestFixture]
public class AvatarStateTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesCorrectly()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var state = AvatarState.Create(characterId);

        // Assert
        state.CharacterId.Should().Be(characterId);
        state.TurnsRemaining.Should().Be(2);
        state.DamageMultiplier.Should().Be(2.0f);
        state.MovementBonus.Should().Be(2);
        state.ImmuneToCrowdControl.Should().BeTrue();
        state.CorruptionGenerated.Should().Be(2);
        state.WillCauseExhaustion.Should().BeTrue();
        state.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        state.StateId.Should().NotBeEmpty();
        state.IsActive().Should().BeTrue();
    }

    // ===== Damage Multiplier Tests =====

    [Test]
    public void GetDamageMultiplier_WhenActive_ReturnsTwoPointZero()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());

        // Act & Assert
        state.GetDamageMultiplier().Should().Be(2.0f);
    }

    [Test]
    public void GetDamageMultiplier_WhenExpired_ReturnsOnePointZero()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());
        state.Tick(); // 2 → 1
        state.Tick(); // 1 → 0

        // Act & Assert
        state.GetDamageMultiplier().Should().Be(1.0f);
    }

    // ===== CC Immunity Tests =====

    [TestCase(CrowdControlType.Stun)]
    [TestCase(CrowdControlType.Root)]
    [TestCase(CrowdControlType.Fear)]
    [TestCase(CrowdControlType.Charm)]
    [TestCase(CrowdControlType.Paralyze)]
    [TestCase(CrowdControlType.Slow)]
    [TestCase(CrowdControlType.Prone)]
    [TestCase(CrowdControlType.ForcedMovement)]
    [TestCase(CrowdControlType.Sleep)]
    [TestCase(CrowdControlType.Confusion)]
    public void IsImmuneToCC_AllTenTypes_ReturnsTrue(CrowdControlType ccType)
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());

        // Act & Assert
        state.IsImmuneToCC(ccType).Should().BeTrue();
    }

    [Test]
    public void CCImmunityList_ContainsTenTypes()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());

        // Act & Assert
        state.CCImmunityList.Should().HaveCount(10);
    }

    // ===== Tick & Duration Tests =====

    [Test]
    public void Tick_DecrementsRemainingTurns()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());
        state.TurnsRemaining.Should().Be(2);

        // Act
        state.Tick();

        // Assert
        state.TurnsRemaining.Should().Be(1);
        state.IsActive().Should().BeTrue();
    }

    [Test]
    public void Tick_ExpiresAfterTwoTicks()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());

        // Act
        state.Tick();
        state.Tick();

        // Assert
        state.TurnsRemaining.Should().Be(0);
        state.IsActive().Should().BeFalse();
    }

    [Test]
    public void Tick_FloorsAtZero()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());
        state.Tick();
        state.Tick();

        // Act — tick past zero
        state.Tick();

        // Assert
        state.TurnsRemaining.Should().Be(0);
    }

    // ===== End Tests =====

    [Test]
    public void End_SetsRemainingToZero()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());

        // Act
        state.End();

        // Assert
        state.TurnsRemaining.Should().Be(0);
        state.IsActive().Should().BeFalse();
    }

    // ===== IsActive Tests =====

    [Test]
    public void IsActive_WhenCreated_ReturnsTrue()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());

        // Act & Assert
        state.IsActive().Should().BeTrue();
    }

    [Test]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());
        state.End();

        // Act & Assert
        state.IsActive().Should().BeFalse();
    }

    // ===== Constants Tests =====

    [Test]
    public void DefaultDuration_IsTwo()
    {
        AvatarState.DefaultDuration.Should().Be(2);
    }

    [Test]
    public void DefaultDamageMultiplier_IsTwoPointZero()
    {
        AvatarState.DefaultDamageMultiplier.Should().Be(2.0f);
    }

    [Test]
    public void DefaultMovementBonus_IsTwo()
    {
        AvatarState.DefaultMovementBonus.Should().Be(2);
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_WhenActive_ShowsEffects()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());

        // Act
        var description = state.GetDescription();

        // Assert
        description.Should().Contain("Avatar of Destruction");
        description.Should().Contain("2 turns");
        description.Should().Contain("2x damage");
        description.Should().Contain("CC immune");
        description.Should().Contain("+2 movement");
    }

    [Test]
    public void GetDescription_WhenInactive_ShowsInactive()
    {
        // Arrange
        var state = AvatarState.Create(Guid.NewGuid());
        state.End();

        // Act
        var description = state.GetDescription();

        // Assert
        description.Should().Contain("Inactive");
    }
}
