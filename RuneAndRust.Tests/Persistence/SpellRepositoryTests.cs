using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using Xunit;

namespace RuneAndRust.Tests.Persistence;

/// <summary>
/// Integration tests for the SpellRepository using InMemory database.
/// </summary>
/// <remarks>See: v0.4.3b (The Grimoire) for spell system implementation.</remarks>
public class SpellRepositoryTests
{
    private RuneAndRustDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RuneAndRustDbContext(options);
    }

    private SpellRepository GetRepository(RuneAndRustDbContext context)
    {
        var genericLogger = Mock.Of<ILogger<GenericRepository<Spell>>>();
        var spellLogger = Mock.Of<ILogger<SpellRepository>>();
        return new SpellRepository(context, genericLogger, spellLogger);
    }

    private Spell CreateTestSpell(
        string name = "Test Spell",
        SpellSchool school = SpellSchool.Destruction,
        SpellTargetType targetType = SpellTargetType.SingleEnemy,
        SpellRange range = SpellRange.Medium,
        int tier = 1,
        int chargeTurns = 0,
        bool requiresConcentration = false,
        ArchetypeType? archetype = null)
    {
        return new Spell
        {
            Name = name,
            Description = $"A test spell called {name}",
            School = school,
            TargetType = targetType,
            Range = range,
            ApCost = 2,
            FluxCost = 5,
            BasePower = 10,
            Tier = tier,
            ChargeTurns = chargeTurns,
            RequiresConcentration = requiresConcentration,
            Archetype = archetype
        };
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Fireball"),
            CreateTestSpell("Ice Shard"),
            CreateTestSpell("Lightning Bolt")
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetAllAsync();

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNone()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);

        // Act
        var results = await repo.GetAllAsync();

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsSpell_WhenFound()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spell = CreateTestSpell("Fireball");
        await context.Spells.AddAsync(spell);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByIdAsync(spell.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Fireball");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);

        // Act
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetBySchoolAsync Tests

    [Fact]
    public async Task GetBySchoolAsync_FiltersCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Fireball", school: SpellSchool.Destruction),
            CreateTestSpell("Heal", school: SpellSchool.Restoration),
            CreateTestSpell("Ice Shard", school: SpellSchool.Destruction)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetBySchoolAsync(SpellSchool.Destruction);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(s => s.School.Should().Be(SpellSchool.Destruction));
    }

    [Fact]
    public async Task GetBySchoolAsync_ReturnsEmpty_WhenNoMatch()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddAsync(CreateTestSpell("Fireball", school: SpellSchool.Destruction));
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetBySchoolAsync(SpellSchool.Divination);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySchoolAsync_OrdersByTierThenName()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Zap", school: SpellSchool.Destruction, tier: 2),
            CreateTestSpell("Alpha Strike", school: SpellSchool.Destruction, tier: 1),
            CreateTestSpell("Beta Strike", school: SpellSchool.Destruction, tier: 1)
        });
        await context.SaveChangesAsync();

        // Act
        var results = (await repo.GetBySchoolAsync(SpellSchool.Destruction)).ToList();

        // Assert
        results[0].Name.Should().Be("Alpha Strike");
        results[1].Name.Should().Be("Beta Strike");
        results[2].Name.Should().Be("Zap");
    }

    #endregion

    #region GetByTargetTypeAsync Tests

    [Fact]
    public async Task GetByTargetTypeAsync_FiltersCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Fireball", targetType: SpellTargetType.SingleEnemy),
            CreateTestSpell("Heal", targetType: SpellTargetType.SingleAlly),
            CreateTestSpell("Ice Shard", targetType: SpellTargetType.SingleEnemy)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByTargetTypeAsync(SpellTargetType.SingleEnemy);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(s => s.TargetType.Should().Be(SpellTargetType.SingleEnemy));
    }

    [Fact]
    public async Task GetByTargetTypeAsync_ReturnsEmpty_WhenNoMatch()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddAsync(CreateTestSpell("Fireball", targetType: SpellTargetType.SingleEnemy));
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByTargetTypeAsync(SpellTargetType.AllEnemies);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region GetByMaxTierAsync Tests

    [Fact]
    public async Task GetByMaxTierAsync_FiltersCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Novice Spell", tier: 1),
            CreateTestSpell("Apprentice Spell", tier: 2),
            CreateTestSpell("Master Spell", tier: 4)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByMaxTierAsync(2);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(s => s.Tier.Should().BeLessThanOrEqualTo(2));
    }

    [Fact]
    public async Task GetByMaxTierAsync_ReturnsAll_WhenMaxTierHigh()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Tier1", tier: 1),
            CreateTestSpell("Tier2", tier: 2),
            CreateTestSpell("Tier3", tier: 3)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByMaxTierAsync(10);

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByMaxTierAsync_OrdersByTierThenName()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Zap", tier: 2),
            CreateTestSpell("Alpha", tier: 1),
            CreateTestSpell("Beta", tier: 1)
        });
        await context.SaveChangesAsync();

        // Act
        var results = (await repo.GetByMaxTierAsync(2)).ToList();

        // Assert
        results[0].Name.Should().Be("Alpha");
        results[1].Name.Should().Be("Beta");
        results[2].Name.Should().Be("Zap");
    }

    #endregion

    #region GetByArchetypeAsync Tests

    [Fact]
    public async Task GetByArchetypeAsync_ReturnsUnrestrictedSpells()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Universal Spell", archetype: null),
            CreateTestSpell("Warrior Only", archetype: ArchetypeType.Warrior)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByArchetypeAsync(ArchetypeType.Mystic);

        // Assert
        results.Should().HaveCount(1);
        results.First().Name.Should().Be("Universal Spell");
    }

    [Fact]
    public async Task GetByArchetypeAsync_IncludesArchetypeSpecificSpells()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Universal Spell", archetype: null),
            CreateTestSpell("Warrior Only", archetype: ArchetypeType.Warrior)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByArchetypeAsync(ArchetypeType.Warrior);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByArchetypeAsync_ExcludesOtherArchetypeSpells()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Mystic Only", archetype: ArchetypeType.Mystic),
            CreateTestSpell("Skirmisher Only", archetype: ArchetypeType.Skirmisher)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByArchetypeAsync(ArchetypeType.Warrior);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_FindsExactMatch()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddAsync(CreateTestSpell("Fireball"));
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByNameAsync("Fireball");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Fireball");
    }

    [Fact]
    public async Task GetByNameAsync_IsCaseInsensitive()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddAsync(CreateTestSpell("Fireball"));
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByNameAsync("FIREBALL");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Fireball");
    }

    [Fact]
    public async Task GetByNameAsync_TrimsWhitespace()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddAsync(CreateTestSpell("Fireball"));
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByNameAsync("  Fireball  ");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Fireball");
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddAsync(CreateTestSpell("Fireball"));
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByNameAsync("Ice Shard");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetChargedSpellsAsync Tests

    [Fact]
    public async Task GetChargedSpellsAsync_FiltersCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Instant", chargeTurns: 0),
            CreateTestSpell("Charged1", chargeTurns: 1),
            CreateTestSpell("Charged2", chargeTurns: 2)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetChargedSpellsAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(s => s.ChargeTurns.Should().BeGreaterThan(0));
    }

    [Fact]
    public async Task GetChargedSpellsAsync_OrdersByChargeTurns()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Slow", chargeTurns: 3),
            CreateTestSpell("Fast", chargeTurns: 1),
            CreateTestSpell("Medium", chargeTurns: 2)
        });
        await context.SaveChangesAsync();

        // Act
        var results = (await repo.GetChargedSpellsAsync()).ToList();

        // Assert
        results[0].ChargeTurns.Should().Be(1);
        results[1].ChargeTurns.Should().Be(2);
        results[2].ChargeTurns.Should().Be(3);
    }

    #endregion

    #region GetInstantSpellsAsync Tests

    [Fact]
    public async Task GetInstantSpellsAsync_FiltersCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Instant1", chargeTurns: 0),
            CreateTestSpell("Instant2", chargeTurns: 0),
            CreateTestSpell("Charged", chargeTurns: 2)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetInstantSpellsAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(s => s.ChargeTurns.Should().Be(0));
    }

    [Fact]
    public async Task GetInstantSpellsAsync_OrdersByName()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Zap", chargeTurns: 0),
            CreateTestSpell("Alpha", chargeTurns: 0)
        });
        await context.SaveChangesAsync();

        // Act
        var results = (await repo.GetInstantSpellsAsync()).ToList();

        // Assert
        results[0].Name.Should().Be("Alpha");
        results[1].Name.Should().Be("Zap");
    }

    #endregion

    #region GetConcentrationSpellsAsync Tests

    [Fact]
    public async Task GetConcentrationSpellsAsync_FiltersCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Concentrate1", requiresConcentration: true),
            CreateTestSpell("Concentrate2", requiresConcentration: true),
            CreateTestSpell("Normal", requiresConcentration: false)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetConcentrationSpellsAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(s => s.RequiresConcentration.Should().BeTrue());
    }

    [Fact]
    public async Task GetConcentrationSpellsAsync_ReturnsEmpty_WhenNone()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddAsync(CreateTestSpell("Normal", requiresConcentration: false));
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetConcentrationSpellsAsync();

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region GetByRangeAsync Tests

    [Fact]
    public async Task GetByRangeAsync_FiltersCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Touch1", range: SpellRange.Touch),
            CreateTestSpell("Touch2", range: SpellRange.Touch),
            CreateTestSpell("Far", range: SpellRange.Far)
        });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByRangeAsync(SpellRange.Touch);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(s => s.Range.Should().Be(SpellRange.Touch));
    }

    [Fact]
    public async Task GetByRangeAsync_ReturnsEmpty_WhenNoMatch()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddAsync(CreateTestSpell("Far", range: SpellRange.Far));
        await context.SaveChangesAsync();

        // Act
        var results = await repo.GetByRangeAsync(SpellRange.Self);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_AddsMultipleSpells()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spells = new[]
        {
            CreateTestSpell("Spell1"),
            CreateTestSpell("Spell2"),
            CreateTestSpell("Spell3")
        };

        // Act
        await repo.AddRangeAsync(spells);
        await repo.SaveChangesAsync();

        // Assert
        var count = await context.Spells.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task AddRangeAsync_HandlesEmptyList()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);

        // Act
        await repo.AddRangeAsync(Array.Empty<Spell>());
        await repo.SaveChangesAsync();

        // Assert
        var count = await context.Spells.CountAsync();
        count.Should().Be(0);
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public async Task AddAsync_AddsSpell()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spell = CreateTestSpell("Fireball");

        // Act
        await repo.AddAsync(spell);
        await repo.SaveChangesAsync();

        // Assert
        var result = await context.Spells.FindAsync(spell.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Fireball");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesSpell()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spell = CreateTestSpell("Fireball");
        await context.Spells.AddAsync(spell);
        await context.SaveChangesAsync();

        // Act
        spell.BasePower = 50;
        await repo.UpdateAsync(spell);
        await repo.SaveChangesAsync();

        // Assert
        var result = await context.Spells.FindAsync(spell.Id);
        result!.BasePower.Should().Be(50);
    }

    [Fact]
    public async Task DeleteAsync_RemovesSpell()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spell = CreateTestSpell("Fireball");
        await context.Spells.AddAsync(spell);
        await context.SaveChangesAsync();

        // Act
        await repo.DeleteAsync(spell.Id);
        await repo.SaveChangesAsync();

        // Assert
        var result = await context.Spells.FindAsync(spell.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_HandlesNonExistent()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);

        // Act & Assert (should not throw)
        await repo.DeleteAsync(Guid.NewGuid());
        await repo.SaveChangesAsync();
    }

    #endregion

    #region Computed Properties Tests

    [Fact]
    public async Task Spell_IsInstantCast_ComputedCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await context.Spells.AddRangeAsync(new[]
        {
            CreateTestSpell("Instant", chargeTurns: 0),
            CreateTestSpell("Charged", chargeTurns: 2)
        });
        await context.SaveChangesAsync();

        // Act
        var instant = await repo.GetByNameAsync("Instant");
        var charged = await repo.GetByNameAsync("Charged");

        // Assert
        instant!.IsInstantCast.Should().BeTrue();
        charged!.IsInstantCast.Should().BeFalse();
    }

    [Fact]
    public async Task Spell_TotalCost_ComputedCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spell = new Spell
        {
            Name = "Cost Test",
            Description = "Testing cost",
            ApCost = 3,
            FluxCost = 7
        };
        await context.Spells.AddAsync(spell);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByNameAsync("Cost Test");

        // Assert
        result!.TotalCost.Should().Be(10); // 3 + 7
    }

    #endregion
}
