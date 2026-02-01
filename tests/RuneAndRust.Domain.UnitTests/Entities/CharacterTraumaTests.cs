namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="CharacterTrauma"/> entity.
/// Verifies factory method, validation, domain methods, and ToString formatting.
/// </summary>
[TestFixture]
public class CharacterTraumaTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Data
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly Guid _characterId = Guid.NewGuid();
    private const string ValidTraumaId = "survivors-guilt";
    private const string ValidSource = "AllyDeath";

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method — Valid Creation
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_CreatesCharacterTrauma()
    {
        // Arrange
        var acquiredAt = DateTime.UtcNow;

        // Act
        var trauma = CharacterTrauma.Create(
            characterId: _characterId,
            traumaDefinitionId: ValidTraumaId,
            source: ValidSource,
            acquiredAt: acquiredAt
        );

        // Assert
        trauma.CharacterId.Should().Be(_characterId);
        trauma.TraumaDefinitionId.Should().Be(ValidTraumaId);
        trauma.Source.Should().Be(ValidSource);
        trauma.AcquiredAt.Should().Be(acquiredAt);
        trauma.StackCount.Should().Be(1);
        trauma.IsActive.Should().BeTrue();
        trauma.ManagedSince.Should().BeNull();
        trauma.Notes.Should().BeNull();
        trauma.Id.Should().NotBe(Guid.Empty);
    }

    [Test]
    public void Create_NormalizesTraumaDefinitionIdToLowercase()
    {
        // Arrange & Act
        var trauma = CharacterTrauma.Create(
            characterId: _characterId,
            traumaDefinitionId: "Survivors-GUILT",
            source: ValidSource,
            acquiredAt: DateTime.UtcNow
        );

        // Assert
        trauma.TraumaDefinitionId.Should().Be("survivors-guilt");
    }

    [Test]
    public void Create_GeneratesUniqueId()
    {
        // Arrange & Act
        var trauma1 = CharacterTrauma.Create(_characterId, "trauma-1", ValidSource, DateTime.UtcNow);
        var trauma2 = CharacterTrauma.Create(_characterId, "trauma-2", ValidSource, DateTime.UtcNow);

        // Assert
        trauma1.Id.Should().NotBe(Guid.Empty);
        trauma2.Id.Should().NotBe(Guid.Empty);
        trauma1.Id.Should().NotBe(trauma2.Id);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method — Validation Errors
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithEmptyCharacterId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => CharacterTrauma.Create(
            characterId: Guid.Empty,
            traumaDefinitionId: ValidTraumaId,
            source: ValidSource,
            acquiredAt: DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("characterId")
            .WithMessage("*CharacterId cannot be empty*");
    }

    [Test]
    public void Create_WithEmptyTraumaDefinitionId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => CharacterTrauma.Create(
            characterId: _characterId,
            traumaDefinitionId: "",
            source: ValidSource,
            acquiredAt: DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("traumaDefinitionId");
    }

    [Test]
    public void Create_WithWhitespaceTraumaDefinitionId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => CharacterTrauma.Create(
            characterId: _characterId,
            traumaDefinitionId: "   ",
            source: ValidSource,
            acquiredAt: DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("traumaDefinitionId");
    }

    [Test]
    public void Create_WithEmptySource_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => CharacterTrauma.Create(
            characterId: _characterId,
            traumaDefinitionId: ValidTraumaId,
            source: "",
            acquiredAt: DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("source");
    }

    [Test]
    public void Create_WithWhitespaceSource_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => CharacterTrauma.Create(
            characterId: _characterId,
            traumaDefinitionId: ValidTraumaId,
            source: "   ",
            acquiredAt: DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("source");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IncrementStackCount
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IncrementStackCount_IncrementsCount()
    {
        // Arrange
        var trauma = CreateTestTrauma();

        // Act
        trauma.IncrementStackCount();

        // Assert
        trauma.StackCount.Should().Be(2);
    }

    [Test]
    public void IncrementStackCount_MultipleTimes_IncrementsCorrectly()
    {
        // Arrange
        var trauma = CreateTestTrauma();

        // Act
        trauma.IncrementStackCount(); // 2
        trauma.IncrementStackCount(); // 3
        trauma.IncrementStackCount(); // 4
        trauma.IncrementStackCount(); // 5

        // Assert
        trauma.StackCount.Should().Be(5);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SetManagementStart
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SetManagementStart_AfterAcquisition_Succeeds()
    {
        // Arrange
        var acquiredAt = DateTime.UtcNow;
        var managedAt = acquiredAt.AddDays(1);
        var trauma = CharacterTrauma.Create(_characterId, ValidTraumaId, ValidSource, acquiredAt);

        // Act
        trauma.SetManagementStart(managedAt);

        // Assert
        trauma.ManagedSince.Should().Be(managedAt);
    }

    [Test]
    public void SetManagementStart_AtExactAcquisitionTime_Succeeds()
    {
        // Arrange
        var acquiredAt = DateTime.UtcNow;
        var trauma = CharacterTrauma.Create(_characterId, ValidTraumaId, ValidSource, acquiredAt);

        // Act
        trauma.SetManagementStart(acquiredAt);

        // Assert
        trauma.ManagedSince.Should().Be(acquiredAt);
    }

    [Test]
    public void SetManagementStart_BeforeAcquisition_ThrowsArgumentException()
    {
        // Arrange
        var acquiredAt = DateTime.UtcNow;
        var managedAt = acquiredAt.AddDays(-1);
        var trauma = CharacterTrauma.Create(_characterId, ValidTraumaId, ValidSource, acquiredAt);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => trauma.SetManagementStart(managedAt));
        ex!.ParamName.Should().Be("managedSince");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SetNotes
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SetNotes_UpdatesNotes()
    {
        // Arrange
        var trauma = CreateTestTrauma();
        var notes = "Acquired during the Siege of Thornhold";

        // Act
        trauma.SetNotes(notes);

        // Assert
        trauma.Notes.Should().Be(notes);
    }

    [Test]
    public void SetNotes_WithNull_ClearsNotes()
    {
        // Arrange
        var trauma = CreateTestTrauma();
        trauma.SetNotes("Initial notes");

        // Act
        trauma.SetNotes(null);

        // Assert
        trauma.Notes.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SetActive
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SetActive_ToFalse_DeactivatesTrauma()
    {
        // Arrange
        var trauma = CreateTestTrauma();
        trauma.IsActive.Should().BeTrue(); // Verify initial state

        // Act
        trauma.SetActive(false);

        // Assert
        trauma.IsActive.Should().BeFalse();
    }

    [Test]
    public void SetActive_ToTrue_ActivatesTrauma()
    {
        // Arrange
        var trauma = CreateTestTrauma();
        trauma.SetActive(false);

        // Act
        trauma.SetActive(true);

        // Assert
        trauma.IsActive.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToString_ActiveTrauma_ReturnsBasicFormat()
    {
        // Arrange
        var trauma = CreateTestTrauma();

        // Act
        var result = trauma.ToString();

        // Assert
        result.Should().Be("[survivors-guilt] x1");
        result.Should().NotContain("[MANAGED]");
    }

    [Test]
    public void ToString_WithStackCount_IncludesStackCount()
    {
        // Arrange
        var trauma = CreateTestTrauma();
        trauma.IncrementStackCount();
        trauma.IncrementStackCount();

        // Act
        var result = trauma.ToString();

        // Assert
        result.Should().Be("[survivors-guilt] x3");
    }

    [Test]
    public void ToString_InactiveTrauma_IncludesManagedTag()
    {
        // Arrange
        var trauma = CreateTestTrauma();
        trauma.SetActive(false);

        // Act
        var result = trauma.ToString();

        // Assert
        result.Should().Contain("[MANAGED]");
    }

    [Test]
    public void ToString_WithManagedSince_IncludesDate()
    {
        // Arrange
        var acquiredAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var managedAt = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var trauma = CharacterTrauma.Create(_characterId, ValidTraumaId, ValidSource, acquiredAt);
        trauma.SetManagementStart(managedAt);

        // Act
        var result = trauma.ToString();

        // Assert
        result.Should().Contain("(since 2026-01-15)");
    }

    [Test]
    public void ToString_InactiveWithManagedSince_IncludesBothTags()
    {
        // Arrange
        var acquiredAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var managedAt = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var trauma = CharacterTrauma.Create(_characterId, ValidTraumaId, ValidSource, acquiredAt);
        trauma.SetManagementStart(managedAt);
        trauma.SetActive(false);

        // Act
        var result = trauma.ToString();

        // Assert
        result.Should().Contain("[MANAGED]");
        result.Should().Contain("(since 2026-01-15)");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    private CharacterTrauma CreateTestTrauma()
    {
        return CharacterTrauma.Create(
            characterId: _characterId,
            traumaDefinitionId: ValidTraumaId,
            source: ValidSource,
            acquiredAt: DateTime.UtcNow
        );
    }
}
