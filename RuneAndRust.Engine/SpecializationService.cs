using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a specialization operation
/// </summary>
public class SpecializationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SpecializationData? Specialization { get; set; }
    public List<SpecializationData>? Specializations { get; set; }
}

/// <summary>
/// v0.19: Service for managing specialization unlock and validation
/// Handles all business logic for specializations
/// </summary>
public class SpecializationService
{
    private static readonly ILogger _log = Log.ForContext<SpecializationService>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;
    private readonly string _connectionString;

    public SpecializationService(string connectionString)
    {
        _connectionString = connectionString;
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
        _log.Debug("SpecializationService initialized");
    }

    #region Get Specializations

    /// <summary>
    /// Get all available specializations for a character's archetype
    /// Returns specializations with unlock status
    /// </summary>
    public SpecializationResult GetAvailableSpecializations(PlayerCharacter character)
    {
        _log.Debug("Getting available specializations for character: {CharacterName}, Class: {Class}",
            character.Name, character.Class);

        try
        {
            // Map CharacterClass to ArchetypeID (1=Warrior, 2=Adept, 3=Scavenger, 4=Mystic)
            int archetypeId = character.Class switch
            {
                CharacterClass.Warrior => 1,
                CharacterClass.Adept => 2,
                CharacterClass.Scavenger => 3,
                CharacterClass.Mystic => 4,
                _ => 0
            };

            if (archetypeId == 0)
            {
                return new SpecializationResult
                {
                    Success = false,
                    Message = "Invalid character class"
                };
            }

            var specializations = _specializationRepo.GetByArchetype(archetypeId);

            _log.Information("Found {Count} specializations for {Class}",
                specializations.Count, character.Class);

            return new SpecializationResult
            {
                Success = true,
                Specializations = specializations,
                Message = $"Found {specializations.Count} specializations"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error getting available specializations");
            return new SpecializationResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Get a specific specialization by ID
    /// </summary>
    public SpecializationResult GetSpecialization(int specializationId)
    {
        _log.Debug("Getting specialization by ID: {SpecializationID}", specializationId);

        try
        {
            var spec = _specializationRepo.GetById(specializationId);

            if (spec == null)
            {
                return new SpecializationResult
                {
                    Success = false,
                    Message = $"Specialization {specializationId} not found"
                };
            }

            return new SpecializationResult
            {
                Success = true,
                Specialization = spec
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error getting specialization {SpecializationID}", specializationId);
            return new SpecializationResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Unlock Specialization

    /// <summary>
    /// Attempt to unlock a specialization for a character
    /// Validates all requirements before unlocking
    /// </summary>
    public SpecializationResult UnlockSpecialization(PlayerCharacter character, int specializationId)
    {
        _log.Information("Attempting to unlock specialization {SpecializationID} for character {CharacterName}",
            specializationId, character.Name);

        try
        {
            var spec = _specializationRepo.GetById(specializationId);

            if (spec == null)
            {
                _log.Warning("Specialization not found: {SpecializationID}", specializationId);
                return new SpecializationResult
                {
                    Success = false,
                    Message = "Specialization not found"
                };
            }

            // Validate archetype match
            var archetypeValidation = ValidateArchetype(character, spec);
            if (!archetypeValidation.Success)
            {
                return archetypeValidation;
            }

            // Check if already unlocked
            if (_specializationRepo.HasUnlocked(GetCharacterId(character), specializationId))
            {
                _log.Warning("Specialization already unlocked: Character={CharacterName}, Spec={SpecName}",
                    character.Name, spec.Name);
                return new SpecializationResult
                {
                    Success = false,
                    Message = $"You have already unlocked {spec.Name}"
                };
            }

            // Validate unlock requirements
            var requirementsValidation = ValidateUnlockRequirements(character, spec);
            if (!requirementsValidation.Success)
            {
                return requirementsValidation;
            }

            // Validate PP cost
            if (character.ProgressionPoints < spec.PPCostToUnlock)
            {
                _log.Warning("Insufficient PP: Character={CharacterName}, Has={PP}, Needs={Cost}",
                    character.Name, character.ProgressionPoints, spec.PPCostToUnlock);
                return new SpecializationResult
                {
                    Success = false,
                    Message = $"Requires {spec.PPCostToUnlock} PP (you have {character.ProgressionPoints})"
                };
            }

            // All validation passed - unlock specialization
            _specializationRepo.UnlockForCharacter(GetCharacterId(character), specializationId);

            // Deduct PP cost
            character.ProgressionPoints -= spec.PPCostToUnlock;

            _log.Information("Specialization unlocked successfully: Character={CharacterName}, Spec={SpecName}, PPSpent={PP}",
                character.Name, spec.Name, spec.PPCostToUnlock);

            return new SpecializationResult
            {
                Success = true,
                Specialization = spec,
                Message = $"✓ You have unlocked the {spec.Name} specialization! ({spec.PPCostToUnlock} PP spent)"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error unlocking specialization");
            return new SpecializationResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate that character's archetype matches specialization
    /// </summary>
    private SpecializationResult ValidateArchetype(PlayerCharacter character, SpecializationData spec)
    {
        int archetypeId = character.Class switch
        {
            CharacterClass.Warrior => 1,
            CharacterClass.Adept => 2,
            CharacterClass.Scavenger => 3,
            CharacterClass.Mystic => 4,
            _ => 0
        };

        if (spec.ArchetypeID != archetypeId)
        {
            _log.Warning("Archetype mismatch: Character={CharacterName} ({Class}), Spec={SpecName} (requires archetype {ArchetypeID})",
                character.Name, character.Class, spec.Name, spec.ArchetypeID);

            string requiredArchetype = spec.ArchetypeID switch
            {
                1 => "Warrior",
                2 => "Adept",
                3 => "Scavenger",
                4 => "Mystic",
                _ => "Unknown"
            };

            return new SpecializationResult
            {
                Success = false,
                Message = $"This specialization requires the {requiredArchetype} archetype"
            };
        }

        return new SpecializationResult { Success = true };
    }

    /// <summary>
    /// Validate character meets all unlock requirements
    /// </summary>
    private SpecializationResult ValidateUnlockRequirements(PlayerCharacter character, SpecializationData spec)
    {
        if (!spec.UnlockRequirements.IsSatisfiedBy(character))
        {
            var unmetMessage = spec.UnlockRequirements.GetUnmetRequirementsMessage(character);

            _log.Warning("Unlock requirements not met: Character={CharacterName}, Spec={SpecName}, Unmet={Unmet}",
                character.Name, spec.Name, unmetMessage);

            return new SpecializationResult
            {
                Success = false,
                Message = $"Requirements not met: {unmetMessage}"
            };
        }

        return new SpecializationResult { Success = true };
    }

    /// <summary>
    /// Check if character can unlock a specialization (without unlocking it)
    /// </summary>
    public SpecializationResult CanUnlock(PlayerCharacter character, int specializationId)
    {
        var spec = _specializationRepo.GetById(specializationId);

        if (spec == null)
        {
            return new SpecializationResult
            {
                Success = false,
                Message = "Specialization not found"
            };
        }

        // Check archetype
        var archetypeCheck = ValidateArchetype(character, spec);
        if (!archetypeCheck.Success) return archetypeCheck;

        // Check if already unlocked
        if (_specializationRepo.HasUnlocked(GetCharacterId(character), specializationId))
        {
            return new SpecializationResult
            {
                Success = false,
                Message = "Already unlocked"
            };
        }

        // Check requirements
        var requirementsCheck = ValidateUnlockRequirements(character, spec);
        if (!requirementsCheck.Success) return requirementsCheck;

        // Check PP
        if (character.ProgressionPoints < spec.PPCostToUnlock)
        {
            return new SpecializationResult
            {
                Success = false,
                Message = $"Requires {spec.PPCostToUnlock} PP (you have {character.ProgressionPoints})"
            };
        }

        return new SpecializationResult
        {
            Success = true,
            Message = "Can unlock"
        };
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if character has unlocked a specialization
    /// </summary>
    public bool HasUnlocked(PlayerCharacter character, int specializationId)
    {
        return _specializationRepo.HasUnlocked(GetCharacterId(character), specializationId);
    }

    /// <summary>
    /// Get PP spent in a specialization tree
    /// </summary>
    public int GetPPSpentInTree(PlayerCharacter character, int specializationId)
    {
        return _specializationRepo.GetPPSpentInTree(GetCharacterId(character), specializationId);
    }

    /// <summary>
    /// Get all specializations unlocked by character
    /// </summary>
    public List<CharacterSpecialization> GetUnlockedSpecializations(PlayerCharacter character)
    {
        return _specializationRepo.GetUnlockedSpecializations(GetCharacterId(character));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get character ID (currently using hashcode - will need proper ID system)
    /// TODO: PlayerCharacter should have a persistent ID field
    /// </summary>
    private int GetCharacterId(PlayerCharacter character)
    {
        // Temporary solution: use character name hash as ID
        // In production, PlayerCharacter should have a proper ID field
        return character.Name.GetHashCode();
    }

    #endregion
}
