using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the DataCaptureService class.
/// Validates capture generation, auto-assignment, and completion percentage calculations.
/// v0.3.25c: Updated to use mock ICaptureTemplateRepository.
/// </summary>
public class DataCaptureServiceTests
{
    private readonly Mock<ILogger<DataCaptureService>> _mockLogger;
    private readonly Mock<IDataCaptureRepository> _mockCaptureRepository;
    private readonly Mock<ICodexEntryRepository> _mockCodexRepository;
    private readonly Mock<ICaptureTemplateRepository> _mockTemplateRepository;
    private readonly DataCaptureService _sut;
    private readonly DataCaptureService _seededSut;

    public DataCaptureServiceTests()
    {
        _mockLogger = new Mock<ILogger<DataCaptureService>>();
        _mockCaptureRepository = new Mock<IDataCaptureRepository>();
        _mockCodexRepository = new Mock<ICodexEntryRepository>();
        _mockTemplateRepository = new Mock<ICaptureTemplateRepository>();

        // Configure mock template repository to return templates for each category
        SetupMockTemplateRepository();

        _sut = new DataCaptureService(
            _mockLogger.Object,
            _mockCaptureRepository.Object,
            _mockCodexRepository.Object,
            _mockTemplateRepository.Object);

        // Seeded instance for deterministic tests (seed 42 produces roll 37 first time)
        _seededSut = new DataCaptureService(
            _mockLogger.Object,
            _mockCaptureRepository.Object,
            _mockCodexRepository.Object,
            _mockTemplateRepository.Object,
            42);
    }

    /// <summary>
    /// Configures the mock template repository with test templates.
    /// </summary>
    private void SetupMockTemplateRepository()
    {
        // Generic container template
        _mockTemplateRepository
            .Setup(r => r.GetRandomAsync("generic-container"))
            .ReturnsAsync(new CaptureTemplateDto
            {
                Id = "test-generic-1",
                Type = CaptureType.TextFragment,
                FragmentContent = "A weathered document found within.",
                Source = "Container Search",
                MatchKeywords = new[] { "container", "salvage" },
                Category = "generic-container"
            });

        // Rusted servitor template
        _mockTemplateRepository
            .Setup(r => r.GetRandomAsync("rusted-servitor"))
            .ReturnsAsync(new CaptureTemplateDto
            {
                Id = "test-servitor-1",
                Type = CaptureType.Specimen,
                FragmentContent = "Corroded metal fragments from an ancient automaton.",
                Source = "Servitor Remains",
                MatchKeywords = new[] { "servitor", "automaton", "machine" },
                Category = "rusted-servitor"
            });

        // Blighted creature template
        _mockTemplateRepository
            .Setup(r => r.GetRandomAsync("blighted-creature"))
            .ReturnsAsync(new CaptureTemplateDto
            {
                Id = "test-blighted-1",
                Type = CaptureType.Specimen,
                FragmentContent = "Tissue sample showing signs of corruption.",
                Source = "Blighted Remains",
                MatchKeywords = new[] { "blight", "corruption", "infected" },
                Category = "blighted-creature"
            });

        // Industrial site template
        _mockTemplateRepository
            .Setup(r => r.GetRandomAsync("industrial-site"))
            .ReturnsAsync(new CaptureTemplateDto
            {
                Id = "test-industrial-1",
                Type = CaptureType.TextFragment,
                FragmentContent = "Schematics for forgotten machinery.",
                Source = "Industrial Site",
                MatchKeywords = new[] { "forge", "mechanism", "industrial" },
                Category = "industrial-site"
            });

        // Ancient ruin template
        _mockTemplateRepository
            .Setup(r => r.GetRandomAsync("ancient-ruin"))
            .ReturnsAsync(new CaptureTemplateDto
            {
                Id = "test-ruin-1",
                Type = CaptureType.RunicTrace,
                FragmentContent = "Faded inscriptions on crumbling stone.",
                Source = "Ancient Inscription",
                MatchKeywords = new[] { "ancient", "ruin", "inscription" },
                Category = "ancient-ruin"
            });
    }

    #region TryGenerateFromSearchAsync Tests

    [Fact]
    public async Task TryGenerateFromSearchAsync_RollSucceeds_ReturnsCapture()
    {
        // Arrange - Use seeded service with high WITS bonus to ensure success
        var characterId = Guid.NewGuid();
        var container = CreateContainer("Rusted Crate");

        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry>());

        // Act
        var result = await _seededSut.TryGenerateFromSearchAsync(characterId, container, 20);

        // Assert
        result.Success.Should().BeTrue();
        result.Capture.Should().NotBeNull();
        result.Capture!.CharacterId.Should().Be(characterId);
        result.Capture.Quality.Should().Be(15); // StandardQuality
    }

    [Fact]
    public async Task TryGenerateFromSearchAsync_RollFails_ReturnsNoCapture()
    {
        // Arrange - Seed that produces high roll (0 wits = 25% chance, need roll < 25)
        // Seed 100 produces first roll of 85
        var failSut = new DataCaptureService(
            _mockLogger.Object,
            _mockCaptureRepository.Object,
            _mockCodexRepository.Object,
            _mockTemplateRepository.Object,
            100);

        var characterId = Guid.NewGuid();
        var container = CreateContainer("Empty Box");

        // Act
        var result = await failSut.TryGenerateFromSearchAsync(characterId, container, 0);

        // Assert
        result.Success.Should().BeFalse();
        result.Capture.Should().BeNull();
        result.Message.Should().Contain("No lore fragments");
    }

    [Fact]
    public async Task TryGenerateFromSearchAsync_WithHighWits_IncreasesChance()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var container = CreateContainer("Salvage Crate");

        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry>());

        // Run multiple iterations to verify higher WITS bonus produces more captures
        var successCountWithBonus = 0;
        var successCountWithoutBonus = 0;

        for (int i = 0; i < 100; i++)
        {
            var sutWithBonus = new DataCaptureService(
                _mockLogger.Object,
                _mockCaptureRepository.Object,
                _mockCodexRepository.Object,
                _mockTemplateRepository.Object,
                i);

            var sutWithoutBonus = new DataCaptureService(
                _mockLogger.Object,
                _mockCaptureRepository.Object,
                _mockCodexRepository.Object,
                _mockTemplateRepository.Object,
                i);

            var withBonus = await sutWithBonus.TryGenerateFromSearchAsync(characterId, container, 10);
            var withoutBonus = await sutWithoutBonus.TryGenerateFromSearchAsync(characterId, container, 0);

            if (withBonus.Success) successCountWithBonus++;
            if (withoutBonus.Success) successCountWithoutBonus++;
        }

        // Assert - With 10 WITS bonus (75% chance) should have more successes than 0 bonus (25% chance)
        successCountWithBonus.Should().BeGreaterThan(successCountWithoutBonus);
    }

    [Fact]
    public async Task TryGenerateFromSearchAsync_ServitorContainer_ReturnsServitorCapture()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var container = CreateContainer("Rusted Servitor Remains", "A corroded automaton, partially disassembled.");

        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry>());

        // Act - Use high WITS to ensure capture
        var result = await _seededSut.TryGenerateFromSearchAsync(characterId, container, 20);

        // Assert
        result.Success.Should().BeTrue();
        result.Capture.Should().NotBeNull();
        result.Capture!.Source.Should().Contain("Rusted Servitor Remains");
    }

    [Fact]
    public async Task TryGenerateFromSearchAsync_PersistsCaptureToRepository()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var container = CreateContainer("Ancient Chest");

        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry>());

        // Act
        await _seededSut.TryGenerateFromSearchAsync(characterId, container, 20);

        // Assert
        _mockCaptureRepository.Verify(r => r.AddAsync(It.IsAny<DataCapture>()), Times.Once);
        _mockCaptureRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region TryGenerateFromExaminationAsync Tests

    [Fact]
    public async Task TryGenerateFromExaminationAsync_ExpertTier_HighChance()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var target = CreateInteractableObject("Ancient Device", "A complex mechanism of unknown origin.");

        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry>());

        // Act - Expert tier (2) has 75% base chance
        var result = await _seededSut.TryGenerateFromExaminationAsync(characterId, target, 2, 0);

        // Assert
        result.Success.Should().BeTrue();
        result.Capture.Should().NotBeNull();
        result.Capture!.Quality.Should().Be(30); // SpecialistQuality for expert tier
    }

    [Fact]
    public async Task TryGenerateFromExaminationAsync_DetailedTier_MediumChance()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var target = CreateInteractableObject("Old Inscription");

        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry>());

        // Detailed tier (1) has 37% base chance
        // Seeded with value that produces roll under 37
        var detailedSut = new DataCaptureService(
            _mockLogger.Object,
            _mockCaptureRepository.Object,
            _mockCodexRepository.Object,
            _mockTemplateRepository.Object,
            5); // Seed 5 produces roll 26

        // Act
        var result = await detailedSut.TryGenerateFromExaminationAsync(characterId, target, 1, 0);

        // Assert
        result.Success.Should().BeTrue();
        result.Capture.Should().NotBeNull();
        result.Capture!.Quality.Should().Be(15); // StandardQuality for detailed tier
    }

    [Fact]
    public async Task TryGenerateFromExaminationAsync_BaseTier_NoCapture()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var target = CreateInteractableObject("Stone Wall");

        // Act - Base tier (0) never generates captures
        var result = await _sut.TryGenerateFromExaminationAsync(characterId, target, 0, 10);

        // Assert
        result.Success.Should().BeFalse();
        result.Capture.Should().BeNull();
        result.Message.Should().Contain("Basic examination");
    }

    [Fact]
    public async Task TryGenerateFromExaminationAsync_ExpertTier_SetsSpecialistQuality()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var target = CreateInteractableObject("Blighted Corpse", "A corrupted body showing signs of mutation.");

        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry>());

        // Act
        var result = await _seededSut.TryGenerateFromExaminationAsync(characterId, target, 2, 5);

        // Assert
        result.Success.Should().BeTrue();
        result.Capture!.Quality.Should().Be(30);
    }

    #endregion

    #region Auto-Assignment Tests

    [Fact]
    public async Task TryGenerateCapture_MatchingEntry_AutoAssigns()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var container = CreateContainer("Servitor Parts", "Components from a rusted automaton.");

        var codexEntry = new CodexEntry
        {
            Id = Guid.NewGuid(),
            Title = "Rusted Servitor",
            Category = EntryCategory.Bestiary,
            TotalFragments = 4
        };

        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry> { codexEntry });

        // Act
        var result = await _seededSut.TryGenerateFromSearchAsync(characterId, container, 20);

        // Assert
        result.Success.Should().BeTrue();
        result.WasAutoAssigned.Should().BeTrue();
        result.Capture!.CodexEntryId.Should().Be(codexEntry.Id);
    }

    [Fact]
    public async Task TryGenerateCapture_NoMatchingEntry_RemainsUnassigned()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var container = CreateContainer("Generic Box");

        // No matching codex entries
        _mockCodexRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CodexEntry>
            {
                new CodexEntry { Title = "Completely Unrelated Topic" }
            });

        // Act
        var result = await _seededSut.TryGenerateFromSearchAsync(characterId, container, 20);

        // Assert
        result.Success.Should().BeTrue();
        result.WasAutoAssigned.Should().BeFalse();
        result.Capture!.CodexEntryId.Should().BeNull();
    }

    #endregion

    #region GetCompletionPercentageAsync Tests

    [Fact]
    public async Task GetCompletionPercentageAsync_NoFragments_ReturnsZero()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var entry = new CodexEntry
        {
            Id = entryId,
            Title = "Test Entry",
            TotalFragments = 4
        };

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync(entry);
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entryId, characterId))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.GetCompletionPercentageAsync(entryId, characterId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetCompletionPercentageAsync_AllFragments_ReturnsHundred()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var entry = new CodexEntry
        {
            Id = entryId,
            Title = "Complete Entry",
            TotalFragments = 4
        };

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync(entry);
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entryId, characterId))
            .ReturnsAsync(4);

        // Act
        var result = await _sut.GetCompletionPercentageAsync(entryId, characterId);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public async Task GetCompletionPercentageAsync_HalfFragments_ReturnsFifty()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var entry = new CodexEntry
        {
            Id = entryId,
            Title = "Half Entry",
            TotalFragments = 4
        };

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync(entry);
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entryId, characterId))
            .ReturnsAsync(2);

        // Act
        var result = await _sut.GetCompletionPercentageAsync(entryId, characterId);

        // Assert
        result.Should().Be(50);
    }

    [Fact]
    public async Task GetCompletionPercentageAsync_EntryNotFound_ReturnsZero()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync((CodexEntry?)null);

        // Act
        var result = await _sut.GetCompletionPercentageAsync(entryId, characterId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetCompletionPercentageAsync_MoreFragmentsThanRequired_CapsAtHundred()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var entry = new CodexEntry
        {
            Id = entryId,
            Title = "Overfilled Entry",
            TotalFragments = 2
        };

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync(entry);
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entryId, characterId))
            .ReturnsAsync(5); // More than TotalFragments

        // Act
        var result = await _sut.GetCompletionPercentageAsync(entryId, characterId);

        // Assert
        result.Should().Be(100); // Capped at 100
    }

    #endregion

    #region GetUnlockedThresholdsAsync Tests

    [Fact]
    public async Task GetUnlockedThresholdsAsync_AtZeroPercent_ReturnsEmpty()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var entry = new CodexEntry
        {
            Id = entryId,
            Title = "Threshold Entry",
            TotalFragments = 4,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" },
                { 100, "FULL_ENTRY" }
            }
        };

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync(entry);
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entryId, characterId))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.GetUnlockedThresholdsAsync(entryId, characterId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnlockedThresholdsAsync_AtTwentyFivePercent_ReturnsFirstThreshold()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var entry = new CodexEntry
        {
            Id = entryId,
            Title = "Threshold Entry",
            TotalFragments = 4,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" },
                { 100, "FULL_ENTRY" }
            }
        };

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync(entry);
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entryId, characterId))
            .ReturnsAsync(1); // 25% completion

        // Act
        var result = await _sut.GetUnlockedThresholdsAsync(entryId, characterId);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain("WEAKNESS_REVEALED");
    }

    [Fact]
    public async Task GetUnlockedThresholdsAsync_AtFiftyPercent_ReturnsTwoThresholds()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var entry = new CodexEntry
        {
            Id = entryId,
            Title = "Threshold Entry",
            TotalFragments = 4,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" },
                { 100, "FULL_ENTRY" }
            }
        };

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync(entry);
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entryId, characterId))
            .ReturnsAsync(2); // 50% completion

        // Act
        var result = await _sut.GetUnlockedThresholdsAsync(entryId, characterId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("WEAKNESS_REVEALED");
        result.Should().Contain("HABITAT_REVEALED");
    }

    [Fact]
    public async Task GetUnlockedThresholdsAsync_AtHundredPercent_ReturnsAllThresholds()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var entry = new CodexEntry
        {
            Id = entryId,
            Title = "Threshold Entry",
            TotalFragments = 4,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 25, "WEAKNESS_REVEALED" },
                { 50, "HABITAT_REVEALED" },
                { 75, "BEHAVIOR_REVEALED" },
                { 100, "FULL_ENTRY" }
            }
        };

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync(entry);
        _mockCaptureRepository.Setup(r => r.GetFragmentCountAsync(entryId, characterId))
            .ReturnsAsync(4); // 100% completion

        // Act
        var result = await _sut.GetUnlockedThresholdsAsync(entryId, characterId);

        // Assert
        result.Should().HaveCount(4);
        result.Should().ContainInOrder(
            "WEAKNESS_REVEALED",
            "HABITAT_REVEALED",
            "BEHAVIOR_REVEALED",
            "FULL_ENTRY");
    }

    [Fact]
    public async Task GetUnlockedThresholdsAsync_EntryNotFound_ReturnsEmpty()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        _mockCodexRepository.Setup(r => r.GetByIdAsync(entryId))
            .ReturnsAsync((CodexEntry?)null);

        // Act
        var result = await _sut.GetUnlockedThresholdsAsync(entryId, characterId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CaptureResult Factory Tests

    [Fact]
    public void CaptureResult_Generated_SetsAllProperties()
    {
        // Arrange
        var capture = new DataCapture
        {
            CharacterId = Guid.NewGuid(),
            Type = CaptureType.Specimen,
            FragmentContent = "Test content"
        };

        // Act
        var result = CaptureResult.Generated("Test message", capture, true);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Test message");
        result.Capture.Should().Be(capture);
        result.WasAutoAssigned.Should().BeTrue();
    }

    [Fact]
    public void CaptureResult_NoCapture_ReturnsFailure()
    {
        // Act
        var result = CaptureResult.NoCapture("No fragments found");

        // Assert
        result.Success.Should().BeFalse();
        result.Capture.Should().BeNull();
        result.WasAutoAssigned.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static InteractableObject CreateContainer(string name, string description = "A container")
    {
        return new InteractableObject
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsContainer = true,
            IsOpen = true
        };
    }

    private static InteractableObject CreateInteractableObject(string name, string description = "An object")
    {
        return new InteractableObject
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsContainer = false
        };
    }

    #endregion
}
