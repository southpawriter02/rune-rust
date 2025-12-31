using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Conditions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for the Dialogue System with seeded data (v0.4.2e - The Archive).
/// Tests end-to-end dialogue flow using seeded dialogue trees, NPCs, and factions.
/// </summary>
public class DialogueIntegrationTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly DialogueRepository _dialogueRepository;
    private readonly IDialogueService _dialogueService;
    private readonly GameState _gameState;
    private readonly Mock<IFactionService> _mockFactionService;
    private readonly Mock<IEventBus> _mockEventBus;

    public DialogueIntegrationTests()
    {
        // Set up in-memory database
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new RuneAndRustDbContext(options);

        // Seed data
        FactionSeeder.SeedAsync(_context).GetAwaiter().GetResult();
        DialogueSeeder.SeedAsync(_context).GetAwaiter().GetResult();
        NpcSeeder.SeedAsync(_context).GetAwaiter().GetResult();

        // Set up repositories and services
        var dialogueRepoLogger = Mock.Of<ILogger<DialogueRepository>>();
        _dialogueRepository = new DialogueRepository(_context, dialogueRepoLogger);

        _mockFactionService = new Mock<IFactionService>();
        _mockEventBus = new Mock<IEventBus>();
        _gameState = new GameState();

        // Set up condition evaluator with mocks
        var mockInventoryService = new Mock<IInventoryService>();
        var mockSpecRepository = new Mock<ISpecializationRepository>();
        var mockDiceService = new Mock<IDiceService>();
        var evaluatorLogger = Mock.Of<ILogger<DialogueConditionEvaluator>>();

        mockSpecRepository
            .Setup(r => r.GetUnlockedNodesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<SpecializationNode>());

        var conditionEvaluator = new DialogueConditionEvaluator(
            _mockFactionService.Object,
            mockInventoryService.Object,
            mockSpecRepository.Object,
            mockDiceService.Object,
            _gameState,
            evaluatorLogger);

        // Set up effect executor with mocks
        var effectLogger = Mock.Of<ILogger<DialogueEffectExecutor>>();
        var mockItemRepository = new Mock<IItemRepository>();
        var effectExecutor = new DialogueEffectExecutor(
            _mockFactionService.Object,
            mockInventoryService.Object,
            mockItemRepository.Object,
            effectLogger);

        // Create dialogue service
        var serviceLogger = Mock.Of<ILogger<DialogueService>>();
        _dialogueService = new DialogueService(
            _dialogueRepository,
            conditionEvaluator,
            effectExecutor,
            _mockEventBus.Object,
            serviceLogger);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private Character CreateTestCharacter(
        string name = "TestChar",
        int wits = 5,
        int might = 5,
        int will = 5,
        int finesse = 5,
        int sturdiness = 5,
        int level = 1)
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = name,
            Wits = wits,
            Might = might,
            Will = will,
            Finesse = finesse,
            Sturdiness = sturdiness,
            Level = level
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Seeder Validation Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FactionSeeder_SeedsAllFourFactions()
    {
        // Assert
        var factions = await _context.Factions.ToListAsync();

        factions.Should().HaveCount(4);
        factions.Should().Contain(f => f.Type == FactionType.IronBanes);
        factions.Should().Contain(f => f.Type == FactionType.Dvergr);
        factions.Should().Contain(f => f.Type == FactionType.TheBound);
        factions.Should().Contain(f => f.Type == FactionType.TheFaceless);
    }

    [Fact]
    public async Task DialogueSeeder_SeedsTwoDialogueTrees()
    {
        // Assert
        var trees = await _context.DialogueTrees.Include(t => t.Nodes).ToListAsync();

        trees.Should().HaveCount(2);
        trees.Should().Contain(t => t.TreeId == "npc_old_scavenger");
        trees.Should().Contain(t => t.TreeId == "npc_kjartan");

        // Verify nodes are seeded
        var scavengerTree = trees.First(t => t.TreeId == "npc_old_scavenger");
        scavengerTree.Nodes.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task NpcSeeder_SeedsFiveNpcs()
    {
        // Assert
        var npcs = await _context.Npcs.ToListAsync();

        npcs.Should().HaveCount(5);
        npcs.Should().Contain(n => n.Name == "Old Scavenger");
        npcs.Should().Contain(n => n.Name == "Kjartan");
        npcs.Should().Contain(n => n.Name == "Bound Acolyte");
        npcs.Should().Contain(n => n.Name == "Masked Trader");
        npcs.Should().Contain(n => n.Name == "Scrap-Guard");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Dialogue Start Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task StartDialogue_WithOldScavenger_ReturnsGreeting()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var result = await _dialogueService.StartDialogueAsync(character, "npc_old_scavenger", _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.ViewModel.Should().NotBeNull();
        result.ViewModel!.NpcName.Should().Be("Old Scavenger");
        result.ViewModel.DialogueText.Should().Contain("wanderer");
    }

    [Fact]
    public async Task StartDialogue_WithKjartan_ReturnsGreeting()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var result = await _dialogueService.StartDialogueAsync(character, "npc_kjartan", _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.ViewModel.Should().NotBeNull();
        result.ViewModel!.NpcName.Should().Be("Kjartan");
        result.ViewModel.DialogueText.Should().Contain("Surface-dweller");
    }

    [Fact]
    public async Task StartDialogue_WithNonexistentTree_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var result = await _dialogueService.StartDialogueAsync(character, "nonexistent_tree", _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Condition-Gated Option Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task StartDialogue_WithHighWits_ShowsWitsGatedOption()
    {
        // Arrange
        var character = CreateTestCharacter(wits: 6);

        // Act
        var result = await _dialogueService.StartDialogueAsync(character, "npc_old_scavenger", _gameState);

        // Navigate to rumors node first (this has the WITS 6 option)
        var rumorsOption = result.ViewModel!.Options.FirstOrDefault(o => o.Text.Contains("deep passages"));
        rumorsOption.Should().NotBeNull();

        var stepResult = await _dialogueService.SelectOptionAsync(character, rumorsOption!.OptionId, _gameState);

        // Assert
        stepResult.Success.Should().BeTrue();
        stepResult.ViewModel.Should().NotBeNull();

        // Should show WITS-gated option as available
        var witsOption = stepResult.ViewModel!.Options.FirstOrDefault(o => o.Text.Contains("WITS"));
        witsOption.Should().NotBeNull();
        witsOption!.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task StartDialogue_WithLowWits_ShowsLockedWitsOption()
    {
        // Arrange
        var character = CreateTestCharacter(wits: 4);

        // Act
        var result = await _dialogueService.StartDialogueAsync(character, "npc_old_scavenger", _gameState);

        // Navigate to rumors node
        var rumorsOption = result.ViewModel!.Options.FirstOrDefault(o => o.Text.Contains("deep passages"));
        var stepResult = await _dialogueService.SelectOptionAsync(character, rumorsOption!.OptionId, _gameState);

        // Assert
        var witsOption = stepResult.ViewModel!.Options.FirstOrDefault(o => o.Text.Contains("WITS"));
        witsOption.Should().NotBeNull();
        witsOption!.IsAvailable.Should().BeFalse();
        witsOption.LockedReason.Should().Contain("Wits");
    }

    [Fact]
    public async Task StartDialogue_WithFriendlyFaction_ShowsReputationGatedOption()
    {
        // Arrange
        var character = CreateTestCharacter();
        _mockFactionService
            .Setup(f => f.MeetsDispositionRequirementAsync(character, FactionType.IronBanes, Disposition.Friendly))
            .ReturnsAsync(true);

        // Act
        var result = await _dialogueService.StartDialogueAsync(character, "npc_old_scavenger", _gameState);

        // Assert
        var specialOption = result.ViewModel!.Options.FirstOrDefault(o => o.Text.Contains("friends of the clan"));
        specialOption.Should().NotBeNull();
        specialOption!.IsAvailable.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // NPC-Dialogue Relationship Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Npc_WithDialogueTreeId_CanTalk()
    {
        // Assert
        var oldScavenger = await _context.Npcs.FirstAsync(n => n.Name == "Old Scavenger");

        oldScavenger.DialogueTreeId.Should().Be("npc_old_scavenger");
        oldScavenger.CanTalk.Should().BeTrue();
    }

    [Fact]
    public async Task Npc_WhenHostile_CannotTalk()
    {
        // Assert
        var boundAcolyte = await _context.Npcs.FirstAsync(n => n.Name == "Bound Acolyte");

        boundAcolyte.IsHostile.Should().BeTrue();
        boundAcolyte.CanTalk.Should().BeFalse();
    }

    [Fact]
    public async Task Npc_WithoutDialogueTreeId_CannotTalk()
    {
        // Assert
        var scrapGuard = await _context.Npcs.FirstAsync(n => n.Name == "Scrap-Guard");

        scrapGuard.DialogueTreeId.Should().BeNull();
        scrapGuard.CanTalk.Should().BeFalse();
    }
}
