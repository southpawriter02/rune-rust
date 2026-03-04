using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds room templates for procedural dungeon generation.
/// Templates are organized by biome and archetype.
/// </summary>
public static class RoomTemplateSeeder
{
    /// <summary>
    /// Gets all seeded room templates.
    /// </summary>
    public static IEnumerable<RoomTemplate> GetAllTemplates()
    {
        return GetCitadelTemplates()
            .Concat(GetTheRootsTemplates())
            .Concat(GetMuspelheimTemplates())
            .Concat(GetNiflheimTemplates())
            .Concat(GetJotunheimTemplates());
    }

    /// <summary>
    /// Gets templates filtered by biome.
    /// </summary>
    public static IEnumerable<RoomTemplate> GetTemplatesByBiome(Biome biome)
    {
        return GetAllTemplates().Where(t => t.Biome == biome);
    }

    #region Citadel Templates

    private static IEnumerable<RoomTemplate> GetCitadelTemplates()
    {
        // Corridors
        var corridor1 = new RoomTemplate(
            "citadel_corridor_01",
            "Stone Passage",
            RoomArchetype.Corridor,
            Biome.Citadel,
            "A {ADJ_SIZE} corridor of fitted stone blocks stretches before you. Torchlight from ancient sconces casts {ADJ_ATMOSPHERE} shadows on the walls.",
            minExits: 2, maxExits: 2, weight: 3);
        corridor1.AddTags(["Stone", "Ancient"]);
        yield return corridor1;

        var corridor2 = new RoomTemplate(
            "citadel_corridor_02",
            "Crumbling Hall",
            RoomArchetype.Corridor,
            Biome.Citadel,
            "The ceiling here has partially collapsed. Rubble lines the {ADJ_CONDITION} floor, and cold air seeps through cracks above.",
            minExits: 2, maxExits: 2, weight: 2);
        corridor2.AddTags(["Damaged", "Cold", "Debris"]);
        yield return corridor2;

        // Chambers
        var chamber1 = new RoomTemplate(
            "citadel_chamber_01",
            "Guard Room",
            RoomArchetype.Chamber,
            Biome.Citadel,
            "A {ADJ_SIZE} chamber that once served as a guard post. Weapon racks line the walls, most now empty. A {ADJ_CONDITION} table sits in the center.",
            minExits: 2, maxExits: 3, weight: 2);
        chamber1.AddTags(["Military", "Functional"]);
        chamber1.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "weapon_rack", 0.6));
        yield return chamber1;

        var chamber2 = new RoomTemplate(
            "citadel_chamber_02",
            "Ancient Library",
            RoomArchetype.Chamber,
            Biome.Citadel,
            "Towering bookshelves dominate this {ADJ_SIZE} room. Most volumes have crumbled to dust, but some fragments remain. The air smells of old paper and forgotten knowledge.",
            minExits: 1, maxExits: 2, weight: 1);
        chamber2.AddTags(["Scholarly", "Dusty", "Quiet"]);
        chamber2.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "bookshelf", 0.8));
        chamber2.AddFeature(new RoomFeature(RoomFeatureType.LightSource, "reading_lamp", 0.3));
        yield return chamber2;

        var chamber3 = new RoomTemplate(
            "citadel_chamber_03",
            "Armory",
            RoomArchetype.Chamber,
            Biome.Citadel,
            "This {ADJ_SIZE} room once stored weapons and armor. The walls are lined with empty weapon mounts. A {ADJ_CONDITION} anvil suggests repairs were made here.",
            minExits: 1, maxExits: 2, weight: 2);
        chamber3.AddTags(["Military", "Storage"]);
        chamber3.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "weapon_crate", 0.5));
        yield return chamber3;

        // Junctions
        var junction1 = new RoomTemplate(
            "citadel_junction_01",
            "Crossroads",
            RoomArchetype.Junction,
            Biome.Citadel,
            "A {ADJ_SIZE} intersection where multiple passages meet. Worn stone tiles show the passage of countless feet. Faded directional carvings mark the walls.",
            minExits: 3, maxExits: 4, weight: 3);
        junction1.AddTags(["Central", "Worn"]);
        yield return junction1;

        var junction2 = new RoomTemplate(
            "citadel_junction_02",
            "Fountain Court",
            RoomArchetype.Junction,
            Biome.Citadel,
            "An open courtyard with a {ADJ_CONDITION} fountain at its center. The basin is dry now, choked with debris. Multiple archways lead away.",
            minExits: 3, maxExits: 4, weight: 2);
        junction2.AddTags(["Open", "Wet"]);
        junction2.AddFeature(new RoomFeature(RoomFeatureType.Decoration, "broken_fountain", 1.0));
        yield return junction2;

        // Dead Ends
        var deadend1 = new RoomTemplate(
            "citadel_deadend_01",
            "Storage Alcove",
            RoomArchetype.DeadEnd,
            Biome.Citadel,
            "A small {ADJ_CONDITION} alcove, likely used for storage. Empty shelves line the walls. A thin layer of dust covers everything.",
            minExits: 1, maxExits: 1, weight: 2);
        deadend1.AddTags(["Small", "Dusty"]);
        deadend1.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "old_crate", 0.4));
        yield return deadend1;

        var deadend2 = new RoomTemplate(
            "citadel_deadend_02",
            "Shrine Niche",
            RoomArchetype.DeadEnd,
            Biome.Citadel,
            "A quiet alcove housing a {ADJ_CONDITION} shrine to forgotten gods. Votive offerings have long since been looted, but the sacred space remains.",
            minExits: 1, maxExits: 1, weight: 1);
        deadend2.AddTags(["Sacred", "Quiet"]);
        yield return deadend2;

        // Stairwell
        var stairs1 = new RoomTemplate(
            "citadel_stairwell_01",
            "Spiral Staircase",
            RoomArchetype.Stairwell,
            Biome.Citadel,
            "A {ADJ_SIZE} spiral staircase carved into the rock itself. The steps are worn smooth by ages of use. Darkness waits above and below.",
            minExits: 2, maxExits: 3, weight: 2);
        stairs1.AddTags(["Vertical", "Ancient"]);
        yield return stairs1;

        // Boss Arena
        var boss1 = new RoomTemplate(
            "citadel_boss_01",
            "Throne Hall",
            RoomArchetype.BossArena,
            Biome.Citadel,
            "A {ADJ_SIZE} hall of ancient power. Crumbling pillars support a vaulted ceiling lost in shadow. At the far end, a massive throne of black stone dominates the chamber. Something waits here.",
            minExits: 1, maxExits: 2, weight: 1);
        boss1.AddTags(["Grand", "Dark", "Ominous"]);
        boss1.AddFeature(new RoomFeature(RoomFeatureType.Decoration, "throne", 1.0));
        boss1.AddFeature(new RoomFeature(RoomFeatureType.LightSource, "brazier", 0.8));
        yield return boss1;
    }

    #endregion

    #region The Roots Templates

    private static IEnumerable<RoomTemplate> GetTheRootsTemplates()
    {
        // Corridors
        var corridor1 = new RoomTemplate(
            "roots_corridor_01",
            "Fungal Tunnel",
            RoomArchetype.Corridor,
            Biome.TheRoots,
            "A {ADJ_SIZE} tunnel lined with bioluminescent fungi. Their pale glow pulses slowly, like breathing. The air is thick with spores.",
            minExits: 2, maxExits: 2, weight: 3);
        corridor1.AddTags(["Organic", "Wet", "Dim"]);
        corridor1.AddFeature(new RoomFeature(RoomFeatureType.LightSource, "glowing_fungus", 0.9));
        yield return corridor1;

        var corridor2 = new RoomTemplate(
            "roots_corridor_02",
            "Root Network",
            RoomArchetype.Corridor,
            Biome.TheRoots,
            "Massive roots form the walls of this passage, intertwined like grasping fingers. The wood creaks and shifts as you move.",
            minExits: 2, maxExits: 2, weight: 2);
        corridor2.AddTags(["Organic", "Creaking"]);
        yield return corridor2;

        // Chambers
        var chamber1 = new RoomTemplate(
            "roots_chamber_01",
            "Spore Den",
            RoomArchetype.Chamber,
            Biome.TheRoots,
            "A {ADJ_SIZE} cavern dominated by towering mushroom growths. Clouds of spores drift lazily through the {ADJ_ATMOSPHERE} air. The floor is soft with decay.",
            minExits: 2, maxExits: 3, weight: 2);
        chamber1.AddTags(["Hazardous", "Organic", "Spores"]);
        chamber1.AddFeature(new RoomFeature(RoomFeatureType.Hazard, "spore_cloud", 0.6));
        yield return chamber1;

        var chamber2 = new RoomTemplate(
            "roots_chamber_02",
            "Machine Graveyard",
            RoomArchetype.Chamber,
            Biome.TheRoots,
            "Broken machines litter this {ADJ_SIZE} chamber, half-consumed by fungal growth. The roots have claimed the Ancients' devices, making them part of the living earth.",
            minExits: 2, maxExits: 3, weight: 2);
        chamber2.AddTags(["Salvage", "Organic", "Machine"]);
        chamber2.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "rusted_terminal", 0.4));
        chamber2.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "salvage_pile", 0.7));
        yield return chamber2;

        // Junction
        var junction1 = new RoomTemplate(
            "roots_junction_01",
            "Root Nexus",
            RoomArchetype.Junction,
            Biome.TheRoots,
            "A {ADJ_SIZE} hollow where great roots converge from all directions. The wood groans with ancient life. Multiple passages wind away into the darkness.",
            minExits: 3, maxExits: 4, weight: 2);
        junction1.AddTags(["Organic", "Central"]);
        yield return junction1;

        // Dead End
        var deadend1 = new RoomTemplate(
            "roots_deadend_01",
            "Fungal Bloom",
            RoomArchetype.DeadEnd,
            Biome.TheRoots,
            "The passage ends in a wall of {ADJ_CONDITION} fungal growth. Shelf mushrooms spiral up into darkness. Something valuable might grow here.",
            minExits: 1, maxExits: 1, weight: 2);
        deadend1.AddTags(["Organic", "Resource"]);
        deadend1.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "harvestable_fungus", 0.5));
        yield return deadend1;

        // Stairwell
        var stairs1 = new RoomTemplate(
            "roots_stairwell_01",
            "Root Descent",
            RoomArchetype.Stairwell,
            Biome.TheRoots,
            "Massive roots form a natural staircase, spiraling down into deeper darkness. The air grows colder and damper with each level.",
            minExits: 2, maxExits: 3, weight: 2);
        stairs1.AddTags(["Organic", "Vertical", "Wet"]);
        yield return stairs1;

        // Boss Arena
        var boss1 = new RoomTemplate(
            "roots_boss_01",
            "The Heart Chamber",
            RoomArchetype.BossArena,
            Biome.TheRoots,
            "A vast {ADJ_SIZE} cavern pulsing with organic light. At its center, a massive fungal growth beats like a living heart, tendrils extending to every wall. The apex predator of The Roots awaits.",
            minExits: 1, maxExits: 2, weight: 1);
        boss1.AddTags(["Organic", "Pulsing", "Ominous"]);
        boss1.AddFeature(new RoomFeature(RoomFeatureType.Decoration, "heart_fungus", 1.0));
        yield return boss1;
    }

    #endregion

    #region Muspelheim Templates

    private static IEnumerable<RoomTemplate> GetMuspelheimTemplates()
    {
        // Corridors
        var corridor1 = new RoomTemplate(
            "muspel_corridor_01",
            "Magma Tube",
            RoomArchetype.Corridor,
            Biome.Muspelheim,
            "A {ADJ_SIZE} tunnel carved by flowing lava. The walls still radiate heat, and orange light glows from cracks in the floor.",
            minExits: 2, maxExits: 2, weight: 3);
        corridor1.AddTags(["Hot", "Volcanic", "Dangerous"]);
        corridor1.AddFeature(new RoomFeature(RoomFeatureType.Hazard, "heat_vent", 0.4));
        yield return corridor1;

        var corridor2 = new RoomTemplate(
            "muspel_corridor_02",
            "Obsidian Passage",
            RoomArchetype.Corridor,
            Biome.Muspelheim,
            "Black volcanic glass lines this {ADJ_SIZE} passage. Your reflection follows you in shattered fragments. The air shimmers with heat.",
            minExits: 2, maxExits: 2, weight: 2);
        corridor2.AddTags(["Hot", "Sharp", "Reflective"]);
        yield return corridor2;

        // Chamber
        var chamber1 = new RoomTemplate(
            "muspel_chamber_01",
            "Forge Hall",
            RoomArchetype.Chamber,
            Biome.Muspelheim,
            "An ancient forge dominates this {ADJ_SIZE} chamber. The fire-pit still glows with unnatural heat. Anvils and cooling troughs suggest this was a place of creation.",
            minExits: 2, maxExits: 3, weight: 2);
        chamber1.AddTags(["Hot", "Industrial", "Ancient"]);
        chamber1.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "ancient_forge", 0.8));
        yield return chamber1;

        // Junction
        var junction1 = new RoomTemplate(
            "muspel_junction_01",
            "Lava Crossroads",
            RoomArchetype.Junction,
            Biome.Muspelheim,
            "A {ADJ_SIZE} intersection where lava flows meet. Stone bridges cross pools of molten rock. The heat is nearly unbearable.",
            minExits: 3, maxExits: 4, weight: 2);
        junction1.AddTags(["Hot", "Dangerous", "Central"]);
        junction1.AddFeature(new RoomFeature(RoomFeatureType.Hazard, "lava_pool", 0.7));
        yield return junction1;

        // Dead End
        var deadend1 = new RoomTemplate(
            "muspel_deadend_01",
            "Cooling Chamber",
            RoomArchetype.DeadEnd,
            Biome.Muspelheim,
            "A small chamber where lava once pooled before cooling. The floor is a pattern of cracked obsidian. {ADJ_CONDITION} mineral deposits line the walls.",
            minExits: 1, maxExits: 1, weight: 2);
        deadend1.AddTags(["Hot", "Resource"]);
        deadend1.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "mineral_vein", 0.5));
        yield return deadend1;

        // Boss Arena
        var boss1 = new RoomTemplate(
            "muspel_boss_01",
            "The Crucible",
            RoomArchetype.BossArena,
            Biome.Muspelheim,
            "A {ADJ_SIZE} chamber of living fire. Lava falls cascade from above into a lake of molten stone. At the center, an island of black rock rises from the inferno. The guardian of flame awaits.",
            minExits: 1, maxExits: 2, weight: 1);
        boss1.AddTags(["Hot", "Volcanic", "Ominous"]);
        boss1.AddFeature(new RoomFeature(RoomFeatureType.Hazard, "lava_lake", 1.0));
        yield return boss1;
    }

    #endregion

    #region Niflheim Templates

    private static IEnumerable<RoomTemplate> GetNiflheimTemplates()
    {
        // Corridors
        var corridor1 = new RoomTemplate(
            "nifl_corridor_01",
            "Frozen Passage",
            RoomArchetype.Corridor,
            Biome.Niflheim,
            "A {ADJ_SIZE} corridor encased in ice. Frost patterns crawl across every surface. Your breath hangs in the air like ghosts.",
            minExits: 2, maxExits: 2, weight: 3);
        corridor1.AddTags(["Cold", "Ice", "Slippery"]);
        yield return corridor1;

        var corridor2 = new RoomTemplate(
            "nifl_corridor_02",
            "Mist Tunnel",
            RoomArchetype.Corridor,
            Biome.Niflheim,
            "Cold mist fills this {ADJ_SIZE} passage, limiting visibility to arm's reach. Ice crystals form on everything, including you.",
            minExits: 2, maxExits: 2, weight: 2);
        corridor2.AddTags(["Cold", "Mist", "LowVisibility"]);
        corridor2.AddFeature(new RoomFeature(RoomFeatureType.Hazard, "freezing_mist", 0.6));
        yield return corridor2;

        // Chamber
        var chamber1 = new RoomTemplate(
            "nifl_chamber_01",
            "Ice Tomb",
            RoomArchetype.Chamber,
            Biome.Niflheim,
            "A {ADJ_SIZE} chamber of perfect ice. Frozen figures stand within the walls, preserved for eternity. Their expressions are peaceful.",
            minExits: 2, maxExits: 3, weight: 2);
        chamber1.AddTags(["Cold", "Tomb", "Quiet"]);
        chamber1.AddFeature(new RoomFeature(RoomFeatureType.Decoration, "frozen_corpse", 0.8));
        yield return chamber1;

        // Junction
        var junction1 = new RoomTemplate(
            "nifl_junction_01",
            "Crystal Nexus",
            RoomArchetype.Junction,
            Biome.Niflheim,
            "A {ADJ_SIZE} chamber where massive ice crystals converge. Light refracts through them, creating dancing rainbows in the frozen air.",
            minExits: 3, maxExits: 4, weight: 2);
        junction1.AddTags(["Cold", "Beautiful", "Central"]);
        junction1.AddFeature(new RoomFeature(RoomFeatureType.LightSource, "ice_crystal", 0.9));
        yield return junction1;

        // Dead End
        var deadend1 = new RoomTemplate(
            "nifl_deadend_01",
            "Frozen Alcove",
            RoomArchetype.DeadEnd,
            Biome.Niflheim,
            "A small {ADJ_CONDITION} alcove where ice has grown thick. Something glitters within the frozen wall, preserved and waiting.",
            minExits: 1, maxExits: 1, weight: 2);
        deadend1.AddTags(["Cold", "Treasure"]);
        deadend1.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "ice_encased_item", 0.5));
        yield return deadend1;

        // Boss Arena
        var boss1 = new RoomTemplate(
            "nifl_boss_01",
            "The Frosthold",
            RoomArchetype.BossArena,
            Biome.Niflheim,
            "A {ADJ_SIZE} throne room of eternal ice. The cold here is absolute, stealing warmth and hope alike. Upon a throne of frozen tears sits the master of this realm, patient as winter itself.",
            minExits: 1, maxExits: 2, weight: 1);
        boss1.AddTags(["Cold", "Grand", "Ominous"]);
        boss1.AddFeature(new RoomFeature(RoomFeatureType.Decoration, "ice_throne", 1.0));
        yield return boss1;
    }

    #endregion

    #region Jotunheim Templates

    private static IEnumerable<RoomTemplate> GetJotunheimTemplates()
    {
        // Corridors
        var corridor1 = new RoomTemplate(
            "jotun_corridor_01",
            "Giant's Passage",
            RoomArchetype.Corridor,
            Biome.Jotunheim,
            "A {ADJ_SIZE} corridor built for beings far larger than humans. The ceiling soars overhead. Your footsteps echo in the vast space.",
            minExits: 2, maxExits: 2, weight: 3);
        corridor1.AddTags(["Massive", "Ancient", "Echo"]);
        yield return corridor1;

        var corridor2 = new RoomTemplate(
            "jotun_corridor_02",
            "Rune Hall",
            RoomArchetype.Corridor,
            Biome.Jotunheim,
            "Giant runes cover every surface of this {ADJ_SIZE} passage. Some still glow with fading power. The language of the Jotun remains incomprehensible.",
            minExits: 2, maxExits: 2, weight: 2);
        corridor2.AddTags(["Runic", "Ancient", "Magical"]);
        corridor2.AddFeature(new RoomFeature(RoomFeatureType.Decoration, "glowing_runes", 0.7));
        yield return corridor2;

        // Chamber
        var chamber1 = new RoomTemplate(
            "jotun_chamber_01",
            "Titan's Feast Hall",
            RoomArchetype.Chamber,
            Biome.Jotunheim,
            "A {ADJ_SIZE} dining hall built for giants. Tables the size of houses line the room. Whatever feast was held here ended long ago.",
            minExits: 2, maxExits: 3, weight: 2);
        chamber1.AddTags(["Massive", "Ancient", "Feast"]);
        chamber1.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "giant_table", 0.6));
        yield return chamber1;

        // Junction
        var junction1 = new RoomTemplate(
            "jotun_junction_01",
            "Colossus Crossroads",
            RoomArchetype.Junction,
            Biome.Jotunheim,
            "A {ADJ_SIZE} intersection where passages built for titans meet. Broken statues of the Jotun stand guard at each exit, their faces worn but still proud.",
            minExits: 3, maxExits: 4, weight: 2);
        junction1.AddTags(["Massive", "Central", "Statues"]);
        junction1.AddFeature(new RoomFeature(RoomFeatureType.Decoration, "broken_statue", 0.8));
        yield return junction1;

        // Dead End
        var deadend1 = new RoomTemplate(
            "jotun_deadend_01",
            "Reliquary",
            RoomArchetype.DeadEnd,
            Biome.Jotunheim,
            "A {ADJ_CONDITION} chamber holding relics of the giants. Even the smallest artifacts here are enormous. Power still lingers in these ancient objects.",
            minExits: 1, maxExits: 1, weight: 2);
        deadend1.AddTags(["Massive", "Sacred", "Treasure"]);
        deadend1.AddFeature(new RoomFeature(RoomFeatureType.Interactable, "giant_artifact", 0.6));
        yield return deadend1;

        // Boss Arena
        var boss1 = new RoomTemplate(
            "jotun_boss_01",
            "The Titan's Rest",
            RoomArchetype.BossArena,
            Biome.Jotunheim,
            "A {ADJ_SIZE} burial chamber of impossible scale. At its heart lies a sarcophagus that could hold a mountain. The lid has been moved. Whatever slumbered within has awakened.",
            minExits: 1, maxExits: 2, weight: 1);
        boss1.AddTags(["Massive", "Tomb", "Ominous"]);
        boss1.AddFeature(new RoomFeature(RoomFeatureType.Decoration, "giant_sarcophagus", 1.0));
        yield return boss1;
    }

    #endregion
}
