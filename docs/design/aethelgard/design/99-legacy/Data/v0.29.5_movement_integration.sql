-- =====================================================
-- v0.29.5: Movement System Integration & Forced Movement
-- =====================================================
-- Version: v0.29.5
-- Author: Rune & Rust Development Team
-- Date: 2025-11-15
-- Prerequisites: v0.29.1 (Muspelheim schema)
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- SCHEMA UPDATES
-- =====================================================

-- Add column to track environmental deaths
-- (separate from heat deaths which are from ambient condition)
ALTER TABLE Characters_BiomeStatus
ADD COLUMN times_died_to_environment INTEGER DEFAULT 0;

-- Update description:
-- times_died_to_heat: Deaths from [Intense Heat] ambient condition
-- times_died_to_environment: Deaths from forced movement into hazards, falling, etc.

COMMIT;

-- =====================================================
-- END v0.29.5 MIGRATION SCRIPT
-- =====================================================
