using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Core.Territory;
using RuneAndRust.Persistence;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.35.2: Test suite for TerritoryControlService
/// Validates sector control calculations, influence management, and state transitions
/// </summary>
[TestFixture]
public class TerritoryControlServiceTests
{
    private TerritoryControlService _service;
    private SaveRepository _saveRepository;
    private string _testDbDirectory;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        // Create unique test database
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"territory_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbDirectory);

        // Initialize database schema (includes v0.35.1 territory tables)
        _saveRepository = new SaveRepository(_testDbDirectory);
        _connectionString = $"Data Source={Path.Combine(_testDbDirectory, "runeandrust.db")}";

        _service = new TerritoryControlService(_connectionString);
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
    public void CalculateSectorControlState_OneFactionOver60Percent_ReturnsStable()
    {
        // Arrange: Sector 2 (Muspelheim) has Iron-Banes at 65%
        int sectorId = 2;

        // Act
        var result = _service.CalculateSectorControlState(sectorId);

        // Assert
        Assert.That(result.State, Is.EqualTo("Stable"));
        Assert.That(result.DominantFaction, Is.EqualTo("IronBanes"));
        Assert.That(result.ContestedFactions, Is.Null);
    }

    [Test]
    public void CalculateSectorControlState_TwoFactionsOver40Percent_ReturnsContested()
    {
        // Arrange: Sector 3 (Niflheim) has Jötun-Readers 48%, Rust-Clans 45%
        int sectorId = 3;

        // Act
        var result = _service.CalculateSectorControlState(sectorId);

        // Assert
        Assert.That(result.State, Is.EqualTo("Contested"));
        Assert.That(result.DominantFaction, Is.Null);
        Assert.That(result.ContestedFactions, Has.Length.EqualTo(2));
        Assert.That(result.ContestedFactions, Does.Contain("JotunReaders"));
        Assert.That(result.ContestedFactions, Does.Contain("RustClans"));
    }

    [Test]
    public void CalculateSectorControlState_ActiveWar_ReturnsWar()
    {
        // Arrange: Sector 3 has active war (seeded in v0.35.1)
        int sectorId = 3;

        // Act
        var result = _service.CalculateSectorControlState(sectorId);

        // Assert
        Assert.That(result.State, Is.EqualTo("War"));
        Assert.That(result.DominantFaction, Is.Null);
        Assert.That(result.ContestedFactions, Has.Length.EqualTo(2));
        Assert.That(result.ContestedFactions, Does.Contain("JotunReaders"));
        Assert.That(result.ContestedFactions, Does.Contain("RustClans"));
    }

    [Test]
    public void CalculateSectorControlState_NoFactionOver40Percent_ReturnsIndependent()
    {
        // Arrange: Sector 1 (Midgard) - no faction over 40%
        int sectorId = 1;

        // Act
        var result = _service.CalculateSectorControlState(sectorId);

        // Assert
        Assert.That(result.State, Is.EqualTo("Independent"));
        Assert.That(result.DominantFaction, Is.EqualTo("Independents"));
    }

    [Test]
    public void GetDominantFaction_StableSector_ReturnsControllingFaction()
    {
        // Arrange: Sector 2 (Muspelheim) controlled by Iron-Banes
        int sectorId = 2;

        // Act
        var dominantFaction = _service.GetDominantFaction(sectorId);

        // Assert
        Assert.That(dominantFaction, Is.EqualTo("IronBanes"));
    }

    [Test]
    public void GetSectorInfluences_ValidSector_ReturnsAllFactions()
    {
        // Arrange: Sector 2 (Muspelheim)
        int sectorId = 2;

        // Act
        var influences = _service.GetSectorInfluences(sectorId);

        // Assert
        Assert.That(influences, Has.Count.EqualTo(5)); // 5 factions
        Assert.That(influences[0].InfluenceValue, Is.GreaterThanOrEqualTo(influences[1].InfluenceValue)); // Ordered descending
        Assert.That(influences[0].FactionName, Is.EqualTo("IronBanes")); // Top faction
    }

    [Test]
    public void ShiftInfluence_PositiveDelta_IncreasesInfluence()
    {
        // Arrange: Sector 1, increase Rust-Clans influence
        int sectorId = 1;
        string factionName = "RustClans";
        double initialInfluence = _service.GetSectorInfluences(sectorId)
            .First(f => f.FactionName == factionName).InfluenceValue;

        // Act
        _service.ShiftInfluence(sectorId, factionName, 10.0, "Test increase");

        // Assert
        var newInfluence = _service.GetSectorInfluences(sectorId)
            .First(f => f.FactionName == factionName).InfluenceValue;

        Assert.That(newInfluence, Is.GreaterThan(initialInfluence));
    }

    [Test]
    public void ShiftInfluence_NegativeDelta_DecreasesInfluence()
    {
        // Arrange: Sector 2, decrease Iron-Banes influence
        int sectorId = 2;
        string factionName = "IronBanes";
        double initialInfluence = _service.GetSectorInfluences(sectorId)
            .First(f => f.FactionName == factionName).InfluenceValue;

        // Act
        _service.ShiftInfluence(sectorId, factionName, -10.0, "Test decrease");

        // Assert
        var newInfluence = _service.GetSectorInfluences(sectorId)
            .First(f => f.FactionName == factionName).InfluenceValue;

        Assert.That(newInfluence, Is.LessThan(initialInfluence));
    }

    [Test]
    public void ShiftInfluence_LargeIncrease_NormalizesToMax100Percent()
    {
        // Arrange: Sector 1, massive increase
        int sectorId = 1;
        string factionName = "RustClans";

        // Act: Add huge amount that would exceed 100% total
        _service.ShiftInfluence(sectorId, factionName, 80.0, "Test normalization");

        // Assert: Total influence should still be ~100%
        var influences = _service.GetSectorInfluences(sectorId);
        var totalInfluence = influences.Sum(f => f.InfluenceValue);

        Assert.That(totalInfluence, Is.LessThanOrEqualTo(100.1)); // Allow tiny floating point error
        Assert.That(totalInfluence, Is.GreaterThanOrEqualTo(99.9));
    }

    [Test]
    public void GetSectors_WorldId1_Returns10Sectors()
    {
        // Arrange: World ID 1 (Aethelgard)
        int worldId = 1;

        // Act
        var sectors = _service.GetSectors(worldId);

        // Assert
        Assert.That(sectors, Has.Count.EqualTo(10));
        Assert.That(sectors.Any(s => s.SectorName == "Midgard"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Muspelheim"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Niflheim"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Alfheim"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Jotunheim"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Svartalfheim"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Vanaheim"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Helheim"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Asgard"), Is.True);
        Assert.That(sectors.Any(s => s.SectorName == "Valhalla"), Is.True);
    }

    [Test]
    public void ShiftInfluence_ChangesControlState_UpdatesAllFactionsInSector()
    {
        // Arrange: Sector 1 (Independent), boost one faction to Stable
        int sectorId = 1;
        string factionName = "RustClans";

        // Act: Shift enough to make it stable (60%+)
        _service.ShiftInfluence(sectorId, factionName, 30.0, "Test state change");

        // Assert: All factions in sector should have updated control_state
        var influences = _service.GetSectorInfluences(sectorId);
        var rustClansInfluence = influences.First(f => f.FactionName == factionName);

        // After normalization, Rust-Clans should have majority
        Assert.That(rustClansInfluence.InfluenceValue, Is.GreaterThan(50.0));

        // All factions should have same control_state (calculated for whole sector)
        var controlStates = influences.Select(f => f.ControlState).Distinct().ToList();
        Assert.That(controlStates, Has.Count.EqualTo(1)); // All same state
    }
}
