// ═══════════════════════════════════════════════════════════════════════════════
// DerivedStatsTests.cs
// Unit tests for the DerivedStats value object verifying factory method
// validation, computed properties, summary formatting, and string representation.
// Version: 0.17.2d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="DerivedStats"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that DerivedStats correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates validated stat blocks with all 7 derived stats</description></item>
///   <item><description>Rejects invalid values (zero HP, zero movement speed, negative stats)</description></item>
///   <item><description>Calculates computed properties (TotalResourcePool, HasCombatStats, HasResourceStats)</description></item>
///   <item><description>Produces correct GetSummary and ToString output for display and debugging</description></item>
/// </list>
/// </remarks>
/// <seealso cref="DerivedStats"/>
[TestFixture]
public class DerivedStatsTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Create
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="DerivedStats.Create"/> creates a valid stat block
    /// with all 7 properties set correctly when given valid parameters.
    /// Uses a Warrior standard build (M4, F3, Wi2, Wl2, S4) as reference.
    /// </summary>
    [Test]
    public void Create_WithValidValues_CreatesDerivedStats()
    {
        // Arrange & Act
        var stats = DerivedStats.Create(
            maxHp: 139,
            maxStamina: 60,
            maxAetherPool: 30,
            initiative: 4,
            soak: 2,
            movementSpeed: 5,
            carryingCapacity: 40);

        // Assert
        stats.MaxHp.Should().Be(139);
        stats.MaxStamina.Should().Be(60);
        stats.MaxAetherPool.Should().Be(30);
        stats.Initiative.Should().Be(4);
        stats.Soak.Should().Be(2);
        stats.MovementSpeed.Should().Be(5);
        stats.CarryingCapacity.Should().Be(40);
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when MaxHp is zero,
    /// since all characters must have positive HP.
    /// </summary>
    [Test]
    public void Create_WithZeroHp_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => DerivedStats.Create(
            maxHp: 0,
            maxStamina: 60,
            maxAetherPool: 30,
            initiative: 4,
            soak: 2,
            movementSpeed: 5,
            carryingCapacity: 40);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when MaxHp is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeHp_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => DerivedStats.Create(
            maxHp: -1,
            maxStamina: 60,
            maxAetherPool: 30,
            initiative: 4,
            soak: 2,
            movementSpeed: 5,
            carryingCapacity: 40);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when MovementSpeed is zero,
    /// since all characters must be able to move.
    /// </summary>
    [Test]
    public void Create_WithZeroMovementSpeed_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => DerivedStats.Create(
            maxHp: 139,
            maxStamina: 60,
            maxAetherPool: 30,
            initiative: 4,
            soak: 2,
            movementSpeed: 0,
            carryingCapacity: 40);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when MaxStamina is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeStamina_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => DerivedStats.Create(
            maxHp: 139,
            maxStamina: -1,
            maxAetherPool: 30,
            initiative: 4,
            soak: 2,
            movementSpeed: 5,
            carryingCapacity: 40);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when MaxAetherPool is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeAetherPool_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => DerivedStats.Create(
            maxHp: 139,
            maxStamina: 60,
            maxAetherPool: -1,
            initiative: 4,
            soak: 2,
            movementSpeed: 5,
            carryingCapacity: 40);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when Soak is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeSoak_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => DerivedStats.Create(
            maxHp: 139,
            maxStamina: 60,
            maxAetherPool: 30,
            initiative: 4,
            soak: -1,
            movementSpeed: 5,
            carryingCapacity: 40);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.Create"/> allows zero values for
    /// Stamina, Aether Pool, and Soak (valid for certain builds).
    /// </summary>
    [Test]
    public void Create_WithZeroOptionalStats_CreatesSuccessfully()
    {
        // Arrange & Act
        var stats = DerivedStats.Create(
            maxHp: 60,
            maxStamina: 0,
            maxAetherPool: 0,
            initiative: 1,
            soak: 0,
            movementSpeed: 5,
            carryingCapacity: 0);

        // Assert
        stats.MaxStamina.Should().Be(0);
        stats.MaxAetherPool.Should().Be(0);
        stats.Soak.Should().Be(0);
        stats.CarryingCapacity.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="DerivedStats.TotalResourcePool"/> returns the sum
    /// of MaxHp, MaxStamina, and MaxAetherPool.
    /// </summary>
    [Test]
    public void TotalResourcePool_ReturnsSumOfResources()
    {
        // Arrange
        var stats = DerivedStats.Create(139, 60, 30, 4, 2, 5, 40);

        // Act & Assert
        stats.TotalResourcePool.Should().Be(229, "139 + 60 + 30 = 229");
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.HasCombatStats"/> returns true when
    /// Initiative or Soak is positive.
    /// </summary>
    [Test]
    public void HasCombatStats_WithPositiveInitiative_ReturnsTrue()
    {
        // Arrange
        var stats = DerivedStats.Create(60, 20, 0, 3, 0, 5, 10);

        // Act & Assert
        stats.HasCombatStats.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.HasResourceStats"/> returns true when
    /// any resource pool is positive.
    /// </summary>
    [Test]
    public void HasResourceStats_WithPositiveHp_ReturnsTrue()
    {
        // Arrange
        var stats = DerivedStats.Create(60, 0, 0, 0, 0, 5, 0);

        // Act & Assert
        stats.HasResourceStats.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="DerivedStats.GetSummary"/> returns a formatted
    /// string with all 7 stats in pipe-separated format.
    /// </summary>
    [Test]
    public void GetSummary_ReturnsFormattedString()
    {
        // Arrange
        var stats = DerivedStats.Create(139, 60, 30, 4, 2, 5, 40);

        // Act
        var summary = stats.GetSummary();

        // Assert
        summary.Should().Be(
            "HP: 139 | Stamina: 60 | AP: 30 | Init: 4 | Soak: 2 | Move: 5 | Carry: 40");
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStats.ToString"/> returns a compact
    /// debug-friendly string with abbreviated labels.
    /// </summary>
    [Test]
    public void ToString_ReturnsCompactString()
    {
        // Arrange
        var stats = DerivedStats.Create(139, 60, 30, 4, 2, 5, 40);

        // Act
        var result = stats.ToString();

        // Assert
        result.Should().Be(
            "DerivedStats [HP:139 ST:60 AP:30 Init:4 Soak:2 Move:5 Carry:40]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALUE EQUALITY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that two DerivedStats with the same values are considered equal
    /// (record struct value equality).
    /// </summary>
    [Test]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var stats1 = DerivedStats.Create(139, 60, 30, 4, 2, 5, 40);
        var stats2 = DerivedStats.Create(139, 60, 30, 4, 2, 5, 40);

        // Act & Assert
        stats1.Should().Be(stats2);
    }

    /// <summary>
    /// Verifies that two DerivedStats with different values are not equal.
    /// </summary>
    [Test]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var stats1 = DerivedStats.Create(139, 60, 30, 4, 2, 5, 40);
        var stats2 = DerivedStats.Create(90, 45, 80, 5, 1, 5, 20);

        // Act & Assert
        stats1.Should().NotBe(stats2);
    }
}
