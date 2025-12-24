using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements procedural description generation using the Three-Tier Composition model.
/// Combines base templates with biome modifiers and danger-level details.
/// All descriptions follow AAM-VOICE (Domain 4) compliance.
/// </summary>
/// <remarks>See: SPEC-DESC-001 for Descriptor Engine design.</remarks>
public class DescriptorEngine : IDescriptorEngine
{
    private readonly ILogger<DescriptorEngine> _logger;

    /// <summary>
    /// Biome-specific atmospheric modifier phrases.
    /// These are appended to base descriptions based on room biome.
    /// </summary>
    private static readonly Dictionary<BiomeType, string[]> BiomeModifiers = new()
    {
        [BiomeType.Ruin] = new[]
        {
            "Dust motes drift through shafts of pale light.",
            "Ancient stonework crumbles beneath your touch.",
            "The air tastes of ages-old decay.",
            "Weathered carvings speak of forgotten purpose.",
            "Debris crunches underfoot with each step."
        },
        [BiomeType.Industrial] = new[]
        {
            "Corroded pipes weep rust-colored stains.",
            "The tang of oxidized metal fills your nostrils.",
            "Dormant machinery looms in the shadows.",
            "Oil-slicked surfaces gleam dimly.",
            "Mechanical groans echo from somewhere distant."
        },
        [BiomeType.Organic] = new[]
        {
            "Pale fungal growths pulse with faint luminescence.",
            "Tendrils of corruption creep across every surface.",
            "The air hangs thick with spores.",
            "Sickly vegetation chokes the passage.",
            "Something squelches unseen in the darkness."
        },
        [BiomeType.Void] = new[]
        {
            "Shadows seem to swallow the light itself.",
            "An oppressive silence presses against your ears.",
            "The darkness feels almost tangible here.",
            "Your footsteps echo into endless nothing.",
            "A chill emanates from the emptiness ahead."
        }
    };

    /// <summary>
    /// Danger-level specific detail phrases.
    /// These convey threat level through atmospheric description.
    /// </summary>
    private static readonly Dictionary<DangerLevel, string[]> DangerDetails = new()
    {
        [DangerLevel.Safe] = new[]
        {
            "An uneasy calm pervades this place.",
            "Nothing stirs beyond the settling dust.",
            "The immediate area seems undisturbed.",
            "For now, the shadows hold nothing but darkness."
        },
        [DangerLevel.Unstable] = new[]
        {
            "The floor trembles with uncertain stability.",
            "Cracks spider across the walls, threatening collapse.",
            "Something groans within the structure itself.",
            "Each step must be placed with care."
        },
        [DangerLevel.Hostile] = new[]
        {
            "Movement flickers at the edge of perception.",
            "Something watches from the darkness.",
            "The air carries the scent of recent violence.",
            "Your instincts scream warning."
        },
        [DangerLevel.Lethal] = new[]
        {
            "Death waits here with patient certainty.",
            "Every shadow promises violence.",
            "The stench of carnage is overwhelming.",
            "Survival is far from assured."
        }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptorEngine"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public DescriptorEngine(ILogger<DescriptorEngine> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string ComposeDescription(string baseTemplate, string? modifier, string? detail)
    {
        _logger.LogDebug("Composing description with base length {BaseLength}, modifier: {HasModifier}, detail: {HasDetail}",
            baseTemplate.Length, modifier != null, detail != null);

        var parts = new List<string> { baseTemplate.Trim() };

        if (!string.IsNullOrWhiteSpace(modifier))
        {
            parts.Add(modifier.Trim());
        }

        if (!string.IsNullOrWhiteSpace(detail))
        {
            parts.Add(detail.Trim());
        }

        var composed = string.Join(" ", parts);

        _logger.LogDebug("Composed description: {Length} characters from {PartCount} parts",
            composed.Length, parts.Count);

        return composed;
    }

    /// <inheritdoc/>
    public string GetModifierForBiome(BiomeType biome)
    {
        _logger.LogDebug("Selecting modifier for biome {Biome}", biome);

        if (!BiomeModifiers.TryGetValue(biome, out var modifiers) || modifiers.Length == 0)
        {
            _logger.LogWarning("No modifiers defined for biome {Biome}", biome);
            return string.Empty;
        }

        var index = Random.Shared.Next(modifiers.Length);
        var selected = modifiers[index];

        _logger.LogDebug("Selected biome modifier index {Index}: \"{Modifier}\"", index, selected);

        return selected;
    }

    /// <inheritdoc/>
    public string GetDetailForDangerLevel(DangerLevel danger)
    {
        _logger.LogDebug("Selecting detail for danger level {DangerLevel}", danger);

        if (!DangerDetails.TryGetValue(danger, out var details) || details.Length == 0)
        {
            _logger.LogWarning("No details defined for danger level {DangerLevel}", danger);
            return string.Empty;
        }

        var index = Random.Shared.Next(details.Length);
        var selected = details[index];

        _logger.LogDebug("Selected danger detail index {Index}: \"{Detail}\"", index, selected);

        return selected;
    }

    /// <inheritdoc/>
    public string GenerateRoomDescription(string baseDescription, BiomeType biome, DangerLevel danger)
    {
        _logger.LogInformation("Generating room description for biome {Biome}, danger {DangerLevel}",
            biome, danger);

        var modifier = GetModifierForBiome(biome);
        var detail = GetDetailForDangerLevel(danger);

        var fullDescription = ComposeDescription(baseDescription, modifier, detail);

        _logger.LogDebug("Generated full room description: {Length} characters", fullDescription.Length);

        return fullDescription;
    }
}
