// SPECIALIZATION SEEDING SCRIPT TEMPLATE
// Add this method to DataSeeder.cs and call it from SeedExistingSpecializations()

/// <summary>
/// v0.19: Seeds [SPEC_NAME] specialization for [ARCHETYPE_NAME]
/// [1-2 sentence description of what this specialization does]
/// </summary>
private void Seed[SPEC_NAME_NO_SPACES]Specialization()
{
    _log.Information("Seeding [SPEC_NAME] specialization");

    // ═══════════════════════════════════════════════════════════
    // STEP 1: CREATE SPECIALIZATION METADATA
    // ═══════════════════════════════════════════════════════════

    var spec = new SpecializationData
    {
        // === IDENTITY ===
        SpecializationID = SPEC_ID,                  // Unique ID (see ID conventions below)
        Name = "[SPEC_NAME]",                        // Display name (use special characters if thematic)
        ArchetypeID = ARCHETYPE_ID,                  // 1=Warrior, 2=Adept, 3=Skald, 4=Jotun-Marked

        // === PATH & MECHANICS ===
        PathType = "PATH_TYPE",                      // "Coherent" or "Heretical"
        MechanicalRole = "MECHANICAL_ROLE",          // e.g., "Healer/Support", "Damage/Control"
        PrimaryAttribute = "ATTRIBUTE",              // "MIGHT", "FINESSE", "WITS", "WILL", "STURDINESS"
        SecondaryAttribute = "ATTRIBUTE",            // Secondary attribute for build optimization

        // === FLAVOR ===
        Description = @"[2-3 paragraph lore description.

        Explain what this specialization represents in the world, who practices it,
        and what themes it explores. Make it engaging and thematic.

        Include cultural context and setting details.]",

        Tagline = "[One punchy sentence that captures the essence]",

        // === UNLOCK REQUIREMENTS ===
        UnlockRequirements = new UnlockRequirements
        {
            MinimumLegend = MIN_LEGEND,              // Usually 1 for early specs, 2-3 for advanced
            RequiredAttribute = "ATTRIBUTE",         // Attribute requirement (or "" for none)
            RequiredAttributeValue = VALUE,          // Minimum attribute value (usually 3-4)
            RequiredQuestComplete = "QUEST_ID",      // Quest flag (or "" for none)
            OtherRequirements = "TEXT"               // Freeform text (or "" for none)
        },

        // === RESOURCE & RISK ===
        ResourceSystem = "RESOURCE",                 // "Stamina", "Stress", "HP", or "Mixed"
        TraumaRisk = "RISK_LEVEL",                   // "None", "Low", "Medium", "High", "Extreme"

        // === METADATA ===
        IconEmoji = "EMOJI",                         // Single emoji that represents the spec
        PPCostToUnlock = PP_COST,                    // Usually 3 PP
        IsActive = true
    };

    _specializationRepo.Insert(spec);

    // ═══════════════════════════════════════════════════════════
    // STEP 2: TIER 1 ABILITIES (3 required)
    // Entry abilities - free with specialization unlock
    // ═══════════════════════════════════════════════════════════

    // Tier 1 - Ability 1: [ABILITY_NAME]
    // [1 sentence description]
    var t1_ability1 = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 1,
        SpecializationID = SPEC_ID,
        Name = "[T1_ABILITY_1_NAME]",
        Description = "[Flavor text]",
        MechanicalSummary = "[What it does mechanically]",
        TierLevel = 1,
        PPCost = 0,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 0
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(t1_ability1);

    // Tier 1 - Ability 2: [ABILITY_NAME]
    // [1 sentence description]
    var t1_ability2 = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 2,
        SpecializationID = SPEC_ID,
        Name = "[T1_ABILITY_2_NAME]",
        Description = "[Flavor text]",
        MechanicalSummary = "[What it does mechanically]",
        TierLevel = 1,
        PPCost = 0,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 0
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(t1_ability2);

    // Tier 1 - Ability 3: [ABILITY_NAME]
    // [1 sentence description]
    var t1_ability3 = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 3,
        SpecializationID = SPEC_ID,
        Name = "[T1_ABILITY_3_NAME]",
        Description = "[Flavor text]",
        MechanicalSummary = "[What it does mechanically]",
        TierLevel = 1,
        PPCost = 0,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 0
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(t1_ability3);

    // ═══════════════════════════════════════════════════════════
    // STEP 3: TIER 2 ABILITIES (3 required)
    // Core abilities - 4 PP each, requires 8 PP in tree
    // ═══════════════════════════════════════════════════════════

    // Tier 2 - Ability 4: [ABILITY_NAME]
    var t2_ability1 = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 4,
        SpecializationID = SPEC_ID,
        Name = "[T2_ABILITY_1_NAME]",
        Description = "[Flavor text]",
        MechanicalSummary = "[What it does mechanically]",
        TierLevel = 2,
        PPCost = 4,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 8,
            RequiredAbilityIDs = new List<int> { }  // Optional: require specific Tier 1 abilities
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(t2_ability1);

    // Tier 2 - Ability 5: [ABILITY_NAME]
    var t2_ability2 = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 5,
        SpecializationID = SPEC_ID,
        Name = "[T2_ABILITY_2_NAME]",
        Description = "[Flavor text]",
        MechanicalSummary = "[What it does mechanically]",
        TierLevel = 2,
        PPCost = 4,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 8
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(t2_ability2);

    // Tier 2 - Ability 6: [ABILITY_NAME]
    var t2_ability3 = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 6,
        SpecializationID = SPEC_ID,
        Name = "[T2_ABILITY_3_NAME]",
        Description = "[Flavor text]",
        MechanicalSummary = "[What it does mechanically]",
        TierLevel = 2,
        PPCost = 4,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 8
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "None",
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(t2_ability3);

    // ═══════════════════════════════════════════════════════════
    // STEP 4: TIER 3 ABILITIES (2 required)
    // Advanced abilities - 5 PP each, requires 16 PP in tree
    // ═══════════════════════════════════════════════════════════

    // Tier 3 - Ability 7: [ABILITY_NAME]
    var t3_ability1 = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 7,
        SpecializationID = SPEC_ID,
        Name = "[T3_ABILITY_1_NAME]",
        Description = "[Flavor text]",
        MechanicalSummary = "[What it does mechanically]",
        TierLevel = 3,
        PPCost = 5,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 16,
            RequiredAbilityIDs = new List<int> { }  // Optional: require specific Tier 2 abilities
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "COOLDOWN",
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(t3_ability1);

    // Tier 3 - Ability 8: [ABILITY_NAME]
    var t3_ability2 = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 8,
        SpecializationID = SPEC_ID,
        Name = "[T3_ABILITY_2_NAME]",
        Description = "[Flavor text]",
        MechanicalSummary = "[What it does mechanically]",
        TierLevel = 3,
        PPCost = 5,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 16
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "COOLDOWN",
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(t3_ability2);

    // ═══════════════════════════════════════════════════════════
    // STEP 5: CAPSTONE ABILITY (1 required)
    // Ultimate ability - 6 PP, requires 24 PP in tree + both Tier 3 abilities
    // ═══════════════════════════════════════════════════════════

    // Capstone - Ability 9: [ABILITY_NAME]
    var capstone = new AbilityData
    {
        AbilityID = SPEC_ID * 100 + 9,
        SpecializationID = SPEC_ID,
        Name = "[CAPSTONE_NAME]",
        Description = "[Epic flavor text describing the pinnacle of this path]",
        MechanicalSummary = "[What it does mechanically - should be powerful]",
        TierLevel = 4,
        PPCost = 6,
        MaxRank = MAX_RANK,
        CostToRank2 = RANK2_COST,
        CostToRank3 = RANK3_COST,
        Prerequisites = new AbilityPrerequisites
        {
            RequiredPPInTree = 24,
            RequiredAbilityIDs = new List<int>
            {
                SPEC_ID * 100 + 7,  // Tier 3 Ability 1
                SPEC_ID * 100 + 8   // Tier 3 Ability 2
            }
        },
        AbilityType = "TYPE",
        ActionType = "ACTION",
        TargetType = "TARGET",
        ResourceCost = new AbilityResourceCost
        {
            Stamina = STAMINA
        },
        AttributeUsed = "attribute",
        BonusDice = DICE,
        SuccessThreshold = THRESHOLD,
        DamageDice = DAMAGE,
        HealingDice = HEALING,
        IgnoresArmor = false,
        StatusEffectsApplied = new List<string> { },
        StatusEffectsRemoved = new List<string> { },
        CooldownType = "COOLDOWN",  // Often "Once Per Combat" or "Once Per Scene"
        IsActive = true,
        Tags = "tags"
    };
    _abilityRepo.Insert(capstone);

    _log.Information("[SPEC_NAME] specialization seeded successfully with 9 abilities");
}

// ═══════════════════════════════════════════════════════════════════════════════
// ID CONVENTIONS (IMPORTANT!)
// ═══════════════════════════════════════════════════════════════════════════════

/*
SPECIALIZATION IDs:
- Adept (ArchetypeID = 2): 1-20
  - BoneSetter = 1
  - JotunReader = 2
  - Skald = 3
  - [Your new Adept specs] = 4-20

- Warrior (ArchetypeID = 1): 21-40
  - [Your Warrior specs] = 21-40

- Skald (ArchetypeID = 3): 41-60
  - [Your Skald specs] = 41-60

- Jotun-Marked (ArchetypeID = 4): 61-80
  - [Your Jotun-Marked specs] = 61-80

ABILITY IDs:
- Formula: (SpecializationID * 100) + AbilityNumber
- Examples:
  - BoneSetter (Spec 1): 101-109
  - JotunReader (Spec 2): 201-209
  - Your Spec 4: 401-409
  - Your Spec 21 (Warrior): 2101-2109

This ensures no ID collisions across 80 specializations.
*/

// ═══════════════════════════════════════════════════════════════════════════════
// ARCHETYPE ID REFERENCE
// ═══════════════════════════════════════════════════════════════════════════════

/*
1 = Warrior  - Combat specialists, physical prowess
2 = Adept    - Magic/psionics, support, healing
3 = Skald    - Performance, inspiration, social
4 = Jotun-Marked - Corruption, heretical powers, transformation
*/

// ═══════════════════════════════════════════════════════════════════════════════
// PRE-SUBMISSION CHECKLIST
// ═══════════════════════════════════════════════════════════════════════════════

/*
☐ Specialization ID is unique and in correct archetype range
☐ All 9 abilities use formula: (SpecID * 100) + (1-9)
☐ Exactly 3 Tier 1, 3 Tier 2, 2 Tier 3, 1 Capstone
☐ PP costs match convention (0, 4, 4, 4, 5, 5, 6)
☐ Prerequisites match tiers (0, 8, 16, 24 PP in tree)
☐ Capstone requires both Tier 3 abilities in RequiredAbilityIDs
☐ Passive abilities have 0 stamina cost and no attribute roll
☐ Active abilities have attribute, dice, threshold defined
☐ All abilities have MechanicalSummary filled out
☐ Description and tagline are thematic and engaging
☐ TraumaRisk is accurate for power level
☐ Method is called in SeedExistingSpecializations()
☐ Validated with SpecializationValidator before deployment
*/
