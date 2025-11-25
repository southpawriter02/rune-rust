namespace RuneAndRust.Core;

/// <summary>
/// v0.44.2: Survivor backgrounds representing pre-crash professions and acquired expertise.
/// The second choice in character creation, defining skills and starting equipment.
/// </summary>
public enum Background
{
    /// <summary>
    /// No background selected yet.
    /// </summary>
    None,

    /// <summary>
    /// Village Blacksmith: Skilled in metalwork and repair.
    /// Bonus: +1 MIGHT, starts with basic repair kit, bonus to equipment maintenance.
    /// </summary>
    VillageBlacksmith,

    /// <summary>
    /// Wandering Healer: Trained in medicine and herbcraft.
    /// Bonus: +1 WITS, starts with medical supplies, enhanced healing actions.
    /// </summary>
    WanderingHealer,

    /// <summary>
    /// Outcast Hunter: Survivor of the wilds, expert tracker.
    /// Bonus: +1 FINESSE, starts with hunting bow, bonus to exploration.
    /// </summary>
    OutcastHunter,

    /// <summary>
    /// Ruin Scholar: Studied the ancient texts and Dvergr systems.
    /// Bonus: +1 WITS, starts with runic codex, can read ancient inscriptions.
    /// </summary>
    RuinScholar,

    /// <summary>
    /// Militia Veteran: Served defending the settlements.
    /// Bonus: +1 STURDINESS, starts with militia equipment, combat experience.
    /// </summary>
    MilitiaVeteran,

    /// <summary>
    /// Temple Acolyte: Trained in the old ways and rune-lore.
    /// Bonus: +1 WILL, starts with runic focus, bonus vs Undying.
    /// </summary>
    TempleAcolyte
}

/// <summary>
/// Extension methods for Background.
/// </summary>
public static class BackgroundExtensions
{
    /// <summary>
    /// Gets the display name for a background.
    /// </summary>
    public static string GetDisplayName(this Background background) => background switch
    {
        Background.VillageBlacksmith => "Village Blacksmith",
        Background.WanderingHealer => "Wandering Healer",
        Background.OutcastHunter => "Outcast Hunter",
        Background.RuinScholar => "Ruin Scholar",
        Background.MilitiaVeteran => "Militia Veteran",
        Background.TempleAcolyte => "Temple Acolyte",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the description for a background.
    /// </summary>
    public static string GetDescription(this Background background) => background switch
    {
        Background.VillageBlacksmith => "You worked the forge before the world ended, shaping metal and mending tools. " +
                                        "Your strength and knowledge of metallurgy will serve you well in the ruins.",
        Background.WanderingHealer => "You traveled between settlements, treating the sick and wounded. " +
                                      "Your medical knowledge may be the difference between life and death.",
        Background.OutcastHunter => "Exiled to the wilds, you learned to survive where others perish. " +
                                    "Your tracking skills and instincts are honed to perfection.",
        Background.RuinScholar => "You dedicated your life to understanding the Dvergr and their fall. " +
                                  "Your knowledge of ancient systems could unlock forbidden secrets.",
        Background.MilitiaVeteran => "You defended your settlement against raiders and worse. " +
                                     "Combat is no stranger to you, and your discipline remains strong.",
        Background.TempleAcolyte => "You trained in the sacred rites and rune-craft of the old faith. " +
                                    "Your spiritual fortitude protects against the horrors of the Undying.",
        _ => "Select a background to view its description."
    };

    /// <summary>
    /// Gets the primary attribute bonus for a background.
    /// </summary>
    public static string GetPrimaryAttributeBonus(this Background background) => background switch
    {
        Background.VillageBlacksmith => "MIGHT",
        Background.WanderingHealer => "WITS",
        Background.OutcastHunter => "FINESSE",
        Background.RuinScholar => "WITS",
        Background.MilitiaVeteran => "STURDINESS",
        Background.TempleAcolyte => "WILL",
        _ => "None"
    };
}
