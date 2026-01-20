using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for the LightSource entity.
/// </summary>
[TestFixture]
public class LightSourceTests
{
    #region Factory Method Tests

    [Test]
    public void Create_WithValidParameters_CreatesLightSource()
    {
        // Act
        var source = LightSource.Create("torch", "Torch", LightLevel.Bright);

        // Assert
        source.Should().NotBeNull();
        source.DefinitionId.Should().Be("torch");
        source.Name.Should().Be("Torch");
        source.ProvidedLight.Should().Be(LightLevel.Bright);
        source.IsActive.Should().BeFalse();
    }

    [Test]
    public void CreateTorch_CreatesDurationBasedSource()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var torch = LightSource.CreateTorch(10, itemId, ownerId);

        // Assert
        torch.DefinitionId.Should().Be("torch");
        torch.HasDuration.Should().BeTrue();
        torch.RemainingDuration.Should().Be(10);
        torch.UsesFuel.Should().BeFalse();
        torch.IsPortable.Should().BeTrue();
    }

    [Test]
    public void CreateLantern_CreatesFuelBasedSource()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var lantern = LightSource.CreateLantern(100, itemId, ownerId);

        // Assert
        lantern.DefinitionId.Should().Be("lantern");
        lantern.UsesFuel.Should().BeTrue();
        lantern.CurrentFuel.Should().Be(100);
        lantern.MaxFuel.Should().Be(100);
        lantern.IsPermanent.Should().BeTrue();
    }

    #endregion

    #region Activation Tests

    [Test]
    public void Activate_WhenNotActive_ReturnsTrue()
    {
        // Arrange
        var source = LightSource.Create("test", "Test Light", LightLevel.Dim);

        // Act
        var result = source.Activate();

        // Assert
        result.Should().BeTrue();
        source.IsActive.Should().BeTrue();
    }

    [Test]
    public void Activate_WhenAlreadyActive_ReturnsFalse()
    {
        // Arrange
        var source = LightSource.Create("test", "Test Light", LightLevel.Dim);
        source.Activate();

        // Act
        var result = source.Activate();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Activate_WhenOutOfFuel_ReturnsFalse()
    {
        // Arrange
        var source = LightSource.Create("lantern", "Lantern", LightLevel.Bright,
            usesFuel: true, maxFuel: 0);

        // Act
        var result = source.Activate();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Deactivate_WhenActive_ReturnsTrue()
    {
        // Arrange
        var source = LightSource.Create("test", "Test", LightLevel.Dim);
        source.Activate();

        // Act
        var result = source.Deactivate();

        // Assert
        result.Should().BeTrue();
        source.IsActive.Should().BeFalse();
    }

    #endregion

    #region Tick Tests

    [Test]
    public void Tick_ConsumeDuration_DecreasesRemaining()
    {
        // Arrange
        var torch = LightSource.CreateTorch(5, Guid.NewGuid(), Guid.NewGuid());
        torch.Activate();

        // Act
        var expired = torch.Tick();

        // Assert
        expired.Should().BeFalse();
        torch.RemainingDuration.Should().Be(4);
    }

    [Test]
    public void Tick_WhenDurationReachesZero_ReturnsExpired()
    {
        // Arrange
        var torch = LightSource.CreateTorch(1, Guid.NewGuid(), Guid.NewGuid());
        torch.Activate();

        // Act
        var expired = torch.Tick();

        // Assert
        expired.Should().BeTrue();
        torch.IsActive.Should().BeFalse();
    }

    [Test]
    public void Tick_ConsumesFuel_DecreasesAmount()
    {
        // Arrange
        var lantern = LightSource.CreateLantern(10, Guid.NewGuid(), Guid.NewGuid());
        lantern.Activate();

        // Act
        lantern.Tick();

        // Assert
        lantern.CurrentFuel.Should().Be(9);
    }

    #endregion

    #region Refuel Tests

    [Test]
    public void Refuel_AddsFuelUpToMax()
    {
        // Arrange
        var lantern = LightSource.CreateLantern(100, Guid.NewGuid(), Guid.NewGuid());
        lantern.Activate();
        for (int i = 0; i < 50; i++) lantern.Tick();

        // Act
        var added = lantern.Refuel(100);

        // Assert
        added.Should().Be(50);
        lantern.CurrentFuel.Should().Be(100);
    }

    [Test]
    public void Refuel_OnNonFuelSource_ReturnsZero()
    {
        // Arrange
        var torch = LightSource.CreateTorch(10, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var added = torch.Refuel(50);

        // Assert
        added.Should().Be(0);
    }

    #endregion
}
