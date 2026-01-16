using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="CombatSoundService"/> and <see cref="AbilitySoundService"/>.
/// </summary>
[TestFixture]
public class CombatSoundServiceTests
{
    private Mock<ISoundEffectService> _sfxMock = null!;
    private CombatSoundService _combatService = null!;
    private AbilitySoundService _abilityService = null!;

    [SetUp]
    public void SetUp()
    {
        _sfxMock = new Mock<ISoundEffectService>();

        _combatService = new CombatSoundService(
            _sfxMock.Object,
            NullLogger<CombatSoundService>.Instance);

        _abilityService = new AbilitySoundService(
            _sfxMock.Object,
            NullLogger<AbilitySoundService>.Instance);
    }

    /// <summary>
    /// Verifies PlayAttackHit plays attack-hit sound.
    /// </summary>
    [Test]
    public void PlayAttackHit_Normal_PlaysAttackHit()
    {
        // Act
        _combatService.PlayAttackHit(isCritical: false);

        // Assert
        _sfxMock.Verify(s => s.Play("attack-hit", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayAttackHit critical plays attack-critical sound.
    /// </summary>
    [Test]
    public void PlayAttackHit_Critical_PlaysAttackCritical()
    {
        // Act
        _combatService.PlayAttackHit(isCritical: true);

        // Assert
        _sfxMock.Verify(s => s.Play("attack-critical", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayDamage maps fire type to damage-fire.
    /// </summary>
    [Test]
    public void PlayDamage_FireType_PlaysDamageFire()
    {
        // Act
        _combatService.PlayDamage("fire");

        // Assert
        _sfxMock.Verify(s => s.Play("damage-fire", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayDamage maps ice/cold/frost to damage-ice.
    /// </summary>
    [Test]
    public void PlayDamage_IceType_PlaysDamageIce()
    {
        // Act
        _combatService.PlayDamage("cold");

        // Assert
        _sfxMock.Verify(s => s.Play("damage-ice", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayDamage with unknown type plays nothing.
    /// </summary>
    [Test]
    public void PlayDamage_UnknownType_PlaysNothing()
    {
        // Act
        _combatService.PlayDamage("physical");

        // Assert
        _sfxMock.Verify(s => s.Play(It.IsAny<string>(), It.IsAny<float>()), Times.Never);
    }

    /// <summary>
    /// Verifies PlayAbilityCast maps fire school to ability-fire.
    /// </summary>
    [Test]
    public void PlayAbilityCast_FireSchool_PlaysAbilityFire()
    {
        // Act
        _abilityService.PlayAbilityCast("fire");

        // Assert
        _sfxMock.Verify(s => s.Play("ability-fire", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayAbilityCast unknown school falls back to ability-cast.
    /// </summary>
    [Test]
    public void PlayAbilityCast_UnknownSchool_FallsBackToGeneric()
    {
        // Act
        _abilityService.PlayAbilityCast("arcane");

        // Assert
        _sfxMock.Verify(s => s.Play("ability-cast", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayHealReceived plays ability-heal.
    /// </summary>
    [Test]
    public void PlayHealReceived_PlaysAbilityHeal()
    {
        // Act
        _abilityService.PlayHealReceived();

        // Assert
        _sfxMock.Verify(s => s.Play("ability-heal", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayMonsterDeath plays death-monster.
    /// </summary>
    [Test]
    public void PlayMonsterDeath_PlaysDeathMonster()
    {
        // Act
        _combatService.PlayMonsterDeath();

        // Assert
        _sfxMock.Verify(s => s.Play("death-monster", 1.0f), Times.Once);
    }
}
