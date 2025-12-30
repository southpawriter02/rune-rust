using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the FactionService (v0.4.2a - The Repute).
/// Tests reputation modification, disposition calculation, and event publishing.
/// </summary>
public class FactionServiceTests
{
    private readonly Mock<IFactionRepository> _mockRepository;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<ILogger<FactionService>> _mockLogger;
    private readonly FactionService _sut;

    public FactionServiceTests()
    {
        _mockRepository = new Mock<IFactionRepository>();
        _mockEventBus = new Mock<IEventBus>();
        _mockLogger = new Mock<ILogger<FactionService>>();

        _sut = new FactionService(
            _mockRepository.Object,
            _mockEventBus.Object,
            _mockLogger.Object);

        // Default setup: No standings exist
        _mockRepository
            .Setup(r => r.GetStandingAsync(It.IsAny<Guid>(), It.IsAny<FactionType>()))
            .ReturnsAsync((CharacterFactionStanding?)null);

        // Default setup: Return faction definitions with default reputation
        _mockRepository
            .Setup(r => r.GetFactionAsync(It.IsAny<FactionType>()))
            .ReturnsAsync((FactionType t) => new Faction
            {
                Type = t,
                Name = t.ToString(),
                Description = $"Description for {t}",
                DefaultReputation = t == FactionType.TheBound ? -25 : 0
            });
    }

    private Character CreateTestCharacter(string name = "TestChar")
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GetDisposition Tests (7 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(-100, Disposition.Hated)]
    [InlineData(-75, Disposition.Hated)]
    [InlineData(-50, Disposition.Hated)]
    public void GetDisposition_WhenReputationIsHated_ReturnsHated(int reputation, Disposition expected)
    {
        var result = _sut.GetDisposition(reputation);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-49, Disposition.Hostile)]
    [InlineData(-25, Disposition.Hostile)]
    [InlineData(-10, Disposition.Hostile)]
    public void GetDisposition_WhenReputationIsHostile_ReturnsHostile(int reputation, Disposition expected)
    {
        var result = _sut.GetDisposition(reputation);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-9, Disposition.Neutral)]
    [InlineData(0, Disposition.Neutral)]
    [InlineData(9, Disposition.Neutral)]
    public void GetDisposition_WhenReputationIsNeutral_ReturnsNeutral(int reputation, Disposition expected)
    {
        var result = _sut.GetDisposition(reputation);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, Disposition.Friendly)]
    [InlineData(30, Disposition.Friendly)]
    [InlineData(49, Disposition.Friendly)]
    public void GetDisposition_WhenReputationIsFriendly_ReturnsFriendly(int reputation, Disposition expected)
    {
        var result = _sut.GetDisposition(reputation);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(50, Disposition.Exalted)]
    [InlineData(75, Disposition.Exalted)]
    [InlineData(100, Disposition.Exalted)]
    public void GetDisposition_WhenReputationIsExalted_ReturnsExalted(int reputation, Disposition expected)
    {
        var result = _sut.GetDisposition(reputation);
        result.Should().Be(expected);
    }

    [Fact]
    public void GetDisposition_AtHatedBoundary_ReturnsHated()
    {
        _sut.GetDisposition(-50).Should().Be(Disposition.Hated);
    }

    [Fact]
    public void GetDisposition_AtExaltedBoundary_ReturnsExalted()
    {
        _sut.GetDisposition(50).Should().Be(Disposition.Exalted);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GetReputation Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetReputationAsync_WhenNoStandingExists_ReturnsDefaultReputation()
    {
        var character = CreateTestCharacter();

        var result = await _sut.GetReputationAsync(character, FactionType.IronBanes);

        result.Should().Be(0); // Default for IronBanes
    }

    [Fact]
    public async Task GetReputationAsync_ForTheBound_ReturnsNegativeDefault()
    {
        var character = CreateTestCharacter();

        var result = await _sut.GetReputationAsync(character, FactionType.TheBound);

        result.Should().Be(-25); // The Bound starts hostile
    }

    [Fact]
    public async Task GetReputationAsync_WhenStandingExists_ReturnsStoredValue()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.Dvergr,
            Reputation = 42
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.Dvergr))
            .ReturnsAsync(standing);

        var result = await _sut.GetReputationAsync(character, FactionType.Dvergr);

        result.Should().Be(42);
    }

    [Fact]
    public async Task GetReputationAsync_WhenFactionNotFound_ReturnsZero()
    {
        var character = CreateTestCharacter();

        _mockRepository
            .Setup(r => r.GetFactionAsync(FactionType.IronBanes))
            .ReturnsAsync((Faction?)null);

        var result = await _sut.GetReputationAsync(character, FactionType.IronBanes);

        result.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ModifyReputation Tests (12 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ModifyReputationAsync_WithPositiveAmount_IncreasesReputation()
    {
        var character = CreateTestCharacter();

        var result = await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 10);

        result.Success.Should().BeTrue();
        result.OldValue.Should().Be(0);
        result.NewValue.Should().Be(10);
        result.Delta.Should().Be(10);
    }

    [Fact]
    public async Task ModifyReputationAsync_WithNegativeAmount_DecreasesReputation()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.IronBanes,
            Reputation = 50
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.IronBanes))
            .ReturnsAsync(standing);

        var result = await _sut.ModifyReputationAsync(character, FactionType.IronBanes, -30);

        result.Success.Should().BeTrue();
        result.OldValue.Should().Be(50);
        result.NewValue.Should().Be(20);
        result.Delta.Should().Be(-30);
    }

    [Fact]
    public async Task ModifyReputationAsync_WithZeroAmount_ReturnsNoChange()
    {
        var character = CreateTestCharacter();

        var result = await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 0);

        result.Success.Should().BeTrue();
        result.Delta.Should().Be(0);
        result.DispositionChanged.Should().BeFalse();
    }

    [Fact]
    public async Task ModifyReputationAsync_ClampsToMaximum()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.IronBanes,
            Reputation = 90
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.IronBanes))
            .ReturnsAsync(standing);

        var result = await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 50);

        result.NewValue.Should().Be(100);
    }

    [Fact]
    public async Task ModifyReputationAsync_ClampsToMinimum()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.IronBanes,
            Reputation = -90
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.IronBanes))
            .ReturnsAsync(standing);

        var result = await _sut.ModifyReputationAsync(character, FactionType.IronBanes, -50);

        result.NewValue.Should().Be(-100);
    }

    [Fact]
    public async Task ModifyReputationAsync_WhenAlreadyAtMax_ReturnsNoChange()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.IronBanes,
            Reputation = 100
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.IronBanes))
            .ReturnsAsync(standing);

        var result = await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 10);

        result.Delta.Should().Be(0);
        result.OldValue.Should().Be(100);
        result.NewValue.Should().Be(100);
    }

    [Fact]
    public async Task ModifyReputationAsync_CreatesNewStanding_WhenNoneExists()
    {
        var character = CreateTestCharacter();

        await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 10);

        _mockRepository.Verify(r => r.AddStandingAsync(It.Is<CharacterFactionStanding>(
            s => s.CharacterId == character.Id &&
                 s.FactionType == FactionType.IronBanes &&
                 s.Reputation == 10)), Times.Once);
    }

    [Fact]
    public async Task ModifyReputationAsync_UpdatesExistingStanding()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.IronBanes,
            Reputation = 20
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.IronBanes))
            .ReturnsAsync(standing);

        await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 10);

        _mockRepository.Verify(r => r.UpdateStandingAsync(It.Is<CharacterFactionStanding>(
            s => s.Reputation == 30)), Times.Once);
    }

    [Fact]
    public async Task ModifyReputationAsync_PublishesReputationChangedEvent()
    {
        var character = CreateTestCharacter();

        await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 10, "Quest completion");

        _mockEventBus.Verify(e => e.PublishAsync(It.Is<ReputationChangedEvent>(
            evt => evt.CharacterId == character.Id &&
                   evt.Faction == FactionType.IronBanes &&
                   evt.OldValue == 0 &&
                   evt.NewValue == 10 &&
                   evt.Source == "Quest completion")), Times.Once);
    }

    [Fact]
    public async Task ModifyReputationAsync_WhenDispositionChanges_PublishesDispositionChangedEvent()
    {
        var character = CreateTestCharacter();

        // Going from 0 (Neutral) to 10 (Friendly)
        var result = await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 10);

        result.DispositionChanged.Should().BeTrue();
        result.OldDisposition.Should().Be(Disposition.Neutral);
        result.NewDisposition.Should().Be(Disposition.Friendly);

        _mockEventBus.Verify(e => e.PublishAsync(It.Is<DispositionChangedEvent>(
            evt => evt.OldDisposition == Disposition.Neutral &&
                   evt.NewDisposition == Disposition.Friendly &&
                   evt.Direction == DispositionChangeDirection.Improved)), Times.Once);
    }

    [Fact]
    public async Task ModifyReputationAsync_WhenDispositionDoesNotChange_DoesNotPublishDispositionEvent()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.IronBanes,
            Reputation = 5
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.IronBanes))
            .ReturnsAsync(standing);

        // Going from 5 to 8, still Neutral
        await _sut.ModifyReputationAsync(character, FactionType.IronBanes, 3);

        _mockEventBus.Verify(e => e.PublishAsync(It.IsAny<DispositionChangedEvent>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SetReputation Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SetReputationAsync_SetsToExactValue()
    {
        var character = CreateTestCharacter();

        var result = await _sut.SetReputationAsync(character, FactionType.IronBanes, 50);

        result.Success.Should().BeTrue();
        result.NewValue.Should().Be(50);
    }

    [Fact]
    public async Task SetReputationAsync_ClampsToValidRange()
    {
        var character = CreateTestCharacter();

        var result = await _sut.SetReputationAsync(character, FactionType.IronBanes, 150);

        result.NewValue.Should().Be(100);
    }

    [Fact]
    public async Task SetReputationAsync_FromExistingValue_CalculatesCorrectDelta()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.IronBanes,
            Reputation = 30
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.IronBanes))
            .ReturnsAsync(standing);

        var result = await _sut.SetReputationAsync(character, FactionType.IronBanes, 50);

        result.OldValue.Should().Be(30);
        result.NewValue.Should().Be(50);
        result.Delta.Should().Be(20);
    }

    [Fact]
    public async Task SetReputationAsync_ToSameValue_ReturnsNoChange()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.IronBanes,
            Reputation = 30
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.IronBanes))
            .ReturnsAsync(standing);

        var result = await _sut.SetReputationAsync(character, FactionType.IronBanes, 30);

        result.Delta.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GetFactionStanding Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetFactionStandingAsync_ReturnsCompleteInfo()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.Dvergr,
            Reputation = 25
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.Dvergr))
            .ReturnsAsync(standing);

        var result = await _sut.GetFactionStandingAsync(character, FactionType.Dvergr);

        result.Faction.Should().Be(FactionType.Dvergr);
        result.Reputation.Should().Be(25);
        result.Disposition.Should().Be(Disposition.Friendly);
        result.IsFriendly.Should().BeTrue();
        result.IsHostile.Should().BeFalse();
    }

    [Fact]
    public async Task GetFactionStandingAsync_WithNoStanding_ReturnsDefault()
    {
        var character = CreateTestCharacter();

        var result = await _sut.GetFactionStandingAsync(character, FactionType.IronBanes);

        result.Reputation.Should().Be(0);
        result.Disposition.Should().Be(Disposition.Neutral);
    }

    [Fact]
    public async Task GetFactionStandingAsync_IncludesFactionName()
    {
        var character = CreateTestCharacter();

        var result = await _sut.GetFactionStandingAsync(character, FactionType.TheFaceless);

        result.FactionName.Should().Be("TheFaceless");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GetAllStandings Tests (2 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAllStandingsAsync_ReturnsAllFactions()
    {
        var character = CreateTestCharacter();

        var result = await _sut.GetAllStandingsAsync(character);

        result.Should().HaveCount(4);
        result.Should().ContainKey(FactionType.IronBanes);
        result.Should().ContainKey(FactionType.Dvergr);
        result.Should().ContainKey(FactionType.TheBound);
        result.Should().ContainKey(FactionType.TheFaceless);
    }

    [Fact]
    public async Task GetAllStandingsAsync_IncludesDefaultAndCustomStandings()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.Dvergr,
            Reputation = 75
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.Dvergr))
            .ReturnsAsync(standing);

        var result = await _sut.GetAllStandingsAsync(character);

        result[FactionType.Dvergr].Reputation.Should().Be(75);
        result[FactionType.IronBanes].Reputation.Should().Be(0); // Default
    }

    // ═══════════════════════════════════════════════════════════════════════
    // IsHostile/IsFriendly Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task IsHostileAsync_WhenHated_ReturnsTrue()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.TheBound,
            Reputation = -75
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.TheBound))
            .ReturnsAsync(standing);

        var result = await _sut.IsHostileAsync(character, FactionType.TheBound);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsHostileAsync_WhenNeutral_ReturnsFalse()
    {
        var character = CreateTestCharacter();

        var result = await _sut.IsHostileAsync(character, FactionType.IronBanes);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsFriendlyAsync_WhenExalted_ReturnsTrue()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.Dvergr,
            Reputation = 75
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.Dvergr))
            .ReturnsAsync(standing);

        var result = await _sut.IsFriendlyAsync(character, FactionType.Dvergr);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFriendlyAsync_WhenNeutral_ReturnsFalse()
    {
        var character = CreateTestCharacter();

        var result = await _sut.IsFriendlyAsync(character, FactionType.IronBanes);

        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // MeetsDispositionRequirement Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task MeetsDispositionRequirementAsync_WhenMeetsRequirement_ReturnsTrue()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.Dvergr,
            Reputation = 25
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.Dvergr))
            .ReturnsAsync(standing);

        var result = await _sut.MeetsDispositionRequirementAsync(
            character, FactionType.Dvergr, Disposition.Friendly);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task MeetsDispositionRequirementAsync_WhenBelowRequirement_ReturnsFalse()
    {
        var character = CreateTestCharacter();

        var result = await _sut.MeetsDispositionRequirementAsync(
            character, FactionType.IronBanes, Disposition.Friendly);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task MeetsDispositionRequirementAsync_WhenExactlyAtRequirement_ReturnsTrue()
    {
        var character = CreateTestCharacter();
        var standing = new CharacterFactionStanding
        {
            CharacterId = character.Id,
            FactionType = FactionType.Dvergr,
            Reputation = 10 // Exactly Friendly threshold
        };

        _mockRepository
            .Setup(r => r.GetStandingAsync(character.Id, FactionType.Dvergr))
            .ReturnsAsync(standing);

        var result = await _sut.MeetsDispositionRequirementAsync(
            character, FactionType.Dvergr, Disposition.Friendly);

        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GetFaction/GetAllFactions Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetFactionAsync_ReturnsFactionDefinition()
    {
        var result = await _sut.GetFactionAsync(FactionType.IronBanes);

        result.Should().NotBeNull();
        result!.Type.Should().Be(FactionType.IronBanes);
        result.Name.Should().Be("IronBanes");
    }

    [Fact]
    public async Task GetFactionAsync_WhenNotFound_ReturnsNull()
    {
        _mockRepository
            .Setup(r => r.GetFactionAsync(FactionType.IronBanes))
            .ReturnsAsync((Faction?)null);

        var result = await _sut.GetFactionAsync(FactionType.IronBanes);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllFactionsAsync_ReturnsAllFactions()
    {
        var factions = new List<Faction>
        {
            new() { Type = FactionType.IronBanes, Name = "Iron-Banes" },
            new() { Type = FactionType.Dvergr, Name = "Dvergr" },
            new() { Type = FactionType.TheBound, Name = "The Bound" },
            new() { Type = FactionType.TheFaceless, Name = "The Faceless" }
        };

        _mockRepository
            .Setup(r => r.GetAllFactionsAsync())
            .ReturnsAsync(factions);

        var result = await _sut.GetAllFactionsAsync();

        result.Should().HaveCount(4);
    }
}
