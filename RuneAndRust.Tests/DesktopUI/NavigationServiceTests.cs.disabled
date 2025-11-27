using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using Serilog;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// v0.43.21: Comprehensive tests for NavigationService.
/// Tests navigation, back navigation, and view model factory registration.
/// </summary>
public class NavigationServiceTests
{
    private readonly ILogger _mockLogger;

    public NavigationServiceTests()
    {
        _mockLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .CreateLogger();
    }

    [Fact]
    public void NavigationService_NavigateTo_SetsCurrentView()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);
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
        var service = new NavigationService(_mockLogger);
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
        var service = new NavigationService(_mockLogger);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.NavigateTo<TestViewModel>(null!));
    }

    [Fact]
    public void NavigationService_NavigateBack_RestoresPreviousView()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);
        var view1 = new TestViewModel { TestId = 1 };
        var view2 = new TestViewModel { TestId = 2 };

        // Act
        service.NavigateTo(view1);
        service.NavigateTo(view2);
        service.NavigateBack();

        // Assert
        Assert.Same(view1, service.CurrentView);
    }

    [Fact]
    public void NavigationService_NavigateBack_UpdatesCanNavigateBack()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);
        var view1 = new TestViewModel();
        var view2 = new TestViewModel();
        var view3 = new TestViewModel();

        // Act & Assert
        service.NavigateTo(view1);
        Assert.False(service.CanNavigateBack);

        service.NavigateTo(view2);
        Assert.True(service.CanNavigateBack);

        service.NavigateTo(view3);
        Assert.True(service.CanNavigateBack);

        service.NavigateBack();
        Assert.True(service.CanNavigateBack);

        service.NavigateBack();
        Assert.False(service.CanNavigateBack);
    }

    [Fact]
    public void NavigationService_NavigateBack_DoesNothingWhenStackEmpty()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);
        var view1 = new TestViewModel();
        service.NavigateTo(view1);

        // Act - try to navigate back when there's nothing on the stack
        service.NavigateBack();

        // Assert - should still be on view1
        Assert.Same(view1, service.CurrentView);
    }

    [Fact]
    public void NavigationService_RegisterViewModelFactory_AllowsGenericNavigation()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);
        var createdViewModel = new TestViewModel { TestId = 42 };
        service.RegisterViewModelFactory(() => createdViewModel);

        // Act
        service.NavigateTo<TestViewModel>();

        // Assert
        Assert.Same(createdViewModel, service.CurrentView);
    }

    [Fact]
    public void NavigationService_NavigateTo_WithoutFactory_ThrowsInvalidOperation()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.NavigateTo<TestViewModel>());
    }

    [Fact]
    public void NavigationService_RegisterViewModelFactory_ThrowsOnNullFactory()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            service.RegisterViewModelFactory<TestViewModel>(null!));
    }

    [Fact]
    public void NavigationService_MultipleNavigations_MaintainsCorrectStack()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);
        var views = Enumerable.Range(1, 5).Select(i => new TestViewModel { TestId = i }).ToList();

        // Act - navigate through all views
        foreach (var view in views)
        {
            service.NavigateTo(view);
        }

        // Assert - current view is the last one
        Assert.Equal(5, ((TestViewModel)service.CurrentView!).TestId);

        // Navigate back through all
        for (int i = 4; i >= 1; i--)
        {
            service.NavigateBack();
            Assert.Equal(i, ((TestViewModel)service.CurrentView!).TestId);
        }

        Assert.False(service.CanNavigateBack);
    }

    [Fact]
    public void NavigationService_PropertyChangedNotifications_AreRaised()
    {
        // Arrange
        var service = new NavigationService(_mockLogger);
        var changedProperties = new List<string>();
        service.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        var view1 = new TestViewModel();
        var view2 = new TestViewModel();

        // Act
        service.NavigateTo(view1);
        service.NavigateTo(view2);
        service.NavigateBack();

        // Assert
        Assert.Contains("CurrentView", changedProperties);
        Assert.Contains("CanNavigateBack", changedProperties);
    }

    private class TestViewModel : ViewModelBase
    {
        public int TestId { get; set; }
    }
}
