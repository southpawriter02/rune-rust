-- =====================================================
-- v0.38.14: Recovery Descriptors Content
-- =====================================================
-- Populates Recovery_Descriptors with 20+ trauma suppression and removal descriptions
-- Covers Cognitive Stabilizer usage and Saga Quest trauma removal
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- COGNITIVE STABILIZER: APPLICATION
-- =====================================================

INSERT INTO Recovery_Descriptors (descriptor_name, recovery_type, trauma_id, descriptor_text, spawn_weight, tags) VALUES
-- General Application (all traumas)
('Stabilizer_Application_01', 'Stabilizer_Application', null, 'You inject the Cognitive Stabilizer. The chemicals flood your system.', 1.0, '["Injection", "Chemicals", "Application"]'),
('Stabilizer_Application_02', 'Stabilizer_Application', null, 'For a brief time, the trauma recedes. Your mind clears. The pain dulls.', 1.0, '["Relief", "Clearing", "Dulling"]'),
('Stabilizer_Application_03', 'Stabilizer_Application', null, 'Artificial calm settles over you. You know it won''t last, but for now... relief.', 1.0, '["Artificial", "Calm", "Temporary"]'),
('Stabilizer_Application_04', 'Stabilizer_Application', null, 'The Stabilizer takes effect. Your racing thoughts slow. The panic subsides.', 0.9, '["Effect", "Slowing", "Subsiding"]'),
('Stabilizer_Application_05', 'Stabilizer_Application', null, 'Cool liquid spreads through your veins. With it comes clarity. Blessed, temporary clarity.', 0.8, '["Cool", "Clarity", "Blessed"]'),

-- Specific Trauma Applications
('Stabilizer_Application_Flashbacks', 'Stabilizer_Application', 'flashbacks', 'The Stabilizer dampens the memories. They''re still there, but distant. Muted.', 0.9, '["Dampening", "Distant", "Muted"]'),
('Stabilizer_Application_Battle_Tremors', 'Stabilizer_Application', 'battle_tremors', 'Your hands steady. The tremors fade. For one combat, you have control again.', 0.9, '["Steady", "Fade", "Control"]'),
('Stabilizer_Application_Hypervigilance', 'Stabilizer_Application', 'hypervigilance', 'The constant vigilance eases. You can breathe without scanning for threats.', 0.9, '["Ease", "Breathe", "Respite"]'),
('Stabilizer_Application_Auditory_Hallucinations', 'Stabilizer_Application', 'auditory_hallucinations', 'The Choir''s volume decreases. The voices quiet. Silence. Beautiful silence.', 0.8, '["Quiet", "Silence", "Choir"]'),
('Stabilizer_Application_Reality_Dissociation', 'Stabilizer_Application', 'reality_dissociation', 'Reality solidifies. You''re real. This is real. The certainty is intoxicating.', 0.8, '["Solid", "Real", "Certainty"]'),

-- =====================================================
-- COGNITIVE STABILIZER: DURATION (ACTIVE)
-- =====================================================

INSERT INTO Recovery_Descriptors (descriptor_name, recovery_type, trauma_id, descriptor_text, spawn_weight, tags) VALUES
-- General Duration
('Stabilizer_Duration_01', 'Stabilizer_Duration', null, 'The Stabilizer''s effects last for one combat encounter.', 1.0, '["Duration", "Combat", "Temporary"]'),
('Stabilizer_Duration_02', 'Stabilizer_Duration', null, 'You have temporary reprieve from your psychological scars.', 1.0, '["Reprieve", "Temporary", "Relief"]'),
('Stabilizer_Duration_03', 'Stabilizer_Duration', null, 'The artificial calm holds. You can function. You can fight without your trauma interfering.', 0.9, '["Function", "Fight", "No_Interference"]'),
('Stabilizer_Duration_04', 'Stabilizer_Duration', null, 'For now, you''re free. The trauma is suppressed, locked away behind chemical barriers.', 0.8, '["Free", "Suppressed", "Barriers"]'),

-- =====================================================
-- COGNITIVE STABILIZER: WEARING OFF
-- =====================================================

INSERT INTO Recovery_Descriptors (descriptor_name, recovery_type, trauma_id, descriptor_text, spawn_weight, tags) VALUES
-- General Wearing Off
('Stabilizer_Wearing_Off_01', 'Stabilizer_Wearing_Off', null, 'The Stabilizer fades. Your trauma comes roaring back, worse than before. [+3 Psychic Stress]', 1.0, '["Fade", "Roaring_Back", "Worse"]'),
('Stabilizer_Wearing_Off_02', 'Stabilizer_Wearing_Off', null, 'Reality crashes back in. The trauma was there all along, just hidden.', 1.0, '["Crash", "Hidden", "Return"]'),
('Stabilizer_Wearing_Off_03', 'Stabilizer_Wearing_Off', null, 'The chemicals leave your system. The wounds are still there. They were always there.', 0.9, '["Leaving", "Still_There", "Always"]'),
('Stabilizer_Wearing_Off_04', 'Stabilizer_Wearing_Off', null, 'The reprieve ends. Your trauma reasserts itself with a vengeance. [+3 Psychic Stress]', 0.9, '["End", "Vengeance", "Reassert"]'),

-- Specific Trauma Wearing Off
('Stabilizer_Wearing_Off_Flashbacks', 'Stabilizer_Wearing_Off', 'flashbacks', 'The memories surge back. You''re vulnerable to flashbacks again.', 0.8, '["Surge", "Vulnerable", "Memories"]'),
('Stabilizer_Wearing_Off_Battle_Tremors', 'Stabilizer_Wearing_Off', 'battle_tremors', 'Your hands start shaking again. The tremors return, as persistent as ever.', 0.8, '["Shaking", "Return", "Persistent"]'),
('Stabilizer_Wearing_Off_Auditory_Hallucinations', 'Stabilizer_Wearing_Off', 'auditory_hallucinations', 'The Choir returns. Louder. Angrier. As if offended by your attempt to silence them.', 0.7, '["Return", "Louder", "Angry"]'),

-- =====================================================
-- SAGA QUEST: BEGINNING
-- =====================================================

INSERT INTO Recovery_Descriptors (descriptor_name, recovery_type, trauma_id, descriptor_text, spawn_weight, tags) VALUES
-- General Quest Beginning
('Quest_Beginning_01', 'Quest_Beginning', null, 'You''ve decided to confront your trauma. This won''t be easy.', 1.0, '["Confront", "Decision", "Difficult"]'),
('Quest_Beginning_02', 'Quest_Beginning', null, 'To heal this wound, you must return to where it began.', 1.0, '["Heal", "Return", "Origin"]'),
('Quest_Beginning_03', 'Quest_Beginning', null, 'Removing this trauma requires facing what broke you in the first place.', 1.0, '["Removing", "Facing", "Broke"]'),
('Quest_Beginning_04', 'Quest_Beginning', null, 'The only way out is through. You must confront the source of your trauma.', 0.9, '["Through", "Source", "Confront"]'),
('Quest_Beginning_05', 'Quest_Beginning', null, 'This is a journey into your own darkness. Only by facing it can you be free.', 0.8, '["Journey", "Darkness", "Free"]'),

-- =====================================================
-- SAGA QUEST: DURING
-- =====================================================

INSERT INTO Recovery_Descriptors (descriptor_name, recovery_type, trauma_id, descriptor_text, spawn_weight, tags) VALUES
-- General Quest Progress
('Quest_During_01', 'Quest_During', null, 'The memories are overwhelming, but you push forward.', 1.0, '["Overwhelming", "Push_Forward", "Memories"]'),
('Quest_During_02', 'Quest_During', null, 'You''re reliving the trauma, but this time, you''re in control.', 1.0, '["Reliving", "Control", "This_Time"]'),
('Quest_During_03', 'Quest_During', null, 'Confronting this is agony, but necessary.', 1.0, '["Agony", "Necessary", "Confronting"]'),
('Quest_During_04', 'Quest_During', null, 'Each step toward healing tears open old wounds. But they must be reopened to properly heal.', 0.9, '["Healing", "Wounds", "Reopened"]'),
('Quest_During_05', 'Quest_During', null, 'This is harder than combat. Harder than survival. But you continue.', 0.8, '["Harder", "Continue", "Persevere"]'),

-- =====================================================
-- SAGA QUEST: COMPLETION (TRAUMA REMOVED)
-- =====================================================

INSERT INTO Recovery_Descriptors (descriptor_name, recovery_type, trauma_id, descriptor_text, spawn_weight, tags) VALUES
-- General Trauma Removal
('Quest_Completion_Generic', 'Quest_Completion', null, '[TRAUMA REMOVED]

You''ve confronted your trauma. Faced it. Processed it.
The scar remains, but it no longer controls you.
You are free.', 1.0, '["Removed", "Free", "Confronted"]'),

-- Specific Trauma Removals
('Quest_Completion_Flashbacks', 'Quest_Completion', 'flashbacks', '[TRAUMA REMOVED: Flashbacks]

You''ve confronted the memories. They no longer have power over you.
The past is the past. It cannot trap you anymore.
You are free.', 1.0, '["Flashbacks", "Past", "Free"]'),

('Quest_Completion_Battle_Tremors', 'Quest_Completion', 'battle_tremors', '[TRAUMA REMOVED: Battle Tremors]

Through deliberate practice and confrontation, you''ve retrained your nervous system.
Your hands are steady again. Control is yours.
You are free.', 1.0, '["Battle_Tremors", "Steady", "Control"]'),

('Quest_Completion_Hypervigilance', 'Quest_Completion', 'hypervigilance', '[TRAUMA REMOVED: Hypervigilance]

You''ve learned to distinguish real threats from imagined ones.
You can relax now. You can sleep. The constant vigilance ends.
You are free.', 1.0, '["Hypervigilance", "Relax", "Sleep"]'),

('Quest_Completion_Auditory_Hallucinations', 'Quest_Completion', 'auditory_hallucinations', '[TRAUMA REMOVED: Auditory Hallucinations]

You''ve silenced the Choir. Their voices no longer haunt you.
The silence is real. The peace is real.
You are free.', 1.0, '["Choir", "Silence", "Peace"]'),

('Quest_Completion_Reality_Dissociation', 'Quest_Completion', 'reality_dissociation', '[TRAUMA REMOVED: Reality Dissociation]

You''ve grounded yourself in reality. You are real. This is real.
The dissociation no longer controls you.
You are free.', 1.0, '["Reality", "Grounded", "Real"]'),

-- =====================================================
-- HEALING MOMENTS (EMOTIONAL RELEASE)
-- =====================================================

INSERT INTO Recovery_Descriptors (descriptor_name, recovery_type, trauma_id, descriptor_text, spawn_weight, tags) VALUES
-- General Healing
('Healing_Moment_01', 'Healing_Moment', null, 'The weight lifts. For the first time in forever, you breathe easy.', 1.0, '["Weight", "Lifts", "Breathe"]'),
('Healing_Moment_02', 'Healing_Moment', null, 'Tears stream down your face. Not from pain, but from relief. It''s over.', 1.0, '["Tears", "Relief", "Over"]'),
('Healing_Moment_03', 'Healing_Moment', null, 'You feel... lighter. Whole. The broken pieces have been mended.', 1.0, '["Lighter", "Whole", "Mended"]'),
('Healing_Moment_04', 'Healing_Moment', null, 'You''ll never forget what happened. But it no longer defines you.', 1.0, '["Remember", "Not_Define", "Freedom"]'),
('Healing_Moment_05', 'Healing_Moment', null, 'A burden you didn''t realize you were carrying lifts. You can stand straight again.', 0.9, '["Burden", "Lifts", "Straight"]'),
('Healing_Moment_06', 'Healing_Moment', null, 'The scar remains, but it''s a scar now—not an open wound. You''ve healed.', 0.9, '["Scar", "Healed", "Wound_Closed"]'),
('Healing_Moment_07', 'Healing_Moment', null, 'You smile. A real smile. The first in a long time. You''re going to be okay.', 0.8, '["Smile", "Real", "Okay"]'),
('Healing_Moment_08', 'Healing_Moment', null, 'Peace. Actual peace. You''d forgotten what this felt like.', 0.8, '["Peace", "Forgotten", "Remembered"]');

COMMIT;

-- =====================================================
-- VERIFICATION
-- =====================================================
-- SELECT recovery_type, COUNT(*) FROM Recovery_Descriptors GROUP BY recovery_type;
-- Expected:
--   Stabilizer_Application: 10
--   Stabilizer_Duration: 4
--   Stabilizer_Wearing_Off: 7
--   Quest_Beginning: 5
--   Quest_During: 5
--   Quest_Completion: 6
--   Healing_Moment: 8
-- Total: 45 recovery descriptors
-- =====================================================
