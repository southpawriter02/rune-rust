// ═══════════════════════════════════════════════════════════════════════════════
// AchievementDefinitionTests.cs
// Unit tests for AchievementDefinition entity, AchievementCondition record,
// AchievementProvider, and achievement-related enums.
// Version: 0.12.1a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the Achievement system components.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>AchievementDefinition factory creation and validation</description></item>
///   <item><description>Points calculation derived from tier</description></item>
///   <item><description>Secret achievement display name/description handling</description></item>
///   <item><description>AchievementCondition evaluation methods</description></item>
///   <item><description>AchievementCondition progress calculation</description></item>
///   <item><description>Achievement enum values and counts</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class AchievementDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════
    // FACTORY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidData_ReturnsDefinition()
    {
        // Arrange
        var conditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100)
        };

        // Act
        var achievement = AchievementDefinition.Create(
            achievementId: "monster-slayer",
            name: "Monster Slayer",
            description: "Defeat 100 monsters",
            category: AchievementCategory.Combat,
            tier: AchievementTier.Silver,
            conditions: conditions);

        // Assert
        achievement.Should().NotBeNull();
        achievement.Id.Should().NotBe(Guid.Empty);
        achievement.AchievementId.Should().Be("monster-slayer");
        achievement.Name.Should().Be("Monster Slayer");
        achievement.Description.Should().Be("Defeat 100 monsters");
        achievement.Category.Should().Be(AchievementCategory.Combat);
        achievement.Tier.Should().Be(AchievementTier.Silver);
        achievement.Conditions.Should().HaveCount(1);
        achievement.IsSecret.Should().BeFalse();
        achievement.IconPath.Should().BeNull();
    }

    [Test]
    public void Create_WithSecretFlag_SetsIsSecretTrue()
    {
        // Arrange
        var conditions = new List<AchievementCondition>
        {
            new("secretsFound", ComparisonOperator.GreaterThanOrEqual, 1)
        };

        // Act
        var achievement = AchievementDefinition.Create(
            achievementId: "hidden-treasure",
            name: "Hidden Treasure",
            description: "Find a secret",
            category: AchievementCategory.Secret,
            tier: AchievementTier.Gold,
            conditions: conditions,
            isSecret: true);

        // Assert
        achievement.IsSecret.Should().BeTrue();
    }

    [Test]
    public void Create_WithIconPath_SetsIconPath()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();

        // Act
        var achievement = AchievementDefinition.Create(
            achievementId: "first-blood",
            name: "First Blood",
            description: "Defeat your first monster",
            category: AchievementCategory.Combat,
            tier: AchievementTier.Bronze,
            conditions: conditions,
            iconPath: "icons/achievements/first-blood.png");

        // Assert
        achievement.IconPath.Should().Be("icons/achievements/first-blood.png");
    }

    [Test]
    public void Create_GeneratesUniqueIds()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();

        // Act
        var achievement1 = AchievementDefinition.Create(
            "test-1", "Test 1", "Description 1",
            AchievementCategory.Combat, AchievementTier.Bronze, conditions);

        var achievement2 = AchievementDefinition.Create(
            "test-2", "Test 2", "Description 2",
            AchievementCategory.Combat, AchievementTier.Bronze, conditions);

        // Assert
        achievement1.Id.Should().NotBe(achievement2.Id);
    }

    [Test]
    public void Create_WithNullAchievementId_ThrowsArgumentException()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();

        // Act
        var act = () => AchievementDefinition.Create(
            achievementId: null!,
            name: "Test",
            description: "Test",
            category: AchievementCategory.Combat,
            tier: AchievementTier.Bronze,
            conditions: conditions);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();

        // Act
        var act = () => AchievementDefinition.Create(
            achievementId: "test",
            name: "",
            description: "Test",
            category: AchievementCategory.Combat,
            tier: AchievementTier.Bronze,
            conditions: conditions);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithWhitespaceDescription_ThrowsArgumentException()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();

        // Act
        var act = () => AchievementDefinition.Create(
            achievementId: "test",
            name: "Test",
            description: "   ",
            category: AchievementCategory.Combat,
            tier: AchievementTier.Bronze,
            conditions: conditions);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithNullConditions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => AchievementDefinition.Create(
            achievementId: "test",
            name: "Test",
            description: "Test description",
            category: AchievementCategory.Combat,
            tier: AchievementTier.Bronze,
            conditions: null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // POINTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [TestCase(AchievementTier.Bronze, 10)]
    [TestCase(AchievementTier.Silver, 25)]
    [TestCase(AchievementTier.Gold, 50)]
    [TestCase(AchievementTier.Platinum, 100)]
    public void Points_DerivedFromTier_ReturnsCorrectValue(AchievementTier tier, int expectedPoints)
    {
        // Arrange
        var conditions = new List<AchievementCondition>();
        var achievement = AchievementDefinition.Create(
            "test", "Test", "Test description",
            AchievementCategory.Combat, tier, conditions);

        // Act
        var points = achievement.Points;

        // Assert
        points.Should().Be(expectedPoints);
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY NAME/DESCRIPTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetDisplayName_SecretAndLocked_ReturnsHidden()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();
        var achievement = AchievementDefinition.Create(
            "secret-test", "Secret Name", "Secret description",
            AchievementCategory.Secret, AchievementTier.Gold, conditions,
            isSecret: true);

        // Act
        var displayName = achievement.GetDisplayName(isUnlocked: false);

        // Assert
        displayName.Should().Be("???");
    }

    [Test]
    public void GetDisplayName_SecretAndUnlocked_ReturnsActualName()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();
        var achievement = AchievementDefinition.Create(
            "secret-test", "Secret Name", "Secret description",
            AchievementCategory.Secret, AchievementTier.Gold, conditions,
            isSecret: true);

        // Act
        var displayName = achievement.GetDisplayName(isUnlocked: true);

        // Assert
        displayName.Should().Be("Secret Name");
    }

    [Test]
    public void GetDisplayName_NotSecret_ReturnsActualNameRegardlessOfUnlockStatus()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();
        var achievement = AchievementDefinition.Create(
            "normal-test", "Normal Name", "Normal description",
            AchievementCategory.Combat, AchievementTier.Bronze, conditions,
            isSecret: false);

        // Act
        var lockedName = achievement.GetDisplayName(isUnlocked: false);
        var unlockedName = achievement.GetDisplayName(isUnlocked: true);

        // Assert
        lockedName.Should().Be("Normal Name");
        unlockedName.Should().Be("Normal Name");
    }

    [Test]
    public void GetDisplayDescription_SecretAndLocked_ReturnsHiddenMessage()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();
        var achievement = AchievementDefinition.Create(
            "secret-test", "Secret Name", "Secret description",
            AchievementCategory.Secret, AchievementTier.Gold, conditions,
            isSecret: true);

        // Act
        var displayDesc = achievement.GetDisplayDescription(isUnlocked: false);

        // Assert
        displayDesc.Should().Be("Hidden achievement - unlock to reveal!");
    }

    [Test]
    public void GetDisplayDescription_SecretAndUnlocked_ReturnsActualDescription()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();
        var achievement = AchievementDefinition.Create(
            "secret-test", "Secret Name", "Secret description",
            AchievementCategory.Secret, AchievementTier.Gold, conditions,
            isSecret: true);

        // Act
        var displayDesc = achievement.GetDisplayDescription(isUnlocked: true);

        // Assert
        displayDesc.Should().Be("Secret description");
    }

    // ═══════════════════════════════════════════════════════════════
    // TO STRING TEST
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var conditions = new List<AchievementCondition>();
        var achievement = AchievementDefinition.Create(
            "first-blood", "First Blood", "Defeat your first monster",
            AchievementCategory.Combat, AchievementTier.Bronze, conditions);

        // Act
        var result = achievement.ToString();

        // Assert
        result.Should().Contain("first-blood");
        result.Should().Contain("First Blood");
        result.Should().Contain("Combat");
        result.Should().Contain("Bronze");
        result.Should().Contain("10 pts");
    }
}

/// <summary>
/// Unit tests for the AchievementCondition record.
/// </summary>
[TestFixture]
public class AchievementConditionTests
{
    // ═══════════════════════════════════════════════════════════════
    // EVALUATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [TestCase(ComparisonOperator.GreaterThanOrEqual, 100, 150, true)]
    [TestCase(ComparisonOperator.GreaterThanOrEqual, 100, 100, true)]
    [TestCase(ComparisonOperator.GreaterThanOrEqual, 100, 50, false)]
    [TestCase(ComparisonOperator.LessThanOrEqual, 10, 5, true)]
    [TestCase(ComparisonOperator.LessThanOrEqual, 10, 10, true)]
    [TestCase(ComparisonOperator.LessThanOrEqual, 10, 15, false)]
    [TestCase(ComparisonOperator.Equals, 50, 50, true)]
    [TestCase(ComparisonOperator.Equals, 50, 49, false)]
    [TestCase(ComparisonOperator.Equals, 50, 51, false)]
    public void Evaluate_WithOperator_ReturnsExpectedResult(
        ComparisonOperator op,
        long threshold,
        long actualValue,
        bool expectedResult)
    {
        // Arrange
        var condition = new AchievementCondition("testStat", op, threshold);

        // Act
        var result = condition.Evaluate(actualValue);

        // Assert
        result.Should().Be(expectedResult);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET PROGRESS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetProgress_GreaterThanOrEqual_ReturnsProgressPercentage()
    {
        // Arrange
        var condition = new AchievementCondition(
            "monstersKilled",
            ComparisonOperator.GreaterThanOrEqual,
            100);

        // Act
        var progress = condition.GetProgress(50);

        // Assert
        progress.Should().BeApproximately(0.5, 0.001);
    }

    [Test]
    public void GetProgress_GreaterThanOrEqual_CapsAtOne()
    {
        // Arrange
        var condition = new AchievementCondition(
            "monstersKilled",
            ComparisonOperator.GreaterThanOrEqual,
            100);

        // Act
        var progress = condition.GetProgress(150);

        // Assert
        progress.Should().Be(1.0);
    }

    [Test]
    public void GetProgress_Equals_ReturnsBinaryProgress()
    {
        // Arrange
        var condition = new AchievementCondition(
            "level",
            ComparisonOperator.Equals,
            10);

        // Act
        var notMet = condition.GetProgress(5);
        var met = condition.GetProgress(10);

        // Assert
        notMet.Should().Be(0.0);
        met.Should().Be(1.0);
    }

    [Test]
    public void GetProgress_LessThanOrEqual_ReturnsBinaryProgress()
    {
        // Arrange
        var condition = new AchievementCondition(
            "deaths",
            ComparisonOperator.LessThanOrEqual,
            0);

        // Act
        var met = condition.GetProgress(0);
        var notMet = condition.GetProgress(1);

        // Assert
        met.Should().Be(1.0);
        notMet.Should().Be(0.0);
    }

    [Test]
    public void GetProgress_WithZeroThreshold_HandlesCorrectly()
    {
        // Arrange
        var condition = new AchievementCondition(
            "deaths",
            ComparisonOperator.GreaterThanOrEqual,
            0);

        // Act
        var progressZero = condition.GetProgress(0);
        var progressNonZero = condition.GetProgress(5);

        // Assert
        progressZero.Should().Be(1.0);
        progressNonZero.Should().Be(0.0);
    }

    // ═══════════════════════════════════════════════════════════════
    // DESCRIPTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Description_FormatsCorrectly()
    {
        // Arrange
        var condition = new AchievementCondition(
            "monstersKilled",
            ComparisonOperator.GreaterThanOrEqual,
            100);

        // Act
        var description = condition.Description;

        // Assert
        description.Should().Be("monstersKilled >= 100");
    }

    [TestCase(ComparisonOperator.GreaterThanOrEqual, ">=")]
    [TestCase(ComparisonOperator.LessThanOrEqual, "<=")]
    [TestCase(ComparisonOperator.Equals, "==")]
    public void OperatorSymbol_ReturnsCorrectSymbol(ComparisonOperator op, string expectedSymbol)
    {
        // Arrange
        var condition = new AchievementCondition("stat", op, 1);

        // Act
        var symbol = condition.OperatorSymbol;

        // Assert
        symbol.Should().Be(expectedSymbol);
    }
}

/// <summary>
/// Unit tests for Achievement-related enums.
/// </summary>
[TestFixture]
public class AchievementEnumTests
{
    // ═══════════════════════════════════════════════════════════════
    // ACHIEVEMENT TIER TESTS
    // ═══════════════════════════════════════════════════════════════

    [TestCase(AchievementTier.Bronze, 10)]
    [TestCase(AchievementTier.Silver, 25)]
    [TestCase(AchievementTier.Gold, 50)]
    [TestCase(AchievementTier.Platinum, 100)]
    public void AchievementTier_HasCorrectPointValue(AchievementTier tier, int expectedValue)
    {
        // Act
        var value = (int)tier;

        // Assert
        value.Should().Be(expectedValue);
    }

    [Test]
    public void AchievementTier_HasFourValues()
    {
        // Act
        var values = Enum.GetValues<AchievementTier>();

        // Assert
        values.Should().HaveCount(4);
    }

    // ═══════════════════════════════════════════════════════════════
    // ACHIEVEMENT CATEGORY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AchievementCategory_HasSixValues()
    {
        // Act
        var values = Enum.GetValues<AchievementCategory>();

        // Assert
        values.Should().HaveCount(6);
        values.Should().Contain(AchievementCategory.Combat);
        values.Should().Contain(AchievementCategory.Exploration);
        values.Should().Contain(AchievementCategory.Progression);
        values.Should().Contain(AchievementCategory.Collection);
        values.Should().Contain(AchievementCategory.Challenge);
        values.Should().Contain(AchievementCategory.Secret);
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPARISON OPERATOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ComparisonOperator_HasThreeValues()
    {
        // Act
        var values = Enum.GetValues<ComparisonOperator>();

        // Assert
        values.Should().HaveCount(3);
        values.Should().Contain(ComparisonOperator.GreaterThanOrEqual);
        values.Should().Contain(ComparisonOperator.LessThanOrEqual);
        values.Should().Contain(ComparisonOperator.Equals);
    }
}
