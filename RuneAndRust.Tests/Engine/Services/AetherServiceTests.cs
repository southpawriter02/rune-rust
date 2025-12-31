using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine.Services;

/// <summary>
/// Unit tests for the AetherService (v0.4.3a - The Aether).
/// Tests flux accumulation, dissipation, threshold transitions, and event publication.
/// </summary>
public class AetherServiceTests
{
    private readonly IEventBus _mockEventBus;
    private readonly ILogger<AetherService> _mockLogger;
    private readonly AetherService _sut;

    public AetherServiceTests()
    {
        _mockEventBus = Substitute.For<IEventBus>();
        _mockLogger = Substitute.For<ILogger<AetherService>>();
        _sut = new AetherService(_mockEventBus, _mockLogger);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // AddFlux Tests (8 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void AddFlux_WithPositiveAmount_IncreasesCurrentFlux()
    {
        // Act
        var result = _sut.AddFlux(10);

        // Assert
        result.Should().Be(10);
        _sut.CurrentFlux.Should().Be(10);
    }

    [Fact]
    public void AddFlux_WhenExceedingMax_ClampsToMaxFlux()
    {
        // Arrange
        _sut.AddFlux(90);

        // Act
        var result = _sut.AddFlux(20);

        // Assert
        result.Should().Be(100);
        _sut.CurrentFlux.Should().Be(100);
    }

    [Fact]
    public void AddFlux_WithZeroAmount_DoesNotChangeFlux()
    {
        // Arrange
        _sut.AddFlux(10);

        // Act
        var result = _sut.AddFlux(0);

        // Assert
        result.Should().Be(10);
        _sut.CurrentFlux.Should().Be(10);
    }

    [Fact]
    public void AddFlux_WithNegativeAmount_TreatsAsZero()
    {
        // Arrange
        _sut.AddFlux(10);

        // Act
        var result = _sut.AddFlux(-5);

        // Assert
        result.Should().Be(10);
        _sut.CurrentFlux.Should().Be(10);
    }

    [Fact]
    public void AddFlux_CrossingThreshold_PublishesEvent()
    {
        // Act
        _sut.AddFlux(25); // Safe (0) -> Elevated (25)

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.PreviousThreshold == FluxThreshold.Safe &&
            e.CurrentThreshold == FluxThreshold.Elevated));
    }

    [Fact]
    public void AddFlux_WithSignificantChange_PublishesEvent()
    {
        // Act
        _sut.AddFlux(15); // 15 points = significant change

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.Delta >= 15));
    }

    [Fact]
    public void AddFlux_HittingMaxBoundary_PublishesEvent()
    {
        // Arrange
        _sut.AddFlux(90);
        _mockEventBus.ClearReceivedCalls();

        // Act
        _sut.AddFlux(10); // Hits max boundary

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.HitBoundary == true));
    }

    [Fact]
    public void AddFlux_SmallChangeWithinThreshold_DoesNotPublishEvent()
    {
        // Arrange
        _sut.AddFlux(10);
        _mockEventBus.ClearReceivedCalls();

        // Act
        _sut.AddFlux(5); // Small change, stays in Safe threshold

        // Assert
        _mockEventBus.DidNotReceive().Publish(Arg.Any<FluxChangedEvent>());
    }

    // ═══════════════════════════════════════════════════════════════════════
    // RemoveFlux Tests (6 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void RemoveFlux_WithPositiveAmount_DecreasesCurrentFlux()
    {
        // Arrange
        _sut.AddFlux(50);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.RemoveFlux(10);

        // Assert
        result.Should().Be(40);
        _sut.CurrentFlux.Should().Be(40);
    }

    [Fact]
    public void RemoveFlux_WhenGoingBelowZero_ClampsToZero()
    {
        // Arrange
        _sut.AddFlux(10);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.RemoveFlux(20);

        // Assert
        result.Should().Be(0);
        _sut.CurrentFlux.Should().Be(0);
    }

    [Fact]
    public void RemoveFlux_WithZeroAmount_DoesNotChangeFlux()
    {
        // Arrange
        _sut.AddFlux(50);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.RemoveFlux(0);

        // Assert
        result.Should().Be(50);
    }

    [Fact]
    public void RemoveFlux_WithNegativeAmount_TreatsAsZero()
    {
        // Arrange
        _sut.AddFlux(50);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.RemoveFlux(-10);

        // Assert
        result.Should().Be(50);
    }

    [Fact]
    public void RemoveFlux_CrossingThreshold_PublishesEvent()
    {
        // Arrange
        _sut.AddFlux(30); // Elevated
        _mockEventBus.ClearReceivedCalls();

        // Act
        _sut.RemoveFlux(10); // Elevated (30) -> Safe (20)

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.PreviousThreshold == FluxThreshold.Elevated &&
            e.CurrentThreshold == FluxThreshold.Safe));
    }

    [Fact]
    public void RemoveFlux_HittingZeroBoundary_PublishesEvent()
    {
        // Arrange
        _sut.AddFlux(10);
        _mockEventBus.ClearReceivedCalls();

        // Act
        _sut.RemoveFlux(10); // Hits zero boundary

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.HitBoundary == true &&
            e.CurrentFlux == 0));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DissipateFlux Tests (5 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DissipateFlux_WithoutAmount_UsesDefaultRate()
    {
        // Arrange
        _sut.AddFlux(50);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.DissipateFlux();

        // Assert
        result.Should().Be(45); // 50 - 5 (default rate)
    }

    [Fact]
    public void DissipateFlux_WithCustomAmount_UsesCustomRate()
    {
        // Arrange
        _sut.AddFlux(50);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.DissipateFlux(10);

        // Assert
        result.Should().Be(40);
    }

    [Fact]
    public void DissipateFlux_AtZero_ReturnsZero()
    {
        // Act
        var result = _sut.DissipateFlux();

        // Assert
        result.Should().Be(0);
        _sut.CurrentFlux.Should().Be(0);
    }

    [Fact]
    public void DissipateFlux_WhenExceedingCurrent_ClampsToZero()
    {
        // Arrange
        _sut.AddFlux(3);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.DissipateFlux(10);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void DissipateFlux_WithNegativeAmount_TreatsAsZero()
    {
        // Arrange
        _sut.AddFlux(50);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.DissipateFlux(-5);

        // Assert
        result.Should().Be(50);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ResetFlux Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void ResetFlux_ResetsToZero()
    {
        // Arrange
        _sut.AddFlux(75);

        // Act
        _sut.ResetFlux();

        // Assert
        _sut.CurrentFlux.Should().Be(0);
    }

    [Fact]
    public void ResetFlux_WhenAlreadyZero_DoesNotPublishEvent()
    {
        // Act
        _sut.ResetFlux();

        // Assert
        _mockEventBus.DidNotReceive().Publish(Arg.Any<FluxChangedEvent>());
    }

    [Fact]
    public void ResetFlux_WhenAboveZero_PublishesEvent()
    {
        // Arrange
        _sut.AddFlux(50);
        _mockEventBus.ClearReceivedCalls();

        // Act
        _sut.ResetFlux();

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.ChangeReason == "Reset" &&
            e.CurrentFlux == 0));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GetFluxState Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void GetFluxState_ReturnsCurrentState()
    {
        // Arrange
        _sut.AddFlux(50);

        // Act
        var state = _sut.GetFluxState();

        // Assert
        state.CurrentFlux.Should().Be(50);
        state.MaxFlux.Should().Be(100);
        state.Threshold.Should().Be(FluxThreshold.Critical);
    }

    [Fact]
    public void GetFluxState_ReturnsClone()
    {
        // Arrange
        _sut.AddFlux(50);

        // Act
        var state1 = _sut.GetFluxState();
        var state2 = _sut.GetFluxState();

        // Assert
        state1.Should().NotBeSameAs(state2);
    }

    [Fact]
    public void GetFluxState_ModifyingCloneDoesNotAffectService()
    {
        // Arrange
        _sut.AddFlux(50);

        // Act
        var state = _sut.GetFluxState();
        state.CurrentFlux = 99;

        // Assert
        _sut.CurrentFlux.Should().Be(50);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GetThreshold Tests (5 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(0, FluxThreshold.Safe)]
    [InlineData(24, FluxThreshold.Safe)]
    public void GetThreshold_InSafeRange_ReturnsSafe(int flux, FluxThreshold expected)
    {
        // Arrange
        _sut.AddFlux(flux);

        // Act
        var threshold = _sut.GetThreshold();

        // Assert
        threshold.Should().Be(expected);
    }

    [Theory]
    [InlineData(25, FluxThreshold.Elevated)]
    [InlineData(49, FluxThreshold.Elevated)]
    public void GetThreshold_InElevatedRange_ReturnsElevated(int flux, FluxThreshold expected)
    {
        // Arrange
        _sut.AddFlux(flux);

        // Act
        var threshold = _sut.GetThreshold();

        // Assert
        threshold.Should().Be(expected);
    }

    [Theory]
    [InlineData(50, FluxThreshold.Critical)]
    [InlineData(74, FluxThreshold.Critical)]
    public void GetThreshold_InCriticalRange_ReturnsCritical(int flux, FluxThreshold expected)
    {
        // Arrange
        _sut.AddFlux(flux);

        // Act
        var threshold = _sut.GetThreshold();

        // Assert
        threshold.Should().Be(expected);
    }

    [Theory]
    [InlineData(75, FluxThreshold.Overload)]
    [InlineData(100, FluxThreshold.Overload)]
    public void GetThreshold_InOverloadRange_ReturnsOverload(int flux, FluxThreshold expected)
    {
        // Arrange
        _sut.AddFlux(flux);

        // Act
        var threshold = _sut.GetThreshold();

        // Assert
        threshold.Should().Be(expected);
    }

    [Fact]
    public void GetThreshold_AtBoundaries_ReturnsCorrectThreshold()
    {
        // Test each boundary transition point
        _sut.AddFlux(24);
        _sut.GetThreshold().Should().Be(FluxThreshold.Safe);

        _sut.AddFlux(1); // 25
        _sut.GetThreshold().Should().Be(FluxThreshold.Elevated);

        _sut.AddFlux(24); // 49
        _sut.GetThreshold().Should().Be(FluxThreshold.Elevated);

        _sut.AddFlux(1); // 50
        _sut.GetThreshold().Should().Be(FluxThreshold.Critical);

        _sut.AddFlux(24); // 74
        _sut.GetThreshold().Should().Be(FluxThreshold.Critical);

        _sut.AddFlux(1); // 75
        _sut.GetThreshold().Should().Be(FluxThreshold.Overload);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Event Publication Tests (6 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void FluxChangedEvent_ContainsCorrectPreviousAndCurrentFlux()
    {
        // Act
        _sut.AddFlux(30);

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.PreviousFlux == 0 &&
            e.CurrentFlux == 30));
    }

    [Fact]
    public void FluxChangedEvent_ContainsCorrectDelta()
    {
        // Act
        _sut.AddFlux(20);

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.Delta == 20));
    }

    [Fact]
    public void FluxChangedEvent_IdentifiesThresholdCrossing()
    {
        // Act
        _sut.AddFlux(50); // Safe -> Critical

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.CrossedThreshold == true));
    }

    [Fact]
    public void FluxChangedEvent_IdentifiesBoundaryHit()
    {
        // Arrange
        _sut.AddFlux(90);
        _mockEventBus.ClearReceivedCalls();

        // Act
        _sut.AddFlux(10); // Hits 100 (max)

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.HitBoundary == true));
    }

    [Fact]
    public void FluxChangedEvent_ContainsChangeReason()
    {
        // Act
        _sut.AddFlux(25);

        // Assert
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.ChangeReason == "AddFlux"));
    }

    [Fact]
    public void FluxChangedEvent_IsIncreaseIsDecrease_AreCorrect()
    {
        // Test increase
        _sut.AddFlux(50);
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.IsIncrease == true && e.IsDecrease == false));

        _mockEventBus.ClearReceivedCalls();

        // Test decrease
        _sut.RemoveFlux(30);
        _mockEventBus.Received(1).Publish(Arg.Is<FluxChangedEvent>(e =>
            e.IsIncrease == false && e.IsDecrease == true));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Edge Case Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void AddFlux_AtMaxFlux_RemainsAtMax()
    {
        // Arrange
        _sut.AddFlux(100);
        _mockEventBus.ClearReceivedCalls();

        // Act
        var result = _sut.AddFlux(10);

        // Assert
        result.Should().Be(100);
        _sut.CurrentFlux.Should().Be(100);
        _mockEventBus.DidNotReceive().Publish(Arg.Any<FluxChangedEvent>());
    }

    [Fact]
    public void RemoveFlux_AtZeroFlux_RemainsAtZero()
    {
        // Act
        var result = _sut.RemoveFlux(10);

        // Assert
        result.Should().Be(0);
        _sut.CurrentFlux.Should().Be(0);
    }

    [Fact]
    public void Properties_CurrentFluxAndMaxFlux_AreAccessible()
    {
        // Assert initial state
        _sut.CurrentFlux.Should().Be(0);
        _sut.MaxFlux.Should().Be(100);

        // Arrange
        _sut.AddFlux(42);

        // Assert updated state
        _sut.CurrentFlux.Should().Be(42);
        _sut.MaxFlux.Should().Be(100);
    }
}
