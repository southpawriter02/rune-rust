namespace RuneAndRust.Core;

/// <summary>
/// v0.44.2: Survivor lineages representing inherited traits and bloodline.
/// The first choice in character creation, defining genetic/bloodline heritage.
/// </summary>
public enum Lineage
{
    /// <summary>
    /// No lineage selected yet.
    /// </summary>
    None,

    /// <summary>
    /// Clan-Born: Raised in the surviving settlements, strong community ties.
    /// Bonus: +5 starting Currency, +1 to social interactions with NPCs.
    /// </summary>
    ClanBorn,

    /// <summary>
    /// Rune-Marked: Born with runic inscriptions, touched by ancient magic.
    /// Bonus: +1 WILL, can sense runic objects, slight aether sensitivity.
    /// </summary>
    RuneMarked,

    /// <summary>
    /// Iron-Blooded: Descended from those who integrated with Dvergr technology.
    /// Bonus: +1 STURDINESS, resistant to Corruption, bonus vs mechanical foes.
    /// </summary>
    IronBlooded,

    /// <summary>
    /// Vargr-Kin: Those with the wolf-blood, hunters and trackers.
    /// Bonus: +1 FINESSE, enhanced senses in exploration, tracking ability.
    /// </summary>
    VargrKin
}

/// <summary>
/// Extension methods for Lineage.
/// </summary>
public static class LineageExtensions
{
    /// <summary>
    /// Gets the display name for a lineage.
    /// </summary>
    public static string GetDisplayName(this Lineage lineage) => lineage switch
    {
        Lineage.ClanBorn => "Clan-Born",
        Lineage.RuneMarked => "Rune-Marked",
        Lineage.IronBlooded => "Iron-Blooded",
        Lineage.VargrKin => "Vargr-Kin",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the description for a lineage.
    /// </summary>
    public static string GetDescription(this Lineage lineage) => lineage switch
    {
        Lineage.ClanBorn => "Raised in the surviving settlements with strong community ties. " +
                            "You start with extra currency and have an easier time with social interactions.",
        Lineage.RuneMarked => "Born with runic inscriptions etched into your skin, touched by ancient magic. " +
                              "You have heightened WILL and can sense runic objects nearby.",
        Lineage.IronBlooded => "Descended from those who integrated with Dvergr technology in ages past. " +
                               "You have enhanced STURDINESS and resistance to mechanical corruption.",
        Lineage.VargrKin => "Those with the wolf-blood running through their veins, natural hunters and trackers. " +
                            "You have heightened FINESSE and enhanced senses during exploration.",
        _ => "Select a lineage to view its description."
    };

    /// <summary>
    /// Gets the attribute modifiers for a lineage.
    /// Returns a dictionary mapping attribute names to their modifiers (+1 or -1).
    /// Each lineage grants +1 to one attribute and -1 to another for balance.
    /// </summary>
    public static Dictionary<string, int> GetAttributeModifiers(this Lineage lineage) => lineage switch
    {
        // Clan-Born: Strong community work builds physical strength, less scholarly
        Lineage.ClanBorn => new Dictionary<string, int>
        {
            { "MIGHT", 1 },
            { "WITS", -1 }
        },
        // Rune-Marked: Magical affinity strengthens will, but weakens physical form
        Lineage.RuneMarked => new Dictionary<string, int>
        {
            { "WILL", 1 },
            { "STURDINESS", -1 }
        },
        // Iron-Blooded: Mechanical integration enhances durability, reduces agility
        Lineage.IronBlooded => new Dictionary<string, int>
        {
            { "STURDINESS", 1 },
            { "FINESSE", -1 }
        },
        // Vargr-Kin: Wolf-blood grants agility, primal nature weakens mental discipline
        Lineage.VargrKin => new Dictionary<string, int>
        {
            { "FINESSE", 1 },
            { "WILL", -1 }
        },
        _ => new Dictionary<string, int>()
    };

    /// <summary>
    /// Gets a short summary of the attribute modifiers for display.
    /// </summary>
    public static string GetAttributeModifierSummary(this Lineage lineage)
    {
        var mods = lineage.GetAttributeModifiers();
        if (mods.Count == 0) return "";

        var parts = new List<string>();
        foreach (var kvp in mods)
        {
            string sign = kvp.Value > 0 ? "+" : "";
            parts.Add($"{sign}{kvp.Value} {kvp.Key}");
        }
        return string.Join(", ", parts);
    }
}
