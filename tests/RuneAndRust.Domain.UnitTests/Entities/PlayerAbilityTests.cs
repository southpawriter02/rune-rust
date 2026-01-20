using FluentAssertions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class PlayerAbilityTests
{
    [Test]
    public void Create_WithUnlockedTrue_StartsReady()
    {
        // Arrange & Act
        var ability = PlayerAbility.Create("flame-bolt", isUnlocked: true);

        // Assert
        ability.AbilityDefinitionId.Should().Be("flame-bolt");
        ability.IsUnlocked.Should().BeTrue();
        ability.IsReady.Should().BeTrue();
        ability.CurrentCooldown.Should().Be(0);
        ability.TimesUsed.Should().Be(0);
        ability.UnlockedAt.Should().NotBeNull();
    }

    [Test]
    public void Use_SetsCooldownAndIncrementsTimesUsed()
    {
        // Arrange
        var ability = PlayerAbility.Create("frost-nova");

        // Act
        ability.Use(cooldownDuration: 4);

        // Assert
        ability.CurrentCooldown.Should().Be(4);
        ability.TimesUsed.Should().Be(1);
        ability.IsOnCooldown.Should().BeTrue();
        ability.IsReady.Should().BeFalse();
    }

    [Test]
    public void ReduceCooldown_DecreasesByOne()
    {
        // Arrange
        var ability = PlayerAbility.Create("frost-nova");
        ability.Use(cooldownDuration: 3);

        // Act
        ability.ReduceCooldown();

        // Assert
        ability.CurrentCooldown.Should().Be(2);
    }

    [Test]
    public void ReduceCooldown_DoesNotGoBelowZero()
    {
        // Arrange
        var ability = PlayerAbility.Create("flame-bolt");
        ability.Use(cooldownDuration: 1);

        // Act
        ability.ReduceCooldown(amount: 5);

        // Assert
        ability.CurrentCooldown.Should().Be(0);
        ability.IsOnCooldown.Should().BeFalse();
        ability.IsReady.Should().BeTrue();
    }

    [Test]
    public void IsReady_WhenCooldownZero_ReturnsTrue()
    {
        // Arrange
        var ability = PlayerAbility.Create("shield-bash");

        // Assert
        ability.CurrentCooldown.Should().Be(0);
        ability.IsReady.Should().BeTrue();
    }

    [Test]
    public void Unlock_SetsIsUnlockedAndTimestamp()
    {
        // Arrange
        var ability = PlayerAbility.Create("ultimate", isUnlocked: false);
        ability.UnlockedAt.Should().BeNull();

        // Act
        ability.Unlock();

        // Assert
        ability.IsUnlocked.Should().BeTrue();
        ability.UnlockedAt.Should().NotBeNull();
        ability.UnlockedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
