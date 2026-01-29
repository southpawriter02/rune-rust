namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Flags indicating special behaviors for container types.
/// </summary>
/// <remarks>
/// <para>
/// These flags define special properties that modify how a container behaves
/// during loot generation and player interaction.
/// </para>
/// <para>
/// Flags can be combined using bitwise OR operations to represent containers
/// with multiple special behaviors (e.g., a hidden cache that may also be empty).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Container with multiple flags
/// var flags = ContainerFlags.MayBeEmpty | ContainerFlags.RequiresDiscovery;
/// 
/// // Check for specific flag
/// if (flags.HasFlag(ContainerFlags.MythForgedChance))
/// {
///     // Apply myth-forged drop logic
/// }
/// </code>
/// </example>
[Flags]
public enum ContainerFlags
{
    /// <summary>
    /// No special flags. Standard container behavior.
    /// </summary>
    None = 0,

    /// <summary>
    /// Container may be completely empty (0 items).
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this flag is set, the loot generation system may generate
    /// no items at all for the container. Used for corpses and other
    /// uncertain loot sources.
    /// </para>
    /// </remarks>
    MayBeEmpty = 1 << 0,

    /// <summary>
    /// Container has a chance to spawn Myth-Forged quality items.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this flag is set, the loot generation system includes
    /// Myth-Forged (tier 4) items in the possible drop pool. Only
    /// applies to boss chests and other special containers.
    /// </para>
    /// </remarks>
    MythForgedChance = 1 << 1,

    /// <summary>
    /// Container requires a discovery check to find.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this flag is set, the container is initially hidden and
    /// requires a successful Perception check against the container's
    /// DiscoveryDC to become visible and interactable.
    /// </para>
    /// </remarks>
    RequiresDiscovery = 1 << 2
}
