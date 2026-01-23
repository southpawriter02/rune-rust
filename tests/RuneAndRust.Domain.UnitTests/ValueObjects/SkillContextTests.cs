using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for SkillContext and modifier value objects.
/// </summary>
/// <remarks>
/// v0.15.1a: Tests for skill context modifiers including equipment, situational,
/// environment, and target modifiers.
/// </remarks>
[TestFixture]
public class SkillContextTests
{
    #region TotalDiceModifier Tests

    [Test]
    public void TotalDiceModifier_SumsAllCategories()
    {
        // Arrange
        var equipment = new List<EquipmentModifier>
        {
            EquipmentModifier.Tool("toolkit", "Toolkit", diceBonus: 2)
        };
        var situational = new List<SituationalModifier>
        {
            SituationalModifier.TimePressure()  // -1d10
        };
        var environment = new List<EnvironmentModifier>
        {
            EnvironmentModifier.FromSurface(SurfaceType.Stable)  // +1d10
        };
        var target = new List<TargetModifier>();

        // Act
        var context = new SkillContext(
            equipment, situational, environment, target,
            Array.Empty<string>());

        // Assert: 2 - 1 + 1 = 2
        context.TotalDiceModifier.Should().Be(2);
    }

    [Test]
    public void TotalDcModifier_SumsAllCategories()
    {
        // Arrange
        var equipment = new List<EquipmentModifier>();
        var situational = new List<SituationalModifier>();
        var environment = new List<EnvironmentModifier>
        {
            EnvironmentModifier.FromLighting(LightingLevel.Dim),       // DC +1
            EnvironmentModifier.FromCorruption(CorruptionTier.Glitched) // DC +2
        };
        var target = new List<TargetModifier>
        {
            TargetModifier.FromSuspicion(5)  // DC +2
        };

        // Act
        var context = new SkillContext(
            equipment, situational, environment, target,
            Array.Empty<string>());

        // Assert: 1 + 2 + 2 = 5
        context.TotalDcModifier.Should().Be(5);
    }

    #endregion

    #region Empty Context Tests

    [Test]
    public void Empty_HasNoModifiers()
    {
        // Act
        var context = SkillContext.Empty;

        // Assert
        context.HasModifiers.Should().BeFalse();
        context.TotalDiceModifier.Should().Be(0);
        context.TotalDcModifier.Should().Be(0);
        context.ModifierCount.Should().Be(0);
    }

    #endregion

    #region EquipmentModifier Tests

    [Test]
    public void EquipmentModifier_ToShortDescription_FormatsCorrectly()
    {
        // Arrange
        var modifier = EquipmentModifier.Tool("toolkit", "Tinker's Toolkit", diceBonus: 2);

        // Act
        var description = modifier.ToShortDescription();

        // Assert
        description.Should().Contain("Tinker's Toolkit");
        description.Should().Contain("+2d10");
    }

    [Test]
    public void EquipmentModifier_Category_IsEquipment()
    {
        // Arrange
        var modifier = EquipmentModifier.Tool("toolkit", "Toolkit", diceBonus: 1);

        // Assert
        modifier.Category.Should().Be(ModifierCategory.Equipment);
    }

    #endregion

    #region SituationalModifier Tests

    [Test]
    public void SituationalModifier_NonStackable_HasCorrectProperties()
    {
        // Arrange
        var mod1 = SituationalModifier.TimePressure("Source 1");
        var mod2 = SituationalModifier.TimePressure("Source 2");

        // Assert: Both have same ModifierId and IsStackable = false
        mod1.ModifierId.Should().Be(mod2.ModifierId);
        mod1.IsStackable.Should().BeFalse();
        mod1.DiceModifier.Should().Be(-1);
    }

    [Test]
    public void SituationalModifier_TakingTime_GivesBonusDice()
    {
        // Arrange & Act
        var modifier = SituationalModifier.TakingTime();

        // Assert
        modifier.DiceModifier.Should().Be(1);
        modifier.Category.Should().Be(ModifierCategory.Situational);
    }

    #endregion

    #region EnvironmentModifier Tests

    [Test]
    public void EnvironmentModifier_FromSurface_CreatesCorrectModifier()
    {
        // Act
        var stable = EnvironmentModifier.FromSurface(SurfaceType.Stable);
        var compromised = EnvironmentModifier.FromSurface(SurfaceType.Compromised);

        // Assert
        stable.DiceModifier.Should().Be(1);
        compromised.DiceModifier.Should().Be(-2);
        stable.Category.Should().Be(ModifierCategory.Environment);
    }

    [Test]
    public void EnvironmentModifier_FromLighting_CreatesCorrectModifier()
    {
        // Act
        var bright = EnvironmentModifier.FromLighting(LightingLevel.Bright);
        var dim = EnvironmentModifier.FromLighting(LightingLevel.Dim);
        var dark = EnvironmentModifier.FromLighting(LightingLevel.Dark);

        // Assert
        bright.DcModifier.Should().Be(-1);
        dim.DcModifier.Should().Be(1);
        dark.DcModifier.Should().Be(2);
    }

    [Test]
    public void EnvironmentModifier_FromCorruption_CreatesCorrectModifier()
    {
        // Act
        var normal = EnvironmentModifier.FromCorruption(CorruptionTier.Normal);
        var glitched = EnvironmentModifier.FromCorruption(CorruptionTier.Glitched);
        var resonance = EnvironmentModifier.FromCorruption(CorruptionTier.Resonance);

        // Assert
        normal.DcModifier.Should().Be(0);
        glitched.DcModifier.Should().Be(2);
        resonance.DcModifier.Should().Be(6);
    }

    #endregion

    #region TargetModifier Tests

    [Test]
    public void TargetModifier_FromDisposition_CreatesCorrectModifier()
    {
        // Act
        var friendly = TargetModifier.FromDisposition(Disposition.Friendly);
        var hostile = TargetModifier.FromDisposition(Disposition.Hostile);

        // Assert
        friendly.DiceModifier.Should().Be(2);
        hostile.DiceModifier.Should().Be(-2);
        friendly.Category.Should().Be(ModifierCategory.Target);
    }

    [Test]
    public void TargetModifier_FromSuspicion_CreatesCorrectModifier()
    {
        // Arrange & Act
        var trusting = TargetModifier.FromSuspicion(0);
        var wary = TargetModifier.FromSuspicion(5);
        var extremelySuspicious = TargetModifier.FromSuspicion(10);

        // Assert
        trusting.DcModifier.Should().Be(0);
        wary.DcModifier.Should().Be(2);
        extremelySuspicious.DcModifier.Should().Be(6);
    }

    #endregion

    #region ToDescription Tests

    [Test]
    public void ToDescription_FormatsAllModifiers()
    {
        // Arrange
        var equipment = new List<EquipmentModifier>
        {
            EquipmentModifier.Tool("toolkit", "Toolkit", diceBonus: 2)
        };
        var situational = new List<SituationalModifier>();
        var environment = new List<EnvironmentModifier>
        {
            EnvironmentModifier.FromLighting(LightingLevel.Dim)
        };
        var target = new List<TargetModifier>();

        var context = new SkillContext(
            equipment, situational, environment, target,
            Array.Empty<string>());

        // Act
        var description = context.ToDescription();

        // Assert
        description.Should().Contain("Equipment:");
        description.Should().Contain("Toolkit");
        description.Should().Contain("Environment:");
        description.Should().Contain("Dim Lighting");
        description.Should().Contain("Total:");
    }

    [Test]
    public void ToString_FormatsCompactSummary()
    {
        // Arrange
        var equipment = new List<EquipmentModifier>
        {
            EquipmentModifier.Tool("toolkit", "Toolkit", diceBonus: 2)
        };
        var situational = new List<SituationalModifier>
        {
            SituationalModifier.TimePressure()
        };
        var environment = new List<EnvironmentModifier>();
        var target = new List<TargetModifier>();

        var context = new SkillContext(
            equipment, situational, environment, target,
            Array.Empty<string>());

        // Act
        var summary = context.ToString();

        // Assert: +1d10 (2 - 1), 2 modifiers
        summary.Should().Contain("+1d10");
        summary.Should().Contain("2 modifiers");
    }

    #endregion

    #region GetAllModifiers Tests

    [Test]
    public void GetAllModifiers_ReturnsAllModifiers()
    {
        // Arrange
        var equipment = new List<EquipmentModifier>
        {
            EquipmentModifier.Tool("toolkit", "Toolkit", diceBonus: 1)
        };
        var situational = new List<SituationalModifier>
        {
            SituationalModifier.TakingTime()
        };
        var environment = new List<EnvironmentModifier>
        {
            EnvironmentModifier.FromSurface(SurfaceType.Stable)
        };
        var target = new List<TargetModifier>
        {
            TargetModifier.FromDisposition(Disposition.Friendly)
        };

        var context = new SkillContext(
            equipment, situational, environment, target,
            Array.Empty<string>());

        // Act
        var allModifiers = context.GetAllModifiers().ToList();

        // Assert
        allModifiers.Should().HaveCount(4);
    }

    #endregion
}
