namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Services;

using Avalonia.Input;
using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Services;

[TestFixture]
public class KeyboardShortcutServiceTests
{
    private KeyboardShortcutService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new KeyboardShortcutService();
    }

    // ============================================================================
    // ProcessKeyDown Tests
    // ============================================================================

    [Test]
    public void ProcessKeyDown_GlobalShortcut_ExecutesAction()
    {
        // Arrange
        var executed = false;
        _service.RegisterAction("show-help", () => executed = true);
        _service.CurrentContext = ShortcutContext.Game;

        // Act
        var result = _service.ProcessKeyDown(Key.F1, KeyModifiers.None);

        // Assert
        result.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Test]
    public void ProcessKeyDown_ContextShortcut_ExecutesWhenInContext()
    {
        // Arrange
        var executed = false;
        _service.RegisterAction("toggle-inventory", () => executed = true);
        _service.CurrentContext = ShortcutContext.Game;

        // Act
        var result = _service.ProcessKeyDown(Key.I, KeyModifiers.None);

        // Assert
        result.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Test]
    public void ProcessKeyDown_ContextShortcut_DoesNotExecuteInWrongContext()
    {
        // Arrange
        var executed = false;
        _service.RegisterAction("toggle-inventory", () => executed = true);
        _service.CurrentContext = ShortcutContext.Combat; // Wrong context

        // Act
        var result = _service.ProcessKeyDown(Key.I, KeyModifiers.None);

        // Assert
        result.Should().BeFalse();
        executed.Should().BeFalse();
    }

    // ============================================================================
    // RegisterAction Tests
    // ============================================================================

    [Test]
    public void RegisterAction_ValidAction_CanBeExecuted()
    {
        // Arrange
        var counter = 0;
        _service.RegisterAction("show-help", () => counter++);

        // Act
        _service.ProcessKeyDown(Key.F1, KeyModifiers.None);
        _service.ProcessKeyDown(Key.F1, KeyModifiers.None);

        // Assert
        counter.Should().Be(2);
    }

    [Test]
    public void UnregisterAction_RemovesAction()
    {
        // Arrange
        var executed = false;
        _service.RegisterAction("show-help", () => executed = true);

        // Act
        _service.UnregisterAction("show-help");
        _service.ProcessKeyDown(Key.F1, KeyModifiers.None);

        // Assert
        executed.Should().BeFalse();
    }

    // ============================================================================
    // GetBinding/SetBinding Tests
    // ============================================================================

    [Test]
    public void GetBinding_DefaultBinding_ReturnsGesture()
    {
        // Act
        var gesture = _service.GetBinding("show-help");

        // Assert
        gesture.Should().NotBeNull();
        gesture!.Key.Should().Be(Key.F1);
    }

    [Test]
    public void SetBinding_UpdatesBinding()
    {
        // Act
        _service.SetBinding("show-help", new KeyGesture(Key.F2, KeyModifiers.None));
        var gesture = _service.GetBinding("show-help");

        // Assert
        gesture.Should().NotBeNull();
        gesture!.Key.Should().Be(Key.F2);
    }

    // ============================================================================
    // Context Filtering Tests
    // ============================================================================

    [Test]
    public void GetBindingsForContext_Global_ReturnsGlobalBindings()
    {
        // Act
        var bindings = _service.GetBindingsForContext(ShortcutContext.Global);

        // Assert
        bindings.Should().ContainKey("show-help");
        bindings.Should().ContainKey("quick-save");
    }

    [Test]
    public void GetAllShortcuts_ReturnsAllBindings()
    {
        // Act
        var shortcuts = _service.GetAllShortcuts();

        // Assert
        shortcuts.Should().Contain(s => s.Context == ShortcutContext.Global);
        shortcuts.Should().Contain(s => s.Context == ShortcutContext.Game);
        shortcuts.Should().Contain(s => s.Context == ShortcutContext.Combat);
    }
}
