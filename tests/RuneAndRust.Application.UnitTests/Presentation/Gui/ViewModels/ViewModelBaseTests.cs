using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Tests for a sample view model inheriting from ViewModelBase.
/// </summary>
public class TestViewModel : ViewModelBase
{
    private string _testProperty = "";
    
    public string TestProperty
    {
        get => _testProperty;
        set => SetProperty(ref _testProperty, value);
    }
}

/// <summary>
/// Unit tests for <see cref="ViewModelBase"/>.
/// </summary>
[TestFixture]
public class ViewModelBaseTests
{
    [Test]
    public void ViewModelBase_DerivedClass_CanBeInstantiated()
    {
        // Act
        var vm = new TestViewModel();

        // Assert
        vm.Should().NotBeNull();
    }

    [Test]
    public void SetProperty_WhenValueChanges_RaisesPropertyChanged()
    {
        // Arrange
        var vm = new TestViewModel();
        var propertyChangedRaised = false;
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(TestViewModel.TestProperty))
                propertyChangedRaised = true;
        };

        // Act
        vm.TestProperty = "New Value";

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Test]
    public void SetProperty_WhenValueSame_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var vm = new TestViewModel { TestProperty = "Same" };
        var propertyChangedRaised = false;
        vm.PropertyChanged += (_, _) => propertyChangedRaised = true;

        // Act
        vm.TestProperty = "Same";

        // Assert
        propertyChangedRaised.Should().BeFalse();
    }

    [Test]
    public void SetProperty_UpdatesUnderlyingValue()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.TestProperty = "Updated";

        // Assert
        vm.TestProperty.Should().Be("Updated");
    }

    [Test]
    public void ViewModelBase_ImplementsINotifyPropertyChanged()
    {
        // Arrange & Act
        var vm = new TestViewModel();

        // Assert
        vm.Should().BeAssignableTo<System.ComponentModel.INotifyPropertyChanged>();
    }
}
