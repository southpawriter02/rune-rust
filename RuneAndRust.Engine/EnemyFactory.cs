using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public class EnemyFactory
{
    public static Enemy CreateEnemy(EnemyType type)
    {
        return type switch
        {
            // v0.1-v0.3 enemies
            EnemyType.CorruptedServitor => CreateCorruptedServitor(),
            EnemyType.BlightDrone => CreateBlightDrone(),
            EnemyType.RuinWarden => CreateRuinWarden(),

            // v0.4 new enemies
            EnemyType.ScrapHound => CreateScrapHound(),
            EnemyType.TestSubject => CreateTestSubject(),
            EnemyType.WarFrame => CreateWarFrame(),
            EnemyType.ForlornScholar => CreateForlornScholar(),
            EnemyType.AethericAberration => CreateAethericAberration(),

            // v0.6 new enemies
            EnemyType.MaintenanceConstruct => CreateMaintenanceConstruct(),
            EnemyType.SludgeCrawler => CreateSludgeCrawler(),
            EnemyType.CorruptedEngineer => CreateCorruptedEngineer(),
            EnemyType.VaultCustodian => CreateVaultCustodian(),
            EnemyType.ForlornArchivist => CreateForlornArchivist(),
            EnemyType.OmegaSentinel => CreateOmegaSentinel(),

            // v0.16 new enemies (Content Expansion)
            EnemyType.CorrodedSentry => CreateCorrodedSentry(),
            EnemyType.HuskEnforcer => CreateHuskEnforcer(),
            EnemyType.ArcWelderUnit => CreateArcWelderUnit(),
            EnemyType.Shrieker => CreateShrieker(),
            EnemyType.JotunReaderFragment => CreateJotunReaderFragment(),
            EnemyType.ServitorSwarm => CreateServitorSwarm(),
            EnemyType.BoneKeeper => CreateBoneKeeper(),
            EnemyType.FailureColossus => CreateFailureColossus(),
            EnemyType.RustWitch => CreateRustWitch(),
            EnemyType.SentinelPrime => CreateSentinelPrime(),

            _ => throw new ArgumentException($"Unknown enemy type: {type}")
        };
    }

    private static Enemy CreateCorruptedServitor()
    {
        return new Enemy
        {
            Name = "Corrupted Servitor",
            Type = EnemyType.CorruptedServitor,
            MaxHP = 15,
            HP = 15,
            Attributes = new Attributes(
                might: 2,
                finesse: 2,
                wits: 0,
                will: 0,
                sturdiness: 2
            ),
            BaseDamageDice = 1,
            DamageBonus = 0,
            IsBoss = false,
            BaseLegendValue = 10 // Minor Act
        };
    }

    private static Enemy CreateBlightDrone()
    {
        return new Enemy
        {
            Name = "Blight-Drone",
            Type = EnemyType.BlightDrone,
            MaxHP = 25,
            HP = 25,
            Attributes = new Attributes(
                might: 3,
                finesse: 3,
                wits: 0,
                will: 0,
                sturdiness: 3
            ),
            BaseDamageDice = 1,
            DamageBonus = 1,
            IsBoss = false,
            BaseLegendValue = 25 // Minor Act
        };
    }

    private static Enemy CreateRuinWarden()
    {
        return new Enemy
        {
            Name = "Ruin-Warden",
            Type = EnemyType.RuinWarden,
            MaxHP = 80,
            HP = 80,
            Attributes = new Attributes(
                might: 5,
                finesse: 3,
                wits: 0,
                will: 0,
                sturdiness: 5
            ),
            BaseDamageDice = 2,
            DamageBonus = 0,
            IsBoss = true,
            Phase = 1,
            BaseLegendValue = 100 // Significant Act
        };
    }

    // ====== v0.4 NEW ENEMIES ======

    private static Enemy CreateScrapHound()
    {
        return new Enemy
        {
            Name = "Scrap-Hound",
            Type = EnemyType.ScrapHound,
            MaxHP = 10,
            HP = 10,
            Attributes = new Attributes(
                might: 2,
                finesse: 4,
                wits: 0,
                will: 0,
                sturdiness: 1
            ),
            BaseDamageDice = 1,
            DamageBonus = 0,
            IsBoss = false,
            BaseLegendValue = 10 // Minor Act
        };
    }

    private static Enemy CreateTestSubject()
    {
        return new Enemy
        {
            Name = "Test Subject",
            Type = EnemyType.TestSubject,
            MaxHP = 15,
            HP = 15,
            Attributes = new Attributes(
                might: 3,
                finesse: 5,
                wits: 0,
                will: 0,
                sturdiness: 2
            ),
            BaseDamageDice = 1,
            DamageBonus = 2, // High damage for tier
            IsBoss = false,
            BaseLegendValue = 20 // Minor Act
        };
    }

    private static Enemy CreateWarFrame()
    {
        return new Enemy
        {
            Name = "War-Frame",
            Type = EnemyType.WarFrame,
            MaxHP = 50,
            HP = 50,
            Attributes = new Attributes(
                might: 4,
                finesse: 3,
                wits: 0,
                will: 0,
                sturdiness: 4
            ),
            BaseDamageDice = 2,
            DamageBonus = 0,
            IsBoss = false, // Mini-boss but not a true boss
            BaseLegendValue = 50 // Moderate Act
        };
    }

    private static Enemy CreateForlornScholar()
    {
        return new Enemy
        {
            Name = "Forlorn Scholar",
            Type = EnemyType.ForlornScholar,
            MaxHP = 30,
            HP = 30,
            Attributes = new Attributes(
                might: 2,
                finesse: 3,
                wits: 4,
                will: 5,
                sturdiness: 2
            ),
            BaseDamageDice = 2,
            DamageBonus = 0,
            IsBoss = false,
            IsForlorn = true, // [v0.5] Inflicts passive Psychic Stress aura
            BaseLegendValue = 35 // Moderate Act (can avoid combat)
        };
    }

    private static Enemy CreateAethericAberration()
    {
        return new Enemy
        {
            Name = "Aetheric Aberration",
            Type = EnemyType.AethericAberration,
            MaxHP = 60,
            HP = 60,
            Attributes = new Attributes(
                might: 2,
                finesse: 4,
                wits: 5,
                will: 6,
                sturdiness: 3
            ),
            BaseDamageDice = 3,
            DamageBonus = 0,
            IsBoss = true,
            IsForlorn = true, // [v0.5] Inflicts passive Psychic Stress aura
            Phase = 1,
            BaseLegendValue = 100 // Significant Act
        };
    }

    // ====== v0.6 NEW ENEMIES (THE LOWER DEPTHS) ======

    private static Enemy CreateMaintenanceConstruct()
    {
        return new Enemy
        {
            Name = "Maintenance Construct",
            Type = EnemyType.MaintenanceConstruct,
            MaxHP = 25,
            HP = 25,
            Attributes = new Attributes(
                might: 3,
                finesse: 2,
                wits: 0,
                will: 0,
                sturdiness: 4
            ),
            BaseDamageDice = 1, // 1d8 ≈ 1d6+1
            DamageBonus = 1,
            IsBoss = false,
            BaseLegendValue = 15 // Minor Act (self-healing makes it tougher)
        };
    }

    private static Enemy CreateSludgeCrawler()
    {
        return new Enemy
        {
            Name = "Sludge-Crawler",
            Type = EnemyType.SludgeCrawler,
            MaxHP = 12,
            HP = 12,
            Attributes = new Attributes(
                might: 2,
                finesse: 3,
                wits: 0,
                will: 0,
                sturdiness: 1
            ),
            BaseDamageDice = 1, // 1d6 base + poison damage handled in AI
            DamageBonus = 0,
            IsBoss = false,
            BaseLegendValue = 10 // Minor Act (weak but dangerous in groups)
        };
    }

    private static Enemy CreateCorruptedEngineer()
    {
        return new Enemy
        {
            Name = "Corrupted Engineer",
            Type = EnemyType.CorruptedEngineer,
            MaxHP = 30,
            HP = 30,
            Attributes = new Attributes(
                might: 2,
                finesse: 3,
                wits: 5,
                will: 0,
                sturdiness: 2
            ),
            BaseDamageDice = 2, // 2d4 ≈ 2d6 (ranged electrical attack)
            DamageBonus = 0,
            IsBoss = false,
            BaseLegendValue = 30 // Moderate Act (support/buffer)
        };
    }

    private static Enemy CreateVaultCustodian()
    {
        return new Enemy
        {
            Name = "Vault Custodian",
            Type = EnemyType.VaultCustodian,
            MaxHP = 70,
            HP = 70,
            Attributes = new Attributes(
                might: 5,
                finesse: 2,
                wits: 0,
                will: 0,
                sturdiness: 5
            ),
            BaseDamageDice = 2, // 2d8 ≈ 2d6+2
            DamageBonus = 2,
            Soak = 6, // [v0.6] Heavy armor damage reduction
            IsBoss = false, // Mini-boss, not full boss
            Phase = 1, // Phase-based AI
            BaseLegendValue = 75 // Moderate-High Act (mini-boss)
        };
    }

    private static Enemy CreateForlornArchivist()
    {
        return new Enemy
        {
            Name = "Forlorn Archivist",
            Type = EnemyType.ForlornArchivist,
            MaxHP = 80,
            HP = 80,
            Attributes = new Attributes(
                might: 2,
                finesse: 4,
                wits: 4,
                will: 7,
                sturdiness: 3
            ),
            BaseDamageDice = 3, // 3d6 psychic damage (ignores armor)
            DamageBonus = 0,
            IsBoss = true,
            IsForlorn = true, // [v0.6] Extra aura: 5 Stress/turn on top of room's Heavy Psychic Resonance
            Phase = 1, // 3-phase boss fight
            BaseLegendValue = 150 // Significant Act (major boss)
        };
    }

    private static Enemy CreateOmegaSentinel()
    {
        return new Enemy
        {
            Name = "Omega Sentinel",
            Type = EnemyType.OmegaSentinel,
            MaxHP = 100,
            HP = 100,
            Attributes = new Attributes(
                might: 6,
                finesse: 3,
                wits: 0,
                will: 0,
                sturdiness: 6
            ),
            BaseDamageDice = 3, // 3d8 ≈ 3d6+3
            DamageBonus = 3,
            Soak = 8, // [v0.6] Massive armor damage reduction
            IsBoss = true,
            Phase = 1, // 3-phase boss fight
            BaseLegendValue = 150 // Significant Act (major boss)
        };
    }

    // ====== v0.16 NEW ENEMIES (CONTENT EXPANSION) ======

    private static Enemy CreateCorrodedSentry()
    {
        return new Enemy
        {
            Name = "Corroded Sentry",
            Type = EnemyType.CorrodedSentry,
            MaxHP = 15,
            HP = 15,
            Attributes = new Attributes(
                might: 2,
                finesse: 1,
                wits: 0,
                will: 0,
                sturdiness: 2
            ),
            BaseDamageDice = 1, // 1d6 (4-6 damage avg)
            DamageBonus = 0,
            DefenseBonus = -1, // Rusted, less accurate
            IsBoss = false,
            BaseLegendValue = 5 // Trivial enemy
        };
    }

    private static Enemy CreateHuskEnforcer()
    {
        return new Enemy
        {
            Name = "Husk Enforcer",
            Type = EnemyType.HuskEnforcer,
            MaxHP = 25,
            HP = 25,
            Attributes = new Attributes(
                might: 3,
                finesse: 1,
                wits: 0,
                will: 0,
                sturdiness: 3
            ),
            BaseDamageDice = 1, // 1d6+2 (6-9 damage avg)
            DamageBonus = 2,
            IsBoss = false,
            BaseLegendValue = 15 // Low tier enemy
        };
    }

    private static Enemy CreateArcWelderUnit()
    {
        return new Enemy
        {
            Name = "Arc-Welder Unit",
            Type = EnemyType.ArcWelderUnit,
            MaxHP = 30,
            HP = 30,
            Attributes = new Attributes(
                might: 2,
                finesse: 3,
                wits: 0,
                will: 0,
                sturdiness: 3
            ),
            BaseDamageDice = 2, // 2d6 (8-12 damage avg, ranged electrical)
            DamageBonus = 0,
            IsBoss = false,
            BaseLegendValue = 20 // Low tier enemy
        };
    }

    private static Enemy CreateShrieker()
    {
        return new Enemy
        {
            Name = "Shrieker",
            Type = EnemyType.Shrieker,
            MaxHP = 35,
            HP = 35,
            Attributes = new Attributes(
                might: 2,
                finesse: 2,
                wits: 0,
                will: 4,
                sturdiness: 2
            ),
            BaseDamageDice = 1, // 1d6 (4-6 damage) + psychic scream AOE
            DamageBonus = 0,
            IsBoss = false,
            IsForlorn = true, // Inflicts Stress
            BaseLegendValue = 30 // Medium tier enemy
        };
    }

    private static Enemy CreateJotunReaderFragment()
    {
        return new Enemy
        {
            Name = "Jötun-Reader Fragment",
            Type = EnemyType.JotunReaderFragment,
            MaxHP = 40,
            HP = 40,
            Attributes = new Attributes(
                might: 1,
                finesse: 4,
                wits: 5,
                will: 5,
                sturdiness: 3
            ),
            BaseDamageDice = 2, // 2d6 (6-10 psychic damage)
            DamageBonus = 0,
            IsBoss = false,
            IsForlorn = true, // Inflicts Corruption
            BaseLegendValue = 35 // Medium tier enemy
        };
    }

    private static Enemy CreateServitorSwarm()
    {
        return new Enemy
        {
            Name = "Servitor Swarm",
            Type = EnemyType.ServitorSwarm,
            MaxHP = 50,
            HP = 50,
            Attributes = new Attributes(
                might: 3,
                finesse: 3,
                wits: 0,
                will: 0,
                sturdiness: 2
            ),
            BaseDamageDice = 2, // 2d6 (8-12 damage, swarm attack)
            DamageBonus = 0,
            DefenseBonus = -2, // Low defense (swarm)
            IsBoss = false,
            BaseLegendValue = 30 // Medium tier enemy
        };
    }

    private static Enemy CreateBoneKeeper()
    {
        return new Enemy
        {
            Name = "Bone-Keeper",
            Type = EnemyType.BoneKeeper,
            MaxHP = 60,
            HP = 60,
            Attributes = new Attributes(
                might: 4,
                finesse: 3,
                wits: 0,
                will: 3,
                sturdiness: 4
            ),
            BaseDamageDice = 3, // 3d6 (12-18 damage, armor piercing)
            DamageBonus = 0,
            IsBoss = false,
            IsForlorn = true, // Body horror, inflicts Stress
            BaseLegendValue = 50 // High tier enemy
        };
    }

    private static Enemy CreateFailureColossus()
    {
        return new Enemy
        {
            Name = "Failure Colossus",
            Type = EnemyType.FailureColossus,
            MaxHP = 80,
            HP = 80,
            Attributes = new Attributes(
                might: 6,
                finesse: 1,
                wits: 0,
                will: 0,
                sturdiness: 6
            ),
            BaseDamageDice = 4, // 4d6+3 (15-25 damage, crushing blows)
            DamageBonus = 3,
            Soak = 4, // Heavy armor
            IsBoss = false,
            BaseLegendValue = 60 // High tier enemy
        };
    }

    private static Enemy CreateRustWitch()
    {
        return new Enemy
        {
            Name = "Rust-Witch",
            Type = EnemyType.RustWitch,
            MaxHP = 70,
            HP = 70,
            Attributes = new Attributes(
                might: 2,
                finesse: 3,
                wits: 4,
                will: 5,
                sturdiness: 3
            ),
            BaseDamageDice = 3, // 3d6 (10-15 damage, ranged)
            DamageBonus = 0,
            IsBoss = false,
            IsForlorn = true, // High Corruption aura
            BaseLegendValue = 75 // Lethal tier enemy
        };
    }

    private static Enemy CreateSentinelPrime()
    {
        return new Enemy
        {
            Name = "Sentinel Prime",
            Type = EnemyType.SentinelPrime,
            MaxHP = 90,
            HP = 90,
            Attributes = new Attributes(
                might: 5,
                finesse: 5,
                wits: 4,
                will: 0,
                sturdiness: 6
            ),
            BaseDamageDice = 5, // 5d6 (18-28 damage, plasma rifle)
            DamageBonus = 0,
            Soak = 6, // Military-grade armor
            IsBoss = false,
            Phase = 1, // Tactical AI phases
            BaseLegendValue = 100 // Lethal tier enemy
        };
    }
}
