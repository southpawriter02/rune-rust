using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Domain.UnitTests.Definitions;

[TestFixture]
public class ResourceTypeDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsResourceType()
    {
        var resourceType = ResourceTypeDefinition.Create(
            "mana", "Arcane Power", "MP", "Magical energy", "#0066FF", 100,
            regenPerTurn: 10);

        Assert.That(resourceType.Id, Is.EqualTo("mana"));
        Assert.That(resourceType.DisplayName, Is.EqualTo("Arcane Power"));
        Assert.That(resourceType.Abbreviation, Is.EqualTo("MP"));
        Assert.That(resourceType.RegenPerTurn, Is.EqualTo(10));
    }

    [Test]
    public void Create_NormalizesIdToLowercase()
    {
        var resourceType = ResourceTypeDefinition.Create(
            "MANA", "Mana", "MP", "Description", "#0066FF", 100);

        Assert.That(resourceType.Id, Is.EqualTo("mana"));
    }

    [Test]
    public void Create_NormalizesAbbreviationToUppercase()
    {
        var resourceType = ResourceTypeDefinition.Create(
            "mana", "Mana", "mp", "Description", "#0066FF", 100);

        Assert.That(resourceType.Abbreviation, Is.EqualTo("MP"));
    }

    [Test]
    public void Regenerates_WhenRegenPerTurnPositive_ReturnsTrue()
    {
        var resourceType = ResourceTypeDefinition.Create(
            "mana", "Mana", "MP", "Desc", "#0066FF", 100, regenPerTurn: 10);

        Assert.That(resourceType.Regenerates, Is.True);
    }

    [Test]
    public void Decays_WhenDecayPerTurnPositive_ReturnsTrue()
    {
        var resourceType = ResourceTypeDefinition.Create(
            "rage", "Fury", "RG", "Desc", "#FF6600", 100, decayPerTurn: 10);

        Assert.That(resourceType.Decays, Is.True);
    }

    [Test]
    public void BuildsFromCombat_WhenBuildOnDamagePositive_ReturnsTrue()
    {
        var resourceType = ResourceTypeDefinition.Create(
            "rage", "Fury", "RG", "Desc", "#FF6600", 100,
            buildOnDamageDealt: 10, buildOnDamageTaken: 15);

        Assert.That(resourceType.BuildsFromCombat, Is.True);
    }
}
