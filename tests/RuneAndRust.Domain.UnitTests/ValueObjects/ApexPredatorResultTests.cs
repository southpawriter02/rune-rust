using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ApexPredatorResult"/> value object.
/// Tests cover concealment denial logic, static evaluation, and description generation.
/// </summary>
/// <remarks>
/// <para>Apex Predator is a Tier 3 passive ability for the Veiðimaðr (Hunter) specialization.
/// It denies all concealment benefits for targets with active Quarry Marks.</para>
/// <para>Key behaviors tested:</para>
/// <list type="bullet">
/// <item><see cref="ApexPredatorResult.ShouldDenyConcealment"/> — true when marked + concealment != None</item>
/// <item><see cref="ApexPredatorResult.IsConcealmentLost"/> — mirrors ConcealmentDenied property</item>
/// <item><see cref="ApexPredatorResult.GetDescription"/> — narrative text for combat logging</item>
/// </list>
/// <para>Introduced in v0.20.7c. Coherent path — zero Corruption risk.</para>
/// </remarks>
[TestFixture]
public class ApexPredatorResultTests
{
    // ===== ShouldDenyConcealment — Marked Target With Concealment =====

    [Test]
    public void ShouldDenyConcealment_MarkedWithLightObscurement_ReturnsTrue()
    {
        // Act
        var result = ApexPredatorResult.ShouldDenyConcealment(ConcealmentType.LightObscurement, isMarked: true);

        // Assert
        result.Should().BeTrue(
            "a marked quarry's light obscurement should be denied by Apex Predator");
    }

    [Test]
    public void ShouldDenyConcealment_MarkedWithInvisibility_ReturnsTrue()
    {
        // Act
        var result = ApexPredatorResult.ShouldDenyConcealment(ConcealmentType.Invisibility, isMarked: true);

        // Assert
        result.Should().BeTrue(
            "a marked quarry's invisibility should be denied by Apex Predator");
    }

    [Test]
    public void ShouldDenyConcealment_MarkedWithMagicalCamo_ReturnsTrue()
    {
        // Act
        var result = ApexPredatorResult.ShouldDenyConcealment(ConcealmentType.MagicalCamo, isMarked: true);

        // Assert
        result.Should().BeTrue(
            "a marked quarry's magical camouflage should be denied by Apex Predator");
    }

    [Test]
    public void ShouldDenyConcealment_MarkedWithHidden_ReturnsTrue()
    {
        // Act
        var result = ApexPredatorResult.ShouldDenyConcealment(ConcealmentType.Hidden, isMarked: true);

        // Assert
        result.Should().BeTrue(
            "a marked quarry's stealth-based concealment should be denied by Apex Predator");
    }

    // ===== ShouldDenyConcealment — No Effect Cases =====

    [Test]
    public void ShouldDenyConcealment_MarkedWithNone_ReturnsFalse()
    {
        // Act
        var result = ApexPredatorResult.ShouldDenyConcealment(ConcealmentType.None, isMarked: true);

        // Assert
        result.Should().BeFalse(
            "ConcealmentType.None means the target has no concealment to deny");
    }

    [Test]
    public void ShouldDenyConcealment_NotMarked_ReturnsFalse()
    {
        // Act
        var result = ApexPredatorResult.ShouldDenyConcealment(ConcealmentType.Invisibility, isMarked: false);

        // Assert
        result.Should().BeFalse(
            "Apex Predator only denies concealment for marked quarries — unmarked targets are unaffected");
    }

    // ===== IsConcealmentLost =====

    [Test]
    public void IsConcealmentLost_WhenDenied_ReturnsTrue()
    {
        // Arrange
        var result = new ApexPredatorResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Shadow Wraith",
            WasConcealed = true,
            ConcealmentType = ConcealmentType.Invisibility,
            ConcealmentDenied = true,
            TargetWasMarked = true
        };

        // Act & Assert
        result.IsConcealmentLost().Should().BeTrue(
            "concealment was denied — the quarry's invisibility is stripped");
    }

    [Test]
    public void IsConcealmentLost_WhenNotDenied_ReturnsFalse()
    {
        // Arrange
        var result = new ApexPredatorResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Hidden Rogue",
            WasConcealed = true,
            ConcealmentType = ConcealmentType.Hidden,
            ConcealmentDenied = false,
            TargetWasMarked = false
        };

        // Act & Assert
        result.IsConcealmentLost().Should().BeFalse(
            "target was not marked — concealment remains intact");
    }

    // ===== GetDescription =====

    [Test]
    public void GetDescription_ConcealmentDenied_ContainsDeniedText()
    {
        // Arrange
        var result = new ApexPredatorResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Frost Phantom",
            WasConcealed = true,
            ConcealmentType = ConcealmentType.MagicalCamo,
            ConcealmentDenied = true,
            TargetWasMarked = true
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("denied",
            "description should indicate that concealment was denied");
        description.Should().Contain("Frost Phantom");
        description.Should().Contain("magical camouflage",
            "description should include the formatted concealment type");
    }

    [Test]
    public void GetDescription_NotMarked_ContainsNotMarkedText()
    {
        // Arrange
        var result = new ApexPredatorResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Cave Spider",
            WasConcealed = true,
            ConcealmentType = ConcealmentType.Hidden,
            ConcealmentDenied = false,
            TargetWasMarked = false
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("not marked",
            "description should explain that the target was not a quarry");
        description.Should().Contain("Cave Spider");
    }

    [Test]
    public void GetDescription_MarkedButNoConcealment_ContainsNoConcealmentText()
    {
        // Arrange
        var result = new ApexPredatorResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Draugr Warrior",
            WasConcealed = false,
            ConcealmentType = ConcealmentType.None,
            ConcealmentDenied = false,
            TargetWasMarked = true
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("no concealment",
            "description should indicate there was nothing to deny");
        description.Should().Contain("Draugr Warrior");
    }
}
