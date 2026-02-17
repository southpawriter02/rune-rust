-- ==============================================================================
-- v0.38.12: Advanced Combat Mechanics Descriptors - Database Schema
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Comprehensive combat mechanics descriptor library
-- Timeline: 10-12 hours
-- Philosophy: Combat depth through detailed defensive options, critical moments, and catastrophic failures
-- ==============================================================================

-- ==============================================================================
-- TABLE 1: Combat_Defensive_Action_Descriptors
-- ==============================================================================
-- Purpose: Defensive action descriptors for block, parry, dodge, and counter
-- Pattern: Follows v0.38.11 NPC descriptor patterns
-- Templates: Use {Variable} placeholders for dynamic content
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Combat_Defensive_Action_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    action_type TEXT NOT NULL,           -- Block, Parry, Dodge, Counter

    outcome_type TEXT NOT NULL,          -- Success, Failure, CriticalSuccess, PartialSuccess,
                                         -- WeaponDamaged, ShieldBroken

    -- Contextual Modifiers (NULL = generic, applies to all)
    weapon_type TEXT,                    -- Shield: LightShield, HeavyShield, TowerShield
                                         -- Weapon: OneHanded, TwoHanded, Dagger, Staff
                                         -- Body: Unarmed, NULL

    attack_intensity TEXT,               -- Light, Heavy, Overwhelming, NULL

    environment_context TEXT,            -- OpenGround, TightQuarters, Hazardous, Elevated, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Defensive action description with {Variables}
                                         -- Available: {ActorName}, {WeaponName}, {ShieldName},
                                         --            {DamageBlocked}, {DamageReduced}, {DamageTaken},
                                         --            {EffectApplied}, {DurationTurns},
                                         --            {CounterOpportunity}, {StaggerEffect}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Dramatic", "Skillful", "Desperate", "Opportunistic"]

    -- Constraints
    CONSTRAINT valid_action_type CHECK (action_type IN (
        'Block', 'Parry', 'Dodge', 'Counter'
    )),
    CONSTRAINT valid_outcome_type CHECK (outcome_type IN (
        'Success', 'Failure', 'CriticalSuccess', 'PartialSuccess', 'WeaponDamaged', 'ShieldBroken'
    )),
    CONSTRAINT valid_attack_intensity CHECK (attack_intensity IS NULL OR attack_intensity IN (
        'Light', 'Heavy', 'Overwhelming'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_defensive_action_lookup
ON Combat_Defensive_Action_Descriptors(action_type, outcome_type, weapon_type, attack_intensity);

-- ==============================================================================
-- TABLE 2: Combat_Stance_Descriptors
-- ==============================================================================
-- Purpose: Combat stance descriptors for tactical positioning
-- Usage: Describes entering, maintaining, and switching between combat stances
-- Integration: Fired when player changes stance or maintains current stance
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Combat_Stance_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    stance_type TEXT NOT NULL,           -- Aggressive, Defensive, Balanced, Reckless, Evasive

    description_moment TEXT NOT NULL,    -- Entering, Maintaining, Switching

    -- Contextual Modifiers (NULL = generic, applies to all)
    previous_stance TEXT,                -- Aggressive, Defensive, Balanced, Reckless, Evasive, NULL
                                         -- Only used for Switching moment

    situation_context TEXT,              -- Winning, Losing, EvenMatch, Outnumbered, Surrounded, NULL

    weapon_configuration TEXT,           -- OneHanded, TwoHanded, DualWield, ShieldAndWeapon, Unarmed, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Stance description with {Variables}
                                         -- Available: {ActorName}, {WeaponName}, {StanceName},
                                         --            {PreviousStance}, {AttackBonus}, {DefenseBonus},
                                         --            {RiskLevel}, {EnemyCount}, {AllyCount}, {Situation}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Tactical", "Defensive", "Aggressive", "Risky"]

    -- Constraints
    CONSTRAINT valid_stance_type CHECK (stance_type IN (
        'Aggressive', 'Defensive', 'Balanced', 'Reckless', 'Evasive'
    )),
    CONSTRAINT valid_description_moment CHECK (description_moment IN (
        'Entering', 'Maintaining', 'Switching'
    )),
    CONSTRAINT valid_previous_stance CHECK (previous_stance IS NULL OR previous_stance IN (
        'Aggressive', 'Defensive', 'Balanced', 'Reckless', 'Evasive'
    )),
    CONSTRAINT valid_situation_context CHECK (situation_context IS NULL OR situation_context IN (
        'Winning', 'Losing', 'EvenMatch', 'Outnumbered', 'Surrounded'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_stance_lookup
ON Combat_Stance_Descriptors(stance_type, description_moment, situation_context);

-- ==============================================================================
-- TABLE 3: Combat_Critical_Hit_Descriptors
-- ==============================================================================
-- Purpose: Critical hit descriptors for devastating combat outcomes
-- Usage: Describes critical hits that inflict maximum damage and special effects
-- Integration: Triggered on critical hit rolls in combat
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Combat_Critical_Hit_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    attack_category TEXT NOT NULL,       -- Melee, Ranged, Magic

    damage_type TEXT NOT NULL,           -- Slashing, Crushing, Piercing, Fire, Ice, Lightning,
                                         -- Necrotic, Radiant

    -- Contextual Modifiers (NULL = generic, applies to all)
    weapon_or_spell_type TEXT,           -- Melee: Sword, Axe, Hammer, Spear, Dagger
                                         -- Ranged: Bow, Crossbow
                                         -- Magic: Fire, Ice, Lightning, Necrotic, Radiant
                                         -- NULL (generic)

    target_type TEXT,                    -- Humanoid, Beast, Construct, Forlorn, Glitch, NULL

    special_effect TEXT,                 -- Bleeding, Stunned, Frozen, Burning, Paralyzed,
                                         -- Dying, InstantKill, ArmorDestroyed, Prone, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Critical hit description with {Variables}
                                         -- Available: {AttackerName}, {WeaponName}, {SpellName},
                                         --            {TargetName}, {TargetType}, {DamageAmount},
                                         --            {DamageType}, {Overkill}, {EffectApplied},
                                         --            {EffectDuration}, {SpecialOutcome}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Devastating", "Lethal", "Brutal", "Spectacular"]

    -- Constraints
    CONSTRAINT valid_attack_category CHECK (attack_category IN (
        'Melee', 'Ranged', 'Magic'
    )),
    CONSTRAINT valid_damage_type CHECK (damage_type IN (
        'Slashing', 'Crushing', 'Piercing', 'Fire', 'Ice', 'Lightning', 'Necrotic', 'Radiant'
    )),
    CONSTRAINT valid_special_effect CHECK (special_effect IS NULL OR special_effect IN (
        'Bleeding', 'Stunned', 'Frozen', 'Burning', 'Paralyzed', 'Dying', 'InstantKill',
        'ArmorDestroyed', 'Prone'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_critical_hit_lookup
ON Combat_Critical_Hit_Descriptors(attack_category, damage_type, weapon_or_spell_type);

-- ==============================================================================
-- TABLE 4: Combat_Fumble_Descriptors
-- ==============================================================================
-- Purpose: Fumble and critical failure descriptors for catastrophic combat outcomes
-- Usage: Describes fumbles in attacks, magic, and defense
-- Integration: Triggered on fumble rolls (natural 1) in combat
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Combat_Fumble_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    fumble_category TEXT NOT NULL,       -- AttackFumble, MagicFumble, DefensiveFumble

    fumble_type TEXT NOT NULL,           -- AttackFumble: Miss, WeaponDrop, SelfInjury, WeaponBreak, Overextension
                                         -- MagicFumble: Miscast, Backfire, WildSurge, Burnout, CorruptionSurge
                                         -- DefensiveFumble: ShieldDrop, Disarmed, Tripped, Exposed, Stumbled

    -- Contextual Modifiers (NULL = generic, applies to all)
    equipment_type TEXT,                 -- Sword, Axe, Hammer, Bow, Shield, Staff, NULL

    severity TEXT,                       -- Minor, Moderate, Severe, Catastrophic, NULL

    environment_factor TEXT,             -- Slippery, Unstable, Hazardous, Cramped, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Fumble description with {Variables}
                                         -- Available: {ActorName}, {WeaponName}, {SpellName},
                                         --            {DamageTaken}, {ItemLost}, {EffectApplied},
                                         --            {EnvironmentFactor}, {HazardTriggered},
                                         --            {TurnsLost}, {EffectDuration}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Embarrassing", "Dangerous", "Costly", "Recoverable"]

    -- Constraints
    CONSTRAINT valid_fumble_category CHECK (fumble_category IN (
        'AttackFumble', 'MagicFumble', 'DefensiveFumble'
    )),
    CONSTRAINT valid_fumble_type CHECK (fumble_type IN (
        'Miss', 'WeaponDrop', 'SelfInjury', 'WeaponBreak', 'Overextension',
        'Miscast', 'Backfire', 'WildSurge', 'Burnout', 'CorruptionSurge',
        'ShieldDrop', 'Disarmed', 'Tripped', 'Exposed', 'Stumbled'
    )),
    CONSTRAINT valid_severity CHECK (severity IS NULL OR severity IN (
        'Minor', 'Moderate', 'Severe', 'Catastrophic'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_fumble_lookup
ON Combat_Fumble_Descriptors(fumble_category, fumble_type, severity);

-- ==============================================================================
-- TABLE 5: Combat_Maneuver_Descriptors
-- ==============================================================================
-- Purpose: Special combat maneuver descriptors (riposte, disarm, trip, grapple)
-- Usage: Describes tactical combat maneuvers and their outcomes
-- Integration: Triggered when player uses special combat abilities
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Combat_Maneuver_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    maneuver_type TEXT NOT NULL,         -- Riposte, Disarm, Trip, Grapple, Shove, Feint

    outcome_type TEXT NOT NULL,          -- Success, Failure, CriticalSuccess

    -- Contextual Modifiers (NULL = generic, applies to all)
    weapon_type TEXT,                    -- Sword, Dagger, Unarmed, Shield, Polearm, NULL

    target_type TEXT,                    -- Humanoid, Beast, Large, Small, Construct, NULL

    environment_context TEXT,            -- OpenGround, TightQuarters, Slippery, Elevated, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Maneuver description with {Variables}
                                         -- Available: {AttackerName}, {WeaponName}, {TargetName},
                                         --            {TargetWeapon}, {EffectApplied}, {StatusInflicted},
                                         --            {AdvantageGained}, {CounterOpportunity}, {BonusAction}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Tactical", "Skillful", "Opportunistic", "Defensive"]

    -- Constraints
    CONSTRAINT valid_maneuver_type CHECK (maneuver_type IN (
        'Riposte', 'Disarm', 'Trip', 'Grapple', 'Shove', 'Feint'
    )),
    CONSTRAINT valid_outcome_type_maneuver CHECK (outcome_type IN (
        'Success', 'Failure', 'CriticalSuccess'
    )),
    CONSTRAINT valid_target_type CHECK (target_type IS NULL OR target_type IN (
        'Humanoid', 'Beast', 'Large', 'Small', 'Construct'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_maneuver_lookup
ON Combat_Maneuver_Descriptors(maneuver_type, outcome_type, target_type);

-- ==============================================================================
-- SCHEMA COMPLETE
-- ==============================================================================
-- Next Steps:
-- 1. Load this schema into the database
-- 2. Populate with v0.38.12_combat_mechanics_descriptors_data.sql
-- 3. Implement DescriptorRepository_CombatMechanicsExtensions.cs
-- 4. Implement CombatFlavorTextService.cs
-- 5. Integrate with CombatEngine and related combat systems
-- ==============================================================================
