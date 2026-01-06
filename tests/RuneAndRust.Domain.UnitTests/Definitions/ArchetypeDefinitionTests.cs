using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Definitions;

[TestFixture]
public class ArchetypeDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsArchetype()
    {
        var archetype = ArchetypeDefinition.Create(
            "warrior", "Warrior", "Masters of martial combat",
            "Frontline fighter", StatTendency.Defensive);

        Assert.That(archetype.Id, Is.EqualTo("warrior"));
        Assert.That(archetype.Name, Is.EqualTo("Warrior"));
        Assert.That(archetype.StatTendency, Is.EqualTo(StatTendency.Defensive));
    }

    [Test]
    public void Create_WithNullId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ArchetypeDefinition.Create(null!, "Warrior", "Description", "Summary", StatTendency.Defensive));
    }

    [Test]
    public void Create_NormalizesIdToLowercase()
    {
        var archetype = ArchetypeDefinition.Create(
            "WARRIOR", "Warrior", "Description", "Summary", StatTendency.Defensive);

        Assert.That(archetype.Id, Is.EqualTo("warrior"));
    }

    [Test]
    public void Create_WithEmptyDescription_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            ArchetypeDefinition.Create("warrior", "Warrior", "", "Summary", StatTendency.Defensive));
    }

    [Test]
    public void StatTendency_AllValuesAreDefined()
    {
        var values = Enum.GetValues<StatTendency>();
        Assert.That(values.Length, Is.EqualTo(4));
    }
}
