using RuneAndRust.Core.BossGauntlet;
using RuneAndRust.Core.ChallengeSectors;
using RuneAndRust.Core.EndlessMode;
using RuneAndRust.Core.NewGamePlus;
using RuneAndRust.Engine.BossGauntlet;
using RuneAndRust.Engine.ChallengeSectors;
using RuneAndRust.Engine.EndlessMode;
using RuneAndRust.Engine.NewGamePlus;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.40: Endgame Content Orchestrator
/// Unified interface for all endgame systems (NG+, Sectors, Gauntlets, Endless)
/// </summary>
public class EndgameContentOrchestrator
{
    private static readonly ILogger _log = Log.ForContext<EndgameContentOrchestrator>();

    private readonly NewGamePlusService _ngPlusService;
    private readonly ChallengeSectorService _sectorService;
    private readonly BossGauntletService _gauntletService;
    private readonly EndlessModeService _endlessService;

    public EndgameContentOrchestrator(
        NewGamePlusRepository ngPlusRepo,
        ChallengeSectorRepository sectorRepo,
        BossGauntletRepository gauntletRepo,
        EndlessModeRepository endlessRepo)
    {
        _ngPlusService = new NewGamePlusService(ngPlusRepo);
        _sectorService = new ChallengeSectorService(sectorRepo);
        _gauntletService = new BossGauntletService(gauntletRepo);
        _endlessService = new EndlessModeService(endlessRepo);

        _log.Information("EndgameContentOrchestrator initialized with all 4 systems");
    }

    // ═══════════════════════════════════════════════════════════
    // UNIFIED CONTENT ACCESS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get all available endgame content for a character
    /// </summary>
    public EndgameContentSummary GetAvailableContent(int characterId)
    {
        // Get current NG+ tier
        var ngPlusTier = _ngPlusService.GetCurrentNGPlusTier(characterId);

        // Get available content from each system
        var sectors = _sectorService.GetAvailableSeasons(characterId, ngPlusTier);
        var gauntlets = _gauntletService.GetAvailableSequences(characterId, ngPlusTier);
        var activeSeason = _endlessService.GetActiveSeason();

        var summary = new EndgameContentSummary
        {
            CharacterId = characterId,
            CurrentNGPlusTier = ngPlusTier,
            AvailableSectors = sectors,
            AvailableGauntlets = gauntlets,
            EndlessSeason = activeSeason,
            TotalContentPieces = sectors.Count + gauntlets.Count + 1 // +1 for endless
        };

        _log.Information("Available endgame content for character {CharacterId}: {Sectors} sectors, {Gauntlets} gauntlets, NG+{Tier}",
            characterId, sectors.Count, gauntlets.Count, ngPlusTier);

        return summary;
    }

    /// <summary>
    /// Get progress across all endgame systems
    /// </summary>
    public EndgameProgressSummary GetProgress(int characterId)
    {
        var ngPlusTier = _ngPlusService.GetCurrentNGPlusTier(characterId);
        var sectorProgress = _sectorService.GetProgress(characterId);

        // Get gauntlet statistics (simplified - would need repository support)
        var gauntletRuns = 0; // Placeholder
        var gauntletVictories = 0; // Placeholder

        // Get endless mode best performance
        var endlessRun = _endlessService.GetActiveRun(characterId);
        var bestWave = 0; // Would query from repository
        var bestScore = 0; // Would query from repository

        return new EndgameProgressSummary
        {
            CharacterId = characterId,
            NGPlusTier = ngPlusTier,
            SectorsCompleted = sectorProgress.TotalSectorsCompleted,
            SectorsAvailable = sectorProgress.TotalSectorsAvailable,
            SectorCompletionPercentage = sectorProgress.CompletionPercentage,
            GauntletRuns = gauntletRuns,
            GauntletVictories = gauntletVictories,
            EndlessBestWave = bestWave,
            EndlessBestScore = bestScore
        };
    }

    // ═══════════════════════════════════════════════════════════
    // NG+ PROGRESSION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Check if character can advance to next NG+ tier
    /// </summary>
    public bool CanAdvanceNGPlusTier(int characterId)
    {
        var currentTier = _ngPlusService.GetCurrentNGPlusTier(characterId);

        // Max tier is 5
        if (currentTier >= 5)
        {
            return false;
        }

        // Can always advance (in real implementation, might require completing certain content)
        return true;
    }

    /// <summary>
    /// Advance to next NG+ tier and return newly unlocked content
    /// </summary>
    public NGPlusAdvancementResult AdvanceNGPlusTier(int characterId, object characterState)
    {
        var currentTier = _ngPlusService.GetCurrentNGPlusTier(characterId);
        var targetTier = currentTier + 1;

        if (!CanAdvanceNGPlusTier(characterId))
        {
            throw new InvalidOperationException($"Cannot advance from NG+{currentTier}");
        }

        // Get content available before advancement
        var beforeSectors = _sectorService.GetAvailableSeasons(characterId, currentTier);
        var beforeGauntlets = _gauntletService.GetAvailableSequences(characterId, currentTier);

        // Initialize next NG+ tier
        var carryover = _ngPlusService.InitializeNewGamePlus(characterId, targetTier, characterState);

        // Get newly unlocked content
        var afterSectors = _sectorService.GetAvailableSeasons(characterId, targetTier);
        var afterGauntlets = _gauntletService.GetAvailableSequences(characterId, targetTier);

        var newSectors = afterSectors.Where(s => !beforeSectors.Any(b => b.SectorId == s.SectorId)).ToList();
        var newGauntlets = afterGauntlets.Where(g => !beforeGauntlets.Any(b => b.SequenceId == g.SequenceId)).ToList();

        var result = new NGPlusAdvancementResult
        {
            NewTier = targetTier,
            Carryover = carryover,
            NewlyUnlockedSectors = newSectors,
            NewlyUnlockedGauntlets = newGauntlets
        };

        _log.Information("Advanced character {CharacterId} to NG+{Tier}: {NewSectors} sectors, {NewGauntlets} gauntlets unlocked",
            characterId, targetTier, newSectors.Count, newGauntlets.Count);

        return result;
    }

    // ═══════════════════════════════════════════════════════════
    // CONTENT RECOMMENDATIONS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get recommended next activity for character
    /// </summary>
    public EndgameRecommendation GetRecommendation(int characterId)
    {
        var ngPlusTier = _ngPlusService.GetCurrentNGPlusTier(characterId);
        var sectorProgress = _sectorService.GetProgress(characterId);

        // Simple recommendation logic
        if (sectorProgress.TotalSectorsCompleted == 0)
        {
            // New to endgame - recommend starting with easy content
            var apprenticeTrial = _gauntletService.GetSequenceById("gauntlet_apprentice");
            if (apprenticeTrial != null)
            {
                return new EndgameRecommendation
                {
                    Type = EndgameContentType.BossGauntlet,
                    ContentId = "gauntlet_apprentice",
                    Title = "Start with Apprentice's Trial",
                    Reason = "Learn gauntlet mechanics with moderate difficulty",
                    Priority = RecommendationPriority.High
                };
            }
        }

        if (sectorProgress.CompletionPercentage < 0.5f && ngPlusTier == 0)
        {
            // More sectors to complete at base difficulty
            var availableSectors = _sectorService.GetAvailableSeasons(characterId, ngPlusTier);
            var incompleteSectors = availableSectors
                .Where(s => !_sectorService.HasCompleted(characterId, s.SectorId))
                .OrderBy(s => s.TotalDifficultyMultiplier)
                .FirstOrDefault();

            if (incompleteSectors != null)
            {
                return new EndgameRecommendation
                {
                    Type = EndgameContentType.ChallengeSector,
                    ContentId = incompleteSectors.SectorId,
                    Title = $"Try {incompleteSectors.Name}",
                    Reason = "Complete more sectors for unique rewards",
                    Priority = RecommendationPriority.Medium
                };
            }
        }

        if (CanAdvanceNGPlusTier(characterId) && sectorProgress.CompletionPercentage > 0.7f)
        {
            // Suggest NG+ advancement
            return new EndgameRecommendation
            {
                Type = EndgameContentType.NGPlusAdvancement,
                ContentId = $"ng_plus_{ngPlusTier + 1}",
                Title = $"Advance to NG+{ngPlusTier + 1}",
                Reason = "Unlock harder content and better rewards",
                Priority = RecommendationPriority.High
            };
        }

        // Default to Endless Mode
        return new EndgameRecommendation
        {
            Type = EndgameContentType.EndlessMode,
            ContentId = "endless_mode",
            Title = "Try Endless Mode",
            Reason = "Compete on seasonal leaderboards",
            Priority = RecommendationPriority.Medium
        };
    }

    // ═══════════════════════════════════════════════════════════
    // LEADERBOARD AGGREGATION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get all leaderboards across systems
    /// </summary>
    public AggregatedLeaderboards GetAllLeaderboards(int limit = 10)
    {
        var endlessWave = _endlessService.GetLeaderboard(
            EndlessLeaderboardCategory.HighestWave, limit: limit);

        var endlessScore = _endlessService.GetLeaderboard(
            EndlessLeaderboardCategory.HighestScore, limit: limit);

        // Would get gauntlet leaderboards similarly
        var gauntletFastest = new List<GauntletLeaderboardEntry>(); // Placeholder
        var gauntletFlawless = new List<GauntletLeaderboardEntry>(); // Placeholder

        return new AggregatedLeaderboards
        {
            EndlessHighestWave = endlessWave,
            EndlessHighestScore = endlessScore,
            GauntletFastest = gauntletFastest,
            GauntletFlawless = gauntletFlawless
        };
    }

    // ═══════════════════════════════════════════════════════════
    // SYSTEM ACCESS (Direct)
    // ═══════════════════════════════════════════════════════════

    public NewGamePlusService NGPlusService => _ngPlusService;
    public ChallengeSectorService SectorService => _sectorService;
    public BossGauntletService GauntletService => _gauntletService;
    public EndlessModeService EndlessService => _endlessService;
}

// ═══════════════════════════════════════════════════════════
// SUPPORTING MODELS
// ═══════════════════════════════════════════════════════════

public class EndgameContentSummary
{
    public int CharacterId { get; set; }
    public int CurrentNGPlusTier { get; set; }
    public List<ChallengeSector> AvailableSectors { get; set; } = new();
    public List<GauntletSequence> AvailableGauntlets { get; set; } = new();
    public EndlessSeason? EndlessSeason { get; set; }
    public int TotalContentPieces { get; set; }

    public string SummaryDisplay => $@"
=== Endgame Content Available ===
NG+ Tier: {CurrentNGPlusTier}
Challenge Sectors: {AvailableSectors.Count}
Boss Gauntlets: {AvailableGauntlets.Count}
Endless Mode: {(EndlessSeason != null ? $"Active ({EndlessSeason.Name})" : "Available")}
Total Content: {TotalContentPieces} pieces
".Trim();
}

public class EndgameProgressSummary
{
    public int CharacterId { get; set; }
    public int NGPlusTier { get; set; }
    public int SectorsCompleted { get; set; }
    public int SectorsAvailable { get; set; }
    public float SectorCompletionPercentage { get; set; }
    public int GauntletRuns { get; set; }
    public int GauntletVictories { get; set; }
    public int EndlessBestWave { get; set; }
    public int EndlessBestScore { get; set; }

    public string ProgressDisplay => $@"
=== Endgame Progress ===
NG+ Tier: {NGPlusTier}

Challenge Sectors:
  Completed: {SectorsCompleted}/{SectorsAvailable}
  Progress: {(SectorCompletionPercentage * 100):F1}%

Boss Gauntlets:
  Runs: {GauntletRuns}
  Victories: {GauntletVictories}
  Win Rate: {(GauntletRuns > 0 ? (float)GauntletVictories / GauntletRuns * 100 : 0):F1}%

Endless Mode:
  Best Wave: {EndlessBestWave}
  Best Score: {EndlessBestScore:N0}
".Trim();
}

public class NGPlusAdvancementResult
{
    public int NewTier { get; set; }
    public CarryoverSnapshot Carryover { get; set; } = new();
    public List<ChallengeSector> NewlyUnlockedSectors { get; set; } = new();
    public List<GauntletSequence> NewlyUnlockedGauntlets { get; set; } = new();

    public string SummaryDisplay => $@"
=== NG+{NewTier} Unlocked ===
Carried Over:
  Gold: {Carryover.CarriedGold:N0}
  Items: {Carryover.ItemCount}
  Skills: Preserved

Newly Unlocked:
  Challenge Sectors: {NewlyUnlockedSectors.Count}
    {string.Join("\n    ", NewlyUnlockedSectors.Select(s => $"- {s.Name}"))}

  Boss Gauntlets: {NewlyUnlockedGauntlets.Count}
    {string.Join("\n    ", NewlyUnlockedGauntlets.Select(g => $"- {g.SequenceName}"))}
".Trim();
}

public class EndgameRecommendation
{
    public EndgameContentType Type { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; }

    public string Display => $@"
[{Priority}] {Title}
{Reason}
Content: {Type}
".Trim();
}

public enum EndgameContentType
{
    ChallengeSector,
    BossGauntlet,
    EndlessMode,
    NGPlusAdvancement
}

public enum RecommendationPriority
{
    Low,
    Medium,
    High
}

public class AggregatedLeaderboards
{
    public List<EndlessLeaderboardEntry> EndlessHighestWave { get; set; } = new();
    public List<EndlessLeaderboardEntry> EndlessHighestScore { get; set; } = new();
    public List<GauntletLeaderboardEntry> GauntletFastest { get; set; } = new();
    public List<GauntletLeaderboardEntry> GauntletFlawless { get; set; } = new();

    public string SummaryDisplay => $@"
=== Global Leaderboards ===

Endless Mode - Highest Wave:
{string.Join("\n", EndlessHighestWave.Take(5).Select(e => $"  #{e.Rank} - {e.PlayerName} (Wave {e.HighestWaveReached})"))}

Endless Mode - Highest Score:
{string.Join("\n", EndlessHighestScore.Take(5).Select(e => $"  #{e.Rank} - {e.PlayerName} ({e.TotalScore:N0} points)"))}

Boss Gauntlet - Fastest:
{string.Join("\n", GauntletFastest.Take(5).Select(g => $"  #{g.Rank} - {g.CharacterName} ({g.TimeDisplay})"))}

Boss Gauntlet - Flawless:
{string.Join("\n", GauntletFlawless.Take(5).Select(g => $"  #{g.Rank} - {g.CharacterName} (0 deaths)"))}
".Trim();
}
