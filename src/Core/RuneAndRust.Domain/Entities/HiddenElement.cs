using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a hidden element in a room (trap, secret door, or cache)
/// that can be revealed via passive or active perception.
/// </summary>
public class HiddenElement : IEntity
{
    public Guid Id { get; private set; }
    public HiddenElementType ElementType { get; private set; }
    public string Name { get; private set; }
    public string DiscoveryText { get; private set; }
    public int DetectionDC { get; private set; }
    public bool IsRevealed { get; private set; }
    public Guid RoomId { get; private set; }

    // Trap-specific properties
    public int? TrapDamage { get; private set; }
    public int? DisarmDC { get; private set; }
    public bool IsDisarmed { get; private set; }

    // SecretDoor-specific properties
    public Guid? LeadsToRoomId { get; private set; }

    // Cache-specific properties
    public string? CacheContents { get; private set; }
    public bool IsLooted { get; private set; }

    private HiddenElement()
    {
        Name = null!;
        DiscoveryText = null!;
    } // For EF Core

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

        var element = new HiddenElement(HiddenElementType.Cache, name, discoveryText, detectionDC, roomId)
        {
            CacheContents = cacheContents
        };
        return element;
    }

    /// <summary>
    /// Reveals this hidden element to the player.
    /// </summary>
    public void Reveal()
    {
        IsRevealed = true;
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
        if (ElementType != HiddenElementType.Cache || IsLooted)
            return false;

        IsLooted = true;
        return true;
    }

    /// <summary>
    /// Checks if this element can be detected with the given passive perception.
    /// </summary>
    public bool CanBeDetectedBy(int passivePerception) =>
        !IsRevealed && passivePerception >= DetectionDC;

    public override string ToString() => $"{ElementType}: {Name} (DC {DetectionDC})";
}
