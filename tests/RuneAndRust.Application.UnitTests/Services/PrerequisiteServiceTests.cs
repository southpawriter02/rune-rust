// ═══════════════════════════════════════════════════════════════════════════════
// PrerequisiteServiceTests.cs
// Unit tests for the PrerequisiteService application service.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;

[TestFixture]
public class PrerequisiteServiceTests
{
    private PrerequisiteService _service = null!;
    private Mock<ILogger<PrerequisiteService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<PrerequisiteService>>();
        _service = new PrerequisiteService(_mockLogger.Object);
    }

    [Test]
    public void CanUnlockAbility_Tier2With8PPInvested_ReturnsSuccess()
    {
        // Arrange — 2 Tier 2 abilities = 4 + 4 = 8 PP
        var unlocked = new List<SkjaldmaerAbilityId>
        {
            SkjaldmaerAbilityId.HoldTheLine,
            SkjaldmaerAbilityId.CounterShield
        };

        // Act
        var result = _service.CanUnlockAbility(SkjaldmaerAbilityId.Rally, unlocked);

        // Assert
        result.CanUnlock.Should().BeTrue();
        result.FailureReason.Should().BeNull();
        result.MissingPrerequisites.Should().BeEmpty();
    }

    [Test]
    public void CanUnlockAbility_Tier2WithInsufficientPP_ReturnsFailure()
    {
        // Arrange — only Tier 1 abilities = 0 PP
        var unlocked = new List<SkjaldmaerAbilityId>
        {
            SkjaldmaerAbilityId.ShieldWall,
            SkjaldmaerAbilityId.Intercept,
            SkjaldmaerAbilityId.Bulwark
        };

        // Act
        var result = _service.CanUnlockAbility(SkjaldmaerAbilityId.HoldTheLine, unlocked);

        // Assert
        result.CanUnlock.Should().BeFalse();
        result.FailureReason.Should().Contain("Insufficient PP");
        result.MissingPrerequisites.Should().NotBeEmpty();
    }

    [Test]
    public void CanUnlockAbility_Tier1_AlwaysSucceeds()
    {
        // Arrange — no abilities unlocked yet
        var unlocked = new List<SkjaldmaerAbilityId>();

        // Act
        var result = _service.CanUnlockAbility(SkjaldmaerAbilityId.ShieldWall, unlocked);

        // Assert
        result.CanUnlock.Should().BeTrue();
    }

    [Test]
    public void GetTotalPPInvested_CalculatesCorrectSum()
    {
        // Arrange — mixed tiers: 0 + 0 + 0 + 4 + 4 = 8 PP
        var unlocked = new List<SkjaldmaerAbilityId>
        {
            SkjaldmaerAbilityId.ShieldWall,     // 0 PP
            SkjaldmaerAbilityId.Intercept,       // 0 PP
            SkjaldmaerAbilityId.Bulwark,         // 0 PP
            SkjaldmaerAbilityId.HoldTheLine,     // 4 PP
            SkjaldmaerAbilityId.CounterShield    // 4 PP
        };

        // Act
        var ppInvested = _service.GetTotalPPInvested(unlocked);

        // Assert
        ppInvested.Should().Be(8);
    }

    [Test]
    public void GetTotalPPInvested_WithEmptyList_ReturnsZero()
    {
        // Arrange
        var unlocked = new List<SkjaldmaerAbilityId>();

        // Act
        var ppInvested = _service.GetTotalPPInvested(unlocked);

        // Assert
        ppInvested.Should().Be(0);
    }

    [Test]
    public void GetAbilityPPCost_ReturnsCorrectCostPerTier()
    {
        // Tier 1: Free
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.ShieldWall).Should().Be(0);
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.Intercept).Should().Be(0);
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.Bulwark).Should().Be(0);

        // Tier 2: 4 PP each
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.HoldTheLine).Should().Be(4);
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.CounterShield).Should().Be(4);
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.Rally).Should().Be(4);

        // Tier 3: 5 PP each
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.Unbreakable).Should().Be(5);
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.GuardiansSacrifice).Should().Be(5);

        // Capstone: 6 PP
        PrerequisiteService.GetAbilityPPCost(SkjaldmaerAbilityId.TheWallLives).Should().Be(6);
    }

    [Test]
    public void GetMissingPrerequisites_WhenPPDeficient_ReportsDeficit()
    {
        // Arrange — only 4 PP invested, need 8 for Tier 2
        var unlocked = new List<SkjaldmaerAbilityId>
        {
            SkjaldmaerAbilityId.HoldTheLine // 4 PP
        };

        // Act
        var missing = _service.GetMissingPrerequisites(SkjaldmaerAbilityId.Rally, unlocked);

        // Assert
        missing.Should().ContainSingle().Which.Should().Contain("4 more PP");
    }
}
