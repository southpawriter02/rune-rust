namespace RuneAndRust.Presentation.Shared.Enums;

/// <summary>
/// Defines the priority levels for screen reader announcements.
/// </summary>
/// <remarks>
/// <para>Used by <see cref="Interfaces.IAccessibilityService"/> to control
/// the urgency of screen reader announcements.</para>
/// <para>Higher priority announcements may interrupt currently speaking text
/// to convey important or time-sensitive information.</para>
/// </remarks>
public enum AnnouncementPriority
{
    /// <summary>
    /// Normal priority announcement that queues after current speech.
    /// </summary>
    /// <remarks>
    /// Use for routine status updates, navigation changes, and
    /// non-urgent information that can wait in the speech queue.
    /// </remarks>
    Normal = 0,

    /// <summary>
    /// Important announcement that interrupts current speech.
    /// </summary>
    /// <remarks>
    /// Use for significant game events, combat alerts, or information
    /// that the user should be aware of promptly but not urgently.
    /// </remarks>
    Important = 1,

    /// <summary>
    /// Assertive announcement that is read immediately.
    /// </summary>
    /// <remarks>
    /// Use sparingly for critical alerts such as low health warnings,
    /// imminent danger, or errors requiring immediate user attention.
    /// Overuse can create a jarring experience for screen reader users.
    /// </remarks>
    Assertive = 2
}
