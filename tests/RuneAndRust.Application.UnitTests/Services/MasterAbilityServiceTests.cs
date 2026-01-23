namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="MasterAbilityService"/>.
/// </summary>
[TestFixture]
public class MasterAbilityServiceTests
{
    private Mock<IMasterAbilityProvider> _mockProvider = null!;
    private Mock<ILogger<MasterAbilityService>> _mockLogger = null!;
    private MasterAbilityService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockProvider = new Mock<IMasterAbilityProvider>();
        _mockLogger = new Mock<ILogger<MasterAbilityService>>();
        _service = new MasterAbilityService(_mockProvider.Object, _mockLogger.Object);
    }

    private Player CreateMasterPlayer(string skillId)
    {
        var player = new Player("TestPlayer");
        var skill = PlayerSkill.Create(skillId, player.Id, SkillProficiency.Master);
        player.AddSkill(skill);
        return player;
    }

    private Player CreateNonMasterPlayer(string skillId)
    {
        var player = new Player("TestPlayer");
        var skill = PlayerSkill.Create(skillId, player.Id, SkillProficiency.Expert);
        player.AddSkill(skill);
        return player;
    }

    // ===== EvaluateForCheck Tests =====

    [Test]
    public void EvaluateForCheck_WhenPlayerNotMaster_ReturnsNone()
    {
        // Arrange
        var player = CreateNonMasterPlayer("acrobatics");

        // Act
        var result = _service.EvaluateForCheck(player, "acrobatics", null, 15);

        // Assert
        result.HasActiveAbilities.Should().BeFalse();
        result.ShouldAutoSucceed.Should().BeFalse();
    }

    [Test]
    public void EvaluateForCheck_WithAutoSucceedAbility_ReturnsAutoSucceed()
    {
        // Arrange
        var player = CreateMasterPlayer("acrobatics");
        var ability = MasterAbility.Create(
            abilityId: "spider-climb",
            skillId: "acrobatics",
            name: "Spider Climb",
            description: "Auto-succeed climbing DC 12 or less",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(12),
            subTypes: new[] { "climbing" });

        _mockProvider.Setup(p => p.GetAbilitiesForSkillAndSubType("acrobatics", "climbing"))
            .Returns(new List<MasterAbility> { ability });

        // Act
        var result = _service.EvaluateForCheck(player, "acrobatics", "climbing", 10);

        // Assert
        result.ShouldAutoSucceed.Should().BeTrue();
        result.AutoSucceedAbility.Should().Be(ability);
    }

    [Test]
    public void EvaluateForCheck_WithDiceBonusAbility_ReturnsTotalBonus()
    {
        // Arrange
        var player = CreateMasterPlayer("rhetoric");
        var ability = MasterAbility.Create(
            abilityId: "fearsome-reputation",
            skillId: "rhetoric",
            name: "Fearsome Reputation",
            description: "+2d10 to intimidation",
            abilityType: MasterAbilityType.DiceBonus,
            effect: MasterAbilityEffect.ForDiceBonus(2),
            subTypes: new[] { "intimidation" });

        _mockProvider.Setup(p => p.GetAbilitiesForSkillAndSubType("rhetoric", "intimidation"))
            .Returns(new List<MasterAbility> { ability });

        // Act
        var result = _service.EvaluateForCheck(player, "rhetoric", "intimidation", 15);

        // Assert
        result.ShouldAutoSucceed.Should().BeFalse();
        result.TotalDiceBonus.Should().Be(2);
        result.DiceBonusAbilities.Should().Contain(ability);
    }

    [Test]
    public void EvaluateForCheck_WhenDCAboveThreshold_DoesNotAutoSucceed()
    {
        // Arrange
        var player = CreateMasterPlayer("acrobatics");
        var ability = MasterAbility.Create(
            abilityId: "spider-climb",
            skillId: "acrobatics",
            name: "Spider Climb",
            description: "Auto-succeed climbing DC 12 or less",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(12),
            subTypes: new[] { "climbing" });

        _mockProvider.Setup(p => p.GetAbilitiesForSkillAndSubType("acrobatics", "climbing"))
            .Returns(new List<MasterAbility> { ability });

        // Act
        var result = _service.EvaluateForCheck(player, "acrobatics", "climbing", 15);

        // Assert
        result.ShouldAutoSucceed.Should().BeFalse();
    }

    // ===== GetApplicableAbilities Tests =====

    [Test]
    public void GetApplicableAbilities_WhenPlayerNotMaster_ReturnsEmpty()
    {
        // Arrange
        var player = CreateNonMasterPlayer("acrobatics");

        // Act
        var result = _service.GetApplicableAbilities(player, "acrobatics", null);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetApplicableAbilities_WhenPlayerIsMaster_ReturnsAbilities()
    {
        // Arrange
        var player = CreateMasterPlayer("acrobatics");
        var abilities = new List<MasterAbility>
        {
            MasterAbility.Create(
                abilityId: "spider-climb",
                skillId: "acrobatics",
                name: "Spider Climb",
                description: "Test",
                abilityType: MasterAbilityType.AutoSucceed,
                effect: MasterAbilityEffect.ForAutoSucceed(12))
        };

        _mockProvider.Setup(p => p.GetAbilitiesForSkillAndSubType("acrobatics", null))
            .Returns(abilities);

        // Act
        var result = _service.GetApplicableAbilities(player, "acrobatics", null);

        // Assert
        result.Should().HaveCount(1);
    }

    // ===== Re-roll Tests =====

    [Test]
    public void CanUseReroll_WhenNeverUsed_ReturnsTrue()
    {
        // Arrange
        var player = CreateMasterPlayer("rhetoric");

        // Act
        var result = _service.CanUseReroll(player, "master-negotiator");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CanUseReroll_AfterUsed_ReturnsFalse()
    {
        // Arrange
        var player = CreateMasterPlayer("rhetoric");
        var ability = MasterAbility.Create(
            abilityId: "master-negotiator",
            skillId: "rhetoric",
            name: "Master Negotiator",
            description: "Re-roll failed negotiation",
            abilityType: MasterAbilityType.RerollFailure,
            effect: MasterAbilityEffect.ForRerollFailure(RerollPeriod.Conversation));

        _mockProvider.Setup(p => p.GetAbilityById("master-negotiator"))
            .Returns(ability);

        _service.UseReroll(player, "master-negotiator");

        // Act
        var result = _service.CanUseReroll(player, "master-negotiator");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void ResetRerollsForPeriod_ResetsMatchingAbilities()
    {
        // Arrange
        var player = CreateMasterPlayer("rhetoric");
        var ability = MasterAbility.Create(
            abilityId: "master-negotiator",
            skillId: "rhetoric",
            name: "Master Negotiator",
            description: "Re-roll failed negotiation",
            abilityType: MasterAbilityType.RerollFailure,
            effect: MasterAbilityEffect.ForRerollFailure(RerollPeriod.Conversation));

        _mockProvider.Setup(p => p.GetAbilityById("master-negotiator"))
            .Returns(ability);
        _mockProvider.Setup(p => p.GetAllAbilities())
            .Returns(new List<MasterAbility> { ability });

        _service.UseReroll(player, "master-negotiator");
        _service.CanUseReroll(player, "master-negotiator").Should().BeFalse();

        // Act
        _service.ResetRerollsForPeriod(player, RerollPeriod.Conversation);

        // Assert
        _service.CanUseReroll(player, "master-negotiator").Should().BeTrue();
    }
}
