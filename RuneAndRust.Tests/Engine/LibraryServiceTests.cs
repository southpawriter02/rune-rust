using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Attributes;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the LibraryService class (v0.3.11a - Dynamic Knowledge Engine).
/// Validates reflection-based Field Guide entry generation from [GameDocument] attributes.
/// </summary>
public class LibraryServiceTests
{
    private readonly Mock<ILogger<LibraryService>> _mockLogger;
    private readonly LibraryService _sut;

    public LibraryServiceTests()
    {
        _mockLogger = new Mock<ILogger<LibraryService>>();
        _sut = new LibraryService(_mockLogger.Object);
    }

    #region GetSystemEntries Tests

    [Fact]
    public void GetSystemEntries_FindsAttributedEnumValues()
    {
        // Act
        var entries = _sut.GetSystemEntries();

        // Assert
        entries.Should().NotBeEmpty("StatusEffectType has [GameDocument] attributes");
        entries.Should().Contain(e => e.Title == "Bleeding");
    }

    [Fact]
    public void GetSystemEntries_IgnoresUndocumentedMembers()
    {
        // Act
        var entries = _sut.GetSystemEntries();

        // Assert - EntryCategory has no [GameDocument] attributes
        entries.Should().NotContain(e => e.Title == "FieldGuide");
        entries.Should().NotContain(e => e.Title == "Bestiary");
    }

    [Fact]
    public void GetSystemEntries_GeneratesDeterministicIds()
    {
        // Act
        var entries1 = _sut.GetSystemEntries().ToList();
        var entries2 = _sut.GetSystemEntries().ToList();

        // Assert - Same input should produce same GUID
        var bleeding1 = entries1.First(e => e.Title == "Bleeding");
        var bleeding2 = entries2.First(e => e.Title == "Bleeding");
        bleeding1.Id.Should().Be(bleeding2.Id);
    }

    [Fact]
    public void GetSystemEntries_ExtractsCorrectTitle()
    {
        // Act
        var entries = _sut.GetSystemEntries();

        // Assert
        var stunned = entries.FirstOrDefault(e => e.Title == "Stunned");
        stunned.Should().NotBeNull();
    }

    [Fact]
    public void GetSystemEntries_ExtractsCorrectDescription()
    {
        // Act
        var entries = _sut.GetSystemEntries();

        // Assert
        var bleeding = entries.First(e => e.Title == "Bleeding");
        bleeding.FullText.Should().Contain("physical affliction");
    }

    [Fact]
    public void GetSystemEntries_ExtractsCorrectCategory()
    {
        // Act
        var entries = _sut.GetSystemEntries();

        // Assert - Default category is FieldGuide
        var bleeding = entries.First(e => e.Title == "Bleeding");
        bleeding.Category.Should().Be(EntryCategory.FieldGuide);
    }

    [Fact]
    public void GetSystemEntries_SetsFullTextFromDescription()
    {
        // Act
        var entries = _sut.GetSystemEntries();

        // Assert
        var vulnerable = entries.First(e => e.Title == "Vulnerable");
        vulnerable.FullText.Should().NotBeNullOrEmpty();
        vulnerable.FullText.Should().Contain("compromised");
    }

    [Fact]
    public void GetSystemEntries_SetsTotalFragmentsToOne()
    {
        // Act
        var entries = _sut.GetSystemEntries();

        // Assert - System entries are always "complete"
        entries.Should().OnlyContain(e => e.TotalFragments == 1);
    }

    #endregion

    #region GetEntriesByCategory Tests

    [Fact]
    public void GetEntriesByCategory_FiltersCorrectly()
    {
        // Act
        var fieldGuideEntries = _sut.GetEntriesByCategory(EntryCategory.FieldGuide);

        // Assert
        fieldGuideEntries.Should().NotBeEmpty();
        fieldGuideEntries.Should().OnlyContain(e => e.Category == EntryCategory.FieldGuide);
    }

    [Fact]
    public void GetEntriesByCategory_ReturnsEmptyForNoMatches()
    {
        // Act - Bestiary has no [GameDocument] attributes on enums
        var bestiaryEntries = _sut.GetEntriesByCategory(EntryCategory.Bestiary);

        // Assert
        bestiaryEntries.Should().BeEmpty("no enums are annotated with Bestiary category");
    }

    #endregion

    #region Caching Tests

    [Fact]
    public void GetSystemEntries_CachesResults()
    {
        // Act
        var entries1 = _sut.GetSystemEntries();
        var entries2 = _sut.GetSystemEntries();

        // Assert - Should return the same cached collection
        entries1.Should().BeSameAs(entries2);
    }

    [Fact]
    public void GetSystemEntries_ScansMultipleEnums()
    {
        // Act
        var entries = _sut.GetSystemEntries();

        // Assert - Should find entries from multiple enums
        entries.Should().Contain(e => e.Title == "Bleeding"); // StatusEffectType
        entries.Should().Contain(e => e.Title == "Sturdiness"); // Attribute
        entries.Should().Contain(e => e.Title == "Physical Damage"); // DamageType
    }

    #endregion

    #region Deterministic ID Tests

    [Fact]
    public void GenerateDeterministicId_SameInputProducesSameOutput()
    {
        // Act
        var id1 = LibraryService.GenerateDeterministicId("StatusEffectType:Bleeding");
        var id2 = LibraryService.GenerateDeterministicId("StatusEffectType:Bleeding");

        // Assert
        id1.Should().Be(id2);
    }

    [Fact]
    public void GenerateDeterministicId_DifferentInputsProduceDifferentOutputs()
    {
        // Act
        var id1 = LibraryService.GenerateDeterministicId("StatusEffectType:Bleeding");
        var id2 = LibraryService.GenerateDeterministicId("StatusEffectType:Poisoned");

        // Assert
        id1.Should().NotBe(id2);
    }

    #endregion

    #region Entry Count Validation

    [Fact]
    public void GetSystemEntries_FindsExpectedNumberOfEntries()
    {
        // Arrange - Total annotated values across all 13 enums:
        // StatusEffectType: 11, Attribute: 5, DamageType: 8, BiomeType: 4,
        // ItemType: 6, ConditionType: 8, TraumaType: 4, EquipmentSlot: 7,
        // CreatureTraitType: 8, HazardType: 3, AttackType: 3, QualityTier: 5,
        // ResourceType: 3 = 75 total
        var expectedMinimum = 70;

        // Act
        var entries = _sut.GetSystemEntries();

        // Assert
        entries.Count().Should().BeGreaterThanOrEqualTo(expectedMinimum,
            $"expected at least {expectedMinimum} annotated enum values");
    }

    #endregion
}
