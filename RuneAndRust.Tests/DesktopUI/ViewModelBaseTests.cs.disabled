using RuneAndRust.DesktopUI.ViewModels;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for ViewModelBase reactive functionality and lifecycle.
/// </summary>
public class ViewModelBaseTests
{
    [Fact]
    public void ViewModelBase_SupportsPropertyChangeNotification()
    {
        // Arrange
        var vm = new TestViewModel();
        string? changedProperty = null;
        vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

        // Act
        vm.TestProperty = "New Value";

        // Assert
        Assert.Equal("TestProperty", changedProperty);
        Assert.Equal("New Value", vm.TestProperty);
    }

    [Fact]
    public void ViewModelBase_SupportsActivation()
    {
        // Arrange
        var vm = new TestViewModel();

        // Assert initial state
        Assert.False(vm.WasActivated);
        Assert.False(vm.WasDeactivated);

        // Act - Activate
        vm.Activator.Activate();

        // Assert activated
        Assert.True(vm.WasActivated);
        Assert.False(vm.WasDeactivated);
    }

    [Fact]
    public void ViewModelBase_SupportsDeactivation()
    {
        // Arrange
        var vm = new TestViewModel();
        vm.Activator.Activate();

        // Act - Deactivate
        vm.Activator.Deactivate();

        // Assert deactivated
        Assert.True(vm.WasActivated);
        Assert.True(vm.WasDeactivated);
    }

    [Fact]
    public void ViewModelBase_SupportsDispose()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Dispose();

        // Assert - should not throw
        Assert.True(true);
    }

    /// <summary>
    /// Test view model for testing base functionality.
    /// </summary>
    private class TestViewModel : ViewModelBase
    {
        private string _testProperty = "";

        public string TestProperty
        {
            get => _testProperty;
            set => this.RaiseAndSetIfChanged(ref _testProperty, value);
        }

        public bool WasActivated { get; private set; }
        public bool WasDeactivated { get; private set; }

        protected override void OnActivated()
        {
            WasActivated = true;
        }

        protected override void OnDeactivated()
        {
            WasDeactivated = true;
        }
    }
}
