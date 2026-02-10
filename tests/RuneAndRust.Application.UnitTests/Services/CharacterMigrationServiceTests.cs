// ═══════════════════════════════════════════════════════════════════════════════
// CharacterMigrationServiceTests.cs
// Unit tests for the CharacterMigrationService, verifying migration lifecycle,
// mapping resolution, specialization selection, and audit logging.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;

[TestFixture]
public class CharacterMigrationServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    private LegacyClassDetectionService _detectionService = null!;
    private CharacterMigrationService _migrationService = null!;

    [SetUp]
    public void SetUp()
    {
        var detectionLogger = new Mock<ILogger<LegacyClassDetectionService>>();
        _detectionService = new LegacyClassDetectionService(detectionLogger.Object);

        var migrationLogger = new Mock<ILogger<CharacterMigrationService>>();
        _migrationService = new CharacterMigrationService(_detectionService, migrationLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MAPPING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(LegacyClassId.Rogue, Archetype.Skirmisher)]
    [TestCase(LegacyClassId.Fighter, Archetype.Warrior)]
    [TestCase(LegacyClassId.Mage, Archetype.Mystic)]
    [TestCase(LegacyClassId.Healer, Archetype.Adept)]
    [TestCase(LegacyClassId.Scholar, Archetype.Adept)]
    [TestCase(LegacyClassId.Crafter, Archetype.Adept)]
    public void GetMappingForLegacyClass_ReturnsCorrectArchetype(
        LegacyClassId legacyClass, Archetype expectedArchetype)
    {
        // Act
        var mapping = _migrationService.GetMappingForLegacyClass(legacyClass);

        // Assert
        mapping.LegacyClass.Should().Be(legacyClass);
        mapping.TargetArchetype.Should().Be(expectedArchetype);
        mapping.SuggestedSpecializations.Should().NotBeEmpty();
        mapping.MigrationDescription.Should().NotBeNullOrWhiteSpace();
        mapping.GrantsFreeSpecialization.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INITIATE MIGRATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void InitiateMigration_WithLegacyCharacter_CreatesMigrationRecord()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Fighter);

        // Act
        var migration = _migrationService.InitiateMigration(characterId);

        // Assert
        migration.Should().NotBeNull();
        migration.CharacterId.Should().Be(characterId);
        migration.OriginalClass.Should().Be(LegacyClassId.Fighter);
        migration.TargetArchetype.Should().Be(Archetype.Warrior);
        migration.Status.Should().Be(MigrationStatus.InProgress);
    }

    [Test]
    public void InitiateMigration_WithoutLegacyClass_ThrowsInvalidOperation()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var act = () => _migrationService.InitiateMigration(characterId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no legacy class found*");
    }

    [Test]
    public void InitiateMigration_WhenAlreadyMigrating_ThrowsInvalidOperation()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Mage);
        _migrationService.InitiateMigration(characterId);

        // Act
        var act = () => _migrationService.InitiateMigration(characterId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*migration already exists*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SPECIALIZATION SELECTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetAvailableSpecializations_ForWarriorMigration_Returns6Specializations()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Fighter);
        var migration = _migrationService.InitiateMigration(characterId);

        // Act
        var available = _migrationService.GetAvailableSpecializations(migration.Id);

        // Assert
        available.Should().HaveCount(6);
        available.Should().Contain(SpecializationId.Berserkr);
        available.Should().Contain(SpecializationId.Skjaldmaer);
    }

    [Test]
    public void SelectSpecialization_WithValidChoice_RecordsSelection()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Rogue);
        var migration = _migrationService.InitiateMigration(characterId);

        // Act
        _migrationService.SelectSpecialization(migration.Id, SpecializationId.MyrkGengr);

        // Assert
        migration.SelectedSpecialization.Should().Be(SpecializationId.MyrkGengr);
    }

    [Test]
    public void SelectSpecialization_WithInvalidChoice_ThrowsInvalidOperation()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Rogue);
        var migration = _migrationService.InitiateMigration(characterId);

        // Act — Skjaldmaer is a Warrior spec, not Skirmisher
        var act = () => _migrationService.SelectSpecialization(
            migration.Id, SpecializationId.Skjaldmaer);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not available for archetype*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPLETE MIGRATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CompleteMigration_WithSpecializationSelected_ReturnsSuccessResult()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Healer);
        var migration = _migrationService.InitiateMigration(characterId);
        _migrationService.SelectSpecialization(migration.Id, SpecializationId.BoneSetter);

        // Act
        var result = _migrationService.CompleteMigration(migration.Id);

        // Assert
        result.Status.Should().Be(MigrationStatus.Completed);
        result.CharacterId.Should().Be(characterId);
        result.OriginalClass.Should().Be(LegacyClassId.Healer);
        result.AssignedArchetype.Should().Be(Archetype.Adept);
        result.SelectedSpecialization.Should().Be(SpecializationId.BoneSetter);
        result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void CompleteMigration_WithoutSpecialization_ThrowsInvalidOperation()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Mage);
        var migration = _migrationService.InitiateMigration(characterId);

        // Act
        var act = () => _migrationService.CompleteMigration(migration.Id);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no specialization*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MIGRATION STATUS AND LOGS TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetMigrationStatus_WithActiveMigration_ReturnsMigration()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Scholar);
        _migrationService.InitiateMigration(characterId);

        // Act
        var status = _migrationService.GetMigrationStatus(characterId);

        // Assert
        status.Should().NotBeNull();
        status!.CharacterId.Should().Be(characterId);
        status.Status.Should().Be(MigrationStatus.InProgress);
    }

    [Test]
    public void GetMigrationStatus_WithNoMigration_ReturnsNull()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var status = _migrationService.GetMigrationStatus(characterId);

        // Assert
        status.Should().BeNull();
    }

    [Test]
    public void GetMigrationLogs_AfterMigrationActions_ReturnsLogEntries()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _detectionService.RegisterLegacyCharacter(characterId, LegacyClassId.Crafter);
        var migration = _migrationService.InitiateMigration(characterId);
        _migrationService.SelectSpecialization(migration.Id, SpecializationId.ScrapTinker);
        _migrationService.CompleteMigration(migration.Id);

        // Act
        var logs = _migrationService.GetMigrationLogs(characterId);

        // Assert
        logs.Should().NotBeEmpty();
        logs.Should().HaveCountGreaterOrEqualTo(3); // Initiated + Selected + Completed
        logs.Should().Contain(l => l.ActionType == "MigrationInitiated");
        logs.Should().Contain(l => l.ActionType == "SpecializationSelected");
        logs.Should().Contain(l => l.ActionType == "MigrationCompleted");
    }
}
