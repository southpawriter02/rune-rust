namespace RuneAndRust.Core;

/// <summary>
/// v0.23.3: Defines loot pools and drop rates for boss encounters
/// Bosses guarantee high-quality drops with chances for legendary/artifact items
/// </summary>
public class BossLootTable
{
    /// <summary>
    /// Boss enemy type this loot table applies to
    /// </summary>
    public EnemyType BossType { get; set; }

    /// <summary>
    /// Minimum quality tier for boss drops (default: ClanForged)
    /// </summary>
    public QualityTier MinimumQuality { get; set; } = QualityTier.ClanForged;

    /// <summary>
    /// Guaranteed quality tier (boss always drops at least this quality)
    /// </summary>
    public QualityTier GuaranteedQuality { get; set; } = QualityTier.ClanForged;

    /// <summary>
    /// Chance (0-100) for Optimized quality drop
    /// </summary>
    public int OptimizedChance { get; set; } = 30;

    /// <summary>
    /// Chance (0-100) for MythForged (legendary) drop
    /// </summary>
    public int LegendaryChance { get; set; } = 15;

    /// <summary>
    /// Chance (0-100) for artifact (unique legendary) drop
    /// Artifacts are boss-specific unique items
    /// </summary>
    public int ArtifactChance { get; set; } = 10;

    /// <summary>
    /// List of unique artifacts this boss can drop
    /// </summary>
    public List<UniqueArtifact> UniqueArtifacts { get; set; } = new();

    /// <summary>
    /// Currency drop range (min, max)
    /// </summary>
    public (int Min, int Max) CurrencyDrop { get; set; } = (300, 800);

    /// <summary>
    /// Guaranteed material drops
    /// </summary>
    public Dictionary<ComponentType, int> GuaranteedMaterials { get; set; } = new();

    /// <summary>
    /// Rare material drop chances
    /// </summary>
    public Dictionary<ComponentType, int> RareMaterialChances { get; set; } = new();

    /// <summary>
    /// Epic/legendary material drop chances
    /// </summary>
    public Dictionary<ComponentType, int> EpicMaterialChances { get; set; } = new();

    /// <summary>
    /// Threat Difficulty Rating modifier for loot scaling
    /// Higher TDR = better loot chances
    /// </summary>
    public double TDRScalingFactor { get; set; } = 1.0;
}

/// <summary>
/// v0.23.3: Defines a unique artifact item that can drop from a specific boss
/// Artifacts are legendary-quality unique items with special properties
/// </summary>
public class UniqueArtifact
{
    /// <summary>
    /// Unique artifact ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Item name
    /// Example: "Ancient Security Core", "Forlorn Commander's Electro-Prod"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Lore description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Item type (Weapon, Armor, Accessory, or special CraftingMaterial)
    /// </summary>
    public ArtifactType Type { get; set; } = ArtifactType.Weapon;

    /// <summary>
    /// Weapon category (if Type = Weapon)
    /// </summary>
    public WeaponCategory? WeaponCategory { get; set; } = null;

    /// <summary>
    /// Damage dice (if weapon)
    /// </summary>
    public int DamageDice { get; set; } = 0;

    /// <summary>
    /// Damage bonus (if weapon)
    /// </summary>
    public int DamageBonus { get; set; } = 0;

    /// <summary>
    /// HP bonus (if armor)
    /// </summary>
    public int HPBonus { get; set; } = 0;

    /// <summary>
    /// Defense bonus (if armor)
    /// </summary>
    public int DefenseBonus { get; set; } = 0;

    /// <summary>
    /// Special effect description
    /// Example: "Inflicts [Stunned] on hit (2 turns)", "+3 MIGHT while below 50% HP"
    /// </summary>
    public string SpecialEffect { get; set; } = string.Empty;

    /// <summary>
    /// Attribute bonuses
    /// </summary>
    public List<EquipmentBonus> Bonuses { get; set; } = new();

    /// <summary>
    /// Drop weight (higher = more likely to drop if artifact chosen)
    /// </summary>
    public int DropWeight { get; set; } = 1;

    /// <summary>
    /// Minimum Threat Difficulty Rating required to drop this artifact
    /// </summary>
    public int MinimumTDR { get; set; } = 0;
}

/// <summary>
/// v0.23.3: Types of artifacts
/// </summary>
public enum ArtifactType
{
    Weapon,
    Armor,
    Accessory,
    CraftingMaterial  // Legendary crafting components
}
