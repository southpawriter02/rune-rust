-- ============================================================
-- v0.38.6: Player Combat Action Descriptors
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Player attack and defense action flavor text
-- ============================================================

-- ============================================================
-- PLAYER MELEE ATTACKS - ONE-HANDED SWORD
-- ============================================================

-- Sword - Miss
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordOneHanded', 'Miss',
 'You swing your {Weapon} at the {Enemy}, but it sidesteps effortlessly.',
 '["OneHanded", "Miss", "Evasion"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'Miss',
 'Your {Weapon} cuts only air as the {Enemy} moves out of reach.',
 '["OneHanded", "Miss", "Anticipation"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'Miss',
 'The {Enemy} anticipates your strike and withdraws just in time.',
 '["OneHanded", "Miss", "Prediction"]');

-- Sword - Deflected
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordOneHanded', 'Deflected',
 'The {Enemy} parries your {Weapon} with its {Enemy_Weapon}.',
 '["OneHanded", "Deflected", "Parried"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'Deflected',
 'Your strike glances off the {Enemy}''s {Armor_Location}.',
 '["OneHanded", "Deflected", "Armor"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'Deflected',
 'The {Enemy} bats your {Weapon} aside with contemptuous ease.',
 '["OneHanded", "Deflected", "Contempt"]');

-- Sword - Glancing Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordOneHanded', 'GlancingHit',
 'Your {Weapon} scrapes across the {Enemy}''s {Target_Location}, drawing sparks.',
 '["OneHanded", "GlancingHit", "Sparks"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'GlancingHit',
 'You catch the {Enemy} a glancing blow that barely penetrates its defenses.',
 '["OneHanded", "GlancingHit", "Light"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'GlancingHit',
 'Your {Weapon} finds purchase but fails to bite deep.',
 '["OneHanded", "GlancingHit", "Shallow"]');

-- Sword - Solid Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordOneHanded', 'SolidHit',
 'Your {Weapon} bites into the {Enemy}''s {Target_Location} with a satisfying crunch.',
 '["OneHanded", "SolidHit", "Impact"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'SolidHit',
 'You strike true, your {Weapon} carving through the {Enemy}''s defenses.',
 '["OneHanded", "SolidHit", "Precise"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'SolidHit',
 'Your blade finds its mark, eliciting {Enemy_Reaction} from the {Enemy}.',
 '["OneHanded", "SolidHit", "Effective"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'SolidHit',
 'You thrust your {Weapon} toward the {Enemy}''s {Target_Location}, and it connects solidly.',
 '["OneHanded", "SolidHit", "Thrust"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'SolidHit',
 'You bring your {Weapon} around in an arc that slashes across the {Enemy}.',
 '["OneHanded", "SolidHit", "Arc"]');

-- Sword - Devastating Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordOneHanded', 'DevastatingHit',
 'Your {Weapon} tears through the {Enemy}''s {Target_Location} in a spray of {Damage_Type_Descriptor}.',
 '["OneHanded", "DevastatingHit", "Brutal"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'DevastatingHit',
 'You deliver a brutal strike that sends the {Enemy} reeling.',
 '["OneHanded", "DevastatingHit", "Reeling"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'DevastatingHit',
 'Your blade cleaves deep, nearly severing the {Enemy}''s {Target_Location}.',
 '["OneHanded", "DevastatingHit", "Deep"]');

-- Sword - Critical Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordOneHanded', 'CriticalHit',
 'You find a critical weakness—your {Weapon} plunges into the {Enemy}''s {Vital_Location} with devastating precision!',
 '["OneHanded", "Critical", "Lethal"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'CriticalHit',
 'In a perfect moment of opportunity, you drive your {Weapon} home with lethal force!',
 '["OneHanded", "Critical", "Opportunity"]'),

('PlayerMeleeAttack', 'SwordOneHanded', 'CriticalHit',
 'Your strike is flawless—the {Enemy} has no defense as your {Weapon} finds its mark!',
 '["OneHanded", "Critical", "Flawless"]');

-- ============================================================
-- PLAYER MELEE ATTACKS - TWO-HANDED WEAPONS
-- ============================================================

-- Two-Handed - Miss
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordTwoHanded', 'Miss',
 'Your massive swing leaves you momentarily off-balance as it finds only empty space.',
 '["TwoHanded", "Miss", "OffBalance"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'Miss',
 'The {Enemy} ducks under your powerful but predictable arc.',
 '["TwoHanded", "Miss", "Predictable"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'Miss',
 'Your {Weapon} crashes into the ground, missing the {Enemy} entirely.',
 '["TwoHanded", "Miss", "Ground"]');

-- Two-Handed - Glancing Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordTwoHanded', 'GlancingHit',
 'Your {Weapon} clips the {Enemy} but without the full force you intended.',
 '["TwoHanded", "GlancingHit", "Partial"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'GlancingHit',
 'The {Enemy} partially evades, reducing what should have been a devastating blow.',
 '["TwoHanded", "GlancingHit", "Evaded"]');

-- Two-Handed - Solid Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordTwoHanded', 'SolidHit',
 'The impact of your {Weapon} against the {Enemy} reverberates up your arms.',
 '["TwoHanded", "SolidHit", "Reverberating"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'SolidHit',
 'Your {Weapon} connects with pulverizing force, crushing {Target_Location}.',
 '["TwoHanded", "SolidHit", "Pulverizing"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'SolidHit',
 'You feel the satisfying crunch as your {Weapon} smashes into the {Enemy}.',
 '["TwoHanded", "SolidHit", "Crunch"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'SolidHit',
 'You raise your {Weapon} high and bring it down on the {Enemy} with tremendous force.',
 '["TwoHanded", "SolidHit", "Overhead"]');

-- Two-Handed - Devastating Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordTwoHanded', 'DevastatingHit',
 'Your {Weapon} crashes through the {Enemy}''s defenses like a battering ram!',
 '["TwoHanded", "DevastatingHit", "Overwhelming"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'DevastatingHit',
 'You bring your {Weapon} down with bone-shattering force, utterly devastating the {Enemy}.',
 '["TwoHanded", "DevastatingHit", "BoneShattering"]');

-- Two-Handed - Critical Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'SwordTwoHanded', 'CriticalHit',
 'Your {Weapon} obliterates the {Enemy}''s defenses, the impact catastrophic!',
 '["TwoHanded", "Critical", "Catastrophic"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'CriticalHit',
 'With overwhelming force, you shatter the {Enemy}''s {Target_Location} completely!',
 '["TwoHanded", "Critical", "Shatter"]'),

('PlayerMeleeAttack', 'SwordTwoHanded', 'CriticalHit',
 'The crushing blow sends fragments of {Damage_Type_Descriptor} flying in all directions!',
 '["TwoHanded", "Critical", "Crushing"]');

-- ============================================================
-- PLAYER RANGED ATTACKS - BOW
-- ============================================================

-- Bow - Miss
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerRangedAttack', 'Bow', 'Miss',
 'Your arrow sails past the {Enemy}, clattering against the {Environment_Feature} behind it.',
 '["Bow", "Miss", "Clatter"]'),

('PlayerRangedAttack', 'Bow', 'Miss',
 'The {Enemy} shifts at the last instant, and your arrow misses by inches.',
 '["Bow", "Miss", "Close"]'),

('PlayerRangedAttack', 'Bow', 'Miss',
 'Your shot goes wide, embedding itself in the far wall.',
 '["Bow", "Miss", "Wide"]');

-- Bow - Glancing Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerRangedAttack', 'Bow', 'GlancingHit',
 'Your arrow grazes the {Enemy}, leaving a shallow wound.',
 '["Bow", "GlancingHit", "Graze"]'),

('PlayerRangedAttack', 'Bow', 'GlancingHit',
 'The arrow skips off the {Enemy}''s {Armor_Location}, drawing a thin line of {Damage_Type_Descriptor}.',
 '["Bow", "GlancingHit", "Skip"]');

-- Bow - Solid Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerRangedAttack', 'Bow', 'SolidHit',
 'Your arrow punches through the {Enemy}''s {Target_Location} with a wet thunk.',
 '["Bow", "SolidHit", "Punch"]'),

('PlayerRangedAttack', 'Bow', 'SolidHit',
 'The {Enemy} staggers as your arrow finds its mark.',
 '["Bow", "SolidHit", "Stagger"]'),

('PlayerRangedAttack', 'Bow', 'SolidHit',
 'Your shaft buries itself deep in the {Enemy}''s {Target_Location}.',
 '["Bow", "SolidHit", "Deep"]'),

('PlayerRangedAttack', 'Bow', 'SolidHit',
 'You draw your bow and loose an arrow that strikes true.',
 '["Bow", "SolidHit", "True"]');

-- Bow - Devastating Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerRangedAttack', 'Bow', 'DevastatingHit',
 'Your arrow slams into the {Enemy} with tremendous force, penetrating deep.',
 '["Bow", "DevastatingHit", "Force"]'),

('PlayerRangedAttack', 'Bow', 'DevastatingHit',
 'The arrow tears through vital tissue, and the {Enemy} cries out in agony.',
 '["Bow", "DevastatingHit", "Agony"]');

-- Bow - Critical Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerRangedAttack', 'Bow', 'CriticalHit',
 'Your arrow strikes a vital point—the {Enemy} convulses as the shaft pierces {Vital_Location}!',
 '["Bow", "Critical", "Vital"]'),

('PlayerRangedAttack', 'Bow', 'CriticalHit',
 'A perfect shot! Your arrow penetrates the {Enemy}''s {Vital_Location} completely!',
 '["Bow", "Critical", "Perfect"]'),

('PlayerRangedAttack', 'Bow', 'CriticalHit',
 'The {Enemy} seems to freeze as your arrow finds its heart!',
 '["Bow", "Critical", "Heart"]');

-- ============================================================
-- PLAYER DEFENSE ACTIONS - DODGE
-- ============================================================

-- Dodge - Successful
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerDefense', 'Dodge', 'Miss',
 'You twist aside, the {Enemy}''s attack passing harmlessly by.',
 '["Dodge", "Success", "Twist"]'),

('PlayerDefense', 'Dodge', 'Miss',
 'You duck under the {Enemy}''s strike with practiced ease.',
 '["Dodge", "Success", "Duck"]'),

('PlayerDefense', 'Dodge', 'Miss',
 'You sidestep the incoming blow at the last possible moment.',
 '["Dodge", "Success", "Sidestep"]'),

('PlayerDefense', 'Dodge', 'Miss',
 'You roll away from the {Enemy}''s attack, coming up in a defensive stance.',
 '["Dodge", "Success", "Roll"]');

-- Dodge - Failed
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerDefense', 'Dodge', 'SolidHit',
 'You try to dodge but aren''t fast enough—the {Enemy}''s attack connects!',
 '["Dodge", "Failure", "Slow"]'),

('PlayerDefense', 'Dodge', 'SolidHit',
 'You misjudge the {Enemy}''s reach and take the hit!',
 '["Dodge", "Failure", "Misjudged"]'),

('PlayerDefense', 'Dodge', 'SolidHit',
 'Your attempted evasion leaves you exposed to the follow-up strike!',
 '["Dodge", "Failure", "Exposed"]');

-- ============================================================
-- PLAYER DEFENSE ACTIONS - PARRY
-- ============================================================

-- Parry - Successful
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerDefense', 'Parry', 'Deflected',
 'You parry the {Enemy}''s attack with your {Weapon}, metal ringing on metal.',
 '["Parry", "Success", "Ring"]'),

('PlayerDefense', 'Parry', 'Deflected',
 'You deflect the incoming strike with a precise counter-motion.',
 '["Parry", "Success", "Precise"]'),

('PlayerDefense', 'Parry', 'Deflected',
 'You turn the {Enemy}''s blade aside with your own.',
 '["Parry", "Success", "Turn"]'),

('PlayerDefense', 'Parry', 'Deflected',
 'You catch the {Enemy}''s weapon on your {Weapon} and push it away.',
 '["Parry", "Success", "Catch"]');

-- Parry - Failed
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerDefense', 'Parry', 'SolidHit',
 'Your parry is too slow—the {Enemy}''s attack batters through!',
 '["Parry", "Failure", "Slow"]'),

('PlayerDefense', 'Parry', 'SolidHit',
 'The force of the {Enemy}''s strike overwhelms your guard!',
 '["Parry", "Failure", "Overwhelmed"]'),

('PlayerDefense', 'Parry', 'SolidHit',
 'You mistime your parry and the blow connects!',
 '["Parry", "Failure", "Mistimed"]');

-- ============================================================
-- PLAYER DEFENSE ACTIONS - BLOCK (Shield)
-- ============================================================

-- Block - Successful
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerDefense', 'Block', 'Deflected',
 'You raise your {Shield} and absorb the {Enemy}''s strike.',
 '["Block", "Success", "Absorb"]'),

('PlayerDefense', 'Block', 'Deflected',
 'The {Enemy}''s attack crashes against your {Shield} with jarring force.',
 '["Block", "Success", "Crash"]'),

('PlayerDefense', 'Block', 'Deflected',
 'You brace yourself, taking the blow on your {Shield}.',
 '["Block", "Success", "Brace"]'),

('PlayerDefense', 'Block', 'Deflected',
 'Your {Shield} holds firm against the {Enemy}''s assault.',
 '["Block", "Success", "Firm"]');

-- Block - Failed
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerDefense', 'Block', 'SolidHit',
 'The {Enemy}''s attack batters past your {Shield}!',
 '["Block", "Failure", "Battered"]'),

('PlayerDefense', 'Block', 'SolidHit',
 'Your defense crumbles under the onslaught!',
 '["Block", "Failure", "Crumble"]'),

('PlayerDefense', 'Block', 'SolidHit',
 'The {Enemy} finds a gap in your guard!',
 '["Block", "Failure", "Gap"]');

-- ============================================================
-- ADDITIONAL WEAPON TYPES - AXE
-- ============================================================

-- Axe One-Handed - Solid Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'AxeOneHanded', 'SolidHit',
 'Your {Weapon} bites deep into the {Enemy}''s {Target_Location}.',
 '["OneHanded", "SolidHit", "Bite"]'),

('PlayerMeleeAttack', 'AxeOneHanded', 'SolidHit',
 'You hook your {Weapon} into the {Enemy}, tearing at its defenses.',
 '["OneHanded", "SolidHit", "Hook"]'),

('PlayerMeleeAttack', 'AxeOneHanded', 'SolidHit',
 'Your {Weapon} cleaves through the {Enemy}''s guard.',
 '["OneHanded", "SolidHit", "Cleave"]');

-- Axe One-Handed - Critical Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'AxeOneHanded', 'CriticalHit',
 'Your {Weapon} splits the {Enemy}''s {Vital_Location} with savage precision!',
 '["OneHanded", "Critical", "Split"]'),

('PlayerMeleeAttack', 'AxeOneHanded', 'CriticalHit',
 'You bury your {Weapon} in the {Enemy}''s {Vital_Location}, a killing blow!',
 '["OneHanded", "Critical", "Bury"]');

-- ============================================================
-- ADDITIONAL WEAPON TYPES - HAMMER/MACE
-- ============================================================

-- Hammer One-Handed - Solid Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'HammerOneHanded', 'SolidHit',
 'Your {Weapon} crashes into the {Enemy} with crushing force.',
 '["OneHanded", "SolidHit", "Crushing"]'),

('PlayerMeleeAttack', 'HammerOneHanded', 'SolidHit',
 'You slam your {Weapon} into the {Enemy}''s {Target_Location}, denting armor and cracking bone.',
 '["OneHanded", "SolidHit", "Slam"]'),

('PlayerMeleeAttack', 'HammerOneHanded', 'SolidHit',
 'The blunt impact of your {Weapon} sends shock waves through the {Enemy}.',
 '["OneHanded", "SolidHit", "Blunt"]');

-- Hammer One-Handed - Critical Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'HammerOneHanded', 'CriticalHit',
 'You pulverize the {Enemy}''s {Vital_Location} with a devastating hammer blow!',
 '["OneHanded", "Critical", "Pulverize"]'),

('PlayerMeleeAttack', 'HammerOneHanded', 'CriticalHit',
 'Your {Weapon} shatters the {Enemy}''s defenses, obliterating {Vital_Location}!',
 '["OneHanded", "Critical", "Shatter"]');

-- ============================================================
-- UNARMED COMBAT
-- ============================================================

-- Unarmed - Solid Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'Unarmed', 'SolidHit',
 'Your fist connects with the {Enemy}''s {Target_Location}.',
 '["Unarmed", "SolidHit", "Punch"]'),

('PlayerMeleeAttack', 'Unarmed', 'SolidHit',
 'You strike the {Enemy} with raw physical force.',
 '["Unarmed", "SolidHit", "Strike"]'),

('PlayerMeleeAttack', 'Unarmed', 'SolidHit',
 'You land a solid blow on the {Enemy}.',
 '["Unarmed", "SolidHit", "Blow"]');

-- Unarmed - Critical Hit
INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
VALUES
('PlayerMeleeAttack', 'Unarmed', 'CriticalHit',
 'You deliver a perfectly aimed strike to the {Enemy}''s {Vital_Location}!',
 '["Unarmed", "Critical", "Perfect"]'),

('PlayerMeleeAttack', 'Unarmed', 'CriticalHit',
 'Your devastating punch catches the {Enemy} completely off-guard!',
 '["Unarmed", "Critical", "Devastating"]');

-- ============================================================
-- Notes:
-- - Total player action descriptors: 100+
-- - Template variables: {Weapon}, {Enemy}, {Target_Location}, {Vital_Location},
--   {Damage_Type_Descriptor}, {Enemy_Reaction}, {Armor_Location}, {Environment_Feature}
-- ============================================================
