using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds room function variants for specialized chamber purposes.
/// 18+ functions covering utility, combat, and specialized areas.
/// </summary>
public static class RoomFunctionSeeder
{
    public static IEnumerable<RoomFunction> GetAllFunctions()
    {
        // Utility/Infrastructure functions
        yield return new RoomFunction(
            "Circulation Station",
            "Massive geothermal engines once circulated superheated fluids through the facility. Now they groan with neglect, leaking scalding steam from rusted seals.",
            [Biome.TheRoots, Biome.Muspelheim],
            weight: 2);

        yield return new RoomFunction(
            "Lightning Station",
            "Banks of lightning-jars and dynamos line the walls, some still humming with residual charge. Iron-vines snake across the floor like iron tendrils.",
            [Biome.TheRoots, Biome.Citadel],
            weight: 2);

        yield return new RoomFunction(
            "Water Purification",
            "Massive purification vats dominate the space, filled with murky liquid. The air is thick with alchemical odors and the sound of dripping.",
            [Biome.TheRoots],
            weight: 1);

        yield return new RoomFunction(
            "Breath-Chamber",
            "Giant wind-wheels hang motionless from the ceiling, their blades casting strange shadows. Air shafts lead off in multiple directions.",
            [Biome.TheRoots, Biome.Citadel],
            weight: 2);

        // Combat/Military functions
        yield return new RoomFunction(
            "Training Hall",
            "Combat dummies and weapon racks line the perimeter. Scorch marks and blade gouges on the floor speak of countless drills.",
            null, // Universal
            weight: 2);

        yield return new RoomFunction(
            "Armory",
            "Weapon racks and armor stands fill the space, though most have been emptied. A few pieces remain, too damaged or specialized for common use.",
            null, // Universal
            weight: 2);

        yield return new RoomFunction(
            "Guard Post",
            "A fortified checkpoint with murder holes and reinforced barriers. The defenders have long since abandoned their posts.",
            [Biome.Citadel, Biome.Jotunheim],
            weight: 2);

        // Research/Knowledge functions
        yield return new RoomFunction(
            "Scholar's Study",
            "Workbenches cluttered with tools and half-finished experiments. Notes and diagrams cover every available surface, their meaning now obscure.",
            [Biome.Alfheim, Biome.Citadel],
            weight: 2);

        yield return new RoomFunction(
            "Archives",
            "Shelves of rune-crystals and ancient tomes stretch into the darkness. The accumulated knowledge of ages gathers dust here.",
            [Biome.Citadel, Biome.Alfheim],
            weight: 1);

        yield return new RoomFunction(
            "Observatory",
            "Astronomical instruments point at a ceiling that may once have shown the stars. Now only darkness answers their vigil.",
            [Biome.Alfheim, Biome.Jotunheim],
            weight: 1);

        // Industrial functions
        yield return new RoomFunction(
            "Forge Hall",
            "Dormant furnaces and anvils of impossible size dominate the space. The air still carries the memory of molten metal and ringing hammers.",
            [Biome.Muspelheim, Biome.Jotunheim],
            weight: 2);

        yield return new RoomFunction(
            "Creation Floor",
            "Moving tracks and creation-engines fill the chamber. Whatever was built here required precision beyond mortal hands.",
            [Biome.TheRoots, Biome.Jotunheim],
            weight: 1);

        yield return new RoomFunction(
            "Smeltery",
            "Massive crucibles and channels for molten material crisscross the floor. The heat here must once have been unbearable.",
            [Biome.Muspelheim],
            weight: 1);

        // Storage/Resource functions
        yield return new RoomFunction(
            "Frost Vault",
            "Rows of frost-coffins line the walls, their contents obscured by frost. Frost-melt drips in the unnaturally cold air.",
            [Biome.Niflheim],
            weight: 2);

        yield return new RoomFunction(
            "Ore Processing",
            "Crushers and sorting machinery stand silent. Piles of processed ore await transport that will never come.",
            [Biome.TheRoots, Biome.Jotunheim],
            weight: 1);

        yield return new RoomFunction(
            "Warehouse",
            "Towering shelves of crates and containers create a labyrinth. The logistics of this place defy comprehension.",
            null, // Universal
            weight: 2);

        // Command/Control functions
        yield return new RoomFunction(
            "Command Nexus",
            "A command center with seeing-stones and control-slabs arranged in concentric rings. From here, entire systems were once directed.",
            [Biome.Citadel, Biome.TheRoots],
            weight: 1);

        yield return new RoomFunction(
            "Whisper-Hall",
            "Listening-spires and message-stones fill the space. Some still blink with activity, sending messages to receivers long destroyed.",
            [Biome.Citadel, Biome.Alfheim],
            weight: 1);

        // Specialized biome functions
        yield return new RoomFunction(
            "Heart-Core",
            "The heart of the facility's power generation. Warding runes flicker weakly around a power source that defies understanding.",
            [Biome.Muspelheim, Biome.Alfheim],
            weight: 1);

        yield return new RoomFunction(
            "Spore Garden",
            "Ghost-light fungi carpet every surface, casting an eerie glow. The air is thick with spores and the musty smell of growth.",
            [Biome.TheRoots],
            weight: 1);

        yield return new RoomFunction(
            "Ice Shrine",
            "Crystalline formations of pure ice form patterns of alien beauty. The cold here seems to emanate from the ice itself.",
            [Biome.Niflheim],
            weight: 1);

        yield return new RoomFunction(
            "Runic Chamber",
            "Ancient runes cover every surface, pulsing with a power that predates recorded history. Their meaning is lost, but their power remains.",
            [Biome.Jotunheim, Biome.Alfheim],
            weight: 1);
    }

    /// <summary>
    /// Gets functions compatible with a specific biome.
    /// </summary>
    public static IEnumerable<RoomFunction> GetFunctionsByBiome(Biome biome) =>
        GetAllFunctions().Where(f => f.HasAffinityFor(biome));
}
