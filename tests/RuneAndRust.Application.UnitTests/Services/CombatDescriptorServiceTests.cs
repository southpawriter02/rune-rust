using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class CombatDescriptorServiceTests
{
    private Mock<ILogger<DescriptorService>> _descriptorLoggerMock = null!;
    private Mock<ILogger<CombatDescriptorService>> _combatLoggerMock = null!;
    private DescriptorService _descriptorService = null!;
    private CombatDescriptorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _descriptorLoggerMock = new Mock<ILogger<DescriptorService>>();
        _combatLoggerMock = new Mock<ILogger<CombatDescriptorService>>();

        var pools = CreateTestPools();
        var theme = CreateTestTheme();

        _descriptorService = new DescriptorService(pools, theme, _descriptorLoggerMock.Object);
        _service = new CombatDescriptorService(_descriptorService, _combatLoggerMock.Object);
    }

    [Test]
    public void GetHitDescription_WithWeaponType_UsesWeaponPool()
    {
        var context = new CombatDescriptorContext
        {
            WeaponType = "sword",
            DamagePercent = 0.35,
            AttackerName = "Hero",
            TargetName = "Goblin"
        };

        var result = _service.GetHitDescription(context);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Hero"));
        Assert.That(result, Does.Contain("Goblin"));
        // Sword-specific text (slashes)
        Assert.That(result, Does.Contain("slashes"));
    }

    [Test]
    public void GetHitDescription_WithoutWeaponType_UsesGenericPool()
    {
        var context = new CombatDescriptorContext
        {
            WeaponType = null,
            DamagePercent = 0.35,
            AttackerName = "Hero",
            TargetName = "Goblin"
        };

        var result = _service.GetHitDescription(context);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Hero"));
        Assert.That(result, Does.Contain("Goblin"));
        // Generic text (strikes)
        Assert.That(result, Does.Contain("strikes"));
    }

    [Test]
    public void GetHitDescription_WhenCritical_UsesCriticalPool()
    {
        var context = new CombatDescriptorContext
        {
            WeaponType = "sword",
            DamagePercent = 0.75,
            IsCritical = true,
            AttackerName = "Hero",
            TargetName = "Goblin"
        };

        var result = _service.GetHitDescription(context);

        Assert.That(result, Is.Not.Empty);
        // Critical hit text (devastating strike)
        Assert.That(result, Does.Contain("devastating strike"));
    }

    [Test]
    public void GetMissDescription_WhenFumble_UsesFumblePool()
    {
        var context = new CombatDescriptorContext
        {
            WeaponType = "sword",
            IsFumble = true,
            AttackerName = "Hero"
        };

        var result = _service.GetMissDescription(context);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Hero"));
        // Fumble text (stumbles)
        Assert.That(result, Does.Contain("stumbles"));
    }

    [Test]
    public void GetMissDescription_NormalMiss_UsesMissPool()
    {
        var context = new CombatDescriptorContext
        {
            WeaponType = "sword",
            IsFumble = false,
            AttackerName = "Hero"
        };

        var result = _service.GetMissDescription(context);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Hero"));
        // Miss text (swings wide)
        Assert.That(result, Does.Contain("swings wide"));
    }

    [Test]
    public void GetDeathDescription_ByCreatureCategory_UsesCorrectPool()
    {
        var context = new DeathDescriptorContext
        {
            CreatureName = "Skeleton",
            CreatureTags = ["undead", "monster"]
        };

        var result = _service.GetDeathDescription(context);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Skeleton"));
        // Undead-specific death text (bones)
        Assert.That(result, Does.Contain("bones"));
    }

    [Test]
    public void GetDeathDescription_WithCriticalKill_IncludesFlair()
    {
        var context = new DeathDescriptorContext
        {
            CreatureName = "Goblin",
            CreatureTags = ["humanoid", "monster"],
            WasCriticalKill = true
        };

        var result = _service.GetDeathDescription(context);

        Assert.That(result, Is.Not.Empty);
        // Critical kill flair
        Assert.That(result, Does.Contain("devastating blow"));
    }

    [Test]
    public void GetNearDeathDescription_AtLowHealth_ReturnsWarning()
    {
        var result = _service.GetNearDeathDescription(0.20);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("wounds"));
    }

    [Test]
    public void GetNearDeathDescription_AtNormalHealth_ReturnsNull()
    {
        var result = _service.GetNearDeathDescription(0.50);

        Assert.That(result, Is.Null);
    }

    private static IReadOnlyDictionary<string, DescriptorPool> CreateTestPools()
    {
        return new Dictionary<string, DescriptorPool>
        {
            ["combat.hit_sword"] = new DescriptorPool
            {
                Id = "hit_sword",
                Name = "Sword Hits",
                Descriptors =
                [
                    new Descriptor { Id = "slashes", Text = "slashes across", Weight = 100 }
                ]
            },
            ["combat.hit_descriptions"] = new DescriptorPool
            {
                Id = "hit_descriptions",
                Name = "Generic Hits",
                Descriptors =
                [
                    new Descriptor { Id = "strikes", Text = "strikes", Weight = 100 }
                ]
            },
            ["combat.critical_hit"] = new DescriptorPool
            {
                Id = "critical_hit",
                Name = "Critical Hits",
                Descriptors =
                [
                    new Descriptor { Id = "devastating", Text = "lands a devastating strike on", Weight = 100 }
                ]
            },
            ["combat.miss_descriptions"] = new DescriptorPool
            {
                Id = "miss_descriptions",
                Name = "Misses",
                Descriptors =
                [
                    new Descriptor { Id = "wide", Text = "swings wide", Weight = 100 }
                ]
            },
            ["combat.fumble_descriptions"] = new DescriptorPool
            {
                Id = "fumble_descriptions",
                Name = "Fumbles",
                Descriptors =
                [
                    new Descriptor { Id = "stumbles", Text = "stumbles badly", Weight = 100 }
                ]
            },
            ["combat.death_undead"] = new DescriptorPool
            {
                Id = "death_undead",
                Name = "Undead Deaths",
                Descriptors =
                [
                    new Descriptor { Id = "bones", Text = "collapses into a heap of bones", Weight = 100 }
                ]
            },
            ["combat.death_humanoid"] = new DescriptorPool
            {
                Id = "death_humanoid",
                Name = "Humanoid Deaths",
                Descriptors =
                [
                    new Descriptor { Id = "crumples", Text = "crumples to the ground", Weight = 100 }
                ]
            },
            ["combat.critical_kill"] = new DescriptorPool
            {
                Id = "critical_kill",
                Name = "Critical Kill Flair",
                Descriptors =
                [
                    new Descriptor { Id = "devastating", Text = "With a devastating blow,", Weight = 100 }
                ]
            },
            ["combat.near_death_warning"] = new DescriptorPool
            {
                Id = "near_death_warning",
                Name = "Near Death Warnings",
                Descriptors =
                [
                    new Descriptor { Id = "wounds", Text = "Your wounds are becoming severe.", Weight = 100 }
                ]
            },
            ["combat.near_death_critical"] = new DescriptorPool
            {
                Id = "near_death_critical",
                Name = "Critical Warnings",
                Descriptors =
                [
                    new Descriptor { Id = "collapse", Text = "You are on the verge of collapse!", Weight = 100 }
                ]
            },
            ["combat.damage_tier_solid"] = new DescriptorPool
            {
                Id = "damage_tier_solid",
                Name = "Solid Damage",
                Descriptors =
                [
                    new Descriptor { Id = "strikes_true", Text = "striking true", Weight = 100 }
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
}
