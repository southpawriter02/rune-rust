using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class AbilityDescriptorServiceTests
{
    private Mock<ILogger<DescriptorService>> _descriptorLoggerMock = null!;
    private Mock<ILogger<AbilityDescriptorService>> _abilityLoggerMock = null!;
    private DescriptorService _descriptorService = null!;
    private AbilityDescriptorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _descriptorLoggerMock = new Mock<ILogger<DescriptorService>>();
        _abilityLoggerMock = new Mock<ILogger<AbilityDescriptorService>>();

        var pools = CreateTestPools();
        var theme = CreateTestTheme();

        _descriptorService = new DescriptorService(pools, theme, _descriptorLoggerMock.Object);
        _service = new AbilityDescriptorService(_descriptorService, _abilityLoggerMock.Object);
    }

    [Test]
    public void GetActivationDescription_WithDamageType_UsesDamageTypePool()
    {
        var context = new AbilityDescriptorContext
        {
            AbilityId = "fireball",
            DamageType = "fire",
            CasterName = "Wizard"
        };

        var result = _service.GetActivationDescription(context);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Wizard"));
        // Fire-specific activation text
        Assert.That(result, Does.Contain("flames"));
    }

    [Test]
    public void GetImpactDescription_WithFireDamage_UsesFireEffects()
    {
        var context = new AbilityDescriptorContext
        {
            AbilityId = "fireball",
            DamageType = "fire",
            CasterName = "Wizard",
            TargetName = "Goblin"
        };

        var result = _service.GetImpactDescription(context);

        Assert.That(result, Is.Not.Empty);
        // Fire impact text
        Assert.That(result, Does.Contain("engulfs"));
    }

    [Test]
    public void GetAftermathDescription_WithAppliedEffects_ReturnsDescription()
    {
        var context = new AbilityDescriptorContext
        {
            AbilityId = "fireball",
            DamageType = "fire",
            CasterName = "Wizard",
            AppliedEffects = ["burning"]
        };

        var result = _service.GetAftermathDescription(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("linger"));
    }

    [Test]
    public void GetAftermathDescription_WithNoEffects_ReturnsNull()
    {
        var context = new AbilityDescriptorContext
        {
            AbilityId = "fireball",
            DamageType = "fire",
            CasterName = "Wizard",
            AppliedEffects = []
        };

        var result = _service.GetAftermathDescription(context);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetStatusEffectAppliedDescription_FormatsCorrectly()
    {
        var result = _service.GetStatusEffectAppliedDescription("burning", "Goblin");

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Goblin"));
        Assert.That(result, Does.Contain("flames"));
    }

    [Test]
    public void GetStatusEffectRemovedDescription_FormatsCorrectly()
    {
        var result = _service.GetStatusEffectRemovedDescription("burning", "Goblin");

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Goblin"));
        Assert.That(result, Does.Contain("fades"));
    }

    [Test]
    public void GetCompleteAbilityNarrative_CombinesAllPhases()
    {
        var context = new AbilityDescriptorContext
        {
            AbilityId = "fireball",
            DamageType = "fire",
            CasterName = "Wizard",
            TargetName = "Goblin",
            AppliedEffects = ["burning"]
        };

        var result = _service.GetCompleteAbilityNarrative(context);

        Assert.That(result, Is.Not.Empty);
        // Should contain activation, impact, and aftermath
        Assert.That(result, Does.Contain("flames"));
        Assert.That(result, Does.Contain("engulfs"));
        Assert.That(result, Does.Contain("linger"));
    }

    private static IReadOnlyDictionary<string, DescriptorPool> CreateTestPools()
    {
        return new Dictionary<string, DescriptorPool>
        {
            ["abilities.activation_fire"] = new DescriptorPool
            {
                Id = "activation_fire",
                Name = "Fire Activation",
                Descriptors =
                [
                    new Descriptor { Id = "flames", Text = "{caster} gathers flames in their palm", Weight = 100 }
                ]
            },
            ["abilities.effects_fire"] = new DescriptorPool
            {
                Id = "effects_fire",
                Name = "Fire Effects",
                Descriptors =
                [
                    new Descriptor { Id = "engulfs", Text = "Fire engulfs {target}!", Weight = 100 }
                ]
            },
            ["abilities.duration"] = new DescriptorPool
            {
                Id = "duration",
                Name = "Ability Duration",
                Descriptors =
                [
                    new Descriptor { Id = "linger", Text = "The effects linger.", Weight = 100 }
                ]
            },
            ["abilities.casting"] = new DescriptorPool
            {
                Id = "casting",
                Name = "Generic Casting",
                Descriptors =
                [
                    new Descriptor { Id = "channels", Text = "{caster} channels arcane power", Weight = 100 }
                ]
            },
            ["abilities.impact"] = new DescriptorPool
            {
                Id = "impact",
                Name = "Generic Impact",
                Descriptors =
                [
                    new Descriptor { Id = "takes_effect", Text = "The ability takes effect", Weight = 100 }
                ]
            },
            ["status.applied_burning"] = new DescriptorPool
            {
                Id = "applied_burning",
                Name = "Burning Applied",
                Descriptors =
                [
                    new Descriptor { Id = "burning", Text = "{target} is engulfed in flames!", Weight = 100 }
                ]
            },
            ["status.removed_burning"] = new DescriptorPool
            {
                Id = "removed_burning",
                Name = "Burning Removed",
                Descriptors =
                [
                    new Descriptor { Id = "fades", Text = "The flames on {target} fades away.", Weight = 100 }
                ]
            },
            ["status.applied_generic"] = new DescriptorPool
            {
                Id = "applied_generic",
                Name = "Generic Applied",
                Descriptors =
                [
                    new Descriptor { Id = "affected", Text = "{target} is affected!", Weight = 100 }
                ]
            },
            ["status.removed_generic"] = new DescriptorPool
            {
                Id = "removed_generic",
                Name = "Generic Removed",
                Descriptors =
                [
                    new Descriptor { Id = "wears_off", Text = "The effect on {target} wears off.", Weight = 100 }
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
