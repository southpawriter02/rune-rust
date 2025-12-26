# Database Initialization Guide

## Overview

This guide explains how to initialize the `runeandrust.db` database with all required schemas and data for the Rune & Rust game, including the newly integrated v0.38.13 Ambient Environmental Descriptors.

---

## Quick Start

### Option 1: Using SQLite3 CLI (Recommended)

If you have `sqlite3` command-line tool installed:

```bash
# Navigate to project root
cd /path/to/rune-rust

# Create and initialize database (all schemas)
sqlite3 runeandrust.db < Data/INITIALIZE_DATABASE.sql

```

### Option 2: Using C# Tests (Automatic)

The database will be automatically created and initialized when you run the tests:

```bash
dotnet test --filter "FullyQualifiedName~AmbientEnvironmentServiceTests"

```

This will:

1. Create a test database
2. Load v0.38.13 ambient descriptor schema and data
3. Run all ambient environment service tests
4. Verify 150+ descriptors loaded successfully

### Option 3: Minimal Setup (v0.38.13 Only)

If you only need the ambient environmental descriptors:

```bash
# Create empty database
sqlite3 runeandrust.db ""

# Load base descriptor framework
sqlite3 runeandrust.db < Data/v0.38.0_descriptor_framework_schema.sql

# Load v0.38.13 ambient environmental descriptors
sqlite3 runeandrust.db < Data/v0.38.13_ambient_environmental_descriptors_schema.sql
sqlite3 runeandrust.db < Data/v0.38.13_ambient_environmental_descriptors_data.sql

```

---

## Verification

After initialization, verify the database:

```bash
# Check ambient sound descriptors
sqlite3 runeandrust.db "SELECT COUNT(*) FROM Ambient_Sound_Descriptors;"
# Expected: 60+

# Check ambient smell descriptors
sqlite3 runeandrust.db "SELECT COUNT(*) FROM Ambient_Smell_Descriptors;"
# Expected: 40+

# Check atmospheric detail descriptors
sqlite3 runeandrust.db "SELECT COUNT(*) FROM Ambient_Atmospheric_Detail_Descriptors;"
# Expected: 30+

# Check background activity descriptors
sqlite3 runeandrust.db "SELECT COUNT(*) FROM Ambient_Background_Activity_Descriptors;"
# Expected: 20+

# Sample ambient sounds by biome
sqlite3 runeandrust.db "SELECT biome, sound_category, descriptor_text FROM Ambient_Sound_Descriptors WHERE biome='The_Roots' LIMIT 5;"

```

---

## Database Schema (v0.38.13)

### Tables Created

1. **Ambient_Sound_Descriptors** - 60+ periodic ambient sounds
    - Biome-specific (The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic)
    - Categories: Mechanical, Decay, Eerie, Creature, Fire, Ice, Wind, Glitch, Industrial
    - Time-of-day awareness (Day, Night, NULL)
2. **Ambient_Smell_Descriptors** - 40+ environmental smells
    - Biome-specific
    - Categories: Decay, Mechanical, Organic, Fire, Cold, Chemical, Paradoxical, Industrial, Psychic
    - Intensity levels: Subtle, Moderate, Overwhelming
3. **Ambient_Atmospheric_Detail_Descriptors** - 30+ atmospheric conditions
    - Biome-specific
    - Categories: AirQuality, Temperature, Humidity, Visibility, TimeOfDay, WeatherEffect
    - Time-of-day transitions
4. **Ambient_Background_Activity_Descriptors** - 20+ background world events
    - Biome-specific
    - Categories: DistantCombat, EnvironmentalEvent, OtherSurvivors, CreatureActivity, RealityEvent
    - Distance levels: Near, Medium, Far, Uncertain

---

## Integration Status

### ✅ Implemented

- **AmbientEnvironmentService.cs** - Service for generating ambient events
- **Program.cs Integration** - Ambient events in exploration loop
- **Room Entry Ambience** - Smells and sounds when entering rooms
- **Time-of-Day Support** - Day/Night contextual ambient events
- **AmbientEnvironmentServiceTests.cs** - Comprehensive test suite

### 🔄 Partial Integration

- **Periodic Events** - Currently 30% chance per exploration loop iteration
    - Can be adjusted via configuration
    - Future: Timer-based periodic events every 30-60 seconds

### 📋 Future Enhancements

- **Time System Integration** - Proper day/night cycle with transitions
- **Weather System** - Dynamic weather affecting ambient descriptors
- **Combat Integration** - Distant combat sounds during player combat
- **Player Preferences** - Configurable ambient event frequency

---

## Troubleshooting

### Database File Not Found

If you get "unable to open database file":

```bash
# Ensure you're in the project root
cd /path/to/rune-rust

# Check database location
ls -la runeandrust.db

# If missing, initialize it
sqlite3 runeandrust.db < Data/INITIALIZE_DATABASE.sql

```

### No Ambient Events Appearing

1. **Check database loaded:**
    
    ```bash
    sqlite3 runeandrust.db "SELECT COUNT(*) FROM Ambient_Sound_Descriptors;"
    
    ```
    
2. **Check service initialization:**
    - Ensure `_ambientEnvironmentService` is initialized in Program.cs
    - Check logs for "AmbientEnvironmentService initialized"
3. **Check biome value:**
    - Room.Biome must match: The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim
    - Or use "Generic" as fallback

### SQLite3 Not Installed

**Ubuntu/Debian:**

```bash
sudo apt-get install sqlite3

```

**macOS:**

```bash
brew install sqlite3

```

**Windows:**
Download from [https://www.sqlite.org/download.html](https://www.sqlite.org/download.html)

---

## Example Output

After initialization and integration, you should see ambient events like:

```
You enter a dimly lit corridor. Rust stains every surface.
The smell of rust and corrosion, metallic and sharp.

[A few turns later]
The distant thrum of still-functioning machinery echoes through the halls.

[Moving to new room]
You move north...
Water drips steadily from corroded pipes. Plink. Plink. Plink.

[In Muspelheim]
You step into a chamber of searing heat. Lava flows nearby.
Sulfur. The smell burns your nose and throat.
Molten rock gurgles and pops in nearby flows.

[In Alfheim]
You enter a corridor where geometry doesn't quite make sense.
The distant, ever-present shriek of the Cursed Choir. It never stops.
The smell is wrong. It's simultaneously floral and putrid.

```

---

## Additional Resources

- **V0.38.13_IMPLEMENTATION_SUMMARY.md** - Complete technical documentation
- **V0.38.13_INTEGRATION_GUIDE.md** - Step-by-step integration instructions
- **RuneAndRust.Tests/AmbientEnvironmentServiceTests.cs** - Test suite with examples
- **RuneAndRust.Engine/AmbientEnvironmentService.cs** - Service implementation

---

## Status

✅ **Ready for Production Use**

- Database schema tested and verified
- 150+ descriptors across all biomes
- Full integration with game loop
- Comprehensive test coverage
- Documentation complete

**Next Steps:**

1. Initialize database using this guide
2. Run the game to experience ambient events
3. Adjust frequency in Program.cs if desired
4. Optional: Run tests to verify functionality

---

**Last Updated:** 2025-11-18
**Version:** v0.38.13
**Status:** Complete and Integrated