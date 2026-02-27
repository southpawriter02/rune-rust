using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="PerfectHuntResult"/> value object.
/// Tests cover auto-critical damage calculations, computed properties, and description generation.
/// </summary>
/// <remarks>
/// <para>The Perfect Hunt is the Capstone (Ultimate) ability for the Veiðimaðr (Hunter) specialization.
/// It delivers an automatic critical hit with doubled base damage against a marked quarry.</para>
/// <para>Key behaviors tested:</para>
/// <list type="bullet">
/// <item><see cref="PerfectHuntResult.TotalDamage"/>: BaseDamageRoll * 2 (critical multiplier)</item>
/// <item><see cref="PerfectHuntResult.CriticalMultiplier"/>: Always 2</item>
/// <item><see cref="PerfectHuntResult.IsCriticalHit"/>: Always true (automatic critical)</item>
/// <item><see cref="PerfectHuntResult.GetDescription"/>: Narrative text for combat logging</item>
/// <item><see cref="PerfectHuntResult.GetDamageBreakdown"/>: Multi-line damage report</item>
/// <item><see cref="PerfectHuntResult.GetStatusMessage"/>: Concise combat log message</item>
/// </list>
/// <para>Introduced in v0.20.7c. Coherent path — zero Corruption risk.</para>
/// </remarks>
[TestFixture]
public class PerfectHuntResultTests
{
    // ===== TotalDamage Tests =====

    [Test]
    public void TotalDamage_DoublesBaseDamage()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Frost Wyrm",
            BaseDamageRoll = 15,
            MarkConsumed = true,
            CapstoneUsed = true,
            NarrativeDescription = "The perfect strike lands."
        };

        // Act & Assert
        result.TotalDamage.Should().Be(30,
            "15 * 2 = 30 — The Perfect Hunt doubles all base damage");
    }

    [Test]
    public void TotalDamage_SmallDamage_StillDoubles()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Goblin",
            BaseDamageRoll = 3,
            MarkConsumed = true,
            CapstoneUsed = true
        };

        // Act & Assert
        result.TotalDamage.Should().Be(6,
            "3 * 2 = 6 — even small base damage gets doubled");
    }

    // ===== CriticalMultiplier Tests =====

    [Test]
    public void CriticalMultiplier_AlwaysTwo()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Target",
            BaseDamageRoll = 10,
            MarkConsumed = true,
            CapstoneUsed = true
        };

        // Act & Assert
        result.CriticalMultiplier.Should().Be(2,
            "The Perfect Hunt always applies a x2 critical multiplier");
    }

    // ===== IsCriticalHit Tests =====

    [Test]
    public void IsCriticalHit_AlwaysTrue()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Target",
            BaseDamageRoll = 10,
            MarkConsumed = true,
            CapstoneUsed = true
        };

        // Act & Assert
        result.IsCriticalHit.Should().BeTrue(
            "The Perfect Hunt is always an automatic critical hit — no roll needed");
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_ContainsPerfectHunt()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Ancient Draugr",
            BaseDamageRoll = 12,
            MarkConsumed = true,
            CapstoneUsed = true
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("PERFECT HUNT",
            "description should include the ability name for combat log visibility");
        description.Should().Contain("Ancient Draugr");
    }

    [Test]
    public void GetDescription_ContainsDamageValues()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Wyrm Lord",
            BaseDamageRoll = 18,
            MarkConsumed = true,
            CapstoneUsed = true
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("36",
            "description should contain total damage (18 * 2 = 36)");
        description.Should().Contain("18",
            "description should contain the base damage roll");
    }

    // ===== GetDamageBreakdown Tests =====

    [Test]
    public void GetDamageBreakdown_ContainsAllSections()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Jörmungandr Spawn",
            BaseDamageRoll = 20,
            MarkConsumed = true,
            CapstoneUsed = true
        };

        // Act
        var breakdown = result.GetDamageBreakdown();

        // Assert
        breakdown.Should().Contain("THE PERFECT HUNT",
            "breakdown should contain the header");
        breakdown.Should().Contain("Jörmungandr Spawn",
            "breakdown should contain the target name");
        breakdown.Should().Contain("20",
            "breakdown should contain the base damage");
        breakdown.Should().Contain("x2",
            "breakdown should contain the critical multiplier");
        breakdown.Should().Contain("40",
            "breakdown should contain the total damage (20 * 2)");
        breakdown.Should().Contain("Auto-Critical: Yes",
            "breakdown should confirm auto-critical status");
        breakdown.Should().Contain("long rest",
            "breakdown should mention cooldown information");
    }

    // ===== GetStatusMessage Tests =====

    [Test]
    public void GetStatusMessage_ContainsDevastated()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Troll Berserker",
            BaseDamageRoll = 14,
            MarkConsumed = true,
            CapstoneUsed = true
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("DEVASTATED",
            "status message should convey the devastating nature of the capstone");
        message.Should().Contain("Troll Berserker");
    }

    [Test]
    public void GetStatusMessage_ContainsDamageCalc()
    {
        // Arrange
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Target",
            BaseDamageRoll = 11,
            MarkConsumed = true,
            CapstoneUsed = true
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("11",
            "status message should contain base damage");
        message.Should().Contain("22",
            "status message should contain total damage (11 * 2 = 22)");
    }

    // ===== NarrativeDescription Preservation Test =====

    [Test]
    public void NarrativeDescription_PreservesValue()
    {
        // Arrange
        var narrative = "The hunter's patience culminates in a single, perfect strike.";
        var result = new PerfectHuntResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Target",
            BaseDamageRoll = 10,
            MarkConsumed = true,
            CapstoneUsed = true,
            NarrativeDescription = narrative
        };

        // Act & Assert
        result.NarrativeDescription.Should().Be(narrative,
            "NarrativeDescription should preserve the exact value set during initialization");
    }
}
