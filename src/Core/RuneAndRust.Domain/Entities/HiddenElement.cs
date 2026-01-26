using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a hidden element in a room (trap, secret door, cache, etc.)
/// that can be revealed via passive or active perception.
/// </summary>
/// <remarks>
/// <para>
/// Hidden elements are automatically checked against passive perception when a character
/// enters a room. If passive perception >= DetectionDC, the element is revealed.
/// </para>
/// <para>
/// Once revealed, hidden elements remain discovered and track who discovered them
/// and when for attribution purposes.
/// </para>
/// </remarks>
public class HiddenElement : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique database identifier for this hidden element instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the configuration-based element ID (e.g., "crypt-pressure-plate-01").
    /// </summary>
    /// <remarks>
    /// Used for identification in configuration files and scripting.
    /// May be null for legacy elements created before v0.15.6a.
    /// </remarks>
    public string? ElementId { get; private set; }

    /// <summary>
    /// Gets the type of hidden element.
    /// </summary>
    public HiddenElementType ElementType { get; private set; }

    /// <summary>
    /// Gets the display name for the hidden element.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the flavor text displayed when this element is discovered.
    /// </summary>
    public string DiscoveryText { get; private set; }

    /// <summary>
    /// Gets the difficulty class required to discover this element.
    /// </summary>
    /// <remarks>
    /// Passive perception must be >= this value for automatic discovery.
    /// Active search checks roll against this DC.
    /// </remarks>
    public int DetectionDC { get; private set; }

    /// <summary>
    /// Gets whether this element has been revealed.
    /// </summary>
    public bool IsRevealed { get; private set; }

    /// <summary>
    /// Gets the ID of the room containing this hidden element.
    /// </summary>
    public Guid RoomId { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // REVELATION TRACKING (v0.15.6a)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the ID of the character who revealed this element (if revealed).
    /// </summary>
    /// <remarks>
    /// Added in v0.15.6a to track which character discovered each element.
    /// May be null for elements revealed via legacy code paths.
    /// </remarks>
    public string? RevealedBy { get; private set; }

    /// <summary>
    /// Gets the timestamp when this element was revealed (if revealed).
    /// </summary>
    /// <remarks>
    /// Added in v0.15.6a for attribution and logging purposes.
    /// May be null for elements revealed via legacy code paths.
    /// </remarks>
    public DateTime? RevealedAt { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRAP-SPECIFIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the damage dealt by this trap (Trap type only).
    /// </summary>
    public int? TrapDamage { get; private set; }

    /// <summary>
    /// Gets the DC required to disarm this trap (Trap type only).
    /// </summary>
    public int? DisarmDC { get; private set; }

    /// <summary>
    /// Gets whether this trap has been disarmed.
    /// </summary>
    public bool IsDisarmed { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECRET DOOR PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the room ID this secret door leads to (SecretDoor type only).
    /// </summary>
    public Guid? LeadsToRoomId { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CACHE PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the contents description for this cache (Cache/HiddenCache type only).
    /// </summary>
    public string? CacheContents { get; private set; }

    /// <summary>
    /// Gets whether this cache has been looted.
    /// </summary>
    public bool IsLooted { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private HiddenElement()
    {
        Name = null!;
        DiscoveryText = null!;
    }

    private HiddenElement(
        HiddenElementType elementType,
        string name,
        string discoveryText,
        int detectionDC,
        Guid roomId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(discoveryText))
            throw new ArgumentException("Discovery text cannot be empty", nameof(discoveryText));
        if (detectionDC < 1)
            throw new ArgumentOutOfRangeException(nameof(detectionDC), "Detection DC must be at least 1");

        Id = Guid.NewGuid();
        ElementType = elementType;
        Name = name;
        DiscoveryText = discoveryText;
        DetectionDC = detectionDC;
        RoomId = roomId;
        IsRevealed = false;
        IsDisarmed = false;
        IsLooted = false;
    }

    /// <summary>
    /// Creates a trap hidden element.
    /// </summary>
    public static HiddenElement CreateTrap(
        string name,
        string discoveryText,
        int detectionDC,
        int trapDamage,
        int disarmDC,
        Guid roomId)
    {
        if (trapDamage < 1)
            throw new ArgumentOutOfRangeException(nameof(trapDamage), "Trap damage must be at least 1");
        if (disarmDC < 1)
            throw new ArgumentOutOfRangeException(nameof(disarmDC), "Disarm DC must be at least 1");

        var element = new HiddenElement(HiddenElementType.Trap, name, discoveryText, detectionDC, roomId)
        {
            TrapDamage = trapDamage,
            DisarmDC = disarmDC
        };
        return element;
    }

    /// <summary>
    /// Creates a secret door hidden element.
    /// </summary>
    public static HiddenElement CreateSecretDoor(
        string name,
        string discoveryText,
        int detectionDC,
        Guid roomId,
        Guid leadsToRoomId)
    {
        var element = new HiddenElement(HiddenElementType.SecretDoor, name, discoveryText, detectionDC, roomId)
        {
            LeadsToRoomId = leadsToRoomId
        };
        return element;
    }

    /// <summary>
    /// Creates a hidden cache element.
    /// </summary>
    public static HiddenElement CreateCache(
        string name,
        string discoveryText,
        int detectionDC,
        Guid roomId,
        string cacheContents)
    {
        if (string.IsNullOrWhiteSpace(cacheContents))
            throw new ArgumentException("Cache contents cannot be empty", nameof(cacheContents));

        var element = new HiddenElement(HiddenElementType.HiddenCache, name, discoveryText, detectionDC, roomId)
        {
            CacheContents = cacheContents
        };
        return element;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DOMAIN METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Reveals this hidden element to the player.
    /// </summary>
    /// <remarks>
    /// Legacy method for backward compatibility. Prefer using Reveal(characterId)
    /// for proper attribution tracking.
    /// </remarks>
    [Obsolete("Use Reveal(string characterId) for proper attribution tracking.")]
    public void Reveal()
    {
        IsRevealed = true;
        RevealedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reveals this hidden element and records the discovering character.
    /// </summary>
    /// <param name="characterId">The ID of the character who discovered this element.</param>
    /// <exception cref="InvalidOperationException">Thrown if element is already revealed.</exception>
    /// <exception cref="ArgumentException">Thrown when characterId is null or empty.</exception>
    public void Reveal(string characterId)
    {
        if (IsRevealed)
        {
            throw new InvalidOperationException(
                $"Hidden element '{Name}' (ID: {Id}) has already been revealed.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);

        IsRevealed = true;
        RevealedBy = characterId;
        RevealedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Attempts to disarm a trap. Only valid for Trap type elements.
    /// </summary>
    public bool TryDisarm()
    {
        if (ElementType != HiddenElementType.Trap)
            return false;

        IsDisarmed = true;
        return true;
    }

    /// <summary>
    /// Marks a cache as looted. Only valid for Cache type elements.
    /// </summary>
    public bool TryLoot()
    {
        if (ElementType != HiddenElementType.HiddenCache || IsLooted)
            return false;

        IsLooted = true;
        return true;
    }

    /// <summary>
    /// Checks if this element can be detected with the given passive perception.
    /// </summary>
    /// <param name="passivePerception">The passive perception value to check.</param>
    /// <returns>True if passive perception >= DetectionDC and element is not already revealed.</returns>
    public bool CanBeDetectedBy(int passivePerception) =>
        !IsRevealed && passivePerception >= DetectionDC;

    /// <summary>
    /// Alias for CanBeDetectedBy for v0.15.6a design spec compatibility.
    /// </summary>
    /// <param name="passivePerception">The passive perception value to check.</param>
    /// <returns>True if passive perception >= DetectionDC and element is not already revealed.</returns>
    public bool CanBeDiscoveredBy(int passivePerception) =>
        CanBeDetectedBy(passivePerception);

    /// <summary>
    /// Returns a string representation of this hidden element.
    /// </summary>
    /// <returns>A string with element type, name, and DC or revealed status.</returns>
    public override string ToString()
    {
        var status = IsRevealed
            ? $"Revealed{(RevealedBy != null ? $" by {RevealedBy}" : "")}"
            : $"DC {DetectionDC}";
        return $"{ElementType}: {Name} ({status})";
    }
}
