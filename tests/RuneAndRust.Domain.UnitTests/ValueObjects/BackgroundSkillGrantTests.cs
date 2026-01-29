using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="BackgroundSkillGrant"/> value object.
/// </summary>
/// <remarks>
/// Verifies factory method behavior, parameter validation, computed properties,
/// normalization, convenience factory methods, and string formatting for the
/// BackgroundSkillGrant value object.
/// </remarks>
[TestFixture]
public class BackgroundSkillGrantTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS - SUCCESSFUL CREATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with valid parameters produces a correctly populated grant.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesGrant()
    {
        // Act
        var grant = BackgroundSkillGrant.Create("craft", 2, SkillGrantType.Permanent);

        // Assert
        grant.SkillId.Should().Be("craft");
        grant.BonusAmount.Should().Be(2);
        grant.GrantType.Should().Be(SkillGrantType.Permanent);
        grant.IsPermanent.Should().BeTrue();
        grant.HasNumericBonus.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS - VALIDATION FAILURES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create throws when skillId is null.
    /// </summary>
    [Test]
    public void Create_WithNullSkillId_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundSkillGrant.Create(null!, 2);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("skillId");
    }

    /// <summary>
    /// Verifies that Create throws when skillId is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceSkillId_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundSkillGrant.Create("   ", 2);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("skillId");
    }

    /// <summary>
    /// Verifies that Create throws when bonusAmount is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeBonusAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        Action act = () => BackgroundSkillGrant.Create("craft", -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("bonusAmount");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NORMALIZATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create normalizes skillId to lowercase.
    /// </summary>
    /// <remarks>
    /// Skill IDs are normalized to lowercase for consistent matching
    /// with the configuration-driven skill system.
    /// </remarks>
    [Test]
    public void Create_NormalizesSkillIdToLowercase()
    {
        // Act
        var grant = BackgroundSkillGrant.Create("CRAFT", 2);

        // Assert
        grant.SkillId.Should().Be("craft",
            "skill IDs should be normalized to lowercase for consistent matching");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONVENIENCE FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Permanent creates a grant with SkillGrantType.Permanent.
    /// </summary>
    [Test]
    public void Permanent_CreatesPermanentGrant()
    {
        // Act
        var grant = BackgroundSkillGrant.Permanent("craft", 2);

        // Assert
        grant.SkillId.Should().Be("craft");
        grant.BonusAmount.Should().Be(2);
        grant.GrantType.Should().Be(SkillGrantType.Permanent);
        grant.IsPermanent.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Starting creates a grant with SkillGrantType.StartingBonus.
    /// </summary>
    [Test]
    public void Starting_CreatesStartingBonusGrant()
    {
        // Act
        var grant = BackgroundSkillGrant.Starting("might", 1);

        // Assert
        grant.GrantType.Should().Be(SkillGrantType.StartingBonus);
        grant.IsStartingBonus.Should().BeTrue();
        grant.IsPermanent.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Proficient creates a grant with zero bonus and Proficiency type.
    /// </summary>
    [Test]
    public void Proficient_CreatesProficiencyGrant()
    {
        // Act
        var grant = BackgroundSkillGrant.Proficient("traps");

        // Assert
        grant.GrantType.Should().Be(SkillGrantType.Proficiency);
        grant.IsProficiency.Should().BeTrue();
        grant.BonusAmount.Should().Be(0);
        grant.HasNumericBonus.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsPermanent returns true for Permanent grants.
    /// </summary>
    [Test]
    public void IsPermanent_WithPermanentType_ReturnsTrue()
    {
        // Arrange
        var grant = BackgroundSkillGrant.Permanent("craft", 2);

        // Assert
        grant.IsPermanent.Should().BeTrue();
        grant.IsStartingBonus.Should().BeFalse();
        grant.IsProficiency.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString formats permanent grants as "skillId +N".
    /// </summary>
    [Test]
    public void ToString_ForPermanentGrant_FormatsCorrectly()
    {
        // Arrange
        var grant = BackgroundSkillGrant.Permanent("craft", 2);

        // Act
        var result = grant.ToString();

        // Assert
        result.Should().Be("craft +2");
    }

    /// <summary>
    /// Verifies that ToString formats starting bonus grants with "(starting)" label.
    /// </summary>
    [Test]
    public void ToString_ForStartingBonusGrant_IncludesStartingLabel()
    {
        // Arrange
        var grant = BackgroundSkillGrant.Starting("might", 1);

        // Act
        var result = grant.ToString();

        // Assert
        result.Should().Be("might +1 (starting)");
    }

    /// <summary>
    /// Verifies that ToString formats proficiency grants with "(proficient)" label.
    /// </summary>
    [Test]
    public void ToString_ForProficiencyGrant_IncludesProficientLabel()
    {
        // Arrange
        var grant = BackgroundSkillGrant.Proficient("traps");

        // Act
        var result = grant.ToString();

        // Assert
        result.Should().Be("traps (proficient)");
    }
}
