-- =====================================================
-- v0.38.1: Room Function Variants Content
-- =====================================================
-- Version: v0.38.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-17
-- Prerequisites: v0.38.1 Room Description Library Schema
-- =====================================================
-- Function variants for Chamber rooms (e.g., "Pumping Station", "Forge Hall")
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- CHAMBER FUNCTION VARIANTS
-- =====================================================

INSERT INTO Room_Function_Variants (function_name, function_detail, biome_affinity, archetype, tags) VALUES
('Pumping Station', 'manages hydraulic systems and fluid transfer throughout the facility', '["The_Roots", "Muspelheim"]', 'Chamber', '["Industrial", "Utility", "Hydraulic"]'),
('Storage Bay', 'served as the primary storage depot for industrial supplies', '["The_Roots"]', 'Storage', '["Industrial", "Salvage", "Resources"]'),
('Observation Dome', 'provided panoramic monitoring of the surrounding biome', '["Alfheim", "Niflheim"]', 'Chamber', '["Research", "Elevated", "Vantage"]'),
('Forge Hall', 'housed massive forges for metalworking and fabrication', '["Muspelheim"]', 'Chamber', '["Industrial", "Fire", "Crafting"]'),
('Cryo Chamber', 'preserved biological specimens and personnel in cryogenic suspension', '["Niflheim"]', 'Chamber', '["Cryogenic", "Preservation", "Cold"]'),
('Power Distribution Hub', 'routed electrical power to various facility sectors', '["The_Roots", "Jotunheim"]', 'PowerStation', '["Industrial", "Energy", "Electrical"]'),
('Research Laboratory', 'conducted Aetheric experiments and reality manipulation studies', '["Alfheim"]', 'Laboratory', '["Research", "Aetheric", "Scientific"]'),
('Command Center', 'coordinated facility operations and security protocols', '["Jotunheim", "The_Roots"]', 'Chamber', '["Command", "Military", "Strategic"]'),
('Reactor Chamber', 'housed the primary power generation reactor', '["Muspelheim", "The_Roots"]', 'PowerStation', '["Energy", "Nuclear", "Dangerous"]'),
('Data Archive', 'stored critical facility records and research data', '["Alfheim", "The_Roots"]', 'Laboratory', '["Data", "Research", "Archives"]'),
('Equipment Depot', 'maintained and distributed facility equipment and tools', '["The_Roots", "Jotunheim"]', 'Storage', '["Industrial", "Equipment", "Maintenance"]'),
('Atmospheric Control Station', 'regulated environmental conditions across facility sectors', '["Niflheim", "Alfheim"]', 'Chamber', '["Environmental", "Control", "Climate"]');

-- =====================================================
-- POWER STATION FUNCTION VARIANTS
-- =====================================================

INSERT INTO Room_Function_Variants (function_name, function_detail, biome_affinity, archetype, tags) VALUES
('Geothermal Tap Station', 'extracted and distributed geothermal energy', '["Muspelheim"]', 'PowerStation', '["Energy", "Geothermal", "Heat"]'),
('Fusion Core Chamber', 'housed a pre-Glitch fusion power generator', '["Jotunheim", "The_Roots"]', 'PowerStation', '["Energy", "Fusion", "Advanced"]'),
('Energy Converter Array', 'transformed raw power into usable electrical current', '["The_Roots"]', 'PowerStation', '["Energy", "Electrical", "Conversion"]');

-- =====================================================
-- LABORATORY FUNCTION VARIANTS
-- =====================================================

INSERT INTO Room_Function_Variants (function_name, function_detail, biome_affinity, archetype, tags) VALUES
('Crystallography Lab', 'studied Aetheric crystal formations and properties', '["Alfheim"]', 'Laboratory', '["Research", "Aetheric", "Crystals"]'),
('Containment Testing Facility', 'tested protocols for containing Aetheric anomalies', '["Alfheim"]', 'Laboratory', '["Research", "Containment", "Safety"]'),
('Metallurgy Laboratory', 'analyzed and developed advanced metal alloys', '["Muspelheim", "Jotunheim"]', 'Laboratory', '["Research", "Materials", "Metal"]');

-- =====================================================
-- ADDITIONAL CHAMBER DETAILS
-- =====================================================

INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
-- Function Details for various chamber types
('Detail', 'Function', 'Massive pumps line the walls, their pistons frozen mid-stroke', '["Chamber", "Pumping", "Industrial"]', 1.0),
('Detail', 'Function', 'The forge equipment sits cold and abandoned', '["Chamber", "Forge", "Muspelheim"]', 1.0),
('Detail', 'Function', 'Cryo pods line the walls, their contents frozen in time', '["Chamber", "Cryo", "Niflheim"]', 1.0),
('Detail', 'Function', 'Power conduits converge here from all directions', '["PowerStation", "Energy", "Electrical"]', 1.0),
('Detail', 'Function', 'Research equipment lies scattered and abandoned', '["Laboratory", "Research", "Alfheim"]', 1.0),
('Detail', 'Function', 'Command consoles stand dark and unresponsive', '["Chamber", "Command", "Military"]', 1.0);

-- Energy States for Power Stations
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Energy', 'residual energy, its systems still partially active', '["PowerStation", "Active", "Dangerous"]', 1.1),
('Detail', 'Energy', 'dead power systems, all energy long depleted', '["PowerStation", "Inactive", "Safe"]', 0.9),
('Detail', 'Energy', 'unstable power fluctuations', '["PowerStation", "Unstable", "Hazardous"]', 1.2);

-- Electrical Warnings
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Warning', 'Arcs of electricity occasionally leap between exposed conduits', '["PowerStation", "Electrical", "Hazard"]', 1.0),
('Detail', 'Warning', 'The smell of ozone is overwhelming', '["PowerStation", "Electrical", "Dangerous"]', 1.0),
('Detail', 'Warning', 'Power surges could prove lethal', '["PowerStation", "Electrical", "Deadly"]', 1.1);

-- Research Equipment
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Research', 'advanced analytical equipment', '["Laboratory", "Research", "Technology"]', 1.0),
('Detail', 'Research', 'Aetheric containment vessels', '["Laboratory", "Alfheim", "Aetheric"]', 1.1),
('Detail', 'Research', 'data terminals and holographic projectors', '["Laboratory", "Research", "Data"]', 1.0);

-- Research Focus
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Research', 'The research here focused on reality manipulation', '["Laboratory", "Alfheim", "Aetheric"]', 1.1),
('Detail', 'Research', 'This laboratory studied materials science and metallurgy', '["Laboratory", "Materials", "Science"]', 1.0),
('Detail', 'Research', 'Data archives suggest biological research was conducted here', '["Laboratory", "Biological", "Research"]', 1.0);

-- Military Details
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Military', 'security checkpoints and fortified positions', '["Barracks", "Military", "Security"]', 1.0),
('Detail', 'Military', 'weapon racks stand empty, their contents long since scavenged', '["Barracks", "Military", "Looted"]', 1.0),
('Detail', 'Military', 'tactical displays show ancient defensive positions', '["Barracks", "Military", "Tactical"]', 1.0);

-- Occupant Descriptions
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Occupant', 'facility security forces', '["Barracks", "Military", "Security"]', 1.0),
('Detail', 'Occupant', 'industrial workers', '["Barracks", "Industrial", "Workers"]', 1.0),
('Detail', 'Occupant', 'military personnel', '["Barracks", "Military", "Armed"]', 1.0);

-- Forge Equipment
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Forge', 'Massive anvils and quenching tanks dominate the space', '["Forge", "Muspelheim", "Equipment"]', 1.0),
('Detail', 'Forge', 'The forge fires have long since cooled', '["Forge", "Muspelheim", "Inactive"]', 0.9),
('Detail', 'Forge', 'Metalworking tools lie scattered about', '["Forge", "Muspelheim", "Tools"]', 1.0);

-- Heat Warnings
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Warning', 'The ambient temperature is dangerously high', '["Forge", "Muspelheim", "Heat"]', 1.1),
('Detail', 'Warning', 'Heat radiates from the very walls', '["Forge", "Muspelheim", "Dangerous"]', 1.0),
('Detail', 'Warning', 'Protective gear is essential to survive here', '["Forge", "Muspelheim", "Deadly"]', 1.2);

-- Cryo Contents
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Cryo', 'hundreds of cryogenic suspension pods', '["Cryo", "Niflheim", "Preservation"]', 1.0),
('Detail', 'Cryo', 'biological specimens and research subjects', '["Cryo", "Niflheim", "Research"]', 1.0),
('Detail', 'Cryo', 'facility personnel frozen during the catastrophe', '["Cryo", "Niflheim", "Tragedy"]', 1.1);

-- Cryo Status
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Cryo', 'Many pods show critical failures', '["Cryo", "Niflheim", "Failed"]', 1.0),
('Detail', 'Cryo', 'The cryogenic systems are still partially functional', '["Cryo", "Niflheim", "Active"]', 1.1),
('Detail', 'Cryo', 'Ice has formed over everything', '["Cryo", "Niflheim", "Frozen"]', 1.0);

-- Cold Warnings
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Warning', 'The temperature is lethally cold', '["Cryo", "Niflheim", "Deadly"]', 1.1),
('Detail', 'Warning', 'Frostbite is a constant danger', '["Cryo", "Niflheim", "Hazard"]', 1.0),
('Detail', 'Warning', 'Your breath crystallizes instantly', '["Cryo", "Niflheim", "Extreme"]', 1.0);

-- Visibility Details
INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight) VALUES
('Detail', 'Visibility', 'Clear sightlines make this an excellent ambush point', '["Observation", "Tactical", "Dangerous"]', 1.0),
('Detail', 'Visibility', 'You can see for considerable distance', '["Observation", "Tactical", "Vantage"]', 1.0),
('Detail', 'Visibility', 'The elevated position offers tactical advantages', '["Observation", "Tactical", "Strategic"]', 1.0);

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Function Variants Count
-- SELECT COUNT(*) as function_count FROM Room_Function_Variants;
-- Expected: 18+ function variants

-- Test 2: Functions by Archetype
-- SELECT archetype, COUNT(*) as count FROM Room_Function_Variants GROUP BY archetype;
-- Expected: Chamber: ~8, PowerStation: ~3, Laboratory: ~3, Storage: ~2

-- Test 3: Additional Fragments
-- SELECT subcategory, COUNT(*) as count FROM Descriptor_Fragments WHERE category = 'Detail' GROUP BY subcategory;
-- Expected: Multiple subcategories with 3+ fragments each

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [x] 18+ room function variants
-- [x] All major archetypes covered
-- [x] Biome affinities assigned
-- [x] 30+ additional detail fragments
-- [x] Function-specific descriptions
-- [x] Warning fragments for hazardous rooms
-- =====================================================

-- END v0.38.1 ROOM FUNCTION VARIANTS
