using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

[TestFixture]
public class ProgressionDefinitionTests
{
    [Test]
    public void Default_ReturnsExpectedValues()
    {
        var progression = ProgressionDefinition.Default;

        Assert.Multiple(() =>
        {
            Assert.That(progression.ExperienceTerminology, Is.EqualTo("XP"));
            Assert.That(progression.LevelTerminology, Is.EqualTo("Level"));
            Assert.That(progression.MaxLevel, Is.EqualTo(20));
            Assert.That(progression.CurveType, Is.EqualTo(ProgressionCurve.Linear));
            Assert.That(progression.BaseXpRequirement, Is.EqualTo(100));
            Assert.That(progression.XpMultiplier, Is.EqualTo(1.5f));
            Assert.That(progression.HealOnLevelUp, Is.True);
        });
    }

    [Test]
    public void GetExperienceForLevel_Level1_ReturnsZero()
    {
        var progression = ProgressionDefinition.Default;

        var xp = progression.GetExperienceForLevel(1);

        Assert.That(xp, Is.EqualTo(0));
    }

    [Test]
    public void GetExperienceForLevel_LevelBelowOne_ReturnsZero()
    {
        var progression = ProgressionDefinition.Default;

        Assert.Multiple(() =>
        {
            Assert.That(progression.GetExperienceForLevel(0), Is.EqualTo(0));
            Assert.That(progression.GetExperienceForLevel(-5), Is.EqualTo(0));
        });
    }

    [Test]
    public void GetExperienceForLevel_LinearCurve_CalculatesCorrectly()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100
        };

        // Linear formula: level * baseXp (matches v0.0.8b: Level N = N * 100 XP)
        Assert.Multiple(() =>
        {
            Assert.That(progression.GetExperienceForLevel(2), Is.EqualTo(200));  // 2 * 100
            Assert.That(progression.GetExperienceForLevel(3), Is.EqualTo(300));  // 3 * 100
            Assert.That(progression.GetExperienceForLevel(5), Is.EqualTo(500));  // 5 * 100
            Assert.That(progression.GetExperienceForLevel(10), Is.EqualTo(1000)); // 10 * 100
        });
    }

    [Test]
    public void GetExperienceForLevel_ExponentialCurve_CalculatesCorrectly()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Exponential,
            BaseXpRequirement = 100,
            XpMultiplier = 1.5f
        };

        // Level 2 = 100
        // Level 3 = 100 + 150 = 250
        // Level 4 = 100 + 150 + 225 = 475
        Assert.Multiple(() =>
        {
            Assert.That(progression.GetExperienceForLevel(2), Is.EqualTo(100));
            Assert.That(progression.GetExperienceForLevel(3), Is.EqualTo(250));
            Assert.That(progression.GetExperienceForLevel(4), Is.EqualTo(475));
        });
    }

    [Test]
    public void GetExperienceForLevel_CustomCurve_UsesOverride()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Custom,
            BaseXpRequirement = 100,
            LevelOverrides = new Dictionary<int, LevelDefinition>
            {
                [2] = new LevelDefinition { Level = 2, XpRequired = 50 },
                [3] = new LevelDefinition { Level = 3, XpRequired = 150 },
                [5] = new LevelDefinition { Level = 5, XpRequired = 500 }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(progression.GetExperienceForLevel(2), Is.EqualTo(50));
            Assert.That(progression.GetExperienceForLevel(3), Is.EqualTo(150));
            Assert.That(progression.GetExperienceForLevel(4), Is.EqualTo(400)); // Falls back to linear: 4*100
            Assert.That(progression.GetExperienceForLevel(5), Is.EqualTo(500));
        });
    }

    [Test]
    public void GetExperienceForLevel_WithOverrideOnNonCustomCurve_UsesOverride()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100,
            LevelOverrides = new Dictionary<int, LevelDefinition>
            {
                [5] = new LevelDefinition { Level = 5, XpRequired = 999 }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(progression.GetExperienceForLevel(4), Is.EqualTo(400)); // Linear: 4*100
            Assert.That(progression.GetExperienceForLevel(5), Is.EqualTo(999)); // Override
            Assert.That(progression.GetExperienceForLevel(6), Is.EqualTo(600)); // Linear: 6*100
        });
    }

    [Test]
    public void GetExperienceForLevel_BeyondMaxLevel_CapsAtMaxLevel()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100,
            MaxLevel = 5
        };

        var xpAtMax = progression.GetExperienceForLevel(5);
        var xpBeyondMax = progression.GetExperienceForLevel(10);

        Assert.That(xpBeyondMax, Is.EqualTo(xpAtMax));
    }

    [Test]
    public void GetLevelForExperience_ZeroXp_ReturnsLevel1()
    {
        var progression = ProgressionDefinition.Default;

        var level = progression.GetLevelForExperience(0);

        Assert.That(level, Is.EqualTo(1));
    }

    [Test]
    public void GetLevelForExperience_LinearCurve_ReturnsCorrectLevel()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100,
            MaxLevel = 20
        };

        // With formula: Level N = N * 100 XP
        // Level 1 = 100 XP (0-199 XP = Level 1), Level 2 = 200, Level 3 = 300, etc.
        Assert.Multiple(() =>
        {
            Assert.That(progression.GetLevelForExperience(0), Is.EqualTo(1));
            Assert.That(progression.GetLevelForExperience(199), Is.EqualTo(1));
            Assert.That(progression.GetLevelForExperience(200), Is.EqualTo(2));
            Assert.That(progression.GetLevelForExperience(299), Is.EqualTo(2));
            Assert.That(progression.GetLevelForExperience(300), Is.EqualTo(3));
            Assert.That(progression.GetLevelForExperience(500), Is.EqualTo(5));
        });
    }

    [Test]
    public void GetLevelForExperience_RespectsMaxLevel()
    {
        var progression = new ProgressionDefinition
        {
            CurveType = ProgressionCurve.Linear,
            BaseXpRequirement = 100,
            MaxLevel = 5
        };

        var level = progression.GetLevelForExperience(999999);

        Assert.That(level, Is.EqualTo(5));
    }

    [Test]
    public void GetStatBonusesForLevel_NoOverride_ReturnsDefault()
    {
        var progression = new ProgressionDefinition
        {
            DefaultStatBonuses = new StatBonusConfig
            {
                MaxHealth = 10,
                Attack = 2,
                Defense = 3
            }
        };

        var bonuses = progression.GetStatBonusesForLevel(5);

        Assert.Multiple(() =>
        {
            Assert.That(bonuses.MaxHealth, Is.EqualTo(10));
            Assert.That(bonuses.Attack, Is.EqualTo(2));
            Assert.That(bonuses.Defense, Is.EqualTo(3));
        });
    }

    [Test]
    public void GetStatBonusesForLevel_WithLevelOverride_ReturnsOverride()
    {
        var progression = new ProgressionDefinition
        {
            DefaultStatBonuses = new StatBonusConfig { MaxHealth = 5, Attack = 1, Defense = 1 },
            LevelOverrides = new Dictionary<int, LevelDefinition>
            {
                [5] = new LevelDefinition
                {
                    Level = 5,
                    StatBonuses = new StatBonusConfig { MaxHealth = 20, Attack = 5, Defense = 5 }
                }
            }
        };

        var normalBonuses = progression.GetStatBonusesForLevel(4);
        var overrideBonuses = progression.GetStatBonusesForLevel(5);

        Assert.Multiple(() =>
        {
            Assert.That(normalBonuses.MaxHealth, Is.EqualTo(5));
            Assert.That(overrideBonuses.MaxHealth, Is.EqualTo(20));
        });
    }

    [Test]
    public void GetStatBonusesForLevel_WithClassGrowthRates_UsesClassRates()
    {
        var progression = new ProgressionDefinition
        {
            DefaultStatBonuses = new StatBonusConfig { MaxHealth = 5, Attack = 1, Defense = 1 }
        };

        var classGrowth = new LevelStatModifiers(15, 3, 2);
        var bonuses = progression.GetStatBonusesForLevel(5, classGrowth);

        Assert.Multiple(() =>
        {
            Assert.That(bonuses.MaxHealth, Is.EqualTo(15));
            Assert.That(bonuses.Attack, Is.EqualTo(3));
            Assert.That(bonuses.Defense, Is.EqualTo(2));
        });
    }

    [Test]
    public void GetStatBonusesForLevel_LevelOverrideTakesPrecedenceOverClassGrowth()
    {
        var progression = new ProgressionDefinition
        {
            DefaultStatBonuses = new StatBonusConfig { MaxHealth = 5, Attack = 1, Defense = 1 },
            LevelOverrides = new Dictionary<int, LevelDefinition>
            {
                [5] = new LevelDefinition
                {
                    Level = 5,
                    StatBonuses = new StatBonusConfig { MaxHealth = 50, Attack = 10, Defense = 10 }
                }
            }
        };

        var classGrowth = new LevelStatModifiers(15, 3, 2);
        var bonuses = progression.GetStatBonusesForLevel(5, classGrowth);

        // Level override should take precedence
        Assert.That(bonuses.MaxHealth, Is.EqualTo(50));
    }

    [Test]
    public void CustomTerminology_CanBeConfigured()
    {
        var progression = new ProgressionDefinition
        {
            ExperienceTerminology = "Glory",
            LevelTerminology = "Rank"
        };

        Assert.Multiple(() =>
        {
            Assert.That(progression.ExperienceTerminology, Is.EqualTo("Glory"));
            Assert.That(progression.LevelTerminology, Is.EqualTo("Rank"));
        });
    }

    [Test]
    public void HealOnLevelUp_DefaultsToTrue()
    {
        var progression = new ProgressionDefinition();

        Assert.That(progression.HealOnLevelUp, Is.True);
    }

    [Test]
    public void HealOnLevelUp_CanBeDisabled()
    {
        var progression = new ProgressionDefinition { HealOnLevelUp = false };

        Assert.That(progression.HealOnLevelUp, Is.False);
    }
}
