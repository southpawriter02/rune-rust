namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of effects that can be triggered by interactive objects when they change state.
/// </summary>
/// <remarks>
/// Effects define what happens to a target object when a source object (like a lever or button)
/// enters a specific state. For example, pulling a lever (source enters Active state) might
/// unlock a door (target receives UnlockTarget effect).
/// </remarks>
public enum EffectType
{
    /// <summary>
    /// Opens the target object.
    /// </summary>
    /// <remarks>
    /// Changes target state from Closed to Open.
    /// Fails if target is locked or cannot be opened.
    /// </remarks>
    OpenTarget,

    /// <summary>
    /// Closes the target object.
    /// </summary>
    /// <remarks>
    /// Changes target state from Open to Closed.
    /// </remarks>
    CloseTarget,

    /// <summary>
    /// Unlocks the target object.
    /// </summary>
    /// <remarks>
    /// Changes target state from Locked to Closed.
    /// Does not consume keys or require lockpicking.
    /// </remarks>
    UnlockTarget,

    /// <summary>
    /// Locks the target object.
    /// </summary>
    /// <remarks>
    /// Changes target state from Closed to Locked.
    /// Requires target to have a lock definition.
    /// </remarks>
    LockTarget,

    /// <summary>
    /// Toggles the target between open/closed or active/inactive states.
    /// </summary>
    /// <remarks>
    /// If target is Open/Active, changes to Closed/Inactive.
    /// If target is Closed/Inactive, changes to Open/Active.
    /// </remarks>
    ToggleTarget,

    /// <summary>
    /// Activates the target object.
    /// </summary>
    /// <remarks>
    /// Changes target state to Active.
    /// Useful for triggering mechanisms.
    /// </remarks>
    ActivateTarget,

    /// <summary>
    /// Deactivates the target object.
    /// </summary>
    /// <remarks>
    /// Changes target state to Inactive.
    /// </remarks>
    DeactivateTarget,

    /// <summary>
    /// Destroys or breaks the target object.
    /// </summary>
    /// <remarks>
    /// Changes target state to Destroyed.
    /// Clears passage if target was blocking.
    /// </remarks>
    DestroyTarget,

    /// <summary>
    /// Reveals a hidden object, making it visible.
    /// </summary>
    /// <remarks>
    /// Sets target's IsVisible property to true.
    /// Used for secret doors, hidden compartments, etc.
    /// </remarks>
    RevealTarget,

    /// <summary>
    /// Displays a message without changing any state.
    /// </summary>
    /// <remarks>
    /// Used for atmospheric effects or feedback.
    /// TargetObjectId can be empty for this effect type.
    /// </remarks>
    Message
}
