-- =====================================================
-- v0.38.14: Breaking Point Descriptors Content
-- =====================================================
-- Populates Breaking_Point_Descriptors with 25+ stress threshold descriptions
-- Covers warning signs, breaking moments, resolve checks, and trauma reveals
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- WARNING SIGNS (75-99% Stress)
-- =====================================================

INSERT INTO Breaking_Point_Descriptors (descriptor_name, stress_threshold_min, stress_threshold_max, phase, descriptor_text, spawn_weight, tags) VALUES
-- Visual/Perceptual Warnings
('Warning_Vision_Narrows', 75, 99, 'Warning', 'Your vision narrows. Reality feels thin, like paper.', 1.0, '["Visual", "Reality", "Thinning"]'),
('Warning_Thoughts_Fragment', 75, 99, 'Warning', 'Your thoughts are fragmenting. You can''t hold them together.', 1.0, '["Mental", "Fragmentation", "Thoughts"]'),
('Warning_Choir_Louder', 75, 99, 'Warning', 'The Choir is getting louder. Or is that just in your head?', 0.8, '["Auditory", "Choir", "Uncertain"]'),
('Warning_On_Edge', 75, 99, 'Warning', 'You''re on the edge. One more push and you''ll break.', 1.0, '["Edge", "Breaking", "Threshold"]'),
('Warning_Hands_Shake', 75, 99, 'Warning', 'Your hands shake. Your breathing is rapid. You''re losing control.', 1.0, '["Physical", "Shaking", "Control"]'),

-- Psychological Warnings
('Warning_Pressure_Building', 75, 99, 'Warning', 'The pressure inside your skull is unbearable. Something has to give.', 0.9, '["Pressure", "Skull", "Unbearable"]'),
('Warning_Reality_Flickers', 75, 99, 'Warning', 'Reality flickers at the edges. You''re not sure what''s real anymore.', 0.8, '["Reality", "Flickering", "Uncertain"]'),
('Warning_Drowning', 75, 99, 'Warning', 'You''re drowning. Not in water—in stress, in horror, in the weight of it all.', 0.9, '["Drowning", "Weight", "Overwhelmed"]'),
('Warning_Screaming_Inside', 75, 99, 'Warning', 'There''s a scream building inside you. You don''t know if you can hold it in.', 0.8, '["Scream", "Building", "Holding"]'),
('Warning_Glass_Cracking', 75, 99, 'Warning', 'Your mind feels like glass under pressure. Cracks spider across your consciousness.', 0.9, '["Glass", "Cracks", "Pressure"]'),

-- Existential Warnings
('Warning_Too_Much', 75, 99, 'Warning', 'It''s too much. All of it. The Blight, the death, the endless ruins. Too much.', 0.8, '["Overwhelmed", "Too_Much", "Endless"]'),
('Warning_Cant_Breathe', 75, 99, 'Warning', 'You can''t breathe properly. The air feels wrong. Everything feels wrong.', 0.9, '["Breathing", "Wrong", "Suffocating"]'),
('Warning_Unraveling', 75, 99, 'Warning', 'You''re unraveling. Thread by thread, your sanity comes undone.', 0.8, '["Unraveling", "Thread", "Sanity"]'),

-- =====================================================
-- BREAKING POINT MOMENTS (100% Stress)
-- =====================================================

INSERT INTO Breaking_Point_Descriptors (descriptor_name, stress_threshold_min, stress_threshold_max, phase, descriptor_text, spawn_weight, tags) VALUES
-- Fracture Moments
('Breaking_Snap', 100, 100, 'Breaking', '[BREAKING POINT REACHED] Something inside you snaps.', 1.0, '["Snap", "Break", "Moment"]'),
('Breaking_Cant_Take_More', 100, 100, 'Breaking', 'Your mind can''t take anymore. The psychic pressure exceeds your capacity. You BREAK.', 1.0, '["Capacity", "Exceeded", "Break"]'),
('Breaking_World_Inverts', 100, 100, 'Breaking', 'The world inverts. Up is down. Real is unreal. You''re screaming but you can''t hear it.', 0.9, '["Inversion", "Scream", "Unreal"]'),
('Breaking_Too_Much_Horror', 100, 100, 'Breaking', 'Everything is too much. The Choir. The Blight. The horror. You shatter like glass.', 1.0, '["Shatter", "Glass", "Horror"]'),

-- Specific Breaking Moments
('Breaking_Mental_Fracture', 100, 100, 'Breaking', 'Your mind fractures. Like stone under too much weight, it cracks and splits.', 0.9, '["Fracture", "Stone", "Cracks"]'),
('Breaking_Overload', 100, 100, 'Breaking', 'Psychic overload. Your brain can''t process this much trauma. Systems fail.', 0.8, '["Overload", "Systems", "Fail"]'),
('Breaking_Choir_Overwhelms', 100, 100, 'Breaking', 'The Cursed Choir surges through your mind, drowning out everything else. You break.', 0.7, '["Choir", "Surge", "Drown"]'),
('Breaking_Reality_Collapses', 100, 100, 'Breaking', 'Your grasp on reality collapses. What''s real? What''s you? The boundaries dissolve.', 0.8, '["Reality", "Collapse", "Boundaries"]'),
('Breaking_Darkness_Swallows', 100, 100, 'Breaking', 'Darkness swallows your consciousness. Not physical darkness—mental void.', 0.7, '["Darkness", "Void", "Swallow"]'),

-- =====================================================
-- SYSTEM MESSAGES
-- =====================================================

INSERT INTO Breaking_Point_Descriptors (descriptor_name, stress_threshold_min, stress_threshold_max, phase, descriptor_text, spawn_weight, tags) VALUES
('System_Breaking_Point_Message', 100, 100, 'SystemMessage', '[SYSTEM: BREAKING POINT REACHED]
Your mind fractures under the weight of the Cursed Choir.
Make a WILL-based Resolve Check (DC 16) to hold yourself together.

Success: Retain sanity, but remain shaken. (Stress resets to 75%, Disoriented 2 turns)
Failure: Gain permanent Trauma. (Stress resets to 50%, Stunned 1 turn)', 1.0, '["System", "Resolve_Check", "Mechanical"]');

-- =====================================================
-- RESOLVE CHECK SUCCESS (Barely Holding)
-- =====================================================

INSERT INTO Breaking_Point_Descriptors (descriptor_name, stress_threshold_min, stress_threshold_max, phase, descriptor_text, spawn_weight, tags) VALUES
('Resolve_Success_Claw_Back', 100, 100, 'ResolveSuccess', 'You claw your way back from the brink. You''re still here. Still sane. Barely.', 1.0, '["Claw", "Brink", "Barely"]'),
('Resolve_Success_Force_Closed', 100, 100, 'ResolveSuccess', 'With monumental effort, you force the fractures closed. You hold. For now.', 1.0, '["Force", "Hold", "Effort"]'),
('Resolve_Success_Pull_Together', 100, 100, 'ResolveSuccess', 'You pull yourself together through sheer willpower. The cracks remain, but you''re intact.', 1.0, '["Willpower", "Intact", "Cracks"]'),
('Resolve_Success_Anchor', 100, 100, 'ResolveSuccess', 'You find an anchor—a memory, a purpose, something real. You cling to it. You survive.', 0.9, '["Anchor", "Memory", "Survive"]'),
('Resolve_Success_Refuse_Break', 100, 100, 'ResolveSuccess', 'No. You refuse. You will not break. Not here. Not now. You hold the pieces together.', 0.9, '["Refuse", "Will_Not", "Hold"]'),
('Resolve_Success_Breathing', 100, 100, 'ResolveSuccess', 'Breathe. Just breathe. Focus on breathing. In. Out. You''re still here. You made it.', 0.8, '["Breathe", "Focus", "Made_It"]'),

-- =====================================================
-- RESOLVE CHECK FAILURE (Trauma Acquisition)
-- =====================================================

INSERT INTO Breaking_Point_Descriptors (descriptor_name, stress_threshold_min, stress_threshold_max, phase, descriptor_text, spawn_weight, tags) VALUES
('Resolve_Failure_Break', 100, 100, 'ResolveFailure', 'You break. Something permanent damages inside you. A scar that will never fully heal.', 1.0, '["Break", "Permanent", "Scar"]'),
('Resolve_Failure_Fracture_Reform', 100, 100, 'ResolveFailure', 'Your mind fractures. When the pieces reform, something is missing. Something is wrong.', 1.0, '["Fracture", "Missing", "Wrong"]'),
('Resolve_Failure_Lose_Part', 100, 100, 'ResolveFailure', 'You lose a part of yourself in that moment. It''s gone forever.', 0.9, '["Lost", "Forever", "Part"]'),
('Resolve_Failure_Shatter', 100, 100, 'ResolveFailure', 'You shatter. The pieces come back together, but not quite right. Never quite right again.', 0.9, '["Shatter", "Wrong", "Never_Right"]'),
('Resolve_Failure_Darkness_Claims', 100, 100, 'ResolveFailure', 'The darkness claims a piece of you. You''ll carry this wound for the rest of your days.', 0.8, '["Claimed", "Wound", "Forever"]'),

-- =====================================================
-- TRAUMA REVEAL MESSAGES (Specific Traumas)
-- =====================================================

INSERT INTO Breaking_Point_Descriptors (descriptor_name, stress_threshold_min, stress_threshold_max, phase, descriptor_text, spawn_weight, tags) VALUES
-- Combat Traumas
('Trauma_Reveal_Flashbacks', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Flashbacks]

Combat will trigger vivid memories of your near-death experiences.
You''ll randomly freeze mid-fight, trapped in past trauma.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Flashbacks", "Combat", "Permanent"]'),

('Trauma_Reveal_Battle_Tremors', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Battle Tremors]

Your nervous system is damaged. Your hands shake uncontrollably.
All attack rolls suffer penalties. Fine motor control is compromised.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Battle_Tremors", "Attack", "Permanent"]'),

('Trauma_Reveal_Hypervigilance', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Hypervigilance]

You can never relax again. Every shadow hides threats.
Rest is ineffective. Exhaustion accumulates faster.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Hypervigilance", "Rest", "Permanent"]'),

-- Blight Traumas
('Trauma_Reveal_Paradoxical_Paranoia', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Paradoxical Paranoia]

The Blight has corrupted your perceptions.
You see patterns that shouldn''t exist. Reality feels uncertain.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Paradoxical_Paranoia", "Blight", "Permanent"]'),

('Trauma_Reveal_Auditory_Hallucinations', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Auditory Hallucinations]

The Cursed Choir follows you now. Always singing.
Concentration is compromised. Threat locations may be confused.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Auditory_Hallucinations", "Choir", "Permanent"]'),

('Trauma_Reveal_Reality_Dissociation', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Reality Dissociation]

You can no longer trust that this is real. That you are real.
Random AP loss as you struggle with existence itself.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Reality_Dissociation", "Reality", "Permanent"]'),

-- Social Traumas
('Trauma_Reveal_Corrupted_Social_Script', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Corrupted Social Script]

Your ability to interact normally is broken.
Severe penalties to all Rhetoric checks. NPCs react poorly.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Corrupted_Social_Script", "Social", "Permanent"]'),

('Trauma_Reveal_Trust_Erosion', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Trust Erosion]

You can''t trust anyone anymore. Not even yourself.
Cooperation is compromised. You refuse help automatically.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Trust_Erosion", "Trust", "Permanent"]'),

-- Existential Traumas
('Trauma_Reveal_Systemic_Apathy', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Systemic Apathy]

Nothing matters anymore. The world is dead. So is your motivation.
Initiative penalties. Reduced engagement with quest objectives.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Systemic_Apathy", "Apathy", "Permanent"]'),

('Trauma_Reveal_Existential_Dread', 100, 100, 'TraumaReveal', '[TRAUMA ACQUIRED: Existential Dread]

You''ve looked into the abyss. It looked back.
Periodic stress accumulation. Vulnerability to despair effects.

This scar is permanent. Only a Saga Quest can remove it.', 1.0, '["Existential_Dread", "Dread", "Permanent"]');

COMMIT;

-- =====================================================
-- VERIFICATION
-- =====================================================
-- SELECT phase, COUNT(*) FROM Breaking_Point_Descriptors GROUP BY phase;
-- Expected:
--   Warning: 13
--   Breaking: 9
--   SystemMessage: 1
--   ResolveSuccess: 6
--   ResolveFailure: 5
--   TraumaReveal: 10
-- Total: 44 breaking point descriptors
-- =====================================================
