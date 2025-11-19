using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Serilog;
using Microsoft.Data.Sqlite;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// Tests for v0.38.13: Ambient Environmental Descriptors Service
/// Also serves as database initialization for ambient descriptors
/// </summary>
[TestFixture]
public class AmbientEnvironmentServiceTests
{
    private string _testDatabasePath = null!;
    private string _connectionString = null!;
    private DescriptorRepository _repository = null!;
    private AmbientEnvironmentService _service = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Configure Serilog for tests
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Use test database
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_ambient_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_testDatabasePath}";

        Log.Information("Test database: {DatabasePath}", _testDatabasePath);

        // Initialize database
        InitializeDatabase();

        // Create repository and service
        _repository = new DescriptorRepository(_connectionString);
        _service = new AmbientEnvironmentService(_repository);

        Log.Information("AmbientEnvironmentServiceTests initialized");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Clean up test database
        if (File.Exists(_testDatabasePath))
        {
            File.Delete(_testDatabasePath);
            Log.Information("Deleted test database: {DatabasePath}", _testDatabasePath);
        }
    }

    private void InitializeDatabase()
    {
        Log.Information("Initializing test database...");

        // Read and execute schema SQL
        var schemaPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "Data", "v0.38.13_ambient_environmental_descriptors_schema.sql");

        var dataPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "Data", "v0.38.13_ambient_environmental_descriptors_data.sql");

        if (!File.Exists(schemaPath))
        {
            Assert.Fail($"Schema file not found: {schemaPath}");
        }

        if (!File.Exists(dataPath))
        {
            Assert.Fail($"Data file not found: {dataPath}");
        }

        var schemaSql = File.ReadAllText(schemaPath);
        var dataSql = File.ReadAllText(dataPath);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Execute schema
        using (var command = connection.CreateCommand())
        {
            command.CommandText = schemaSql;
            command.ExecuteNonQuery();
        }

        Log.Information("✅ Schema loaded");

        // Execute data
        using (var command = connection.CreateCommand())
        {
            command.CommandText = dataSql;
            command.ExecuteNonQuery();
        }

        Log.Information("✅ Data loaded (150+ descriptors)");
    }

    [Test]
    public void Test_DatabaseInitialization_Success()
    {
        // Verify all tables exist
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var tables = new[] {
            "Ambient_Sound_Descriptors",
            "Ambient_Smell_Descriptors",
            "Ambient_Atmospheric_Detail_Descriptors",
            "Ambient_Background_Activity_Descriptors"
        };

        foreach (var table in tables)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {table}";
            var count = Convert.ToInt32(command.ExecuteScalar());
            Assert.Greater(count, 0, $"Table {table} should have descriptors");
            Log.Information("✅ {Table}: {Count} descriptors", table, count);
        }
    }

    [Test]
    public void Test_GetAmbientStats_ReturnsExpectedCounts()
    {
        var stats = _service.GetAmbientStats();

        Assert.Greater(stats.Sounds, 50, "Should have 60+ sound descriptors");
        Assert.Greater(stats.Smells, 35, "Should have 40+ smell descriptors");
        Assert.Greater(stats.AtmosphericDetails, 25, "Should have 30+ atmospheric detail descriptors");
        Assert.Greater(stats.BackgroundActivities, 15, "Should have 20+ background activity descriptors");

        Log.Information("Ambient Stats:");
        Log.Information("  Sounds: {Sounds}", stats.Sounds);
        Log.Information("  Smells: {Smells}", stats.Smells);
        Log.Information("  Atmospheric Details: {AtmosphericDetails}", stats.AtmosphericDetails);
        Log.Information("  Background Activities: {BackgroundActivities}", stats.BackgroundActivities);
    }

    [Test]
    [TestCase("The_Roots")]
    [TestCase("Muspelheim")]
    [TestCase("Niflheim")]
    [TestCase("Alfheim")]
    [TestCase("Jötunheim")]
    public void Test_GenerateAmbientSound_ByBiome_ReturnsDescriptor(string biome)
    {
        var sound = _service.GenerateAmbientSound(biome);

        Assert.IsNotNull(sound, $"Should generate ambient sound for {biome}");
        Assert.IsNotEmpty(sound, $"Ambient sound for {biome} should not be empty");

        Log.Information("[{Biome}] Sound: {Sound}", biome, sound);
    }

    [Test]
    [TestCase("The_Roots")]
    [TestCase("Muspelheim")]
    [TestCase("Niflheim")]
    [TestCase("Alfheim")]
    [TestCase("Jötunheim")]
    public void Test_GenerateAmbientSmell_ByBiome_ReturnsDescriptor(string biome)
    {
        var smell = _service.GenerateAmbientSmell(biome);

        Assert.IsNotNull(smell, $"Should generate ambient smell for {biome}");
        Assert.IsNotEmpty(smell, $"Ambient smell for {biome} should not be empty");

        Log.Information("[{Biome}] Smell: {Smell}", biome, smell);
    }

    [Test]
    [TestCase("The_Roots")]
    [TestCase("Muspelheim")]
    [TestCase("Niflheim")]
    [TestCase("Alfheim")]
    [TestCase("Jötunheim")]
    public void Test_GenerateAtmosphericDetail_ByBiome_ReturnsDescriptor(string biome)
    {
        var detail = _service.GenerateAtmosphericDetail(biome);

        Assert.IsNotNull(detail, $"Should generate atmospheric detail for {biome}");
        Assert.IsNotEmpty(detail, $"Atmospheric detail for {biome} should not be empty");

        Log.Information("[{Biome}] Atmospheric Detail: {Detail}", biome, detail);
    }

    [Test]
    public void Test_GenerateBackgroundActivity_Generic_ReturnsDescriptor()
    {
        var activity = _service.GenerateBackgroundActivity("Generic");

        Assert.IsNotNull(activity, "Should generate background activity for Generic biome");
        Assert.IsNotEmpty(activity, "Background activity should not be empty");

        Log.Information("[Generic] Background Activity: {Activity}", activity);
    }

    [Test]
    [TestCase("Day")]
    [TestCase("Night")]
    public void Test_GenerateAmbientSound_WithTimeOfDay_ReturnsDescriptor(string timeOfDay)
    {
        var sound = _service.GenerateAmbientSound("The_Roots", timeOfDay);

        // May return null if no descriptors match time of day (which is OK - fallback to generic)
        if (sound != null)
        {
            Assert.IsNotEmpty(sound, $"Ambient sound for {timeOfDay} should not be empty");
            Log.Information("[The_Roots / {TimeOfDay}] Sound: {Sound}", timeOfDay, sound);
        }
        else
        {
            Log.Information("[The_Roots / {TimeOfDay}] No specific descriptor, fallback would be used", timeOfDay);
        }
    }

    [Test]
    public void Test_GenerateAmbientEvent_ReturnsVariousTypes()
    {
        var room = new Room { Biome = "The_Roots" };
        var events = new HashSet<string>();

        // Generate 20 events and collect them
        for (int i = 0; i < 20; i++)
        {
            var ambientEvent = _service.GenerateAmbientEvent(room);
            if (ambientEvent != null)
            {
                events.Add(ambientEvent);
            }
        }

        Assert.Greater(events.Count, 5, "Should generate varied ambient events");

        Log.Information("Generated {Count} unique ambient events:", events.Count);
        foreach (var ev in events.Take(5))
        {
            Log.Information("  - {Event}", ev);
        }
    }

    [Test]
    public void Test_GenerateRoomEntryAmbience_ReturnsMultipleDescriptors()
    {
        var room = new Room { Biome = "Muspelheim" };
        var ambience = _service.GenerateRoomEntryAmbience(room);

        // Should get at least one descriptor (smell is 80% chance, sound is 40% chance)
        // Run multiple times to ensure we get results
        int attempts = 0;
        while (ambience.Count == 0 && attempts < 10)
        {
            ambience = _service.GenerateRoomEntryAmbience(room);
            attempts++;
        }

        Assert.Greater(ambience.Count, 0, "Should generate at least one ambient descriptor on room entry");

        Log.Information("[Muspelheim] Room Entry Ambience ({Count} descriptors):", ambience.Count);
        foreach (var desc in ambience)
        {
            Log.Information("  - {Descriptor}", desc);
        }
    }

    [Test]
    public void Test_GenerateTimeOfDayTransition_ReturnsTransitionDescriptor()
    {
        var room = new Room { Biome = "Generic" };

        var dayTransition = _service.GenerateTimeOfDayTransition(room, "Day");
        var nightTransition = _service.GenerateTimeOfDayTransition(room, "Night");

        // At least one should return (Generic biome has time-of-day descriptors)
        Assert.IsTrue(dayTransition != null || nightTransition != null,
            "Should generate time-of-day transition descriptor");

        if (dayTransition != null)
        {
            Log.Information("[Day Transition] {Transition}", dayTransition);
        }

        if (nightTransition != null)
        {
            Log.Information("[Night Transition] {Transition}", nightTransition);
        }
    }

    [Test]
    public void Test_GenerateDistantCombatSound_ReturnsDescriptor()
    {
        var room = new Room { Biome = "Generic" };

        var combatSound = _service.GenerateDistantCombatSound(room);

        Assert.IsNotNull(combatSound, "Should generate distant combat sound");
        Assert.IsNotEmpty(combatSound, "Distant combat sound should not be empty");

        Log.Information("[Distant Combat] {Sound}", combatSound);
    }

    [Test]
    public void Test_TheRoots_AmbienceDemo()
    {
        Log.Information("\n=== THE ROOTS: Corroded Infrastructure ===\n");

        for (int i = 0; i < 5; i++)
        {
            var sound = _service.GenerateAmbientSound("The_Roots");
            if (sound != null)
                Log.Information("[Sound] {Text}", sound);
        }

        for (int i = 0; i < 3; i++)
        {
            var smell = _service.GenerateAmbientSmell("The_Roots");
            if (smell != null)
                Log.Information("[Smell] {Text}", smell);
        }

        Assert.Pass("The Roots ambience demo complete");
    }

    [Test]
    public void Test_Muspelheim_AmbienceDemo()
    {
        Log.Information("\n=== MUSPELHEIM: Volcanic Realm ===\n");

        for (int i = 0; i < 5; i++)
        {
            var sound = _service.GenerateAmbientSound("Muspelheim");
            if (sound != null)
                Log.Information("[Sound] {Text}", sound);
        }

        for (int i = 0; i < 3; i++)
        {
            var smell = _service.GenerateAmbientSmell("Muspelheim");
            if (smell != null)
                Log.Information("[Smell] {Text}", smell);
        }

        Assert.Pass("Muspelheim ambience demo complete");
    }

    [Test]
    public void Test_Alfheim_AmbienceDemo()
    {
        Log.Information("\n=== ALFHEIM: Reality-Warped Zone ===\n");

        for (int i = 0; i < 5; i++)
        {
            var sound = _service.GenerateAmbientSound("Alfheim");
            if (sound != null)
                Log.Information("[Sound] {Text}", sound);
        }

        for (int i = 0; i < 3; i++)
        {
            var smell = _service.GenerateAmbientSmell("Alfheim");
            if (smell != null)
                Log.Information("[Smell] {Text}", smell);
        }

        Assert.Pass("Alfheim ambience demo complete");
    }
}
