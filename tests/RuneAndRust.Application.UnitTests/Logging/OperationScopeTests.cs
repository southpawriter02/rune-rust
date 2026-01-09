using FluentAssertions;
using RuneAndRust.Application.Services;
using RuneAndRust.TestUtilities.Logging;

namespace RuneAndRust.Application.UnitTests.Logging;

/// <summary>
/// Tests for the OperationScope correlation utility.
/// </summary>
[TestFixture]
public class OperationScopeTests
{
    private TestLogger<OperationScope> _logger = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new TestLogger<OperationScope>();
    }

    [Test]
    public void OperationScope_GeneratesUniqueId_OnConstruction()
    {
        // Arrange & Act
        var scope1 = new OperationScope(_logger);
        var scope2 = new OperationScope(_logger);

        // Assert
        scope1.OperationId.Should().NotBeNullOrEmpty();
        scope2.OperationId.Should().NotBeNullOrEmpty();
        scope1.OperationId.Should().NotBe(scope2.OperationId);
        scope1.OperationId.Should().HaveLength(8); // 8-char hex string
    }

    [Test]
    public void OperationScope_TracksSessionId_WhenProvided()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var scope = new OperationScope(_logger, sessionId);

        // Assert
        scope.SessionId.Should().Be(sessionId);
    }

    [Test]
    public void OperationScope_SessionId_IsNullByDefault()
    {
        // Arrange & Act
        var scope = new OperationScope(_logger);

        // Assert
        scope.SessionId.Should().BeNull();
    }

    [Test]
    public void OperationScope_BeginOperation_ReturnsDisposable()
    {
        // Arrange
        var scope = new OperationScope(_logger);

        // Act
        var operation = scope.BeginOperation("TestOperation");

        // Assert
        operation.Should().NotBeNull();
        operation.Should().BeAssignableTo<IDisposable>();
    }

    [Test]
    public void OperationScope_SetSessionId_UpdatesProperty()
    {
        // Arrange
        var scope = new OperationScope(_logger);
        var newSessionId = Guid.NewGuid();

        // Act
        scope.SetSessionId(newSessionId);

        // Assert
        scope.SessionId.Should().Be(newSessionId);
    }

    [Test]
    public void OperationScope_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act
        var act = () => new OperationScope(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }
}
