namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SpecializationBonusProvider"/>.
/// </summary>
[TestFixture]
public class SpecializationBonusProviderTests
{
    private Mock<ILogger<SpecializationBonusProvider>> _loggerMock = null!;
    private SpecializationBonusProvider _provider = null!;
    private string _configPath = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<SpecializationBonusProvider>>();
        _configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "config", "specialization-bonuses.json");

        // Resolve to absolute path
        _configPath = Path.GetFullPath(_configPath);

        _provider = new SpecializationBonusProvider(_configPath, _loggerMock.Object);
    }

    [Test]
    public void GetSkillBonus_UnknownSpecialization_ReturnsNone()
    {
        // Act
        var bonus = _provider.GetSkillBonus("unknown-spec", "climbing", CreateEmptyContext());

        // Assert
        bonus.HasEffect.Should().BeFalse();
        bonus.ShouldApply.Should().BeFalse();
    }

    [Test]
    public void GetSkillBonus_KnownSpecWithBonus_ReturnsBonus()
    {
        // Act
        var bonus = _provider.GetSkillBonus("gantry-runner", "climbing", CreateEmptyContext());

        // Assert
        bonus.DiceBonus.Should().Be(2);
        bonus.HasEffect.Should().BeTrue();
        bonus.ShouldApply.Should().BeTrue();
        bonus.Description.Should().Contain("Heights");
    }

    [Test]
    public void GetSkillBonus_SpecWithoutSkillMatch_ReturnsNone()
    {
        // Act
        var bonus = _provider.GetSkillBonus("gantry-runner", "persuasion", CreateEmptyContext());

        // Assert
        bonus.HasEffect.Should().BeFalse();
    }

    [Test]
    public void HasBonusForSkill_ReturnsTrueForConfiguredCombination()
    {
        // Assert
        _provider.HasBonusForSkill("gantry-runner", "climbing").Should().BeTrue();
        _provider.HasBonusForSkill("gantry-runner", "persuasion").Should().BeFalse();
    }

    [Test]
    public void GetAllBonuses_ReturnsAllBonusesForSpec()
    {
        // Act
        var bonuses = _provider.GetAllBonuses("gantry-runner");

        // Assert
        bonuses.Should().HaveCountGreaterOrEqualTo(2);
        bonuses.Should().Contain(b => b.SkillId == "climbing");
        bonuses.Should().Contain(b => b.SkillId == "leaping");
    }

    [Test]
    public void GetSpecializationsWithBonuses_ReturnsAllSpecIds()
    {
        // Act
        var specs = _provider.GetSpecializationsWithBonuses();

        // Assert
        specs.Should().HaveCountGreaterOrEqualTo(5);
        specs.Should().Contain("gantry-runner");
        specs.Should().Contain("thul");
    }

    [Test]
    public void GetSkillBonus_ThulPersuasion_HasSpecialAbility()
    {
        // Act
        var bonus = _provider.GetSkillBonus("thul", "persuasion", CreateEmptyContext());

        // Assert
        bonus.HasSpecialAbility.Should().BeTrue();
        bonus.SpecialAbility.Should().Be("no-reputation-loss-on-failure");
    }

    private static SkillContext CreateEmptyContext()
    {
        return new SkillContext(
            equipmentModifiers: Array.Empty<EquipmentModifier>(),
            situationalModifiers: Array.Empty<SituationalModifier>(),
            environmentModifiers: Array.Empty<EnvironmentModifier>(),
            targetModifiers: Array.Empty<TargetModifier>(),
            appliedStatuses: Array.Empty<string>());
    }
}
