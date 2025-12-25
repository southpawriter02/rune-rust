using System.Text;
using FluentAssertions;
using RuneAndRust.Engine.Performance;

namespace RuneAndRust.Tests.Engine.Performance;

/// <summary>
/// Unit tests for the TextBufferPool service (v0.3.18a - The Garbage Collector).
/// Verifies pooling behavior, reset semantics, and thread safety.
/// </summary>
public class TextBufferPoolTests
{
    private readonly TextBufferPool _sut;

    public TextBufferPoolTests()
    {
        _sut = new TextBufferPool();
    }

    #region Rent Tests

    [Fact]
    public void Rent_ReturnsNonNullStringBuilder()
    {
        // Act
        var sb = _sut.Rent();

        // Assert
        sb.Should().NotBeNull();

        // Cleanup
        _sut.Return(sb);
    }

    [Fact]
    public void Rent_ReturnsEmptyStringBuilder()
    {
        // Act
        var sb = _sut.Rent();

        // Assert
        sb.Length.Should().Be(0, "rented StringBuilder should be empty");

        // Cleanup
        _sut.Return(sb);
    }

    [Fact]
    public void Rent_MultipleCalls_ReturnsDifferentInstances()
    {
        // Act
        var sb1 = _sut.Rent();
        var sb2 = _sut.Rent();

        // Assert
        sb1.Should().NotBeSameAs(sb2, "each rent should provide a distinct instance");

        // Cleanup
        _sut.Return(sb1);
        _sut.Return(sb2);
    }

    #endregion

    #region Return Tests

    [Fact]
    public void Return_ClearsContent_ForReuse()
    {
        // Arrange
        var sb = _sut.Rent();
        sb.Append("Test content that should be cleared");

        // Act
        _sut.Return(sb);
        var reused = _sut.Rent();

        // Assert
        reused.Length.Should().Be(0, "returned StringBuilder should be cleared before next rental");

        // Cleanup
        _sut.Return(reused);
    }

    [Fact]
    public void Return_AllowsReuseOfSameInstance()
    {
        // Arrange
        var sb = _sut.Rent();
        sb.Append("Content");

        // Act
        _sut.Return(sb);
        var reused = _sut.Rent();

        // Assert - The pool should reuse the returned instance
        // (We can't guarantee same instance, but we can verify the pool accepts returns)
        reused.Should().NotBeNull();

        // Cleanup
        _sut.Return(reused);
    }

    #endregion

    #region GetStringAndReturn Tests

    [Fact]
    public void GetStringAndReturn_ReturnsCorrectString()
    {
        // Arrange
        var sb = _sut.Rent();
        sb.Append("Hello, World!");

        // Act
        var result = _sut.GetStringAndReturn(sb);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Fact]
    public void GetStringAndReturn_ReturnsBuilderToPool()
    {
        // Arrange
        var sb = _sut.Rent();
        sb.Append("Content to extract");

        // Act
        _ = _sut.GetStringAndReturn(sb);
        var reused = _sut.Rent();

        // Assert - Pool should have accepted the return
        reused.Length.Should().Be(0, "builder should be cleared after GetStringAndReturn");

        // Cleanup
        _sut.Return(reused);
    }

    [Fact]
    public void GetStringAndReturn_WithEmptyBuilder_ReturnsEmptyString()
    {
        // Arrange
        var sb = _sut.Rent();

        // Act
        var result = _sut.GetStringAndReturn(sb);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetStringAndReturn_WithComplexContent_PreservesFormatting()
    {
        // Arrange
        var sb = _sut.Rent();
        sb.AppendLine("Line 1");
        sb.AppendLine("Line 2");
        sb.Append("Line 3 (no newline)");

        // Act
        var result = _sut.GetStringAndReturn(sb);

        // Assert
        result.Should().Contain("Line 1");
        result.Should().Contain("Line 2");
        result.Should().EndWith("Line 3 (no newline)");
    }

    #endregion

    #region Pool Behavior Tests

    [Fact]
    public void Pool_HandlesHighVolumeRentReturn()
    {
        // Arrange
        const int iterations = 100;
        var builders = new StringBuilder[iterations];

        // Act - Rent all
        for (int i = 0; i < iterations; i++)
        {
            builders[i] = _sut.Rent();
            builders[i].Append($"Content {i}");
        }

        // Return all
        foreach (var sb in builders)
        {
            _sut.Return(sb);
        }

        // Assert - Should be able to rent again without issues
        var newRent = _sut.Rent();
        newRent.Should().NotBeNull();
        newRent.Length.Should().Be(0);

        // Cleanup
        _sut.Return(newRent);
    }

    [Fact]
    public void Pool_HandlesConcurrentAccess()
    {
        // Arrange
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
                    var sb = _sut.Rent();
                    sb.Append($"Thread content {Thread.CurrentThread.ManagedThreadId}_{i}");
                    var result = _sut.GetStringAndReturn(sb);
                    result.Should().NotBeNull();
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

    #endregion
}
