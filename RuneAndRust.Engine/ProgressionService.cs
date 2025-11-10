using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public class ProgressionService
{
    // Level thresholds as specified in design doc
    private static readonly Dictionary<int, int> LevelThresholds = new()
    {
        { 2, 50 },
        { 3, 100 },
        { 4, 150 },
        { 5, 200 }
    };

    private const int MaxLevel = 5;

    public void AwardXP(PlayerCharacter player, int xpAmount)
    {
        if (player.Level >= MaxLevel)
        {
            return; // Already at max level
        }

        player.CurrentXP += xpAmount;
    }

    public bool CanLevelUp(PlayerCharacter player)
    {
        if (player.Level >= MaxLevel)
        {
            return false;
        }

        int nextLevel = player.Level + 1;
        if (LevelThresholds.TryGetValue(nextLevel, out int requiredXP))
        {
            return player.CurrentXP >= requiredXP;
        }

        return false;
    }

    public void LevelUp(PlayerCharacter player, string attributeToIncrease)
    {
        if (!CanLevelUp(player))
        {
            throw new InvalidOperationException("Player cannot level up yet.");
        }

        // Increase level
        player.Level++;

        // Update XP to next level
        if (player.Level < MaxLevel && LevelThresholds.TryGetValue(player.Level + 1, out int nextThreshold))
        {
            player.XPToNextLevel = nextThreshold;
        }
        else if (player.Level >= MaxLevel)
        {
            player.XPToNextLevel = 0; // At max level
        }

        // Grant level up rewards
        player.MaxHP += 10;
        player.MaxStamina += 5;

        // Full heal on level up
        player.HP = player.MaxHP;
        player.Stamina = player.MaxStamina;

        // Apply attribute point (player chooses which attribute)
        ApplyAttributePoint(player, attributeToIncrease);
    }

    private void ApplyAttributePoint(PlayerCharacter player, string attributeName)
    {
        const int AttributeCap = 6;

        switch (attributeName.ToLower())
        {
            case "might":
                if (player.Attributes.Might < AttributeCap)
                    player.Attributes.Might++;
                break;
            case "finesse":
                if (player.Attributes.Finesse < AttributeCap)
                    player.Attributes.Finesse++;
                break;
            case "wits":
                if (player.Attributes.Wits < AttributeCap)
                    player.Attributes.Wits++;
                break;
            case "will":
                if (player.Attributes.Will < AttributeCap)
                    player.Attributes.Will++;
                break;
            case "sturdiness":
                if (player.Attributes.Sturdiness < AttributeCap)
                    player.Attributes.Sturdiness++;
                break;
            default:
                throw new ArgumentException($"Invalid attribute name: {attributeName}");
        }
    }

    public int GetXPToNextLevel(PlayerCharacter player)
    {
        if (player.Level >= MaxLevel)
        {
            return 0;
        }

        int nextLevel = player.Level + 1;
        if (LevelThresholds.TryGetValue(nextLevel, out int requiredXP))
        {
            return requiredXP - player.CurrentXP;
        }

        return 0;
    }

    public bool IsAbilityUnlockedAtLevel(int level)
    {
        return level == 3 || level == 5;
    }

    public int GetAbilitySlotToUnlock(int level)
    {
        return level switch
        {
            3 => 2, // Third ability (index 2)
            5 => 3, // Fourth ability (index 3)
            _ => -1
        };
    }

    public int GetUnlockedAbilityCount(int level)
    {
        return level switch
        {
            1 => 2,  // Start with 2 abilities
            2 => 2,  // Still only 2
            3 => 3,  // Unlock 3rd
            4 => 3,  // Still only 3
            >= 5 => 4,  // Unlock 4th at level 5
            _ => 2
        };
    }

    public List<Ability> GetUsableAbilities(PlayerCharacter player)
    {
        int unlockedCount = GetUnlockedAbilityCount(player.Level);
        return player.Abilities.Take(unlockedCount).ToList();
    }
}
