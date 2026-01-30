// ═══════════════════════════════════════════════════════════════════════════════
// SpecialResourceDefinitionTests.cs
// Unit tests for the SpecialResourceDefinition value object verifying factory
// method creation, parameter validation, computed properties, static properties,
// and display formatting.
// Version: 0.17.4b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="SpecialResourceDefinition"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that SpecialResourceDefinition correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates instances with valid parameters via the factory method</description></item>
///   <item><description>Normalizes resource IDs to lowercase</description></item>
///   <item><description>Validates all parameters including string, range, and numeric constraints</description></item>
///   <item><description>Provides correct None static property with empty/zero values</description></item>
///   <item><description>Computes HasResource correctly for valid and empty resources</description></item>
///   <item><description>Formats ToString output for display and debugging</description></item>
/// </list>
/// </remarks>
/// <seealso cref="SpecialResourceDefinition"/>
[TestFixture]
public class SpecialResourceDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create() with valid parameters returns a fully populated
    /// SpecialResourceDefinition with all properties correctly assigned.
    /// Uses Block Charges (Skjaldmaer) as the test case — a resource that
    /// starts at max and regenerates passively.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesResource()
    {
        // Arrange & Act
        var resource = SpecialResourceDefinition.Create(
            resourceId: "block-charges",
            displayName: "Block Charges",
            minValue: 0,
            maxValue: 3,
            startsAt: 3,
            regenPerTurn: 1,
            decayPerTurn: 0,
            description: "Stored defensive reactions ready to deflect incoming attacks");

        // Assert
        resource.ResourceId.Should().Be("block-charges");
        resource.DisplayName.Should().Be("Block Charges");
        resource.MinValue.Should().Be(0);
        resource.MaxValue.Should().Be(3);
        resource.StartsAt.Should().Be(3);
        resource.RegenPerTurn.Should().Be(1);
        resource.DecayPerTurn.Should().Be(0);
        resource.Description.Should().Be("Stored defensive reactions ready to deflect incoming attacks");
        resource.HasResource.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Create() normalizes the resource ID to lowercase,
    /// ensuring consistent key comparison regardless of input casing.
    /// </summary>
    [Test]
    public void Create_NormalizesResourceIdToLowerCase()
    {
        // Arrange & Act
        var resource = SpecialResourceDefinition.Create(
            resourceId: "RAGE",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Fury that builds with each strike");

        // Assert
        resource.ResourceId.Should().Be("rage");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="SpecialResourceDefinition.None"/> returns
    /// a resource definition where HasResource is false and ResourceId is empty.
    /// Used for the 12 specializations without unique resources.
    /// </summary>
    [Test]
    public void None_HasResourceReturnsFalse()
    {
        // Arrange & Act
        var none = SpecialResourceDefinition.None;

        // Assert
        none.HasResource.Should().BeFalse();
        none.ResourceId.Should().BeEmpty();
        none.DisplayName.Should().BeEmpty();
        none.Description.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="SpecialResourceDefinition.None"/> has all
    /// numeric properties set to zero.
    /// </summary>
    [Test]
    public void None_HasZeroValues()
    {
        // Arrange & Act
        var none = SpecialResourceDefinition.None;

        // Assert
        none.MinValue.Should().Be(0);
        none.MaxValue.Should().Be(0);
        none.StartsAt.Should().Be(0);
        none.RegenPerTurn.Should().Be(0);
        none.DecayPerTurn.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create() throws ArgumentException when resourceId is null.
    /// </summary>
    [Test]
    public void Create_WithNullResourceId_ThrowsArgumentException()
    {
        // Arrange
        var act = () => SpecialResourceDefinition.Create(
            resourceId: null!,
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentException when displayName is null.
    /// </summary>
    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange
        var act = () => SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: null!,
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentOutOfRangeException when
    /// minValue exceeds maxValue, which is an invalid range definition.
    /// </summary>
    [Test]
    public void Create_WithInvalidRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var act = () => SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 100,
            maxValue: 0,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Test");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentOutOfRangeException when
    /// startsAt is below the minimum value.
    /// </summary>
    [Test]
    public void Create_WithStartsBelowMin_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var act = () => SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: -1,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Test");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentOutOfRangeException when
    /// startsAt exceeds the maximum value.
    /// </summary>
    [Test]
    public void Create_WithStartsAboveMax_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var act = () => SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 101,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Test");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentOutOfRangeException when
    /// regenPerTurn is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeRegen_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var act = () => SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: -1,
            decayPerTurn: 5,
            description: "Test");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentOutOfRangeException when
    /// decayPerTurn is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeDecay_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var act = () => SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: -1,
            description: "Test");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasResource returns true for a valid resource created
    /// via the factory method with a non-empty resource ID.
    /// </summary>
    [Test]
    public void HasResource_WithValidResource_ReturnsTrue()
    {
        // Arrange
        var resource = SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Fury that builds with each strike");

        // Assert
        resource.HasResource.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString() returns a formatted string with the display
    /// name and range for valid resources.
    /// </summary>
    [Test]
    public void ToString_WithResource_ReturnsFormattedString()
    {
        // Arrange
        var resource = SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Fury that builds with each strike");

        // Act
        var result = resource.ToString();

        // Assert
        result.Should().Be("Rage (0-100)");
    }

    /// <summary>
    /// Verifies that ToString() returns "None" for the empty resource
    /// definition used by specializations without unique resources.
    /// </summary>
    [Test]
    public void ToString_WithNone_ReturnsNoneString()
    {
        // Arrange
        var none = SpecialResourceDefinition.None;

        // Act
        var result = none.ToString();

        // Assert
        result.Should().Be("None");
    }
}
