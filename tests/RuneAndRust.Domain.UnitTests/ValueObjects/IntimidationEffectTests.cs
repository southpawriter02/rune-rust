using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="IntimidationEffect"/> value object.
/// </summary>
[TestFixture]
public class IntimidationEffectTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesCorrectly()
    {
        // Arrange
        var casterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        // Act
        var effect = IntimidationEffect.Create(casterId, targetId, "Draugr Warrior", 40);

        // Assert
        effect.CasterId.Should().Be(casterId);
        effect.TargetId.Should().Be(targetId);
        effect.TargetName.Should().Be("Draugr Warrior");
        effect.SaveDc.Should().Be(14); // 12 + 40/20 = 14
        effect.TurnsRemaining.Should().Be(3);
        effect.SaveRoll.Should().BeNull();
        effect.DidSave.Should().BeFalse();
        effect.ImmuneUntil.Should().BeNull();
        effect.EffectId.Should().NotBeEmpty();
    }

    // ===== Save DC Scaling Tests =====

    [Test]
    public void CalculateSaveDc_ScalesWithRage()
    {
        // Act & Assert — verify DC at each Rage tier
        IntimidationEffect.CalculateSaveDc(0).Should().Be(12);   // 12 + 0
        IntimidationEffect.CalculateSaveDc(19).Should().Be(12);  // 12 + 0
        IntimidationEffect.CalculateSaveDc(20).Should().Be(13);  // 12 + 1
        IntimidationEffect.CalculateSaveDc(40).Should().Be(14);  // 12 + 2
        IntimidationEffect.CalculateSaveDc(60).Should().Be(15);  // 12 + 3
        IntimidationEffect.CalculateSaveDc(80).Should().Be(16);  // 12 + 4
        IntimidationEffect.CalculateSaveDc(100).Should().Be(17); // 12 + 5
    }

    // ===== Failed Save Tests =====

    [Test]
    public void ApplySaveResult_FailedSave_AppliesPenalty()
    {
        // Arrange
        var effect = IntimidationEffect.Create(Guid.NewGuid(), Guid.NewGuid(), "Draugr", 40);
        // DC is 14

        // Act — roll of 12 < 14 = failed
        effect.ApplySaveResult(12);

        // Assert
        effect.DidSave.Should().BeFalse();
        effect.SaveRoll.Should().Be(12);
        effect.AttackPenalty.Should().Be(-2);
        effect.TurnsRemaining.Should().Be(3);
        effect.ImmuneUntil.Should().BeNull();
        effect.IsActive().Should().BeTrue();
    }

    // ===== Successful Save Tests =====

    [Test]
    public void ApplySaveResult_SuccessfulSave_GrantsImmunity()
    {
        // Arrange
        var effect = IntimidationEffect.Create(Guid.NewGuid(), Guid.NewGuid(), "Draugr", 40);
        // DC is 14

        // Act — roll of 16 >= 14 = saved
        effect.ApplySaveResult(16);

        // Assert
        effect.DidSave.Should().BeTrue();
        effect.SaveRoll.Should().Be(16);
        effect.TurnsRemaining.Should().Be(0); // Effect ends on successful save
        effect.ImmuneUntil.Should().NotBeNull();
        effect.ImmuneUntil.Should().BeCloseTo(
            DateTime.UtcNow.Add(IntimidationEffect.ImmunityDuration),
            TimeSpan.FromSeconds(1));
        effect.IsActive().Should().BeFalse();
        effect.IsImmune().Should().BeTrue();
    }

    [Test]
    public void ApplySaveResult_ExactlyMeetsDc_CountsAsSaved()
    {
        // Arrange
        var effect = IntimidationEffect.Create(Guid.NewGuid(), Guid.NewGuid(), "Draugr", 40);
        // DC is 14

        // Act — roll of 14 == 14 = saved (meets DC)
        effect.ApplySaveResult(14);

        // Assert
        effect.DidSave.Should().BeTrue();
    }

    // ===== Tick Tests =====

    [Test]
    public void Tick_DecrementsTurnsRemaining()
    {
        // Arrange
        var effect = IntimidationEffect.Create(Guid.NewGuid(), Guid.NewGuid(), "Draugr", 0);
        effect.ApplySaveResult(5); // Failed save (5 < DC 12)

        // Act & Assert
        effect.TurnsRemaining.Should().Be(3);
        effect.Tick();
        effect.TurnsRemaining.Should().Be(2);
        effect.IsActive().Should().BeTrue();

        effect.Tick();
        effect.TurnsRemaining.Should().Be(1);
        effect.IsActive().Should().BeTrue();

        effect.Tick();
        effect.TurnsRemaining.Should().Be(0);
        effect.IsActive().Should().BeFalse();
    }

    [Test]
    public void Tick_DoesNotGoBelowZero()
    {
        // Arrange
        var effect = IntimidationEffect.Create(Guid.NewGuid(), Guid.NewGuid(), "Draugr", 0);
        effect.ApplySaveResult(5); // Failed
        effect.Tick();
        effect.Tick();
        effect.Tick(); // Now at 0

        // Act
        effect.Tick(); // Extra tick

        // Assert
        effect.TurnsRemaining.Should().Be(0);
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_BeforeSave_ShowsPending()
    {
        // Arrange
        var effect = IntimidationEffect.Create(Guid.NewGuid(), Guid.NewGuid(), "Draugr", 40);

        // Act
        var description = effect.GetDescription();

        // Assert
        description.Should().Contain("Draugr");
        description.Should().Contain("Save pending");
        description.Should().Contain("DC 14");
    }

    [Test]
    public void GetDescription_AfterFailedSave_ShowsPenaltyAndDuration()
    {
        // Arrange
        var effect = IntimidationEffect.Create(Guid.NewGuid(), Guid.NewGuid(), "Draugr", 40);
        effect.ApplySaveResult(10);

        // Act
        var description = effect.GetDescription();

        // Assert
        description.Should().Contain("FAILED");
        description.Should().Contain("3 turns remaining");
        description.Should().Contain("-2 Attack");
    }

    [Test]
    public void GetDescription_AfterSuccessfulSave_ShowsImmunity()
    {
        // Arrange
        var effect = IntimidationEffect.Create(Guid.NewGuid(), Guid.NewGuid(), "Draugr", 40);
        effect.ApplySaveResult(18);

        // Act
        var description = effect.GetDescription();

        // Assert
        description.Should().Contain("SAVED");
        description.Should().Contain("immune 24h");
    }

    // ===== ImmunityDuration Tests =====

    [Test]
    public void ImmunityDuration_Is24Hours()
    {
        // Assert
        IntimidationEffect.ImmunityDuration.Should().Be(TimeSpan.FromHours(24));
    }
}
