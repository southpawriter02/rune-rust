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
            "skeleton_minion", "Bone-Thrall", factionId, Biome.Citadel,
            cost: 3, new Stats(15, 5, 2, 5));
        skeleton.AddTags(["Undead", "Brittle", "Melee"]);
        yield return skeleton;

        var ghostLight = EntityTemplate.CreateSwarm(
            "ghost_light", "Spirit-Wisp", factionId, Biome.Citadel,
            cost: 2, new Stats(8, 3, 0, 8));
        ghostLight.AddTags(["Undead", "Flying", "Glowing"]);
        yield return ghostLight;

        // Grunt units (cost 5-40)
        var skeletonWarrior = EntityTemplate.CreateGrunt(
            "skeleton_warrior", "Bone-Guard", factionId, Biome.Citadel,
            cost: 10, EntityRole.Melee, new Stats(30, 10, 5, 6));
        skeletonWarrior.AddTags(["Undead", "Armed", "Melee"]);
        yield return skeletonWarrior;

        var skeletonArcher = EntityTemplate.CreateGrunt(
            "skeleton_archer", "Bone-Bow", factionId, Biome.Citadel,
            cost: 12, EntityRole.Ranged, new Stats(20, 12, 3, 8));
        skeletonArcher.AddTags(["Undead", "Armed", "Ranged"]);
        yield return skeletonArcher;

        var wight = EntityTemplate.CreateGrunt(
            "wight", "Grave-Wight", factionId, Biome.Citadel,
            cost: 25, EntityRole.Tank, new Stats(50, 12, 10, 10));
        wight.AddTags(["Undead", "Heavy", "Fearsome"]);
        yield return wight;

        // Elite unit (cost 40+)
        var deathKnight = EntityTemplate.CreateElite(
            "death_knight", "Iron-Bound Captain", factionId, Biome.Citadel,
            cost: 60, new Stats(80, 18, 15, 12));
        deathKnight.AddTags(["Undead", "Heavy", "Melee", "Commander"]);
        yield return deathKnight;

        // Boss unit (cost 100+)
        var lichLord = EntityTemplate.CreateBoss(
            "lich_lord", "Dead-King", factionId, Biome.Citadel,
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
            "rust_mite", "Iron-Louse", factionId, Biome.TheRoots,
            cost: 2, new Stats(10, 4, 1, 4));
        rustMite.AddTags(["Mechanical", "Swarm", "Corrosive"]);
        yield return rustMite;

        var sporeCloud = EntityTemplate.CreateSwarm(
            "spore_cloud", "Rot-Mist", factionId, Biome.TheRoots,
            cost: 3, new Stats(12, 3, 0, 6));
        sporeCloud.AddTags(["Organic", "Flying", "Toxic"]);
        yield return sporeCloud;

        // Grunt units
        var scavengerDrone = EntityTemplate.CreateGrunt(
            "scavenger_drone", "Scrap-Hawk", factionId, Biome.TheRoots,
            cost: 15, EntityRole.Melee, new Stats(35, 12, 6, 8));
        scavengerDrone.AddTags(["Mechanical", "Salvager", "Melee"]);
        yield return scavengerDrone;

        var fungalStalker = EntityTemplate.CreateGrunt(
            "fungal_stalker", "Mold-Walker", factionId, Biome.TheRoots,
            cost: 18, EntityRole.Melee, new Stats(40, 14, 5, 12));
        fungalStalker.AddTags(["Organic", "Stealthy", "Melee"]);
        yield return fungalStalker;

        var spitterNode = EntityTemplate.CreateGrunt(
            "spitter_node", "Blight-Gland", factionId, Biome.TheRoots,
            cost: 20, EntityRole.Ranged, new Stats(30, 15, 4, 6));
        spitterNode.AddTags(["Organic", "Stationary", "Ranged", "Toxic"]);
        yield return spitterNode;

        // Elite unit
        var heavyLoader = EntityTemplate.CreateElite(
            "heavy_loader", "Titan-Lifter", factionId, Biome.TheRoots,
            cost: 55, new Stats(90, 20, 18, 6));
        heavyLoader.AddTags(["Mechanical", "Heavy", "Melee", "Armored"]);
        yield return heavyLoader;

        // Boss unit
        var rootMother = EntityTemplate.CreateBoss(
            "root_mother", "The Rot-Mother", factionId, Biome.TheRoots,
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
            "fire_imp", "Spark-Imp", factionId, Biome.Muspelheim,
            cost: 4, new Stats(12, 6, 2, 10));
        fireImp.AddTags(["Fire", "Small", "Ranged"]);
        yield return fireImp;

        var cinder = EntityTemplate.CreateSwarm(
            "cinder", "Ember-Mote", factionId, Biome.Muspelheim,
            cost: 3, new Stats(8, 5, 0, 8));
        cinder.AddTags(["Fire", "Flying", "Explosive"]);
        yield return cinder;

        // Grunt units
        var flameCultist = EntityTemplate.CreateGrunt(
            "flame_cultist", "Ash-Touched", factionId, Biome.Muspelheim,
            cost: 12, EntityRole.Ranged, new Stats(25, 12, 4, 10));
        flameCultist.AddTags(["Human", "Fire", "Ranged"]);
        yield return flameCultist;

        var magmaElemental = EntityTemplate.CreateGrunt(
            "magma_elemental", "Molten-Walk", factionId, Biome.Muspelheim,
            cost: 30, EntityRole.Tank, new Stats(60, 15, 12, 6));
        magmaElemental.AddTags(["Elemental", "Fire", "Heavy", "Melee"]);
        yield return magmaElemental;

        var ashWraith = EntityTemplate.CreateGrunt(
            "ash_wraith", "Cinder-Ghost", factionId, Biome.Muspelheim,
            cost: 22, EntityRole.Melee, new Stats(40, 16, 6, 12));
        ashWraith.AddTags(["Undead", "Fire", "Phasing", "Melee"]);
        yield return ashWraith;

        // Elite unit
        var flameWarden = EntityTemplate.CreateElite(
            "flame_warden", "Pyre-Keeper", factionId, Biome.Muspelheim,
            cost: 65, new Stats(85, 22, 14, 14));
        flameWarden.AddTags(["Human", "Fire", "Heavy", "Melee", "Commander"]);
        yield return flameWarden;

        // Boss unit
        var emberKing = EntityTemplate.CreateBoss(
            "ember_king", "Lord of Cinders", factionId, Biome.Muspelheim,
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
            "ice_sprite", "Frost-Needle", factionId, Biome.Niflheim,
            cost: 3, new Stats(10, 4, 1, 12));
        iceSprite.AddTags(["Cold", "Small", "Flying", "Freezing"]);
        yield return iceSprite;

        var frostWorm = EntityTemplate.CreateSwarm(
            "frost_worm", "Ice-Wyrm", factionId, Biome.Niflheim,
            cost: 4, new Stats(15, 5, 3, 4));
        frostWorm.AddTags(["Cold", "Burrowing", "Melee"]);
        yield return frostWorm;

        // Grunt units
        var frozenCorpse = EntityTemplate.CreateGrunt(
            "frozen_corpse", "Rime-Husk", factionId, Biome.Niflheim,
            cost: 14, EntityRole.Melee, new Stats(35, 10, 8, 5));
        frozenCorpse.AddTags(["Undead", "Cold", "Slow", "Melee"]);
        yield return frozenCorpse;

        var iceWraith = EntityTemplate.CreateGrunt(
            "ice_wraith", "Winter-Ghost", factionId, Biome.Niflheim,
            cost: 20, EntityRole.Melee, new Stats(30, 14, 4, 14));
        iceWraith.AddTags(["Undead", "Cold", "Phasing", "Melee", "Freezing"]);
        yield return iceWraith;

        var frostArcher = EntityTemplate.CreateGrunt(
            "frost_archer", "Chill-Bow", factionId, Biome.Niflheim,
            cost: 16, EntityRole.Ranged, new Stats(25, 13, 5, 10));
        frostArcher.AddTags(["Human", "Cold", "Ranged", "Freezing"]);
        yield return frostArcher;

        // Elite unit
        var glacialBehemoth = EntityTemplate.CreateElite(
            "glacial_behemoth", "Mountain-Shaker", factionId, Biome.Niflheim,
            cost: 70, new Stats(100, 18, 20, 6));
        glacialBehemoth.AddTags(["Elemental", "Cold", "Massive", "Melee", "Armored"]);
        yield return glacialBehemoth;

        // Boss unit
        var frostQueen = EntityTemplate.CreateBoss(
            "frost_queen", "Lady of Winter", factionId, Biome.Niflheim,
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
            "stone_sprite", "Pebble-Kin", factionId, Biome.Jotunheim,
            cost: 4, new Stats(18, 5, 5, 6));
        stoneSprite.AddTags(["Elemental", "Stone", "Small"]);
        yield return stoneSprite;

        var runeWisp = EntityTemplate.CreateSwarm(
            "rune_wisp", "Glyph-Light", factionId, Biome.Jotunheim,
            cost: 5, new Stats(12, 7, 0, 14));
        runeWisp.AddTags(["Magical", "Flying", "Ranged"]);
        yield return runeWisp;

        // Grunt units
        var giantkin = EntityTemplate.CreateGrunt(
            "giantkin", "Titan-Blood", factionId, Biome.Jotunheim,
            cost: 25, EntityRole.Melee, new Stats(55, 16, 10, 8));
        giantkin.AddTags(["Giant", "Heavy", "Melee"]);
        yield return giantkin;

        var runeguard = EntityTemplate.CreateGrunt(
            "runeguard", "Glyph-Sentinel", factionId, Biome.Jotunheim,
            cost: 30, EntityRole.Tank, new Stats(70, 12, 15, 10));
        runeguard.AddTags(["Construct", "Magical", "Armored", "Melee"]);
        yield return runeguard;

        var stormcaller = EntityTemplate.CreateGrunt(
            "stormcaller", "Thunder-Voice", factionId, Biome.Jotunheim,
            cost: 28, EntityRole.Ranged, new Stats(40, 18, 6, 14));
        stormcaller.AddTags(["Giant", "Magical", "Ranged", "Lightning"]);
        yield return stormcaller;

        // Elite unit
        var jotunChampion = EntityTemplate.CreateElite(
            "jotun_champion", "High-Kin", factionId, Biome.Jotunheim,
            cost: 80, new Stats(120, 24, 18, 10));
        jotunChampion.AddTags(["Giant", "Heavy", "Melee", "Commander"]);
        yield return jotunChampion;

        // Boss unit
        var titanAwakened = EntityTemplate.CreateBoss(
            "titan_awakened", "Mountain-Walker", factionId, Biome.Jotunheim,
            cost: 200, new Stats(200, 35, 25, 12));
        titanAwakened.AddTags(["Giant", "Massive", "Ancient", "Commander"]);
        yield return titanAwakened;
    }

    #endregion
}
