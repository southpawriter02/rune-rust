-- ============================================================
-- v0.38.6: Enemy Voice Profiles & Combat Descriptors
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Enemy archetype-specific combat personalities
-- ============================================================

-- ============================================================
-- ENEMY ARCHETYPE: SERVITOR (Corrupted Machines)
-- ============================================================

-- Servitor Attack Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Servitor', NULL,
 'The Servitor''s articulated limb swings at you with mechanical precision.',
 '["Servitor", "Attack", "Mechanical"]'),

('EnemyAttack', NULL, 'Servitor', NULL,
 'With a whir of servos, the Servitor strikes.',
 '["Servitor", "Attack", "Servos"]'),

('EnemyAttack', NULL, 'Servitor', NULL,
 'The Servitor''s manipulator arm lashes out, trailing sparks.',
 '["Servitor", "Attack", "Sparks"]'),

('EnemyAttack', NULL, 'Servitor', NULL,
 'Runic glyphs flare on the Servitor''s chassis as it attacks.',
 '["Servitor", "Attack", "Runic"]'),

('EnemyAttack', NULL, 'Servitor', NULL,
 'The Servitor''s corrupted appendage strikes with unnatural speed.',
 '["Servitor", "Attack", "Corrupted"]'),

('EnemyAttack', NULL, 'Servitor', NULL,
 'Warning lights flash as the Servitor''s weapon systems engage.',
 '["Servitor", "Attack", "Warning"]');

-- Servitor Special Attacks
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Servitor', NULL,
 'The Servitor''s corrupted power core crackles—electricity arcs toward you!',
 '["Servitor", "Special", "Electrical"]'),

('EnemyAttack', NULL, 'Servitor', NULL,
 'Warning runes flash moments before the Servitor discharges its capacitors!',
 '["Servitor", "Special", "Discharge"]'),

('EnemyAttack', NULL, 'Servitor', NULL,
 'The Servitor channels energy through its damaged systems, lashing out with a bolt of lightning!',
 '["Servitor", "Special", "Lightning"]');

-- Servitor Damage Reactions
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Servitor', 'SolidHit',
 'The Servitor''s chassis dents under your blow, circuits sparking.',
 '["Servitor", "Reaction", "Dented"]'),

('EnemyDefense', NULL, 'Servitor', 'SolidHit',
 'Warning klaxons blare from the damaged Servitor.',
 '["Servitor", "Reaction", "Klaxon"]'),

('EnemyDefense', NULL, 'Servitor', 'SolidHit',
 'The Servitor continues its assault, heedless of the damage.',
 '["Servitor", "Reaction", "Heedless"]'),

('EnemyDefense', NULL, 'Servitor', 'SolidHit',
 'Runic light stutters and flickers as you strike the Servitor.',
 '["Servitor", "Reaction", "Flicker"]'),

('EnemyDefense', NULL, 'Servitor', 'DevastatingHit',
 'Critical systems fail in the Servitor—oil sprays from ruptured lines!',
 '["Servitor", "Reaction", "Critical"]');

-- Servitor Death Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Servitor', 'CriticalHit',
 'The Servitor collapses, its corrupted runes dimming to darkness.',
 '["Servitor", "Death", "Collapse"]'),

('EnemyDefense', NULL, 'Servitor', 'CriticalHit',
 'With a final sputter of failing systems, the Servitor goes still.',
 '["Servitor", "Death", "Failing"]'),

('EnemyDefense', NULL, 'Servitor', 'CriticalHit',
 'The Servitor''s chassis crumples, oil and coolant leaking onto the floor.',
 '["Servitor", "Death", "Crumple"]'),

('EnemyDefense', NULL, 'Servitor', 'CriticalHit',
 'The runic corruption fades as the Servitor powers down permanently.',
 '["Servitor", "Death", "PowerDown"]');

-- ============================================================
-- ENEMY ARCHETYPE: FORLORN (Undead)
-- ============================================================

-- Forlorn Attack Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Forlorn', NULL,
 'The Forlorn reaches for you with grasping, desperate hands.',
 '["Forlorn", "Attack", "Grasping"]'),

('EnemyAttack', NULL, 'Forlorn', NULL,
 'The hollow-eyed revenant lurches forward, clawing at you.',
 '["Forlorn", "Attack", "Lurching"]'),

('EnemyAttack', NULL, 'Forlorn', NULL,
 'The Forlorn''s movements are jerky and unnatural as it strikes.',
 '["Forlorn", "Attack", "Unnatural"]'),

('EnemyAttack', NULL, 'Forlorn', NULL,
 'You hear a whispered lament as the Forlorn attacks.',
 '["Forlorn", "Attack", "Lament"]'),

('EnemyAttack', NULL, 'Forlorn', NULL,
 'The undead creature shambles toward you, arms outstretched.',
 '["Forlorn", "Attack", "Shambling"]'),

('EnemyAttack', NULL, 'Forlorn', NULL,
 'With surprising speed, the Forlorn lunges at you.',
 '["Forlorn", "Attack", "Lunge"]');

-- Forlorn Special Attacks
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Forlorn', NULL,
 'The Forlorn opens its mouth impossibly wide—a soundless scream tears at your mind!',
 '["Forlorn", "Special", "Scream"]'),

('EnemyAttack', NULL, 'Forlorn', NULL,
 'Anguish 800 years old washes over you as the Forlorn wails!',
 '["Forlorn", "Special", "Wail"]'),

('EnemyAttack', NULL, 'Forlorn', NULL,
 'The Forlorn''s psychic agony threatens to overwhelm your consciousness!',
 '["Forlorn", "Special", "Psychic"]');

-- Forlorn Damage Reactions
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Forlorn', 'SolidHit',
 'The Forlorn barely seems to notice the blow.',
 '["Forlorn", "Reaction", "Unfazed"]'),

('EnemyDefense', NULL, 'Forlorn', 'SolidHit',
 'Your weapon passes through withered flesh with little resistance.',
 '["Forlorn", "Reaction", "Withered"]'),

('EnemyDefense', NULL, 'Forlorn', 'SolidHit',
 'The Forlorn lets out a mournful moan as you strike it.',
 '["Forlorn", "Reaction", "Moan"]'),

('EnemyDefense', NULL, 'Forlorn', 'SolidHit',
 'The revenant staggers but continues its advance.',
 '["Forlorn", "Reaction", "Staggers"]'),

('EnemyDefense', NULL, 'Forlorn', 'DevastatingHit',
 'Ancient bones crack and splinter under your assault.',
 '["Forlorn", "Reaction", "Splintering"]');

-- Forlorn Death Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Forlorn', 'CriticalHit',
 'The Forlorn crumbles to dust, finally released from its torment.',
 '["Forlorn", "Death", "Dust"]'),

('EnemyDefense', NULL, 'Forlorn', 'CriticalHit',
 'As the Forlorn falls, you hear a whisper: ''Thank you...''',
 '["Forlorn", "Death", "Release"]'),

('EnemyDefense', NULL, 'Forlorn', 'CriticalHit',
 'The revenant collapses, its animating force dissipating.',
 '["Forlorn", "Death", "Dissipate"]'),

('EnemyDefense', NULL, 'Forlorn', 'CriticalHit',
 'The hollow light in the Forlorn''s eyes finally gutters out.',
 '["Forlorn", "Death", "Gutters"]');

-- ============================================================
-- ENEMY ARCHETYPE: CORRUPTED DVERGR
-- ============================================================

-- Corrupted Dvergr Attack Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Corrupted_Dvergr', NULL,
 'The Corrupted Dvergr swings its improvised weapon with manic energy.',
 '["Corrupted_Dvergr", "Attack", "Manic"]'),

('EnemyAttack', NULL, 'Corrupted_Dvergr', NULL,
 '''Recalibrating!'' shrieks the mad Dvergr as it attacks.',
 '["Corrupted_Dvergr", "Attack", "Shriek"]'),

('EnemyAttack', NULL, 'Corrupted_Dvergr', NULL,
 'The Corrupted Dvergr strikes with disturbing precision despite its madness.',
 '["Corrupted_Dvergr", "Attack", "Precise"]'),

('EnemyAttack', NULL, 'Corrupted_Dvergr', NULL,
 '''Adjusting for optimal damage!'' The Dvergr''s attack is calculated chaos.',
 '["Corrupted_Dvergr", "Attack", "Calculated"]'),

('EnemyAttack', NULL, 'Corrupted_Dvergr', NULL,
 'The mad engineer attacks with a tool repurposed as a weapon.',
 '["Corrupted_Dvergr", "Attack", "Tool"]');

-- Corrupted Dvergr Special Attacks
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Corrupted_Dvergr', NULL,
 '''Thermal runaway detected!'' The Dvergr hurls a smoking device at you!',
 '["Corrupted_Dvergr", "Special", "Explosive"]'),

('EnemyAttack', NULL, 'Corrupted_Dvergr', NULL,
 'The Corrupted Dvergr cackles as it primes an unstable explosive!',
 '["Corrupted_Dvergr", "Special", "Unstable"]'),

('EnemyAttack', NULL, 'Corrupted_Dvergr', NULL,
 '''Safety protocols... DISENGAGED!'' The Dvergr''s contraption explodes!',
 '["Corrupted_Dvergr", "Special", "Contraption"]');

-- Corrupted Dvergr Damage Reactions
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Corrupted_Dvergr', 'SolidHit',
 '''Structural integrity compromised!'' wails the Dvergr.',
 '["Corrupted_Dvergr", "Reaction", "Compromised"]'),

('EnemyDefense', NULL, 'Corrupted_Dvergr', 'SolidHit',
 'The Corrupted Dvergr laughs manically as you strike it.',
 '["Corrupted_Dvergr", "Reaction", "Laughing"]'),

('EnemyDefense', NULL, 'Corrupted_Dvergr', 'SolidHit',
 '''Pain receptors malfunctioning... optimal!'' giggles the mad engineer.',
 '["Corrupted_Dvergr", "Reaction", "Giggling"]'),

('EnemyDefense', NULL, 'Corrupted_Dvergr', 'SolidHit',
 '''Damage within acceptable parameters!'' the Dvergr cries out.',
 '["Corrupted_Dvergr", "Reaction", "Acceptable"]'),

('EnemyDefense', NULL, 'Corrupted_Dvergr', 'DevastatingHit',
 '''Critical failure imminent!'' The Dvergr''s voice rises in pitch.',
 '["Corrupted_Dvergr", "Reaction", "Critical"]');

-- Corrupted Dvergr Death Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Corrupted_Dvergr', 'CriticalHit',
 'The Corrupted Dvergr collapses, muttering technical specifications.',
 '["Corrupted_Dvergr", "Death", "Muttering"]'),

('EnemyDefense', NULL, 'Corrupted_Dvergr', 'CriticalHit',
 '''System failure... initiating shutdown...'' The Dvergr goes still.',
 '["Corrupted_Dvergr", "Death", "Shutdown"]'),

('EnemyDefense', NULL, 'Corrupted_Dvergr', 'CriticalHit',
 'As it dies, the madness drains from the Dvergr''s eyes.',
 '["Corrupted_Dvergr", "Death", "Clarity"]'),

('EnemyDefense', NULL, 'Corrupted_Dvergr', 'CriticalHit',
 '''Calibration... complete...'' The Dvergr''s final words echo.',
 '["Corrupted_Dvergr", "Death", "Final"]');

-- ============================================================
-- ENEMY ARCHETYPE: BLIGHT-TOUCHED BEAST
-- ============================================================

-- Blight-Touched Beast Attack Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'The beast lunges with unnatural speed, jaws snapping.',
 '["Blight_Touched_Beast", "Attack", "Lunge"]'),

('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'Corrupted flesh ripples as the creature strikes.',
 '["Blight_Touched_Beast", "Attack", "Ripple"]'),

('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'The Blight-twisted animal''s attack is feral and desperate.',
 '["Blight_Touched_Beast", "Attack", "Feral"]'),

('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'Crystalline growths glitter as the beast lashes out.',
 '["Blight_Touched_Beast", "Attack", "Crystalline"]'),

('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'The corrupted creature attacks with animalistic fury.',
 '["Blight_Touched_Beast", "Attack", "Fury"]'),

('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'Runic energy crackles around the beast as it charges.',
 '["Blight_Touched_Beast", "Attack", "Charge"]');

-- Blight-Touched Beast Special Attacks
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'The beast''s bite carries the Blight''s corruption—you feel it seeping into your wound!',
 '["Blight_Touched_Beast", "Special", "Infection"]'),

('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'Runic energy pulses from the creature''s maw as it bites!',
 '["Blight_Touched_Beast", "Special", "Pulse"]'),

('EnemyAttack', NULL, 'Blight_Touched_Beast', NULL,
 'The beast''s claws rake you, leaving trails of paradoxical energy!',
 '["Blight_Touched_Beast", "Special", "Paradox"]');

-- Blight-Touched Beast Damage Reactions
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Blight_Touched_Beast', 'SolidHit',
 'The beast howls in pain and rage.',
 '["Blight_Touched_Beast", "Reaction", "Howl"]'),

('EnemyDefense', NULL, 'Blight_Touched_Beast', 'SolidHit',
 'Blight-energy crackles around the wounded creature.',
 '["Blight_Touched_Beast", "Reaction", "Crackle"]'),

('EnemyDefense', NULL, 'Blight_Touched_Beast', 'SolidHit',
 'The animal''s suffering is palpable even through its corruption.',
 '["Blight_Touched_Beast", "Reaction", "Suffering"]'),

('EnemyDefense', NULL, 'Blight_Touched_Beast', 'SolidHit',
 'The beast recoils, Blight-crystals shattering from its hide.',
 '["Blight_Touched_Beast", "Reaction", "Recoil"]'),

('EnemyDefense', NULL, 'Blight_Touched_Beast', 'DevastatingHit',
 'The creature stumbles, corruption flickering as life ebbs.',
 '["Blight_Touched_Beast", "Reaction", "Ebbing"]');

-- Blight-Touched Beast Death Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Blight_Touched_Beast', 'CriticalHit',
 'The beast collapses, the Blight''s influence fading from its eyes.',
 '["Blight_Touched_Beast", "Death", "Fading"]'),

('EnemyDefense', NULL, 'Blight_Touched_Beast', 'CriticalHit',
 'As it dies, the creature seems almost peaceful.',
 '["Blight_Touched_Beast", "Death", "Peaceful"]'),

('EnemyDefense', NULL, 'Blight_Touched_Beast', 'CriticalHit',
 'The Blight-touched animal finally finds rest.',
 '["Blight_Touched_Beast", "Death", "Rest"]'),

('EnemyDefense', NULL, 'Blight_Touched_Beast', 'CriticalHit',
 'The runic corruption dissipates as the beast breathes its last.',
 '["Blight_Touched_Beast", "Death", "Dissipate"]');

-- ============================================================
-- ENEMY ARCHETYPE: AETHER-WRAITH (Alfheim)
-- ============================================================

-- Aether-Wraith Attack Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Aether_Wraith', NULL,
 'The Wraith''s form flickers as it phases through reality to strike you.',
 '["Aether_Wraith", "Attack", "Phase"]'),

('EnemyAttack', NULL, 'Aether_Wraith', NULL,
 'Impossibly, the Wraith attacks from multiple angles simultaneously.',
 '["Aether_Wraith", "Attack", "Impossible"]'),

('EnemyAttack', NULL, 'Aether_Wraith', NULL,
 'The Wraith''s touch burns with the cold fire of paradox.',
 '["Aether_Wraith", "Attack", "Paradox"]'),

('EnemyAttack', NULL, 'Aether_Wraith', NULL,
 'Reality fractures as the Wraith lashes out.',
 '["Aether_Wraith", "Attack", "Fracture"]'),

('EnemyAttack', NULL, 'Aether_Wraith', NULL,
 'The Wraith strikes from a direction that shouldn''t exist.',
 '["Aether_Wraith", "Attack", "Impossible"]');

-- Aether-Wraith Special Attacks
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyAttack', NULL, 'Aether_Wraith', NULL,
 'The Wraith tears at the fabric of reality itself—the wound bleeds into you!',
 '["Aether_Wraith", "Special", "RealityTear"]'),

('EnemyAttack', NULL, 'Aether_Wraith', NULL,
 'Space inverts as the Wraith attacks, your mind struggling to process what''s happening!',
 '["Aether_Wraith", "Special", "Inversion"]'),

('EnemyAttack', NULL, 'Aether_Wraith', NULL,
 'The Wraith''s assault defies comprehension—is this even an attack or are you remembering being hurt?',
 '["Aether_Wraith", "Special", "Temporal"]');

-- Aether-Wraith Damage Reactions
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Aether_Wraith', 'SolidHit',
 'Your weapon passes through where the Wraith was—will be—is.',
 '["Aether_Wraith", "Reaction", "Temporal"]'),

('EnemyDefense', NULL, 'Aether_Wraith', 'SolidHit',
 'The Wraith shimmers, existing in multiple states of injury.',
 '["Aether_Wraith", "Reaction", "Superposition"]'),

('EnemyDefense', NULL, 'Aether_Wraith', 'SolidHit',
 'Did you hit it, or did it allow itself to be struck?',
 '["Aether_Wraith", "Reaction", "Uncertain"]'),

('EnemyDefense', NULL, 'Aether_Wraith', 'SolidHit',
 'Reality warps around the impact, the Wraith flickering.',
 '["Aether_Wraith", "Reaction", "Warp"]'),

('EnemyDefense', NULL, 'Aether_Wraith', 'DevastatingHit',
 'The Wraith''s form destabilizes, paradox energy leaking.',
 '["Aether_Wraith", "Reaction", "Destabilize"]');

-- Aether-Wraith Death Descriptors
INSERT INTO Combat_Action_Descriptors (category, weapon_type, enemy_archetype, outcome_type, descriptor_text, tags)
VALUES
('EnemyDefense', NULL, 'Aether_Wraith', 'CriticalHit',
 'The Wraith unravels, reality reasserting itself where it stood.',
 '["Aether_Wraith", "Death", "Unravel"]'),

('EnemyDefense', NULL, 'Aether_Wraith', 'CriticalHit',
 'As the Wraith dies, you briefly see what it once was—a person.',
 '["Aether_Wraith", "Death", "Revelation"]'),

('EnemyDefense', NULL, 'Aether_Wraith', 'CriticalHit',
 'The paradox collapses, leaving only a fading echo.',
 '["Aether_Wraith", "Death", "Echo"]'),

('EnemyDefense', NULL, 'Aether_Wraith', 'CriticalHit',
 'The Wraith''s form dissolves into impossibility.',
 '["Aether_Wraith", "Death", "Dissolve"]');

-- ============================================================
-- ENEMY VOICE PROFILES
-- ============================================================

-- Note: These will be populated after we get the descriptor_ids from the inserts above.
-- In a real migration, you would need to query for the descriptor_ids and then insert them here.
-- For now, we'll create the profiles with placeholder IDs that will be updated in the service layer.

INSERT INTO Enemy_Voice_Profiles (
    enemy_archetype,
    voice_description,
    setting_context,
    attack_descriptors,
    reaction_damage,
    reaction_death,
    special_attacks
) VALUES (
    'Servitor',
    'Mechanical, emotionless, relentless',
    'Jötun-built maintenance drones corrupted by the Blight',
    '[]',  -- Will be populated by service layer
    '[]',
    '[]',
    '[]'
);

INSERT INTO Enemy_Voice_Profiles (
    enemy_archetype,
    voice_description,
    setting_context,
    attack_descriptors,
    reaction_damage,
    reaction_death,
    special_attacks
) VALUES (
    'Forlorn',
    'Mournful, hollow, echoing with lost humanity',
    'Victims of the Blight, trapped between life and death',
    '[]',
    '[]',
    '[]',
    '[]'
);

INSERT INTO Enemy_Voice_Profiles (
    enemy_archetype,
    voice_description,
    setting_context,
    attack_descriptors,
    reaction_damage,
    reaction_death,
    special_attacks
) VALUES (
    'Corrupted_Dvergr',
    'Fractured speech, technical jargon mixed with madness',
    'Former engineers driven insane by the Blight',
    '[]',
    '[]',
    '[]',
    '[]'
);

INSERT INTO Enemy_Voice_Profiles (
    enemy_archetype,
    voice_description,
    setting_context,
    attack_descriptors,
    reaction_damage,
    reaction_death,
    special_attacks
) VALUES (
    'Blight_Touched_Beast',
    'Animalistic, corrupted, suffering',
    'Natural creatures twisted by Runic Blight',
    '[]',
    '[]',
    '[]',
    '[]'
);

INSERT INTO Enemy_Voice_Profiles (
    enemy_archetype,
    voice_description,
    setting_context,
    attack_descriptors,
    reaction_damage,
    reaction_death,
    special_attacks
) VALUES (
    'Aether_Wraith',
    'Reality-bending, incomprehensible, horrifying',
    'Manifestations of pure paradox from Alfheim',
    '[]',
    '[]',
    '[]',
    '[]'
);

-- ============================================================
-- Notes:
-- - Total enemy descriptors: 80+ across 5 archetypes
-- - Each archetype has distinct voice and personality
-- - Voice profile descriptor arrays will be auto-populated by service
-- ============================================================
