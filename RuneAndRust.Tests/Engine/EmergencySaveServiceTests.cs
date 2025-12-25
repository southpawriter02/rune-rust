using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;

// Alias to resolve ambiguous Character reference
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for EmergencySaveService (v0.3.16b).
/// Validates emergency save, load, and cleanup operations.
/// </summary>
public class EmergencySaveServiceTests : IDisposable
{
    private readonly EmergencySaveService _sut;
    private readonly string _testPath = Path.Combine("data", "saves", "emergency.json");
    private readonly string _testDir = Path.Combine("data", "saves");

    public EmergencySaveServiceTests()
    {
        _sut = new EmergencySaveService();

        // Clean up before tests
        CleanupTestFiles();
    }

    public void Dispose()
    {
        // Clean up after tests
        CleanupTestFiles();
    }

    private void CleanupTestFiles()
    {
        if (File.Exists(_testPath))
        {
            try
            {
                File.Delete(_testPath);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }

    #region TryEmergencySave Tests

    [Fact]
    public void TryEmergencySave_WritesFile_WhenStateValid()
    {
        // Arrange
        var gameState = CreateValidGameState();

        // Act
        _sut.TryEmergencySave(gameState);

        // Assert
        File.Exists(_testPath).Should().BeTrue("emergency save file should be created");
    }

    [Fact]
    public void TryEmergencySave_ReturnsTrue_OnSuccess()
    {
        // Arrange
        var gameState = CreateValidGameState();

        // Act
        var result = _sut.TryEmergencySave(gameState);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TryEmergencySave_ReturnsFalse_WhenStateNull()
    {
        // Act
        var result = _sut.TryEmergencySave(null!);

        // Assert
        result.Should().BeFalse();
        File.Exists(_testPath).Should().BeFalse("no file should be created for null state");
    }

    [Fact]
    public void TryEmergencySave_ReturnsFalse_WhenCharacterNull()
    {
        // Arrange
        var gameState = new GameState
        {
            Phase = GamePhase.Exploration,
            IsSessionActive = true,
            CurrentCharacter = null
        };

        // Act
        var result = _sut.TryEmergencySave(gameState);

        // Assert
        result.Should().BeFalse();
        File.Exists(_testPath).Should().BeFalse("no file should be created without character");
    }

    [Fact]
    public void TryEmergencySave_ReturnsFalse_WhenSessionInactive()
    {
        // Arrange
        var gameState = new GameState
        {
            Phase = GamePhase.Exploration,
            IsSessionActive = false,
            CurrentCharacter = CreateTestCharacter()
        };

        // Act
        var result = _sut.TryEmergencySave(gameState);

        // Assert
        result.Should().BeFalse();
        File.Exists(_testPath).Should().BeFalse("no file should be created for inactive session");
    }

    [Fact]
    public void TryEmergencySave_CreatesDirectory_IfMissing()
    {
        // Arrange
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }

        var gameState = CreateValidGameState();

        // Act
        _sut.TryEmergencySave(gameState);

        // Assert
        Directory.Exists(_testDir).Should().BeTrue("saves directory should be created");
        File.Exists(_testPath).Should().BeTrue("emergency save file should be created");
    }

    [Fact]
    public void TryEmergencySave_OverwritesExisting()
    {
        // Arrange
        var firstState = CreateValidGameState();
        firstState.TurnCount = 10;
        _sut.TryEmergencySave(firstState);

        var secondState = CreateValidGameState();
        secondState.TurnCount = 50;

        // Act
        var result = _sut.TryEmergencySave(secondState);

        // Assert
        result.Should().BeTrue();
        var content = File.ReadAllText(_testPath);
        content.Should().Contain("\"turnCount\": 50");
        content.Should().NotContain("\"turnCount\": 10");
    }

    [Fact]
    public void TryEmergencySave_WritesValidJson()
    {
        // Arrange
        var gameState = CreateValidGameState();
        gameState.TurnCount = 42;

        // Act
        _sut.TryEmergencySave(gameState);

        // Assert
        var content = File.ReadAllText(_testPath);
        content.Should().Contain("\"phase\":");
        content.Should().Contain("\"currentCharacter\":");
        content.Should().Contain("\"turnCount\": 42");
        content.Should().Contain("\"isSessionActive\": true");
    }

    #endregion

    #region EmergencySaveExists Tests

    [Fact]
    public void EmergencySaveExists_ReturnsTrue_WhenFileExists()
    {
        // Arrange
        var gameState = CreateValidGameState();
        _sut.TryEmergencySave(gameState);

        // Act
        var result = _sut.EmergencySaveExists();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EmergencySaveExists_ReturnsFalse_WhenNoFile()
    {
        // Arrange - ensure file doesn't exist
        CleanupTestFiles();

        // Act
        var result = _sut.EmergencySaveExists();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LoadEmergencySave Tests

    [Fact]
    public void LoadEmergencySave_ReturnsState_WhenFileValid()
    {
        // Arrange
        var originalState = CreateValidGameState();
        originalState.TurnCount = 99;
        _sut.TryEmergencySave(originalState);

        // Act
        var loadedState = _sut.LoadEmergencySave();

        // Assert
        loadedState.Should().NotBeNull();
        loadedState!.TurnCount.Should().Be(99);
        loadedState.IsSessionActive.Should().BeTrue();
        loadedState.CurrentCharacter.Should().NotBeNull();
        loadedState.CurrentCharacter!.Name.Should().Be("Test Hero");
    }

    [Fact]
    public void LoadEmergencySave_ReturnsNull_WhenFileCorrupt()
    {
        // Arrange
        Directory.CreateDirectory(_testDir);
        var corruptJson = "{ this is not valid json }}}}";
        File.WriteAllText(_testPath, corruptJson);

        // Act
        var result = _sut.LoadEmergencySave();

        // Assert
        result.Should().BeNull("should return null for corrupt JSON");
    }

    [Fact]
    public void LoadEmergencySave_ReturnsNull_WhenFileMissing()
    {
        // Arrange - ensure file doesn't exist
        CleanupTestFiles();

        // Act
        var result = _sut.LoadEmergencySave();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void LoadEmergencySave_PreservesCharacterData()
    {
        // Arrange
        var originalState = CreateValidGameState();
        originalState.CurrentCharacter!.CurrentHP = 75;
        originalState.CurrentCharacter.CurrentStamina = 50;
        _sut.TryEmergencySave(originalState);

        // Act
        var loadedState = _sut.LoadEmergencySave();

        // Assert
        loadedState.Should().NotBeNull();
        loadedState!.CurrentCharacter.Should().NotBeNull();
        loadedState.CurrentCharacter!.CurrentHP.Should().Be(75);
        loadedState.CurrentCharacter.CurrentStamina.Should().Be(50);
    }

    #endregion

    #region ClearEmergencySave Tests

    [Fact]
    public void ClearEmergencySave_DeletesFile()
    {
        // Arrange
        var gameState = CreateValidGameState();
        _sut.TryEmergencySave(gameState);
        File.Exists(_testPath).Should().BeTrue("precondition: file should exist");

        // Act
        _sut.ClearEmergencySave();

        // Assert
        File.Exists(_testPath).Should().BeFalse("emergency save should be deleted");
    }

    [Fact]
    public void ClearEmergencySave_DoesNotThrow_WhenFileMissing()
    {
        // Arrange - ensure file doesn't exist
        CleanupTestFiles();

        // Act & Assert
        var act = () => _sut.ClearEmergencySave();
        act.Should().NotThrow("should handle missing file gracefully");
    }

    #endregion

    #region Helper Methods

    private static GameState CreateValidGameState()
    {
        return new GameState
        {
            Phase = GamePhase.Exploration,
            IsSessionActive = true,
            CurrentCharacter = CreateTestCharacter(),
            TurnCount = 1,
            CurrentRoomId = Guid.NewGuid()
        };
    }

    private static Character CreateTestCharacter()
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "Test Hero",
            CurrentHP = 100,
            MaxHP = 100,
            CurrentStamina = 60,
            MaxStamina = 60,
            Level = 1
        };
    }

    #endregion
}
