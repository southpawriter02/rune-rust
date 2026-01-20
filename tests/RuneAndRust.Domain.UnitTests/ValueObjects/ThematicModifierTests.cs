using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class ThematicModifierTests
{
    [Test]
    public void GetEffectTags_ReturnsBrittleTag()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Rusted",
            PrimaryBiome = Biome.TheRoots,
            Adjective = "corroded",
            DetailFragment = "shows decay",
            IsBrittle = true
        };

        // Act
        var tags = modifier.GetEffectTags().ToList();

        // Assert
        tags.Should().Contain("Brittle");
    }

    [Test]
    public void GetEffectTags_ReturnsSlipperyTag()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Frozen",
            PrimaryBiome = Biome.Niflheim,
            Adjective = "ice-covered",
            DetailFragment = "is encased in frost",
            IsSlippery = true
        };

        // Act
        var tags = modifier.GetEffectTags().ToList();

        // Assert
        tags.Should().Contain("Slippery");
    }

    [Test]
    public void GetEffectTags_ReturnsLightSourceAndDazzleTags()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Crystalline",
            PrimaryBiome = Biome.Alfheim,
            Adjective = "crystalline",
            DetailFragment = "defies physics",
            IsLightSource = true,
            CanDazzle = true
        };

        // Act
        var tags = modifier.GetEffectTags().ToList();

        // Assert
        tags.Should().Contain("LightSource");
        tags.Should().Contain("Dazzle");
    }

    [Test]
    public void GetEffectTags_ReturnsDamageAuraTag()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Scorched",
            PrimaryBiome = Biome.Muspelheim,
            Adjective = "scorched",
            DetailFragment = "radiates heat",
            DamagePerTurn = 2,
            DamageType = "fire"
        };

        // Act
        var tags = modifier.GetEffectTags().ToList();

        // Assert
        tags.Should().Contain("DamageAura:fire:2");
    }

    [Test]
    public void GetEffectTags_ReturnsScaleTag()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Monolithic",
            PrimaryBiome = Biome.Jotunheim,
            Adjective = "monolithic",
            DetailFragment = "towers at massive scale",
            ScaleMultiplier = 2.0
        };

        // Act
        var tags = modifier.GetEffectTags().ToList();

        // Assert
        tags.Should().Contain("Scale:2.0");
    }

    [Test]
    public void GetEffectTags_ReturnsHpModTag()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Rusted",
            PrimaryBiome = Biome.TheRoots,
            Adjective = "corroded",
            DetailFragment = "shows decay",
            HpMultiplier = 0.7
        };

        // Act
        var tags = modifier.GetEffectTags().ToList();

        // Assert
        tags.Should().Contain("HpMod:0.7");
    }

    [Test]
    public void GetEffectTags_ReturnsEmptyForDefaultModifier()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Ancient",
            PrimaryBiome = Biome.Citadel,
            Adjective = "weathered",
            DetailFragment = "bears age"
        };

        // Act
        var tags = modifier.GetEffectTags().ToList();

        // Assert
        tags.Should().BeEmpty();
    }

    [Test]
    public void HasMechanicalEffects_ReturnsTrueWhenBrittle()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "test",
            DetailFragment = "test",
            IsBrittle = true
        };

        // Act & Assert
        modifier.HasMechanicalEffects.Should().BeTrue();
    }

    [Test]
    public void HasMechanicalEffects_ReturnsTrueWhenSlippery()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "test",
            DetailFragment = "test",
            IsSlippery = true
        };

        // Act & Assert
        modifier.HasMechanicalEffects.Should().BeTrue();
    }

    [Test]
    public void HasMechanicalEffects_ReturnsTrueWhenDamagePerTurn()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "test",
            DetailFragment = "test",
            DamagePerTurn = 1
        };

        // Act & Assert
        modifier.HasMechanicalEffects.Should().BeTrue();
    }

    [Test]
    public void HasMechanicalEffects_ReturnsFalseForDefaultModifier()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Ancient",
            PrimaryBiome = Biome.Citadel,
            Adjective = "weathered",
            DetailFragment = "bears age"
        };

        // Act & Assert
        modifier.HasMechanicalEffects.Should().BeFalse();
    }

    [Test]
    public void ToString_ReturnsNameAndBiome()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Frozen",
            PrimaryBiome = Biome.Niflheim,
            Adjective = "ice-covered",
            DetailFragment = "encased in frost"
        };

        // Act
        var result = modifier.ToString();

        // Assert
        result.Should().Be("Frozen (Niflheim)");
    }

    [Test]
    public void RecordEquality_WorksCorrectly()
    {
        // Arrange
        var modifier1 = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "test",
            DetailFragment = "test"
        };

        var modifier2 = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "test",
            DetailFragment = "test"
        };

        // Act & Assert
        modifier1.Should().Be(modifier2);
        (modifier1 == modifier2).Should().BeTrue();
    }

    [Test]
    public void DefaultValues_AreCorrect()
    {
        // Arrange
        var modifier = new ThematicModifier
        {
            Name = "Test",
            PrimaryBiome = Biome.Citadel,
            Adjective = "test",
            DetailFragment = "test"
        };

        // Assert
        modifier.HpMultiplier.Should().Be(1.0);
        modifier.ScaleMultiplier.Should().Be(1.0);
        modifier.IsBrittle.Should().BeFalse();
        modifier.IsSlippery.Should().BeFalse();
        modifier.IsLightSource.Should().BeFalse();
        modifier.CanDazzle.Should().BeFalse();
        modifier.DamagePerTurn.Should().BeNull();
        modifier.DamageType.Should().BeNull();
    }
}
