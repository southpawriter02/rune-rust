// ═══════════════════════════════════════════════════════════════════════════════
// DerivedStatFormulaTests.cs
// Unit tests for the DerivedStatFormula value object verifying formula
// calculations, archetype bonuses, lineage bonuses, lineage multipliers,
// factory validation, helper methods, and string representation.
// Version: 0.17.2d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="DerivedStatFormula"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that DerivedStatFormula correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Calculates derived stats from attribute values with correct scaling</description></item>
///   <item><description>Applies archetype bonuses to calculations</description></item>
///   <item><description>Applies lineage flat bonuses to calculations</description></item>
///   <item><description>Applies lineage multipliers after all additive bonuses</description></item>
///   <item><description>Validates factory method parameters</description></item>
///   <item><description>Returns correct helper method values and string output</description></item>
/// </list>
/// </remarks>
/// <seealso cref="DerivedStatFormula"/>
/// <seealso cref="DerivedStats"/>
[TestFixture]
public class DerivedStatFormulaTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates the standard Warrior attribute build: M4, F3, Wi2, Wl2, S4.
    /// Total points: 15 (4+3+2+2+4 = 15 base values minus 5 base of 1 = 10 points spent).
    /// </summary>
    private static Dictionary<CoreAttribute, int> CreateWarriorAttributes() =>
        new()
        {
            { CoreAttribute.Might, 4 },
            { CoreAttribute.Finesse, 3 },
            { CoreAttribute.Wits, 2 },
            { CoreAttribute.Will, 2 },
            { CoreAttribute.Sturdiness, 4 }
        };

    /// <summary>
    /// Creates the standard Mystic attribute build: M2, F3, Wi4, Wl4, S2.
    /// Total points: 15 (2+3+4+4+2 = 15 base values minus 5 base of 1 = 10 points spent).
    /// </summary>
    private static Dictionary<CoreAttribute, int> CreateMysticAttributes() =>
        new()
        {
            { CoreAttribute.Might, 2 },
            { CoreAttribute.Finesse, 3 },
            { CoreAttribute.Wits, 4 },
            { CoreAttribute.Will, 4 },
            { CoreAttribute.Sturdiness, 2 }
        };

    /// <summary>
    /// Creates the Max HP formula: (STURDINESS × 10) + 50 + ArchetypeBonus + LineageBonus.
    /// </summary>
    private static DerivedStatFormula CreateMaxHpFormula() =>
        DerivedStatFormula.Create(
            statName: "MaxHp",
            baseValue: 50,
            attributeScaling: new Dictionary<CoreAttribute, float>
            {
                { CoreAttribute.Sturdiness, 10f }
            },
            archetypeBonuses: new Dictionary<string, int>
            {
                { "warrior", 49 },
                { "skirmisher", 30 },
                { "mystic", 20 },
                { "adept", 30 }
            },
            lineageBonuses: new Dictionary<string, int>
            {
                { "clan-born", 5 }
            });

    /// <summary>
    /// Creates the Max Aether Pool formula without lineage multiplier:
    /// (WILL × 10) + (WITS × 5) + ArchetypeBonus.
    /// </summary>
    private static DerivedStatFormula CreateMaxAetherPoolFormula() =>
        DerivedStatFormula.Create(
            statName: "MaxAetherPool",
            baseValue: 0,
            attributeScaling: new Dictionary<CoreAttribute, float>
            {
                { CoreAttribute.Will, 10f },
                { CoreAttribute.Wits, 5f }
            },
            archetypeBonuses: new Dictionary<string, int>
            {
                { "mystic", 20 }
            });

    /// <summary>
    /// Creates the Max Aether Pool formula with Rune-Marked lineage multiplier:
    /// ((WILL × 10) + (WITS × 5) + ArchetypeBonus + LineageBonus) × LineageMultiplier.
    /// </summary>
    private static DerivedStatFormula CreateMaxAetherPoolFormulaWithMultiplier() =>
        DerivedStatFormula.Create(
            statName: "MaxAetherPool",
            baseValue: 0,
            attributeScaling: new Dictionary<CoreAttribute, float>
            {
                { CoreAttribute.Will, 10f },
                { CoreAttribute.Wits, 5f }
            },
            archetypeBonuses: new Dictionary<string, int>
            {
                { "mystic", 20 }
            },
            lineageBonuses: new Dictionary<string, int>
            {
                { "rune-marked", 5 }
            },
            lineageMultipliers: new Dictionary<string, float>
            {
                { "rune-marked", 1.1f }
            });

    /// <summary>
    /// Creates the Soak formula: (STURDINESS ÷ 2) + LineageBonus.
    /// </summary>
    private static DerivedStatFormula CreateSoakFormula() =>
        DerivedStatFormula.Create(
            statName: "Soak",
            baseValue: 0,
            attributeScaling: new Dictionary<CoreAttribute, float>
            {
                { CoreAttribute.Sturdiness, 0.5f }
            },
            lineageBonuses: new Dictionary<string, int>
            {
                { "iron-blooded", 2 }
            });

    /// <summary>
    /// Creates the Movement Speed formula: 5 + LineageBonus.
    /// </summary>
    private static DerivedStatFormula CreateMovementSpeedFormula() =>
        DerivedStatFormula.Create(
            statName: "MovementSpeed",
            baseValue: 5,
            attributeScaling: new Dictionary<CoreAttribute, float>(),
            lineageBonuses: new Dictionary<string, int>
            {
                { "vargr-kin", 1 }
            });

    /// <summary>
    /// Creates the Carrying Capacity formula: MIGHT × 10.
    /// </summary>
    private static DerivedStatFormula CreateCarryingCapacityFormula() =>
        DerivedStatFormula.Create(
            statName: "CarryingCapacity",
            baseValue: 0,
            attributeScaling: new Dictionary<CoreAttribute, float>
            {
                { CoreAttribute.Might, 10f }
            });

    /// <summary>
    /// Creates the Initiative formula: FINESSE + (WITS ÷ 2).
    /// Uses Finesse × 1.0 and Wits × 0.5 to achieve integer division behavior.
    /// </summary>
    private static DerivedStatFormula CreateInitiativeFormula() =>
        DerivedStatFormula.Create(
            statName: "Initiative",
            baseValue: 0,
            attributeScaling: new Dictionary<CoreAttribute, float>
            {
                { CoreAttribute.Finesse, 1f },
                { CoreAttribute.Wits, 0.5f }
            });

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION TESTS — Max HP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Warrior Max HP formula calculates correctly:
    /// (4 × 10) + 50 + 49 = 139.
    /// </summary>
    [Test]
    public void Calculate_WarriorMaxHp_Returns139()
    {
        // Arrange
        var formula = CreateMaxHpFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", null);

        // Assert
        result.Should().Be(139, "(4 × 10) + 50 + 49 = 139");
    }

    /// <summary>
    /// Verifies that the Warrior Max HP formula with Clan-Born lineage bonus
    /// calculates correctly: (4 × 10) + 50 + 49 + 5 = 144.
    /// </summary>
    [Test]
    public void Calculate_WarriorMaxHpWithClanBorn_Returns144()
    {
        // Arrange
        var formula = CreateMaxHpFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", "clan-born");

        // Assert
        result.Should().Be(144, "(4 × 10) + 50 + 49 + 5 = 144");
    }

    /// <summary>
    /// Verifies that the Mystic Max HP formula calculates correctly:
    /// (2 × 10) + 50 + 20 = 90.
    /// </summary>
    [Test]
    public void Calculate_MysticMaxHp_Returns90()
    {
        // Arrange
        var formula = CreateMaxHpFormula();
        var attributes = CreateMysticAttributes();

        // Act
        var result = formula.Calculate(attributes, "mystic", null);

        // Assert
        result.Should().Be(90, "(2 × 10) + 50 + 20 = 90");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION TESTS — Max Aether Pool
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Mystic Max Aether Pool formula (without lineage multiplier)
    /// calculates correctly: (4 × 10) + (4 × 5) + 20 = 80.
    /// </summary>
    [Test]
    public void Calculate_MysticMaxAetherPool_Returns80()
    {
        // Arrange
        var formula = CreateMaxAetherPoolFormula();
        var attributes = CreateMysticAttributes();

        // Act
        var result = formula.Calculate(attributes, "mystic", null);

        // Assert
        result.Should().Be(80, "(4 × 10) + (4 × 5) + 20 = 80");
    }

    /// <summary>
    /// Verifies that the Rune-Marked lineage multiplier applies correctly to
    /// the Mystic Aether Pool: ((4 × 10) + (4 × 5) + 20 + 5) × 1.10 = 93.5 → 93.
    /// </summary>
    [Test]
    public void Calculate_RuneMarkedMysticAether_AppliesMultiplier()
    {
        // Arrange
        var formula = CreateMaxAetherPoolFormulaWithMultiplier();
        var attributes = CreateMysticAttributes();

        // Act
        var result = formula.Calculate(attributes, "mystic", "rune-marked");

        // Assert — (40 + 20 + 20 + 5) × 1.10 = 93.5 → truncated to 93
        result.Should().Be(93, "((4×10) + (4×5) + 20 + 5) × 1.10 = 93.5 → 93");
    }

    /// <summary>
    /// Verifies that the Warrior Aether Pool (no archetype bonus, no lineage)
    /// calculates correctly: (2 × 10) + (2 × 5) = 30.
    /// </summary>
    [Test]
    public void Calculate_WarriorAetherPool_Returns30()
    {
        // Arrange
        var formula = CreateMaxAetherPoolFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", null);

        // Assert
        result.Should().Be(30, "(2 × 10) + (2 × 5) + 0 = 30");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION TESTS — Soak
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Soak formula calculates correctly for a Warrior:
    /// (4 × 0.5) = 2.
    /// </summary>
    [Test]
    public void Calculate_WarriorSoak_Returns2()
    {
        // Arrange
        var formula = CreateSoakFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", null);

        // Assert
        result.Should().Be(2, "(4 × 0.5) = 2");
    }

    /// <summary>
    /// Verifies that the Iron-Blooded lineage bonus applies correctly to Soak:
    /// (4 × 0.5) + 2 = 4.
    /// </summary>
    [Test]
    public void Calculate_IronBloodedSoak_AppliesLineageBonus()
    {
        // Arrange
        var formula = CreateSoakFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", "iron-blooded");

        // Assert
        result.Should().Be(4, "(4 × 0.5) + 2 = 4");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION TESTS — Movement Speed
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the base Movement Speed is 5 without lineage bonus.
    /// </summary>
    [Test]
    public void Calculate_BaseMovementSpeed_Returns5()
    {
        // Arrange
        var formula = CreateMovementSpeedFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", null);

        // Assert
        result.Should().Be(5, "base movement speed is 5");
    }

    /// <summary>
    /// Verifies that the Vargr-Kin lineage bonus applies correctly to Movement Speed:
    /// 5 + 1 = 6.
    /// </summary>
    [Test]
    public void Calculate_VargrKinMovement_AppliesLineageBonus()
    {
        // Arrange
        var formula = CreateMovementSpeedFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", "vargr-kin");

        // Assert
        result.Should().Be(6, "5 + 1 = 6");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION TESTS — Carrying Capacity
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Carrying Capacity formula calculates correctly:
    /// Warrior with Might 4: 4 × 10 = 40.
    /// </summary>
    [Test]
    public void Calculate_CarryingCapacity_ReturnsCorrectValue()
    {
        // Arrange
        var formula = CreateCarryingCapacityFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", null);

        // Assert
        result.Should().Be(40, "4 × 10 = 40");
    }

    /// <summary>
    /// Verifies that the Mystic Carrying Capacity calculates correctly:
    /// Mystic with Might 2: 2 × 10 = 20.
    /// </summary>
    [Test]
    public void Calculate_MysticCarryingCapacity_Returns20()
    {
        // Arrange
        var formula = CreateCarryingCapacityFormula();
        var attributes = CreateMysticAttributes();

        // Act
        var result = formula.Calculate(attributes, "mystic", null);

        // Assert
        result.Should().Be(20, "2 × 10 = 20");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION TESTS — Initiative
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Warrior Initiative formula calculates correctly:
    /// 3 + (2 × 0.5) = 3 + 1 = 4.
    /// </summary>
    [Test]
    public void Calculate_WarriorInitiative_Returns4()
    {
        // Arrange
        var formula = CreateInitiativeFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "warrior", null);

        // Assert
        result.Should().Be(4, "3 + (2 × 0.5) = 4");
    }

    /// <summary>
    /// Verifies that the Mystic Initiative formula calculates correctly:
    /// 3 + (4 × 0.5) = 3 + 2 = 5.
    /// </summary>
    [Test]
    public void Calculate_MysticInitiative_Returns5()
    {
        // Arrange
        var formula = CreateInitiativeFormula();
        var attributes = CreateMysticAttributes();

        // Act
        var result = formula.Calculate(attributes, "mystic", null);

        // Assert
        result.Should().Be(5, "3 + (4 × 0.5) = 5");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION TESTS — No Archetype/Lineage
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Calculate works when archetypeId and lineageId are both null,
    /// returning only base + attribute scaling.
    /// </summary>
    [Test]
    public void Calculate_WithNullArchetypeAndLineage_ReturnsBaseAndScaling()
    {
        // Arrange
        var formula = CreateMaxHpFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, null, null);

        // Assert — (4 × 10) + 50 = 90, no archetype or lineage bonus
        result.Should().Be(90, "(4 × 10) + 50 = 90 with no bonuses");
    }

    /// <summary>
    /// Verifies that Calculate ignores unknown archetype IDs gracefully.
    /// </summary>
    [Test]
    public void Calculate_WithUnknownArchetype_IgnoresBonus()
    {
        // Arrange
        var formula = CreateMaxHpFormula();
        var attributes = CreateWarriorAttributes();

        // Act
        var result = formula.Calculate(attributes, "unknown-archetype", null);

        // Assert — (4 × 10) + 50 = 90, unknown archetype has no bonus
        result.Should().Be(90, "unknown archetype should not contribute a bonus");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Create
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.Create"/> throws
    /// <see cref="ArgumentException"/> when the stat name is null.
    /// </summary>
    [Test]
    public void Create_WithNullStatName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DerivedStatFormula.Create(
            statName: null!,
            baseValue: 50,
            attributeScaling: new Dictionary<CoreAttribute, float>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.Create"/> throws
    /// <see cref="ArgumentException"/> when the stat name is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceStatName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DerivedStatFormula.Create(
            statName: "   ",
            baseValue: 50,
            attributeScaling: new Dictionary<CoreAttribute, float>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.Create"/> throws
    /// <see cref="ArgumentNullException"/> when attribute scaling is null.
    /// </summary>
    [Test]
    public void Create_WithNullAttributeScaling_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => DerivedStatFormula.Create(
            statName: "MaxHp",
            baseValue: 50,
            attributeScaling: null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.Create"/> creates a formula
    /// with empty optional dictionaries when nulls are passed.
    /// </summary>
    [Test]
    public void Create_WithNullOptionalDictionaries_DefaultsToEmpty()
    {
        // Arrange & Act
        var formula = DerivedStatFormula.Create(
            statName: "TestStat",
            baseValue: 10,
            attributeScaling: new Dictionary<CoreAttribute, float>());

        // Assert
        formula.ArchetypeBonuses.Should().BeEmpty();
        formula.LineageBonuses.Should().BeEmpty();
        formula.LineageMultipliers.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.HasArchetypeBonuses"/> returns
    /// true when archetype bonuses are defined.
    /// </summary>
    [Test]
    public void HasArchetypeBonuses_WithBonuses_ReturnsTrue()
    {
        // Arrange
        var formula = CreateMaxHpFormula();

        // Act & Assert
        formula.HasArchetypeBonuses.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.HasArchetypeBonuses"/> returns
    /// false when no archetype bonuses are defined.
    /// </summary>
    [Test]
    public void HasArchetypeBonuses_WithoutBonuses_ReturnsFalse()
    {
        // Arrange
        var formula = CreateCarryingCapacityFormula();

        // Act & Assert
        formula.HasArchetypeBonuses.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.HasLineageMultipliers"/> returns
    /// true for the Aether Pool formula with Rune-Marked multiplier.
    /// </summary>
    [Test]
    public void HasLineageMultipliers_WithMultiplier_ReturnsTrue()
    {
        // Arrange
        var formula = CreateMaxAetherPoolFormulaWithMultiplier();

        // Act & Assert
        formula.HasLineageMultipliers.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.ScaledAttributeCount"/> returns
    /// the correct count of scaling entries.
    /// </summary>
    [Test]
    public void ScaledAttributeCount_ReturnsCorrectCount()
    {
        // Arrange
        var formula = CreateMaxAetherPoolFormula();

        // Act & Assert — Will and Wits
        formula.ScaledAttributeCount.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.GetArchetypeBonus"/> returns
    /// the correct bonus for a known archetype.
    /// </summary>
    [Test]
    public void GetArchetypeBonus_KnownArchetype_ReturnsBonus()
    {
        // Arrange
        var formula = CreateMaxHpFormula();

        // Act & Assert
        formula.GetArchetypeBonus("warrior").Should().Be(49);
        formula.GetArchetypeBonus("mystic").Should().Be(20);
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.GetArchetypeBonus"/> returns
    /// 0 for an unknown archetype.
    /// </summary>
    [Test]
    public void GetArchetypeBonus_UnknownArchetype_ReturnsZero()
    {
        // Arrange
        var formula = CreateMaxHpFormula();

        // Act & Assert
        formula.GetArchetypeBonus("unknown").Should().Be(0);
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.GetLineageBonus"/> returns
    /// the correct bonus for a known lineage.
    /// </summary>
    [Test]
    public void GetLineageBonus_KnownLineage_ReturnsBonus()
    {
        // Arrange
        var formula = CreateSoakFormula();

        // Act & Assert
        formula.GetLineageBonus("iron-blooded").Should().Be(2);
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.GetLineageBonus"/> returns
    /// 0 for an unknown lineage.
    /// </summary>
    [Test]
    public void GetLineageBonus_UnknownLineage_ReturnsZero()
    {
        // Arrange
        var formula = CreateSoakFormula();

        // Act & Assert
        formula.GetLineageBonus("unknown").Should().Be(0);
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.GetLineageMultiplier"/> returns
    /// the correct multiplier for a known lineage.
    /// </summary>
    [Test]
    public void GetLineageMultiplier_KnownLineage_ReturnsMultiplier()
    {
        // Arrange
        var formula = CreateMaxAetherPoolFormulaWithMultiplier();

        // Act & Assert
        formula.GetLineageMultiplier("rune-marked").Should().BeApproximately(1.1f, 0.001f);
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.GetLineageMultiplier"/> returns
    /// 1.0 for an unknown lineage (no effect).
    /// </summary>
    [Test]
    public void GetLineageMultiplier_UnknownLineage_ReturnsOne()
    {
        // Arrange
        var formula = CreateMaxAetherPoolFormulaWithMultiplier();

        // Act & Assert
        formula.GetLineageMultiplier("unknown").Should().Be(1.0f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATE VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.Calculate"/> throws
    /// <see cref="ArgumentNullException"/> when attribute values are null.
    /// </summary>
    [Test]
    public void Calculate_WithNullAttributeValues_ThrowsArgumentNullException()
    {
        // Arrange
        var formula = CreateMaxHpFormula();

        // Act
        var act = () => formula.Calculate(null!, "warrior", null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.ToString"/> returns a
    /// formatted string showing the formula configuration.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var formula = CreateMaxHpFormula();

        // Act
        var result = formula.ToString();

        // Assert
        result.Should().Be("Formula [MaxHp]: base=50, scaling=1, archetypes=4, lineages=1");
    }

    /// <summary>
    /// Verifies that <see cref="DerivedStatFormula.ToString"/> handles formulas
    /// with no bonuses correctly.
    /// </summary>
    [Test]
    public void ToString_NoBonuses_ReturnsFormattedString()
    {
        // Arrange
        var formula = CreateCarryingCapacityFormula();

        // Act
        var result = formula.ToString();

        // Assert
        result.Should().Be("Formula [CarryingCapacity]: base=0, scaling=1, archetypes=0, lineages=0");
    }
}
