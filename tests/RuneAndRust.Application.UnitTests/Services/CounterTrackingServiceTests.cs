using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="CounterTrackingService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Technique bonus calculation (additive stacking)</description></item>
///   <item><description>Time multiplier calculation (multiplicative compounding)</description></item>
///   <item><description>Environmental requirement validation</description></item>
///   <item><description>Concealment DC clamping (10-30)</description></item>
///   <item><description>TrackingState integration</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class CounterTrackingServiceTests
{
    private SkillCheckService _skillCheckService = null!;
    private ILogger<CounterTrackingService> _logger = null!;
    private CounterTrackingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _skillCheckService = CreateMockSkillCheckService();
        _logger = Substitute.For<ILogger<CounterTrackingService>>();
        _sut = new CounterTrackingService(_skillCheckService, _logger);
    }

    #region Technique Bonus Tests

    [Test]
    public void CalculateTotalBonus_WithNoTechniques_ReturnsZero()
    {
        // Arrange
        var techniques = Array.Empty<ConcealmentTechnique>();

        // Act
        var bonus = _sut.CalculateTotalBonus(techniques);

        // Assert
        bonus.Should().Be(0);
    }

    [Test]
    public void CalculateTotalBonus_WithSingleTechnique_ReturnsCorrectBonus()
    {
        // Arrange & Act & Assert
        _sut.CalculateTotalBonus(new[] { ConcealmentTechnique.HardSurfaces }).Should().Be(2);
        _sut.CalculateTotalBonus(new[] { ConcealmentTechnique.BrushTracks }).Should().Be(4);
        _sut.CalculateTotalBonus(new[] { ConcealmentTechnique.FalseTrail }).Should().Be(6);
        _sut.CalculateTotalBonus(new[] { ConcealmentTechnique.WaterCrossing }).Should().Be(8);
        _sut.CalculateTotalBonus(new[] { ConcealmentTechnique.Backtracking }).Should().Be(4);
    }

    [Test]
    public void CalculateTotalBonus_WithMultipleTechniques_StacksAdditively()
    {
        // Arrange - BrushTracks (+4) + Backtracking (+4) = +8
        var techniques = new[]
        {
            ConcealmentTechnique.BrushTracks,
            ConcealmentTechnique.Backtracking
        };

        // Act
        var bonus = _sut.CalculateTotalBonus(techniques);

        // Assert
        bonus.Should().Be(8);
    }

    [Test]
    public void CalculateTotalBonus_WithAllTechniques_ReturnsCorrectSum()
    {
        // Arrange - All techniques: +2 +4 +6 +8 +4 = +24
        var techniques = new[]
        {
            ConcealmentTechnique.HardSurfaces,
            ConcealmentTechnique.BrushTracks,
            ConcealmentTechnique.FalseTrail,
            ConcealmentTechnique.WaterCrossing,
            ConcealmentTechnique.Backtracking
        };

        // Act
        var bonus = _sut.CalculateTotalBonus(techniques);

        // Assert
        bonus.Should().Be(24);
    }

    [Test]
    public void GetTechniqueBonus_ReturnsCorrectBonusForEachTechnique()
    {
        // Assert
        _sut.GetTechniqueBonus(ConcealmentTechnique.HardSurfaces).Should().Be(2);
        _sut.GetTechniqueBonus(ConcealmentTechnique.BrushTracks).Should().Be(4);
        _sut.GetTechniqueBonus(ConcealmentTechnique.FalseTrail).Should().Be(6);
        _sut.GetTechniqueBonus(ConcealmentTechnique.WaterCrossing).Should().Be(8);
        _sut.GetTechniqueBonus(ConcealmentTechnique.Backtracking).Should().Be(4);
    }

    #endregion

    #region Time Multiplier Tests

    [Test]
    public void CalculateTimeMultiplier_WithNoTechniques_ReturnsOne()
    {
        // Arrange
        var techniques = Array.Empty<ConcealmentTechnique>();

        // Act
        var multiplier = _sut.CalculateTimeMultiplier(techniques);

        // Assert
        multiplier.Should().Be(1.0m);
    }

    [Test]
    public void CalculateTimeMultiplier_WithSingleTechnique_ReturnsCorrectMultiplier()
    {
        // Arrange & Act & Assert
        _sut.CalculateTimeMultiplier(new[] { ConcealmentTechnique.HardSurfaces }).Should().Be(1.0m);
        _sut.CalculateTimeMultiplier(new[] { ConcealmentTechnique.BrushTracks }).Should().Be(1.5m);
        _sut.CalculateTimeMultiplier(new[] { ConcealmentTechnique.FalseTrail }).Should().Be(2.0m);
        _sut.CalculateTimeMultiplier(new[] { ConcealmentTechnique.WaterCrossing }).Should().Be(1.0m);
        _sut.CalculateTimeMultiplier(new[] { ConcealmentTechnique.Backtracking }).Should().Be(1.25m);
    }

    [Test]
    public void CalculateTimeMultiplier_WithMultipleTechniques_CompoundsMultiplicatively()
    {
        // Arrange - BrushTracks (x1.5) × Backtracking (x1.25) = x1.875
        var techniques = new[]
        {
            ConcealmentTechnique.BrushTracks,
            ConcealmentTechnique.Backtracking
        };

        // Act
        var multiplier = _sut.CalculateTimeMultiplier(techniques);

        // Assert
        multiplier.Should().Be(1.875m);
    }

    [Test]
    public void CalculateTimeMultiplier_WithAllTechniques_ReturnsCompoundedMultiplier()
    {
        // Arrange - 1.0 × 1.5 × 2.0 × 1.0 × 1.25 = 3.75
        var techniques = new[]
        {
            ConcealmentTechnique.HardSurfaces,
            ConcealmentTechnique.BrushTracks,
            ConcealmentTechnique.FalseTrail,
            ConcealmentTechnique.WaterCrossing,
            ConcealmentTechnique.Backtracking
        };

        // Act
        var multiplier = _sut.CalculateTimeMultiplier(techniques);

        // Assert
        multiplier.Should().Be(3.75m);
    }

    [Test]
    public void GetTechniqueTimeMultiplier_ReturnsCorrectMultiplierForEachTechnique()
    {
        // Assert
        _sut.GetTechniqueTimeMultiplier(ConcealmentTechnique.HardSurfaces).Should().Be(1.0m);
        _sut.GetTechniqueTimeMultiplier(ConcealmentTechnique.BrushTracks).Should().Be(1.5m);
        _sut.GetTechniqueTimeMultiplier(ConcealmentTechnique.FalseTrail).Should().Be(2.0m);
        _sut.GetTechniqueTimeMultiplier(ConcealmentTechnique.WaterCrossing).Should().Be(1.0m);
        _sut.GetTechniqueTimeMultiplier(ConcealmentTechnique.Backtracking).Should().Be(1.25m);
    }

    #endregion

    #region Technique Validation Tests

    [Test]
    public void ValidateTechniques_WaterCrossingWithoutWater_ReturnsFalse()
    {
        // Arrange
        var context = new CounterTrackingContext(
            ConcealerId: "player-1",
            TechniquesUsed: new[] { ConcealmentTechnique.WaterCrossing },
            EnvironmentId: "test-area",
            HasWaterNearby: false, // No water
            HasFoliageOrDebris: true);

        // Act
        var isValid = _sut.ValidateTechniques(context);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void ValidateTechniques_WaterCrossingWithWater_ReturnsTrue()
    {
        // Arrange
        var context = new CounterTrackingContext(
            ConcealerId: "player-1",
            TechniquesUsed: new[] { ConcealmentTechnique.WaterCrossing },
            EnvironmentId: "test-area",
            HasWaterNearby: true, // Has water
            HasFoliageOrDebris: false);

        // Act
        var isValid = _sut.ValidateTechniques(context);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void ValidateTechniques_BrushTracksWithoutFoliage_ReturnsFalse()
    {
        // Arrange
        var context = new CounterTrackingContext(
            ConcealerId: "player-1",
            TechniquesUsed: new[] { ConcealmentTechnique.BrushTracks },
            EnvironmentId: "test-area",
            HasWaterNearby: true,
            HasFoliageOrDebris: false); // No foliage

        // Act
        var isValid = _sut.ValidateTechniques(context);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void ValidateTechniques_BrushTracksWithFoliage_ReturnsTrue()
    {
        // Arrange
        var context = new CounterTrackingContext(
            ConcealerId: "player-1",
            TechniquesUsed: new[] { ConcealmentTechnique.BrushTracks },
            EnvironmentId: "test-area",
            HasWaterNearby: false,
            HasFoliageOrDebris: true); // Has foliage

        // Act
        var isValid = _sut.ValidateTechniques(context);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void ValidateTechniques_AlwaysAvailableTechniques_ReturnsTrue()
    {
        // Arrange - HardSurfaces, FalseTrail, Backtracking are always available
        var context = new CounterTrackingContext(
            ConcealerId: "player-1",
            TechniquesUsed: new[]
            {
                ConcealmentTechnique.HardSurfaces,
                ConcealmentTechnique.FalseTrail,
                ConcealmentTechnique.Backtracking
            },
            EnvironmentId: "test-area",
            HasWaterNearby: false,
            HasFoliageOrDebris: false);

        // Act
        var isValid = _sut.ValidateTechniques(context);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void GetAvailableTechniques_WithNoResources_ReturnsBaseThree()
    {
        // Act
        var techniques = _sut.GetAvailableTechniques(
            hasWaterNearby: false,
            hasFoliageOrDebris: false);

        // Assert
        techniques.Should().HaveCount(3);
        techniques.Should().Contain(ConcealmentTechnique.HardSurfaces);
        techniques.Should().Contain(ConcealmentTechnique.FalseTrail);
        techniques.Should().Contain(ConcealmentTechnique.Backtracking);
    }

    [Test]
    public void GetAvailableTechniques_WithWater_IncludesWaterCrossing()
    {
        // Act
        var techniques = _sut.GetAvailableTechniques(
            hasWaterNearby: true,
            hasFoliageOrDebris: false);

        // Assert
        techniques.Should().HaveCount(4);
        techniques.Should().Contain(ConcealmentTechnique.WaterCrossing);
    }

    [Test]
    public void GetAvailableTechniques_WithFoliage_IncludesBrushTracks()
    {
        // Act
        var techniques = _sut.GetAvailableTechniques(
            hasWaterNearby: false,
            hasFoliageOrDebris: true);

        // Assert
        techniques.Should().HaveCount(4);
        techniques.Should().Contain(ConcealmentTechnique.BrushTracks);
    }

    [Test]
    public void GetAvailableTechniques_WithAllResources_ReturnsFive()
    {
        // Act
        var techniques = _sut.GetAvailableTechniques(
            hasWaterNearby: true,
            hasFoliageOrDebris: true);

        // Assert
        techniques.Should().HaveCount(5);
    }

    #endregion

    #region Technique Description Tests

    [Test]
    public void GetTechniqueDescription_ReturnsNameAndDescription()
    {
        // Act & Assert
        var (name, desc) = _sut.GetTechniqueDescription(ConcealmentTechnique.HardSurfaces);
        name.Should().Be("Hard Surfaces");
        desc.Should().NotBeNullOrWhiteSpace();

        (name, desc) = _sut.GetTechniqueDescription(ConcealmentTechnique.BrushTracks);
        name.Should().Be("Brush Tracks");
        desc.Should().NotBeNullOrWhiteSpace();

        (name, desc) = _sut.GetTechniqueDescription(ConcealmentTechnique.FalseTrail);
        name.Should().Be("False Trail");
        desc.Should().NotBeNullOrWhiteSpace();

        (name, desc) = _sut.GetTechniqueDescription(ConcealmentTechnique.WaterCrossing);
        name.Should().Be("Water Crossing");
        desc.Should().NotBeNullOrWhiteSpace();

        (name, desc) = _sut.GetTechniqueDescription(ConcealmentTechnique.Backtracking);
        name.Should().Be("Backtracking");
        desc.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region TrackingState Integration Tests

    [Test]
    public void ApplyToTrackingState_SetsContestedDc()
    {
        // Arrange
        var trackingState = TrackingState.Create("tracker-1", "target", TrailAge.Fresh);
        var result = CounterTrackingResult.Create(
            netSuccesses: 5,
            totalBonus: 8,
            timeMultiplier: 1.5m,
            techniques: new[] { ConcealmentTechnique.BrushTracks, ConcealmentTechnique.Backtracking },
            rollDetails: "Test roll");

        // Act
        _sut.ApplyToTrackingState(trackingState, result);

        // Assert
        trackingState.HasCounterTracking.Should().BeTrue();
        trackingState.ContestedDc.Should().Be(result.ConcealmentDc);
        trackingState.ConcealmentTimeMultiplier.Should().Be(1.5m);
    }

    [Test]
    public void ClearFromTrackingState_RemovesContestedDc()
    {
        // Arrange
        var trackingState = TrackingState.Create("tracker-1", "target", TrailAge.Fresh);
        var result = CounterTrackingResult.Create(
            netSuccesses: 5,
            totalBonus: 8,
            timeMultiplier: 1.5m,
            techniques: new[] { ConcealmentTechnique.BrushTracks },
            rollDetails: "Test roll");
        _sut.ApplyToTrackingState(trackingState, result);

        // Act
        _sut.ClearFromTrackingState(trackingState);

        // Assert
        trackingState.HasCounterTracking.Should().BeFalse();
        trackingState.ContestedDc.Should().BeNull();
        trackingState.ConcealmentTimeMultiplier.Should().Be(1.0m);
    }

    [Test]
    public void ApplyToTrackingState_ActualBaseDcReflectsContestedDc()
    {
        // Arrange
        var trackingState = TrackingState.Create("tracker-1", "target", TrailAge.Fresh);
        var originalBaseDc = trackingState.BaseDc;

        var result = CounterTrackingResult.Create(
            netSuccesses: 10,
            totalBonus: 10,
            timeMultiplier: 1.0m,
            techniques: new[] { ConcealmentTechnique.WaterCrossing },
            rollDetails: "Test roll");

        // Act
        _sut.ApplyToTrackingState(trackingState, result);

        // Assert
        trackingState.ActualBaseDc.Should().Be(result.ConcealmentDc);
        trackingState.ActualBaseDc.Should().NotBe(originalBaseDc);
    }

    #endregion

    #region CounterTrackingResult Tests

    [Test]
    public void CounterTrackingResult_ClampsMinimumDc()
    {
        // Arrange - Very low net successes should clamp to 10
        var result = CounterTrackingResult.Create(
            netSuccesses: -5,
            totalBonus: 2,
            timeMultiplier: 1.0m,
            techniques: new[] { ConcealmentTechnique.HardSurfaces },
            rollDetails: "Low roll");

        // Assert
        result.ConcealmentDc.Should().Be(10); // Minimum DC
    }

    [Test]
    public void CounterTrackingResult_ClampsMaximumDc()
    {
        // Arrange - Very high values should clamp to 30
        var result = CounterTrackingResult.Create(
            netSuccesses: 25,
            totalBonus: 24, // All techniques
            timeMultiplier: 1.0m,
            techniques: Enum.GetValues<ConcealmentTechnique>(),
            rollDetails: "High roll");

        // Assert
        result.ConcealmentDc.Should().Be(30); // Maximum DC
    }

    [Test]
    public void CounterTrackingResult_CalculatesEffectivenessRating()
    {
        // Arrange & Act
        // Ratings: 10-14 = Minimal, 15-19 = Moderate, 20-24 = Strong, 25-30 = Exceptional
        var minimalResult = CounterTrackingResult.Create(3, 5, 1.0m, Array.Empty<ConcealmentTechnique>(), "");
        var moderateResult = CounterTrackingResult.Create(10, 5, 1.0m, Array.Empty<ConcealmentTechnique>(), "");
        var strongResult = CounterTrackingResult.Create(15, 5, 1.0m, Array.Empty<ConcealmentTechnique>(), "");
        var exceptionalResult = CounterTrackingResult.Create(20, 5, 1.0m, Array.Empty<ConcealmentTechnique>(), "");

        // Assert
        minimalResult.EffectivenessRating.Should().Be("Minimal"); // DC 10 (clamped from 8)
        moderateResult.EffectivenessRating.Should().Be("Moderate"); // DC 15
        strongResult.EffectivenessRating.Should().Be("Strong"); // DC 20
        exceptionalResult.EffectivenessRating.Should().Be("Exceptional"); // DC 25
    }

    [Test]
    public void CounterTrackingResult_Failed_ReturnsMinimumDc()
    {
        // Act
        var result = CounterTrackingResult.Failed(0, "Failed concealment");

        // Assert
        result.ConcealmentDc.Should().Be(10);
        // Note: IsEffective is true even for failed results because DC is clamped to minimum 10,
        // which meets the threshold. This is by design - even poor concealment provides some protection.
        result.IsEffective.Should().BeTrue();
        result.NetSuccesses.Should().Be(0);
        result.TotalBonus.Should().Be(0);
        result.EffectivenessRating.Should().Be("Minimal"); // DC 10 = Minimal effectiveness
    }

    [Test]
    public void CounterTrackingResult_HasTimePenalty_WhenMultiplierAboveOne()
    {
        // Arrange
        var withPenalty = CounterTrackingResult.Create(10, 4, 1.5m, new[] { ConcealmentTechnique.BrushTracks }, "");
        var withoutPenalty = CounterTrackingResult.Create(10, 2, 1.0m, new[] { ConcealmentTechnique.HardSurfaces }, "");

        // Assert
        withPenalty.HasTimePenalty.Should().BeTrue();
        withPenalty.TimePenaltyPercent.Should().Be(50);
        withoutPenalty.HasTimePenalty.Should().BeFalse();
        withoutPenalty.TimePenaltyPercent.Should().Be(0);
    }

    #endregion

    #region CounterTrackingContext Tests

    [Test]
    public void CounterTrackingContext_GetValidTechniques_FiltersInvalidTechniques()
    {
        // Arrange
        var context = new CounterTrackingContext(
            ConcealerId: "player-1",
            TechniquesUsed: new[]
            {
                ConcealmentTechnique.HardSurfaces,    // Valid (always)
                ConcealmentTechnique.WaterCrossing,   // Invalid (no water)
                ConcealmentTechnique.BrushTracks,     // Invalid (no foliage)
                ConcealmentTechnique.Backtracking     // Valid (always)
            },
            EnvironmentId: "test-area",
            HasWaterNearby: false,
            HasFoliageOrDebris: false);

        // Act
        var valid = context.GetValidTechniques();
        var invalid = context.GetInvalidTechniques();

        // Assert
        valid.Should().HaveCount(2);
        valid.Should().Contain(ConcealmentTechnique.HardSurfaces);
        valid.Should().Contain(ConcealmentTechnique.Backtracking);

        invalid.Should().HaveCount(2);
        invalid.Should().Contain(ConcealmentTechnique.WaterCrossing);
        invalid.Should().Contain(ConcealmentTechnique.BrushTracks);
    }

    [Test]
    public void CounterTrackingContext_CreateDefault_HasNoEnvironmentalFeatures()
    {
        // Act
        var context = CounterTrackingContext.CreateDefault(
            "player-1",
            ConcealmentTechnique.HardSurfaces,
            ConcealmentTechnique.WaterCrossing);

        // Assert
        context.HasWaterNearby.Should().BeFalse();
        context.HasFoliageOrDebris.Should().BeFalse();
        context.ValidTechniqueCount.Should().Be(1); // Only HardSurfaces
        context.InvalidTechniqueCount.Should().Be(1); // WaterCrossing
    }

    [Test]
    public void CounterTrackingContext_CreateNearWater_HasWater()
    {
        // Act
        var context = CounterTrackingContext.CreateNearWater(
            "player-1",
            "river-zone",
            hasFoliage: true,
            ConcealmentTechnique.WaterCrossing,
            ConcealmentTechnique.BrushTracks);

        // Assert
        context.HasWaterNearby.Should().BeTrue();
        context.HasFoliageOrDebris.Should().BeTrue();
        context.AreAllTechniquesValid().Should().BeTrue();
    }

    [Test]
    public void CounterTrackingContext_ToSkillContext_CreatesValidContext()
    {
        // Arrange
        var context = new CounterTrackingContext(
            ConcealerId: "player-1",
            TechniquesUsed: new[]
            {
                ConcealmentTechnique.HardSurfaces,
                ConcealmentTechnique.Backtracking
            },
            EnvironmentId: "test-area",
            HasWaterNearby: false,
            HasFoliageOrDebris: false);

        // Act
        var skillContext = context.ToSkillContext();

        // Assert
        skillContext.Should().NotBeNull();
        skillContext.SituationalModifiers.Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock SkillCheckService for testing.
    /// </summary>
    /// <remarks>
    /// This is a simplified mock that returns consistent results.
    /// Uses a seeded random for reproducibility.
    /// </remarks>
    private static SkillCheckService CreateMockSkillCheckService()
    {
        // Create deterministic random for tests
        var seededRandom = new Random(42);
        var diceLogger = Substitute.For<ILogger<DiceService>>();

        var diceService = new DiceService(diceLogger, seededRandom);
        var configProvider = Substitute.For<IGameConfigurationProvider>();
        var logger = Substitute.For<ILogger<SkillCheckService>>();

        return new SkillCheckService(diceService, configProvider, logger);
    }

    #endregion
}
