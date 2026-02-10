// ═══════════════════════════════════════════════════════════════════════════════
// AbilitySlotServiceTests.cs
// Unit tests for the AbilitySlotService, verifying slot initialization,
// tier unlocking, and PP cost calculations.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;

[TestFixture]
public class AbilitySlotServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    private AbilitySlotService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var logger = new Mock<ILogger<AbilitySlotService>>();
        _service = new AbilitySlotService(logger.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INITIALIZATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void InitializeAbilitySlots_CreatesStandardLayout()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var slots = _service.InitializeAbilitySlots(characterId, SpecializationId.Skjaldmaer);

        // Assert
        slots.CharacterId.Should().Be(characterId);
        slots.SpecializationId.Should().Be(SpecializationId.Skjaldmaer);
        slots.Tier1Slots.Should().Be(3);
        slots.Tier2Slots.Should().Be(3);
        slots.Tier3Slots.Should().Be(2);
        slots.CapstoneSlots.Should().Be(1);
        slots.TotalSlots.Should().Be(9);
    }

    [Test]
    public void GetAbilitySlots_AfterInitialization_ReturnsSlots()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _service.InitializeAbilitySlots(characterId, SpecializationId.BoneSetter);

        // Act
        var slots = _service.GetAbilitySlots(characterId, SpecializationId.BoneSetter);

        // Assert
        slots.Should().NotBeNull();
        slots!.TotalSlots.Should().Be(9);
    }

    [Test]
    public void GetAbilitySlots_WithoutInitialization_ReturnsNull()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var slots = _service.GetAbilitySlots(characterId, SpecializationId.Seidkona);

        // Assert
        slots.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIER UNLOCK TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsTierUnlocked_Tier1_AlwaysUnlocked()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _service.InitializeAbilitySlots(characterId, SpecializationId.Berserkr);

        // Act & Assert
        _service.IsTierUnlocked(characterId, SpecializationId.Berserkr, 1).Should().BeTrue();
    }

    [Test]
    public void IsTierUnlocked_Tier2_WithInsufficientPP_ReturnsFalse()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _service.InitializeAbilitySlots(characterId, SpecializationId.Berserkr);

        // Act & Assert — 0 PP invested, need 8
        _service.IsTierUnlocked(characterId, SpecializationId.Berserkr, 2).Should().BeFalse();
    }

    [Test]
    public void IsTierUnlocked_Tier2_WithSufficientPP_ReturnsTrue()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _service.InitializeAbilitySlots(characterId, SpecializationId.Berserkr);
        _service.InvestPP(characterId, SpecializationId.Berserkr, 8);

        // Act & Assert
        _service.IsTierUnlocked(characterId, SpecializationId.Berserkr, 2).Should().BeTrue();
    }

    [Test]
    public void IsTierUnlocked_Tier3_RequiresPP16()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _service.InitializeAbilitySlots(characterId, SpecializationId.Veidimadr);
        _service.InvestPP(characterId, SpecializationId.Veidimadr, 16);

        // Act & Assert
        _service.IsTierUnlocked(characterId, SpecializationId.Veidimadr, 3).Should().BeTrue();
    }

    [Test]
    public void IsTierUnlocked_Capstone_RequiresPP24()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _service.InitializeAbilitySlots(characterId, SpecializationId.Seidkona);
        _service.InvestPP(characterId, SpecializationId.Seidkona, 24);

        // Act & Assert
        _service.IsTierUnlocked(characterId, SpecializationId.Seidkona, 4).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NEXT TIER COST TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetNextTierUnlockCost_WithNoPP_ReturnsTier2Cost()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _service.InitializeAbilitySlots(characterId, SpecializationId.MyrkGengr);

        // Act
        var cost = _service.GetNextTierUnlockCost(characterId, SpecializationId.MyrkGengr);

        // Assert — Tier 2 cost is 4 PP per slot
        cost.Should().Be(4);
    }

    [Test]
    public void GetNextTierUnlockCost_AllTiersUnlocked_ReturnsZero()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _service.InitializeAbilitySlots(characterId, SpecializationId.JotunReader);
        _service.InvestPP(characterId, SpecializationId.JotunReader, 24);

        // Act
        var cost = _service.GetNextTierUnlockCost(characterId, SpecializationId.JotunReader);

        // Assert
        cost.Should().Be(0);
    }
}
