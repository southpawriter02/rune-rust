-- v0.37.3: Inventory & Equipment Commands
-- Database schema updates for carry capacity tracking

-- ====================
-- Characters Table Updates
-- ====================

-- Add carry capacity tracking columns to Characters table
-- These track weight-based encumbrance system
ALTER TABLE Characters
ADD COLUMN current_carry_weight INTEGER DEFAULT 0;

ALTER TABLE Characters
ADD COLUMN max_carry_capacity INTEGER DEFAULT 24;

-- ====================
-- Notes on Carry Capacity
-- ====================

-- The carry capacity system uses a simple slot-based model:
-- - Each item counts as 1 slot by default
-- - Heavy items (2H weapons, heavy armor) could count as 2 slots (future)
-- - Light items (consumables, components) typically don't count (stored separately)
--
-- Default capacity: 24 slots (normal load)
-- - 0-18: Normal (no penalty)
-- - 19-24: Heavy (no penalty, but close to limit)
-- - 25+: Encumbered (-2 FINESSE, movement costs +2 Stamina)
--
-- Capacity can be increased by:
-- - Strength-based characters (Warriors, Sturdiness builds)
-- - Special equipment (backpacks, carrying gear)
-- - Perks/abilities (future)

-- ====================
-- Existing Tables (No Changes Required)
-- ====================

-- Equipment and Inventory tracking already handled by:
-- - Equipment table (from v0.3)
-- - PlayerInventory table (from v0.3)
-- - Consumables table (from v0.7)
-- - CraftingComponents table (from v0.7)

-- ====================
-- Example Queries
-- ====================

-- Get player's current inventory status:
-- SELECT
--     character_id,
--     character_name,
--     current_carry_weight,
--     max_carry_capacity,
--     CASE
--         WHEN current_carry_weight >= max_carry_capacity THEN 'Encumbered'
--         WHEN current_carry_weight >= max_carry_capacity * 0.75 THEN 'Heavy'
--         ELSE 'Normal'
--     END AS carry_status
-- FROM Characters
-- WHERE character_id = ?;

-- Update carry weight after picking up item:
-- UPDATE Characters
-- SET current_carry_weight = current_carry_weight + 1
-- WHERE character_id = ?;

-- Update carry weight after dropping item:
-- UPDATE Characters
-- SET current_carry_weight = current_carry_weight - 1
-- WHERE character_id = ? AND current_carry_weight > 0;
