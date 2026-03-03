using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ReputationService"/>.
/// Covers applying reputation changes, tier detection, requirement checks,
/// and edge cases per SPEC-REPUTATION-001.
/// </summary>
[TestFixture]
public class ReputationServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════

    private Mock<IFactionDefinitionProvider> _mockProvider = null!;
    private Mock<ILogger<ReputationService>> _mockLogger = null!;
    private ReputationService _service = null!;
    private Player _player = null!;

    [SetUp]
    public void SetUp()
    {
        _mockProvider = new Mock<IFactionDefinitionProvider>();
        _mockLogger = new Mock<ILogger<ReputationService>>();
        _service = new ReputationService(_mockProvider.Object, _mockLogger.Object);
        _player = new Player("TestHero");

        // Default: all 5 factions exist
        SetupFaction("iron-banes", "Iron-Banes", allies: ["rust-clans"], enemies: ["god-sleeper-cultists"]);
        SetupFaction("god-sleeper-cultists", "God-Sleeper Cultists", allies: [], enemies: ["iron-banes"]);
        SetupFaction("jotun-readers", "Jötun-Readers", allies: ["rust-clans"], enemies: []);
        SetupFaction("rust-clans", "Rust-Clans", allies: ["iron-banes", "jotun-readers"], enemies: []);
        SetupFaction("independents", "Independents", allies: [], enemies: []);
    }

    /// <summary>
    /// Helper to configure a faction in the mock provider.
    /// </summary>
    private void SetupFaction(string factionId, string name, string[] allies, string[] enemies)
    {
        var dto = new FactionDefinitionDto
        {
            FactionId = factionId,
            Name = name,
            Allies = allies,
            Enemies = enemies,
            DefaultReputation = 0
        };

        _mockProvider.Setup(p => p.FactionExists(factionId)).Returns(true);
        _mockProvider.Setup(p => p.GetFaction(factionId)).Returns(dto);
    }

    // ═══════════════════════════════════════════════════════════════
    // ApplyReputationChanges TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyReputationChanges_NullPlayer_ThrowsArgumentNullException()
    {
        // Arrange
        var changes = new Dictionary<string, int> { { "iron-banes", 10 } };

        // Act & Assert
        FluentActions.Invoking(() => _service.ApplyReputationChanges(null!, changes, WitnessContext.Direct))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ApplyReputationChanges_NullChanges_ReturnsEmptyList()
    {
        // Act
        var results = _service.ApplyReputationChanges(_player, null!, WitnessContext.Direct);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void ApplyReputationChanges_EmptyChanges_ReturnsEmptyList()
    {
        // Act
        var results = _service.ApplyReputationChanges(
            _player, new Dictionary<string, int>(), WitnessContext.Direct);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void ApplyReputationChanges_SingleFaction_AppliesDelta()
    {
        // Arrange
        var changes = new Dictionary<string, int> { { "iron-banes", 25 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert
        results.Should().HaveCount(1);
        var result = results[0];
        result.FactionId.Should().Be("iron-banes");
        result.FactionName.Should().Be("Iron-Banes");
        result.RawDelta.Should().Be(25);
        result.ActualDelta.Should().Be(25);
        result.NewValue.Should().Be(25);
        result.OldTier.Should().Be(ReputationTier.Neutral);
        result.NewTier.Should().Be(ReputationTier.Friendly);
        result.TierChanged.Should().BeTrue();
        result.Message.Should().Be("+25 Iron-Banes Reputation");
        result.TierTransitionMessage.Should().Be("Your standing with Iron-Banes is now Friendly!");

        // Verify player state was updated
        _player.GetFactionReputation("iron-banes").Value.Should().Be(25);
    }

    [Test]
    public void ApplyReputationChanges_MultipleFactions_AppliesAll()
    {
        // Arrange — quest gives +40 Iron-Banes, -60 God-Sleepers
        var changes = new Dictionary<string, int>
        {
            { "iron-banes", 40 },
            { "god-sleeper-cultists", -60 }
        };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert
        results.Should().HaveCount(2);

        var ironResult = results.First(r => r.FactionId == "iron-banes");
        ironResult.NewValue.Should().Be(40);
        ironResult.NewTier.Should().Be(ReputationTier.Friendly);

        var cultistResult = results.First(r => r.FactionId == "god-sleeper-cultists");
        cultistResult.NewValue.Should().Be(-60);
        cultistResult.NewTier.Should().Be(ReputationTier.Hostile);
    }

    [Test]
    public void ApplyReputationChanges_NegativeDelta_DecreasesReputation()
    {
        // Arrange — start with some reputation, then lose it
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 30));
        var changes = new Dictionary<string, int> { { "iron-banes", -40 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert
        results.Should().HaveCount(1);
        results[0].NewValue.Should().Be(-10); // 30 + (-40) = -10
        results[0].NewTier.Should().Be(ReputationTier.Neutral);
    }

    [Test]
    public void ApplyReputationChanges_UnknownFaction_SkipsAndLogsWarning()
    {
        // Arrange — "raiders" doesn't exist in config
        _mockProvider.Setup(p => p.FactionExists("raiders")).Returns(false);
        var changes = new Dictionary<string, int>
        {
            { "iron-banes", 10 },
            { "raiders", 50 } // Should be skipped
        };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert — only iron-banes appears in results
        results.Should().HaveCount(1);
        results[0].FactionId.Should().Be("iron-banes");

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("raiders")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ApplyReputationChanges_ClampsAtMax()
    {
        // Arrange — start at 95, add 20
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 95));
        var changes = new Dictionary<string, int> { { "iron-banes", 20 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert — clamped to 100, effective delta is 5
        results[0].NewValue.Should().Be(100);
        results[0].ActualDelta.Should().Be(5); // 100 - 95
    }

    [Test]
    public void ApplyReputationChanges_ClampsAtMin()
    {
        // Arrange — start at -90, subtract 20
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", -90));
        var changes = new Dictionary<string, int> { { "iron-banes", -20 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert — clamped to -100, effective delta is -10
        results[0].NewValue.Should().Be(-100);
        results[0].ActualDelta.Should().Be(-10); // -100 - (-90)
    }

    // ═══════════════════════════════════════════════════════════════
    // WITNESS MULTIPLIER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyReputationChanges_WitnessedContext_Applies75Percent()
    {
        // Arrange — raw delta is 20, witnessed = 75% = 15
        var changes = new Dictionary<string, int> { { "iron-banes", 20 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Witnessed);

        // Assert — (int)(20 * 0.75) = 15
        results[0].RawDelta.Should().Be(20);
        results[0].NewValue.Should().Be(15);
    }

    [Test]
    public void ApplyReputationChanges_UnwitnessedContext_AppliesZero()
    {
        // Arrange — raw delta is 20, unwitnessed = 0%
        var changes = new Dictionary<string, int> { { "iron-banes", 20 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Unwitnessed);

        // Assert — (int)(20 * 0.0) = 0
        results[0].ActualDelta.Should().Be(0);
        results[0].NewValue.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // TIER TRANSITION DETECTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyReputationChanges_NoTierChange_TierChangedIsFalse()
    {
        // Arrange — small delta stays within Neutral (-25 to +24)
        var changes = new Dictionary<string, int> { { "iron-banes", 5 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert
        results[0].TierChanged.Should().BeFalse();
        results[0].TierTransitionMessage.Should().BeNull();
    }

    [Test]
    public void ApplyReputationChanges_TierChange_IncludesTransitionMessage()
    {
        // Arrange — push from Neutral to Friendly
        var changes = new Dictionary<string, int> { { "iron-banes", 30 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert
        results[0].TierChanged.Should().BeTrue();
        results[0].OldTier.Should().Be(ReputationTier.Neutral);
        results[0].NewTier.Should().Be(ReputationTier.Friendly);
        results[0].TierTransitionMessage.Should().Be("Your standing with Iron-Banes is now Friendly!");
    }

    [Test]
    public void ApplyReputationChanges_MultiTierJump_ReportsEndTier()
    {
        // Arrange — huge negative delta goes from Neutral to Hated
        var changes = new Dictionary<string, int> { { "iron-banes", -80 } };

        // Act
        var results = _service.ApplyReputationChanges(_player, changes, WitnessContext.Direct);

        // Assert
        results[0].OldTier.Should().Be(ReputationTier.Neutral);
        results[0].NewTier.Should().Be(ReputationTier.Hated);
        results[0].TierChanged.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetTier TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetTier_NoReputation_ReturnsNeutral()
    {
        // Act
        var tier = _service.GetTier(_player, "iron-banes");

        // Assert
        tier.Should().Be(ReputationTier.Neutral);
    }

    [Test]
    public void GetTier_WithReputation_ReturnsCorrectTier()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 75));

        // Act
        var tier = _service.GetTier(_player, "iron-banes");

        // Assert
        tier.Should().Be(ReputationTier.Exalted);
    }

    [Test]
    public void GetTier_NullFactionId_ReturnsNeutral()
    {
        // Act
        var tier = _service.GetTier(_player, null!);

        // Assert
        tier.Should().Be(ReputationTier.Neutral);
    }

    [Test]
    public void GetTier_EmptyFactionId_ReturnsNeutral()
    {
        // Act
        var tier = _service.GetTier(_player, "");

        // Assert
        tier.Should().Be(ReputationTier.Neutral);
    }

    // ═══════════════════════════════════════════════════════════════
    // MeetsReputationRequirement TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void MeetsReputationRequirement_ExactValue_ReturnsTrue()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 25));

        // Act & Assert
        _service.MeetsReputationRequirement(_player, "iron-banes", 25).Should().BeTrue();
    }

    [Test]
    public void MeetsReputationRequirement_AboveValue_ReturnsTrue()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 50));

        // Act & Assert
        _service.MeetsReputationRequirement(_player, "iron-banes", 25).Should().BeTrue();
    }

    [Test]
    public void MeetsReputationRequirement_BelowValue_ReturnsFalse()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 10));

        // Act & Assert
        _service.MeetsReputationRequirement(_player, "iron-banes", 25).Should().BeFalse();
    }

    [Test]
    public void MeetsReputationRequirement_NoReputation_DefaultZero()
    {
        // Act — no reputation set, default is 0
        _service.MeetsReputationRequirement(_player, "iron-banes", 0).Should().BeTrue();
        _service.MeetsReputationRequirement(_player, "iron-banes", 1).Should().BeFalse();
    }

    [Test]
    public void MeetsReputationRequirement_NullFactionId_ReturnsFalse()
    {
        // Act & Assert
        _service.MeetsReputationRequirement(_player, null!, 0).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // MeetsTierRequirement TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void MeetsTierRequirement_ExactTier_ReturnsTrue()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 25));

        // Act & Assert — Friendly (25) meets Friendly requirement
        _service.MeetsTierRequirement(_player, "iron-banes", ReputationTier.Friendly).Should().BeTrue();
    }

    [Test]
    public void MeetsTierRequirement_HigherTier_ReturnsTrue()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 75));

        // Act & Assert — Exalted (75) meets Friendly requirement
        _service.MeetsTierRequirement(_player, "iron-banes", ReputationTier.Friendly).Should().BeTrue();
    }

    [Test]
    public void MeetsTierRequirement_LowerTier_ReturnsFalse()
    {
        // Arrange — Neutral (0) does NOT meet Friendly requirement
        _service.MeetsTierRequirement(_player, "iron-banes", ReputationTier.Friendly).Should().BeFalse();
    }

    [Test]
    public void MeetsTierRequirement_NullFactionId_ReturnsFalse()
    {
        // Act & Assert
        _service.MeetsTierRequirement(_player, null!, ReputationTier.Neutral).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetFactionName / GetAllFactionIds TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetFactionName_KnownFaction_ReturnsDisplayName()
    {
        // Act
        var name = _service.GetFactionName("iron-banes");

        // Assert
        name.Should().Be("Iron-Banes");
    }

    [Test]
    public void GetFactionName_UnknownFaction_ReturnsFactionId()
    {
        // Arrange
        _mockProvider.Setup(p => p.GetFaction("unknown")).Returns((FactionDefinitionDto?)null);

        // Act
        var name = _service.GetFactionName("unknown");

        // Assert
        name.Should().Be("unknown");
    }

    [Test]
    public void GetFactionName_NullOrEmpty_ReturnsEmpty()
    {
        _service.GetFactionName(null!).Should().BeEmpty();
        _service.GetFactionName("").Should().BeEmpty();
    }

    [Test]
    public void GetAllFactionIds_ReturnsAllFactionIds()
    {
        // Arrange
        var allFactions = new List<FactionDefinitionDto>
        {
            new() { FactionId = "iron-banes" },
            new() { FactionId = "rust-clans" },
            new() { FactionId = "independents" }
        };
        _mockProvider.Setup(p => p.GetAllFactions()).Returns(allFactions);

        // Act
        var ids = _service.GetAllFactionIds();

        // Assert
        ids.Should().HaveCount(3);
        ids.Should().Contain("iron-banes");
        ids.Should().Contain("rust-clans");
        ids.Should().Contain("independents");
    }

    // ═══════════════════════════════════════════════════════════════
    // PLAYER INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Player_GetFactionReputation_UnknownFaction_ReturnsNeutral()
    {
        // Act
        var rep = _player.GetFactionReputation("never-heard-of-them");

        // Assert
        rep.Value.Should().Be(0);
        rep.Tier.Should().Be(ReputationTier.Neutral);
        rep.FactionId.Should().Be("never-heard-of-them");
    }

    [Test]
    public void Player_SetAndGetFactionReputation_RoundTrips()
    {
        // Arrange
        var rep = FactionReputation.Create("iron-banes", 42);

        // Act
        _player.SetFactionReputation(rep);
        var retrieved = _player.GetFactionReputation("iron-banes");

        // Assert
        retrieved.Value.Should().Be(42);
        retrieved.Tier.Should().Be(ReputationTier.Friendly);
        retrieved.FactionId.Should().Be("iron-banes");
    }

    [Test]
    public void Player_SetFactionReputation_IsCaseInsensitive()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("Iron-Banes", 50));

        // Act — retrieve with different casing
        var rep = _player.GetFactionReputation("iron-banes");

        // Assert
        rep.Value.Should().Be(50);
    }

    [Test]
    public void Player_GetFactionTier_ReturnsCorrectTier()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("rust-clans", -80));

        // Act
        var tier = _player.GetFactionTier("rust-clans");

        // Assert
        tier.Should().Be(ReputationTier.Hated);
    }

    [Test]
    public void Player_GetFactionReputationValues_ReturnsIntDictionary()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 30));
        _player.SetFactionReputation(FactionReputation.Create("rust-clans", -15));

        // Act
        var values = _player.GetFactionReputationValues();

        // Assert — suitable for FailureCheckContext.FactionReputations
        values.Should().HaveCount(2);
        values["iron-banes"].Should().Be(30);
        values["rust-clans"].Should().Be(-15);
    }

    [Test]
    public void Player_FactionReputations_ReadOnlyView_ReflectsChanges()
    {
        // Arrange
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 10));

        // Act & Assert
        _player.FactionReputations.Should().HaveCount(1);
        _player.FactionReputations["iron-banes"].Value.Should().Be(10);
    }

    [Test]
    public void Player_SetFactionReputation_EmptyFactionId_Throws()
    {
        // Arrange
        var rep = FactionReputation.Create("valid-id", 10);
        // We need to create a reputation with empty FactionId — but FactionReputation
        // requires non-null FactionId. So test the Player's guard clause directly.
        // The FactionReputation.Create would throw for null, but an empty string passes.

        // Act & Assert — Player guards against whitespace FactionId
        FluentActions.Invoking(() =>
            _player.SetFactionReputation(FactionReputation.Create("", 10)))
            .Should().Throw<ArgumentException>();
    }
}
