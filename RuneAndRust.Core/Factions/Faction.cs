namespace RuneAndRust.Core.Factions;

/// <summary>
/// v0.33.1: Represents a faction in the world
/// Corresponds to database Factions table
/// </summary>
public class Faction
{
    public int FactionId { get; set; }
    public string FactionName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Philosophy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryLocation { get; set; } = string.Empty;
    public string AlliedFactions { get; set; } = string.Empty;
    public string EnemyFactions { get; set; } = string.Empty;

    /// <summary>
    /// Gets list of allied faction names
    /// </summary>
    public List<string> GetAlliedFactionNames()
    {
        if (string.IsNullOrEmpty(AlliedFactions))
            return new List<string>();

        return AlliedFactions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();
    }

    /// <summary>
    /// Gets list of enemy faction names
    /// </summary>
    public List<string> GetEnemyFactionNames()
    {
        if (string.IsNullOrEmpty(EnemyFactions))
            return new List<string>();

        return EnemyFactions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();
    }

    /// <summary>
    /// Checks if another faction is an ally
    /// </summary>
    public bool IsAlly(string factionName)
    {
        return GetAlliedFactionNames().Contains(factionName);
    }

    /// <summary>
    /// Checks if another faction is an enemy
    /// </summary>
    public bool IsEnemy(string factionName)
    {
        return GetEnemyFactionNames().Contains(factionName);
    }
}
