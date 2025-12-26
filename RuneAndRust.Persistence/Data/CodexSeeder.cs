using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the Codex with initial entries for the Scavenger's Journal.
/// Includes Field Guide (tutorial) entries and sample Bestiary content.
/// </summary>
/// <remarks>
/// Field Guide entries are discovered via gameplay triggers (v0.1.3b).
/// This seeder only creates the CodexEntry definitions - players must
/// still discover the DataCaptures to unlock them.
///
/// All content must be Domain 4 compliant (no precision measurements).
///
/// See: SPEC-SEED-001 for Database Seeding System design.
/// Note: This seeder exists but is not currently called in Program.cs initialization.
/// </remarks>
public static class CodexSeeder
{
    /// <summary>
    /// Seeds the Codex entries if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        if (await context.CodexEntries.AnyAsync())
        {
            logger?.LogDebug("Codex entries already exist, skipping seed");
            return;
        }

        logger?.LogInformation("Seeding Codex entries...");

        var entries = GetSeedEntries();
        await context.CodexEntries.AddRangeAsync(entries);
        await context.SaveChangesAsync();

        logger?.LogInformation("Seeded {Count} Codex entries", entries.Count);
    }

    /// <summary>
    /// Gets the seed entries for initial Codex population.
    /// </summary>
    /// <returns>A list of CodexEntry entities to seed.</returns>
    public static List<CodexEntry> GetSeedEntries()
    {
        return new List<CodexEntry>
        {
            // Field Guide Entries (Tutorials)
            CreateFieldGuideEntry(
                "The Breaking Mind", // Formerly "Psychic Stress"
                "The horrors of this world weigh heavy. The Blight does not just corrupt flesh; it eats at the spirit. " +
                "When you witness things that should not be—monsters, twisted runes, the void itself—your mind begins to crack. " +
                "This is the Stress. If you let it grow, the shadows will lengthen, and you will hear whispers in the silence. " +
                "Rest, drink, and prayer may steady you, but some scars never truly fade."),

            CreateFieldGuideEntry(
                "The Art of Survival", // Formerly "Combat Basics"
                "To fight in the Rust-lands is to dance with death. Do not swing your weapon blindly; " +
                "every blow drains your breath (Stamina), and every mistake spills your blood. " +
                "Watch your enemy. Strike when they falter. And remember: a coward lives to scavenge another day. " +
                "The dead are many; the old are few."),

            CreateFieldGuideEntry(
                "The Burden of Greed", // Formerly "Burden and Carrying Capacity"
                "A Scavenger's life is measured in what they can carry. But greed is a heavy chain. " +
                "Every scrap of metal, every weapon, every meal adds to your Burden. " +
                "Carry too much, and you will tire quickly. You will be slow when the wolves come. " +
                "Learn to balance your need against your strength. Leave the rust; take the iron."),

            // Bestiary Entry (Sample Lore)
            new CodexEntry
            {
                Title = "Iron-Husk", // Formerly "Rusted Servitor"
                Category = EntryCategory.Bestiary,
                TotalFragments = 4,
                FullText =
                    "The Iron-Husk shambles through the ruins, a mockery of a man made of cold metal. " +
                    "Standing as tall as a warrior, its shell is eaten by rust, its joints screaming with every step.\n\n" +
                    "WEAKNESS: The Husk's spark-vessel—the heart of fire—glows faintly through the ribs of its chest. " +
                    "It is the seat of its false life. Smash it, and the thing will sleep forever.\n\n" +
                    "HABITAT: They are found in the deep guts of the world, wandering the halls where the Old Ones once toiled. " +
                    "They seem drawn to places where the lightning still hums.\n\n" +
                    "BEHAVIOR: Without a master to command them, they follow forgotten rites. They walk the same paths, " +
                    "mend nothing, and crush anything that breathes within their sight.\n\n" +
                    "NOTE: I have seen them pause, head tilted, as if listening to a command from a god that died centuries ago. " +
                    "Then the silence returns, and the killing begins again.",
                UnlockThresholds = new Dictionary<int, string>
                {
                    { 25, "WEAKNESS_REVEALED" },
                    { 50, "HABITAT_REVEALED" },
                    { 75, "BEHAVIOR_REVEALED" },
                    { 100, "FULL_ENTRY" }
                }
            }
        };
    }

    /// <summary>
    /// Creates a Field Guide (tutorial) entry with standard configuration.
    /// </summary>
    private static CodexEntry CreateFieldGuideEntry(string title, string fullText)
    {
        return new CodexEntry
        {
            Title = title,
            Category = EntryCategory.FieldGuide,
            TotalFragments = 1,
            FullText = fullText,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 100, "MECHANIC_UNLOCKED" }
            }
        };
    }
}
