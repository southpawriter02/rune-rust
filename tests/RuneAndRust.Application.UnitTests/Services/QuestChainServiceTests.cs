using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="QuestChainService"/>.
/// Covers quest availability (legend + prerequisites + reputation),
/// chain summaries, chain ordering, and edge cases per SPEC-FACTION-QUESTS-001.
/// </summary>
/// <remarks>
/// <para><b>Test Strategy:</b> All dependencies (<see cref="IQuestDefinitionProvider"/>,
/// <see cref="IReputationService"/>) are mocked. A real <see cref="Player"/> instance
/// is used since the service interacts with its faction reputation dictionary.</para>
///
/// <para><b>Standard Test Data:</b> A 3-quest Iron-Banes chain is configured by default
/// in <see cref="SetUp"/>: quest_1 (no rep), quest_2 (Friendly, 25 rep), quest_3 (Allied, 50 rep).
/// Additional quests are added per-test as needed.</para>
/// </remarks>
[TestFixture]
public class QuestChainServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════

    private Mock<IQuestDefinitionProvider> _mockQuestProvider = null!;
    private Mock<IReputationService> _mockRepService = null!;
    private Mock<ILogger<QuestChainService>> _mockLogger = null!;
    private QuestChainService _service = null!;
    private Player _player = null!;

    /// <summary>
    /// Standard Iron-Banes chain: 3 quests with escalating reputation gates.
    /// quest_ib_1: ChainOrder=1, no rep requirement (chain opener)
    /// quest_ib_2: ChainOrder=2, requires 25 rep (Friendly)
    /// quest_ib_3: ChainOrder=3, requires 50 rep (Allied)
    /// </summary>
    private List<QuestDefinitionDto> _allQuests = null!;

    [SetUp]
    public void SetUp()
    {
        _mockQuestProvider = new Mock<IQuestDefinitionProvider>();
        _mockRepService = new Mock<IReputationService>();
        _mockLogger = new Mock<ILogger<QuestChainService>>();

        _service = new QuestChainService(
            _mockQuestProvider.Object,
            _mockRepService.Object,
            _mockLogger.Object);

        _player = new Player("TestHero");

        // Build the standard 3-quest Iron-Banes chain
        _allQuests = new List<QuestDefinitionDto>
        {
            CreateQuest("quest_ib_1", "Purge the Rust", "iron-banes", "ironbane_initiation", chainOrder: 1, minRep: 0, minLegend: 0),
            CreateQuest("quest_ib_2", "Defend the Anvil", "iron-banes", "ironbane_initiation", chainOrder: 2, minRep: 25, minLegend: 0),
            CreateQuest("quest_ib_3", "Undying Commander", "iron-banes", "ironbane_initiation", chainOrder: 3, minRep: 50, minLegend: 0),
        };

        // Configure the mock provider to return our standard quest set
        ConfigureProvider(_allQuests);

        // Default reputation service setup: faction name lookup
        _mockRepService
            .Setup(r => r.GetFactionName("iron-banes"))
            .Returns("Iron-Banes");
        _mockRepService
            .Setup(r => r.GetFactionName(string.Empty))
            .Returns(string.Empty);
        _mockRepService
            .Setup(r => r.GetFactionName(It.Is<string>(s => s != "iron-banes" && s != string.Empty)))
            .Returns<string>(id => id); // Return raw ID for unknown factions
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullQuestProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QuestChainService(null!, _mockRepService.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("questProvider");
    }

    [Test]
    public void Constructor_NullReputationService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QuestChainService(_mockQuestProvider.Object, null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("reputationService");
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QuestChainService(_mockQuestProvider.Object, _mockRepService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAvailableQuestsForPlayer TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetAvailableQuestsForPlayer_NullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.GetAvailableQuestsForPlayer(null!, 1, new HashSet<string>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetAvailableQuestsForPlayer_NullCompletedSet_TreatedAsEmpty()
    {
        // Arrange — provider returns a quest with no faction/rep requirement
        var noRepQuest = CreateQuest("quest_simple", "Simple Quest", null, null, minRep: 0);
        _mockQuestProvider
            .Setup(p => p.GetAvailableQuests(1, It.IsAny<IReadOnlySet<string>>()))
            .Returns(new List<QuestDefinitionDto> { noRepQuest });

        // Act — passing null completedQuestIds
        var result = _service.GetAvailableQuestsForPlayer(_player, 1, null!);

        // Assert — should not throw, treats null as empty set
        result.Should().HaveCount(1);
    }

    [Test]
    public void GetAvailableQuestsForPlayer_NoFactionQuest_AlwaysAvailable()
    {
        // Arrange — quest with no faction (no reputation gate)
        var genericQuest = CreateQuest("quest_generic", "Generic Quest", null, null, minRep: 0);
        _mockQuestProvider
            .Setup(p => p.GetAvailableQuests(5, It.IsAny<IReadOnlySet<string>>()))
            .Returns(new List<QuestDefinitionDto> { genericQuest });

        // Act
        var result = _service.GetAvailableQuestsForPlayer(_player, 5, new HashSet<string>());

        // Assert — no faction = no reputation check = available
        result.Should().HaveCount(1);
        result[0].QuestId.Should().Be("quest_generic");
    }

    [Test]
    public void GetAvailableQuestsForPlayer_MeetsReputation_ReturnsQuest()
    {
        // Arrange — player has 30 rep with iron-banes (Friendly)
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 30));

        // Provider returns quest_ib_2 (requires 25 rep) as base-available
        _mockQuestProvider
            .Setup(p => p.GetAvailableQuests(5, It.IsAny<IReadOnlySet<string>>()))
            .Returns(new List<QuestDefinitionDto> { _allQuests[1] }); // quest_ib_2

        // Act
        var result = _service.GetAvailableQuestsForPlayer(_player, 5, new HashSet<string>());

        // Assert — player rep 30 >= required 25
        result.Should().HaveCount(1);
        result[0].QuestId.Should().Be("quest_ib_2");
    }

    [Test]
    public void GetAvailableQuestsForPlayer_BelowReputation_FiltersOut()
    {
        // Arrange — player has 10 rep with iron-banes (still Neutral, below 25)
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 10));

        // Provider returns quest_ib_2 (requires 25 rep)
        _mockQuestProvider
            .Setup(p => p.GetAvailableQuests(5, It.IsAny<IReadOnlySet<string>>()))
            .Returns(new List<QuestDefinitionDto> { _allQuests[1] });

        // Act
        var result = _service.GetAvailableQuestsForPlayer(_player, 5, new HashSet<string>());

        // Assert — player rep 10 < required 25, filtered out
        result.Should().BeEmpty();
    }

    [Test]
    public void GetAvailableQuestsForPlayer_ExactReputationThreshold_ReturnsQuest()
    {
        // Arrange — player rep exactly at the required minimum
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 25));

        _mockQuestProvider
            .Setup(p => p.GetAvailableQuests(5, It.IsAny<IReadOnlySet<string>>()))
            .Returns(new List<QuestDefinitionDto> { _allQuests[1] }); // requires 25

        // Act
        var result = _service.GetAvailableQuestsForPlayer(_player, 5, new HashSet<string>());

        // Assert — exactly at threshold = available
        result.Should().HaveCount(1);
    }

    [Test]
    public void GetAvailableQuestsForPlayer_MixedRepRequirements_FiltersCorrectly()
    {
        // Arrange — player has 30 rep (Friendly), which meets quest_ib_2 (25) but not quest_ib_3 (50)
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 30));

        _mockQuestProvider
            .Setup(p => p.GetAvailableQuests(5, It.IsAny<IReadOnlySet<string>>()))
            .Returns(new List<QuestDefinitionDto>
            {
                _allQuests[0], // quest_ib_1: no rep req
                _allQuests[1], // quest_ib_2: 25 rep req — passes
                _allQuests[2], // quest_ib_3: 50 rep req — fails
            });

        // Act
        var result = _service.GetAvailableQuestsForPlayer(_player, 5, new HashSet<string>());

        // Assert — 2 of 3 should pass
        result.Should().HaveCount(2);
        result.Select(q => q.QuestId).Should().Contain("quest_ib_1");
        result.Select(q => q.QuestId).Should().Contain("quest_ib_2");
        result.Select(q => q.QuestId).Should().NotContain("quest_ib_3");
    }

    [Test]
    public void GetAvailableQuestsForPlayer_ZeroMinimumReputation_AlwaysAvailable()
    {
        // Arrange — quest with Faction set but MinimumReputation = 0
        var quest = CreateQuest("quest_open", "Open Quest", "iron-banes", null, minRep: 0);

        _mockQuestProvider
            .Setup(p => p.GetAvailableQuests(1, It.IsAny<IReadOnlySet<string>>()))
            .Returns(new List<QuestDefinitionDto> { quest });

        // Act — player has default 0 rep
        var result = _service.GetAvailableQuestsForPlayer(_player, 1, new HashSet<string>());

        // Assert — MinimumReputation 0 with any faction means no rep gate
        result.Should().HaveCount(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetQuestsInChain TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetQuestsInChain_ValidChain_ReturnsOrderedByChainOrder()
    {
        // Act
        var result = _service.GetQuestsInChain("ironbane_initiation");

        // Assert — 3 quests in order 1, 2, 3
        result.Should().HaveCount(3);
        result[0].QuestId.Should().Be("quest_ib_1");
        result[1].QuestId.Should().Be("quest_ib_2");
        result[2].QuestId.Should().Be("quest_ib_3");
    }

    [Test]
    public void GetQuestsInChain_NullChainId_ReturnsEmpty()
    {
        // Act
        var result = _service.GetQuestsInChain(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetQuestsInChain_EmptyChainId_ReturnsEmpty()
    {
        // Act
        var result = _service.GetQuestsInChain("");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetQuestsInChain_WhitespaceChainId_ReturnsEmpty()
    {
        // Act
        var result = _service.GetQuestsInChain("   ");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetQuestsInChain_UnknownChainId_ReturnsEmpty()
    {
        // Act
        var result = _service.GetQuestsInChain("nonexistent_chain");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetQuestsInChain_CaseInsensitive_ReturnsChain()
    {
        // Act — use mixed-case chain ID
        var result = _service.GetQuestsInChain("Ironbane_Initiation");

        // Assert — should match via OrdinalIgnoreCase
        result.Should().HaveCount(3);
    }

    [Test]
    public void GetQuestsInChain_NullChainOrder_SortedToEnd()
    {
        // Arrange — add a quest with null ChainOrder to the chain
        var questNoOrder = CreateQuest("quest_ib_bonus", "Bonus Quest", "iron-banes", "ironbane_initiation", chainOrder: null, minRep: 0);
        _allQuests.Add(questNoOrder);
        ConfigureProvider(_allQuests);

        // Act
        var result = _service.GetQuestsInChain("ironbane_initiation");

        // Assert — null ChainOrder treated as int.MaxValue, sorted after all numbered quests
        result.Should().HaveCount(4);
        result[3].QuestId.Should().Be("quest_ib_bonus"); // Last due to null ChainOrder
    }

    [Test]
    public void GetQuestsInChain_SameChainOrder_TiebreakerByQuestId()
    {
        // Arrange — two quests with the same ChainOrder
        var questA = CreateQuest("quest_ib_alpha", "Alpha Quest", "iron-banes", "ironbane_test", chainOrder: 1, minRep: 0);
        var questZ = CreateQuest("quest_ib_zulu", "Zulu Quest", "iron-banes", "ironbane_test", chainOrder: 1, minRep: 0);
        _allQuests.Add(questA);
        _allQuests.Add(questZ);
        ConfigureProvider(_allQuests);

        // Act
        var result = _service.GetQuestsInChain("ironbane_test");

        // Assert — alphabetical tiebreaker on QuestId
        result.Should().HaveCount(2);
        result[0].QuestId.Should().Be("quest_ib_alpha");
        result[1].QuestId.Should().Be("quest_ib_zulu");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetNextQuestInChain TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetNextQuestInChain_NoneCompleted_ReturnsFirstQuest()
    {
        // Act
        var result = _service.GetNextQuestInChain("ironbane_initiation", new HashSet<string>());

        // Assert — quest_ib_1 is the first uncompleted
        result.Should().NotBeNull();
        result!.QuestId.Should().Be("quest_ib_1");
    }

    [Test]
    public void GetNextQuestInChain_FirstCompleted_ReturnsSecond()
    {
        // Arrange
        var completed = new HashSet<string> { "quest_ib_1" };

        // Act
        var result = _service.GetNextQuestInChain("ironbane_initiation", completed);

        // Assert
        result.Should().NotBeNull();
        result!.QuestId.Should().Be("quest_ib_2");
    }

    [Test]
    public void GetNextQuestInChain_AllCompleted_ReturnsNull()
    {
        // Arrange
        var completed = new HashSet<string> { "quest_ib_1", "quest_ib_2", "quest_ib_3" };

        // Act
        var result = _service.GetNextQuestInChain("ironbane_initiation", completed);

        // Assert — chain fully complete
        result.Should().BeNull();
    }

    [Test]
    public void GetNextQuestInChain_NullChainId_ReturnsNull()
    {
        // Act
        var result = _service.GetNextQuestInChain(null!, new HashSet<string>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetNextQuestInChain_EmptyChainId_ReturnsNull()
    {
        // Act
        var result = _service.GetNextQuestInChain("", new HashSet<string>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetNextQuestInChain_NullCompletedSet_TreatedAsEmpty()
    {
        // Act
        var result = _service.GetNextQuestInChain("ironbane_initiation", null!);

        // Assert — null treated as empty set, so first quest is returned
        result.Should().NotBeNull();
        result!.QuestId.Should().Be("quest_ib_1");
    }

    [Test]
    public void GetNextQuestInChain_MiddleSkipped_ReturnsMiddle()
    {
        // Arrange — player completed quest 1 and 3, but not 2
        // This is unusual but GetNextQuestInChain returns first uncompleted in order
        var completed = new HashSet<string> { "quest_ib_1", "quest_ib_3" };

        // Act
        var result = _service.GetNextQuestInChain("ironbane_initiation", completed);

        // Assert — quest_ib_2 is the first uncompleted in chain order
        result.Should().NotBeNull();
        result!.QuestId.Should().Be("quest_ib_2");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetChainSummary TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetChainSummary_NoneCompleted_ShowsZeroProgress()
    {
        // Act
        var result = _service.GetChainSummary("ironbane_initiation", _player, new HashSet<string>());

        // Assert
        result.Should().NotBeNull();
        result!.ChainId.Should().Be("ironbane_initiation");
        result.FactionId.Should().Be("iron-banes");
        result.FactionName.Should().Be("Iron-Banes");
        result.TotalQuests.Should().Be(3);
        result.CompletedQuests.Should().Be(0);
        result.IsComplete.Should().BeFalse();
        result.CompletionPercent.Should().Be(0);
        result.NextQuestId.Should().Be("quest_ib_1");
        result.NextQuestName.Should().Be("Purge the Rust");
    }

    [Test]
    public void GetChainSummary_OneCompleted_ShowsProgress()
    {
        // Arrange
        var completed = new HashSet<string> { "quest_ib_1" };

        // Act
        var result = _service.GetChainSummary("ironbane_initiation", _player, completed);

        // Assert
        result.Should().NotBeNull();
        result!.CompletedQuests.Should().Be(1);
        result.CompletionPercent.Should().Be(33); // 1/3 = 33%
        result.NextQuestId.Should().Be("quest_ib_2");
        result.NextQuestName.Should().Be("Defend the Anvil");
    }

    [Test]
    public void GetChainSummary_AllCompleted_IsComplete()
    {
        // Arrange
        var completed = new HashSet<string> { "quest_ib_1", "quest_ib_2", "quest_ib_3" };

        // Act
        var result = _service.GetChainSummary("ironbane_initiation", _player, completed);

        // Assert
        result.Should().NotBeNull();
        result!.CompletedQuests.Should().Be(3);
        result.TotalQuests.Should().Be(3);
        result.IsComplete.Should().BeTrue();
        result.CompletionPercent.Should().Be(100);
        result.NextQuestId.Should().BeNull();
        result.NextQuestName.Should().BeNull();
    }

    [Test]
    public void GetChainSummary_NextQuestReputationLocked_ShowsLockInfo()
    {
        // Arrange — player has 10 rep, next quest (quest_ib_2) requires 25
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 10));
        var completed = new HashSet<string> { "quest_ib_1" };

        // Act
        var result = _service.GetChainSummary("ironbane_initiation", _player, completed);

        // Assert
        result.Should().NotBeNull();
        result!.IsNextQuestReputationLocked.Should().BeTrue();
        result.NextQuestReputationRequired.Should().Be(25);
        result.PlayerReputation.Should().Be(10);
    }

    [Test]
    public void GetChainSummary_NextQuestReputationMet_NotLocked()
    {
        // Arrange — player has 30 rep, next quest (quest_ib_2) requires 25
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 30));
        var completed = new HashSet<string> { "quest_ib_1" };

        // Act
        var result = _service.GetChainSummary("ironbane_initiation", _player, completed);

        // Assert
        result.Should().NotBeNull();
        result!.IsNextQuestReputationLocked.Should().BeFalse();
        result.NextQuestReputationRequired.Should().Be(25);
        result.PlayerReputation.Should().Be(30);
    }

    [Test]
    public void GetChainSummary_FirstQuestNoRepRequirement_NotLocked()
    {
        // Arrange — next quest is quest_ib_1 which has MinimumReputation = 0

        // Act
        var result = _service.GetChainSummary("ironbane_initiation", _player, new HashSet<string>());

        // Assert
        result.Should().NotBeNull();
        result!.IsNextQuestReputationLocked.Should().BeFalse();
        result.NextQuestReputationRequired.Should().BeNull();
    }

    [Test]
    public void GetChainSummary_NullChainId_ReturnsNull()
    {
        // Act
        var result = _service.GetChainSummary(null!, _player, new HashSet<string>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetChainSummary_EmptyChainId_ReturnsNull()
    {
        // Act
        var result = _service.GetChainSummary("", _player, new HashSet<string>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetChainSummary_UnknownChainId_ReturnsNull()
    {
        // Act
        var result = _service.GetChainSummary("nonexistent_chain", _player, new HashSet<string>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetChainSummary_NullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.GetChainSummary("ironbane_initiation", null!, new HashSet<string>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetChainSummary_NullCompletedSet_TreatedAsEmpty()
    {
        // Act — should not throw
        var result = _service.GetChainSummary("ironbane_initiation", _player, null!);

        // Assert
        result.Should().NotBeNull();
        result!.CompletedQuests.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllChainSummaries TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetAllChainSummaries_SingleChain_ReturnsOneSummary()
    {
        // Act
        var result = _service.GetAllChainSummaries(_player, new HashSet<string>());

        // Assert — standard setup has 1 chain (ironbane_initiation)
        result.Should().HaveCount(1);
        result[0].ChainId.Should().Be("ironbane_initiation");
    }

    [Test]
    public void GetAllChainSummaries_MultipleChains_ReturnsAllSummaries()
    {
        // Arrange — add a second chain (God-Sleeper Cultists)
        _allQuests.Add(CreateQuest("quest_gs_1", "Awaken the Sleeper", "god-sleeper-cultists", "godsleeper_awakening", chainOrder: 1, minRep: 0));
        _allQuests.Add(CreateQuest("quest_gs_2", "Dream Walk", "god-sleeper-cultists", "godsleeper_awakening", chainOrder: 2, minRep: 25));
        ConfigureProvider(_allQuests);

        _mockRepService
            .Setup(r => r.GetFactionName("god-sleeper-cultists"))
            .Returns("God-Sleeper Cultists");

        // Act
        var result = _service.GetAllChainSummaries(_player, new HashSet<string>());

        // Assert
        result.Should().HaveCount(2);
        result.Select(s => s.ChainId).Should().Contain("ironbane_initiation");
        result.Select(s => s.ChainId).Should().Contain("godsleeper_awakening");
    }

    [Test]
    public void GetAllChainSummaries_NoChains_ReturnsEmpty()
    {
        // Arrange — only quests with no ChainId
        var unchainedQuests = new List<QuestDefinitionDto>
        {
            CreateQuest("quest_solo", "Solo Quest", null, null, minRep: 0)
        };
        ConfigureProvider(unchainedQuests);

        // Act
        var result = _service.GetAllChainSummaries(_player, new HashSet<string>());

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetAllChainSummaries_NullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.GetAllChainSummaries(null!, new HashSet<string>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetAllChainSummaries_NullCompletedSet_TreatedAsEmpty()
    {
        // Act — should not throw
        var result = _service.GetAllChainSummaries(_player, null!);

        // Assert
        result.Should().HaveCount(1);
        result[0].CompletedQuests.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // IsQuestAvailable TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsQuestAvailable_NullQuest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.IsQuestAvailable(null!, _player, 5, new HashSet<string>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void IsQuestAvailable_NullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.IsQuestAvailable(_allQuests[0], null!, 5, new HashSet<string>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void IsQuestAvailable_AllRequirementsMet_ReturnsTrue()
    {
        // Arrange — quest_ib_1: no legend req, no prerequisites, no rep req
        // Act
        var result = _service.IsQuestAvailable(_allQuests[0], _player, 5, new HashSet<string>());

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsQuestAvailable_LegendLevelTooLow_ReturnsFalse()
    {
        // Arrange — quest requires legend level 10
        var questHighLevel = CreateQuest("quest_high", "High Level Quest", null, null, minRep: 0, minLegend: 10);

        // Act — player is legend 5
        var result = _service.IsQuestAvailable(questHighLevel, _player, 5, new HashSet<string>());

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsQuestAvailable_LegendLevelExactly_ReturnsTrue()
    {
        // Arrange — quest requires exactly legend 5
        var questLevel5 = CreateQuest("quest_l5", "Level 5 Quest", null, null, minRep: 0, minLegend: 5);

        // Act
        var result = _service.IsQuestAvailable(questLevel5, _player, 5, new HashSet<string>());

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsQuestAvailable_PrerequisiteNotMet_ReturnsFalse()
    {
        // Arrange — quest requires quest_ib_1 completed
        var questWithPrereq = CreateQuest("quest_prereq", "Prereq Quest", null, null, minRep: 0,
            prerequisites: new[] { "quest_ib_1" });

        // Act — empty completed set
        var result = _service.IsQuestAvailable(questWithPrereq, _player, 5, new HashSet<string>());

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsQuestAvailable_PrerequisiteMet_ReturnsTrue()
    {
        // Arrange
        var questWithPrereq = CreateQuest("quest_prereq", "Prereq Quest", null, null, minRep: 0,
            prerequisites: new[] { "quest_ib_1" });

        // Act — quest_ib_1 is completed
        var result = _service.IsQuestAvailable(questWithPrereq, _player, 5, new HashSet<string> { "quest_ib_1" });

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsQuestAvailable_MultiplePrerequisites_AllMustBeMet()
    {
        // Arrange — requires both quest_ib_1 AND quest_ib_2
        var questMultiPrereq = CreateQuest("quest_multi", "Multi Prereq Quest", null, null, minRep: 0,
            prerequisites: new[] { "quest_ib_1", "quest_ib_2" });

        // Act — only one prerequisite met
        var result = _service.IsQuestAvailable(questMultiPrereq, _player, 5, new HashSet<string> { "quest_ib_1" });

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsQuestAvailable_MultiplePrerequisitesAllMet_ReturnsTrue()
    {
        // Arrange
        var questMultiPrereq = CreateQuest("quest_multi", "Multi Prereq Quest", null, null, minRep: 0,
            prerequisites: new[] { "quest_ib_1", "quest_ib_2" });

        // Act
        var result = _service.IsQuestAvailable(questMultiPrereq, _player, 5,
            new HashSet<string> { "quest_ib_1", "quest_ib_2" });

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsQuestAvailable_ReputationTooLow_ReturnsFalse()
    {
        // Arrange — quest requires 50 rep with iron-banes
        // Player has only 10 rep
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 10));

        // Act
        var result = _service.IsQuestAvailable(_allQuests[2], _player, 5, new HashSet<string>());

        // Assert — player rep 10 < required 50
        result.Should().BeFalse();
    }

    [Test]
    public void IsQuestAvailable_ReputationMet_ReturnsTrue()
    {
        // Arrange — quest requires 50 rep, player has 55
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 55));

        // Act
        var result = _service.IsQuestAvailable(_allQuests[2], _player, 5, new HashSet<string>());

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsQuestAvailable_NullCompletedSet_TreatedAsEmpty()
    {
        // Arrange — quest with no prerequisites or rep requirement
        // Act — null completed set
        var result = _service.IsQuestAvailable(_allQuests[0], _player, 5, null!);

        // Assert — should not throw, treats null as empty
        result.Should().BeTrue();
    }

    [Test]
    public void IsQuestAvailable_LegendAndRepBothFail_ReturnsFalse()
    {
        // Arrange — quest requires legend 10 AND 50 rep
        var hardQuest = CreateQuest("quest_hard", "Hard Quest", "iron-banes", null, minRep: 50, minLegend: 10);
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 10));

        // Act — player is legend 5 with 10 rep (both fail)
        var result = _service.IsQuestAvailable(hardQuest, _player, 5, new HashSet<string>());

        // Assert — fails on legend check (first check)
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // QuestChainSummaryDto COMPUTED PROPERTIES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void QuestChainSummaryDto_IsComplete_TrueWhenAllDone()
    {
        // Arrange
        var dto = new QuestChainSummaryDto
        {
            TotalQuests = 3,
            CompletedQuests = 3
        };

        // Assert
        dto.IsComplete.Should().BeTrue();
    }

    [Test]
    public void QuestChainSummaryDto_IsComplete_FalseWhenPartial()
    {
        // Arrange
        var dto = new QuestChainSummaryDto
        {
            TotalQuests = 3,
            CompletedQuests = 2
        };

        // Assert
        dto.IsComplete.Should().BeFalse();
    }

    [Test]
    public void QuestChainSummaryDto_CompletionPercent_CalculatesCorrectly()
    {
        // 2 of 3 = 66%
        new QuestChainSummaryDto { TotalQuests = 3, CompletedQuests = 2 }
            .CompletionPercent.Should().Be(66);

        // 1 of 3 = 33%
        new QuestChainSummaryDto { TotalQuests = 3, CompletedQuests = 1 }
            .CompletionPercent.Should().Be(33);

        // 0 of 3 = 0%
        new QuestChainSummaryDto { TotalQuests = 3, CompletedQuests = 0 }
            .CompletionPercent.Should().Be(0);

        // 3 of 3 = 100%
        new QuestChainSummaryDto { TotalQuests = 3, CompletedQuests = 3 }
            .CompletionPercent.Should().Be(100);
    }

    [Test]
    public void QuestChainSummaryDto_CompletionPercent_ZeroTotal_ReturnsZero()
    {
        // Edge case: empty chain
        new QuestChainSummaryDto { TotalQuests = 0, CompletedQuests = 0 }
            .CompletionPercent.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTEGRATION SCENARIO TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Scenario_FullChainProgression_TracksSummaryCorrectly()
    {
        // Simulates a player progressing through the Iron-Banes chain, checking summary at each step

        // Step 1: Start — no quests completed, no reputation
        var completed = new HashSet<string>();
        var summary = _service.GetChainSummary("ironbane_initiation", _player, completed);
        summary.Should().NotBeNull();
        summary!.CompletedQuests.Should().Be(0);
        summary.NextQuestId.Should().Be("quest_ib_1");
        summary.IsNextQuestReputationLocked.Should().BeFalse();

        // Step 2: Complete quest 1, gain some rep
        completed.Add("quest_ib_1");
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 15));
        summary = _service.GetChainSummary("ironbane_initiation", _player, completed);
        summary!.CompletedQuests.Should().Be(1);
        summary.NextQuestId.Should().Be("quest_ib_2");
        summary.IsNextQuestReputationLocked.Should().BeTrue(); // 15 < 25 required
        summary.PlayerReputation.Should().Be(15);

        // Step 3: Gain more rep, now meets quest 2 threshold
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 30));
        summary = _service.GetChainSummary("ironbane_initiation", _player, completed);
        summary!.IsNextQuestReputationLocked.Should().BeFalse(); // 30 >= 25

        // Step 4: Complete quest 2
        completed.Add("quest_ib_2");
        summary = _service.GetChainSummary("ironbane_initiation", _player, completed);
        summary!.CompletedQuests.Should().Be(2);
        summary.CompletionPercent.Should().Be(66);
        summary.NextQuestId.Should().Be("quest_ib_3");
        summary.IsNextQuestReputationLocked.Should().BeTrue(); // 30 < 50 required

        // Step 5: Max rep, complete quest 3
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 75));
        completed.Add("quest_ib_3");
        summary = _service.GetChainSummary("ironbane_initiation", _player, completed);
        summary!.CompletedQuests.Should().Be(3);
        summary.IsComplete.Should().BeTrue();
        summary.CompletionPercent.Should().Be(100);
        summary.NextQuestId.Should().BeNull();
    }

    [Test]
    public void Scenario_MultiFactionChains_IndependentTracking()
    {
        // Arrange — add a second faction chain
        _allQuests.Add(CreateQuest("quest_gs_1", "Dark Ritual", "god-sleeper-cultists", "godsleeper_ritual", chainOrder: 1, minRep: 0));
        _allQuests.Add(CreateQuest("quest_gs_2", "Nightmare Pact", "god-sleeper-cultists", "godsleeper_ritual", chainOrder: 2, minRep: 25));
        ConfigureProvider(_allQuests);

        _mockRepService
            .Setup(r => r.GetFactionName("god-sleeper-cultists"))
            .Returns("God-Sleeper Cultists");

        // Complete 1 Iron-Banes quest, 0 God-Sleeper quests
        var completed = new HashSet<string> { "quest_ib_1" };
        _player.SetFactionReputation(FactionReputation.Create("iron-banes", 30));

        // Act
        var summaries = _service.GetAllChainSummaries(_player, completed);

        // Assert — each chain tracked independently
        var ibSummary = summaries.First(s => s.ChainId == "ironbane_initiation");
        var gsSummary = summaries.First(s => s.ChainId == "godsleeper_ritual");

        ibSummary.CompletedQuests.Should().Be(1);
        ibSummary.TotalQuests.Should().Be(3);

        gsSummary.CompletedQuests.Should().Be(0);
        gsSummary.TotalQuests.Should().Be(2);
        gsSummary.NextQuestId.Should().Be("quest_gs_1");
    }

    [Test]
    public void Scenario_ChainWithNoFaction_NoRepLocking()
    {
        // Arrange — a chain with no faction affiliation (exploration chain)
        var explorationChain = new List<QuestDefinitionDto>
        {
            CreateQuest("quest_exp_1", "Map the Depths", null, "exploration_depths", chainOrder: 1, minRep: 0),
            CreateQuest("quest_exp_2", "Find the Exit", null, "exploration_depths", chainOrder: 2, minRep: 0),
        };
        _allQuests.AddRange(explorationChain);
        ConfigureProvider(_allQuests);

        // Act
        var summary = _service.GetChainSummary("exploration_depths", _player, new HashSet<string>());

        // Assert — no faction means no rep locking
        summary.Should().NotBeNull();
        summary!.FactionId.Should().BeEmpty();
        summary.IsNextQuestReputationLocked.Should().BeFalse();
        summary.NextQuestReputationRequired.Should().BeNull();
        summary.PlayerReputation.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Factory method for creating test <see cref="QuestDefinitionDto"/> instances.
    /// </summary>
    /// <param name="questId">Unique quest identifier.</param>
    /// <param name="name">Display name.</param>
    /// <param name="faction">Optional faction ID (null = no faction).</param>
    /// <param name="chainId">Optional chain ID (null = not in a chain).</param>
    /// <param name="chainOrder">Order within chain (null = unordered).</param>
    /// <param name="minRep">Minimum faction reputation to accept quest (0 = no gate).</param>
    /// <param name="minLegend">Minimum legend level (0 = no requirement).</param>
    /// <param name="prerequisites">Array of prerequisite quest IDs.</param>
    /// <returns>A new QuestDefinitionDto configured for testing.</returns>
    private static QuestDefinitionDto CreateQuest(
        string questId,
        string name,
        string? faction,
        string? chainId,
        int? chainOrder = null,
        int minRep = 0,
        int minLegend = 0,
        string[]? prerequisites = null)
    {
        return new QuestDefinitionDto
        {
            QuestId = questId,
            Name = name,
            Faction = faction,
            ChainId = chainId,
            ChainOrder = chainOrder,
            MinimumReputation = minRep,
            MinimumLegend = minLegend,
            PrerequisiteQuestIds = prerequisites ?? [],
            Category = "Faction",
            Type = "Faction"
        };
    }

    /// <summary>
    /// Configures the mock <see cref="IQuestDefinitionProvider"/> to return the given quest list
    /// for both <c>GetAllDefinitions()</c> and <c>GetAvailableQuests()</c> calls.
    /// </summary>
    /// <param name="quests">The quest list to return from the mock.</param>
    /// <remarks>
    /// <c>GetAvailableQuests</c> is set up as a pass-through (returns all quests) since
    /// the service's reputation filtering is what we're testing on top of that.
    /// </remarks>
    private void ConfigureProvider(List<QuestDefinitionDto> quests)
    {
        _mockQuestProvider
            .Setup(p => p.GetAllDefinitions())
            .Returns(quests);

        // Default: GetAvailableQuests returns all quests (reputation filtering is the SUT's job)
        _mockQuestProvider
            .Setup(p => p.GetAvailableQuests(It.IsAny<int>(), It.IsAny<IReadOnlySet<string>>()))
            .Returns(quests);
    }
}
