using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="SignInterpretationService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>DC calculation (base DC, age modifiers, unknown faction +4)</description></item>
///   <item><description>Faction knowledge (major factions, learned factions, unknown factions)</description></item>
///   <item><description>Sign meaning (success meaning, critical context, misinterpretation)</description></item>
///   <item><description>SignInterpretationResult value object (factory methods, properties)</description></item>
///   <item><description>ScavengerSign value object (factory methods, computed properties)</description></item>
///   <item><description>ScavengerSignType enum extensions</description></item>
///   <item><description>SignAge enum extensions</description></item>
///   <item><description>Interpretation prerequisites</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class SignInterpretationServiceTests
{
    private SkillCheckService _skillCheckService = null!;
    private ILogger<SignInterpretationService> _logger = null!;
    private SignInterpretationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _skillCheckService = CreateMockSkillCheckService();
        _logger = Substitute.For<ILogger<SignInterpretationService>>();
        _sut = new SignInterpretationService(_skillCheckService, _logger);
    }

    #region Base DC Tests

    [Test]
    [TestCase(ScavengerSignType.TerritoryMarker, 10)]
    [TestCase(ScavengerSignType.WarningSign, 12)]
    [TestCase(ScavengerSignType.CacheIndicator, 14)]
    [TestCase(ScavengerSignType.TrailBlaze, 10)]
    [TestCase(ScavengerSignType.HuntMarker, 14)]
    [TestCase(ScavengerSignType.TabooSign, 12)]
    public void GetBaseDc_ReturnsCorrectDcForEachSignType(ScavengerSignType signType, int expectedDc)
    {
        // Act
        var dc = _sut.GetBaseDc(signType);

        // Assert
        dc.Should().Be(expectedDc);
    }

    [Test]
    [TestCase(SignAge.Fresh, 0)]
    [TestCase(SignAge.Recent, 0)]
    [TestCase(SignAge.Old, 1)]
    [TestCase(SignAge.Faded, 2)]
    [TestCase(SignAge.Ancient, 4)]
    public void GetAgeDcModifier_ReturnsCorrectModifierForEachAge(SignAge age, int expectedModifier)
    {
        // Act
        var modifier = _sut.GetAgeDcModifier(age);

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    [Test]
    public void GetUnknownFactionModifier_ReturnsFour()
    {
        // Act
        var modifier = _sut.GetUnknownFactionModifier();

        // Assert
        modifier.Should().Be(4);
    }

    #endregion

    #region DC Calculation Tests

    [Test]
    public void GetSignDc_FreshTerritoryMarkerFromKnownFaction_ReturnsTen()
    {
        // Arrange
        var player = CreateTestPlayer();
        var sign = ScavengerSign.TerritoryMarker("iron-covenant", SignAge.Fresh);

        // Act
        var dc = _sut.GetSignDc(player, sign);

        // Assert
        dc.Should().Be(10); // Base 10 + 0 age + 0 known faction
    }

    [Test]
    public void GetSignDc_FadedCacheIndicatorFromUnknownFaction_ReturnsTwenty()
    {
        // Arrange
        var player = CreateTestPlayer();
        var sign = ScavengerSign.CacheIndicator(
            "unknown-faction",
            "north",
            "50 meters",
            SignAge.Faded);

        // Act
        var dc = _sut.GetSignDc(player, sign);

        // Assert
        dc.Should().Be(20); // Base 14 + 2 age + 4 unknown faction
    }

    [Test]
    public void GetSignDc_AncientTabooSignFromMajorFaction_ReturnsSixteen()
    {
        // Arrange
        var player = CreateTestPlayer();
        var sign = ScavengerSign.TabooSign(
            "verdant-circle",
            "Cursed ground, supernatural entities present",
            SignAge.Ancient);

        // Act
        var dc = _sut.GetSignDc(player, sign);

        // Assert
        dc.Should().Be(16); // Base 12 + 4 age + 0 major faction
    }

    [Test]
    public void GetSignDc_OldWarningSignFromLearnedFaction_ReturnsThirteen()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddKnownFaction("scrap-runners");
        var sign = ScavengerSign.WarningSign(
            "scrap-runners",
            "Radiation leak detected",
            SignAge.Old);

        // Act
        var dc = _sut.GetSignDc(player, sign);

        // Assert
        dc.Should().Be(13); // Base 12 + 1 age + 0 known faction
    }

    [Test]
    public void GetSignDc_RecentHuntMarkerFromUnknownFaction_ReturnsEighteen()
    {
        // Arrange
        var player = CreateTestPlayer();
        var sign = ScavengerSign.HuntMarker(
            "shadow-hunters",
            "Radstag",
            "east",
            "2 days",
            SignAge.Recent);

        // Act
        var dc = _sut.GetSignDc(player, sign);

        // Assert
        dc.Should().Be(18); // Base 14 + 0 age + 4 unknown faction
    }

    #endregion

    #region Faction Knowledge Tests

    [Test]
    [TestCase("iron-covenant")]
    [TestCase("rust-walkers")]
    [TestCase("silent-ones")]
    [TestCase("verdant-circle")]
    [TestCase("ash-born")]
    public void IsFactionKnown_MajorFactions_AlwaysReturnsTrue(string factionId)
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var isKnown = _sut.IsFactionKnown(player, factionId);

        // Assert
        isKnown.Should().BeTrue();
    }

    [Test]
    public void IsFactionKnown_UnknownMinorFaction_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var isKnown = _sut.IsFactionKnown(player, "unknown-scavengers");

        // Assert
        isKnown.Should().BeFalse();
    }

    [Test]
    public void IsFactionKnown_LearnedFaction_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddKnownFaction("scrap-runners");

        // Act
        var isKnown = _sut.IsFactionKnown(player, "scrap-runners");

        // Assert
        isKnown.Should().BeTrue();
    }

    [Test]
    public void IsFactionKnown_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var isKnown = _sut.IsFactionKnown(player, "IRON-COVENANT");

        // Assert
        isKnown.Should().BeTrue();
    }

    [Test]
    public void IsFactionKnown_EmptyFactionId_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var isKnown = _sut.IsFactionKnown(player, "");

        // Assert
        isKnown.Should().BeFalse();
    }

    #endregion

    #region Sign Meaning Tests

    [Test]
    public void GetSignMeaning_ReturnsNonEmptyStringForAllTypes()
    {
        // Act & Assert
        foreach (var signType in Enum.GetValues<ScavengerSignType>())
        {
            var meaning = _sut.GetSignMeaning(signType);
            meaning.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void GetCriticalContext_ReturnsNonEmptyStringForAllTypes()
    {
        // Act & Assert
        foreach (var signType in Enum.GetValues<ScavengerSignType>())
        {
            var context = _sut.GetCriticalContext(signType);
            context.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void GetMisinterpretation_ReturnsNonEmptyStringForAllTypes()
    {
        // Act & Assert
        foreach (var signType in Enum.GetValues<ScavengerSignType>())
        {
            var misinterpretation = _sut.GetMisinterpretation(signType);
            misinterpretation.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void GetMisinterpretation_TerritoryMarker_SuggestsSafeHaven()
    {
        // Act
        var misinterpretation = _sut.GetMisinterpretation(ScavengerSignType.TerritoryMarker);

        // Assert
        misinterpretation.Should().ContainAny("safe", "haven", "welcome");
    }

    [Test]
    public void GetMisinterpretation_WarningSign_SuggestsValuableSalvage()
    {
        // Act
        var misinterpretation = _sut.GetMisinterpretation(ScavengerSignType.WarningSign);

        // Assert
        misinterpretation.Should().ContainAny("salvage", "valuable", "hurry");
    }

    [Test]
    public void GetMisinterpretation_TabooSign_SuggestsTreasure()
    {
        // Act
        var misinterpretation = _sut.GetMisinterpretation(ScavengerSignType.TabooSign);

        // Assert
        misinterpretation.Should().ContainAny("treasure", "sacred");
    }

    #endregion

    #region Faction Display Name Tests

    [Test]
    [TestCase("iron-covenant", "Iron Covenant")]
    [TestCase("rust-walkers", "Rust Walkers")]
    [TestCase("silent-ones", "Silent Ones")]
    [TestCase("verdant-circle", "Verdant Circle")]
    [TestCase("ash-born", "Ash-Born")]
    public void GetFactionDisplayName_MajorFactions_ReturnsCorrectName(string factionId, string expectedName)
    {
        // Act
        var displayName = _sut.GetFactionDisplayName(factionId);

        // Assert
        displayName.Should().Be(expectedName);
    }

    [Test]
    public void GetFactionDisplayName_UnknownFaction_ConvertsKebabCaseToTitleCase()
    {
        // Act
        var displayName = _sut.GetFactionDisplayName("scrap-runners");

        // Assert
        displayName.Should().Be("Scrap Runners");
    }

    [Test]
    public void GetFactionDisplayName_EmptyString_ReturnsUnknownFaction()
    {
        // Act
        var displayName = _sut.GetFactionDisplayName("");

        // Assert
        displayName.Should().Be("Unknown Faction");
    }

    #endregion

    #region Sign Age Information Tests

    [Test]
    public void GetAgeDisplayString_ReturnsNarrativeDescriptionForAllAges()
    {
        // Act & Assert
        _sut.GetAgeDisplayString(SignAge.Fresh).Should().Contain("fresh");
        _sut.GetAgeDisplayString(SignAge.Recent).Should().Contain("recent");
        _sut.GetAgeDisplayString(SignAge.Old).Should().Contain("old");
        _sut.GetAgeDisplayString(SignAge.Faded).Should().Contain("faded");
        _sut.GetAgeDisplayString(SignAge.Ancient).Should().Contain("ancient");
    }

    [Test]
    public void GetReliabilityWarning_FreshAndRecent_ReturnsNull()
    {
        // Act & Assert
        _sut.GetReliabilityWarning(SignAge.Fresh).Should().BeNull();
        _sut.GetReliabilityWarning(SignAge.Recent).Should().BeNull();
    }

    [Test]
    public void GetReliabilityWarning_OldFadedAndAncient_ReturnsWarning()
    {
        // Act & Assert
        _sut.GetReliabilityWarning(SignAge.Old).Should().NotBeNullOrEmpty();
        _sut.GetReliabilityWarning(SignAge.Faded).Should().NotBeNullOrEmpty();
        _sut.GetReliabilityWarning(SignAge.Ancient).Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Interpretation Prerequisites Tests

    [Test]
    public void CanInterpret_NormalPlayer_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var canInterpret = _sut.CanInterpret(player);

        // Assert
        canInterpret.Should().BeTrue();
    }

    [Test]
    public void GetInterpretationBlockedReason_NormalPlayer_ReturnsNull()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var reason = _sut.GetInterpretationBlockedReason(player);

        // Assert
        reason.Should().BeNull();
    }

    #endregion

    #region SignInterpretationResult Value Object Tests

    [Test]
    public void SignInterpretationResult_Success_HasCorrectProperties()
    {
        // Act
        var result = SignInterpretationResult.Success(
            ScavengerSignType.TerritoryMarker,
            "This area belongs to Iron Covenant",
            "Iron Covenant",
            factionKnown: true,
            netSuccesses: 3,
            targetDc: 10);

        // Assert
        result.Interpreted.Should().BeTrue();
        result.SignType.Should().Be(ScavengerSignType.TerritoryMarker);
        result.Meaning.Should().Contain("Iron Covenant");
        result.FactionName.Should().Be("Iron Covenant");
        result.FactionKnown.Should().BeTrue();
        result.IsMisinterpretation.Should().BeFalse();
        result.IsCritical.Should().BeFalse(); // 3 < 5
        result.IsFailed.Should().BeFalse();
        result.IsAccurate.Should().BeTrue();
        result.GainedInformation.Should().BeTrue();
    }

    [Test]
    public void SignInterpretationResult_CriticalSuccess_HasCorrectProperties()
    {
        // Act
        var result = SignInterpretationResult.CriticalSuccess(
            ScavengerSignType.CacheIndicator,
            "Hidden supplies concealed north",
            "Rust Walkers",
            factionKnown: true,
            additionalContext: "Cache is hidden below ground level",
            netSuccesses: 7,
            targetDc: 14);

        // Assert
        result.Interpreted.Should().BeTrue();
        result.IsCritical.Should().BeTrue(); // 7 >= 5
        result.AdditionalContext.Should().Contain("below ground");
        result.IsMisinterpretation.Should().BeFalse();
        result.IsAccurate.Should().BeTrue();
        result.Margin.Should().Be(-7); // 7 - 14
    }

    [Test]
    public void SignInterpretationResult_Failure_HasCorrectProperties()
    {
        // Act
        var result = SignInterpretationResult.Failure(
            netSuccesses: 8,
            targetDc: 16);

        // Assert
        result.Interpreted.Should().BeFalse();
        result.SignType.Should().BeNull();
        result.Meaning.Should().Contain("incomprehensible");
        result.IsFailed.Should().BeTrue();
        result.IsMisinterpretation.Should().BeFalse();
        result.GainedInformation.Should().BeFalse();
        result.Margin.Should().Be(-8); // 8 - 16
    }

    [Test]
    public void SignInterpretationResult_Misinterpretation_HasCorrectProperties()
    {
        // Act
        var result = SignInterpretationResult.Misinterpretation(
            ScavengerSignType.WarningSign,
            "This indicates valuable salvage ahead!",
            "Silent Ones",
            targetDc: 12);

        // Assert
        result.Interpreted.Should().BeTrue(); // Player thinks they understood
        result.IsMisinterpretation.Should().BeTrue();
        result.Meaning.Should().Contain("salvage");
        result.IsCritical.Should().BeFalse();
        result.IsFailed.Should().BeFalse();
        result.IsAccurate.Should().BeFalse(); // The info is wrong
        result.GainedInformation.Should().BeTrue(); // Player believes they learned
    }

    [Test]
    public void SignInterpretationResult_Empty_HasCorrectProperties()
    {
        // Act
        var result = SignInterpretationResult.Empty();

        // Assert
        result.Interpreted.Should().BeFalse();
        result.NetSuccesses.Should().Be(0);
        result.TargetDc.Should().Be(0);
        result.GainedInformation.Should().BeFalse();
    }

    [Test]
    public void SignInterpretationResult_IsCritical_TrueWhenNetSuccessesAtLeast5()
    {
        // Arrange & Act
        var criticalResult = SignInterpretationResult.Success(
            ScavengerSignType.TerritoryMarker, "Test", "Test", true, 5, 10);
        var nonCriticalResult = SignInterpretationResult.Success(
            ScavengerSignType.TerritoryMarker, "Test", "Test", true, 4, 10);

        // Assert
        criticalResult.IsCritical.Should().BeTrue();
        nonCriticalResult.IsCritical.Should().BeFalse();
    }

    [Test]
    public void SignInterpretationResult_ToDisplayString_FailedInterpretation()
    {
        // Arrange
        var result = SignInterpretationResult.Failure();

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("meaning eludes you");
    }

    [Test]
    public void SignInterpretationResult_ToDisplayString_SuccessfulInterpretation()
    {
        // Arrange
        var result = SignInterpretationResult.Success(
            ScavengerSignType.TrailBlaze,
            "Path leads east safely",
            "Verdant Circle",
            true,
            3,
            10);

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("Trail Blaze");
        display.Should().Contain("Verdant Circle");
        display.Should().Contain("Path leads east safely");
    }

    [Test]
    public void SignInterpretationResult_ToDisplayString_CriticalSuccessShowsAdditionalContext()
    {
        // Arrange
        var result = SignInterpretationResult.CriticalSuccess(
            ScavengerSignType.TrailBlaze,
            "Path leads east safely",
            "Verdant Circle",
            true,
            "Path only safe during daylight",
            6,
            10);

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("[CRITICAL]");
        display.Should().Contain("daylight");
    }

    [Test]
    public void SignInterpretationResult_ToString_FormatsCorrectly()
    {
        // Arrange
        var success = SignInterpretationResult.Success(
            ScavengerSignType.HuntMarker, "Prey spotted", "Iron Covenant", true, 4, 14);
        var failure = SignInterpretationResult.Failure(10, 14);
        var fumble = SignInterpretationResult.Misinterpretation(
            ScavengerSignType.TabooSign, "Safe zone", "Unknown", 12);

        // Act & Assert
        success.ToString().Should().Contain("Interpreted");
        success.ToString().Should().Contain("Hunt Marker");

        failure.ToString().Should().Contain("Failed");

        fumble.ToString().Should().Contain("FUMBLE");
        fumble.ToString().Should().Contain("Misinterpreted");
    }

    #endregion

    #region ScavengerSign Value Object Tests

    [Test]
    public void ScavengerSign_TerritoryMarker_HasCorrectProperties()
    {
        // Act
        var sign = ScavengerSign.TerritoryMarker("iron-covenant", SignAge.Fresh);

        // Assert
        sign.SignType.Should().Be(ScavengerSignType.TerritoryMarker);
        sign.FactionId.Should().Be("iron-covenant");
        sign.Age.Should().Be(SignAge.Fresh);
        sign.BaseDc.Should().Be(10);
        sign.AgeModifier.Should().Be(0);
        sign.IsMajorFaction.Should().BeTrue();
        sign.IsReliable.Should().BeTrue();
        sign.IndicatesRecentActivity.Should().BeTrue();
    }

    [Test]
    public void ScavengerSign_CalculateInterpretationDc_KnownFaction()
    {
        // Arrange
        var sign = ScavengerSign.TerritoryMarker("rust-walkers", SignAge.Old);

        // Act
        var dc = sign.CalculateInterpretationDc(factionKnown: true);

        // Assert
        dc.Should().Be(11); // Base 10 + 1 age + 0 known
    }

    [Test]
    public void ScavengerSign_CalculateInterpretationDc_UnknownFaction()
    {
        // Arrange
        var sign = ScavengerSign.CacheIndicator(
            "mystery-clan",
            "north",
            "100m",
            SignAge.Faded);

        // Act
        var dc = sign.CalculateInterpretationDc(factionKnown: false);

        // Assert
        dc.Should().Be(20); // Base 14 + 2 age + 4 unknown
    }

    [Test]
    public void ScavengerSign_Create_UsesDefaultVisualDescription()
    {
        // Act
        var sign = ScavengerSign.Create(
            ScavengerSignType.WarningSign,
            "ash-born",
            "Danger ahead: Fire hazard",
            SignAge.Recent);

        // Assert
        sign.VisualDescription.Should().Contain("jagged lines");
    }

    [Test]
    public void ScavengerSign_WarningSign_HasCorrectVisualDescription()
    {
        // Act
        var sign = ScavengerSign.WarningSign(
            "iron-covenant",
            "Structural collapse risk");

        // Assert
        sign.VisualDescription.Should().Contain("jagged");
    }

    [Test]
    public void ScavengerSign_CacheIndicator_HasCorrectMeaning()
    {
        // Act
        var sign = ScavengerSign.CacheIndicator(
            "rust-walkers",
            "east",
            "200 meters");

        // Assert
        sign.Meaning.Should().Contain("east");
        sign.Meaning.Should().Contain("200 meters");
    }

    [Test]
    public void ScavengerSign_TrailBlaze_HasCorrectMeaning()
    {
        // Act
        var sign = ScavengerSign.TrailBlaze(
            "silent-ones",
            "northwest",
            SignAge.Fresh);

        // Assert
        sign.Meaning.Should().Contain("northwest");
        sign.Meaning.Should().Contain("safe");
    }

    [Test]
    public void ScavengerSign_HuntMarker_HasCorrectMeaning()
    {
        // Act
        var sign = ScavengerSign.HuntMarker(
            "verdant-circle",
            "Mole rats",
            "south",
            "3 hours");

        // Assert
        sign.Meaning.Should().Contain("Mole rats");
        sign.Meaning.Should().Contain("south");
        sign.Meaning.Should().Contain("3 hours");
    }

    [Test]
    public void ScavengerSign_TabooSign_HasCorrectMeaning()
    {
        // Act
        var sign = ScavengerSign.TabooSign(
            "ash-born",
            "Glitch pocket detected");

        // Assert
        sign.Meaning.Should().Contain("forbidden");
        sign.Meaning.Should().Contain("Glitch pocket");
    }

    [Test]
    public void ScavengerSign_ToDiscoveryString_DescribesVisualAndAge()
    {
        // Arrange
        var sign = ScavengerSign.WarningSign(
            "iron-covenant",
            "Radiation leak",
            SignAge.Old);

        // Act
        var discovery = sign.ToDiscoveryString();

        // Assert
        discovery.Should().Contain("notice");
        discovery.Should().Contain("old");
    }

    [Test]
    public void ScavengerSign_IsMajorFactionId_StaticMethod()
    {
        // Assert
        ScavengerSign.IsMajorFactionId("iron-covenant").Should().BeTrue();
        ScavengerSign.IsMajorFactionId("rust-walkers").Should().BeTrue();
        ScavengerSign.IsMajorFactionId("unknown-faction").Should().BeFalse();
        ScavengerSign.IsMajorFactionId("").Should().BeFalse();
    }

    [Test]
    public void ScavengerSign_GetMajorFactionIds_ReturnsFiveFactions()
    {
        // Act
        var majorFactions = ScavengerSign.GetMajorFactionIds();

        // Assert
        majorFactions.Should().HaveCount(5);
        majorFactions.Should().Contain("iron-covenant");
        majorFactions.Should().Contain("rust-walkers");
        majorFactions.Should().Contain("silent-ones");
        majorFactions.Should().Contain("verdant-circle");
        majorFactions.Should().Contain("ash-born");
    }

    #endregion

    #region ScavengerSignType Enum Extension Tests

    [Test]
    public void ScavengerSignType_GetBaseDc_ReturnsCorrectValues()
    {
        // Assert
        ScavengerSignType.TerritoryMarker.GetBaseDc().Should().Be(10);
        ScavengerSignType.WarningSign.GetBaseDc().Should().Be(12);
        ScavengerSignType.CacheIndicator.GetBaseDc().Should().Be(14);
        ScavengerSignType.TrailBlaze.GetBaseDc().Should().Be(10);
        ScavengerSignType.HuntMarker.GetBaseDc().Should().Be(14);
        ScavengerSignType.TabooSign.GetBaseDc().Should().Be(12);
    }

    [Test]
    public void ScavengerSignType_GetDisplayName_ReturnsCorrectNames()
    {
        // Assert
        ScavengerSignType.TerritoryMarker.GetDisplayName().Should().Be("Territory Marker");
        ScavengerSignType.WarningSign.GetDisplayName().Should().Be("Warning Sign");
        ScavengerSignType.CacheIndicator.GetDisplayName().Should().Be("Cache Indicator");
        ScavengerSignType.TrailBlaze.GetDisplayName().Should().Be("Trail Blaze");
        ScavengerSignType.HuntMarker.GetDisplayName().Should().Be("Hunt Marker");
        ScavengerSignType.TabooSign.GetDisplayName().Should().Be("Taboo Sign");
    }

    [Test]
    public void ScavengerSignType_GetDescription_ReturnsNonEmptyForAll()
    {
        // Act & Assert
        foreach (var signType in Enum.GetValues<ScavengerSignType>())
        {
            signType.GetDescription().Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void ScavengerSignType_IndicatesDanger_CorrectForEachType()
    {
        // Assert
        ScavengerSignType.TerritoryMarker.IndicatesDanger().Should().BeTrue();
        ScavengerSignType.WarningSign.IndicatesDanger().Should().BeTrue();
        ScavengerSignType.CacheIndicator.IndicatesDanger().Should().BeFalse();
        ScavengerSignType.TrailBlaze.IndicatesDanger().Should().BeFalse();
        ScavengerSignType.HuntMarker.IndicatesDanger().Should().BeFalse();
        ScavengerSignType.TabooSign.IndicatesDanger().Should().BeTrue();
    }

    [Test]
    public void ScavengerSignType_IndicatesResource_CorrectForEachType()
    {
        // Assert
        ScavengerSignType.TerritoryMarker.IndicatesResource().Should().BeFalse();
        ScavengerSignType.WarningSign.IndicatesResource().Should().BeFalse();
        ScavengerSignType.CacheIndicator.IndicatesResource().Should().BeTrue();
        ScavengerSignType.TrailBlaze.IndicatesResource().Should().BeTrue();
        ScavengerSignType.HuntMarker.IndicatesResource().Should().BeTrue();
        ScavengerSignType.TabooSign.IndicatesResource().Should().BeFalse();
    }

    [Test]
    public void ScavengerSignType_ProvidesNavigation_CorrectForEachType()
    {
        // Assert
        ScavengerSignType.TerritoryMarker.ProvidesNavigation().Should().BeFalse();
        ScavengerSignType.WarningSign.ProvidesNavigation().Should().BeFalse();
        ScavengerSignType.CacheIndicator.ProvidesNavigation().Should().BeTrue();
        ScavengerSignType.TrailBlaze.ProvidesNavigation().Should().BeTrue();
        ScavengerSignType.HuntMarker.ProvidesNavigation().Should().BeTrue();
        ScavengerSignType.TabooSign.ProvidesNavigation().Should().BeFalse();
    }

    [Test]
    public void ScavengerSignType_MisinterpretationCausesFactionConflict_CorrectForEachType()
    {
        // Assert
        ScavengerSignType.TerritoryMarker.MisinterpretationCausesFactionConflict().Should().BeTrue();
        ScavengerSignType.WarningSign.MisinterpretationCausesFactionConflict().Should().BeFalse();
        ScavengerSignType.CacheIndicator.MisinterpretationCausesFactionConflict().Should().BeFalse();
        ScavengerSignType.TrailBlaze.MisinterpretationCausesFactionConflict().Should().BeFalse();
        ScavengerSignType.HuntMarker.MisinterpretationCausesFactionConflict().Should().BeFalse();
        ScavengerSignType.TabooSign.MisinterpretationCausesFactionConflict().Should().BeTrue();
    }

    [Test]
    public void ScavengerSignType_MisinterpretationCausesEnvironmentalDanger_CorrectForEachType()
    {
        // Assert
        ScavengerSignType.TerritoryMarker.MisinterpretationCausesEnvironmentalDanger().Should().BeFalse();
        ScavengerSignType.WarningSign.MisinterpretationCausesEnvironmentalDanger().Should().BeTrue();
        ScavengerSignType.CacheIndicator.MisinterpretationCausesEnvironmentalDanger().Should().BeFalse();
        ScavengerSignType.TrailBlaze.MisinterpretationCausesEnvironmentalDanger().Should().BeTrue();
        ScavengerSignType.HuntMarker.MisinterpretationCausesEnvironmentalDanger().Should().BeFalse();
        ScavengerSignType.TabooSign.MisinterpretationCausesEnvironmentalDanger().Should().BeTrue();
    }

    #endregion

    #region SignAge Enum Extension Tests

    [Test]
    public void SignAge_GetDcModifier_ReturnsCorrectValues()
    {
        // Assert
        SignAge.Fresh.GetDcModifier().Should().Be(0);
        SignAge.Recent.GetDcModifier().Should().Be(0);
        SignAge.Old.GetDcModifier().Should().Be(1);
        SignAge.Faded.GetDcModifier().Should().Be(2);
        SignAge.Ancient.GetDcModifier().Should().Be(4);
    }

    [Test]
    public void SignAge_GetDisplayName_ReturnsCorrectNames()
    {
        // Assert
        SignAge.Fresh.GetDisplayName().Should().Be("Fresh");
        SignAge.Recent.GetDisplayName().Should().Be("Recent");
        SignAge.Old.GetDisplayName().Should().Be("Old");
        SignAge.Faded.GetDisplayName().Should().Be("Faded");
        SignAge.Ancient.GetDisplayName().Should().Be("Ancient");
    }

    [Test]
    public void SignAge_ToDisplayString_ReturnsNarrativeDescriptions()
    {
        // Assert
        SignAge.Fresh.ToDisplayString().Should().Contain("fresh");
        SignAge.Recent.ToDisplayString().Should().Contain("recent");
        SignAge.Old.ToDisplayString().Should().Contain("old");
        SignAge.Faded.ToDisplayString().Should().Contain("faded");
        SignAge.Ancient.ToDisplayString().Should().Contain("ancient");
    }

    [Test]
    public void SignAge_IsReliable_TrueOnlyForFreshAndRecent()
    {
        // Assert
        SignAge.Fresh.IsReliable().Should().BeTrue();
        SignAge.Recent.IsReliable().Should().BeTrue();
        SignAge.Old.IsReliable().Should().BeFalse();
        SignAge.Faded.IsReliable().Should().BeFalse();
        SignAge.Ancient.IsReliable().Should().BeFalse();
    }

    [Test]
    public void SignAge_IndicatesRecentActivity_TrueForFreshRecentOld()
    {
        // Assert
        SignAge.Fresh.IndicatesRecentActivity().Should().BeTrue();
        SignAge.Recent.IndicatesRecentActivity().Should().BeTrue();
        SignAge.Old.IndicatesRecentActivity().Should().BeTrue();
        SignAge.Faded.IndicatesRecentActivity().Should().BeFalse();
        SignAge.Ancient.IndicatesRecentActivity().Should().BeFalse();
    }

    [Test]
    public void SignAge_GetApproximateTime_ReturnsReasonableDescriptions()
    {
        // Assert
        SignAge.Fresh.GetApproximateTime().Should().Contain("hour");
        SignAge.Recent.GetApproximateTime().Should().Contain("day");
        SignAge.Old.GetApproximateTime().Should().Contain("week");
        SignAge.Faded.GetApproximateTime().Should().Contain("weeks");
        SignAge.Ancient.GetApproximateTime().Should().Contain("months");
    }

    [Test]
    public void SignAge_GetReliabilityWarning_NullForReliableAges()
    {
        // Assert
        SignAge.Fresh.GetReliabilityWarning().Should().BeNull();
        SignAge.Recent.GetReliabilityWarning().Should().BeNull();
    }

    [Test]
    public void SignAge_GetReliabilityWarning_WarningsForUnreliableAges()
    {
        // Assert
        SignAge.Old.GetReliabilityWarning().Should().NotBeNullOrEmpty();
        SignAge.Faded.GetReliabilityWarning().Should().Contain("Warning");
        SignAge.Ancient.GetReliabilityWarning().Should().Contain("Caution");
    }

    [Test]
    public void SignAge_GetDescription_ReturnsDetailedDescriptionForAll()
    {
        // Act & Assert
        foreach (var age in Enum.GetValues<SignAge>())
        {
            age.GetDescription().Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock SkillCheckService for testing.
    /// </summary>
    private static SkillCheckService CreateMockSkillCheckService()
    {
        var seededRandom = new Random(42);
        var diceLogger = Substitute.For<ILogger<DiceService>>();
#pragma warning disable CS0618 // Type or member is obsolete
        var diceService = new DiceService(diceLogger, seededRandom);
#pragma warning restore CS0618
        var configProvider = Substitute.For<IGameConfigurationProvider>();
        var logger = Substitute.For<ILogger<SkillCheckService>>();

        return new SkillCheckService(diceService, configProvider, logger);
    }

    /// <summary>
    /// Creates a test player with standard attributes for interpretation tests.
    /// </summary>
    private static Player CreateTestPlayer()
    {
        return new Player(
            name: "Test Scavenger",
            raceId: "human",
            backgroundId: "scavenger",
            attributes: new PlayerAttributes(
                might: 8,
                fortitude: 8,
                will: 10,
                wits: 12,
                finesse: 10
            )
        );
    }

    #endregion
}
