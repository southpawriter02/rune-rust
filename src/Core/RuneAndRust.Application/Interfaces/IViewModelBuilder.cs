// ═══════════════════════════════════════════════════════════════════════════════
// IViewModelBuilder.cs
// Interface defining the contract for building CharacterCreationViewModel
// instances from CharacterCreationState. Coordinates with lineage, background,
// archetype, specialization, and derived stat providers to assemble complete
// presentation data for the character creation TUI.
// Version: 0.17.5b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Builds <see cref="CharacterCreationViewModel"/> from <see cref="CharacterCreationState"/>.
/// Coordinates with various providers to calculate preview data.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IViewModelBuilder"/> is the central service for transforming raw creation
/// state into display-ready presentation data. It assembles step information, navigation
/// state, selection display names, preview calculations (derived stats, abilities,
/// equipment), and validation status into an immutable ViewModel.
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong> The Controller calls <see cref="Build"/> after every
/// state change to obtain a fresh ViewModel for the TUI to render. During Step 3
/// (Attribute Allocation), the Controller may call <see cref="BuildDerivedStatsPreview"/>
/// independently for real-time stat feedback as the player adjusts attributes.
/// </para>
/// <para>
/// <strong>Provider Dependencies:</strong> Implementations coordinate with:
/// </para>
/// <list type="bullet">
///   <item><description><c>ILineageProvider</c> — lineage display names and HP/AP bonuses</description></item>
///   <item><description><c>IBackgroundProvider</c> — background display names and equipment grants</description></item>
///   <item><description><c>IArchetypeProvider</c> — archetype display names, resource bonuses, and starting abilities</description></item>
///   <item><description><c>ISpecializationProvider</c> — specialization display names and Tier 1 abilities</description></item>
///   <item><description><c>IDerivedStatCalculator</c> — derived stat calculation for HP/Stamina/AP preview</description></item>
/// </list>
/// <para>
/// <strong>Consumers:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>CharacterCreationController (v0.17.5d) — rebuilds ViewModel after each step change</description></item>
///   <item><description>TUI Screens (v0.17.5f) — renders from ViewModel data</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CharacterCreationViewModel"/>
/// <seealso cref="CharacterCreationState"/>
/// <seealso cref="DerivedStatsPreview"/>
public interface IViewModelBuilder
{
    /// <summary>
    /// Builds a complete ViewModel from the current creation state.
    /// </summary>
    /// <param name="state">
    /// The current <see cref="CharacterCreationState"/> containing all player selections,
    /// current step, and validation errors. Must not be null.
    /// </param>
    /// <returns>
    /// An immutable <see cref="CharacterCreationViewModel"/> ready for presentation,
    /// containing step display info, navigation state, selection names, preview data,
    /// and validation status.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="state"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is called after every state change (selection, navigation, validation).
    /// It constructs a fresh ViewModel each time — there is no caching. The returned
    /// ViewModel is immutable and safe to pass to the TUI for rendering.
    /// </para>
    /// <para>
    /// Preview data (derived stats, abilities, equipment) is populated based on
    /// which selections have been made:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Derived stats: present when attributes are allocated (Step 3+)</description></item>
    ///   <item><description>Abilities: present when archetype is selected (Step 4+)</description></item>
    ///   <item><description>Equipment: present when background is selected (Step 2+)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = CharacterCreationState.Create();
    /// state.SelectedLineage = Lineage.ClanBorn;
    /// state.FlexibleAttributeBonus = CoreAttribute.Might;
    ///
    /// var viewModel = builder.Build(state);
    /// // viewModel.StepTitle == "Choose Your Lineage"
    /// // viewModel.CanGoForward == true (lineage selected with bonus)
    /// // viewModel.SelectedLineageName == "Clan-Born"
    /// </code>
    /// </example>
    CharacterCreationViewModel Build(CharacterCreationState state);

    /// <summary>
    /// Builds only the derived stats preview for real-time updates.
    /// </summary>
    /// <param name="state">
    /// The current <see cref="CharacterCreationState"/> containing attribute allocation
    /// and optional lineage/archetype selections. Must not be null.
    /// </param>
    /// <returns>
    /// A <see cref="DerivedStatsPreview"/> reflecting the current allocation state,
    /// or <see cref="DerivedStatsPreview.Empty"/> if no attributes have been allocated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="state"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is optimized for the Step 3 (Attribute Allocation) screen where
    /// derived stats need to update in real-time as the player adjusts attributes.
    /// It calculates only the stats preview without rebuilding the full ViewModel.
    /// </para>
    /// <para>
    /// If lineage and/or archetype have been selected, their bonuses are included
    /// in the preview. Otherwise, only the attribute-based portion is calculated.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = CharacterCreationState.Create();
    /// state.Attributes = AttributeAllocationState.CreateFromRecommendedBuild(
    ///     "warrior", 4, 3, 2, 2, 4, 15);
    /// state.SelectedArchetype = Archetype.Warrior;
    ///
    /// var preview = builder.BuildDerivedStatsPreview(state);
    /// // preview.MaxHp == 139 (attribute-based + archetype bonus)
    /// </code>
    /// </example>
    DerivedStatsPreview BuildDerivedStatsPreview(CharacterCreationState state);
}
