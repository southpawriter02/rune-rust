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
            "Pumping Station",
            "Massive geothermal pumps once circulated superheated fluids through the facility. Now they groan with neglect, leaking scalding steam from corroded seals.",
            [Biome.TheRoots, Biome.Muspelheim],
            weight: 2);

        yield return new RoomFunction(
            "Power Station",
            "Banks of capacitors and generators line the walls, some still humming with residual charge. Cables snake across the floor like mechanical tendrils.",
            [Biome.TheRoots, Biome.Citadel],
            weight: 2);

        yield return new RoomFunction(
            "Water Treatment",
            "Massive filtration tanks dominate the space, filled with murky liquid. The air is thick with chemical odors and the sound of dripping.",
            [Biome.TheRoots],
            weight: 1);

        yield return new RoomFunction(
            "Ventilation Hub",
            "Giant fans hang motionless from the ceiling, their blades casting strange shadows. Air shafts lead off in multiple directions.",
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
            "Research Lab",
            "Workbenches cluttered with equipment and half-finished experiments. Notes and diagrams cover every available surface, their meaning now obscure.",
            [Biome.Alfheim, Biome.Citadel],
            weight: 2);

        yield return new RoomFunction(
            "Archives",
            "Shelves of data crystals and ancient tomes stretch into the darkness. The accumulated knowledge of ages gathers dust here.",
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
            "Assembly Floor",
            "Conveyor systems and assembly machinery fill the chamber. Whatever was built here required precision beyond mortal hands.",
            [Biome.TheRoots, Biome.Jotunheim],
            weight: 1);

        yield return new RoomFunction(
            "Smeltery",
            "Massive crucibles and channels for molten material crisscross the floor. The heat here must once have been unbearable.",
            [Biome.Muspelheim],
            weight: 1);

        // Storage/Resource functions
        yield return new RoomFunction(
            "Cryo Storage",
            "Rows of cryogenic pods line the walls, their contents obscured by frost. Condensation drips in the unnaturally cold air.",
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
            "Operations Nexus",
            "A command center with displays and consoles arranged in concentric rings. From here, entire systems were once directed.",
            [Biome.Citadel, Biome.TheRoots],
            weight: 1);

        yield return new RoomFunction(
            "Communications Array",
            "Antenna arrays and signal processors fill the space. Some still blink with activity, sending messages to receivers long destroyed.",
            [Biome.Citadel, Biome.Alfheim],
            weight: 1);

        // Specialized biome functions
        yield return new RoomFunction(
            "Reactor Core",
            "The heart of the facility's power generation. Containment fields flicker weakly around an energy source that defies understanding.",
            [Biome.Muspelheim, Biome.Alfheim],
            weight: 1);

        yield return new RoomFunction(
            "Spore Garden",
            "Bioluminescent fungi carpet every surface, casting an eerie glow. The air is thick with spores and the musty smell of growth.",
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
