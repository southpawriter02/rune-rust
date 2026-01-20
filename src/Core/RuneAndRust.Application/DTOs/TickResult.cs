namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of ticking status effects for a turn.
/// </summary>
public record TickResult
{
    /// <summary>Total damage dealt by DoT effects.</summary>
    public int DamageDealt { get; set; }

    /// <summary>Total healing done by HoT effects.</summary>
    public int HealingDone { get; set; }

    /// <summary>Effect IDs that expired this tick.</summary>
    public List<string> ExpiredEffects { get; } = new();

    /// <summary>Effect IDs that triggered this tick.</summary>
    public List<string> TriggeredEffects { get; } = new();

    /// <summary>Damage breakdown by effect.</summary>
    public Dictionary<string, int> DamageByEffect { get; } = new();

    /// <summary>Healing breakdown by effect.</summary>
    public Dictionary<string, int> HealingByEffect { get; } = new();

    /// <summary>Whether any effects were processed.</summary>
    public bool HadEffects => TriggeredEffects.Count > 0 || ExpiredEffects.Count > 0;
}
