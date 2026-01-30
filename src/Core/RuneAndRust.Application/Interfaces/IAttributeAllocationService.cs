// ═══════════════════════════════════════════════════════════════════════════════
// IAttributeAllocationService.cs
// Interface defining the contract for managing attribute point allocation during
// character creation, supporting both Simple (archetype-based) and Advanced
// (point-buy) allocation modes.
// Version: 0.17.2f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages attribute point allocation during character creation.
/// </summary>
/// <remarks>
/// <para>
/// IAttributeAllocationService defines the contract for creating, modifying, validating,
/// and managing attribute allocation state during the character creation workflow.
/// It handles both allocation modes:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Simple Mode:</strong> Attributes are auto-set from an archetype's
///       recommended build. Manual modification is blocked. The fastest path for
///       new players.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Advanced Mode:</strong> Full point-buy customization where all
///       attributes start at 1 and the player distributes points manually.
///       Cost scaling applies at higher values (values 9-10 cost 2 points each).
///     </description>
///   </item>
/// </list>
/// <para>
/// The service maintains immutable state — every modification returns a new
/// <see cref="AttributeAllocationState"/> instance rather than mutating the existing one.
/// This ensures thread safety and enables undo/redo patterns in the future.
/// </para>
/// <para>
/// <strong>Dependencies:</strong> Implementations depend on <see cref="IAttributeProvider"/>
/// for configuration data (recommended builds, point-buy costs, starting points).
/// </para>
/// <para>
/// <strong>Usage Flow:</strong>
/// <code>
/// // 1. Create initial state
/// var state = service.CreateAllocationState(AttributeAllocationMode.Advanced);
///
/// // 2. Modify attributes
/// var result = service.TryModifyAttribute(state, CoreAttribute.Might, 5);
/// if (result.Success) state = result.NewState!.Value;
///
/// // 3. Validate before proceeding
/// var validation = service.ValidateAllocation(state);
/// if (validation.IsValid) { /* proceed to next step */ }
/// </code>
/// </para>
/// </remarks>
/// <seealso cref="AttributeAllocationState"/>
/// <seealso cref="AttributeModificationResult"/>
/// <seealso cref="AllocationValidationResult"/>
/// <seealso cref="IAttributeProvider"/>
public interface IAttributeAllocationService
{
    /// <summary>
    /// Creates the initial allocation state for the specified mode.
    /// </summary>
    /// <param name="mode">
    /// The allocation mode to initialize.
    /// <see cref="AttributeAllocationMode.Simple"/> creates a state with
    /// archetype-recommended attribute values; <see cref="AttributeAllocationMode.Advanced"/>
    /// creates a state with all attributes at 1 and the full point pool available.
    /// </param>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "mystic", "adept").
    /// Required for Simple mode to determine the recommended build.
    /// Optional for Advanced mode (used to determine starting points — 14 for Adept, 15 for others).
    /// Case-insensitive.
    /// </param>
    /// <returns>A new <see cref="AttributeAllocationState"/> initialized for the specified mode.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="mode"/> is <see cref="AttributeAllocationMode.Simple"/>
    /// and <paramref name="archetypeId"/> is null, empty, or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Simple mode with Warrior recommended build
    /// var simpleState = service.CreateAllocationState(
    ///     AttributeAllocationMode.Simple, "warrior");
    /// // simpleState.CurrentMight == 4, simpleState.IsComplete == true
    ///
    /// // Advanced mode with default 15 points
    /// var advancedState = service.CreateAllocationState(
    ///     AttributeAllocationMode.Advanced);
    /// // advancedState.CurrentMight == 1, advancedState.PointsRemaining == 15
    ///
    /// // Advanced mode with Adept's 14 points
    /// var adeptState = service.CreateAllocationState(
    ///     AttributeAllocationMode.Advanced, "adept");
    /// // adeptState.PointsRemaining == 14
    /// </code>
    /// </example>
    AttributeAllocationState CreateAllocationState(
        AttributeAllocationMode mode,
        string? archetypeId = null);

    /// <summary>
    /// Attempts to modify an attribute value in the current allocation state.
    /// </summary>
    /// <param name="state">The current allocation state to modify.</param>
    /// <param name="attribute">The core attribute to change (Might, Finesse, Wits, Will, or Sturdiness).</param>
    /// <param name="newValue">The target value for the attribute (must be between 1 and 10).</param>
    /// <returns>
    /// An <see cref="AttributeModificationResult"/> indicating success or failure.
    /// On success, contains the new state with the modified attribute.
    /// On failure, contains an error message explaining why the modification was rejected.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Modification is rejected in the following cases:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>State is in Simple mode (manual modification not allowed)</description></item>
    ///   <item><description>Target value is outside the valid range [1, 10]</description></item>
    ///   <item><description>Insufficient remaining points for the cost of the increase</description></item>
    /// </list>
    /// <para>
    /// Decreasing an attribute always succeeds (within range) because it refunds points
    /// rather than spending them.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = service.CreateAllocationState(AttributeAllocationMode.Advanced);
    ///
    /// // Successful increase
    /// var result = service.TryModifyAttribute(state, CoreAttribute.Might, 5);
    /// // result.Success == true, result.PointsSpent == 4
    ///
    /// // Rejected: insufficient points
    /// var fail = service.TryModifyAttribute(state, CoreAttribute.Might, 10);
    /// // fail.Success may be false if not enough points
    /// </code>
    /// </example>
    AttributeModificationResult TryModifyAttribute(
        AttributeAllocationState state,
        CoreAttribute attribute,
        int newValue);

    /// <summary>
    /// Calculates the point cost to change an attribute from one value to another.
    /// </summary>
    /// <param name="fromValue">The current attribute value (1-10).</param>
    /// <param name="toValue">The target attribute value (1-10).</param>
    /// <returns>
    /// Points to spend (positive) for increases, or points to refund (negative) for decreases.
    /// Returns 0 when <paramref name="fromValue"/> equals <paramref name="toValue"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// service.CalculateCost(1, 5);   // 4 (4 × 1 point each, standard tier)
    /// service.CalculateCost(8, 10);  // 4 (2 × 2 points each, premium tier)
    /// service.CalculateCost(10, 8);  // -4 (refund of 2 premium increments)
    /// service.CalculateCost(5, 5);   // 0 (no change)
    /// </code>
    /// </example>
    int CalculateCost(int fromValue, int toValue);

    /// <summary>
    /// Validates that the current allocation state is complete and valid.
    /// </summary>
    /// <param name="state">The allocation state to validate.</param>
    /// <returns>
    /// An <see cref="AllocationValidationResult"/> indicating whether the state is valid.
    /// Contains error messages for any constraint violations found.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validation checks the following constraints:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>All attribute values are within the valid range [1, 10]</description></item>
    ///   <item><description>Points are not overspent (PointsRemaining >= 0)</description></item>
    ///   <item><description>Simple mode has an archetype selected</description></item>
    /// </list>
    /// <para>
    /// All constraints are checked simultaneously — the result may contain
    /// multiple error messages if multiple violations are found.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = service.CreateAllocationState(
    ///     AttributeAllocationMode.Simple, "warrior");
    /// var validation = service.ValidateAllocation(state);
    /// // validation.IsValid == true
    ///
    /// // Check before proceeding to next creation step
    /// if (!validation.IsValid)
    /// {
    ///     foreach (var error in validation.Errors)
    ///         ShowError(error);
    /// }
    /// </code>
    /// </example>
    AllocationValidationResult ValidateAllocation(AttributeAllocationState state);

    /// <summary>
    /// Switches the allocation mode, transforming the state accordingly.
    /// </summary>
    /// <param name="currentState">The current allocation state to switch from.</param>
    /// <param name="newMode">The target allocation mode.</param>
    /// <param name="archetypeId">
    /// Required when switching to Simple mode (determines which recommended build to apply).
    /// Ignored when switching to Advanced mode.
    /// Case-insensitive.
    /// </param>
    /// <returns>
    /// A new <see cref="AttributeAllocationState"/> in the target mode.
    /// Returns <paramref name="currentState"/> unchanged if already in the target mode.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when switching to Simple mode and <paramref name="archetypeId"/>
    /// is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Mode switching behavior:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <strong>To Simple:</strong> Discards current values and applies the
    ///       archetype's recommended build. All points are considered spent.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>To Advanced:</strong> Preserves current attribute values but
    ///       recalculates points spent based on the point-buy cost table.
    ///       Enables manual adjustment. Clears the archetype selection.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Start in Simple mode with Warrior build
    /// var simple = service.CreateAllocationState(
    ///     AttributeAllocationMode.Simple, "warrior");
    ///
    /// // Switch to Advanced, keeping Warrior's values
    /// var advanced = service.SwitchMode(
    ///     simple, AttributeAllocationMode.Advanced);
    /// // advanced.CurrentMight == 4, advanced.Mode == Advanced
    ///
    /// // Switch back to Simple with Mystic build
    /// var mystic = service.SwitchMode(
    ///     advanced, AttributeAllocationMode.Simple, "mystic");
    /// // mystic.CurrentWill == 4, mystic.Mode == Simple
    /// </code>
    /// </example>
    AttributeAllocationState SwitchMode(
        AttributeAllocationState currentState,
        AttributeAllocationMode newMode,
        string? archetypeId = null);

    /// <summary>
    /// Resets the allocation to initial defaults for the specified mode.
    /// </summary>
    /// <param name="mode">The allocation mode to reset to.</param>
    /// <param name="archetypeId">
    /// Required for Simple mode (determines the recommended build).
    /// Optional for Advanced mode (determines starting points).
    /// Case-insensitive.
    /// </param>
    /// <returns>
    /// A fresh <see cref="AttributeAllocationState"/> as if
    /// <see cref="CreateAllocationState"/> were called with the same parameters.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="mode"/> is <see cref="AttributeAllocationMode.Simple"/>
    /// and <paramref name="archetypeId"/> is null, empty, or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Reset Advanced mode to all-1s with full point pool
    /// var fresh = service.ResetAllocation(AttributeAllocationMode.Advanced);
    /// // fresh.CurrentMight == 1, fresh.PointsRemaining == 15
    ///
    /// // Reset Simple mode with a specific archetype
    /// var warrior = service.ResetAllocation(
    ///     AttributeAllocationMode.Simple, "warrior");
    /// // warrior.CurrentMight == 4, warrior.IsComplete == true
    /// </code>
    /// </example>
    AttributeAllocationState ResetAllocation(
        AttributeAllocationMode mode,
        string? archetypeId = null);
}
