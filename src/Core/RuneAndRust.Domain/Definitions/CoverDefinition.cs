using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Configuration for a type of cover object.
/// </summary>
/// <remarks>
/// <para>
/// CoverDefinition is immutable after creation and serves as a template for creating
/// <see cref="RuneAndRust.Domain.Entities.CoverObject"/> instances.
/// </para>
/// <para>
/// Cover can be:
/// <list type="bullet">
///   <item><description>Partial: Provides a defense bonus but doesn't block attacks.</description></item>
///   <item><description>Full: Completely blocks attacks from specific angles.</description></item>
///   <item><description>Destructible: Can be damaged and eventually destroyed.</description></item>
/// </list>
/// </para>
/// </remarks>
public class CoverDefinition
{
    /// <summary>Gets the unique identifier for this cover type (kebab-case).</summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>Gets the display name for this cover type.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the cover type category (None, Partial, Full).</summary>
    public CoverType CoverType { get; private set; } = CoverType.Partial;

    /// <summary>Gets the defense bonus granted by this cover (typically +2 for partial).</summary>
    public int DefenseBonus { get; private set; } = 2;

    /// <summary>Gets whether this cover can be damaged and destroyed.</summary>
    public bool IsDestructible { get; private set; }

    /// <summary>Gets the maximum hit points for destructible cover.</summary>
    public int MaxHitPoints { get; private set; } = 10;

    /// <summary>Gets whether this cover blocks movement through its cell.</summary>
    public bool BlocksMovement { get; private set; } = true;

    /// <summary>Gets whether this cover blocks line of sight.</summary>
    public bool BlocksLOS { get; private set; }

    /// <summary>Gets the ASCII display character for this cover.</summary>
    public char DisplayChar { get; private set; } = '▪';

    /// <summary>Gets the description of this cover type.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Private constructor for factory pattern.
    /// </summary>
    private CoverDefinition() { }

    /// <summary>
    /// Creates a new cover definition with the specified properties.
    /// </summary>
    /// <param name="id">Unique identifier (will be normalized to lowercase).</param>
    /// <param name="name">Display name.</param>
    /// <param name="coverType">Type of cover (Partial or Full).</param>
    /// <param name="defenseBonus">Defense bonus for partial cover (default: 2).</param>
    /// <param name="isDestructible">Whether cover can be destroyed.</param>
    /// <param name="maxHitPoints">Max HP for destructible cover.</param>
    /// <param name="blocksMovement">Whether cover blocks movement.</param>
    /// <param name="blocksLOS">Whether cover blocks line of sight.</param>
    /// <param name="displayChar">ASCII display character.</param>
    /// <param name="description">Description text.</param>
    /// <returns>A new CoverDefinition instance.</returns>
    /// <exception cref="ArgumentException">Thrown when id is null or whitespace.</exception>
    public static CoverDefinition Create(
        string id,
        string name,
        CoverType coverType,
        int defenseBonus = 2,
        bool isDestructible = false,
        int maxHitPoints = 10,
        bool blocksMovement = true,
        bool blocksLOS = false,
        char displayChar = '▪',
        string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return new CoverDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            CoverType = coverType,
            DefenseBonus = defenseBonus,
            IsDestructible = isDestructible,
            MaxHitPoints = maxHitPoints,
            BlocksMovement = blocksMovement,
            BlocksLOS = blocksLOS,
            DisplayChar = displayChar,
            Description = description
        };
    }

    /// <summary>
    /// Gets whether this cover provides any protection (Partial or Full).
    /// </summary>
    public bool ProvidesCover => CoverType != CoverType.None;
}
