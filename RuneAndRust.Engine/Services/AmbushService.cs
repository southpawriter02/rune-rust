using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements ambush risk calculation and encounter generation for wilderness rest.
/// Uses dice pools for mitigation and generates biome-appropriate enemy groups.
/// </summary>
public class AmbushService : IAmbushService
{
    private readonly IDiceService _diceService;
    private readonly ILogger<AmbushService> _logger;

    /// <summary>
    /// Mitigation percentage per success on the Wits roll.
    /// </summary>
    private const int MitigationPerSuccess = 5;

    /// <summary>
    /// Minimum risk floor in dangerous zones (DangerLevel > Safe).
    /// </summary>
    private const int MinimumRiskFloor = 5;

    /// <summary>
    /// Budget multiplier for ambush encounters (80% of standard).
    /// </summary>
    private const float AmbushBudgetMultiplier = 0.8f;

    /// <summary>
    /// Standard base encounter budget before scaling.
    /// </summary>
    private const float BaseEncounterBudget = 100f;

    /// <summary>
    /// Enemy template IDs prioritized for ambush encounters (fast/stealthy).
    /// </summary>
    private static readonly string[] AmbushTemplates =
    [
        "bst_vargr_01",    // Ash-Vargr: GlassCannon, Beast, fast
        "mec_serv_01",     // Utility Servitor: Swarm, cheap
        "hum_raider_01"    // Rust-Clan Scav: Support, opportunistic
    ];

    /// <summary>
    /// Approximate cost per enemy type for budget allocation.
    /// </summary>
    private static readonly Dictionary<string, float> TemplateCosts = new()
    {
        ["bst_vargr_01"] = 40f,
        ["mec_serv_01"] = 25f,
        ["hum_raider_01"] = 35f
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbushService"/> class.
    /// </summary>
    /// <param name="diceService">The dice service for probability rolls.</param>
    /// <param name="logger">The logger for traceability.</param>
    public AmbushService(IDiceService diceService, ILogger<AmbushService> logger)
    {
        _diceService = diceService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AmbushResult> CalculateAmbushAsync(Character character, Room room)
    {
        _logger.LogInformation(
            "[Rest] Calculating ambush risk for {Character} in {Room} (DangerLevel: {Danger})",
            character.Name, room.Name, room.DangerLevel);

        // Step 1: Get base risk from danger level
        var baseRisk = GetBaseRisk(room.DangerLevel);

        // Step 2: Safe zones have no ambush risk
        if (baseRisk == 0)
        {
            _logger.LogInformation("[Rest] Base Risk: 0% (Safe zone). No ambush possible.");
            return new AmbushResult(
                IsAmbush: false,
                Message: "The watch passes uneventfully. The area appears secure.",
                BaseRiskPercent: 0,
                MitigationPercent: 0,
                FinalRiskPercent: 0,
                RollValue: 0,
                MitigationSuccesses: 0
            );
        }

        // Step 3: Roll Wits for mitigation (Camp Craft check)
        // Note: Skill system not yet implemented, use Wits attribute only
        var witsPool = character.Wits;
        var mitigationRoll = _diceService.Roll(witsPool, "Camp Craft (Wits)");
        var successes = mitigationRoll.Successes;
        var mitigation = successes * MitigationPerSuccess;

        // Step 4: Calculate final risk with floor
        var rawFinalRisk = Math.Max(0, baseRisk - mitigation);
        var finalRisk = room.DangerLevel > DangerLevel.Safe
            ? Math.Max(MinimumRiskFloor, rawFinalRisk)
            : rawFinalRisk;

        _logger.LogInformation(
            "[Rest] Base Risk: {Base}%, Mitigation: -{Mit}% (Successes: {S}), Final: {Final}%",
            baseRisk, mitigation, successes, finalRisk);

        // Step 5: Roll d100 against final risk
        var ambushRoll = _diceService.RollSingle(100, "Ambush Determination");

        var isAmbush = ambushRoll <= finalRisk;

        _logger.LogInformation(
            "[Rest] Ambush Roll: {Roll} vs {Risk}. Result: {IsAmbush}",
            ambushRoll, finalRisk, isAmbush ? "AMBUSH!" : "Safe");

        if (!isAmbush)
        {
            return new AmbushResult(
                IsAmbush: false,
                Message: "Your vigilance pays off. The night passes without incident.",
                BaseRiskPercent: baseRisk,
                MitigationPercent: mitigation,
                FinalRiskPercent: finalRisk,
                RollValue: ambushRoll,
                MitigationSuccesses: successes
            );
        }

        // Step 6: Generate ambush encounter
        var encounter = GenerateAmbushEncounter(room, character.Level);

        _logger.LogWarning(
            "[Combat] Ambush triggered! Spawning {Count} enemies.",
            encounter.TemplateIds.Count);

        return new AmbushResult(
            IsAmbush: true,
            Message: "Shadows move at the edge of your camp. You are not alone!",
            BaseRiskPercent: baseRisk,
            MitigationPercent: mitigation,
            FinalRiskPercent: finalRisk,
            RollValue: ambushRoll,
            MitigationSuccesses: successes,
            Encounter: encounter
        );
    }

    /// <inheritdoc/>
    public int GetBaseRisk(DangerLevel dangerLevel)
    {
        return dangerLevel switch
        {
            DangerLevel.Safe => 0,
            DangerLevel.Unstable => 15,
            DangerLevel.Hostile => 30,
            DangerLevel.Lethal => 50,
            _ => 15 // Default to Unstable risk
        };
    }

    /// <inheritdoc/>
    public EncounterDefinition GenerateAmbushEncounter(Room room, int partyLevel = 1)
    {
        // Calculate budget: base budget scaled by party level and ambush multiplier
        var scaledBudget = BaseEncounterBudget * (1f + (partyLevel - 1) * 0.1f);
        var ambushBudget = scaledBudget * AmbushBudgetMultiplier;

        _logger.LogDebug(
            "[Encounter] Generating ambush: Budget={Budget:F0}, Biome={Biome}",
            ambushBudget, room.BiomeType);

        // Select templates prioritizing ambush-appropriate enemies
        var templates = SelectAmbushTemplates(room.BiomeType, ambushBudget);

        return new EncounterDefinition(
            TemplateIds: templates,
            Budget: ambushBudget,
            IsAmbush: true,
            EncounterType: "Ambush"
        );
    }

    /// <summary>
    /// Selects enemy templates appropriate for an ambush encounter.
    /// Prioritizes fast/stealthy types and fills budget.
    /// </summary>
    /// <param name="biome">The biome type for context (future use).</param>
    /// <param name="budget">The available budget for enemy spawning.</param>
    /// <returns>A list of template IDs to spawn.</returns>
    private List<string> SelectAmbushTemplates(BiomeType biome, float budget)
    {
        var templates = new List<string>();
        var remainingBudget = budget;

        // Prioritize Ash-Vargr (GlassCannon) for ambush flavor
        if (remainingBudget >= TemplateCosts["bst_vargr_01"])
        {
            templates.Add("bst_vargr_01");
            remainingBudget -= TemplateCosts["bst_vargr_01"];
        }

        // Fill remaining budget with minions
        while (remainingBudget >= TemplateCosts["mec_serv_01"])
        {
            templates.Add("mec_serv_01");
            remainingBudget -= TemplateCosts["mec_serv_01"];
        }

        // Ensure at least one enemy
        if (templates.Count == 0)
        {
            templates.Add("mec_serv_01");
        }

        _logger.LogDebug(
            "[Encounter] Selected {Count} enemies: [{Templates}]",
            templates.Count, string.Join(", ", templates));

        return templates;
    }
}
