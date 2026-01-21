// ═══════════════════════════════════════════════════════════════════════════════
// IComponentLifecycle.cs
// Defines the standard lifecycle contract for presentation components.
// Version: 0.13.5c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Interfaces;

/// <summary>
/// Defines the standard lifecycle contract for presentation components.
/// </summary>
/// <remarks>
/// <para>
/// This interface establishes a consistent lifecycle pattern for all presentation
/// components across TUI and GUI layers, enabling standardized initialization,
/// activation, deactivation, and disposal handling.
/// </para>
/// <para>
/// <b>Lifecycle State Machine:</b>
/// <code>
/// Created → Initialize() → Initialized
///                               ↓
///                         Activate() ←→ Deactivate()
///                               ↓
///                         Dispose() → Disposed
/// </code>
/// </para>
/// <para>
/// <b>State Transitions:</b>
/// <list type="bullet">
///   <item><description>Created: Component constructed via DI, not yet initialized</description></item>
///   <item><description>Initialized: One-time setup complete, ready for activation</description></item>
///   <item><description>Active: Component visible/in-use, can be deactivated and reactivated</description></item>
///   <item><description>Inactive: Component hidden/backgrounded, can be reactivated</description></item>
///   <item><description>Disposed: Component destroyed, cannot be reused</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyComponent : TuiComponentBase
/// {
///     protected override void OnInitialize()
///     {
///         // One-time setup (load configs, create resources)
///     }
///     
///     protected override void OnActivate()
///     {
///         // Subscribe to events, start animations
///     }
///     
///     protected override void OnDeactivate()
///     {
///         // Unsubscribe from events, pause animations
///     }
///     
///     protected override void OnDispose()
///     {
///         // Release unmanaged resources
///     }
/// }
/// </code>
/// </example>
public interface IComponentLifecycle : IDisposable
{
    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE STATE PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether <see cref="Initialize"/> has been called.
    /// </summary>
    /// <value>
    /// <c>true</c> if the component has been initialized; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Once set to <c>true</c>, this value never changes back to <c>false</c>.
    /// Initialization is a one-time operation.
    /// </remarks>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets a value indicating whether the component is currently active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the component is active; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// A component is active when it is visible and should respond to user input.
    /// This value toggles between <c>true</c> and <c>false</c> as the component
    /// is activated and deactivated throughout its lifetime.
    /// </remarks>
    bool IsActive { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Performs one-time initialization after construction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Called once before the first use of the component. This method should
    /// perform expensive setup operations that only need to happen once, such as:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Loading configuration data</description></item>
    ///   <item><description>Creating reusable resources</description></item>
    ///   <item><description>Validating dependencies</description></item>
    ///   <item><description>Establishing event subscriptions that persist across activations</description></item>
    /// </list>
    /// <para>
    /// This method is idempotent - calling it multiple times has no additional effect
    /// after the first call.
    /// </para>
    /// </remarks>
    void Initialize();

    /// <summary>
    /// Activates the component, making it visible and responsive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Called when the component should become visible and start responding to
    /// user input. This method may be called multiple times during the component's
    /// lifetime as it is shown and hidden.
    /// </para>
    /// <para>
    /// If <see cref="Initialize"/> has not been called, it will be called automatically
    /// before activation proceeds.
    /// </para>
    /// <para>
    /// Typical activation actions include:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Starting animations</description></item>
    ///   <item><description>Subscribing to transient events</description></item>
    ///   <item><description>Refreshing displayed data</description></item>
    ///   <item><description>Announcing to screen readers (accessibility)</description></item>
    /// </list>
    /// </remarks>
    void Activate();

    /// <summary>
    /// Deactivates the component, making it hidden and non-responsive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Called when the component should become hidden and stop responding to
    /// user input. This method may be called multiple times during the component's
    /// lifetime as it is shown and hidden.
    /// </para>
    /// <para>
    /// Typical deactivation actions include:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Pausing/stopping animations</description></item>
    ///   <item><description>Unsubscribing from transient events</description></item>
    ///   <item><description>Releasing temporary resources</description></item>
    ///   <item><description>Saving component state</description></item>
    /// </list>
    /// <para>
    /// If the component is not currently active, calling this method has no effect.
    /// </para>
    /// </remarks>
    void Deactivate();
}
