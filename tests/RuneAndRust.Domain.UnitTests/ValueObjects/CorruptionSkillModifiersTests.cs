namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CorruptionSkillModifiers"/> value object.
/// Tests cover the Create factory method, the None static property, all stored
/// properties, the HasModifiers arrow property, stage-based modifier verification,
/// and ToString formatting.
/// </summary>
[TestFixture]
public class CorruptionSkillModifiersTests
{
    // -------------------------------------------------------------------------
    // Factory — Create
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Create stores all properties correctly.
    /// </summary>
    [Test]
    public void Create_StoresAllProperties()
    {
        // Arrange & Act
        var modifiers = CorruptionSkillModifiers.Create(
            techBonus: 2,
            socialPenalty: -2,
            factionLocked: true);

        // Assert
        modifiers.TechBonus.Should().Be(2);
        modifiers.SocialPenalty.Should().Be(-2);
        modifiers.FactionLocked.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Factory — None
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that None returns zero modifiers with no faction lock.
    /// </summary>
    [Test]
    public void None_ReturnsZeroModifiers()
    {
        // Act
        var modifiers = CorruptionSkillModifiers.None;

        // Assert
        modifiers.TechBonus.Should().Be(0);
        modifiers.SocialPenalty.Should().Be(0);
        modifiers.FactionLocked.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Arrow — HasModifiers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that HasModifiers returns false when all values are zero/false.
    /// </summary>
    [Test]
    public void HasModifiers_AllZero_ReturnsFalse()
    {
        // Arrange
        var modifiers = CorruptionSkillModifiers.None;

        // Assert
        modifiers.HasModifiers.Should().BeFalse(
            because: "an uncorrupted character has no modifiers");
    }

    /// <summary>
    /// Verifies that HasModifiers returns true when any modifier is non-zero.
    /// </summary>
    [Test]
    [TestCase(1, 0, false, Description = "TechBonus only")]
    [TestCase(0, -1, false, Description = "SocialPenalty only")]
    [TestCase(0, 0, true, Description = "FactionLocked only")]
    [TestCase(2, -2, true, Description = "All modifiers active")]
    [TestCase(1, -1, false, Description = "Tainted stage modifiers")]
    public void HasModifiers_AnyNonZero_ReturnsTrue(
        int techBonus, int socialPenalty, bool factionLocked)
    {
        // Arrange
        var modifiers = CorruptionSkillModifiers.Create(techBonus, socialPenalty, factionLocked);

        // Assert
        modifiers.HasModifiers.Should().BeTrue(
            because: "at least one modifier is active");
    }

    // -------------------------------------------------------------------------
    // Stage-Based Modifier Verification
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies modifiers for the Uncorrupted stage (0-19 corruption).
    /// </summary>
    [Test]
    public void Create_UncorruptedStage_HasNoModifiers()
    {
        // Arrange — Uncorrupted: +0 Tech, -0 Social, no lock
        var modifiers = CorruptionSkillModifiers.Create(0, 0, false);

        // Assert
        modifiers.TechBonus.Should().Be(0);
        modifiers.SocialPenalty.Should().Be(0);
        modifiers.FactionLocked.Should().BeFalse();
        modifiers.HasModifiers.Should().BeFalse();
    }

    /// <summary>
    /// Verifies modifiers for the Tainted stage (20-39 corruption).
    /// </summary>
    [Test]
    public void Create_TaintedStage_HasMinorModifiers()
    {
        // Arrange — Tainted: +1 Tech, -1 Social, no lock
        var modifiers = CorruptionSkillModifiers.Create(1, -1, false);

        // Assert
        modifiers.TechBonus.Should().Be(1);
        modifiers.SocialPenalty.Should().Be(-1);
        modifiers.FactionLocked.Should().BeFalse();
        modifiers.HasModifiers.Should().BeTrue();
    }

    /// <summary>
    /// Verifies modifiers for the Infected stage (40-59 corruption).
    /// </summary>
    [Test]
    public void Create_InfectedStage_HasFullModifiersAndFactionLock()
    {
        // Arrange — Infected: +2 Tech, -2 Social, faction locked
        var modifiers = CorruptionSkillModifiers.Create(2, -2, true);

        // Assert
        modifiers.TechBonus.Should().Be(2);
        modifiers.SocialPenalty.Should().Be(-2);
        modifiers.FactionLocked.Should().BeTrue();
        modifiers.HasModifiers.Should().BeTrue();
    }

    /// <summary>
    /// Verifies modifiers for the Blighted stage (60-79 corruption).
    /// Blighted uses same modifiers as Infected.
    /// </summary>
    [Test]
    public void Create_BlightedStage_MatchesInfectedModifiers()
    {
        // Arrange — Blighted: +2 Tech, -2 Social, faction locked
        var modifiers = CorruptionSkillModifiers.Create(2, -2, true);

        // Assert
        modifiers.TechBonus.Should().Be(2);
        modifiers.SocialPenalty.Should().Be(-2);
        modifiers.FactionLocked.Should().BeTrue();
    }

    /// <summary>
    /// Verifies modifiers for the Corrupted stage (80-99 corruption).
    /// Corrupted uses same modifiers as Infected.
    /// </summary>
    [Test]
    public void Create_CorruptedStage_MatchesInfectedModifiers()
    {
        // Arrange — Corrupted: +2 Tech, -2 Social, faction locked
        var modifiers = CorruptionSkillModifiers.Create(2, -2, true);

        // Assert
        modifiers.TechBonus.Should().Be(2);
        modifiers.SocialPenalty.Should().Be(-2);
        modifiers.FactionLocked.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // ToString — With Modifiers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies ToString for modifiers with faction lock.
    /// </summary>
    [Test]
    public void ToString_WithModifiersAndFactionLock_FormatsCorrectly()
    {
        // Arrange
        var modifiers = CorruptionSkillModifiers.Create(2, -2, true);

        // Act
        var display = modifiers.ToString();

        // Assert
        display.Should().Contain("Tech +2");
        display.Should().Contain("Social -2");
        display.Should().Contain("FACTION LOCKED");
    }

    /// <summary>
    /// Verifies ToString for modifiers without faction lock.
    /// </summary>
    [Test]
    public void ToString_WithModifiersNoFactionLock_FormatsCorrectly()
    {
        // Arrange
        var modifiers = CorruptionSkillModifiers.Create(1, -1, false);

        // Act
        var display = modifiers.ToString();

        // Assert
        display.Should().Contain("Tech +1");
        display.Should().Contain("Social -1");
        display.Should().NotContain("FACTION LOCKED");
    }

    // -------------------------------------------------------------------------
    // ToString — No Modifiers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies ToString for no modifiers (Uncorrupted stage).
    /// </summary>
    [Test]
    public void ToString_NoModifiers_ShowsNoModifiers()
    {
        // Arrange
        var modifiers = CorruptionSkillModifiers.None;

        // Act
        var display = modifiers.ToString();

        // Assert
        display.Should().Contain("No modifiers");
    }

    // -------------------------------------------------------------------------
    // Record Equality
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that two modifiers with identical properties are equal (record semantics).
    /// </summary>
    [Test]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var modifiers1 = CorruptionSkillModifiers.Create(2, -2, true);
        var modifiers2 = CorruptionSkillModifiers.Create(2, -2, true);

        // Assert
        modifiers1.Should().Be(modifiers2);
    }

    /// <summary>
    /// Verifies that two modifiers with different properties are not equal.
    /// </summary>
    [Test]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var modifiers1 = CorruptionSkillModifiers.Create(1, -1, false);
        var modifiers2 = CorruptionSkillModifiers.Create(2, -2, true);

        // Assert
        modifiers1.Should().NotBe(modifiers2);
    }

    /// <summary>
    /// Verifies that None equals a manually created zero-modifier instance.
    /// </summary>
    [Test]
    public void Equality_NoneEqualsCreateZero()
    {
        // Arrange
        var none = CorruptionSkillModifiers.None;
        var zero = CorruptionSkillModifiers.Create(0, 0, false);

        // Assert
        none.Should().Be(zero);
    }
}
