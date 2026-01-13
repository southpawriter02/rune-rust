using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an active source of light in a room.
/// </summary>
/// <remarks>
/// <para>
/// Light sources provide illumination and can be portable (carried by players)
/// or fixed (room fixtures like wall sconces).
/// </para>
/// <para>
/// Two consumption models are supported:
/// <list type="bullet">
///   <item><description>Duration-based (torches): Burns for a set number of turns</description></item>
///   <item><description>Fuel-based (lanterns): Consumes fuel per turn, can be refueled</description></item>
/// </list>
/// </para>
/// </remarks>
public class LightSource : IEntity
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for this light source.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the definition ID from configuration.
    /// </summary>
    public string DefinitionId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display name of this light source.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the light level this source provides.
    /// </summary>
    public LightLevel ProvidedLight { get; private set; } = LightLevel.Bright;

    /// <summary>
    /// Gets whether this light source is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets whether this light source is portable (carried by player).
    /// </summary>
    public bool IsPortable { get; private set; }

    #endregion

    #region Duration Properties

    /// <summary>
    /// Gets the remaining duration in turns (-1 = permanent).
    /// </summary>
    public int RemainingDuration { get; private set; } = -1;

    /// <summary>
    /// Gets whether this light source has limited duration.
    /// </summary>
    public bool HasDuration => RemainingDuration >= 0;

    /// <summary>
    /// Gets whether this light source is permanent (no duration).
    /// </summary>
    public bool IsPermanent => RemainingDuration < 0;

    #endregion

    #region Fuel Properties

    /// <summary>
    /// Gets whether this light source uses fuel.
    /// </summary>
    public bool UsesFuel { get; private set; }

    /// <summary>
    /// Gets the current fuel amount (for lanterns).
    /// </summary>
    public int CurrentFuel { get; private set; }

    /// <summary>
    /// Gets the maximum fuel capacity.
    /// </summary>
    public int MaxFuel { get; private set; }

    /// <summary>
    /// Gets whether this light source has fuel remaining.
    /// </summary>
    public bool HasFuel => !UsesFuel || CurrentFuel > 0;

    /// <summary>
    /// Gets whether this light source has expired (duration/fuel exhausted).
    /// </summary>
    public bool IsExpired => (HasDuration && RemainingDuration <= 0) || (UsesFuel && CurrentFuel <= 0);

    #endregion

    #region Association Properties

    /// <summary>
    /// Gets the associated item ID (if this is an item-based light).
    /// </summary>
    public Guid? AssociatedItemId { get; private set; }

    /// <summary>
    /// Gets the owner player ID (for portable lights).
    /// </summary>
    public Guid? OwnerId { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private LightSource() { }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new light source.
    /// </summary>
    public static LightSource Create(
        string definitionId,
        string name,
        LightLevel providedLight,
        bool isPortable = false,
        int duration = -1,
        bool usesFuel = false,
        int maxFuel = 0,
        Guid? associatedItemId = null,
        Guid? ownerId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new LightSource
        {
            Id = Guid.NewGuid(),
            DefinitionId = definitionId,
            Name = name,
            ProvidedLight = providedLight,
            IsPortable = isPortable,
            RemainingDuration = duration,
            UsesFuel = usesFuel,
            MaxFuel = maxFuel,
            CurrentFuel = maxFuel,
            AssociatedItemId = associatedItemId,
            OwnerId = ownerId
        };
    }

    /// <summary>
    /// Creates a torch light source (duration-based).
    /// </summary>
    /// <param name="duration">Turns before the torch burns out.</param>
    /// <param name="itemId">Associated item ID.</param>
    /// <param name="ownerId">Owner player ID.</param>
    /// <returns>A new torch light source.</returns>
    public static LightSource CreateTorch(int duration, Guid itemId, Guid ownerId) =>
        Create("torch", "Torch", LightLevel.Bright, true, duration, false, 0, itemId, ownerId);

    /// <summary>
    /// Creates a lantern light source (fuel-based).
    /// </summary>
    /// <param name="maxFuel">Maximum fuel capacity.</param>
    /// <param name="itemId">Associated item ID.</param>
    /// <param name="ownerId">Owner player ID.</param>
    /// <returns>A new lantern light source.</returns>
    public static LightSource CreateLantern(int maxFuel, Guid itemId, Guid ownerId) =>
        Create("lantern", "Lantern", LightLevel.Bright, true, -1, true, maxFuel, itemId, ownerId);

    #endregion

    #region Light Control Methods

    /// <summary>
    /// Activates this light source.
    /// </summary>
    /// <returns>True if activated, false if already active or out of fuel.</returns>
    public bool Activate()
    {
        if (IsActive)
            return false;

        if (UsesFuel && CurrentFuel <= 0)
            return false;

        IsActive = true;
        return true;
    }

    /// <summary>
    /// Deactivates this light source.
    /// </summary>
    /// <returns>True if deactivated, false if not active.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
            return false;

        IsActive = false;
        return true;
    }

    /// <summary>
    /// Processes a turn tick, consuming duration or fuel.
    /// </summary>
    /// <returns>True if the light source burned out this tick.</returns>
    public bool Tick()
    {
        if (!IsActive)
            return false;

        if (HasDuration)
        {
            RemainingDuration--;
            if (RemainingDuration <= 0)
            {
                IsActive = false;
                return true; // Burned out
            }
        }

        if (UsesFuel)
        {
            CurrentFuel--;
            if (CurrentFuel <= 0)
            {
                IsActive = false;
                return true; // Out of fuel
            }
        }

        return false;
    }

    /// <summary>
    /// Refuels this light source.
    /// </summary>
    /// <param name="fuelAmount">Amount of fuel to add.</param>
    /// <returns>Amount of fuel actually added.</returns>
    public int Refuel(int fuelAmount)
    {
        if (!UsesFuel)
            return 0;

        var spaceAvailable = MaxFuel - CurrentFuel;
        var actualFuel = Math.Min(fuelAmount, spaceAvailable);
        CurrentFuel += actualFuel;
        return actualFuel;
    }

    /// <summary>
    /// Gets the percentage of fuel remaining.
    /// </summary>
    /// <returns>Fuel percentage (0-100), or 100 if not fuel-based.</returns>
    public int GetFuelPercentage()
    {
        if (!UsesFuel || MaxFuel == 0)
            return 100;

        return (int)((CurrentFuel / (float)MaxFuel) * 100);
    }

    #endregion

    /// <summary>
    /// Returns a string representation of this light source.
    /// </summary>
    public override string ToString() =>
        IsActive
            ? $"LightSource({Name}, {ProvidedLight}, Active)"
            : $"LightSource({Name}, Inactive)";
}
