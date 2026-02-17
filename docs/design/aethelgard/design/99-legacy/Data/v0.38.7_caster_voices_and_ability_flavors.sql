-- ==============================================================================
-- v0.38.7: Galdr Caster Voice Profiles & Ability Flavor Descriptors
-- ==============================================================================
-- Purpose: Caster personality profiles + non-Galdr ability flavor text
-- ==============================================================================

-- ==============================================================================
-- GALDR CASTER VOICE PROFILES
-- ==============================================================================

INSERT INTO Galdr_Caster_Voices (caster_archetype, voice_name, voice_description, casting_style, setting_context, preferred_schools, is_active, tags)
VALUES
-- VARD-WARDEN (Player Mystic Specialization)
('VardWarden', 'Vard-Warden''s Reverent Chanting', 'Reverent and methodical, invoking divine order through sacred wards.', 'Formal runic incantations, slow and deliberate, with ritual gestures.', 'Vard-Wardens are defensive mystics who specialize in Tiwaz (protection) and Berkanan (healing) runes, creating barriers and sanctified ground to protect against the Blight. They view magic as sacred duty, channeling divine order against chaos.', '["Tiwaz", "Berkanan"]', 1, '["Player", "Defensive", "Holy"]'),

-- RUST-WITCH (Player Mystic Specialization)
('RustWitch', 'Rust-Witch''s Heretical Whispers', 'Desperate and heretical, whispering forbidden entropy to rust and decay.', 'Broken whispered chants, feverish and erratic, with self-inflicted corruption.', 'Rust-Witches practice heretical magic, embracing Naudiz (draining) and Isa (stasis) to channel entropy and decay. They accept personal corruption as the price of power, walking the line between control and madness. Their magic corrodes reality itself.', '["Naudiz", "Isa"]', 1, '["Player", "Dark", "Corrupted"]'),

-- VÖLVA (NPC Seer/Prophet)
('Völva', 'Völva''s Prophetic Song', 'Ancient and authoritative, channeling prophetic visions through runes.', 'Sung Galdr in ancient melodies, voice resonant with otherworldly power.', 'Völvas are seers and prophets, practitioners of seiðr who divine the future through runic magic. They sing the threads of fate, their Galdr weaving between past, present, and future. Their magic reveals truth—no matter how terrible.', '["Ansuz", "Laguz"]', 1, '["NPC", "Prophet", "Ancient"]'),

-- SEIÐKONA (NPC Female Practitioner)
('Seiðkona', 'Seiðkona''s Woven Chant', 'Melodic and hypnotic, weaving reality with voice and will.', 'Sung Galdr in melodic verses, weaving reality like a loom.', 'Seiðkonas are female practitioners of seiðr, singing magic that weaves reality itself. Their Galdr is beautiful and terrifying, bending the world to match their vision. They are respected and feared in equal measure.', '["Jera", "Berkanan", "Laguz"]', 1, '["NPC", "Female", "Weaver"]'),

-- SEIÐMAÐR (NPC Male Practitioner)
('Seiðmaðr', 'Seiðmaðr''s Grim Invocation', 'Grim and unwavering, commanding power through absolute certainty.', 'Deep, resonant chants that demand obedience from reality itself.', 'Seiðmenn are male practitioners of seiðr, a role considered unmanly in Norse tradition—yet they persist, wielding power through sheer force of will. Their Galdr commands rather than requests, compelling magic through absolute conviction.', '["Thurisaz", "Tiwaz", "Ansuz"]', 1, '["NPC", "Male", "Commanding"]'),

-- ELDER RUNESMITH (NPC Ancient Craftsman-Mage)
('Elder_Runesmith', 'Elder Runesmith''s Forge-Song', 'Patient and precise, crafting magic with the care of a master artisan.', 'Rhythmic chants that echo forge-work, measured and methodical.', 'Elder Runesmiths are ancient craftsmen who carve runes into reality as others carve them into stone. Their magic is patient, precise, and permanent—each spell a masterwork. They view Galdr as craftsmanship, magic as art.', '["Fehu", "Kenaz", "Tiwaz"]', 1, '["NPC", "Craftsman", "Ancient"]'),

-- CORRUPTED MAGE (NPC Enemy)
('CorruptedMage', 'Corrupted Mage''s Fractured Chanting', 'Broken and mad, magic warped by Blight corruption.', 'Disjointed, stammering incantations that hurt to hear.', 'Corrupted Mages have been consumed by the Runic Blight, their minds and magic twisted beyond recognition. Their Galdr is paradoxical, wrong—runes that shouldn''t exist, spells that defy logic. They are pitiful and terrifying in equal measure.', '["Naudiz", "Hagalaz", "Isa"]', 1, '["NPC", "Enemy", "Corrupted", "Horrifying"]'),

-- BOSS MAGE (NPC Boss Enemy)
('BossMage', 'Boss Mage''s Reality-Rending Invocation', 'Overwhelming and terrible, magic that defies mortal comprehension.', 'Multi-layered Galdr sung in impossible harmonics, reality bending around each syllable.', 'Boss Mages wield power beyond mortal limits, their Galdr rewriting reality itself. Each incantation is a masterpiece of magical prowess—and a horror to witness. They are what happens when a mage transcends human limits... in the wrong direction.', '["Ansuz", "Jera", "Mannaz", "Laguz"]', 1, '["NPC", "Boss", "Powerful", "Horrifying"]');

-- ==============================================================================
-- ABILITY FLAVOR DESCRIPTORS - WEAPON ARTS
-- ==============================================================================

INSERT INTO Ability_Flavor_Descriptors (ability_category, ability_name, weapon_type, success_level, descriptor_text, weight, tags)
VALUES
-- WHIRLWIND STRIKE (Two-Handed Weapon Art)
('WeaponArt', 'WhirlwindStrike', 'TwoHanded', 'SolidSuccess', 'You spin, your {Weapon} becoming a devastating blur of steel!', 1.0, '["Dramatic"]'),
('WeaponArt', 'WhirlwindStrike', 'TwoHanded', 'SolidSuccess', 'With a battle cry, you unleash a whirlwind of strikes—your {Weapon} finds every gap in the {Enemy}''s defense!', 1.0, '["Brutal"]'),
('WeaponArt', 'WhirlwindStrike', 'TwoHanded', 'ExceptionalSuccess', 'You become a storm of blades! Your {Weapon} strikes from every angle—the {Enemy} has no defense!', 1.0, '["Epic"]'),
('WeaponArt', 'WhirlwindStrike', 'TwoHanded', 'MinorSuccess', 'You spin, but your {Weapon} cuts only air—the attack lacks momentum!', 1.0, '["Failure"]'),

-- PRECISION STRIKE (One-Handed Weapon Art)
('WeaponArt', 'PrecisionStrike', 'OneHanded', 'SolidSuccess', 'You wait for the perfect moment, then strike with surgical precision at the {Enemy}''s weak point!', 1.0, '["Precise"]'),
('WeaponArt', 'PrecisionStrike', 'OneHanded', 'SolidSuccess', 'Your blade finds the gap in the {Enemy}''s armor with practiced ease!', 1.0, '["Skilled"]'),
('WeaponArt', 'PrecisionStrike', 'OneHanded', 'ExceptionalSuccess', 'Perfect strike! Your {Weapon} slips between armor plates, finding the {Enemy}''s {Vital_Location} with deadly accuracy!', 1.0, '["Lethal"]'),

-- POWER STRIKE (Two-Handed Weapon Art)
('WeaponArt', 'PowerStrike', 'TwoHanded', 'SolidSuccess', 'You bring your {Weapon} down with bone-crushing force!', 1.0, '["Brutal"]'),
('WeaponArt', 'PowerStrike', 'TwoHanded', 'ExceptionalSuccess', 'You channel all your might into one devastating blow! Your {Weapon} shatters the {Enemy}''s defenses!', 1.0, '["Devastating"]'),

-- CLEAVE (Two-Handed Weapon Art)
('WeaponArt', 'Cleave', 'TwoHanded', 'SolidSuccess', 'Your {Weapon} carves through the {Enemy} and into another foe behind it!', 1.0, '["Brutal"]'),
('WeaponArt', 'Cleave', 'ExceptionalSuccess', 'Your strike cleaves through multiple enemies! Bodies fall like wheat before the scythe!', 1.0, '["Epic", "Brutal"]');

-- ==============================================================================
-- ABILITY FLAVOR DESCRIPTORS - DEFENSIVE ABILITIES
-- ==============================================================================

INSERT INTO Ability_Flavor_Descriptors (ability_category, ability_name, success_level, descriptor_text, weight, tags)
VALUES
-- DEFENSIVE STANCE
('DefensiveAbility', 'DefensiveStance', 'SolidSuccess', 'You settle into a defensive posture, weapon ready, guard unbreakable.', 1.0, '["Defensive"]'),
('DefensiveAbility', 'DefensiveStance', 'SolidSuccess', 'You become a wall—let them come!', 1.0, '["Determined"]'),
('DefensiveAbility', 'DefensiveStance', 'ExceptionalSuccess', 'You plant yourself like an immovable fortress! No attack shall pass!', 1.0, '["Epic"]'),

-- PARRY
('DefensiveAbility', 'Parry', 'SolidSuccess', 'You deflect the {Enemy}''s attack with a swift counter-motion!', 1.0, '["Skilled"]'),
('DefensiveAbility', 'Parry', 'ExceptionalSuccess', 'Perfect parry! You redirect the {Enemy}''s force back at them!', 1.0, '["Masterful"]'),
('DefensiveAbility', 'Parry', 'Failure', 'You attempt to parry but misjudge—the blow connects!', 1.0, '["Failure"]'),

-- DODGE
('DefensiveAbility', 'Dodge', 'SolidSuccess', 'You twist aside, the {Enemy}''s attack whistling past harmlessly!', 1.0, '["Agile"]'),
('DefensiveAbility', 'Dodge', 'ExceptionalSuccess', 'You flow like water around the attack—the {Enemy} strikes only air!', 1.0, '["Graceful"]');

-- ==============================================================================
-- ABILITY FLAVOR DESCRIPTORS - TACTICAL ABILITIES
-- ==============================================================================

INSERT INTO Ability_Flavor_Descriptors (ability_category, ability_name, success_level, descriptor_text, weight, tags)
VALUES
-- SPRINT
('TacticalAbility', 'Sprint', 'SolidSuccess', 'You dash across the battlefield with explosive speed!', 1.0, '["Fast"]'),
('TacticalAbility', 'Sprint', 'SolidSuccess', 'Your tactical repositioning catches the {Enemy} off-guard!', 1.0, '["Tactical"]'),

-- RALLY
('TacticalAbility', 'Rally', 'SolidSuccess', 'You shout encouragement, rallying your resolve! Determination renewed!', 1.0, '["Inspiring"]'),
('TacticalAbility', 'Rally', 'ExceptionalSuccess', 'Your battle cry echoes! Allies take heart, enemies falter!', 1.0, '["Inspiring", "Epic"]'),

-- FEINT
('TacticalAbility', 'Feint', 'SolidSuccess', 'You fake left, strike right! The {Enemy} falls for it completely!', 1.0, '["Deceptive"]'),
('TacticalAbility', 'Feint', 'ExceptionalSuccess', 'Perfect deception! The {Enemy} commits to the wrong defense—you exploit the opening brutally!', 1.0, '["Masterful"]');

-- ==============================================================================
-- ABILITY FLAVOR DESCRIPTORS - SPECIALIZATION-SPECIFIC
-- ==============================================================================

INSERT INTO Ability_Flavor_Descriptors (ability_category, ability_name, specialization, success_level, descriptor_text, weight, tags)
VALUES
-- SKAR HORDE ASPIRANT (Berserker with Savagery resource)
('ResourceAbility', 'RageFury', 'SkarHordeAspirant', 'SolidSuccess', 'Fury fills you! The Skar''s savage strength flows through your veins!', 1.0, '["Berserker"]'),
('ResourceAbility', 'RageFury', 'SkarHordeAspirant', 'ExceptionalSuccess', 'RAGE! You channel the Horde''s primal fury—nothing can stand before you!', 1.0, '["Epic", "Berserker"]'),

-- IRON BANE (Anti-mechanical zealot)
('WeaponArt', 'MachinebreakersWrath', 'IronBane', 'SolidSuccess', 'Righteous fury guides your strike! Metal crumples, circuits shatter—the machine dies!', 1.0, '["Zealot", "Anti-Mechanical"]'),
('WeaponArt', 'MachinebreakersWrath', 'IronBane', 'ExceptionalSuccess', 'You invoke the Machinebreaker''s blessing! Your blow tears through the Servitor''s chassis like parchment!', 1.0, '["Epic", "Zealot"]'),

-- ATGEIR WIELDER (Versatile reach weapon specialist)
('WeaponArt', 'AtgeirSweep', 'AtgeirWielder', 'SolidSuccess', 'You sweep your atgeir in a wide arc—enemies at every distance threatened!', 1.0, '["Reach", "Versatile"]'),
('WeaponArt', 'AtgeirSweep', 'AtgeirWielder', 'ExceptionalSuccess', 'Perfect sweep! Your atgeir becomes a wheel of death—none can approach!', 1.0, '["Epic", "Reach"]'),

-- BONE SETTER (Non-magical medic)
('ResourceAbility', 'FieldMedicine', 'BoneSetter', 'SolidSuccess', 'Your skilled hands set bones, stitch wounds, staunch bleeding. Practical healing, no magic needed.', 1.0, '["Medical", "Practical"]'),
('ResourceAbility', 'FieldMedicine', 'BoneSetter', 'ExceptionalSuccess', 'Master surgery! Your medical expertise saves lives where magic would fail!', 1.0, '["Medical", "Masterful"]'),

-- SCRAP TINKER (Crafting specialist)
('ResourceAbility', 'QuickRepair', 'ScrapTinker', 'SolidSuccess', 'You jury-rig a solution from scrap! It''ll hold... probably.', 1.0, '["Crafting", "Improvised"]'),
('ResourceAbility', 'QuickRepair', 'ScrapTinker', 'ExceptionalSuccess', 'Genius improvisation! You craft a working solution from nothing but junk and ingenuity!', 1.0, '["Crafting", "Genius"]');

-- ==============================================================================
-- ABILITY FLAVOR DESCRIPTORS - PASSIVE ABILITIES
-- ==============================================================================

INSERT INTO Ability_Flavor_Descriptors (ability_category, ability_name, descriptor_text, weight, tags)
VALUES
('PassiveAbility', 'CombatReflexes', 'Your honed reflexes allow you to react faster in combat!', 1.0, '["Passive"]'),
('PassiveAbility', 'ToughenedHide', 'Years of battle have made you resilient—wounds that would fell others barely slow you!', 1.0, '["Passive"]'),
('PassiveAbility', 'KeenEye', 'Nothing escapes your notice—you spot weaknesses others miss!', 1.0, '["Passive"]'),
('PassiveAbility', 'BattleHardened', 'You''ve seen worse. Fear and intimidation have no hold on you.', 1.0, '["Passive"]'),
('PassiveAbility', 'Resourceful', 'You make do with what you have—scarcity breeds ingenuity!', 1.0, '["Passive"]');

-- ==============================================================================
-- Statistics Queries
-- ==============================================================================
-- SELECT caster_archetype, voice_name
-- FROM Galdr_Caster_Voices
-- ORDER BY caster_archetype;
--
-- SELECT ability_category, ability_name, COUNT(*) as count
-- FROM Ability_Flavor_Descriptors
-- GROUP BY ability_category, ability_name
-- ORDER BY ability_category, count DESC;
