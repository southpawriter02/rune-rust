using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds entity templates for the threat budget population system.
/// Entities are organized by biome and faction.
/// </summary>
public static class EntityTableSeeder
{
    /// <summary>
    /// Gets all seeded entity templates.
    /// </summary>
    public static IEnumerable<EntityTemplate> GetAllTemplates()
    {
        return GetCitadelEntities()
            .Concat(GetTheRootsEntities())
            .Concat(GetMuspelheimEntities())
            .Concat(GetNiflheimEntities())
            .Concat(GetJotunheimEntities());
    }

    /// <summary>
    /// Gets templates filtered by biome.
    /// </summary>
    public static IEnumerable<EntityTemplate> GetTemplatesByBiome(Biome biome)
    {
        return GetAllTemplates().Where(t => t.Biome == biome);
    }

    /// <summary>
    /// Gets templates filtered by faction.
    /// </summary>
    public static IEnumerable<EntityTemplate> GetTemplatesByFaction(string factionId)
    {
        return GetAllTemplates().Where(t => t.BelongsToFaction(factionId));
    }

    #region Citadel Entities - Undead Faction

    private static IEnumerable<EntityTemplate> GetCitadelEntities()
    {
        const string factionId = "undead_legion";

        // Swarm units (cost 1-5)
        var skeleton = EntityTemplate.CreateSwarm(
            "skeleton_minion", "Skeleton Minion",
            "A clattering assembly of old bones, bound by malice and rusted wire. It moves with a jerky, puppet-like gait, seeking to share its death.",
            factionId, Biome.Citadel,
            cost: 3, new Stats(15, 5, 2, 5));
        skeleton.AddTags(["Undead", "Brittle", "Melee"]);
        yield return skeleton;

        var ghostLight = EntityTemplate.CreateSwarm(
            "ghost_light", "Ghost Light",
            "A flickering sphere of cold, pale fire that drifts through the dark. It smells of ozone and old graves, leading the foolish to their doom.",
            factionId, Biome.Citadel,
            cost: 2, new Stats(8, 3, 0, 8));
        ghostLight.AddTags(["Undead", "Flying", "Glowing"]);
        yield return ghostLight;

        // Grunt units (cost 5-40)
        var skeletonWarrior = EntityTemplate.CreateGrunt(
            "skeleton_warrior", "Skeleton Warrior",
            "This one wears scraps of ancient iron-skin and wields a blade jagged with rust. The empty sockets of its skull burn with a hateful, distant light.",
            factionId, Biome.Citadel,
            cost: 10, EntityRole.Melee, new Stats(30, 10, 5, 6));
        skeletonWarrior.AddTags(["Undead", "Armed", "Melee"]);
        yield return skeletonWarrior;

        var skeletonArcher = EntityTemplate.CreateGrunt(
            "skeleton_archer", "Skeleton Archer",
            "Its bow is strung with dried sinew, its arrows tipped with scrap-metal. It draws with the patience of the grave, the wood creaking like a coffin lid.",
            factionId, Biome.Citadel,
            cost: 12, EntityRole.Ranged, new Stats(20, 12, 3, 8));
        skeletonArcher.AddTags(["Undead", "Armed", "Ranged"]);
        yield return skeletonArcher;

        var wight = EntityTemplate.CreateGrunt(
            "wight", "Barrow Wight",
            "A bloating horror shrouded in grave-mold and tattered finery. The air around it grows impossibly cold, tasting of wet earth and stagnation.",
            factionId, Biome.Citadel,
            cost: 25, EntityRole.Tank, new Stats(50, 12, 10, 10));
        wight.AddTags(["Undead", "Heavy", "Fearsome"]);
        yield return wight;

        // Elite unit (cost 40+)
        var deathKnight = EntityTemplate.CreateElite(
            "death_knight", "Death Knight",
            "A towering figure encased in black iron-skin, etched with runes that hurt the eyes. Its blade hums with a deep, subsonic dread.",
            factionId, Biome.Citadel,
            cost: 60, new Stats(80, 18, 15, 12));
        deathKnight.AddTags(["Undead", "Heavy", "Melee", "Commander"]);
        yield return deathKnight;

        // Boss unit (cost 100+)
        var lichLord = EntityTemplate.CreateBoss(
            "lich_lord", "Lich Lord",
            "A withered husk floating on currents of necrotic wind. It whispers words that make the skin crawl, commanding the dead with gestures of its rot-blackened hands.",
            factionId, Biome.Citadel,
            cost: 150, new Stats(120, 25, 12, 18));
        lichLord.AddTags(["Undead", "Magical", "Ranged", "Commander"]);
        yield return lichLord;
    }

    #endregion

    #region The Roots Entities - Rust Swarm Faction

    private static IEnumerable<EntityTemplate> GetTheRootsEntities()
    {
        const string factionId = "rust_swarm";

        // Swarm units
        var rustMite = EntityTemplate.CreateSwarm(
            "rust_mite", "Rust Mite",
            "A thumb-sized horror of twitching legs and grinding mandibles. It feeds on the iron-bones of the world, leaving only red dust in its wake.",
            factionId, Biome.TheRoots,
            cost: 2, new Stats(10, 4, 1, 4));
        rustMite.AddTags(["Mechanical", "Swarm", "Corrosive"]);
        yield return rustMite;

        var sporeCloud = EntityTemplate.CreateSwarm(
            "spore_cloud", "Spore Cloud",
            "A drifting haze of choking dust and viral life. To breathe it is to invite the garden into your lungs.",
            factionId, Biome.TheRoots,
            cost: 3, new Stats(12, 3, 0, 6));
        sporeCloud.AddTags(["Organic", "Flying", "Toxic"]);
        yield return sporeCloud;

        // Grunt units
        var scavengerDrone = EntityTemplate.CreateGrunt(
            "scavenger_drone", "Iron-Beak Scavenger",
            "A small, flying machine with a single glass eye and grasping claws. It flits through the shadows like a bat made of scrap, hunting for fresh parts.",
            factionId, Biome.TheRoots,
            cost: 15, EntityRole.Melee, new Stats(35, 12, 6, 8));
        scavengerDrone.AddTags(["Mechanical", "Salvager", "Melee"]);
        yield return scavengerDrone;

        var fungalStalker = EntityTemplate.CreateGrunt(
            "fungal_stalker", "Fungal Stalker",
            "Once human, now a walking hive of mushrooms and moss. It moves in silence, its footsteps muffled by the soft rot that covers its body.",
            factionId, Biome.TheRoots,
            cost: 18, EntityRole.Melee, new Stats(40, 14, 5, 12));
        fungalStalker.AddTags(["Organic", "Stealthy", "Melee"]);
        yield return fungalStalker;

        var spitterNode = EntityTemplate.CreateGrunt(
            "spitter_node", "Spittle-Mound",
            "A pulsating growth rooted to the floor, resembling a boil on the world's skin. It heaves and coughs, launching globs of burning acid at intruders.",
            factionId, Biome.TheRoots,
            cost: 20, EntityRole.Ranged, new Stats(30, 15, 4, 6));
        spitterNode.AddTags(["Organic", "Stationary", "Ranged", "Toxic"]);
        yield return spitterNode;

        // Elite unit
        var heavyLoader = EntityTemplate.CreateElite(
            "heavy_loader", "Titan-Lifter",
            "A massive, hunched iron-walker, built for labor but repurposed for violence. Its hydraulic limbs hiss and groan as it crushes stone and bone alike.",
            factionId, Biome.TheRoots,
            cost: 55, new Stats(90, 20, 18, 6));
        heavyLoader.AddTags(["Mechanical", "Heavy", "Melee", "Armored"]);
        yield return heavyLoader;

        // Boss unit
        var rootMother = EntityTemplate.CreateBoss(
            "root_mother", "The Root Mother",
            "A colossal mass of tangled vines and weeping sores, fused into the very architecture. She screams with the voices of a thousand consumed souls.",
            factionId, Biome.TheRoots,
            cost: 140, new Stats(150, 22, 15, 14));
        rootMother.AddTags(["Organic", "Massive", "Spawner", "Commander"]);
        yield return rootMother;
    }

    #endregion

    #region Muspelheim Entities - Fire Cult Faction

    private static IEnumerable<EntityTemplate> GetMuspelheimEntities()
    {
        const string factionId = "fire_cult";

        // Swarm units
        var fireImp = EntityTemplate.CreateSwarm(
            "fire_imp", "Fire Imp",
            "A small, cackling spirit composed of soot and spark. It darts about like a stray ember, eager to set the world alight.",
            factionId, Biome.Muspelheim,
            cost: 4, new Stats(12, 6, 2, 10));
        fireImp.AddTags(["Fire", "Small", "Ranged"]);
        yield return fireImp;

        var cinder = EntityTemplate.CreateSwarm(
            "cinder", "Living Cinder",
            "A floating flake of burning ash, driven by a hateful will. It glows with a dull, throbbing heat, seeking fuel for its hunger.",
            factionId, Biome.Muspelheim,
            cost: 3, new Stats(8, 5, 0, 8));
        cinder.AddTags(["Fire", "Flying", "Explosive"]);
        yield return cinder;

        // Grunt units
        var flameCultist = EntityTemplate.CreateGrunt(
            "flame_cultist", "Flame Cultist",
            "A fanatic wrapped in scorched leathers, skin branded with the signs of the burning god. They chant prayers that smell of gasoline and sulfur.",
            factionId, Biome.Muspelheim,
            cost: 12, EntityRole.Ranged, new Stats(25, 12, 4, 10));
        flameCultist.AddTags(["Human", "Fire", "Ranged"]);
        yield return flameCultist;

        var magmaElemental = EntityTemplate.CreateGrunt(
            "magma_elemental", "Magma Elemental",
            "A lumbering construct of cooling slag and liquid fire. The heat radiating from it distorts the air, promising a slow, melting death.",
            factionId, Biome.Muspelheim,
            cost: 30, EntityRole.Tank, new Stats(60, 15, 12, 6));
        magmaElemental.AddTags(["Elemental", "Fire", "Heavy", "Melee"]);
        yield return magmaElemental;

        var ashWraith = EntityTemplate.CreateGrunt(
            "ash_wraith", "Ash Wraith",
            "A phantom shape formed from the smoke of funeral pyres. It chokes the light and silence, leaving only the taste of dust in the mouth.",
            factionId, Biome.Muspelheim,
            cost: 22, EntityRole.Melee, new Stats(40, 16, 6, 12));
        ashWraith.AddTags(["Undead", "Fire", "Phasing", "Melee"]);
        yield return ashWraith;

        // Elite unit
        var flameWarden = EntityTemplate.CreateElite(
            "flame_warden", "Flame Warden",
            "A champion of the cult, armored in plates of blackened steel that still hold the forge's heat. Their weapon is a torch of unquenchable chemical fire.",
            factionId, Biome.Muspelheim,
            cost: 65, new Stats(85, 22, 14, 14));
        flameWarden.AddTags(["Human", "Fire", "Heavy", "Melee", "Commander"]);
        yield return flameWarden;

        // Boss unit
        var emberKing = EntityTemplate.CreateBoss(
            "ember_king", "The Ember King",
            "A towering lord of ruin, wreathed in a crown of perpetual explosion. His laughter is the sound of a collapsing building, his gaze the flash of a bomb.",
            factionId, Biome.Muspelheim,
            cost: 160, new Stats(140, 28, 18, 16));
        emberKing.AddTags(["Elemental", "Fire", "Massive", "Ranged", "Commander"]);
        yield return emberKing;
    }

    #endregion

    #region Niflheim Entities - Frost Horrors Faction

    private static IEnumerable<EntityTemplate> GetNiflheimEntities()
    {
        const string factionId = "frost_horrors";

        // Swarm units
        var iceSprite = EntityTemplate.CreateSwarm(
            "ice_sprite", "Ice Sprite",
            "A jagged shard of living crystal that flits on the freezing wind. Its touch burns with a cold so deep it stops the blood.",
            factionId, Biome.Niflheim,
            cost: 3, new Stats(10, 4, 1, 12));
        iceSprite.AddTags(["Cold", "Small", "Flying", "Freezing"]);
        yield return iceSprite;

        var frostWorm = EntityTemplate.CreateSwarm(
            "frost_worm", "Frost Worm",
            "A pale, segmented thing that burrows through the frozen earth. It erupts from the ground with a spray of ice, mandibles clicking.",
            factionId, Biome.Niflheim,
            cost: 4, new Stats(15, 5, 3, 4));
        frostWorm.AddTags(["Cold", "Burrowing", "Melee"]);
        yield return frostWorm;

        // Grunt units
        var frozenCorpse = EntityTemplate.CreateGrunt(
            "frozen_corpse", "Frozen Corpse",
            "A victim of the cold, perfectly preserved in a shell of blue rime. It shambles forward, stiff and unyielding, driven by a frozen hunger.",
            factionId, Biome.Niflheim,
            cost: 14, EntityRole.Melee, new Stats(35, 10, 8, 5));
        frozenCorpse.AddTags(["Undead", "Cold", "Slow", "Melee"]);
        yield return frozenCorpse;

        var iceWraith = EntityTemplate.CreateGrunt(
            "ice_wraith", "Ice Wraith",
            "A translucent spirit that shimmers like heat-haze over snow. It passes through walls and armor alike, leaving frostbite on the soul.",
            factionId, Biome.Niflheim,
            cost: 20, EntityRole.Melee, new Stats(30, 14, 4, 14));
        iceWraith.AddTags(["Undead", "Cold", "Phasing", "Melee", "Freezing"]);
        yield return iceWraith;

        var frostArcher = EntityTemplate.CreateGrunt(
            "frost_archer", "Frost Archer",
            "A hunter clad in furs stiff with ice. It waits in the blizzard, invisible until its shaft of black ice strikes home.",
            factionId, Biome.Niflheim,
            cost: 16, EntityRole.Ranged, new Stats(25, 13, 5, 10));
        frostArcher.AddTags(["Human", "Cold", "Ranged", "Freezing"]);
        yield return frostArcher;

        // Elite unit
        var glacialBehemoth = EntityTemplate.CreateElite(
            "glacial_behemoth", "Glacial Behemoth",
            "A mountain of muscle and shaggy white fur, caked in glaciers. Its roar shakes the icicles from the ceiling and cracks the floor.",
            factionId, Biome.Niflheim,
            cost: 70, new Stats(100, 18, 20, 6));
        glacialBehemoth.AddTags(["Elemental", "Cold", "Massive", "Melee", "Armored"]);
        yield return glacialBehemoth;

        // Boss unit
        var frostQueen = EntityTemplate.CreateBoss(
            "frost_queen", "The Frost Queen",
            "An ancient sorceress whose heart froze centuries ago. She sits upon a throne of black ice, commanding the winter to scour the world clean.",
            factionId, Biome.Niflheim,
            cost: 155, new Stats(130, 24, 16, 18));
        frostQueen.AddTags(["Undead", "Cold", "Magical", "Ranged", "Commander"]);
        yield return frostQueen;
    }

    #endregion

    #region Jotunheim Entities - Awakened Titans Faction

    private static IEnumerable<EntityTemplate> GetJotunheimEntities()
    {
        const string factionId = "awakened_titans";

        // Swarm units
        var stoneSprite = EntityTemplate.CreateSwarm(
            "stone_sprite", "Stone Sprite",
            "A tumbling rock that has forgotten it is stone. It mimics the shapes of life, grinding and clicking as it rolls toward you.",
            factionId, Biome.Jotunheim,
            cost: 4, new Stats(18, 5, 5, 6));
        stoneSprite.AddTags(["Elemental", "Stone", "Small"]);
        yield return stoneSprite;

        var runeWisp = EntityTemplate.CreateSwarm(
            "rune_wisp", "Rune Wisp",
            "A fragment of ancient writing that has peeled from the wall and taken flight. It pulses with a headache-inducing light.",
            factionId, Biome.Jotunheim,
            cost: 5, new Stats(12, 7, 0, 14));
        runeWisp.AddTags(["Magical", "Flying", "Ranged"]);
        yield return runeWisp;

        // Grunt units
        var giantkin = EntityTemplate.CreateGrunt(
            "giantkin", "Giantkin Warrior",
            "A towering brute with skin like granite and a voice like a rockslide. It wields a pillar of stone as a club.",
            factionId, Biome.Jotunheim,
            cost: 25, EntityRole.Melee, new Stats(55, 16, 10, 8));
        giantkin.AddTags(["Giant", "Heavy", "Melee"]);
        yield return giantkin;

        var runeguard = EntityTemplate.CreateGrunt(
            "runeguard", "Rune Guardian",
            "An empty suit of armor animated by the blue glow of inscribed runes. It stands vigil over secrets that should have remained forgotten.",
            factionId, Biome.Jotunheim,
            cost: 30, EntityRole.Tank, new Stats(70, 12, 15, 10));
        runeguard.AddTags(["Construct", "Magical", "Armored", "Melee"]);
        yield return runeguard;

        var stormcaller = EntityTemplate.CreateGrunt(
            "stormcaller", "Storm Caller",
            "A giant who speaks the language of thunder. Lightning crackles between its fingers, smelling of ozone and burnt hair.",
            factionId, Biome.Jotunheim,
            cost: 28, EntityRole.Ranged, new Stats(40, 18, 6, 14));
        stormcaller.AddTags(["Giant", "Magical", "Ranged", "Lightning"]);
        yield return stormcaller;

        // Elite unit
        var jotunChampion = EntityTemplate.CreateElite(
            "jotun_champion", "Jotun Champion",
            "A lord among the giantkin, clad in the bones of dragons. Each step it takes is an earthquake, each blow a catastrophe.",
            factionId, Biome.Jotunheim,
            cost: 80, new Stats(120, 24, 18, 10));
        jotunChampion.AddTags(["Giant", "Heavy", "Melee", "Commander"]);
        yield return jotunChampion;

        // Boss unit
        var titanAwakened = EntityTemplate.CreateBoss(
            "titan_awakened", "Awakened Titan",
            "A god of the old world, shaken from its slumber. It is less a creature and more a walking mountain, intent on crushing the insects that woke it.",
            factionId, Biome.Jotunheim,
            cost: 200, new Stats(200, 35, 25, 12));
        titanAwakened.AddTags(["Giant", "Massive", "Ancient", "Commander"]);
        yield return titanAwakened;
    }

    #endregion
}
