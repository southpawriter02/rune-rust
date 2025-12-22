using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Generates Domain 4-compliant narrative text for game sequences (v0.3.4c).
/// Provides immersive prologue text based on character background, lineage, and archetype.
/// </summary>
public class NarrativeService : INarrativeService
{
    private readonly ILogger<NarrativeService> _logger;

    private static readonly Dictionary<BackgroundType, (string Name, string Description)> BackgroundInfo = new()
    {
        { BackgroundType.Scavenger, ("Scavenger", "Born among the rust heaps, surviving on salvage from dead machines.") },
        { BackgroundType.Exile, ("Exile", "Cast out from your settlement for crimes real or imagined.") },
        { BackgroundType.Scholar, ("Scholar", "Seeker of forbidden knowledge from the world before the Glitch.") },
        { BackgroundType.Soldier, ("Soldier", "Former defender of a fallen outpost, survivor of countless skirmishes.") },
        { BackgroundType.Noble, ("Noble", "Descendant of the old rulers, now stripped of title and power.") },
        { BackgroundType.Cultist, ("Cultist", "Once a follower of a corrupted faith, now seeking redemption or ruin.") }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="NarrativeService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public NarrativeService(ILogger<NarrativeService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string GetPrologueText(Character character)
    {
        _logger.LogInformation("[Narrative] Generating prologue for {Name}, Background: {Background}",
            character.Name, character.Background);

        // Domain 4 compliant: No precision measurements, no modern tech terms
        var text = character.Background switch
        {
            BackgroundType.Scavenger =>
                $"The rust has always been your horizon, {character.Name}. You were born in the shadow of " +
                $"the Great Looms, scavenging the bones of dead gods while others huddled behind their walls. " +
                $"But the scrap is thinning. The whispers in the dark grow louder. You seek the {character.Archetype} path " +
                $"not for glory, but to survive the silence that follows the machines.",

            BackgroundType.Scholar =>
                $"You have read the forbidden slates, {character.Name}. You know the world is not a tragedy, " +
                $"but a broken mechanism. While others pray to the Iron Hearts for salvation, you seek the logic " +
                $"that shattered them. As a {character.Archetype}, you carry the burden of understanding—and the " +
                $"danger of knowing too much.",

            BackgroundType.Exile =>
                $"They drove you out with stones and curses, {character.Name}. Perhaps you deserved it. Perhaps not. " +
                $"The wastelands beyond the walls care nothing for the judgments of frightened folk. " +
                $"Now you walk the {character.Archetype} path alone, with only your wits and the cold stars for company.",

            BackgroundType.Soldier =>
                $"You remember the night the walls fell, {character.Name}. The screaming iron. The silence after. " +
                $"You were trained to protect, but there was nothing left to protect. Now you wander as a {character.Archetype}, " +
                $"seeking purpose in a world that has forgotten what peace means.",

            BackgroundType.Noble =>
                $"Your ancestors ruled from towers of black glass, {character.Name}. Now those towers are rubble, " +
                $"and your bloodline means nothing to the beasts that roam the ruins. You have learned to fight as a {character.Archetype}, " +
                $"to earn with blood what your name no longer grants.",

            BackgroundType.Cultist =>
                $"You once knelt before the Corroded Altar, {character.Name}. You whispered the prayers that should not be spoken. " +
                $"Something answered—and now you cannot unhear its voice. You walk the {character.Archetype} path seeking silence, " +
                $"or perhaps the courage to listen again.",

            _ =>
                $"You awake to the smell of ozone and old dust, {character.Name}. The world is broken, and it will not fix itself. " +
                $"As a {character.Archetype}, you must forge your own path through the ruins."
        };

        _logger.LogDebug("[Narrative] Prologue generated: {Length} characters", text.Length);
        return text;
    }

    /// <inheritdoc />
    public string GetBackgroundDisplayName(BackgroundType background) =>
        BackgroundInfo.TryGetValue(background, out var info) ? info.Name : background.ToString();

    /// <inheritdoc />
    public string GetBackgroundDescription(BackgroundType background) =>
        BackgroundInfo.TryGetValue(background, out var info) ? info.Description : string.Empty;
}
