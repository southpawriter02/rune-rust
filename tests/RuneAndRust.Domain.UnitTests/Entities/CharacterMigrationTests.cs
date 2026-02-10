// ═══════════════════════════════════════════════════════════════════════════════
// CharacterMigrationTests.cs
// Unit tests for the CharacterMigration entity, verifying state transitions,
// validation, and error handling.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

[TestFixture]
public class CharacterMigrationTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    private static CharacterMigration CreateMigration(
        LegacyClassId originalClass = LegacyClassId.Fighter,
        Archetype targetArchetype = Archetype.Warrior) => new()
    {
        Id = Guid.NewGuid(),
        CharacterId = Guid.NewGuid(),
        OriginalClass = originalClass,
        TargetArchetype = targetArchetype,
        CreatedAt = DateTime.UtcNow
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_DefaultStatus_IsPending()
    {
        // Arrange & Act
        var migration = CreateMigration();

        // Assert
        migration.Status.Should().Be(MigrationStatus.Pending);
        migration.SelectedSpecialization.Should().BeNull();
        migration.PpRefunded.Should().Be(0);
        migration.CompletedAt.Should().BeNull();
        migration.ErrorMessage.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BEGIN MIGRATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void BeginMigration_WhenPending_TransitionsToInProgress()
    {
        // Arrange
        var migration = CreateMigration();

        // Act
        migration.BeginMigration();

        // Assert
        migration.Status.Should().Be(MigrationStatus.InProgress);
    }

    [Test]
    public void BeginMigration_WhenAlreadyInProgress_ThrowsInvalidOperation()
    {
        // Arrange
        var migration = CreateMigration();
        migration.BeginMigration();

        // Act
        var act = () => migration.BeginMigration();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*current status is InProgress*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SELECT SPECIALIZATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SelectSpecialization_WhenInProgress_SetsSpecialization()
    {
        // Arrange
        var migration = CreateMigration();
        migration.BeginMigration();

        // Act
        migration.SelectSpecialization(SpecializationId.Skjaldmaer);

        // Assert
        migration.SelectedSpecialization.Should().Be(SpecializationId.Skjaldmaer);
    }

    [Test]
    public void SelectSpecialization_WhenPending_ThrowsInvalidOperation()
    {
        // Arrange
        var migration = CreateMigration();

        // Act
        var act = () => migration.SelectSpecialization(SpecializationId.Skjaldmaer);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*current status is Pending*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECORD REFUND TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RecordRefund_WhenInProgress_AccumulatesAmount()
    {
        // Arrange
        var migration = CreateMigration();
        migration.BeginMigration();

        // Act
        migration.RecordRefund(5);
        migration.RecordRefund(3);

        // Assert
        migration.PpRefunded.Should().Be(8);
    }

    [Test]
    public void RecordRefund_WithNegativeAmount_ThrowsArgumentOutOfRange()
    {
        // Arrange
        var migration = CreateMigration();
        migration.BeginMigration();

        // Act
        var act = () => migration.RecordRefund(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RecordRefund_WhenPending_ThrowsInvalidOperation()
    {
        // Arrange
        var migration = CreateMigration();

        // Act
        var act = () => migration.RecordRefund(5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*current status is Pending*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPLETE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Complete_WhenInProgressWithSpecialization_TransitionsToCompleted()
    {
        // Arrange
        var migration = CreateMigration();
        migration.BeginMigration();
        migration.SelectSpecialization(SpecializationId.Berserkr);

        // Act
        migration.Complete();

        // Assert
        migration.Status.Should().Be(MigrationStatus.Completed);
        migration.CompletedAt.Should().NotBeNull();
        migration.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Complete_WithoutSpecialization_ThrowsInvalidOperation()
    {
        // Arrange
        var migration = CreateMigration();
        migration.BeginMigration();

        // Act
        var act = () => migration.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no specialization has been selected*");
    }

    [Test]
    public void Complete_WhenPending_ThrowsInvalidOperation()
    {
        // Arrange
        var migration = CreateMigration();

        // Act
        var act = () => migration.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*current status is Pending*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FAIL TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Fail_WithErrorMessage_TransitionsToFailedAndRecordsMessage()
    {
        // Arrange
        var migration = CreateMigration();
        migration.BeginMigration();

        // Act
        migration.Fail("Migration failed due to corrupted data.");

        // Assert
        migration.Status.Should().Be(MigrationStatus.Failed);
        migration.ErrorMessage.Should().Be("Migration failed due to corrupted data.");
        migration.CompletedAt.Should().NotBeNull();
    }

    [Test]
    public void Fail_WithNullMessage_ThrowsArgumentException()
    {
        // Arrange
        var migration = CreateMigration();

        // Act
        var act = () => migration.Fail(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Fail_WithEmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var migration = CreateMigration();

        // Act
        var act = () => migration.Fail("  ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FULL LIFECYCLE TEST
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FullLifecycle_PendingToCompleted_SuccessfulMigration()
    {
        // Arrange
        var migration = CreateMigration(LegacyClassId.Rogue, Archetype.Skirmisher);

        // Act
        migration.BeginMigration();
        migration.SelectSpecialization(SpecializationId.MyrkGengr);
        migration.RecordRefund(10);
        migration.Complete();

        // Assert
        migration.Status.Should().Be(MigrationStatus.Completed);
        migration.SelectedSpecialization.Should().Be(SpecializationId.MyrkGengr);
        migration.PpRefunded.Should().Be(10);
        migration.CompletedAt.Should().NotBeNull();
        migration.ErrorMessage.Should().BeNull();
    }
}
