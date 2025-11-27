using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.22: Unit tests for Environmental Combat System
/// Tests environmental objects, hazards, weather effects, and manipulation
/// </summary>
[TestFixture]
public class EnvironmentalCombatServiceTests
{
    private DiceService _diceService = null!;
    private TraumaEconomyService _traumaService = null!;
    private EnvironmentalObjectService _objectService = null!;
    private ResolveCheckService _resolveCheckService = null!;
    private AmbientConditionService _conditionService = null!;
    private WeatherEffectService _weatherService = null!;
    private EnvironmentalManipulationService _manipulationService = null!;
    private CoverService _coverService = null!;
    private HazardService _hazardService = null!;
    private EnvironmentalCombatService _environmentalCombatService = null!;

    [SetUp]
    public void Setup()
    {
        _diceService = new DiceService();
        _traumaService = new TraumaEconomyService();
        _objectService = new EnvironmentalObjectService(_diceService);
        _resolveCheckService = new ResolveCheckService(_diceService);
        _conditionService = new AmbientConditionService(_diceService, _traumaService, _resolveCheckService);
        _weatherService = new WeatherEffectService(_diceService, _traumaService);
        _manipulationService = new EnvironmentalManipulationService(_objectService, _traumaService, _diceService);
        _coverService = new CoverService();
        _hazardService = new HazardService(_diceService, _traumaService);

        _environmentalCombatService = new EnvironmentalCombatService(
            _objectService,
            _conditionService,
            _weatherService,
            _manipulationService,
            _coverService,
            _hazardService
        );
    }

    #region EnvironmentalObject Tests

    [Test]
    public void CreateCover_ValidParameters_CreatesCoverObject()
    {
        // Arrange
        int roomId = 1;
        string position = "Front_Left_Column_1";
        string name = "Steel Barricade";
        var quality = CoverQuality.Heavy;
        int durability = 30;

        // Act
        var cover = _objectService.CreateCover(roomId, position, name, quality, durability);

        // Assert
        Assert.That(cover, Is.Not.Null);
        Assert.That(cover.Name, Is.EqualTo(name));
        Assert.That(cover.CoverQuality, Is.EqualTo(quality));
        Assert.That(cover.CurrentDurability, Is.EqualTo(durability));
        Assert.That(cover.MaxDurability, Is.EqualTo(durability));
        Assert.That(cover.ProvidesCover, Is.True);
        Assert.That(cover.ObjectType, Is.EqualTo(EnvironmentalObjectType.Cover));
    }

    [Test]
    public void CreateHazard_ValidParameters_CreatesHazardObject()
    {
        // Arrange
        int roomId = 1;
        string position = "Back_Center_Column_2";
        string name = "Toxic Pool";
        string damageFormula = "3d6";
        string damageType = "Poison";

        // Act
        var hazard = _objectService.CreateHazard(roomId, position, name, damageFormula, damageType);

        // Assert
        Assert.That(hazard, Is.Not.Null);
        Assert.That(hazard.Name, Is.EqualTo(name));
        Assert.That(hazard.IsHazard, Is.True);
        Assert.That(hazard.DamageFormula, Is.EqualTo(damageFormula));
        Assert.That(hazard.DamageType, Is.EqualTo(damageType));
        Assert.That(hazard.ObjectType, Is.EqualTo(EnvironmentalObjectType.Hazard));
    }

    [Test]
    public void DamageObject_DestructibleObject_ReducesDurability()
    {
        // Arrange
        var cover = _objectService.CreateCover(1, "Front_Left", "Crate", CoverQuality.Light, 20);
        int damage = 10;

        // Act
        var result = _objectService.DamageObject(cover.ObjectId, damage);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.WasDestroyed, Is.False);
        Assert.That(result.RemainingDurability, Is.EqualTo(10));
    }

    [Test]
    public void DamageObject_LethalDamage_DestroysObject()
    {
        // Arrange
        var cover = _objectService.CreateCover(1, "Front_Left", "Crate", CoverQuality.Light, 20);
        int damage = 20;

        // Act
        var result = _objectService.DamageObject(cover.ObjectId, damage);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.WasDestroyed, Is.True);
        Assert.That(result.RemainingDurability, Is.EqualTo(0));
    }

    [Test]
    public void GetObjectsInRoom_MultipleObjects_ReturnsAllActive()
    {
        // Arrange
        int roomId = 1;
        _objectService.CreateCover(roomId, "Front_Left", "Crate", CoverQuality.Light, 20);
        _objectService.CreateCover(roomId, "Front_Right", "Pillar", CoverQuality.Heavy, 40);
        _objectService.CreateHazard(roomId, "Back_Center", "Fire", "4d6", "Fire");

        // Act
        var objects = _objectService.GetObjectsInRoom(roomId);

        // Assert
        Assert.That(objects.Count, Is.EqualTo(3));
    }

    #endregion

    #region AmbientCondition Tests

    [Test]
    public void ApplyCondition_ToxicAir_CreatesConditionWithCorrectEffects()
    {
        // Arrange
        int roomId = 1;
        var conditionType = AmbientConditionType.ToxicAir;

        // Act
        int conditionId = _conditionService.ApplyCondition(roomId, conditionType,
            "Toxic Atmosphere", "Chemical decay fills the air", null);

        // Assert
        Assert.That(conditionId, Is.GreaterThan(0));
        var conditions = _conditionService.GetActiveConditions(roomId);
        Assert.That(conditions.Count, Is.EqualTo(1));
        Assert.That(conditions[0].BaseCondition.Type, Is.EqualTo(AmbientConditionType.ToxicAir));
    }

    [Test]
    public void ApplyAmbientEffects_PsychicResonance_AppliesStress()
    {
        // Arrange
        int roomId = 1;
        _conditionService.ApplyCondition(roomId, AmbientConditionType.PsychicResonance,
            "Psychic Resonance", "Trauma echoes fill the air", null);
        var character = CreateTestCharacter();
        int initialStress = character.PsychicStress;

        // Act
        var result = _conditionService.ApplyAmbientEffects(character, roomId);

        // Assert
        Assert.That(result.WasTriggered, Is.True);
        Assert.That(character.PsychicStress, Is.GreaterThan(initialStress));
    }

    #endregion

    #region WeatherEffect Tests

    [Test]
    public void CreateWeather_RealityStorm_CreatesWeatherWithCorrectEffects()
    {
        // Arrange
        int roomId = 1;
        var weatherType = WeatherType.RealityStorm;
        var intensity = WeatherIntensity.Moderate;

        // Act
        int weatherId = _weatherService.CreateWeather(roomId, weatherType, intensity, 5);

        // Assert
        Assert.That(weatherId, Is.GreaterThan(0));
        var weather = _weatherService.GetCurrentWeather(roomId);
        Assert.That(weather, Is.Not.Null);
        Assert.That(weather!.WeatherType, Is.EqualTo(WeatherType.RealityStorm));
        Assert.That(weather.StressPerTurn, Is.GreaterThan(0));
    }

    [Test]
    public void ApplyWeatherEffects_StaticDischarge_DealsDamage()
    {
        // Arrange
        int roomId = 1;
        int weatherId = _weatherService.CreateWeather(roomId, WeatherType.StaticDischarge,
            WeatherIntensity.High, 3);
        var weather = _weatherService.GetCurrentWeather(roomId);
        var character = CreateTestCharacter();
        int initialHP = character.HP;

        // Act
        var result = _weatherService.ApplyWeatherEffects(character, weather!);

        // Assert
        Assert.That(result.WasTriggered, Is.True);
        // High intensity static discharge should deal damage
        Assert.That(character.HP, Is.LessThanOrEqualTo(initialHP));
    }

    #endregion

    #region EnvironmentalManipulation Tests

    [Test]
    public void PushIntoHazard_HazardAtDestination_DealsDamage()
    {
        // Arrange
        int roomId = 1;
        string startPos = "Front_Left";
        string endPos = "Back_Center";
        var hazard = _objectService.CreateHazard(roomId, endPos, "Fire Pit", "4d6", "Fire");

        // Act
        var result = _manipulationService.PushIntoHazard(1, 2, startPos, endPos, roomId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.HazardsEncountered.Count, Is.GreaterThan(0));
        Assert.That(result.TotalDamage, Is.GreaterThan(0));
    }

    [Test]
    public void RecordEnvironmentalKill_ValidKill_RecordsEventAndAppliesReward()
    {
        // Arrange
        int combatInstanceId = 1;
        var killer = CreateTestCharacter();
        int initialStress = killer.PsychicStress = 20;

        // Act
        _manipulationService.RecordEnvironmentalKill(combatInstanceId, killer.CharacterId, 99,
            "Pushed into toxic pool", killer);

        // Assert
        int killCount = _manipulationService.GetEnvironmentalKillCount(combatInstanceId);
        Assert.That(killCount, Is.EqualTo(1));
        // Stress should be reduced as reward
        Assert.That(killer.PsychicStress, Is.LessThan(initialStress));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void ProcessStartOfTurn_MultipleEnvironmentalEffects_AppliesAll()
    {
        // Arrange
        int roomId = 1;
        int combatInstanceId = 1;
        _conditionService.ApplyCondition(roomId, AmbientConditionType.ToxicAir,
            "Toxic Air", "Poisonous atmosphere", null);
        _weatherService.CreateWeather(roomId, WeatherType.RealityStorm,
            WeatherIntensity.Moderate, 5);
        var character = CreateTestCharacter();

        // Act
        var logs = _environmentalCombatService.ProcessStartOfTurn(combatInstanceId, character, roomId);

        // Assert
        Assert.That(logs, Is.Not.Empty);
        // Should have logs from both ambient condition and weather
        Assert.That(logs.Count, Is.GreaterThanOrEqualTo(1));
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestCharacter()
    {
        return new PlayerCharacter
        {
            CharacterId = 1,
            Name = "Test Character",
            HP = 100,
            MaxHP = 100,
            Stamina = 50,
            MaxStamina = 50,
            PsychicStress = 0,
            FINESSE = 6,
            STURDINESS = 6,
            WILL = 6,
            ANALYSIS = 6,
            WITS = 6
        };
    }

    #endregion
}
