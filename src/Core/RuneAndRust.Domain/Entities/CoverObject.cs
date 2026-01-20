using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a cover-providing object on the combat grid.
/// </summary>
/// <remarks>
/// <para>
/// CoverObject is created from a <see cref="CoverDefinition"/> and placed on the combat grid.
/// It tracks position, current hit points (if destructible), and provides defense bonuses
/// to entities behind it.
/// </para>
/// <para>
/// Cover mechanics:
/// <list type="bullet">
///   <item><description>Partial cover: Target gains defense bonus, can still be attacked.</description></item>
///   <item><description>Full cover: Target cannot be attacked from the blocked angle.</description></item>
///   <item><description>Destructible: Can be damaged and removed when HP reaches 0.</description></item>
/// </list>
/// </para>
/// </remarks>
public class CoverObject : IEntity
{
    /// <summary>Gets the unique identifier for this cover instance.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the definition ID this cover was created from.</summary>
    public string DefinitionId { get; private set; } = string.Empty;

    /// <summary>Gets the display name of this cover object.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the position of this cover on the grid.</summary>
    public GridPosition Position { get; private set; }

    /// <summary>Gets the cover type (Partial or Full).</summary>
    public CoverType CoverType { get; private set; } = CoverType.Partial;

    /// <summary>Gets the defense bonus provided by this cover.</summary>
    public int DefenseBonus { get; private set; } = 2;

    /// <summary>Gets whether this cover can be damaged and destroyed.</summary>
    public bool IsDestructible { get; private set; }

    /// <summary>Gets the current hit points of this cover.</summary>
    public int CurrentHitPoints { get; private set; }

    /// <summary>Gets the maximum hit points of this cover.</summary>
    public int MaxHitPoints { get; private set; }

    /// <summary>Gets whether this cover blocks movement through its cell.</summary>
    public bool BlocksMovement { get; private set; } = true;

    /// <summary>Gets whether this cover blocks line of sight.</summary>
    public bool BlocksLOS { get; private set; }

    /// <summary>Gets the ASCII display character for this cover.</summary>
    public char DisplayChar { get; private set; } = '▪';

    /// <summary>
    /// Gets whether this cover has been destroyed.
    /// </summary>
    /// <remarks>
    /// A cover is destroyed when it is destructible and its current HP is 0 or less.
    /// </remarks>
    public bool IsDestroyed => IsDestructible && CurrentHitPoints <= 0;

    /// <summary>
    /// Private constructor for EF Core and factory pattern.
    /// </summary>
    private CoverObject()
    {
        DefinitionId = null!;
        Name = null!;
    }

    /// <summary>
    /// Creates a new cover object from a definition at the specified position.
    /// </summary>
    /// <param name="definition">The cover definition to use.</param>
    /// <param name="position">The grid position for this cover.</param>
    /// <returns>A new CoverObject instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when definition is null.</exception>
    public static CoverObject Create(CoverDefinition definition, GridPosition position)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return new CoverObject
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Name = definition.Name,
            Position = position,
            CoverType = definition.CoverType,
            DefenseBonus = definition.DefenseBonus,
            IsDestructible = definition.IsDestructible,
            CurrentHitPoints = definition.MaxHitPoints,
            MaxHitPoints = definition.MaxHitPoints,
            BlocksMovement = definition.BlocksMovement,
            BlocksLOS = definition.BlocksLOS,
            DisplayChar = definition.DisplayChar
        };
    }

    /// <summary>
    /// Applies damage to this cover object.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <returns>True if the cover is destroyed by this damage; false otherwise.</returns>
    /// <remarks>
    /// If the cover is not destructible or already destroyed, no damage is applied
    /// and the method returns false.
    /// </remarks>
    public bool TakeDamage(int damage)
    {
        if (!IsDestructible || IsDestroyed)
            return false;

        CurrentHitPoints = Math.Max(0, CurrentHitPoints - damage);
        return IsDestroyed;
    }

    /// <summary>
    /// Gets the HP percentage of this cover (0-100).
    /// </summary>
    /// <returns>The current HP as a percentage of max HP.</returns>
    public int GetHpPercentage() =>
        MaxHitPoints == 0 ? 100 : (int)((CurrentHitPoints / (float)MaxHitPoints) * 100);
}
