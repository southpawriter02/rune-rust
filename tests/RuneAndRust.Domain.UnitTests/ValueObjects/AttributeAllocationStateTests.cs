// ═══════════════════════════════════════════════════════════════════════════════
// AttributeAllocationStateTests.cs
// Unit tests for the AttributeAllocationState value object verifying factory
// methods, state transitions, computed properties, and mode restrictions.
// Version: 0.17.2b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="AttributeAllocationState"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that AttributeAllocationState correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates Advanced mode defaults with all attributes at 1</description></item>
///   <item><description>Creates Simple mode states from recommended archetype builds</description></item>
///   <item><description>Prevents attribute modification in Simple mode</description></item>
///   <item><description>Allows attribute modification in Advanced mode with correct point tracking</description></item>
///   <item><description>Switches from Simple to Advanced mode while preserving attribute values</description></item>
///   <item><description>Computes IsComplete, AllowsManualAdjustment, and HasSelectedArchetype correctly</description></item>
///   <item><description>Retrieves individual attribute values via GetAttributeValue</description></item>
///   <item><description>Produces correct ToString output for debugging</description></item>
/// </list>
/// </remarks>
/// <seealso cref="AttributeAllocationState"/>
/// <seealso cref="AttributeAllocationMode"/>
/// <seealso cref="CoreAttribute"/>
[TestFixture]
public class AttributeAllocationStateTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — CreateAdvancedDefault
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.CreateAdvancedDefault"/>
    /// creates a state with all attributes at 1, zero points spent, and the full
    /// point pool available for allocation.
    /// </summary>
    [Test]
    public void CreateAdvancedDefault_SetsAllAttributesToOne()
    {
        // Arrange & Act
        var state = AttributeAllocationState.CreateAdvancedDefault(15);

        // Assert
        state.Mode.Should().Be(AttributeAllocationMode.Advanced);
        state.CurrentMight.Should().Be(1);
        state.CurrentFinesse.Should().Be(1);
        state.CurrentWits.Should().Be(1);
        state.CurrentWill.Should().Be(1);
        state.CurrentSturdiness.Should().Be(1);
        state.PointsSpent.Should().Be(0);
        state.PointsRemaining.Should().Be(15);
        state.TotalPoints.Should().Be(15);
        state.SelectedArchetypeId.Should().BeNull();
        state.IsComplete.Should().BeFalse();
        state.AllowsManualAdjustment.Should().BeTrue();
        state.HasSelectedArchetype.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.CreateAdvancedDefault"/>
    /// correctly handles the Adept archetype's reduced 14-point pool.
    /// </summary>
    [Test]
    public void CreateAdvancedDefault_WithAdeptPoints_SetsCorrectPool()
    {
        // Arrange & Act
        var state = AttributeAllocationState.CreateAdvancedDefault(14);

        // Assert
        state.TotalPoints.Should().Be(14);
        state.PointsRemaining.Should().Be(14);
        state.PointsSpent.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — CreateFromRecommendedBuild
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.CreateFromRecommendedBuild"/>
    /// correctly applies the Warrior archetype's recommended attribute values and
    /// sets the state to Simple mode with all points spent.
    /// </summary>
    [Test]
    public void CreateFromRecommendedBuild_WarriorBuild_AppliesArchetypeValues()
    {
        // Arrange & Act
        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);

        // Assert
        state.Mode.Should().Be(AttributeAllocationMode.Simple);
        state.CurrentMight.Should().Be(4);
        state.CurrentFinesse.Should().Be(3);
        state.CurrentWits.Should().Be(2);
        state.CurrentWill.Should().Be(2);
        state.CurrentSturdiness.Should().Be(4);
        state.PointsSpent.Should().Be(15);
        state.PointsRemaining.Should().Be(0);
        state.TotalPoints.Should().Be(15);
        state.SelectedArchetypeId.Should().Be("warrior");
        state.IsComplete.Should().BeTrue();
        state.AllowsManualAdjustment.Should().BeFalse();
        state.HasSelectedArchetype.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.CreateFromRecommendedBuild"/>
    /// normalizes the archetype ID to lowercase for consistent matching.
    /// </summary>
    [Test]
    public void CreateFromRecommendedBuild_NormalizesArchetypeIdToLowercase()
    {
        // Arrange & Act
        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            "MYSTIC", might: 2, finesse: 3, wits: 4, will: 4, sturdiness: 2, totalPoints: 15);

        // Assert
        state.SelectedArchetypeId.Should().Be("mystic");
    }

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.CreateFromRecommendedBuild"/>
    /// throws <see cref="ArgumentException"/> when the archetype ID is null.
    /// </summary>
    [Test]
    public void CreateFromRecommendedBuild_WithNullArchetypeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => AttributeAllocationState.CreateFromRecommendedBuild(
            null!, might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.CreateFromRecommendedBuild"/>
    /// throws <see cref="ArgumentException"/> when the archetype ID is whitespace.
    /// </summary>
    [Test]
    public void CreateFromRecommendedBuild_WithWhitespaceArchetypeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => AttributeAllocationState.CreateFromRecommendedBuild(
            "   ", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE TRANSITION TESTS — WithAttributeValue
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.WithAttributeValue"/>
    /// throws <see cref="InvalidOperationException"/> when attempting to modify
    /// attributes in Simple mode, enforcing the mode restriction.
    /// </summary>
    [Test]
    public void WithAttributeValue_InSimpleMode_ThrowsInvalidOperationException()
    {
        // Arrange
        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);

        // Act
        var act = () => state.WithAttributeValue(CoreAttribute.Might, 5, 1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Simple mode*");
    }

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.WithAttributeValue"/>
    /// correctly updates the attribute value and recalculates points in Advanced mode.
    /// </summary>
    [Test]
    public void WithAttributeValue_InAdvancedMode_UpdatesAttributeAndPoints()
    {
        // Arrange
        var state = AttributeAllocationState.CreateAdvancedDefault(15);

        // Act — Increase Might from 1 to 4 (costs 3 points)
        var updated = state.WithAttributeValue(CoreAttribute.Might, 4, 3);

        // Assert
        updated.CurrentMight.Should().Be(4);
        updated.PointsSpent.Should().Be(3);
        updated.PointsRemaining.Should().Be(12);
        // Other attributes remain unchanged
        updated.CurrentFinesse.Should().Be(1);
        updated.CurrentWits.Should().Be(1);
        updated.CurrentWill.Should().Be(1);
        updated.CurrentSturdiness.Should().Be(1);
    }

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.WithAttributeValue"/>
    /// correctly handles each of the five core attributes via separate updates.
    /// </summary>
    /// <param name="attribute">The core attribute to modify.</param>
    [Test]
    [TestCase(CoreAttribute.Might)]
    [TestCase(CoreAttribute.Finesse)]
    [TestCase(CoreAttribute.Wits)]
    [TestCase(CoreAttribute.Will)]
    [TestCase(CoreAttribute.Sturdiness)]
    public void WithAttributeValue_AllAttributes_UpdatesCorrectAttribute(CoreAttribute attribute)
    {
        // Arrange
        var state = AttributeAllocationState.CreateAdvancedDefault(15);

        // Act
        var updated = state.WithAttributeValue(attribute, 5, 4);

        // Assert
        updated.GetAttributeValue(attribute).Should().Be(5);
        updated.PointsSpent.Should().Be(4);
        updated.PointsRemaining.Should().Be(11);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE TRANSITION TESTS — SwitchToAdvanced
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.SwitchToAdvanced"/>
    /// preserves current attribute values, changes mode to Advanced, clears
    /// the archetype ID, and recalculates points.
    /// </summary>
    [Test]
    public void SwitchToAdvanced_PreservesValuesAndCalculatesPoints()
    {
        // Arrange — Start with Warrior recommended build
        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);

        // Act
        var advanced = state.SwitchToAdvanced(15);

        // Assert
        advanced.Mode.Should().Be(AttributeAllocationMode.Advanced);
        advanced.CurrentMight.Should().Be(4);
        advanced.CurrentFinesse.Should().Be(3);
        advanced.CurrentWits.Should().Be(2);
        advanced.CurrentWill.Should().Be(2);
        advanced.CurrentSturdiness.Should().Be(4);
        advanced.PointsSpent.Should().Be(15);
        advanced.PointsRemaining.Should().Be(0);
        advanced.TotalPoints.Should().Be(15);
        advanced.SelectedArchetypeId.Should().BeNull();
        advanced.AllowsManualAdjustment.Should().BeTrue();
        advanced.HasSelectedArchetype.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ATTRIBUTE ACCESS TESTS — GetAttributeValue
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.GetAttributeValue"/>
    /// returns the correct value for each core attribute from a recommended build.
    /// </summary>
    /// <param name="attribute">The core attribute to retrieve.</param>
    /// <param name="expected">The expected value from the Warrior build.</param>
    [Test]
    [TestCase(CoreAttribute.Might, 4)]
    [TestCase(CoreAttribute.Finesse, 3)]
    [TestCase(CoreAttribute.Wits, 2)]
    [TestCase(CoreAttribute.Will, 2)]
    [TestCase(CoreAttribute.Sturdiness, 4)]
    public void GetAttributeValue_ReturnsCorrectValue(CoreAttribute attribute, int expected)
    {
        // Arrange — Warrior build: M4, F3, Wi2, Wl2, S4
        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);

        // Act
        var value = state.GetAttributeValue(attribute);

        // Assert
        value.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.GetAttributeValue"/>
    /// throws <see cref="ArgumentOutOfRangeException"/> for an invalid attribute value.
    /// </summary>
    [Test]
    public void GetAttributeValue_WithInvalidAttribute_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var state = AttributeAllocationState.CreateAdvancedDefault(15);

        // Act
        var act = () => state.GetAttributeValue((CoreAttribute)99);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.ToString"/> returns
    /// the expected formatted string with mode, attribute values, and points.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            "warrior", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be("[Simple] M:4 F:3 Wi:2 Wl:2 S:4 (0/15 remaining)");
    }

    /// <summary>
    /// Verifies that <see cref="AttributeAllocationState.ToString"/> correctly
    /// displays Advanced mode state with remaining points.
    /// </summary>
    [Test]
    public void ToString_AdvancedDefault_ShowsCorrectFormat()
    {
        // Arrange
        var state = AttributeAllocationState.CreateAdvancedDefault(15);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be("[Advanced] M:1 F:1 Wi:1 Wl:1 S:1 (15/15 remaining)");
    }
}
