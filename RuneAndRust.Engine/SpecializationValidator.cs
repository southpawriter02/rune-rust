using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public void AddError(string error)
    {
        IsValid = false;
        Errors.Add(error);
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public string GetSummary()
    {
        var summary = IsValid ? "✓ Validation passed" : "✗ Validation failed";
        if (Errors.Count > 0)
        {
            summary += $"\n  {Errors.Count} error(s):";
            foreach (var error in Errors)
            {
                summary += $"\n    • {error}";
            }
        }
        if (Warnings.Count > 0)
        {
            summary += $"\n  {Warnings.Count} warning(s):";
            foreach (var warning in Warnings)
            {
                summary += $"\n    • {warning}";
            }
        }
        return summary;
    }
}

/// <summary>
/// v0.19: Validates specialization integrity and template compliance
/// Ensures all specializations follow conventions and have valid data
/// </summary>
public class SpecializationValidator
{
    private static readonly ILogger _log = Log.ForContext<SpecializationValidator>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public SpecializationValidator(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
        _log.Debug("SpecializationValidator initialized");
    }

    #region Full System Validation

    /// <summary>
    /// Validate all specializations in the database
    /// </summary>
    public ValidationResult ValidateAllSpecializations()
    {
        _log.Information("Starting full specialization validation");

        var result = new ValidationResult();
        var specializations = _specializationRepo.GetAll();

        if (specializations.Count == 0)
        {
            result.AddWarning("No specializations found in database");
            return result;
        }

        _log.Information("Validating {Count} specializations", specializations.Count);

        foreach (var spec in specializations)
        {
            var specResult = ValidateSpecialization(spec.SpecializationID);

            // Merge results
            foreach (var error in specResult.Errors)
            {
                result.AddError($"[{spec.Name}] {error}");
            }
            foreach (var warning in specResult.Warnings)
            {
                result.AddWarning($"[{spec.Name}] {warning}");
            }
        }

        if (result.IsValid)
        {
            _log.Information("All specializations validated successfully");
        }
        else
        {
            _log.Warning("Specialization validation found {ErrorCount} error(s) and {WarningCount} warning(s)",
                result.Errors.Count, result.Warnings.Count);
        }

        return result;
    }

    #endregion

    #region Single Specialization Validation

    /// <summary>
    /// Validate a single specialization by ID
    /// </summary>
    public ValidationResult ValidateSpecialization(int specializationId)
    {
        _log.Debug("Validating specialization {SpecializationID}", specializationId);

        var result = new ValidationResult();
        var spec = _specializationRepo.GetById(specializationId);

        if (spec == null)
        {
            result.AddError($"Specialization {specializationId} not found");
            return result;
        }

        // Get abilities for this specialization
        var abilities = _abilityRepo.GetBySpecialization(specializationId);

        // Run validation rules
        ValidateMetadata(spec, result);
        ValidateAbilityCount(abilities, result);
        ValidateTierStructure(abilities, result);
        ValidatePPCosts(abilities, result);
        ValidateTotalPPCost(abilities, result);
        ValidatePrerequisites(abilities, result);
        ValidateAbilityMetadata(abilities, result);

        return result;
    }

    #endregion

    #region Validation Rules

    /// <summary>
    /// Rule 1: Validate specialization metadata completeness
    /// </summary>
    private void ValidateMetadata(SpecializationData spec, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(spec.Name))
        {
            result.AddError("Specialization name is empty");
        }

        if (string.IsNullOrWhiteSpace(spec.Description))
        {
            result.AddWarning("Specialization description is empty");
        }

        if (string.IsNullOrWhiteSpace(spec.Tagline))
        {
            result.AddWarning("Specialization tagline is empty");
        }

        if (string.IsNullOrWhiteSpace(spec.MechanicalRole))
        {
            result.AddError("Mechanical role is not specified");
        }

        if (string.IsNullOrWhiteSpace(spec.PrimaryAttribute))
        {
            result.AddError("Primary attribute is not specified");
        }

        if (spec.PathType != "Coherent" && spec.PathType != "Heretical")
        {
            result.AddError($"Invalid path type: {spec.PathType} (must be 'Coherent' or 'Heretical')");
        }

        var validTraumaRisks = new[] { "None", "Low", "Medium", "High", "Extreme" };
        if (!validTraumaRisks.Contains(spec.TraumaRisk))
        {
            result.AddError($"Invalid trauma risk: {spec.TraumaRisk}");
        }

        if (spec.PPCostToUnlock <= 0)
        {
            result.AddError($"Invalid PP cost to unlock: {spec.PPCostToUnlock}");
        }

        if (spec.ArchetypeID <= 0)
        {
            result.AddError($"Invalid archetype ID: {spec.ArchetypeID}");
        }
    }

    /// <summary>
    /// Rule 2: Validate ability count (must have exactly 9 abilities)
    /// </summary>
    private void ValidateAbilityCount(List<AbilityData> abilities, ValidationResult result)
    {
        if (abilities.Count != 9)
        {
            result.AddError($"Invalid ability count: {abilities.Count} (expected 9: 3 Tier 1, 3 Tier 2, 2 Tier 3, 1 Capstone)");
        }
    }

    /// <summary>
    /// Rule 3: Validate tier structure (3/3/2/1 pattern)
    /// </summary>
    private void ValidateTierStructure(List<AbilityData> abilities, ValidationResult result)
    {
        var tier1Count = abilities.Count(a => a.TierLevel == 1);
        var tier2Count = abilities.Count(a => a.TierLevel == 2);
        var tier3Count = abilities.Count(a => a.TierLevel == 3);
        var capstoneCount = abilities.Count(a => a.TierLevel == 4);

        if (tier1Count != 3)
        {
            result.AddError($"Tier 1 has {tier1Count} abilities (expected 3)");
        }

        if (tier2Count != 3)
        {
            result.AddError($"Tier 2 has {tier2Count} abilities (expected 3)");
        }

        if (tier3Count != 2)
        {
            result.AddError($"Tier 3 has {tier3Count} abilities (expected 2)");
        }

        if (capstoneCount != 1)
        {
            result.AddError($"Capstone tier has {capstoneCount} abilities (expected 1)");
        }
    }

    /// <summary>
    /// Rule 4: Validate PP costs follow convention
    /// Tier 1: 0 PP (free with specialization)
    /// Tier 2: 4 PP each
    /// Tier 3: 5 PP each
    /// Capstone: 6 PP
    /// </summary>
    private void ValidatePPCosts(List<AbilityData> abilities, ValidationResult result)
    {
        foreach (var ability in abilities)
        {
            int expectedCost = ability.TierLevel switch
            {
                1 => 0, // Tier 1 abilities free
                2 => 4, // Tier 2 abilities
                3 => 5, // Tier 3 abilities
                4 => 6, // Capstone
                _ => -1
            };

            if (expectedCost == -1)
            {
                result.AddError($"Ability '{ability.Name}' has invalid tier: {ability.TierLevel}");
            }
            else if (ability.PPCost != expectedCost)
            {
                result.AddWarning($"Ability '{ability.Name}' costs {ability.PPCost} PP (convention: {expectedCost} PP for Tier {ability.TierLevel})");
            }
        }
    }

    /// <summary>
    /// Rule 5: Validate total PP cost is reasonable
    /// Standard: 0 + (3×4) + (2×5) + 6 = 28 PP total
    /// Acceptable range: 20-35 PP
    /// </summary>
    private void ValidateTotalPPCost(List<AbilityData> abilities, ValidationResult result)
    {
        var totalCost = abilities.Sum(a => a.PPCost);

        if (totalCost < 20 || totalCost > 35)
        {
            result.AddWarning($"Total PP cost is {totalCost} (acceptable range: 20-35, standard: 28)");
        }

        _log.Debug("Total PP cost: {TotalCost}", totalCost);
    }

    /// <summary>
    /// Rule 6: Validate prerequisites are valid
    /// - All referenced abilities must exist in the same specialization
    /// - No circular dependencies
    /// - Tier requirements match PP in tree requirements
    /// </summary>
    private void ValidatePrerequisites(List<AbilityData> abilities, ValidationResult result)
    {
        var abilityIds = abilities.Select(a => a.AbilityID).ToHashSet();

        foreach (var ability in abilities)
        {
            // Check that all prerequisite abilities exist
            foreach (var reqAbilityId in ability.Prerequisites.RequiredAbilityIDs)
            {
                if (!abilityIds.Contains(reqAbilityId))
                {
                    result.AddError($"Ability '{ability.Name}' references non-existent prerequisite ability ID: {reqAbilityId}");
                }
                else
                {
                    // Check that prerequisite is from a lower tier
                    var prereqAbility = abilities.First(a => a.AbilityID == reqAbilityId);
                    if (prereqAbility.TierLevel >= ability.TierLevel)
                    {
                        result.AddError($"Ability '{ability.Name}' (Tier {ability.TierLevel}) has prerequisite '{prereqAbility.Name}' (Tier {prereqAbility.TierLevel}) which is not from a lower tier");
                    }
                }
            }

            // Validate PP in tree requirements match tier
            var expectedPPInTree = ability.TierLevel switch
            {
                1 => 0,
                2 => 8,  // Need 8 PP in tree (Tier 1 abilities + some learning)
                3 => 16, // Need 16 PP in tree
                4 => 24, // Need 24 PP in tree + Tier 3 prerequisites
                _ => 0
            };

            if (ability.Prerequisites.RequiredPPInTree != expectedPPInTree)
            {
                result.AddWarning($"Ability '{ability.Name}' (Tier {ability.TierLevel}) has PPInTree requirement of {ability.Prerequisites.RequiredPPInTree} (convention: {expectedPPInTree})");
            }

            // Capstone should require Tier 3 abilities
            if (ability.TierLevel == 4 && ability.Prerequisites.RequiredAbilityIDs.Count == 0)
            {
                result.AddWarning($"Capstone ability '{ability.Name}' has no prerequisite abilities (convention: require both Tier 3 abilities)");
            }
        }
    }

    /// <summary>
    /// Rule 7: Validate ability metadata completeness
    /// </summary>
    private void ValidateAbilityMetadata(List<AbilityData> abilities, ValidationResult result)
    {
        foreach (var ability in abilities)
        {
            if (string.IsNullOrWhiteSpace(ability.Name))
            {
                result.AddError($"Ability at tier {ability.TierLevel} has empty name");
            }

            if (string.IsNullOrWhiteSpace(ability.Description))
            {
                result.AddWarning($"Ability '{ability.Name}' has empty description");
            }

            if (string.IsNullOrWhiteSpace(ability.MechanicalSummary))
            {
                result.AddWarning($"Ability '{ability.Name}' has empty mechanical summary");
            }

            var validAbilityTypes = new[] { "Active", "Passive", "Reaction" };
            if (!validAbilityTypes.Contains(ability.AbilityType))
            {
                result.AddError($"Ability '{ability.Name}' has invalid type: {ability.AbilityType}");
            }

            var validActionTypes = new[] { "Standard Action", "Bonus Action", "Free Action", "Performance", "Reaction" };
            if (!validActionTypes.Contains(ability.ActionType))
            {
                result.AddError($"Ability '{ability.Name}' has invalid action type: {ability.ActionType}");
            }

            if (ability.MaxRank < 1 || ability.MaxRank > 3)
            {
                result.AddError($"Ability '{ability.Name}' has invalid max rank: {ability.MaxRank} (must be 1-3)");
            }

            // Passive abilities shouldn't have stamina costs
            if (ability.AbilityType == "Passive" && ability.ResourceCost.Stamina > 0)
            {
                result.AddWarning($"Passive ability '{ability.Name}' has stamina cost (typically 0)");
            }
        }
    }

    #endregion

    #region Validation Reports

    /// <summary>
    /// Generate a detailed validation report for all specializations
    /// </summary>
    public string GenerateValidationReport()
    {
        var result = ValidateAllSpecializations();
        var report = "╔══════════════════════════════════════════════════════════════╗\n";
        report += "║           SPECIALIZATION VALIDATION REPORT                  ║\n";
        report += "╚══════════════════════════════════════════════════════════════╝\n\n";

        var specializations = _specializationRepo.GetAll();
        report += $"Total Specializations: {specializations.Count}\n";
        report += $"Validation Status: {(result.IsValid ? "✓ PASSED" : "✗ FAILED")}\n\n";

        if (result.Errors.Count > 0)
        {
            report += $"ERRORS ({result.Errors.Count}):\n";
            foreach (var error in result.Errors)
            {
                report += $"  ✗ {error}\n";
            }
            report += "\n";
        }

        if (result.Warnings.Count > 0)
        {
            report += $"WARNINGS ({result.Warnings.Count}):\n";
            foreach (var warning in result.Warnings)
            {
                report += $"  ⚠ {warning}\n";
            }
            report += "\n";
        }

        if (result.IsValid && result.Errors.Count == 0 && result.Warnings.Count == 0)
        {
            report += "✓ All specializations passed validation with no errors or warnings.\n";
        }

        return report;
    }

    #endregion
}
