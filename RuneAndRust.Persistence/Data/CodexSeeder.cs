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
                "Stress is the measure of your mind's coherence in the face of the Blight's corruption. " +
                "When you witness horrors, push beyond your limits, or encounter anomalous phenomena, " +
                "Stress accumulates. At critical levels, your perception warps—shadows lengthen, " +
                "whispers emerge from silence, and reality itself seems to fray at the edges. " +
                "Rest and certain consumables can restore clarity, but some traumas leave permanent marks."),

            CreateFieldGuideEntry(
                "Combat Basics",
                "Violence in the Rust-lands is brutal and swift. Each exchange carries weight—" +
                "every swing drains Stamina, every wound bleeds precious vitality. " +
                "The wise fighter picks their battles, uses terrain and surprise, " +
                "and knows when to retreat. The dead are many; the survivors are cautious."),

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
                    "The Rusted Servitor shambles through the ruins of its former masters' works, " +
                    "a humanoid automaton of ancient Aesir design. Standing roughly as tall as a grown man, " +
                    "its frame is corroded beyond recognition, joints seizing with each labored movement.\n\n" +
                    "WEAKNESS: The Servitor's power core, visible as a faint amber glow through gaps " +
                    "in its chest plating, remains vulnerable to piercing strikes. Destroying it " +
                    "causes immediate shutdown.\n\n" +
                    "HABITAT: Found wandering industrial complexes, maintenance tunnels, and " +
                    "locations where Aesir machinery once operated. They seem drawn to " +
                    "operational power sources.\n\n" +
                    "BEHAVIOR: Without direction from their extinct masters, Servitors follow " +
                    "corrupted maintenance protocols. They patrol fixed routes, repair nothing, " +
                    "and attack any organic life that enters their designated zones.\n\n" +
                    "NOTE: Some Scavengers report Servitors that pause mid-attack, as if receiving " +
                    "orders from a source long silent. These moments pass quickly, and the violence resumes.",
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
