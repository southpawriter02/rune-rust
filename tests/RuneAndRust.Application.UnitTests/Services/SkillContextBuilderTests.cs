using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for SkillContextBuilder.
/// </summary>
/// <remarks>
/// v0.15.1a: Tests for the fluent skill context builder including
/// modifier addition, duplicate handling, and reset behavior.
/// </remarks>
[TestFixture]
public class SkillContextBuilderTests
{
    private Mock<ILogger<SkillContextBuilder>> _mockLogger = null!;
    private SkillContextBuilder _builder = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<SkillContextBuilder>>();
        _builder = new SkillContextBuilder(_mockLogger.Object);
    }

    #region Build Tests

    [Test]
    public void Build_CreatesContextWithAllModifiers()
    {
        // Act
        var context = _builder
            .WithEquipment("toolkit", "Toolkit", diceModifier: 2)
            .WithSituation("time-pressure", "Time Pressure", diceModifier: -1)
            .WithEnvironment(SurfaceType.Stable, LightingLevel.Dim)
            .WithTargetDisposition(Disposition.Friendly)
            .Build();

        // Assert
        context.EquipmentModifiers.Should().HaveCount(1);
        context.SituationalModifiers.Should().HaveCount(1);
        context.EnvironmentModifiers.Should().HaveCount(2); // Surface + Lighting
        context.TargetModifiers.Should().HaveCount(1);
        context.HasModifiers.Should().BeTrue();
    }

    [Test]
    public void Build_ResetsBuilderAutomatically()
    {
        // Arrange: Build first context
        _builder.WithEquipment("toolkit", "Toolkit", diceModifier: 2);
        var context1 = _builder.Build();

        // Act: Build second context without re-adding
        var context2 = _builder.Build();

        // Assert
        context1.EquipmentModifiers.Should().HaveCount(1);
        context2.EquipmentModifiers.Should().HaveCount(0); // Empty after auto-reset
    }

    [Test]
    public void Build_CalculatesTotalsCorrectly()
    {
        // Arrange & Act
        var context = _builder
            .WithEquipment("toolkit", "Toolkit", diceModifier: 2)
            .WithSituation("familiar", "Familiarity", diceModifier: 1)
            .WithEnvironment(lighting: LightingLevel.Dim)  // DC +1
            .WithCorruption(CorruptionTier.Glitched)       // DC +2
            .Build();

        // Assert
        context.TotalDiceModifier.Should().Be(3);  // 2 + 1
        context.TotalDcModifier.Should().Be(3);    // 1 + 2
    }

    #endregion

    #region Situational Modifier Duplicate Prevention

    [Test]
    public void WithSituation_PreventsDuplicateNonStackable()
    {
        // Act
        var context = _builder
            .WithSituation(SituationalModifier.TimePressure("Source 1"))
            .WithSituation(SituationalModifier.TimePressure("Source 2"))
            .Build();

        // Assert: Only one should be added
        context.SituationalModifiers.Should().HaveCount(1);
    }

    [Test]
    public void WithSituation_AllowsDifferentModifiers()
    {
        // Act
        var context = _builder
            .WithSituation(SituationalModifier.TimePressure())
            .WithSituation(SituationalModifier.TakingTime())
            .Build();

        // Assert: Both should be added (different ModifierId)
        context.SituationalModifiers.Should().HaveCount(2);
    }

    #endregion

    #region Environment Modifier Replacement

    [Test]
    public void WithEnvironment_ReplacesSameTypeSurface()
    {
        // Act
        var context = _builder
            .WithEnvironment(EnvironmentModifier.FromSurface(SurfaceType.Stable))
            .WithEnvironment(EnvironmentModifier.FromSurface(SurfaceType.Compromised))
            .Build();

        // Assert: Only one surface modifier (latest)
        context.EnvironmentModifiers.Should().HaveCount(1);
        context.EnvironmentModifiers[0].SurfaceType.Should().Be(SurfaceType.Compromised);
    }

    [Test]
    public void WithEnvironment_AllowsDifferentTypes()
    {
        // Act
        var context = _builder
            .WithEnvironment(EnvironmentModifier.FromSurface(SurfaceType.Stable))
            .WithEnvironment(EnvironmentModifier.FromLighting(LightingLevel.Dim))
            .WithCorruption(CorruptionTier.Glitched)
            .Build();

        // Assert: All three different types should be added
        context.EnvironmentModifiers.Should().HaveCount(3);
    }

    #endregion

    #region Applied Status Tests

    [Test]
    public void WithAppliedStatus_AddsStatusOnce()
    {
        // Act
        var context = _builder
            .WithAppliedStatus("hidden")
            .WithAppliedStatus("hidden") // Duplicate
            .WithAppliedStatus("alerted")
            .Build();

        // Assert
        context.AppliedStatuses.Should().HaveCount(2);
        context.AppliedStatuses.Should().Contain("hidden");
        context.AppliedStatuses.Should().Contain("alerted");
    }

    #endregion

    #region Fluent API Tests

    [Test]
    public void FluentChaining_WorksCorrectly()
    {
        // Act
        var context = _builder
            .WithEquipment("picks", "Lockpicks", diceModifier: 1, required: true)
            .WithSituation("time-pressure", "Time Pressure", diceModifier: -1)
            .WithEnvironment(SurfaceType.Normal, LightingLevel.Dim)
            .WithTargetSuspicion(5)
            .WithAppliedStatus("hidden")
            .Build();

        // Assert
        context.HasModifiers.Should().BeTrue();
        // 1 equipment + 1 situational + 1 lighting (Normal surface is skipped) + 1 target = 4
        context.ModifierCount.Should().Be(4);
        context.EquipmentModifiers.Should().HaveCount(1);
        context.SituationalModifiers.Should().HaveCount(1);
        context.EnvironmentModifiers.Should().HaveCount(1);
        context.TargetModifiers.Should().HaveCount(1);
    }

    [Test]
    public void Reset_ClearsAllModifiers()
    {
        // Arrange
        _builder
            .WithEquipment("toolkit", "Toolkit", diceModifier: 2)
            .WithSituation("familiar", "Familiarity", diceModifier: 1);

        // Act
        _builder.Reset();
        var context = _builder.Build();

        // Assert
        context.HasModifiers.Should().BeFalse();
    }

    #endregion

    #region Value Object Overload Tests

    [Test]
    public void WithEquipment_AcceptsValueObject()
    {
        // Arrange
        var modifier = EquipmentModifier.Tool("toolkit", "Toolkit", diceBonus: 2);

        // Act
        var context = _builder.WithEquipment(modifier).Build();

        // Assert
        context.EquipmentModifiers.Should().HaveCount(1);
        context.EquipmentModifiers[0].EquipmentName.Should().Be("Toolkit");
    }

    [Test]
    public void WithSituation_AcceptsValueObject()
    {
        // Arrange
        var modifier = SituationalModifier.Familiarity("this lock type");

        // Act
        var context = _builder.WithSituation(modifier).Build();

        // Assert
        context.SituationalModifiers.Should().HaveCount(1);
        context.SituationalModifiers[0].Name.Should().Be("Familiarity");
    }

    [Test]
    public void WithTarget_AcceptsValueObject()
    {
        // Arrange
        var modifier = TargetModifier.FromDisposition(Disposition.Friendly, "npc-001");

        // Act
        var context = _builder.WithTarget(modifier).Build();

        // Assert
        context.TargetModifiers.Should().HaveCount(1);
        context.TargetModifiers[0].TargetId.Should().Be("npc-001");
    }

    #endregion
}
