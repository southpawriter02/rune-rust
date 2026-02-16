using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="MedicalSuppliesResource"/> value object.
/// Tests immutable supply management including creation, spending, adding, and queries.
/// </summary>
[TestFixture]
public class MedicalSuppliesResourceTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_Empty_InitializesWithDefaultCapacity()
    {
        // Act
        var resource = MedicalSuppliesResource.Create();

        // Assert
        resource.Supplies.Should().BeEmpty();
        resource.MaxCarryCapacity.Should().Be(MedicalSuppliesResource.DefaultMaxCapacity);
        resource.GetTotalSupplyCount().Should().Be(0);
        resource.LastModifiedAt.Should().NotBeNull();
    }

    [Test]
    public void Create_WithSupplies_InitializesCorrectly()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 2, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Salve, "Salve", "Healing salve", 3, "purchase")
        };

        // Act
        var resource = MedicalSuppliesResource.Create(supplies);

        // Assert
        resource.Supplies.Should().HaveCount(2);
        resource.MaxCarryCapacity.Should().Be(MedicalSuppliesResource.DefaultMaxCapacity);
        resource.GetTotalSupplyCount().Should().Be(2);
    }

    [Test]
    public void Create_WithCustomCapacity_SetsCapacityCorrectly()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 1, "salvage")
        };

        // Act
        var resource = MedicalSuppliesResource.Create(supplies, 5);

        // Assert
        resource.MaxCarryCapacity.Should().Be(5);
    }

    [Test]
    public void Create_ExceedingCapacity_ThrowsArgumentException()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B1", "desc", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B2", "desc", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B3", "desc", 1, "salvage")
        };

        // Act & Assert
        FluentActions.Invoking(() => MedicalSuppliesResource.Create(supplies, 2))
            .Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        FluentActions.Invoking(() => MedicalSuppliesResource.Create([], 0))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    // ===== AddSupply Tests =====

    [Test]
    public void AddSupply_WithinCapacity_ReturnsNewResourceWithItem()
    {
        // Arrange
        var resource = MedicalSuppliesResource.Create();
        var item = MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 2, "salvage");

        // Act
        var result = resource.AddSupply(item);

        // Assert
        result.Should().NotBeSameAs(resource); // Immutable — new instance
        result.GetTotalSupplyCount().Should().Be(1);
        resource.GetTotalSupplyCount().Should().Be(0); // Original unchanged
    }

    [Test]
    public void AddSupply_AtCapacity_ThrowsInvalidOperationException()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B1", "desc", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B2", "desc", 1, "salvage")
        };
        var resource = MedicalSuppliesResource.Create(supplies, 2);
        var extraItem = MedicalSupplyItem.Create(MedicalSupplyType.Salve, "Salve", "desc", 2, "purchase");

        // Act & Assert
        FluentActions.Invoking(() => resource.AddSupply(extraItem))
            .Should().Throw<InvalidOperationException>();
    }

    // ===== SpendSupply Tests =====

    [Test]
    public void SpendSupply_ByType_RemovesOneItemAndReturnsNewResource()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B1", "desc", 2, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Salve, "S1", "desc", 3, "purchase"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B2", "desc", 1, "salvage")
        };
        var resource = MedicalSuppliesResource.Create(supplies);

        // Act
        var result = resource.SpendSupply(MedicalSupplyType.Bandage);

        // Assert
        result.Should().NotBeSameAs(resource); // Immutable — new instance
        result.GetTotalSupplyCount().Should().Be(2);
        result.GetCountByType(MedicalSupplyType.Bandage).Should().Be(1); // One bandage removed
        result.GetCountByType(MedicalSupplyType.Salve).Should().Be(1); // Salve untouched
        resource.GetTotalSupplyCount().Should().Be(3); // Original unchanged
    }

    [Test]
    public void SpendSupply_TypeNotAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B1", "desc", 2, "salvage")
        };
        var resource = MedicalSuppliesResource.Create(supplies);

        // Act & Assert
        FluentActions.Invoking(() => resource.SpendSupply(MedicalSupplyType.Salve))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void SpendAnySupply_ConsumesLowestQualityFirst()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Good Bandage", "desc", 3, "craft"),
            MedicalSupplyItem.Create(MedicalSupplyType.Salve, "Poor Salve", "desc", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Herbs, "Standard Herbs", "desc", 2, "purchase")
        };
        var resource = MedicalSuppliesResource.Create(supplies);

        // Act
        var (newResource, spentItem) = resource.SpendAnySupply();

        // Assert
        spentItem.Quality.Should().Be(1); // Lowest quality first
        spentItem.SupplyType.Should().Be(MedicalSupplyType.Salve);
        newResource.GetTotalSupplyCount().Should().Be(2);
    }

    [Test]
    public void SpendAnySupply_EmptyInventory_ThrowsInvalidOperationException()
    {
        // Arrange
        var resource = MedicalSuppliesResource.Create();

        // Act & Assert
        FluentActions.Invoking(() => resource.SpendAnySupply())
            .Should().Throw<InvalidOperationException>();
    }

    // ===== Query Tests =====

    [Test]
    public void HasSupply_TypePresent_ReturnsTrue()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Bandage", "desc", 2, "salvage")
        };
        var resource = MedicalSuppliesResource.Create(supplies);

        // Act & Assert
        resource.HasSupply(MedicalSupplyType.Bandage).Should().BeTrue();
        resource.HasSupply(MedicalSupplyType.Salve).Should().BeFalse();
    }

    [Test]
    public void CanCarryMore_BelowCapacity_ReturnsTrue()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B1", "desc", 1, "salvage")
        };
        var resource = MedicalSuppliesResource.Create(supplies, 3);

        // Act & Assert
        resource.CanCarryMore().Should().BeTrue();
    }

    [Test]
    public void CanCarryMore_AtCapacity_ReturnsFalse()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B1", "desc", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B2", "desc", 1, "salvage")
        };
        var resource = MedicalSuppliesResource.Create(supplies, 2);

        // Act & Assert
        resource.CanCarryMore().Should().BeFalse();
    }

    [Test]
    public void GetHighestQualitySupply_ByType_ReturnsHighestQuality()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Poor Bandage", "desc", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Superior Bandage", "desc", 5, "craft"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Standard Bandage", "desc", 2, "purchase")
        };
        var resource = MedicalSuppliesResource.Create(supplies);

        // Act
        var best = resource.GetHighestQualitySupply(MedicalSupplyType.Bandage);

        // Assert
        best.Should().NotBeNull();
        best!.Quality.Should().Be(5);
        best.Name.Should().Be("Superior Bandage");
    }

    [Test]
    public void GetHighestQualitySupply_TypeNotPresent_ReturnsNull()
    {
        // Arrange
        var resource = MedicalSuppliesResource.Create();

        // Act
        var result = resource.GetHighestQualitySupply(MedicalSupplyType.Salve);

        // Assert
        result.Should().BeNull();
    }

    // ===== Display Tests =====

    [Test]
    public void GetInventorySummary_WithMixedSupplies_FormatsCorrectly()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B1", "desc", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B2", "desc", 2, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Salve, "S1", "desc", 3, "purchase")
        };
        var resource = MedicalSuppliesResource.Create(supplies);

        // Act
        var summary = resource.GetInventorySummary();

        // Assert
        summary.Should().Contain("Bandage(2)");
        summary.Should().Contain("Salve(1)");
        summary.Should().EndWith("3/10");
    }

    [Test]
    public void GetInventorySummary_Empty_ReturnsEmptySummary()
    {
        // Arrange
        var resource = MedicalSuppliesResource.Create();

        // Act
        var summary = resource.GetInventorySummary();

        // Assert
        summary.Should().Contain("Empty");
        summary.Should().Contain("0/10");
    }

    [Test]
    public void GetFormattedValue_ReturnsCurrentOverMax()
    {
        // Arrange
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "B1", "desc", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Salve, "S1", "desc", 2, "purchase")
        };
        var resource = MedicalSuppliesResource.Create(supplies, 5);

        // Act
        var formatted = resource.GetFormattedValue();

        // Assert
        formatted.Should().Be("2/5");
    }
}
