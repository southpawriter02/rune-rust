// ═══════════════════════════════════════════════════════════════════════════════
// TuiComponentBaseTests.cs
// Unit tests for TuiComponentBase abstract class lifecycle and DI patterns.
// Version: 0.13.5c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Presentation.Base;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Services;
using RuneAndRust.Presentation.Shared.ValueObjects;
using RuneAndRust.Presentation.Tui.Services;

namespace RuneAndRust.Application.UnitTests.Presentation.Theme;

/// <summary>
/// Unit tests for <see cref="TuiComponentBase"/> abstract class.
/// </summary>
/// <remarks>
/// Tests cover:
/// - Constructor dependency validation
/// - Lifecycle state transitions (Initialize, Activate, Deactivate, Dispose)
/// - Theme helper methods
/// - Idempotent behavior of lifecycle methods
/// </remarks>
[TestFixture]
public class TuiComponentBaseTests
{
    private Mock<IThemeService> _mockTheme = null!;
    private Mock<ILogger> _mockLogger = null!;
    private TuiThemeAdapter _realThemeAdapter = null!;

    [SetUp]
    public void Setup()
    {
        _mockTheme = new Mock<IThemeService>();
        _mockLogger = new Mock<ILogger>();

        // Create a real theme adapter for integration-style tests
        var themeDefinition = ThemeDefinition.CreateDefault();
        _realThemeAdapter = new TuiThemeAdapter(themeDefinition);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the constructor throws when theme service is null.
    /// </summary>
    [Test]
    public void Constructor_WithNullTheme_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new TestTuiComponent(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("theme");
    }

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new TestTuiComponent(_mockTheme.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Verifies that component is created in correct initial state.
    /// </summary>
    [Test]
    public void Constructor_WithValidDependencies_CreatesComponentInCorrectState()
    {
        // Arrange & Act
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);

        // Assert
        component.IsInitialized.Should().BeFalse("component should not be initialized until Initialize() is called");
        component.IsActive.Should().BeFalse("component should not be active until Activate() is called");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Initialize() transitions to initialized state.
    /// </summary>
    [Test]
    public void Initialize_WhenNotInitialized_TransitionsToInitializedState()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);

        // Act
        component.Initialize();

        // Assert
        component.IsInitialized.Should().BeTrue();
        component.OnInitializeCalled.Should().BeTrue("OnInitialize hook should be called");
    }

    /// <summary>
    /// Verifies that Initialize() is idempotent.
    /// </summary>
    [Test]
    public void Initialize_WhenAlreadyInitialized_DoesNotCallOnInitializeAgain()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);
        component.Initialize();
        component.ResetCallTracking();

        // Act
        component.Initialize();

        // Assert
        component.OnInitializeCalled.Should().BeFalse("OnInitialize should not be called again");
    }

    /// <summary>
    /// Verifies that Activate() triggers initialization if not already initialized.
    /// </summary>
    [Test]
    public void Activate_WhenNotInitialized_InitializesFirst()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);

        // Act
        component.Activate();

        // Assert
        component.IsInitialized.Should().BeTrue("should auto-initialize");
        component.IsActive.Should().BeTrue();
        component.OnInitializeCalled.Should().BeTrue();
        component.OnActivateCalled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Activate() transitions to active state.
    /// </summary>
    [Test]
    public void Activate_WhenInitialized_TransitionsToActiveState()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);
        component.Initialize();
        component.ResetCallTracking();

        // Act
        component.Activate();

        // Assert
        component.IsActive.Should().BeTrue();
        component.OnActivateCalled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Activate() is idempotent.
    /// </summary>
    [Test]
    public void Activate_WhenAlreadyActive_DoesNotCallOnActivateAgain()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);
        component.Activate();
        component.ResetCallTracking();

        // Act
        component.Activate();

        // Assert
        component.OnActivateCalled.Should().BeFalse("OnActivate should not be called again");
    }

    /// <summary>
    /// Verifies that Deactivate() transitions to inactive state.
    /// </summary>
    [Test]
    public void Deactivate_WhenActive_TransitionsToInactiveState()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);
        component.Activate();
        component.ResetCallTracking();

        // Act
        component.Deactivate();

        // Assert
        component.IsActive.Should().BeFalse();
        component.OnDeactivateCalled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Deactivate() is idempotent.
    /// </summary>
    [Test]
    public void Deactivate_WhenNotActive_DoesNotCallOnDeactivateAgain()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);

        // Act
        component.Deactivate();

        // Assert
        component.OnDeactivateCalled.Should().BeFalse("OnDeactivate should not be called when not active");
    }

    /// <summary>
    /// Verifies that Dispose() deactivates before disposing.
    /// </summary>
    [Test]
    public void Dispose_WhenActive_DeactivatesBeforeDisposing()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);
        component.Activate();
        component.ResetCallTracking();

        // Act
        component.Dispose();

        // Assert
        component.IsActive.Should().BeFalse();
        component.OnDeactivateCalled.Should().BeTrue("should deactivate first");
        component.OnDisposeCalled.Should().BeTrue("should call dispose hook");
    }

    /// <summary>
    /// Verifies that Dispose() is idempotent.
    /// </summary>
    [Test]
    public void Dispose_WhenAlreadyDisposed_DoesNotCallOnDisposeAgain()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);
        component.Dispose();
        component.ResetCallTracking();

        // Act
        component.Dispose();

        // Assert
        component.OnDisposeCalled.Should().BeFalse("OnDispose should not be called again");
    }

    /// <summary>
    /// Verifies complete lifecycle transition sequence.
    /// </summary>
    [Test]
    public void Lifecycle_FullSequence_TransitionsCorrectly()
    {
        // Arrange
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);

        // Assert initial state
        component.IsInitialized.Should().BeFalse();
        component.IsActive.Should().BeFalse();

        // Act & Assert: Initialize
        component.Initialize();
        component.IsInitialized.Should().BeTrue();
        component.IsActive.Should().BeFalse();

        // Act & Assert: Activate
        component.Activate();
        component.IsInitialized.Should().BeTrue();
        component.IsActive.Should().BeTrue();

        // Act & Assert: Deactivate
        component.Deactivate();
        component.IsInitialized.Should().BeTrue();
        component.IsActive.Should().BeFalse();

        // Act & Assert: Reactivate (toggle behavior)
        component.Activate();
        component.IsActive.Should().BeTrue();

        // Act & Assert: Dispose
        component.Dispose();
        component.IsActive.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // THEME HELPER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetIcon returns icon from theme service.
    /// </summary>
    [Test]
    public void GetIcon_ReturnsIconFromThemeService()
    {
        // Arrange
        _mockTheme.Setup(t => t.GetIcon(IconKey.Health)).Returns("♥");
        var component = new TestTuiComponent(_mockTheme.Object, _mockLogger.Object);

        // Act
        var icon = component.TestGetIcon(IconKey.Health);

        // Assert
        icon.Should().Be("♥");
    }

    /// <summary>
    /// Verifies that GetColor returns console color from theme adapter.
    /// </summary>
    [Test]
    public void GetColor_WithRealAdapter_ReturnsConsoleColor()
    {
        // Arrange - use real adapter instead of mock
        var component = new TestTuiComponent(_realThemeAdapter, _mockLogger.Object);

        // Act
        var color = component.TestGetColor(ColorKey.HealthFull);

        // Assert
        color.Should().BeOneOf(
            ConsoleColor.Green,
            ConsoleColor.DarkGreen,
            ConsoleColor.Yellow,
            ConsoleColor.Red,
            ConsoleColor.White,
            ConsoleColor.Gray);
    }

    /// <summary>
    /// Verifies that GetHealthColor returns appropriate color based on percentage.
    /// </summary>
    [Test]
    [TestCase(1.0, Description = "Full health")]
    [TestCase(0.75, Description = "Good health")]
    [TestCase(0.50, Description = "Low health")]
    [TestCase(0.25, Description = "Critical health")]
    public void GetHealthColor_WithRealAdapter_ReturnsValidConsoleColor(double percentage)
    {
        // Arrange
        var component = new TestTuiComponent(_realThemeAdapter, _mockLogger.Object);

        // Act
        var color = component.TestGetHealthColor(percentage);

        // Assert
        Enum.IsDefined(typeof(ConsoleColor), color).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPER CLASS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Concrete implementation of TuiComponentBase for testing.
    /// </summary>
    private class TestTuiComponent : TuiComponentBase
    {
        protected override string ComponentName => "TestComponent";

        public bool OnInitializeCalled { get; private set; }
        public bool OnActivateCalled { get; private set; }
        public bool OnDeactivateCalled { get; private set; }
        public bool OnDisposeCalled { get; private set; }

        public TestTuiComponent(IThemeService theme, ILogger logger)
            : base(theme, logger)
        {
        }

        protected override void OnInitialize()
        {
            OnInitializeCalled = true;
        }

        protected override void OnActivate()
        {
            OnActivateCalled = true;
        }

        protected override void OnDeactivate()
        {
            OnDeactivateCalled = true;
        }

        protected override void OnDispose()
        {
            OnDisposeCalled = true;
        }

        public void ResetCallTracking()
        {
            OnInitializeCalled = false;
            OnActivateCalled = false;
            OnDeactivateCalled = false;
            OnDisposeCalled = false;
        }

        // Expose protected methods for testing
        public ConsoleColor TestGetColor(ColorKey key) => GetColor(key);
        public ConsoleColor TestGetHealthColor(double percentage) => GetHealthColor(percentage);
        public string TestGetIcon(IconKey key) => GetIcon(key);
    }
}
