// ═══════════════════════════════════════════════════════════════════════════════
// CharacterCreationStateTests.cs
// Unit tests for the CharacterCreationState entity, including creation defaults,
// forward/backward navigation, step completion validation, Clan-Born flexible
// bonus requirement, state reset, and IsComplete computation.
// Version: 0.17.5a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="CharacterCreationState"/> entity.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that:
/// </para>
/// <list type="bullet">
///   <item><description>Create() initializes all properties with correct defaults</description></item>
///   <item><description>SessionId is unique for each creation instance</description></item>
///   <item><description>TryAdvance validates step completion before advancing</description></item>
///   <item><description>TryAdvance returns false for incomplete steps</description></item>
///   <item><description>Clan-Born lineage requires FlexibleAttributeBonus to advance</description></item>
///   <item><description>TryGoBack returns false from Step 1 (Lineage)</description></item>
///   <item><description>TryGoBack preserves all existing selections</description></item>
///   <item><description>Reset clears all selections and returns to Step 1</description></item>
///   <item><description>IsComplete returns true only when all selections present</description></item>
///   <item><description>IsStepComplete validates per-step requirements correctly</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CharacterCreationState"/>
/// <seealso cref="CharacterCreationStep"/>
[TestFixture]
public class CharacterCreationStateTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create() initializes all properties with the correct default values:
    /// unique Id and SessionId, CurrentStep at Lineage, Simple allocation mode,
    /// all selections null, and IsComplete false.
    /// </summary>
    [Test]
    public void Create_InitializesWithCorrectDefaults()
    {
        // Act
        var state = CharacterCreationState.Create();

        // Assert
        state.Id.Should().NotBeEmpty();
        state.SessionId.Should().NotBeNullOrEmpty();
        state.SessionId.Should().HaveLength(32, because: "SessionId should be a GUID with no hyphens");
        state.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
        state.AttributeAllocationMode.Should().Be(AttributeAllocationMode.Simple);
        state.SelectedLineage.Should().BeNull();
        state.FlexibleAttributeBonus.Should().BeNull();
        state.SelectedBackground.Should().BeNull();
        state.Attributes.Should().BeNull();
        state.SelectedArchetype.Should().BeNull();
        state.SelectedSpecialization.Should().BeNull();
        state.CharacterName.Should().BeNull();
        state.IsComplete.Should().BeFalse();
        state.ValidationErrors.Should().BeEmpty();
        state.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        state.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Verifies that each call to Create() generates a unique SessionId,
    /// ensuring creation sessions can be individually tracked.
    /// </summary>
    [Test]
    public void Create_GeneratesUniqueSessionIds()
    {
        // Act
        var state1 = CharacterCreationState.Create();
        var state2 = CharacterCreationState.Create();

        // Assert
        state1.SessionId.Should().NotBe(state2.SessionId);
        state1.Id.Should().NotBe(state2.Id);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRY ADVANCE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that TryAdvance returns false when the current step (Lineage)
    /// has no selection, leaving the state unchanged.
    /// </summary>
    [Test]
    public void TryAdvance_FromLineage_WhenLineageNotSelected_ReturnsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var result = state.TryAdvance();

        // Assert
        result.Should().BeFalse();
        state.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
    }

    /// <summary>
    /// Verifies that TryAdvance succeeds and advances to Background when a
    /// non-Clan-Born lineage is selected (no flexible bonus required).
    /// </summary>
    [Test]
    public void TryAdvance_FromLineage_WhenLineageSelected_ReturnsTrue()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.IronBlooded;

        // Act
        var result = state.TryAdvance();

        // Assert
        result.Should().BeTrue();
        state.CurrentStep.Should().Be(CharacterCreationStep.Background);
    }

    /// <summary>
    /// Verifies that TryAdvance returns false when Clan-Born is selected
    /// without a FlexibleAttributeBonus, as Clan-Born requires the flexible
    /// +1 bonus selection before advancing.
    /// </summary>
    [Test]
    public void TryAdvance_FromLineage_WhenClanBornWithoutFlexibleBonus_ReturnsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = null;

        // Act
        var result = state.TryAdvance();

        // Assert
        result.Should().BeFalse();
        state.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
    }

    /// <summary>
    /// Verifies that TryAdvance succeeds when Clan-Born is selected with
    /// a valid FlexibleAttributeBonus.
    /// </summary>
    [Test]
    public void TryAdvance_FromLineage_WhenClanBornWithFlexibleBonus_ReturnsTrue()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.ClanBorn;
        state.FlexibleAttributeBonus = CoreAttribute.Might;

        // Act
        var result = state.TryAdvance();

        // Assert
        result.Should().BeTrue();
        state.CurrentStep.Should().Be(CharacterCreationStep.Background);
    }

    /// <summary>
    /// Verifies that TryAdvance returns false from Summary, as there is no
    /// step after the final confirmation screen.
    /// </summary>
    [Test]
    public void TryAdvance_FromSummary_ReturnsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CurrentStep = CharacterCreationStep.Summary;
        state.CharacterName = "Sigurd";

        // Act
        var result = state.TryAdvance();

        // Assert
        result.Should().BeFalse();
        state.CurrentStep.Should().Be(CharacterCreationStep.Summary);
    }

    /// <summary>
    /// Verifies that TryAdvance updates LastModifiedAt on successful advancement.
    /// </summary>
    [Test]
    public void TryAdvance_OnSuccess_UpdatesLastModifiedAt()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.VargrKin;
        var originalTimestamp = state.LastModifiedAt;

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        state.TryAdvance();

        // Assert
        state.LastModifiedAt.Should().BeAfter(originalTimestamp);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRY GO BACK TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that TryGoBack returns false from Step 1 (Lineage),
    /// as there is no preceding step.
    /// </summary>
    [Test]
    public void TryGoBack_FromLineage_ReturnsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act
        var result = state.TryGoBack();

        // Assert
        result.Should().BeFalse();
        state.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
    }

    /// <summary>
    /// Verifies that TryGoBack returns true from Background and navigates
    /// to Lineage, while preserving all existing selections.
    /// </summary>
    [Test]
    public void TryGoBack_FromBackground_ReturnsTrue_PreservesSelections()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.VargrKin;
        state.CurrentStep = CharacterCreationStep.Background;
        state.SelectedBackground = Background.VillageSmith;

        // Act
        var result = state.TryGoBack();

        // Assert
        result.Should().BeTrue();
        state.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
        state.SelectedLineage.Should().Be(Lineage.VargrKin);
        state.SelectedBackground.Should().Be(Background.VillageSmith);
    }

    /// <summary>
    /// Verifies that TryGoBack from a later step preserves all selections
    /// made across multiple steps.
    /// </summary>
    [Test]
    public void TryGoBack_FromSpecialization_PreservesAllEarlierSelections()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.RuneMarked;
        state.SelectedBackground = Background.WanderingSkald;
        state.SelectedArchetype = Archetype.Mystic;
        state.CurrentStep = CharacterCreationStep.Specialization;
        state.SelectedSpecialization = SpecializationId.Seidkona;

        // Act
        var result = state.TryGoBack();

        // Assert
        result.Should().BeTrue();
        state.CurrentStep.Should().Be(CharacterCreationStep.Archetype);
        state.SelectedLineage.Should().Be(Lineage.RuneMarked);
        state.SelectedBackground.Should().Be(Background.WanderingSkald);
        state.SelectedArchetype.Should().Be(Archetype.Mystic);
        state.SelectedSpecialization.Should().Be(SpecializationId.Seidkona);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RESET TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Reset clears all selections and returns to Step 1 (Lineage),
    /// while preserving the session identity (Id, SessionId, CreatedAt).
    /// </summary>
    [Test]
    public void Reset_ClearsAllSelectionsAndReturnsToLineage()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        var originalSessionId = state.SessionId;
        var originalId = state.Id;
        var originalCreatedAt = state.CreatedAt;

        state.SelectedLineage = Lineage.RuneMarked;
        state.SelectedBackground = Background.WanderingSkald;
        state.SelectedArchetype = Archetype.Mystic;
        state.SelectedSpecialization = SpecializationId.Seidkona;
        state.CharacterName = "Astrid";
        state.CurrentStep = CharacterCreationStep.Summary;
        state.AttributeAllocationMode = AttributeAllocationMode.Advanced;

        // Act
        state.Reset();

        // Assert — Selections cleared
        state.CurrentStep.Should().Be(CharacterCreationStep.Lineage);
        state.SelectedLineage.Should().BeNull();
        state.FlexibleAttributeBonus.Should().BeNull();
        state.SelectedBackground.Should().BeNull();
        state.Attributes.Should().BeNull();
        state.SelectedArchetype.Should().BeNull();
        state.SelectedSpecialization.Should().BeNull();
        state.CharacterName.Should().BeNull();
        state.AttributeAllocationMode.Should().Be(AttributeAllocationMode.Simple);
        state.ValidationErrors.Should().BeEmpty();

        // Assert — Identity preserved
        state.Id.Should().Be(originalId);
        state.SessionId.Should().Be(originalSessionId);
        state.CreatedAt.Should().Be(originalCreatedAt);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IS COMPLETE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsComplete returns false when not all selections are present.
    /// In this case, only Lineage and Background are selected.
    /// </summary>
    [Test]
    public void IsComplete_ReturnsFalse_WhenAnySelectionMissing()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.IronBlooded;
        state.SelectedBackground = Background.VillageSmith;
        // Missing: Attributes, Archetype, Specialization, Name

        // Act & Assert
        state.IsComplete.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsComplete returns true when all six steps have valid
    /// selections and there are no validation errors.
    /// </summary>
    [Test]
    public void IsComplete_ReturnsTrue_WhenAllSelectionsPresent()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.IronBlooded;
        state.SelectedBackground = Background.VillageSmith;
        state.Attributes = AttributeAllocationState.CreateFromRecommendedBuild(
            "Warrior", 4, 3, 2, 2, 4, 15);
        state.SelectedArchetype = Archetype.Warrior;
        state.SelectedSpecialization = SpecializationId.Skjaldmaer;
        state.CharacterName = "Sigurd";

        // Act & Assert
        state.IsComplete.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsComplete returns false when validation errors are present,
    /// even if all selections are provided.
    /// </summary>
    [Test]
    public void IsComplete_ReturnsFalse_WhenValidationErrorsPresent()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.IronBlooded;
        state.SelectedBackground = Background.VillageSmith;
        state.Attributes = AttributeAllocationState.CreateFromRecommendedBuild(
            "Warrior", 4, 3, 2, 2, 4, 15);
        state.SelectedArchetype = Archetype.Warrior;
        state.SelectedSpecialization = SpecializationId.Skjaldmaer;
        state.CharacterName = "Sigurd";
        state.ValidationErrors.Add("Some validation error");

        // Act & Assert
        state.IsComplete.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsComplete returns false when the character name is
    /// whitespace-only, as names require non-whitespace content.
    /// </summary>
    [Test]
    public void IsComplete_ReturnsFalse_WhenCharacterNameIsWhitespace()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.IronBlooded;
        state.SelectedBackground = Background.VillageSmith;
        state.Attributes = AttributeAllocationState.CreateFromRecommendedBuild(
            "Warrior", 4, 3, 2, 2, 4, 15);
        state.SelectedArchetype = Archetype.Warrior;
        state.SelectedSpecialization = SpecializationId.Skjaldmaer;
        state.CharacterName = "   ";

        // Act & Assert
        state.IsComplete.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IS STEP COMPLETE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsStepComplete for Lineage returns true for non-Clan-Born
    /// lineages that don't require a flexible bonus.
    /// </summary>
    [Test]
    public void IsStepComplete_Lineage_NonClanBorn_ReturnsTrue()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.IronBlooded;

        // Act & Assert
        state.IsStepComplete(CharacterCreationStep.Lineage).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsStepComplete for Lineage returns false when no lineage
    /// is selected.
    /// </summary>
    [Test]
    public void IsStepComplete_Lineage_NoSelection_ReturnsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act & Assert
        state.IsStepComplete(CharacterCreationStep.Lineage).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsStepComplete for Background returns true when a
    /// background is selected.
    /// </summary>
    [Test]
    public void IsStepComplete_Background_WithSelection_ReturnsTrue()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedBackground = Background.RuinDelver;

        // Act & Assert
        state.IsStepComplete(CharacterCreationStep.Background).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsStepComplete for Attributes returns true when attributes
    /// are fully allocated (IsComplete on the allocation state).
    /// </summary>
    [Test]
    public void IsStepComplete_Attributes_WithCompleteAllocation_ReturnsTrue()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.Attributes = AttributeAllocationState.CreateFromRecommendedBuild(
            "Warrior", 4, 3, 2, 2, 4, 15);

        // Act & Assert
        state.IsStepComplete(CharacterCreationStep.Attributes).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsStepComplete for Attributes returns false when the
    /// allocation state has not been set.
    /// </summary>
    [Test]
    public void IsStepComplete_Attributes_Null_ReturnsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act & Assert
        state.IsStepComplete(CharacterCreationStep.Attributes).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsStepComplete for Summary returns true when a
    /// non-empty character name is entered.
    /// </summary>
    [Test]
    public void IsStepComplete_Summary_WithName_ReturnsTrue()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.CharacterName = "Sigurd";

        // Act & Assert
        state.IsStepComplete(CharacterCreationStep.Summary).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsStepComplete for Summary returns false when no
    /// character name is entered.
    /// </summary>
    [Test]
    public void IsStepComplete_Summary_WithoutName_ReturnsFalse()
    {
        // Arrange
        var state = CharacterCreationState.Create();

        // Act & Assert
        state.IsStepComplete(CharacterCreationStep.Summary).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString returns a formatted string containing the session
    /// ID, current step, and key state information.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedStateString()
    {
        // Arrange
        var state = CharacterCreationState.Create();
        state.SelectedLineage = Lineage.VargrKin;

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("CharacterCreationState");
        result.Should().Contain(state.SessionId);
        result.Should().Contain("Lineage");
        result.Should().Contain("VargrKin");
    }
}
