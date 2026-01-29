using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="MythForgedResult"/> value object.
/// </summary>
/// <remarks>
/// Tests cover the factory methods, validation logic, property access,
/// and string representation for debugging.
/// </remarks>
[TestFixture]
public class MythForgedResultTests
{
    /// <summary>
    /// Verifies that Succeeded creates a success result with correct properties.
    /// </summary>
    [Test]
    public void Succeeded_WithItem_CreatesSuccessResult()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var result = MythForgedResult.Succeeded(item, affinityApplied: true, poolSize: 5);

        // Assert
        result.Success.Should().BeTrue();
        result.Item.Should().Be(item);
        result.AffinityApplied.Should().BeTrue();
        result.FallbackReason.Should().Be(FallbackReason.None);
        result.PoolSize.Should().Be(5);
    }

    /// <summary>
    /// Verifies that Succeeded without affinity sets AffinityApplied to false.
    /// </summary>
    [Test]
    public void Succeeded_WithoutAffinity_SetsAffinityAppliedFalse()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var result = MythForgedResult.Succeeded(item, affinityApplied: false, poolSize: 3);

        // Assert
        result.Success.Should().BeTrue();
        result.AffinityApplied.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Failed creates a failed result with correct properties.
    /// </summary>
    [Test]
    public void Failed_WithReason_CreatesFailedResult()
    {
        // Arrange & Act
        var result = MythForgedResult.Failed(FallbackReason.PoolExhausted, poolSize: 0);

        // Assert
        result.Success.Should().BeFalse();
        result.Item.Should().BeNull();
        result.FallbackReason.Should().Be(FallbackReason.PoolExhausted);
        result.AffinityApplied.Should().BeFalse();
        result.PoolSize.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Failed with pool size records the value.
    /// </summary>
    [Test]
    public void Failed_WithPoolSize_RecordsPoolSize()
    {
        // Arrange & Act
        var result = MythForgedResult.Failed(FallbackReason.DropChanceFailed, poolSize: 5);

        // Assert
        result.PoolSize.Should().Be(5);
    }

    /// <summary>
    /// Verifies that Failed throws ArgumentException for None reason.
    /// </summary>
    [Test]
    public void Failed_WithNoneReason_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MythForgedResult.Failed(FallbackReason.None);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*fallback reason*");
    }

    /// <summary>
    /// Verifies that Succeeded throws ArgumentNullException for null item.
    /// </summary>
    [Test]
    public void Succeeded_WithNullItem_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => MythForgedResult.Succeeded(null!, false, 0);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("item");
    }

    /// <summary>
    /// Verifies that ToString for success result includes item ID and affinity.
    /// </summary>
    [Test]
    public void ToString_ForSuccessResult_ContainsItemIdAndAffinity()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var result = MythForgedResult.Succeeded(item, affinityApplied: true, poolSize: 5);

        // Act
        var text = result.ToString();

        // Assert
        text.Should().Contain("Success");
        text.Should().Contain("test-item");
        text.Should().Contain("Affinity:True");
        text.Should().Contain("Pool:5");
    }

    /// <summary>
    /// Verifies that ToString for failed result includes reason.
    /// </summary>
    [Test]
    public void ToString_ForFailedResult_ContainsReason()
    {
        // Arrange
        var result = MythForgedResult.Failed(FallbackReason.PoolExhausted);

        // Act
        var text = result.ToString();

        // Assert
        text.Should().Contain("Failed");
        text.Should().Contain("PoolExhausted");
        text.Should().Contain("Pool:0");
    }

    /// <summary>
    /// Verifies all fallback reasons can be used in Failed.
    /// </summary>
    [TestCase(FallbackReason.PoolExhausted)]
    [TestCase(FallbackReason.NoMatchingSource)]
    [TestCase(FallbackReason.DropChanceFailed)]
    [TestCase(FallbackReason.TierTooLow)]
    public void Failed_WithDifferentReasons_CreatesFailed(FallbackReason reason)
    {
        // Arrange & Act
        var result = MythForgedResult.Failed(reason);

        // Assert
        result.Success.Should().BeFalse();
        result.FallbackReason.Should().Be(reason);
    }

    /// <summary>
    /// Verifies that two success results with same values are equal.
    /// </summary>
    [Test]
    public void Equality_SuccessWithSameValues_AreEqual()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var result1 = MythForgedResult.Succeeded(item, true, 5);
        var result2 = MythForgedResult.Succeeded(item, true, 5);

        // Assert
        result1.Should().Be(result2);
        (result1 == result2).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that two failed results with same values are equal.
    /// </summary>
    [Test]
    public void Equality_FailedWithSameValues_AreEqual()
    {
        // Arrange
        var result1 = MythForgedResult.Failed(FallbackReason.PoolExhausted, 0);
        var result2 = MythForgedResult.Failed(FallbackReason.PoolExhausted, 0);

        // Assert
        result1.Should().Be(result2);
    }

    /// <summary>
    /// Verifies that results with different success states are not equal.
    /// </summary>
    [Test]
    public void Equality_DifferentSuccessStates_AreNotEqual()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var successResult = MythForgedResult.Succeeded(item, false, 1);
        var failedResult = MythForgedResult.Failed(FallbackReason.PoolExhausted);

        // Assert
        successResult.Should().NotBe(failedResult);
    }

    /// <summary>
    /// Creates a test UniqueItem for unit testing.
    /// </summary>
    private static UniqueItem CreateTestUniqueItem() =>
        UniqueItem.Create(
            "test-item",
            "Test Item",
            "A test item for unit testing.",
            "Flavor text for the test item.",
            EquipmentCategory.Weapon,
            ItemStats.Empty,
            new[] { DropSource.Create(DropSourceType.Boss, "test-boss", 5.0m) });
}
