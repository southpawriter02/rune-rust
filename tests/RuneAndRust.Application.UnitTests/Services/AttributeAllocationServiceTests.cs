// ═══════════════════════════════════════════════════════════════════════════════
// AttributeAllocationServiceTests.cs
// Unit tests for AttributeAllocationService (v0.17.2f).
// Tests cover state creation, attribute modification, validation, mode switching,
// cost calculation, and allocation reset for both Simple and Advanced modes.
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="AttributeAllocationService"/> (v0.17.2f).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Creating allocation state in Simple and Advanced modes</description></item>
///   <item><description>Modifying attributes with sufficient and insufficient points</description></item>
///   <item><description>Blocking modifications in Simple mode</description></item>
///   <item><description>Rejecting out-of-range attribute values</description></item>
///   <item><description>Switching between Simple and Advanced modes</description></item>
///   <item><description>Validating allocation states</description></item>
///   <item><description>Resetting allocations to initial defaults</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class AttributeAllocationServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Mock for the attribute provider dependency.</summary>
    private Mock<IAttributeProvider> _providerMock = null!;

    /// <summary>Mock for the logger dependency.</summary>
    private Mock<ILogger<AttributeAllocationService>> _loggerMock = null!;

    /// <summary>The service under test.</summary>
    private AttributeAllocationService _service = null!;

    /// <summary>
    /// Sets up test dependencies and default mock behaviors before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _providerMock = new Mock<IAttributeProvider>();
        _loggerMock = new Mock<ILogger<AttributeAllocationService>>();

        SetupDefaultMocks();

        _service = new AttributeAllocationService(
            _providerMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Configures default mock behaviors for all provider methods.
    /// </summary>
    /// <remarks>
    /// Sets up:
    /// <list type="bullet">
    ///   <item><description>Warrior recommended build: M4/F3/Wi2/Wl2/S4 (15 pts)</description></item>
    ///   <item><description>Mystic recommended build: M2/F3/Wi4/Wl4/S2 (15 pts)</description></item>
    ///   <item><description>Default PointBuyConfiguration (15 pts, range 1-10)</description></item>
    ///   <item><description>Starting points: 15 for most, 14 for Adept</description></item>
    /// </list>
    /// </remarks>
    private void SetupDefaultMocks()
    {
        // Warrior recommended build (Simple mode)
        var warriorState = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);
        _providerMock
            .Setup(p => p.GetRecommendedBuild("warrior"))
            .Returns(warriorState);

        // Mystic recommended build (Simple mode)
        var mysticState = AttributeAllocationState.CreateFromRecommendedBuild(
            "mystic", might: 2, finesse: 3, wits: 4, will: 4, sturdiness: 2, totalPoints: 15);
        _providerMock
            .Setup(p => p.GetRecommendedBuild("mystic"))
            .Returns(mysticState);

        // Point-buy configuration with standard game balance values
        var config = PointBuyConfiguration.CreateDefault();
        _providerMock
            .Setup(p => p.GetPointBuyConfiguration())
            .Returns(config);

        // Starting points: 15 default, 14 for Adept
        _providerMock
            .Setup(p => p.GetStartingPoints(It.IsAny<string?>()))
            .Returns(15);
        _providerMock
            .Setup(p => p.GetStartingPoints("adept"))
            .Returns(14);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateAllocationState TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Simple mode returns the archetype's recommended build
    /// with correct attribute values and mode settings.
    /// </summary>
    [Test]
    public void CreateAllocationState_SimpleMode_ReturnsRecommendedBuild()
    {
        // Act
        var state = _service.CreateAllocationState(
            AttributeAllocationMode.Simple, "warrior");

        // Assert
        state.Mode.Should().Be(AttributeAllocationMode.Simple);
        state.CurrentMight.Should().Be(4);
        state.CurrentFinesse.Should().Be(3);
        state.CurrentWits.Should().Be(2);
        state.CurrentWill.Should().Be(2);
        state.CurrentSturdiness.Should().Be(4);
        state.IsComplete.Should().BeTrue();
        state.SelectedArchetypeId.Should().Be("warrior");
    }

    /// <summary>
    /// Verifies that Advanced mode returns a default state with all attributes
    /// at 1 and the full point pool available.
    /// </summary>
    [Test]
    public void CreateAllocationState_AdvancedMode_ReturnsDefaultState()
    {
        // Act
        var state = _service.CreateAllocationState(AttributeAllocationMode.Advanced);

        // Assert
        state.Mode.Should().Be(AttributeAllocationMode.Advanced);
        state.CurrentMight.Should().Be(1);
        state.CurrentFinesse.Should().Be(1);
        state.CurrentWits.Should().Be(1);
        state.CurrentWill.Should().Be(1);
        state.CurrentSturdiness.Should().Be(1);
        state.PointsRemaining.Should().Be(15);
        state.PointsSpent.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TryModifyAttribute TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that modifying an attribute succeeds when enough points are
    /// available, returning the correct new state with updated points.
    /// </summary>
    [Test]
    public void TryModifyAttribute_WithEnoughPoints_Succeeds()
    {
        // Arrange — Advanced mode with 15 points available
        var state = _service.CreateAllocationState(AttributeAllocationMode.Advanced);

        // Act — Increase Might from 1 to 5 (costs 4 points)
        var result = _service.TryModifyAttribute(state, CoreAttribute.Might, 5);

        // Assert
        result.Success.Should().BeTrue();
        result.NewState.Should().NotBeNull();
        result.NewState!.Value.CurrentMight.Should().Be(5);
        result.PointsSpent.Should().Be(4);
        result.PointsRemaining.Should().Be(11);
        result.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Verifies that modifying an attribute fails when insufficient points
    /// are available, returning an appropriate error message.
    /// </summary>
    [Test]
    public void TryModifyAttribute_InsufficientPoints_Fails()
    {
        // Arrange — Start with Advanced mode and spend most points first
        var state = _service.CreateAllocationState(AttributeAllocationMode.Advanced);

        // Spend 7 points on Might (1 → 8), leaving 8 remaining
        var firstResult = _service.TryModifyAttribute(state, CoreAttribute.Might, 8);
        firstResult.Success.Should().BeTrue("setup modification should succeed");
        var partialState = firstResult.NewState!.Value;

        // Act — Try to set Finesse to 10 (costs 11 points, only 8 remaining)
        var result = _service.TryModifyAttribute(partialState, CoreAttribute.Finesse, 10);

        // Assert
        result.Success.Should().BeFalse();
        result.NewState.Should().BeNull();
        result.ErrorMessage.Should().Contain("Insufficient points");
        result.PointsSpent.Should().Be(partialState.PointsSpent);
        result.PointsRemaining.Should().Be(partialState.PointsRemaining);
    }

    /// <summary>
    /// Verifies that modifying attributes in Simple mode is rejected with
    /// an appropriate error message.
    /// </summary>
    [Test]
    public void TryModifyAttribute_InSimpleMode_Fails()
    {
        // Arrange — Simple mode with Warrior build
        var state = _service.CreateAllocationState(
            AttributeAllocationMode.Simple, "warrior");

        // Act
        var result = _service.TryModifyAttribute(state, CoreAttribute.Might, 5);

        // Assert
        result.Success.Should().BeFalse();
        result.NewState.Should().BeNull();
        result.ErrorMessage.Should().Contain("Simple mode");
    }

    /// <summary>
    /// Verifies that setting an attribute value outside the valid range [1, 10]
    /// is rejected with an appropriate error message.
    /// </summary>
    [Test]
    public void TryModifyAttribute_ValueOutOfRange_Fails()
    {
        // Arrange — Advanced mode
        var state = _service.CreateAllocationState(AttributeAllocationMode.Advanced);

        // Act — Try to set value to 11 (max is 10)
        var result = _service.TryModifyAttribute(state, CoreAttribute.Might, 11);

        // Assert
        result.Success.Should().BeFalse();
        result.NewState.Should().BeNull();
        result.ErrorMessage.Should().Contain("between");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SwitchMode TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that switching from Simple to Advanced mode preserves the
    /// current attribute values while enabling manual adjustment.
    /// </summary>
    [Test]
    public void SwitchMode_SimpleToAdvanced_PreservesValues()
    {
        // Arrange — Simple mode with Warrior build (M4/F3/Wi2/Wl2/S4)
        var simpleState = _service.CreateAllocationState(
            AttributeAllocationMode.Simple, "warrior");

        // Act — Switch to Advanced mode
        var advancedState = _service.SwitchMode(
            simpleState, AttributeAllocationMode.Advanced);

        // Assert — Attribute values preserved, mode changed
        advancedState.Mode.Should().Be(AttributeAllocationMode.Advanced);
        advancedState.CurrentMight.Should().Be(4);
        advancedState.CurrentFinesse.Should().Be(3);
        advancedState.CurrentWits.Should().Be(2);
        advancedState.CurrentWill.Should().Be(2);
        advancedState.CurrentSturdiness.Should().Be(4);
        advancedState.AllowsManualAdjustment.Should().BeTrue();
        advancedState.SelectedArchetypeId.Should().BeNull();
    }

    /// <summary>
    /// Verifies that switching from Advanced to Simple mode applies the
    /// archetype's recommended build, replacing current values.
    /// </summary>
    [Test]
    public void SwitchMode_AdvancedToSimple_AppliesRecommendedBuild()
    {
        // Arrange — Start in Advanced mode
        var advancedState = _service.CreateAllocationState(AttributeAllocationMode.Advanced);

        // Act — Switch to Simple mode with Mystic archetype
        var simpleState = _service.SwitchMode(
            advancedState, AttributeAllocationMode.Simple, "mystic");

        // Assert — Mystic recommended build applied (M2/F3/Wi4/Wl4/S2)
        simpleState.Mode.Should().Be(AttributeAllocationMode.Simple);
        simpleState.CurrentMight.Should().Be(2);
        simpleState.CurrentFinesse.Should().Be(3);
        simpleState.CurrentWits.Should().Be(4);
        simpleState.CurrentWill.Should().Be(4);
        simpleState.CurrentSturdiness.Should().Be(2);
        simpleState.SelectedArchetypeId.Should().Be("mystic");
        simpleState.IsComplete.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ValidateAllocation TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a valid Simple mode state passes validation.
    /// </summary>
    [Test]
    public void ValidateAllocation_ValidState_ReturnsValid()
    {
        // Arrange — Simple mode with Warrior build (all constraints met)
        var state = _service.CreateAllocationState(
            AttributeAllocationMode.Simple, "warrior");

        // Act
        var result = _service.ValidateAllocation(state);

        // Assert
        result.IsValid.Should().BeTrue();
        result.HasErrors.Should().BeFalse();
        result.ErrorCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ResetAllocation TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that resetting to Advanced mode returns the initial default state
    /// with all attributes at 1 and the full point pool available.
    /// </summary>
    [Test]
    public void ResetAllocation_ReturnsInitialState()
    {
        // Act
        var state = _service.ResetAllocation(AttributeAllocationMode.Advanced);

        // Assert
        state.Mode.Should().Be(AttributeAllocationMode.Advanced);
        state.CurrentMight.Should().Be(1);
        state.CurrentFinesse.Should().Be(1);
        state.CurrentWits.Should().Be(1);
        state.CurrentWill.Should().Be(1);
        state.CurrentSturdiness.Should().Be(1);
        state.PointsRemaining.Should().Be(15);
        state.PointsSpent.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CalculateCost TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CalculateCost delegates correctly to the provider's
    /// point-buy configuration for standard and premium tier values.
    /// </summary>
    [Test]
    public void CalculateCost_DelegatesToProvider_ReturnsCorrectCost()
    {
        // Act & Assert — Standard tier (1 point each)
        _service.CalculateCost(1, 5).Should().Be(4, "cost from 1 to 5 is 4 standard points");
        _service.CalculateCost(1, 8).Should().Be(7, "cost from 1 to 8 is 7 standard points");

        // Act & Assert — Premium tier (2 points each)
        _service.CalculateCost(8, 10).Should().Be(4, "cost from 8 to 10 is 4 premium points");

        // Act & Assert — Full range
        _service.CalculateCost(1, 10).Should().Be(11, "cost from 1 to 10 is 11 total points");

        // Act & Assert — Refund (negative cost)
        _service.CalculateCost(5, 1).Should().Be(-4, "refund from 5 to 1 is -4 points");

        // Act & Assert — No change
        _service.CalculateCost(5, 5).Should().Be(0, "no change means zero cost");
    }
}
