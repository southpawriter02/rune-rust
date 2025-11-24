using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.23 Master Seeder: Orchestrates seeding of all boss encounter data
/// Ensures proper initialization order for foreign key relationships
/// </summary>
public class BossMasterSeeder
{
    private static readonly ILogger _log = Log.ForContext<BossMasterSeeder>();
    private readonly BossEncounterRepository _repository;

    public BossMasterSeeder(BossEncounterRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Seed all boss encounter data in correct dependency order:
    /// 1. Boss encounters (v0.23.1)
    /// 2. Boss abilities (v0.23.2)
    /// 3. Boss loot tables (v0.23.3)
    /// </summary>
    public void SeedAllBossData()
    {
        _log.Information("TPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPW");
        _log.Information("Q v0.23 BOSS SYSTEM - MASTER SEEDER");
        _log.Information("ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");

        try
        {
            // Phase 1: Seed boss encounter configurations (v0.23.1)
            _log.Information("Phase 1: Seeding boss encounters and phase definitions...");
            var encounterSeeder = new BossEncounterSeeder(_repository);
            encounterSeeder.SeedBossEncounters();
            _log.Information(" Boss encounters seeded successfully");

            // Phase 2: Seed boss abilities and AI patterns (v0.23.2)
            _log.Information("Phase 2: Seeding boss abilities and telegraphed attacks...");
            var abilitySeeder = new BossAbilitySeeder(_repository);
            abilitySeeder.SeedBossAbilities();
            _log.Information(" Boss abilities seeded successfully");

            // Phase 3: Seed boss loot tables and artifacts (v0.23.3)
            _log.Information("Phase 3: Seeding boss loot tables and legendary artifacts...");
            var lootSeeder = new BossLootSeeder(_repository);
            lootSeeder.SeedBossLoot();
            _log.Information(" Boss loot seeded successfully");

            _log.Information("TPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPW");
            _log.Information("Q  BOSS SYSTEM SEEDING COMPLETE");
            _log.Information("`PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPc");
            _log.Information("Q Seeded Data Summary:");
            _log.Information("Q - 4 Boss Encounters (Ruin-Warden, Aetheric Aberration,");
            _log.Information("Q                      Forlorn Archivist, Omega Sentinel)");
            _log.Information("Q - 12 Phase Definitions (3 phases per boss)");
            _log.Information("Q - 12 AI Patterns (adaptive behavior per phase)");
            _log.Information("Q - 40+ Boss Abilities (standard, telegraphed, ultimates)");
            _log.Information("Q - 4 Loot Tables (quality tiers, currency, materials)");
            _log.Information("Q - 15+ Legendary Artifacts (3 complete sets)");
            _log.Information("Q - 6 Set Bonuses (2pc/4pc for each set)");
            _log.Information("Q - 4 Once-Per-Character Unique Items");
            _log.Information("ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to seed boss system data");
            throw;
        }
    }

    /// <summary>
    /// Validate that all boss data was seeded correctly
    /// </summary>
    public BossSystemValidationResult ValidateSeedData()
    {
        _log.Information("Validating seeded boss data...");

        var result = new BossSystemValidationResult();

        try
        {
            // Validate boss encounters
            for (int encounterId = 1; encounterId <= 4; encounterId++)
            {
                var encounter = _repository.GetBossEncounterByEncounterId(encounterId);
                if (encounter == null)
                {
                    result.Errors.Add($"Boss encounter {encounterId} not found");
                    continue;
                }

                result.BossEncountersValidated++;

                // Validate phases
                var phases = _repository.GetPhaseDefinitions(encounter.BossEncounterId);
                result.PhasesValidated += phases.Count;

                if (phases.Count != encounter.TotalPhases)
                {
                    result.Warnings.Add($"Boss {encounter.BossName}: Expected {encounter.TotalPhases} phases, found {phases.Count}");
                }

                // Validate AI patterns
                var aiPatterns = _repository.GetBossAIPatterns(encounter.BossEncounterId);
                result.AIpatternsValidated += aiPatterns.Count;

                if (aiPatterns.Count != encounter.TotalPhases)
                {
                    result.Warnings.Add($"Boss {encounter.BossName}: Expected {encounter.TotalPhases} AI patterns, found {aiPatterns.Count}");
                }

                // Validate abilities
                var abilities = _repository.GetBossAbilities(encounter.BossEncounterId);
                result.AbilitiesValidated += abilities.Count;

                if (abilities.Count == 0)
                {
                    result.Errors.Add($"Boss {encounter.BossName}: No abilities found");
                }

                // Validate loot table
                var lootTable = _repository.GetBossLootTable(encounter.BossEncounterId);
                if (lootTable != null)
                {
                    result.LootTablesValidated++;
                }
                else
                {
                    result.Warnings.Add($"Boss {encounter.BossName}: No loot table found");
                }

                // Validate unique items
                var uniqueItems = _repository.GetBossUniqueItems(encounter.BossEncounterId);
                result.UniqueItemsValidated += uniqueItems.Count;
            }

            // Validate artifact sets
            var guardianSet = _repository.GetArtifactsBySet("Guardian's Aegis");
            var voidSet = _repository.GetArtifactsBySet("Void-Touched Vestments");
            var reaverSet = _repository.GetArtifactsBySet("Shadow Reaver's Arsenal");

            result.ArtifactsValidated = guardianSet.Count + voidSet.Count + reaverSet.Count;

            if (guardianSet.Count < 4)
            {
                result.Warnings.Add($"Guardian's Aegis: Expected 4+ pieces, found {guardianSet.Count}");
            }

            if (voidSet.Count < 4)
            {
                result.Warnings.Add($"Void-Touched Vestments: Expected 4+ pieces, found {voidSet.Count}");
            }

            if (reaverSet.Count < 4)
            {
                result.Warnings.Add($"Shadow Reaver's Arsenal: Expected 4+ pieces, found {reaverSet.Count}");
            }

            // Validate set bonuses
            var guardianBonuses = _repository.GetSetBonuses("Guardian's Aegis");
            var voidBonuses = _repository.GetSetBonuses("Void-Touched Vestments");
            var reaverBonuses = _repository.GetSetBonuses("Shadow Reaver's Arsenal");

            result.SetBonusesValidated = guardianBonuses.Count + voidBonuses.Count + reaverBonuses.Count;

            result.IsValid = result.Errors.Count == 0;

            // Log validation results
            _log.Information("Validation Results:");
            _log.Information("  Boss Encounters: {Count}", result.BossEncountersValidated);
            _log.Information("  Phase Definitions: {Count}", result.PhasesValidated);
            _log.Information("  AI Patterns: {Count}", result.AIpatternsValidated);
            _log.Information("  Boss Abilities: {Count}", result.AbilitiesValidated);
            _log.Information("  Loot Tables: {Count}", result.LootTablesValidated);
            _log.Information("  Artifacts: {Count}", result.ArtifactsValidated);
            _log.Information("  Set Bonuses: {Count}", result.SetBonusesValidated);
            _log.Information("  Unique Items: {Count}", result.UniqueItemsValidated);
            _log.Information("  Errors: {Count}", result.Errors.Count);
            _log.Information("  Warnings: {Count}", result.Warnings.Count);

            if (result.Errors.Any())
            {
                _log.Error("Validation errors found:");
                foreach (var error in result.Errors)
                {
                    _log.Error("  - {Error}", error);
                }
            }

            if (result.Warnings.Any())
            {
                _log.Warning("Validation warnings:");
                foreach (var warning in result.Warnings)
                {
                    _log.Warning("  - {Warning}", warning);
                }
            }

            if (result.IsValid)
            {
                _log.Information(" All boss data validated successfully");
            }
            else
            {
                _log.Error(" Boss data validation failed");
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Validation failed with exception");
            result.IsValid = false;
            result.Errors.Add($"Validation exception: {ex.Message}");
        }

        return result;
    }
}

/// <summary>
/// Validation result for boss system seeding
/// </summary>
public class BossSystemValidationResult
{
    public bool IsValid { get; set; } = true;
    public int BossEncountersValidated { get; set; }
    public int PhasesValidated { get; set; }
    public int AIpatternsValidated { get; set; }
    public int AbilitiesValidated { get; set; }
    public int LootTablesValidated { get; set; }
    public int ArtifactsValidated { get; set; }
    public int SetBonusesValidated { get; set; }
    public int UniqueItemsValidated { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
