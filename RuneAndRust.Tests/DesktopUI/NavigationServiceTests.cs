using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for NavigationService.
/// </summary>
public class NavigationServiceTests
{
    [Fact]
    public void NavigationService_NavigateTo_SetsCurrentView()
    {
        // Arrange
        var service = new NavigationService();
        var viewModel = new TestViewModel();

        // Act
        service.NavigateTo(viewModel);

        // Assert
        Assert.Equal(viewModel, service.CurrentView);
    }

    [Fact]
    public void NavigationService_NavigateTo_RaisesCurrentViewChanged()
    {
        // Arrange
        var service = new NavigationService();
        var viewModel = new TestViewModel();
        ViewModelBase? changedView = null;
        service.CurrentViewChanged += (s, vm) => changedView = vm;

        // Act
        service.NavigateTo(viewModel);

        // Assert
        Assert.Equal(viewModel, changedView);
    }

    [Fact]
    public void NavigationService_NavigateTo_ThrowsOnNull()
    {
        // Arrange
        var service = new NavigationService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.NavigateTo<TestViewModel>(null!));
    }

    private class TestViewModel : ViewModelBase
    {
    }
}
