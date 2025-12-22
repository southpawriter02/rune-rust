using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Provides stat preview calculations for the character creation wizard (v0.3.4b).
/// Uses StatCalculationService for lineage/archetype bonuses and derived stat formulas.
/// </summary>
public class WizardService : IWizardService
{
    private readonly IStatCalculationService _statService;
    private readonly ILogger<WizardService> _logger;

    /// <summary>
    /// Base value for all attributes before bonuses.
    /// </summary>
    public const int BaseAttributeValue = 5;

    private static readonly Dictionary<LineageType, (string Name, string Description, string Bonuses)> LineageInfo = new()
    {
        { LineageType.Human, ("Human", "Adaptable survivors who endure through sheer determination.", "+1 to all attributes") },
        { LineageType.RuneMarked, ("Rune-Marked", "Descendants bearing ancient runic inscriptions in their skin.", "+2 Wits, +2 Will, -1 Sturdiness") },
        { LineageType.IronBlooded, ("Iron-Blooded", "Those with machine-integrated bloodlines from the old world.", "+2 Sturdiness, +2 Might, -1 Wits") },
        { LineageType.VargrKin, ("Vargr-Kin", "Wolf-kin bearing the curse of the northern wastes.", "+2 Finesse, +2 Wits, -1 Will") }
    };

    private static readonly Dictionary<ArchetypeType, (string Name, string Description, string Bonuses)> ArchetypeInfo = new()
    {
        { ArchetypeType.Warrior, ("Warrior", "Frontline combatant specializing in durability and melee damage.", "+2 Sturdiness, +1 Might") },
        { ArchetypeType.Skirmisher, ("Skirmisher", "Cunning fighter favoring speed and precision over raw power.", "+2 Finesse, +1 Wits") },
        { ArchetypeType.Adept, ("Adept", "Runic practitioner channeling ancient power through inscribed formulas.", "+2 Wits, +1 Will") },
        { ArchetypeType.Mystic, ("Mystic", "Wielder of primal forces, drawing power from the world's corruption.", "+2 Will, +1 Sturdiness") }
    };

    public WizardService(
        IStatCalculationService statService,
        ILogger<WizardService> logger)
    {
        _statService = statService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Dictionary<CharacterAttribute, int> GetPreviewStats(
        WizardContext context,
        LineageType? previewLineage = null,
        ArchetypeType? previewArchetype = null)
    {
        _logger.LogTrace(
            "[Wizard] Preview requested - Context.Lineage: {ContextLineage}, PreviewLineage: {PreviewLineage}, PreviewArchetype: {PreviewArchetype}",
            context.Lineage, previewLineage, previewArchetype);

        // Start with base stats
        var stats = new Dictionary<CharacterAttribute, int>();
        foreach (var attr in Enum.GetValues<CharacterAttribute>())
        {
            stats[attr] = BaseAttributeValue;
        }

        // Apply confirmed lineage bonuses (if set in context)
        if (context.Lineage.HasValue)
        {
            ApplyBonuses(stats, _statService.GetLineageBonuses(context.Lineage.Value));
        }

        // Apply preview lineage bonuses (if hovering and lineage not yet confirmed)
        if (previewLineage.HasValue && !context.Lineage.HasValue)
        {
            ApplyBonuses(stats, _statService.GetLineageBonuses(previewLineage.Value));
        }

        // Apply confirmed archetype bonuses (if set in context)
        if (context.Archetype.HasValue)
        {
            ApplyBonuses(stats, _statService.GetArchetypeBonuses(context.Archetype.Value));
        }

        // Apply preview archetype bonuses (if hovering and archetype not yet confirmed)
        if (previewArchetype.HasValue && !context.Archetype.HasValue)
        {
            ApplyBonuses(stats, _statService.GetArchetypeBonuses(previewArchetype.Value));
        }

        // Clamp all values to valid range [1, 10]
        foreach (var attr in Enum.GetValues<CharacterAttribute>())
        {
            stats[attr] = _statService.ClampAttribute(stats[attr]);
        }

        _logger.LogTrace(
            "[Wizard] Preview stats: STU:{Sturdiness} MIG:{Might} WIT:{Wits} WIL:{Will} FIN:{Finesse}",
            stats[CharacterAttribute.Sturdiness],
            stats[CharacterAttribute.Might],
            stats[CharacterAttribute.Wits],
            stats[CharacterAttribute.Will],
            stats[CharacterAttribute.Finesse]);

        return stats;
    }

    /// <inheritdoc/>
    public DerivedStats GetDerivedStats(Dictionary<CharacterAttribute, int> stats, ArchetypeType? archetype)
    {
        var maxHP = _statService.CalculateMaxHP(stats[CharacterAttribute.Sturdiness]);
        var maxStamina = _statService.CalculateMaxStamina(
            stats[CharacterAttribute.Finesse],
            stats[CharacterAttribute.Sturdiness]);
        var actionPoints = _statService.CalculateActionPoints(stats[CharacterAttribute.Wits]);

        return new DerivedStats(maxHP, maxStamina, actionPoints);
    }

    /// <inheritdoc/>
    public string GetLineageDisplayName(LineageType lineage) =>
        LineageInfo.TryGetValue(lineage, out var info) ? info.Name : lineage.ToString();

    /// <inheritdoc/>
    public string GetLineageDescription(LineageType lineage) =>
        LineageInfo.TryGetValue(lineage, out var info) ? info.Description : string.Empty;

    /// <inheritdoc/>
    public string GetLineageBonusSummary(LineageType lineage) =>
        LineageInfo.TryGetValue(lineage, out var info) ? info.Bonuses : string.Empty;

    /// <inheritdoc/>
    public string GetArchetypeDisplayName(ArchetypeType archetype) =>
        ArchetypeInfo.TryGetValue(archetype, out var info) ? info.Name : archetype.ToString();

    /// <inheritdoc/>
    public string GetArchetypeDescription(ArchetypeType archetype) =>
        ArchetypeInfo.TryGetValue(archetype, out var info) ? info.Description : string.Empty;

    /// <inheritdoc/>
    public string GetArchetypeBonusSummary(ArchetypeType archetype) =>
        ArchetypeInfo.TryGetValue(archetype, out var info) ? info.Bonuses : string.Empty;

    private static void ApplyBonuses(Dictionary<CharacterAttribute, int> stats, Dictionary<CharacterAttribute, int> bonuses)
    {
        foreach (var (attribute, bonus) in bonuses)
        {
            stats[attribute] += bonus;
        }
    }
}
