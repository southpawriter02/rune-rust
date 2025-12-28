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
/// Integration tests for the SpecializationRepository using InMemory database.
/// </summary>
/// <remarks>See: v0.4.1a for specialization system implementation.</remarks>
public class SpecializationRepositoryTests
{
    private RuneAndRustDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RuneAndRustDbContext(options);
    }

    private SpecializationRepository GetRepository(RuneAndRustDbContext context)
    {
        var logger = Mock.Of<ILogger<SpecializationRepository>>();
        return new SpecializationRepository(context, logger);
    }

    private async Task SeedAbilitiesAsync(RuneAndRustDbContext context)
    {
        var abilities = new List<ActiveAbility>
        {
            new ActiveAbility { Name = "Test Ability 1", Description = "Test 1", EffectScript = "DAMAGE:Physical:1d6" },
            new ActiveAbility { Name = "Test Ability 2", Description = "Test 2", EffectScript = "DAMAGE:Physical:1d6" },
            new ActiveAbility { Name = "Test Ability 3", Description = "Test 3", EffectScript = "DAMAGE:Physical:1d6" }
        };
        await context.ActiveAbilities.AddRangeAsync(abilities);
        await context.SaveChangesAsync();
    }

    private async Task<Specialization> SeedSpecializationAsync(RuneAndRustDbContext context)
    {
        await SeedAbilitiesAsync(context);
        var abilities = await context.ActiveAbilities.ToListAsync();

        var spec = new Specialization
        {
            Type = SpecializationType.Berserkr,
            Name = "Berserkr",
            Description = "Rage fighter",
            RequiredArchetype = ArchetypeType.Warrior,
            RequiredLevel = 1
        };

        var node1 = new SpecializationNode
        {
            SpecializationId = spec.Id,
            AbilityId = abilities[0].Id,
            Ability = abilities[0],
            Tier = 1,
            CostPP = 1,
            PositionX = 0,
            PositionY = 0,
            ParentNodeIds = new List<Guid>()
        };

        var node2 = new SpecializationNode
        {
            SpecializationId = spec.Id,
            AbilityId = abilities[1].Id,
            Ability = abilities[1],
            Tier = 2,
            CostPP = 2,
            PositionX = 0,
            PositionY = 1,
            ParentNodeIds = new List<Guid> { node1.Id }
        };

        spec.Nodes = new List<SpecializationNode> { node1, node2 };

        await context.Specializations.AddAsync(spec);
        await context.SaveChangesAsync();

        return spec;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await SeedSpecializationAsync(context);

        // Act
        var results = await repo.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
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

    [Fact]
    public async Task GetAllAsync_IncludesNodes()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await SeedSpecializationAsync(context);

        // Act
        var results = await repo.GetAllAsync();
        var spec = results.First();

        // Assert
        spec.Nodes.Should().HaveCount(2);
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

    [Fact]
    public async Task GetByIdAsync_ReturnsWithNodes()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var seeded = await SeedSpecializationAsync(context);

        // Act
        var result = await repo.GetByIdAsync(seeded.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Nodes.Should().HaveCount(2);
        result.Name.Should().Be("Berserkr");
    }

    [Fact]
    public async Task GetByTypeAsync_FindsByEnum()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await SeedSpecializationAsync(context);

        // Act
        var result = await repo.GetByTypeAsync(SpecializationType.Berserkr);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(SpecializationType.Berserkr);
        result.Name.Should().Be("Berserkr");
    }

    [Fact]
    public async Task GetByTypeAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await SeedSpecializationAsync(context);

        // Act
        var result = await repo.GetByTypeAsync(SpecializationType.Skald);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByArchetypeAsync_FiltersCorrectly()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await SeedSpecializationAsync(context);

        // Act
        var results = await repo.GetByArchetypeAsync(ArchetypeType.Warrior);

        // Assert
        results.Should().HaveCount(1);
        results.First().RequiredArchetype.Should().Be(ArchetypeType.Warrior);
    }

    [Fact]
    public async Task GetByArchetypeAsync_ReturnsEmpty_WhenNoMatch()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await SeedSpecializationAsync(context);

        // Act
        var results = await repo.GetByArchetypeAsync(ArchetypeType.Mystic);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNodeByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);

        // Act
        var result = await repo.GetNodeByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetNodeByIdAsync_ReturnsNode()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spec = await SeedSpecializationAsync(context);
        var nodeId = spec.Nodes.First().Id;

        // Act
        var result = await repo.GetNodeByIdAsync(nodeId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(nodeId);
    }

    [Fact]
    public async Task GetNodesForSpecializationAsync_ReturnsOrderedByTier()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spec = await SeedSpecializationAsync(context);

        // Act
        var results = (await repo.GetNodesForSpecializationAsync(spec.Id)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].Tier.Should().Be(1);
        results[1].Tier.Should().Be(2);
    }

    [Fact]
    public async Task GetUnlockedNodesAsync_ReturnsEmpty_WhenNone()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        await SeedSpecializationAsync(context);
        var characterId = Guid.NewGuid();

        // Act
        var results = await repo.GetUnlockedNodesAsync(characterId);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task RecordNodeUnlockAsync_CreatesProgress()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spec = await SeedSpecializationAsync(context);
        var characterId = Guid.NewGuid();
        var nodeId = spec.Nodes.First().Id;

        // Create a character for the FK
        var character = new Character { Id = characterId, Name = "Test Char" };
        await context.Characters.AddAsync(character);
        await context.SaveChangesAsync();

        // Act
        await repo.RecordNodeUnlockAsync(characterId, nodeId);
        await repo.SaveChangesAsync();

        // Assert
        var progress = await context.CharacterSpecializationProgress
            .FirstOrDefaultAsync(p => p.CharacterId == characterId && p.NodeId == nodeId);
        progress.Should().NotBeNull();
    }

    [Fact]
    public async Task RecordNodeUnlockAsync_SetsTimestamp()
    {
        // Arrange
        var context = GetContext();
        var repo = GetRepository(context);
        var spec = await SeedSpecializationAsync(context);
        var characterId = Guid.NewGuid();
        var nodeId = spec.Nodes.First().Id;
        var before = DateTime.UtcNow;

        // Create a character for the FK
        var character = new Character { Id = characterId, Name = "Test Char" };
        await context.Characters.AddAsync(character);
        await context.SaveChangesAsync();

        // Act
        await repo.RecordNodeUnlockAsync(characterId, nodeId);
        await repo.SaveChangesAsync();

        // Assert
        var progress = await context.CharacterSpecializationProgress
            .FirstOrDefaultAsync(p => p.CharacterId == characterId && p.NodeId == nodeId);
        progress.Should().NotBeNull();
        progress!.UnlockedAt.Should().BeOnOrAfter(before);
    }
}
