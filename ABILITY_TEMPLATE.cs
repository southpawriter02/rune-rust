// ABILITY SPECIFICATION TEMPLATE
// Copy this template for each ability in your specialization
// Replace ALL_CAPS placeholders with actual values

/*
ABILITY: [ABILITY_NAME]
SPECIALIZATION: [SPEC_NAME]
TIER: [1/2/3/4]
TYPE: [Active/Passive/Reaction]
ROLE: [What this ability does in 1 sentence]
*/

var ABILITY_VARIABLE_NAME = new AbilityData
{
    // === IDENTITY ===
    AbilityID = ABILITY_ID,                          // Unique ID (e.g., 201 for first ability in spec ID 2)
    SpecializationID = SPEC_ID,                      // Parent specialization ID
    Name = "ABILITY_NAME",                           // Display name
    Description = "FLAVOR_TEXT",                     // 2-3 sentences of lore/flavor
    MechanicalSummary = "MECHANICAL_DESCRIPTION",    // 1 sentence: what it does mechanically

    // === TIER & PROGRESSION ===
    TierLevel = TIER_LEVEL,                          // 1, 2, 3, or 4 (Capstone)
    PPCost = PP_COST,                                // Convention: 0 (T1), 4 (T2), 5 (T3), 6 (T4)
    MaxRank = MAX_RANK,                              // 1, 2, or 3
    CostToRank2 = RANK2_COST,                        // 5 PP (or 0 if MaxRank = 1)
    CostToRank3 = RANK3_COST,                        // 7 PP (or 0 if MaxRank < 3)

    // === PREREQUISITES ===
    Prerequisites = new AbilityPrerequisites
    {
        RequiredAbilityIDs = new List<int> { },      // Empty for Tier 1, list of ability IDs for higher tiers
        RequiredPPInTree = PP_IN_TREE                // 0 (T1), 8 (T2), 16 (T3), 24 (T4)
    },

    // === ABILITY TYPE ===
    AbilityType = "ABILITY_TYPE",                    // "Active", "Passive", or "Reaction"
    ActionType = "ACTION_TYPE",                      // "Standard Action", "Bonus Action", "Free Action", "Performance", "Reaction"
    TargetType = "TARGET_TYPE",                      // "Single Enemy", "Self", "All Allies", "Area", etc.

    // === RESOURCE COSTS ===
    ResourceCost = new AbilityResourceCost
    {
        Stamina = STAMINA_COST,                      // 0-100 (0 for Passive abilities)
        Stress = STRESS_COST,                        // 0-20 (rare, for high-risk abilities)
        HP = HP_COST                                 // 0-50 (rare, for blood magic/sacrifice)
    },

    // === ATTRIBUTE & ROLL (for Active/Reaction abilities) ===
    AttributeUsed = "ATTRIBUTE",                     // "might", "finesse", "wits", "will", or "" for Passive
    BonusDice = BONUS_DICE,                          // 0-3 extra dice added to roll
    SuccessThreshold = SUCCESS_THRESHOLD,            // 1-4 successes needed

    // === DAMAGE/HEALING ===
    DamageDice = DAMAGE_DICE,                        // 0-6 dice (0 for non-damaging abilities)
    HealingDice = HEALING_DICE,                      // 0-6 dice (0 for non-healing abilities)
    IgnoresArmor = IGNORES_ARMOR,                    // true/false (true for psychic/trauma damage)

    // === STATUS EFFECTS ===
    StatusEffectsApplied = new List<string>          // e.g., "Bleeding", "Stunned", "Inspired"
    {
        // "STATUS_NAME"
    },
    StatusEffectsRemoved = new List<string>          // e.g., "Poisoned", "Bleeding"
    {
        // "STATUS_NAME"
    },

    // === COOLDOWN ===
    CooldownType = "COOLDOWN",                       // "None", "Once Per Combat", "Once Per Scene", "Once Per Session"

    // === METADATA ===
    IsActive = true,
    Tags = "TAG1,TAG2"                               // e.g., "healing,support" or "damage,aoe"
};

// === TIER-SPECIFIC GUIDELINES ===

/*
TIER 1 (Entry Abilities):
- PPCost = 0 (free with specialization unlock)
- RequiredPPInTree = 0
- MaxRank = 1-3 (higher ranks for build-around abilities)
- Purpose: Establish specialization identity
- Power Level: Modest but flavorful

Example Power Levels:
- Passive: +1 to specific rolls, small resource gains
- Active: 2-3d6 damage, 10-15 stamina cost
- Utility: Niche but useful effects
*/

/*
TIER 2 (Core Abilities):
- PPCost = 4
- RequiredPPInTree = 8
- MaxRank = 2-3
- Purpose: Build on Tier 1 themes, offer meaningful choices
- Power Level: Solid workhorses

Example Power Levels:
- Passive: +2 to rolls, conditional advantages
- Active: 3-4d6 damage, 15-25 stamina cost
- Utility: Combat-relevant effects
*/

/*
TIER 3 (Advanced Abilities):
- PPCost = 5
- RequiredPPInTree = 16
- MaxRank = 2-3
- Purpose: High-impact specialization-defining abilities
- Power Level: Strong but focused

Example Power Levels:
- Passive: Significant bonuses, unique mechanics
- Active: 4-5d6 damage, 25-35 stamina cost, powerful effects
- Utility: Game-changing in specific scenarios
*/

/*
CAPSTONE (Ultimate Ability):
- PPCost = 6
- RequiredPPInTree = 24
- RequiredAbilityIDs = [BOTH Tier 3 ability IDs]
- MaxRank = 1-2 (rarely 3)
- Purpose: Peak expression of specialization fantasy
- Power Level: Extremely powerful, potentially encounter-defining

Example Power Levels:
- Passive: Game-changing persistent effects
- Active: 5-6d6+ damage, 40+ stamina cost, multiple effects
- Once Per Combat/Scene cooldowns common
*/

// === ABILITY TYPE GUIDELINES ===

/*
ACTIVE ABILITIES:
- Must have AttributeUsed, BonusDice, SuccessThreshold
- Stamina cost typically 10-50 depending on tier
- ActionType usually "Standard Action" or "Bonus Action"
- Can deal damage, heal, or apply effects

PASSIVE ABILITIES:
- No AttributeUsed (set to "")
- No resource costs (Stamina = 0)
- ActionType = "Free Action" (always active)
- Provide persistent bonuses or conditional benefits
- No BonusDice/SuccessThreshold/DamageDice

REACTION ABILITIES:
- ActionType = "Reaction"
- Triggered by specific events
- Can have resource costs
- Often defensive or counter-effects
*/

// === BALANCING CHECKLIST ===

/*
☐ PP cost matches tier convention (0/4/5/6)
☐ Prerequisites match tier (0/8/16/24 PP in tree)
☐ Stamina cost reasonable for tier (10-15/15-25/25-35/40+ by tier)
☐ Damage/healing dice appropriate for tier and stamina cost
☐ Passive abilities have NO stamina cost
☐ Active abilities have attribute roll defined
☐ Status effects are existing game effects (check StatusEffect enum)
☐ MaxRank and rank costs make sense (higher ranks for build-arounds)
☐ Cooldown type appropriate for power level
☐ Description is flavorful and thematic
☐ MechanicalSummary is clear and accurate
☐ Tags accurately describe ability function
*/
