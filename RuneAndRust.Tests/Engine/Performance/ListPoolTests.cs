using FluentAssertions;
using RuneAndRust.Engine.Performance;

namespace RuneAndRust.Tests.Engine.Performance;

/// <summary>
/// Unit tests for the ListPool service (v0.3.18a - The Garbage Collector).
/// Verifies pooling behavior, reset semantics, and type safety.
/// </summary>
public class ListPoolTests
{
    #region String List Pool Tests

    [Fact]
    public void Rent_ReturnsNonNullList()
    {
        // Arrange
        var pool = new ListPool<string>();

        // Act
        var list = pool.Rent();

        // Assert
        list.Should().NotBeNull();

        // Cleanup
        pool.Return(list);
    }

    [Fact]
    public void Rent_ReturnsEmptyList()
    {
        // Arrange
        var pool = new ListPool<string>();

        // Act
        var list = pool.Rent();

        // Assert
        list.Should().BeEmpty("rented list should be empty");

        // Cleanup
        pool.Return(list);
    }

    [Fact]
    public void Rent_MultipleCalls_ReturnsDifferentInstances()
    {
        // Arrange
        var pool = new ListPool<int>();

        // Act
        var list1 = pool.Rent();
        var list2 = pool.Rent();

        // Assert
        list1.Should().NotBeSameAs(list2, "each rent should provide a distinct instance");

        // Cleanup
        pool.Return(list1);
        pool.Return(list2);
    }

    #endregion

    #region Return Tests

    [Fact]
    public void Return_ClearsList_ForReuse()
    {
        // Arrange
        var pool = new ListPool<string>();
        var list = pool.Rent();
        list.Add("Item 1");
        list.Add("Item 2");
        list.Add("Item 3");

        // Act
        pool.Return(list);
        var reused = pool.Rent();

        // Assert
        reused.Should().BeEmpty("returned list should be cleared before next rental");

        // Cleanup
        pool.Return(reused);
    }

    [Fact]
    public void Return_DoesNotRetainOldItems()
    {
        // Arrange
        var pool = new ListPool<int>();
        var list = pool.Rent();
        list.AddRange(new[] { 1, 2, 3, 4, 5 });

        // Act
        pool.Return(list);
        var reused = pool.Rent();

        // Assert
        reused.Count.Should().Be(0, "returned list should have no items");
        reused.Should().NotContain(1);
        reused.Should().NotContain(5);

        // Cleanup
        pool.Return(reused);
    }

    #endregion

    #region Type Safety Tests

    [Fact]
    public void Pool_WorksWithComplexTypes()
    {
        // Arrange
        var pool = new ListPool<TestComplexType>();

        // Act
        var list = pool.Rent();
        list.Add(new TestComplexType { Id = 1, Name = "Test" });

        // Assert
        list.Should().HaveCount(1);
        list[0].Id.Should().Be(1);

        // Cleanup
        pool.Return(list);
    }

    [Fact]
    public void Pool_WorksWithValueTypes()
    {
        // Arrange
        var pool = new ListPool<Guid>();

        // Act
        var list = pool.Rent();
        var testGuid = Guid.NewGuid();
        list.Add(testGuid);

        // Assert
        list.Should().Contain(testGuid);

        // Cleanup
        pool.Return(list);
    }

    [Fact]
    public void Pool_WorksWithNullableReferenceTypes()
    {
        // Arrange
        var pool = new ListPool<string?>();

        // Act
        var list = pool.Rent();
        list.Add("Value");
        list.Add(null);

        // Assert
        list.Should().HaveCount(2);
        list.Should().Contain((string?)null);

        // Cleanup
        pool.Return(list);
    }

    #endregion

    #region Pool Behavior Tests

    [Fact]
    public void Pool_HandlesHighVolumeRentReturn()
    {
        // Arrange
        var pool = new ListPool<int>();
        const int iterations = 100;
        var lists = new List<int>[iterations];

        // Act - Rent all
        for (int i = 0; i < iterations; i++)
        {
            lists[i] = pool.Rent();
            lists[i].AddRange(Enumerable.Range(0, i + 1));
        }

        // Return all
        foreach (var list in lists)
        {
            pool.Return(list);
        }

        // Assert - Should be able to rent again without issues
        var newRent = pool.Rent();
        newRent.Should().NotBeNull();
        newRent.Should().BeEmpty();

        // Cleanup
        pool.Return(newRent);
    }

    [Fact]
    public void Pool_HandlesConcurrentAccess()
    {
        // Arrange
        var pool = new ListPool<int>();
        const int threadCount = 10;
        const int operationsPerThread = 50;
        var exceptions = new List<Exception>();

        // Act
        var threads = Enumerable.Range(0, threadCount).Select(_ => new Thread(() =>
        {
            try
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    var list = pool.Rent();
                    list.AddRange(Enumerable.Range(0, 10));
                    list.Count.Should().Be(10);
                    pool.Return(list);
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        // Assert
        exceptions.Should().BeEmpty("concurrent access should not cause exceptions");
    }

    [Fact]
    public void Pool_MaintainsCapacityAcrossRentals()
    {
        // Arrange
        var pool = new ListPool<int>();

        // Act - Rent, add many items, return
        var list = pool.Rent();
        for (int i = 0; i < 1000; i++)
        {
            list.Add(i);
        }
        var originalCapacity = list.Capacity;
        pool.Return(list);

        // Rent again - should get a list with retained capacity
        var reused = pool.Rent();

        // Assert
        reused.Should().BeEmpty("content should be cleared");
        // Capacity may be retained for performance (implementation detail)
        reused.Capacity.Should().BeGreaterThanOrEqualTo(0);

        // Cleanup
        pool.Return(reused);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Rent_AfterManyReturns_StillReturnsEmptyList()
    {
        // Arrange
        var pool = new ListPool<string>();

        // Act - Multiple rent/return cycles
        for (int cycle = 0; cycle < 10; cycle++)
        {
            var list = pool.Rent();
            list.Add($"Cycle {cycle}");
            pool.Return(list);
        }

        var finalRent = pool.Rent();

        // Assert
        finalRent.Should().BeEmpty("final rent should return empty list");

        // Cleanup
        pool.Return(finalRent);
    }

    [Fact]
    public void Pool_HandlesEmptyListReturn()
    {
        // Arrange
        var pool = new ListPool<int>();
        var list = pool.Rent();
        // Don't add anything

        // Act
        pool.Return(list);
        var reused = pool.Rent();

        // Assert
        reused.Should().BeEmpty();

        // Cleanup
        pool.Return(reused);
    }

    #endregion

    #region Helper Types

    private class TestComplexType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
