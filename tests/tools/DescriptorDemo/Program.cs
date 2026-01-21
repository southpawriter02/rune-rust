// Standalone console demo for the Three-Tier Descriptor Composition System
// Run with: dotnet run --project tools/DescriptorDemo

using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Providers;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var repository = new SeederBasedDescriptorRepository();
var service = new RoomDescriptorService(repository);

// Parse command line args
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
var seed = cmdArgs.Length > 0 && int.TryParse(cmdArgs[0], out var s) ? s : Environment.TickCount;
var random = new Random(seed);

Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     THREE-TIER DESCRIPTOR COMPOSITION SYSTEM DEMO                  ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"Seed: {seed} (pass a number as argument to set seed)");
Console.WriteLine();

// Statistics
var totalFragments = repository.GetFragments(FragmentCategory.Spatial).Count +
                     repository.GetFragments(FragmentCategory.Architectural).Count +
                     repository.GetFragments(FragmentCategory.Detail).Count +
                     repository.GetFragments(FragmentCategory.Atmospheric).Count +
                     repository.GetFragments(FragmentCategory.Direction).Count;

Console.WriteLine("┌─────────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ STATISTICS                                                          │");
Console.WriteLine("├─────────────────────────────────────────────────────────────────────┤");
Console.WriteLine($"│ Base Templates:        {repository.GetAllBaseTemplates().Count,4}                                        │");
Console.WriteLine($"│ Thematic Modifiers:    {repository.GetAllModifiers().Count,4}                                        │");
Console.WriteLine($"│ Room Functions:        {repository.GetAllFunctions().Count,4}                                        │");
Console.WriteLine($"│ Total Fragments:       {totalFragments,4}                                        │");
Console.WriteLine("│   Spatial:             {0,4}                                        │", repository.GetFragments(FragmentCategory.Spatial).Count);
Console.WriteLine("│   Architectural:       {0,4}                                        │", repository.GetFragments(FragmentCategory.Architectural).Count);
Console.WriteLine("│   Detail:              {0,4}                                        │", repository.GetFragments(FragmentCategory.Detail).Count);
Console.WriteLine("│   Atmospheric:         {0,4}                                        │", repository.GetFragments(FragmentCategory.Atmospheric).Count);
Console.WriteLine("│   Direction:           {0,4}                                        │", repository.GetFragments(FragmentCategory.Direction).Count);
Console.WriteLine("└─────────────────────────────────────────────────────────────────────┘");

// Estimated combinations
var templates = repository.GetAllBaseTemplates().Count;
var modifiers = repository.GetAllModifiers().Count;
var spatial = repository.GetFragments(FragmentCategory.Spatial).Count;
var architectural = repository.GetFragments(FragmentCategory.Architectural).Count;
var detail = repository.GetFragments(FragmentCategory.Detail).Count;
var atmospheric = repository.GetFragments(FragmentCategory.Atmospheric).Count;
var estimatedCombinations = (long)templates * modifiers * spatial * architectural * detail * atmospheric;
Console.WriteLine($"\nEstimated unique combinations: {estimatedCombinations:N0}");
Console.WriteLine();

// Sample rooms by biome
var biomes = new[] { Biome.TheRoots, Biome.Muspelheim, Biome.Niflheim, Biome.Alfheim, Biome.Jotunheim };
var archetypes = new[] { RoomArchetype.Chamber, RoomArchetype.Corridor, RoomArchetype.Junction };

foreach (var biome in biomes)
{
    var modifier = repository.GetModifier(biome);
    var effects = modifier.GetEffectTags().ToList();

    Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
    Console.WriteLine($"  BIOME: {biome}");
    Console.WriteLine($"  Modifier: {modifier.Name} | Adjective: \"{modifier.Adjective}\"");
    Console.WriteLine($"  Effects: {(effects.Any() ? string.Join(", ", effects) : "(none)")}");
    Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
    Console.WriteLine();

    foreach (var archetype in archetypes)
    {
        var template = repository.GetBaseTemplate(archetype);
        if (template == null) continue;

        var name = service.GenerateRoomName(template, modifier);
        var description = service.GenerateRoomDescription(
            template, modifier, new[] { "abandoned", "dark" }, random);

        Console.WriteLine($"  ╭─ {archetype.ToString().ToUpper()} ─────────────────────────────────────────────────");
        Console.WriteLine($"  │ {name}");
        Console.WriteLine($"  │");

        // Word wrap description at ~65 chars
        var words = description.Split(' ');
        var line = "  │ ";
        foreach (var word in words)
        {
            if (line.Length + word.Length > 70)
            {
                Console.WriteLine(line);
                line = "  │ " + word + " ";
            }
            else
            {
                line += word + " ";
            }
        }
        if (line.Trim().Length > 3)
            Console.WriteLine(line);

        Console.WriteLine($"  ╰───────────────────────────────────────────────────────────────────");
        Console.WriteLine();
    }
}

// Room functions demo
Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
Console.WriteLine("  ROOM FUNCTIONS DEMO (Muspelheim Chambers)");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
Console.WriteLine();

var functions = repository.GetFunctionsByBiome(Biome.Muspelheim);
var chamberTemplate = repository.GetBaseTemplate(RoomArchetype.Chamber)!;
var muspelheimModifier = repository.GetModifier(Biome.Muspelheim);

foreach (var function in functions.Take(5))
{
    var name = service.GenerateRoomName(chamberTemplate, muspelheimModifier, function);
    var description = service.GenerateRoomDescription(
        chamberTemplate, muspelheimModifier, Array.Empty<string>(), random, function);

    Console.WriteLine($"  ╭─ FUNCTION: {function.FunctionName.ToUpper()} ─────────────────────────────────────");
    Console.WriteLine($"  │ {name}");
    Console.WriteLine($"  │");

    var words = description.Split(' ');
    var line = "  │ ";
    foreach (var word in words)
    {
        if (line.Length + word.Length > 70)
        {
            Console.WriteLine(line);
            line = "  │ " + word + " ";
        }
        else
        {
            line += word + " ";
        }
    }
    if (line.Trim().Length > 3)
        Console.WriteLine(line);

    Console.WriteLine($"  ╰───────────────────────────────────────────────────────────────────");
    Console.WriteLine();
}

// Variety test
Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
Console.WriteLine("  VARIETY TEST: 10 Chamber descriptions with same biome");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
Console.WriteLine();

var varietyModifier = repository.GetModifier(Biome.TheRoots);
for (var i = 0; i < 10; i++)
{
    var desc = service.GenerateRoomDescription(
        chamberTemplate, varietyModifier, Array.Empty<string>(), new Random(seed + i));

    // Truncate for display
    if (desc.Length > 100)
        desc = desc[..97] + "...";

    Console.WriteLine($"  [{i + 1:D2}] {desc}");
}

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
Console.WriteLine("  Demo complete. Run with different seed for different output.");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
