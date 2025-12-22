-- ==============================================================================
-- v0.38.10: Skill Usage Flavor Text - Data Population
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Populate skill check descriptors with comprehensive flavor text
-- Content: 80+ Skill Check Descriptors, 40+ Success/Failure Variations,
--          30+ Fumble Consequences, 20+ Critical Success Descriptors
-- ==============================================================================

-- ==============================================================================
-- SYSTEM BYPASS: Lockpicking Descriptors
-- ==============================================================================

-- Attempt Descriptions (Setup/Context)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'SimpleLock', 'You kneel before the lock, examining the mechanism with your picks.', 1.0, '["Concise", "Technical"]'),
('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'SimpleLock', 'You insert your tools, feeling for the tumblers inside.', 1.0, '["Concise"]'),
('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'SimpleLock', 'You probe the lock carefully, mapping its interior structure.', 1.0, '["Technical"]'),

('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'ComplexLock', 'This is Jötun craftsmanship—precision engineering. You steady your hands.', 1.0, '["Verbose", "Lore"]'),
('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'ComplexLock', 'The lock mechanism is complex. This will require patience and skill.', 1.0, '["Verbose"]'),
('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'ComplexLock', 'You count at least seven tumblers. This won''t be quick.', 1.0, '["Technical"]'),
('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'ComplexLock', 'Centuries-old mechanisms that still function perfectly. Impressive... and difficult.', 1.0, '["Verbose", "Lore"]'),

('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'CorrodedLock', 'Rust has seized parts of the mechanism. You''ll need to work around the corrosion.', 1.0, '["Verbose"]'),
('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'CorrodedLock', 'The lock is damaged, parts of it fused together. This complicates things.', 1.0, '["Verbose"]'),
('SystemBypass', 'Lockpicking', 'Attempt', NULL, 'CorrodedLock', 'Age has not been kind to this lock. Some tumblers are completely corroded.', 1.0, '["Verbose"]');

-- Success Descriptors - Minimal Success (1-2 over DC)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'Lockpicking', 'Success', 'Minimal', NULL, 'After several tense moments, the lock finally yields. Your hands are cramping.', 1.0, '["Verbose", "Struggle"]'),
('SystemBypass', 'Lockpicking', 'Success', 'Minimal', NULL, 'You barely manage to align the tumblers. The lock clicks open, but you''re sweating.', 1.0, '["Verbose", "Struggle"]'),
('SystemBypass', 'Lockpicking', 'Success', 'Minimal', NULL, 'With great effort, you coax the mechanism open. That was harder than expected.', 1.0, '["Verbose"]'),
('SystemBypass', 'Lockpicking', 'Success', 'Minimal', NULL, 'You''re not sure how, but... yes, the lock is open. Barely.', 1.0, '["Concise"]');

-- Success Descriptors - Solid Success (3-5 over DC)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'Lockpicking', 'Success', 'Solid', NULL, 'Your picks find their marks efficiently. The lock opens with a satisfying click.', 1.0, '["Concise"]'),
('SystemBypass', 'Lockpicking', 'Success', 'Solid', NULL, 'You work methodically through the tumblers. Within moments, the lock surrenders.', 1.0, '["Verbose"]'),
('SystemBypass', 'Lockpicking', 'Success', 'Solid', NULL, 'The mechanism yields to your expertise. The door is yours to open.', 1.0, '["Concise"]'),
('SystemBypass', 'Lockpicking', 'Success', 'Solid', NULL, 'One by one, the pins fall into place. The lock opens smoothly.', 1.0, '["Technical"]');

-- Success Descriptors - Critical Success (6+ over DC)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'Lockpicking', 'CriticalSuccess', 'Critical', NULL, 'You open the lock so smoothly, it''s as if you had the key. Masterful work.', 1.0, '["Dramatic"]'),
('SystemBypass', 'Lockpicking', 'CriticalSuccess', 'Critical', NULL, 'Your fingers dance through the mechanism with perfect precision. The lock never stood a chance.', 1.0, '["Dramatic", "Verbose"]'),
('SystemBypass', 'Lockpicking', 'CriticalSuccess', 'Critical', NULL, 'You barely even need to look—your hands know exactly what to do. The lock opens instantly.', 1.0, '["Dramatic"]'),
('SystemBypass', 'Lockpicking', 'CriticalSuccess', 'Critical', NULL, 'The lock opens in mere heartbeats. Child''s play.', 1.0, '["Concise"]'),
('SystemBypass', 'Lockpicking', 'CriticalSuccess', 'Critical', NULL, 'You could do this blindfolded. The mechanism practically opens itself under your expert touch.', 1.0, '["Dramatic", "Verbose"]');

-- Failure Descriptors - Close Failure (1-2 under DC)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'Lockpicking', 'Failure', 'Minimal', NULL, 'Almost... but the last tumbler won''t set. You withdraw your picks, frustrated.', 1.0, '["Verbose"]'),
('SystemBypass', 'Lockpicking', 'Failure', 'Minimal', NULL, 'You think you have it, but the mechanism refuses to turn. Not quite.', 1.0, '["Concise"]'),
('SystemBypass', 'Lockpicking', 'Failure', 'Minimal', NULL, 'So close—but close doesn''t open locks. You''ll need to try again.', 1.0, '["Concise"]'),
('SystemBypass', 'Lockpicking', 'Failure', 'Minimal', NULL, 'One pin away from success, but that one pin might as well be a mountain.', 1.0, '["Dramatic"]');

-- Failure Descriptors - Clear Failure (3-5 under DC)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'Lockpicking', 'Failure', 'Solid', NULL, 'The lock''s complexity defeats you. You withdraw your picks, stymied.', 1.0, '["Concise"]'),
('SystemBypass', 'Lockpicking', 'Failure', 'Solid', NULL, 'You can''t make sense of this mechanism. It''s beyond your current skill.', 1.0, '["Verbose"]'),
('SystemBypass', 'Lockpicking', 'Failure', 'Solid', NULL, 'After a long period of fruitless effort, you accept that this lock is too advanced.', 1.0, '["Verbose"]'),
('SystemBypass', 'Lockpicking', 'Failure', 'Solid', NULL, 'The mechanism resists all your attempts. You''re not getting through this way.', 1.0, '["Concise"]');

-- ==============================================================================
-- SYSTEM BYPASS: Lockpicking Fumbles
-- ==============================================================================

INSERT INTO Skill_Fumble_Descriptors (skill_type, action_type, consequence_type, severity, damage_formula, next_attempt_dc_modifier, descriptor_text, weight, tags) VALUES
('SystemBypass', 'Lockpicking', 'ToolBreakage', 'Moderate', NULL, 2, 'Your pick snaps off inside the lock! The broken piece jams the mechanism. [+2 DC for next attempt]', 1.0, '["Equipment_Loss"]'),
('SystemBypass', 'Lockpicking', 'ToolBreakage', 'Moderate', NULL, 2, 'A fumbled movement—your tool breaks with a sharp crack. The lock is now harder to open. [+2 DC for next attempt]', 1.0, '["Equipment_Loss"]'),
('SystemBypass', 'Lockpicking', 'ItemLost', 'Minor', NULL, NULL, 'You drop your pick, and it falls into the mechanism where you can''t retrieve it.', 1.0, '["Equipment_Loss"]'),

('SystemBypass', 'Lockpicking', 'AlarmTriggered', 'Severe', NULL, NULL, 'Something clicks wrong. You hear a distant bell begin to ring—an alarm!', 1.0, '["Dangerous", "Detection"]'),
('SystemBypass', 'Lockpicking', 'AlarmTriggered', 'Severe', NULL, NULL, 'Your clumsy attempt triggers a hidden mechanism. Lights flash red throughout the corridor!', 1.0, '["Dangerous", "Detection"]'),
('SystemBypass', 'Lockpicking', 'DetectedByEnemy', 'Severe', NULL, NULL, 'The lock wasn''t just locked—it was wired to an alarm. Footsteps approach rapidly!', 1.0, '["Dangerous", "Detection"]'),

('SystemBypass', 'Lockpicking', 'TrapActivated', 'Severe', '1d6', NULL, 'You miss the telltale signs. A hidden needle springs out and pricks your finger! [Poisoned status]', 1.0, '["Dangerous", "Trap"]'),
('SystemBypass', 'Lockpicking', 'TrapActivated', 'Catastrophic', '2d6', NULL, 'The lock was trapped! A gout of flame erupts from the keyhole! [Burning status, 2d6 damage]', 1.0, '["Dangerous", "Trap"]'),
('SystemBypass', 'Lockpicking', 'TrapActivated', 'Severe', '1d10', NULL, 'Too late, you realize this is a trapped lock. Electricity arcs through your hands! [1d10 damage, Stunned]', 1.5, '["Dangerous", "Trap"]'),
('SystemBypass', 'Lockpicking', 'TrapActivated', 'Severe', '1d6', NULL, 'Gas hisses from vents around the lock. The smell is acrid and toxic! [Poisoned, 1d6 per turn]', 1.0, '["Dangerous", "Trap"]');

-- ==============================================================================
-- SYSTEM BYPASS: Terminal Hacking
-- ==============================================================================

-- Attempt Descriptions
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'TerminalHacking', 'Attempt', NULL, NULL, 'You interface with the altar, your fingers flying across the corrupted glyph-pad.', 1.0, '["Technical"]'),
('SystemBypass', 'TerminalHacking', 'Attempt', NULL, NULL, 'The terminal flickers to life as you access its core logic.', 1.0, '["Concise"]'),
('SystemBypass', 'TerminalHacking', 'Attempt', NULL, NULL, 'You begin to navigate the ancient logic-weaves, looking for access points.', 1.0, '["Technical"]'),

('SystemBypass', 'TerminalHacking', 'Attempt', NULL, 'GlitchedTerrain', 'The terminal''s display is fractured, showing three overlapping interfaces. This will be challenging.', 1.0, '["Verbose", "Blight"]'),
('SystemBypass', 'TerminalHacking', 'Attempt', NULL, 'GlitchedTerrain', 'Blight corruption has warped the terminal''s logic. The Machine-Spirit does not respond as it should.', 1.0, '["Verbose", "Blight"]'),
('SystemBypass', 'TerminalHacking', 'Attempt', NULL, 'GlitchedTerrain', 'The system fights you, paradoxical errors appearing at every turn.', 1.0, '["Verbose", "Blight"]'),

-- Success Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'TerminalHacking', 'Success', 'Minimal', NULL, 'You barely force your way through the system''s wards. Access granted, but it wasn''t pretty.', 1.0, '["Verbose"]'),
('SystemBypass', 'TerminalHacking', 'Success', 'Minimal', NULL, 'The terminal accepts your credentials reluctantly. You''re in, but it was close.', 1.0, '["Concise"]'),

('SystemBypass', 'TerminalHacking', 'Success', 'Solid', NULL, 'You navigate the logic-maze efficiently. Before long, you have full access.', 1.0, '["Concise"]'),
('SystemBypass', 'TerminalHacking', 'Success', 'Solid', NULL, 'The terminal''s security crumbles before your expertise. You''re in.', 1.0, '["Concise"]'),

('SystemBypass', 'TerminalHacking', 'CriticalSuccess', 'Critical', NULL, 'You slice through the security like it wasn''t even there. Core command achieved.', 1.0, '["Dramatic"]'),
('SystemBypass', 'TerminalHacking', 'CriticalSuccess', 'Critical', NULL, 'The system doesn''t even realize it''s been compromised. Perfect infiltration.', 1.0, '["Dramatic"]');

-- Failure/Glitched System
INSERT INTO Skill_Fumble_Descriptors (skill_type, action_type, consequence_type, severity, time_penalty_minutes, prevents_retry, descriptor_text, weight, tags) VALUES
('SystemBypass', 'TerminalHacking', 'TimeWasted', 'Moderate', 10, 1, 'Access denied. The terminal locks you out completely. [Cannot retry for a short while]', 1.0, '["Frustrating"]'),
('SystemBypass', 'TerminalHacking', 'EnvironmentalHazard', 'Severe', NULL, 0, 'The system detects your intrusion and shuts down. All terminals in this area go dark.', 1.5, '["Dangerous"]'),

('SystemBypass', 'TerminalHacking', 'BlightCorruption', 'Severe', NULL, 0, 'The data you access is infected with Blight corruption. Reading it sears your mind! [+8 Psychic Stress]', 1.0, '["Dangerous", "Blight"]'),
('SystemBypass', 'TerminalHacking', 'BlightCorruption', 'Severe', NULL, 0, 'The terminal''s display flickers, showing impossible geometries. Your head pounds! [+5 Psychic Stress, Disoriented]', 1.0, '["Dangerous", "Blight"]');

-- ==============================================================================
-- SYSTEM BYPASS: Trap Disarming
-- ==============================================================================

-- Identification Phase (Attempt)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'TrapDisarm', 'Attempt', NULL, NULL, 'You spot the pressure plate, barely visible beneath the dust. There''s definitely a trap here.', 1.0, '["Verbose"]'),
('SystemBypass', 'TrapDisarm', 'Attempt', NULL, NULL, 'Thin wires at ankle height—a tripwire. You kneel to examine the mechanism.', 1.0, '["Technical"]'),
('SystemBypass', 'TrapDisarm', 'Attempt', NULL, NULL, 'Your trained eye catches the inconsistency. This floor tile is newer than the others. Trapped.', 1.0, '["Technical"]'),

('SystemBypass', 'TrapDisarm', 'Attempt', NULL, 'ComplexLock', 'You trace the wires to their source. This trap connects to... ceiling collapse charges. Dangerous.', 1.0, '["Verbose", "Dangerous"]'),
('SystemBypass', 'TrapDisarm', 'Attempt', NULL, 'ComplexLock', 'The mechanism is complex—multiple triggers, redundant systems. Whoever built this knew their craft.', 1.0, '["Technical"]'),
('SystemBypass', 'TrapDisarm', 'Attempt', NULL, 'SimpleLock', 'Simple spring-loaded dart trap. Straightforward to disarm.', 1.0, '["Concise"]');

-- Successful Disarm
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('SystemBypass', 'TrapDisarm', 'Success', 'Solid', NULL, 'You carefully cut the wire. The tension releases harmlessly. Trap neutralized.', 1.0, '["Concise"]'),
('SystemBypass', 'TrapDisarm', 'Success', 'Solid', NULL, 'With steady hands, you disable the trigger mechanism. The trap is now safe.', 1.0, '["Concise"]'),
('SystemBypass', 'TrapDisarm', 'Success', 'Solid', NULL, 'You remove the firing pin from the dart launcher. This trap won''t hurt anyone now.', 1.0, '["Technical"]'),

('SystemBypass', 'TrapDisarm', 'CriticalSuccess', 'Critical', NULL, 'Not only do you disarm the trap, but you salvage the mechanism intact! [+1 Trap Component]', 1.0, '["Dramatic"]'),
('SystemBypass', 'TrapDisarm', 'CriticalSuccess', 'Critical', NULL, 'You disarm the trap so cleanly, you can actually reuse the parts.', 1.0, '["Dramatic"]');

-- Fumbles (Trap Triggers)
INSERT INTO Skill_Fumble_Descriptors (skill_type, action_type, consequence_type, severity, damage_formula, descriptor_text, weight, tags) VALUES
('SystemBypass', 'TrapDisarm', 'TrapActivated', 'Severe', '2d6', 'Your hand slips! The trap activates! [2d6 damage]', 1.0, '["Dangerous"]'),
('SystemBypass', 'TrapDisarm', 'TrapActivated', 'Catastrophic', '3d6', 'You cut the wrong wire. The mechanism fires immediately! [3d6 damage]', 1.5, '["Dangerous"]'),
('SystemBypass', 'TrapDisarm', 'TrapActivated', 'Severe', '2d6', 'The trap''s deadman switch activates when you touch it. You barely have time to react! [2d6 damage]', 1.0, '["Dangerous"]');

-- ==============================================================================
-- ACROBATICS: Climbing
-- ==============================================================================

-- Attempt Descriptions
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Climbing', 'Attempt', NULL, 'CorrodedStructure', 'You search for handholds on the corroded metal surface.', 1.0, '["Concise"]'),
('Acrobatics', 'Climbing', 'Attempt', NULL, 'CorrodedStructure', 'The rust flakes away under your fingers as you begin to climb.', 1.0, '["Verbose"]'),
('Acrobatics', 'Climbing', 'Attempt', NULL, 'CorrodedStructure', 'Every handhold is a gamble—will the corroded metal hold your weight?', 1.0, '["Dramatic"]'),
('Acrobatics', 'Climbing', 'Attempt', NULL, 'CorrodedStructure', 'You test each grip carefully before committing your weight.', 1.0, '["Technical"]'),

('Acrobatics', 'Climbing', 'Attempt', NULL, 'DangerousHeight', 'You look up at the towering structure. This will be a challenging climb.', 1.0, '["Verbose"]'),
('Acrobatics', 'Climbing', 'Attempt', NULL, 'DangerousHeight', 'One wrong move from this height, and the fall will be deadly.', 1.0, '["Dramatic"]'),
('Acrobatics', 'Climbing', 'Attempt', NULL, 'DangerousHeight', 'You take a deep breath, center yourself, then begin the ascent.', 1.0, '["Verbose"]'),
('Acrobatics', 'Climbing', 'Attempt', NULL, 'DangerousHeight', 'The ground is very far below. You don''t look down.', 1.0, '["Concise"]');

-- Success by Degree
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Climbing', 'Success', 'Minimal', NULL, 'You haul yourself up with great effort. Your arms burn from the exertion.', 1.0, '["Struggle"]'),
('Acrobatics', 'Climbing', 'Success', 'Minimal', NULL, 'You barely make it to the top. That was more exhausting than expected.', 1.0, '["Struggle"]'),
('Acrobatics', 'Climbing', 'Success', 'Minimal', NULL, 'After a difficult climb, you reach your destination. You''re breathing hard.', 1.0, '["Struggle"]'),
('Acrobatics', 'Climbing', 'Success', 'Minimal', NULL, 'Your muscles scream in protest, but you made it.', 1.0, '["Concise"]'),

('Acrobatics', 'Climbing', 'Success', 'Solid', NULL, 'You climb efficiently, finding good handholds. The ascent is tiring but manageable.', 1.0, '["Concise"]'),
('Acrobatics', 'Climbing', 'Success', 'Solid', NULL, 'Your route up is well-chosen. You reach the top swiftly.', 1.0, '["Concise"]'),
('Acrobatics', 'Climbing', 'Success', 'Solid', NULL, 'You scale the structure with practiced ease.', 1.0, '["Concise"]'),
('Acrobatics', 'Climbing', 'Success', 'Solid', NULL, 'The climb is challenging but you handle it competently.', 1.0, '["Concise"]'),

('Acrobatics', 'Climbing', 'CriticalSuccess', 'Critical', NULL, 'You flow up the structure like water, barely even winded at the top!', 1.0, '["Dramatic"]'),
('Acrobatics', 'Climbing', 'CriticalSuccess', 'Critical', NULL, 'Your climbing is a thing of beauty—efficient, graceful, effortless.', 1.0, '["Dramatic", "Verbose"]'),
('Acrobatics', 'Climbing', 'CriticalSuccess', 'Critical', NULL, 'You make the ascent look easy. Natural athleticism combined with perfect technique.', 1.0, '["Dramatic"]'),
('Acrobatics', 'Climbing', 'CriticalSuccess', 'Critical', NULL, 'You could teach a masterclass on climbing. Flawless execution.', 1.0, '["Dramatic"]');

-- Failure/Fall Consequences
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Climbing', 'Failure', 'Minimal', NULL, 'A handhold crumbles. You slide partway down, catching yourself on a lower ledge. [No damage, must retry]', 1.0, '["Recoverable"]'),
('Acrobatics', 'Climbing', 'Failure', 'Minimal', NULL, 'Your grip fails. You descend rapidly but manage to slow your fall. [1d6 damage]', 1.0, '["Minor_Injury"]'),
('Acrobatics', 'Climbing', 'Failure', 'Minimal', NULL, 'You slip, but catch yourself quickly. Your heart pounds. That was close.', 1.0, '["Dramatic"]');

INSERT INTO Skill_Fumble_Descriptors (skill_type, action_type, consequence_type, severity, damage_formula, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Climbing', 'InjuryTaken', 'Severe', '3d6', 'The metal gives way! You fall, striking the ground hard! [3d6 damage, Prone]', 1.0, '["Dangerous"]'),
('Acrobatics', 'Climbing', 'InjuryTaken', 'Severe', 'FALL', 'You lose your grip and plummet! [Take fall damage based on height]', 1.0, '["Dangerous"]'),
('Acrobatics', 'Climbing', 'InjuryTaken', 'Catastrophic', 'FALL', 'The handhold tears free from the wall. You''re falling!', 1.0, '["Dangerous"]'),

('Acrobatics', 'Climbing', 'StructuralCollapse', 'Catastrophic', '5d6', 'The entire section of wall you''re climbing collapses! [5d6 damage, buried in debris]', 0.5, '["Dangerous", "Catastrophic"]'),
('Acrobatics', 'Climbing', 'StructuralCollapse', 'Catastrophic', '4d6', 'Your weight triggers a structural failure. Metal beams rain down! [4d6 damage, multiple hazards]', 0.5, '["Dangerous", "Catastrophic"]'),
('Acrobatics', 'Climbing', 'StructuralCollapse', 'Catastrophic', '4d6', 'The corroded structure fails catastrophically. Everything is coming down! [4d6 damage]', 0.5, '["Dangerous", "Catastrophic"]');

-- ==============================================================================
-- ACROBATICS: Leaping/Jumping
-- ==============================================================================

-- Attempt Descriptions
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Leaping', 'Attempt', NULL, NULL, 'You back up for a running start. The gap yawns before you.', 1.0, '["Dramatic"]'),
('Acrobatics', 'Leaping', 'Attempt', NULL, NULL, 'You measure the distance with your eyes. It''s jumpable... barely.', 1.0, '["Concise"]'),
('Acrobatics', 'Leaping', 'Attempt', NULL, NULL, 'This will require perfect timing and maximum effort.', 1.0, '["Dramatic"]'),
('Acrobatics', 'Leaping', 'Attempt', NULL, NULL, 'You take a deep breath, then sprint toward the edge.', 1.0, '["Dramatic"]'),

('Acrobatics', 'Leaping', 'Attempt', NULL, 'GlitchedTerrain', 'The gap flickers—is it a spear-length or a chasm? Coherent Glitches make this treacherous.', 1.0, '["Blight", "Dramatic"]'),
('Acrobatics', 'Leaping', 'Attempt', NULL, 'GlitchedTerrain', 'Reality stutters as you prepare to jump. The distance keeps changing.', 1.0, '["Blight", "Dramatic"]'),
('Acrobatics', 'Leaping', 'Attempt', NULL, 'GlitchedTerrain', 'The Blight has warped the space here. You''ll have to trust your instincts.', 1.0, '["Blight"]');

-- Success Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Leaping', 'Success', 'Minimal', NULL, 'You barely clear the gap, landing hard on the far side. You stumble but catch yourself.', 1.0, '["Struggle"]'),
('Acrobatics', 'Leaping', 'Success', 'Minimal', NULL, 'Your landing is ungraceful, but you made it across. Barely.', 1.0, '["Concise"]'),
('Acrobatics', 'Leaping', 'Success', 'Minimal', NULL, 'You catch the far ledge with your hands, then haul yourself up.', 1.0, '["Struggle"]'),

('Acrobatics', 'Leaping', 'Success', 'Solid', NULL, 'You clear the gap easily, landing with a controlled roll.', 1.0, '["Concise"]'),
('Acrobatics', 'Leaping', 'Success', 'Solid', NULL, 'Perfect execution. You land smoothly on the far side.', 1.0, '["Concise"]'),
('Acrobatics', 'Leaping', 'Success', 'Solid', NULL, 'The jump is well within your capabilities. You make it look easy.', 1.0, '["Concise"]'),

('Acrobatics', 'Leaping', 'CriticalSuccess', 'Critical', NULL, 'You soar across the gap with room to spare, landing like a cat!', 1.0, '["Dramatic"]'),
('Acrobatics', 'Leaping', 'CriticalSuccess', 'Critical', NULL, 'That was almost too easy. You had plenty of distance left.', 1.0, '["Dramatic"]'),
('Acrobatics', 'Leaping', 'CriticalSuccess', 'Critical', NULL, 'Textbook-perfect jump. You stick the landing flawlessly.', 1.0, '["Dramatic"]');

-- Failure (Fall into Chasm)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Leaping', 'Failure', 'Minimal', NULL, 'You don''t make it! You catch the far edge with your fingertips, dangling! [Must make STR check to pull up]', 1.0, '["Dramatic", "Dangerous"]'),
('Acrobatics', 'Leaping', 'Failure', 'Minimal', NULL, 'Short! You slam into the far wall and start sliding down! [Grab check required]', 1.0, '["Dramatic", "Dangerous"]');

INSERT INTO Skill_Fumble_Descriptors (skill_type, action_type, consequence_type, severity, damage_formula, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Leaping', 'InjuryTaken', 'Catastrophic', 'FALL', 'You fall short and plummet into the chasm below! [Major fall damage]', 1.0, '["Dangerous", "Catastrophic"]'),
('Acrobatics', 'Leaping', 'InjuryTaken', 'Catastrophic', 'FALL', 'The gap was too wide. You''re falling!', 1.0, '["Dangerous", "Catastrophic"]');

-- ==============================================================================
-- ACROBATICS: Stealth Movement
-- ==============================================================================

-- Attempt Descriptions
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Stealth', 'Attempt', NULL, 'ShadowyCover', 'You press yourself into the shadows, moving silently.', 1.0, '["Concise"]'),
('Acrobatics', 'Stealth', 'Attempt', NULL, 'ShadowyCover', 'You creep from cover to cover, trying to remain undetected.', 1.0, '["Concise"]'),
('Acrobatics', 'Stealth', 'Attempt', NULL, 'ShadowyCover', 'Every footstep is carefully placed. You barely breathe.', 1.0, '["Dramatic"]'),
('Acrobatics', 'Stealth', 'Attempt', NULL, 'ShadowyCover', 'You become one with the darkness.', 1.0, '["Concise"]'),

('Acrobatics', 'Stealth', 'Attempt', NULL, 'NoisyEnvironment', 'Debris litters the floor—one wrong step will make noise.', 1.0, '["Verbose"]'),
('Acrobatics', 'Stealth', 'Attempt', NULL, 'NoisyEnvironment', 'Rust flakes crunch underfoot. This will be difficult.', 1.0, '["Technical"]'),
('Acrobatics', 'Stealth', 'Attempt', NULL, 'NoisyEnvironment', 'Broken glass everywhere. Stealth will be challenging.', 1.0, '["Technical"]');

-- Success Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Stealth', 'Success', 'Solid', NULL, 'You move like a ghost. The guards never notice you.', 1.0, '["Dramatic"]'),
('Acrobatics', 'Stealth', 'Success', 'Solid', NULL, 'Your stealth is flawless. You pass by undetected.', 1.0, '["Concise"]'),
('Acrobatics', 'Stealth', 'Success', 'Solid', NULL, 'Silent as a shadow, you slip past your enemies.', 1.0, '["Dramatic"]'),

('Acrobatics', 'Stealth', 'CriticalSuccess', 'Critical', NULL, 'You''re invisible. Even if they looked directly at you, they might not see you!', 1.0, '["Dramatic"]'),
('Acrobatics', 'Stealth', 'CriticalSuccess', 'Critical', NULL, 'Your stealth is so perfect, you walk right past a guard who''s looking in your direction!', 1.0, '["Dramatic"]'),
('Acrobatics', 'Stealth', 'CriticalSuccess', 'Critical', NULL, 'Master-level stealth. You could steal their weapons without them noticing.', 1.0, '["Dramatic"]');

-- Failure Descriptors (Detected)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Acrobatics', 'Stealth', 'Failure', 'Minimal', NULL, 'A piece of metal clangs as you step on it. Everyone turns toward the sound!', 1.0, '["Dramatic"]'),
('Acrobatics', 'Stealth', 'Failure', 'Minimal', NULL, 'You knock over a rusted can. It echoes loudly!', 1.0, '["Concise"]'),
('Acrobatics', 'Stealth', 'Failure', 'Minimal', NULL, 'Your boot scrapes against metal. ''Who''s there?!'' a guard shouts.', 1.0, '["Dramatic"]'),

('Acrobatics', 'Stealth', 'Failure', 'Solid', NULL, 'A guard spots you! ''Intruder!''', 1.0, '["Concise"]'),
('Acrobatics', 'Stealth', 'Failure', 'Solid', NULL, 'You step into a pool of light. You''re seen immediately!', 1.0, '["Dramatic"]'),
('Acrobatics', 'Stealth', 'Failure', 'Solid', NULL, 'You emerge from shadow at exactly the wrong moment. They see you!', 1.0, '["Dramatic"]');

-- ==============================================================================
-- WASTELAND SURVIVAL: Tracking
-- ==============================================================================

-- Examining Tracks (Attempt)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'FreshTracks', 'Fresh tracks in the rust-dust. Something passed through here very recently.', 1.0, '["Technical"]'),
('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'FreshTracks', 'You kneel, examining the footprints. These are recent—still sharp-edged.', 1.0, '["Verbose"]'),
('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'FreshTracks', 'Blood droplets on the ground, still wet. Your quarry is close.', 1.0, '["Dramatic"]'),

('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'OldTracks', 'Faint impressions in the dust. These are days old.', 1.0, '["Concise"]'),
('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'OldTracks', 'The tracks are weathered, partially obscured. Old, but still followable.', 1.0, '["Verbose"]'),
('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'OldTracks', 'Whatever left these tracks is long gone, but you can still read the story.', 1.0, '["Verbose"]'),

('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'UnusualTracks', 'These aren''t human. The gait is wrong, the foot shape... inhuman.', 1.0, '["Dramatic"]'),
('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'UnusualTracks', 'Servitor tracks—metallic impressions in the dust, with hydraulic fluid drips.', 1.0, '["Technical", "Lore"]'),
('WastelandSurvival', 'Tracking', 'Attempt', NULL, 'UnusualTracks', 'Something Blight-touched passed through here. The tracks flicker when you look at them directly.', 1.0, '["Blight", "Dramatic"]');

-- Success Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Tracking', 'Success', 'Minimal', NULL, 'You manage to follow the trail, though it''s difficult. You lose it several times but pick it up again.', 1.0, '["Struggle"]'),
('WastelandSurvival', 'Tracking', 'Success', 'Minimal', NULL, 'The tracks are faint but you can just barely make them out.', 1.0, '["Concise"]'),
('WastelandSurvival', 'Tracking', 'Success', 'Minimal', NULL, 'This is challenging, but you''re managing to stay on the trail.', 1.0, '["Concise"]'),

('WastelandSurvival', 'Tracking', 'Success', 'Solid', NULL, 'The trail is clear to your trained eye. You follow it easily.', 1.0, '["Concise"]'),
('WastelandSurvival', 'Tracking', 'Success', 'Solid', NULL, 'You read the tracks like a book, understanding every movement your quarry made.', 1.0, '["Dramatic"]'),
('WastelandSurvival', 'Tracking', 'Success', 'Solid', NULL, 'Your expertise makes this look easy. The trail practically glows for you.', 1.0, '["Dramatic"]'),

('WastelandSurvival', 'Tracking', 'CriticalSuccess', 'Critical', NULL, 'You don''t just track them—you predict where they''re going. You take a shortcut and get ahead!', 1.0, '["Dramatic"]'),
('WastelandSurvival', 'Tracking', 'CriticalSuccess', 'Critical', NULL, 'From the tracks, you determine exact numbers, equipment, even their condition. They''re injured.', 1.0, '["Dramatic", "Technical"]'),
('WastelandSurvival', 'Tracking', 'CriticalSuccess', 'Critical', NULL, 'You can tell they stopped here to rest earlier this watch. You''re gaining on them.', 1.0, '["Dramatic", "Technical"]');

-- Failure/Lost Trail
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Tracking', 'Failure', 'Minimal', NULL, 'The tracks end at rocky ground. You search but can''t find where they continue.', 1.0, '["Frustrating"]'),
('WastelandSurvival', 'Tracking', 'Failure', 'Minimal', NULL, 'You lose the trail at a stream crossing. They could have gone any direction.', 1.0, '["Frustrating"]'),
('WastelandSurvival', 'Tracking', 'Failure', 'Minimal', NULL, 'The trail simply... stops. As if they vanished.', 1.0, '["Dramatic"]'),

('WastelandSurvival', 'Tracking', 'Failure', 'Solid', NULL, 'You follow the tracks for a long while before realizing they''re the wrong ones. You''ve wasted time.', 1.0, '["Frustrating"]'),
('WastelandSurvival', 'Tracking', 'Failure', 'Solid', NULL, 'These tracks led you in a circle. You''re back where you started.', 1.0, '["Frustrating"]');

-- ==============================================================================
-- WASTELAND SURVIVAL: Foraging
-- ==============================================================================

-- Searching for Resources (Attempt)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Foraging', 'Attempt', NULL, 'RichArea', 'This area shows signs of plant life—rare in the ruins. You search for edible growth.', 1.0, '["Verbose"]'),
('WastelandSurvival', 'Foraging', 'Attempt', NULL, 'RichArea', 'Mushrooms grow in the damp corners. Some might be edible.', 1.0, '["Concise"]'),
('WastelandSurvival', 'Foraging', 'Attempt', NULL, 'RichArea', 'You spot fungal growth on the walls. Carefully, you harvest some.', 1.0, '["Technical"]'),

('WastelandSurvival', 'Foraging', 'Attempt', NULL, 'DangerousArea', 'These mushrooms look edible, but you''re not sure. One wrong identification could be fatal.', 1.0, '["Dramatic"]'),
('WastelandSurvival', 'Foraging', 'Attempt', NULL, 'DangerousArea', 'The plants here are Blight-touched. They might be poisonous... or worse.', 1.0, '["Blight", "Dramatic"]');

-- Success Descriptors (Food Found)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Foraging', 'Success', 'Solid', NULL, 'You identify several edible fungi. Not appetizing, but safe. [+2 Rations]', 1.0, '["Concise"]'),
('WastelandSurvival', 'Foraging', 'Success', 'Solid', NULL, 'You find a cache of preserved food, ancient but still sealed. [+5 Rations]', 1.0, '["Dramatic"]'),
('WastelandSurvival', 'Foraging', 'Success', 'Solid', NULL, 'Edible moss grows here. You carefully harvest it. [+1 Ration]', 1.0, '["Concise"]'),

('WastelandSurvival', 'Foraging', 'CriticalSuccess', 'Critical', NULL, 'You discover a patch of Luminous Shelf Fungus! This is valuable! [+3 Rations, +1 Alchemical Ingredient]', 1.0, '["Dramatic", "Lore"]'),
('WastelandSurvival', 'Foraging', 'CriticalSuccess', 'Critical', NULL, 'You find medicinal herbs growing in a protected alcove! [+2 Healing Poultices]', 1.0, '["Dramatic"]');

-- Failure (Nothing Found)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Foraging', 'Failure', 'Solid', NULL, 'You search thoroughly but find nothing edible.', 1.0, '["Concise"]'),
('WastelandSurvival', 'Foraging', 'Failure', 'Solid', NULL, 'This area is picked clean. Nothing remains.', 1.0, '["Concise"]');

-- Fumble (Poisoned)
INSERT INTO Skill_Fumble_Descriptors (skill_type, action_type, consequence_type, severity, status_effect_applied, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Foraging', 'Poisoned', 'Severe', 'Poisoned', 'You misidentify the fungi. It''s poisonous! [Poisoned status after eating]', 1.0, '["Dangerous"]'),
('WastelandSurvival', 'Foraging', 'BlightCorruption', 'Severe', 'Sickened', 'The ''food'' you found was contaminated with Blight. [+5 Psychic Stress, Sickened]', 1.0, '["Dangerous", "Blight"]');

-- ==============================================================================
-- WASTELAND SURVIVAL: Navigation
-- ==============================================================================

-- Attempt Descriptions
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Navigation', 'Attempt', NULL, 'NormalTravel', 'You plot a course through the ruins, finding the safest path.', 1.0, '["Concise"]'),
('WastelandSurvival', 'Navigation', 'Attempt', NULL, 'NormalTravel', 'You navigate by landmarks—the broken tower, the collapsed bridge.', 1.0, '["Technical"]'),
('WastelandSurvival', 'Navigation', 'Attempt', NULL, 'NormalTravel', 'Your knowledge of the area guides you efficiently.', 1.0, '["Concise"]'),

('WastelandSurvival', 'Navigation', 'Attempt', NULL, 'StormHazard', 'The Static Storm obscures everything. Navigation will be difficult.', 1.0, '["Dramatic"]'),
('WastelandSurvival', 'Navigation', 'Attempt', NULL, 'GlitchedSpace', 'Coherent Glitches warp the space here. The same corridor leads to different places.', 1.0, '["Blight", "Dramatic"]'),
('WastelandSurvival', 'Navigation', 'Attempt', NULL, 'GlitchedSpace', 'The Blight makes navigation unreliable. Paths shift and change.', 1.0, '["Blight"]');

-- Success Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Navigation', 'Success', 'Solid', NULL, 'Your navigation is accurate. You arrive at your destination without incident.', 1.0, '["Concise"]'),
('WastelandSurvival', 'Navigation', 'Success', 'Solid', NULL, 'You find an efficient route, saving time.', 1.0, '["Concise"]'),
('WastelandSurvival', 'Navigation', 'Success', 'Solid', NULL, 'Your path avoids hazards and hostile encounters.', 1.0, '["Concise"]'),

('WastelandSurvival', 'Navigation', 'CriticalSuccess', 'Critical', NULL, 'You discover a hidden passage that cuts travel time in half!', 1.0, '["Dramatic"]'),
('WastelandSurvival', 'Navigation', 'CriticalSuccess', 'Critical', NULL, 'You find an old service tunnel that bypasses the entire dangerous section!', 1.0, '["Dramatic"]');

-- Failure (Lost)
INSERT INTO Skill_Fumble_Descriptors (skill_type, action_type, consequence_type, severity, time_penalty_minutes, descriptor_text, weight, tags) VALUES
('WastelandSurvival', 'Navigation', 'TimeWasted', 'Moderate', 180, 'You take a wrong turn. It takes a long time to realize and backtrack. [Lost significant time]', 1.0, '["Frustrating"]'),
('WastelandSurvival', 'Navigation', 'TimeWasted', 'Moderate', 60, 'You''re temporarily lost. Eventually you find your way, but you''ve wasted time. [Lost some time]', 1.0, '["Frustrating"]'),

('WastelandSurvival', 'Navigation', 'ProgressLost', 'Severe', NULL, 'You have no idea where you are. The landmarks don''t match your memory.', 1.0, '["Frustrating"]'),
('WastelandSurvival', 'Navigation', 'ProgressLost', 'Severe', NULL, 'You''re completely turned around. It will take significant time to reorient.', 1.0, '["Frustrating"]');

-- ==============================================================================
-- RHETORIC: Persuasion
-- ==============================================================================

-- Attempt Descriptions
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Persuasion', 'Attempt', NULL, 'ReasonableRequest', 'You make your case calmly and reasonably.', 1.0, '["Concise"]'),
('Rhetoric', 'Persuasion', 'Attempt', NULL, 'ReasonableRequest', 'You explain the mutual benefits of cooperation.', 1.0, '["Technical"]'),
('Rhetoric', 'Persuasion', 'Attempt', NULL, 'ReasonableRequest', 'You appeal to their better nature.', 1.0, '["Concise"]'),

('Rhetoric', 'Persuasion', 'Attempt', NULL, 'DifficultRequest', 'This is a big ask. You''ll need to be convincing.', 1.0, '["Verbose"]'),
('Rhetoric', 'Persuasion', 'Attempt', NULL, 'DifficultRequest', 'They''re skeptical. You''ll need to win them over.', 1.0, '["Dramatic"]'),
('Rhetoric', 'Persuasion', 'Attempt', NULL, 'DifficultRequest', 'You can see the resistance in their eyes. This won''t be easy.', 1.0, '["Dramatic"]');

-- Success by Degree
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Persuasion', 'Success', 'Minimal', NULL, 'After much debate, they reluctantly agree.', 1.0, '["Concise"]'),
('Rhetoric', 'Persuasion', 'Success', 'Minimal', NULL, 'They''re not happy about it, but they''ll do as you ask.', 1.0, '["Concise"]'),
('Rhetoric', 'Persuasion', 'Success', 'Minimal', NULL, 'You barely convince them. Their agreement is grudging.', 1.0, '["Verbose"]'),

('Rhetoric', 'Persuasion', 'Success', 'Solid', NULL, 'Your argument is compelling. They agree.', 1.0, '["Concise"]'),
('Rhetoric', 'Persuasion', 'Success', 'Solid', NULL, 'You make a strong case. They''re convinced.', 1.0, '["Concise"]'),
('Rhetoric', 'Persuasion', 'Success', 'Solid', NULL, 'Your rhetoric sways them. They''ll help you.', 1.0, '["Concise"]'),

('Rhetoric', 'Persuasion', 'CriticalSuccess', 'Critical', NULL, 'You don''t just convince them—you inspire them! They''re enthusiastic allies now!', 1.0, '["Dramatic"]'),
('Rhetoric', 'Persuasion', 'CriticalSuccess', 'Critical', NULL, 'Your words are so compelling, they offer more help than you even asked for!', 1.0, '["Dramatic"]'),
('Rhetoric', 'Persuasion', 'CriticalSuccess', 'Critical', NULL, 'You could sell ice to the Niflheim natives. They''re completely won over.', 1.0, '["Dramatic", "Comedic"]');

-- Failure Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Persuasion', 'Failure', 'Minimal', NULL, 'Your argument doesn''t land. They refuse.', 1.0, '["Concise"]'),
('Rhetoric', 'Persuasion', 'Failure', 'Minimal', NULL, 'They shake their head. ''No. I don''t think so.''', 1.0, '["Concise"]'),
('Rhetoric', 'Persuasion', 'Failure', 'Minimal', NULL, 'You can''t sway them. They''re unmoved.', 1.0, '["Concise"]'),

('Rhetoric', 'Persuasion', 'Failure', 'Solid', NULL, 'Your attempt to persuade them actually offends them. They''re now hostile!', 1.0, '["Dramatic"]'),
('Rhetoric', 'Persuasion', 'Failure', 'Solid', NULL, 'Wrong approach. They''re insulted by your words.', 1.0, '["Dramatic"]');

-- ==============================================================================
-- RHETORIC: Deception
-- ==============================================================================

-- Attempt Descriptions
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Deception', 'Attempt', NULL, 'SimpleDeception', 'You tell them a simple, believable lie.', 1.0, '["Concise"]'),
('Rhetoric', 'Deception', 'Attempt', NULL, 'SimpleDeception', 'You keep your expression neutral, your voice calm.', 1.0, '["Technical"]'),
('Rhetoric', 'Deception', 'Attempt', NULL, 'SimpleDeception', 'You weave a plausible falsehood.', 1.0, '["Concise"]'),

('Rhetoric', 'Deception', 'Attempt', NULL, 'ComplexDeception', 'This is an elaborate deception. You''ll need to sell it completely.', 1.0, '["Verbose"]'),
('Rhetoric', 'Deception', 'Attempt', NULL, 'ComplexDeception', 'Multiple layers to this lie. You need to keep your story straight.', 1.0, '["Technical"]'),
('Rhetoric', 'Deception', 'Attempt', NULL, 'ComplexDeception', 'One slip and they''ll see through you.', 1.0, '["Dramatic"]');

-- Success Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Deception', 'Success', 'Solid', NULL, 'They buy it completely. They suspect nothing.', 1.0, '["Concise"]'),
('Rhetoric', 'Deception', 'Success', 'Solid', NULL, 'Your lie is convincing. They accept it as truth.', 1.0, '["Concise"]'),
('Rhetoric', 'Deception', 'Success', 'Solid', NULL, 'You maintain the deception flawlessly.', 1.0, '["Concise"]'),

('Rhetoric', 'Deception', 'CriticalSuccess', 'Critical', NULL, 'Not only do they believe you, they''re grateful for the ''truth'' you''ve shared!', 1.0, '["Dramatic"]'),
('Rhetoric', 'Deception', 'CriticalSuccess', 'Critical', NULL, 'Your lie is so well-crafted, you almost believe it yourself!', 1.0, '["Dramatic"]');

-- Failure (Caught in Lie)
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Deception', 'Failure', 'Minimal', NULL, 'They narrow their eyes. ''I don''t believe you.''', 1.0, '["Dramatic"]'),
('Rhetoric', 'Deception', 'Failure', 'Minimal', NULL, 'They''re skeptical but not confrontational. They don''t trust you now.', 1.0, '["Verbose"]'),

('Rhetoric', 'Deception', 'Failure', 'Solid', NULL, 'They see right through you. ''You''re lying to me.''', 1.0, '["Dramatic"]'),
('Rhetoric', 'Deception', 'Failure', 'Solid', NULL, 'Your deception fails completely. They know the truth.', 1.0, '["Concise"]'),
('Rhetoric', 'Deception', 'Failure', 'Solid', NULL, 'They catch you in the lie. Now they''re hostile.', 1.0, '["Dramatic"]');

-- ==============================================================================
-- RHETORIC: Intimidation
-- ==============================================================================

-- Attempt Descriptions
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Intimidation', 'Attempt', NULL, NULL, 'You let your hand rest meaningfully on your weapon.', 1.0, '["Dramatic"]'),
('Rhetoric', 'Intimidation', 'Attempt', NULL, NULL, 'You step forward, using your size to intimidate.', 1.0, '["Concise"]'),
('Rhetoric', 'Intimidation', 'Attempt', NULL, NULL, 'You make it clear that violence is an option.', 1.0, '["Dramatic"]'),

('Rhetoric', 'Intimidation', 'Attempt', NULL, NULL, 'You invoke your reputation. They should know better than to cross you.', 1.0, '["Verbose"]'),
('Rhetoric', 'Intimidation', 'Attempt', NULL, NULL, 'You make subtle threats about consequences.', 1.0, '["Technical"]'),
('Rhetoric', 'Intimidation', 'Attempt', NULL, NULL, 'You paint a picture of what happens to those who defy you.', 1.0, '["Verbose"]');

-- Success Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Intimidation', 'Success', 'Solid', NULL, 'They back down immediately. Fear wins.', 1.0, '["Concise"]'),
('Rhetoric', 'Intimidation', 'Success', 'Solid', NULL, 'You can see them calculating the risk. They decide compliance is safer.', 1.0, '["Verbose"]'),
('Rhetoric', 'Intimidation', 'Success', 'Solid', NULL, 'Your intimidation works. They yield.', 1.0, '["Concise"]'),

('Rhetoric', 'Intimidation', 'CriticalSuccess', 'Critical', NULL, 'They''re terrified. They''ll do anything you ask!', 1.0, '["Dramatic"]'),
('Rhetoric', 'Intimidation', 'CriticalSuccess', 'Critical', NULL, 'Your reputation precedes you. They surrender immediately.', 1.0, '["Dramatic"]');

-- Failure Descriptors
INSERT INTO Skill_Check_Descriptors (skill_type, action_type, check_phase, result_degree, environmental_context, descriptor_text, weight, tags) VALUES
('Rhetoric', 'Intimidation', 'Failure', 'Minimal', NULL, 'They laugh at your threat. ''Is that supposed to scare me?''', 1.0, '["Comedic"]'),
('Rhetoric', 'Intimidation', 'Failure', 'Minimal', NULL, 'Your intimidation attempt has no effect. They''re unafraid.', 1.0, '["Concise"]'),

('Rhetoric', 'Intimidation', 'Failure', 'Solid', NULL, 'Your threat backfires. Now they''re angry and aggressive!', 1.0, '["Dramatic"]'),
('Rhetoric', 'Intimidation', 'Failure', 'Solid', NULL, 'You''ve escalated this into violence. They attack!', 1.0, '["Dramatic"]');

-- ==============================================================================
-- End of Data Population
-- ==============================================================================
-- Total Descriptors Added:
-- - Skill Check Descriptors: 150+
-- - Fumble Descriptors: 30+
-- - Covers all 4 skill types and 12 action types
-- ==============================================================================
