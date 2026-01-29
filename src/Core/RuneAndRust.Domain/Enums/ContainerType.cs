namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the categories of containers that can contain loot.
/// </summary>
/// <remarks>
/// <para>
/// Each container type has associated specifications defining item counts,
/// quality tier ranges, currency amounts, and special behaviors.
/// Specifications are loaded from container-types.json configuration.
/// </para>
/// <para>
/// Container types range from simple corpses with minimal loot to boss chests
/// with guaranteed high-tier items. Some containers have special requirements
/// (discovery checks, category restrictions) that affect loot generation.
/// </para>
/// </remarks>
public enum ContainerType
{
    /// <summary>
    /// A small treasure chest with modest contents.
    /// </summary>
    /// <remarks>
    /// <para>Items: 1-2, Tier: 0-1 (JuryRigged to Scavenged), Currency: 10-30 gold.</para>
    /// <para>Common in early game areas and starting dungeons.</para>
    /// </remarks>
    SmallChest = 0,

    /// <summary>
    /// A medium-sized treasure chest with moderate contents.
    /// </summary>
    /// <remarks>
    /// <para>Items: 2-4, Tier: 1-2 (Scavenged to ClanForged), Currency: 25-75 gold.</para>
    /// <para>Standard container for mid-level areas and progression zones.</para>
    /// </remarks>
    MediumChest = 1,

    /// <summary>
    /// A large treasure chest with valuable contents.
    /// </summary>
    /// <remarks>
    /// <para>Items: 3-5, Tier: 2-3 (ClanForged to Optimized), Currency: 50-150 gold.</para>
    /// <para>Found in challenging areas, secret rooms, or as quest rewards.</para>
    /// </remarks>
    LargeChest = 2,

    /// <summary>
    /// A boss-level treasure chest with exceptional contents.
    /// </summary>
    /// <remarks>
    /// <para>Items: 4-6, Tier: 3-4 (Optimized to MythForged), Currency: 100-300 gold.</para>
    /// <para>Has a chance to contain Myth-Forged quality items. Only appears after boss defeats.</para>
    /// </remarks>
    BossChest = 3,

    /// <summary>
    /// A hidden cache requiring discovery.
    /// </summary>
    /// <remarks>
    /// <para>Items: 1-3, Tier: 1-3 (Scavenged to Optimized), Currency: 50-200 gold.</para>
    /// <para>Requires DC 14 Perception check to find. Rewards exploration and awareness.</para>
    /// </remarks>
    HiddenCache = 4,

    /// <summary>
    /// A corpse that may contain loot.
    /// </summary>
    /// <remarks>
    /// <para>Items: 0-2, Tier: 0-1 (JuryRigged to Scavenged), Currency: 5-20 gold.</para>
    /// <para>May be completely empty (minimum 0 items). Common after combat encounters.</para>
    /// </remarks>
    Corpse = 5,

    /// <summary>
    /// A storage locker with general supplies.
    /// </summary>
    /// <remarks>
    /// <para>Items: 1-3, Tier: 0-2 (JuryRigged to ClanForged), Currency: 15-50 gold.</para>
    /// <para>Common in civilized areas, bunkers, and dungeon living quarters.</para>
    /// </remarks>
    Locker = 6,

    /// <summary>
    /// A supply crate containing consumable items only.
    /// </summary>
    /// <remarks>
    /// <para>Items: 2-3, Tier: 0-1 (JuryRigged to Scavenged), No currency.</para>
    /// <para>Restricted to consumable item category (potions, rations, ammunition).</para>
    /// </remarks>
    SupplyCrate = 7,

    /// <summary>
    /// A weapon rack containing weapons only.
    /// </summary>
    /// <remarks>
    /// <para>Items: 1-2, Tier: 1-3 (Scavenged to Optimized), No currency.</para>
    /// <para>Restricted to weapon item category. Found in armories and guard posts.</para>
    /// </remarks>
    WeaponRack = 8,

    /// <summary>
    /// An armor stand containing a single armor piece.
    /// </summary>
    /// <remarks>
    /// <para>Items: 1, Tier: 1-3 (Scavenged to Optimized), No currency.</para>
    /// <para>Restricted to armor item category. Guaranteed single high-quality piece.</para>
    /// </remarks>
    ArmorStand = 9
}
