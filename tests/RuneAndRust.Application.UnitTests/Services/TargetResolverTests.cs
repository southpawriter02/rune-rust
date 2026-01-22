using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for TargetResolver service.
/// </summary>
[TestFixture]
public class TargetResolverTests
{
    private TargetResolver _resolver = null!;

    [SetUp]
    public void SetUp()
    {
        _resolver = new TargetResolver(NullLogger<TargetResolver>.Instance);
    }

    private static InitiativeRoll CreateInitiativeRoll(int rollValue, int modifier)
    {
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [rollValue]);
        return new InitiativeRoll(diceResult, modifier);
    }

    private static CombatEncounter CreateEncounterWithMonsters(int goblinCount, int skeletonCount = 0)
    {
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        var player = new Player("TestHero");
        encounter.AddCombatant(Combatant.ForPlayer(player, CreateInitiativeRoll(10, 0)));

        var goblinNum = 0;
        for (var i = 0; i < goblinCount; i++)
        {
            goblinNum++;
            var monster = Monster.CreateGoblin();
            var displayNum = goblinCount > 1 ? goblinNum : 0;
            encounter.AddCombatant(Combatant.ForMonster(monster, CreateInitiativeRoll(5, 0), displayNum));
        }

        var skelNum = 0;
        for (var i = 0; i < skeletonCount; i++)
        {
            skelNum++;
            var monster = Monster.CreateSkeleton();
            var displayNum = skeletonCount > 1 ? skelNum : 0;
            encounter.AddCombatant(Combatant.ForMonster(monster, CreateInitiativeRoll(4, 0), displayNum));
        }

        encounter.Start();
        return encounter;
    }

    [Test]
    public void ResolveMonsterTarget_WithNoTarget_ShouldReturnFirstMonster()
    {
        // Arrange
        var encounter = CreateEncounterWithMonsters(2);

        // Act
        var result = _resolver.ResolveMonsterTarget(encounter, null);

        // Assert
        result.Success.Should().BeTrue();
        result.Target.Should().NotBeNull();
        result.Target!.IsMonster.Should().BeTrue();
    }

    [Test]
    public void ResolveMonsterTarget_WithNumber_ShouldReturnCorrectMonster()
    {
        // Arrange
        var encounter = CreateEncounterWithMonsters(3);

        // Act
        var result = _resolver.ResolveMonsterTarget(encounter, "2");

        // Assert
        result.Success.Should().BeTrue();
        result.Target.Should().NotBeNull();
    }

    [Test]
    public void ResolveMonsterTarget_WithInvalidNumber_ShouldReturnNotFound()
    {
        // Arrange
        var encounter = CreateEncounterWithMonsters(2);

        // Act
        var result = _resolver.ResolveMonsterTarget(encounter, "5");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no enemy #5");
    }

    [Test]
    public void ResolveMonsterTarget_WithName_ShouldReturnMatchingMonster()
    {
        // Arrange
        var encounter = CreateEncounterWithMonsters(1, skeletonCount: 1);

        // Act
        var result = _resolver.ResolveMonsterTarget(encounter, "skeleton");

        // Assert
        result.Success.Should().BeTrue();
        result.Target!.DisplayName.Should().Be("Skeleton");
    }

    [Test]
    public void ResolveMonsterTarget_WithAmbiguousName_ShouldReturnFirstAndMarkAmbiguous()
    {
        // Arrange
        var encounter = CreateEncounterWithMonsters(3);

        // Act
        var result = _resolver.ResolveMonsterTarget(encounter, "goblin");

        // Assert
        result.Success.Should().BeTrue();
        result.IsAmbiguous.Should().BeTrue();
        result.Target.Should().NotBeNull();
    }

    [Test]
    public void ResolveMonsterTarget_WithNameAndNumber_ShouldReturnSpecificMonster()
    {
        // Arrange
        var encounter = CreateEncounterWithMonsters(3);

        // Act
        var result = _resolver.ResolveMonsterTarget(encounter, "goblin 2");

        // Assert
        result.Success.Should().BeTrue();
        result.Target.Should().NotBeNull();
    }

    [Test]
    public void ResolveMonsterTarget_WithUnknownName_ShouldReturnNotFound()
    {
        // Arrange
        var encounter = CreateEncounterWithMonsters(2);

        // Act
        var result = _resolver.ResolveMonsterTarget(encounter, "dragon");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No enemy matches");
    }

    [Test]
    public void ResolveMonsterTarget_WithNoMonsters_ShouldReturnNotFound()
    {
        // Arrange - encounter with only player (no monsters)
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        var player = new Player("TestHero");
        encounter.AddCombatant(Combatant.ForPlayer(player, CreateInitiativeRoll(10, 0)));
        // Note: Can't actually start with only one combatant, so use different approach

        var monster = Monster.CreateGoblin();
        monster.TakeDamage(1000); // Kill it
        encounter.AddCombatant(Combatant.ForMonster(monster, CreateInitiativeRoll(5, 0), 0));
        encounter.Start();

        // Act
        var result = _resolver.ResolveMonsterTarget(encounter, null);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no enemies");
    }

    [Test]
    public void GetValidTargets_ShouldReturnFormattedList()
    {
        // Arrange
        var encounter = CreateEncounterWithMonsters(2);

        // Act
        var targets = _resolver.GetValidTargets(encounter);

        // Assert
        targets.Should().HaveCount(2);
        targets[0].Should().Contain("[1]");
        targets[1].Should().Contain("[2]");
    }

    [Test]
    public void ResolveMonsterTarget_WithNullEncounter_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _resolver.ResolveMonsterTarget(null!, "1");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
