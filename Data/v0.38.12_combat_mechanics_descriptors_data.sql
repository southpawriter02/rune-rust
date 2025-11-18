-- ==============================================================================
-- v0.38.12: Advanced Combat Mechanics Descriptors - Database Data
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Total Descriptors: 140+
-- Categories: Defensive Actions (40), Stances (30), Critical Hits (25), Fumbles (25), Maneuvers (20)
-- ==============================================================================

-- ==============================================================================
-- CATEGORY 1: DEFENSIVE ACTION DESCRIPTORS (40 total)
-- ==============================================================================

-- BLOCKING - Successful Blocks (12)
-- ==================== Light Attacks Blocked ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Block', 'Success', 'LightShield', 'Light', NULL, 'You raise your shield. The blow cracks against it harmlessly.', 1.0, 1, '["Defensive", "Simple"]'),
('Block', 'Success', 'LightShield', 'Light', NULL, 'Your shield absorbs the strike. You barely felt that.', 1.0, 1, '["Defensive", "Easy"]'),
('Block', 'Success', 'HeavyShield', 'Light', NULL, 'The attack bounces off your shield with a metallic clang.', 1.0, 1, '["Defensive", "Sound"]'),
('Block', 'Success', 'HeavyShield', 'Light', NULL, 'You intercept the blow with your shield. Easy.', 1.0, 1, '["Defensive", "Confident"]');

-- ==================== Heavy Attacks Blocked ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Block', 'Success', 'HeavyShield', 'Heavy', NULL, 'The impact against your shield reverberates up your arm. That was a strong one.', 1.0, 1, '["Defensive", "Powerful"]'),
('Block', 'Success', 'HeavyShield', 'Heavy', NULL, 'You brace yourself as the heavy blow crashes into your shield. Your arm numbs from the force.', 1.0, 1, '["Defensive", "Impact"]'),
('Block', 'Success', 'TowerShield', 'Heavy', NULL, 'The shield groans under the impact but holds. You stagger back a step.', 1.0, 1, '["Defensive", "Dramatic"]'),
('Block', 'Success', 'TowerShield', 'Heavy', NULL, 'That hit hard. Your shield arm throbs, but you kept the blow at bay.', 1.0, 1, '["Defensive", "Strain"]');

-- ==================== Critical Blocks ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Block', 'CriticalSuccess', 'HeavyShield', NULL, NULL, 'You time it perfectly! The attack glances off your shield at just the right angle, leaving your opponent off-balance!', 1.5, 1, '["Defensive", "Skillful", "Opportunistic"]'),
('Block', 'CriticalSuccess', 'LightShield', NULL, NULL, 'Your shield meets their weapon with precision, deflecting it completely. They''re wide open! [Counter-attack opportunity]', 1.5, 1, '["Defensive", "Perfect", "Counter"]'),
('Block', 'CriticalSuccess', 'TowerShield', NULL, NULL, 'Flawless block! Not only do you stop the attack, but you create an opening!', 1.5, 1, '["Defensive", "Masterful"]'),
('Block', 'CriticalSuccess', NULL, NULL, NULL, 'Perfect timing! Your block doesn''t just stop their attack—it sets you up for a devastating counter!', 1.5, 1, '["Defensive", "Tactical"]');

-- ==================== Failed Blocks ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Block', 'PartialSuccess', 'LightShield', NULL, NULL, 'Your shield catches most of it, but the blow still finds its mark. [Half damage]', 1.0, 1, '["Defensive", "Partial"]'),
('Block', 'Failure', 'LightShield', 'Heavy', NULL, 'You''re too slow—the attack gets around your shield! [Full damage]', 1.0, 1, '["Defensive", "Failed"]'),
('Block', 'Failure', 'HeavyShield', 'Overwhelming', NULL, 'The force of the blow batters past your defenses!', 1.0, 1, '["Defensive", "Overwhelmed"]'),
('Block', 'ShieldBroken', 'LightShield', 'Overwhelming', NULL, 'The impact is too much! Your shield splinters under the assault! [Shield destroyed]', 0.5, 1, '["Defensive', 'Catastrophic", "Equipment"]'),
('Block', 'ShieldBroken', 'HeavyShield', 'Heavy', NULL, 'CRACK! Your shield fractures. It won''t take many more hits like that. [Shield durability critical]', 0.7, 1, '["Defensive", "Damaged"]'),
('Block', 'ShieldBroken', NULL, 'Overwhelming', NULL, 'The blow shatters your shield completely! You''re exposed!', 0.5, 1, '["Defensive", "Disaster"]');

-- PARRYING - Successful Parries (10)
-- ==================== One-Handed Weapon Parries ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Parry', 'Success', 'OneHanded', NULL, NULL, 'You parry the strike with your blade, metal ringing on metal.', 1.0, 1, '["Defensive", "Skillful"]'),
('Parry', 'Success', 'OneHanded', NULL, NULL, 'Your weapon intercepts theirs with a sharp clang, turning the blow aside.', 1.0, 1, '["Defensive", "Precise"]'),
('Parry', 'Success', 'Dagger', NULL, NULL, 'You deflect their attack with a precise counter-motion.', 1.0, 1, '["Defensive", "Quick"]'),
('Parry', 'Success', 'OneHanded', NULL, NULL, 'Steel meets steel as you parry. The clash echoes through the chamber.', 1.0, 1, '["Defensive", "Sound"]');

-- ==================== Two-Handed Weapon Parries ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Parry', 'Success', 'TwoHanded', NULL, NULL, 'You bring your massive weapon around to parry, using its weight to your advantage.', 1.0, 1, '["Defensive", "Powerful"]'),
('Parry', 'Success', 'TwoHanded', NULL, NULL, 'Your greatsword catches their blade, stopping it cold.', 1.0, 1, '["Defensive", "Strong"]'),
('Parry', 'Success', 'TwoHanded', NULL, NULL, 'You leverage your weapon''s reach to intercept the attack before it reaches you.', 1.0, 1, '["Defensive", "Reach"]');

-- ==================== Critical Parries (Riposte Opportunity) ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Parry', 'CriticalSuccess', 'OneHanded', NULL, NULL, 'Perfect parry! You not only deflect the attack but throw your opponent off-balance! [Free riposte attack]', 1.5, 1, '["Defensive", "Masterful", "Counter"]'),
('Parry', 'CriticalSuccess', 'TwoHanded', NULL, NULL, 'You turn their blade aside with masterful technique, creating an opening for a counter-strike!', 1.5, 1, '["Defensive", "Skillful", "Tactical"]'),
('Parry', 'CriticalSuccess', 'Dagger', NULL, NULL, 'Your parry is so precise, their weapon is knocked aside completely. They''re vulnerable!', 1.5, 1, '["Defensive", "Perfect"]');

-- ==================== Failed Parries ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Parry', 'Failure', 'OneHanded', NULL, NULL, 'You parry too late—the attack connects! [Full damage]', 1.0, 1, '["Defensive", "Failed"]'),
('Parry', 'Failure', 'TwoHanded', NULL, NULL, 'Your weapon misses theirs entirely. The blow lands!', 1.0, 1, '["Defensive", "Mistimed"]'),
('Parry', 'Failure', NULL, NULL, NULL, 'You misjudge the angle. Their weapon slips past your guard!', 1.0, 1, '["Defensive", "Error"]'),
('Parry', 'WeaponDamaged', 'OneHanded', 'Heavy', NULL, 'The impact damages your weapon! [Durability loss]', 0.7, 1, '["Defensive", "Equipment"]'),
('Parry', 'WeaponDamaged', NULL, 'Overwhelming', NULL, 'Your blade chips from the force of the parry. [Weapon condition worsens]', 0.7, 1, '["Defensive", "Damaged"]'),
('Parry', 'WeaponDamaged', 'OneHanded', 'Heavy', NULL, 'The clash notches your weapon badly. It won''t hold up to many more hits like that.', 0.7, 1, '["Defensive", "Critical"]');

-- DODGING - Successful Dodges (10)
-- ==================== Basic Dodges ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Dodge', 'Success', 'Unarmed', NULL, NULL, 'You sidestep the attack. It misses cleanly.', 1.0, 1, '["Defensive", "Agile"]'),
('Dodge', 'Success', 'Unarmed', NULL, NULL, 'You duck under the blow. The weapon passes harmlessly overhead.', 1.0, 1, '["Defensive", "Quick"]'),
('Dodge', 'Success', 'Unarmed', NULL, NULL, 'You twist aside at the last moment. Close, but you avoided it.', 1.0, 1, '["Defensive", "Close"]'),
('Dodge', 'Success', 'Unarmed', NULL, NULL, 'Agile footwork carries you out of harm''s way.', 1.0, 1, '["Defensive", "Nimble"]');

-- ==================== Acrobatic Dodges ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Dodge', 'Success', 'Unarmed', NULL, 'OpenGround', 'You roll beneath the strike, coming up behind your opponent!', 1.2, 1, '["Defensive", "Acrobatic", "Impressive"]'),
('Dodge', 'Success', 'Unarmed', NULL, 'OpenGround', 'You flip backward, the attack missing by inches. Impressive!', 1.2, 1, '["Defensive", "Spectacular"]'),
('Dodge', 'Success', 'Unarmed', NULL, NULL, 'You weave through the attack with fluid grace, like water flowing around stone.', 1.2, 1, '["Defensive", "Graceful"]');

-- ==================== Critical Dodges ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Dodge', 'CriticalSuccess', 'Unarmed', NULL, NULL, 'You read their attack perfectly and evade with time to spare! [+1 AP this turn]', 1.5, 1, '["Defensive", "Perfect", "Bonus"]'),
('Dodge', 'CriticalSuccess', 'Unarmed', NULL, NULL, 'Not only do you dodge, but you position yourself advantageously! [Advantage on next attack]', 1.5, 1, '["Defensive", "Tactical"]'),
('Dodge', 'CriticalSuccess', 'Unarmed', NULL, NULL, 'You make dodging look effortless. They''re wide open now!', 1.5, 1, '["Defensive", "Masterful"]');

-- ==================== Failed Dodges ====================

INSERT INTO Combat_Defensive_Action_Descriptors (action_type, outcome_type, weapon_type, attack_intensity, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Dodge', 'Failure', 'Unarmed', NULL, NULL, 'You try to dodge but aren''t fast enough. The attack connects! [Full damage]', 1.0, 1, '["Defensive", "Failed"]'),
('Dodge', 'Failure', 'Unarmed', NULL, NULL, 'Your reflexes fail you. You take the hit!', 1.0, 1, '["Defensive", "Slow"]'),
('Dodge', 'Failure', 'Unarmed', NULL, 'TightQuarters', 'You stumble while trying to evade. The blow catches you off-guard!', 1.0, 1, '["Defensive", "Cramped"]'),
('Dodge', 'Failure', 'Unarmed', NULL, 'Hazardous', 'You dodge the attack but back into the wall! [No damage from attack, but stunned 1 turn]', 0.7, 1, '["Defensive", "Environmental"]'),
('Dodge', 'Failure', 'Unarmed', NULL, 'Hazardous', 'Evading the blow, you step on unstable ground and nearly fall! [Disadvantage next turn]', 0.7, 1, '["Defensive", "Unstable"]'),
('Dodge', 'Failure', 'Unarmed', NULL, 'Hazardous', 'You dodge right into the flames! [Avoid attack, take environmental damage]', 0.5, 1, '["Defensive", "Hazard"]');

-- ==============================================================================
-- CATEGORY 2: COMBAT STANCE DESCRIPTORS (30 total)
-- ==============================================================================

-- AGGRESSIVE STANCE (6)
-- ==================== Entering Aggressive Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Aggressive', 'Entering', NULL, NULL, NULL, 'You shift into an aggressive stance, weapon raised for maximum offense.', 1.0, 1, '["Tactical", "Offensive"]'),
('Aggressive', 'Entering', NULL, NULL, NULL, 'You abandon caution, focusing entirely on attack.', 1.0, 1, '["Tactical", "Risky"]'),
('Aggressive', 'Entering', NULL, NULL, 'TwoHanded', 'Your posture becomes predatory, ready to strike at any opening.', 1.0, 1, '["Tactical", "Threatening"]'),
('Aggressive', 'Entering', NULL, 'Winning', NULL, 'You commit to the offense, defenses be damned.', 1.0, 1, '["Tactical", "Confident"]');

-- ==================== Maintaining Aggressive Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Aggressive', 'Maintaining', NULL, NULL, NULL, 'You press the attack relentlessly!', 1.0, 1, '["Tactical", "Persistent"]'),
('Aggressive', 'Maintaining', NULL, NULL, NULL, 'Your aggressive stance leaves you open, but you strike with devastating force!', 1.0, 1, '["Tactical", "Powerful"]'),
('Aggressive', 'Maintaining', NULL, NULL, NULL, 'Every swing is meant to kill. Defense is secondary.', 1.0, 1, '["Tactical", "Brutal"]');

-- DEFENSIVE STANCE (6)
-- ==================== Entering Defensive Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Defensive', 'Entering', NULL, NULL, NULL, 'You settle into a defensive posture, weapon ready, guard unbreakable.', 1.0, 1, '["Tactical", "Protective"]'),
('Defensive', 'Entering', NULL, NULL, 'ShieldAndWeapon', 'You prioritize protection, your stance low and stable.', 1.0, 1, '["Tactical", "Stable"]'),
('Defensive', 'Entering', NULL, NULL, NULL, 'You become a wall. Let them come.', 1.0, 1, '["Tactical", "Resolute"]'),
('Defensive', 'Entering', NULL, 'Losing', NULL, 'You shift to a defensive stance, sacrificing offense for survival.', 1.0, 1, '["Tactical", "Survival"]');

-- ==================== Maintaining Defensive Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Defensive', 'Maintaining', NULL, NULL, NULL, 'You weather their attacks, shield and armor taking the brunt.', 1.0, 1, '["Tactical", "Enduring"]'),
('Defensive', 'Maintaining', NULL, NULL, NULL, 'Your defensive stance holds firm against the onslaught.', 1.0, 1, '["Tactical", "Unyielding"]'),
('Defensive', 'Maintaining', NULL, 'Outnumbered', NULL, 'You give no ground. Every attack is met with stalwart defense.', 1.0, 1, '["Tactical", "Steadfast"]');

-- BALANCED STANCE (4)
-- ==================== Entering Balanced Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Balanced', 'Entering', NULL, NULL, NULL, 'You adopt a balanced stance, ready to attack or defend as needed.', 1.0, 1, '["Tactical", "Versatile"]'),
('Balanced', 'Entering', NULL, NULL, NULL, 'You center yourself, finding equilibrium between offense and defense.', 1.0, 1, '["Tactical", "Centered"]'),
('Balanced', 'Entering', NULL, 'EvenMatch', NULL, 'Your stance is neutral, adaptable to any situation.', 1.0, 1, '["Tactical", "Adaptive"]');

-- ==================== Maintaining Balanced Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Balanced', 'Maintaining', NULL, NULL, NULL, 'You maintain your balance, attacking and defending with equal measure.', 1.0, 1, '["Tactical", "Measured"]'),
('Balanced', 'Maintaining', NULL, NULL, NULL, 'Your versatile stance allows you to adapt to the flow of combat.', 1.0, 1, '["Tactical", "Flexible"]');

-- RECKLESS STANCE (6)
-- ==================== Entering Reckless Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Reckless', 'Entering', NULL, NULL, NULL, 'You throw caution to the wind, committing to all-out assault!', 1.0, 1, '["Tactical", "Dangerous", "Aggressive"]'),
('Reckless', 'Entering', NULL, NULL, NULL, 'Survival be damned—you''re going for maximum damage!', 1.0, 1, '["Tactical", "Risky", "Devastating"]'),
('Reckless', 'Entering', NULL, 'Losing', 'TwoHanded', 'You abandon all defense in favor of overwhelming offense!', 1.0, 1, '["Tactical", "Desperate"]');

-- ==================== Maintaining Reckless Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Reckless', 'Maintaining', NULL, NULL, NULL, 'You trade blows without regard for your own safety!', 1.0, 1, '["Tactical", "Fearless"]'),
('Reckless', 'Maintaining', NULL, NULL, NULL, 'Your attacks are devastating but leave you completely exposed!', 1.0, 1, '["Tactical", "Reckless"]'),
('Reckless', 'Maintaining', NULL, NULL, NULL, 'You''re a whirlwind of violence, heedless of danger!', 1.0, 1, '["Tactical", "Furious"]');

-- EVASIVE STANCE (4)
-- ==================== Entering Evasive Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Evasive', 'Entering', NULL, NULL, 'Unarmed', 'You shift to an evasive stance, prioritizing mobility above all.', 1.0, 1, '["Tactical", "Mobile"]'),
('Evasive', 'Entering', NULL, 'Outnumbered', NULL, 'You adopt a defensive posture focused on avoiding hits rather than blocking them.', 1.0, 1, '["Tactical', 'Survival"]');

-- ==================== Maintaining Evasive Stance ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Evasive', 'Maintaining', NULL, NULL, NULL, 'You stay light on your feet, dodging and weaving between attacks.', 1.0, 1, '["Tactical", "Agile"]'),
('Evasive', 'Maintaining', NULL, NULL, NULL, 'Your evasive movements make you a difficult target.', 1.0, 1, '["Tactical", "Elusive"]');

-- STANCE SWITCHING (4)
-- ==================== Switching Stances ====================

INSERT INTO Combat_Stance_Descriptors (stance_type, description_moment, previous_stance, situation_context, weapon_configuration, descriptor_text, weight, is_active, tags)
VALUES
('Aggressive', 'Switching', 'Defensive', 'Winning', NULL, 'You shift from defense to offense, sensing weakness!', 1.2, 1, '["Tactical", "Opportunistic"]'),
('Defensive', 'Switching', 'Aggressive', 'Losing', NULL, 'You fall back to a defensive stance, reassessing the situation.', 1.2, 1, '["Tactical", "Prudent"]'),
('Reckless', 'Switching', 'Balanced', 'Losing', NULL, 'Desperate times call for desperate measures! You abandon all caution!', 1.2, 1, '["Tactical", "Desperate"]'),
('Balanced', 'Switching', 'Reckless', NULL, NULL, 'You reign in your aggression, returning to a more measured approach.', 1.2, 1, '["Tactical", "Controlled"]');

-- ==============================================================================
-- CATEGORY 3: CRITICAL HIT DESCRIPTORS (25 total)
-- ==============================================================================

-- MELEE CRITICAL HITS (12)
-- ==================== Slashing Weapons ====================

INSERT INTO Combat_Critical_Hit_Descriptors (attack_category, damage_type, weapon_or_spell_type, target_type, special_effect, descriptor_text, weight, is_active, tags)
VALUES
('Melee', 'Slashing', 'Sword', NULL, 'Bleeding', 'Your blade finds the perfect angle—it carves through armor and flesh like butter! [Maximum damage + bleeding]', 1.5, 1, '["Devastating", "Lethal"]'),
('Melee', 'Slashing', 'Axe', NULL, 'Bleeding', 'A devastating slash! Your weapon opens a grievous wound! [Double damage]', 1.5, 1, '["Brutal", "Bloody"]'),
('Melee', 'Slashing', 'Sword', 'Humanoid', NULL, 'You strike with surgical precision, finding a gap in their defenses! [Critical damage + weakened status]', 1.5, 1, '["Skillful", "Precise"]'),
('Melee', 'Slashing', NULL, NULL, 'Bleeding', 'Your blade bites deep. They stagger, blood pouring from the wound!', 1.5, 1, '["Devastating", "Dramatic"]');

-- ==================== Crushing Weapons ====================

INSERT INTO Combat_Critical_Hit_Descriptors (attack_category, damage_type, weapon_or_spell_type, target_type, special_effect, descriptor_text, weight, is_active, tags)
VALUES
('Melee', 'Crushing', 'Hammer', NULL, 'Stunned', 'The impact is catastrophic! Bones shatter under your weapon''s weight! [Maximum damage + stunned]', 1.5, 1, '["Devastating", "Brutal"]'),
('Melee', 'Crushing', 'Hammer', 'Humanoid', 'ArmorDestroyed', 'You pulverize their defenses! The crunch of breaking armor echoes! [Armor destroyed, massive damage]', 1.5, 1, '["Overwhelming", "Equipment"]'),
('Melee', 'Crushing', 'Hammer', NULL, 'Prone', 'A crushing blow that would fell an ox! They crumple! [Double damage + prone]', 1.5, 1, '["Powerful", "Knockdown"]'),
('Melee', 'Crushing', NULL, 'Construct', NULL, 'You cave in their chassis/armor with devastating force!', 1.5, 1, '["Mechanical", "Destructive"]');

-- ==================== Piercing Weapons ====================

INSERT INTO Combat_Critical_Hit_Descriptors (attack_category, damage_type, weapon_or_spell_type, target_type, special_effect, descriptor_text, weight, is_active, tags)
VALUES
('Melee', 'Piercing', 'Spear', 'Humanoid', 'Dying', 'You drive your weapon through a vital point! [Maximum damage + critical bleeding]', 1.5, 1, '["Lethal", "Precise"]'),
('Melee', 'Piercing', 'Dagger', 'Humanoid', 'InstantKill', 'Perfect thrust! Your weapon finds the heart! [Instant death on weak enemies, massive damage otherwise]', 1.7, 1, '["Lethal", "Assassin"]'),
('Melee', 'Piercing', NULL, NULL, NULL, 'You punch through their defenses completely! [Ignore armor, critical damage]', 1.5, 1, '["Armor-Piercing", "Devastating"]'),
('Melee', 'Piercing', 'Spear', NULL, NULL, 'Your weapon slides between armor plates with deadly accuracy!', 1.5, 1, '["Precise", "Skillful"]');

-- RANGED CRITICAL HITS (5)
-- ==================== Bows and Crossbows ====================

INSERT INTO Combat_Critical_Hit_Descriptors (attack_category, damage_type, weapon_or_spell_type, target_type, special_effect, descriptor_text, weight, is_active, tags)
VALUES
('Ranged', 'Piercing', 'Bow', 'Humanoid', 'InstantKill', 'The arrow strikes true—right through the eye! [Instant kill on weak enemies]', 1.7, 1, '["Lethal", "Marksmanship"]'),
('Ranged', 'Piercing', 'Bow', NULL, 'Dying', 'Your shot pierces a vital organ! They collapse immediately! [Maximum damage + dying status]', 1.5, 1, '["Deadly", "Accurate"]'),
('Ranged', 'Piercing', 'Crossbow', NULL, NULL, 'Perfect marksmanship! The bolt finds its mark with lethal precision! [Triple damage]', 1.5, 1, '["Spectacular", "Devastating"]'),
('Ranged', 'Piercing', 'Bow', 'Beast', NULL, 'Your arrow penetrates completely, emerging from the other side!', 1.5, 1, '["Brutal", "Powerful"]'),
('Ranged', 'Piercing', 'Crossbow', 'Construct', NULL, 'The bolt punches through metal plating like parchment!', 1.5, 1, '["Armor-Piercing"]');

-- MAGIC CRITICAL HITS (8)
-- ==================== Fire Magic ====================

INSERT INTO Combat_Critical_Hit_Descriptors (attack_category, damage_type, weapon_or_spell_type, target_type, special_effect, descriptor_text, weight, is_active, tags)
VALUES
('Magic', 'Fire', 'Fire', NULL, 'Burning', 'Your Galdr resonates perfectly! The target ignites like a torch! [Maximum damage + severe burning]', 1.5, 1, '["Magical", "Devastating"]'),
('Magic', 'Fire', 'Fire', NULL, 'InstantKill', 'The fire doesn''t just burn—it consumes! Nothing remains but ash!', 1.7, 1, '["Lethal", "Spectacular"]'),
('Magic', 'Fire', 'Fire', NULL, 'Burning', 'Fehu answers with terrible power! Immolation is instant!', 1.5, 1, '["Runic", "Overwhelming"]');

-- ==================== Ice Magic ====================

INSERT INTO Combat_Critical_Hit_Descriptors (attack_category, damage_type, weapon_or_spell_type, target_type, special_effect, descriptor_text, weight, is_active, tags)
VALUES
('Magic', 'Ice', 'Ice', NULL, 'InstantKill', 'Your frost magic crystallizes them instantly! They shatter like glass! [Instant kill]', 1.7, 1, '["Lethal", "Spectacular"]'),
('Magic', 'Ice', 'Ice', NULL, 'Frozen', 'Perfect cold! Their body freezes solid! [Maximum damage + frozen status]', 1.5, 1, '["Devastating", "Control"]'),
('Magic', 'Ice', 'Ice', NULL, 'Frozen', 'Þurisaz''s wrath incarnate! They''re encased in ice!', 1.5, 1, '["Runic", "Powerful"]');

-- ==================== Lightning Magic ====================

INSERT INTO Combat_Critical_Hit_Descriptors (attack_category, damage_type, weapon_or_spell_type, target_type, special_effect, descriptor_text, weight, is_active, tags)
VALUES
('Magic', 'Lightning', 'Lightning', NULL, NULL, 'The lightning doesn''t just strike—it chains! Multiple targets convulse! [Damage to all nearby enemies]', 1.7, 1, '["Area", "Devastating"]'),
('Magic', 'Lightning', 'Lightning', 'Humanoid', 'Paralyzed', 'Ansuz''s fury courses through them! Their nervous system fries! [Maximum damage + paralyzed]', 1.5, 1, '["Runic", "Lethal"]'),
('Magic', 'Lightning', 'Lightning', NULL, 'Dying', 'The electrical discharge is so powerful, their heart stops!', 1.5, 1, '["Lethal", "Brutal"]');

-- ==============================================================================
-- CATEGORY 4: FUMBLE DESCRIPTORS (25 total)
-- ==============================================================================

-- ATTACK FUMBLES (10)
-- ==================== Weapon Fumbles ====================

INSERT INTO Combat_Fumble_Descriptors (fumble_category, fumble_type, equipment_type, severity, environment_factor, descriptor_text, weight, is_active, tags)
VALUES
('AttackFumble', 'Overextension', NULL, 'Minor', NULL, 'Your swing goes wide—you overextend and stumble! [Lose 1 AP, enemy gets advantage]', 1.0, 1, '["Embarrassing", "Recoverable"]'),
('AttackFumble', 'WeaponDrop', 'Sword', 'Moderate', NULL, 'Your weapon slips from your grip! [Weapon drops, must use action to retrieve]', 0.8, 1, '["Costly", "Equipment"]'),
('AttackFumble', 'Miss', NULL, 'Minor', NULL, 'You strike at empty air, throwing yourself completely off-balance! [Prone, defense reduced]', 1.0, 1, '["Embarrassing"]'),
('AttackFumble', 'Overextension', NULL, 'Moderate', NULL, 'Your attack is so poorly executed, you leave yourself wide open! [Enemy gets free attack]', 0.9, 1, '["Dangerous", "Punishing"]'),
('AttackFumble', 'WeaponBreak', 'Sword', 'Catastrophic', NULL, 'Your weapon strikes at a bad angle and SNAPS! [Weapon destroyed]', 0.3, 1, '["Catastrophic", "Equipment"]'),
('AttackFumble', 'WeaponBreak', NULL, 'Severe', NULL, 'The strain of combat proves too much—your weapon shatters! [Disarmed, weapon unusable]', 0.4, 1, '["Disaster", "Equipment"]'),
('AttackFumble', 'WeaponBreak', 'Sword', 'Catastrophic', NULL, 'Your blade breaks against their armor! You''re left holding a hilt!', 0.3, 1, '["Catastrophic", "Disarmed"]'),
('AttackFumble', 'SelfInjury', NULL, 'Moderate', NULL, 'You fumble your attack and cut yourself! [1d6 self-damage]', 0.7, 1, '["Embarrassing", "Painful"]'),
('AttackFumble', 'SelfInjury', NULL, 'Severe', NULL, 'Your weapon rebounds off their armor and strikes your own leg! [1d8 damage, slowed]', 0.5, 1, '["Dangerous", "Debilitating"]'),
('AttackFumble', 'SelfInjury', NULL, 'Severe', NULL, 'Catastrophic failure! Your weapon''s backswing hits you! [2d6 self-damage]', 0.5, 1, '["Catastrophic", "Painful"]');

-- MAGIC FUMBLES (8)
-- ==================== Blight Corruption Surge ====================

INSERT INTO Combat_Fumble_Descriptors (fumble_category, fumble_type, equipment_type, severity, environment_factor, descriptor_text, weight, is_active, tags)
VALUES
('MagicFumble', 'CorruptionSurge', 'Staff', 'Severe', NULL, 'Your Galdr falters—the Blight warps the spell! Paradoxical energy erupts! [+10 Psychic Stress, random effect]', 0.6, 1, '["Dangerous", "Corruption"]'),
('MagicFumble', 'Backfire', NULL, 'Moderate', NULL, 'The rune inverts! The spell backfires catastrophically! [Take spell damage yourself]', 0.7, 1, '["Painful", "Embarrassing"]'),
('MagicFumble', 'CorruptionSurge', 'Staff', 'Catastrophic', NULL, 'Reality rejects your magic! The All-Rune interferes! [+15 Psychic Stress, spell fails, Corruption +5]', 0.3, 1, '["Catastrophic", "Corruption"]');

-- ==================== Wild Magic Surge (Alfheim) ====================

INSERT INTO Combat_Fumble_Descriptors (fumble_category, fumble_type, equipment_type, severity, environment_factor, descriptor_text, weight, is_active, tags)
VALUES
('MagicFumble', 'WildSurge', NULL, 'Moderate', NULL, 'The spell goes wrong in impossible ways! [Random magical effect from table]', 0.7, 1, '["Chaotic", "Unpredictable"]'),
('MagicFumble', 'WildSurge', NULL, 'Severe', NULL, 'Your Galdr succeeds... but also creates a Reality Tear! [Spell works + Glitch spawns]', 0.4, 1, '["Dangerous", "Corruption"]'),
('MagicFumble', 'WildSurge', NULL, 'Severe', NULL, 'Magic spirals out of control! Everyone nearby is affected!', 0.5, 1, '["Area", "Chaotic"]');

-- ==================== Spell Burnout ====================

INSERT INTO Combat_Fumble_Descriptors (fumble_category, fumble_type, equipment_type, severity, environment_factor, descriptor_text, weight, is_active, tags)
VALUES
('MagicFumble', 'Burnout', 'Staff', 'Severe', NULL, 'The magical strain is too much! Your mind reels! [Stunned 2 turns, +8 Psychic Stress]', 0.5, 1, '["Debilitating", "Psychic"]'),
('MagicFumble', 'Burnout', NULL, 'Catastrophic', NULL, 'You push too hard—the Galdr sears your consciousness! [Cannot cast for 3 turns, +12 Stress]', 0.3, 1, '["Catastrophic", "Debilitating"]');

-- DEFENSIVE FUMBLES (7)
-- ==================== Shield Fumbles ====================

INSERT INTO Combat_Fumble_Descriptors (fumble_category, fumble_type, equipment_type, severity, environment_factor, descriptor_text, weight, is_active, tags)
VALUES
('DefensiveFumble', 'Exposed', 'Shield', 'Moderate', NULL, 'You raise your shield at the wrong moment—it''s knocked aside! [Take double damage]', 0.8, 1, '["Dangerous", "Mistimed"]'),
('DefensiveFumble', 'ShieldDrop', 'Shield', 'Moderate', NULL, 'You lose your grip on your shield! [Shield drops, must retrieve]', 0.7, 1, '["Equipment", "Recoverable"]'),
('DefensiveFumble', 'ShieldDrop', 'Shield', 'Severe', 'TightQuarters', 'Your shield gets stuck in the environment! [Shield unusable until freed]', 0.5, 1, '["Environmental", "Problematic"]');

-- ==================== Dodge Fumbles ====================

INSERT INTO Combat_Fumble_Descriptors (fumble_category, fumble_type, equipment_type, severity, environment_factor, descriptor_text, weight, is_active, tags)
VALUES
('DefensiveFumble', 'Exposed', NULL, 'Severe', NULL, 'You dodge directly into their follow-up attack! [Take extra damage]', 0.6, 1, '["Dangerous", "Painful"]'),
('DefensiveFumble', 'Tripped', NULL, 'Moderate', 'Slippery', 'You trip while trying to evade! [Prone, defense severely reduced]', 0.7, 1, '["Embarrassing", "Vulnerable"]'),
('DefensiveFumble', 'Tripped', NULL, 'Moderate', NULL, 'You dodge backward and fall! [Prone, lose next turn]', 0.7, 1, '["Embarrassing", "Debilitating"]');

-- ==================== Parry Fumbles ====================

INSERT INTO Combat_Fumble_Descriptors (fumble_category, fumble_type, equipment_type, severity, environment_factor, descriptor_text, weight, is_active, tags)
VALUES
('DefensiveFumble', 'Disarmed', 'Sword', 'Severe', NULL, 'Your parry is mistimed—their weapon disarms you! [Weapon knocked away]', 0.5, 1, '["Dangerous", "Equipment"]'),
('DefensiveFumble', 'Exposed', NULL, 'Severe', NULL, 'You parry so badly, you expose yourself completely! [Enemy gets critical hit opportunity]', 0.5, 1, '["Catastrophic", "Vulnerable"]'),
('DefensiveFumble', 'Disarmed', NULL, 'Moderate', NULL, 'Your weapon is knocked from your hands! [Disarmed]', 0.7, 1, '["Equipment", "Recoverable"]');

-- ==============================================================================
-- CATEGORY 5: COMBAT MANEUVER DESCRIPTORS (20 total)
-- ==============================================================================

-- RIPOSTE (4)
-- ==================== Successful Riposte ====================

INSERT INTO Combat_Maneuver_Descriptors (maneuver_type, outcome_type, weapon_type, target_type, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Riposte', 'Success', 'Sword', NULL, NULL, 'You parry and immediately counter-strike! Your blade finds its mark!', 1.2, 1, '["Skillful", "Counter"]'),
('Riposte', 'Success', 'Dagger', 'Humanoid', NULL, 'In one fluid motion, you deflect their attack and strike back!', 1.2, 1, '["Skillful", "Quick"]'),
('Riposte', 'CriticalSuccess', 'Sword', NULL, NULL, 'Perfect timing! Your riposte catches them completely off-guard!', 1.5, 1, '["Masterful", "Devastating"]');

-- ==================== Failed Riposte ====================

INSERT INTO Combat_Maneuver_Descriptors (maneuver_type, outcome_type, weapon_type, target_type, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Riposte', 'Failure', NULL, NULL, NULL, 'You attempt a riposte but they''re too quick! Your counter misses!', 1.0, 1, '["Failed", "Mistimed"]'),
('Riposte', 'Failure', 'Sword', 'Humanoid', NULL, 'Your counter-attack is predictable. They evade easily.', 1.0, 1, '["Failed", "Telegraphed"]');

-- DISARM (4)
-- ==================== Successful Disarm ====================

INSERT INTO Combat_Maneuver_Descriptors (maneuver_type, outcome_type, weapon_type, target_type, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Disarm', 'Success', 'Sword', 'Humanoid', NULL, 'You strike their weapon hand! Their weapon flies away! [Enemy disarmed]', 1.2, 1, '["Tactical", "Effective"]'),
('Disarm', 'Success', NULL, 'Humanoid', NULL, 'A precise blow knocks their weapon loose! It clatters to the ground!', 1.2, 1, '["Skillful", "Tactical"]'),
('Disarm', 'CriticalSuccess', 'Sword', 'Humanoid', NULL, 'You twist their weapon from their grip! They''re unarmed!', 1.5, 1, '["Masterful", "Decisive"]');

-- ==================== Failed Disarm ====================

INSERT INTO Combat_Maneuver_Descriptors (maneuver_type, outcome_type, weapon_type, target_type, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Disarm', 'Failure', NULL, 'Humanoid', NULL, 'They hold onto their weapon despite your attempt!', 1.0, 1, '["Failed", "Resilient"]'),
('Disarm', 'Failure', 'Sword', NULL, NULL, 'Your disarm fails—their grip is too strong!', 1.0, 1, '["Failed"]');

-- TRIP/KNOCKDOWN (3)
-- ==================== Successful Trip ====================

INSERT INTO Combat_Maneuver_Descriptors (maneuver_type, outcome_type, weapon_type, target_type, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Trip', 'Success', 'Unarmed', 'Humanoid', NULL, 'You sweep their legs! They crash to the ground! [Enemy prone]', 1.2, 1, '["Tactical", "Effective"]'),
('Trip', 'Success', NULL, 'Humanoid', NULL, 'Your attack takes out their footing! They fall hard!', 1.2, 1, '["Skillful", "Knockdown"]'),
('Trip', 'CriticalSuccess', 'Unarmed', NULL, NULL, 'They topple! You''ve knocked them down!', 1.5, 1, '["Effective", "Control"]');

-- FAILED TRIP (2)

INSERT INTO Combat_Maneuver_Descriptors (maneuver_type, outcome_type, weapon_type, target_type, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Trip', 'Failure', NULL, 'Humanoid', NULL, 'They maintain their balance despite your attempt!', 1.0, 1, '["Failed", "Stable"]'),
('Trip', 'Failure', 'Unarmed', NULL, NULL, 'You try to trip them but they sidestep!', 1.0, 1, '["Failed", "Evasive"]');

-- GRAPPLE (4)
-- ==================== Successful Grapple ====================

INSERT INTO Combat_Maneuver_Descriptors (maneuver_type, outcome_type, weapon_type, target_type, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Grapple', 'Success', 'Unarmed', 'Humanoid', NULL, 'You seize them in a hold! They struggle but can''t break free! [Enemy grappled]', 1.2, 1, '["Control", "Effective"]'),
('Grapple', 'Success', 'Unarmed', 'Humanoid', NULL, 'You wrestle them to the ground! They''re pinned!', 1.2, 1, '["Control", "Dominant"]'),
('Grapple', 'CriticalSuccess', 'Unarmed', 'Humanoid', NULL, 'You lock them in a chokehold! [Grappled, taking suffocation damage]', 1.5, 1, '["Lethal", "Control"]');

-- ==================== Failed Grapple ====================

INSERT INTO Combat_Maneuver_Descriptors (maneuver_type, outcome_type, weapon_type, target_type, environment_context, descriptor_text, weight, is_active, tags)
VALUES
('Grapple', 'Failure', 'Unarmed', NULL, NULL, 'They slip out of your grasp!', 1.0, 1, '["Failed", "Evasive"]'),
('Grapple', 'Failure', 'Unarmed', 'Humanoid', NULL, 'You can''t get a hold on them!', 1.0, 1, '["Failed"]'),
('Grapple', 'Failure', 'Unarmed', 'Beast', NULL, 'They''re too quick—your grapple fails!', 1.0, 1, '["Failed", "Agile"]');

-- ==============================================================================
-- DATA POPULATION COMPLETE
-- ==============================================================================
-- Total Descriptors Inserted: 140
-- - Defensive Actions: 40
-- - Combat Stances: 30
-- - Critical Hits: 25
-- - Fumbles: 25
-- - Combat Maneuvers: 20
-- ==============================================================================
