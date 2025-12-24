using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Attributes;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Provides access to system-generated Field Guide entries via reflection.
/// Scans the RuneAndRust.Core assembly for [GameDocument] attributes and
/// creates transient CodexEntry objects for display in the Scavenger's Journal.
/// </summary>
/// <remarks>
/// <para>
/// The Dynamic Knowledge Engine ensures in-game documentation stays
/// synchronized with actual code definitions. System-generated entries
/// are cached in memory and never persisted to the database.
/// </para>
/// <para>
/// Entry IDs are deterministic using MD5 hash of TypeName:MemberName,
/// ensuring stable references across sessions.
/// </para>
/// </remarks>
public class LibraryService : ILibraryService
{
    private readonly ILogger<LibraryService> _logger;
    private readonly object _cacheLock = new();
    private List<CodexEntry>? _cachedEntries;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public LibraryService(ILogger<LibraryService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public IEnumerable<CodexEntry> GetSystemEntries()
    {
        _logger.LogTrace("[Library] GetSystemEntries called");

        lock (_cacheLock)
        {
            if (_cachedEntries != null)
            {
                _logger.LogTrace("[Library] Returning {Count} cached entries", _cachedEntries.Count);
                return _cachedEntries;
            }
        }

        var entries = ScanAssembly();

        lock (_cacheLock)
        {
            _cachedEntries = entries;
            _logger.LogDebug("[Library] Cached {Count} system entries", _cachedEntries.Count);
        }

        return entries;
    }

    /// <inheritdoc/>
    public IEnumerable<CodexEntry> GetEntriesByCategory(EntryCategory category)
    {
        _logger.LogTrace("[Library] GetEntriesByCategory called with {Category}", category);
        return GetSystemEntries().Where(e => e.Category == category);
    }

    /// <inheritdoc/>
    public CodexEntry? GetEntryById(Guid id)
    {
        _logger.LogTrace("[Library] GetEntryById called with {Id}", id);
        return GetSystemEntries().FirstOrDefault(e => e.Id == id);
    }

    /// <summary>
    /// Scans the RuneAndRust.Core assembly for types and members with [GameDocument].
    /// </summary>
    /// <returns>A list of CodexEntry objects generated from attributed members.</returns>
    private List<CodexEntry> ScanAssembly()
    {
        var entries = new List<CodexEntry>();
        var seenIds = new HashSet<Guid>();

        // Scan the Core assembly where enums and models are defined
        var coreAssembly = typeof(EntryCategory).Assembly;
        _logger.LogTrace("[Library] Starting reflection scan of assembly {AssemblyName}", coreAssembly.GetName().Name);

        foreach (var type in coreAssembly.GetTypes())
        {
            // Check for [GameDocument] on the type itself
            var typeAttr = type.GetCustomAttribute<GameDocumentAttribute>();
            if (typeAttr != null)
            {
                var entry = CreateEntry(type.FullName ?? type.Name, null, typeAttr);
                if (TryAddEntry(entries, seenIds, entry))
                {
                    _logger.LogTrace("[Library] Generated entry: {Title} ({Category})", entry.Title, entry.Category);
                }
            }

            // For enums, check each field (enum value)
            if (type.IsEnum)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var fieldAttr = field.GetCustomAttribute<GameDocumentAttribute>();
                    if (fieldAttr != null)
                    {
                        var entry = CreateEntry(type.FullName ?? type.Name, field.Name, fieldAttr);
                        if (TryAddEntry(entries, seenIds, entry))
                        {
                            _logger.LogTrace("[Library] Generated entry: {Title} ({Category})", entry.Title, entry.Category);
                        }
                    }
                }
            }

            // For classes, check public properties and methods
            if (type.IsClass)
            {
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    var memberAttr = member.GetCustomAttribute<GameDocumentAttribute>();
                    if (memberAttr != null)
                    {
                        var entry = CreateEntry(type.FullName ?? type.Name, member.Name, memberAttr);
                        if (TryAddEntry(entries, seenIds, entry))
                        {
                            _logger.LogTrace("[Library] Generated entry: {Title} ({Category})", entry.Title, entry.Category);
                        }
                    }
                }
            }
        }

        return entries;
    }

    /// <summary>
    /// Creates a CodexEntry from a [GameDocument] attribute.
    /// </summary>
    /// <param name="typeName">The full type name.</param>
    /// <param name="memberName">The member name (null if type-level).</param>
    /// <param name="attr">The GameDocument attribute.</param>
    /// <returns>A transient CodexEntry object.</returns>
    private static CodexEntry CreateEntry(string typeName, string? memberName, GameDocumentAttribute attr)
    {
        var idSource = memberName != null ? $"{typeName}:{memberName}" : typeName;

        return new CodexEntry
        {
            Id = GenerateDeterministicId(idSource),
            Title = attr.Title,
            Category = attr.Category,
            FullText = attr.Description,
            TotalFragments = 1, // System entries are always "complete"
            UnlockThresholds = new Dictionary<int, string>
            {
                { 100, "SYSTEM_ENTRY" }
            },
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Tries to add an entry to the collection, checking for duplicate IDs.
    /// </summary>
    /// <param name="entries">The entry collection.</param>
    /// <param name="seenIds">Set of already-seen IDs.</param>
    /// <param name="entry">The entry to add.</param>
    /// <returns>True if added, false if duplicate.</returns>
    private bool TryAddEntry(List<CodexEntry> entries, HashSet<Guid> seenIds, CodexEntry entry)
    {
        if (seenIds.Contains(entry.Id))
        {
            _logger.LogWarning("[Library] Skipping duplicate entry ID for {Title}", entry.Title);
            return false;
        }

        seenIds.Add(entry.Id);
        entries.Add(entry);
        return true;
    }

    /// <summary>
    /// Generates a deterministic GUID from a string using MD5 hash.
    /// The same input always produces the same GUID.
    /// </summary>
    /// <param name="source">The source string (TypeName:MemberName format).</param>
    /// <returns>A deterministic GUID.</returns>
    public static Guid GenerateDeterministicId(string source)
    {
        var inputBytes = Encoding.UTF8.GetBytes(source);
        var hashBytes = MD5.HashData(inputBytes);
        return new Guid(hashBytes);
    }
}
