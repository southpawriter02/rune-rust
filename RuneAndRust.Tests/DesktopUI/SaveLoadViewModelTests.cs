using Moq;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// v0.43.21: Comprehensive tests for SaveLoadViewModel.
/// Tests save file management, loading, and quick save/load functionality.
/// </summary>
public class SaveLoadViewModelTests
{
    private readonly Mock<ISaveGameService> _mockSaveService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<INavigationService> _mockNavigationService;

    public SaveLoadViewModelTests()
    {
        _mockSaveService = new Mock<ISaveGameService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNavigationService = new Mock<INavigationService>();

        // Setup default mock returns
        _mockSaveService.Setup(s => s.GetAllSaveFiles()).Returns(new List<SaveFileMetadata>());
        _mockDialogService.Setup(d => d.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockDialogService.Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public void Constructor_InitializesEmptySaveFiles()
    {
        // Act
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Assert
        Assert.NotNull(vm.SaveFiles);
        Assert.Empty(vm.SaveFiles);
    }

    [Fact]
    public void Constructor_LoadsSaveFilesFromService()
    {
        // Arrange
        var saves = new List<SaveFileMetadata>
        {
            CreateTestSaveMetadata("save1", "Test Save 1"),
            CreateTestSaveMetadata("save2", "Test Save 2")
        };
        _mockSaveService.Setup(s => s.GetAllSaveFiles()).Returns(saves);

        // Act
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Assert
        Assert.Equal(2, vm.SaveFiles.Count);
        Assert.Equal(2, vm.SaveCount);
    }

    [Fact]
    public void SelectedSave_WhenSet_UpdatesRelatedProperties()
    {
        // Arrange
        var save = CreateTestSaveMetadata("save1", "Test Save");
        _mockSaveService.Setup(s => s.GetAllSaveFiles()).Returns(new List<SaveFileMetadata> { save });

        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);
        var changedProperties = new List<string>();
        vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        // Act
        vm.SelectedSave = vm.SaveFiles.First();

        // Assert
        Assert.NotNull(vm.SelectedSave);
        Assert.True(vm.HasSelectedSave);
        Assert.True(vm.CanLoad);
        Assert.True(vm.CanDelete);
        Assert.Contains("CanLoad", changedProperties);
        Assert.Contains("CanDelete", changedProperties);
        Assert.Contains("HasSelectedSave", changedProperties);
    }

    [Fact]
    public void NewSaveName_WhenSet_UpdatesCanSave()
    {
        // Arrange
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Assert - initially empty
        Assert.False(vm.CanSave);

        // Act
        vm.NewSaveName = "My New Save";

        // Assert
        Assert.True(vm.CanSave);
    }

    [Fact]
    public void CanSave_ReturnsFalseWhenNameIsWhitespace()
    {
        // Arrange
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Act
        vm.NewSaveName = "   ";

        // Assert
        Assert.False(vm.CanSave);
    }

    [Fact]
    public void CanLoad_ReturnsFalseWhenNoSaveSelected()
    {
        // Arrange
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Assert
        Assert.False(vm.CanLoad);
    }

    [Fact]
    public void CanDelete_ReturnsFalseWhenNoSaveSelected()
    {
        // Arrange
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Assert
        Assert.False(vm.CanDelete);
    }

    [Fact]
    public async Task LoadGameCommand_LoadsSelectedSave()
    {
        // Arrange
        var save = CreateTestSaveMetadata("save1", "Test Save");
        _mockSaveService.Setup(s => s.GetAllSaveFiles()).Returns(new List<SaveFileMetadata> { save });
        _mockSaveService.Setup(s => s.LoadGameAsync("save1")).ReturnsAsync(true);

        var vm = new SaveLoadViewModel(
            _mockSaveService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object);

        vm.SelectedSave = vm.SaveFiles.First();

        // Act
        await Task.Run(() => vm.LoadGameCommand.Execute(null));
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        _mockSaveService.Verify(s => s.LoadGameAsync("save1"), Times.Once);
    }

    [Fact]
    public async Task DeleteSaveCommand_DeletesSelectedSave()
    {
        // Arrange
        var save = CreateTestSaveMetadata("save1", "Test Save");
        _mockSaveService.Setup(s => s.GetAllSaveFiles()).Returns(new List<SaveFileMetadata> { save });

        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);
        vm.SelectedSave = vm.SaveFiles.First();

        // Act
        await Task.Run(() => vm.DeleteSaveCommand.Execute(null));
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        _mockSaveService.Verify(s => s.DeleteSave("save1"), Times.Once);
    }

    [Fact]
    public void RefreshSavesCommand_ReloadsFromService()
    {
        // Arrange
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Act
        vm.RefreshSavesCommand.Execute(null);

        // Assert
        _mockSaveService.Verify(s => s.GetAllSaveFiles(), Times.AtLeast(2));
    }

    [Fact]
    public async Task QuickSaveAsync_PerformsQuickSave()
    {
        // Arrange
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Act
        await vm.QuickSaveAsync();

        // Assert
        _mockSaveService.Verify(s => s.QuickSaveAsync(), Times.Once);
        Assert.Contains("Quick save", vm.StatusMessage);
    }

    [Fact]
    public async Task QuickLoadAsync_PerformsQuickLoad()
    {
        // Arrange
        _mockSaveService.Setup(s => s.HasQuickSave).Returns(true);
        _mockSaveService.Setup(s => s.QuickLoadAsync()).ReturnsAsync(true);

        var vm = new SaveLoadViewModel(
            _mockSaveService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object);

        // Act
        await vm.QuickLoadAsync();

        // Assert
        _mockSaveService.Verify(s => s.QuickLoadAsync(), Times.Once);
    }

    [Fact]
    public async Task QuickLoadAsync_ShowsMessage_WhenNoQuickSave()
    {
        // Arrange
        _mockSaveService.Setup(s => s.HasQuickSave).Returns(false);

        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Act
        await vm.QuickLoadAsync();

        // Assert
        _mockSaveService.Verify(s => s.QuickLoadAsync(), Times.Never);
        Assert.Contains("No quick save", vm.StatusMessage);
    }

    [Fact]
    public void SelectSave_SetsSelectedSave()
    {
        // Arrange
        var save = CreateTestSaveMetadata("save1", "Test Save");
        _mockSaveService.Setup(s => s.GetAllSaveFiles()).Returns(new List<SaveFileMetadata> { save });

        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);
        var saveVm = vm.SaveFiles.First();

        // Act
        vm.SelectSave(saveVm);

        // Assert
        Assert.Same(saveVm, vm.SelectedSave);
        Assert.True(saveVm.IsSelected);
    }

    [Fact]
    public void SelectSave_DeselectsPreviousSave()
    {
        // Arrange
        var saves = new List<SaveFileMetadata>
        {
            CreateTestSaveMetadata("save1", "Test Save 1"),
            CreateTestSaveMetadata("save2", "Test Save 2")
        };
        _mockSaveService.Setup(s => s.GetAllSaveFiles()).Returns(saves);

        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);
        var save1 = vm.SaveFiles[0];
        var save2 = vm.SaveFiles[1];

        // Act
        vm.SelectSave(save1);
        vm.SelectSave(save2);

        // Assert
        Assert.False(save1.IsSelected);
        Assert.True(save2.IsSelected);
    }

    [Fact]
    public void BackCommand_NavigatesBack()
    {
        // Arrange
        var vm = new SaveLoadViewModel(
            _mockSaveService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object);

        // Act
        vm.BackCommand.Execute(null);

        // Assert
        _mockNavigationService.Verify(n => n.NavigateBack(), Times.Once);
    }

    [Fact]
    public void StatusMessage_UpdatesDuringOperations()
    {
        // Arrange
        var vm = new SaveLoadViewModel(_mockSaveService.Object, _mockDialogService.Object);

        // Assert - shows load message after construction
        Assert.Contains("Loaded", vm.StatusMessage);
    }

    private static SaveFileMetadata CreateTestSaveMetadata(string fileName, string saveName)
    {
        return new SaveFileMetadata
        {
            FileName = fileName,
            SaveName = saveName,
            CharacterName = "Test Hero",
            CharacterClass = "Warrior",
            Specialization = "None",
            Legend = 1,
            CurrentFloor = 1,
            SaveDate = DateTime.Now,
            PlayTime = TimeSpan.FromHours(1),
            IsAutoSave = false,
            IsQuickSave = false
        };
    }
}

/// <summary>
/// Tests for SaveFileViewModel.
/// </summary>
public class SaveFileViewModelTests
{
    [Fact]
    public void Constructor_InitializesFromMetadata()
    {
        // Arrange
        var metadata = new SaveFileMetadata
        {
            FileName = "test_save",
            SaveName = "My Save",
            CharacterName = "Hero",
            CharacterClass = "Warrior",
            Specialization = "Berserker",
            Legend = 5,
            CurrentFloor = 3,
            SaveDate = new DateTime(2024, 1, 15, 14, 30, 0),
            PlayTime = TimeSpan.FromHours(4) + TimeSpan.FromMinutes(30),
            IsAutoSave = false,
            IsQuickSave = false,
            BossDefeated = true
        };

        // Act
        var vm = new SaveFileViewModel(metadata);

        // Assert
        Assert.Equal("test_save", vm.FileName);
        Assert.Equal("My Save", vm.SaveName);
        Assert.Equal("Hero", vm.CharacterName);
        Assert.Equal("Warrior", vm.CharacterClass);
        Assert.Equal("Berserker", vm.Specialization);
        Assert.Equal(5, vm.Legend);
        Assert.Equal(3, vm.CurrentFloor);
        Assert.True(vm.BossDefeated);
    }

    [Fact]
    public void DisplayDate_FormatsCorrectly()
    {
        // Arrange
        var metadata = new SaveFileMetadata
        {
            SaveDate = new DateTime(2024, 1, 15, 14, 30, 45)
        };

        // Act
        var vm = new SaveFileViewModel(metadata);

        // Assert
        Assert.Equal("2024-01-15 14:30:45", vm.DisplayDate);
    }

    [Fact]
    public void DisplayPlayTime_FormatsHoursAndMinutes()
    {
        // Arrange
        var metadata = new SaveFileMetadata
        {
            PlayTime = TimeSpan.FromHours(4) + TimeSpan.FromMinutes(30)
        };

        // Act
        var vm = new SaveFileViewModel(metadata);

        // Assert
        Assert.Equal("4h 30m", vm.DisplayPlayTime);
    }

    [Fact]
    public void DisplayPlayTime_FormatsMinutesOnly()
    {
        // Arrange
        var metadata = new SaveFileMetadata
        {
            PlayTime = TimeSpan.FromMinutes(45)
        };

        // Act
        var vm = new SaveFileViewModel(metadata);

        // Assert
        Assert.Equal("45m", vm.DisplayPlayTime);
    }

    [Fact]
    public void DisplayProgress_IncludesBossDefeated()
    {
        // Arrange
        var metadata = new SaveFileMetadata
        {
            Legend = 5,
            CurrentFloor = 3,
            BossDefeated = true
        };

        // Act
        var vm = new SaveFileViewModel(metadata);

        // Assert
        Assert.Contains("Boss Defeated", vm.DisplayProgress);
    }

    [Fact]
    public void DisplayProgress_ExcludesBossDefeated_WhenFalse()
    {
        // Arrange
        var metadata = new SaveFileMetadata
        {
            Legend = 5,
            CurrentFloor = 3,
            BossDefeated = false
        };

        // Act
        var vm = new SaveFileViewModel(metadata);

        // Assert
        Assert.DoesNotContain("Boss Defeated", vm.DisplayProgress);
    }

    [Fact]
    public void CharacterSummary_IncludesSpecialization()
    {
        // Arrange
        var metadata = new SaveFileMetadata
        {
            CharacterName = "Hero",
            CharacterClass = "Warrior",
            Specialization = "Berserker"
        };

        // Act
        var vm = new SaveFileViewModel(metadata);

        // Assert
        Assert.Equal("Hero - Warrior (Berserker)", vm.CharacterSummary);
    }

    [Fact]
    public void CharacterSummary_ExcludesSpecialization_WhenNone()
    {
        // Arrange
        var metadata = new SaveFileMetadata
        {
            CharacterName = "Hero",
            CharacterClass = "Warrior",
            Specialization = "None"
        };

        // Act
        var vm = new SaveFileViewModel(metadata);

        // Assert
        Assert.Equal("Hero - Warrior", vm.CharacterSummary);
    }

    [Fact]
    public void IsSelected_RaisesPropertyChanged()
    {
        // Arrange
        var vm = new SaveFileViewModel(new SaveFileMetadata());
        var changedProperties = new List<string>();
        vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        // Act
        vm.IsSelected = true;

        // Assert
        Assert.Contains("IsSelected", changedProperties);
        Assert.True(vm.IsSelected);
    }
}
