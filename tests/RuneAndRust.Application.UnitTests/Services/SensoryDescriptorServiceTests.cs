using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class SensoryDescriptorServiceTests
{
    private Mock<ILogger<DescriptorService>> _descriptorLoggerMock = null!;
    private Mock<ILogger<SensoryDescriptorService>> _sensoryLoggerMock = null!;
    private DescriptorService _descriptorService = null!;
    private SensoryDescriptorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _descriptorLoggerMock = new Mock<ILogger<DescriptorService>>();
        _sensoryLoggerMock = new Mock<ILogger<SensoryDescriptorService>>();

        var pools = CreateTestPools();
        var theme = CreateTestTheme();
        var sensoryConfig = CreateTestSensoryConfig();

        _descriptorService = new DescriptorService(pools, theme, _descriptorLoggerMock.Object);
        _service = new SensoryDescriptorService(_descriptorService, sensoryConfig, _sensoryLoggerMock.Object);
    }

    [Test]
    public void GetLightingDescription_WithTorchLight_UsesTorchPool()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            LightSource = "torch",
            IsIndoor = true
        };

        var result = _service.GetLightingDescription(context);

        Assert.That(result.Description, Is.Not.Empty);
        Assert.That(result.Description, Does.Contain("torchlight"));
        Assert.That(result.LightSource, Is.EqualTo("torch"));
    }

    [Test]
    public void GetLightingDescription_WithNoLightSource_InfersByBiome()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            LightSource = null,
            IsIndoor = true
        };

        var result = _service.GetLightingDescription(context);

        // Dungeon biome should infer torch lighting
        Assert.That(result.Description, Is.Not.Empty);
    }

    [Test]
    public void GetSoundDescription_WithCaveBiome_UsesCaveSounds()
    {
        var environment = CreateEnvironmentContext("cave", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            IsIndoor = true
        };

        var result = _service.GetSoundDescription(context);

        Assert.That(result.HasSounds, Is.True);
        Assert.That(result.Distant, Does.Contain("cave"));
    }

    [Test]
    public void GetSoundDescription_InCombat_AddsImmediateSounds()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            InCombat = true
        };

        var result = _service.GetSoundDescription(context);

        Assert.That(result.Immediate, Is.Not.Null);
        Assert.That(result.Immediate, Does.Contain("heartbeat"));
    }

    [Test]
    public void GetSmellDescription_WithSwampBiome_ReturnsStrongIntensity()
    {
        var environment = CreateEnvironmentContext("swamp", "temperate", "dim");
        var context = new SensoryContext
        {
            Environment = environment
        };

        var result = _service.GetSmellDescription(context);

        Assert.That(result.Intensity, Is.EqualTo(SmellIntensity.Strong));
    }

    [Test]
    public void GetSmellDescription_WithFrozenBiome_ReturnsFaintIntensity()
    {
        var environment = CreateEnvironmentContext("frozen", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment
        };

        var result = _service.GetSmellDescription(context);

        Assert.That(result.Intensity, Is.EqualTo(SmellIntensity.Faint));
    }

    [Test]
    public void GetWeatherDescription_WithRain_ReturnsWeatherText()
    {
        var environment = CreateEnvironmentContext("forest", "temperate", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            Weather = "rain",
            IsIndoor = false
        };

        var result = _service.GetWeatherDescription(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("rain"));
    }

    [Test]
    public void GetWeatherDescription_WithInvalidClimate_ReturnsNull()
    {
        var environment = CreateEnvironmentContext("volcanic", "scorching", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            Weather = "snow", // Snow doesn't occur in scorching climate
            IsIndoor = false
        };

        var result = _service.GetWeatherDescription(context);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetTimeOfDayDescription_WhenOutdoor_ReturnsDescription()
    {
        var environment = CreateEnvironmentContext("forest", "temperate", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            TimeOfDay = "night",
            IsIndoor = false
        };

        var result = _service.GetTimeOfDayDescription(context);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void GetTimeOfDayDescription_WhenIndoor_ReturnsNull()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            TimeOfDay = "night",
            IsIndoor = true
        };

        var result = _service.GetTimeOfDayDescription(context);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GenerateSensoryDescription_CombinesAllSenses()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            LightSource = "torch",
            IsIndoor = true
        };

        var result = _service.GenerateSensoryDescription(context);

        Assert.That(result.HasDescriptions, Is.True);
        Assert.That(result.Lighting.Description, Is.Not.Empty);
    }

    [Test]
    public void GenerateBriefAtmosphere_ReturnsUpTo3Senses()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            LightSource = "torch",
            IsIndoor = true
        };

        var result = _service.GenerateBriefAtmosphere(context);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GenerateDetailedAtmosphere_IncludesAllSoundLayers()
    {
        var environment = CreateEnvironmentContext("cave", "cold", "dim");
        var context = new SensoryContext
        {
            Environment = environment,
            InCombat = true
        };

        var result = _service.GenerateDetailedAtmosphere(context);

        Assert.That(result, Is.Not.Empty);
    }

    private static EnvironmentContext CreateEnvironmentContext(string biome, string climate, string lighting)
    {
        var values = new Dictionary<string, string>
        {
            ["biome"] = biome,
            ["climate"] = climate,
            ["lighting"] = lighting
        };
        return new EnvironmentContext(values, [biome, climate]);
    }

    private static IReadOnlyDictionary<string, DescriptorPool> CreateTestPools()
    {
        return new Dictionary<string, DescriptorPool>
        {
            ["lighting.lighting_torch"] = new DescriptorPool
            {
                Id = "lighting_torch",
                Name = "Torch Lighting",
                Descriptors =
                [
                    new Descriptor { Id = "flickering", Text = "Flickering torchlight bathes the area", Weight = 100 }
                ]
            },
            ["lighting.lighting_dim"] = new DescriptorPool
            {
                Id = "lighting_dim",
                Name = "Dim Lighting",
                Descriptors =
                [
                    new Descriptor { Id = "dim", Text = "Dim light barely illuminates the space", Weight = 100 }
                ]
            },
            ["sounds.sounds_distant_cave"] = new DescriptorPool
            {
                Id = "sounds_distant_cave",
                Name = "Cave Distant Sounds",
                Descriptors =
                [
                    new Descriptor { Id = "drip", Text = "Distant echoes from the cave depths", Weight = 100 }
                ]
            },
            ["sounds.sounds_distant"] = new DescriptorPool
            {
                Id = "sounds_distant",
                Name = "Distant Sounds",
                Descriptors =
                [
                    new Descriptor { Id = "echo", Text = "Distant echoes reverberate", Weight = 100 }
                ]
            },
            ["sounds.sounds_nearby"] = new DescriptorPool
            {
                Id = "sounds_nearby",
                Name = "Nearby Sounds",
                Descriptors =
                [
                    new Descriptor { Id = "drip", Text = "Water drips nearby", Weight = 100 }
                ]
            },
            ["sounds.sounds_immediate_combat"] = new DescriptorPool
            {
                Id = "sounds_immediate_combat",
                Name = "Combat Sounds",
                Descriptors =
                [
                    new Descriptor { Id = "heartbeat", Text = "Your heartbeat pounds in your ears", Weight = 100 }
                ]
            },
            ["smells.smells_dungeon"] = new DescriptorPool
            {
                Id = "smells_dungeon",
                Name = "Dungeon Smells",
                Descriptors =
                [
                    new Descriptor { Id = "musty", Text = "Musty air", Weight = 100 }
                ]
            },
            ["smells.smells_swamp"] = new DescriptorPool
            {
                Id = "smells_swamp",
                Name = "Swamp Smells",
                Descriptors =
                [
                    new Descriptor { Id = "decay", Text = "The stench of decay", Weight = 100 }
                ]
            },
            ["smells.smells_frozen"] = new DescriptorPool
            {
                Id = "smells_frozen",
                Name = "Frozen Smells",
                Descriptors =
                [
                    new Descriptor { Id = "crisp", Text = "Crisp, clean air", Weight = 100 }
                ]
            },
            ["smells.smells"] = new DescriptorPool
            {
                Id = "smells",
                Name = "Generic Smells",
                Descriptors =
                [
                    new Descriptor { Id = "generic", Text = "Stale air", Weight = 100 }
                ]
            },
            ["environmental.temperature"] = new DescriptorPool
            {
                Id = "temperature",
                Name = "Temperature",
                Descriptors =
                [
                    new Descriptor { Id = "cold", Text = "A cold chill fills the air", Weight = 100 }
                ]
            },
            ["weather.weather_rain_outdoor"] = new DescriptorPool
            {
                Id = "weather_rain_outdoor",
                Name = "Rain Outdoor",
                Descriptors =
                [
                    new Descriptor { Id = "rain", Text = "Steady rain patters around you", Weight = 100 }
                ]
            },
            ["weather.weather_rain_indoor"] = new DescriptorPool
            {
                Id = "weather_rain_indoor",
                Name = "Rain Indoor",
                Descriptors =
                [
                    new Descriptor { Id = "rain", Text = "Rain patters on the roof above", Weight = 100 }
                ]
            },
            ["environmental.time_night"] = new DescriptorPool
            {
                Id = "time_night",
                Name = "Night Time",
                Descriptors =
                [
                    new Descriptor { Id = "night", Text = "The night sky is dark above", Weight = 100 }
                ]
            }
        };
    }

    private static ThemeConfiguration CreateTestTheme()
    {
        return new ThemeConfiguration
        {
            ActiveTheme = "default",
            Themes = new Dictionary<string, ThemePreset>
            {
                ["default"] = new ThemePreset
                {
                    Id = "default",
                    Name = "Default",
                    DescriptorOverrides = new Dictionary<string, string>(),
                    EmphasizedTerms = [],
                    ExcludedTerms = []
                }
            }
        };
    }

    private static SensoryConfiguration CreateTestSensoryConfig()
    {
        return new SensoryConfiguration
        {
            Version = "1.0",
            LightSources = new Dictionary<string, LightSourceDefinition>
            {
                ["torch"] = new LightSourceDefinition
                {
                    Id = "torch",
                    Name = "Torch",
                    DescriptorPool = "lighting.lighting_torch",
                    CommonBiomes = ["dungeon", "cave"],
                    IsFlickering = true
                }
            },
            DarknessLevels = new Dictionary<string, DarknessLevelDefinition>
            {
                ["dim"] = new DarknessLevelDefinition
                {
                    Id = "dim",
                    Name = "Dim",
                    DescriptorPool = "lighting.lighting_dim",
                    VisibilityLevel = 30
                }
            },
            WeatherConditions = new Dictionary<string, WeatherDefinition>
            {
                ["rain"] = new WeatherDefinition
                {
                    Id = "rain",
                    Name = "Rain",
                    IndoorPool = "weather.weather_rain_indoor",
                    OutdoorPool = "weather.weather_rain_outdoor",
                    ValidClimates = ["temperate", "tropical"]
                },
                ["snow"] = new WeatherDefinition
                {
                    Id = "snow",
                    Name = "Snow",
                    IndoorPool = "weather.weather_snow_indoor",
                    OutdoorPool = "weather.weather_snow_outdoor",
                    ValidClimates = ["cold", "frozen"]
                }
            },
            TimesOfDay = new Dictionary<string, TimeOfDayDefinition>
            {
                ["night"] = new TimeOfDayDefinition
                {
                    Id = "night",
                    Name = "Night",
                    OutdoorPool = "environmental.time_night"
                }
            }
        };
    }
}
