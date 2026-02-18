using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds ExaminationDescriptors for room features and universal structural elements.
/// Provides layered examination text (Cursory/Detailed/Expert) based on WITS checks.
/// </summary>
public static class RoomFeatureExaminationSeeder
{
    public static IEnumerable<ExaminationDescriptor> GetAllDescriptors()
    {
        return GetInteractableDescriptors()
            .Concat(GetDecorationDescriptors())
            .Concat(GetLightSourceDescriptors())
            .Concat(GetHazardDescriptors())
            .Concat(GetUniversalStructuralDescriptors());
    }

    #region Interactable Features

    private static IEnumerable<ExaminationDescriptor> GetInteractableDescriptors()
    {
        // weapon_rack
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Machinery, "weapon_rack",
            "A wooden rack designed to hold weapons. Most slots are empty.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Machinery, "weapon_rack",
            "The wood shows tool marks from hasty repairs. Some mounting pegs are new, suggesting recent use.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Machinery, "weapon_rack",
            "Dvergr craftsmanship, predating the Glitch. The empty slots tell a story of desperate arming.");

        // bookshelf
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "bookshelf",
            "Towering shelves crammed with dusty volumes. Many have crumbled to fragments.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "bookshelf",
            "Some volumes remain intact. Technical manuals, personal journals, administrative records.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "bookshelf",
            "A pre-Glitch library index card is wedged behind one shelf. This collection was catalogued once.", revealsHint: true);

        // weapon_crate
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Container, "weapon_crate",
            "A reinforced wooden crate with military markings.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Container, "weapon_crate",
            "The seals are broken. Someone got here first, but may have left something behind.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Container, "weapon_crate",
            "Citadel armory supply crate, form of the Seventh Age. These held standard-issue weapons for garrison troops.");

        // old_crate
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Container, "old_crate",
            "A generic storage crate, dusty and forgotten.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Container, "old_crate",
            "The lid is loose. Contents have shifted over the centuries.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Container, "old_crate",
            "The wood is from surface trees - this was brought down from above, long ago.");

        // rusted_spirit_slate
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Machinery, "rusted_spirit_slate",
            "An ancient spirit-slate, its glass face dark and cracked.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Machinery, "rusted_spirit_slate",
            "Rust has claimed most of the inner-guts, but the spark-vein might be salvageable.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Machinery, "rusted_spirit_slate",
            "Oracle-box, Old World Third Age. If the thought-stone survived, it might hold records of the Old Law.", revealsHint: true);

        // salvage_pile
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Container, "salvage_pile",
            "A heap of broken iron-works and debris.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Container, "salvage_pile",
            "Among the junk: intact gears, copper wiring, sleeping iron-muscles. Worth sifting through.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Container, "salvage_pile",
            "Most of this is Dvergr manufacture. The components could be repurposed for healing machines.");

        // harvestable_fungus
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Flora, "harvestable_fungus",
            "Shelf mushrooms spiral up the wall in a bioluminescent cascade.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Flora, "harvestable_fungus",
            "Some species are edible. Others are poisonous. A few have alchemical properties.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Flora, "harvestable_fungus",
            "The blue-glow variety is Whisper Fungus - useful for calming draughts and sleep aids.");

        // ancient_forge
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Machinery, "ancient_forge",
            "A massive forge built into the rock itself. Heat still radiates from its depths.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Machinery, "ancient_forge",
            "The bellows mechanism appears functional. The fire... the fire never went out.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Machinery, "ancient_forge",
            "Jotun construction, adapted by Dvergr smiths. The eternal flame is runic in nature.", revealsHint: true);

        // mineral_vein
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "mineral_vein",
            "Metallic ore gleams in the volcanic stone.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "mineral_vein",
            "High iron content with trace silver. Worth extracting if you have the tools.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "mineral_vein",
            "This vein runs deep. The ancients mined here extensively - follow the chisel marks.");

        // ice_encased_item
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Container, "ice_encased_item",
            "Something glitters within the frozen wall, preserved for eternity.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Container, "ice_encased_item",
            "The ice is clear enough to see the shape - armor? A weapon? Valuable either way.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Container, "ice_encased_item",
            "The preservation is too perfect. This was frozen deliberately, not by nature.", revealsHint: true);

        // giant_table
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "giant_table",
            "A dining surface sized for beings three times human height.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "giant_table",
            "Despite its size, the craftsmanship is delicate. Carved runes circle the edge.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "giant_table",
            "A feast table from the Titan Age. The runes are blessings for hospitality and abundance.");

        // giant_artifact
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "giant_artifact",
            "Massive relics of the Jotun civilization tower overhead.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "giant_artifact",
            "Power still hums within. These artifacts shaped the world before humanity emerged.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "giant_artifact",
            "This is a Worldstone fragment - one of the primal tools of creation. Handle with extreme care.", revealsHint: true);
    }

    #endregion

    #region Decoration Features

    private static IEnumerable<ExaminationDescriptor> GetDecorationDescriptors()
    {
        // broken_fountain
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "broken_fountain",
            "A dry fountain, its basin choked with debris.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "broken_fountain",
            "The plumbing is intact beneath the rubble. This could flow again.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "broken_fountain",
            "Commemorative fountain, marking the founding of this citadel level. The date is worn but readable.");

        // throne
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "throne",
            "A massive throne of black stone dominates the chamber.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "throne",
            "Runes of authority are carved into every surface. This was a seat of power.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "throne",
            "The throne is a Judgment Seat - those who sat here could command obedience through runic binding.", revealsHint: true);

        // heart_fungus
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Flora, "heart_fungus",
            "A pulsing mass of organic material, veined with bioluminescence.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Flora, "heart_fungus",
            "This is the apex organism of The Roots. Everything here connects to it.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Flora, "heart_fungus",
            "The Heart is sentient, after a fashion. It feels your presence. It does not welcome you.", revealsHint: true);

        // frozen_corpse
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Corpse, "frozen_corpse",
            "Bodies preserved in ice, their expressions frozen in time.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Corpse, "frozen_corpse",
            "Their equipment is ancient but potentially functional if thawed carefully.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Corpse, "frozen_corpse",
            "These are the original garrison. They never saw the Glitch coming.");

        // ice_throne
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "ice_throne",
            "A throne carved from a single block of eternal ice.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "ice_throne",
            "The cold radiating from it is unnatural. Whoever sits here commands the cold itself.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "ice_throne",
            "A binding throne. The ruler of Niflheim is not merely seated here - they are imprisoned.", revealsHint: true);

        // broken_statue
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "broken_statue",
            "Fragments of a massive statue litter the area.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "broken_statue",
            "Jotun proportions. This was a memorial or deity representation.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "broken_statue",
            "The statue depicted Ymir, the First Giant. Its destruction was deliberate.");

        // glowing_runes
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Inscription, "glowing_runes",
            "Ancient symbols cover the walls, some still pulsing with power.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Inscription, "glowing_runes",
            "The script is Old Jotun. A few phrases are translatable: warnings, directions, names.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Inscription, "glowing_runes",
            "This is a historical record. The Giants documented their fall here, for whoever would follow.", revealsHint: true);

        // giant_sarcophagus
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "giant_sarcophagus",
            "A burial chamber of impossible scale. The lid has been moved.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "giant_sarcophagus",
            "Runes of binding encircle the base. Whatever was sealed here was meant to sleep forever.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "giant_sarcophagus",
            "This held a Titan - not Jotun, but older. And now it wakes.", revealsHint: true);
    }

    #endregion

    #region Light Source Features

    private static IEnumerable<ExaminationDescriptor> GetLightSourceDescriptors()
    {
        // reading_lamp
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "reading_lamp",
            "An ancient lamp, still flickering with ghostly light.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "reading_lamp",
            "The fuel is long exhausted. The light is runic, not natural.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "reading_lamp",
            "A Scholar's Lamp - it brightens when focused attention is near. Useful for research.");

        // glowing_fungus
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Flora, "glowing_fungus",
            "Bioluminescent mushrooms provide a pale blue glow.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Flora, "glowing_fungus",
            "The light pulses slowly, like breathing. The spores in the air shimmer.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Flora, "glowing_fungus",
            "Deathcap Luminaries - beautiful but toxic. Don't touch them or breathe too deeply.", revealsHint: true);

        // brazier
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "brazier",
            "A stone brazier holds flames that seem to burn without fuel.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "brazier",
            "Runic fire. It provides light and heat but consumes nothing.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "brazier",
            "An Ever-Flame, created during the Titan Age. It will burn until the world ends.");

        // ice_crystal
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Decorative, "ice_crystal",
            "Massive crystals refract what little light exists into rainbows.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Decorative, "ice_crystal",
            "The crystals amplify ambient light. Even a candle here would illuminate the room.");
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Decorative, "ice_crystal",
            "Natural resonance crystals. They respond to sound as well as light. Speak softly.");
    }

    #endregion

    #region Hazard Features

    private static IEnumerable<ExaminationDescriptor> GetHazardDescriptors()
    {
        // spore_cloud
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Machinery, "spore_cloud",
            "Clouds of spores drift through the air.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Machinery, "spore_cloud",
            "Breathing deeply would be unwise. Cover your face.", revealsHint: true);
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Machinery, "spore_cloud",
            "Dream-death spores - prolonged exposure causes waking nightmares. Get out quickly.");

        // heat_vent
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Machinery, "heat_vent",
            "Cracks in the floor leak visible heat.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Machinery, "heat_vent",
            "The venting is predictable. Watch the rhythm and time your passage.", revealsHint: true);
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Machinery, "heat_vent",
            "Earth-blood pressure release. The intervals are regular - safe for a few heartbeats.");

        // lava_pool
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Machinery, "lava_pool",
            "Molten rock fills portions of the chamber.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Machinery, "lava_pool",
            "The surface has cooled in places. A careful path exists.", revealsHint: true);
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Machinery, "lava_pool",
            "The lava is unnaturally active. Something feeds it from below.");

        // lava_lake
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Machinery, "lava_lake",
            "A lake of molten stone stretches before you.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Machinery, "lava_lake",
            "Stone platforms rise from the surface. A path exists, barely.", revealsHint: true);
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Machinery, "lava_lake",
            "The lake responds to movement. Step carefully or it will surge.");

        // freezing_mist
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Machinery, "freezing_mist",
            "Cold mist limits visibility and steals warmth.");
        yield return ExaminationDescriptor.CreateDetailed(
            ObjectCategory.Machinery, "freezing_mist",
            "Stay low. The mist rises. Near the floor, you can see and breathe.", revealsHint: true);
        yield return ExaminationDescriptor.CreateExpert(
            ObjectCategory.Machinery, "freezing_mist",
            "The mist is a defense mechanism. Something doesn't want to be found.");
    }

    #endregion

    #region Universal Structural Elements

    private static IEnumerable<ExaminationDescriptor> GetUniversalStructuralDescriptors()
    {
        // Walls by biome
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "walls",
            "Fitted stone blocks, laid with precision. Ancient but solid.",
            Biome.Citadel);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "walls",
            "Intertwined roots form living walls. They shift subtly.",
            Biome.TheRoots);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "walls",
            "Black volcanic glass, still warm to the touch.",
            Biome.Muspelheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "walls",
            "Ice several feet thick. Shadows move within.",
            Biome.Niflheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "walls",
            "Massive blocks sized for titans. You feel small.",
            Biome.Jotunheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "walls",
            "Crystalline formations catch and scatter light impossibly.",
            Biome.Alfheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "walls",
            "Weathered stone shows signs of the Glitch's corruption.",
            Biome.Surface);

        // Also support "wall" singular
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "wall",
            "Fitted stone blocks, laid with precision. Ancient but solid.",
            Biome.Citadel);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "wall",
            "Intertwined roots form a living wall. It shifts subtly.",
            Biome.TheRoots);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "wall",
            "Black volcanic glass, still warm to the touch.",
            Biome.Muspelheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "wall",
            "Ice several feet thick. Shadows move within.",
            Biome.Niflheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "wall",
            "Massive blocks sized for titans. You feel small.",
            Biome.Jotunheim);

        // Floor by biome
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "floor",
            "Worn flagstones, grooved by centuries of footsteps.",
            Biome.Citadel);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "floor",
            "Soft humus and fungal growth cushion each step.",
            Biome.TheRoots);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "floor",
            "Cracked obsidian, glowing red between the fractures.",
            Biome.Muspelheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "floor",
            "Frozen solid. Your breath crystallizes instantly.",
            Biome.Niflheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "floor",
            "Flagstones as large as houses. Giant-scale construction.",
            Biome.Jotunheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "floor",
            "Crystalline tiles pulse with inner light.",
            Biome.Alfheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "floor",
            "Cracked earth showing signs of corruption.",
            Biome.Surface);

        // Also support "ground"
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ground",
            "Worn flagstones, grooved by centuries of footsteps.",
            Biome.Citadel);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ground",
            "Soft humus and fungal growth cushion each step.",
            Biome.TheRoots);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ground",
            "Cracked obsidian, glowing red between the fractures.",
            Biome.Muspelheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ground",
            "Frozen solid. Your breath crystallizes instantly.",
            Biome.Niflheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ground",
            "Flagstones as large as houses. Giant-scale construction.",
            Biome.Jotunheim);

        // Ceiling by biome
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ceiling",
            "Vaulted stone overhead, lost in shadows.",
            Biome.Citadel);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ceiling",
            "Roots tangle overhead like grasping fingers.",
            Biome.TheRoots);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ceiling",
            "Smoke-stained stone, reflecting hellish light.",
            Biome.Muspelheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ceiling",
            "Icicles hang like frozen daggers.",
            Biome.Niflheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ceiling",
            "So high it's lost in darkness. Built for colossi.",
            Biome.Jotunheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ceiling",
            "Crystal formations create a glittering canopy.",
            Biome.Alfheim);
        yield return ExaminationDescriptor.CreateCursory(
            ObjectCategory.Structural, "ceiling",
            "Open to the corrupted sky above.",
            Biome.Surface);
    }

    #endregion
}
