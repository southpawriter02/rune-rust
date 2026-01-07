using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class PlayerAbilityMethodsTests
{
    [Test]
    public void Player_GetAbility_ReturnsAbility()
    {
        // Arrange
        var player = CreateTestPlayer();
        var ability = PlayerAbility.Create("flame-bolt");
        player.AddAbility(ability);

        // Act
        var result = player.GetAbility("flame-bolt");

        // Assert
        result.Should().NotBeNull();
        result!.AbilityDefinitionId.Should().Be("flame-bolt");
    }

    [Test]
    public void Player_HasAbility_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddAbility(PlayerAbility.Create("shield-bash"));

        // Act & Assert
        player.HasAbility("shield-bash").Should().BeTrue();
        player.HasAbility("nonexistent").Should().BeFalse();
    }

    [Test]
    public void Player_AddAbility_AddsToCollection()
    {
        // Arrange
        var player = CreateTestPlayer();
        var ability = PlayerAbility.Create("healing-word");

        // Act
        player.AddAbility(ability);

        // Assert
        player.Abilities.Should().HaveCount(1);
        player.HasAbility("healing-word").Should().BeTrue();
    }

    [Test]
    public void Player_GetReadyAbilities_ReturnsOnlyReady()
    {
        // Arrange
        var player = CreateTestPlayer();

        var readyAbility = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        var onCooldownAbility = PlayerAbility.Create("frost-nova", isUnlocked: true);
        onCooldownAbility.Use(cooldownDuration: 3);
        var lockedAbility = PlayerAbility.Create("ultimate", isUnlocked: false);

        player.AddAbility(readyAbility);
        player.AddAbility(onCooldownAbility);
        player.AddAbility(lockedAbility);

        // Act
        var readyAbilities = player.GetReadyAbilities().ToList();

        // Assert
        readyAbilities.Should().HaveCount(1);
        readyAbilities.First().AbilityDefinitionId.Should().Be("flame-bolt");
    }

    private static Player CreateTestPlayer()
    {
        return new Player("TestPlayer", new Stats(100, 10, 5));
    }
}
