-- ==============================================================================
-- v0.38.15: Enemy Descriptions & Flavor Text - Data Population
-- ==============================================================================
-- Purpose: Provide immersive, Cargo Cult descriptions for enemies in the Bestiary.
-- Content: Descriptions for all standard enemy types defined in SPEC-COMBAT-012.
-- Note: Replaces technical terms (Robot, AI) with setting-appropriate voice.
-- ==============================================================================

-- Table Structure (Assumed based on previous patterns - adapted for Enemy Descriptions)
-- If this table does not exist, it should be created via schema update.
-- For now, we assume a table `Enemy_Descriptors` or similar exists or will be mapped.
-- Since I cannot see the schema, I will use a generic insert structure compatible with the other descriptor files.
-- We will assume a new table `Enemy_Flavor_Text` or append to `Examination_Descriptors` if they are examine targets.
-- Given `Examination_Descriptors` has `object_category`, `object_type`, `detail_level`, etc.

-- We will treat Enemies as "Creature" category for Examination_Descriptors.

-- ============================================================
-- CATEGORY: LOW THREAT ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Corrupted Servitor
('Creature', 'CorruptedServitor', 'Cursory', NULL, 'Alive', 'A hunched metal figure, twitching with the shakes of the long sleep.', 1.0, '["Basic", "Machine"]'),
('Creature', 'CorruptedServitor', 'Detailed', NULL, 'Alive', 'A Labor-Husk from the Old World. Its motive-joints grind with rust, and its single eye-lens flickers with a maddened red light. It seeks to crush anything that lives.', 1.0, '["Lore", "Machine"]'),
('Creature', 'CorruptedServitor', 'Expert', NULL, 'Alive', 'The Iron Law binds them no more. Their command runes are scratched out. If it glows red, it has forgotten its purpose. Strike the joints—they are stiff with age.', 1.0, '["Technical", "Machine"]'),

-- Scrap-Hound
('Creature', 'ScrapHound', 'Cursory', NULL, 'Alive', 'A four-legged jagged metal beast, low to the ground.', 1.0, '["Basic", "Machine"]'),
('Creature', 'ScrapHound', 'Detailed', NULL, 'Alive', 'A scavenger-beast made of blades and wire. It moves with a clattering speed, hunting for metal to consume and flesh to rend.', 1.0, '["Lore", "Machine"]'),
('Creature', 'ScrapHound', 'Expert', NULL, 'Alive', 'A Chaser-Spirit. Its jaws are shears meant to cut steel like cloth. Do not run—it is faster than fear. Break its legs to stop the hunt.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Sludge-Crawler
('Creature', 'SludgeCrawler', 'Cursory', NULL, 'Alive', 'A glistening mass of slime dragging itself forward.', 1.0, '["Basic", "Organic"]'),
('Creature', 'SludgeCrawler', 'Detailed', NULL, 'Alive', 'A bloat of alchemical waste given life. It leaves a trail of smoking acid. The smell is of rotten eggs and burning copper.', 1.0, '["Lore", "Bio"]'),
('Creature', 'SludgeCrawler', 'Expert', NULL, 'Alive', 'Living rot, born of the poison-waters. Do not touch it—your skin will weep and your iron will soften. Fire cleanses it best.', 1.0, '["Technical", "Bio", "Tactical"]'),

-- Corroded Sentry
('Creature', 'CorrodedSentry', 'Cursory', NULL, 'Alive', 'A floating metal eye, drifting on invisible winds.', 1.0, '["Basic", "Machine"]'),
('Creature', 'CorrodedSentry', 'Detailed', NULL, 'Alive', 'An ancient watcher, eaten by time. It leaks black oil like blood. It scans the dark for intruders to report to masters long dead.', 1.0, '["Lore", "Machine"]'),
('Creature', 'CorrodedSentry', 'Expert', NULL, 'Alive', 'A Watcher-Spirit that has lost its way. Its eye is cracked, its aim poor. But if it sees you, it screams to its kin. Blind it first.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Maintenance Construct
('Creature', 'MaintenanceConstruct', 'Cursory', NULL, 'Alive', 'A boxy, multi-armed machine moving with purpose.', 1.0, '["Basic", "Machine"]'),
('Creature', 'MaintenanceConstruct', 'Detailed', NULL, 'Alive', 'A Builder-Crab, obsessed with mending the world. It will weld your armor to your skin if it thinks you are broken. It ignores pain, focusing only on its work.', 1.0, '["Lore", "Machine"]'),
('Creature', 'MaintenanceConstruct', 'Expert', NULL, 'Alive', 'The Menders do not know when to stop. They carry fire-spitters and rivet-drivers. They heal themselves if you give them respite. Strike hard and fast.', 1.0, '["Technical", "Machine", "Tactical"]');


-- ============================================================
-- CATEGORY: MEDIUM THREAT ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Husk Enforcer
('Creature', 'HuskEnforcer', 'Cursory', NULL, 'Alive', 'A corpse in dead-man''s plate, shuffling forward.', 1.0, '["Basic", "Undead"]'),
('Creature', 'HuskEnforcer', 'Detailed', NULL, 'Alive', 'A dead guard, walked by the invisible fire. The armor is fused to the bone. It raises a lightning-stick with muscle memory.', 1.0, '["Lore", "Undead"]'),
('Creature', 'HuskEnforcer', 'Expert', NULL, 'Alive', 'The Blight won''t let them sleep. It pulls the strings of the dead. Take the head, or break the spine—make the body useless to the puppet-master.', 1.0, '["Technical", "Undead", "Tactical"]'),

-- Test Subject
('Creature', 'TestSubject', 'Cursory', NULL, 'Alive', 'A pale, twitching thing with glass-veins in its flesh.', 1.0, '["Basic", "Mutant"]'),
('Creature', 'TestSubject', 'Detailed', NULL, 'Alive', 'A victim of the flesh-shapers. Its limbs are stretched thin, skin like wet paper. It weeps as it tries to kill you.', 1.0, '["Lore", "Mutant"]'),
('Creature', 'TestSubject', 'Expert', NULL, 'Alive', 'Twisted by the alchymies of the Old World. They are fast, fueled by panic-blood. They break easily, but they strike with a frenzy that ignores pain.', 1.0, '["Technical", "Mutant", "Tactical"]'),

-- Blight-Drone
('Creature', 'BlightDrone', 'Cursory', NULL, 'Alive', 'A flying iron-wasp, buzzing loudly like an angry spirit.', 1.0, '["Basic", "Machine"]'),
('Creature', 'BlightDrone', 'Detailed', NULL, 'Alive', 'A hovering iron-wasp the size of a shield. It carries a belly full of sickness. The air around it tastes of copper.', 1.0, '["Lore", "Machine"]'),
('Creature', 'BlightDrone', 'Expert', NULL, 'Alive', 'Sky-Spirits meant to rain poison. Do not let them hover above you. Clip their wings or shoot them down before they vomit the sickness upon you.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Arc-Welder Unit (Industrial Robot)
('Creature', 'ArcWelderUnit', 'Cursory', NULL, 'Alive', 'A heavy iron-walker with a crackling lightning-spear.', 1.0, '["Basic", "Machine"]'),
('Creature', 'ArcWelderUnit', 'Detailed', NULL, 'Alive', 'A massive industrial walker. One arm ends in a pincer, the other in a rod that spits blinding lightning. It moves with the weight of a collapsing wall.', 1.0, '["Lore", "Machine"]'),
('Creature', 'ArcWelderUnit', 'Expert', NULL, 'Alive', 'A Heavy Builder. Its lightning-arm melts steel like wax. The spark-vessel is exposed on its back—a glowing target. Strike it there to let the spirit out.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Corrupted Engineer
('Creature', 'CorruptedEngineer', 'Cursory', NULL, 'Alive', 'A figure in torn white robes, surrounded by floating lights.', 1.0, '["Basic", "Humanoid"]'),
('Creature', 'CorruptedEngineer', 'Detailed', NULL, 'Alive', 'A Wright who stared too long into the abyss. They speak the secret tongues to the machines. They do not fight alone.', 1.0, '["Lore", "Humanoid"]'),
('Creature', 'CorruptedEngineer', 'Expert', NULL, 'Alive', 'A corrupted Speaker. They use a spirit-slate to enrage the iron-walkers. Silence them first, or the machines will fight with double the fury.', 1.0, '["Technical", "Humanoid", "Tactical"]'),

-- Shrieker
('Creature', 'Shrieker', 'Cursory', NULL, 'Alive', 'A mouthless face on a twisted body.', 1.0, '["Basic", "Horror"]'),
('Creature', 'Shrieker', 'Detailed', NULL, 'Alive', 'A horror of the deep dark. It has no eyes, only a gaping maw that screams a sound you feel in your teeth. Being near it clouds your mind.', 1.0, '["Lore", "Horror"]'),
('Creature', 'Shrieker', 'Expert', NULL, 'Alive', 'A Mind-Breaker. It screams with a voice that is not sound. Being near it feels like drowning. Silence it before your own thoughts turn against you.', 1.0, '["Technical", "Horror", "Tactical"]'),

-- Forlorn Scholar
('Creature', 'ForlornScholar', 'Cursory', NULL, 'Alive', 'A robed figure clutching a glowing tome.', 1.0, '["Basic", "Humanoid"]'),
('Creature', 'ForlornScholar', 'Detailed', NULL, 'Alive', 'A seeker of knowledge who found only madness. They whisper forbidden runes that warp the air. They might speak to you, but do not listen.', 1.0, '["Lore", "Humanoid"]'),
('Creature', 'ForlornScholar', 'Expert', NULL, 'Alive', 'A Weaver of corrupted Galdr. They attack the mind, not the body. Iron stops a blade, but it will not stop their whispers. Close the distance and strike.', 1.0, '["Technical", "Humanoid", "Tactical"]'),

-- Jötun-Reader Fragment (AI)
('Creature', 'JotunReaderFragment', 'Cursory', NULL, 'Alive', 'A spirit-light face of a giant, twitching and screaming.', 1.0, '["Basic", "Spirit"]'),
('Creature', 'JotunReaderFragment', 'Detailed', NULL, 'Alive', 'A shard of a dead god''s mind, projected into the air. It speaks in a thousand voices, demanding words you do not know. It burns with cold light.', 1.0, '["Lore", "Spirit"]'),
('Creature', 'JotunReaderFragment', 'Expert', NULL, 'Alive', 'A Ghost of the Machine-Mind. Hard light that cuts. It attacks with memory-spikes that ignore armor. Find the projector-box nearby—smash the glass to banish the ghost.', 1.0, '["Technical", "Spirit", "Tactical"]'),

-- Servitor Swarm
('Creature', 'ServitorSwarm', 'Cursory', NULL, 'Alive', 'A tide of small, broken machines.', 1.0, '["Basic", "Swarm"]'),
('Creature', 'ServitorSwarm', 'Detailed', NULL, 'Alive', 'Hundreds of tiny worker-husks moving as one. They strip flesh from bone like piranhas. Individually weak, but together they are a landslide of metal.', 1.0, '["Lore", "Swarm"]'),
('Creature', 'ServitorSwarm', 'Expert', NULL, 'Alive', 'A Swarm of Menders. You cannot kill them one by one—there are too many. Burn them all at once, or be eaten.', 1.0, '["Technical", "Swarm", "Tactical"]'),

-- War-Frame
('Creature', 'WarFrame', 'Cursory', NULL, 'Alive', 'A humanoid suit of armor, empty but moving.', 1.0, '["Basic", "Machine"]'),
('Creature', 'WarFrame', 'Detailed', NULL, 'Alive', 'A suit of Jötun battle-plate, walked by a dark spirit. It wields a blade taller than a man. It fights with the skill of a veteran warrior.', 1.0, '["Lore", "Machine"]'),
('Creature', 'WarFrame', 'Expert', NULL, 'Alive', 'An Empty-Walker. The armor is thick—small blades will just scratch the paint. It has no ranged weapons. Keep away and let it chase you.', 1.0, '["Technical", "Machine", "Tactical"]');


-- ============================================================
-- CATEGORY: HIGH THREAT ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Bone-Keeper
('Creature', 'BoneKeeper', 'Cursory', NULL, 'Alive', 'A skeletal giant woven with metal wire.', 1.0, '["Basic", "Undead"]'),
('Creature', 'BoneKeeper', 'Detailed', NULL, 'Alive', 'A collector of the dead. It wears the bones of its victims as armor. Its touch rots iron and withers flesh.', 1.0, '["Lore", "Undead"]'),
('Creature', 'BoneKeeper', 'Expert', NULL, 'Alive', 'A Corpse-Weaver. Hard to hurt, for it wears death like a coat. It heals by eating the dead nearby. Burn the bodies before it can feed.', 1.0, '["Technical", "Undead", "Tactical"]'),

-- Vault Custodian
('Creature', 'VaultCustodian', 'Cursory', NULL, 'Alive', 'A floating sphere with heavy shield plates.', 1.0, '["Basic", "Machine"]'),
('Creature', 'VaultCustodian', 'Detailed', NULL, 'Alive', 'The Warden of the Deep. It blocks the way with walls of hard light. It will not pursue, but it will not let you pass.', 1.0, '["Lore", "Machine"]'),
('Creature', 'VaultCustodian', 'Expert', NULL, 'Alive', 'The Door-Keeper. It hides behind walls of spirit-light. It will not chase you. Wait for the shield to flicker, then strike, but beware the push-back.', 1.0, '["Technical", "Machine", "Tactical"]');


-- ============================================================
-- CATEGORY: LETHAL THREAT ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Failure Colossus
('Creature', 'FailureColossus', 'Cursory', NULL, 'Alive', 'A mountain of scrap metal, dragging itself on fluid-muscle arms.', 1.0, '["Basic", "Machine"]'),
('Creature', 'FailureColossus', 'Detailed', NULL, 'Alive', 'A monument to error. It is built from the wreckage of a hundred lesser machines, fused together in a mockery of life. It does not feel pain. It only crushes.', 1.0, '["Lore", "Machine"]'),
('Creature', 'FailureColossus', 'Expert', NULL, 'Alive', 'A Siege-Husk. Slow as a glacier, but it hits like a falling mountain. Do not stand near it. Attack the back, where the armor is thin.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Rust-Witch
('Creature', 'RustWitch', 'Cursory', NULL, 'Alive', 'A woman fused into a throne of cables.', 1.0, '["Basic", "Horror"]'),
('Creature', 'RustWitch', 'Detailed', NULL, 'Alive', 'She is the queen of the scrap heap. The machines whisper to her, and she screams back. The rust on the walls obeys her command.', 1.0, '["Lore", "Horror"]'),
('Creature', 'RustWitch', 'Expert', NULL, 'Alive', 'A Wire-Queen. She speaks to the walls and makes them bite. She can make metal explode with a thought. Kill her quickly, before the room itself kills you.', 1.0, '["Technical", "Horror", "Tactical"]'),

-- Sentinel Prime
('Creature', 'SentinelPrime', 'Cursory', NULL, 'Alive', 'A pristine, towering war-walker in white armor.', 1.0, '["Basic", "Machine"]'),
('Creature', 'SentinelPrime', 'Detailed', NULL, 'Alive', 'The Old World''s wrath incarnate. It shines with a terrible, clean light. Its weapons utilize the invisible fire. It does not speak; it judges.', 1.0, '["Lore", "Machine"]'),
('Creature', 'SentinelPrime', 'Expert', NULL, 'Alive', 'A God-Machine. Armed with sun-spears and spirit-shields. It learns how you fight. Do not repeat yourself, or you will die.', 1.0, '["Technical", "Machine", "Tactical"]');


-- ============================================================
-- CATEGORY: BOSS ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Ruin-Warden
('Creature', 'RuinWarden', 'Cursory', NULL, 'Alive', 'A giant in rusted plate, wielding a hammer made of a heart-engine block.', 1.0, '["Basic", "Boss"]'),
('Creature', 'RuinWarden', 'Detailed', NULL, 'Alive', 'The King of the Scrap. He has ruled this sector for centuries. His armor is a patchwork of trophies. He fights with the desperation of a dying god.', 1.0, '["Lore", "Boss"]'),
('Creature', 'RuinWarden', 'Expert', NULL, 'Alive', 'The Overseer. As he bleeds, he hits harder. When he roars, keep away—his rage is a weapon.', 1.0, '["Technical", "Boss", "Tactical"]'),

-- Aetheric Aberration
('Creature', 'AethericAberration', 'Cursory', NULL, 'Alive', 'A swirling vortex of spirit-light and debris.', 1.0, '["Basic", "Boss"]'),
('Creature', 'AethericAberration', 'Detailed', NULL, 'Alive', 'A hole in the world. The laws of nature do not apply here. Looking at it makes your eyes bleed. It is the Blight given form.', 1.0, '["Lore", "Boss"]'),
('Creature', 'AethericAberration', 'Expert', NULL, 'Alive', 'A Breach in the Weave. Your blades will pass through it like smoke in the First Form. Use Galdr, or wait for it to become solid.', 1.0, '["Technical", "Boss", "Tactical"]'),

-- Forlorn Archivist
('Creature', 'ForlornArchivist', 'Cursory', NULL, 'Alive', 'A creature of robes and scrolls, floating above the ground.', 1.0, '["Basic", "Boss"]'),
('Creature', 'ForlornArchivist', 'Detailed', NULL, 'Alive', 'The Keeper of Forbidden Truths. It knows the name of the Glitch. It will unmake your mind with a word.', 1.0, '["Lore", "Boss"]'),
('Creature', 'ForlornArchivist', 'Expert', NULL, 'Alive', 'The Mind-Eater. It calls forth the dead in its Second Form. Guard your thoughts—iron cannot protect you here.', 1.0, '["Technical", "Boss", "Tactical"]'),

-- Omega Sentinel
('Creature', 'OmegaSentinel', 'Cursory', NULL, 'Alive', 'A walking fortress, bristling with thunder-mouths.', 1.0, '["Basic", "Boss"]'),
('Creature', 'OmegaSentinel', 'Detailed', NULL, 'Alive', 'The End of All Things. A weapon of the Last War, woken from its sleep. Its steps shake the citadel. It brings the thunder of the gods.', 1.0, '["Lore", "Boss"]'),
('Creature', 'OmegaSentinel', 'Expert', NULL, 'Alive', 'The Thunder-God. Form I: Lightning-Spears that cut the air. Form II: Rain of Fire. Form III: The Sun-Burst. If it begins to glow white... Run.', 1.0, '["Technical", "Boss", "Tactical"]');
