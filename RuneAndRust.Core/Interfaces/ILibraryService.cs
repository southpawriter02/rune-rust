using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Provides access to system-generated Field Guide entries via reflection.
/// The Dynamic Knowledge Engine scans for [GameDocument] attributes at startup
/// and creates transient CodexEntry objects for display in the Scavenger's Journal.
/// </summary>
/// <remarks>
/// <para>
/// System-generated entries are NOT persisted to the database. They are
/// created on-demand and cached in memory for the session lifetime.
/// This ensures in-game documentation stays synchronized with actual code.
/// </para>
/// <para>
/// Entry IDs are deterministic (MD5 hash of TypeName:MemberName) so
/// references to system entries remain stable across sessions.
/// </para>
/// </remarks>
public interface ILibraryService
{
    /// <summary>
    /// Gets all system-generated entries from attributed types and members.
    /// Results are cached after the first scan.
    /// </summary>
    /// <returns>A collection of transient CodexEntry objects.</returns>
    IEnumerable<CodexEntry> GetSystemEntries();

    /// <summary>
    /// Gets system-generated entries filtered by category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>Entries matching the specified category.</returns>
    IEnumerable<CodexEntry> GetEntriesByCategory(EntryCategory category);

    /// <summary>
    /// Gets a specific system entry by its deterministic ID.
    /// </summary>
    /// <param name="id">The entry ID (MD5 hash-based GUID).</param>
    /// <returns>The matching entry, or null if not found.</returns>
    CodexEntry? GetEntryById(Guid id);
}
