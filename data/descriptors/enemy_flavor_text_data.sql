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
('Creature', 'CorruptedServitor', 'Cursory', NULL, 'Alive', 'A hunched metal figure, twitching with erratic motion.', 1.0, '["Basic", "Machine"]'),
('Creature', 'CorruptedServitor', 'Detailed', NULL, 'Alive', 'A Labor-Husk from the Old World. Its motive-joints grind with rust, and its single eye-lens flickers with a maddened red light. It seeks to crush anything that lives.', 1.0, '["Lore", "Machine"]'),
('Creature', 'CorruptedServitor', 'Expert', NULL, 'Alive', 'The Twelfth Servant of the Iron Law. The command runes on its shell have been scored out and replaced with jagged scratches. It no longer obeys the Rites of Order. It knows only the breaking of things.', 1.0, '["Technical", "Machine"]'),

-- Scrap-Hound
('Creature', 'ScrapHound', 'Cursory', NULL, 'Alive', 'A four-legged jagged metal beast, low to the ground.', 1.0, '["Basic", "Machine"]'),
('Creature', 'ScrapHound', 'Detailed', NULL, 'Alive', 'A scavenger-beast made of blades and wire. It moves with a clattering speed, hunting for metal to consume and flesh to rend.', 1.0, '["Lore", "Machine"]'),
('Creature', 'ScrapHound', 'Expert', NULL, 'Alive', 'The Hound that Hunts for the Iron Masters. Its jaws are driven by oil-blood to snap through hull plating. Do not let it close the distance; its speed is its weapon.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Sludge-Crawler
('Creature', 'SludgeCrawler', 'Cursory', NULL, 'Alive', 'A glistening mass of slime dragging itself forward.', 1.0, '["Basic", "Organic"]'),
('Creature', 'SludgeCrawler', 'Detailed', NULL, 'Alive', 'A bloat of alchemical waste given life. It leaves a trail of smoking acid. The smell is of rotten eggs and burning copper.', 1.0, '["Lore", "Bio"]'),
('Creature', 'SludgeCrawler', 'Expert', NULL, 'Alive', 'A colony of the Invisible Rot that Eats Iron, mutated by the Blight. Do not touch it. Its very touch dissolves iron and flesh alike.', 1.0, '["Technical", "Bio", "Tactical"]'),

-- Corroded Sentry
('Creature', 'CorrodedSentry', 'Cursory', NULL, 'Alive', 'A floating metal eye, drifting aimlessly.', 1.0, '["Basic", "Machine"]'),
('Creature', 'CorrodedSentry', 'Detailed', NULL, 'Alive', 'An ancient watcher, eaten by time. It leaks black oil like blood. It scans the dark for intruders to report to masters long dead.', 1.0, '["Lore", "Machine"]'),
('Creature', 'CorrodedSentry', 'Expert', NULL, 'Alive', 'A Sky-Eye that Watches for the Old Ones. Its spirit-sight is clouded, making its aim poor, but it can still scream to other horrors of your location.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Maintenance Construct
('Creature', 'MaintenanceConstruct', 'Cursory', NULL, 'Alive', 'A boxy, multi-armed machine moving with purpose.', 1.0, '["Basic", "Machine"]'),
('Creature', 'MaintenanceConstruct', 'Detailed', NULL, 'Alive', 'A Builder-Crab, obsessed with mending. It will fuse your armor to your skin if it thinks you are broken. It ignores pain, focusing only on its work.', 1.0, '["Lore", "Machine"]'),
('Creature', 'MaintenanceConstruct', 'Expert', NULL, 'Alive', 'A Mender of the Broken Shells. Equipped with blinding-torches and rivet-spitters. It has self-mending rites; break it quickly, or it will rebuild itself before your eyes.', 1.0, '["Technical", "Machine", "Tactical"]');


-- ============================================================
-- CATEGORY: MEDIUM THREAT ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Husk Enforcer
('Creature', 'HuskEnforcer', 'Cursory', NULL, 'Alive', 'A corpse in riot armor, shuffling forward.', 1.0, '["Basic", "Undead"]'),
('Creature', 'HuskEnforcer', 'Detailed', NULL, 'Alive', 'A dead guard, animated by the invisible fire. The armor is fused to the bone. It raises a shock-baton with muscle memory.', 1.0, '["Lore", "Undead"]'),
('Creature', 'HuskEnforcer', 'Expert', NULL, 'Alive', 'A puppet of dead flesh. The Blight sings to the nerves, forcing the body to fight long after the spirit has fled. Strike the head or sever the spine to silence it.', 1.0, '["Technical", "Undead", "Tactical"]'),

-- Test Subject
('Creature', 'TestSubject', 'Cursory', NULL, 'Alive', 'A pale, twitching humanoid with tubes in its flesh.', 1.0, '["Basic", "Mutant"]'),
('Creature', 'TestSubject', 'Detailed', NULL, 'Alive', 'A victim of the flesh-shapers. Its limbs are elongated, and its skin is translucent. It weeps as it attacks.', 1.0, '["Lore", "Mutant"]'),
('Creature', 'TestSubject', 'Expert', NULL, 'Alive', 'A Twisted Flesh-Pattern from the Bio-Pits. The alchemical fire in its blood makes it incredibly fast but fragile. Its unstable form makes it prone to the madness-rage.', 1.0, '["Technical", "Mutant", "Tactical"]'),

-- Blight-Drone
('Creature', 'BlightDrone', 'Cursory', NULL, 'Alive', 'A flying insect-machine, buzzing loudly.', 1.0, '["Basic", "Machine"]'),
('Creature', 'BlightDrone', 'Detailed', NULL, 'Alive', 'A hovering metal wasp the size of a shield. It carries a payload of sickness. The air around it tastes of copper.', 1.0, '["Lore", "Machine"]'),
('Creature', 'BlightDrone', 'Expert', NULL, 'Alive', 'A Cloud-Spreader of the Sickness. Originally for killing crops, now filled with Blight-breath. It attacks from the sky. Bring it to the earth immediately.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Arc-Welder Unit (Industrial Robot)
('Creature', 'ArcWelderUnit', 'Cursory', NULL, 'Alive', 'A heavy machine with a crackling lightning-spear.', 1.0, '["Basic", "Machine"]'),
('Creature', 'ArcWelderUnit', 'Detailed', NULL, 'Alive', 'A massive industrial walker. One arm ends in a pincer, the other in a rod that spits blinding lightning. It moves with the weight of a collapsing wall.', 1.0, '["Lore", "Machine"]'),
('Creature', 'ArcWelderUnit', 'Expert', NULL, 'Alive', 'A Titan-Shell for Building the World. The lightning-arm can melt plate armor in a heartbeat. Its spark-vessel is exposed on the back—a place to strike if you can turn it.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Corrupted Engineer
('Creature', 'CorruptedEngineer', 'Cursory', NULL, 'Alive', 'A figure in a torn lab coat, surrounded by floating lights.', 1.0, '["Basic", "Humanoid"]'),
('Creature', 'CorruptedEngineer', 'Detailed', NULL, 'Alive', 'A Wright who stared too long into the abyss. They command the machines with a twisted voice. They do not fight alone.', 1.0, '["Lore", "Humanoid"]'),
('Creature', 'CorruptedEngineer', 'Expert', NULL, 'Alive', 'A Keeper of the Machine-Rites. They use a portable totem to wake the spirits of nearby iron-walkers. Silence them first to break the enemy''s will.', 1.0, '["Technical", "Humanoid", "Tactical"]'),

-- Shrieker
('Creature', 'Shrieker', 'Cursory', NULL, 'Alive', 'A mouthless face on a twisted body.', 1.0, '["Basic", "Horror"]'),
('Creature', 'Shrieker', 'Detailed', NULL, 'Alive', 'A horror of the deep dark. It has no eyes, only a gaping maw that screams a sound you feel in your teeth. Being near it clouds your mind.', 1.0, '["Lore", "Horror"]'),
('Creature', 'Shrieker', 'Expert', NULL, 'Alive', 'A Spirit of Pure Terror. It sings a song that breaks the mind (Stress). Its scream is a weapon. Silence it before you lose yourself.', 1.0, '["Technical", "Horror", "Tactical"]'),

-- Forlorn Scholar
('Creature', 'ForlornScholar', 'Cursory', NULL, 'Alive', 'A robed figure clutching a glowing tome.', 1.0, '["Basic", "Humanoid"]'),
('Creature', 'ForlornScholar', 'Detailed', NULL, 'Alive', 'A seeker of knowledge who found only madness. They whisper forbidden runes that warp the air. They might speak to you, but do not listen.', 1.0, '["Lore", "Humanoid"]'),
('Creature', 'ForlornScholar', 'Expert', NULL, 'Alive', 'A Caster wielding Corrupted Galdr. They can inflict heavy soul-scars. Their spirit-shield is weak against cold iron, but their mind-magics are potent.', 1.0, '["Technical", "Humanoid", "Tactical"]'),

-- Jötun-Reader Fragment (AI)
('Creature', 'JotunReaderFragment', 'Cursory', NULL, 'Alive', 'A ghost of a giant face, glitching and screaming.', 1.0, '["Basic", "Spirit"]'),
('Creature', 'JotunReaderFragment', 'Detailed', NULL, 'Alive', 'A shard of a dead god''s mind, projected into the air. It speaks in a thousand voices, demanding words of power you do not know. It burns with cold light.', 1.0, '["Lore", "Spirit"]'),
('Creature', 'JotunReaderFragment', 'Expert', NULL, 'Alive', 'A Shattered Spirit of the Oracle. Hard light made form. It attacks with mind-spikes that ignore armor. It is tethered to a nearby totem—break the stone to banish the ghost.', 1.0, '["Technical", "Spirit", "Tactical"]'),

-- Servitor Swarm
('Creature', 'ServitorSwarm', 'Cursory', NULL, 'Alive', 'A tide of small, broken machines.', 1.0, '["Basic", "Swarm"]'),
('Creature', 'ServitorSwarm', 'Detailed', NULL, 'Alive', 'Hundreds of tiny worker-bots moving as one. They strip flesh from bone like piranhas. Individually weak, but together they are a landslide of metal.', 1.0, '["Lore", "Swarm"]'),
('Creature', 'ServitorSwarm', 'Expert', NULL, 'Alive', 'A Swarm of Invisible Menders gone mad. Fire and wide swings are required. Striking one at a time is like fighting the rain.', 1.0, '["Technical", "Swarm", "Tactical"]'),

-- War-Frame
('Creature', 'WarFrame', 'Cursory', NULL, 'Alive', 'A humanoid suit of armor, empty but moving.', 1.0, '["Basic", "Machine"]'),
('Creature', 'WarFrame', 'Detailed', NULL, 'Alive', 'A suit of Jötun battle-plate, animated by a dark spirit. It wields a blade taller than a man. It fights with the skill of a veteran warrior.', 1.0, '["Lore", "Machine"]'),
('Creature', 'WarFrame', 'Expert', NULL, 'Alive', 'A Hollow Shell that Walks Alone. Heavy plating makes it laugh at small blades. It cannot strike at distance—dance away from it if you can.', 1.0, '["Technical", "Machine", "Tactical"]');


-- ============================================================
-- CATEGORY: HIGH THREAT ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Bone-Keeper
('Creature', 'BoneKeeper', 'Cursory', NULL, 'Alive', 'A skeletal giant woven with metal wire.', 1.0, '["Basic", "Undead"]'),
('Creature', 'BoneKeeper', 'Detailed', NULL, 'Alive', 'A collector of the dead. It wears the bones of its victims as armor. Its touch rots iron and withers flesh.', 1.0, '["Lore", "Undead"]'),
('Creature', 'BoneKeeper', 'Expert', NULL, 'Alive', 'A Thing of Dead Flesh and Cold Iron. High resistance to blows. It mends itself by consuming the dead nearby. Leave nothing for it to eat.', 1.0, '["Technical", "Undead", "Tactical"]'),

-- Vault Custodian
('Creature', 'VaultCustodian', 'Cursory', NULL, 'Alive', 'A floating sphere with heavy shield plates.', 1.0, '["Basic", "Machine"]'),
('Creature', 'VaultCustodian', 'Detailed', NULL, 'Alive', 'The Warden of the Deep. It blocks the way with walls of hard light. It will not pursue, but it will not let you pass.', 1.0, '["Lore", "Machine"]'),
('Creature', 'VaultCustodian', 'Expert', NULL, 'Alive', 'A Silent Guardian of the Forbidden Gates. Projecting high-strength spirit-walls. It has many faces; when the shield falls, it unleashes a wave of force.', 1.0, '["Technical", "Machine", "Tactical"]');


-- ============================================================
-- CATEGORY: LETHAL THREAT ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Failure Colossus
('Creature', 'FailureColossus', 'Cursory', NULL, 'Alive', 'A mountain of scrap metal, dragging itself on hydraulic arms.', 1.0, '["Basic", "Machine"]'),
('Creature', 'FailureColossus', 'Detailed', NULL, 'Alive', 'A monument to error. It is built from the wreckage of a hundred lesser machines, fused together in a mockery of life. It does not feel pain. It only crushes.', 1.0, '["Lore", "Machine"]'),
('Creature', 'FailureColossus', 'Expert', NULL, 'Alive', 'A Mountain-Breaker of the Last War. Extremely slow but its blow can shatter the earth. Do not stand within reach. Its plating is thickest on the face.', 1.0, '["Technical", "Machine", "Tactical"]'),

-- Rust-Witch
('Creature', 'RustWitch', 'Cursory', NULL, 'Alive', 'A woman fused into a throne of cables.', 1.0, '["Basic", "Horror"]'),
('Creature', 'RustWitch', 'Detailed', NULL, 'Alive', 'She is the queen of the scrap heap. The machines whisper to her, and she screams back. The rust on the walls obeys her command.', 1.0, '["Lore", "Horror"]'),
('Creature', 'RustWitch', 'Expert', NULL, 'Alive', 'A Witch Bound to the Machine-Spirit. She commands the battlefield itself. Can make nearby iron explode. Kill her first.', 1.0, '["Technical", "Horror", "Tactical"]'),

-- Sentinel Prime
('Creature', 'SentinelPrime', 'Cursory', NULL, 'Alive', 'A pristine, towering war-machine in white armor.', 1.0, '["Basic", "Machine"]'),
('Creature', 'SentinelPrime', 'Detailed', NULL, 'Alive', 'The Old World''s wrath incarnate. It shines with a terrible, clean light. Its weapons utilize the invisible fire. It does not speak; it judges.', 1.0, '["Lore", "Machine"]'),
('Creature', 'SentinelPrime', 'Expert', NULL, 'Alive', 'A Knight of the White Flame. Equipped with sun-spears and shifting shields. It learns how you fight. Do not repeat yourself.', 1.0, '["Technical", "Machine", "Tactical"]');


-- ============================================================
-- CATEGORY: BOSS ENEMIES
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Ruin-Warden
('Creature', 'RuinWarden', 'Cursory', NULL, 'Alive', 'A giant in rusted plate, wielding a hammer made of an engine block.', 1.0, '["Basic", "Boss"]'),
('Creature', 'RuinWarden', 'Detailed', NULL, 'Alive', 'The King of the Scrap. He has ruled this sector for centuries. His armor is a patchwork of trophies. He fights with the desperation of a dying god.', 1.0, '["Lore", "Boss"]'),
('Creature', 'RuinWarden', 'Expert', NULL, 'Alive', 'The Lord of the Rust-Kingdom. His strength grows as his blood spills (Berserk). Keep distance when the red rage takes him.', 1.0, '["Technical", "Boss", "Tactical"]'),

-- Aetheric Aberration
('Creature', 'AethericAberration', 'Cursory', NULL, 'Alive', 'A swirling vortex of light and debris.', 1.0, '["Basic", "Boss"]'),
('Creature', 'AethericAberration', 'Detailed', NULL, 'Alive', 'A hole in the world. The laws of nature do not apply here. Looking at it makes your eyes bleed. It is the Blight given form.', 1.0, '["Lore", "Boss"]'),
('Creature', 'AethericAberration', 'Expert', NULL, 'Alive', 'A Ghost from the Void Between Worlds. Iron passes through it like mist. Use runic rites or wait for it to become solid.', 1.0, '["Technical", "Boss", "Tactical"]'),

-- Forlorn Archivist
('Creature', 'ForlornArchivist', 'Cursory', NULL, 'Alive', 'A creature of robes and scrolls, floating above the ground.', 1.0, '["Basic", "Boss"]'),
('Creature', 'ForlornArchivist', 'Detailed', NULL, 'Alive', 'The Keeper of Forbidden Truths. It knows the name of the Glitch. It will unmake your mind with a word.', 1.0, '["Lore", "Boss"]'),
('Creature', 'ForlornArchivist', 'Expert', NULL, 'Alive', 'A Spirit of the Old Words. Summons lesser ghosts. You must have a strong will to survive its whispers.', 1.0, '["Technical", "Boss", "Tactical"]'),

-- Omega Sentinel
('Creature', 'OmegaSentinel', 'Cursory', NULL, 'Alive', 'A walking fortress, bristling with cannons.', 1.0, '["Basic", "Boss"]'),
('Creature', 'OmegaSentinel', 'Detailed', NULL, 'Alive', 'The End of All Things. A weapon of the Last War, woken from its sleep. Its steps shake the citadel. It brings the thunder of the gods.', 1.0, '["Lore", "Boss"]'),
('Creature', 'OmegaSentinel', 'Expert', NULL, 'Alive', 'A Thunder-Bringer of the Gods. First: The Lightning-Spear (Line of Death). Second: The Rain of Fire (No Safe Ground). Third: The Final Light (The Spirit Breaks Free). Run.', 1.0, '["Technical", "Boss", "Tactical"]');
