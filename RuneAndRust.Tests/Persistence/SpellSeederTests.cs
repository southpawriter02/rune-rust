using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Tests.Persistence;

/// <summary>
/// Unit tests for SpellSeeder (v0.4.3e - The Resonance).
/// Verifies spell data integrity without database dependencies.
/// </summary>
public class SpellSeederTests
{
    #region Spell Count Tests

    [Fact]
    public void GetStarterSpells_Returns12Spells()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();

        // Assert
        spells.Should().HaveCount(12);
    }

    [Fact]
    public void GetDestructionSpells_Returns4Spells()
    {
        // Act
        var spells = SpellSeeder.GetDestructionSpells();

        // Assert
        spells.Should().HaveCount(4);
        spells.Should().AllSatisfy(s => s.School.Should().Be(SpellSchool.Destruction));
    }

    [Fact]
    public void GetRestorationSpells_Returns3Spells()
    {
        // Act
        var spells = SpellSeeder.GetRestorationSpells();

        // Assert
        spells.Should().HaveCount(3);
        spells.Should().AllSatisfy(s => s.School.Should().Be(SpellSchool.Restoration));
    }

    [Fact]
    public void GetAlterationSpells_Returns3Spells()
    {
        // Act
        var spells = SpellSeeder.GetAlterationSpells();

        // Assert
        spells.Should().HaveCount(3);
        spells.Should().AllSatisfy(s => s.School.Should().Be(SpellSchool.Alteration));
    }

    [Fact]
    public void GetDivinationSpells_Returns2Spells()
    {
        // Act
        var spells = SpellSeeder.GetDivinationSpells();

        // Assert
        spells.Should().HaveCount(2);
        spells.Should().AllSatisfy(s => s.School.Should().Be(SpellSchool.Divination));
    }

    #endregion

    #region Spell Validity Tests

    [Fact]
    public void AllSpells_HaveUniqueIds()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();
        var ids = spells.Select(s => s.Id).ToList();

        // Assert
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllSpells_HaveUniqueNames()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();
        var names = spells.Select(s => s.Name).ToList();

        // Assert
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllSpells_HaveNonEmptyDescriptions()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();

        // Assert
        spells.Should().AllSatisfy(s =>
        {
            s.Description.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public void AllSpells_HaveValidApCost()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();

        // Assert
        spells.Should().AllSatisfy(s =>
        {
            s.ApCost.Should().BeGreaterThan(0);
            s.ApCost.Should().BeLessThanOrEqualTo(10);
        });
    }

    [Fact]
    public void AllSpells_HaveValidFluxCost()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();

        // Assert
        spells.Should().AllSatisfy(s =>
        {
            s.FluxCost.Should().BeGreaterThan(0);
            s.FluxCost.Should().BeLessThanOrEqualTo(25);
        });
    }

    [Fact]
    public void AllSpells_HaveValidTier()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();

        // Assert
        spells.Should().AllSatisfy(s =>
        {
            s.Tier.Should().BeGreaterThan(0);
            s.Tier.Should().BeLessThanOrEqualTo(4);
        });
    }

    [Fact]
    public void AllSpells_HaveEffectScript()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();

        // Assert
        spells.Should().AllSatisfy(s =>
        {
            s.EffectScript.Should().NotBeNullOrWhiteSpace();
        });
    }

    #endregion

    #region School Distribution Tests

    [Fact]
    public void StarterSpells_HaveCorrectSchoolDistribution()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();
        var bySchool = spells.GroupBy(s => s.School).ToDictionary(g => g.Key, g => g.Count());

        // Assert
        bySchool.Should().ContainKey(SpellSchool.Destruction).WhoseValue.Should().Be(4);
        bySchool.Should().ContainKey(SpellSchool.Restoration).WhoseValue.Should().Be(3);
        bySchool.Should().ContainKey(SpellSchool.Alteration).WhoseValue.Should().Be(3);
        bySchool.Should().ContainKey(SpellSchool.Divination).WhoseValue.Should().Be(2);
    }

    [Fact]
    public void StarterSpells_HaveMixOfTiers()
    {
        // Act
        var spells = SpellSeeder.GetStarterSpells();
        var tiers = spells.Select(s => s.Tier).Distinct().ToList();

        // Assert
        tiers.Should().Contain(1);  // Has tier 1 spells
        tiers.Should().Contain(2);  // Has tier 2 spells
    }

    #endregion

    #region Specific Spell Tests

    [Fact]
    public void DestructionSpells_IncludeSpark()
    {
        // Act
        var spells = SpellSeeder.GetDestructionSpells();
        var spark = spells.FirstOrDefault(s => s.Name == "Spark");

        // Assert
        spark.Should().NotBeNull();
        spark!.ApCost.Should().Be(2);
        spark.FluxCost.Should().Be(8);
        spark.EffectScript.Should().Contain("DAMAGE:Fire");
    }

    [Fact]
    public void RestorationSpells_IncludeMendFlesh()
    {
        // Act
        var spells = SpellSeeder.GetRestorationSpells();
        var mend = spells.FirstOrDefault(s => s.Name == "Mend Flesh");

        // Assert
        mend.Should().NotBeNull();
        mend!.EffectScript.Should().Contain("HEAL:");
    }

    #endregion
}
