using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Infrastructure.Providers;

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Integration tests for <see cref="JsonFactionDefinitionProvider"/>.
/// Loads the actual config/factions.json and verifies all 5 factions
/// are parsed correctly with their ally/enemy relationships.
/// </summary>
[TestFixture]
public class JsonFactionDefinitionProviderTests
{
    private JsonFactionDefinitionProvider _provider = null!;
    private Mock<ILogger<JsonFactionDefinitionProvider>> _mockLogger = null!;

    /// <summary>
    /// Resolves the path to config/factions.json relative to the test execution directory.
    /// </summary>
    private static string GetConfigPath()
    {
        // Walk up from bin/Debug/net9.0 to find config/
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        return Path.Combine(projectRoot, "config", "factions.json");
    }

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JsonFactionDefinitionProvider>>();
        var configPath = GetConfigPath();

        // Verify config file exists before attempting to load
        if (!File.Exists(configPath))
        {
            Assert.Inconclusive($"Config file not found at: {configPath}");
            return;
        }

        _provider = new JsonFactionDefinitionProvider(configPath, _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // LOADING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetAllFactions_LoadsAllFiveFactions()
    {
        // Act
        var factions = _provider.GetAllFactions();

        // Assert — design doc defines exactly 5 factions
        factions.Should().HaveCount(5);
    }

    [Test]
    public void GetAllFactions_ContainsAllExpectedFactionIds()
    {
        // Act
        var factions = _provider.GetAllFactions();
        var ids = factions.Select(f => f.FactionId).ToList();

        // Assert
        ids.Should().Contain("iron-banes");
        ids.Should().Contain("god-sleeper-cultists");
        ids.Should().Contain("jotun-readers");
        ids.Should().Contain("rust-clans");
        ids.Should().Contain("independents");
    }

    // ═══════════════════════════════════════════════════════════════
    // INDIVIDUAL FACTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetFaction_IronBanes_HasCorrectProperties()
    {
        // Act
        var faction = _provider.GetFaction("iron-banes");

        // Assert
        faction.Should().NotBeNull();
        faction!.Name.Should().Be("Iron-Banes");
        faction.Philosophy.Should().Contain("Undying");
        faction.Location.Should().Contain("Trunk");
    }

    [Test]
    public void GetFaction_CaseInsensitive()
    {
        // Act — mixed case lookup
        var faction = _provider.GetFaction("Iron-Banes");

        // Assert
        faction.Should().NotBeNull();
        faction!.FactionId.Should().Be("iron-banes");
    }

    [Test]
    public void GetFaction_Unknown_ReturnsNull()
    {
        // Act
        var faction = _provider.GetFaction("nonexistent-faction");

        // Assert
        faction.Should().BeNull();
    }

    [Test]
    public void GetFaction_NullOrEmpty_ReturnsNull()
    {
        _provider.GetFaction(null!).Should().BeNull();
        _provider.GetFaction("").Should().BeNull();
        _provider.GetFaction("  ").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTION EXISTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FactionExists_KnownFaction_ReturnsTrue()
    {
        _provider.FactionExists("iron-banes").Should().BeTrue();
        _provider.FactionExists("god-sleeper-cultists").Should().BeTrue();
        _provider.FactionExists("jotun-readers").Should().BeTrue();
        _provider.FactionExists("rust-clans").Should().BeTrue();
        _provider.FactionExists("independents").Should().BeTrue();
    }

    [Test]
    public void FactionExists_UnknownFaction_ReturnsFalse()
    {
        _provider.FactionExists("raiders").Should().BeFalse();
    }

    [Test]
    public void FactionExists_NullOrEmpty_ReturnsFalse()
    {
        _provider.FactionExists(null!).Should().BeFalse();
        _provider.FactionExists("").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // ALLY/ENEMY RELATIONSHIP TESTS
    // Per design doc Section 4.1:
    //   Iron-Banes ↔ God-Sleepers: Hostile
    //   Rust-Clans ↔ Jötun-Readers: Friendly
    //   Independents ↔ Any: Neutral
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetAllies_IronBanes_ReturnsRustClans()
    {
        // Act
        var allies = _provider.GetAllies("iron-banes");

        // Assert
        allies.Should().HaveCount(1);
        allies.Should().Contain("rust-clans");
    }

    [Test]
    public void GetEnemies_IronBanes_ReturnsGodSleeperCultists()
    {
        // Act
        var enemies = _provider.GetEnemies("iron-banes");

        // Assert
        enemies.Should().HaveCount(1);
        enemies.Should().Contain("god-sleeper-cultists");
    }

    [Test]
    public void GetAllies_RustClans_ReturnsIronBanesAndJotunReaders()
    {
        // Act
        var allies = _provider.GetAllies("rust-clans");

        // Assert
        allies.Should().HaveCount(2);
        allies.Should().Contain("iron-banes");
        allies.Should().Contain("jotun-readers");
    }

    [Test]
    public void GetEnemies_GodSleeperCultists_ReturnsIronBanes()
    {
        // Act
        var enemies = _provider.GetEnemies("god-sleeper-cultists");

        // Assert
        enemies.Should().HaveCount(1);
        enemies.Should().Contain("iron-banes");
    }

    [Test]
    public void GetAllies_Independents_ReturnsEmpty()
    {
        // Act — Independents have no allies per design doc
        var allies = _provider.GetAllies("independents");

        // Assert
        allies.Should().BeEmpty();
    }

    [Test]
    public void GetEnemies_Independents_ReturnsEmpty()
    {
        // Act — Independents have no enemies per design doc
        var enemies = _provider.GetEnemies("independents");

        // Assert
        enemies.Should().BeEmpty();
    }

    [Test]
    public void GetAllies_UnknownFaction_ReturnsEmpty()
    {
        _provider.GetAllies("nonexistent").Should().BeEmpty();
    }

    [Test]
    public void GetEnemies_NullFaction_ReturnsEmpty()
    {
        _provider.GetEnemies(null!).Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // MISSING CONFIG FILE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_MissingConfigFile_LoadsEmpty()
    {
        // Arrange
        var logger = new Mock<ILogger<JsonFactionDefinitionProvider>>();
        var provider = new JsonFactionDefinitionProvider("/nonexistent/path/factions.json", logger.Object);

        // Act & Assert — gracefully degrades
        provider.GetAllFactions().Should().BeEmpty();
        provider.FactionExists("iron-banes").Should().BeFalse();
        provider.GetFaction("iron-banes").Should().BeNull();
    }
}
