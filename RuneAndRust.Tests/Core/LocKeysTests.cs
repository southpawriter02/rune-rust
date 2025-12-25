using System.Text.RegularExpressions;
using FluentAssertions;
using RuneAndRust.Core.Constants;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Unit tests for LocKeys static class (v0.3.15a - The Lexicon).
/// Validates key format, uniqueness, and AllKeys property correctness.
/// </summary>
public class LocKeysTests
{
    [Fact]
    public void AllKeys_ShouldNotBeEmpty()
    {
        // Act
        var keys = LocKeys.AllKeys;

        // Assert
        keys.Should().NotBeEmpty();
        keys.Count.Should().BeGreaterThan(50, "we expect at least 50+ localization keys");
    }

    [Fact]
    public void AllKeys_ShouldHaveValidFormat()
    {
        // Arrange - Valid format is "Category.Subcategory.Element" or deeper
        var validPattern = new Regex(@"^[A-Za-z]+(\.[A-Za-z]+)+$");

        // Act
        var keys = LocKeys.AllKeys;

        // Assert - All keys should match the dot-notation pattern
        foreach (var key in keys)
        {
            validPattern.IsMatch(key).Should().BeTrue(
                $"key '{key}' should match dot-notation format (e.g., 'UI.MainMenu.NewGame')");
        }
    }

    [Fact]
    public void AllKeys_ShouldBeUnique()
    {
        // Act
        var keys = LocKeys.AllKeys;

        // Assert
        keys.Should().OnlyHaveUniqueItems("duplicate localization keys would cause lookup conflicts");
    }

    [Fact]
    public void ConstantValues_ShouldMatchExpectedPattern()
    {
        // Arrange - Sample a few constants to verify naming convention
        var samples = new Dictionary<string, string>
        {
            { nameof(LocKeys.UI_MainMenu_NewGame), "UI.MainMenu.NewGame" },
            { nameof(LocKeys.UI_Options_Title), "UI.Options.Title" },
            { nameof(LocKeys.UI_Creation_Title), "UI.Creation.Title" },
            { nameof(LocKeys.Art_TitleScreen_Logo), "Art.TitleScreen.Logo" }
        };

        // Assert - Verify naming convention: underscores in constant name = dots in key value
        foreach (var (constantName, expectedValue) in samples)
        {
            var field = typeof(LocKeys).GetField(constantName);
            field.Should().NotBeNull($"constant {constantName} should exist");

            var actualValue = field!.GetRawConstantValue() as string;
            actualValue.Should().Be(expectedValue,
                $"constant {constantName} should have value '{expectedValue}'");
        }
    }

    [Fact]
    public void AllKeys_ShouldContainMainMenuKeys()
    {
        // Arrange
        var expectedMainMenuKeys = new[]
        {
            "UI.MainMenu.SelectOption",
            "UI.MainMenu.NewGame",
            "UI.MainMenu.LoadGame",
            "UI.MainMenu.Options",
            "UI.MainMenu.Quit",
            "UI.MainMenu.Version",
            "UI.MainMenu.NoSaves"
        };

        // Act
        var keys = LocKeys.AllKeys;

        // Assert
        foreach (var expectedKey in expectedMainMenuKeys)
        {
            keys.Should().Contain(expectedKey,
                $"AllKeys should include main menu key '{expectedKey}'");
        }
    }

    [Fact]
    public void AllKeys_ShouldContainOptionsKeys()
    {
        // Arrange - Sample of Options keys
        var expectedOptionsKeys = new[]
        {
            "UI.Options.Title",
            "UI.Options.Tabs.General",
            "UI.Options.Settings.AutosaveInterval",
            "UI.Options.Toggle.On",
            "UI.Options.Toggle.Off"
        };

        // Act
        var keys = LocKeys.AllKeys;

        // Assert
        foreach (var expectedKey in expectedOptionsKeys)
        {
            keys.Should().Contain(expectedKey,
                $"AllKeys should include options key '{expectedKey}'");
        }
    }

    [Fact]
    public void AllKeys_ShouldContainCreationKeys()
    {
        // Arrange - Sample of Creation keys
        var expectedCreationKeys = new[]
        {
            "UI.Creation.Title",
            "UI.Creation.Steps.Lineage",
            "UI.Creation.Steps.Archetype",
            "UI.Creation.Steps.Background",
            "UI.Creation.Success.Title"
        };

        // Act
        var keys = LocKeys.AllKeys;

        // Assert
        foreach (var expectedKey in expectedCreationKeys)
        {
            keys.Should().Contain(expectedKey,
                $"AllKeys should include creation key '{expectedKey}'");
        }
    }
}
