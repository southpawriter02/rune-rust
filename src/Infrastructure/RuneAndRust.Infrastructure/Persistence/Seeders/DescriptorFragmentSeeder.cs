using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds Tier 3 descriptor fragments for composite description generation.
/// Contains 200+ fragments across all categories.
/// </summary>
public static class DescriptorFragmentSeeder
{
    public static IEnumerable<DescriptorFragment> GetAllFragments()
    {
        return GetSpatialFragments()
            .Concat(GetArchitecturalFragments())
            .Concat(GetDetailFragments())
            .Concat(GetAtmosphericFragments())
            .Concat(GetDirectionFragments());
    }

    #region Spatial Fragments (8+)

    private static IEnumerable<DescriptorFragment> GetSpatialFragments()
    {
        // Confined/Cramped
        yield return DescriptorFragment.CreateSpatial(
            "The ceiling presses low overhead, and the walls feel uncomfortably close",
            weight: 2, tags: ["Cramped", "Confined"]);

        yield return DescriptorFragment.CreateSpatial(
            "The narrow confines force you to move carefully",
            weight: 2, tags: ["Cramped"]);

        yield return DescriptorFragment.CreateSpatial(
            "Claustrophobic walls seem to close in from all sides",
            weight: 1, tags: ["Cramped", "Ominous"]);

        // Vast/Expansive
        yield return DescriptorFragment.CreateSpatial(
            "The chamber is vast, its far walls barely visible in the dim light",
            weight: 2, tags: ["Vast", "Large"]);

        yield return DescriptorFragment.CreateSpatial(
            "An enormous space opens before you, dwarfing all who enter",
            weight: 2, tags: ["Vast", "Massive"]);

        yield return DescriptorFragment.CreateSpatial(
            "The scale of this place defies human proportion",
            biomeAffinity: Biome.Jotunheim,
            weight: 2, tags: ["Massive"]);

        // Vertical
        yield return DescriptorFragment.CreateSpatial(
            "The space extends dramatically upward, disappearing into darkness",
            weight: 2, tags: ["Vertical", "Tall"]);

        yield return DescriptorFragment.CreateSpatial(
            "Dizzying heights loom above, the ceiling lost to shadow",
            weight: 1, tags: ["Vertical"]);

        // General
        yield return DescriptorFragment.CreateSpatial(
            "The proportions here feel subtly wrong, as if built for different beings",
            weight: 2, tags: []);

        yield return DescriptorFragment.CreateSpatial(
            "The space feels balanced, neither too large nor too small",
            weight: 3, tags: []);

        yield return DescriptorFragment.CreateSpatial(
            "Strange angles make it difficult to judge the true size of this place",
            weight: 1, tags: ["Disorienting"]);
    }

    #endregion

    #region Architectural Fragments (12+)

    private static IEnumerable<DescriptorFragment> GetArchitecturalFragments()
    {
        // Wall subcategory
        yield return DescriptorFragment.CreateArchitectural(
            "Corroded metal plates form the walls, held together by massive rivets",
            ArchitecturalSubcategory.Wall,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "The walls are reinforced with massive girders and support struts",
            ArchitecturalSubcategory.Wall,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Smooth, seamless walls suggest advanced pre-Glitch fabrication",
            ArchitecturalSubcategory.Wall,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Stone walls bear the marks of ancient craftsmanship",
            ArchitecturalSubcategory.Wall,
            biomeAffinity: Biome.Citadel,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Obsidian walls gleam with inner fire",
            ArchitecturalSubcategory.Wall,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Walls of solid ice reflect distorted images",
            ArchitecturalSubcategory.Wall,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Crystal formations grow from the walls like geometric tumors",
            ArchitecturalSubcategory.Wall,
            biomeAffinity: Biome.Alfheim,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Walls of fitted stone blocks rise to dizzying heights",
            ArchitecturalSubcategory.Wall,
            biomeAffinity: Biome.Jotunheim,
            weight: 2);

        // Ceiling subcategory
        yield return DescriptorFragment.CreateArchitectural(
            "Pipes snake across the ceiling like mechanical serpents",
            ArchitecturalSubcategory.Ceiling,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Vaulted arches support the weight of ages above",
            ArchitecturalSubcategory.Ceiling,
            biomeAffinity: Biome.Citadel,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "The ceiling drips with condensation that sizzles when it hits the floor",
            ArchitecturalSubcategory.Ceiling,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Stalactites of ice hang like frozen daggers overhead",
            ArchitecturalSubcategory.Ceiling,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "The ceiling glows with bioluminescent fungal growth",
            ArchitecturalSubcategory.Ceiling,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        // Floor subcategory
        yield return DescriptorFragment.CreateArchitectural(
            "Grated flooring reveals machinery churning below",
            ArchitecturalSubcategory.Floor,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "Worn flagstones show the passage of countless feet",
            ArchitecturalSubcategory.Floor,
            biomeAffinity: Biome.Citadel,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "The floor radiates heat from the magma beneath",
            ArchitecturalSubcategory.Floor,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "A layer of frost covers every surface, making footing treacherous",
            ArchitecturalSubcategory.Floor,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateArchitectural(
            "The floor is carpeted with soft fungal growth",
            ArchitecturalSubcategory.Floor,
            biomeAffinity: Biome.TheRoots,
            weight: 2);
    }

    #endregion

    #region Detail Fragments (28+)

    private static IEnumerable<DescriptorFragment> GetDetailFragments()
    {
        // Decay subcategory
        yield return DescriptorFragment.CreateDetail(
            "Rust streaks mark the surfaces like old blood",
            DetailSubcategory.Decay,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Corrosion has eaten through many of the structural supports",
            DetailSubcategory.Decay,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Everything here shows signs of advanced degradation",
            DetailSubcategory.Decay,
            weight: 3);

        yield return DescriptorFragment.CreateDetail(
            "Time has not been kind to this place",
            DetailSubcategory.Decay,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Cracks spider across every surface",
            DetailSubcategory.Decay,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "The scars of neglect mark this place",
            DetailSubcategory.Decay,
            weight: 2);

        // Runes subcategory
        yield return DescriptorFragment.CreateDetail(
            "Faded runes pulse weakly along the doorframes",
            DetailSubcategory.Runes,
            biomeAffinity: Biome.Jotunheim,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Ancient symbols cover the walls in patterns that hurt to look at",
            DetailSubcategory.Runes,
            biomeAffinity: Biome.Alfheim,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Glowing inscriptions trace paths of forgotten power",
            DetailSubcategory.Runes,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Runes of warning mark the threshold",
            DetailSubcategory.Runes,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "The script here predates all known languages",
            DetailSubcategory.Runes,
            biomeAffinity: Biome.Jotunheim,
            weight: 1);

        // Activity subcategory
        yield return DescriptorFragment.CreateDetail(
            "Signs of recent passage disturb the dust",
            DetailSubcategory.Activity,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Something has been through here, and not long ago",
            DetailSubcategory.Activity,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Footprints in the grime suggest you are not the first visitor",
            DetailSubcategory.Activity,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Disturbed debris hints at recent activity",
            DetailSubcategory.Activity,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "The dust here has been disturbed, but by what?",
            DetailSubcategory.Activity,
            weight: 2);

        // Ominous subcategory
        yield return DescriptorFragment.CreateDetail(
            "A sense of wrongness pervades the air",
            DetailSubcategory.Ominous,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Something terrible happened here, and the walls remember",
            DetailSubcategory.Ominous,
            weight: 1);

        yield return DescriptorFragment.CreateDetail(
            "Shadows seem to move at the edge of your vision",
            DetailSubcategory.Ominous,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Every instinct screams that this place is dangerous",
            DetailSubcategory.Ominous,
            weight: 1);

        yield return DescriptorFragment.CreateDetail(
            "The silence here feels watchful, expectant",
            DetailSubcategory.Ominous,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Dark stains on the floor tell a story best left unread",
            DetailSubcategory.Ominous,
            weight: 2);

        // Loot subcategory
        yield return DescriptorFragment.CreateDetail(
            "Scattered debris might hide something of value",
            DetailSubcategory.Loot,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Containers of unknown origin line the walls",
            DetailSubcategory.Loot,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Glints of metal catch your eye among the wreckage",
            DetailSubcategory.Loot,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "This place may have been picked over, but treasures could remain",
            DetailSubcategory.Loot,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "The previous occupants left much behind in their haste",
            DetailSubcategory.Loot,
            weight: 2);

        yield return DescriptorFragment.CreateDetail(
            "Equipment and supplies lie abandoned throughout",
            DetailSubcategory.Loot,
            weight: 2);
    }

    #endregion

    #region Atmospheric Fragments (155+)

    private static IEnumerable<DescriptorFragment> GetAtmosphericFragments()
    {
        return GetSmellFragments()
            .Concat(GetSoundFragments())
            .Concat(GetLightFragments())
            .Concat(GetTemperatureFragments());
    }

    private static IEnumerable<DescriptorFragment> GetSmellFragments()
    {
        // General smells
        yield return DescriptorFragment.CreateAtmospheric(
            "musty and old, like a tomb long sealed",
            AtmosphericSubcategory.Smell,
            weight: 3);

        yield return DescriptorFragment.CreateAtmospheric(
            "sharp with the tang of metal and machine oil",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "thick with the smell of rust and decay",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "heavy with sulfur and brimstone",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "clean and cold, almost sterile",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "strange and otherworldly, defying description",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Alfheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "ancient, like stone that has never known sunlight",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Jotunheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "damp and earthy, with notes of fungal growth",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "acrid with smoke and ash",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "stale and dead, as if the air itself has given up",
            AtmosphericSubcategory.Smell,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "tinged with the bitter scent of lightning",
            AtmosphericSubcategory.Smell,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "rich with the scent of ages past",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Citadel,
            weight: 2);

        // Additional smell fragments
        yield return DescriptorFragment.CreateAtmospheric(
            "carrying hints of something sweet and rotten",
            AtmosphericSubcategory.Smell,
            weight: 1);

        yield return DescriptorFragment.CreateAtmospheric(
            "burned and scorched, memories of fire",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "crisp with frost, numbing to breathe",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "chemical and wrong, like nothing natural",
            AtmosphericSubcategory.Smell,
            biomeAffinity: Biome.Surface,
            weight: 2);
    }

    private static IEnumerable<DescriptorFragment> GetSoundFragments()
    {
        // General sounds
        yield return DescriptorFragment.CreateAtmospheric(
            "Your footsteps echo into infinity",
            AtmosphericSubcategory.Sound,
            weight: 3);

        yield return DescriptorFragment.CreateAtmospheric(
            "The silence here is absolute and oppressive",
            AtmosphericSubcategory.Sound,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Distant machinery rumbles like a sleeping beast",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Water drips somewhere in the darkness",
            AtmosphericSubcategory.Sound,
            weight: 3);

        yield return DescriptorFragment.CreateAtmospheric(
            "Metal groans and settles in the walls",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "The crackling of flames provides constant backdrop",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Ice cracks and shifts with glacial patience",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Crystalline chimes ring without source",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.Alfheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Stone groans under weight you cannot see",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.Jotunheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "The wind howls through unseen passages",
            AtmosphericSubcategory.Sound,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Something skitters in the shadows",
            AtmosphericSubcategory.Sound,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "A low hum vibrates through everything",
            AtmosphericSubcategory.Sound,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Whispers seem to come from the walls themselves",
            AtmosphericSubcategory.Sound,
            weight: 1);

        yield return DescriptorFragment.CreateAtmospheric(
            "The hiss of steam punctuates the silence",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Pipes gurgle with unknown fluids",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Magma bubbles somewhere below",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "The crack of splitting ice echoes",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Distant thunder rolls through the stone",
            AtmosphericSubcategory.Sound,
            biomeAffinity: Biome.Jotunheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "The hum of sleeping thunder fills the air",
            AtmosphericSubcategory.Sound,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "The flutter of unseen wings",
            AtmosphericSubcategory.Sound,
            weight: 1);
    }

    private static IEnumerable<DescriptorFragment> GetLightFragments()
    {
        // General light
        yield return DescriptorFragment.CreateAtmospheric(
            "Darkness pools in every corner",
            AtmosphericSubcategory.Light,
            weight: 3);

        yield return DescriptorFragment.CreateAtmospheric(
            "Faint light filters from an unknown source",
            AtmosphericSubcategory.Light,
            weight: 3);

        yield return DescriptorFragment.CreateAtmospheric(
            "Emergency lighting flickers weakly",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Bioluminescent fungi provide eerie illumination",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Lava flows cast everything in orange light",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "The glow of molten rock paints dancing shadows",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Ice reflects light in fractured rainbows",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "A cold, blue light suffuses everything",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Crystals pulse with inner radiance",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Alfheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Impossible colors shimmer at the edge of perception",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Alfheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Ancient runes provide dim illumination",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Jotunheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Shadows here seem deeper than they should be",
            AtmosphericSubcategory.Light,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Light struggles to penetrate the gloom",
            AtmosphericSubcategory.Light,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Your light source seems dimmed by the darkness",
            AtmosphericSubcategory.Light,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Torch sconces line the walls, long extinguished",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Citadel,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Phosphorescent moss clings to the walls",
            AtmosphericSubcategory.Light,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "Glowing particles drift through the air",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Alfheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "The darkness feels alive and watching",
            AtmosphericSubcategory.Light,
            weight: 1);

        yield return DescriptorFragment.CreateAtmospheric(
            "Ghost-sparks dance in the shadows",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Surface,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "A sickly glow emanates from unknown sources",
            AtmosphericSubcategory.Light,
            biomeAffinity: Biome.Surface,
            weight: 2);
    }

    private static IEnumerable<DescriptorFragment> GetTemperatureFragments()
    {
        // General temperature
        yield return DescriptorFragment.CreateAtmospheric(
            "charged with tension",
            AtmosphericSubcategory.Temperature,
            weight: 3);

        yield return DescriptorFragment.CreateAtmospheric(
            "thick and hard to breathe",
            AtmosphericSubcategory.Temperature,
            weight: 3);

        yield return DescriptorFragment.CreateAtmospheric(
            "uncomfortably warm and humid",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.TheRoots,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "oppressively hot, stealing your breath",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "scorching, waves of heat distorting your vision",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "bitterly cold, numbing exposed skin",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "freezing, your breath crystallizing instantly",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "strange, as if temperature has no meaning here",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Alfheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "chill and still, like a forgotten crypt",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Citadel,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "surprisingly temperate despite appearances",
            AtmosphericSubcategory.Temperature,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "damp and cool, moisture clinging to everything",
            AtmosphericSubcategory.Temperature,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "stifling, as if the walls themselves radiate heat",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Muspelheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "bone-chilling, the cold seeping into your very soul",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Niflheim,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "dry and dusty, preserving all it touches",
            AtmosphericSubcategory.Temperature,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "heavy with moisture and the promise of rain",
            AtmosphericSubcategory.Temperature,
            weight: 2);

        yield return DescriptorFragment.CreateAtmospheric(
            "shifting between hot and cold with no pattern",
            AtmosphericSubcategory.Temperature,
            biomeAffinity: Biome.Alfheim,
            weight: 2);
    }

    #endregion

    #region Direction Fragments (6+)

    private static IEnumerable<DescriptorFragment> GetDirectionFragments()
    {
        yield return DescriptorFragment.CreateDirection(
            "into darkness ahead",
            weight: 3);

        yield return DescriptorFragment.CreateDirection(
            "before you, narrowing into shadow",
            weight: 2);

        yield return DescriptorFragment.CreateDirection(
            "away from you in both directions",
            weight: 2);

        yield return DescriptorFragment.CreateDirection(
            "into the unknown depths",
            weight: 2);

        yield return DescriptorFragment.CreateDirection(
            "toward an uncertain destination",
            weight: 2);

        yield return DescriptorFragment.CreateDirection(
            "downward into deeper darkness",
            weight: 2);

        yield return DescriptorFragment.CreateDirection(
            "upward toward distant light",
            weight: 2);

        yield return DescriptorFragment.CreateDirection(
            "in a direction you cannot quite name",
            biomeAffinity: Biome.Alfheim,
            weight: 1);

        yield return DescriptorFragment.CreateDirection(
            "into chambers beyond counting",
            biomeAffinity: Biome.Jotunheim,
            weight: 2);

        yield return DescriptorFragment.CreateDirection(
            "through rusted portals",
            biomeAffinity: Biome.TheRoots,
            weight: 2);
    }

    #endregion

    /// <summary>
    /// Gets fragments by category.
    /// </summary>
    public static IEnumerable<DescriptorFragment> GetFragmentsByCategory(FragmentCategory category) =>
        GetAllFragments().Where(f => f.Category == category);

    /// <summary>
    /// Gets fragments compatible with a biome.
    /// </summary>
    public static IEnumerable<DescriptorFragment> GetFragmentsByBiome(Biome biome) =>
        GetAllFragments().Where(f => f.MatchesBiome(biome));
}
