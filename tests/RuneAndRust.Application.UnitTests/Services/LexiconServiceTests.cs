using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class LexiconServiceTests
{
    private LexiconService _service = null!;
    private Mock<ILogger<LexiconService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<LexiconService>>();

        var config = new LexiconConfiguration
        {
            Terms = new Dictionary<string, TermDefinition>
            {
                ["attack"] = new TermDefinition
                {
                    Default = "attack",
                    Synonyms = ["strike", "slash", "swing"],
                    Contextual = new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["combat"] = new List<string> { "strike", "slash", "thrust" },
                        ["formal"] = new List<string> { "engage", "assail" }
                    },
                    Weights = new Dictionary<string, int>
                    {
                        ["strike"] = 30,
                        ["slash"] = 25,
                        ["swing"] = 20
                    }
                },
                ["damage"] = new TermDefinition
                {
                    Default = "damage",
                    Severity = new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["light"] = new List<string> { "graze", "scratch" },
                        ["moderate"] = new List<string> { "wound", "injure" },
                        ["heavy"] = new List<string> { "devastate", "ravage" },
                        ["critical"] = new List<string> { "obliterate" }
                    }
                },
                ["quantities"] = new TermDefinition
                {
                    Default = "some",
                    Severity = new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["none"] = new List<string> { "none", "no" },
                        ["one"] = new List<string> { "a", "one" },
                        ["few"] = new List<string> { "a few", "some" },
                        ["many"] = new List<string> { "many", "numerous" },
                        ["horde"] = new List<string> { "a horde of", "swarms of" }
                    }
                },
                ["conditions"] = new TermDefinition
                {
                    Default = "wounded",
                    Severity = new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["healthy"] = new List<string> { "unharmed", "at full strength" },
                        ["wounded"] = new List<string> { "wounded", "injured" },
                        ["bloodied"] = new List<string> { "bloodied", "badly wounded" },
                        ["nearDeath"] = new List<string> { "near death", "barely standing" }
                    }
                }
            }
        };

        _service = new LexiconService(config, _mockLogger.Object);
    }

    [Test]
    public void GetTerm_UnknownTerm_ReturnsFallback()
    {
        var result = _service.GetTerm("unknown_term");
        Assert.That(result, Is.EqualTo("unknown_term"));
    }

    [Test]
    public void GetTerm_WithoutSynonym_ReturnsDefault()
    {
        var result = _service.GetTerm("attack", useSynonym: false);
        Assert.That(result, Is.EqualTo("attack"));
    }

    [Test]
    public void GetTerm_WithSynonym_ReturnsFromPool()
    {
        var result = _service.GetTerm("attack");
        var validOptions = new[] { "attack", "strike", "slash", "swing" };
        Assert.That(validOptions, Contains.Item(result));
    }

    [Test]
    public void GetTerm_WithContext_ReturnsContextualSynonym()
    {
        // Run multiple times to ensure we get contextual results
        var results = new HashSet<string>();
        for (int i = 0; i < 20; i++)
        {
            results.Add(_service.GetTerm("attack", context: "combat"));
        }

        // Should get at least one contextual result
        var contextualOptions = new[] { "strike", "slash", "thrust" };
        Assert.That(results.Intersect(contextualOptions), Is.Not.Empty);
    }

    [Test]
    public void GetDamageSeverity_LightDamage_ReturnsLightDescriptor()
    {
        var result = _service.GetDamageSeverity(0.05); // 5%
        var lightOptions = new[] { "graze", "scratch" };
        Assert.That(lightOptions, Contains.Item(result));
    }

    [Test]
    public void GetDamageSeverity_HeavyDamage_ReturnsHeavyDescriptor()
    {
        var result = _service.GetDamageSeverity(0.35); // 35%
        var heavyOptions = new[] { "devastate", "ravage" };
        Assert.That(heavyOptions, Contains.Item(result));
    }

    [Test]
    public void GetDamageSeverity_CriticalDamage_ReturnsCriticalDescriptor()
    {
        var result = _service.GetDamageSeverity(0.75); // 75%
        Assert.That(result, Is.EqualTo("obliterate"));
    }

    [Test]
    public void GetQuantity_Zero_ReturnsNoneDescriptor()
    {
        var result = _service.GetQuantity(0);
        var noneOptions = new[] { "none", "no" };
        Assert.That(noneOptions, Contains.Item(result));
    }

    [Test]
    public void GetQuantity_One_ReturnsOneDescriptor()
    {
        var result = _service.GetQuantity(1);
        var oneOptions = new[] { "a", "one" };
        Assert.That(oneOptions, Contains.Item(result));
    }

    [Test]
    public void GetQuantity_ManyItems_ReturnsManyDescriptor()
    {
        var result = _service.GetQuantity(7);
        var manyOptions = new[] { "many", "numerous" };
        Assert.That(manyOptions, Contains.Item(result));
    }

    [Test]
    public void GetCondition_FullHealth_ReturnsHealthyDescriptor()
    {
        var result = _service.GetCondition(1.0);
        var healthyOptions = new[] { "unharmed", "at full strength" };
        Assert.That(healthyOptions, Contains.Item(result));
    }

    [Test]
    public void GetCondition_LowHealth_ReturnsNearDeathDescriptor()
    {
        var result = _service.GetCondition(0.1);
        var nearDeathOptions = new[] { "near death", "barely standing" };
        Assert.That(nearDeathOptions, Contains.Item(result));
    }
}
