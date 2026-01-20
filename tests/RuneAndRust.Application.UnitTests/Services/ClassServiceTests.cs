using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class ClassServiceTests
{
    private ClassService _service = null!;
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private Mock<ILogger<ClassService>> _mockLogger = null!;
    private AbilityService _abilityService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockConfig = new Mock<IGameConfigurationProvider>();
        _mockLogger = new Mock<ILogger<ClassService>>();

        var archetypes = new List<ArchetypeDefinition>
        {
            ArchetypeDefinition.Create("warrior", "Warrior", "Martial combat", "Frontline", StatTendency.Defensive),
            ArchetypeDefinition.Create("mystic", "Mystic", "Magic user", "Spellcaster", StatTendency.Offensive)
        };

        var classes = new List<ClassDefinition>
        {
            ClassDefinition.Create("shieldmaiden", "Shieldmaiden", "Defender", "warrior",
                new StatModifiers { MaxHealth = 20 }, StatModifiers.None, "rage"),
            ClassDefinition.Create("galdr-caster", "Galdr-Caster", "Mage", "mystic",
                new StatModifiers { Attack = 8 }, StatModifiers.None, "mana")
        };

        _mockConfig.Setup(c => c.GetArchetypes()).Returns(archetypes);
        _mockConfig.Setup(c => c.GetClasses()).Returns(classes);
        _mockConfig.Setup(c => c.GetArchetypeById("warrior")).Returns(archetypes[0]);
        _mockConfig.Setup(c => c.GetClassById("shieldmaiden")).Returns(classes[0]);
        _mockConfig.Setup(c => c.GetClassById("galdr-caster")).Returns(classes[1]);
        _mockConfig.Setup(c => c.GetAbilities()).Returns(new List<AbilityDefinition>());

        // Create ResourceService and AbilityService for ClassService dependency
        var mockResourceLogger = new Mock<ILogger<ResourceService>>();
        _mockConfig.Setup(c => c.GetResourceTypes()).Returns(new List<ResourceTypeDefinition>());
        var resourceService = new ResourceService(_mockConfig.Object, mockResourceLogger.Object);

        var mockAbilityLogger = new Mock<ILogger<AbilityService>>();
        _abilityService = new AbilityService(_mockConfig.Object, resourceService, mockAbilityLogger.Object);

        _service = new ClassService(_mockConfig.Object, _abilityService, _mockLogger.Object);
    }

    [Test]
    public void GetAllArchetypes_ReturnsAllArchetypes()
    {
        var result = _service.GetAllArchetypes();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("Warrior"));
    }

    [Test]
    public void GetClassesForArchetype_ReturnsMatchingClasses()
    {
        var result = _service.GetClassesForArchetype("warrior");

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Shieldmaiden"));
    }

    [Test]
    public void ValidateClassRequirements_NoRequirements_ReturnsValid()
    {
        var attrs = new Dictionary<string, int> { ["might"] = 10 };

        var result = _service.ValidateClassRequirements("shieldmaiden", "human", attrs);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void CalculateModifiedStats_AppliesModifiers()
    {
        var baseStats = new Stats(100, 10, 5);

        var result = _service.CalculateModifiedStats(baseStats, "shieldmaiden");

        Assert.That(result.MaxHealth, Is.EqualTo(120));
    }
}
