-- ==============================================================================
-- v0.38.8: Status Effects & Condition Descriptors - Data Population
-- ==============================================================================
-- Purpose: Comprehensive status effect descriptor library
-- Contents: 50+ application descriptors, 40+ tick descriptors, 30+ end descriptors
-- ==============================================================================

-- ==============================================================================
-- PART 1: STATUS EFFECT APPLICATION DESCRIPTORS (OnApply)
-- ==============================================================================

-- ============================================================
-- BURNING - APPLICATION DESCRIPTORS (OnApply)
-- ============================================================

-- Source: Enemy Attack (Fire)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Burning', 'OnApply', NULL, 'EnemyAttack', 'Fire', 'Player', 'The {Enemy}''s flames catch on your clothing—you''re burning!', 1.0, '["Dramatic", "Visceral"]'),
('Burning', 'OnApply', NULL, 'EnemyAttack', 'Fire', 'Player', 'Fire clings to you, eating through leather and flesh alike!', 1.0, '["Dramatic", "Visceral"]'),
('Burning', 'OnApply', NULL, 'EnemyAttack', 'Fire', 'Player', 'You''re engulfed in flames—the heat is excruciating!', 1.0, '["Dramatic", "Extreme"]'),
('Burning', 'OnApply', NULL, 'EnemyAttack', 'Fire', 'Enemy', 'Your flames catch on the {Enemy}—it burns!', 1.0, '["Concise"]'),
('Burning', 'OnApply', NULL, 'EnemyAttack', 'Fire', 'Enemy', 'The {Enemy} is engulfed in fire!', 1.0, '["Concise"]');

-- Source: Environmental Hazard
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Burning', 'OnApply', NULL, 'Environmental', 'Lava', 'Player', 'You step too close to the lava flow—your boots ignite!', 1.0, '["Environmental", "Dramatic"]'),
('Burning', 'OnApply', NULL, 'Environmental', 'BrokenPipe', 'Player', 'A gout of flame from the broken pipe washes over you!', 1.0, '["Environmental", "Dramatic"]'),
('Burning', 'OnApply', NULL, 'Environmental', 'CollapsingCeiling', 'Player', 'Burning embers land on you from the collapsing ceiling!', 1.0, '["Environmental", "Dramatic"]');

-- Source: Galdr Backfire
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Burning', 'OnApply', NULL, 'GaldrBackfire', 'Fehu', 'Player', 'Your miscast Fehu turns inward—you''re burning from your own magic!', 1.0, '["Magical", "Ironic"]'),
('Burning', 'OnApply', NULL, 'GaldrBackfire', 'Paradox', 'Player', 'Paradoxical flame erupts from your hands, searing your flesh!', 1.0, '["Magical", "Blight"]');

-- ============================================================
-- BLEEDING - APPLICATION DESCRIPTORS (OnApply)
-- ============================================================

-- Source: Weapon (Slashing)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Bleeding', 'OnApply', NULL, 'EnemyAttack', 'Slashing', 'Player', 'The {Enemy}''s blade opens a deep gash in your {Location}—blood flows freely!', 1.0, '["Visceral", "Dramatic"]'),
('Bleeding', 'OnApply', NULL, 'EnemyAttack', 'Slashing', 'Player', 'You feel the cut—warm blood soaking through your armor!', 1.0, '["Visceral"]'),
('Bleeding', 'OnApply', NULL, 'EnemyAttack', 'Slashing', 'Player', 'The wound is deep—you''re bleeding heavily!', 1.0, '["Dramatic"]'),
('Bleeding', 'OnApply', NULL, 'EnemyAttack', 'Slashing', 'Enemy', 'Your blade cuts deep—the {Enemy} bleeds!', 1.0, '["Concise"]'),
('Bleeding', 'OnApply', NULL, 'EnemyAttack', 'Slashing', 'Enemy', 'Blood pours from the {Enemy}''s wound!', 1.0, '["Concise"]');

-- Source: Weapon (Piercing)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Bleeding', 'OnApply', NULL, 'EnemyAttack', 'Piercing', 'Player', 'The arrow punches through—when you pull it free, blood pulses from the wound!', 1.0, '["Visceral", "Detailed"]'),
('Bleeding', 'OnApply', NULL, 'EnemyAttack', 'Piercing', 'Player', 'The {Enemy}''s strike pierces clean through—you begin bleeding profusely!', 1.0, '["Dramatic"]'),
('Bleeding', 'OnApply', NULL, 'EnemyAttack', 'Piercing', 'Enemy', 'Your strike pierces the {Enemy}—blood flows!', 1.0, '["Concise"]');

-- Source: Environmental
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Bleeding', 'OnApply', NULL, 'Environmental', 'JaggedMetal', 'Player', 'Jagged metal tears your flesh as you stumble—you''re bleeding!', 1.0, '["Environmental", "Visceral"]'),
('Bleeding', 'OnApply', NULL, 'Environmental', 'BrokenGlass', 'Player', 'Broken glass from shattered crystals slashes you open!', 1.0, '["Environmental", "Dramatic"]');

-- ============================================================
-- POISONED - APPLICATION DESCRIPTORS (OnApply)
-- ============================================================

-- Source: Beast Bite
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Poisoned', 'OnApply', NULL, 'BeastBite', 'Venom', 'Player', 'The beast''s fangs sink deep—you feel venom burning through your veins!', 1.0, '["Visceral", "Dramatic"]'),
('Poisoned', 'OnApply', NULL, 'BeastBite', 'Venom', 'Player', 'Poison seeps from the creature''s bite, spreading cold numbness!', 1.0, '["Visceral", "Sensory"]'),
('Poisoned', 'OnApply', NULL, 'BeastBite', 'BlightTouched', 'Player', 'The Blight-touched animal''s saliva carries corruption—you''re poisoned!', 1.0, '["Blight", "Dramatic"]');

-- Source: Environmental (Toxic Haze)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Poisoned', 'OnApply', NULL, 'ToxicHaze', 'Gas', 'Player', 'The toxic fumes sear your lungs—you''ve inhaled poison!', 1.0, '["Environmental", "Visceral"]'),
('Poisoned', 'OnApply', NULL, 'ToxicHaze', 'Chemical', 'Player', 'Chemical vapors burn your throat and nose!', 1.0, '["Environmental", "Sensory"]'),
('Poisoned', 'OnApply', NULL, 'ToxicHaze', 'Caustic', 'Player', 'You breathe the caustic air—immediately you feel sick!', 1.0, '["Environmental", "Dramatic"]');

-- Source: Weapon (Coated)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Poisoned', 'OnApply', NULL, 'WeaponCoated', 'Venom', 'Player', 'The blade was poisoned! You feel it spreading from the wound!', 1.0, '["Tactical", "Dramatic"]'),
('Poisoned', 'OnApply', NULL, 'WeaponCoated', 'Venom', 'Player', 'Venom from the coated weapon enters your bloodstream!', 1.0, '["Tactical", "Visceral"]'),
('Poisoned', 'OnApply', NULL, 'WeaponCoated', 'Venom', 'Enemy', 'Your poisoned blade strikes true—the {Enemy} is poisoned!', 1.0, '["Tactical", "Concise"]');

-- ============================================================
-- STUNNED - APPLICATION DESCRIPTORS (OnApply)
-- ============================================================

-- Source: Lightning
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Stunned', 'OnApply', NULL, 'Lightning', 'Electric', 'Player', 'Electricity courses through you—your muscles lock up!', 1.0, '["Dramatic", "Visceral"]'),
('Stunned', 'OnApply', NULL, 'Lightning', 'Electric', 'Player', 'The lightning bolt leaves you convulsing, unable to act!', 1.0, '["Extreme", "Visceral"]'),
('Stunned', 'OnApply', NULL, 'Lightning', 'Electric', 'Player', 'Your nervous system rebels—you''re paralyzed by the shock!', 1.0, '["Technical", "Dramatic"]'),
('Stunned', 'OnApply', NULL, 'Lightning', 'Electric', 'Enemy', 'Lightning strikes the {Enemy}—it''s stunned!', 1.0, '["Concise"]');

-- Source: Concussive Force
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Stunned', 'OnApply', NULL, 'ConcussiveForce', 'Impact', 'Player', 'The impact rattles your brain—everything spins!', 1.0, '["Visceral", "Sensory"]'),
('Stunned', 'OnApply', NULL, 'ConcussiveForce', 'Impact', 'Player', 'Your head rings from the blow—you stagger, dazed!', 1.0, '["Visceral", "Dramatic"]'),
('Stunned', 'OnApply', NULL, 'ConcussiveForce', 'Explosion', 'Player', 'The explosion sends you reeling, ears ringing, vision swimming!', 1.0, '["Extreme", "Sensory"]'),
('Stunned', 'OnApply', NULL, 'ConcussiveForce', 'Impact', 'Enemy', 'Your strike dazes the {Enemy}!', 1.0, '["Concise"]');

-- ============================================================
-- BLIGHT CORRUPTION - APPLICATION DESCRIPTORS (OnApply)
-- ============================================================

-- Source: Alfheim Reality Tear
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('BlightCorruption', 'OnApply', NULL, 'AlfheimTear', 'Paradox', 'Player', 'Reality fractures—your mind struggles to process the paradox!', 1.0, '["Blight", "Cosmic", "Dramatic"]'),
('BlightCorruption', 'OnApply', NULL, 'AlfheimTear', 'Paradox', 'Player', 'The Blight''s corruption seeps into you—thoughts become contradictory!', 1.0, '["Blight", "Psychological"]'),
('BlightCorruption', 'OnApply', NULL, 'AlfheimTear', 'AllRune', 'Player', 'You feel the All-Rune''s influence—is this real or memory or future?', 1.0, '["Blight", "Cosmic", "Verbose"]');

-- Source: Forlorn Touch
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('BlightCorruption', 'OnApply', NULL, 'ForlornTouch', 'Psychic', 'Player', 'The Forlorn''s touch is ice and emptiness—something vital drains from you!', 1.0, '["Blight", "Psychic", "Dramatic"]'),
('BlightCorruption', 'OnApply', NULL, 'ForlornTouch', 'Psychic', 'Player', 'Psychic anguish washes over you, the Forlorn sharing its eternal torment!', 1.0, '["Blight", "Psychic", "Verbose"]');

-- ============================================================
-- SLOWED - APPLICATION DESCRIPTORS (OnApply)
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Slowed', 'OnApply', NULL, 'EnemyAttack', 'Ice', 'Player', 'Frost spreads across your limbs—you can barely move!', 1.0, '["Dramatic", "Sensory"]'),
('Slowed', 'OnApply', NULL, 'EnemyAttack', 'Ice', 'Player', 'Cold seeps into your bones, slowing your movements!', 1.0, '["Visceral"]'),
('Slowed', 'OnApply', NULL, 'EnemyAttack', 'Exhaustion', 'Player', 'Your limbs feel like lead—you''re slowed!', 1.0, '["Concise"]'),
('Slowed', 'OnApply', NULL, 'EnemyAttack', 'Ice', 'Enemy', 'Ice encrusts the {Enemy}—it moves sluggishly!', 1.0, '["Concise"]');

-- ============================================================
-- WEAKENED - APPLICATION DESCRIPTORS (OnApply)
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Weakened', 'OnApply', NULL, 'EnemyAttack', 'Drain', 'Player', 'Your strength drains away—you feel weakened!', 1.0, '["Dramatic"]'),
('Weakened', 'OnApply', NULL, 'EnemyAttack', 'Curse', 'Player', 'Dark energy saps your vitality!', 1.0, '["Dramatic", "Magical"]'),
('Weakened', 'OnApply', NULL, 'EnemyAttack', 'Drain', 'Enemy', 'The {Enemy} looks weakened!', 1.0, '["Concise"]');

-- ============================================================
-- CORRODING - APPLICATION DESCRIPTORS (OnApply)
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, source_detail, target_type, descriptor_text, weight, tags)
VALUES
('Corroding', 'OnApply', NULL, 'EnemyAttack', 'Acid', 'Player', 'Acid splashes across you—it burns through armor and skin!', 1.0, '["Visceral", "Dramatic"]'),
('Corroding', 'OnApply', NULL, 'EnemyAttack', 'Acid', 'Player', 'The caustic liquid eats into your flesh!', 1.0, '["Visceral", "Extreme"]'),
('Corroding', 'OnApply', NULL, 'Environmental', 'Acid', 'Player', 'You stumble into corrosive liquid—it burns!', 1.0, '["Environmental", "Dramatic"]'),
('Corroding', 'OnApply', NULL, 'EnemyAttack', 'Acid', 'Enemy', 'Acid eats into the {Enemy}!', 1.0, '["Concise"]');

-- ==============================================================================
-- PART 2: STATUS EFFECT TICK DESCRIPTORS (OnTick) - Ongoing Effects
-- ==============================================================================

-- ============================================================
-- BURNING - TICK DESCRIPTORS (OnTick)
-- ============================================================

-- Minor Severity (1-2 damage/turn)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Burning', 'OnTick', 'Minor', NULL, 'Player', 'Small flames lick at your clothing—the burns are painful but manageable.', 1.0, '["Concise"]'),
('Burning', 'OnTick', 'Minor', NULL, 'Player', 'You''re still smoking, small fires smoldering in your gear.', 1.0, '["Concise"]'),
('Burning', 'OnTick', 'Minor', NULL, 'Enemy', 'The {Enemy} smolders, still burning.', 1.0, '["Concise"]');

-- Moderate Severity (3-5 damage/turn)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Burning', 'OnTick', 'Moderate', NULL, 'Player', 'Fire consumes your armor—the heat is intense!', 1.0, '["Dramatic"]'),
('Burning', 'OnTick', 'Moderate', NULL, 'Player', 'You''re engulfed in flames—every moment is agony!', 1.0, '["Dramatic", "Extreme"]'),
('Burning', 'OnTick', 'Moderate', NULL, 'Player', 'The burning spreads—you desperately try to extinguish yourself!', 1.0, '["Dramatic", "Urgent"]'),
('Burning', 'OnTick', 'Moderate', NULL, 'Enemy', 'The {Enemy} burns fiercely!', 1.0, '["Concise"]');

-- Severe Severity (6+ damage/turn)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Burning', 'OnTick', 'Severe', NULL, 'Player', 'The flames are all-consuming—your flesh blackens and chars!', 1.0, '["Extreme", "Visceral"]'),
('Burning', 'OnTick', 'Severe', NULL, 'Player', 'You''re a pillar of fire—the pain is beyond comprehension!', 1.0, '["Extreme", "Dramatic"]'),
('Burning', 'OnTick', 'Severe', NULL, 'Player', 'Immolation is certain if you don''t stop the burning NOW!', 1.0, '["Extreme", "Urgent"]'),
('Burning', 'OnTick', 'Severe', NULL, 'Enemy', 'The {Enemy} is consumed by flames!', 1.0, '["Dramatic"]');

-- ============================================================
-- BLEEDING - TICK DESCRIPTORS (OnTick)
-- ============================================================

-- Minor Severity
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Bleeding', 'OnTick', 'Minor', NULL, 'Player', 'Blood seeps steadily from your wound.', 1.0, '["Concise"]'),
('Bleeding', 'OnTick', 'Minor', NULL, 'Player', 'The cut throbs—you''re losing blood.', 1.0, '["Concise"]'),
('Bleeding', 'OnTick', 'Minor', NULL, 'Enemy', 'The {Enemy} continues bleeding.', 1.0, '["Concise"]');

-- Moderate Severity
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Bleeding', 'OnTick', 'Moderate', NULL, 'Player', 'You''re bleeding heavily—crimson spreads across your clothing!', 1.0, '["Dramatic", "Visceral"]'),
('Bleeding', 'OnTick', 'Moderate', NULL, 'Player', 'Blood pulses from the wound with each heartbeat!', 1.0, '["Visceral", "Detailed"]'),
('Bleeding', 'OnTick', 'Moderate', NULL, 'Enemy', 'Blood pours from the {Enemy}''s wounds!', 1.0, '["Dramatic"]');

-- Severe Severity
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Bleeding', 'OnTick', 'Severe', NULL, 'Player', 'You''re hemorrhaging—the world swims as blood loss takes its toll!', 1.0, '["Extreme", "Sensory"]'),
('Bleeding', 'OnTick', 'Severe', NULL, 'Player', 'So much blood—you''re growing faint!', 1.0, '["Extreme", "Urgent"]'),
('Bleeding', 'OnTick', 'Severe', NULL, 'Player', 'The bleeding is catastrophic—you need to stanch this NOW!', 1.0, '["Extreme", "Urgent"]'),
('Bleeding', 'OnTick', 'Severe', NULL, 'Enemy', 'The {Enemy} hemorrhages, near death!', 1.0, '["Dramatic"]');

-- ============================================================
-- POISONED - TICK DESCRIPTORS (OnTick)
-- ============================================================

-- Minor Severity
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Poisoned', 'OnTick', 'Minor', NULL, 'Player', 'Nausea grips you—the poison works through your system.', 1.0, '["Visceral"]'),
('Poisoned', 'OnTick', 'Minor', NULL, 'Player', 'You feel weak and dizzy from the toxin.', 1.0, '["Sensory"]'),
('Poisoned', 'OnTick', 'Minor', NULL, 'Enemy', 'The {Enemy} looks sickened by poison.', 1.0, '["Concise"]');

-- Moderate Severity
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Poisoned', 'OnTick', 'Moderate', NULL, 'Player', 'The poison burns through your veins like liquid fire!', 1.0, '["Dramatic", "Visceral"]'),
('Poisoned', 'OnTick', 'Moderate', NULL, 'Player', 'You retch violently—the toxin is potent!', 1.0, '["Visceral", "Extreme"]'),
('Poisoned', 'OnTick', 'Moderate', NULL, 'Player', 'Your vision blurs as poison clouds your mind!', 1.0, '["Sensory", "Dramatic"]'),
('Poisoned', 'OnTick', 'Moderate', NULL, 'Enemy', 'The {Enemy} convulses from poison!', 1.0, '["Dramatic"]');

-- Severe Severity
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Poisoned', 'OnTick', 'Severe', NULL, 'Player', 'The venom ravages your body—every breath is agony!', 1.0, '["Extreme", "Visceral"]'),
('Poisoned', 'OnTick', 'Severe', NULL, 'Player', 'You convulse as poison shuts down your organs!', 1.0, '["Extreme", "Medical"]'),
('Poisoned', 'OnTick', 'Severe', NULL, 'Player', 'Death seems certain if you don''t find an antidote!', 1.0, '["Extreme", "Urgent"]'),
('Poisoned', 'OnTick', 'Severe', NULL, 'Enemy', 'The {Enemy} writhes in poison''s death throes!', 1.0, '["Dramatic"]');

-- ============================================================
-- BLIGHT CORRUPTION - TICK DESCRIPTORS (OnTick)
-- ============================================================

-- Low (1-2 stacks)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('BlightCorruption', 'OnTick', 'Minor', NULL, 'Player', 'Reality feels slightly wrong—thoughts become slippery.', 1.0, '["Blight", "Psychological"]'),
('BlightCorruption', 'OnTick', 'Minor', NULL, 'Player', 'You catch glimpses of things that aren''t there... or are they?', 1.0, '["Blight", "Cosmic"]');

-- Moderate (3-4 stacks)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('BlightCorruption', 'OnTick', 'Moderate', NULL, 'Player', 'The world flickers—was that door always there?', 1.0, '["Blight", "Cosmic", "Sensory"]'),
('BlightCorruption', 'OnTick', 'Moderate', NULL, 'Player', 'Your memories contradict themselves. Did this happen? Will it happen?', 1.0, '["Blight", "Psychological", "Verbose"]'),
('BlightCorruption', 'OnTick', 'Moderate', NULL, 'Player', 'Paradox builds in your mind—coherence becomes difficult!', 1.0, '["Blight", "Dramatic"]');

-- High (5+ stacks, near threshold)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('BlightCorruption', 'OnTick', 'Severe', NULL, 'Player', 'Reality fractures around you—cause and effect lose meaning!', 1.0, '["Blight", "Cosmic", "Extreme"]'),
('BlightCorruption', 'OnTick', 'Severe', NULL, 'Player', 'You exist in multiple states simultaneously—which is the real you?', 1.0, '["Blight", "Cosmic", "Verbose"]'),
('BlightCorruption', 'OnTick', 'Severe', NULL, 'Player', 'The All-Rune''s influence is overwhelming—sanity slips away!', 1.0, '["Blight", "Extreme", "Urgent"]');

-- ============================================================
-- STUNNED - TICK DESCRIPTORS (OnTick)
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Stunned', 'OnTick', NULL, NULL, 'Player', 'You''re still dazed, unable to act!', 1.0, '["Concise"]'),
('Stunned', 'OnTick', NULL, NULL, 'Player', 'Your muscles refuse to respond!', 1.0, '["Visceral"]'),
('Stunned', 'OnTick', NULL, NULL, 'Enemy', 'The {Enemy} remains stunned!', 1.0, '["Concise"]');

-- ============================================================
-- SLOWED - TICK DESCRIPTORS (OnTick)
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Slowed', 'OnTick', NULL, NULL, 'Player', 'Your movements are sluggish and slow.', 1.0, '["Concise"]'),
('Slowed', 'OnTick', NULL, NULL, 'Player', 'You struggle to move at normal speed.', 1.0, '["Concise"]'),
('Slowed', 'OnTick', NULL, NULL, 'Enemy', 'The {Enemy} moves sluggishly.', 1.0, '["Concise"]');

-- ============================================================
-- CORRODING - TICK DESCRIPTORS (OnTick)
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Corroding', 'OnTick', 'Minor', NULL, 'Player', 'The acid continues to eat at your flesh.', 1.0, '["Visceral"]'),
('Corroding', 'OnTick', 'Moderate', NULL, 'Player', 'Caustic burns spread across your skin!', 1.0, '["Dramatic", "Visceral"]'),
('Corroding', 'OnTick', 'Severe', NULL, 'Player', 'The acid dissolves flesh and bone!', 1.0, '["Extreme", "Visceral"]'),
('Corroding', 'OnTick', NULL, NULL, 'Enemy', 'Acid continues eating into the {Enemy}!', 1.0, '["Concise"]');

-- ==============================================================================
-- PART 3: STATUS EFFECT END DESCRIPTORS (OnExpire/OnRemove)
-- ==============================================================================

-- ============================================================
-- BURNING - END DESCRIPTORS
-- ============================================================

-- Natural Expiration
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Burning', 'OnExpire', NULL, 'Natural', 'Player', 'The flames finally gutter out, leaving you singed but alive.', 1.0, '["Relief"]'),
('Burning', 'OnExpire', NULL, 'Natural', 'Player', 'You manage to smother the fire—the burns will heal.', 1.0, '["Relief", "Hopeful"]'),
('Burning', 'OnExpire', NULL, 'Natural', 'Enemy', 'The flames on the {Enemy} die out.', 1.0, '["Concise"]');

-- Active Removal (Roll in water, etc.)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Burning', 'OnRemove', NULL, NULL, 'Player', 'You roll frantically—the flames are extinguished!', 1.0, '["Active", "Relief"]'),
('Burning', 'OnRemove', NULL, NULL, 'Player', 'You plunge into the water—blessed relief as the fire dies!', 1.0, '["Active", "Relief", "Dramatic"]');

-- ============================================================
-- BLEEDING - END DESCRIPTORS
-- ============================================================

-- Natural Clotting
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Bleeding', 'OnExpire', NULL, 'Natural', 'Player', 'The wound finally clots—the bleeding stops.', 1.0, '["Relief"]'),
('Bleeding', 'OnExpire', NULL, 'Natural', 'Player', 'Your body''s defenses kick in—the blood flow slows to a trickle.', 1.0, '["Relief", "Medical"]'),
('Bleeding', 'OnExpire', NULL, 'Natural', 'Enemy', 'The {Enemy}''s wounds clot.', 1.0, '["Concise"]');

-- Bandaged
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Bleeding', 'OnRemove', NULL, 'Bandaged', 'Player', 'You bind the wound tightly—the bleeding stops.', 1.0, '["Active", "Relief"]'),
('Bleeding', 'OnRemove', NULL, 'Bandaged', 'Player', 'The bandage holds—crisis averted.', 1.0, '["Active", "Relief", "Concise"]');

-- ============================================================
-- POISONED - END DESCRIPTORS
-- ============================================================

-- Natural Expiration
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Poisoned', 'OnExpire', NULL, 'Natural', 'Player', 'Your body fights off the toxin—the nausea fades.', 1.0, '["Relief"]'),
('Poisoned', 'OnExpire', NULL, 'Natural', 'Player', 'The poison works its way through your system—you survive.', 1.0, '["Relief", "Hopeful"]'),
('Poisoned', 'OnExpire', NULL, 'Natural', 'Enemy', 'The poison''s effects fade from the {Enemy}.', 1.0, '["Concise"]');

-- Antidote
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Poisoned', 'OnRemove', NULL, 'Antidote', 'Player', 'The antidote burns its way down—immediately the poison''s effects diminish!', 1.0, '["Active", "Relief", "Dramatic"]'),
('Poisoned', 'OnRemove', NULL, 'Antidote', 'Player', 'You feel the cure taking effect—strength returns!', 1.0, '["Active", "Relief", "Hopeful"]');

-- ============================================================
-- BLIGHT CORRUPTION - END DESCRIPTORS
-- ============================================================

-- Below Threshold (Stabilized)
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('BlightCorruption', 'OnExpire', NULL, NULL, 'Player', 'The paradox subsides—reality reasserts itself.', 1.0, '["Blight", "Relief"]'),
('BlightCorruption', 'OnExpire', NULL, NULL, 'Player', 'Your mind clears—coherence returns.', 1.0, '["Blight", "Relief", "Psychological"]'),
('BlightCorruption', 'OnExpire', NULL, NULL, 'Player', 'The All-Rune''s influence fades—you remain yourself.', 1.0, '["Blight", "Relief", "Hopeful"]');

-- At Threshold (Paradox Cascade) - Catastrophic
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('BlightCorruption', 'OnExpire', 'Catastrophic', NULL, 'Player', 'TOO MUCH. Reality fractures. You see all possible yous. Which one survives?', 1.0, '["Blight", "Catastrophic", "Cosmic"]'),
('BlightCorruption', 'OnExpire', 'Catastrophic', NULL, 'Player', 'The Blight wins. You exist and don''t exist. Were you ever real?', 1.0, '["Blight", "Catastrophic", "Existential"]');

-- ============================================================
-- STUNNED - END DESCRIPTORS
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Stunned', 'OnExpire', NULL, 'Natural', 'Player', 'Your senses clear—you can act again!', 1.0, '["Relief"]'),
('Stunned', 'OnExpire', NULL, 'Natural', 'Player', 'Control returns to your muscles!', 1.0, '["Relief", "Visceral"]'),
('Stunned', 'OnExpire', NULL, 'Natural', 'Enemy', 'The {Enemy} recovers from being stunned!', 1.0, '["Concise"]');

-- ============================================================
-- SLOWED - END DESCRIPTORS
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Slowed', 'OnExpire', NULL, 'Natural', 'Player', 'You shake off the sluggishness—full speed returns!', 1.0, '["Relief"]'),
('Slowed', 'OnExpire', NULL, 'Natural', 'Player', 'The ice melts away—you can move freely again!', 1.0, '["Relief", "Sensory"]'),
('Slowed', 'OnExpire', NULL, 'Natural', 'Enemy', 'The {Enemy} regains its speed!', 1.0, '["Concise"]');

-- ============================================================
-- CORRODING - END DESCRIPTORS
-- ============================================================
INSERT INTO Status_Effect_Descriptors (effect_type, application_context, severity, source_type, target_type, descriptor_text, weight, tags)
VALUES
('Corroding', 'OnExpire', NULL, 'Natural', 'Player', 'The acid neutralizes, leaving painful burns.', 1.0, '["Relief", "Visceral"]'),
('Corroding', 'OnExpire', NULL, 'Natural', 'Player', 'The caustic burning finally stops.', 1.0, '["Relief"]'),
('Corroding', 'OnExpire', NULL, 'Natural', 'Enemy', 'The acid stops eating into the {Enemy}.', 1.0, '["Concise"]');

-- ==============================================================================
-- PART 4: SEVERITY PROFILES
-- ==============================================================================

INSERT INTO Status_Effect_Severity_Profiles (effect_type, severity, damage_per_turn_min, damage_per_turn_max, intensity_description, urgency_level)
VALUES
-- Burning
('Burning', 'Minor', 1, 2, 'Manageable pain, small flames', 'Low'),
('Burning', 'Moderate', 3, 5, 'Intense heat, spreading fire', 'Medium'),
('Burning', 'Severe', 6, 99, 'Excruciating agony, immolation', 'Critical'),

-- Bleeding
('Bleeding', 'Minor', 1, 2, 'Steady seepage, manageable', 'Low'),
('Bleeding', 'Moderate', 3, 5, 'Heavy bleeding, significant loss', 'High'),
('Bleeding', 'Severe', 6, 99, 'Hemorrhaging, life-threatening', 'Critical'),

-- Poisoned
('Poisoned', 'Minor', 1, 2, 'Nausea, weakness', 'Low'),
('Poisoned', 'Moderate', 3, 5, 'Potent toxin, violent reaction', 'High'),
('Poisoned', 'Severe', 6, 99, 'Organ failure, near death', 'Critical'),

-- Corroding
('Corroding', 'Minor', 1, 2, 'Caustic burns, manageable', 'Low'),
('Corroding', 'Moderate', 3, 5, 'Spreading acid, significant damage', 'High'),
('Corroding', 'Severe', 6, 99, 'Dissolving flesh, catastrophic', 'Critical');

-- BlightCorruption (stack-based)
INSERT INTO Status_Effect_Severity_Profiles (effect_type, severity, stack_count_min, stack_count_max, intensity_description, urgency_level)
VALUES
('BlightCorruption', 'Minor', 1, 2, 'Slight reality distortion', 'Low'),
('BlightCorruption', 'Moderate', 3, 4, 'Significant paradox buildup', 'High'),
('BlightCorruption', 'Severe', 5, 99, 'Near cascade, sanity breaking', 'Critical');

-- ==============================================================================
-- PART 5: INTERACTION DESCRIPTORS (Examples)
-- ==============================================================================

INSERT INTO Status_Effect_Interaction_Descriptors (effect_type_1, effect_type_2, interaction_type, result_effect, descriptor_text, weight)
VALUES
-- Burning + Freezing = Neutralize
('Burning', 'Freezing', 'Neutralize', NULL, 'The flames and ice clash—steam hisses as both effects neutralize!', 1.0),
('Freezing', 'Burning', 'Neutralize', NULL, 'Fire meets ice—both status effects cancel in a cloud of steam!', 1.0),

-- Bleeding + Poisoned = Amplify
('Bleeding', 'Poisoned', 'Amplify', NULL, 'Poison mixes with blood—the toxin spreads faster through your open wounds!', 1.0),

-- Stunned + Stunned = Transform (to longer stun)
('Stunned', 'Stunned', 'Amplify', NULL, 'Another stunning blow—you''re completely disoriented!', 1.0);

-- ==============================================================================
-- PART 6: ENVIRONMENTAL CONTEXT (Examples)
-- ==============================================================================

INSERT INTO Status_Effect_Environmental_Context (effect_type, biome_name, application_context, environmental_descriptor, duration_modifier, damage_modifier, trigger_chance, weight)
VALUES
-- Burning in Muspelheim (amplified)
('Burning', 'Muspelheim', 'OnTick', 'The volcanic heat intensifies the flames!', 1.5, 1.25, 0.40, 1.0),
('Burning', 'Muspelheim', 'OnApply', 'In this realm of fire, the flames find eager purchase!', NULL, NULL, 0.30, 1.0),

-- Burning in Niflheim (reduced)
('Burning', 'Niflheim', 'OnTick', 'The eternal cold fights the flames, weakening them.', 0.75, 0.75, 0.40, 1.0),

-- Bleeding in Niflheim (faster clotting)
('Bleeding', 'Niflheim', 'OnTick', 'The freezing cold slows the bleeding to a trickle.', 0.5, NULL, 0.30, 1.0),

-- BlightCorruption in Alfheim (amplified)
('BlightCorruption', 'Alfheim', 'OnTick', 'The Runic Blight''s influence is strongest here—reality warps violently!', 1.5, NULL, 0.50, 1.0),

-- Poisoned in The_Roots (natural resistance)
('Poisoned', 'The_Roots', 'OnTick', 'The ancient forest''s vitality helps purge the toxin.', 0.75, 0.8, 0.25, 1.0);

-- ==============================================================================
-- DATA POPULATION COMPLETE
-- ==============================================================================
-- Total Descriptors Added:
-- - Application (OnApply): 50+
-- - Tick (OnTick): 40+
-- - End (OnExpire/OnRemove): 30+
-- - Severity Profiles: 10
-- - Interaction Descriptors: 4
-- - Environmental Contexts: 6
--
-- Total: 140+ descriptors
-- ==============================================================================
