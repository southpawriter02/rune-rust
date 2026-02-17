# Example Specialization Walkthrough: "Berserker"

This document provides a complete, step-by-step walkthrough of creating a new specialization from concept to deployment. Use this as a reference when building your own specializations.

---

## Concept: Berserker (Warrior Specialization)

**Design Brief:**

- **Archetype**: Warrior (ArchetypeID = 1)
- **Specialization ID**: 21 (first Warrior spec in range 21-40)
- **Theme**: Primal rage, high-risk/high-reward combat, blood frenzy
- **Mechanical Role**: Burst Damage / Glass Cannon
- **Primary Attribute**: MIGHT
- **Secondary Attribute**: STURDINESS
- **Path Type**: Coherent (fits within cultural norms)
- **Trauma Risk**: Medium (pushes boundaries but not heretical)
- **Resource System**: HP + Stamina (uses HP as resource for power)

**Core Fantasy**: A warrior who channels rage into devastating attacks, sacrificing defense and HP for overwhelming offense.

---

## Step 1: Design the 9 Abilities

### Tier 1 (Entry Abilities - 0 PP)

**1. Battle Fury (Passive)**

- When below 50% HP, gain +1 to all attack rolls
- MaxRank: 3 (Rank 2: 66% HP, Rank 3: 100% HP - always active)

**2. Reckless Strike (Active)**

- Standard Action, Single Enemy
- Wits + 1d vs 2 successes
- 3d6 damage, but take 1d6 damage yourself
- 15 stamina
- MaxRank: 3 (more damage and self-damage per rank)

**3. Primal Roar (Active)**

- Bonus Action, All Enemies (Area)
- Will + 0d vs 2 successes
- Applies "Frightened" status to enemies
- 20 stamina
- MaxRank: 2

### Tier 2 (Core Abilities - 4 PP)

**4. Blood Frenzy (Passive)**

- When you take damage, gain +1 bonus die to your next attack
- Stacks up to 3 times
- MaxRank: 2 (Rank 2: stacks to 5)

**5. Cleaving Blow (Active)**

- Standard Action, Up to 3 Enemies
- Might + 2d vs 3 successes
- 4d6 damage split among targets
- 25 stamina
- MaxRank: 3

**6. Savage Resilience (Passive)**

- Reduce all incoming damage by 1 (after armor)
- When below 25% HP, reduce by 2
- MaxRank: 1

### Tier 3 (Advanced Abilities - 5 PP)

**7. Crimson Rage (Active)**

- Standard Action, Self
- Spend 10 HP to gain +3 bonus dice to all attacks for 3 rounds
- No stamina cost
- Cooldown: Once Per Combat
- MaxRank: 2 (Rank 2: lasts 5 rounds)

**8. Death's Door (Passive)**

- The first time you would be reduced to 0 HP each combat, survive at 1 HP instead
- Gain +4 bonus dice to next attack
- MaxRank: 1

### Capstone (Ultimate - 6 PP)

**9. Unstoppable Rampage (Active)**

- Standard Action, Self
- Reduce HP to 1, then make 3 attacks this turn with +5 bonus dice each
- Each attack that hits heals you for damage dealt
- No stamina cost
- Cooldown: Once Per Scene
- MaxRank: 1

---

## Step 2: Create the Seeding Script

Create method in `RuneAndRust.Persistence/DataSeeder.cs`:

```csharp
/// <summary>
/// v0.19: Seeds Berserker specialization for Warrior
/// High-risk/high-reward melee combat focused on rage and HP sacrifice
/// </summary>
private void SeedBerserkerSpecialization()
{
    _log.Information("Seeding Berserker specialization");

    // Specialization metadata
    var spec = new SpecializationData
    {
        SpecializationID = 21,
        Name = "Berserker",
        ArchetypeID = 1, // Warrior
        PathType = "Coherent",
        MechanicalRole = "Burst Damage / Glass Cannon",
        PrimaryAttribute = "MIGHT",
        SecondaryAttribute = "STURDINESS",
        Description = @"Berserkers are warriors who embrace the primal fury within, channeling raw rage into devastating combat prowess. Where other warriors rely on discipline and technique, Berserkers abandon control for pure, overwhelming power.

        In the frozen wastes, Berserkers are both feared and respected. They walk the edge between life and death, trading their own blood for victory. Many fall in battle, consumed by the very rage they sought to harness—but those who survive become legends.

        The path of the Berserker is not for the cautious. It demands absolute commitment, the willingness to sacrifice everything for a single, perfect moment of carnage.",
        Tagline = "Fury unbound, blood for blood, victory at any cost.",
        UnlockRequirements = new UnlockRequirements
        {
            MinimumLegend = 1,
            RequiredAttribute = "MIGHT",
            RequiredAttributeValue = 3,
            RequiredQuestComplete = "",
            OtherRequirements = ""
        },
        ResourceSystem = "HP + Stamina",
        TraumaRisk = "Medium",
        IconEmoji = "⚔️",
        PPCostToUnlock = 3,
        IsActive = true
    };
    _specializationRepo.Insert(spec);

    // Tier 1 - Ability 1: Battle Fury (Passive)
    var battleFury = new AbilityData
    {
        AbilityID = 2101,
        SpecializationID = 21,
        Name = "Battle Fury",
        Description = "Pain sharpens focus. When wounded, your strikes become more precise, more lethal. The scent of your own blood awakens something primal.",
        MechanicalSummary = "When below 50% HP, gain +1 to all attack rolls",
        TierLevel = 1,
        PPCost = 0,
        MaxRank = 3,
        CostToRank2 = 5,
        CostToRank3 = 7,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 0
        },
        AbilityType = "Passive",
        ActionType = "Free Action",
        TargetType = "Self",
        ResourceCost = new AbilityResourceCost(),
        AttributeUsed = "",
        BonusDice = 0,
        SuccessThreshold = 0,
        DamageDice = 0,
        HealingDice = 0,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "damage,passive,conditional"
    };
    _abilityRepo.Insert(battleFury);

    // Tier 1 - Ability 2: Reckless Strike (Active)
    var recklessStrike = new AbilityData
    {
        AbilityID = 2102,
        SpecializationID = 21,
        Name = "Reckless Strike",
        Description = "Abandon all defense for a devastating blow. You expose yourself to harm, but the power of the strike is undeniable—armor shatters, bones break, enemies fall.",
        MechanicalSummary = "Deal 3d6 damage to an enemy, but take 1d6 damage yourself",
        TierLevel = 1,
        PPCost = 0,
        MaxRank = 3,
        CostToRank2 = 5,
        CostToRank3 = 7,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 0
        },
        AbilityType = "Active",
        ActionType = "Standard Action",
        TargetType = "Single Enemy",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = 15
        },
        AttributeUsed = "might",
        BonusDice = 1,
        SuccessThreshold = 2,
        DamageDice = 3,
        HealingDice = 0,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "damage,self-harm,attack"
    };
    _abilityRepo.Insert(recklessStrike);

    // Tier 1 - Ability 3: Primal Roar (Active)
    var primalRoar = new AbilityData
    {
        AbilityID = 2103,
        SpecializationID = 21,
        Name = "Primal Roar",
        Description = "Release a terrifying bellow that echoes with savage fury. Enemies falter, their courage shaken by the promise of violence to come.",
        MechanicalSummary = "Frighten all enemies in the area with a primal battle cry",
        TierLevel = 1,
        PPCost = 0,
        MaxRank = 2,
        CostToRank2 = 5,
        CostToRank3 = 0,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 0
        },
        AbilityType = "Active",
        ActionType = "Bonus Action",
        TargetType = "All Enemies",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = 20
        },
        AttributeUsed = "will",
        BonusDice = 0,
        SuccessThreshold = 2,
        DamageDice = 0,
        HealingDice = 0,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { "Frightened" },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "control,debuff,aoe"
    };
    _abilityRepo.Insert(primalRoar);

    // Tier 2 - Ability 4: Blood Frenzy (Passive)
    var bloodFrenzy = new AbilityData
    {
        AbilityID = 2104,
        SpecializationID = 21,
        Name = "Blood Frenzy",
        Description = "Each wound inflames your rage. Pain becomes fuel, injury becomes inspiration. The more you bleed, the more deadly you become.",
        MechanicalSummary = "When you take damage, gain +1 bonus die to next attack (stacks to 3)",
        TierLevel = 2,
        PPCost = 4,
        MaxRank = 2,
        CostToRank2 = 5,
        CostToRank3 = 0,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 8
        },
        AbilityType = "Passive",
        ActionType = "Free Action",
        TargetType = "Self",
        ResourceCost = new AbilityResourceCost(),
        AttributeUsed = "",
        BonusDice = 0,
        SuccessThreshold = 0,
        DamageDice = 0,
        HealingDice = 0,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "damage,passive,reactive"
    };
    _abilityRepo.Insert(bloodFrenzy);

    // Tier 2 - Ability 5: Cleaving Blow (Active)
    var cleavingBlow = new AbilityData
    {
        AbilityID = 2105,
        SpecializationID = 21,
        Name = "Cleaving Blow",
        Description = "A wide, sweeping strike that cuts through multiple foes. Your weapon becomes an arc of destruction, carving a bloody path through the battlefield.",
        MechanicalSummary = "Deal 4d6 damage split among up to 3 enemies",
        TierLevel = 2,
        PPCost = 4,
        MaxRank = 3,
        CostToRank2 = 5,
        CostToRank3 = 7,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 8
        },
        AbilityType = "Active",
        ActionType = "Standard Action",
        TargetType = "Up to 3 Enemies",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = 25
        },
        AttributeUsed = "might",
        BonusDice = 2,
        SuccessThreshold = 3,
        DamageDice = 4,
        HealingDice = 0,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "damage,aoe,attack"
    };
    _abilityRepo.Insert(cleavingBlow);

    // Tier 2 - Ability 6: Savage Resilience (Passive)
    var savageResilience = new AbilityData
    {
        AbilityID = 2106,
        SpecializationID = 21,
        Name = "Savage Resilience",
        Description = "Your body hardens through constant battle. Blows that would fell lesser warriors barely slow you down. Even near death, you endure.",
        MechanicalSummary = "Reduce all incoming damage by 1 (2 when below 25% HP)",
        TierLevel = 2,
        PPCost = 4,
        MaxRank = 1,
        CostToRank2 = 0,
        CostToRank3 = 0,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 8
        },
        AbilityType = "Passive",
        ActionType = "Free Action",
        TargetType = "Self",
        ResourceCost = new AbilityResourceCost(),
        AttributeUsed = "",
        BonusDice = 0,
        SuccessThreshold = 0,
        DamageDice = 0,
        HealingDice = 0,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "defense,passive,damage-reduction"
    };
    _abilityRepo.Insert(savageResilience);

    // Tier 3 - Ability 7: Crimson Rage (Active)
    var crimsonRage = new AbilityData
    {
        AbilityID = 2107,
        SpecializationID = 21,
        Name = "Crimson Rage",
        Description = "You open old wounds, letting blood flow freely to fuel an unstoppable onslaught. Pain transforms into power, and for a brief, terrible moment, you are invincible.",
        MechanicalSummary = "Spend 10 HP to gain +3 bonus dice to all attacks for 3 rounds",
        TierLevel = 3,
        PPCost = 5,
        MaxRank = 2,
        CostToRank2 = 5,
        CostToRank3 = 0,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 16
        },
        AbilityType = "Active",
        ActionType = "Bonus Action",
        TargetType = "Self",
        ResourceCost = new AbilityResourceCost
        {
            HP = 10
        },
        AttributeUsed = "",
        BonusDice = 0,
        SuccessThreshold = 0,
        DamageDice = 0,
        HealingDice = 0,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "Once Per Combat",
        IsActive = true,
        Tags = "buff,self-harm,damage"
    };
    _abilityRepo.Insert(crimsonRage);

```

```
// Tier 3 - Ability 8: Death's Door (Passive)
var deathsDoor = new AbilityData
{
    AbilityID = 2108,
    SpecializationID = 21,
    Name = "Death's Door",
    Description = "You've stared into the abyss so many times it no longer frightens you. When death comes knocking, you answer with violence.",
    MechanicalSummary = "First time per combat you reach 0 HP, survive at 1 HP and gain +4 dice to next attack",
    TierLevel = 3,
    PPCost = 5,
    MaxRank = 1,
    CostToRank2 = 0,
    CostToRank3 = 0,
    Prerequisites = new AbilityPrerequisites
    {
        RequiredPPInTree = 16
    },
    AbilityType = "Passive",
    ActionType = "Reaction",
    TargetType = "Self",
    ResourceCost = new AbilityResourceCost(),
    AttributeUsed = "",
    BonusDice = 0,
    SuccessThreshold = 0,
    DamageDice = 0,
    HealingDice = 0,
    IgnoresArmor = false,
    StatusEffectsApplied = new List<string> { },
    StatusEffectsRemoved = new List<string> { },
    CooldownType = "Once Per Combat",
    IsActive = true,
    Tags = "survival,defensive,clutch"
};
_abilityRepo.Insert(deathsDoor);

// Capstone - Ability 9: Unstoppable Rampage (Active)
var unstoppableRampage = new AbilityData
{
    AbilityID = 2109,
    SpecializationID = 21,
    Name = "Unstoppable Rampage",
    Description = "The ultimate expression of berserk fury. You give everything—every drop of blood, every ounce of strength—for one final, apocalyptic assault. Enemies fall like wheat before the scythe, and with each kill, life flows back into you. This is the edge between triumph and oblivion.",
    MechanicalSummary = "Reduce HP to 1, make 3 attacks with +5 dice each, heal for damage dealt",
    TierLevel = 4,
    PPCost = 6,
    MaxRank = 1,
    CostToRank2 = 0,
    CostToRank3 = 0,
    Prerequisites = new AbilityPrerequisites
    {
        RequiredPPInTree = 24,
        RequiredAbilityIDs = new List<int> { 2107, 2108 } // Crimson Rage + Death's Door
    },
    AbilityType = "Active",
    ActionType = "Standard Action",
    TargetType = "Self",
    ResourceCost = new AbilityResourceCost
    {
        HP = 999 // Special: reduces HP to 1 (handled in game logic)
    },
    AttributeUsed = "might",
    BonusDice = 5,
    SuccessThreshold = 2,
    DamageDice = 4,
    HealingDice = 0,
    IgnoresArmor = false,
    StatusEffectsApplied = new List<string> { },
    StatusEffectsRemoved = new List<string> { },
    CooldownType = "Once Per Scene",
    IsActive = true,
    Tags = "capstone,damage,heal,high-risk"
};
_abilityRepo.Insert(unstoppableRampage);

_log.Information("Berserker specialization seeded successfully with 9 abilities");

```

}

```

---

## Step 3: Call Seeding Method

In `RuneAndRust.Persistence/DataSeeder.cs`, add to `SeedExistingSpecializations()`:

```csharp
public void SeedExistingSpecializations()
{
    _log.Information("Seeding all specializations...");

    // Existing Adept specializations
    SeedBoneSetterSpecialization();
    SeedJotunReaderSpecialization();
    SeedSkaldSpecialization();

    // NEW: Warrior specializations
    SeedBerserkerSpecialization();

    _log.Information("All specializations seeded successfully");
}

```

---

## Step 4: Write Unit Tests

Create test file `RuneAndRust.Tests/BerserkerTests.cs`:

```csharp
using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

[TestFixture]
public class BerserkerTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private SpecializationValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _connectionString = "Data Source=:memory:";
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var saveRepo = new SaveRepository(":memory:");
        var seeder = new DataSeeder(_connectionString);
        seeder.SeedExistingSpecializations();

        _specializationService = new SpecializationService(_connectionString);
        _abilityService = new AbilityService(_connectionString);
        _validator = new SpecializationValidator(_connectionString);
    }

    [Test]
    public void Berserker_Validation_PassesAllRules()
    {
        var result = _validator.ValidateSpecialization(21);
        Assert.That(result.IsValid, Is.True, result.GetSummary());
    }

    [Test]
    public void Berserker_UnlockAsWarrior_Success()
    {
        var character = CreateTestCharacter(CharacterClass.Warrior);
        character.ProgressionPoints = 10;

        var result = _specializationService.UnlockSpecialization(character, 21);

        Assert.That(result.Success, Is.True);
        Assert.That(character.ProgressionPoints, Is.EqualTo(7));
    }

    [Test]
    public void Berserker_UnlockAsAdept_Fails()
    {
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;

        var result = _specializationService.UnlockSpecialization(character, 21);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("archetype"));
    }

    [Test]
    public void Berserker_LearnBattleFury_Success()
    {
        var character = CreateTestCharacter(CharacterClass.Warrior);
        character.ProgressionPoints = 10;
        _specializationService.UnlockSpecialization(character, 21);

        var result = _abilityService.LearnAbility(character, 2101);

        Assert.That(result.Success, Is.True);
        Assert.That(character.ProgressionPoints, Is.EqualTo(7)); // Tier 1 is free
    }

    [Test]
    public void Berserker_LearnCapstoneWithoutPrerequisites_Fails()
    {
        var character = CreateTestCharacter(CharacterClass.Warrior);
        character.ProgressionPoints = 100;
        _specializationService.UnlockSpecialization(character, 21);

        var repo = new SpecializationRepository(_connectionString);
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), 21, 24);

        var result = _abilityService.LearnAbility(character, 2109);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Prerequisites not met"));
    }

    private PlayerCharacter CreateTestCharacter(CharacterClass characterClass)
    {
        return new PlayerCharacter
        {
            Name = $"TestWarrior_{Guid.NewGuid()}",
            Class = characterClass,
            Specialization = Specialization.None,
            ProgressionPoints = 0,
            CurrentLegend = 1,
            Corruption = 0,
            HP = 100,
            MaxHP = 100,
            Stamina = 100,
            MaxStamina = 100,
            PsychicStress = 0,
            Attributes = new Attributes
            {
                Might = 4,
                Finesse = 3,
                Wits = 3,
                Will = 3,
                Sturdiness = 4
            }
        };
    }
}

```

---

## Step 5: Run Validation

Build and run the validation report:

```bash
dotnet build
dotnet test --filter "FullyQualifiedName~BerserkerTests"

```

**Expected Output:**

```
✓ Berserker_Validation_PassesAllRules
✓ Berserker_UnlockAsWarrior_Success
✓ Berserker_UnlockAsAdept_Fails
✓ Berserker_LearnBattleFury_Success
✓ Berserker_LearnCapstoneWithoutPrerequisites_Fails

5 tests passed

```

---

## Step 6: Test in UI

1. Launch the game
2. Create a Warrior character
3. Navigate to Specialization Browser
4. Find "Berserker" in the list
5. View details and unlock
6. Browse ability tree
7. Learn abilities in order: Tier 1 → Tier 2 → Tier 3 → Capstone

**Verify all UI elements display correctly.**

---

## Step 7: Commit and Deploy

```bash
git add .
git commit -m "feat(v0.19): Add Berserker specialization for Warriors

- 9 abilities following 3/3/2/1 tier structure
- High-risk/high-reward combat style using HP as resource
- Passes all 68 validation tests
- Total PP cost: 28 (standard)
- Trauma risk: Medium"

git push origin claude/specialization-system-foundation-011CV55Kw1piEzT7GQ8wu6Es

```

---

## Summary

**Time Breakdown:**

- Concept/Design: 2 hours
- Seeding Script: 3 hours
- Unit Tests: 1 hour
- Validation/Debugging: 1 hour
- UI Testing: 30 minutes
- **Total: ~7.5 hours**

**Checklist Completed:**
✅ 9 abilities (3/3/2/1 pattern)
✅ Unique ID (21) in Warrior range
✅ All ability IDs use formula (2101-2109)
✅ Total PP cost is 28
✅ Capstone requires both Tier 3 abilities
✅ Validation passes with 0 errors
✅ All 5 unit tests pass
✅ UI displays correctly
✅ Code committed and pushed

**Result**: Berserker specialization is ready for player use!

---

## Key Takeaways

1. **Design first**: Spend time on the concept before writing code
2. **Use templates**: Copy/paste from SEEDING_SCRIPT_TEMPLATE.cs to save time
3. **Validate early**: Run SpecializationValidator after seeding
4. **Test incrementally**: Don't wait until all 9 abilities are done
5. **UI matters**: Even if mechanics work, poor UI ruins the experience
6. **Balance is iterative**: First pass doesn't need to be perfect

**Remember**: This process gets faster with practice. Your first spec might take 10-15 hours, but by your third, you'll be down to 5-8 hours.

Happy specialization creation! 🎮⚔️