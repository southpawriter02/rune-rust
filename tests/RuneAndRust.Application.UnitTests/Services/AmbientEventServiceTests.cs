using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class AmbientEventServiceTests
{
    private Mock<ILogger<DescriptorService>> _descriptorLoggerMock = null!;
    private Mock<ILogger<AmbientEventService>> _ambientLoggerMock = null!;
    private DescriptorService _descriptorService = null!;
    private AmbientEventService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _descriptorLoggerMock = new Mock<ILogger<DescriptorService>>();
        _ambientLoggerMock = new Mock<ILogger<AmbientEventService>>();

        var pools = CreateTestPools();
        var theme = CreateTestTheme();
        var ambientConfig = CreateTestAmbientConfig();

        _descriptorService = new DescriptorService(pools, theme, _descriptorLoggerMock.Object);
        _service = new AmbientEventService(_descriptorService, ambientConfig, _ambientLoggerMock.Object);
    }

    [Test]
    public void TryGenerateEvent_OnCooldown_ReturnsNoEvent()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.Exploration,
            TimeSinceLastEvent = TimeSpan.FromSeconds(10),
            InCombat = false
        };

        var result = _service.TryGenerateEvent(context);

        Assert.That(result.HasEvent, Is.False);
    }

    [Test]
    public void TryGenerateEvent_DuringCombat_SuppressesNonCombatEvents()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.Exploration,
            TimeSinceLastEvent = TimeSpan.FromMinutes(5),
            InCombat = true
        };

        var result = _service.TryGenerateEvent(context);

        Assert.That(result.HasEvent, Is.False);
    }

    [Test]
    public void ForceGenerateEvent_AlwaysReturnsEvent()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.Exploration,
            TimeSinceLastEvent = TimeSpan.FromMinutes(5),
            InCombat = false
        };

        var result = _service.ForceGenerateEvent(context);

        Assert.That(result.HasEvent, Is.True);
        Assert.That(result.Description, Is.Not.Empty);
    }

    [Test]
    public void GetEventOfType_Sound_ReturnsSoundEvent()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.Exploration
        };

        var result = _service.GetEventOfType(AmbientEventType.Sound, context);

        Assert.That(result.EventType, Is.EqualTo(AmbientEventType.Sound));
        Assert.That(result.Description, Is.Not.Empty);
    }

    [Test]
    public void GetEventOfType_Visual_ReturnsVisualEvent()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.Periodic
        };

        var result = _service.GetEventOfType(AmbientEventType.Visual, context);

        Assert.That(result.EventType, Is.EqualTo(AmbientEventType.Visual));
        Assert.That(result.Description, Is.Not.Empty);
    }

    [Test]
    public void GetEventOfType_Creature_ReturnsCreatureEvent()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.RoomEntry
        };

        var result = _service.GetEventOfType(AmbientEventType.Creature, context);

        Assert.That(result.EventType, Is.EqualTo(AmbientEventType.Creature));
        Assert.That(result.Description, Is.Not.Empty);
    }

    [Test]
    public void GetEventOfType_Environmental_ReturnsEnvironmentalEvent()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.PlayerAction
        };

        var result = _service.GetEventOfType(AmbientEventType.Environmental, context);

        Assert.That(result.EventType, Is.EqualTo(AmbientEventType.Environmental));
        Assert.That(result.Description, Is.Not.Empty);
    }

    [Test]
    public void GetEventOfType_CaveBiome_UsesCavePool()
    {
        var environment = CreateEnvironmentContext("cave", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.Exploration
        };

        var result = _service.GetEventOfType(AmbientEventType.Sound, context);

        // Should use sound_cave pool
        Assert.That(result.Description, Does.Contain("cave"));
    }

    [Test]
    public void AmbientEvent_None_HasNoEvent()
    {
        var noEvent = AmbientEvent.None;

        Assert.That(noEvent.HasEvent, Is.False);
        Assert.That(noEvent.Description, Is.Empty);
    }

    [Test]
    public void AmbientEvent_WithDescription_HasEvent()
    {
        var withEvent = new AmbientEvent
        {
            EventType = AmbientEventType.Sound,
            Description = "A distant sound echoes",
            Intensity = 0.5f,
            IsInterruptive = false
        };

        Assert.That(withEvent.HasEvent, Is.True);
        Assert.That(withEvent.Description, Is.EqualTo("A distant sound echoes"));
    }

    [Test]
    public void ForceGenerateEvent_WithSwampBiome_ReturnsSwampEvent()
    {
        var environment = CreateEnvironmentContext("swamp", "temperate", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.Exploration,
            TimeSinceLastEvent = TimeSpan.FromMinutes(5),
            InCombat = false
        };

        var result = _service.ForceGenerateEvent(context);

        Assert.That(result.HasEvent, Is.True);
    }

    [Test]
    public void GetEventOfType_WithIntensity_SetsAppropriateIntensity()
    {
        var environment = CreateEnvironmentContext("dungeon", "cold", "dim");
        var context = new AmbientEventContext
        {
            Environment = environment,
            Trigger = AmbientEventTrigger.Exploration,
            InCombat = false
        };

        var result = _service.GetEventOfType(AmbientEventType.Creature, context);

        // Creature events have higher base intensity
        Assert.That(result.Intensity, Is.GreaterThan(0f));
        Assert.That(result.Intensity, Is.LessThanOrEqualTo(1f));
    }

    private static IReadOnlyDictionary<string, DescriptorPool> CreateTestPools()
    {
        return new Dictionary<string, DescriptorPool>
        {
            ["ambient.sound"] = CreatePool("sound", [
                ("distant_sound", "A distant sound echoes through the corridors", 20),
                ("footsteps", "Footsteps echo from somewhere you can't place", 20)
            ]),
            ["ambient.sound_cave"] = CreatePool("sound_cave", [
                ("drip_echo", "A cave drip echoes in the darkness", 25),
                ("bat_screech", "A bat's screech pierces the cave silence", 20)
            ]),
            ["ambient.sound_dungeon"] = CreatePool("sound_dungeon", [
                ("chain_rattle", "Chains rattle somewhere in the dungeon darkness", 25)
            ]),
            ["ambient.visual"] = CreatePool("visual", [
                ("shadow_move", "Shadows shift at the corner of your vision", 25),
                ("light_flicker", "The light flickers for a moment", 25)
            ]),
            ["ambient.creature"] = CreatePool("creature", [
                ("rat_scatter", "Rats scatter as you approach", 25),
                ("spider_web", "You brush through an invisible spider web", 20)
            ]),
            ["ambient.creature_swamp"] = CreatePool("creature_swamp", [
                ("frog_leap", "A frog leaps into murky water", 25)
            ]),
            ["ambient.environmental"] = CreatePool("environmental", [
                ("dust_settle", "Ancient dust settles around you", 25),
                ("cold_draft", "A cold draft washes over you", 25)
            ])
        };
    }

    private static DescriptorPool CreatePool(string id, (string id, string text, int weight)[] descriptors)
    {
        return new DescriptorPool
        {
            Id = id,
            Name = id,
            Descriptors = descriptors.Select(d => new Descriptor
            {
                Id = d.id,
                Text = d.text,
                Weight = d.weight
            }).ToList()
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
                    Description = "Standard theme"
                }
            }
        };
    }

    private static AmbientEventConfiguration CreateTestAmbientConfig()
    {
        return new AmbientEventConfiguration
        {
            BaseProbability = 0.15f,
            CooldownSeconds = 30,
            EventPools = new Dictionary<string, AmbientEventPool>
            {
                ["sound"] = new AmbientEventPool
                {
                    Id = "sound",
                    Name = "Sound Events",
                    ValidBiomes = [],
                    ValidTriggers = ["exploration", "periodic", "room_entry"]
                },
                ["sound_cave"] = new AmbientEventPool
                {
                    Id = "sound_cave",
                    Name = "Cave Sound Events",
                    ValidBiomes = ["cave"],
                    ValidTriggers = ["exploration", "periodic", "room_entry"]
                },
                ["visual"] = new AmbientEventPool
                {
                    Id = "visual",
                    Name = "Visual Events",
                    ValidBiomes = [],
                    ValidTriggers = ["exploration", "periodic"]
                },
                ["creature"] = new AmbientEventPool
                {
                    Id = "creature",
                    Name = "Creature Events",
                    ValidBiomes = [],
                    ValidTriggers = ["exploration", "room_entry"]
                },
                ["environmental"] = new AmbientEventPool
                {
                    Id = "environmental",
                    Name = "Environmental Events",
                    ValidBiomes = [],
                    ValidTriggers = ["exploration", "periodic", "player_action"]
                }
            }
        };
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
}
