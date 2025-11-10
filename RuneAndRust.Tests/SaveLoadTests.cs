using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Persistence;
using System.IO;

namespace RuneAndRust.Tests;

[TestFixture]
public class SaveLoadTests
{
    private SaveRepository _repository;
    private string _testDbPath;

    [SetUp]
    public void Setup()
    {
        // Create a unique test database for each test
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_runeandrust_{Guid.NewGuid()}.db");
        _repository = new SaveRepository(Path.GetTempPath());
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [Test]
    public void SaveGame_NewCharacter_SavesCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        var worldState = new WorldState
        {
            CurrentRoomId = 1,
            ClearedRoomIds = new List<int> { 0 },
            PuzzleSolved = false,
            BossDefeated = false
        };

        // Act
        _repository.SaveGame(player, worldState);

        // Assert
        Assert.That(_repository.SaveExists(player.Name), Is.True);
    }

    [Test]
    public void LoadGame_ExistingSave_LoadsCorrectly()
    {
        // Arrange
        var originalPlayer = CreateTestPlayer();
        var originalWorldState = new WorldState
        {
            CurrentRoomId = 2,
            ClearedRoomIds = new List<int> { 0, 1 },
            PuzzleSolved = true,
            BossDefeated = false
        };

        _repository.SaveGame(originalPlayer, originalWorldState);

        // Act
        var (loadedPlayer, loadedWorldState) = _repository.LoadGame(originalPlayer.Name);

        // Assert
        Assert.That(loadedPlayer, Is.Not.Null);
        Assert.That(loadedWorldState, Is.Not.Null);
        Assert.That(loadedPlayer!.Name, Is.EqualTo(originalPlayer.Name));
        Assert.That(loadedPlayer.CurrentMilestone, Is.EqualTo(originalPlayer.CurrentMilestone));
        Assert.That(loadedPlayer.CurrentLegend, Is.EqualTo(originalPlayer.CurrentLegend));
        Assert.That(loadedPlayer.ProgressionPoints, Is.EqualTo(originalPlayer.ProgressionPoints));
    }

    [Test]
    public void LoadGame_NonExistentSave_ReturnsNull()
    {
        // Act
        var (player, worldState) = _repository.LoadGame("NonExistent");

        // Assert
        Assert.That(player, Is.Null);
        Assert.That(worldState, Is.Null);
    }

    [Test]
    public void SaveGame_ProgressionData_PersistsCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.CurrentMilestone = 2;
        player.CurrentLegend = 175;
        player.ProgressionPoints = 5;
        player.LegendToNextMilestone = 200;

        var worldState = new WorldState();
        _repository.SaveGame(player, worldState);

        // Act
        var (loadedPlayer, _) = _repository.LoadGame(player.Name);

        // Assert
        Assert.That(loadedPlayer!.CurrentMilestone, Is.EqualTo(2));
        Assert.That(loadedPlayer.CurrentLegend, Is.EqualTo(175));
        Assert.That(loadedPlayer.ProgressionPoints, Is.EqualTo(5));
        Assert.That(loadedPlayer.LegendToNextMilestone, Is.EqualTo(200));
    }

    [Test]
    public void SaveGame_AttributeData_PersistsCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.Attributes.Might = 5;
        player.Attributes.Finesse = 4;
        player.Attributes.Wits = 3;
        player.Attributes.Will = 2;
        player.Attributes.Sturdiness = 6;

        var worldState = new WorldState();
        _repository.SaveGame(player, worldState);

        // Act
        var (loadedPlayer, _) = _repository.LoadGame(player.Name);

        // Assert
        Assert.That(loadedPlayer!.Attributes.Might, Is.EqualTo(5));
        Assert.That(loadedPlayer.Attributes.Finesse, Is.EqualTo(4));
        Assert.That(loadedPlayer.Attributes.Wits, Is.EqualTo(3));
        Assert.That(loadedPlayer.Attributes.Will, Is.EqualTo(2));
        Assert.That(loadedPlayer.Attributes.Sturdiness, Is.EqualTo(6));
    }

    [Test]
    public void SaveGame_ResourceData_PersistsCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.HP = 45;
        player.MaxHP = 70;
        player.Stamina = 25;
        player.MaxStamina = 40;

        var worldState = new WorldState();
        _repository.SaveGame(player, worldState);

        // Act
        var (loadedPlayer, _) = _repository.LoadGame(player.Name);

        // Assert
        Assert.That(loadedPlayer!.HP, Is.EqualTo(45));
        Assert.That(loadedPlayer.MaxHP, Is.EqualTo(70));
        Assert.That(loadedPlayer.Stamina, Is.EqualTo(25));
        Assert.That(loadedPlayer.MaxStamina, Is.EqualTo(40));
        Assert.That(loadedPlayer.AP, Is.EqualTo(10)); // Always restored to 10
    }

    [Test]
    public void SaveGame_WorldStateData_PersistsCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        var worldState = new WorldState
        {
            CurrentRoomId = 4,
            ClearedRoomIds = new List<int> { 0, 1, 2, 3 },
            PuzzleSolved = true,
            BossDefeated = false
        };

        _repository.SaveGame(player, worldState);

        // Act
        var (_, loadedWorldState) = _repository.LoadGame(player.Name);

        // Assert
        Assert.That(loadedWorldState!.CurrentRoomId, Is.EqualTo(4));
        Assert.That(loadedWorldState.ClearedRoomIds, Has.Count.EqualTo(4));
        Assert.That(loadedWorldState.ClearedRoomIds, Contains.Item(3));
        Assert.That(loadedWorldState.PuzzleSolved, Is.True);
        Assert.That(loadedWorldState.BossDefeated, Is.False);
    }

    [Test]
    public void SaveGame_Overwrite_UpdatesExistingRecord()
    {
        // Arrange
        var player = CreateTestPlayer();
        var worldState = new WorldState();

        _repository.SaveGame(player, worldState);
        player.CurrentMilestone = 3;
        player.CurrentLegend = 500;

        // Act
        _repository.SaveGame(player, worldState); // Overwrite
        var (loadedPlayer, _) = _repository.LoadGame(player.Name);

        // Assert
        Assert.That(loadedPlayer!.CurrentMilestone, Is.EqualTo(3));
        Assert.That(loadedPlayer.CurrentLegend, Is.EqualTo(500));
    }

    [Test]
    public void ListSaves_MultipleSaves_ReturnsAllSaves()
    {
        // Arrange
        var player1 = CreateTestPlayer("Hero1");
        var player2 = CreateTestPlayer("Hero2");
        var worldState = new WorldState();

        _repository.SaveGame(player1, worldState);
        _repository.SaveGame(player2, worldState);

        // Act
        var saves = _repository.ListSaves();

        // Assert
        Assert.That(saves, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(saves.Any(s => s.CharacterName == "Hero1"), Is.True);
        Assert.That(saves.Any(s => s.CharacterName == "Hero2"), Is.True);
    }

    [Test]
    public void ListSaves_SaveInfo_ContainsCorrectData()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.CurrentMilestone = 2;
        var worldState = new WorldState { BossDefeated = true };

        _repository.SaveGame(player, worldState);

        // Act
        var saves = _repository.ListSaves();
        var saveInfo = saves.First(s => s.CharacterName == player.Name);

        // Assert
        Assert.That(saveInfo.CharacterName, Is.EqualTo(player.Name));
        Assert.That(saveInfo.Class, Is.EqualTo(player.Class));
        Assert.That(saveInfo.CurrentMilestone, Is.EqualTo(2));
        Assert.That(saveInfo.BossDefeated, Is.True);
    }

    [Test]
    public void DeleteSave_ExistingSave_RemovesFromDatabase()
    {
        // Arrange
        var player = CreateTestPlayer();
        var worldState = new WorldState();
        _repository.SaveGame(player, worldState);

        // Act
        _repository.DeleteSave(player.Name);

        // Assert
        Assert.That(_repository.SaveExists(player.Name), Is.False);
    }

    [Test]
    public void SaveExists_AfterSave_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();
        var worldState = new WorldState();

        // Act
        _repository.SaveGame(player, worldState);

        // Assert
        Assert.That(_repository.SaveExists(player.Name), Is.True);
    }

    [Test]
    public void SaveExists_BeforeSave_ReturnsFalse()
    {
        // Assert
        Assert.That(_repository.SaveExists("NonExistent"), Is.False);
    }

    private PlayerCharacter CreateTestPlayer(string name = "TestHero")
    {
        return new PlayerCharacter
        {
            Name = name,
            Class = CharacterClass.Warrior,
            CurrentMilestone = 1,
            CurrentLegend = 125,
            LegendToNextMilestone = 150,
            ProgressionPoints = 3,
            Attributes = new Attributes(4, 3, 2, 2, 3),
            HP = 60,
            MaxHP = 60,
            Stamina = 35,
            MaxStamina = 35,
            AP = 10
        };
    }
}
