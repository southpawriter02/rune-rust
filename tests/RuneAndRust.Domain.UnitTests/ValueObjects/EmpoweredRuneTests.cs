using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="EmpoweredRune"/> value object.
/// Tests creation, element validation, tick/expiry lifecycle, and display methods.
/// </summary>
[TestFixture]
public class EmpoweredRuneTests
{
    private static readonly Guid TestItemId = Guid.NewGuid();

    // ===== Creation Tests =====

    [Test]
    public void Create_WithFireElement_CreatesRuneCorrectly()
    {
        // Arrange & Act
        var rune = EmpoweredRune.Create(TestItemId, "fire");

        // Assert
        rune.TargetItemId.Should().Be(TestItemId);
        rune.ElementalTypeId.Should().Be("fire");
        rune.BonusDice.Should().Be("1d6");
        rune.Duration.Should().Be(EmpoweredRune.DefaultDuration);
        rune.OriginalDuration.Should().Be(EmpoweredRune.DefaultDuration);
        rune.RuneId.Should().NotBeEmpty();
        rune.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Create_WithColdElement_CreatesRuneCorrectly()
    {
        // Arrange & Act
        var rune = EmpoweredRune.Create(TestItemId, "cold");

        // Assert
        rune.ElementalTypeId.Should().Be("cold");
    }

    [Test]
    public void Create_WithLightningElement_CreatesRuneCorrectly()
    {
        // Arrange & Act
        var rune = EmpoweredRune.Create(TestItemId, "lightning");

        // Assert
        rune.ElementalTypeId.Should().Be("lightning");
    }

    [Test]
    public void Create_WithAethericElement_CreatesRuneCorrectly()
    {
        // Arrange & Act
        var rune = EmpoweredRune.Create(TestItemId, "aetheric");

        // Assert
        rune.ElementalTypeId.Should().Be("aetheric");
    }

    [Test]
    public void Create_NormalizesElementIdToLowerCase()
    {
        // Arrange & Act
        var rune = EmpoweredRune.Create(TestItemId, "FIRE");

        // Assert
        rune.ElementalTypeId.Should().Be("fire");
    }

    [Test]
    public void Create_WithCustomDuration_UsesProvidedDuration()
    {
        // Arrange & Act
        var rune = EmpoweredRune.Create(TestItemId, "fire", duration: 5);

        // Assert
        rune.Duration.Should().Be(5);
        rune.OriginalDuration.Should().Be(5);
    }

    // ===== Validation Tests =====

    [Test]
    public void Create_WithInvalidElement_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => EmpoweredRune.Create(TestItemId, "poison");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("elementalTypeId");
    }

    [Test]
    public void Create_WithNullElement_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => EmpoweredRune.Create(TestItemId, null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithEmptyElement_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => EmpoweredRune.Create(TestItemId, "");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [TestCase("fire", true)]
    [TestCase("cold", true)]
    [TestCase("lightning", true)]
    [TestCase("aetheric", true)]
    [TestCase("FIRE", true)]
    [TestCase("poison", false)]
    [TestCase("physical", false)]
    [TestCase("", false)]
    public void IsValidElement_ReturnsCorrectResult(string elementId, bool expected)
    {
        // Arrange & Act & Assert
        EmpoweredRune.IsValidElement(elementId).Should().Be(expected);
    }

    // ===== Tick & Expiry Tests =====

    [Test]
    public void Tick_DecrementsRemainingDuration()
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, "fire");
        var initialDuration = rune.Duration;

        // Act
        rune.Tick();

        // Assert
        rune.Duration.Should().Be(initialDuration - 1);
        rune.OriginalDuration.Should().Be(initialDuration);
    }

    [Test]
    public void Tick_AtZeroDuration_DoesNotGoNegative()
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, "fire", duration: 0);

        // Act
        rune.Tick();

        // Assert
        rune.Duration.Should().Be(0);
    }

    [Test]
    public void IsExpired_WhenDurationPositive_ReturnsFalse()
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, "fire");

        // Act & Assert
        rune.IsExpired().Should().BeFalse();
    }

    [Test]
    public void IsExpired_WhenDurationReachesZero_ReturnsTrue()
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, "fire", duration: 1);

        // Act
        rune.Tick();

        // Assert
        rune.IsExpired().Should().BeTrue();
    }

    [Test]
    public void FullLifecycle_TicksDownToExpiry()
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, "fire", duration: 3);

        // Act & Assert — tick through full lifecycle
        rune.IsExpired().Should().BeFalse();

        rune.Tick(); // 2 remaining
        rune.Duration.Should().Be(2);
        rune.IsExpired().Should().BeFalse();

        rune.Tick(); // 1 remaining
        rune.Duration.Should().Be(1);
        rune.IsExpired().Should().BeFalse();

        rune.Tick(); // 0 remaining
        rune.Duration.Should().Be(0);
        rune.IsExpired().Should().BeTrue();
    }

    // ===== Display Tests =====

    [TestCase("fire", "Fire (flames flicker)")]
    [TestCase("cold", "Cold (frost rime)")]
    [TestCase("lightning", "Lightning (sparks crackle)")]
    [TestCase("aetheric", "Aetheric (purple glow)")]
    public void GetElementDisplay_ReturnsDescriptiveString(string element, string expected)
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, element);

        // Act & Assert
        rune.GetElementDisplay().Should().Be(expected);
    }

    [Test]
    public void GetAverageDamage_Returns3Point5()
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, "fire");

        // Act & Assert
        rune.GetAverageDamage().Should().Be(3.5m);
    }

    [Test]
    public void GetDurationDisplay_ReturnsCorrectFormat()
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, "fire");
        rune.Tick(); // 9/10

        // Act & Assert
        rune.GetDurationDisplay().Should().Be("9/10 turns");
    }

    [Test]
    public void GetCombinedEffectDisplay_ContainsElementAndDuration()
    {
        // Arrange
        var rune = EmpoweredRune.Create(TestItemId, "fire");

        // Act
        var display = rune.GetCombinedEffectDisplay();

        // Assert
        display.Should().Contain("+2 damage");
        display.Should().Contain("1d6");
        display.Should().Contain("Fire");
        display.Should().Contain("10 turns");
    }

    // ===== Constants Tests =====

    [Test]
    public void DefaultDuration_Is10()
    {
        EmpoweredRune.DefaultDuration.Should().Be(10);
    }

    [Test]
    public void DefaultBonusDice_Is1d6()
    {
        EmpoweredRune.DefaultBonusDice.Should().Be("1d6");
    }

    [Test]
    public void AllowedElements_ContainsFourTypes()
    {
        EmpoweredRune.AllowedElements.Should().HaveCount(4);
        EmpoweredRune.AllowedElements.Should().Contain("fire");
        EmpoweredRune.AllowedElements.Should().Contain("cold");
        EmpoweredRune.AllowedElements.Should().Contain("lightning");
        EmpoweredRune.AllowedElements.Should().Contain("aetheric");
    }
}
