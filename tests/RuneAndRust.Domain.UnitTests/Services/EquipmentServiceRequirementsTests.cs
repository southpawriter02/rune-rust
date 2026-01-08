using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Services;

[TestFixture]
public class EquipmentServiceRequirementsTests
{
    private EquipmentService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new EquipmentService(NullLogger<EquipmentService>.Instance);
    }

    [Test]
    public void TryEquip_RequirementsMet_Succeeds()
    {
        // Arrange
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            new PlayerAttributes(10, 14, 10, 10, 10) // Fortitude = 14
        );
        var chainMail = Item.CreateChainMail(); // Requires Fort 12
        player.TryPickUpItem(chainMail);

        // Act
        var result = _service.TryEquipByName(player, "Chain Mail");

        // Assert
        result.Success.Should().BeTrue();
        player.GetEquippedItem(Domain.Enums.EquipmentSlot.Armor).Should().NotBeNull();
    }

    [Test]
    public void TryEquip_RequirementsNotMet_Fails()
    {
        // Arrange
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            new PlayerAttributes(10, 10, 10, 10, 10) // Fortitude = 10, below requirement
        );
        var chainMail = Item.CreateChainMail(); // Requires Fort 12
        player.TryPickUpItem(chainMail);

        // Act
        var result = _service.TryEquipByName(player, "Chain Mail");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Fortitude 12 required");
        player.GetEquippedItem(Domain.Enums.EquipmentSlot.Armor).Should().BeNull();
    }

    [Test]
    public void CanEquip_ReturnsUnmetRequirements()
    {
        // Arrange
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            new PlayerAttributes(10, 10, 10, 10, 10) // Low stats
        );
        var plateArmor = Item.CreatePlateArmor(); // Requires Fort 14, Might 12

        // Act
        var (canEquip, unmetRequirements) = _service.CanEquip(player, plateArmor);

        // Assert
        canEquip.Should().BeFalse();
        unmetRequirements.Should().HaveCount(2);
        unmetRequirements.Should().Contain(r => r.Contains("Fortitude 14 required"));
        unmetRequirements.Should().Contain(r => r.Contains("Might 12 required"));
    }

    [Test]
    public void CanEquip_NoRequirements_ReturnsTrue()
    {
        // Arrange
        var player = new Player("TestHero");
        var leatherArmor = Item.CreateLeatherArmor(); // No requirements

        // Act
        var (canEquip, unmetRequirements) = _service.CanEquip(player, leatherArmor);

        // Assert
        canEquip.Should().BeTrue();
        unmetRequirements.Should().BeEmpty();
    }

    [Test]
    public void TryEquip_PlateArmor_HighStatsPlayer_Succeeds()
    {
        // Arrange
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            new PlayerAttributes(14, 16, 10, 10, 10) // Might = 14, Fort = 16
        );
        var plateArmor = Item.CreatePlateArmor();
        player.TryPickUpItem(plateArmor);

        // Act
        var result = _service.TryEquipByName(player, "Plate Armor");

        // Assert
        result.Success.Should().BeTrue();
        player.GetEquippedItem(Domain.Enums.EquipmentSlot.Armor)?.Name.Should().Be("Plate Armor");
    }
}
