using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="LineagePassiveBonuses"/> value object.
/// </summary>
/// <remarks>
/// Verifies correct behavior of static factory properties, validation,
/// skill bonus lookups, and ToString formatting.
/// </remarks>
[TestFixture]
public class LineagePassiveBonusesTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC FACTORY PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ClanBorn has +5 HP and +1 Social.
    /// </summary>
    [Test]
    public void ClanBorn_HasCorrectBonuses()
    {
        // Arrange & Act
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Assert
        clanBorn.MaxHpBonus.Should().Be(5);
        clanBorn.MaxApBonus.Should().Be(0);
        clanBorn.SoakBonus.Should().Be(0);
        clanBorn.MovementBonus.Should().Be(0);
        clanBorn.HasHpBonus.Should().BeTrue();
        clanBorn.HasApBonus.Should().BeFalse();
        clanBorn.HasSkillBonuses.Should().BeTrue();
        clanBorn.SkillBonuses.Should().HaveCount(1);
        clanBorn.GetSkillBonus("social").Should().Be(1);
    }

    /// <summary>
    /// Verifies that RuneMarked has +5 AP and +1 Lore.
    /// </summary>
    [Test]
    public void RuneMarked_HasCorrectBonuses()
    {
        // Arrange & Act
        var runeMarked = LineagePassiveBonuses.RuneMarked;

        // Assert
        runeMarked.MaxHpBonus.Should().Be(0);
        runeMarked.MaxApBonus.Should().Be(5);
        runeMarked.SoakBonus.Should().Be(0);
        runeMarked.MovementBonus.Should().Be(0);
        runeMarked.HasApBonus.Should().BeTrue();
        runeMarked.GetSkillBonus("lore").Should().Be(1);
    }

    /// <summary>
    /// Verifies that IronBlooded has +2 Soak and +1 Craft.
    /// </summary>
    [Test]
    public void IronBlooded_HasCorrectBonuses()
    {
        // Arrange & Act
        var ironBlooded = LineagePassiveBonuses.IronBlooded;

        // Assert
        ironBlooded.MaxHpBonus.Should().Be(0);
        ironBlooded.MaxApBonus.Should().Be(0);
        ironBlooded.SoakBonus.Should().Be(2);
        ironBlooded.MovementBonus.Should().Be(0);
        ironBlooded.HasSoakBonus.Should().BeTrue();
        ironBlooded.GetSkillBonus("craft").Should().Be(1);
    }

    /// <summary>
    /// Verifies that VargrKin has +1 Movement and +1 Survival.
    /// </summary>
    [Test]
    public void VargrKin_HasCorrectBonuses()
    {
        // Arrange & Act
        var vargrKin = LineagePassiveBonuses.VargrKin;

        // Assert
        vargrKin.MaxHpBonus.Should().Be(0);
        vargrKin.MaxApBonus.Should().Be(0);
        vargrKin.SoakBonus.Should().Be(0);
        vargrKin.MovementBonus.Should().Be(1);
        vargrKin.HasMovementBonus.Should().BeTrue();
        vargrKin.GetSkillBonus("survival").Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create throws for negative HP bonus.
    /// </summary>
    [Test]
    public void Create_WithNegativeHpBonus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => LineagePassiveBonuses.Create(-1, 0, 0, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxHpBonus");
    }

    /// <summary>
    /// Verifies that Create throws for negative AP bonus.
    /// </summary>
    [Test]
    public void Create_WithNegativeApBonus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => LineagePassiveBonuses.Create(0, -1, 0, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxApBonus");
    }

    /// <summary>
    /// Verifies that Create throws for negative Soak bonus.
    /// </summary>
    [Test]
    public void Create_WithNegativeSoakBonus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => LineagePassiveBonuses.Create(0, 0, -1, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("soakBonus");
    }

    /// <summary>
    /// Verifies that Create throws for negative Movement bonus.
    /// </summary>
    [Test]
    public void Create_WithNegativeMovementBonus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => LineagePassiveBonuses.Create(0, 0, 0, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("movementBonus");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GETSKILLBONUS TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSkillBonus returns correct amount for existing skill.
    /// </summary>
    [Test]
    public void GetSkillBonus_ForExistingSkill_ReturnsCorrectAmount()
    {
        // Arrange
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Act
        var bonus = clanBorn.GetSkillBonus("social");

        // Assert
        bonus.Should().Be(1);
    }

    /// <summary>
    /// Verifies that GetSkillBonus returns zero for non-existent skill.
    /// </summary>
    [Test]
    public void GetSkillBonus_ForNonExistentSkill_ReturnsZero()
    {
        // Arrange
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Act
        var bonus = clanBorn.GetSkillBonus("lore");

        // Assert
        bonus.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetSkillBonus is case-insensitive.
    /// </summary>
    [Test]
    public void GetSkillBonus_IsCaseInsensitive()
    {
        // Arrange
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Act & Assert
        clanBorn.GetSkillBonus("SOCIAL").Should().Be(1);
        clanBorn.GetSkillBonus("Social").Should().Be(1);
        clanBorn.GetSkillBonus("sOcIaL").Should().Be(1);
    }

    /// <summary>
    /// Verifies that GetSkillBonus throws for null skill ID.
    /// </summary>
    [Test]
    public void GetSkillBonus_WithNullSkillId_ThrowsArgumentException()
    {
        // Arrange
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Act
        var act = () => clanBorn.GetSkillBonus(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HASBONUSFORSKILL TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies HasBonusForSkill returns true for skills with bonuses.
    /// </summary>
    [Test]
    public void HasBonusForSkill_ForExistingSkill_ReturnsTrue()
    {
        // Arrange
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Act & Assert
        clanBorn.HasBonusForSkill("social").Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasBonusForSkill returns false for skills without bonuses.
    /// </summary>
    [Test]
    public void HasBonusForSkill_ForNonExistentSkill_ReturnsFalse()
    {
        // Arrange
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Act & Assert
        clanBorn.HasBonusForSkill("lore").Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasBonusForSkill returns false for null.
    /// </summary>
    [Test]
    public void HasBonusForSkill_ForNull_ReturnsFalse()
    {
        // Arrange
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Act & Assert
        clanBorn.HasBonusForSkill(null).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NONE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that None has no bonuses.
    /// </summary>
    [Test]
    public void None_HasNoBonuses()
    {
        // Arrange & Act
        var none = LineagePassiveBonuses.None;

        // Assert
        none.MaxHpBonus.Should().Be(0);
        none.MaxApBonus.Should().Be(0);
        none.SoakBonus.Should().Be(0);
        none.MovementBonus.Should().Be(0);
        none.SkillBonuses.Should().BeEmpty();
        none.HasAnyBonuses.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies ToString lists all bonuses.
    /// </summary>
    [Test]
    public void ToString_ForClanBorn_ListsAllBonuses()
    {
        // Arrange
        var clanBorn = LineagePassiveBonuses.ClanBorn;

        // Act
        var result = clanBorn.ToString();

        // Assert
        result.Should().Contain("+5 Max HP");
        result.Should().Contain("social +1");
    }

    /// <summary>
    /// Verifies ToString for None shows no bonuses message.
    /// </summary>
    [Test]
    public void ToString_ForNone_ShowsNoBonusesMessage()
    {
        // Arrange
        var none = LineagePassiveBonuses.None;

        // Act
        var result = none.ToString();

        // Assert
        result.Should().Be("No passive bonuses");
    }
}
