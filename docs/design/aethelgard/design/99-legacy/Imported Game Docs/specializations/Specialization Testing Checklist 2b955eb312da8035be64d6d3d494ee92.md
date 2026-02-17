# Specialization Testing Checklist

Use this checklist to verify your new specialization is complete and working correctly before deployment.

---

## Phase 1: Data Validation

### Automated Validation

Run the SpecializationValidator to catch common errors:

```csharp
// In your test setup or console
var validator = new SpecializationValidator(connectionString);
var result = validator.ValidateSpecialization(yourSpecId);

if (!result.IsValid)
{
    Console.WriteLine(result.GetSummary());
}

```

**Expected Result**: ✓ Validation passed with 0 errors

### Manual Validation Checklist

- [ ]  **Specialization ID is unique** (check `_specializationRepo.GetById(id)` returns null before seeding)
- [ ]  **Archetype ID is correct** (1=Warrior, 2=Adept, 3=Skald, 4=Jotun-Marked)
- [ ]  **PathType is valid** ("Coherent" or "Heretical")
- [ ]  **TraumaRisk is valid** ("None", "Low", "Medium", "High", or "Extreme")
- [ ]  **Primary/Secondary attributes are valid** ("MIGHT", "FINESSE", "WITS", "WILL", "STURDINESS")
- [ ]  **Description is filled out** (2-3 paragraphs of engaging lore)
- [ ]  **Tagline is punchy** (one sentence that captures the essence)
- [ ]  **IconEmoji renders correctly** (test in terminal with `AnsiConsole.MarkupLine($"{emoji}")`)
- [ ]  **PPCostToUnlock is reasonable** (usually 3 PP)

---

## Phase 2: Ability Structure

### Ability Count and Tier Distribution

- [ ]  **Exactly 9 abilities total**
- [ ]  **3 Tier 1 abilities** (TierLevel = 1)
- [ ]  **3 Tier 2 abilities** (TierLevel = 2)
- [ ]  **2 Tier 3 abilities** (TierLevel = 3)
- [ ]  **1 Capstone ability** (TierLevel = 4)

### Ability IDs

- [ ]  **IDs follow formula**: `(SpecializationID * 100) + AbilityNumber`
- [ ]  **IDs are sequential** (e.g., 401, 402, 403, ..., 409)
- [ ]  **No ID collisions** with existing abilities

### PP Costs (Convention)

- [ ]  **All Tier 1 abilities cost 0 PP**
- [ ]  **All Tier 2 abilities cost 4 PP**
- [ ]  **All Tier 3 abilities cost 5 PP**
- [ ]  **Capstone costs 6 PP**
- [ ]  **Total PP cost is 28** (0+0+0 + 4+4+4 + 5+5 + 6)

### Prerequisites

- [ ]  **Tier 1 abilities**: RequiredPPInTree = 0, RequiredAbilityIDs = []
- [ ]  **Tier 2 abilities**: RequiredPPInTree = 8
- [ ]  **Tier 3 abilities**: RequiredPPInTree = 16
- [ ]  **Capstone**: RequiredPPInTree = 24, RequiredAbilityIDs = [both Tier 3 ability IDs]

---

## Phase 3: Ability Metadata

For each of the 9 abilities, verify:

### Required Fields

- [ ]  **Name is unique** within the specialization
- [ ]  **Description is flavorful** (2-3 sentences of lore)
- [ ]  **MechanicalSummary is accurate** (1 sentence describing what it does)
- [ ]  **AbilityType is valid** ("Active", "Passive", or "Reaction")
- [ ]  **ActionType is valid** ("Standard Action", "Bonus Action", "Free Action", "Performance", "Reaction")
- [ ]  **TargetType makes sense** ("Self", "Single Enemy", "All Allies", "Area", etc.)

### Type-Specific Validation

### For Active Abilities:

- [ ]  **AttributeUsed is set** (e.g., "wits", "might", "will")
- [ ]  **BonusDice is reasonable** (0-3)
- [ ]  **SuccessThreshold is reasonable** (1-4)
- [ ]  **Stamina cost > 0** (10-50 depending on tier)
- [ ]  **ActionType is NOT "Free Action"**

### For Passive Abilities:

- [ ]  **AttributeUsed is empty string** (`""`)
- [ ]  **Stamina cost is 0**
- [ ]  **BonusDice is 0**
- [ ]  **SuccessThreshold is 0**
- [ ]  **DamageDice is 0**
- [ ]  **ActionType is "Free Action"**

### For Reaction Abilities:

- [ ]  **ActionType is "Reaction"**
- [ ]  **Trigger condition is clear** in description
- [ ]  **AttributeUsed is set** (if requires a roll)

### Resource Costs

- [ ]  **Stamina costs are tier-appropriate** (10-15/15-25/25-35/40+ by tier)
- [ ]  **Stress costs are rare** (only for high-risk/trauma abilities)
- [ ]  **HP costs are rare** (only for blood magic/sacrifice abilities)
- [ ]  **Passive abilities have 0 stamina**

### Damage/Healing

- [ ]  **DamageDice is reasonable** (0-6 dice depending on tier and stamina cost)
- [ ]  **HealingDice is reasonable** (0-6 dice)
- [ ]  **IgnoresArmor is appropriate** (true for psychic/trauma damage, false otherwise)
- [ ]  **Healing abilities don't also deal damage** (unless explicitly designed that way)

### Status Effects

- [ ]  **StatusEffectsApplied uses existing effects** (check `StatusEffect` enum)
- [ ]  **StatusEffectsRemoved uses existing effects**
- [ ]  **Effects match ability theme** (e.g., fire abilities apply "Burning")

### Cooldowns

- [ ]  **CooldownType is valid** ("None", "Once Per Combat", "Once Per Scene", "Once Per Session")
- [ ]  **Powerful abilities have cooldowns** (Tier 3 and Capstone often do)

### Rank Progression

- [ ]  **MaxRank is 1-3**
- [ ]  **CostToRank2 is set** (usually 5 PP, or 0 if MaxRank = 1)
- [ ]  **CostToRank3 is set** (usually 7 PP, or 0 if MaxRank < 3)
- [ ]  **Build-around abilities have higher MaxRank** (2-3 for key abilities)

---

## Phase 4: Unit Tests

### Test: Specialization Unlock

```csharp
[Test]
public void Unlock[YourSpecName]_WithValidCharacter_Success()
{
    // Arrange
    var character = CreateTestCharacter(CharacterClass.[CorrectArchetype]);
    character.ProgressionPoints = 10;

    // Act
    var result = _specializationService.UnlockSpecialization(character, [yourSpecId]);

    // Assert
    Assert.That(result.Success, Is.True);
    Assert.That(character.ProgressionPoints, Is.EqualTo(7)); // 10 - 3 = 7
}

```

- [ ]  **Test passes**
- [ ]  **PP is deducted correctly**
- [ ]  **Unlock is tracked** in database

### Test: Wrong Archetype Rejection

```csharp
[Test]
public void Unlock[YourSpecName]_WithWrongArchetype_Fails()
{
    // Arrange
    var character = CreateTestCharacter(CharacterClass.[WrongArchetype]);
    character.ProgressionPoints = 10;

    // Act
    var result = _specializationService.UnlockSpecialization(character, [yourSpecId]);

    // Assert
    Assert.That(result.Success, Is.False);
    Assert.That(result.Message, Does.Contain("archetype"));
}

```

- [ ]  **Test passes**
- [ ]  **Error message is clear**

### Test: Learn Tier 1 Ability

```csharp
[Test]
public void LearnTier1Ability_AfterUnlock_Success()
{
    // Arrange
    var character = CreateTestCharacter(CharacterClass.[CorrectArchetype]);
    character.ProgressionPoints = 10;
    _specializationService.UnlockSpecialization(character, [yourSpecId]);

    // Act
    var result = _abilityService.LearnAbility(character, [tier1AbilityId]);

    // Assert
    Assert.That(result.Success, Is.True);
    Assert.That(character.ProgressionPoints, Is.EqualTo(7)); // Tier 1 is free (0 PP)
}

```

- [ ]  **Test passes**
- [ ]  **No PP cost for Tier 1**

### Test: Learn Tier 2 Without Prerequisites

```csharp
[Test]
public void LearnTier2Ability_WithoutPrerequisites_Fails()
{
    // Arrange
    var character = CreateTestCharacter(CharacterClass.[CorrectArchetype]);
    character.ProgressionPoints = 50;
    _specializationService.UnlockSpecialization(character, [yourSpecId]);
    // Don't spend 8 PP in tree

    // Act
    var result = _abilityService.LearnAbility(character, [tier2AbilityId]);

    // Assert
    Assert.That(result.Success, Is.False);
    Assert.That(result.Message, Does.Contain("Prerequisites not met"));
}

```

- [ ]  **Test passes**
- [ ]  **Prerequisites are enforced**

### Test: Learn Capstone Without Tier 3 Abilities

```csharp
[Test]
public void LearnCapstone_WithoutTier3Abilities_Fails()
{
    // Arrange
    var character = CreateTestCharacter(CharacterClass.[CorrectArchetype]);
    character.ProgressionPoints = 100;
    _specializationService.UnlockSpecialization(character, [yourSpecId]);

    var repo = new SpecializationRepository(_connectionString);
    repo.UpdatePPSpentInTree(character.Name.GetHashCode(), [yourSpecId], 24);

    // Act (try to learn capstone without Tier 3 prerequisites)
    var result = _abilityService.LearnAbility(character, [capstoneAbilityId]);

    // Assert
    Assert.That(result.Success, Is.False);
    Assert.That(result.Message, Does.Contain("Prerequisites not met"));
}

```

- [ ]  **Test passes**
- [ ]  **Capstone requires Tier 3 abilities**

### Test: Full Progression Flow

```csharp
[Test]
public void FullProgression_LearnAllAbilities_Success()
{
    // Test complete unlock → learn all Tier 1 → learn all Tier 2 → Tier 3 → Capstone
    // (See SpecializationIntegrationTests.cs for example)
}

```

- [ ]  **Test passes**
- [ ]  **All abilities can be learned in order**
- [ ]  **Total PP spent is correct**

---

## Phase 5: Integration Tests

### Test: Validation Passes

```csharp
[Test]
public void Validation_[YourSpecName]_PassesAllRules()
{
    // Act
    var result = _validator.ValidateSpecialization([yourSpecId]);

    // Assert
    Assert.That(result.IsValid, Is.True,
        $"Validation failed:\\n{result.GetSummary()}");
}

```

- [ ]  **Test passes**
- [ ]  **0 errors, minimal warnings**

### Test: All Specializations Still Valid

```csharp
[Test]
public void Validation_AllSpecializations_PassValidation()
{
    // Act
    var result = _validator.ValidateAllSpecializations();

    // Assert
    Assert.That(result.IsValid, Is.True,
        $"Validation failed:\\n{result.GetSummary()}");
}

```

- [ ]  **Test passes**
- [ ]  **Your new spec doesn't break existing ones**

---

## Phase 6: UI Testing

### Manual UI Test: Specialization Browser

1. Launch game
2. Navigate to Specialization Browser
3. Find your new specialization in the list

**Verify:**

- [ ]  **Specialization appears in correct archetype section**
- [ ]  **Name displays correctly** (special characters render)
- [ ]  **Icon emoji renders correctly**
- [ ]  **Status shows "○ Unlockable"** (if requirements met)
- [ ]  **Mechanical role is displayed**
- [ ]  **Requirements are shown**

### Manual UI Test: Unlock Flow

1. Select your specialization
2. View details page
3. Unlock the specialization

**Verify:**

- [ ]  **Description displays correctly** (formatting, line breaks)
- [ ]  **Tagline is shown**
- [ ]  **Requirements are accurate**
- [ ]  **Unlock confirmation prompt appears**
- [ ]  **PP is deducted correctly**
- [ ]  **Status changes to "✓ Unlocked"**

### Manual UI Test: Ability Tree

1. After unlocking, view ability tree
2. Browse all 9 abilities

**Verify:**

- [ ]  **All 9 abilities appear**
- [ ]  **Grouped correctly by tier** (Tier 1, Tier 2, Tier 3, Capstone)
- [ ]  **Type icons correct** (⚔ Active, ◈ Passive, ⚡ Reaction)
- [ ]  **Status indicators work** (✓ Learned, ○ Available, ✗ Locked)
- [ ]  **Tier 1 abilities show "Available"** immediately
- [ ]  **Tier 2+ show "Locked"** until prerequisites met

### Manual UI Test: Learn Abilities

1. Learn a Tier 1 ability
2. Learn enough to unlock Tier 2
3. Learn a Tier 2 ability

**Verify:**

- [ ]  **Ability details display correctly**
- [ ]  **Resource costs shown**
- [ ]  **Mechanical summary accurate**
- [ ]  **Learn confirmation works**
- [ ]  **PP deducted correctly**
- [ ]  **Status changes to "✓ Rank 1"**
- [ ]  **Prerequisites unlock correctly**

### Manual UI Test: Rank Up

1. Learn an ability with MaxRank > 1
2. Attempt to rank it up

**Verify:**

- [ ]  **Rank up option appears**
- [ ]  **Cost is displayed correctly**
- [ ]  **Rank up succeeds**
- [ ]  **Status updates to new rank**
- [ ]  **Abilities at max rank show "Maximum rank reached"**

---

## Phase 7: Balance Review

### Power Level Check

- [ ]  **Tier 1 abilities are modest** (establish identity, not overpowered)
- [ ]  **Tier 2 abilities are solid workhorses** (competitive with other specs)
- [ ]  **Tier 3 abilities are powerful** (high impact, specialization-defining)
- [ ]  **Capstone is impressive** (encounter-defining, peak of specialization)

### Comparative Balance

Compare your specialization to existing ones (BoneSetter, JotunReader, Skald):

- [ ]  **Similar total PP cost** (28 PP is standard)
- [ ]  **Similar power per tier** (not strictly better/worse)
- [ ]  **Unique mechanical identity** (doesn't invalidate existing specs)
- [ ]  **Fills a niche** (offers something distinct)

### Resource Economy

- [ ]  **Stamina costs match power** (10-15 for basic, 40+ for powerful)
- [ ]  **Cooldowns on powerful effects** (prevents spam of strong abilities)
- [ ]  **Not all abilities are stamina-intensive** (allows sustained use)
- [ ]  **Passive abilities provide value** (worth the PP investment)

---

## Phase 8: Documentation Review

### Code Documentation

- [ ]  **Method has XML summary** (`/// <summary>`)
- [ ]  **Ability variables have comments** explaining what they do
- [ ]  **Complex mechanics are explained** in comments

### Player-Facing Content

- [ ]  **Descriptions are engaging** (lore-rich, thematic)
- [ ]  **Mechanical summaries are clear** (players understand what it does)
- [ ]  **No typos or grammatical errors**
- [ ]  **Consistent terminology** with rest of game

---

## Phase 9: Pre-Deployment Checklist

### Final Checks

- [ ]  **All tests pass** (68+ tests including your new ones)
- [ ]  **Validation passes** with 0 errors
- [ ]  **UI tested manually** end-to-end
- [ ]  **No console errors** or exceptions
- [ ]  **Code is committed** with clear commit message
- [ ]  **Changes are pushed** to remote repository

### Integration

- [ ]  **Seeding method called** in `DataSeeder.SeedExistingSpecializations()`
- [ ]  **Database schema up to date** (tables created)
- [ ]  **No migration errors** on fresh database

### Performance

- [ ]  **Seeding completes quickly** (< 1 second)
- [ ]  **Validation runs fast** (< 100ms per spec)
- [ ]  **No database deadlocks** or slowdowns

---

## Common Issues and Fixes

### Issue: Validation fails with "Invalid ability count"

**Fix**: Ensure exactly 9 abilities are seeded (3/3/2/1 pattern)

### Issue: "Prerequisites not met" when learning Tier 1

**Fix**: Check that specialization is unlocked first (`HasUnlocked()` returns true)

### Issue: Capstone can be learned without Tier 3 abilities

**Fix**: Verify `RequiredAbilityIDs` contains both Tier 3 ability IDs

### Issue: Passive ability has stamina cost warning

**Fix**: Set `Stamina = 0` in ResourceCost for passive abilities

### Issue: UI doesn't show new specialization

**Fix**: Ensure `IsActive = true` and seeding method is called

### Issue: Ability IDs collide with existing abilities

**Fix**: Use formula `(SpecID * 100) + AbilityNumber` strictly

### Issue: "Invalid path type" error

**Fix**: Use exactly "Coherent" or "Heretical" (case-sensitive)

### Issue: "Invalid trauma risk" error

**Fix**: Use exactly "None", "Low", "Medium", "High", or "Extreme" (case-sensitive)

---

## Success Criteria

Your specialization is ready for deployment when:

✅ All automated tests pass
✅ Validation reports 0 errors
✅ UI displays correctly in all screens
✅ Full progression flow works end-to-end
✅ Balance is comparable to existing specializations
✅ Documentation is complete and accurate
✅ No console errors or exceptions

**Estimated time for full testing**: 2-3 hours per specialization

---

## Quick Test Script

Run this in your test suite to verify everything at once:

```csharp
[Test]
public void QuickVerification_[YourSpecName]_AllChecks()
{
    // 1. Validation
    var validationResult = _validator.ValidateSpecialization([yourSpecId]);
    Assert.That(validationResult.IsValid, Is.True, validationResult.GetSummary());

    // 2. Unlock
    var character = CreateTestCharacter(CharacterClass.[CorrectArchetype]);
    character.ProgressionPoints = 100;
    var unlockResult = _specializationService.UnlockSpecialization(character, [yourSpecId]);
    Assert.That(unlockResult.Success, Is.True);

    // 3. Learn Tier 1
    var tier1Result = _abilityService.LearnAbility(character, [yourSpecId] * 100 + 1);
    Assert.That(tier1Result.Success, Is.True);

    // 4. PP tracking
    var ppSpent = _specializationService.GetPPSpentInTree(character, [yourSpecId]);
    Assert.That(ppSpent, Is.GreaterThan(0));

    // 5. Ability count
    var abilities = _abilityService.GetAbilitiesForSpecialization([yourSpecId]).Abilities;
    Assert.That(abilities.Count, Is.EqualTo(9));

    Console.WriteLine("✓ All quick checks passed!");
}

```

---

**Remember**: Thorough testing now saves hours of debugging later. Take the time to verify everything works before moving to the next specialization!