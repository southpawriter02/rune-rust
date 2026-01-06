using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.Services;

public record CombatResult(
    int DamageDealt,
    int DamageReceived,
    bool MonsterDefeated,
    bool PlayerDefeated
);

public class CombatService
{
    private readonly Random _random = new();

    public CombatResult ResolveCombatRound(Player player, Monster monster)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(monster);

        // Player attacks first
        var playerDamage = CalculateDamage(player.Stats.Attack, monster.Stats.Defense);
        var actualPlayerDamage = monster.TakeDamage(playerDamage);

        var monsterDamage = 0;
        var actualMonsterDamage = 0;

        // Monster attacks if still alive
        if (monster.IsAlive)
        {
            monsterDamage = CalculateDamage(monster.Stats.Attack, player.Stats.Defense);
            actualMonsterDamage = player.TakeDamage(monsterDamage);
        }

        return new CombatResult(
            DamageDealt: actualPlayerDamage,
            DamageReceived: actualMonsterDamage,
            MonsterDefeated: monster.IsDefeated,
            PlayerDefeated: player.IsDead
        );
    }

    private int CalculateDamage(int attack, int defense)
    {
        // Simple damage calculation with some randomness
        var baseDamage = Math.Max(1, attack - defense);
        var variance = _random.Next(-2, 3); // -2 to +2 variance
        return Math.Max(1, baseDamage + variance);
    }

    public string GetCombatDescription(CombatResult result, string playerName, string monsterName)
    {
        var lines = new List<string>();

        if (result.DamageDealt > 0)
        {
            lines.Add($"{playerName} attacks the {monsterName} for {result.DamageDealt} damage!");
        }

        if (result.MonsterDefeated)
        {
            lines.Add($"The {monsterName} has been defeated!");
        }
        else if (result.DamageReceived > 0)
        {
            lines.Add($"The {monsterName} strikes back for {result.DamageReceived} damage!");
        }

        if (result.PlayerDefeated)
        {
            lines.Add($"{playerName} has fallen in battle...");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
