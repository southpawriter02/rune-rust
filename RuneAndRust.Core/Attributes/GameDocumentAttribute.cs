using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Attributes;

/// <summary>
/// Marks a type or member for inclusion in the auto-generated Field Guide.
/// The LibraryService scans for this attribute at startup to create transient
/// CodexEntry objects that appear in the Scavenger's Journal.
/// </summary>
/// <remarks>
/// <para>
/// This attribute enables the Dynamic Knowledge Engine to keep in-game
/// documentation synchronized with actual code definitions. When applied
/// to an enum value, the Title and Description become the entry content.
/// </para>
/// <para>
/// Descriptions should follow AAM-VOICE guidelines:
/// - Clinical observer perspective (Jötun-Reader voice)
/// - No precision measurements (Domain 4 compliant)
/// - Focus on observable effects and remedies
/// - Epistemic uncertainty where appropriate
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
public class GameDocumentAttribute : System.Attribute
{
    /// <summary>
    /// The display title for the Field Guide entry.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The full description text displayed when viewing the entry.
    /// Should be written in AAM-VOICE (clinical, qualitative, Domain 4 compliant).
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The Codex category for this entry. Defaults to FieldGuide.
    /// </summary>
    public EntryCategory Category { get; }

    /// <summary>
    /// If true, this entry is hidden until explicitly unlocked.
    /// Secret entries are not shown in the Field Guide until discovered.
    /// </summary>
    public bool IsSecret { get; }

    /// <summary>
    /// Creates a new GameDocument attribute for Field Guide generation.
    /// </summary>
    /// <param name="title">The entry title displayed in the Journal.</param>
    /// <param name="description">The full text content of the entry.</param>
    /// <param name="category">The Codex category. Defaults to FieldGuide.</param>
    /// <param name="isSecret">Whether this entry is hidden until unlocked.</param>
    public GameDocumentAttribute(
        string title,
        string description,
        EntryCategory category = EntryCategory.FieldGuide,
        bool isSecret = false)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Category = category;
        IsSecret = isSecret;
    }
}
