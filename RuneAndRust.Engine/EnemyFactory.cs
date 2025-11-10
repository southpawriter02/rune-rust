using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public class EnemyFactory
{
    public static Enemy CreateEnemy(EnemyType type)
    {
        return type switch
        {
            EnemyType.CorruptedServitor => CreateCorruptedServitor(),
            EnemyType.BlightDrone => CreateBlightDrone(),
            EnemyType.RuinWarden => CreateRuinWarden(),
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
            IsBoss = false
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
            IsBoss = false
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
            Phase = 1
        };
    }
}
