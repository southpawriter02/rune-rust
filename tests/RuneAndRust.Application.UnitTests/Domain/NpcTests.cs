using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Entities;

/// <summary>
/// Comprehensive unit tests for the Npc domain entity.
/// Tests cover construction, validation, property initialization, state management,
/// interaction methods, quest management, and factory methods.
/// Naming convention: [MethodName]_[Scenario]_[ExpectedResult]
/// </summary>
[TestFixture]
public class NpcTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES & HELPERS
    // ═══════════════════════════════════════════════════════════════

    private const string ValidDefinitionId = "thorvald_guard";
    private const string ValidName = "Thorvald the Guard";
    private const string ValidDescription = "A seasoned warrior guarding the hold.";
    private const string ValidInitialGreeting = "Hail, traveler! What brings you here?";
    private const NpcArchetype ValidArchetype = NpcArchetype.Guard;

    /// <summary>
    /// Creates a valid Npc instance for testing with default values.
    /// </summary>
    private Npc CreateValidNpc(
        string definitionId = ValidDefinitionId,
        string name = ValidName,
        string description = ValidDescription,
        string initialGreeting = ValidInitialGreeting,
        NpcArchetype archetype = ValidArchetype,
        int baseDisposition = 0)
    {
        return new Npc(definitionId, name, description, initialGreeting, archetype, baseDisposition);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullDefinitionId_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(null!, ValidName, ValidDescription, ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithEmptyDefinitionId_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(string.Empty, ValidName, ValidDescription, ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithWhitespaceDefinitionId_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc("   ", ValidName, ValidDescription, ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, null!, ValidDescription, ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, string.Empty, ValidDescription, ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, "   ", ValidDescription, ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullDescription_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, ValidName, null!, ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithEmptyDescription_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, ValidName, string.Empty, ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithWhitespaceDescription_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, ValidName, "   ", ValidInitialGreeting, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullInitialGreeting_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, ValidName, ValidDescription, null!, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithEmptyInitialGreeting_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, ValidName, ValidDescription, string.Empty, ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithWhitespaceInitialGreeting_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => new Npc(ValidDefinitionId, ValidName, ValidDescription, "   ", ValidArchetype);
        action.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR PROPERTY INITIALIZATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithValidParameters_SetsIdToNewGuid()
    {
        // Act
        var npc = CreateValidNpc();

        // Assert
        npc.Id.Should().NotBeEmpty();
        npc.Id.GetType().Should().Be(typeof(Guid));
    }

    [Test]
    public void Constructor_MultipleInstances_HaveDifferentIds()
    {
        // Act
        var npc1 = CreateValidNpc();
        var npc2 = CreateValidNpc();

        // Assert
        npc1.Id.Should().NotBe(npc2.Id);
    }

    [Test]
    public void Constructor_WithMixedCaseDefinitionId_LowercasesDefinitionId()
    {
        // Act
        var npc = CreateValidNpc(definitionId: "ThorvAld_GuARd");

        // Assert
        npc.DefinitionId.Should().Be("thorvald_guard");
    }

    [Test]
    public void Constructor_WithUppercaseDefinitionId_LowercasesDefinitionId()
    {
        // Act
        var npc = CreateValidNpc(definitionId: "THORVALD_GUARD");

        // Assert
        npc.DefinitionId.Should().Be("thorvald_guard");
    }

    [Test]
    public void Constructor_WithValidParameters_SetsNameProperty()
    {
        // Act
        var npc = CreateValidNpc(name: "Astrid the Sage");

        // Assert
        npc.Name.Should().Be("Astrid the Sage");
    }

    [Test]
    public void Constructor_WithValidParameters_SetsDescriptionProperty()
    {
        // Act
        var npc = CreateValidNpc(description: "A wise scholar");

        // Assert
        npc.Description.Should().Be("A wise scholar");
    }

    [Test]
    public void Constructor_WithValidParameters_SetsInitialGreetingProperty()
    {
        // Act
        var npc = CreateValidNpc(initialGreeting: "Welcome, friend!");

        // Assert
        npc.InitialGreeting.Should().Be("Welcome, friend!");
    }

    [Test]
    public void Constructor_WithValidParameters_SetsArchetypeProperty()
    {
        // Act
        var npc = CreateValidNpc(archetype: NpcArchetype.Scholar);

        // Assert
        npc.Archetype.Should().Be(NpcArchetype.Scholar);
    }

    [Test]
    public void Constructor_WithPositiveBaseDisposition_ClampsAtMaximum()
    {
        // Act
        var npc = CreateValidNpc(baseDisposition: 150);

        // Assert
        npc.BaseDisposition.Should().Be(100);
    }

    [Test]
    public void Constructor_WithNegativeBaseDisposition_ClampsAtMinimum()
    {
        // Act
        var npc = CreateValidNpc(baseDisposition: -150);

        // Assert
        npc.BaseDisposition.Should().Be(-100);
    }

    [Test]
    public void Constructor_WithBaseDispositionAt100_Succeeds()
    {
        // Act
        var npc = CreateValidNpc(baseDisposition: 100);

        // Assert
        npc.BaseDisposition.Should().Be(100);
    }

    [Test]
    public void Constructor_WithBaseDispositionAtNegative100_Succeeds()
    {
        // Act
        var npc = CreateValidNpc(baseDisposition: -100);

        // Assert
        npc.BaseDisposition.Should().Be(-100);
    }

    [Test]
    public void Constructor_WithBaseDispositionZero_Succeeds()
    {
        // Act
        var npc = CreateValidNpc(baseDisposition: 0);

        // Assert
        npc.BaseDisposition.Should().Be(0);
    }

    [Test]
    public void Constructor_SetCurrentDispositionEqualToBaseDisposition()
    {
        // Act
        var npc = CreateValidNpc(baseDisposition: 42);

        // Assert
        npc.CurrentDisposition.Should().Be(42);
    }

    [Test]
    public void Constructor_WithBaseDispositionNegative_CurrentDispositionMatchesBase()
    {
        // Act
        var npc = CreateValidNpc(baseDisposition: -50);

        // Assert
        npc.CurrentDisposition.Should().Be(-50);
    }

    // ═══════════════════════════════════════════════════════════════
    // DEFAULT STATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_IsAliveDefaultsToTrue()
    {
        // Act
        var npc = CreateValidNpc();

        // Assert
        npc.IsAlive.Should().BeTrue();
    }

    [Test]
    public void Constructor_HasBeenMetDefaultsToFalse()
    {
        // Act
        var npc = CreateValidNpc();

        // Assert
        npc.HasBeenMet.Should().BeFalse();
    }

    [Test]
    public void Constructor_RootDialogueIdIsNullByDefault()
    {
        // Act
        var npc = CreateValidNpc();

        // Assert
        npc.RootDialogueId.Should().BeNull();
    }

    [Test]
    public void Constructor_FactionIsNullByDefault()
    {
        // Act
        var npc = CreateValidNpc();

        // Assert
        npc.Faction.Should().BeNull();
    }

    [Test]
    public void Constructor_IsMerchantDefaultsToFalse()
    {
        // Act
        var npc = CreateValidNpc();

        // Assert
        npc.IsMerchant.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void HasDialogue_WhenRootDialogueIdIsNull_ReturnsFalse()
    {
        // Act
        var npc = CreateValidNpc();

        // Assert
        npc.HasDialogue.Should().BeFalse();
    }

    [Test]
    public void HasDialogue_WhenRootDialogueIdIsSet_ReturnsTrue()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.SetRootDialogue("dialogue-tree-1");

        // Assert
        npc.HasDialogue.Should().BeTrue();
    }

    [Test]
    public void CanGiveQuests_WhenNoQuestsAdded_ReturnsFalse()
    {
        // Act
        var npc = CreateValidNpc();

        // Assert
        npc.CanGiveQuests.Should().BeFalse();
    }

    [Test]
    public void CanGiveQuests_WhenQuestsAdded_ReturnsTrue()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.AddAvailableQuest("quest-1");

        // Assert
        npc.CanGiveQuests.Should().BeTrue();
    }

    [Test]
    public void CanGiveQuests_WhenQuestRemoved_ReturnsFalse()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.AddAvailableQuest("quest-1");

        // Act
        npc.RemoveAvailableQuest("quest-1");

        // Assert
        npc.CanGiveQuests.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // MEET INTERACTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Meet_SetsHasBeenMetToTrue()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.Meet();

        // Assert
        npc.HasBeenMet.Should().BeTrue();
    }

    [Test]
    public void Meet_CalledMultipleTimes_HasBeenMetRemainsTrue()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.Meet();

        // Act
        npc.Meet();

        // Assert
        npc.HasBeenMet.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // CURRENT GREETING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CurrentGreeting_WhenNotMet_ReturnsInitialGreeting()
    {
        // Act
        var npc = CreateValidNpc(initialGreeting: "Greetings, stranger!");

        // Assert
        npc.CurrentGreeting.Should().Be("Greetings, stranger!");
    }

    [Test]
    public void CurrentGreeting_WhenMet_ReturnsRecognitionGreeting()
    {
        // Arrange
        var npc = CreateValidNpc(name: "Thorvald");
        npc.Meet();

        // Act
        var greeting = npc.CurrentGreeting;

        // Assert
        greeting.Should().Be("Thorvald nods in recognition.");
    }

    [Test]
    public void CurrentGreeting_ReturnsNameInRecognitionGreeting()
    {
        // Arrange
        var npc = CreateValidNpc(name: "Astrid the Sage");
        npc.Meet();

        // Act
        var greeting = npc.CurrentGreeting;

        // Assert
        greeting.Should().Contain("Astrid the Sage");
    }

    [Test]
    public void CurrentGreeting_WithLongName_IncludesFullNameInRecognition()
    {
        // Arrange
        var npc = CreateValidNpc(name: "Bjorn the Exile of Mirkwood");
        npc.Meet();

        // Act
        var greeting = npc.CurrentGreeting;

        // Assert
        greeting.Should().Be("Bjorn the Exile of Mirkwood nods in recognition.");
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPOSITION ADJUSTMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AdjustDisposition_PositiveAmount_IncreasesCurrentDisposition()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 0);

        // Act
        npc.AdjustDisposition(25);

        // Assert
        npc.CurrentDisposition.Should().Be(25);
    }

    [Test]
    public void AdjustDisposition_NegativeAmount_DecreasesCurrentDisposition()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 50);

        // Act
        npc.AdjustDisposition(-30);

        // Assert
        npc.CurrentDisposition.Should().Be(20);
    }

    [Test]
    public void AdjustDisposition_PositiveExceedingMaximum_ClampsAt100()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 90);

        // Act
        npc.AdjustDisposition(50);

        // Assert
        npc.CurrentDisposition.Should().Be(100);
    }

    [Test]
    public void AdjustDisposition_NegativeExceedingMinimum_ClampsAtNegative100()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: -80);

        // Act
        npc.AdjustDisposition(-50);

        // Assert
        npc.CurrentDisposition.Should().Be(-100);
    }

    [Test]
    public void AdjustDisposition_PushingToMaximum_SucceedsAt100()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 0);

        // Act
        npc.AdjustDisposition(100);

        // Assert
        npc.CurrentDisposition.Should().Be(100);
    }

    [Test]
    public void AdjustDisposition_PushingToMinimum_SucceedsAtNegative100()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 0);

        // Act
        npc.AdjustDisposition(-100);

        // Assert
        npc.CurrentDisposition.Should().Be(-100);
    }

    [Test]
    public void AdjustDisposition_MultipleAdjustments_CumulativelyModifies()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 0);

        // Act
        npc.AdjustDisposition(20);
        npc.AdjustDisposition(15);
        npc.AdjustDisposition(-10);

        // Assert
        npc.CurrentDisposition.Should().Be(25);
    }

    [Test]
    public void AdjustDisposition_ZeroAmount_NoChange()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 50);

        // Act
        npc.AdjustDisposition(0);

        // Assert
        npc.CurrentDisposition.Should().Be(50);
    }

    // ═══════════════════════════════════════════════════════════════
    // KILL INTERACTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Kill_SetsIsAliveToFalse()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.Kill();

        // Assert
        npc.IsAlive.Should().BeFalse();
    }

    [Test]
    public void Kill_CalledMultipleTimes_IsAliveRemainsFalse()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.Kill();

        // Act
        npc.Kill();

        // Assert
        npc.IsAlive.Should().BeFalse();
    }

    [Test]
    public void Kill_DispositionUnaffected()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 50);

        // Act
        npc.Kill();

        // Assert
        npc.CurrentDisposition.Should().Be(50);
    }

    // ═══════════════════════════════════════════════════════════════
    // QUEST MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AddAvailableQuest_WithValidQuestId_AddsQuest()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.AddAvailableQuest("slay-trolls");

        // Assert
        var quests = npc.GetAvailableQuestIds();
        quests.Should().Contain("slay-trolls");
    }

    [Test]
    public void AddAvailableQuest_WithNullQuestId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.AddAvailableQuest(null!);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddAvailableQuest_WithEmptyQuestId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.AddAvailableQuest(string.Empty);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddAvailableQuest_WithWhitespaceQuestId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.AddAvailableQuest("   ");
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddAvailableQuest_WithDuplicateQuestId_IgnoresDuplicate()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.AddAvailableQuest("slay-trolls");
        npc.AddAvailableQuest("slay-trolls");

        // Assert
        var quests = npc.GetAvailableQuestIds();
        quests.Should().HaveCount(1);
        quests.Should().Contain("slay-trolls");
    }

    [Test]
    public void AddAvailableQuest_WithMixedCaseQuestId_Lowercases()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.AddAvailableQuest("Slay-Trolls");

        // Assert
        var quests = npc.GetAvailableQuestIds();
        quests.Should().Contain("slay-trolls");
    }

    [Test]
    public void AddAvailableQuest_MultipleDistinctQuests_AllAdded()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.AddAvailableQuest("quest-1");
        npc.AddAvailableQuest("quest-2");
        npc.AddAvailableQuest("quest-3");

        // Assert
        var quests = npc.GetAvailableQuestIds();
        quests.Should().HaveCount(3);
        quests.Should().Contain("quest-1");
        quests.Should().Contain("quest-2");
        quests.Should().Contain("quest-3");
    }

    [Test]
    public void RemoveAvailableQuest_WithValidQuestId_RemovesQuest()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.AddAvailableQuest("slay-trolls");

        // Act
        var removed = npc.RemoveAvailableQuest("slay-trolls");

        // Assert
        removed.Should().BeTrue();
        var quests = npc.GetAvailableQuestIds();
        quests.Should().NotContain("slay-trolls");
    }

    [Test]
    public void RemoveAvailableQuest_WithNonexistentQuestId_ReturnsFalse()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        var removed = npc.RemoveAvailableQuest("nonexistent-quest");

        // Assert
        removed.Should().BeFalse();
    }

    [Test]
    public void RemoveAvailableQuest_WithNullQuestId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.RemoveAvailableQuest(null!);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RemoveAvailableQuest_WithEmptyQuestId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.RemoveAvailableQuest(string.Empty);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RemoveAvailableQuest_WithWhitespaceQuestId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.RemoveAvailableQuest("   ");
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RemoveAvailableQuest_WithMixedCaseQuestId_RemovesCorrectly()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.AddAvailableQuest("slay-trolls");

        // Act
        var removed = npc.RemoveAvailableQuest("Slay-Trolls");

        // Assert
        removed.Should().BeTrue();
    }

    [Test]
    public void RemoveAvailableQuest_FromMultipleQuests_RemovesOnlyTarget()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.AddAvailableQuest("quest-1");
        npc.AddAvailableQuest("quest-2");
        npc.AddAvailableQuest("quest-3");

        // Act
        var removed = npc.RemoveAvailableQuest("quest-2");

        // Assert
        removed.Should().BeTrue();
        var quests = npc.GetAvailableQuestIds();
        quests.Should().HaveCount(2);
        quests.Should().Contain("quest-1");
        quests.Should().Contain("quest-3");
        quests.Should().NotContain("quest-2");
    }

    [Test]
    public void GetAvailableQuestIds_ReturnsReadOnlyList()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.AddAvailableQuest("quest-1");

        // Act
        var quests = npc.GetAvailableQuestIds();

        // Assert
        quests.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Test]
    public void GetAvailableQuestIds_EmptyNpc_ReturnsEmptyList()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        var quests = npc.GetAvailableQuestIds();

        // Assert
        quests.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // DIALOGUE CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetRootDialogue_WithValidDialogueId_SetsRootDialogueId()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.SetRootDialogue("dialogue-tree-1");

        // Assert
        npc.RootDialogueId.Should().Be("dialogue-tree-1");
    }

    [Test]
    public void SetRootDialogue_WithNullDialogueId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.SetRootDialogue(null!);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void SetRootDialogue_WithEmptyDialogueId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.SetRootDialogue(string.Empty);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void SetRootDialogue_WithWhitespaceDialogueId_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.SetRootDialogue("   ");
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void SetRootDialogue_Overwrites_PreviousDialogueId()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.SetRootDialogue("dialogue-1");

        // Act
        npc.SetRootDialogue("dialogue-2");

        // Assert
        npc.RootDialogueId.Should().Be("dialogue-2");
    }

    [Test]
    public void SetRootDialogue_EnablesHasDialogue()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.HasDialogue.Should().BeFalse();

        // Act
        npc.SetRootDialogue("dialogue-tree-1");

        // Assert
        npc.HasDialogue.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTION CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetFaction_WithValidFaction_SetsFaction()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.SetFaction("IronBanes");

        // Assert
        npc.Faction.Should().Be("IronBanes");
    }

    [Test]
    public void SetFaction_WithNullFaction_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.SetFaction(null!);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void SetFaction_WithEmptyFaction_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.SetFaction(string.Empty);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void SetFaction_WithWhitespaceFaction_ThrowsArgumentException()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act & Assert
        var action = () => npc.SetFaction("   ");
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void SetFaction_Overwrites_PreviousFaction()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.SetFaction("IronBanes");

        // Act
        npc.SetFaction("MidgardCombine");

        // Assert
        npc.Faction.Should().Be("MidgardCombine");
    }

    // ═══════════════════════════════════════════════════════════════
    // MERCHANT CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetMerchant_WithTrue_SetsIsMerchantTrue()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.SetMerchant(true);

        // Assert
        npc.IsMerchant.Should().BeTrue();
    }

    [Test]
    public void SetMerchant_WithFalse_SetsIsMerchantFalse()
    {
        // Arrange
        var npc = CreateValidNpc();
        npc.SetMerchant(true);

        // Act
        npc.SetMerchant(false);

        // Assert
        npc.IsMerchant.Should().BeFalse();
    }

    [Test]
    public void SetMerchant_CanToggleMultipleTimes()
    {
        // Arrange
        var npc = CreateValidNpc();

        // Act
        npc.SetMerchant(true);
        npc.SetMerchant(false);
        npc.SetMerchant(true);

        // Assert
        npc.IsMerchant.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithMinimalParameters_CreatesValidInstance()
    {
        // Act
        var npc = Npc.Create(
            ValidDefinitionId,
            ValidName,
            ValidDescription,
            ValidInitialGreeting,
            ValidArchetype);

        // Assert
        npc.Should().NotBeNull();
        npc.DefinitionId.Should().Be("thorvald_guard");
        npc.Name.Should().Be(ValidName);
        npc.Description.Should().Be(ValidDescription);
        npc.InitialGreeting.Should().Be(ValidInitialGreeting);
        npc.Archetype.Should().Be(ValidArchetype);
    }

    [Test]
    public void Create_WithAllParameters_CreatesFullyConfiguredInstance()
    {
        // Act
        var npc = Npc.Create(
            "merchant-kjartan",
            "Kjartan the Merchant",
            "A wealthy trader",
            "Welcome to my wares!",
            NpcArchetype.Merchant,
            baseDisposition: 25,
            faction: "MidgardCombine",
            rootDialogueId: "dialogue-merchant-greeting",
            isMerchant: true,
            questIds: new[] { "trade-quest-1", "trade-quest-2" });

        // Assert
        npc.DefinitionId.Should().Be("merchant-kjartan");
        npc.Name.Should().Be("Kjartan the Merchant");
        npc.BaseDisposition.Should().Be(25);
        npc.CurrentDisposition.Should().Be(25);
        npc.Faction.Should().Be("MidgardCombine");
        npc.RootDialogueId.Should().Be("dialogue-merchant-greeting");
        npc.IsMerchant.Should().BeTrue();
        var quests = npc.GetAvailableQuestIds();
        quests.Should().HaveCount(2);
        quests.Should().Contain("trade-quest-1");
        quests.Should().Contain("trade-quest-2");
    }

    [Test]
    public void Create_WithOptionalParametersAsNull_CreatesInstance()
    {
        // Act
        var npc = Npc.Create(
            ValidDefinitionId,
            ValidName,
            ValidDescription,
            ValidInitialGreeting,
            ValidArchetype,
            faction: null,
            rootDialogueId: null,
            questIds: null);

        // Assert
        npc.Faction.Should().BeNull();
        npc.RootDialogueId.Should().BeNull();
        npc.CanGiveQuests.Should().BeFalse();
    }

    [Test]
    public void Create_WithBaseDisposition_FactoryClamps()
    {
        // Act
        var npc = Npc.Create(
            ValidDefinitionId,
            ValidName,
            ValidDescription,
            ValidInitialGreeting,
            ValidArchetype,
            baseDisposition: 150);

        // Assert
        npc.BaseDisposition.Should().Be(100);
        npc.CurrentDisposition.Should().Be(100);
    }

    [Test]
    public void Create_WithMixedCaseDefinitionId_Lowercases()
    {
        // Act
        var npc = Npc.Create(
            "MiXeD_CaSe_ID",
            ValidName,
            ValidDescription,
            ValidInitialGreeting,
            ValidArchetype);

        // Assert
        npc.DefinitionId.Should().Be("mixed_case_id");
    }

    [Test]
    public void Create_WithEmptyQuestIds_CreatesInstanceWithoutQuests()
    {
        // Act
        var npc = Npc.Create(
            ValidDefinitionId,
            ValidName,
            ValidDescription,
            ValidInitialGreeting,
            ValidArchetype,
            questIds: Array.Empty<string>());

        // Assert
        npc.CanGiveQuests.Should().BeFalse();
    }

    [Test]
    public void Create_SetsMerchantFalseByDefault()
    {
        // Act
        var npc = Npc.Create(
            ValidDefinitionId,
            ValidName,
            ValidDescription,
            ValidInitialGreeting,
            ValidArchetype);

        // Assert
        npc.IsMerchant.Should().BeFalse();
    }

    [Test]
    public void Create_GeneratesUniqueIds()
    {
        // Act
        var npc1 = Npc.Create(
            ValidDefinitionId,
            ValidName,
            ValidDescription,
            ValidInitialGreeting,
            ValidArchetype);
        var npc2 = Npc.Create(
            ValidDefinitionId,
            ValidName,
            ValidDescription,
            ValidInitialGreeting,
            ValidArchetype);

        // Assert
        npc1.Id.Should().NotBe(npc2.Id);
    }

    [Test]
    public void Create_WithAllArchetypes_Succeeds()
    {
        // Arrange
        var archetypes = new[]
        {
            NpcArchetype.Citizen,
            NpcArchetype.Merchant,
            NpcArchetype.Guard,
            NpcArchetype.Scholar,
            NpcArchetype.Artisan,
            NpcArchetype.Exile,
            NpcArchetype.Hermit,
            NpcArchetype.Scavenger,
            NpcArchetype.Cultist
        };

        // Act & Assert
        foreach (var archetype in archetypes)
        {
            var npc = Npc.Create(
                $"npc_{archetype.ToString().ToLowerInvariant()}",
                ValidName,
                ValidDescription,
                ValidInitialGreeting,
                archetype);

            npc.Archetype.Should().Be(archetype);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // INTEGRATION & COMPLEX SCENARIO TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void MultipleOperations_CompleteWorkflow()
    {
        // Arrange
        var npc = Npc.Create(
            "thorvald-guard",
            "Thorvald the Guard",
            "A seasoned protector",
            "What brings you to the hold?",
            NpcArchetype.Guard,
            baseDisposition: 0);

        // Act
        // First meeting
        npc.Meet();
        npc.AdjustDisposition(20);
        npc.AddAvailableQuest("defeat-bandits");

        // Assert first state
        npc.HasBeenMet.Should().BeTrue();
        npc.CurrentDisposition.Should().Be(20);
        npc.CanGiveQuests.Should().BeTrue();
        npc.CurrentGreeting.Should().Contain("nods in recognition");

        // Act continued - quest accepted, disposition declines
        npc.RemoveAvailableQuest("defeat-bandits");
        npc.AdjustDisposition(-40);

        // Assert second state
        npc.CanGiveQuests.Should().BeFalse();
        npc.CurrentDisposition.Should().Be(-20);

        // Act continued - NPC is killed
        npc.Kill();

        // Assert final state
        npc.IsAlive.Should().BeFalse();
        npc.CurrentDisposition.Should().Be(-20); // Disposition unaffected by death
    }

    [Test]
    public void DispositionAdjustmentPreservesBaseDisposition()
    {
        // Arrange
        var npc = CreateValidNpc(baseDisposition: 50);

        // Act
        npc.AdjustDisposition(-100);

        // Assert
        npc.BaseDisposition.Should().Be(50); // Unchanged
        npc.CurrentDisposition.Should().Be(-50);
    }

    [Test]
    public void QuestManagementPreservesOtherProperties()
    {
        // Arrange
        var npc = CreateValidNpc(name: "Test NPC");
        npc.Meet();
        var originalDisposition = npc.CurrentDisposition;

        // Act
        npc.AddAvailableQuest("quest-1");
        npc.RemoveAvailableQuest("quest-1");

        // Assert
        npc.Name.Should().Be("Test NPC");
        npc.HasBeenMet.Should().BeTrue();
        npc.CurrentDisposition.Should().Be(originalDisposition);
    }
}
