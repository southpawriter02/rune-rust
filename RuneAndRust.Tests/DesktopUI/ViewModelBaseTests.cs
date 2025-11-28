using RuneAndRust.DesktopUI.ViewModels;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for ViewModelBase reactive functionality and lifecycle.
/// v0.43.21: Updated tests for current ViewModelBase implementation.
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

    [Fact]
    public void ViewModelBase_ActivatorIsNotNull()
    {
        // Arrange & Act
        var vm = new TestViewModel();

        // Assert
        Assert.NotNull(vm.Activator);
    }

    [Fact]
    public void ViewModelBase_PropertyChangeNotification_OnlyFiresOnChange()
    {
        // Arrange
        var vm = new TestViewModel();
        vm.TestProperty = "Initial";
        int changeCount = 0;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "TestProperty")
                changeCount++;
        };

        // Act - Set to same value
        vm.TestProperty = "Initial";

        // Assert - Should not fire
        Assert.Equal(0, changeCount);

        // Act - Set to different value
        vm.TestProperty = "Changed";

        // Assert - Should fire once
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public void ViewModelBase_MultiplePropertyChanges_RaisesMultipleNotifications()
    {
        // Arrange
        var vm = new TestViewModel();
        var changedProperties = new List<string>();
        vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        // Act
        vm.TestProperty = "Value1";
        vm.AnotherProperty = 42;

        // Assert
        Assert.Contains("TestProperty", changedProperties);
        Assert.Contains("AnotherProperty", changedProperties);
    }

    [Fact]
    public void ViewModelBase_ActivationDeactivationCycle_CanRepeat()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act - First cycle
        vm.Activator.Activate();
        vm.Activator.Deactivate();

        // Reset tracking
        vm.ResetTracking();

        // Act - Second cycle
        vm.Activator.Activate();
        vm.Activator.Deactivate();

        // Assert - Both cycles completed
        Assert.True(vm.WasActivated);
        Assert.True(vm.WasDeactivated);
    }

    /// <summary>
    /// Test view model for testing base functionality.
    /// </summary>
    private class TestViewModel : ViewModelBase
    {
        private string _testProperty = "";
        private int _anotherProperty;

        public string TestProperty
        {
            get => _testProperty;
            set => this.RaiseAndSetIfChanged(ref _testProperty, value);
        }

        public int AnotherProperty
        {
            get => _anotherProperty;
            set => this.RaiseAndSetIfChanged(ref _anotherProperty, value);
        }

        public bool WasActivated { get; private set; }
        public bool WasDeactivated { get; private set; }

        public void ResetTracking()
        {
            WasActivated = false;
            WasDeactivated = false;
        }

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
