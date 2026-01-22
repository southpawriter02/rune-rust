using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Manages RNG contexts and their associated seeding behavior.
/// </summary>
/// <remarks>
/// <para>
/// This service maintains a stack of active contexts and coordinates
/// seed locking/unlocking based on context transitions.
/// </para>
/// <para>
/// Context stack allows nested contexts (e.g., dialogue within exploration),
/// with proper cleanup when exiting inner contexts.
/// </para>
/// </remarks>
public interface IRngContextService
{
    /// <summary>
    /// Enters a new RNG context, pushing it onto the context stack.
    /// </summary>
    /// <param name="context">The context to enter.</param>
    /// <remarks>
    /// <para>
    /// If the context has a locked seed (Combat, Crafting), the RNG
    /// will be set to that seed. Otherwise, behavior depends on context type.
    /// </para>
    /// <para>
    /// Contexts can be nested. The innermost context determines behavior.
    /// </para>
    /// </remarks>
    void EnterContext(RngContext context);

    /// <summary>
    /// Exits the current RNG context, popping it from the stack.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns to the previous context's seeding behavior.
    /// If no contexts remain, defaults to Default context.
    /// </para>
    /// <para>
    /// Does nothing if no context is currently active.
    /// </para>
    /// </remarks>
    void ExitContext();

    /// <summary>
    /// Gets the currently active RNG context.
    /// </summary>
    /// <returns>The current context, or Default if no context is active.</returns>
    RngContext GetCurrentContext();

    /// <summary>
    /// Locks a specific seed for a context.
    /// </summary>
    /// <param name="context">The context to lock.</param>
    /// <param name="seed">The seed to use when this context is active.</param>
    /// <remarks>
    /// <para>
    /// Locked seeds are used when entering the context. This prevents
    /// save-scumming by ensuring the same random sequence.
    /// </para>
    /// <para>
    /// If the specified context is currently active, the seed is
    /// applied immediately.
    /// </para>
    /// </remarks>
    void LockSeedForContext(RngContext context, int seed);

    /// <summary>
    /// Releases the seed lock for a context.
    /// </summary>
    /// <param name="context">The context to unlock.</param>
    /// <remarks>
    /// <para>
    /// After unlocking, the context will generate a fresh seed
    /// each time it becomes active.
    /// </para>
    /// <para>
    /// Typically called when a locked session ends (e.g., combat ends).
    /// </para>
    /// </remarks>
    void ReleaseSeedLock(RngContext context);

    /// <summary>
    /// Checks if a context has a locked seed.
    /// </summary>
    /// <param name="context">The context to check.</param>
    /// <returns>True if the context has a locked seed; otherwise false.</returns>
    bool IsContextSeedLocked(RngContext context);

    /// <summary>
    /// Gets the locked seed for a context, if any.
    /// </summary>
    /// <param name="context">The context to query.</param>
    /// <returns>The locked seed, or null if not locked.</returns>
    int? GetLockedSeed(RngContext context);
}
