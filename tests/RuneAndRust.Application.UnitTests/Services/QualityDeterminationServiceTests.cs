// ═══════════════════════════════════════════════════════════════════════════════
// QualityDeterminationServiceTests.cs
// Unit tests for the QualityDeterminationService.
// Version: 0.11.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="QualityDeterminationService"/>.
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Quality determination based on roll margin</description></item>
///   <item><description>Natural 20 always results in Legendary</description></item>
///   <item><description>Margin thresholds for Fine (5+) and Masterwork (10+)</description></item>
///   <item><description>Tier and modifier lookups</description></item>
///   <item><description>QualityModifier calculation methods</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class QualityDeterminationServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mock quality tier provider for testing.
    /// </summary>
    private Mock<IQualityTierProvider> _mockTierProvider = null!;

    /// <summary>
    /// Mock logger for the service.
    /// </summary>
    private Mock<ILogger<QualityDeterminationService>> _mockLogger = null!;

    /// <summary>
    /// The service under test.
    /// </summary>
    private QualityDeterminationService _service = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // Setup
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up the test fixtures before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockTierProvider = new Mock<IQualityTierProvider>();
        _mockLogger = new Mock<ILogger<QualityDeterminationService>>();

        // Set up default tier provider behavior
        SetupDefaultTierProvider();

        // Create the service under test
        _service = new QualityDeterminationService(
            _mockTierProvider.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// Configures the mock tier provider with default tier definitions.
    /// </summary>
    private void SetupDefaultTierProvider()
    {
        var standardTier = QualityTierDefinition.CreateStandard();
        var fineTier = QualityTierDefinition.CreateFine();
        var masterworkTier = QualityTierDefinition.CreateMasterwork();
        var legendaryTier = QualityTierDefinition.CreateLegendary();

        // Setup GetTier for each quality level
        _mockTierProvider
            .Setup(p => p.GetTier(CraftedItemQuality.Standard))
            .Returns(standardTier);
        _mockTierProvider
            .Setup(p => p.GetTier(CraftedItemQuality.Fine))
            .Returns(fineTier);
        _mockTierProvider
            .Setup(p => p.GetTier(CraftedItemQuality.Masterwork))
            .Returns(masterworkTier);
        _mockTierProvider
            .Setup(p => p.GetTier(CraftedItemQuality.Legendary))
            .Returns(legendaryTier);

        // Setup GetModifiers for each quality level
        _mockTierProvider
            .Setup(p => p.GetModifiers(CraftedItemQuality.Standard))
            .Returns(standardTier.Modifiers);
        _mockTierProvider
            .Setup(p => p.GetModifiers(CraftedItemQuality.Fine))
            .Returns(fineTier.Modifiers);
        _mockTierProvider
            .Setup(p => p.GetModifiers(CraftedItemQuality.Masterwork))
            .Returns(masterworkTier.Modifiers);
        _mockTierProvider
            .Setup(p => p.GetModifiers(CraftedItemQuality.Legendary))
            .Returns(legendaryTier.Modifiers);

        // Setup GetAllTiers
        _mockTierProvider
            .Setup(p => p.GetAllTiers())
            .Returns(new List<QualityTierDefinition>
            {
                standardTier,
                fineTier,
                masterworkTier,
                legendaryTier
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the constructor throws when tier provider is null.
    /// </summary>
    [Test]
    public void Constructor_WhenTierProviderIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QualityDeterminationService(
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("tierProvider");
    }

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QualityDeterminationService(
            _mockTierProvider.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DetermineQuality Tests - Natural 20
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a natural 20 always results in Legendary quality.
    /// </summary>
    [Test]
    public void DetermineQuality_WhenNatural20_ReturnsLegendary()
    {
        // Arrange
        int rollResult = 25;
        int dc = 15;
        bool isNatural20 = true;

        // Act
        var result = _service.DetermineQuality(rollResult, dc, isNatural20);

        // Assert
        result.Should().Be(CraftedItemQuality.Legendary);
    }

    /// <summary>
    /// Verifies that natural 20 takes precedence over margin calculations.
    /// </summary>
    [Test]
    public void DetermineQuality_WhenNatural20WithLowMargin_StillReturnsLegendary()
    {
        // Arrange - Natural 20 but margin is only 2
        int rollResult = 17;
        int dc = 15;
        bool isNatural20 = true;

        // Act
        var result = _service.DetermineQuality(rollResult, dc, isNatural20);

        // Assert
        result.Should().Be(CraftedItemQuality.Legendary);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DetermineQuality Tests - Margin Thresholds
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a margin of 10 or more results in Masterwork quality.
    /// </summary>
    [Test]
    [TestCase(25, 15, ExpectedResult = CraftedItemQuality.Masterwork)] // Margin = 10 (exact threshold)
    [TestCase(26, 15, ExpectedResult = CraftedItemQuality.Masterwork)] // Margin = 11
    [TestCase(30, 15, ExpectedResult = CraftedItemQuality.Masterwork)] // Margin = 15
    public CraftedItemQuality DetermineQuality_WhenMarginAtLeast10_ReturnsMasterwork(
        int rollResult, int dc)
    {
        // Act
        return _service.DetermineQuality(rollResult, dc, isNatural20: false);
    }

    /// <summary>
    /// Verifies that a margin of 5-9 results in Fine quality.
    /// </summary>
    [Test]
    [TestCase(20, 15, ExpectedResult = CraftedItemQuality.Fine)] // Margin = 5 (exact threshold)
    [TestCase(21, 15, ExpectedResult = CraftedItemQuality.Fine)] // Margin = 6
    [TestCase(24, 15, ExpectedResult = CraftedItemQuality.Fine)] // Margin = 9 (just below Masterwork)
    public CraftedItemQuality DetermineQuality_WhenMarginBetween5And9_ReturnsFine(
        int rollResult, int dc)
    {
        // Act
        return _service.DetermineQuality(rollResult, dc, isNatural20: false);
    }

    /// <summary>
    /// Verifies that a margin of 0-4 results in Standard quality.
    /// </summary>
    [Test]
    [TestCase(15, 15, ExpectedResult = CraftedItemQuality.Standard)] // Margin = 0
    [TestCase(16, 15, ExpectedResult = CraftedItemQuality.Standard)] // Margin = 1
    [TestCase(19, 15, ExpectedResult = CraftedItemQuality.Standard)] // Margin = 4 (just below Fine)
    public CraftedItemQuality DetermineQuality_WhenMarginLessThan5_ReturnsStandard(
        int rollResult, int dc)
    {
        // Act
        return _service.DetermineQuality(rollResult, dc, isNatural20: false);
    }

    /// <summary>
    /// Verifies the exact boundary between Standard and Fine quality.
    /// </summary>
    [Test]
    public void DetermineQuality_AtFineThresholdBoundary_ReturnsCorrectQuality()
    {
        // Arrange
        int dc = 15;

        // Act - Margin of 4 should be Standard
        var marginFour = _service.DetermineQuality(19, dc, false);

        // Act - Margin of 5 should be Fine
        var marginFive = _service.DetermineQuality(20, dc, false);

        // Assert
        marginFour.Should().Be(CraftedItemQuality.Standard, "margin 4 should be Standard");
        marginFive.Should().Be(CraftedItemQuality.Fine, "margin 5 should be Fine");
    }

    /// <summary>
    /// Verifies the exact boundary between Fine and Masterwork quality.
    /// </summary>
    [Test]
    public void DetermineQuality_AtMasterworkThresholdBoundary_ReturnsCorrectQuality()
    {
        // Arrange
        int dc = 15;

        // Act - Margin of 9 should be Fine
        var marginNine = _service.DetermineQuality(24, dc, false);

        // Act - Margin of 10 should be Masterwork
        var marginTen = _service.DetermineQuality(25, dc, false);

        // Assert
        marginNine.Should().Be(CraftedItemQuality.Fine, "margin 9 should be Fine");
        marginTen.Should().Be(CraftedItemQuality.Masterwork, "margin 10 should be Masterwork");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetQualityTier Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetQualityTier returns the correct tier definition.
    /// </summary>
    [Test]
    public void GetQualityTier_ReturnsCorrectDefinition()
    {
        // Act
        var tier = _service.GetQualityTier(CraftedItemQuality.Masterwork);

        // Assert
        tier.Quality.Should().Be(CraftedItemQuality.Masterwork);
        tier.DisplayName.Should().Be("Masterwork");
        tier.ColorCode.Should().Be("#0070DD");
        tier.Modifiers.StatMultiplier.Should().Be(1.25m);
        tier.Modifiers.ValueMultiplier.Should().Be(2.5m);
    }

    /// <summary>
    /// Verifies that GetQualityTier calls the tier provider.
    /// </summary>
    [Test]
    public void GetQualityTier_CallsTierProvider()
    {
        // Act
        _ = _service.GetQualityTier(CraftedItemQuality.Fine);

        // Assert
        _mockTierProvider.Verify(
            p => p.GetTier(CraftedItemQuality.Fine),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetQualityModifiers Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetQualityModifiers returns the correct multipliers.
    /// </summary>
    [Test]
    public void GetQualityModifiers_ReturnsCorrectMultipliers()
    {
        // Act
        var modifiers = _service.GetQualityModifiers(CraftedItemQuality.Legendary);

        // Assert
        modifiers.StatMultiplier.Should().Be(1.50m);
        modifiers.ValueMultiplier.Should().Be(5.0m);
    }

    /// <summary>
    /// Verifies that GetQualityModifiers calls the tier provider.
    /// </summary>
    [Test]
    public void GetQualityModifiers_CallsTierProvider()
    {
        // Act
        _ = _service.GetQualityModifiers(CraftedItemQuality.Standard);

        // Assert
        _mockTierProvider.Verify(
            p => p.GetModifiers(CraftedItemQuality.Standard),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QualityModifier Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ApplyToStat correctly scales base stats.
    /// </summary>
    [Test]
    [TestCase(100, 1.0, ExpectedResult = 100)]  // Standard: no change
    [TestCase(100, 1.10, ExpectedResult = 110)] // Fine: +10%
    [TestCase(100, 1.25, ExpectedResult = 125)] // Masterwork: +25%
    [TestCase(100, 1.50, ExpectedResult = 150)] // Legendary: +50%
    public int QualityModifier_ApplyToStat_CalculatesCorrectly(
        int baseStat, decimal multiplier)
    {
        // Arrange
        var modifier = QualityModifier.Create(multiplier, 1.0m);

        // Act
        return modifier.ApplyToStat(baseStat);
    }

    /// <summary>
    /// Verifies that ApplyToValue correctly scales base values.
    /// </summary>
    [Test]
    [TestCase(100, 1.0, ExpectedResult = 100)]  // Standard: no change
    [TestCase(100, 1.5, ExpectedResult = 150)]  // Fine: +50%
    [TestCase(100, 2.5, ExpectedResult = 250)]  // Masterwork: +150%
    [TestCase(100, 5.0, ExpectedResult = 500)]  // Legendary: +400%
    public int QualityModifier_ApplyToValue_CalculatesCorrectly(
        int baseValue, decimal multiplier)
    {
        // Arrange
        var modifier = QualityModifier.Create(1.0m, multiplier);

        // Act
        return modifier.ApplyToValue(baseValue);
    }

    /// <summary>
    /// Verifies that QualityModifier.None returns neutral multipliers.
    /// </summary>
    [Test]
    public void QualityModifier_None_ReturnsNeutralMultipliers()
    {
        // Act
        var modifier = QualityModifier.None;

        // Assert
        modifier.StatMultiplier.Should().Be(1.0m);
        modifier.ValueMultiplier.Should().Be(1.0m);
        modifier.HasEffect.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasEffect returns true when multipliers differ from 1.0.
    /// </summary>
    [Test]
    public void QualityModifier_HasEffect_ReturnsTrueWhenNotNeutral()
    {
        // Arrange
        var modifier = QualityModifier.Create(1.25m, 2.5m);

        // Act & Assert
        modifier.HasEffect.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Edge Case Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies behavior with very high roll margins.
    /// </summary>
    [Test]
    public void DetermineQuality_WithVeryHighMargin_ReturnsMasterwork()
    {
        // Arrange - Extremely high margin (natural 20 is false)
        int rollResult = 50;
        int dc = 10;

        // Act
        var result = _service.DetermineQuality(rollResult, dc, isNatural20: false);

        // Assert - Still Masterwork (not Legendary without natural 20)
        result.Should().Be(CraftedItemQuality.Masterwork);
    }

    /// <summary>
    /// Verifies rounding behavior in stat calculations.
    /// </summary>
    [Test]
    public void QualityModifier_ApplyToStat_RoundsCorrectly()
    {
        // Arrange - 1.10 * 15 = 16.5, should round to 16 or 17
        var modifier = QualityModifier.Create(1.10m, 1.0m);

        // Act
        int result = modifier.ApplyToStat(15);

        // Assert - Math.Round uses banker's rounding by default
        result.Should().BeOneOf(16, 17);
    }
}
