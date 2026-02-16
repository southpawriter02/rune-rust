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
            "Great iron hearts once pushed the life-blood of the mountain. Now they groan in their sleep, leaking scalding breath from rusted wounds.",
            [Biome.TheRoots, Biome.Muspelheim],
            weight: 2);

        yield return new RoomFunction(
            "Spark-Hall",
            "Great iron totems line the walls, singing the song of the invisible fire. Copper vines snake across the floor, warm to the touch and humming with the anger of sleeping gods.",
            [Biome.TheRoots, Biome.Citadel],
            weight: 2);

        yield return new RoomFunction(
            "Purification Vault",
            "Great cisterns hold the black waters, where silent spirits work to cleanse the rot. The air tastes of stinging salts, and the dark liquid ripples with unseen movement.",
            [Biome.TheRoots],
            weight: 1);

        yield return new RoomFunction(
            "Breath-Chambers",
            "The lungs of the mountain hang silent above, their iron ribs casting long shadows. Wind howls through the deep throats of the tunnels, carrying the scent of dust and old time.",
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
            "Scribe's Sanctum",
            "Tables crowded with the tools of the Old Ones' craft. Scrolls of white material, brittle as winter ice, bear drawings that hurt the eyes to follow. They sought answers here, but found only silence.",
            [Biome.Alfheim, Biome.Citadel],
            weight: 2);

        yield return new RoomFunction(
            "Memory Tomb",
            "Shelves of spirit-stones and rotting books stretch into the dark. The memories of a thousand dead years sleep here, waiting for a mind strong enough to wake them.",
            [Biome.Citadel, Biome.Alfheim],
            weight: 1);

        yield return new RoomFunction(
            "Star-Watcher's Eye",
            "Great brass tubes point at a ceiling that may once have shown the heavens. Now only darkness answers their vigil, and the glass lenses are clouded with age.",
            [Biome.Alfheim, Biome.Jotunheim],
            weight: 1);

        // Industrial functions
        yield return new RoomFunction(
            "Forge Hall",
            "Sleeping furnaces and anvils of impossible size dominate the space. The air still carries the memory of molten metal and the ringing hammers of giants.",
            [Biome.Muspelheim, Biome.Jotunheim],
            weight: 2);

        yield return new RoomFunction(
            "Creation Floor",
            "Iron tracks and many-armed machines fill the chamber. Whatever was built here required hands of steel and eyes of glass.",
            [Biome.TheRoots, Biome.Jotunheim],
            weight: 1);

        yield return new RoomFunction(
            "Smeltery",
            "Massive stone cauldrons and channels for river-fire crisscross the floor. The heat here must once have been enough to crack bone.",
            [Biome.Muspelheim],
            weight: 1);

        // Storage/Resource functions
        yield return new RoomFunction(
            "Frost-Sleep Vault",
            "Glass coffins line the walls, holding the frozen dead in a sleep that is not death. The air bites with a cold that comes not from winter, but from the stillness of time itself.",
            [Biome.Niflheim],
            weight: 2);

        yield return new RoomFunction(
            "Rock-Breaker's Hall",
            "Great jaws of iron and sorting-sieves stand silent. Piles of crushed stone await a journey that will never begin.",
            [Biome.TheRoots, Biome.Jotunheim],
            weight: 1);

        yield return new RoomFunction(
            "Warehouse",
            "Towering shelves of crates and containers create a labyrinth. The logistics of this place defy comprehension.",
            null, // Universal
            weight: 2);

        // Command/Control functions
        yield return new RoomFunction(
            "Watching-Eye Hall",
            "A circle of dark mirrors surrounds the central throne. They once showed the world to the masters of this place, but now reflect only ghosts.",
            [Biome.Citadel, Biome.TheRoots],
            weight: 1);

        yield return new RoomFunction(
            "Voice-Catcher Spire",
            "Iron trees reach for the sky, their branches tangled in a web of copper. Lights blink like dying stars, crying out to a void that no longer answers.",
            [Biome.Citadel, Biome.Alfheim],
            weight: 1);

        // Specialized biome functions
        yield return new RoomFunction(
            "Heart of the Sun",
            "The burning heart of the ruin. Cages of light flicker and fail around a piece of the sun itself, trapped and angry, pulsing with a heat that tastes of blood.",
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
            "Runic Sanctum",
            "Ancient marks scar every surface, glowing with the light of the deep earth. We do not know the words, but we feel their weight in our marrow.",
            [Biome.Jotunheim, Biome.Alfheim],
            weight: 1);
    }

    /// <summary>
    /// Gets functions compatible with a specific biome.
    /// </summary>
    public static IEnumerable<RoomFunction> GetFunctionsByBiome(Biome biome) =>
        GetAllFunctions().Where(f => f.HasAffinityFor(biome));
}
