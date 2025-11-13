using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of an ability operation
/// </summary>
public class AbilityResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AbilityData? Ability { get; set; }
    public List<AbilityData>? Abilities { get; set; }
    public int PPCost { get; set; }
}

/// <summary>
/// v0.19: Service for managing ability learning, ranking, and usage
/// Handles all business logic for abilities
/// </summary>
public class AbilityService
{
    private static readonly ILogger _log = Log.ForContext<AbilityService>();
    private readonly AbilityRepository _abilityRepo;
    private readonly SpecializationRepository _specializationRepo;
    private readonly string _connectionString;

    public AbilityService(string connectionString)
    {
        _connectionString = connectionString;
        _abilityRepo = new AbilityRepository(connectionString);
        _specializationRepo = new SpecializationRepository(connectionString);
        _log.Debug("AbilityService initialized");
    }

    #region Get Abilities

    /// <summary>
    /// Get all abilities for a specialization, organized by tier
    /// </summary>
    public AbilityResult GetAbilitiesForSpecialization(int specializationId)
    {
        _log.Debug("Getting abilities for specialization: {SpecializationID}", specializationId);

        try
        {
            var abilities = _abilityRepo.GetBySpecialization(specializationId);

            _log.Information("Found {Count} abilities for specialization {SpecializationID}",
                abilities.Count, specializationId);

            return new AbilityResult
            {
                Success = true,
                Abilities = abilities,
                Message = $"Found {abilities.Count} abilities"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error getting abilities for specialization {SpecializationID}", specializationId);
            return new AbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Get abilities by tier for a specialization
    /// </summary>
    public AbilityResult GetAbilitiesByTier(int specializationId, int tierLevel)
    {
        _log.Debug("Getting Tier {TierLevel} abilities for specialization {SpecializationID}",
            tierLevel, specializationId);

        try
        {
            var abilities = _abilityRepo.GetBySpecializationAndTier(specializationId, tierLevel);

            return new AbilityResult
            {
                Success = true,
                Abilities = abilities,
                Message = $"Found {abilities.Count} Tier {tierLevel} abilities"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error getting Tier {TierLevel} abilities", tierLevel);
            return new AbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Get a specific ability by name for a specialization
    /// </summary>
    public AbilityResult GetAbilitiesByName(int specializationId, string abilityName)
    {
        _log.Debug("Getting ability {AbilityName} for specialization {SpecializationID}",
            abilityName, specializationId);

        try
        {
            var abilities = _abilityRepo.GetBySpecialization(specializationId);
            var ability = abilities.FirstOrDefault(a =>
                a.Name.Equals(abilityName, StringComparison.OrdinalIgnoreCase));

            if (ability == null)
            {
                return new AbilityResult
                {
                    Success = false,
                    Message = $"Ability '{abilityName}' not found for specialization {specializationId}"
                };
            }

            return new AbilityResult
            {
                Success = true,
                Ability = ability,
                Message = $"Found ability: {ability.Name}"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error getting ability {AbilityName}", abilityName);
            return new AbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Get a specific ability by ID
    /// </summary>
    public AbilityResult GetAbility(int abilityId)
    {
        _log.Debug("Getting ability by ID: {AbilityID}", abilityId);

        try
        {
            var ability = _abilityRepo.GetById(abilityId);

            if (ability == null)
            {
                return new AbilityResult
                {
                    Success = false,
                    Message = $"Ability {abilityId} not found"
                };
            }

            return new AbilityResult
            {
                Success = true,
                Ability = ability
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error getting ability {AbilityID}", abilityId);
            return new AbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Learn Ability

    /// <summary>
    /// Attempt to learn an ability
    /// Validates prerequisites, PP cost, and specialization unlock
    /// </summary>
    public AbilityResult LearnAbility(PlayerCharacter character, int abilityId)
    {
        _log.Information("Attempting to learn ability {AbilityID} for character {CharacterName}",
            abilityId, character.Name);

        try
        {
            var ability = _abilityRepo.GetById(abilityId);

            if (ability == null)
            {
                _log.Warning("Ability not found: {AbilityID}", abilityId);
                return new AbilityResult
                {
                    Success = false,
                    Message = "Ability not found"
                };
            }

            var characterId = GetCharacterId(character);

            // Check if already learned
            if (_abilityRepo.HasLearned(characterId, abilityId))
            {
                _log.Warning("Ability already learned: Character={CharacterName}, Ability={AbilityName}",
                    character.Name, ability.Name);
                return new AbilityResult
                {
                    Success = false,
                    Message = $"You have already learned {ability.Name}"
                };
            }

            // Validate specialization is unlocked
            var specializationValidation = ValidateSpecializationUnlocked(character, ability);
            if (!specializationValidation.Success)
            {
                return specializationValidation;
            }

            // Validate prerequisites
            var prerequisiteValidation = ValidatePrerequisites(character, ability);
            if (!prerequisiteValidation.Success)
            {
                return prerequisiteValidation;
            }

            // Validate PP cost
            if (character.ProgressionPoints < ability.PPCost)
            {
                _log.Warning("Insufficient PP: Character={CharacterName}, Has={PP}, Needs={Cost}",
                    character.Name, character.ProgressionPoints, ability.PPCost);
                return new AbilityResult
                {
                    Success = false,
                    Message = $"Requires {ability.PPCost} PP (you have {character.ProgressionPoints})"
                };
            }

            // All validation passed - learn ability
            _abilityRepo.LearnForCharacter(characterId, abilityId);

            // Deduct PP cost
            character.ProgressionPoints -= ability.PPCost;

            // Update PP spent in tree
            _specializationRepo.UpdatePPSpentInTree(characterId, ability.SpecializationID, ability.PPCost);

            _log.Information("Ability learned successfully: Character={CharacterName}, Ability={AbilityName}, PPSpent={PP}",
                character.Name, ability.Name, ability.PPCost);

            return new AbilityResult
            {
                Success = true,
                Ability = ability,
                PPCost = ability.PPCost,
                Message = $"✓ You have learned {ability.Name}! ({ability.PPCost} PP spent)"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error learning ability");
            return new AbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Rank Up Ability

    /// <summary>
    /// Attempt to rank up an ability from Rank 1→2 or Rank 2→3
    /// </summary>
    public AbilityResult RankUpAbility(PlayerCharacter character, int abilityId)
    {
        _log.Information("Attempting to rank up ability {AbilityID} for character {CharacterName}",
            abilityId, character.Name);

        try
        {
            var ability = _abilityRepo.GetById(abilityId);

            if (ability == null)
            {
                return new AbilityResult
                {
                    Success = false,
                    Message = "Ability not found"
                };
            }

            var characterId = GetCharacterId(character);

            // Check if ability is learned
            if (!_abilityRepo.HasLearned(characterId, abilityId))
            {
                _log.Warning("Cannot rank up unlearned ability: {AbilityName}", ability.Name);
                return new AbilityResult
                {
                    Success = false,
                    Message = $"You have not learned {ability.Name} yet"
                };
            }

            var currentRank = _abilityRepo.GetCurrentRank(characterId, abilityId);

            // Determine rank up cost
            int rankUpCost = 0;
            int nextRank = currentRank + 1;

            if (nextRank == 2)
            {
                rankUpCost = ability.CostToRank2;
            }
            else if (nextRank == 3)
            {
                rankUpCost = ability.CostToRank3;
            }
            else
            {
                _log.Warning("Ability already at max rank: {AbilityName}, Rank={Rank}", ability.Name, currentRank);
                return new AbilityResult
                {
                    Success = false,
                    Message = $"{ability.Name} is already at maximum rank ({currentRank})"
                };
            }

            // Check if rank up is available
            if (rankUpCost == 0)
            {
                _log.Warning("Rank {NextRank} not available for ability: {AbilityName}", nextRank, ability.Name);
                return new AbilityResult
                {
                    Success = false,
                    Message = $"Rank {nextRank} is not available for {ability.Name}"
                };
            }

            // Validate PP cost
            if (character.ProgressionPoints < rankUpCost)
            {
                _log.Warning("Insufficient PP for rank up: Character={CharacterName}, Has={PP}, Needs={Cost}",
                    character.Name, character.ProgressionPoints, rankUpCost);
                return new AbilityResult
                {
                    Success = false,
                    Message = $"Requires {rankUpCost} PP (you have {character.ProgressionPoints})"
                };
            }

            // All validation passed - rank up ability
            _abilityRepo.RankUp(characterId, abilityId);

            // Deduct PP cost
            character.ProgressionPoints -= rankUpCost;

            // Update PP spent in tree
            _specializationRepo.UpdatePPSpentInTree(characterId, ability.SpecializationID, rankUpCost);

            _log.Information("Ability ranked up: Character={CharacterName}, Ability={AbilityName}, NewRank={Rank}, PPSpent={PP}",
                character.Name, ability.Name, nextRank, rankUpCost);

            return new AbilityResult
            {
                Success = true,
                Ability = ability,
                PPCost = rankUpCost,
                Message = $"✓ {ability.Name} ranked up to Rank {nextRank}! ({rankUpCost} PP spent)"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error ranking up ability");
            return new AbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate that character has unlocked the ability's specialization
    /// </summary>
    private AbilityResult ValidateSpecializationUnlocked(PlayerCharacter character, AbilityData ability)
    {
        var characterId = GetCharacterId(character);

        if (!_specializationRepo.HasUnlocked(characterId, ability.SpecializationID))
        {
            var spec = _specializationRepo.GetById(ability.SpecializationID);
            var specName = spec?.Name ?? $"Specialization {ability.SpecializationID}";

            _log.Warning("Specialization not unlocked: Character={CharacterName}, Spec={SpecName}",
                character.Name, specName);

            return new AbilityResult
            {
                Success = false,
                Message = $"You must unlock the {specName} specialization first"
            };
        }

        return new AbilityResult { Success = true };
    }

    /// <summary>
    /// Validate character meets all ability prerequisites
    /// </summary>
    private AbilityResult ValidatePrerequisites(PlayerCharacter character, AbilityData ability)
    {
        var characterId = GetCharacterId(character);

        // Get PP spent in this specialization tree
        var ppSpentInTree = _specializationRepo.GetPPSpentInTree(characterId, ability.SpecializationID);

        // Get all learned ability IDs for this character
        var learnedAbilities = _abilityRepo.GetLearnedAbilities(characterId);
        var learnedAbilityIDs = learnedAbilities.Select(ca => ca.AbilityID).ToList();

        // Check prerequisites
        if (!ability.Prerequisites.IsSatisfiedBy(character, ability.SpecializationID, learnedAbilityIDs, ppSpentInTree))
        {
            // Build ability name lookup for better error messages
            var abilityNames = new Dictionary<int, string>();
            foreach (var reqAbilityId in ability.Prerequisites.RequiredAbilityIDs)
            {
                var reqAbility = _abilityRepo.GetById(reqAbilityId);
                if (reqAbility != null)
                {
                    abilityNames[reqAbilityId] = reqAbility.Name;
                }
            }

            var unmetMessage = ability.Prerequisites.GetUnmetPrerequisitesMessage(
                ppSpentInTree, learnedAbilityIDs, abilityNames);

            _log.Warning("Prerequisites not met: Character={CharacterName}, Ability={AbilityName}, Unmet={Unmet}",
                character.Name, ability.Name, unmetMessage);

            return new AbilityResult
            {
                Success = false,
                Message = $"Prerequisites not met: {unmetMessage}"
            };
        }

        return new AbilityResult { Success = true };
    }

    /// <summary>
    /// Check if character can learn an ability (without learning it)
    /// </summary>
    public AbilityResult CanLearn(PlayerCharacter character, int abilityId)
    {
        var ability = _abilityRepo.GetById(abilityId);

        if (ability == null)
        {
            return new AbilityResult
            {
                Success = false,
                Message = "Ability not found"
            };
        }

        var characterId = GetCharacterId(character);

        // Check if already learned
        if (_abilityRepo.HasLearned(characterId, abilityId))
        {
            return new AbilityResult
            {
                Success = false,
                Message = "Already learned"
            };
        }

        // Check specialization
        var specCheck = ValidateSpecializationUnlocked(character, ability);
        if (!specCheck.Success) return specCheck;

        // Check prerequisites
        var prereqCheck = ValidatePrerequisites(character, ability);
        if (!prereqCheck.Success) return prereqCheck;

        // Check PP
        if (character.ProgressionPoints < ability.PPCost)
        {
            return new AbilityResult
            {
                Success = false,
                Message = $"Requires {ability.PPCost} PP (you have {character.ProgressionPoints})"
            };
        }

        return new AbilityResult
        {
            Success = true,
            Message = "Can learn"
        };
    }

    /// <summary>
    /// Check if character can afford to use an ability (resource check)
    /// </summary>
    public AbilityResult CanAfford(PlayerCharacter character, int abilityId)
    {
        var ability = _abilityRepo.GetById(abilityId);

        if (ability == null)
        {
            return new AbilityResult
            {
                Success = false,
                Message = "Ability not found"
            };
        }

        if (!ability.ResourceCost.IsAffordableBy(character))
        {
            var message = ability.ResourceCost.GetUnaffordableMessage(character);
            return new AbilityResult
            {
                Success = false,
                Message = message
            };
        }

        return new AbilityResult
        {
            Success = true,
            Message = "Can afford"
        };
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if character has learned an ability
    /// </summary>
    public bool HasLearned(PlayerCharacter character, int abilityId)
    {
        return _abilityRepo.HasLearned(GetCharacterId(character), abilityId);
    }

    /// <summary>
    /// Get all abilities learned by character
    /// </summary>
    public List<CharacterAbility> GetLearnedAbilities(PlayerCharacter character)
    {
        return _abilityRepo.GetLearnedAbilities(GetCharacterId(character));
    }

    /// <summary>
    /// Get current rank of an ability for character
    /// </summary>
    public int GetCurrentRank(PlayerCharacter character, int abilityId)
    {
        return _abilityRepo.GetCurrentRank(GetCharacterId(character), abilityId);
    }

    /// <summary>
    /// Increment usage count for an ability
    /// </summary>
    public void IncrementUsage(PlayerCharacter character, int abilityId)
    {
        _abilityRepo.IncrementUsage(GetCharacterId(character), abilityId);
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
