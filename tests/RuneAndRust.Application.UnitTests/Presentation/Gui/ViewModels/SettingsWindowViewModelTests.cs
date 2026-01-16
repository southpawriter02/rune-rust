namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.ViewModels.Settings;

[TestFixture]
public class SettingsWindowViewModelTests
{
    // ============================================================================
    // Audio Tab Tests
    // ============================================================================

    [Test]
    public void AudioTab_MasterVolume_TracksChanges()
    {
        // Arrange
        var changed = false;
        var settings = new AudioSettings { MasterVolume = 50 };
        var vm = new AudioSettingsTabViewModel(settings, () => changed = true);

        // Act
        vm.MasterVolume = 75;

        // Assert
        changed.Should().BeTrue();
        vm.MasterVolume.Should().Be(75);
    }

    [Test]
    public void AudioTab_ToSettings_ReturnsCorrectValues()
    {
        // Arrange
        var vm = new AudioSettingsTabViewModel
        {
            MasterVolume = 80,
            MusicVolume = 60,
            SfxVolume = 90,
            EnableSoundEffects = false,
            MuteAll = true
        };

        // Act
        var result = vm.ToSettings();

        // Assert
        result.MasterVolume.Should().Be(80);
        result.MusicVolume.Should().Be(60);
        result.SfxVolume.Should().Be(90);
        result.EnableSoundEffects.Should().BeFalse();
        result.MuteAll.Should().BeTrue();
    }

    // ============================================================================
    // Display Tab Tests
    // ============================================================================

    [Test]
    public void DisplayTab_ThemeChange_TracksChanges()
    {
        // Arrange
        var changed = false;
        var settings = new DisplaySettings { Theme = "Dark Fantasy" };
        var vm = new DisplaySettingsTabViewModel(settings, () => changed = true);

        // Act
        vm.SelectedTheme = "High Fantasy";

        // Assert
        changed.Should().BeTrue();
        vm.SelectedTheme.Should().Be("High Fantasy");
    }

    [Test]
    public void DisplayTab_ToSettings_ReturnsCorrectValues()
    {
        // Arrange
        var vm = new DisplaySettingsTabViewModel
        {
            SelectedTheme = "Classic",
            FontSizePercent = 120,
            EnableCombatAnimations = false,
            HighContrastMode = true
        };

        // Act
        var result = vm.ToSettings();

        // Assert
        result.Theme.Should().Be("Classic");
        result.FontSizePercent.Should().Be(120);
        result.EnableCombatAnimations.Should().BeFalse();
        result.HighContrastMode.Should().BeTrue();
    }

    // ============================================================================
    // Gameplay Tab Tests
    // ============================================================================

    [Test]
    public void GameplayTab_DifficultyDescription_UpdatesOnChange()
    {
        // Arrange
        var vm = new GameplaySettingsTabViewModel { Difficulty = "Normal" };

        // Act & Assert
        vm.DifficultyDescription.Should().Be("Standard experience");

        vm.Difficulty = "Hard";
        vm.DifficultyDescription.Should().Contain("-20% damage dealt");
    }

    [Test]
    public void GameplayTab_ToSettings_ReturnsCorrectValues()
    {
        // Arrange
        var vm = new GameplaySettingsTabViewModel
        {
            Difficulty = "Easy",
            AutoSaveFrequency = "Every 1 minute",
            ShowCombatTooltips = false,
            ConfirmDangerousActions = true
        };

        // Act
        var result = vm.ToSettings();

        // Assert
        result.Difficulty.Should().Be("Easy");
        result.AutoSaveFrequency.Should().Be("Every 1 minute");
        result.ShowCombatTooltips.Should().BeFalse();
        result.ConfirmDangerousActions.Should().BeTrue();
    }

    // ============================================================================
    // Apply/Cancel/OK Tests
    // ============================================================================

    [Test]
    public void SettingsWindow_Apply_TriggersOnSettingsSaved()
    {
        // Arrange
        var settings = new GameSettings();
        GameSettings? savedSettings = null;
        var vm = new SettingsWindowViewModel(settings);
        vm.OnSettingsSaved += s => savedSettings = s;

        vm.AudioTab.MasterVolume = 99; // This sets HasChanges

        // Act
        vm.ApplyCommand.Execute(null);

        // Assert
        savedSettings.Should().NotBeNull();
        savedSettings!.Audio.MasterVolume.Should().Be(99);
        vm.HasChanges.Should().BeFalse();
    }

    [Test]
    public void SettingsWindow_Cancel_ClosesWindow()
    {
        // Arrange
        var closed = false;
        var settings = new GameSettings();
        var vm = new SettingsWindowViewModel(settings, () => closed = true);

        // Act
        vm.CancelCommand.Execute(null);

        // Assert
        closed.Should().BeTrue();
    }

    // ============================================================================
    // Persistence Tests
    // ============================================================================

    [Test]
    public void GameSettings_Clone_CreatesDeepCopy()
    {
        // Arrange
        var original = new GameSettings
        {
            Audio = new AudioSettings { MasterVolume = 50 },
            Display = new DisplaySettings { Theme = "Classic" }
        };

        // Act
        var clone = original.Clone();
        clone.Audio.MasterVolume = 100;
        clone.Display.Theme = "Minimal";

        // Assert
        original.Audio.MasterVolume.Should().Be(50);
        original.Display.Theme.Should().Be("Classic");
    }

    [Test]
    public void ControlsTab_ResetToDefaults_RestoresBindings()
    {
        // Arrange
        var changed = false;
        var settings = new ControlsSettings { GlobalBindings = new() { ["Custom"] = "X" } };
        var vm = new ControlsSettingsTabViewModel(settings, () => changed = true);

        // Act
        vm.ResetToDefaultsCommand.Execute(null);

        // Assert
        changed.Should().BeTrue();
        vm.GlobalBindings.Should().Contain(b => b.ActionName == "Help / Shortcuts");
        vm.GlobalBindings.Should().NotContain(b => b.ActionName == "Custom");
    }
}
