using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="StealthService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Surface type DC calculation</description></item>
///   <item><description>[Hidden] status tracking</description></item>
///   <item><description>Party stealth weakest-link rule</description></item>
///   <item><description>Fumble triggering [System-Wide Alert]</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class StealthServiceTests
{
    private IDiceService _diceService = null!;
    private ILogger<StealthService> _logger = null!;
    private StealthService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _diceService = Substitute.For<IDiceService>();
        _logger = Substitute.For<ILogger<StealthService>>();
        _sut = new StealthService(_diceService, _logger);
    }

    #region Surface DC Tests

    [Test]
    public void GetStealthDc_SilentSurface_ReturnsDc2()
    {
        // Act
        var dc = _sut.GetStealthDc(StealthSurface.Silent);

        // Assert
        dc.Should().Be(2);
    }

    [Test]
    public void GetStealthDc_NormalSurface_ReturnsDc3()
    {
        // Act
        var dc = _sut.GetStealthDc(StealthSurface.Normal);

        // Assert
        dc.Should().Be(3);
    }

    [Test]
    public void GetStealthDc_NoisySurface_ReturnsDc4()
    {
        // Act
        var dc = _sut.GetStealthDc(StealthSurface.Noisy);

        // Assert
        dc.Should().Be(4);
    }

    [Test]
    public void GetStealthDc_VeryNoisySurface_ReturnsDc5()
    {
        // Act
        var dc = _sut.GetStealthDc(StealthSurface.VeryNoisy);

        // Assert
        dc.Should().Be(5);
    }

    [Test]
    public void GetStealthDc_WithDimLight_ReducesDcBy1()
    {
        // Act
        var dc = _sut.GetStealthDc(StealthSurface.Normal, isDimLight: true);

        // Assert
        dc.Should().Be(2); // 3 - 1 = 2
    }

    [Test]
    public void GetStealthDc_WithIllumination_IncreasesDcBy2()
    {
        // Act
        var dc = _sut.GetStealthDc(StealthSurface.Normal, isIlluminated: true);

        // Assert
        dc.Should().Be(5); // 3 + 2 = 5
    }

    [Test]
    public void GetStealthDc_WithEnemiesAlerted_IncreasesDcBy1()
    {
        // Act
        var dc = _sut.GetStealthDc(StealthSurface.Normal, enemiesAlerted: true);

        // Assert
        dc.Should().Be(4); // 3 + 1 = 4
    }

    [Test]
    public void GetStealthDc_MinimumDcIs1()
    {
        // Arrange - Silent (DC 2) with dim light (-1) and psychic resonance (-2)
        var context = StealthContext.CreateWithModifiers(
            StealthSurface.Silent,
            isDimLight: true,
            inPsychicResonance: true);

        // Act
        var dc = context.EffectiveDc;

        // Assert - 2 - 1 - 2 = -1, but minimum is 1
        dc.Should().Be(1);
    }

    #endregion

    #region Individual Stealth Tests

    [Test]
    public void AttemptStealth_WithSuccessfulRoll_ReturnsSuccess()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal); // DC 3
        SetupDiceRoll(successes: 4, botches: 0); // 4 successes vs DC 3

        // Act
        var result = _sut.AttemptStealth("player-1", context, dicePool: 5);

        // Assert
        result.BecameHidden.Should().BeTrue();
        result.Succeeded.Should().BeTrue();
        result.Margin.Should().Be(1); // 4 - 3 = 1
    }

    [Test]
    public void AttemptStealth_WithFailedRoll_ReturnsFailure()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal); // DC 3
        SetupDiceRoll(successes: 2, botches: 0); // 2 successes vs DC 3

        // Act
        var result = _sut.AttemptStealth("player-1", context, dicePool: 5);

        // Assert
        result.BecameHidden.Should().BeFalse();
        result.Succeeded.Should().BeFalse();
        result.WasDetected.Should().BeTrue();
        result.Margin.Should().Be(-1); // 2 - 3 = -1
    }

    [Test]
    public void AttemptStealth_WithFumble_TriggersFumbleConsequence()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal);
        SetupFumbleRoll(); // 0 successes, 1 botch = fumble

        // Act
        var result = _sut.AttemptStealth("player-1", context, dicePool: 5);

        // Assert
        result.FumbleTriggered.Should().BeTrue();
        result.BecameHidden.Should().BeFalse();
        result.Outcome.Should().Be(SkillOutcome.CriticalFailure);
    }

    [Test]
    public void AttemptStealth_CriticalSuccess_GrantsEnhancedDetectionModifier()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal); // DC 3
        SetupDiceRoll(successes: 8, botches: 0); // 8 successes = margin 5 = critical

        // Act
        var result = _sut.AttemptStealth("player-1", context, dicePool: 10);

        // Assert
        result.IsCritical.Should().BeTrue();
        result.Outcome.Should().Be(SkillOutcome.CriticalSuccess);
        result.DetectionModifier.Should().Be(2);
    }

    [Test]
    public void AttemptStealth_MarginalSuccess_EnemiesSuspicious()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal); // DC 3
        SetupDiceRoll(successes: 3, botches: 0); // Exactly DC = margin 0

        // Act
        var result = _sut.AttemptStealth("player-1", context, dicePool: 5);

        // Assert
        result.BecameHidden.Should().BeTrue();
        result.EnemiesSuspicious.Should().BeTrue();
        result.Margin.Should().Be(0);
    }

    #endregion

    #region Hidden Status Tests

    [Test]
    public void IsHidden_AfterSuccessfulStealth_ReturnsTrue()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal);
        SetupDiceRoll(successes: 5, botches: 0);
        _sut.AttemptStealth("player-1", context, dicePool: 5);

        // Act
        var isHidden = _sut.IsHidden("player-1");

        // Assert
        isHidden.Should().BeTrue();
    }

    [Test]
    public void IsHidden_AfterFailedStealth_ReturnsFalse()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal);
        SetupDiceRoll(successes: 1, botches: 0);
        _sut.AttemptStealth("player-1", context, dicePool: 5);

        // Act
        var isHidden = _sut.IsHidden("player-1");

        // Assert
        isHidden.Should().BeFalse();
    }

    [Test]
    public void BreakHidden_WithAttackCondition_BreaksHiddenStatus()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal);
        SetupDiceRoll(successes: 5, botches: 0);
        _sut.AttemptStealth("player-1", context, dicePool: 5);

        // Act
        var broke = _sut.BreakHidden("player-1", HiddenBreakCondition.Attack);

        // Assert
        broke.Should().BeTrue();
        _sut.IsHidden("player-1").Should().BeFalse();
    }

    [Test]
    public void BreakHidden_WhenNotHidden_ReturnsFalse()
    {
        // Act
        var broke = _sut.BreakHidden("player-1", HiddenBreakCondition.Attack);

        // Assert
        broke.Should().BeFalse();
    }

    [Test]
    public void GetHiddenStatus_AfterSuccessfulStealth_ReturnsStatus()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal);
        SetupDiceRoll(successes: 5, botches: 0);
        _sut.AttemptStealth("player-1", context, dicePool: 5);

        // Act
        var status = _sut.GetHiddenStatus("player-1");

        // Assert
        status.Should().NotBeNull();
        status!.IsHidden.Should().BeTrue();
        status.CharacterId.Should().Be("player-1");
    }

    [Test]
    public void ApplyHiddenStatus_ViaSlipIntoShadow_AppliesEnhancedModifier()
    {
        // Act
        var status = _sut.ApplyHiddenStatus("player-1", "SlipIntoShadow");

        // Assert
        status.IsHidden.Should().BeTrue();
        status.DetectionModifier.Should().Be(1);
        status.Source.Should().Be("SlipIntoShadow");
        _sut.IsHidden("player-1").Should().BeTrue();
    }

    [Test]
    public void ApplyHiddenStatus_ViaOneWithTheStatic_AppliesZoneCondition()
    {
        // Act
        var status = _sut.ApplyHiddenStatus("player-1", "OneWithTheStatic");

        // Assert
        status.IsHidden.Should().BeTrue();
        status.DetectionModifier.Should().Be(2);
        status.BreakConditions.Should().Contain(HiddenBreakCondition.LeaveZone);
    }

    [Test]
    public void ClearAllHiddenStatuses_ClearsAllStatuses()
    {
        // Arrange
        var context = StealthContext.Create(StealthSurface.Normal);
        SetupDiceRoll(successes: 5, botches: 0);
        _sut.AttemptStealth("player-1", context, dicePool: 5);
        _sut.AttemptStealth("player-2", context, dicePool: 5);

        // Act
        _sut.ClearAllHiddenStatuses();

        // Assert
        _sut.IsHidden("player-1").Should().BeFalse();
        _sut.IsHidden("player-2").Should().BeFalse();
    }

    #endregion

    #region Party Stealth Tests

    [Test]
    public void FindWeakestMember_ReturnsLowestPool()
    {
        // Arrange
        var pools = new Dictionary<string, int>
        {
            { "kira", 7 },
            { "theron", 4 },
            { "elara", 6 }
        };

        // Act
        var (weakestId, weakestPool) = _sut.FindWeakestMember(pools);

        // Assert
        weakestId.Should().Be("theron");
        weakestPool.Should().Be(4);
    }

    [Test]
    public void AttemptPartyStealth_UsesWeakestMemberPool()
    {
        // Arrange
        var pools = new Dictionary<string, int>
        {
            { "kira", 7 },
            { "theron", 4 },
            { "elara", 6 }
        };
        var context = StealthContext.Create(StealthSurface.Normal); // DC 3

        // Set up dice service to capture the pool used
        DicePool? capturedPool = null;
        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>())
            .Returns(callInfo =>
            {
                capturedPool = callInfo.Arg<DicePool>();
                return new DiceRollResult(DicePool.D10(4), new[] { 8, 5, 9, 3 }); // 2 successes
            });

        // Act
        var result = _sut.AttemptPartyStealth(pools, context);

        // Assert
        result.WeakestMemberId.Should().Be("theron");
        result.WeakestPool.Should().Be(4);
        capturedPool.Should().NotBeNull();
        capturedPool!.Value.Count.Should().Be(4); // Theron's pool of 4
    }

    [Test]
    public void AttemptPartyStealth_Success_HidesAllMembers()
    {
        // Arrange
        var pools = new Dictionary<string, int>
        {
            { "kira", 7 },
            { "theron", 4 },
            { "elara", 6 }
        };
        var context = StealthContext.Create(StealthSurface.Normal); // DC 3
        SetupDiceRoll(successes: 4, botches: 0); // Success

        // Act
        var result = _sut.AttemptPartyStealth(pools, context);

        // Assert
        result.PartyHidden.Should().BeTrue();
        _sut.IsHidden("kira").Should().BeTrue();
        _sut.IsHidden("theron").Should().BeTrue();
        _sut.IsHidden("elara").Should().BeTrue();
    }

    [Test]
    public void AttemptPartyStealth_Failure_ExposesAllMembers()
    {
        // Arrange
        var pools = new Dictionary<string, int>
        {
            { "kira", 7 },
            { "theron", 4 },
            { "elara", 6 }
        };
        var context = StealthContext.Create(StealthSurface.Normal);
        SetupDiceRoll(successes: 1, botches: 0); // Failure

        // Act
        var result = _sut.AttemptPartyStealth(pools, context);

        // Assert
        result.PartyHidden.Should().BeFalse();
        result.WasDetected.Should().BeTrue();
        _sut.IsHidden("kira").Should().BeFalse();
        _sut.IsHidden("theron").Should().BeFalse();
        _sut.IsHidden("elara").Should().BeFalse();
    }

    [Test]
    public void AttemptPartyStealth_Fumble_TriggersSystemWideAlert()
    {
        // Arrange
        var pools = new Dictionary<string, int>
        {
            { "kira", 7 },
            { "theron", 4 }
        };
        var context = StealthContext.Create(StealthSurface.Normal);
        SetupFumbleRoll();

        // Act
        var result = _sut.AttemptPartyStealth(pools, context);

        // Assert
        result.FumbleTriggered.Should().BeTrue();
        result.PartyHidden.Should().BeFalse();
        result.Outcome.Should().Be(SkillOutcome.CriticalFailure);
    }

    [Test]
    public void AttemptPartyStealth_EmptyParty_ThrowsException()
    {
        // Arrange
        var pools = new Dictionary<string, int>();
        var context = StealthContext.Create(StealthSurface.Normal);

        // Act & Assert
        var act = () => _sut.AttemptPartyStealth(pools, context);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one member*");
    }

    #endregion

    #region StealthContext Tests

    [Test]
    public void StealthContext_Create_UsesCorrectBaseDc()
    {
        // Act
        var context = StealthContext.Create(StealthSurface.Noisy);

        // Assert
        context.BaseDc.Should().Be(4);
        context.EffectiveDc.Should().Be(4);
        context.SurfaceType.Should().Be(StealthSurface.Noisy);
    }

    [Test]
    public void StealthContext_CreateWithModifiers_CalculatesCorrectDc()
    {
        // Act
        var context = StealthContext.CreateWithModifiers(
            StealthSurface.Normal,
            isDimLight: true,
            enemiesAlerted: true,
            inPsychicResonance: true);

        // Assert
        // Base 3, dim light -1, alerted +1, psychic resonance -2 = 1
        context.EffectiveDc.Should().Be(1);
        context.HasFavorableLighting.Should().BeTrue();
        context.EnemiesAlerted.Should().BeTrue();
        context.InBeneficialZone.Should().BeTrue();
    }

    [Test]
    public void StealthContext_CreatePartyCheck_SetsPartyFlag()
    {
        // Arrange
        var members = new List<string> { "kira", "theron", "elara" };

        // Act
        var context = StealthContext.CreatePartyCheck(StealthSurface.Normal, members);

        // Assert
        context.IsPartyCheck.Should().BeTrue();
        context.PartyMemberIds.Should().BeEquivalentTo(members);
    }

    #endregion

    #region Helper Methods

    private void SetupDiceRoll(int successes, int botches)
    {
        // Create individual values that produce the expected successes/botches
        var values = new List<int>();
        for (var i = 0; i < successes; i++)
            values.Add(8); // success value
        for (var i = 0; i < botches; i++)
            values.Add(1); // botch value

        // Fill remaining with neutral values
        while (values.Count < 5)
            values.Add(5);

        var result = new DiceRollResult(DicePool.D10(values.Count), values.ToArray());

        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>())
            .Returns(result);
    }

    private void SetupFumbleRoll()
    {
        // Fumble = 0 successes, at least 1 botch
        var result = new DiceRollResult(DicePool.D10(5), new[] { 1, 3, 4, 2, 5 });

        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>())
            .Returns(result);
    }

    #endregion
}
