using RuneAndRust.Core.Enums;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Static capture template definitions for Data Capture generation.
/// All content follows AAM-VOICE Layer 2 (Diagnostic) constraints.
/// Domain 4 compliant: No precision measurements or modern terminology.
/// </summary>
public static class CaptureTemplates
{
    #region Rusted Servitor Templates

    /// <summary>
    /// Capture templates for Rusted Servitor encounters.
    /// Matched by keywords: "servitor", "automaton", "machine".
    /// </summary>
    public static readonly CaptureTemplate[] RustedServitor = new[]
    {
        new CaptureTemplate(
            CaptureType.Specimen,
            "The servo-motor shows signs of organic fungal infiltration. " +
            "Mycelial threads have woven through the mechanical joints, " +
            "creating an unsettling fusion of rust and growth.",
            "Servitor examination",
            new[] { "servitor", "automaton", "machine", "mechanical" }),

        new CaptureTemplate(
            CaptureType.TextFragment,
            "Recovered maintenance logs, partially corrupted: " +
            "'...Unit 7-Alpha reporting nominal function. Awaiting directives from Central. " +
            "Note: Unusual signal interference detected in lower sectors...'",
            "Recovered data-slate",
            new[] { "servitor", "automaton", "aesir" }),

        new CaptureTemplate(
            CaptureType.VisualRecord,
            "Crystalline growths cluster around the primary actuator joints. " +
            "The formations pulse with a faint amber luminescence when the unit " +
            "attempts movement, suggesting some form of energy harvesting.",
            "Visual inspection",
            new[] { "servitor", "automaton", "crystal" }),

        new CaptureTemplate(
            CaptureType.RunicTrace,
            "Faint Aetheric resonance detected in the power coupling. " +
            "The signature matches pre-Glitch Aesir manufacturing standards, " +
            "though degradation patterns suggest centuries of dormancy.",
            "Runic analysis",
            new[] { "servitor", "aesir", "rune", "aetheric" })
    };

    #endregion

    #region Generic Container Templates

    /// <summary>
    /// Generic templates for container searches.
    /// Used when no specific object templates match.
    /// </summary>
    public static readonly CaptureTemplate[] GenericContainer = new[]
    {
        new CaptureTemplate(
            CaptureType.TextFragment,
            "A crumpled note with faded writing. Most is illegible, but " +
            "a fragment remains: '...the eastern tunnels are blocked. " +
            "Whatever happened there, the Clans have sealed it permanently.'",
            "Found in container",
            new[] { "container", "chest", "crate", "box" }),

        new CaptureTemplate(
            CaptureType.OralHistory,
            "This container bears clan-marks matching stories told in the taverns: " +
            "'Old Tormund's cache,' they called it. The Scavenger who never returned " +
            "from the deep ruins. Finding this explains his fate.",
            "Local knowledge",
            new[] { "container", "cache", "storage" }),

        new CaptureTemplate(
            CaptureType.TextFragment,
            "Inventory records scratched into the container's lid. " +
            "Most items have been crossed out, but notes remain: " +
            "'Claimed by the Blight-touched. Avoid sector after nightfall.'",
            "Container markings",
            new[] { "container", "crate", "supply" })
    };

    #endregion

    #region Blighted Creature Templates

    /// <summary>
    /// Templates for Blight-corrupted creature encounters.
    /// Matched by keywords: "blight", "corrupted", "infected".
    /// </summary>
    public static readonly CaptureTemplate[] BlightedCreature = new[]
    {
        new CaptureTemplate(
            CaptureType.Specimen,
            "Tissue samples reveal extensive cellular mutation. " +
            "The corruption spreads in fractal patterns, each branch " +
            "terminating in crystalline nodules that pulse with faint light.",
            "Biological analysis",
            new[] { "blight", "corrupted", "infected", "creature" }),

        new CaptureTemplate(
            CaptureType.VisualRecord,
            "The creature's eyes retained a disturbing awareness " +
            "even after death. Markings on the skull suggest this one " +
            "was intelligent before the Blight claimed it.",
            "Field observation",
            new[] { "blight", "creature", "beast", "monster" }),

        new CaptureTemplate(
            CaptureType.RunicTrace,
            "Residual Aetheric contamination lingers in the remains. " +
            "The corruption signature differs from natural Blight patterns, " +
            "suggesting this specimen was deliberately exposed.",
            "Aetheric analysis",
            new[] { "blight", "aetheric", "corruption" })
    };

    #endregion

    #region Industrial Location Templates

    /// <summary>
    /// Templates for industrial environment discoveries.
    /// Matched by keywords: "machine", "device", "mechanism", "industrial".
    /// </summary>
    public static readonly CaptureTemplate[] IndustrialSite = new[]
    {
        new CaptureTemplate(
            CaptureType.EchoRecording,
            "A damaged echo-stone preserves a fragment: " +
            "'...production quotas exceeded for the third cycle. The Overseer " +
            "demands we push the forges harder. Workers report strange sounds " +
            "from the lower foundry...' The rest dissolves into static.",
            "Echo-stone playback",
            new[] { "industrial", "forge", "foundry", "factory" }),

        new CaptureTemplate(
            CaptureType.VisualRecord,
            "Schematics etched into a metal plate. The design appears to be " +
            "a power distribution network, though several sections are marked " +
            "with warning glyphs in a script no longer taught.",
            "Technical schematic",
            new[] { "machine", "device", "mechanism", "technical" }),

        new CaptureTemplate(
            CaptureType.TextFragment,
            "Safety notices posted throughout the area, written in formal Aesir: " +
            "'Unauthorized access to Level Seven facilities carries immediate " +
            "termination. This is not negotiable.'",
            "Warning placard",
            new[] { "industrial", "aesir", "facility" })
    };

    #endregion

    #region Ruin Location Templates

    /// <summary>
    /// Templates for ancient ruin discoveries.
    /// Matched by keywords: "ruin", "ancient", "collapsed", "abandoned".
    /// </summary>
    public static readonly CaptureTemplate[] AncientRuin = new[]
    {
        new CaptureTemplate(
            CaptureType.TextFragment,
            "Wall inscriptions, partially eroded: '...the Sleepers must not wake. " +
            "We have sealed the chambers according to the old rites. " +
            "May those who come after us understand why.'",
            "Wall inscription",
            new[] { "ruin", "ancient", "inscription", "tomb" }),

        new CaptureTemplate(
            CaptureType.OralHistory,
            "This site matches descriptions from the Wanderer's Tales: " +
            "'Where the three rivers once met, before the Glitch drank them dry, " +
            "the Aesir built their watching towers.' Only foundations remain now.",
            "Historical correlation",
            new[] { "ruin", "ancient", "tower", "aesir" }),

        new CaptureTemplate(
            CaptureType.VisualRecord,
            "Murals cover what remains of the walls. The art depicts " +
            "figures in flowing robes overseeing great machines. " +
            "Their faces have been systematically defaced.",
            "Artistic record",
            new[] { "ruin", "mural", "art", "ancient" })
    };

    #endregion

    #region Field Guide Trigger Templates

    /// <summary>
    /// Templates for Field Guide entry discoveries.
    /// These are triggered by specific gameplay events.
    /// </summary>
    public static readonly CaptureTemplate[] FieldGuideTriggers = new[]
    {
        new CaptureTemplate(
            CaptureType.OralHistory,
            "The veterans speak of Psychic Stress in hushed tones: " +
            "'The Blight doesn't just corrupt flesh. It seeps into your thoughts, " +
            "twists your perceptions. Keep your mind anchored, or lose it entirely.'",
            "Veteran's warning",
            new[] { "psychic", "stress", "mental" }),

        new CaptureTemplate(
            CaptureType.TextFragment,
            "A training manual, dog-eared and stained: " +
            "'Combat in the wastes is not about honor. It is about survival. " +
            "Strike fast, strike true, and know when to flee.'",
            "Combat manual",
            new[] { "combat", "fighting", "battle" }),

        new CaptureTemplate(
            CaptureType.OralHistory,
            "The pack-master's wisdom: 'Every gram matters out there. " +
            "I've seen Scavengers die because they couldn't run fast enough, " +
            "weighed down by treasure they'd never spend.'",
            "Pack-master's lesson",
            new[] { "burden", "weight", "carrying", "inventory" })
    };

    #endregion
}

#region Template Record

/// <summary>
/// Template for Data Capture generation.
/// </summary>
/// <param name="Type">The type of capture (TextFragment, Specimen, etc.).</param>
/// <param name="FragmentContent">The lore content of the capture.</param>
/// <param name="Source">Description of where the capture was found.</param>
/// <param name="MatchingKeywords">Keywords used to match captures to Codex entries.</param>
public record CaptureTemplate(
    CaptureType Type,
    string FragmentContent,
    string Source,
    string[] MatchingKeywords);

#endregion
