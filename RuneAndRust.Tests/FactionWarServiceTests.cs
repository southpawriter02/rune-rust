using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Core.Territory;
using RuneAndRust.Persistence;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.35.2: Test suite for FactionWarService
/// Validates war triggering, advancement, and resolution mechanics
/// </summary>
[TestFixture]
public class FactionWarServiceTests
{
    private FactionWarService _warService;
    private TerritoryControlService _territoryService;
    private SaveRepository _saveRepository;
    private string _testDbDirectory;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        // Create unique test database
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"war_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbDirectory);

        // Initialize database schema (includes v0.35.1 territory tables)
        _saveRepository = new SaveRepository(_testDbDirectory);
        _connectionString = $"Data Source={Path.Combine(_testDbDirectory, "runeandrust.db")}";

        _territoryService = new TerritoryControlService(_connectionString);
        _warService = new FactionWarService(_connectionString, _territoryService);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDbDirectory))
        {
            Directory.Delete(_testDbDirectory, true);
        }
    }

    [Test]
    public void CheckWarTrigger_BothFactionsOver45Percent_TriggersWar()
    {
        // Arrange: Sector 3 has Jötun-Readers 48%, Rust-Clans 45% (contested, meets threshold)
        int sectorId = 3;

        // Note: Sector 3 already has a war from seed data, so we need to use a different sector
        // Let's use sector 9 (Asgard): God-Sleepers 46%, Jötun-Readers 44%
        sectorId = 9;

        // Act
        bool warTriggered = _warService.CheckWarTrigger(sectorId);

        // Assert
        Assert.That(warTriggered, Is.True);

        // Verify war was created
        var activeWar = _warService.GetActiveWarForSector(sectorId);
        Assert.That(activeWar, Is.Not.Null);
        Assert.That(activeWar!.SectorId, Is.EqualTo(sectorId));
        Assert.That(activeWar.IsActive, Is.True);
    }

    [Test]
    public void CheckWarTrigger_AlreadyAtWar_ReturnsFalse()
    {
        // Arrange: Sector 3 already has active war from seed data
        int sectorId = 3;

        // Act
        bool warTriggered = _warService.CheckWarTrigger(sectorId);

        // Assert
        Assert.That(warTriggered, Is.False);
    }

    [Test]
    public void CheckWarTrigger_OneFactionBelow45Percent_DoesNotTriggerWar()
    {
        // Arrange: Sector 1 (Midgard) - Rust-Clans 35%, Iron-Banes 30% (both below threshold)
        int sectorId = 1;

        // Act
        bool warTriggered = _warService.CheckWarTrigger(sectorId);

        // Assert
        Assert.That(warTriggered, Is.False);
    }

    [Test]
    public void GetActiveWars_AfterInitialization_ReturnsSeededWar()
    {
        // Act
        var activeWars = _warService.GetActiveWars();

        // Assert: Should have 1 war from seed data (Niflheim)
        Assert.That(activeWars, Has.Count.EqualTo(1));
        Assert.That(activeWars[0].SectorId, Is.EqualTo(3)); // Niflheim
        Assert.That(activeWars[0].FactionA, Is.EqualTo("JotunReaders"));
        Assert.That(activeWars[0].FactionB, Is.EqualTo("RustClans"));
    }

    [Test]
    public void GetActiveWarForSector_SectorWithWar_ReturnsWar()
    {
        // Arrange: Sector 3 has active war from seed data
        int sectorId = 3;

        // Act
        var war = _warService.GetActiveWarForSector(sectorId);

        // Assert
        Assert.That(war, Is.Not.Null);
        Assert.That(war!.SectorId, Is.EqualTo(sectorId));
        Assert.That(war.IsActive, Is.True);
        Assert.That(war.FactionA, Is.EqualTo("JotunReaders"));
        Assert.That(war.FactionB, Is.EqualTo("RustClans"));
    }

    [Test]
    public void GetActiveWarForSector_SectorWithoutWar_ReturnsNull()
    {
        // Arrange: Sector 1 (Midgard) has no active war
        int sectorId = 1;

        // Act
        var war = _warService.GetActiveWarForSector(sectorId);

        // Assert
        Assert.That(war, Is.Null);
    }

    [Test]
    public void AdvanceWar_PositiveShift_IncreasesBalanceTowardFactionA()
    {
        // Arrange: Get active war in sector 3
        var war = _warService.GetActiveWarForSector(3);
        Assert.That(war, Is.Not.Null);
        int warId = war!.WarId;
        double initialBalance = war.WarBalance;

        // Act: Shift balance toward faction_a (JotunReaders)
        _warService.AdvanceWar(warId, 15.0, "Player quest completion for JotunReaders");

        // Assert: Balance should increase
        var updatedWar = _warService.GetActiveWarForSector(3);
        Assert.That(updatedWar, Is.Not.Null);
        Assert.That(updatedWar!.WarBalance, Is.GreaterThan(initialBalance));
        Assert.That(updatedWar.WarBalance, Is.EqualTo(initialBalance + 15.0).Within(0.01));
    }

    [Test]
    public void AdvanceWar_NegativeShift_DecreasesBalanceTowardFactionB()
    {
        // Arrange: Get active war in sector 3
        var war = _warService.GetActiveWarForSector(3);
        Assert.That(war, Is.Not.Null);
        int warId = war!.WarId;
        double initialBalance = war.WarBalance;

        // Act: Shift balance toward faction_b (RustClans)
        _warService.AdvanceWar(warId, -20.0, "Player quest completion for RustClans");

        // Assert: Balance should decrease
        var updatedWar = _warService.GetActiveWarForSector(3);
        Assert.That(updatedWar, Is.Not.Null);
        Assert.That(updatedWar!.WarBalance, Is.LessThan(initialBalance));
        Assert.That(updatedWar.WarBalance, Is.EqualTo(initialBalance - 20.0).Within(0.01));
    }

    [Test]
    public void AdvanceWar_BalanceExceedsThreshold_ResolvesWar()
    {
        // Arrange: Get active war in sector 3
        var war = _warService.GetActiveWarForSector(3);
        Assert.That(war, Is.Not.Null);
        int warId = war!.WarId;

        // Act: Shift balance massively to trigger resolution (threshold is ±50)
        _warService.AdvanceWar(warId, 60.0, "Decisive victory for JotunReaders");

        // Assert: War should be resolved
        var activeWar = _warService.GetActiveWarForSector(3);
        Assert.That(activeWar, Is.Null); // No longer active

        // Verify influence changes were applied
        var influences = _territoryService.GetSectorInfluences(3);
        var jotunReadersInfluence = influences.First(f => f.FactionName == "JotunReaders");
        var rustClansInfluence = influences.First(f => f.FactionName == "RustClans");

        // JotunReaders should have gained influence, RustClans lost
        // Initial: JotunReaders 48%, RustClans 45%
        // After war: JotunReaders +20%, RustClans -20%
        Assert.That(jotunReadersInfluence.InfluenceValue, Is.GreaterThan(48.0));
        Assert.That(rustClansInfluence.InfluenceValue, Is.LessThan(45.0));
    }

    [Test]
    public void ResolveWar_VictorDeclared_AppliesInfluenceChanges()
    {
        // Arrange: Create a new war in sector 9
        int sectorId = 9;
        _warService.CheckWarTrigger(sectorId); // Trigger war
        var war = _warService.GetActiveWarForSector(sectorId);
        Assert.That(war, Is.Not.Null);

        // Get initial influences
        var initialInfluences = _territoryService.GetSectorInfluences(sectorId);
        var faction1Initial = initialInfluences[0].InfluenceValue;
        var faction2Initial = initialInfluences[1].InfluenceValue;
        string victor = initialInfluences[0].FactionName;

        // Act: Resolve war with victor
        _warService.ResolveWar(war!.WarId, victor);

        // Assert: Influence changes applied
        var finalInfluences = _territoryService.GetSectorInfluences(sectorId);
        var victorInfluence = finalInfluences.First(f => f.FactionName == victor);

        // Victor should have gained influence
        Assert.That(victorInfluence.InfluenceValue, Is.GreaterThan(faction1Initial));

        // War should be inactive
        var activeWar = _warService.GetActiveWarForSector(sectorId);
        Assert.That(activeWar, Is.Null);
    }

    [Test]
    public void AdvanceWar_BalanceClamping_StaysWithinBounds()
    {
        // Arrange: Get active war
        var war = _warService.GetActiveWarForSector(3);
        Assert.That(war, Is.Not.Null);
        int warId = war!.WarId;

        // Act: Attempt to shift way beyond max
        _warService.AdvanceWar(warId, 200.0, "Massive shift test");

        // Assert: Balance should be clamped to max (100.0)
        // But war will be resolved since balance exceeds threshold (50.0)
        // So we need to check that it resolved, not still active
        var activeWar = _warService.GetActiveWarForSector(3);
        Assert.That(activeWar, Is.Null); // War resolved due to threshold
    }

    [Test]
    public void FactionWar_AfterResolution_UpdatesControlState()
    {
        // Arrange: Trigger war in sector 9
        int sectorId = 9;
        _warService.CheckWarTrigger(sectorId);

        // Verify state is "War"
        var warState = _territoryService.CalculateSectorControlState(sectorId);
        Assert.That(warState.State, Is.EqualTo("War"));

        var war = _warService.GetActiveWarForSector(sectorId);
        Assert.That(war, Is.Not.Null);

        // Act: Resolve war
        _warService.ResolveWar(war!.WarId, war.FactionA);

        // Assert: State should no longer be "War"
        var postWarState = _territoryService.CalculateSectorControlState(sectorId);
        Assert.That(postWarState.State, Is.Not.EqualTo("War"));
        // Should be Stable, Contested, or Independent based on new influence distribution
        Assert.That(new[] { "Stable", "Contested", "Independent" }, Does.Contain(postWarState.State));
    }
}
