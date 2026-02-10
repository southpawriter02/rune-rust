// ═══════════════════════════════════════════════════════════════════════════════
// DiscoveryRecord.cs
// Immutable value object recording a discovery event for the Jötun-Reader
// specialization. Used for tracking discoveries, Technical Memory ability,
// and quest triggers.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Records a discovery event made by a Jötun-Reader character.
/// </summary>
/// <remarks>
/// <para>
/// Discovery records track all examination and discovery activities, enabling:
/// </para>
/// <list type="bullet">
///   <item><description>Lore Insight generation tracking</description></item>
///   <item><description>Technical Memory ability (Tier 2) — re-reading past discoveries</description></item>
///   <item><description>Quest progression triggers based on discovery history</description></item>
///   <item><description>First-examination detection for bonus insight opportunities</description></item>
/// </list>
/// <para>
/// This is an immutable value object. Use the <see cref="Create"/> factory method
/// to construct new instances.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var record = DiscoveryRecord.Create(
///     discovererId: characterId,
///     discoveryType: DiscoveryType.JotunMachinery,
///     targetId: machineId,
///     targetName: "Jötun Power Conduit",
///     insightGenerated: 1,
///     loreText: "An ancient power routing device...");
/// </code>
/// </example>
public sealed record DiscoveryRecord
{
    /// <summary>
    /// Gets the unique identifier for this discovery event.
    /// </summary>
    public Guid DiscoveryId { get; init; }

    /// <summary>
    /// Gets the ID of the character who made the discovery.
    /// </summary>
    public Guid DiscovererId { get; init; }

    /// <summary>
    /// Gets the type of discovery (machinery, artifact, text, etc.).
    /// </summary>
    public LoreDiscoveryType DiscoveryType { get; init; }

    /// <summary>
    /// Gets the ID of the discovered object.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Gets the name or description of the discovered object.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the number of Lore Insight points generated from this discovery.
    /// </summary>
    public int InsightGenerated { get; init; }

    /// <summary>
    /// Gets the UTC timestamp of when the discovery was made.
    /// </summary>
    public DateTime DiscoveredAt { get; init; }

    /// <summary>
    /// Gets the optional lore text revealed by the discovery.
    /// </summary>
    public string? LoreText { get; init; }

    /// <summary>
    /// Gets whether this discovery was a critical success.
    /// </summary>
    public bool WasCritical { get; init; }

    /// <summary>
    /// Creates a new discovery record.
    /// </summary>
    /// <param name="discovererId">ID of the character who made the discovery.</param>
    /// <param name="discoveryType">Type of discovery made.</param>
    /// <param name="targetId">ID of the discovered object.</param>
    /// <param name="targetName">Name of the discovered object.</param>
    /// <param name="insightGenerated">Lore Insight points generated.</param>
    /// <param name="loreText">Optional lore text revealed.</param>
    /// <param name="wasCritical">Whether this was a critical success.</param>
    /// <returns>A new <see cref="DiscoveryRecord"/> instance.</returns>
    public static DiscoveryRecord Create(
        Guid discovererId,
        LoreDiscoveryType discoveryType,
        Guid targetId,
        string targetName,
        int insightGenerated,
        string? loreText = null,
        bool wasCritical = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        ArgumentOutOfRangeException.ThrowIfNegative(insightGenerated);

        return new DiscoveryRecord
        {
            DiscoveryId = Guid.NewGuid(),
            DiscovererId = discovererId,
            DiscoveryType = discoveryType,
            TargetId = targetId,
            TargetName = targetName,
            InsightGenerated = insightGenerated,
            DiscoveredAt = DateTime.UtcNow,
            LoreText = loreText,
            WasCritical = wasCritical
        };
    }

    /// <summary>
    /// Gets a summary of the discovery for display or logging.
    /// </summary>
    /// <returns>Formatted summary string (e.g., "JotunMachinery: Jötun Power Conduit (+1 Lore Insight)").</returns>
    public string GetSummary() =>
        $"{DiscoveryType}: {TargetName} (+{InsightGenerated} Lore Insight)";

    /// <summary>
    /// Determines if this discovery contains lore information.
    /// </summary>
    /// <returns><c>true</c> if <see cref="LoreText"/> is non-empty; otherwise, <c>false</c>.</returns>
    public bool HasLore() => !string.IsNullOrWhiteSpace(LoreText);

    /// <summary>
    /// Gets the lore text safely, returning an empty string if null.
    /// </summary>
    /// <returns>The lore text, or <see cref="string.Empty"/> if none.</returns>
    public string GetLoreTextOrEmpty() => LoreText ?? string.Empty;

    /// <summary>
    /// Returns a human-readable representation of the discovery.
    /// </summary>
    public override string ToString() => GetSummary();
}
