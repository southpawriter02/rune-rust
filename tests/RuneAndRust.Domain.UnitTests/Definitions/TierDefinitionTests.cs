using FluentAssertions;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for TierDefinition (v0.0.9c).
/// </summary>
[TestFixture]
public class TierDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsTierDefinition()
    {
        var tier = TierDefinition.Create(
            id: "elite",
            name: "Elite",
            namePrefix: "Elite",
            healthMultiplier: 2.0f,
            attackMultiplier: 1.5f,
            defenseMultiplier: 1.5f,
            experienceMultiplier: 3.0f,
            lootMultiplier: 3.0f,
            color: "orange3",
            spawnWeight: 8,
            generatesUniqueName: false,
            sortOrder: 2);

        tier.Should().NotBeNull();
        tier.Id.Should().Be("elite");
        tier.Name.Should().Be("Elite");
        tier.NamePrefix.Should().Be("Elite");
        tier.HealthMultiplier.Should().Be(2.0f);
        tier.AttackMultiplier.Should().Be(1.5f);
        tier.DefenseMultiplier.Should().Be(1.5f);
        tier.ExperienceMultiplier.Should().Be(3.0f);
        tier.LootMultiplier.Should().Be(3.0f);
        tier.Color.Should().Be("orange3");
        tier.SpawnWeight.Should().Be(8);
        tier.GeneratesUniqueName.Should().BeFalse();
        tier.SortOrder.Should().Be(2);
    }

    [Test]
    public void Create_WithNullId_ThrowsArgumentException()
    {
        var act = () => TierDefinition.Create(
            id: null!,
            name: "Test",
            healthMultiplier: 1.0f,
            attackMultiplier: 1.0f,
            defenseMultiplier: 1.0f,
            experienceMultiplier: 1.0f,
            lootMultiplier: 1.0f);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }

    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        var act = () => TierDefinition.Create(
            id: "test",
            name: null!,
            healthMultiplier: 1.0f,
            attackMultiplier: 1.0f,
            defenseMultiplier: 1.0f,
            experienceMultiplier: 1.0f,
            lootMultiplier: 1.0f);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Common_ReturnsDefaultCommonTier()
    {
        var common = TierDefinition.Common;

        common.Id.Should().Be("common");
        common.Name.Should().Be("Common");
        common.HealthMultiplier.Should().Be(1.0f);
        common.AttackMultiplier.Should().Be(1.0f);
        common.DefenseMultiplier.Should().Be(1.0f);
        common.ExperienceMultiplier.Should().Be(1.0f);
        common.LootMultiplier.Should().Be(1.0f);
        common.SpawnWeight.Should().Be(70); // Common tier has weight 70 as per design spec
        common.GeneratesUniqueName.Should().BeFalse();
    }
}
