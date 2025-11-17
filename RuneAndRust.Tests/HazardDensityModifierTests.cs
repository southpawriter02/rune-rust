using NUnit.Framework;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.35.3: Test suite for HazardDensityModifier
/// Validates hazard density calculations based on faction control and war status
/// </summary>
[TestFixture]
public class HazardDensityModifierTests
{
    private HazardDensityModifier _modifier;

    [SetUp]
    public void Setup()
    {
        _modifier = new HazardDensityModifier();
    }

    [Test]
    public void CalculateHazardDensity_GodSleeperControl_Increased()
    {
        // Arrange: God-Sleepers cause +25% hazards
        string faction = "GodSleeperCultists";
        bool isWarZone = false;

        // Act
        double density = _modifier.CalculateHazardDensity(faction, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(1.25).Within(0.01));
    }

    [Test]
    public void CalculateHazardDensity_IronBanesControl_Decreased()
    {
        // Arrange: Iron-Banes cause -10% hazards
        string faction = "IronBanes";
        bool isWarZone = false;

        // Act
        double density = _modifier.CalculateHazardDensity(faction, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(0.90).Within(0.01));
    }

    [Test]
    public void CalculateHazardDensity_IndependentsControl_Decreased()
    {
        // Arrange: Independents cause -15% hazards
        string faction = "Independents";
        bool isWarZone = false;

        // Act
        double density = _modifier.CalculateHazardDensity(faction, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(0.85).Within(0.01));
    }

    [Test]
    public void CalculateHazardDensity_RustClansControl_SlightlyDecreased()
    {
        // Arrange: Rust-Clans cause -5% hazards
        string faction = "RustClans";
        bool isWarZone = false;

        // Act
        double density = _modifier.CalculateHazardDensity(faction, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(0.95).Within(0.01));
    }

    [Test]
    public void CalculateHazardDensity_JotunReadersControl_Normal()
    {
        // Arrange: Jötun-Readers have no effect
        string faction = "JotunReaders";
        bool isWarZone = false;

        // Act
        double density = _modifier.CalculateHazardDensity(faction, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(1.0).Within(0.01));
    }

    [Test]
    public void CalculateHazardDensity_NoFactionControl_Normal()
    {
        // Arrange: No faction control
        bool isWarZone = false;

        // Act
        double density = _modifier.CalculateHazardDensity(null, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(1.0).Within(0.01));
    }

    [Test]
    public void CalculateHazardDensity_WarZone_IncreaseBy50Percent()
    {
        // Arrange: War zone adds +50% hazards
        string faction = "JotunReaders";
        bool isWarZone = true;

        // Act
        double density = _modifier.CalculateHazardDensity(faction, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(1.5).Within(0.01)); // 1.0 * 1.5
    }

    [Test]
    public void CalculateHazardDensity_GodSleepersWarZone_VeryHigh()
    {
        // Arrange: God-Sleepers + War = 1.25 * 1.5 = 1.875
        string faction = "GodSleeperCultists";
        bool isWarZone = true;

        // Act
        double density = _modifier.CalculateHazardDensity(faction, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(1.875).Within(0.01));
    }

    [Test]
    public void CalculateHazardDensity_IndependentsWarZone_Balanced()
    {
        // Arrange: Independents + War = 0.85 * 1.5 = 1.275
        string faction = "Independents";
        bool isWarZone = true;

        // Act
        double density = _modifier.CalculateHazardDensity(faction, isWarZone);

        // Assert
        Assert.That(density, Is.EqualTo(1.275).Within(0.01));
    }

    [Test]
    public void CalculateHazardSpawnChance_AppliesMultiplier()
    {
        // Arrange
        double baseChance = 0.10; // 10% base
        double densityMultiplier = 1.5;

        // Act
        double modifiedChance = _modifier.CalculateHazardSpawnChance(baseChance, densityMultiplier);

        // Assert
        Assert.That(modifiedChance, Is.EqualTo(0.15).Within(0.01)); // 15%
    }

    [Test]
    public void CalculateHazardSpawnChance_ClampedToMin1Percent()
    {
        // Arrange
        double baseChance = 0.10;
        double densityMultiplier = 0.01; // Would result in 0.1%

        // Act
        double modifiedChance = _modifier.CalculateHazardSpawnChance(baseChance, densityMultiplier);

        // Assert
        Assert.That(modifiedChance, Is.EqualTo(0.01).Within(0.001)); // Clamped to 1%
    }

    [Test]
    public void CalculateHazardSpawnChance_ClampedToMax50Percent()
    {
        // Arrange
        double baseChance = 0.40;
        double densityMultiplier = 2.0; // Would result in 80%

        // Act
        double modifiedChance = _modifier.CalculateHazardSpawnChance(baseChance, densityMultiplier);

        // Assert
        Assert.That(modifiedChance, Is.EqualTo(0.50).Within(0.01)); // Clamped to 50%
    }

    [Test]
    public void GetHazardDensityTier_Extreme()
    {
        // Arrange
        double density = 1.5;

        // Act
        string tier = _modifier.GetHazardDensityTier(density);

        // Assert
        Assert.That(tier, Is.EqualTo("Extreme"));
    }

    [Test]
    public void GetHazardDensityTier_VeryHigh()
    {
        // Arrange
        double density = 1.25;

        // Act
        string tier = _modifier.GetHazardDensityTier(density);

        // Assert
        Assert.That(tier, Is.EqualTo("Very High"));
    }

    [Test]
    public void GetHazardDensityTier_Normal()
    {
        // Arrange
        double density = 1.0;

        // Act
        string tier = _modifier.GetHazardDensityTier(density);

        // Assert
        Assert.That(tier, Is.EqualTo("Normal"));
    }

    [Test]
    public void GetHazardDensityTier_Low()
    {
        // Arrange
        double density = 0.85;

        // Act
        string tier = _modifier.GetHazardDensityTier(density);

        // Assert
        Assert.That(tier, Is.EqualTo("Low"));
    }

    [Test]
    public void CalculateExpectedHazardCount_AppliesMultiplier()
    {
        // Arrange
        int baseCount = 10;
        double density = 1.5;

        // Act
        int modifiedCount = _modifier.CalculateExpectedHazardCount(baseCount, density);

        // Assert
        Assert.That(modifiedCount, Is.EqualTo(15));
    }

    [Test]
    public void CalculateExpectedHazardCount_RoundsDown()
    {
        // Arrange
        int baseCount = 10;
        double density = 0.85; // Would be 8.5

        // Act
        int modifiedCount = _modifier.CalculateExpectedHazardCount(baseCount, density);

        // Assert
        Assert.That(modifiedCount, Is.EqualTo(8));
    }

    [Test]
    public void CalculateExpectedHazardCount_MinimumZero()
    {
        // Arrange
        int baseCount = 10;
        double density = 0.0;

        // Act
        int modifiedCount = _modifier.CalculateExpectedHazardCount(baseCount, density);

        // Assert
        Assert.That(modifiedCount, Is.EqualTo(0));
    }

    [Test]
    public void GetEnvironmentalDescription_GodSleepersWarZone()
    {
        // Arrange
        string faction = "GodSleeperCultists";
        bool isWarZone = true;
        double density = 1.875;

        // Act
        string description = _modifier.GetEnvironmentalDescription(faction, isWarZone, density);

        // Assert
        Assert.That(description, Does.Contain("awakening rituals"));
        Assert.That(description, Does.Contain("hazards"));
    }

    [Test]
    public void ApplyEventModifier_Catastrophe_Increases()
    {
        // Arrange
        double baseDensity = 1.0;
        string eventType = "Catastrophe";
        int daysRemaining = 3;

        // Act
        double modified = _modifier.ApplyEventModifier(baseDensity, eventType, daysRemaining);

        // Assert
        Assert.That(modified, Is.GreaterThan(baseDensity));
    }

    [Test]
    public void ApplyEventModifier_AwakeningRitual_Increases()
    {
        // Arrange
        double baseDensity = 1.0;
        string eventType = "Awakening_Ritual";
        int daysRemaining = 5;

        // Act
        double modified = _modifier.ApplyEventModifier(baseDensity, eventType, daysRemaining);

        // Assert
        Assert.That(modified, Is.GreaterThan(baseDensity));
    }

    [Test]
    public void ApplyEventModifier_EventExpired_NoEffect()
    {
        // Arrange
        double baseDensity = 1.0;
        string eventType = "Catastrophe";
        int daysRemaining = 0;

        // Act
        double modified = _modifier.ApplyEventModifier(baseDensity, eventType, daysRemaining);

        // Assert
        Assert.That(modified, Is.EqualTo(baseDensity).Within(0.01));
    }
}
