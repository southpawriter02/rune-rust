using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for WizardService (v0.3.4b).
/// Tests preview stat calculations and derived stat formulas.
/// </summary>
public class WizardServiceTests
{
    private readonly Mock<IStatCalculationService> _mockStatService;
    private readonly Mock<ILogger<WizardService>> _mockLogger;
    private readonly WizardService _sut;

    public WizardServiceTests()
    {
        _mockStatService = new Mock<IStatCalculationService>();
        _mockLogger = new Mock<ILogger<WizardService>>();

        // Setup default bonus dictionaries
        SetupLineageBonuses();
        SetupArchetypeBonuses();
        SetupDerivedStatFormulas();
        SetupClampBehavior();

        _sut = new WizardService(_mockStatService.Object, _mockLogger.Object);
    }

    #region GetPreviewStats - Base Stats Tests

    [Fact]
    public void GetPreviewStats_NoSelections_ReturnsBaseStats()
    {
        // Arrange
        var context = new WizardContext();

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert
        stats.Should().HaveCount(5);
        stats.Values.Should().OnlyContain(v => v == 5, "all base stats should be 5");
    }

    [Fact]
    public void GetPreviewStats_EmptyContext_AllAttributesFive()
    {
        // Arrange
        var context = new WizardContext();

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert
        stats[CharacterAttribute.Sturdiness].Should().Be(5);
        stats[CharacterAttribute.Might].Should().Be(5);
        stats[CharacterAttribute.Wits].Should().Be(5);
        stats[CharacterAttribute.Will].Should().Be(5);
        stats[CharacterAttribute.Finesse].Should().Be(5);
    }

    #endregion

    #region GetPreviewStats - Lineage Bonus Tests

    [Fact]
    public void GetPreviewStats_HumanLineage_AddsPlusOneToAll()
    {
        // Arrange
        var context = new WizardContext { Lineage = LineageType.Human };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - Human gets +1 to all (5+1 = 6)
        stats[CharacterAttribute.Sturdiness].Should().Be(6);
        stats[CharacterAttribute.Might].Should().Be(6);
        stats[CharacterAttribute.Wits].Should().Be(6);
        stats[CharacterAttribute.Will].Should().Be(6);
        stats[CharacterAttribute.Finesse].Should().Be(6);
    }

    [Fact]
    public void GetPreviewStats_RuneMarkedLineage_AppliesBonuses()
    {
        // Arrange
        var context = new WizardContext { Lineage = LineageType.RuneMarked };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - RuneMarked: +2 Wits, +2 Will, -1 Sturdiness
        stats[CharacterAttribute.Sturdiness].Should().Be(4); // 5 - 1
        stats[CharacterAttribute.Wits].Should().Be(7);       // 5 + 2
        stats[CharacterAttribute.Will].Should().Be(7);       // 5 + 2
        stats[CharacterAttribute.Might].Should().Be(5);      // unchanged
        stats[CharacterAttribute.Finesse].Should().Be(5);    // unchanged
    }

    [Fact]
    public void GetPreviewStats_IronBloodedLineage_AppliesBonuses()
    {
        // Arrange
        var context = new WizardContext { Lineage = LineageType.IronBlooded };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - IronBlooded: +2 Sturdiness, +2 Might, -1 Wits
        stats[CharacterAttribute.Sturdiness].Should().Be(7); // 5 + 2
        stats[CharacterAttribute.Might].Should().Be(7);      // 5 + 2
        stats[CharacterAttribute.Wits].Should().Be(4);       // 5 - 1
        stats[CharacterAttribute.Will].Should().Be(5);       // unchanged
        stats[CharacterAttribute.Finesse].Should().Be(5);    // unchanged
    }

    [Fact]
    public void GetPreviewStats_VargrKinLineage_AppliesBonuses()
    {
        // Arrange
        var context = new WizardContext { Lineage = LineageType.VargrKin };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - VargrKin: +2 Finesse, +2 Wits, -1 Will
        stats[CharacterAttribute.Finesse].Should().Be(7);    // 5 + 2
        stats[CharacterAttribute.Wits].Should().Be(7);       // 5 + 2
        stats[CharacterAttribute.Will].Should().Be(4);       // 5 - 1
        stats[CharacterAttribute.Sturdiness].Should().Be(5); // unchanged
        stats[CharacterAttribute.Might].Should().Be(5);      // unchanged
    }

    #endregion

    #region GetPreviewStats - Archetype Bonus Tests

    [Fact]
    public void GetPreviewStats_WarriorArchetype_AppliesBonuses()
    {
        // Arrange
        var context = new WizardContext
        {
            Lineage = LineageType.Human, // Need lineage first
            Archetype = ArchetypeType.Warrior
        };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - Human (+1 all) + Warrior (+2 STU, +1 MIG)
        stats[CharacterAttribute.Sturdiness].Should().Be(8); // 5 + 1 + 2
        stats[CharacterAttribute.Might].Should().Be(7);      // 5 + 1 + 1
        stats[CharacterAttribute.Wits].Should().Be(6);       // 5 + 1
        stats[CharacterAttribute.Will].Should().Be(6);       // 5 + 1
        stats[CharacterAttribute.Finesse].Should().Be(6);    // 5 + 1
    }

    [Fact]
    public void GetPreviewStats_SkirmisherArchetype_AppliesBonuses()
    {
        // Arrange
        var context = new WizardContext
        {
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Skirmisher
        };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - Human (+1 all) + Skirmisher (+2 FIN, +1 WIT)
        stats[CharacterAttribute.Finesse].Should().Be(8);    // 5 + 1 + 2
        stats[CharacterAttribute.Wits].Should().Be(7);       // 5 + 1 + 1
        stats[CharacterAttribute.Sturdiness].Should().Be(6); // 5 + 1
        stats[CharacterAttribute.Might].Should().Be(6);      // 5 + 1
        stats[CharacterAttribute.Will].Should().Be(6);       // 5 + 1
    }

    [Fact]
    public void GetPreviewStats_AdeptArchetype_AppliesBonuses()
    {
        // Arrange
        var context = new WizardContext
        {
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Adept
        };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - Human (+1 all) + Adept (+2 WIT, +1 WIL)
        stats[CharacterAttribute.Wits].Should().Be(8);       // 5 + 1 + 2
        stats[CharacterAttribute.Will].Should().Be(7);       // 5 + 1 + 1
        stats[CharacterAttribute.Sturdiness].Should().Be(6); // 5 + 1
        stats[CharacterAttribute.Might].Should().Be(6);      // 5 + 1
        stats[CharacterAttribute.Finesse].Should().Be(6);    // 5 + 1
    }

    [Fact]
    public void GetPreviewStats_MysticArchetype_AppliesBonuses()
    {
        // Arrange
        var context = new WizardContext
        {
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Mystic
        };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - Human (+1 all) + Mystic (+2 WIL, +1 STU)
        stats[CharacterAttribute.Will].Should().Be(8);       // 5 + 1 + 2
        stats[CharacterAttribute.Sturdiness].Should().Be(7); // 5 + 1 + 1
        stats[CharacterAttribute.Wits].Should().Be(6);       // 5 + 1
        stats[CharacterAttribute.Might].Should().Be(6);      // 5 + 1
        stats[CharacterAttribute.Finesse].Should().Be(6);    // 5 + 1
    }

    #endregion

    #region GetPreviewStats - Preview Mode Tests

    [Fact]
    public void GetPreviewStats_PreviewLineage_AppliesWhenContextLineageIsNull()
    {
        // Arrange
        var context = new WizardContext(); // No lineage set

        // Act
        var stats = _sut.GetPreviewStats(context, previewLineage: LineageType.Human);

        // Assert - Should apply Human bonuses
        stats.Values.Should().OnlyContain(v => v == 6, "Human adds +1 to all");
    }

    [Fact]
    public void GetPreviewStats_PreviewLineage_IgnoredWhenContextHasLineage()
    {
        // Arrange
        var context = new WizardContext { Lineage = LineageType.Human };

        // Act - Preview IronBlooded but context has Human
        var stats = _sut.GetPreviewStats(context, previewLineage: LineageType.IronBlooded);

        // Assert - Should use Human from context, not IronBlooded preview
        stats.Values.Should().OnlyContain(v => v == 6, "Human is confirmed, preview ignored");
    }

    [Fact]
    public void GetPreviewStats_PreviewArchetype_AppliesWhenContextArchetypeIsNull()
    {
        // Arrange
        var context = new WizardContext { Lineage = LineageType.Human };

        // Act
        var stats = _sut.GetPreviewStats(context, previewArchetype: ArchetypeType.Warrior);

        // Assert - Human (+1 all) + Warrior preview (+2 STU, +1 MIG)
        stats[CharacterAttribute.Sturdiness].Should().Be(8);
        stats[CharacterAttribute.Might].Should().Be(7);
    }

    [Fact]
    public void GetPreviewStats_PreviewArchetype_IgnoredWhenContextHasArchetype()
    {
        // Arrange
        var context = new WizardContext
        {
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior
        };

        // Act - Preview Skirmisher but context has Warrior
        var stats = _sut.GetPreviewStats(context, previewArchetype: ArchetypeType.Skirmisher);

        // Assert - Should use Warrior from context
        stats[CharacterAttribute.Sturdiness].Should().Be(8); // Warrior bonus applied
        stats[CharacterAttribute.Finesse].Should().Be(6);    // No Skirmisher bonus
    }

    #endregion

    #region GetPreviewStats - Combined Bonus Tests

    [Fact]
    public void GetPreviewStats_LineageAndArchetype_StackBonuses()
    {
        // Arrange - RuneMarked (+2 WIT, +2 WIL, -1 STU) + Adept (+2 WIT, +1 WIL)
        var context = new WizardContext
        {
            Lineage = LineageType.RuneMarked,
            Archetype = ArchetypeType.Adept
        };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert
        stats[CharacterAttribute.Wits].Should().Be(9);       // 5 + 2 + 2
        stats[CharacterAttribute.Will].Should().Be(8);       // 5 + 2 + 1
        stats[CharacterAttribute.Sturdiness].Should().Be(4); // 5 - 1
    }

    [Fact]
    public void GetPreviewStats_IronBloodedWarrior_MaxSturdiness()
    {
        // Arrange - IronBlooded (+2 STU, +2 MIG, -1 WIT) + Warrior (+2 STU, +1 MIG)
        var context = new WizardContext
        {
            Lineage = LineageType.IronBlooded,
            Archetype = ArchetypeType.Warrior
        };

        // Act
        var stats = _sut.GetPreviewStats(context);

        // Assert - Sturdiness would be 5+2+2=9, Might would be 5+2+1=8, Wits 5-1=4
        stats[CharacterAttribute.Sturdiness].Should().Be(9);
        stats[CharacterAttribute.Might].Should().Be(8);
        stats[CharacterAttribute.Wits].Should().Be(4);
    }

    #endregion

    #region GetDerivedStats Tests

    [Fact]
    public void GetDerivedStats_CalculatesHPCorrectly()
    {
        // Arrange - HP = 50 + Sturdiness * 10
        var stats = new Dictionary<CharacterAttribute, int>
        {
            { CharacterAttribute.Sturdiness, 5 },
            { CharacterAttribute.Might, 5 },
            { CharacterAttribute.Wits, 5 },
            { CharacterAttribute.Will, 5 },
            { CharacterAttribute.Finesse, 5 }
        };

        // Act
        var derived = _sut.GetDerivedStats(stats, ArchetypeType.Warrior);

        // Assert
        derived.MaxHP.Should().Be(100); // 50 + 5*10
    }

    [Fact]
    public void GetDerivedStats_CalculatesStaminaCorrectly()
    {
        // Arrange - Stamina = 20 + Finesse*5 + Sturdiness*3
        var stats = new Dictionary<CharacterAttribute, int>
        {
            { CharacterAttribute.Sturdiness, 5 },
            { CharacterAttribute.Might, 5 },
            { CharacterAttribute.Wits, 5 },
            { CharacterAttribute.Will, 5 },
            { CharacterAttribute.Finesse, 5 }
        };

        // Act
        var derived = _sut.GetDerivedStats(stats, ArchetypeType.Warrior);

        // Assert
        derived.MaxStamina.Should().Be(60); // 20 + 5*5 + 5*3
    }

    [Fact]
    public void GetDerivedStats_CalculatesAPCorrectly()
    {
        // Arrange - AP = 2 + Wits/4
        var stats = new Dictionary<CharacterAttribute, int>
        {
            { CharacterAttribute.Sturdiness, 5 },
            { CharacterAttribute.Might, 5 },
            { CharacterAttribute.Wits, 8 },
            { CharacterAttribute.Will, 5 },
            { CharacterAttribute.Finesse, 5 }
        };

        // Act
        var derived = _sut.GetDerivedStats(stats, ArchetypeType.Warrior);

        // Assert
        derived.ActionPoints.Should().Be(4); // 2 + 8/4
    }

    [Fact]
    public void GetDerivedStats_HighStats_CalculatesCorrectly()
    {
        // Arrange - Max stats scenario
        var stats = new Dictionary<CharacterAttribute, int>
        {
            { CharacterAttribute.Sturdiness, 10 },
            { CharacterAttribute.Might, 10 },
            { CharacterAttribute.Wits, 10 },
            { CharacterAttribute.Will, 10 },
            { CharacterAttribute.Finesse, 10 }
        };

        // Act
        var derived = _sut.GetDerivedStats(stats, ArchetypeType.Warrior);

        // Assert
        derived.MaxHP.Should().Be(150);     // 50 + 10*10
        derived.MaxStamina.Should().Be(100); // 20 + 10*5 + 10*3
        derived.ActionPoints.Should().Be(4); // 2 + 10/4 = 4 (integer division)
    }

    [Fact]
    public void GetDerivedStats_LowStats_CalculatesCorrectly()
    {
        // Arrange - Min stats scenario
        var stats = new Dictionary<CharacterAttribute, int>
        {
            { CharacterAttribute.Sturdiness, 1 },
            { CharacterAttribute.Might, 1 },
            { CharacterAttribute.Wits, 1 },
            { CharacterAttribute.Will, 1 },
            { CharacterAttribute.Finesse, 1 }
        };

        // Act
        var derived = _sut.GetDerivedStats(stats, ArchetypeType.Warrior);

        // Assert
        derived.MaxHP.Should().Be(60);      // 50 + 1*10
        derived.MaxStamina.Should().Be(28);  // 20 + 1*5 + 1*3
        derived.ActionPoints.Should().Be(2); // 2 + 1/4 = 2
    }

    #endregion

    #region Display Name and Description Tests

    [Theory]
    [InlineData(LineageType.Human, "Human")]
    [InlineData(LineageType.RuneMarked, "Rune-Marked")]
    [InlineData(LineageType.IronBlooded, "Iron-Blooded")]
    [InlineData(LineageType.VargrKin, "Vargr-Kin")]
    public void GetLineageDisplayName_ReturnsCorrectName(LineageType lineage, string expectedName)
    {
        // Act
        var name = _sut.GetLineageDisplayName(lineage);

        // Assert
        name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(ArchetypeType.Warrior, "Warrior")]
    [InlineData(ArchetypeType.Skirmisher, "Skirmisher")]
    [InlineData(ArchetypeType.Adept, "Adept")]
    [InlineData(ArchetypeType.Mystic, "Mystic")]
    public void GetArchetypeDisplayName_ReturnsCorrectName(ArchetypeType archetype, string expectedName)
    {
        // Act
        var name = _sut.GetArchetypeDisplayName(archetype);

        // Assert
        name.Should().Be(expectedName);
    }

    [Fact]
    public void GetLineageDescription_ReturnsNonEmpty()
    {
        // Arrange & Act
        var descriptions = Enum.GetValues<LineageType>()
            .Select(l => _sut.GetLineageDescription(l))
            .ToList();

        // Assert
        descriptions.Should().OnlyContain(d => !string.IsNullOrEmpty(d));
    }

    [Fact]
    public void GetArchetypeDescription_ReturnsNonEmpty()
    {
        // Arrange & Act
        var descriptions = Enum.GetValues<ArchetypeType>()
            .Select(a => _sut.GetArchetypeDescription(a))
            .ToList();

        // Assert
        descriptions.Should().OnlyContain(d => !string.IsNullOrEmpty(d));
    }

    [Fact]
    public void GetLineageBonusSummary_ReturnsNonEmpty()
    {
        // Arrange & Act
        var summaries = Enum.GetValues<LineageType>()
            .Select(l => _sut.GetLineageBonusSummary(l))
            .ToList();

        // Assert
        summaries.Should().OnlyContain(s => !string.IsNullOrEmpty(s));
    }

    [Fact]
    public void GetArchetypeBonusSummary_ReturnsNonEmpty()
    {
        // Arrange & Act
        var summaries = Enum.GetValues<ArchetypeType>()
            .Select(a => _sut.GetArchetypeBonusSummary(a))
            .ToList();

        // Assert
        summaries.Should().OnlyContain(s => !string.IsNullOrEmpty(s));
    }

    #endregion

    #region Helper Methods

    private void SetupLineageBonuses()
    {
        _mockStatService.Setup(s => s.GetLineageBonuses(LineageType.Human))
            .Returns(new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 1 },
                { CharacterAttribute.Might, 1 },
                { CharacterAttribute.Wits, 1 },
                { CharacterAttribute.Will, 1 },
                { CharacterAttribute.Finesse, 1 }
            });

        _mockStatService.Setup(s => s.GetLineageBonuses(LineageType.RuneMarked))
            .Returns(new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Wits, 2 },
                { CharacterAttribute.Will, 2 },
                { CharacterAttribute.Sturdiness, -1 }
            });

        _mockStatService.Setup(s => s.GetLineageBonuses(LineageType.IronBlooded))
            .Returns(new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 2 },
                { CharacterAttribute.Might, 2 },
                { CharacterAttribute.Wits, -1 }
            });

        _mockStatService.Setup(s => s.GetLineageBonuses(LineageType.VargrKin))
            .Returns(new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, 2 },
                { CharacterAttribute.Wits, 2 },
                { CharacterAttribute.Will, -1 }
            });
    }

    private void SetupArchetypeBonuses()
    {
        _mockStatService.Setup(s => s.GetArchetypeBonuses(ArchetypeType.Warrior))
            .Returns(new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 2 },
                { CharacterAttribute.Might, 1 }
            });

        _mockStatService.Setup(s => s.GetArchetypeBonuses(ArchetypeType.Skirmisher))
            .Returns(new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, 2 },
                { CharacterAttribute.Wits, 1 }
            });

        _mockStatService.Setup(s => s.GetArchetypeBonuses(ArchetypeType.Adept))
            .Returns(new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Wits, 2 },
                { CharacterAttribute.Will, 1 }
            });

        _mockStatService.Setup(s => s.GetArchetypeBonuses(ArchetypeType.Mystic))
            .Returns(new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Will, 2 },
                { CharacterAttribute.Sturdiness, 1 }
            });
    }

    private void SetupDerivedStatFormulas()
    {
        _mockStatService.Setup(s => s.CalculateMaxHP(It.IsAny<int>()))
            .Returns((int stu) => 50 + stu * 10);

        _mockStatService.Setup(s => s.CalculateMaxStamina(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int fin, int stu) => 20 + fin * 5 + stu * 3);

        _mockStatService.Setup(s => s.CalculateActionPoints(It.IsAny<int>()))
            .Returns((int wit) => 2 + wit / 4);
    }

    private void SetupClampBehavior()
    {
        _mockStatService.Setup(s => s.ClampAttribute(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int val, int min, int max) => Math.Clamp(val, min, max));
    }

    #endregion
}
