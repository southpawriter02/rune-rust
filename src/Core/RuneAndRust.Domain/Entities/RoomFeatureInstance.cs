using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an instantiated room feature that can be examined by the player.
/// Features are spawned from RoomTemplate.Features based on spawn probability.
/// </summary>
public class RoomFeatureInstance : IEntity
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public RoomFeatureType FeatureType { get; private set; }
    public string FeatureId { get; private set; }
    public string DisplayName { get; private set; }
    public string? DescriptorOverride { get; private set; }
    public bool IsExamined { get; private set; }

    private RoomFeatureInstance()
    {
        FeatureId = null!;
        DisplayName = null!;
    } // For EF Core

    private RoomFeatureInstance(
        RoomFeatureType featureType,
        string featureId,
        string displayName,
        Guid roomId,
        string? descriptorOverride = null)
    {
        Id = Guid.NewGuid();
        FeatureType = featureType;
        FeatureId = featureId ?? throw new ArgumentNullException(nameof(featureId));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        RoomId = roomId;
        DescriptorOverride = descriptorOverride;
        IsExamined = false;
    }

    /// <summary>
    /// Creates a new room feature instance.
    /// </summary>
    public static RoomFeatureInstance Create(
        RoomFeatureType featureType,
        string featureId,
        string displayName,
        Guid roomId,
        string? descriptorOverride = null)
    {
        return new RoomFeatureInstance(featureType, featureId, displayName, roomId, descriptorOverride);
    }

    /// <summary>
    /// Marks this feature as having been examined by the player.
    /// </summary>
    public void MarkExamined()
    {
        IsExamined = true;
    }

    /// <summary>
    /// Checks if this feature matches the given name (case-insensitive).
    /// Matches against both DisplayName and FeatureId.
    /// </summary>
    public bool MatchesName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
               FeatureId.Equals(name, StringComparison.OrdinalIgnoreCase) ||
               FeatureId.Replace("_", " ").Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => $"{DisplayName} ({FeatureType})";
}
