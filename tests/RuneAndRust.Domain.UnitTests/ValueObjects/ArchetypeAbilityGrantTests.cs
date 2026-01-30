// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeAbilityGrantTests.cs
// Unit tests for the ArchetypeAbilityGrant value object verifying factory
// methods, computed properties, validation, ID normalization, and formatting.
// Version: 0.17.3c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ArchetypeAbilityGrant"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that ArchetypeAbilityGrant correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates ability grants via the validated Create factory method</description></item>
///   <item><description>Creates ability grants via convenience factories (CreateActive, CreatePassive, CreateStance)</description></item>
///   <item><description>Normalizes AbilityId to lowercase for consistent lookups</description></item>
///   <item><description>Rejects null or whitespace AbilityId, AbilityName, and Description</description></item>
///   <item><description>Computes IsPassive, IsActive, IsStance, and RequiresActivation correctly</description></item>
///   <item><description>Returns correct TypeDisplay tags ([ACTIVE], [PASSIVE], [STANCE])</description></item>
///   <item><description>Produces correctly formatted display and debug strings</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ArchetypeAbilityGrant"/>
/// <seealso cref="AbilityType"/>
[TestFixture]
public class ArchetypeAbilityGrantTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Create
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.Create"/> creates a valid
    /// ability grant with correct properties when given valid data.
    /// </summary>
    [Test]
    public void Create_WithValidData_ReturnsGrant()
    {
        // Arrange & Act
        var grant = ArchetypeAbilityGrant.Create(
            "power-strike",
            "Power Strike",
            AbilityType.Active,
            "A powerful melee attack that deals bonus damage.");

        // Assert
        grant.AbilityId.Should().Be("power-strike");
        grant.AbilityName.Should().Be("Power Strike");
        grant.AbilityType.Should().Be(AbilityType.Active);
        grant.Description.Should().Be("A powerful melee attack that deals bonus damage.");
        grant.IsActive.Should().BeTrue();
        grant.IsPassive.Should().BeFalse();
        grant.IsStance.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.Create"/> normalizes
    /// the AbilityId to lowercase for consistent lookups against the Ability System.
    /// </summary>
    [Test]
    public void Create_NormalizesAbilityIdToLowercase()
    {
        // Arrange & Act
        var grant = ArchetypeAbilityGrant.Create(
            "Power-Strike",
            "Power Strike",
            AbilityType.Active,
            "A powerful melee attack.");

        // Assert
        grant.AbilityId.Should().Be("power-strike");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.Create"/> throws
    /// <see cref="ArgumentException"/> when the ability ID is null.
    /// </summary>
    [Test]
    public void Create_WithNullAbilityId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeAbilityGrant.Create(
            null!,
            "Power Strike",
            AbilityType.Active,
            "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.Create"/> throws
    /// <see cref="ArgumentException"/> when the ability ID is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceAbilityId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeAbilityGrant.Create(
            "   ",
            "Power Strike",
            AbilityType.Active,
            "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.Create"/> throws
    /// <see cref="ArgumentException"/> when the ability name is null.
    /// </summary>
    [Test]
    public void Create_WithNullAbilityName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeAbilityGrant.Create(
            "power-strike",
            null!,
            AbilityType.Active,
            "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.Create"/> throws
    /// <see cref="ArgumentException"/> when the ability name is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceAbilityName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeAbilityGrant.Create(
            "power-strike",
            "   ",
            AbilityType.Active,
            "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.Create"/> throws
    /// <see cref="ArgumentException"/> when the description is null.
    /// </summary>
    [Test]
    public void Create_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeAbilityGrant.Create(
            "power-strike",
            "Power Strike",
            AbilityType.Active,
            null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.Create"/> throws
    /// <see cref="ArgumentException"/> when the description is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeAbilityGrant.Create(
            "power-strike",
            "Power Strike",
            AbilityType.Active,
            "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONVENIENCE FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.CreateActive"/> creates
    /// a grant with the correct Active ability type and computed properties.
    /// </summary>
    [Test]
    public void CreateActive_SetsCorrectType()
    {
        // Arrange & Act
        var grant = ArchetypeAbilityGrant.CreateActive(
            "power-strike",
            "Power Strike",
            "A powerful melee attack that deals bonus damage.");

        // Assert
        grant.AbilityType.Should().Be(AbilityType.Active);
        grant.IsActive.Should().BeTrue();
        grant.IsPassive.Should().BeFalse();
        grant.IsStance.Should().BeFalse();
        grant.RequiresActivation.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.CreatePassive"/> creates
    /// a grant with the correct Passive ability type and computed properties.
    /// </summary>
    [Test]
    public void CreatePassive_SetsCorrectType()
    {
        // Arrange & Act
        var grant = ArchetypeAbilityGrant.CreatePassive(
            "iron-will",
            "Iron Will",
            "Resistance to fear and mental effects.");

        // Assert
        grant.AbilityType.Should().Be(AbilityType.Passive);
        grant.IsPassive.Should().BeTrue();
        grant.IsActive.Should().BeFalse();
        grant.IsStance.Should().BeFalse();
        grant.RequiresActivation.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.CreateStance"/> creates
    /// a grant with the correct Stance ability type and computed properties.
    /// </summary>
    [Test]
    public void CreateStance_SetsCorrectType()
    {
        // Arrange & Act
        var grant = ArchetypeAbilityGrant.CreateStance(
            "defensive-stance",
            "Defensive Stance",
            "Reduces damage taken but slows movement.");

        // Assert
        grant.AbilityType.Should().Be(AbilityType.Stance);
        grant.IsStance.Should().BeTrue();
        grant.IsActive.Should().BeFalse();
        grant.IsPassive.Should().BeFalse();
        grant.RequiresActivation.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.IsPassive"/> returns
    /// <c>false</c> for an Active ability.
    /// </summary>
    [Test]
    public void IsPassive_ReturnsFalse_ForActiveAbility()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateActive(
            "power-strike", "Power Strike", "Description");

        // Assert
        grant.IsPassive.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.IsStance"/> returns
    /// <c>false</c> for a Passive ability.
    /// </summary>
    [Test]
    public void IsStance_ReturnsFalse_ForPassiveAbility()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreatePassive(
            "iron-will", "Iron Will", "Description");

        // Assert
        grant.IsStance.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.RequiresActivation"/>
    /// returns <c>true</c> for an Active ability.
    /// </summary>
    [Test]
    public void RequiresActivation_ReturnsTrue_ForActiveAbility()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateActive(
            "power-strike", "Power Strike", "Description");

        // Assert
        grant.RequiresActivation.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.RequiresActivation"/>
    /// returns <c>true</c> for a Stance ability.
    /// </summary>
    [Test]
    public void RequiresActivation_ReturnsTrue_ForStanceAbility()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateStance(
            "defensive-stance", "Defensive Stance", "Description");

        // Assert
        grant.RequiresActivation.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.RequiresActivation"/>
    /// returns <c>false</c> for a Passive ability.
    /// </summary>
    [Test]
    public void RequiresActivation_ReturnsFalse_ForPassiveAbility()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreatePassive(
            "iron-will", "Iron Will", "Description");

        // Assert
        grant.RequiresActivation.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TYPE DISPLAY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.TypeDisplay"/> returns
    /// "[ACTIVE]" for Active abilities.
    /// </summary>
    [Test]
    public void TypeDisplay_Active_ReturnsActiveTag()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateActive(
            "power-strike", "Power Strike", "Description");

        // Assert
        grant.TypeDisplay.Should().Be("[ACTIVE]");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.TypeDisplay"/> returns
    /// "[PASSIVE]" for Passive abilities.
    /// </summary>
    [Test]
    public void TypeDisplay_Passive_ReturnsPassiveTag()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreatePassive(
            "iron-will", "Iron Will", "Description");

        // Assert
        grant.TypeDisplay.Should().Be("[PASSIVE]");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.TypeDisplay"/> returns
    /// "[STANCE]" for Stance abilities.
    /// </summary>
    [Test]
    public void TypeDisplay_Stance_ReturnsStanceTag()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateStance(
            "defensive-stance", "Defensive Stance", "Description");

        // Assert
        grant.TypeDisplay.Should().Be("[STANCE]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY AND FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.GetDisplayString"/>
    /// returns a formatted multi-line string containing the ability name,
    /// type tag, and description.
    /// </summary>
    [Test]
    public void GetDisplayString_ReturnsFormattedOutput()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateActive(
            "power-strike",
            "Power Strike",
            "A powerful melee attack that deals bonus damage.");

        // Act
        var display = grant.GetDisplayString();

        // Assert
        display.Should().Contain("Power Strike");
        display.Should().Contain("[ACTIVE]");
        display.Should().Contain("A powerful melee attack that deals bonus damage.");
        display.Should().Be("Power Strike [ACTIVE]\nA powerful melee attack that deals bonus damage.");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.GetShortDisplay"/>
    /// returns a compact single-line string with the ability name and type tag.
    /// </summary>
    [Test]
    public void GetShortDisplay_ReturnsNameAndType()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateActive(
            "power-strike",
            "Power Strike",
            "A powerful melee attack.");

        // Act
        var shortDisplay = grant.GetShortDisplay();

        // Assert
        shortDisplay.Should().Be("Power Strike [ACTIVE]");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.GetShortDisplay"/>
    /// returns the correct format for a Passive ability.
    /// </summary>
    [Test]
    public void GetShortDisplay_Passive_ReturnsNameAndPassiveTag()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreatePassive(
            "iron-will",
            "Iron Will",
            "Resistance to fear.");

        // Act
        var shortDisplay = grant.GetShortDisplay();

        // Assert
        shortDisplay.Should().Be("Iron Will [PASSIVE]");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.ToString"/> returns
    /// the expected debug format string.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateActive(
            "power-strike",
            "Power Strike",
            "A powerful melee attack.");

        // Act
        var result = grant.ToString();

        // Assert
        result.Should().Be("power-strike: Power Strike (Active)");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeAbilityGrant.ToString"/> returns
    /// the correct format for a Stance ability.
    /// </summary>
    [Test]
    public void ToString_Stance_ReturnsFormattedString()
    {
        // Arrange
        var grant = ArchetypeAbilityGrant.CreateStance(
            "defensive-stance",
            "Defensive Stance",
            "Reduces damage taken.");

        // Act
        var result = grant.ToString();

        // Assert
        result.Should().Be("defensive-stance: Defensive Stance (Stance)");
    }
}
