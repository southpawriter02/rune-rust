using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class DescriptorServiceEnvironmentTests
{
    private Dictionary<string, DescriptorPool> _pools = null!;
    private ThemeConfiguration _theme = null!;
    private Mock<ILogger<DescriptorService>> _descriptorLoggerMock = null!;
    private Mock<ILogger<EnvironmentCoherenceService>> _coherenceLoggerMock = null!;
    private EnvironmentCoherenceService _coherenceService = null!;
    private DescriptorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _pools = CreateTestPools();
        _theme = CreateTestTheme();
        _descriptorLoggerMock = new Mock<ILogger<DescriptorService>>();
        _coherenceLoggerMock = new Mock<ILogger<EnvironmentCoherenceService>>();

        var categoryConfig = CreateTestCategoryConfig();
        var biomeConfig = CreateTestBiomeConfig();
        _coherenceService = new EnvironmentCoherenceService(categoryConfig, biomeConfig, _coherenceLoggerMock.Object);

        _service = new DescriptorService(_pools, _theme, _descriptorLoggerMock.Object, _coherenceService);
    }

    [Test]
    public void GetDescriptorWithEnvironment_AppliesBiomeOverrides()
    {
        var environment = _coherenceService.CreateFromBiome("cave");
        var context = new DescriptorContext
        {
            Environment = environment,
            IncludeEnvironmentTags = true
        };

        // Run multiple times to account for randomness
        var results = new HashSet<string>();
        for (int i = 0; i < 20; i++)
        {
            var result = _service.GetDescriptorWithEnvironment("environmental.lighting", context);
            if (!string.IsNullOrEmpty(result))
                results.Add(result);
        }

        // Should get descriptors from the pool (we have overridden pool in biome config)
        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void GetDescriptorWithEnvironment_IncludesDerivedTags()
    {
        var environment = new EnvironmentContext(
            new Dictionary<string, string> { ["biome"] = "cave" },
            ["underground", "natural"]);

        var context = new DescriptorContext
        {
            Environment = environment,
            IncludeEnvironmentTags = true
        };

        var effectiveTags = context.GetEffectiveTags().ToList();

        Assert.That(effectiveTags, Does.Contain("underground"));
        Assert.That(effectiveTags, Does.Contain("natural"));
    }

    [Test]
    public void GenerateRoomAtmosphereWithEnvironment_ProducesOutput()
    {
        var environment = _coherenceService.CreateFromBiome("cave");

        var results = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var result = _service.GenerateRoomAtmosphereWithEnvironment(environment);
            if (!string.IsNullOrEmpty(result))
                results.Add(result);
        }

        // Should produce some output
        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void GetEffectiveTags_CombinesExplicitAndDerivedTags()
    {
        var environment = new EnvironmentContext(
            new Dictionary<string, string> { ["biome"] = "cave" },
            ["underground", "natural"]);

        var context = new DescriptorContext
        {
            Tags = ["dungeon", "danger"],
            Environment = environment,
            IncludeEnvironmentTags = true
        };

        var effectiveTags = context.GetEffectiveTags().ToList();

        Assert.That(effectiveTags, Does.Contain("dungeon"));
        Assert.That(effectiveTags, Does.Contain("danger"));
        Assert.That(effectiveTags, Does.Contain("underground"));
        Assert.That(effectiveTags, Does.Contain("natural"));
    }

    [Test]
    public void GetEffectiveTags_WhenEnvironmentTagsDisabled_OnlyExplicitTags()
    {
        var environment = new EnvironmentContext(
            new Dictionary<string, string> { ["biome"] = "cave" },
            ["underground", "natural"]);

        var context = new DescriptorContext
        {
            Tags = ["dungeon"],
            Environment = environment,
            IncludeEnvironmentTags = false
        };

        var effectiveTags = context.GetEffectiveTags().ToList();

        Assert.That(effectiveTags, Does.Contain("dungeon"));
        Assert.That(effectiveTags, Does.Not.Contain("underground"));
        Assert.That(effectiveTags, Does.Not.Contain("natural"));
    }

    [Test]
    public void DescriptorContext_Environment_SupportsNullable()
    {
        var context = new DescriptorContext
        {
            Tags = ["dungeon"]
        };

        Assert.That(context.Environment, Is.Null);
        Assert.That(context.GetEffectiveTags().ToList(), Does.Contain("dungeon"));
    }

    private static Dictionary<string, DescriptorPool> CreateTestPools()
    {
        return new Dictionary<string, DescriptorPool>
        {
            ["environmental.lighting"] = new DescriptorPool
            {
                Id = "lighting",
                Name = "Lighting",
                Descriptors =
                [
                    new Descriptor
                    {
                        Id = "torch_light",
                        Text = "The area is dimly lit by flickering torches.",
                        Weight = 10,
                        Tags = ["indoor", "dungeon"]
                    },
                    new Descriptor
                    {
                        Id = "cave_phosphorescence",
                        Text = "Bioluminescent fungi cast an eerie glow.",
                        Weight = 10,
                        Tags = ["cave", "underground"]
                    }
                ]
            },
            ["environmental.lighting.cave"] = new DescriptorPool
            {
                Id = "lighting_cave",
                Name = "Cave Lighting",
                Descriptors =
                [
                    new Descriptor
                    {
                        Id = "cave_darkness",
                        Text = "Darkness presses in from all sides.",
                        Weight = 10,
                        Tags = ["underground", "natural"]  // Match derived tags from cave biome
                    }
                ]
            },
            ["environmental.sounds"] = new DescriptorPool
            {
                Id = "sounds",
                Name = "Sounds",
                Descriptors =
                [
                    new Descriptor
                    {
                        Id = "dripping",
                        Text = "Water drips somewhere in the distance.",
                        Weight = 10,
                        Tags = ["cave", "underground"]
                    }
                ]
            },
            ["environmental.smells"] = new DescriptorPool
            {
                Id = "smells",
                Name = "Smells",
                Descriptors =
                [
                    new Descriptor
                    {
                        Id = "musty",
                        Text = "A musty odor fills the air.",
                        Weight = 10,
                        Tags = ["underground"]
                    }
                ]
            },
            ["environmental.temperature"] = new DescriptorPool
            {
                Id = "temperature",
                Name = "Temperature",
                Descriptors =
                [
                    new Descriptor
                    {
                        Id = "cool",
                        Text = "Cool air brushes against your skin.",
                        Weight = 10,
                        Tags = ["cave", "underground"]
                    }
                ]
            }
        };
    }

    private static ThemeConfiguration CreateTestTheme()
    {
        return new ThemeConfiguration
        {
            ActiveTheme = "dark_fantasy",
            Themes = new Dictionary<string, ThemePreset>
            {
                ["dark_fantasy"] = new ThemePreset
                {
                    Id = "dark_fantasy",
                    Name = "Dark Fantasy",
                    Description = "Grim, atmospheric",
                    DescriptorOverrides = new Dictionary<string, string>(),
                    ExcludedTerms = [],
                    EmphasizedTerms = []
                }
            }
        };
    }

    private static EnvironmentCategoryConfiguration CreateTestCategoryConfig()
    {
        return new EnvironmentCategoryConfiguration
        {
            Version = "1.0",
            Categories = new Dictionary<string, EnvironmentCategory>
            {
                ["biome"] = new EnvironmentCategory
                {
                    Id = "biome",
                    Name = "Biome",
                    IsRequired = true,
                    DefaultValue = "dungeon",
                    Values =
                    [
                        new CategoryValue { Id = "cave", Name = "Cave", ImpliedTags = ["underground", "natural"] },
                        new CategoryValue { Id = "dungeon", Name = "Dungeon", ImpliedTags = ["underground", "constructed"] }
                    ]
                },
                ["climate"] = new EnvironmentCategory
                {
                    Id = "climate",
                    Name = "Climate",
                    IsRequired = false,
                    DefaultValue = "temperate",
                    Values =
                    [
                        new CategoryValue { Id = "cold", Name = "Cold", ImpliedTags = ["cold"] },
                        new CategoryValue { Id = "temperate", Name = "Temperate", ImpliedTags = [] }
                    ]
                },
                ["lighting"] = new EnvironmentCategory
                {
                    Id = "lighting",
                    Name = "Lighting",
                    IsRequired = true,
                    DefaultValue = "dim",
                    Values =
                    [
                        new CategoryValue { Id = "dim", Name = "Dim", ImpliedTags = ["dim"] }
                    ]
                }
            },
            ExclusionRules = []
        };
    }

    private static BiomeConfiguration CreateTestBiomeConfig()
    {
        return new BiomeConfiguration
        {
            Version = "1.0",
            Biomes = new Dictionary<string, BiomeDefinition>
            {
                ["cave"] = new BiomeDefinition
                {
                    Id = "cave",
                    Name = "Cave",
                    Description = "Natural underground cavern",
                    DefaultCategoryValues = new Dictionary<string, string>
                    {
                        ["climate"] = "cold",
                        ["lighting"] = "dim"
                    },
                    ImpliedTags = ["underground", "natural", "stone"],
                    DescriptorPoolOverrides = new Dictionary<string, string>
                    {
                        ["environmental.lighting"] = "environmental.lighting.cave"
                    }
                }
            }
        };
    }
}
