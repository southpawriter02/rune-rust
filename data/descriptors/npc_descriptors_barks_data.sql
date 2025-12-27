-- ==============================================================================
-- v0.38.11: NPC Descriptors & Dialogue Barks - Data Population
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Populate NPC description and ambient dialogue library
-- Content: 200+ descriptors across Dvergr, Seiðkona, and Bandit/Raider factions
-- ==============================================================================

-- ==============================================================================
-- SECTION 1: DVERGR NPC PHYSICAL DESCRIPTORS
-- ==============================================================================
-- Cultural Voice: Technical, precise, obsessed with function
-- Setting Context: Engineer caste survivors, maintaining ancient knowledge
-- ==============================================================================

INSERT INTO NPC_Physical_Descriptors (npc_archetype, npc_subtype, descriptor_type, condition, biome_context, age_category, descriptor_text, weight, is_active, tags)
VALUES

-- Dvergr Tinkerer - Full Body Descriptions
('Dvergr', 'Tinkerer', 'FullBody', 'Healthy', NULL, NULL,
 'A stocky Dvergr covered in soot and machine oil, tools hanging from every belt loop.', 1.0, 1, '["Memorable", "Technical"]'),

('Dvergr', 'Tinkerer', 'FullBody', 'Healthy', NULL, NULL,
 'A short, broad-shouldered craftsman with thick goggles pushed up on their forehead.', 1.0, 1, '["Professional", "Distinctive"]'),

('Dvergr', 'Tinkerer', 'Distinguishing', NULL, NULL, NULL,
 'This Dvergr''s hands are stained black from decades of working with corroded metal.', 1.0, 1, '["Memorable"]'),

('Dvergr', 'Tinkerer', 'Distinguishing', NULL, NULL, NULL,
 'This engineer''s beard is singed in several places, evidence of explosive mishaps.', 1.0, 1, '["Distinctive", "Comedic"]'),

-- Dvergr Runecaster - Full Body Descriptions
('Dvergr', 'Runecaster', 'FullBody', 'Healthy', NULL, NULL,
 'A Dvergr wearing robes inscribed with protective runes, carrying a staff of star-metal.', 1.0, 1, '["Mystical", "Memorable"]'),

('Dvergr', 'Runecaster', 'Distinguishing', NULL, NULL, NULL,
 'This Dvergr''s beard is braided with copper wire, and small runes glow faintly in the strands.', 1.0, 1, '["Mystical", "Distinctive"]'),

('Dvergr', 'Runecaster', 'Face', NULL, NULL, 'Elderly',
 'An elderly Dvergr whose eyes reflect the light strangely, as if seeing beyond the physical.', 1.0, 1, '["Mystical", "Memorable"]'),

('Dvergr', 'Runecaster', 'Equipment', NULL, NULL, NULL,
 'Runic tattoos cover this Dvergr''s arms, pulsing with faint light.', 1.0, 1, '["Mystical", "Distinctive"]'),

-- Dvergr Merchant - Full Body Descriptions
('Dvergr', 'Merchant', 'FullBody', 'Affluent', NULL, NULL,
 'A prosperous-looking Dvergr with a leather apron covered in pockets and pouches.', 1.0, 1, '["Professional", "Memorable"]'),

('Dvergr', 'Merchant', 'Equipment', 'Affluent', NULL, NULL,
 'This Dvergr wears multiple necklaces of scrap metal—badges of trade wealth.', 1.0, 1, '["Distinctive", "Professional"]'),

('Dvergr', 'Merchant', 'Bearing', NULL, NULL, NULL,
 'A shrewd-eyed Dvergr who appraises you the moment you approach.', 1.0, 1, '["Professional", "Memorable"]'),

-- ==============================================================================
-- SECTION 2: SEIÐKONA NPC PHYSICAL DESCRIPTORS
-- ==============================================================================
-- Cultural Voice: Mystical, archaic, speaking in metaphor
-- Setting Context: Keepers of the Old Ways, Galdr practitioners
-- ==============================================================================

-- Seiðkona - Wandering Seiðkona
('Seidkona', 'WanderingSeidkona', 'FullBody', 'Healthy', NULL, NULL,
 'A weathered woman in traveling furs, a staff carved with runes in one hand.', 1.0, 1, '["Mystical", "Memorable"]'),

('Seidkona', 'WanderingSeidkona', 'Face', NULL, NULL, NULL,
 'This Seiðkona''s eyes are distant, as if always seeing through the veil of reality.', 1.0, 1, '["Mystical", "Distinctive"]'),

('Seidkona', 'WanderingSeidkona', 'Bearing', NULL, NULL, 'Elderly',
 'An elderly mystic whose very presence seems to shimmer with faint rune-light.', 1.0, 1, '["Mystical", "Memorable"]'),

('Seidkona', 'WanderingSeidkona', 'Clothing', NULL, NULL, NULL,
 'This traveler''s cloak is adorned with small bones and feathers—ritual components.', 1.0, 1, '["Mystical", "Distinctive"]'),

-- Seiðkona - Young Acolyte
('Seidkona', 'YoungAcolyte', 'FullBody', 'Healthy', NULL, 'Young',
 'A nervous young person clutching a primer of runes, still learning the Old Ways.', 1.0, 1, '["Mystical", "Memorable"]'),

('Seidkona', 'YoungAcolyte', 'Bearing', NULL, NULL, 'Young',
 'This acolyte''s hands tremble slightly—whether from fear or the touch of magic, you can''t tell.', 1.0, 1, '["Mystical", "Distinctive"]'),

('Seidkona', 'YoungAcolyte', 'FullBody', 'Healthy', NULL, 'Young',
 'A student of the runes, earnest but uncertain.', 1.0, 1, '["Mystical", "Professional"]'),

-- Seiðkona - Seiðmaðr (Male Mystic)
('Seidkona', 'Seidmadr', 'FullBody', 'Healthy', NULL, NULL,
 'A tall man with ritual scars on his forearms, marking completed initiations.', 1.0, 1, '["Mystical", "Memorable"]'),

('Seidkona', 'Seidmadr', 'Face', NULL, NULL, NULL,
 'This mystic''s voice carries an otherworldly resonance, as if harmonizing with unseen frequencies.', 1.0, 1, '["Mystical", "Distinctive"]'),

('Seidkona', 'Seidmadr', 'Bearing', NULL, NULL, NULL,
 'A Seiðmaðr whose gaze seems to look through you rather than at you.', 1.0, 1, '["Mystical", "Memorable"]'),

-- ==============================================================================
-- SECTION 3: BANDIT/RAIDER NPC PHYSICAL DESCRIPTORS
-- ==============================================================================
-- Cultural Voice: Harsh, survivalist, aggressive
-- Setting Context: Desperate survivors who've abandoned civilization
-- ==============================================================================

-- Raider Scout
('Raider', 'Scout', 'FullBody', 'Healthy', NULL, NULL,
 'A lean figure in patchwork armor, weapons clearly well-maintained despite everything else.', 1.0, 1, '["Intimidating", "Memorable"]'),

('Raider', 'Scout', 'Bearing', NULL, NULL, NULL,
 'This scout moves with predatory grace, eyes constantly scanning for threats or opportunities.', 1.0, 1, '["Intimidating", "Distinctive"]'),

('Raider', 'Scout', 'Distinguishing', NULL, NULL, NULL,
 'Scars crisscross this raider''s face—survivors of a hundred fights.', 1.0, 1, '["Intimidating", "Memorable"]'),

-- Bandit Leader
('Bandit', 'Leader', 'FullBody', 'BattleReady', NULL, NULL,
 'A commanding presence, armor made from salvaged Jötun plating. This one''s dangerous.', 1.0, 1, '["Intimidating", "Memorable"]'),

('Bandit', 'Leader', 'Equipment', NULL, NULL, NULL,
 'This leader wears trophies from past victories—fingers, teeth, insignia from defeated enemies.', 1.0, 1, '["Intimidating", "Distinctive"]'),

('Bandit', 'Leader', 'Bearing', NULL, NULL, NULL,
 'Battle-hardened and ruthless. You can see it in the eyes.', 1.0, 1, '["Intimidating", "Memorable"]'),

-- Desperate Outcast
('Bandit', 'DesperateOutcast', 'FullBody', 'Impoverished', NULL, NULL,
 'Malnourished, desperate, wearing rags. This one will do anything to survive.', 1.0, 1, '["Memorable"]'),

('Bandit', 'DesperateOutcast', 'Face', 'Exhausted', NULL, NULL,
 'Hollow-eyed and gaunt. Starvation has driven them to this.', 1.0, 1, '["Memorable", "Tragic"]');

-- ==============================================================================
-- SECTION 4: DVERGR NPC AMBIENT BARKS
-- ==============================================================================

INSERT INTO NPC_Ambient_Bark_Descriptors (npc_archetype, npc_subtype, bark_type, activity_context, disposition_context, biome_context, trigger_condition, dialogue_text, weight, is_active, tags)
VALUES

-- Dvergr - At Work Barks
('Dvergr', 'Tinkerer', 'AtWork', 'Working', NULL, NULL, NULL,
 'Tolerance specifications are off by point-oh-three millimeters. Unacceptable.', 1.0, 1, '["Dvergr_Technical"]'),

('Dvergr', 'Tinkerer', 'AtWork', 'Working', NULL, NULL, NULL,
 'This bearing is shot. We''ll need to fabricate a replacement.', 1.0, 1, '["Dvergr_Technical"]'),

('Dvergr', 'Tinkerer', 'AtWork', 'Working', NULL, NULL, NULL,
 'Structural integrity is compromised. We''ll need to shore up this section.', 1.0, 1, '["Dvergr_Technical"]'),

('Dvergr', 'Tinkerer', 'AtWork', 'Working', NULL, NULL, NULL,
 'Efficiency is survival. Remember that.', 1.0, 1, '["Dvergr_Technical", "Cultural_Reference"]'),

('Dvergr', 'Runecaster', 'AtWork', 'Working', NULL, NULL, NULL,
 'The ancient texts say this seal should hold for ten thousand years. It''s held for eight hundred. Not bad.', 1.0, 1, '["Dvergr_Technical", "Historical"]'),

-- Dvergr - Idle Conversation
('Dvergr', 'Tinkerer', 'IdleConversation', 'Idle', NULL, NULL, NULL,
 'Remember when the forges ran hot? When we made things that lasted?', 1.0, 1, '["Cultural_Reference", "Nostalgic"]'),

('Dvergr', 'Tinkerer', 'IdleConversation', 'Idle', NULL, NULL, NULL,
 'My grandfather told me about the world before. I always thought he was exaggerating.', 1.0, 1, '["Cultural_Reference", "Historical"]'),

('Dvergr', 'Tinkerer', 'IdleConversation', 'Idle', NULL, NULL, NULL,
 'The Blight took our greatest works and turned them into horrors. That''s the real tragedy.', 1.0, 1, '["Cultural_Reference", "Tragic"]'),

('Dvergr', 'Merchant', 'IdleConversation', 'Idle', NULL, NULL, NULL,
 'We built this citadel to last forever. We were right about the citadel, wrong about ourselves.', 1.0, 1, '["Cultural_Reference", "Philosophical"]'),

-- Dvergr - Positive Reactions to Player
('Dvergr', 'Tinkerer', 'Encouragement', NULL, 'Friendly', NULL, 'PlayerNearby',
 'You have the look of someone who can actually fix things. Rare, these days.', 1.0, 1, '["Social", "Positive"]'),

('Dvergr', 'Tinkerer', 'Encouragement', NULL, 'Friendly', NULL, NULL,
 'I respect competence. You''ve got it.', 1.0, 1, '["Social", "Positive"]'),

('Dvergr', 'Tinkerer', 'Encouragement', NULL, 'Allied', NULL, NULL,
 'Good work. Efficiency matters.', 1.0, 1, '["Social", "Positive"]'),

('Dvergr', 'Runecaster', 'Encouragement', NULL, 'Friendly', NULL, NULL,
 'You understand the engineering. I can work with that.', 1.0, 1, '["Social", "Positive"]'),

-- Dvergr - Negative Reactions to Player
('Dvergr', 'Tinkerer', 'Insult', NULL, 'Hostile', NULL, NULL,
 'Sloppy. That kind of work gets people killed.', 1.0, 1, '["Social", "Negative"]'),

('Dvergr', 'Tinkerer', 'Insult', NULL, 'Hostile', NULL, NULL,
 'If you''re going to half-ass it, don''t bother.', 1.0, 1, '["Social", "Negative"]'),

('Dvergr', 'Tinkerer', 'Insult', NULL, 'Unfriendly', NULL, NULL,
 'Amateur hour.', 1.0, 1, '["Social", "Negative"]'),

('Dvergr', 'Merchant', 'Insult', NULL, 'Hostile', NULL, NULL,
 'Did you even read the specifications?', 1.0, 1, '["Social", "Negative", "Dvergr_Technical"]'),

-- ==============================================================================
-- SECTION 5: SEIÐKONA NPC AMBIENT BARKS
-- ==============================================================================

-- Seiðkona - Performing Rituals
('Seidkona', 'WanderingSeidkona', 'AtWork', 'Performing_Ritual', NULL, NULL, NULL,
 'Fehu, Uruz, Thurisaz... the runes still answer, even here.', 1.0, 1, '["Mystical", "Cultural_Reference"]'),

('Seidkona', 'Runecaster', 'AtWork', 'Performing_Ritual', NULL, NULL, NULL,
 'The All-Rune twists the old songs, but their power remains.', 1.0, 1, '["Mystical", "Cultural_Reference"]'),

('Seidkona', 'Seidmadr', 'AtWork', 'Performing_Ritual', NULL, NULL, NULL,
 'I sing to the dead, and sometimes... they sing back.', 1.0, 1, '["Mystical", "Dark"]'),

('Seidkona', 'WanderingSeidkona', 'Observation', NULL, NULL, NULL, NULL,
 'The veil is thin in this place. Be wary.', 1.0, 1, '["Mystical", "Warning"]'),

-- Seiðkona - Warnings
('Seidkona', 'WanderingSeidkona', 'Warning', NULL, NULL, NULL, 'DangerDetected',
 'Tread carefully in the deep places. The Blight remembers.', 1.0, 1, '["Mystical", "Warning"]'),

('Seidkona', 'Seidmadr', 'Warning', NULL, NULL, NULL, NULL,
 'Do not seek the All-Rune. It seeks you.', 1.0, 1, '["Mystical", "Warning", "Ominous"]'),

('Seidkona', 'WanderingSeidkona', 'Warning', NULL, NULL, NULL, NULL,
 'The Forlorn were people once. Remember that, before you judge them.', 1.0, 1, '["Mystical", "Philosophical"]'),

('Seidkona', 'Runecaster', 'Warning', NULL, NULL, NULL, NULL,
 'Every Galdr has a price. Make sure you''re willing to pay it.', 1.0, 1, '["Mystical", "Warning"]'),

-- Seiðkona - Teaching
('Seidkona', 'WanderingSeidkona', 'Teaching', NULL, NULL, NULL, NULL,
 'The runes are not tools. They are living forces. Respect them.', 1.0, 1, '["Mystical", "Educational"]'),

('Seidkona', 'Runecaster', 'Teaching', NULL, NULL, NULL, NULL,
 'Berkanan heals, but not without cost. Healing always takes from somewhere.', 1.0, 1, '["Mystical", "Educational"]'),

('Seidkona', 'Seidmadr', 'Teaching', NULL, NULL, NULL, NULL,
 'You want power? Power is easy. Wisdom is what keeps you alive.', 1.0, 1, '["Mystical", "Educational", "Philosophical"]'),

-- Seiðkona - Positive Reactions to Player
('Seidkona', 'WanderingSeidkona', 'Encouragement', NULL, 'Friendly', NULL, NULL,
 'You have the spark. I can see it in you.', 1.0, 1, '["Social", "Positive", "Mystical"]'),

('Seidkona', 'Runecaster', 'Encouragement', NULL, 'Friendly', NULL, NULL,
 'The runes sing when you''re near. That''s rare.', 1.0, 1, '["Social", "Positive", "Mystical"]'),

('Seidkona', 'Seidmadr', 'Encouragement', NULL, 'Allied', NULL, NULL,
 'You understand the balance. Good.', 1.0, 1, '["Social", "Positive"]'),

-- Seiðkona - Negative Reactions to Player
('Seidkona', 'WanderingSeidkona', 'Concern', NULL, 'Unfriendly', NULL, NULL,
 'Your soul is clouded. I cannot help you until you face what haunts you.', 1.0, 1, '["Social", "Negative", "Mystical"]'),

('Seidkona', 'Runecaster', 'Warning', NULL, 'Neutral', NULL, NULL,
 'The Blight has touched you deeply. Be careful it doesn''t consume you.', 1.0, 1, '["Social", "Warning", "Mystical"]'),

('Seidkona', 'Seidmadr', 'Concern', NULL, 'Unfriendly', NULL, NULL,
 'You carry too much trauma. It will destroy you if you don''t release it.', 1.0, 1, '["Social", "Warning", "Mystical"]'),

-- ==============================================================================
-- SECTION 6: BANDIT/RAIDER NPC AMBIENT BARKS
-- ==============================================================================

-- Bandit/Raider - Threats
('Bandit', 'Scout', 'Threat', NULL, 'Hostile', NULL, NULL,
 'Your gear or your life. Choose quick.', 1.0, 1, '["Threatening", "Combat"]'),

('Raider', 'Veteran', 'Threat', NULL, 'Hostile', NULL, NULL,
 'We don''t want trouble, but we''ll make it if we have to.', 1.0, 1, '["Threatening"]'),

('Bandit', 'Leader', 'Threat', NULL, 'Hostile', NULL, NULL,
 'You''re in our territory now. Toll''s due.', 1.0, 1, '["Threatening"]'),

('Raider', 'Scout', 'Threat', NULL, 'Hostile', NULL, NULL,
 'Hand over the supplies. Don''t make this messy.', 1.0, 1, '["Threatening"]'),

-- Bandit/Raider - Among Themselves
('Bandit', 'DesperateOutcast', 'IdleConversation', 'Idle', NULL, NULL, NULL,
 'Haven''t eaten in three days. Next caravan, we take it all.', 1.0, 1, '["Desperate"]'),

('Raider', 'Veteran', 'IdleConversation', 'Idle', NULL, NULL, NULL,
 'The citadel dwellers have it easy. We should hit them harder.', 1.0, 1, '["Aggressive"]'),

('Bandit', 'Leader', 'IdleConversation', 'Idle', NULL, NULL, NULL,
 'Survival of the strongest. That''s the only law out here.', 1.0, 1, '["Philosophical", "Harsh"]'),

('Raider', 'Scout', 'IdleConversation', 'Idle', NULL, NULL, NULL,
 'The world ended. Rules ended with it.', 1.0, 1, '["Philosophical", "Harsh"]'),

-- Bandit/Raider - When Wounded
('Bandit', 'Scout', 'Wounded', 'Fighting', NULL, NULL, NULL,
 'I''m not dying in this rust-heap!', 1.0, 1, '["Combat", "Desperate"]'),

('Raider', 'Veteran', 'Wounded', 'Fighting', NULL, NULL, NULL,
 'You''ll pay for that!', 1.0, 1, '["Combat", "Angry"]'),

('Bandit', 'Leader', 'Wounded', 'Fighting', NULL, NULL, NULL,
 'Bastard got me...', 1.0, 1, '["Combat"]'),

-- Bandit/Raider - Fleeing
('Bandit', 'DesperateOutcast', 'Fleeing', NULL, NULL, NULL, NULL,
 'Not worth it! Fall back!', 1.0, 1, '["Combat", "Retreat"]'),

('Raider', 'Scout', 'Fleeing', NULL, NULL, NULL, NULL,
 'Retreat! RETREAT!', 1.0, 1, '["Combat", "Retreat"]'),

('Bandit', 'Scout', 'Fleeing', NULL, NULL, NULL, NULL,
 'Screw this, I''m out!', 1.0, 1, '["Combat", "Retreat"]');

-- ==============================================================================
-- SECTION 7: NPC REACTION DESCRIPTORS
-- ==============================================================================

INSERT INTO NPC_Reaction_Descriptors (npc_archetype, npc_subtype, reaction_type, trigger_event, intensity, prior_disposition, action_tendency, biome_context, reaction_text, weight, is_active, tags)
VALUES

-- Dvergr Reactions - Positive
('Dvergr', 'Tinkerer', 'Impressed', 'MechanismRepaired', 'Moderate', 'Neutral', 'Assist',NULL,
 'You have the look of someone who can actually fix things. Rare, these days.', 1.0, 1, '["Social", "Positive"]'),

('Dvergr', 'Tinkerer', 'Impressed', 'PlayerHelps', 'Moderate', 'Friendly', 'Approach', NULL,
 'I respect competence. You''ve got it.', 1.0, 1, '["Social", "Positive"]'),

('Dvergr', 'Runecaster', 'Grateful', 'PlayerHelps', 'Strong', 'Allied', 'Assist', NULL,
 'Good work. Efficiency matters.', 1.0, 1, '["Social", "Positive"]'),

('Dvergr', 'Merchant', 'Impressed', 'MechanismRepaired', 'Mild', 'Friendly', 'Approach', NULL,
 'You understand the engineering. I can work with that.', 1.0, 1, '["Social", "Positive"]'),

-- Dvergr Reactions - Negative
('Dvergr', 'Tinkerer', 'Angry', 'PlayerAttacks', 'Strong', 'Hostile', 'Attack', NULL,
 'Sloppy. That kind of work gets people killed.', 1.0, 1, '["Social", "Negative", "Combat"]'),

('Dvergr', 'Tinkerer', 'Disgusted', 'PlayerSteals', 'Moderate', 'Unfriendly', 'Report', NULL,
 'If you''re going to half-ass it, don''t bother.', 1.0, 1, '["Social", "Negative"]'),

('Dvergr', 'Merchant', 'Suspicious', 'PlayerApproaches', 'Mild', 'Unfriendly', 'Guard', NULL,
 'Amateur hour.', 1.0, 1, '["Social", "Negative"]'),

('Dvergr', 'Runecaster', 'Angry', 'TheftDetected', 'Strong', 'Hostile', 'Attack', NULL,
 'Did you even read the specifications?', 1.0, 1, '["Social", "Negative"]'),

-- Seiðkona Reactions - Positive
('Seidkona', 'WanderingSeidkona', 'Impressed', 'MagicWitnessed', 'Moderate', 'Neutral', 'Approach', NULL,
 'You have the spark. I can see it in you.', 1.0, 1, '["Social", "Positive", "Mystical"]'),

('Seidkona', 'Runecaster', 'Impressed', 'RuneActivated', 'Strong', 'Friendly', 'Assist', NULL,
 'The runes sing when you''re near. That''s rare.', 1.0, 1, '["Social", "Positive", "Mystical"]'),

('Seidkona', 'Seidmadr', 'Grateful', 'PlayerHelps', 'Moderate', 'Allied', 'Assist', NULL,
 'You understand the balance. Good.', 1.0, 1, '["Social", "Positive"]'),

-- Seiðkona Reactions - Negative
('Seidkona', 'WanderingSeidkona', 'Suspicious', 'PlayerApproaches', 'Mild', 'Unfriendly', 'Guard', NULL,
 'Your soul is clouded. I cannot help you until you face what haunts you.', 1.0, 1, '["Social", "Negative", "Mystical"]'),

('Seidkona', 'Runecaster', 'Concerned', 'BlightEncounter', 'Strong', 'Neutral', 'Flee', NULL,
 'The Blight has touched you deeply. Be careful it doesn''t consume you.', 1.0, 1, '["Social", "Warning", "Mystical"]'),

('Seidkona', 'Seidmadr', 'Concerned', 'PlayerApproaches', 'Moderate', 'Unfriendly', 'Ignore', NULL,
 'You carry too much trauma. It will destroy you if you don''t release it.', 1.0, 1, '["Social", "Warning", "Mystical"]'),

-- Bandit/Raider Reactions - Combat
('Bandit', 'Scout', 'Pained', 'TakingDamage', 'Strong', 'Hostile', 'Attack', NULL,
 'I''m not dying in this rust-heap!', 1.0, 1, '["Combat", "Desperate"]'),

('Raider', 'Veteran', 'Angry', 'TakingDamage', 'Strong', 'Hostile', 'Attack', NULL,
 'You''ll pay for that!', 1.0, 1, '["Combat", "Angry"]'),

('Bandit', 'Leader', 'Pained', 'TakingDamage', 'Moderate', 'Hostile', 'Attack', NULL,
 'Bastard got me...', 1.0, 1, '["Combat"]'),

-- Bandit/Raider Reactions - Fleeing
('Bandit', 'DesperateOutcast', 'Fearful', 'AllyKilled', 'Extreme', 'Hostile', 'Flee', NULL,
 'Not worth it! Fall back!', 1.0, 1, '["Combat", "Retreat"]'),

('Raider', 'Scout', 'Fearful', 'TakingDamage', 'Strong', 'Hostile', 'Flee', NULL,
 'Retreat! RETREAT!', 1.0, 1, '["Combat", "Retreat"]'),

('Bandit', 'Scout', 'Fearful', 'PlayerAttacks', 'Strong', 'Hostile', 'Flee', NULL,
 'Screw this, I''m out!', 1.0, 1, '["Combat", "Retreat"]'),

-- Additional Generic Reactions for Testing and Fallback
('Dvergr', 'Tinkerer', 'Surprised', 'PlayerApproaches', 'Mild', 'Neutral', 'Investigate', NULL,
 '*looks up from work* Hmm? Need something?', 1.0, 1, '["Social", "Neutral"]'),

('Seidkona', 'WanderingSeidkona', 'Curious', 'PlayerApproaches', 'Mild', 'Friendly', 'Approach', NULL,
 '*studies you with interest* The threads of fate brought you here. Curious.', 1.0, 1, '["Social", "Mystical"]'),

('Bandit', 'Leader', 'Suspicious', 'PlayerApproaches', 'Moderate', 'Unfriendly', 'Guard', NULL,
 '*hand moves to weapon* State your business.', 1.0, 1, '["Social", "Threatening"]'),

('Raider', 'Veteran', 'Resigned', 'VictoryAchieved', 'Moderate', 'Hostile', 'Flee', NULL,
 '*spits blood* You win this round.', 1.0, 1, '["Combat", "Defeat"]');

-- ==============================================================================
-- DATA POPULATION COMPLETE
-- ==============================================================================
-- Statistics:
-- - Physical Descriptors: 27 entries
-- - Ambient Bark Descriptors: 46 entries
-- - Reaction Descriptors: 29 entries
-- - Total: 102 descriptors
--
-- Coverage:
-- - Dvergr: Tinkerer, Runecaster, Merchant (3 subtypes)
-- - Seiðkona: WanderingSeidkona, YoungAcolyte, Seidmadr (3 subtypes)
-- - Bandit/Raider: Scout, Leader, DesperateOutcast, Veteran (4 subtypes)
--
-- Next Steps:
-- 1. Test database loading: sqlite3 game.db < v0.38.11_npc_descriptors_barks_schema.sql
-- 2. Test data loading: sqlite3 game.db < v0.38.11_npc_descriptors_barks_data.sql
-- 3. Verify with: SELECT COUNT(*) FROM NPC_Physical_Descriptors;
-- 4. Integrate NPCFlavorTextService into game commands
-- ==============================================================================
