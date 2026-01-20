using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class EnvironmentCoherenceServiceTests
{
    private EnvironmentCategoryConfiguration _categoryConfig = null!;
    private BiomeConfiguration _biomeConfig = null!;
    private Mock<ILogger<EnvironmentCoherenceService>> _loggerMock = null!;
    private EnvironmentCoherenceService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _categoryConfig = CreateTestCategoryConfig();
        _biomeConfig = CreateTestBiomeConfig();
        _loggerMock = new Mock<ILogger<EnvironmentCoherenceService>>();
        _service = new EnvironmentCoherenceService(_categoryConfig, _biomeConfig, _loggerMock.Object);
    }

    [Test]
    public void Validate_WithNoConflicts_ReturnsValid()
    {
        var context = new EnvironmentContext(
            new Dictionary<string, string>
            {
                ["biome"] = "cave",
                ["climate"] = "cold"
            },
            []);

        var result = _service.Validate(context);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Violations, Is.Empty);
    }

    [Test]
    public void Validate_WithHardConflict_ReturnsViolation()
    {
        var context = new EnvironmentContext(
            new Dictionary<string, string>
            {
                ["biome"] = "volcanic",
                ["climate"] = "freezing"
            },
            []);

        var result = _service.Validate(context);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.HasHardViolations, Is.True);
        Assert.That(result.Violations, Has.Count.EqualTo(1));
        Assert.That(result.Violations[0].RuleId, Is.EqualTo("volcanic_freezing"));
    }

    [Test]
    public void Validate_WithSoftConflict_ReturnsWarningViolation()
    {
        var context = new EnvironmentContext(
            new Dictionary<string, string>
            {
                ["era"] = "ancient",
                ["condition"] = "pristine"
            },
            []);

        var result = _service.Validate(context);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.HasSoftViolationsOnly, Is.True);
        Assert.That(result.Violations[0].IsHardRule, Is.False);
    }

    [Test]
    public void CreateFromBiome_WithValidBiome_CreatesContext()
    {
        var context = _service.CreateFromBiome("cave");

        Assert.That(context.Biome, Is.EqualTo("cave"));
        Assert.That(context.Climate, Is.EqualTo("cold"));
        Assert.That(context.Lighting, Is.EqualTo("dim"));
    }

    [Test]
    public void CreateFromBiome_WithUnknownBiome_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateFromBiome("unknown_biome"));
    }

    [Test]
    public void CreateFromBiome_AppliesDefaultValues()
    {
        var context = _service.CreateFromBiome("cave");

        // Cave has climate: cold, lighting: dim as defaults
        Assert.That(context.Climate, Is.EqualTo("cold"));
        Assert.That(context.Lighting, Is.EqualTo("dim"));
    }

    [Test]
    public void CreateFromBiome_AppliesOverrides()
    {
        var overrides = new Dictionary<string, string>
        {
            ["climate"] = "temperate"
        };

        var context = _service.CreateFromBiome("cave", overrides);

        Assert.That(context.Climate, Is.EqualTo("temperate"));
    }

    [Test]
    public void CreateFromBiome_WithConflictingOverride_ThrowsException()
    {
        var overrides = new Dictionary<string, string>
        {
            ["climate"] = "freezing"
        };

        // volcanic + freezing is a hard conflict
        Assert.Throws<InvalidOperationException>(() =>
            _service.CreateFromBiome("volcanic", overrides));
    }

    [Test]
    public void GetBiome_WithValidId_ReturnsDefinition()
    {
        var biome = _service.GetBiome("cave");

        Assert.That(biome, Is.Not.Null);
        Assert.That(biome!.Name, Is.EqualTo("Cave"));
    }

    [Test]
    public void GetBiome_WithInvalidId_ReturnsNull()
    {
        var biome = _service.GetBiome("nonexistent");

        Assert.That(biome, Is.Null);
    }

    [Test]
    public void GetAllBiomes_ReturnsAllBiomes()
    {
        var biomes = _service.GetAllBiomes().ToList();

        Assert.That(biomes, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetValidValues_ExcludesConflictingValues()
    {
        var context = new EnvironmentContext(
            new Dictionary<string, string>
            {
                ["biome"] = "volcanic"
            },
            []);

        var validClimates = _service.GetValidValues("climate", context).ToList();

        // Volcanic cannot have freezing or cold (hard rule)
        Assert.That(validClimates.Select(v => v.Id), Does.Not.Contain("freezing"));
        Assert.That(validClimates.Select(v => v.Id), Does.Not.Contain("cold"));
    }

    [Test]
    public void CreateFromBiome_CollectsDerivedTags()
    {
        var context = _service.CreateFromBiome("cave");

        // Cave biome has implied tags: underground, natural, stone
        Assert.That(context.DerivedTags, Does.Contain("underground"));
        Assert.That(context.DerivedTags, Does.Contain("natural"));
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
                        new CategoryValue { Id = "volcanic", Name = "Volcanic", ImpliedTags = ["volcanic", "danger"] }
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
                        new CategoryValue { Id = "freezing", Name = "Freezing", ImpliedTags = ["freezing", "cold"] },
                        new CategoryValue { Id = "cold", Name = "Cold", ImpliedTags = ["cold"] },
                        new CategoryValue { Id = "temperate", Name = "Temperate", ImpliedTags = [] },
                        new CategoryValue { Id = "scorching", Name = "Scorching", ImpliedTags = ["scorching", "hot"] }
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
                        new CategoryValue { Id = "dim", Name = "Dim", ImpliedTags = ["dim"] },
                        new CategoryValue { Id = "bright", Name = "Bright", ImpliedTags = ["bright"] }
                    ]
                },
                ["era"] = new EnvironmentCategory
                {
                    Id = "era",
                    Name = "Era",
                    IsRequired = false,
                    Values =
                    [
                        new CategoryValue { Id = "ancient", Name = "Ancient", ImpliedTags = ["ancient"] }
                    ]
                },
                ["condition"] = new EnvironmentCategory
                {
                    Id = "condition",
                    Name = "Condition",
                    IsRequired = false,
                    Values =
                    [
                        new CategoryValue { Id = "pristine", Name = "Pristine", ImpliedTags = ["pristine"] }
                    ]
                }
            },
            ExclusionRules =
            [
                new CategoryExclusionRule
                {
                    Id = "volcanic_freezing",
                    Reason = "Volcanic areas cannot be freezing",
                    Category1 = "biome",
                    Values1 = ["volcanic"],
                    Category2 = "climate",
                    Values2 = ["freezing", "cold"],
                    IsHardRule = true
                },
                new CategoryExclusionRule
                {
                    Id = "ancient_pristine",
                    Reason = "Ancient structures unlikely pristine",
                    Category1 = "era",
                    Values1 = ["ancient"],
                    Category2 = "condition",
                    Values2 = ["pristine"],
                    IsHardRule = false
                }
            ]
        };
    }

    private static BiomeConfiguration CreateTestBiomeConfig()
    {
        return new BiomeConfiguration
        {
            Version = "1.0",
            Biomes = new Dictionary<string, BiomeConfigurationDto>
            {
                ["cave"] = new BiomeConfigurationDto
                {
                    Id = "cave",
                    Name = "Cave",
                    Description = "Natural underground cavern",
                    DefaultCategoryValues = new Dictionary<string, string>
                    {
                        ["climate"] = "cold",
                        ["lighting"] = "dim"
                    },
                    ImpliedTags = ["underground", "natural", "stone"]
                },
                ["volcanic"] = new BiomeConfigurationDto
                {
                    Id = "volcanic",
                    Name = "Volcanic",
                    Description = "Volcanic region",
                    DefaultCategoryValues = new Dictionary<string, string>
                    {
                        ["climate"] = "scorching",
                        ["lighting"] = "dim"
                    },
                    ImpliedTags = ["volcanic", "danger", "heat"]
                }
            }
        };
    }
}
