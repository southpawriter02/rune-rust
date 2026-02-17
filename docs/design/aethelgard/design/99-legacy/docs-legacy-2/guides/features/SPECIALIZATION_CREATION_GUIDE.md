# Specialization Creation Guide (v0.19)

## Overview

This guide walks you through creating a new specialization for Rune & Rust using the data-driven specialization system introduced in v0.19.

**Time to create a new specialization:** 15-25 hours (down from 40+ hours pre-v0.19)

**What you'll create:**
- 1 specialization metadata entry
- 9 abilities (3 Tier 1, 3 Tier 2, 2 Tier 3, 1 Capstone)
- Database seeding script
- Validation tests

---

## Step 1: Design Your Specialization

### Specialization Metadata Checklist

Before writing any code, design your specialization on paper:

- [ ] **Name**: Short, memorable name (e.g., "Bone-Setter", "Rune-Breaker")
- [ ] **Tagline**: One-sentence hook (e.g., "Keep party alive with healing items")
- [ ] **Archetype**: Which class? (1=Warrior, 2=Adept, 3=Scavenger, 4=Mystic)
- [ ] **Mechanical Role**: Tank, DPS, Support, Controller, Utility
- [ ] **Path Type**: Coherent (safe) or Heretical (corrupting)
- [ ] **Primary Attribute**: MIGHT, FINESSE, WITS, WILL, STURDINESS
- [ ] **Secondary Attribute**: (optional)
- [ ] **Resource System**: Stamina (default), or custom (Fury, Momentum, etc.)
- [ ] **Trauma Risk**: None, Low, Medium, High, Extreme
- [ ] **Icon Emoji**: Visual identifier (e.g., 🩺, ⚔️, 🛡️)
- [ ] **Unlock Requirements**: Legend tier? Quest completion?

### Example: Designing "Rune-Breaker" (Anti-Mystic Warrior)

```
Name: Rune-Breaker
Tagline: Disrupt and nullify mystic powers
Archetype: Warrior (1)
Mechanical Role: System Purifier
Path Type: Coherent
Primary Attribute: MIGHT
Secondary Attribute: WITS
Resource System: Stamina
Trauma Risk: Medium
Icon Emoji: 🔨
Unlock Requirements: Legend 3+
```

---

## Step 2: Design Your 9 Abilities

### Ability Structure (MANDATORY)

Every specialization MUST have exactly 9 abilities in this structure:

```
Tier 1 (Entry): 3 abilities @ 0 PP each
Tier 2 (Advanced): 3 abilities @ 4 PP each (requires 8 PP in tree)
Tier 3 (Expert): 2 abilities @ 5 PP each (requires 16 PP in tree)
Capstone: 1 ability @ 6 PP (requires 24 PP in tree + Tier 3 prerequisites)

Total PP: 0 + 12 + 10 + 6 = 28 PP
```

### Ability Design Checklist (per ability)

For each of your 9 abilities:

- [ ] **Name**: Clear, evocative name
- [ ] **Description**: 1-2 sentences explaining what it does
- [ ] **Mechanical Summary**: Short summary (e.g., "2d6 damage, ignores armor")
- [ ] **Ability Type**: Active, Passive, or Reaction
- [ ] **Action Type**: Standard Action, Bonus Action, Free Action, Performance
- [ ] **Target Type**: Single Enemy, All Enemies, Self, Single Ally, All Allies, etc.
- [ ] **Resource Cost**: Stamina, Stress, HP, Corruption
- [ ] **Attribute Used**: For roll checks (MIGHT, WITS, etc.)
- [ ] **Bonus Dice**: Extra dice for roll
- [ ] **Success Threshold**: Successes needed
- [ ] **Effects**: Damage dice, healing dice, status effects
- [ ] **Max Rank**: 1 (passive/ultimate) or 3 (standard)
- [ ] **Cooldown**: None, Per Combat, Per Expedition, Per Day

### Design Tips

**Tier 1 (Free with specialization):**
- Should be immediately useful
- Define the specialization's identity
- Usually includes 1 passive that enhances the core mechanic
- Example: Field Medic I (+2 to crafting, 3 free healing items)

**Tier 2 (Mid-game power):**
- More powerful active abilities
- Introduce unique mechanics
- Should feel like a significant upgrade
- Example: Anatomical Insight (apply [Vulnerable] to enemy)

**Tier 3 (Mastery):**
- High-cost, high-impact abilities
- Should feel "expert-level"
- Often includes a powerful passive
- Example: Cognitive Realignment (remove mental debuffs + restore Stress)

**Capstone (Ultimate power):**
- Most powerful ability in the tree
- Should feel game-changing
- Often limited uses (Per Combat, Per Expedition)
- Should require both Tier 3 abilities as prerequisites
- Example: Miracle Worker (massive heal + cleanse all debuffs)

---

## Step 3: Write the Database Seeding Script

### Template: Specialization Entry

```csharp
// In DataSeeder.cs, add a new method:

private void SeedRuneBreakerSpecialization()
{
    _log.Information("Seeding RuneBreaker specialization");

    var runeBreaker = new SpecializationData
    {
        SpecializationID = 4, // Next available ID
        Name = "Rune-Breaker",
        ArchetypeID = 1, // Warrior
        PathType = "Coherent",
        MechanicalRole = "System Purifier",
        PrimaryAttribute = "MIGHT",
        SecondaryAttribute = "WITS",
        Description = "Anti-mystic specialist who disrupts magical systems and nullifies supernatural threats through understanding of runic corruption patterns.",
        Tagline = "Disrupt and nullify mystic powers",
        UnlockRequirements = new UnlockRequirements { MinLegend = 3, MaxCorruption = 100 },
        ResourceSystem = "Stamina",
        TraumaRisk = "Medium",
        IconEmoji = "🔨",
        PPCostToUnlock = 3,
        IsActive = true
    };

    _specializationRepo.Insert(runeBreaker);

    SeedRuneBreakerTier1();
    SeedRuneBreakerTier2();
    SeedRuneBreakerTier3();
    SeedRuneBreakerCapstone();

    _log.Information("RuneBreaker seeding complete: 9 abilities");
}
```

### Template: Tier 1 Abilities (3 required)

```csharp
private void SeedRuneBreakerTier1()
{
    // Ability 1: Passive that defines the specialization
    _abilityRepo.Insert(new AbilityData
    {
        AbilityID = 401,
        SpecializationID = 4,
        Name = "System Breaker I",
        Description = "[PASSIVE] +2 to all checks against Jötun-Forged systems. Magical effects targeting you have -1 Accuracy.",
        TierLevel = 1,
        PPCost = 0, // Tier 1 is free
        Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
        AbilityType = "Passive",
        ActionType = "Free Action",
        TargetType = "Self",
        ResourceCost = new AbilityResourceCost(),
        MechanicalSummary = "+2 vs systems, enemies -1 Accuracy on magic",
        MaxRank = 1, // Passives don't rank up
        IsActive = true
    });

    // Ability 2: Active ability
    _abilityRepo.Insert(new AbilityData
    {
        AbilityID = 402,
        SpecializationID = 4,
        Name = "Rune Strike",
        Description = "Strike with anti-mystic force. Deals 2d6+MIGHT damage, ignores magical armor/shields.",
        TierLevel = 1,
        PPCost = 0,
        Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
        AbilityType = "Active",
        ActionType = "Standard Action",
        TargetType = "Single Enemy",
        ResourceCost = new AbilityResourceCost { Stamina = 15 },
        AttributeUsed = "might",
        BonusDice = 1,
        SuccessThreshold = 2,
        MechanicalSummary = "2d6+MIGHT damage, ignores magical defenses",
        DamageDice = 2,
        IgnoresArmor = true, // For magical armor
        MaxRank = 3,
        CostToRank2 = 5,
        IsActive = true
    });

    // Ability 3: Utility ability
    _abilityRepo.Insert(new AbilityData
    {
        AbilityID = 403,
        SpecializationID = 4,
        Name = "Dispel Magic",
        Description = "Attempt to nullify one magical effect on target ally. WITS check to remove [Cursed], [Hexed], or magical debuffs.",
        TierLevel = 1,
        PPCost = 0,
        Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
        AbilityType = "Active",
        ActionType = "Standard Action",
        TargetType = "Single Ally",
        ResourceCost = new AbilityResourceCost { Stamina = 20 },
        AttributeUsed = "wits",
        BonusDice = 2,
        SuccessThreshold = 2,
        MechanicalSummary = "Remove magical debuffs from ally",
        StatusEffectsRemoved = new List<string> { "Cursed", "Hexed" },
        MaxRank = 3,
        CostToRank2 = 5,
        IsActive = true
    });
}
```

### Template: Tier 2 Abilities (3 required)

```csharp
private void SeedRuneBreakerTier2()
{
    // Each ability costs 4 PP and requires 8 PP in tree
    _abilityRepo.Insert(new AbilityData
    {
        AbilityID = 404,
        SpecializationID = 4,
        Name = "Ability Name",
        Description = "Description here",
        TierLevel = 2,
        PPCost = 4,
        Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
        // ... rest of properties
    });

    // Repeat for 2 more Tier 2 abilities (405, 406)
}
```

### Template: Tier 3 Abilities (2 required)

```csharp
private void SeedRuneBreakerTier3()
{
    // Each ability costs 5 PP and requires 16 PP in tree
    _abilityRepo.Insert(new AbilityData
    {
        AbilityID = 407,
        SpecializationID = 4,
        Name = "Ability Name",
        Description = "Description here",
        TierLevel = 3,
        PPCost = 5,
        Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
        // ... rest of properties
    });

    // One more Tier 3 ability (408)
}
```

### Template: Capstone (1 required)

```csharp
private void SeedRuneBreakerCapstone()
{
    _abilityRepo.Insert(new AbilityData
    {
        AbilityID = 409,
        SpecializationID = 4,
        Name = "System Collapse",
        Description = "⭐ CAPSTONE: Overload mystic systems. Deals 4d6 damage to all enemies, removes all magical effects from battlefield. Costs 10 Stress.",
        TierLevel = 4,
        PPCost = 6,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 24,
            RequiredAbilityIDs = new List<int> { 407, 408 } // Both Tier 3 abilities
        },
        AbilityType = "Active",
        ActionType = "Standard Action",
        TargetType = "All Enemies",
        ResourceCost = new AbilityResourceCost { Stamina = 50, Stress = 10 },
        AttributeUsed = "might",
        BonusDice = 3,
        SuccessThreshold = 3,
        MechanicalSummary = "4d6 AOE damage + dispel all magical effects, 10 Stress",
        DamageDice = 4,
        MaxRank = 3,
        CostToRank2 = 5,
        CooldownType = "Per Combat",
        IsActive = true,
        Notes = "Ultimate anti-magic ability"
    });
}
```

---

## Step 4: Register Your Specialization in DataSeeder

Add your seeding method to `SeedExistingSpecializations()`:

```csharp
public void SeedExistingSpecializations()
{
    _log.Information("Starting specialization data seeding");

    SeedBoneSetterSpecialization();
    SeedJotunReaderSpecialization();
    SeedSkaldSpecialization();
    SeedRuneBreakerSpecialization(); // ADD THIS LINE

    _log.Information("Specialization data seeding completed successfully");
}
```

---

## Step 5: Run Validation

After seeding, validate your specialization:

```csharp
var validator = new SpecializationValidator(connectionString);
var result = validator.ValidateSpecialization(4); // Your spec ID

if (result.IsValid)
{
    Console.WriteLine("✓ Specialization is valid!");
}
else
{
    Console.WriteLine(result.GetSummary());
}
```

### Common Validation Errors

**"Invalid ability count: X"**
- You don't have exactly 9 abilities
- Check Tier 1/2/3/Capstone counts

**"Tier 1 has X abilities (expected 3)"**
- Wrong tier distribution
- Verify 3/3/2/1 pattern

**"Ability costs X PP (convention: Y PP)"**
- PP costs don't follow convention
- Use 0/4/5/6 for Tier 1/2/3/Capstone

**"Prerequisites not met"**
- Capstone doesn't require Tier 3 abilities
- Add RequiredAbilityIDs to capstone

**"Total PP cost is X"**
- Total should be 20-35 PP (standard: 28)
- Adjust individual ability costs

---

## Step 6: Testing Checklist

### Manual Testing Steps

- [ ] Specialization appears in browser for correct archetype
- [ ] Unlock requirements work (Legend, Corruption, Quest)
- [ ] PP cost deduction works correctly
- [ ] All 9 abilities appear in tree
- [ ] Tier 1 abilities are immediately available
- [ ] Tier 2 locked until 8 PP in tree
- [ ] Tier 3 locked until 16 PP in tree
- [ ] Capstone locked until 24 PP + Tier 3 abilities
- [ ] Learning abilities deducts correct PP
- [ ] Rank progression works (1→2→3)
- [ ] PP spent in tree tracks correctly
- [ ] Resource costs deduct properly (Stamina/Stress/HP)

### Automated Testing

Add integration test for your specialization:

```csharp
[Test]
public void Validation_RuneBreaker_PassesAllRules()
{
    // Act
    var result = _validator.ValidateSpecialization(4); // RuneBreaker

    // Assert
    Assert.That(result.IsValid, Is.True,
        $"RuneBreaker validation failed:\n{result.GetSummary()}");
}
```

---

## Step 7: Documentation

Document your specialization in code comments:

```csharp
// RUNE-BREAKER (Anti-Mystic Specialist)
//
// Role: System Purifier / Anti-Magic DPS
// Path: Coherent (no Corruption cost)
// Resources: Stamina
// Trauma Risk: Medium
//
// Core Fantasy: Warrior who understands mystic systems enough to break them
//
// Tier 1 (Entry):
//   - System Breaker I: +2 vs systems, enemies -1 Accuracy on magic
//   - Rune Strike: 2d6+MIGHT, ignores magical armor
//   - Dispel Magic: Remove magical debuffs from ally
//
// Tier 2 (Advanced):
//   - Nullify Aether: Prevent enemy from using magical abilities for 2 turns
//   - Runic Ward: +4 Defense vs magical attacks for 3 turns
//   - Mystic Feedback: Deal damage back to spellcasters who target you
//
// Tier 3 (Expert):
//   - System Overload: Massive damage to magical constructs
//   - Adaptive Defense: Passive +2 to all defenses
//
// Capstone:
//   - System Collapse: AOE damage + dispel all magical effects
//
// Synergies:
//   - Strong vs: Jötun-Forged, Undying, magical enemies
//   - Weak vs: Physical bruisers, swarms
//   - Party role: Anti-magic protection, magical threat elimination
```

---

## Quick Reference: Ability IDs

Reserve ID ranges for each specialization:

```
1-99:    Reserved (system)
101-109: BoneSetter
201-209: JotunReader
301-309: Skald
401-409: RuneBreaker
501-509: [Next Warrior Spec]
601-609: [Next Adept Spec]
... etc
```

---

## Quick Reference: Archetype IDs

```
1 = Warrior
2 = Adept
3 = Scavenger
4 = Mystic
```

---

## Quick Reference: Validation Rules

Your specialization must pass these 7 rules:

1. **Metadata**: All required fields populated, valid values
2. **Ability Count**: Exactly 9 abilities
3. **Tier Structure**: 3/3/2/1 pattern
4. **PP Costs**: 0/4/5/6 per tier (convention)
5. **Total PP**: 20-35 PP range (28 standard)
6. **Prerequisites**: Valid ability IDs, lower-tier prerequisites
7. **Ability Metadata**: Valid types, action types, ranks

---

## Common Pitfalls

❌ **Forgetting to add capstone prerequisites**
- Capstone should require both Tier 3 abilities

❌ **Wrong PP costs**
- Use convention: 0/4/5/6 for Tier 1/2/3/Capstone

❌ **Passive abilities with stamina costs**
- Passives should have 0 stamina cost

❌ **Abilities with MaxRank > 3**
- Max rank is 3 (most abilities) or 1 (passives/ultimates)

❌ **Missing mechanical summary**
- Every ability needs a short summary for UI display

❌ **Invalid ability type**
- Use "Active", "Passive", or "Reaction" only

---

## Next Steps

After creating your specialization:

1. Run full validation: `validator.ValidateAllSpecializations()`
2. Generate validation report: `validator.GenerateValidationReport()`
3. Test in-game: Create character, unlock spec, learn abilities
4. Balance tuning: Adjust costs/effects based on playtesting
5. Documentation: Add to CHANGELOG.md with version number

---

## Need Help?

- Review existing specializations in `DataSeeder.cs`
- Check `SpecializationValidator.cs` for validation rules
- Run integration tests in `SpecializationIntegrationTests.cs`
- Consult this guide's examples

Happy specialization crafting! 🎮
