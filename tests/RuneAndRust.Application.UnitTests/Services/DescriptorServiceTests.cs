using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class DescriptorServiceTests
{
    private DescriptorService _service = null!;
    private Mock<ILogger<DescriptorService>> _mockLogger = null!;
    private Dictionary<string, DescriptorPool> _pools = null!;
    private ThemeConfiguration _theme = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<DescriptorService>>();

        _pools = new Dictionary<string, DescriptorPool>
        {
            ["environmental.lighting"] = new DescriptorPool
            {
                Id = "lighting",
                Name = "Lighting",
                Descriptors =
                [
                    new Descriptor { Id = "dim_torch", Text = "dimly lit by flickering torches", Weight = 20, Tags = ["dungeon"] },
                    new Descriptor { Id = "dark_shadows", Text = "shrouded in darkness", Weight = 30, Tags = ["danger"], Themes = ["dark_fantasy"] },
                    new Descriptor { Id = "bright_light", Text = "bathed in bright light", Weight = 15, Themes = ["high_fantasy"] }
                ]
            },
            ["combat.hit_descriptions"] = new DescriptorPool
            {
                Id = "hit_descriptions",
                Name = "Hit Descriptions",
                Descriptors =
                [
                    new Descriptor { Id = "strikes_true", Text = "strikes true", Weight = 25 },
                    new Descriptor { Id = "connects_solidly", Text = "connects solidly", Weight = 25 },
                    new Descriptor { Id = "devastating", Text = "lands a devastating blow", Weight = 10, MinDamagePercent = 0.25 }
                ]
            },
            ["combat.damage_severity"] = new DescriptorPool
            {
                Id = "damage_severity",
                Name = "Damage Severity",
                Descriptors =
                [
                    new Descriptor { Id = "grazes", Text = "grazes", Weight = 100, MaxDamagePercent = 0.1 },
                    new Descriptor { Id = "wounds", Text = "wounds", Weight = 100, MinDamagePercent = 0.1, MaxDamagePercent = 0.25 },
                    new Descriptor { Id = "devastates", Text = "devastates", Weight = 100, MinDamagePercent = 0.25 }
                ]
            }
        };

        _theme = new ThemeConfiguration
        {
            ActiveTheme = "dark_fantasy",
            Themes = new Dictionary<string, ThemePreset>
            {
                ["dark_fantasy"] = new ThemePreset
                {
                    Id = "dark_fantasy",
                    Name = "Dark Fantasy",
                    EmphasizedTerms = ["darkness", "shadows"],
                    ExcludedTerms = ["cheerful", "bright"]
                }
            }
        };

        _service = new DescriptorService(_pools, _theme, _mockLogger.Object);
    }

    [Test]
    public void GetDescriptor_ValidPool_ReturnsDescriptor()
    {
        var result = _service.GetDescriptor("environmental.lighting");
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetDescriptor_UnknownPool_ReturnsEmpty()
    {
        var result = _service.GetDescriptor("unknown.pool");
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetDescriptor_WithTags_FiltersResults()
    {
        // Run multiple times to see filtered results
        var results = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var result = _service.GetDescriptor("environmental.lighting", tags: new[] { "dungeon" });
            if (!string.IsNullOrEmpty(result))
                results.Add(result);
        }

        // Should get dungeon-tagged descriptor
        Assert.That(results, Contains.Item("dimly lit by flickering torches"));
    }

    [Test]
    public void GetDescriptor_ActiveTheme_FiltersToTheme()
    {
        var results = new HashSet<string>();
        for (int i = 0; i < 20; i++)
        {
            var result = _service.GetDescriptor("environmental.lighting");
            if (!string.IsNullOrEmpty(result))
                results.Add(result);
        }

        // high_fantasy-only descriptors should not appear with dark_fantasy active
        Assert.That(results, Does.Not.Contain("bathed in bright light"));
    }

    [Test]
    public void GetDescriptor_ExcludedTerms_FiltersOut()
    {
        var results = new HashSet<string>();
        for (int i = 0; i < 20; i++)
        {
            var result = _service.GetDescriptor("environmental.lighting");
            if (!string.IsNullOrEmpty(result))
                results.Add(result);
        }

        // "bright" is excluded in dark_fantasy theme
        Assert.That(results.All(r => !r.Contains("bright", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public void GetCombatHitDescription_ReturnsDescription()
    {
        var result = _service.GetCombatHitDescription(50, 100); // 50% damage

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetCombatHitDescription_HighDamage_IncludesDevastating()
    {
        // With 50% damage, the "devastates" descriptor should be available
        var results = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(_service.GetCombatHitDescription(50, 100));
        }

        Assert.That(results.Any(r => r.Contains("devastat", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public void GenerateRoomAtmosphere_ReturnsCombinedDescription()
    {
        var result = _service.GenerateRoomAtmosphere(new[] { "dungeon" });

        // Should have some content
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetDescriptor_AvoidsRecentlyUsed()
    {
        // Call multiple times and verify we get variety
        var results = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(_service.GetDescriptor("combat.hit_descriptions"));
        }

        // Should have more than one unique result
        Assert.That(results.Distinct().Count(), Is.GreaterThan(1));
    }
}
