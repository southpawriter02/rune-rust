namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Aggregated stats from all equipment modifications
/// </summary>
public class ModificationStats
{
    public int BonusDamage { get; set; }
    public int BonusMitigation { get; set; }
    public int BonusAccuracy { get; set; }
    public int BonusEvasion { get; set; }
    public Dictionary<string, int> Resistances { get; set; } = new();
    public List<ElementalEffect> ElementalDamage { get; set; } = new();
    public List<ModificationStatusEffect> StatusEffects { get; set; } = new();
    public int RegenerationPerTurn { get; set; }
}

/// <summary>
/// Elemental damage effect from modifications
/// </summary>
public class ElementalEffect
{
    public string Element { get; set; } = string.Empty; // Fire, Ice, Lightning, etc.
    public int BonusDamage { get; set; }
    public double ApplicationChance { get; set; } // 0.0 to 1.0
}

/// <summary>
/// Status effect from modifications (distinct from Core.StatusEffect)
/// </summary>
public class ModificationStatusEffect
{
    public string StatusName { get; set; } = string.Empty; // Bleed, Poison, Slow, etc.
    public double ApplicationChance { get; set; } // 0.0 to 1.0
    public int Duration { get; set; } // Turns
}
