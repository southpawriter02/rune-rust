using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Composes descriptions using tag propagation and combination rules.
/// Tags from rooms and entities combine to produce contextual modifiers.
/// </summary>
public class TagBasedDescriptorComposer : IDescriptorComposer
{
    // Tag combination rules: (tag1, tag2) -> modifier text
    private readonly Dictionary<(string, string), string> _tagCombinations;

    // Single tag modifiers
    private readonly Dictionary<string, string[]> _tagModifiers;

    // Monster presence templates
    private readonly string[] _hostilePresenceTemplates;
    private readonly string[] _multipleEnemiesTemplates;

    public TagBasedDescriptorComposer()
    {
        // Initialize tag combination rules
        _tagCombinations = new Dictionary<(string, string), string>()
        {
            // Environmental + Movement combinations
            [("Heavy", "Wet")] = "The sound of sloshing footsteps echoes through the dampness.",
            [("Wet", "Heavy")] = "The sound of sloshing footsteps echoes through the dampness.",
            [("Cold", "Hot")] = "Steam hisses where opposing elements meet.",
            [("Hot", "Cold")] = "Steam hisses where opposing elements meet.",
            [("Fire", "Wet")] = "Steam rises as fire meets moisture.",
            [("Wet", "Fire")] = "Steam rises as fire meets moisture.",
            [("Organic", "Mechanical")] = "Nature and machine intertwine in unsettling ways.",
            [("Mechanical", "Organic")] = "Nature and machine intertwine in unsettling ways.",
            [("Ancient", "Runic")] = "Ancient runes pulse with forgotten power.",
            [("Runic", "Ancient")] = "Ancient runes pulse with forgotten power.",
            [("Dark", "Glowing")] = "Faint light struggles against the pressing darkness.",
            [("Glowing", "Dark")] = "Faint light struggles against the pressing darkness.",
            [("Stealthy", "Quiet")] = "Something moves in the silence, barely perceptible.",
            [("Quiet", "Stealthy")] = "Something moves in the silence, barely perceptible.",
            [("Flying", "Cramped")] = "Wings beat against the low ceiling.",
            [("Cramped", "Flying")] = "Wings beat against the low ceiling.",
            [("Massive", "Small")] = "The creature seems almost comical in this confined space.",
            [("Small", "Massive")] = "The creature seems almost comical in this confined space.",
            [("Toxic", "Organic")] = "A sickly-sweet smell of decay and poison fills the air.",
            [("Organic", "Toxic")] = "A sickly-sweet smell of decay and poison fills the air.",
            [("Undead", "Sacred")] = "The undead recoil from the sacred presence.",
            [("Sacred", "Undead")] = "The undead recoil from the sacred presence.",
        };

        // Single tag atmosphere modifiers
        _tagModifiers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Cold"] = ["The chill seeps into your bones.", "Frost clings to every surface.", "Your breath mists in the frigid air."],
            ["Hot"] = ["Waves of heat distort the air.", "Sweat beads on your skin instantly.", "The heat is oppressive."],
            ["Wet"] = ["Moisture drips from above.", "The floor is slick with water.", "The air is thick with humidity."],
            ["Dark"] = ["Shadows pool in every corner.", "The darkness feels alive.", "Light struggles to penetrate the gloom."],
            ["Ancient"] = ["The weight of ages presses down.", "History whispers from the walls.", "Time has left its mark here."],
            ["Organic"] = ["Living growth covers every surface.", "The walls seem to breathe.", "Life thrives in unexpected ways."],
            ["Mechanical"] = ["Gears click and whir in the distance.", "The smell of oil and metal fills the air.", "Machinery hums with dormant purpose."],
            ["Runic"] = ["Runes pulse with faint light.", "Ancient symbols cover the walls.", "Power thrums through carved stones."],
            ["Massive"] = ["The scale is beyond human measure.", "Everything here dwarfs you.", "Giants once walked these halls."],
            ["Volcanic"] = ["The ground trembles with geological fury.", "Lava's glow paints everything orange.", "The earth itself seems angry."],
            ["Icy"] = ["Ice crystals form on every surface.", "The cold is absolute.", "Frozen beauty conceals deadly danger."],
            ["Spores"] = ["Clouds of spores drift lazily.", "Each breath tastes of fungal growth.", "The air itself seems alive."],
            ["Quiet"] = ["Silence hangs heavy.", "Even your heartbeat seems too loud.", "The stillness is unnerving."],
            ["Ominous"] = ["A sense of dread pervades.", "Something terrible waits.", "Every instinct screams danger."],
        };

        _hostilePresenceTemplates =
        [
            "Something lurks here.",
            "You are not alone.",
            "Hostile eyes watch from the shadows.",
            "Movement catches your attention.",
            "A presence makes itself known.",
        ];

        _multipleEnemiesTemplates =
        [
            "Multiple threats await.",
            "You are surrounded.",
            "They move to intercept you.",
            "Numbers are not in your favor.",
            "The enemy is many.",
        ];
    }

    public string ComposeRoomDescription(Room room)
    {
        var description = room.Description;
        var additions = new List<string>();

        // Add atmosphere modifiers based on tags
        var tagModifier = GetRandomTagModifier(room.Tags);
        if (tagModifier != null)
            additions.Add(tagModifier);

        // Add monster presence description
        var aliveMonsters = room.GetAliveMonsters().ToList();
        if (aliveMonsters.Count > 0)
        {
            var presenceText = aliveMonsters.Count switch
            {
                1 => GetRandomTemplate(_hostilePresenceTemplates),
                _ => GetRandomTemplate(_multipleEnemiesTemplates)
            };
            additions.Add(presenceText);
        }

        if (additions.Count > 0)
        {
            description = $"{description} {string.Join(" ", additions)}";
        }

        return description;
    }

    public string ComposeEntityDescription(Monster monster, Room room)
    {
        var baseName = monster.Name;
        var modifiers = new List<string>();

        // Get tag combination modifiers
        // This is a simplified implementation - in practice, Monster would also have Tags
        var roomTags = room.Tags.ToList();

        // Apply room context to entity description
        if (room.HasTag("Dark") && !room.HasTag("Glowing"))
            modifiers.Add("barely visible in the darkness");
        if (room.HasTag("Cold"))
            modifiers.Add("frost-rimed");
        if (room.HasTag("Hot"))
            modifiers.Add("heat-shimmering");
        if (room.HasTag("Wet"))
            modifiers.Add("dripping");
        if (room.HasTag("Spores"))
            modifiers.Add("spore-covered");

        if (modifiers.Count > 0)
        {
            return $"A {string.Join(", ", modifiers)} {baseName}";
        }

        return $"A {baseName}";
    }

    public string? GetTagModifier(IEnumerable<string> tags)
    {
        var tagList = tags.ToList();

        // Check for tag combinations first
        for (var i = 0; i < tagList.Count; i++)
        {
            for (var j = i + 1; j < tagList.Count; j++)
            {
                var key = (tagList[i], tagList[j]);
                if (_tagCombinations.TryGetValue(key, out var combinationModifier))
                    return combinationModifier;
            }
        }

        // Fall back to single tag modifiers
        return GetRandomTagModifier(tags);
    }

    private string? GetRandomTagModifier(IEnumerable<string> tags)
    {
        var random = new Random();
        var applicableModifiers = new List<string>();

        foreach (var tag in tags)
        {
            if (_tagModifiers.TryGetValue(tag, out var modifiers))
            {
                applicableModifiers.Add(modifiers[random.Next(modifiers.Length)]);
            }
        }

        if (applicableModifiers.Count == 0)
            return null;

        return applicableModifiers[random.Next(applicableModifiers.Count)];
    }

    private static string GetRandomTemplate(string[] templates)
    {
        return templates[new Random().Next(templates.Length)];
    }
}
