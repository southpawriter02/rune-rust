-- ==============================================================================
-- v0.38.7: Ability & Galdr Flavor Text - Outcome Descriptors
-- ==============================================================================
-- Purpose: Ability outcome narratives (damage, healing, effects)
-- Coverage: Success levels, target types, effect categories
-- ==============================================================================

-- ==============================================================================
-- FLAME BOLT - Damage Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
-- Hit outcomes
('FlameBolt', 'Hit', 3, 'Enemy', 'Damage', 'Fire engulfs the {Target}, scorching their {Target_Location}! Acrid smoke rises from charred flesh.', 1.0, '["Damage", "Fire"]'),
('FlameBolt', 'Hit', 3, 'Enemy', 'Damage', 'Flames wash over the {Target}! They cry out as fire sears exposed skin.', 1.0, '["Dramatic"]'),
('FlameBolt', 'Hit', 4, 'Enemy', 'Damage', 'Your fire bolt strikes true! The {Target} staggers, wreathed in flames!', 1.0, '["Impactful"]'),

-- Critical Hit
('FlameBolt', 'CriticalHit', 5, 'Enemy', 'Damage', 'The Fehu rune erupts! The {Target} is consumed by primal fire, their {Vital_Location} charred to ash!', 1.0, '["Devastating", "Epic"]'),
('FlameBolt', 'CriticalHit', 6, 'Enemy', 'Damage', 'Your flame bolt strikes the {Target}''s {Vital_Location} directly! Fire explodes outward—they collapse, burning!', 1.0, '["Lethal"]'),
('FlameBolt', 'CriticalHit', 5, 'Enemy', 'Damage', 'Perfect strike! Fire erupts from within the {Target}—the rune has found their core!', 1.0, '["Brutal"]'),

-- Miss
('FlameBolt', 'Miss', 1, 'Enemy', 'Damage', 'Your flames dissipate before reaching the {Target}!', 1.0, '["Failure"]'),
('FlameBolt', 'Miss', 2, 'Enemy', 'Damage', 'The {Target} dodges your fire bolt with surprising agility!', 1.0, '["Avoided"]'),

-- Resisted
('FlameBolt', 'Resisted', 3, 'Enemy', 'Damage', 'The {Target} shrugs off your flames, barely phased! Fire-resistant hide turns away the worst of it.', 1.0, '["Ineffective"]'),
('FlameBolt', 'Resisted', 3, 'Enemy', 'Damage', 'Your Fehu rune flickers—the {Target} resists! Flames wash harmlessly over them.', 1.0, '["Nullified"]');

-- ==============================================================================
-- FROST LANCE - Damage Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
('FrostLance', 'Hit', 3, 'Enemy', 'Damage', 'Ice pierces the {Target}''s {Target_Location}! Frost spreads from the wound, freezing flesh!', 1.0, '["Ice", "Damage"]'),
('FrostLance', 'Hit', 4, 'Enemy', 'Damage', 'Your frost lance strikes home! The {Target} howls as crystalline ice burrows into them!', 1.0, '["Brutal"]'),
('FrostLance', 'CriticalHit', 5, 'Enemy', 'Damage', 'Thurisaz''s full fury! The lance plunges through the {Target}''s {Vital_Location}—ice erupts from within, flash-freezing them!', 1.0, '["Devastating"]'),
('FrostLance', 'CriticalHit', 6, 'Enemy', 'Damage', 'Perfect strike! Your ice lance impales the {Target}—they freeze solid, a statue of agony!', 1.0, '["Lethal", "Horrifying"]'),
('FrostLance', 'Miss', 1, 'Enemy', 'Damage', 'Your frost lance shatters against the ground, missing the {Target} entirely!', 1.0, '["Failure"]');

-- ==============================================================================
-- LIGHTNING BOLT - Damage Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
('LightningBolt', 'Hit', 3, 'Enemy', 'Damage', 'Lightning arcs into the {Target}! The invisible fire courses through them—muscles spasming involuntarily!', 1.0, '["Lightning", "Damage"]'),
('LightningBolt', 'Hit', 4, 'Enemy', 'Damage', 'Your bolt strikes true! The {Target} convulses as lightning ravages their {Target_Location}!', 1.0, '["Brutal"]'),
('LightningBolt', 'CriticalHit', 5, 'Enemy', 'Damage', 'Ansuz erupts in white-hot fury! Lightning chains through the {Target}—their {Vital_Location} explodes in sparks!', 1.0, '["Devastating"]'),
('LightningBolt', 'CriticalHit', 6, 'Enemy', 'Damage', 'Direct hit! Lightning overloads the {Target}—they collapse, smoke rising from charred remains!', 1.0, '["Lethal"]'),
('LightningBolt', 'Amplified', 5, 'Enemy', 'Damage', 'Your lightning finds conductive material! The bolt amplifies, arcing through the {Target} with doubled fury!', 1.0, '["Amplified", "Environmental"]');

-- ==============================================================================
-- HEALING CHANT - Healing Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
('HealingChant', 'FullEffect', 3, 'Self', 'Healing', 'Berkanan''s warmth flows through you. Wounds knit, bruises fade, bones realign. [+{Healing} HP]', 1.0, '["Healing"]'),
('HealingChant', 'FullEffect', 4, 'Self', 'Healing', 'Green light bathes your injuries. Pain recedes as flesh mends itself! [+{Healing} HP]', 1.0, '["Restorative"]'),
('HealingChant', 'FullEffect', 5, 'Self', 'Healing', 'Perfect healing! Berkanan''s life-force surges—even grievous wounds vanish, vitality fully restored! [+{Healing} HP]', 1.0, '["Epic"]'),
('HealingChant', 'PartialEffect', 2, 'Self', 'Healing', 'Berkanan''s touch is gentle but weak. Minor wounds close, but deeper injuries remain. [+{Healing} HP]', 1.0, '["Limited"]'),
('HealingChant', 'FullEffect', 3, 'Ally', 'Healing', 'You direct Berkanan''s gift toward {Target}. Green light envelops them—wounds closing before your eyes! [+{Healing} HP]', 1.0, '["Support"]');

-- ==============================================================================
-- RUNE WARD - Defensive Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
('RuneWard', 'FullEffect', 3, 'Self', 'Buff', 'Tiwaz blazes golden! Protective wards surround you—divine barriers against harm! [Ward Active]', 1.0, '["Defensive"]'),
('RuneWard', 'FullEffect', 5, 'Self', 'Buff', 'Perfect invocation! Tiwaz manifests as unbreakable wards—nothing shall pierce this sanctified ground! [Ward Active]', 1.0, '["Epic", "Defensive"]');

-- ==============================================================================
-- DRAIN LIFE - Draining Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
('DrainLife', 'Hit', 3, 'Enemy', 'Debuff', 'Naudiz''s shadow tendrils sink into the {Target}! Life force flows from them into you—they wither as you strengthen! [Drained: {Damage}]', 1.0, '["Dark", "Draining"]'),
('DrainLife', 'Hit', 4, 'Enemy', 'Debuff', 'Your Galdr siphons vitality! The {Target} pales, weakening visibly as life-blood floods into you! [Drained: {Damage}]', 1.0, '["Vampiric"]'),
('DrainLife', 'CriticalHit', 5, 'Enemy', 'Debuff', 'Naudiz erupts! Shadow pours from the {Target}—their life force torn away completely! You feel invigorated! [Drained: {Damage}]', 1.0, '["Devastating"]'),
('DrainLife', 'Resisted', 2, 'Enemy', 'Debuff', 'The {Target} resists your draining magic! Naudiz''s tendrils dissipate before taking hold.', 1.0, '["Ineffective"]');

-- ==============================================================================
-- FROZEN TIME - Control Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
('FrozenTime', 'FullEffect', 4, 'Enemy', 'Control', 'Isa takes hold! The {Target} freezes mid-motion, trapped in crystalline stasis! [Stunned: {Duration} turns]', 1.0, '["Control"]'),
('FrozenTime', 'FullEffect', 5, 'Enemy', 'Control', 'Time stops! The {Target} is locked completely—a statue frozen in the moment! [Stunned: {Duration} turns]', 1.0, '["Epic", "Control"]'),
('FrozenTime', 'PartialEffect', 3, 'Enemy', 'Control', 'Isa partially manifests—the {Target} slows but doesn''t freeze completely! [Slowed: {Duration} turns]', 1.0, '["Partial"]'),
('FrozenTime', 'Resisted', 2, 'Enemy', 'Control', 'The {Target} breaks free of Isa''s grip! They move sluggishly for a moment, then resume normal speed!', 1.0, '["Resisted"]');

-- ==============================================================================
-- EMPOWER - Buff Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
('Empower', 'FullEffect', 3, 'Self', 'Buff', 'Mannaz surges through you! Strength, speed, will—all capabilities enhanced! [Empowered: {Duration} turns]', 1.0, '["Buff"]'),
('Empower', 'FullEffect', 5, 'Self', 'Buff', 'Perfect empowerment! Mannaz unlocks human potential—you feel invincible! [Empowered: {Duration} turns]', 1.0, '["Epic", "Buff"]');

-- ==============================================================================
-- CLEANSING WAVE - Purification Outcomes
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, effect_category, descriptor_text, weight, tags)
VALUES
('CleansingWave', 'FullEffect', 3, 'Self', 'Utility', 'Laguz''s purifying waters wash over you! Poison, corruption, disease—all cleansed away! [Status Effects Removed]', 1.0, '["Purification"]'),
('CleansingWave', 'FullEffect', 5, 'Self', 'Utility', 'Perfect purification! Laguz manifests as cascading purity—even Blight corruption cannot withstand it! [All Debuffs Removed]', 1.0, '["Epic", "Cleansing"]');

-- ==============================================================================
-- ENEMY ARCHETYPE-SPECIFIC OUTCOMES
-- ==============================================================================

INSERT INTO Galdr_Outcome_Descriptors (ability_name, outcome_type, success_count, target_type, enemy_archetype, effect_category, descriptor_text, weight, tags)
VALUES
-- Servitor (mechanical)
('FlameBolt', 'Hit', 3, 'Enemy', 'Servitor', 'Damage', 'Fire washes over the Servitor''s chassis! Circuits melt, hydraulics burst—it jerks spasmodically!', 1.0, '["Mechanical"]'),
('LightningBolt', 'Hit', 3, 'Enemy', 'Servitor', 'Damage', 'Lightning surges into the Servitor! Its systems overload—sparks erupting from joints!', 1.0, '["Electrical", "Effective"]'),

-- Forlorn (undead)
('FlameBolt', 'Hit', 3, 'Enemy', 'Forlorn', 'Damage', 'Fire consumes the Forlorn''s desiccated flesh! It moans hollowly as flames spread across withered limbs!', 1.0, '["Undead"]'),
('HealingChant', 'Resisted', 0, 'Enemy', 'Forlorn', 'Damage', 'Berkanan recoils from the Forlorn—healing magic has no purchase on the dead! Instead, it burns them!', 1.0, '["Reversed", "Undead"]'),

-- Corrupted_Dvergr
('CleansingWave', 'Hit', 4, 'Enemy', 'Corrupted_Dvergr', 'Damage', 'Laguz''s purifying wave strikes the Corrupted Dvergr! Blight corruption screams as it''s torn away—the dvergr howls in agony!', 1.0, '["Effective", "Purification"]'),

-- Blight_Touched_Beast
('DrainLife', 'Amplified', 4, 'Enemy', 'Blight_Touched_Beast', 'Debuff', 'Naudiz feeds on Blight corruption! Your draining is amplified by the beast''s tainted life force!', 1.0, '["Amplified", "Dark"]');

-- ==============================================================================
-- Statistics Query
-- ==============================================================================
-- SELECT ability_name, outcome_type, COUNT(*) as count
-- FROM Galdr_Outcome_Descriptors
-- GROUP BY ability_name, outcome_type
-- ORDER BY ability_name, count DESC;
