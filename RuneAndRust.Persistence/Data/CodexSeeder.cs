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
                "Psychic Stress",
                "We call it the Shakes, the Rattle, the Long Blink. It ain't just fear. " +
                "It's your mind rejecting what the eyes see. Witness enough horror, " +
                "and the walls crumble. Shadows lengthen. Whispers start. " +
                "We've seen good scavengers lost to the madness before the beasts ever touched them. " +
                "Rest often. Drink deep. Don't let the dark in."),

            CreateFieldGuideEntry(
                "Combat Basics",
                "The Rust-lands don't forgive a clumsy blade. Every swing costs you breath, " +
                "every scratch invites the rot. Don't just flail—watch your enemy. " +
                "Use the terrain. Strike when they falter. And if the odds turn? " +
                "Run. There's no shame in living to scavenge another day."),

            CreateFieldGuideEntry(
                "Burden and Carrying Capacity",
                "What you carry defines what you can do. Every piece of salvage, every weapon, " +
                "every morsel of food adds to your Burden. When Burden grows too heavy, " +
                "movement slows, fatigue sets in faster, and escape becomes difficult. " +
                "A Scavenger learns to balance need against weight, abandoning the merely useful " +
                "to carry the essential."),

            // Bestiary Entry (Sample Lore)
            new CodexEntry
            {
                Title = "Rusted Servitor",
                Category = EntryCategory.Bestiary,
                TotalFragments = 4,
                FullText =
                    "OBSERVATION: The Rusted Servitor shambles through the ruins of its former masters' works, " +
                    "a clockwork-husk of ancient Aesir design. Standing roughly as tall as a grown man, " +
                    "its frame is corroded beyond recognition, joints seizing with each labored movement.\n\n" +
                    "ENGAGEMENT: The Servitor's power core, visible as a faint amber glow through gaps " +
                    "in its chest plating, remains vulnerable to piercing strikes. Destroying it " +
                    "causes immediate collapse.\n\n" +
                    "HABITAT: Found wandering industrial complexes, maintenance tunnels, and " +
                    "locations where Aesir machinery once operated. They seem drawn to " +
                    "operational power sources.\n\n" +
                    "BEHAVIOR: Without direction from their extinct masters, Servitors follow " +
                    "corrupted maintenance protocols. They patrol fixed routes, repair nothing, " +
                    "and attack any organic life that enters their designated zones.\n\n" +
                    "AFTERMATH: Some Scavengers report Servitors that pause mid-attack, as if receiving " +
                    "orders from a source long silent. These moments pass quickly, and the violence resumes.",
                UnlockThresholds = new Dictionary<int, string>
                {
                    { 25, "ENGAGEMENT_REVEALED" },
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
