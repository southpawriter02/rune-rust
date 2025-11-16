using RuneAndRust.Core;
using RuneAndRust.Core.Factions;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.33.4: Generates faction-based random encounters
/// Adjusts encounter frequency and hostility based on reputation
/// </summary>
public class FactionEncounterService
{
    private static readonly ILogger _log = Log.ForContext<FactionEncounterService>();
    private readonly FactionService _factionService;
    private readonly ReputationService _reputationService;
    private readonly Random _random;

    public FactionEncounterService(FactionService factionService, ReputationService reputationService)
    {
        _factionService = factionService;
        _reputationService = reputationService;
        _random = new Random();
    }

    /// <summary>
    /// Generates a faction encounter based on character reputation and location
    /// </summary>
    public FactionEncounter? GenerateFactionEncounter(int characterId, string biome, int characterLevel)
    {
        _log.Debug("Generating faction encounter: CharacterId={CharacterId}, Biome={Biome}, Level={Level}",
            characterId, biome, characterLevel);

        // Get factions present in this biome
        var biomeFactions = GetBiomeFactions(biome);
        if (biomeFactions.Count == 0)
        {
            _log.Debug("No factions present in biome: {Biome}", biome);
            return null;
        }

        // Select random faction weighted by presence in biome
        var selectedFaction = biomeFactions[_random.Next(biomeFactions.Count)];

        // Get reputation with selected faction
        var reputation = _factionService.GetCharacterReputation(characterId, selectedFaction.FactionId);
        var reputationValue = reputation?.ReputationValue ?? 0;
        var tier = _reputationService.GetReputationTier(reputationValue);

        // Calculate encounter chance based on reputation
        var encounterModifier = _reputationService.GetEncounterFrequencyModifier(tier);
        var baseEncounterChance = 0.3f; // 30% base chance
        var adjustedChance = baseEncounterChance * encounterModifier;

        // Roll for encounter
        if (_random.NextDouble() > adjustedChance)
        {
            _log.Debug("Encounter roll failed: Chance={Chance}, Modifier={Modifier}",
                adjustedChance, encounterModifier);
            return null;
        }

        // Determine encounter type based on reputation
        var encounterType = DetermineEncounterType(tier);
        var encounter = CreateEncounter(selectedFaction, encounterType, characterLevel, reputationValue);

        _log.Information("Faction encounter generated: FactionId={FactionId}, Type={Type}, CharacterId={CharacterId}",
            selectedFaction.FactionId, encounterType, characterId);

        return encounter;
    }

    /// <summary>
    /// Gets factions that have presence in a specific biome
    /// </summary>
    private List<Faction> GetBiomeFactions(string biome)
    {
        var allFactions = _factionService.GetAllFactions();
        var biomeFactions = new List<Faction>();

        foreach (var faction in allFactions)
        {
            // Check if faction has presence in this biome
            if (faction.PrimaryLocation.Contains(biome, StringComparison.OrdinalIgnoreCase) ||
                faction.PrimaryLocation.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                biomeFactions.Add(faction);
            }
        }

        return biomeFactions;
    }

    /// <summary>
    /// Determines encounter type based on reputation tier
    /// </summary>
    private FactionEncounterType DetermineEncounterType(FactionReputationTier tier)
    {
        return tier switch
        {
            FactionReputationTier.Exalted => FactionEncounterType.FriendlyAssistance,
            FactionReputationTier.Allied => _random.NextDouble() > 0.5 ? FactionEncounterType.FriendlyPatrol : FactionEncounterType.FriendlyAssistance,
            FactionReputationTier.Friendly => FactionEncounterType.FriendlyPatrol,
            FactionReputationTier.Neutral => FactionEncounterType.NeutralPatrol,
            FactionReputationTier.Hostile => FactionEncounterType.HostilePatrol,
            FactionReputationTier.Hated => _random.NextDouble() > 0.5 ? FactionEncounterType.HostileAmbush : FactionEncounterType.HostilePatrol,
            _ => FactionEncounterType.NeutralPatrol
        };
    }

    /// <summary>
    /// Creates a faction encounter with appropriate NPCs and behavior
    /// </summary>
    private FactionEncounter CreateEncounter(Faction faction, FactionEncounterType encounterType, int characterLevel, int reputation)
    {
        var encounter = new FactionEncounter
        {
            Faction = faction,
            EncounterType = encounterType,
            CharacterLevel = characterLevel,
            Reputation = reputation
        };

        // Determine encounter size based on type
        var encounterSize = encounterType switch
        {
            FactionEncounterType.HostileAmbush => _random.Next(4, 8),
            FactionEncounterType.HostilePatrol => _random.Next(2, 5),
            FactionEncounterType.NeutralPatrol => _random.Next(2, 4),
            FactionEncounterType.FriendlyPatrol => _random.Next(2, 4),
            FactionEncounterType.FriendlyAssistance => _random.Next(1, 3),
            _ => 2
        };

        encounter.EncounterSize = encounterSize;

        // Generate encounter description
        encounter.Description = GenerateEncounterDescription(faction, encounterType, encounterSize);

        _log.Debug("Encounter created: Faction={FactionName}, Type={Type}, Size={Size}",
            faction.DisplayName, encounterType, encounterSize);

        return encounter;
    }

    /// <summary>
    /// Generates narrative description for the encounter
    /// </summary>
    private string GenerateEncounterDescription(Faction faction, FactionEncounterType encounterType, int size)
    {
        var groupSize = size switch
        {
            1 => "a lone",
            2 => "a pair of",
            <= 4 => "a small group of",
            <= 6 => "a patrol of",
            _ => "a large force of"
        };

        return encounterType switch
        {
            FactionEncounterType.HostileAmbush => $"You are ambushed by {groupSize} {faction.DisplayName} fighters! They attack without warning, clearly hostile to your presence.",
            FactionEncounterType.HostilePatrol => $"You encounter {groupSize} {faction.DisplayName} patrol members. They recognize you and prepare for combat, their hostility evident.",
            FactionEncounterType.NeutralPatrol => $"{groupSize} {faction.DisplayName} patrol passes by. They acknowledge your presence but do not interfere.",
            FactionEncounterType.FriendlyPatrol => $"{groupSize} {faction.DisplayName} patrol greets you respectfully. They recognize you as an ally and offer safe passage.",
            FactionEncounterType.FriendlyAssistance => $"{groupSize} {faction.DisplayName} members approach and offer assistance. Your reputation with them has earned their aid.",
            _ => $"You encounter {groupSize} {faction.DisplayName} members."
        };
    }

    /// <summary>
    /// Calculates chance of faction ambush in hostile territory
    /// </summary>
    public float GetAmbushChance(int characterId, int factionId)
    {
        var reputation = _factionService.GetCharacterReputation(characterId, factionId);
        if (reputation == null) return 0f;

        var tier = reputation.ReputationTier;

        return tier switch
        {
            FactionReputationTier.Hated => 0.40f,    // 40% chance of ambush
            FactionReputationTier.Hostile => 0.20f,  // 20% chance of ambush
            _ => 0f
        };
    }

    /// <summary>
    /// Determines if faction members will offer assistance
    /// </summary>
    public bool WillOfferAssistance(int characterId, int factionId)
    {
        var reputation = _factionService.GetCharacterReputation(characterId, factionId);
        if (reputation == null) return false;

        return reputation.ReputationTier == FactionReputationTier.Exalted ||
               reputation.ReputationTier == FactionReputationTier.Allied;
    }

    /// <summary>
    /// Generates rewards for completing faction encounters
    /// </summary>
    public FactionEncounterReward GenerateEncounterReward(Faction faction, FactionEncounterType encounterType, int characterLevel)
    {
        var reward = new FactionEncounterReward
        {
            FactionId = faction.FactionId,
            ReputationGain = 0,
            Experience = 0,
            Currency = 0
        };

        // Rewards vary by encounter type
        switch (encounterType)
        {
            case FactionEncounterType.HostileAmbush:
                // Defeating ambush gives reputation loss but combat rewards
                reward.ReputationGain = -10;
                reward.Experience = 100 + (characterLevel * 20);
                reward.Currency = 50 + (characterLevel * 10);
                break;

            case FactionEncounterType.HostilePatrol:
                // Defeating patrol
                reward.ReputationGain = -15;
                reward.Experience = 75 + (characterLevel * 15);
                reward.Currency = 40 + (characterLevel * 8);
                break;

            case FactionEncounterType.NeutralPatrol:
                // Peaceful interaction
                reward.ReputationGain = 2;
                break;

            case FactionEncounterType.FriendlyPatrol:
                // Friendly interaction
                reward.ReputationGain = 5;
                reward.Experience = 50;
                break;

            case FactionEncounterType.FriendlyAssistance:
                // Accepting help
                reward.ReputationGain = 10;
                reward.Experience = 100;
                break;
        }

        return reward;
    }
}

/// <summary>
/// v0.33.4: Types of faction encounters
/// </summary>
public enum FactionEncounterType
{
    HostileAmbush,        // Surprise attack by hostile faction
    HostilePatrol,        // Hostile faction patrol
    NeutralPatrol,        // Neutral faction patrol
    FriendlyPatrol,       // Friendly faction patrol
    FriendlyAssistance    // Faction offers aid
}

/// <summary>
/// v0.33.4: Represents a faction encounter
/// </summary>
public class FactionEncounter
{
    public Faction Faction { get; set; } = null!;
    public FactionEncounterType EncounterType { get; set; }
    public int EncounterSize { get; set; }
    public int CharacterLevel { get; set; }
    public int Reputation { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// v0.33.4: Rewards from faction encounter resolution
/// </summary>
public class FactionEncounterReward
{
    public int FactionId { get; set; }
    public int ReputationGain { get; set; }
    public int Experience { get; set; }
    public int Currency { get; set; }
}
