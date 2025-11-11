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
}
