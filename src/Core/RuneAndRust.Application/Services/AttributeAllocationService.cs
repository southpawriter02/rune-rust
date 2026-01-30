// ═══════════════════════════════════════════════════════════════════════════════
// AttributeAllocationService.cs
// Service managing attribute point allocation during character creation,
// supporting both Simple (archetype-based) and Advanced (point-buy) modes.
// Handles state creation, attribute modification, validation, cost calculation,
// mode switching, and allocation reset.
// Version: 0.17.2f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages attribute allocation during character creation.
/// </summary>
/// <remarks>
/// <para>
/// AttributeAllocationService implements <see cref="IAttributeAllocationService"/>
/// to provide the core logic for attribute point allocation. It supports two modes:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Simple Mode:</strong> Uses archetype-recommended builds loaded from
///       the <see cref="IAttributeProvider"/>. Manual modification is blocked.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Advanced Mode:</strong> Full point-buy customization using the
///       cost table from <see cref="IAttributeProvider"/>. All attributes start at 1
///       and the player distributes points manually.
///     </description>
///   </item>
/// </list>
/// <para>
/// All state transitions are immutable — each modification returns a new
/// <see cref="AttributeAllocationState"/> instance. The service delegates to
/// <see cref="IAttributeProvider"/> for configuration data (recommended builds,
/// point-buy costs, starting point pools).
/// </para>
/// </remarks>
/// <seealso cref="IAttributeAllocationService"/>
/// <seealso cref="IAttributeProvider"/>
/// <seealso cref="AttributeAllocationState"/>
/// <seealso cref="AttributeModificationResult"/>
/// <seealso cref="AllocationValidationResult"/>
public class AttributeAllocationService : IAttributeAllocationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for attribute configuration, recommended builds, and point-buy costs.
    /// </summary>
    private readonly IAttributeProvider _provider;

    /// <summary>
    /// Logger for diagnostic output during allocation operations.
    /// </summary>
    private readonly ILogger<AttributeAllocationService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="AttributeAllocationService"/>.
    /// </summary>
    /// <param name="provider">
    /// The attribute provider supplying configuration, recommended builds, and
    /// point-buy cost data. Must not be null.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="provider"/> or <paramref name="logger"/> is null.
    /// </exception>
    public AttributeAllocationService(
        IAttributeProvider provider,
        ILogger<AttributeAllocationService> logger)
    {
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _provider = provider;
        _logger = logger;

        _logger.LogDebug(
            "AttributeAllocationService initialized with IAttributeProvider");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - STATE CREATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public AttributeAllocationState CreateAllocationState(
        AttributeAllocationMode mode,
        string? archetypeId = null)
    {
        _logger.LogDebug(
            "Creating allocation state. Mode={Mode}, ArchetypeId='{ArchetypeId}'",
            mode,
            archetypeId ?? "null");

        if (mode == AttributeAllocationMode.Simple)
        {
            // Simple mode requires an archetype ID to look up the recommended build
            ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId, nameof(archetypeId));

            _logger.LogDebug(
                "Simple mode requested. Fetching recommended build for archetype '{ArchetypeId}'",
                archetypeId);

            var recommendedBuild = _provider.GetRecommendedBuild(archetypeId);

            _logger.LogInformation(
                "Created Simple mode allocation state from recommended build. " +
                "ArchetypeId='{ArchetypeId}', " +
                "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, " +
                "TotalPoints={TotalPoints}, IsComplete={IsComplete}",
                archetypeId,
                recommendedBuild.CurrentMight,
                recommendedBuild.CurrentFinesse,
                recommendedBuild.CurrentWits,
                recommendedBuild.CurrentWill,
                recommendedBuild.CurrentSturdiness,
                recommendedBuild.TotalPoints,
                recommendedBuild.IsComplete);

            return recommendedBuild;
        }

        // Advanced mode: create default state with appropriate starting points
        var startingPoints = _provider.GetStartingPoints(archetypeId);

        _logger.LogDebug(
            "Advanced mode requested. StartingPoints={StartingPoints}, " +
            "ArchetypeId='{ArchetypeId}'",
            startingPoints,
            archetypeId ?? "null");

        var advancedState = AttributeAllocationState.CreateAdvancedDefault(startingPoints);

        _logger.LogInformation(
            "Created Advanced mode allocation state. " +
            "StartingPoints={StartingPoints}, AllAttributesAt=1, " +
            "PointsRemaining={PointsRemaining}",
            startingPoints,
            advancedState.PointsRemaining);

        return advancedState;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - ATTRIBUTE MODIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public AttributeModificationResult TryModifyAttribute(
        AttributeAllocationState state,
        CoreAttribute attribute,
        int newValue)
    {
        _logger.LogDebug(
            "TryModifyAttribute: {Attribute} → {NewValue}. " +
            "CurrentMode={Mode}, PointsRemaining={PointsRemaining}",
            attribute,
            newValue,
            state.Mode,
            state.PointsRemaining);

        // Simple mode blocks all manual modifications
        if (state.Mode == AttributeAllocationMode.Simple)
        {
            _logger.LogWarning(
                "Cannot modify attributes in Simple mode. " +
                "Attribute={Attribute}, RequestedValue={NewValue}, " +
                "SelectedArchetype='{SelectedArchetype}'. " +
                "Switch to Advanced mode to manually adjust attribute values.",
                attribute,
                newValue,
                state.SelectedArchetypeId ?? "null");

            return AttributeModificationResult.Failed(
                "Cannot modify attributes in Simple mode", state);
        }

        // Validate the target value is within the allowed range
        var config = _provider.GetPointBuyConfiguration();

        if (newValue < config.MinAttributeValue || newValue > config.MaxAttributeValue)
        {
            _logger.LogWarning(
                "Attribute value out of range. Attribute={Attribute}, " +
                "RequestedValue={NewValue}, ValidRange=[{Min}-{Max}]",
                attribute,
                newValue,
                config.MinAttributeValue,
                config.MaxAttributeValue);

            return AttributeModificationResult.Failed(
                $"Value must be between {config.MinAttributeValue} and {config.MaxAttributeValue}",
                state);
        }

        // Calculate the cost of the change
        var currentValue = state.GetAttributeValue(attribute);
        var cost = config.CalculateCost(currentValue, newValue);

        _logger.LogDebug(
            "Calculated modification cost. Attribute={Attribute}, " +
            "CurrentValue={CurrentValue}, NewValue={NewValue}, " +
            "Cost={Cost}, PointsRemaining={PointsRemaining}",
            attribute,
            currentValue,
            newValue,
            cost,
            state.PointsRemaining);

        // Check affordability for increases (decreases always refund, so cost <= 0)
        if (cost > 0 && cost > state.PointsRemaining)
        {
            _logger.LogWarning(
                "Insufficient points for modification. Attribute={Attribute}, " +
                "CurrentValue={CurrentValue}, NewValue={NewValue}, " +
                "Cost={Cost}, PointsRemaining={PointsRemaining}",
                attribute,
                currentValue,
                newValue,
                cost,
                state.PointsRemaining);

            return AttributeModificationResult.Failed(
                $"Insufficient points: need {cost}, have {state.PointsRemaining}",
                state);
        }

        // Apply the modification to create a new immutable state
        var newState = state.WithAttributeValue(attribute, newValue, cost);

        _logger.LogInformation(
            "Modified {Attribute}: {OldValue} → {NewValue}, cost={Cost}. " +
            "PointsSpent={PointsSpent}, PointsRemaining={PointsRemaining}",
            attribute,
            currentValue,
            newValue,
            cost,
            newState.PointsSpent,
            newState.PointsRemaining);

        return AttributeModificationResult.Succeeded(newState);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - COST CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public int CalculateCost(int fromValue, int toValue)
    {
        _logger.LogDebug(
            "CalculateCost: FromValue={FromValue}, ToValue={ToValue}",
            fromValue,
            toValue);

        var cost = _provider.GetPointBuyConfiguration().CalculateCost(fromValue, toValue);

        _logger.LogDebug(
            "CalculateCost result: FromValue={FromValue}, ToValue={ToValue}, Cost={Cost}",
            fromValue,
            toValue,
            cost);

        return cost;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public AllocationValidationResult ValidateAllocation(AttributeAllocationState state)
    {
        _logger.LogDebug(
            "Validating allocation state. Mode={Mode}, " +
            "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, " +
            "PointsSpent={PointsSpent}, PointsRemaining={PointsRemaining}",
            state.Mode,
            state.CurrentMight,
            state.CurrentFinesse,
            state.CurrentWits,
            state.CurrentWill,
            state.CurrentSturdiness,
            state.PointsSpent,
            state.PointsRemaining);

        var errors = new List<string>();
        var config = _provider.GetPointBuyConfiguration();

        // Check all attribute values are within the valid range
        foreach (CoreAttribute attr in Enum.GetValues<CoreAttribute>())
        {
            var value = state.GetAttributeValue(attr);

            if (value < config.MinAttributeValue || value > config.MaxAttributeValue)
            {
                var errorMessage = $"{attr} value {value} is out of range " +
                    $"[{config.MinAttributeValue}-{config.MaxAttributeValue}]";
                errors.Add(errorMessage);

                _logger.LogDebug(
                    "Validation error: {Error}. Attribute={Attribute}, Value={Value}",
                    errorMessage,
                    attr,
                    value);
            }
        }

        // Check points are not overspent
        if (state.PointsRemaining < 0)
        {
            var errorMessage = $"Points overspent by {Math.Abs(state.PointsRemaining)}";
            errors.Add(errorMessage);

            _logger.LogDebug(
                "Validation error: {Error}. PointsRemaining={PointsRemaining}",
                errorMessage,
                state.PointsRemaining);
        }

        // Simple mode must have an archetype selected
        if (state.Mode == AttributeAllocationMode.Simple &&
            string.IsNullOrWhiteSpace(state.SelectedArchetypeId))
        {
            var errorMessage = "Simple mode requires archetype selection";
            errors.Add(errorMessage);

            _logger.LogDebug(
                "Validation error: {Error}. Mode={Mode}, SelectedArchetypeId='{ArchetypeId}'",
                errorMessage,
                state.Mode,
                state.SelectedArchetypeId ?? "null");
        }

        // Build the result
        if (errors.Count == 0)
        {
            _logger.LogDebug(
                "Allocation validation passed. Mode={Mode}, " +
                "PointsSpent={PointsSpent}, PointsRemaining={PointsRemaining}",
                state.Mode,
                state.PointsSpent,
                state.PointsRemaining);

            return AllocationValidationResult.Valid();
        }

        _logger.LogWarning(
            "Allocation validation failed: {ErrorCount} error(s). " +
            "Errors=[{Errors}]",
            errors.Count,
            string.Join("; ", errors));

        return AllocationValidationResult.Invalid(errors);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - MODE SWITCHING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public AttributeAllocationState SwitchMode(
        AttributeAllocationState currentState,
        AttributeAllocationMode newMode,
        string? archetypeId = null)
    {
        _logger.LogDebug(
            "SwitchMode requested. CurrentMode={CurrentMode}, NewMode={NewMode}, " +
            "ArchetypeId='{ArchetypeId}'",
            currentState.Mode,
            newMode,
            archetypeId ?? "null");

        // If already in the target mode, return the current state unchanged
        if (currentState.Mode == newMode)
        {
            _logger.LogDebug(
                "Already in target mode {Mode}. Returning current state unchanged.",
                newMode);

            return currentState;
        }

        _logger.LogInformation(
            "Switching mode: {OldMode} → {NewMode}",
            currentState.Mode,
            newMode);

        // Switching TO Simple: apply the archetype's recommended build
        if (newMode == AttributeAllocationMode.Simple)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId, nameof(archetypeId));

            _logger.LogDebug(
                "Switching to Simple mode. Fetching recommended build for " +
                "archetype '{ArchetypeId}'",
                archetypeId);

            var recommendedBuild = _provider.GetRecommendedBuild(archetypeId);

            _logger.LogInformation(
                "Switched to Simple mode with archetype '{ArchetypeId}'. " +
                "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, " +
                "TotalPoints={TotalPoints}",
                archetypeId,
                recommendedBuild.CurrentMight,
                recommendedBuild.CurrentFinesse,
                recommendedBuild.CurrentWits,
                recommendedBuild.CurrentWill,
                recommendedBuild.CurrentSturdiness,
                recommendedBuild.TotalPoints);

            return recommendedBuild;
        }

        // Switching TO Advanced: preserve current values, recalculate points spent
        var config = _provider.GetPointBuyConfiguration();
        var pointsSpent = 0;

        _logger.LogDebug(
            "Switching to Advanced mode. Calculating points spent from current values. " +
            "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}",
            currentState.CurrentMight,
            currentState.CurrentFinesse,
            currentState.CurrentWits,
            currentState.CurrentWill,
            currentState.CurrentSturdiness);

        foreach (CoreAttribute attr in Enum.GetValues<CoreAttribute>())
        {
            var attrValue = currentState.GetAttributeValue(attr);
            var attrCost = config.GetCumulativeCost(attrValue);
            pointsSpent += attrCost;

            _logger.LogDebug(
                "Attribute cost: {Attribute}={Value}, CumulativeCost={Cost}",
                attr,
                attrValue,
                attrCost);
        }

        _logger.LogDebug(
            "Total points spent from current values: {PointsSpent}",
            pointsSpent);

        var advancedState = currentState.SwitchToAdvanced(pointsSpent);

        _logger.LogInformation(
            "Switched to Advanced mode. " +
            "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, " +
            "PointsSpent={PointsSpent}, PointsRemaining={PointsRemaining}",
            advancedState.CurrentMight,
            advancedState.CurrentFinesse,
            advancedState.CurrentWits,
            advancedState.CurrentWill,
            advancedState.CurrentSturdiness,
            advancedState.PointsSpent,
            advancedState.PointsRemaining);

        return advancedState;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - RESET
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public AttributeAllocationState ResetAllocation(
        AttributeAllocationMode mode,
        string? archetypeId = null)
    {
        _logger.LogDebug(
            "Resetting allocation. Mode={Mode}, ArchetypeId='{ArchetypeId}'",
            mode,
            archetypeId ?? "null");

        var state = CreateAllocationState(mode, archetypeId);

        _logger.LogInformation(
            "Allocation reset complete. Mode={Mode}, " +
            "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, " +
            "PointsRemaining={PointsRemaining}",
            state.Mode,
            state.CurrentMight,
            state.CurrentFinesse,
            state.CurrentWits,
            state.CurrentWill,
            state.CurrentSturdiness,
            state.PointsRemaining);

        return state;
    }
}
