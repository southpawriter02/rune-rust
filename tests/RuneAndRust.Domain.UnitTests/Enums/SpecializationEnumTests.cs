// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationEnumTests.cs
// Unit tests for the SpecializationPathType and SpecializationId enums,
// including path type classification, parent archetype mapping, display names
// with Norse diacritics, Corruption risk descriptions, and extension methods.
// Version: 0.17.4a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="SpecializationPathType"/> and
/// <see cref="SpecializationId"/> enums, including their extension methods.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that:
/// </para>
/// <list type="bullet">
///   <item><description>SpecializationPathType has exactly 2 values (Coherent, Heretical)</description></item>
///   <item><description>SpecializationId has exactly 17 values across 4 archetypes</description></item>
///   <item><description>Path type classification: 5 Heretical, 12 Coherent</description></item>
///   <item><description>Parent archetype mappings: Warrior (6), Skirmisher (4), Mystic (2), Adept (5)</description></item>
///   <item><description>Display names include Old Norse diacritics (ð, ö, ú)</description></item>
///   <item><description>Corruption risk descriptions for Heretical specializations</description></item>
///   <item><description>Extension methods for path type warnings and descriptions</description></item>
/// </list>
/// </remarks>
/// <seealso cref="SpecializationPathType"/>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="SpecializationPathTypeExtensions"/>
/// <seealso cref="SpecializationIdExtensions"/>
[TestFixture]
public class SpecializationEnumTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SPECIALIZATION PATH TYPE ENUM TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the SpecializationPathType enum has exactly two values,
    /// one for each path classification in the Specialization system.
    /// </summary>
    [Test]
    public void SpecializationPathType_HasTwoValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<SpecializationPathType>();

        // Assert
        values.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that the SpecializationPathType enum contains both expected
    /// values: Coherent and Heretical.
    /// </summary>
    [Test]
    public void SpecializationPathType_ContainsAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<SpecializationPathType>();

        // Assert
        values.Should().Contain(SpecializationPathType.Coherent);
        values.Should().Contain(SpecializationPathType.Heretical);
    }

    /// <summary>
    /// Verifies that each SpecializationPathType enum value has the correct
    /// explicit integer assignment for stable serialization and database storage.
    /// </summary>
    /// <param name="pathType">The SpecializationPathType enum value to verify.</param>
    /// <param name="expected">The expected integer value.</param>
    /// <remarks>
    /// Coherent (0) works within stable reality.
    /// Heretical (1) interfaces with corrupted Aether.
    /// </remarks>
    [Test]
    [TestCase(SpecializationPathType.Coherent, 0)]
    [TestCase(SpecializationPathType.Heretical, 1)]
    public void SpecializationPathType_HasCorrectIntegerValues(
        SpecializationPathType pathType,
        int expected)
    {
        // Assert
        ((int)pathType).Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SPECIALIZATION ID ENUM TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the SpecializationId enum has exactly 17 values,
    /// covering all specializations across all four archetypes.
    /// </summary>
    /// <remarks>
    /// 17 total: Warrior (6) + Skirmisher (4) + Mystic (2) + Adept (5).
    /// </remarks>
    [Test]
    public void SpecializationId_Has17Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<SpecializationId>();

        // Assert
        values.Should().HaveCount(17);
    }

    /// <summary>
    /// Verifies that all 17 expected specialization values are defined
    /// in the SpecializationId enum.
    /// </summary>
    [Test]
    public void SpecializationId_ContainsAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            // Warrior (6)
            SpecializationId.Berserkr,
            SpecializationId.IronBane,
            SpecializationId.Skjaldmaer,
            SpecializationId.SkarHorde,
            SpecializationId.AtgeirWielder,
            SpecializationId.GorgeMaw,
            // Skirmisher (4)
            SpecializationId.Veidimadr,
            SpecializationId.MyrkGengr,
            SpecializationId.Strandhogg,
            SpecializationId.HlekkrMaster,
            // Mystic (2)
            SpecializationId.Seidkona,
            SpecializationId.EchoCaller,
            // Adept (5)
            SpecializationId.BoneSetter,
            SpecializationId.JotunReader,
            SpecializationId.Skald,
            SpecializationId.ScrapTinker,
            SpecializationId.Einbui
        };

        // Act
        var actualValues = Enum.GetValues<SpecializationId>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
    }

    /// <summary>
    /// Verifies that each SpecializationId enum value has the correct explicit
    /// integer assignment for stable serialization and database storage.
    /// </summary>
    /// <param name="specializationId">The SpecializationId enum value to verify.</param>
    /// <param name="expected">The expected integer value.</param>
    /// <remarks>
    /// Values are grouped by archetype: Warrior (0-5), Skirmisher (6-9),
    /// Mystic (10-11), Adept (12-16).
    /// </remarks>
    [Test]
    [TestCase(SpecializationId.Berserkr, 0)]
    [TestCase(SpecializationId.IronBane, 1)]
    [TestCase(SpecializationId.Skjaldmaer, 2)]
    [TestCase(SpecializationId.SkarHorde, 3)]
    [TestCase(SpecializationId.AtgeirWielder, 4)]
    [TestCase(SpecializationId.GorgeMaw, 5)]
    [TestCase(SpecializationId.Veidimadr, 6)]
    [TestCase(SpecializationId.MyrkGengr, 7)]
    [TestCase(SpecializationId.Strandhogg, 8)]
    [TestCase(SpecializationId.HlekkrMaster, 9)]
    [TestCase(SpecializationId.Seidkona, 10)]
    [TestCase(SpecializationId.EchoCaller, 11)]
    [TestCase(SpecializationId.BoneSetter, 12)]
    [TestCase(SpecializationId.JotunReader, 13)]
    [TestCase(SpecializationId.Skald, 14)]
    [TestCase(SpecializationId.ScrapTinker, 15)]
    [TestCase(SpecializationId.Einbui, 16)]
    public void SpecializationId_HasCorrectIntegerValues(
        SpecializationId specializationId,
        int expected)
    {
        // Assert
        ((int)specializationId).Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PATH TYPE CLASSIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that exactly 5 specializations are classified as Heretical.
    /// </summary>
    /// <remarks>
    /// Heretical specializations: Berserkr, GorgeMaw, MyrkGengr, Seidkona, EchoCaller.
    /// </remarks>
    [Test]
    public void SpecializationId_HereticalCount_IsFive()
    {
        // Arrange & Act
        var allSpecs = Enum.GetValues<SpecializationId>();
        var hereticalCount = allSpecs.Count(s => s.IsHeretical());

        // Assert
        hereticalCount.Should().Be(5);
    }

    /// <summary>
    /// Verifies that the 5 specific Heretical specializations are correctly
    /// classified by the GetPathType extension method.
    /// </summary>
    [Test]
    public void SpecializationId_SpecificHereticalSpecializations_AreHeretical()
    {
        // Assert
        SpecializationId.Berserkr.IsHeretical().Should().BeTrue();
        SpecializationId.GorgeMaw.IsHeretical().Should().BeTrue();
        SpecializationId.MyrkGengr.IsHeretical().Should().BeTrue();
        SpecializationId.Seidkona.IsHeretical().Should().BeTrue();
        SpecializationId.EchoCaller.IsHeretical().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that representative Coherent specializations are correctly
    /// classified as non-Heretical.
    /// </summary>
    [Test]
    public void SpecializationId_CoherentExamples_AreNotHeretical()
    {
        // Assert
        SpecializationId.Skjaldmaer.IsHeretical().Should().BeFalse();
        SpecializationId.BoneSetter.IsHeretical().Should().BeFalse();
        SpecializationId.Skald.IsHeretical().Should().BeFalse();
        SpecializationId.Veidimadr.IsHeretical().Should().BeFalse();
        SpecializationId.Strandhogg.IsHeretical().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetPathType returns the correct path type for each
    /// of the 17 specializations.
    /// </summary>
    /// <param name="specializationId">The specialization to check.</param>
    /// <param name="expectedPathType">The expected path type.</param>
    [Test]
    [TestCase(SpecializationId.Berserkr, SpecializationPathType.Heretical)]
    [TestCase(SpecializationId.IronBane, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.Skjaldmaer, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.SkarHorde, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.AtgeirWielder, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.GorgeMaw, SpecializationPathType.Heretical)]
    [TestCase(SpecializationId.Veidimadr, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.MyrkGengr, SpecializationPathType.Heretical)]
    [TestCase(SpecializationId.Strandhogg, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.HlekkrMaster, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.Seidkona, SpecializationPathType.Heretical)]
    [TestCase(SpecializationId.EchoCaller, SpecializationPathType.Heretical)]
    [TestCase(SpecializationId.BoneSetter, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.JotunReader, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.Skald, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.ScrapTinker, SpecializationPathType.Coherent)]
    [TestCase(SpecializationId.Einbui, SpecializationPathType.Coherent)]
    public void GetPathType_ReturnsCorrectType_ForAllSpecializations(
        SpecializationId specializationId,
        SpecializationPathType expectedPathType)
    {
        // Act
        var result = specializationId.GetPathType();

        // Assert
        result.Should().Be(expectedPathType);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PARENT ARCHETYPE MAPPING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetParentArchetype returns the correct archetype for
    /// each of the 17 specializations.
    /// </summary>
    /// <param name="specializationId">The specialization to check.</param>
    /// <param name="expectedArchetype">The expected parent archetype.</param>
    [Test]
    [TestCase(SpecializationId.Berserkr, Archetype.Warrior)]
    [TestCase(SpecializationId.IronBane, Archetype.Warrior)]
    [TestCase(SpecializationId.Skjaldmaer, Archetype.Warrior)]
    [TestCase(SpecializationId.SkarHorde, Archetype.Warrior)]
    [TestCase(SpecializationId.AtgeirWielder, Archetype.Warrior)]
    [TestCase(SpecializationId.GorgeMaw, Archetype.Warrior)]
    [TestCase(SpecializationId.Veidimadr, Archetype.Skirmisher)]
    [TestCase(SpecializationId.MyrkGengr, Archetype.Skirmisher)]
    [TestCase(SpecializationId.Strandhogg, Archetype.Skirmisher)]
    [TestCase(SpecializationId.HlekkrMaster, Archetype.Skirmisher)]
    [TestCase(SpecializationId.Seidkona, Archetype.Mystic)]
    [TestCase(SpecializationId.EchoCaller, Archetype.Mystic)]
    [TestCase(SpecializationId.BoneSetter, Archetype.Adept)]
    [TestCase(SpecializationId.JotunReader, Archetype.Adept)]
    [TestCase(SpecializationId.Skald, Archetype.Adept)]
    [TestCase(SpecializationId.ScrapTinker, Archetype.Adept)]
    [TestCase(SpecializationId.Einbui, Archetype.Adept)]
    public void GetParentArchetype_ReturnsCorrectArchetype_ForAllSpecializations(
        SpecializationId specializationId,
        Archetype expectedArchetype)
    {
        // Act
        var result = specializationId.GetParentArchetype();

        // Assert
        result.Should().Be(expectedArchetype);
    }

    /// <summary>
    /// Verifies that Warrior has exactly 6 specializations.
    /// </summary>
    [Test]
    public void GetParentArchetype_WarriorHasSixSpecializations()
    {
        // Arrange & Act
        var allSpecs = Enum.GetValues<SpecializationId>();
        var warriorCount = allSpecs.Count(s => s.GetParentArchetype() == Archetype.Warrior);

        // Assert
        warriorCount.Should().Be(6);
    }

    /// <summary>
    /// Verifies that Skirmisher has exactly 4 specializations.
    /// </summary>
    [Test]
    public void GetParentArchetype_SkirmisherHasFourSpecializations()
    {
        // Arrange & Act
        var allSpecs = Enum.GetValues<SpecializationId>();
        var skirmisherCount = allSpecs.Count(s => s.GetParentArchetype() == Archetype.Skirmisher);

        // Assert
        skirmisherCount.Should().Be(4);
    }

    /// <summary>
    /// Verifies that Mystic has exactly 2 specializations.
    /// </summary>
    [Test]
    public void GetParentArchetype_MysticHasTwoSpecializations()
    {
        // Arrange & Act
        var allSpecs = Enum.GetValues<SpecializationId>();
        var mysticCount = allSpecs.Count(s => s.GetParentArchetype() == Archetype.Mystic);

        // Assert
        mysticCount.Should().Be(2);
    }

    /// <summary>
    /// Verifies that Adept has exactly 5 specializations.
    /// </summary>
    [Test]
    public void GetParentArchetype_AdeptHasFiveSpecializations()
    {
        // Arrange & Act
        var allSpecs = Enum.GetValues<SpecializationId>();
        var adeptCount = allSpecs.Count(s => s.GetParentArchetype() == Archetype.Adept);

        // Assert
        adeptCount.Should().Be(5);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IS HERETICAL EXTENSION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsHeretical returns true for all Heretical specializations.
    /// </summary>
    /// <param name="specializationId">A Heretical specialization to check.</param>
    [Test]
    [TestCase(SpecializationId.Berserkr)]
    [TestCase(SpecializationId.GorgeMaw)]
    [TestCase(SpecializationId.MyrkGengr)]
    [TestCase(SpecializationId.Seidkona)]
    [TestCase(SpecializationId.EchoCaller)]
    public void IsHeretical_ReturnsTrue_ForHereticalSpecs(
        SpecializationId specializationId)
    {
        // Act & Assert
        specializationId.IsHeretical().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsHeretical returns false for representative Coherent
    /// specializations from each archetype.
    /// </summary>
    /// <param name="specializationId">A Coherent specialization to check.</param>
    [Test]
    [TestCase(SpecializationId.IronBane)]
    [TestCase(SpecializationId.Skjaldmaer)]
    [TestCase(SpecializationId.Veidimadr)]
    [TestCase(SpecializationId.BoneSetter)]
    [TestCase(SpecializationId.Skald)]
    public void IsHeretical_ReturnsFalse_ForCoherentSpecs(
        SpecializationId specializationId)
    {
        // Act & Assert
        specializationId.IsHeretical().Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY NAME TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetDisplayName returns the correct formatted name
    /// for each specialization, including Old Norse diacritics.
    /// </summary>
    /// <param name="specializationId">The specialization to check.</param>
    /// <param name="expectedName">The expected display name.</param>
    [Test]
    [TestCase(SpecializationId.Berserkr, "Berserkr")]
    [TestCase(SpecializationId.IronBane, "Iron-Bane")]
    [TestCase(SpecializationId.Skjaldmaer, "Skjaldmaer")]
    [TestCase(SpecializationId.SkarHorde, "Skar-Horde")]
    [TestCase(SpecializationId.AtgeirWielder, "Atgeir-Wielder")]
    [TestCase(SpecializationId.GorgeMaw, "Gorge-Maw")]
    [TestCase(SpecializationId.Veidimadr, "Veiðimaðr")]
    [TestCase(SpecializationId.MyrkGengr, "Myrk-gengr")]
    [TestCase(SpecializationId.Strandhogg, "Strandhögg")]
    [TestCase(SpecializationId.HlekkrMaster, "Hlekkr-master")]
    [TestCase(SpecializationId.Seidkona, "Seiðkona")]
    [TestCase(SpecializationId.EchoCaller, "Echo-Caller")]
    [TestCase(SpecializationId.BoneSetter, "Bone-Setter")]
    [TestCase(SpecializationId.JotunReader, "Jötun-Reader")]
    [TestCase(SpecializationId.Skald, "Skald")]
    [TestCase(SpecializationId.ScrapTinker, "Scrap-Tinker")]
    [TestCase(SpecializationId.Einbui, "Einbúi")]
    public void GetDisplayName_ReturnsFormattedName_ForAllSpecs(
        SpecializationId specializationId,
        string expectedName)
    {
        // Act
        var result = specializationId.GetDisplayName();

        // Assert
        result.Should().Be(expectedName);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CORRUPTION RISK TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetCorruptionRisk returns a non-null description for
    /// all Heretical specializations.
    /// </summary>
    /// <param name="specializationId">A Heretical specialization to check.</param>
    /// <param name="expectedRisk">The expected Corruption risk description.</param>
    [Test]
    [TestCase(SpecializationId.Berserkr, "Rage abilities may trigger Corruption gain")]
    [TestCase(SpecializationId.GorgeMaw, "Consuming enemies risks Corruption")]
    [TestCase(SpecializationId.MyrkGengr, "Shadow manipulation risks Corruption")]
    [TestCase(SpecializationId.Seidkona, "Aether spellcasting risks Corruption")]
    [TestCase(SpecializationId.EchoCaller, "Communing with the dead risks Corruption")]
    public void GetCorruptionRisk_ReturnsDescription_ForHereticalSpecs(
        SpecializationId specializationId,
        string expectedRisk)
    {
        // Act
        var result = specializationId.GetCorruptionRisk();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedRisk);
    }

    /// <summary>
    /// Verifies that GetCorruptionRisk returns null for representative
    /// Coherent specializations that carry no Corruption risk.
    /// </summary>
    /// <param name="specializationId">A Coherent specialization to check.</param>
    [Test]
    [TestCase(SpecializationId.IronBane)]
    [TestCase(SpecializationId.Skjaldmaer)]
    [TestCase(SpecializationId.Veidimadr)]
    [TestCase(SpecializationId.BoneSetter)]
    [TestCase(SpecializationId.Skald)]
    public void GetCorruptionRisk_ReturnsNull_ForCoherentSpecs(
        SpecializationId specializationId)
    {
        // Act
        var result = specializationId.GetCorruptionRisk();

        // Assert
        result.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PATH TYPE EXTENSION METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RisksCorruption returns true for Heretical path type.
    /// </summary>
    [Test]
    public void RisksCorruption_Heretical_ReturnsTrue()
    {
        // Act & Assert
        SpecializationPathType.Heretical.RisksCorruption().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that RisksCorruption returns false for Coherent path type.
    /// </summary>
    [Test]
    public void RisksCorruption_Coherent_ReturnsFalse()
    {
        // Act & Assert
        SpecializationPathType.Coherent.RisksCorruption().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetCreationWarning returns a warning message for
    /// Heretical path type containing key terms about corrupted Aether
    /// and Corruption gain.
    /// </summary>
    [Test]
    public void GetCreationWarning_Heretical_ReturnsWarningText()
    {
        // Act
        var result = SpecializationPathType.Heretical.GetCreationWarning();

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("corrupted Aether");
        result.Should().Contain("Corruption gain");
    }

    /// <summary>
    /// Verifies that GetCreationWarning returns null for Coherent path type,
    /// indicating no warning is needed.
    /// </summary>
    [Test]
    public void GetCreationWarning_Coherent_ReturnsNull()
    {
        // Act
        var result = SpecializationPathType.Coherent.GetCreationWarning();

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetDescription returns the correct human-readable
    /// description for each path type.
    /// </summary>
    /// <param name="pathType">The path type to describe.</param>
    /// <param name="expectedDescription">The expected description text.</param>
    [Test]
    [TestCase(SpecializationPathType.Coherent, "Works within stable reality")]
    [TestCase(SpecializationPathType.Heretical, "Interfaces with corrupted Aether")]
    public void GetDescription_ReturnsCorrectText_ForBothPathTypes(
        SpecializationPathType pathType,
        string expectedDescription)
    {
        // Act
        var result = pathType.GetDescription();

        // Assert
        result.Should().Be(expectedDescription);
    }
}
