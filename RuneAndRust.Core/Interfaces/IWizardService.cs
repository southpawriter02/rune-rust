using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Provides stat preview calculations for the character creation wizard (v0.3.4b).
/// Computes hypothetical stats based on current selections and hovered options.
/// </summary>
public interface IWizardService
{
    /// <summary>
    /// Calculates preview stats for the current wizard context with optional hover previews.
    /// </summary>
    /// <param name="context">The current wizard context with confirmed selections.</param>
    /// <param name="previewLineage">Optional lineage being hovered over (not yet confirmed).</param>
    /// <param name="previewArchetype">Optional archetype being hovered over (not yet confirmed).</param>
    /// <returns>Dictionary mapping attributes to their calculated values.</returns>
    Dictionary<CharacterAttribute, int> GetPreviewStats(
        WizardContext context,
        LineageType? previewLineage = null,
        ArchetypeType? previewArchetype = null);

    /// <summary>
    /// Calculates derived stats (HP, Stamina, AP) from the given attributes.
    /// </summary>
    /// <param name="stats">The calculated attribute values.</param>
    /// <param name="archetype">The archetype (confirmed or previewed) for AP calculation.</param>
    /// <returns>A record containing MaxHP, MaxStamina, and ActionPoints.</returns>
    DerivedStats GetDerivedStats(Dictionary<CharacterAttribute, int> stats, ArchetypeType? archetype);

    /// <summary>
    /// Gets the display name for a lineage.
    /// </summary>
    string GetLineageDisplayName(LineageType lineage);

    /// <summary>
    /// Gets the description for a lineage.
    /// </summary>
    string GetLineageDescription(LineageType lineage);

    /// <summary>
    /// Gets the bonus summary for a lineage.
    /// </summary>
    string GetLineageBonusSummary(LineageType lineage);

    /// <summary>
    /// Gets the display name for an archetype.
    /// </summary>
    string GetArchetypeDisplayName(ArchetypeType archetype);

    /// <summary>
    /// Gets the description for an archetype.
    /// </summary>
    string GetArchetypeDescription(ArchetypeType archetype);

    /// <summary>
    /// Gets the bonus summary for an archetype.
    /// </summary>
    string GetArchetypeBonusSummary(ArchetypeType archetype);
}

/// <summary>
/// Represents calculated derived stats for preview display.
/// </summary>
/// <param name="MaxHP">Maximum hit points (50 + Sturdiness * 10).</param>
/// <param name="MaxStamina">Maximum stamina (20 + Finesse * 5 + Sturdiness * 3).</param>
/// <param name="ActionPoints">Action points per turn (2 + Wits / 4).</param>
public record DerivedStats(int MaxHP, int MaxStamina, int ActionPoints);
