// ═══════════════════════════════════════════════════════════════════════════════
// DispelEffectResultTests.cs
// Unit tests for the DispelEffectResult value object covering creation,
// empty factory, computed properties, and summary display.
// Version: 0.20.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class DispelEffectResultTests
{
    // ═══════ Creation Tests ═══════

    [Test]
    public void Create_WithEffects_CalculatesTotalCorrectly()
    {
        // Arrange
        var effects = new List<string> { "Shield of Faith", "Bless", "Haste" };
        var entities = new List<Guid> { Guid.NewGuid() };
        var items = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var characters = new List<Guid> { Guid.NewGuid() };

        // Act
        var result = DispelEffectResult.Create(effects, entities, items, characters);

        // Assert
        result.EffectsRemoved.Should().HaveCount(3);
        result.EntitiesDestroyed.Should().HaveCount(1);
        result.ItemsAffected.Should().HaveCount(2);
        result.AffectedCharacters.Should().HaveCount(1);
        result.TotalEffectsDispelled.Should().Be(6, "3 effects + 1 entity + 2 items = 6");
    }

    [Test]
    public void Create_WithNullArgument_ThrowsArgumentNullException()
    {
        // Act
        var act = () => DispelEffectResult.Create(
            null!,
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            Array.Empty<Guid>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════ Empty Factory Tests ═══════

    [Test]
    public void Empty_ReturnsZeroResult()
    {
        // Act
        var result = DispelEffectResult.Empty();

        // Assert
        result.EffectsRemoved.Should().BeEmpty();
        result.EntitiesDestroyed.Should().BeEmpty();
        result.ItemsAffected.Should().BeEmpty();
        result.AffectedCharacters.Should().BeEmpty();
        result.TotalEffectsDispelled.Should().Be(0);
        result.DestroyedEntities.Should().BeFalse();
    }

    // ═══════ Computed Property Tests ═══════

    [Test]
    public void DestroyedEntities_WhenPresent_ReturnsTrue()
    {
        // Arrange
        var result = DispelEffectResult.Create(
            Array.Empty<string>(),
            new List<Guid> { Guid.NewGuid() },
            Array.Empty<Guid>(),
            Array.Empty<Guid>());

        // Assert
        result.DestroyedEntities.Should().BeTrue();
    }

    [Test]
    public void DestroyedEntities_WhenEmpty_ReturnsFalse()
    {
        // Arrange
        var result = DispelEffectResult.Create(
            new List<string> { "Buff" },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            Array.Empty<Guid>());

        // Assert
        result.DestroyedEntities.Should().BeFalse();
    }

    // ═══════ Display Tests ═══════

    [Test]
    public void GetSummaryDisplay_WithEffects_ContainsAllCategories()
    {
        // Arrange
        var result = DispelEffectResult.Create(
            new List<string> { "Shield of Faith", "Bless" },
            new List<Guid> { Guid.NewGuid() },
            new List<Guid> { Guid.NewGuid() },
            new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

        // Act
        var display = result.GetSummaryDisplay();

        // Assert
        display.Should().Contain("Word of Unmaking");
        display.Should().Contain("Dispelled 4 effects");
        display.Should().Contain("Effects removed:");
        display.Should().Contain("Shield of Faith");
        display.Should().Contain("Entities destroyed: 1");
        display.Should().Contain("Characters affected: 2");
    }

    [Test]
    public void ToString_ReturnsHumanReadableSummary()
    {
        // Arrange
        var result = DispelEffectResult.Create(
            new List<string> { "Buff" },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            new List<Guid> { Guid.NewGuid() });

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("Dispel Result");
        str.Should().Contain("1 statuses");
        str.Should().Contain("1 characters affected");
    }
}
