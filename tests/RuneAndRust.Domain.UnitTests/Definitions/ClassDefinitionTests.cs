using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

[TestFixture]
public class ClassDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsClass()
    {
        var modifiers = new StatModifiers { MaxHealth = 20, Attack = -2, Defense = 5 };
        var growth = new StatModifiers { MaxHealth = 12, Attack = 2, Defense = 3 };

        var classDef = ClassDefinition.Create(
            "shieldmaiden", "Shieldmaiden", "Stalwart defender", "warrior",
            modifiers, growth, "rage", ["shield-bash", "taunt"]);

        Assert.That(classDef.Id, Is.EqualTo("shieldmaiden"));
        Assert.That(classDef.ArchetypeId, Is.EqualTo("warrior"));
        Assert.That(classDef.PrimaryResourceId, Is.EqualTo("rage"));
        Assert.That(classDef.StartingAbilityIds.Count, Is.EqualTo(2));
    }

    [Test]
    public void Create_WithNullArchetypeId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ClassDefinition.Create(
                "shieldmaiden", "Shieldmaiden", "Description", null!,
                StatModifiers.None, StatModifiers.None, "rage"));
    }

    [Test]
    public void Create_WithNoStartingAbilities_HasEmptyList()
    {
        var classDef = ClassDefinition.Create(
            "test", "Test", "Description", "warrior",
            StatModifiers.None, StatModifiers.None, "rage");

        Assert.That(classDef.StartingAbilityIds, Is.Empty);
    }

    [Test]
    public void Create_WithNullRequirements_HasNullRequirements()
    {
        var classDef = ClassDefinition.Create(
            "test", "Test", "Description", "warrior",
            StatModifiers.None, StatModifiers.None, "rage", requirements: null);

        Assert.That(classDef.Requirements, Is.Null);
    }

    [Test]
    public void Create_NormalizesIdsToLowercase()
    {
        var classDef = ClassDefinition.Create(
            "SHIELDMAIDEN", "Shieldmaiden", "Description", "WARRIOR",
            StatModifiers.None, StatModifiers.None, "RAGE");

        Assert.That(classDef.Id, Is.EqualTo("shieldmaiden"));
        Assert.That(classDef.ArchetypeId, Is.EqualTo("warrior"));
        Assert.That(classDef.PrimaryResourceId, Is.EqualTo("rage"));
    }

    [Test]
    public void Create_WithRequirements_StoresRequirements()
    {
        var requirements = new ClassRequirements
        {
            AllowedRaceIds = ["elf"],
            MinimumAttributes = new Dictionary<string, int> { ["will"] = 12 }
        };

        var classDef = ClassDefinition.Create(
            "test", "Test", "Description", "warrior",
            StatModifiers.None, StatModifiers.None, "rage",
            requirements: requirements);

        Assert.That(classDef.Requirements, Is.Not.Null);
        Assert.That(classDef.Requirements!.Value.HasRequirements, Is.True);
    }
}
