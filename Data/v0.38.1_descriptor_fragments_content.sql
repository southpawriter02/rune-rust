-- =====================================================
-- v0.38.1: Descriptor Fragments Content
-- =====================================================
-- Version: v0.38.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-17
-- Prerequisites: v0.38.1 Room Description Library Schema
-- =====================================================
-- This file contains 20+ descriptor fragments for room generation
-- Categories: SpatialDescriptor, ArchitecturalFeature, Detail, Atmospheric
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- SPATIAL DESCRIPTORS (7+ fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('SpatialDescriptor', 'Confined', 'The ceiling presses low overhead, and the walls feel uncomfortably close', '["Small", "Narrow", "Corridor"]', 1.0),
('SpatialDescriptor', 'Vast', 'The chamber is vast, its far walls barely visible in the dim light', '["Large", "Chamber", "Arena"]', 1.0),
('SpatialDescriptor', 'Vertical', 'The space extends dramatically upward, disappearing into darkness above', '["Vertical", "Shaft", "Canopy"]', 1.0),
('SpatialDescriptor', 'Cramped', 'There''s barely room to maneuver in this tight space', '["Small", "Narrow", "Secret"]', 0.8),
('SpatialDescriptor', 'Cavernous', 'The room is cavernous, your footsteps echoing into the distance', '["Large", "Chamber", "Arena"]', 1.2),
('SpatialDescriptor', 'Narrow', 'The passage is narrow, forcing single-file movement', '["Small", "Corridor", "Transit"]', 1.0),
('SpatialDescriptor', 'Sprawling', 'The chamber sprawls in multiple directions, irregular and expansive', '["Large", "Junction", "Hub"]', 1.0),
('SpatialDescriptor', 'Oppressive', 'The space feels oppressive, as if the walls themselves are malevolent', '["Medium", "Industrial", "Dark"]', 0.9);

-- =====================================================
-- ARCHITECTURAL FEATURES - WALLS (4 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('ArchitecturalFeature', 'Wall', 'Corroded metal plates form the walls, held together by massive rivets', '["Industrial", "The_Roots", "Rusted"]', 1.0),
('ArchitecturalFeature', 'Wall', 'The walls are reinforced with massive girders and support struts', '["Industrial", "Fortified", "Structural"]', 1.0),
('ArchitecturalFeature', 'Wall', 'Smooth, seamless walls suggest advanced pre-Glitch fabrication', '["Alfheim", "Advanced", "Pristine"]', 0.8),
('ArchitecturalFeature', 'Wall', 'The walls are rough-hewn stone, ancient and weathered', '["Jotunheim", "Ancient", "Monolithic"]', 0.9);

-- =====================================================
-- ARCHITECTURAL FEATURES - CEILINGS (4 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('ArchitecturalFeature', 'Ceiling', 'The ceiling is a tangle of exposed conduits and pipes', '["Industrial", "The_Roots", "Utility"]', 1.0),
('ArchitecturalFeature', 'Ceiling', 'A vaulted ceiling arches overhead, built on a grand scale', '["Large", "Chamber", "Monolithic"]', 1.0),
('ArchitecturalFeature', 'Ceiling', 'The ceiling has partially collapsed, revealing the structure above', '["Damaged", "Rusted", "Hazardous"]', 0.8),
('ArchitecturalFeature', 'Ceiling', 'The ceiling is studded with defunct light panels, their glow long extinguished', '["Industrial", "Dark", "Abandoned"]', 1.0);

-- =====================================================
-- ARCHITECTURAL FEATURES - FLOORS (4 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('ArchitecturalFeature', 'Floor', 'The floor is corrugated metal grating, revealing machinery below', '["Industrial", "Grating", "Transparent"]', 1.0),
('ArchitecturalFeature', 'Floor', 'Smooth stone flags pave the floor, worn by countless footsteps', '["Ancient", "Stone", "Monolithic"]', 0.9),
('ArchitecturalFeature', 'Floor', 'The floor is littered with debris and rubble from structural failure', '["Damaged", "Hazardous", "Rusted"]', 0.8),
('ArchitecturalFeature', 'Floor', 'Industrial tiles, cracked and discolored, cover the floor', '["Industrial", "Damaged", "The_Roots"]', 1.0);

-- =====================================================
-- DETAIL FRAGMENTS - DECAY (5 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Decay', 'Rust streaks mark the surfaces like old blood', '["Rusted", "The_Roots", "Corrosion"]', 1.2),
('Detail', 'Decay', 'Corrosion has eaten through many of the structural supports', '["Rusted", "The_Roots", "Dangerous"]', 1.0),
('Detail', 'Decay', 'The walls are pitted and scarred by centuries of neglect', '["Ancient", "Damaged", "Weathered"]', 1.0),
('Detail', 'Decay', 'Everything here shows signs of advanced degradation', '["Rusted", "The_Roots", "Decay"]', 1.1),
('Detail', 'Decay', 'The metal surfaces are crusted with layers of oxidation', '["Rusted", "Corrosion", "Industrial"]', 1.0);

-- =====================================================
-- DETAIL FRAGMENTS - RUNES (4 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Runes', 'Runic glyphs flicker weakly on the walls, their light stuttering', '["Runic", "Mystical", "Failing"]', 1.0),
('Detail', 'Runes', 'Inscribed runes pulse with an irregular rhythm', '["Runic", "Mystical", "Active"]', 0.9),
('Detail', 'Runes', 'The walls bear ancient runic inscriptions, now barely legible', '["Runic", "Ancient", "Faded"]', 0.8),
('Detail', 'Runes', 'Glowing sigils cast eerie shadows that seem to move independently', '["Runic", "Mystical", "Ominous"]', 1.1);

-- =====================================================
-- DETAIL FRAGMENTS - ACTIVITY (4 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Activity', 'Fresh tracks mar the dust on the floor', '["Recent", "Enemy", "Warning"]', 1.0),
('Detail', 'Activity', 'Something has passed through here recently, disturbing the decay', '["Recent", "Enemy", "Warning"]', 0.9),
('Detail', 'Activity', 'The debris has been moved aside, creating a clear path', '["Recent", "Cleared", "Inhabited"]', 0.8),
('Detail', 'Activity', 'Evidence of habitation—or something worse—is present', '["Inhabited", "Enemy", "Warning"]', 1.1);

-- =====================================================
-- ATMOSPHERIC DETAILS (6 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Atmospheric', 'Smell', 'smells of rust and stale water', '["Rusted", "The_Roots", "Damp"]', 1.0),
('Atmospheric', 'Smell', 'is thick with the metallic tang of corrosion', '["Rusted", "The_Roots", "Metallic"]', 1.0),
('Atmospheric', 'Smell', 'carries the scent of decay and neglect', '["Decay", "Abandoned", "Stale"]', 0.9),
('Atmospheric', 'Smell', 'is thick with the smell of brimstone and superheated metal', '["Scorched", "Muspelheim", "Fire"]', 1.2),
('Atmospheric', 'Smell', 'burns your lungs with each breath', '["Scorched", "Muspelheim", "Toxic"]', 1.1),
('Atmospheric', 'Smell', 'carries the sharp scent of ozone and frozen moisture', '["Frozen", "Niflheim", "Cold"]', 1.0);

-- =====================================================
-- DIRECTION DESCRIPTORS (6 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Direction', NULL, 'before you, narrowing into darkness', '["Corridor", "Forward"]', 1.0),
('Direction', NULL, 'ahead, its far end lost in shadow', '["Corridor", "Long"]', 1.0),
('Direction', NULL, 'to the left and right, branching ominously', '["Junction", "Branching"]', 1.0),
('Direction', NULL, 'upward into the darkness above', '["Vertical", "Shaft", "Ascending"]', 1.0),
('Direction', NULL, 'downward into the depths below', '["Vertical", "Shaft", "Descending"]', 1.0),
('Direction', NULL, 'in multiple directions, creating a maze-like network', '["Junction", "Complex"]', 0.9);

-- =====================================================
-- OMINOUS DETAILS (4 fragments for Boss Arenas)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Ominous', 'The silence here is absolute, broken only by your own breathing', '["Boss", "Arena", "Ominous"]', 1.0),
('Detail', 'Ominous', 'Something massive once occupied this space, and may do so again', '["Boss", "Arena", "Warning"]', 1.1),
('Detail', 'Ominous', 'The air itself feels charged with malevolent potential', '["Boss", "Arena", "Dangerous"]', 1.0),
('Detail', 'Ominous', 'Ancient bloodstains mark the floor in disturbingly large patterns', '["Boss", "Arena", "Battle"]', 1.2);

-- =====================================================
-- LOOT HINTS (3 fragments for Secret Rooms)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Loot', 'Valuable salvage glints among the wreckage', '["Secret", "Loot", "Treasure"]', 1.0),
('Detail', 'Loot', 'This place was clearly used to store items of value', '["Secret", "Loot", "Storage"]', 1.0),
('Detail', 'Loot', 'The air shimmers with the residual Aether of powerful artifacts', '["Secret", "Loot", "Magical"]', 1.2);

-- =====================================================
-- EXIT DESCRIPTIONS (3 fragments for Junctions)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Exits', 'Passages lead off in three distinct directions', '["Junction", "Branching"]', 1.0),
('Detail', 'Exits', 'Four corridors converge here, each promising different challenges', '["Junction', 'Branching"]', 1.0),
('Detail', 'Exits', 'Multiple exits offer choices, each as ominous as the last', '["Junction", "Branching", "Ominous"]', 1.1);

-- =====================================================
-- TRAVERSAL WARNINGS (3 fragments for Vertical Shafts)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Warning', 'The climb looks treacherous and potentially deadly', '["Vertical", "Dangerous", "Climbing"]', 1.0),
('Detail', 'Warning', 'Rusted rungs and crumbling handholds make this a perilous ascent', '["Vertical", "Dangerous", "Rusted"]', 1.1),
('Detail', 'Warning', 'One misstep here would be fatal', '["Vertical", "Dangerous", "Warning"]', 1.2);

-- =====================================================
-- INDUSTRIAL DETAILS (4 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Industrial', 'Pipes and conduits snake across every surface', '["Industrial", "Utility", "Complex"]', 1.0),
('Detail', 'Industrial', 'The machinery here is massive, built on an industrial scale', '["Industrial", "Large", "Machinery"]', 1.0),
('Detail', 'Industrial', 'Control panels line the walls, their displays long dead', '["Industrial", "Abandoned", "Technology"]', 0.9),
('Detail', 'Industrial', 'The equipment here once served a vital function', '["Industrial", "Functional", "Abandoned"]', 1.0);

-- =====================================================
-- STORAGE CONTENTS (4 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Storage', 'rows of supply crates, most already pried open and looted', '["Storage", "Salvage", "Looted"]', 1.0),
('Detail', 'Storage', 'industrial equipment and spare parts', '["Storage", "Industrial", "Parts"]', 1.0),
('Detail', 'Storage', 'shelves of sealed containers, their contents unknown', '["Storage", "Mystery", "Sealed"]', 1.1),
('Detail', 'Storage', 'material salvage and scrap metal', '["Storage", "Salvage", "Scrap"]', 1.0);

-- =====================================================
-- SALVAGE POTENTIAL (3 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Salvage', 'Useful salvage could be recovered here with effort', '["Salvage", "Loot", "Resources"]', 1.0),
('Detail', 'Salvage', 'Much of the equipment, though damaged, retains salvageable components', '["Salvage", "Industrial", "Parts"]', 1.0),
('Detail', 'Salvage', 'This is a scavenger''s dream, despite the dangers', '["Salvage", "Loot", "Dangerous"]', 1.1);

-- =====================================================
-- VANTAGE DESCRIPTIONS (3 fragments)
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Vantage', 'a commanding view of the surrounding area', '["Observation", "Tactical", "Elevated"]', 1.0),
('Detail', 'Vantage', 'an elevated position with clear sightlines', '["Observation", "Tactical", "Cover"]', 1.0),
('Detail', 'Vantage', 'a tactical advantage, though also exposure', '["Observation", "Tactical", "Risk"]', 1.1);

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Fragment Count by Category
-- SELECT category, COUNT(*) as count FROM Descriptor_Fragments GROUP BY category;
-- Expected: SpatialDescriptor: 8, ArchitecturalFeature: 12, Detail: 28+, Atmospheric: 6, Direction: 6

-- Test 2: Total Fragments
-- SELECT COUNT(*) as total_fragments FROM Descriptor_Fragments;
-- Expected: 60+ fragments

-- Test 3: Tagged Fragments
-- SELECT COUNT(*) as tagged_count FROM Descriptor_Fragments WHERE tags IS NOT NULL;
-- Expected: 60+ (all fragments should have tags)

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [x] 8+ spatial descriptors
-- [x] 12+ architectural features (walls, ceilings, floors)
-- [x] 28+ detail fragments (decay, runes, activity, etc.)
-- [x] 6+ atmospheric details
-- [x] 6+ direction descriptors
-- [x] All fragments tagged for filtering
-- [x] Weighted for random selection
-- [x] Total: 60+ descriptor fragments
-- =====================================================

-- END v0.38.1 DESCRIPTOR FRAGMENTS CONTENT
