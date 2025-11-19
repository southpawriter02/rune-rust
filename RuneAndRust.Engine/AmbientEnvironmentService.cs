using RuneAndRust.Core;
using RuneAndRust.Core.AmbientFlavor;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for generating ambient environmental events
/// v0.38.13: Ambient Environmental Descriptors
/// </summary>
public class AmbientEnvironmentService
{
    private readonly DescriptorRepository _repository;
    private static readonly ILogger _log = Log.ForContext<AmbientEnvironmentService>();

    public AmbientEnvironmentService(DescriptorRepository repository)
    {
        _repository = repository;
        _log.Information("AmbientEnvironmentService initialized");
    }

    /// <summary>
    /// Generates a random ambient event for the current room
    /// </summary>
    /// <param name="currentRoom">The current room</param>
    /// <param name="timeOfDay">Day, Night, or null for any time</param>
    /// <returns>Ambient event description text, or null if none generated</returns>
    public string? GenerateAmbientEvent(Room currentRoom, string? timeOfDay = null)
    {
        var biome = currentRoom.Biome ?? "Generic";

        // Randomly select event type (weighted toward sounds)
        var eventType = Random.Shared.Next(10);

        if (eventType < 4) // 40% chance - Ambient Sound
        {
            return GenerateAmbientSound(biome, timeOfDay);
        }
        else if (eventType < 7) // 30% chance - Ambient Smell
        {
            return GenerateAmbientSmell(biome);
        }
        else if (eventType < 9) // 20% chance - Atmospheric Detail
        {
            return GenerateAtmosphericDetail(biome, timeOfDay);
        }
        else // 10% chance - Background Activity
        {
            return GenerateBackgroundActivity(biome, timeOfDay);
        }
    }

    /// <summary>
    /// Generates an ambient sound event
    /// </summary>
    /// <param name="biome">The biome (The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic)</param>
    /// <param name="timeOfDay">Day, Night, or null for any time</param>
    /// <returns>Ambient sound description text, or null if none found</returns>
    public string? GenerateAmbientSound(string biome, string? timeOfDay = null)
    {
        var descriptor = _repository.GetRandomAmbientSoundDescriptor(biome, null, timeOfDay);

        if (descriptor != null)
        {
            _log.Debug("Generated ambient sound: {Biome} / {Category} / {Subcategory}",
                biome, descriptor.SoundCategory, descriptor.SoundSubcategory);
            return descriptor.DescriptorText;
        }

        _log.Debug("No ambient sound descriptor found for: {Biome} / {TimeOfDay}", biome, timeOfDay);
        return null;
    }

    /// <summary>
    /// Generates an ambient smell event
    /// </summary>
    /// <param name="biome">The biome (The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic)</param>
    /// <returns>Ambient smell description text, or null if none found</returns>
    public string? GenerateAmbientSmell(string biome)
    {
        var descriptor = _repository.GetRandomAmbientSmellDescriptor(biome);

        if (descriptor != null)
        {
            _log.Debug("Generated ambient smell: {Biome} / {Category} / {Subcategory}",
                biome, descriptor.SmellCategory, descriptor.SmellSubcategory);
            return descriptor.DescriptorText;
        }

        _log.Debug("No ambient smell descriptor found for: {Biome}", biome);
        return null;
    }

    /// <summary>
    /// Generates an atmospheric detail event
    /// </summary>
    /// <param name="biome">The biome (The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic)</param>
    /// <param name="timeOfDay">Day, Night, Transition, or null for any time</param>
    /// <returns>Atmospheric detail description text, or null if none found</returns>
    public string? GenerateAtmosphericDetail(string biome, string? timeOfDay = null)
    {
        var descriptor = _repository.GetRandomAmbientAtmosphericDetailDescriptor(biome, null, timeOfDay);

        if (descriptor != null)
        {
            _log.Debug("Generated atmospheric detail: {Biome} / {Category} / {Subcategory}",
                biome, descriptor.DetailCategory, descriptor.DetailSubcategory);
            return descriptor.DescriptorText;
        }

        _log.Debug("No atmospheric detail descriptor found for: {Biome} / {TimeOfDay}", biome, timeOfDay);
        return null;
    }

    /// <summary>
    /// Generates a background activity event
    /// </summary>
    /// <param name="biome">The biome (The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic)</param>
    /// <param name="timeOfDay">Day, Night, or null for any time</param>
    /// <returns>Background activity description text, or null if none found</returns>
    public string? GenerateBackgroundActivity(string biome, string? timeOfDay = null)
    {
        var descriptor = _repository.GetRandomAmbientBackgroundActivityDescriptor(biome, null, timeOfDay);

        if (descriptor != null)
        {
            _log.Debug("Generated background activity: {Biome} / {Category} / {Subcategory}",
                biome, descriptor.ActivityCategory, descriptor.ActivitySubcategory);
            return descriptor.DescriptorText;
        }

        _log.Debug("No background activity descriptor found for: {Biome} / {TimeOfDay}", biome, timeOfDay);
        return null;
    }

    /// <summary>
    /// Generates ambient descriptions when entering a room
    /// </summary>
    /// <param name="currentRoom">The room being entered</param>
    /// <returns>List of ambient descriptions (smell + optional sound)</returns>
    public List<string> GenerateRoomEntryAmbience(Room currentRoom)
    {
        var ambience = new List<string>();
        var biome = currentRoom.Biome ?? "Generic";

        // Always add a smell on room entry (80% chance)
        if (Random.Shared.NextDouble() < 0.8)
        {
            var smell = GenerateAmbientSmell(biome);
            if (smell != null)
            {
                ambience.Add(smell);
            }
        }

        // Sometimes add a sound on room entry (40% chance)
        if (Random.Shared.NextDouble() < 0.4)
        {
            var sound = GenerateAmbientSound(biome);
            if (sound != null)
            {
                ambience.Add(sound);
            }
        }

        return ambience;
    }

    /// <summary>
    /// Generates atmospheric transition description for time-of-day changes
    /// </summary>
    /// <param name="currentRoom">The current room</param>
    /// <param name="newTimeOfDay">Day or Night</param>
    /// <returns>Atmospheric transition description, or null if none found</returns>
    public string? GenerateTimeOfDayTransition(Room currentRoom, string newTimeOfDay)
    {
        var biome = currentRoom.Biome ?? "Generic";

        // Get atmospheric detail specifically for time-of-day transitions
        var descriptor = _repository.GetRandomAmbientAtmosphericDetailDescriptor(biome, "TimeOfDay", newTimeOfDay);

        if (descriptor != null)
        {
            _log.Information("Time-of-day transition: {Biome} → {TimeOfDay}", biome, newTimeOfDay);
            return descriptor.DescriptorText;
        }

        return null;
    }

    /// <summary>
    /// Generates distant combat sounds (for use during player combat)
    /// </summary>
    /// <param name="currentRoom">The current room</param>
    /// <returns>Distant combat description, or null if none found</returns>
    public string? GenerateDistantCombatSound(Room currentRoom)
    {
        var biome = currentRoom.Biome ?? "Generic";

        // Get background activity specifically for distant combat
        var descriptor = _repository.GetRandomAmbientBackgroundActivityDescriptor(biome, "DistantCombat");

        if (descriptor != null)
        {
            _log.Debug("Generated distant combat sound: {Biome}", biome);
            return descriptor.DescriptorText;
        }

        return null;
    }

    /// <summary>
    /// Gets statistics for ambient environmental descriptors
    /// </summary>
    /// <returns>Tuple with counts of (Sounds, Smells, AtmosphericDetails, BackgroundActivities)</returns>
    public (int Sounds, int Smells, int AtmosphericDetails, int BackgroundActivities) GetAmbientStats()
    {
        return _repository.GetAmbientEnvironmentalStats();
    }
}
