using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="VeidimadurAbilityService"/>.
/// Tests Tier 1 abilities: Mark Quarry (active targeting), Keen Senses (passive bonus),
/// Read the Signs (active investigation with skill check); and Tier 2 abilities:
/// Hunter's Eye (passive cover bypass), Trap Mastery (place/detect traps),
/// and Predator's Patience (stationary hit bonus stance).
/// </summary>
/// <remarks>
/// <para>Follows the test subclass pattern established by
/// <see cref="BoneSetterAbilityServiceTests"/>. The <see cref="TestVeidimadurAbilityService"/>
/// overrides the <c>internal virtual Roll1D20()</c> and <c>Roll1D8()</c> methods
/// for deterministic testing.</para>
/// <para>The <see cref="IVeidimadurQuarryMarksService"/> is mocked via Moq. All Quarry Marks
/// state is managed through mock setups rather than real Player state, ensuring these are
/// true unit tests with no cross-service dependencies.</para>
/// <para>Tier 1 tests introduced in v0.20.7a. Tier 2 tests added in v0.20.7b.</para>
/// </remarks>
[TestFixture]
public class VeidimadurAbilityServiceTests
{
    private Mock<IVeidimadurQuarryMarksService> _mockQuarryMarksService = null!;
    private TestVeidimadurAbilityService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockQuarryMarksService = new Mock<IVeidimadurQuarryMarksService>();
        _service = new TestVeidimadurAbilityService(
            _mockQuarryMarksService.Object,
            Mock.Of<ILogger<VeidimadurAbilityService>>());
    }

    /// <summary>
    /// Test subclass that overrides dice methods for deterministic testing.
    /// Provides <see cref="Fixed1D20"/> for skill checks and <see cref="Fixed1D8"/>
    /// for trap damage calculations.
    /// </summary>
    private class TestVeidimadurAbilityService : VeidimadurAbilityService
    {
        public int Fixed1D20 { get; set; } = 10;
        public int Fixed1D8 { get; set; } = 5;

        public TestVeidimadurAbilityService(
            IVeidimadurQuarryMarksService quarryMarksService,
            ILogger<VeidimadurAbilityService> logger)
            : base(quarryMarksService, logger) { }

        internal override int Roll1D20() => Fixed1D20;
        internal override int Roll1D8() => Fixed1D8;
    }

    /// <summary>
    /// Creates a Veiðimaðr player with the specified abilities unlocked and
    /// an initialized Quarry Marks resource.
    /// </summary>
    /// <param name="abilities">The Veiðimaðr abilities to unlock.</param>
    /// <returns>A configured Veiðimaðr player ready for testing.</returns>
    private static Player CreateVeidimadur(params VeidimadurAbilityId[] abilities)
    {
        var player = new Player("Test Veiðimaðr");
        player.SetSpecialization("veidimadur");
        player.InitializeQuarryMarks();
        player.CurrentAP = 10;
        foreach (var ability in abilities)
        {
            player.UnlockVeidimadurAbility(ability);
        }
        return player;
    }

    // ===== Mark Quarry Tests =====

    [Test]
    public void ExecuteMarkQuarry_WithValidPrereqs_ReturnsResult()
    {
        // Arrange
        var player = CreateVeidimadur(VeidimadurAbilityId.MarkQuarry);
        var targetId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();

        _mockQuarryMarksService
            .Setup(s => s.GetMarkCount(player))
            .Returns(0);
        _mockQuarryMarksService
            .Setup(s => s.AddMark(player, It.IsAny<QuarryMark>()))
            .Returns((QuarryMark?)null);
        // After adding, count is 1
        _mockQuarryMarksService
            .SetupSequence(s => s.GetMarkCount(player))
            .Returns(0)  // Before adding
            .Returns(1); // After adding

        // Act
        var result = _service.ExecuteMarkQuarry(player, targetId, "Draugr Scout", encounterId);

        // Assert
        result.Should().NotBeNull();
        result!.HunterId.Should().Be(player.Id);
        result.HunterName.Should().Be("Test Veiðimaðr");
        result.TargetId.Should().Be(targetId);
        result.TargetName.Should().Be("Draugr Scout");
        result.MarkCreated.Should().NotBeNull();
        result.MarkCreated.HitBonus.Should().Be(QuarryMark.DefaultHitBonus);
        result.MarkCreated.TargetId.Should().Be(targetId);
        result.PreviousMarksCount.Should().Be(0);
        result.CurrentMarksCount.Should().Be(1);
        result.ReplacedMark.Should().BeNull();
        result.WasReplacement().Should().BeFalse();
        player.CurrentAP.Should().Be(9); // 10 - 1
    }

    [Test]
    public void ExecuteMarkQuarry_AtCapacity_ReturnsResultWithReplacement()
    {
        // Arrange
        var player = CreateVeidimadur(VeidimadurAbilityId.MarkQuarry);
        var targetId = Guid.NewGuid();
        var replacedMark = QuarryMark.Create(Guid.NewGuid(), "Old Target", player.Id);

        _mockQuarryMarksService
            .SetupSequence(s => s.GetMarkCount(player))
            .Returns(3)  // Before adding (at capacity)
            .Returns(3); // After adding (still at capacity, FIFO replaced)
        _mockQuarryMarksService
            .Setup(s => s.AddMark(player, It.IsAny<QuarryMark>()))
            .Returns(replacedMark);

        // Act
        var result = _service.ExecuteMarkQuarry(player, targetId, "New Target");

        // Assert
        result.Should().NotBeNull();
        result!.WasReplacement().Should().BeTrue();
        result.ReplacedMark.Should().NotBeNull();
        result.ReplacedMark!.TargetName.Should().Be("Old Target");
        result.PreviousMarksCount.Should().Be(3);
        result.CurrentMarksCount.Should().Be(3);
        player.CurrentAP.Should().Be(9); // 10 - 1
    }

    [Test]
    public void ExecuteMarkQuarry_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateVeidimadur(VeidimadurAbilityId.MarkQuarry);
        player.CurrentAP = 0; // Need 1

        // Act
        var result = _service.ExecuteMarkQuarry(player, Guid.NewGuid(), "Target");

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(0); // Unchanged
    }

    [Test]
    public void ExecuteMarkQuarry_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteMarkQuarry(player, Guid.NewGuid(), "Target");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteMarkQuarry_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Veiðimaðr with no abilities unlocked
        var player = new Player("Test Veiðimaðr");
        player.SetSpecialization("veidimadur");
        player.InitializeQuarryMarks();
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteMarkQuarry(player, Guid.NewGuid(), "Target");

        // Assert
        result.Should().BeNull();
    }

    // ===== Read the Signs Tests =====

    [Test]
    public void ExecuteReadTheSigns_HighRoll_ReturnsSuccess()
    {
        // Arrange
        var player = CreateVeidimadur(
            VeidimadurAbilityId.ReadTheSigns,
            VeidimadurAbilityId.KeenSenses);
        _service.Fixed1D20 = 15; // 15 + 4 (ability) + 1 (keen senses) = 20 vs DC 10

        // Act
        var result = _service.ExecuteReadTheSigns(
            player,
            "muddy trail near the river",
            CreatureTrackType.Fresh,
            creatureType: "Corrupted Wolf",
            creatureCount: 3,
            timePassedEstimate: "1-2 hours ago",
            directionOfTravel: "northeast",
            creatureCondition: "bleeding");

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.SkillCheckRoll.Should().Be(20); // 15 + 4 + 1
        result.SkillCheckDc.Should().Be(10); // Fresh
        result.BonusApplied.Should().Be(5); // 4 (ability) + 1 (keen senses)
        result.CreatureType.Should().Be("Corrupted Wolf");
        result.CreatureCount.Should().Be(3);
        result.TimePassedEstimate.Should().Be("1-2 hours ago");
        result.DirectionOfTravel.Should().Be("northeast");
        result.CreatureCondition.Should().Be("bleeding");
        result.InformationRevealed.Should().HaveCount(5);
        result.LocationDescription.Should().Be("muddy trail near the river");
        player.CurrentAP.Should().Be(9); // 10 - 1
    }

    [Test]
    public void ExecuteReadTheSigns_LowRoll_ReturnsFailure()
    {
        // Arrange
        var player = CreateVeidimadur(VeidimadurAbilityId.ReadTheSigns);
        _service.Fixed1D20 = 2; // 2 + 4 (ability) + 0 (no keen senses) = 6 vs DC 20

        // Act
        var result = _service.ExecuteReadTheSigns(
            player,
            "ancient burial mound",
            CreatureTrackType.Unclear,
            creatureType: "Draugr Lord",
            creatureCount: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.SkillCheckRoll.Should().Be(6); // 2 + 4
        result.SkillCheckDc.Should().Be(20); // Unclear
        result.BonusApplied.Should().Be(4); // 4 (ability) + 0 (no keen senses)
        result.CreatureType.Should().BeNull(); // Nullified on failure
        result.CreatureCount.Should().BeNull(); // Nullified on failure
        result.InformationRevealed.Should().HaveCount(2); // Two vague impressions
        result.InformationRevealed[0].Should().Contain("too indistinct");
        player.CurrentAP.Should().Be(9); // 10 - 1
    }

    [Test]
    public void ExecuteReadTheSigns_WithKeenSenses_AppliesBonusToRoll()
    {
        // Arrange
        var player = CreateVeidimadur(
            VeidimadurAbilityId.ReadTheSigns,
            VeidimadurAbilityId.KeenSenses);
        _service.Fixed1D20 = 8; // 8 + 4 + 1 = 13 vs DC 12 (Recent) → success

        // Act
        var result = _service.ExecuteReadTheSigns(
            player,
            "forest path",
            CreatureTrackType.Recent,
            creatureType: "Troll");

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.BonusApplied.Should().Be(5); // 4 + 1 (Keen Senses)
        result.SkillCheckRoll.Should().Be(13); // 8 + 5
    }

    [Test]
    public void ExecuteReadTheSigns_WithoutKeenSenses_NoBonusToRoll()
    {
        // Arrange
        var player = CreateVeidimadur(VeidimadurAbilityId.ReadTheSigns);
        _service.Fixed1D20 = 8; // 8 + 4 + 0 = 12 vs DC 12 (Recent) → success (>=)

        // Act
        var result = _service.ExecuteReadTheSigns(
            player,
            "forest path",
            CreatureTrackType.Recent,
            creatureType: "Troll");

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.BonusApplied.Should().Be(4); // 4 only, no Keen Senses
        result.SkillCheckRoll.Should().Be(12); // 8 + 4
    }

    [Test]
    public void ExecuteReadTheSigns_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateVeidimadur(VeidimadurAbilityId.ReadTheSigns);
        player.CurrentAP = 0;

        // Act
        var result = _service.ExecuteReadTheSigns(
            player, "trail", CreatureTrackType.Fresh);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(0); // Unchanged
    }

    [Test]
    public void ExecuteReadTheSigns_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Bone-Setter");
        player.SetSpecialization("bone-setter");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteReadTheSigns(
            player, "trail", CreatureTrackType.Fresh);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteReadTheSigns_AllTrackDCs_CorrectDCValues()
    {
        // Arrange
        var player = CreateVeidimadur(VeidimadurAbilityId.ReadTheSigns);
        _service.Fixed1D20 = 20; // Max roll — always succeeds

        // Act & Assert — verify each track type maps to correct DC
        var freshResult = _service.ExecuteReadTheSigns(player, "loc", CreatureTrackType.Fresh);
        player.CurrentAP = 10; // Reset AP
        freshResult!.SkillCheckDc.Should().Be(10);

        var recentResult = _service.ExecuteReadTheSigns(player, "loc", CreatureTrackType.Recent);
        player.CurrentAP = 10;
        recentResult!.SkillCheckDc.Should().Be(12);

        var wornResult = _service.ExecuteReadTheSigns(player, "loc", CreatureTrackType.Worn);
        player.CurrentAP = 10;
        wornResult!.SkillCheckDc.Should().Be(15);

        var ancientResult = _service.ExecuteReadTheSigns(player, "loc", CreatureTrackType.Ancient);
        player.CurrentAP = 10;
        ancientResult!.SkillCheckDc.Should().Be(18);

        var unclearResult = _service.ExecuteReadTheSigns(player, "loc", CreatureTrackType.Unclear);
        unclearResult!.SkillCheckDc.Should().Be(20);
    }

    // ===== Keen Senses Tests =====

    [Test]
    public void GetKeenSensesBonus_WhenUnlocked_Returns1()
    {
        // Arrange
        var player = CreateVeidimadur(VeidimadurAbilityId.KeenSenses);

        // Act
        var bonus = _service.GetKeenSensesBonus(player);

        // Assert
        bonus.Should().Be(1);
    }

    [Test]
    public void GetKeenSensesBonus_WhenNotUnlocked_Returns0()
    {
        // Arrange
        var player = CreateVeidimadur(); // No abilities unlocked

        // Act
        var bonus = _service.GetKeenSensesBonus(player);

        // Assert
        bonus.Should().Be(0);
    }

    [Test]
    public void GetKeenSensesBonus_WrongSpecialization_Returns0()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");

        // Act
        var bonus = _service.GetKeenSensesBonus(player);

        // Assert
        bonus.Should().Be(0);
    }

    // ===== Utility Method Tests =====

    [Test]
    public void GetAbilityReadiness_WithUnlockedAbilities_ReturnsCorrectReadiness()
    {
        // Arrange
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.KeenSenses,
            VeidimadurAbilityId.ReadTheSigns);
        player.CurrentAP = 1; // Enough for Mark Quarry (1) and Read the Signs (1)

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(VeidimadurAbilityId.MarkQuarry)
            .WhoseValue.Should().BeTrue();
        readiness.Should().ContainKey(VeidimadurAbilityId.KeenSenses)
            .WhoseValue.Should().BeTrue(); // Passive, always ready
        readiness.Should().ContainKey(VeidimadurAbilityId.ReadTheSigns)
            .WhoseValue.Should().BeTrue();
    }

    [Test]
    public void GetAbilityReadiness_InsufficientAP_ActiveAbilitiesNotReady()
    {
        // Arrange
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.KeenSenses,
            VeidimadurAbilityId.ReadTheSigns);
        player.CurrentAP = 0; // Insufficient for active abilities

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness[VeidimadurAbilityId.MarkQuarry].Should().BeFalse();
        readiness[VeidimadurAbilityId.KeenSenses].Should().BeTrue(); // Passive, always ready
        readiness[VeidimadurAbilityId.ReadTheSigns].Should().BeFalse();
    }

    [Test]
    public void GetAbilityReadiness_WrongSpecialization_ReturnsEmptyDictionary()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().BeEmpty();
    }

    [Test]
    public void GetPPInvested_WithTier1Abilities_ReturnsCorrectTotal()
    {
        // Arrange
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.KeenSenses,
            VeidimadurAbilityId.ReadTheSigns);

        // Act
        var ppInvested = _service.GetPPInvested(player);

        // Assert
        ppInvested.Should().Be(0); // T1 abilities cost 0 PP each
    }

    [Test]
    public void CanUnlockTier2_InsufficientPP_ReturnsFalse()
    {
        // Arrange — T1 abilities only (0 PP invested)
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.KeenSenses);

        // Act
        var canUnlock = _service.CanUnlockTier2(player);

        // Assert
        canUnlock.Should().BeFalse(); // Need 8 PP, have 0
    }

    [Test]
    public void CanUnlockTier2_WrongSpecialization_ReturnsFalse()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");

        // Act
        var canUnlock = _service.CanUnlockTier2(player);

        // Assert
        canUnlock.Should().BeFalse();
    }

    [Test]
    public void CanUnlockTier3_InsufficientPP_ReturnsFalse()
    {
        // Arrange
        var player = CreateVeidimadur();

        // Act
        var canUnlock = _service.CanUnlockTier3(player);

        // Assert
        canUnlock.Should().BeFalse(); // Need 16 PP
    }

    [Test]
    public void CanUnlockCapstone_InsufficientPP_ReturnsFalse()
    {
        // Arrange
        var player = CreateVeidimadur();

        // Act
        var canUnlock = _service.CanUnlockCapstone(player);

        // Assert
        canUnlock.Should().BeFalse(); // Need 24 PP
    }

    // ===== Tier 2 Helper =====

    /// <summary>
    /// Creates a Veiðimaðr player with all Tier 1 and Tier 2 abilities unlocked,
    /// plus any additional abilities specified.
    /// </summary>
    /// <param name="extraAbilities">Additional abilities beyond the standard T1+T2 set.</param>
    /// <returns>A configured Veiðimaðr player ready for Tier 2 testing.</returns>
    private static Player CreateTier2Veidimadur(params VeidimadurAbilityId[] extraAbilities)
    {
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.KeenSenses,
            VeidimadurAbilityId.ReadTheSigns,
            VeidimadurAbilityId.HuntersEye,
            VeidimadurAbilityId.TrapMastery,
            VeidimadurAbilityId.PredatorsPatience);
        foreach (var ability in extraAbilities)
            player.UnlockVeidimadurAbility(ability);
        return player;
    }

    // ===== Hunter's Eye Tests (v0.20.7b) =====

    [Test]
    public void ExecuteHuntersEye_PartialCover_IgnoresCoverReturnsBonus()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        var targetId = Guid.NewGuid();

        // Act
        var result = _service.ExecuteHuntersEye(
            player, targetId, "Draugr Archer", CoverType.Partial, 8);

        // Assert
        result.Should().NotBeNull();
        result!.CoverIgnored.Should().BeTrue();
        result.BonusFromCoverIgnored.Should().Be(2);
        result.OriginalCoverType.Should().Be(CoverType.Partial);
        result.TargetName.Should().Be("Draugr Archer");
        result.TargetId.Should().Be(targetId);
        result.HunterId.Should().Be(player.Id);
        result.Distance.Should().Be(8);
        result.WasCoverIgnored().Should().BeTrue();
    }

    [Test]
    public void ExecuteHuntersEye_FullCover_DoesNotIgnore()
    {
        // Arrange
        var player = CreateTier2Veidimadur();

        // Act
        var result = _service.ExecuteHuntersEye(
            player, Guid.NewGuid(), "Troll Warden", CoverType.Full, 5);

        // Assert
        result.Should().NotBeNull();
        result!.CoverIgnored.Should().BeFalse();
        result.BonusFromCoverIgnored.Should().Be(0);
        result.OriginalCoverType.Should().Be(CoverType.Full);
        result.WasCoverIgnored().Should().BeFalse();
    }

    [Test]
    public void ExecuteHuntersEye_NoCover_NoEffectReturnsResult()
    {
        // Arrange
        var player = CreateTier2Veidimadur();

        // Act
        var result = _service.ExecuteHuntersEye(
            player, Guid.NewGuid(), "Corrupted Wolf", CoverType.None, 6);

        // Assert
        result.Should().NotBeNull();
        result!.CoverIgnored.Should().BeFalse();
        result.BonusFromCoverIgnored.Should().Be(0);
        result.OriginalCoverType.Should().Be(CoverType.None);
    }

    [Test]
    public void ExecuteHuntersEye_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — only Tier 1 abilities
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.KeenSenses,
            VeidimadurAbilityId.ReadTheSigns);

        // Act
        var result = _service.ExecuteHuntersEye(
            player, Guid.NewGuid(), "Target", CoverType.Partial, 5);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteHuntersEye_WrongSpec_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteHuntersEye(
            player, Guid.NewGuid(), "Target", CoverType.Partial, 5);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteHuntersEye_NoAPCost_APUnchanged()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        var initialAP = player.CurrentAP;

        // Act
        _service.ExecuteHuntersEye(
            player, Guid.NewGuid(), "Target", CoverType.Partial, 5);

        // Assert
        player.CurrentAP.Should().Be(initialAP,
            "Hunter's Eye is a passive ability — no AP cost");
    }

    // ===== Trap Mastery Place Tests (v0.20.7b) =====

    [Test]
    public void ExecutePlaceTrap_ValidPrereqs_ReturnsSuccessResult()
    {
        // Arrange
        var player = CreateTier2Veidimadur();

        // Act
        var result = _service.ExecutePlaceTrap(player, 5, 3, TrapType.Spike);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Type.Should().Be(TrapMasteryResult.ResultType.TrapPlaced);
        result.PlacedTrap.Should().NotBeNull();
        result.PlacedTrap!.Type.Should().Be(TrapType.Spike);
        result.PlacedTrap.X.Should().Be(5);
        result.PlacedTrap.Y.Should().Be(3);
        result.PlacedTrap.PlacedBy.Should().Be(player.Id);
        result.PlacedTrap.Status.Should().Be(TrapStatus.Armed);
        result.LocationX.Should().Be(5);
        result.LocationY.Should().Be(3);
        player.CurrentAP.Should().Be(8); // 10 - 2
        player.HuntingTraps.Should().HaveCount(1);
    }

    [Test]
    public void ExecutePlaceTrap_AtMaxTraps_ReturnsFailure()
    {
        // Arrange
        var player = CreateTier2Veidimadur();

        // Place 2 traps (the max)
        _service.ExecutePlaceTrap(player, 0, 0, TrapType.Net);
        _service.ExecutePlaceTrap(player, 1, 1, TrapType.Snare);
        player.GetArmedHuntingTraps().Should().HaveCount(2);
        player.CurrentAP = 10; // Reset AP for the test

        // Act — try to place a 3rd
        var result = _service.ExecutePlaceTrap(player, 2, 2, TrapType.PitFall);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("Maximum active traps");
        player.HuntingTraps.Should().HaveCount(2); // Still 2
        player.CurrentAP.Should().Be(10); // AP not deducted on failure
    }

    [Test]
    public void ExecutePlaceTrap_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        player.CurrentAP = 1; // Need 2

        // Act
        var result = _service.ExecutePlaceTrap(player, 0, 0, TrapType.Spike);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(1); // Unchanged
    }

    [Test]
    public void ExecutePlaceTrap_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.KeenSenses,
            VeidimadurAbilityId.ReadTheSigns);

        // Act
        var result = _service.ExecutePlaceTrap(player, 0, 0, TrapType.Spike);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecutePlaceTrap_MultipleTrapTypes_AllSucceed()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        player.CurrentAP = 20; // Plenty of AP

        // Act — place two different trap types
        var spikeResult = _service.ExecutePlaceTrap(player, 1, 1, TrapType.Spike);
        var netResult = _service.ExecutePlaceTrap(player, 2, 2, TrapType.Net);

        // Assert
        spikeResult.Should().NotBeNull();
        spikeResult!.PlacedTrap!.Type.Should().Be(TrapType.Spike);
        netResult.Should().NotBeNull();
        netResult!.PlacedTrap!.Type.Should().Be(TrapType.Net);
        player.HuntingTraps.Should().HaveCount(2);
    }

    // ===== Trap Mastery Detect Tests (v0.20.7b) =====

    [Test]
    public void ExecuteDetectTraps_HighRoll_ReturnsSuccess()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        _service.Fixed1D20 = 15; // 15 + 3 (trap bonus) + 1 (keen senses) = 19 vs DC 13

        // Act
        var result = _service.ExecuteDetectTraps(player, 5, 5);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Type.Should().Be(TrapMasteryResult.ResultType.TrapsDetected);
        result.PerceptionRoll.Should().Be(19); // 15 + 3 + 1
        result.PerceptionDc.Should().Be(13);
        result.PerceptionBonus.Should().Be(4); // 3 (trap) + 1 (keen senses)
        player.CurrentAP.Should().Be(8); // 10 - 2
    }

    [Test]
    public void ExecuteDetectTraps_LowRoll_ReturnsFailure()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        _service.Fixed1D20 = 2; // 2 + 3 + 1 = 6 vs DC 13

        // Act
        var result = _service.ExecuteDetectTraps(player, 5, 5);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.PerceptionRoll.Should().Be(6); // 2 + 3 + 1
        result.PerceptionDc.Should().Be(13);
        result.PerceptionBonus.Should().Be(4); // 3 + 1
        result.DetectedTrapsCount.Should().Be(0);
        player.CurrentAP.Should().Be(8); // 10 - 2 (AP spent even on failure)
    }

    [Test]
    public void ExecuteDetectTraps_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        player.CurrentAP = 1; // Need 2

        // Act
        var result = _service.ExecuteDetectTraps(player, 0, 0);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(1); // Unchanged
    }

    [Test]
    public void ExecuteDetectTraps_WithoutKeenSenses_LowerBonus()
    {
        // Arrange — T2 abilities but NO Keen Senses
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.ReadTheSigns,
            VeidimadurAbilityId.HuntersEye,
            VeidimadurAbilityId.TrapMastery,
            VeidimadurAbilityId.PredatorsPatience);
        _service.Fixed1D20 = 10; // 10 + 3 + 0 = 13 vs DC 13 (exact match = success)

        // Act
        var result = _service.ExecuteDetectTraps(player, 3, 3);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.PerceptionBonus.Should().Be(3); // Only trap bonus, no Keen Senses
        result.PerceptionRoll.Should().Be(13); // 10 + 3
    }

    // ===== Predator's Patience Tests (v0.20.7b) =====

    [Test]
    public void ActivatePredatorsPatience_ValidPrereqs_ReturnsActiveState()
    {
        // Arrange
        var player = CreateTier2Veidimadur();

        // Act
        var result = _service.ActivatePredatorsPatience(player);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
        result.HunterId.Should().Be(player.Id);
        result.HitBonus.Should().Be(3);
        result.HasMovedThisTurn.Should().BeFalse();
        result.GetCurrentBonus().Should().Be(3);
        player.CurrentAP.Should().Be(9); // 10 - 1
        player.PredatorsPatience.Should().NotBeNull();
        player.PredatorsPatience!.IsActive.Should().BeTrue();
    }

    [Test]
    public void ActivatePredatorsPatience_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        player.CurrentAP = 0;

        // Act
        var result = _service.ActivatePredatorsPatience(player);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(0);
    }

    [Test]
    public void ActivatePredatorsPatience_WrongSpec_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ActivatePredatorsPatience(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ActivatePredatorsPatience_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateVeidimadur(
            VeidimadurAbilityId.MarkQuarry,
            VeidimadurAbilityId.KeenSenses,
            VeidimadurAbilityId.ReadTheSigns);

        // Act
        var result = _service.ActivatePredatorsPatience(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void DeactivatePredatorsPatience_ActiveStance_ReturnsTrue()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        _service.ActivatePredatorsPatience(player); // Activate first
        player.PredatorsPatience!.IsActive.Should().BeTrue(); // Precondition

        // Act
        var result = _service.DeactivatePredatorsPatience(player);

        // Assert
        result.Should().BeTrue();
        player.PredatorsPatience!.IsActive.Should().BeFalse();
    }

    [Test]
    public void DeactivatePredatorsPatience_NotActive_ReturnsFalse()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        // Don't activate the stance

        // Act
        var result = _service.DeactivatePredatorsPatience(player);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void DeactivatePredatorsPatience_WrongSpec_ReturnsFalse()
    {
        // Arrange
        var player = new Player("Test Bone-Setter");
        player.SetSpecialization("bone-setter");
        player.CurrentAP = 10;

        // Act
        var result = _service.DeactivatePredatorsPatience(player);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetPredatorsPatienceBonus_ActiveNoMovement_Returns3()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        _service.ActivatePredatorsPatience(player);

        // Act
        var bonus = _service.GetPredatorsPatienceBonus(player);

        // Assert
        bonus.Should().Be(3);
    }

    [Test]
    public void GetPredatorsPatienceBonus_NotActive_Returns0()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        // Stance not activated

        // Act
        var bonus = _service.GetPredatorsPatienceBonus(player);

        // Assert
        bonus.Should().Be(0);
    }

    [Test]
    public void GetPredatorsPatienceBonus_AfterMovement_Returns0()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        _service.ActivatePredatorsPatience(player);
        player.PredatorsPatience!.RecordMovement(); // Movement breaks stance

        // Act
        var bonus = _service.GetPredatorsPatienceBonus(player);

        // Assert
        bonus.Should().Be(0);
    }

    // ===== Tier 2 Readiness & PP Tests (v0.20.7b) =====

    [Test]
    public void GetAbilityReadiness_IncludesTier2Abilities()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        player.CurrentAP = 2; // Enough for all T2 actives

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(VeidimadurAbilityId.HuntersEye)
            .WhoseValue.Should().BeTrue(); // Passive — always ready
        readiness.Should().ContainKey(VeidimadurAbilityId.TrapMastery)
            .WhoseValue.Should().BeTrue(); // 2 AP available
        readiness.Should().ContainKey(VeidimadurAbilityId.PredatorsPatience)
            .WhoseValue.Should().BeTrue(); // 1 AP available (2 >= 1)
    }

    [Test]
    public void GetAbilityReadiness_Tier2InsufficientAP_ActiveAbilitiesNotReady()
    {
        // Arrange
        var player = CreateTier2Veidimadur();
        player.CurrentAP = 0;

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness[VeidimadurAbilityId.HuntersEye].Should().BeTrue(); // Passive
        readiness[VeidimadurAbilityId.TrapMastery].Should().BeFalse(); // Needs 2 AP
        readiness[VeidimadurAbilityId.PredatorsPatience].Should().BeFalse(); // Needs 1 AP
    }

    [Test]
    public void GetPPInvested_WithTier2Abilities_Returns12()
    {
        // Arrange
        var player = CreateTier2Veidimadur();

        // Act
        var ppInvested = _service.GetPPInvested(player);

        // Assert
        ppInvested.Should().Be(12); // T1: 3×0 = 0, T2: 3×4 = 12 → total 12
    }

    [Test]
    public void CanUnlockTier2_WithAllTier2_ReturnsTrue()
    {
        // Arrange
        var player = CreateTier2Veidimadur();

        // Act
        var canUnlock = _service.CanUnlockTier2(player);

        // Assert
        canUnlock.Should().BeTrue(); // 12 PP >= 8 PP requirement
    }
}
