using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Builders;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for DefenseActionService.
/// </summary>
/// <remarks>
/// <para>Tests cover all three defense actions:</para>
/// <list type="bullet">
///   <item><description>Block: Shield-based damage reduction</description></item>
///   <item><description>Dodge: Reaction-based evasion roll</description></item>
///   <item><description>Parry: Reaction-based deflection with counter-attack</description></item>
/// </list>
/// <para>Also tests reaction management and eligibility checks.</para>
/// </remarks>
[TestFixture]
public class DefenseActionServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<DefenseActionService>> _mockLogger = null!;
    private DefenseActionService _service = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockDiceService = new Mock<IDiceService>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<DefenseActionService>>();
        _service = new DefenseActionService(
            _mockDiceService.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // BLOCK ELIGIBILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a player with a shield equipped can block.
    /// </summary>
    [Test]
    public void CanBlock_WithShield_ReturnsTrue()
    {
        // Arrange
        var player = PlayerBuilder.Create()
            .WithAttributes(10, 10, 10, 10, 10)
            .Build();
        var shield = Item.CreateWoodenShield();
        player.TryEquip(shield);
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        // Act
        var result = _service.CanBlock(combatant);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a player without a shield cannot block.
    /// </summary>
    [Test]
    public void CanBlock_WithoutShield_ReturnsFalse()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        // Act
        var result = _service.CanBlock(combatant);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // BLOCK EXECUTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that block reduces damage by 50% base reduction.
    /// </summary>
    [Test]
    public void UseBlock_ReducesDamageBy50Percent()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var shield = Item.CreateWoodenShield(); // DefenseBonus = 1
        player.TryEquip(shield);
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var incomingDamage = 20;

        // Act
        var result = _service.UseBlock(combatant, incomingDamage);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // 20 * 0.5 = 10, then - 1 shield bonus = 9 final damage
        result.FinalDamage.Should().Be(9);
    }

    /// <summary>
    /// Verifies that block adds shield defense bonus to reduction.
    /// </summary>
    [Test]
    public void UseBlock_AddsShieldBonus()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var shield = Item.CreateWoodenShield(); // DefenseBonus = 1
        player.TryEquip(shield);
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var incomingDamage = 10;

        // Act
        var result = _service.UseBlock(combatant, incomingDamage);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ShieldBonus.Should().Be(1);
        // 10 * 0.5 = 5, then - 1 shield bonus = 4 final damage
        result.FinalDamage.Should().Be(4);
        result.DamagePrevented.Should().Be(6); // 10 - 4 = 6
    }

    /// <summary>
    /// Verifies that block fails without a shield equipped.
    /// </summary>
    [Test]
    public void UseBlock_WithoutShield_ReturnsFailed()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        // Act
        var result = _service.UseBlock(combatant, 20);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Contain("shield");
    }

    // ═══════════════════════════════════════════════════════════════
    // DODGE ELIGIBILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a player with reaction available can dodge.
    /// </summary>
    [Test]
    public void CanDodge_WithReaction_ReturnsTrue()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());
        // Player starts with reaction available

        // Act
        var result = _service.CanDodge(combatant);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a player in heavy armor cannot dodge.
    /// </summary>
    [Test]
    public void CanDodge_InHeavyArmor_ReturnsFalse()
    {
        // Arrange - Create player with high attributes to meet plate armor requirements
        var player = PlayerBuilder.Create()
            .WithAttributes(14, 14, 10, 10, 10)
            .Build();
        var plateArmor = Item.CreatePlateArmor(); // Heavy armor
        player.TryEquip(plateArmor);
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        // Act
        var result = _service.CanDodge(combatant);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // DODGE EXECUTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a successful dodge roll avoids the attack entirely.
    /// </summary>
    [Test]
    public void UseDodge_WhenRollMeetsAttack_AvoidsAttack()
    {
        // Arrange
        var player = PlayerBuilder.Create()
            .WithAttributes(10, 10, 10, 10, 16) // Finesse 16 -> modifier of 8
            .Build();
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var attackRoll = 15;

        // Mock dice roll: 1d10 = 10, + 8 finesse mod = 18 (beats 15)
        SetupDiceRoll(10);

        // Act
        var result = _service.UseDodge(combatant, attackRoll);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AvoidedAttack.Should().BeTrue();
        result.DodgeRoll.Should().Be(18); // 10 + 8
        result.AttackRoll.Should().Be(15);
    }

    /// <summary>
    /// Verifies that a failed dodge roll does not avoid the attack.
    /// </summary>
    [Test]
    public void UseDodge_WhenRollBelowAttack_DoesNotAvoid()
    {
        // Arrange
        var player = PlayerBuilder.Create()
            .WithAttributes(10, 10, 10, 10, 10) // Finesse 10 -> modifier of 5
            .Build();
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());
        var attackRoll = 18;

        // Mock dice roll: 1d10 = 5, + 5 finesse mod = 10 (below 18)
        SetupDiceRoll(5);

        // Act
        var result = _service.UseDodge(combatant, attackRoll);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Action succeeded
        result.AvoidedAttack.Should().BeFalse(); // But didn't avoid
        result.DodgeRoll.Should().Be(10); // 5 + 5
    }

    /// <summary>
    /// Verifies that dodge consumes the combatant's reaction.
    /// </summary>
    [Test]
    public void UseDodge_ConsumesReaction()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());
        SetupDiceRoll(10);

        combatant.HasReaction.Should().BeTrue();

        // Act
        _service.UseDodge(combatant, 15);

        // Assert
        combatant.HasReaction.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // PARRY ELIGIBILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a player with a melee weapon and reaction can parry.
    /// </summary>
    [Test]
    public void CanParry_WithMeleeWeapon_ReturnsTrue()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var sword = Item.CreateIronSword();
        player.TryEquip(sword);
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        // Act
        var result = _service.CanParry(combatant);

        // Assert
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // PARRY EXECUTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a successful parry triggers a counter-attack.
    /// </summary>
    [Test]
    public void UseParry_OnSuccess_TriggersCounter()
    {
        // Arrange
        var player = PlayerBuilder.Create()
            .WithAttributes(10, 10, 10, 10, 16) // Finesse 16 -> modifier of 8
            .WithStats(100, 10, 5)
            .Build();
        var sword = Item.CreateIronSword();
        player.TryEquip(sword);
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        var attackerMonster = MonsterBuilder.Goblin().Build();
        var attacker = Combatant.ForMonster(attackerMonster, CreateInitiativeRoll(5), 0);

        var attackRoll = 12;

        // Parry DC = 12 + 2 = 14
        // Mock parry roll: 1d10 = 10, + 8 finesse mod = 18 (beats 14)
        // Then counter-attack roll: 1d10 = 15 (hit), damage roll
        SetupSequentialDiceRolls(new[] { 10, 15, 6 });

        // Act
        var result = _service.UseParry(combatant, attacker, attackRoll);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Deflected.Should().BeTrue();
        result.CounterAttack.Should().NotBeNull();
        result.DC.Should().Be(14); // attackRoll + 2
    }

    /// <summary>
    /// Verifies that parry has a higher DC than dodge (attack + 2).
    /// </summary>
    [Test]
    public void UseParry_HasHigherDCThanDodge()
    {
        // Arrange
        var player = PlayerBuilder.Create()
            .WithAttributes(10, 10, 10, 10, 12) // Finesse 12 -> modifier of 6
            .Build();
        var sword = Item.CreateIronSword();
        player.TryEquip(sword);
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        var attackerMonster = MonsterBuilder.Goblin().Build();
        var attacker = Combatant.ForMonster(attackerMonster, CreateInitiativeRoll(5), 0);

        var attackRoll = 15;

        // Parry DC = 15 + 2 = 17
        // Mock roll: 1d10 = 10, + 6 finesse mod = 16 (fails vs 17)
        SetupDiceRoll(10);

        // Act
        var result = _service.UseParry(combatant, attacker, attackRoll);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Action executed
        result.Deflected.Should().BeFalse(); // But didn't deflect
        result.DC.Should().Be(17); // 15 + 2
        result.ParryRoll.Should().Be(16); // 10 + 6
    }

    /// <summary>
    /// Verifies that parry consumes the combatant's reaction.
    /// </summary>
    [Test]
    public void UseParry_ConsumesReaction()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var sword = Item.CreateIronSword();
        player.TryEquip(sword);
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        var attackerMonster = MonsterBuilder.Goblin().Build();
        var attacker = Combatant.ForMonster(attackerMonster, CreateInitiativeRoll(5), 0);

        SetupSequentialDiceRolls(new[] { 5, 10, 4 }); // Parry roll, counter roll, damage roll

        combatant.HasReaction.Should().BeTrue();

        // Act
        _service.UseParry(combatant, attacker, 15);

        // Assert
        combatant.HasReaction.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // REACTION MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ResetReaction restores the combatant's reaction.
    /// </summary>
    [Test]
    public void ResetReaction_RestoresReaction()
    {
        // Arrange
        var player = PlayerBuilder.Create().Build();
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());
        combatant.UseReaction(); // Consume reaction
        combatant.HasReaction.Should().BeFalse();

        // Act
        _service.ResetReaction(combatant);

        // Assert
        combatant.HasReaction.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid InitiativeRoll for testing.
    /// </summary>
    /// <param name="rollValue">The dice roll value.</param>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>An InitiativeRoll instance.</returns>
    private static InitiativeRoll CreateInitiativeRoll(int rollValue = 10, int modifier = 0)
    {
        var diceResult = new DiceRollResult
        {
            Pool = DicePool.Parse("1d10"),
            Rolls = new[] { rollValue },
            ExplosionRolls = Array.Empty<int>(),
            DiceTotal = rollValue,
            Total = rollValue
        };
        return new InitiativeRoll(diceResult, modifier);
    }

    /// <summary>
    /// Sets up the dice service mock to return a specific roll value.
    /// </summary>
    /// <param name="rollValue">The value to return from the dice roll.</param>
    private void SetupDiceRoll(int rollValue)
    {
        var rollResult = new DiceRollResult
        {
            Pool = DicePool.Parse("1d10"),
            Rolls = new[] { rollValue },
            ExplosionRolls = Array.Empty<int>(),
            DiceTotal = rollValue,
            Total = rollValue
        };

        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<string>(), It.IsAny<AdvantageType>()))
            .Returns(rollResult);
    }

    /// <summary>
    /// Sets up the dice service mock to return a sequence of roll values.
    /// </summary>
    /// <param name="rollValues">The sequence of values to return from dice rolls.</param>
    private void SetupSequentialDiceRolls(int[] rollValues)
    {
        var callCount = 0;

        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<string>(), It.IsAny<AdvantageType>()))
            .Returns(() =>
            {
                var value = rollValues[Math.Min(callCount, rollValues.Length - 1)];
                callCount++;
                return new DiceRollResult
                {
                    Pool = DicePool.Parse("1d10"),
                    Rolls = new[] { value },
                    ExplosionRolls = Array.Empty<int>(),
                    DiceTotal = value,
                    Total = value
                };
            });
    }
}
