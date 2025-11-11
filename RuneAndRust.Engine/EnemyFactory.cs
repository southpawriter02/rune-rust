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
}
