# v0.44.6: Death & Game Over Handling

Type: Technical
Description: DeathController for death detection (HP/Corruption/Stress), death screen display, run statistics, Hall of Legends meta-progression updates, and clean return to main menu.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.44.4, v0.41 (Meta-Progression)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.44: Game Flow Integration & Controllers (v0%2044%20Game%20Flow%20Integration%20&%20Controllers%200f28bd7b1ab1400fb9cc0377e89bb095.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.44.4, v0.41 (Meta-Progression)

**Estimated Time:** 5-7 hours

**Phase:** End Conditions

**Deliverable:** Clean death handling with statistics and meta-progression

---

## Executive Summary

v0.44.6 implements the DeathController that handles player death gracefully, displays run statistics, updates meta-progression, and returns to the main menu.

**What This Delivers:**

- DeathController implementation
- Death detection across all game states
- Death screen UI coordination
- Run statistics display
- Meta-progression updates on death
- Return to main menu

**Success Metric:** Survivor death ends the saga cleanly, displaying trauma statistics (final Corruption/Psychic Stress levels), Legend earned, and updating Hall of Legends with permanent meta-progression unlocks.

---

## Service Implementation

### DeathController

```csharp
using RuneAndRust.Core;
using RuneAndRust.Core.Combat;
using RuneAndRust.Core.Progression;
using RuneAndRust.Core.MetaProgression;
using RuneAndRust.UI.Views;
using Serilog;
using System;
using System.Threading.Tasks;

namespace RuneAndRust.Controllers
{
    /// <summary>
    /// Handles player death, run statistics, meta-progression updates, and game over flow.
    /// </summary>
    public class DeathController
    {
        private readonly ILogger _logger;
        private readonly CombatSystem _combatSystem;
        private readonly ProgressionSystem _progressionSystem;
        private readonly MetaProgressionSystem _metaProgressionSystem;
        private readonly GameStateController _gameStateController;
        private readonly DeathScreenView _deathScreenView;
        
        public DeathController(
            ILogger logger,
            CombatSystem combatSystem,
            ProgressionSystem progressionSystem,
            MetaProgressionSystem metaProgressionSystem,
            GameStateController gameStateController,
            DeathScreenView deathScreenView)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
            _progressionSystem = progressionSystem ?? throw new ArgumentNullException(nameof(progressionSystem));
            _metaProgressionSystem = metaProgressionSystem ?? throw new ArgumentNullException(nameof(metaProgressionSystem));
            _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
            _deathScreenView = deathScreenView ?? throw new ArgumentNullException(nameof(deathScreenView));
        }
        
        /// <summary>
        /// Handles player death - displays death screen, calculates statistics, updates meta-progression.
        /// </summary>
        public async Task HandlePlayerDeathAsync(Character deadSurvivor, string causeOfDeath)
        {
            using (_logger.BeginTimedOperation("HandlePlayerDeath", new { SurvivorName = [deadSurvivor.Name](http://deadSurvivor.Name), Cause = causeOfDeath }))
            {
                try
                {
                    _logger.Information("Survivor {Name} has died: {Cause}", [deadSurvivor.Name](http://deadSurvivor.Name), causeOfDeath);
                    
                    // Transition to Death state
                    _gameStateController.TransitionToState(GamePhase.Death);
                    
                    // Calculate run statistics
                    var runStats = await CalculateRunStatisticsAsync(deadSurvivor);
                    
                    // Update meta-progression (Legend to Hall of Legends)
                    await UpdateMetaProgressionAsync(deadSurvivor, runStats);
                    
                    // Record death in database
                    await RecordDeathAsync(deadSurvivor, causeOfDeath, runStats);
                    
                    // Display death screen with statistics
                    _deathScreenView.DisplayDeathScreen(deadSurvivor, causeOfDeath, runStats);
                    
                    // Wait for player acknowledgment
                    await _deathScreenView.WaitForConfirmationAsync();
                    
                    // Return to main menu
                    _gameStateController.TransitionToState(GamePhase.MainMenu);
                    
                    _logger.Information("Death handling complete. Returning to main menu.");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error handling player death for {Name}", [deadSurvivor.Name](http://deadSurvivor.Name));
                    // Ensure we return to main menu even on error
                    _gameStateController.TransitionToState(GamePhase.MainMenu);
                }
            }
        }
        
        /// <summary>
        /// Calculates comprehensive run statistics for death screen display.
        /// </summary>
        private async Task<RunStatistics> CalculateRunStatisticsAsync(Character survivor)
        {
            var stats = new RunStatistics
            {
                // Basic Info
                SurvivorName = [survivor.Name](http://survivor.Name),
                Lineage = survivor.Lineage,
                Background = survivor.Background,
                Archetype = survivor.Archetype,
                Specialization = survivor.Specialization,
                
                // Progression
                FinalLevel = survivor.CurrentMilestoneLevel,
                TotalLegendEarned = survivor.TotalLegendEarned,
                LegendToHallOfLegends = CalculateLegendForHallOfLegends(survivor),
                ProgressionPointsSpent = survivor.ProgressionPointsSpent,
                
                // Trauma
                FinalPsychicStress = survivor.PsychicStress,
                FinalCorruption = survivor.RunicBlightCorruption,
                MaxPsychicStressReached = survivor.MaxPsychicStressReached,
                MaxCorruptionReached = survivor.MaxCorruptionReached,
                TimesRested = survivor.TimesRested,
                
                // Combat
                TotalKills = await _combatSystem.GetTotalKillsAsync(survivor.CharacterId),
                TotalDamageDealt = await _combatSystem.GetTotalDamageDealtAsync(survivor.CharacterId),
                TotalDamageTaken = await _combatSystem.GetTotalDamageTakenAsync(survivor.CharacterId),
                CombatsWon = await _combatSystem.GetCombatsWonAsync(survivor.CharacterId),
                CombatsLost = 1, // This death
                
                // Exploration
                SectorsExplored = survivor.SectorsExplored,
                RoomsVisited = survivor.RoomsVisited,
                SecretsFound = survivor.SecretsFound,
                ChestsOpened = survivor.ChestsOpened,
                
                // Time
                PlaytimeMinutes = survivor.TotalPlaytimeMinutes,
                DeathTimestamp = DateTime.UtcNow
            };
            
            return stats;
        }
        
        /// <summary>
        /// Calculates how much Legend converts to Hall of Legends (meta-progression currency).
        /// Formula: 10% of total Legend earned (rounded down).
        /// </summary>
        private int CalculateLegendForHallOfLegends(Character survivor)
        {
            int totalLegend = survivor.TotalLegendEarned;
            int hallOfLegendsAmount = totalLegend / 10; // 10% conversion
            
            _logger.Debug("Converting {Total} Legend to {HallAmount} Hall of Legends", 
                totalLegend, hallOfLegendsAmount);
            
            return hallOfLegendsAmount;
        }
        
        /// <summary>
        /// Updates meta-progression system with Legend earned, death recorded, etc.
        /// </summary>
        private async Task UpdateMetaProgressionAsync(Character survivor, RunStatistics stats)
        {
            try
            {
                // Add Legend to Hall of Legends
                await _metaProgressionSystem.AddLegendToHallOfLegendsAsync(stats.LegendToHallOfLegends);
                
                // Record death for statistics
                await _metaProgressionSystem.RecordDeathAsync(
                    survivor.Archetype,
                    survivor.Specialization,
                    survivor.CurrentMilestoneLevel,
                    stats.FinalCorruption,
                    stats.FinalPsychicStress);
                
                // Check for meta-progression unlocks
                await _metaProgressionSystem.CheckForUnlocksAsync();
                
                _logger.Information("Meta-progression updated: {Legend} added to Hall of Legends", 
                    stats.LegendToHallOfLegends);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating meta-progression for {Name}", [survivor.Name](http://survivor.Name));
                // Don't throw - allow death handling to continue
            }
        }
        
        /// <summary>
        /// Records death in RunEndings database table for permanent record.
        /// </summary>
        private async Task RecordDeathAsync(Character survivor, string causeOfDeath, RunStatistics stats)
        {
            try
            {
                await _progressionSystem.RecordRunEndingAsync(new RunEnding
                {
                    GameSessionId = _gameStateController.CurrentSessionId,
                    CharacterId = survivor.CharacterId,
                    EndingType = "Death",
                    CauseOfDeath = causeOfDeath,
                    FinalLevel = stats.FinalLevel,
                    TotalLegendEarned = stats.TotalLegendEarned,
                    LegendToHallOfLegends = stats.LegendToHallOfLegends,
                    FinalPsychicStress = stats.FinalPsychicStress,
                    FinalCorruption = stats.FinalCorruption,
                    TotalKills = stats.TotalKills,
                    SectorsExplored = stats.SectorsExplored,
                    PlaytimeMinutes = stats.PlaytimeMinutes,
                    DeathTimestamp = stats.DeathTimestamp
                });
                
                _logger.Information("Death recorded in database for {Name}", [survivor.Name](http://survivor.Name));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error recording death in database for {Name}", [survivor.Name](http://survivor.Name));
                // Don't throw - allow death handling to continue
            }
        }
        
        /// <summary>
        /// Checks if the player is dead during any game state.
        /// Called from ExplorationController, CombatController, etc.
        /// </summary>
        public bool IsSurvivorDead(Character survivor)
        {
            if (survivor == null)
            {
                _logger.Warning("IsSurvivorDead called with null survivor");
                return true;
            }
            
            bool isDead = survivor.CurrentHP <= 0;
            
            if (isDead)
            {
                _logger.Information("Survivor {Name} detected as dead (HP: {HP})", 
                    [survivor.Name](http://survivor.Name), survivor.CurrentHP);
            }
            
            return isDead;
        }
    }
    
    /// <summary>
    /// Data structure for run statistics displayed on death screen.
    /// </summary>
    public class RunStatistics
    {
        // Basic Info
        public string SurvivorName { get; set; }
        public string Lineage { get; set; }
        public string Background { get; set; }
        public string Archetype { get; set; }
        public string Specialization { get; set; }
        
        // Progression
        public int FinalLevel { get; set; }
        public int TotalLegendEarned { get; set; }
        public int LegendToHallOfLegends { get; set; }
        public int ProgressionPointsSpent { get; set; }
        
        // Trauma
        public int FinalPsychicStress { get; set; }
        public int FinalCorruption { get; set; }
        public int MaxPsychicStressReached { get; set; }
        public int MaxCorruptionReached { get; set; }
        public int TimesRested { get; set; }
        
        // Combat
        public int TotalKills { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int CombatsWon { get; set; }
        public int CombatsLost { get; set; }
        
        // Exploration
        public int SectorsExplored { get; set; }
        public int RoomsVisited { get; set; }
        public int SecretsFound { get; set; }
        public int ChestsOpened { get; set; }
        
        // Time
        public int PlaytimeMinutes { get; set; }
        public DateTime DeathTimestamp { get; set; }
    }
    
    /// <summary>
    /// Data structure for recording run endings in database.
    /// </summary>
    public class RunEnding
    {
        public int RunEndingId { get; set; }
        public int GameSessionId { get; set; }
        public int CharacterId { get; set; }
        public string EndingType { get; set; } // "Death" or "Victory"
        public string CauseOfDeath { get; set; } // If EndingType = Death
        public int FinalLevel { get; set; }
        public int TotalLegendEarned { get; set; }
        public int LegendToHallOfLegends { get; set; }
        public int FinalPsychicStress { get; set; }
        public int FinalCorruption { get; set; }
        public int TotalKills { get; set; }
        public int SectorsExplored { get; set; }
        public int PlaytimeMinutes { get; set; }
        public DateTime DeathTimestamp { get; set; }
    }
}
```

---

## Database Schema

### RunEndings Table

Stores permanent record of all run endings (deaths and victories).

```sql
CREATE TABLE RunEndings (
    run_ending_id INTEGER PRIMARY KEY AUTOINCREMENT,
    game_session_id INTEGER NOT NULL,
    character_id INTEGER NOT NULL,
    ending_type TEXT NOT NULL, -- 'Death' or 'Victory'
    cause_of_death TEXT, -- If ending_type = 'Death'
    final_level INTEGER NOT NULL,
    total_legend_earned INTEGER NOT NULL,
    legend_to_hall_of_legends INTEGER NOT NULL,
    final_psychic_stress INTEGER NOT NULL,
    final_corruption INTEGER NOT NULL,
    total_kills INTEGER NOT NULL,
    sectors_explored INTEGER NOT NULL,
    playtime_minutes INTEGER NOT NULL,
    death_timestamp TEXT NOT NULL, -- ISO-8601 datetime
    FOREIGN KEY (game_session_id) REFERENCES GameSessions(session_id),
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);

CREATE INDEX idx_runend_session ON RunEndings(game_session_id);
CREATE INDEX idx_runend_character ON RunEndings(character_id);
CREATE INDEX idx_runend_type ON RunEndings(ending_type);
```

### Additional Character Fields

Track maximums for death screen display.

```sql
-- Add to Characters table
ALTER TABLE Characters ADD COLUMN max_psychic_stress_reached INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN max_corruption_reached INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN times_rested INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN total_legend_earned INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN progression_points_spent INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN sectors_explored INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN rooms_visited INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN secrets_found INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN chests_opened INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN total_playtime_minutes INTEGER DEFAULT 0;
```

---

## Integration Points

### Death Detection in CombatController

```csharp
public async Task ProcessCombatRoundAsync()
{
    // ... combat logic ...
    
    // After damage resolution, check for player death
    var player = _combatSystem.GetPlayerCharacter();
    if (_deathController.IsSurvivorDead(player))
    {
        string causeOfDeath = DetermineCauseOfDeath(player);
        await _deathController.HandlePlayerDeathAsync(player, causeOfDeath);
        return; // Exit combat loop
    }
    
    // ... continue combat if alive ...
}

private string DetermineCauseOfDeath(Character survivor)
{
    // Check last damage source
    var lastDamageSource = _combatSystem.GetLastDamageSource(survivor.CharacterId);
    if (lastDamageSource != null)
        return $"Slain by {lastDamageSource.EnemyName}";
    
    // Check Psychic Stress threshold
    if (survivor.PsychicStress >= 100)
        return "Psychological collapse (Psychic Stress exceeded threshold)";
    
    // Check Corruption threshold
    if (survivor.RunicBlightCorruption >= 100)
        return "Consumed by Runic Blight (Corruption exceeded threshold)";
    
    // Default
    return "Unknown cause";
}
```

### Death Detection in ExplorationController

```csharp
public async Task ProcessExplorationActionAsync(string command)
{
    // Before processing any action, check death from Corruption/Stress
    var player = _progressionSystem.GetPlayer();
    
    if (_deathController.IsSurvivorDead(player))
    {
        string cause = "Corruption overwhelm";
        if (player.PsychicStress >= 100)
            cause = "Psychic collapse";
        
        await _deathController.HandlePlayerDeathAsync(player, cause);
        return;
    }
    
    // ... process exploration action ...
}
```

### Death Detection in ProgressionController

```csharp
public async Task ApplyTraumaAsync(Character survivor, int psychicStress, int corruption)
{
    // Apply trauma
    survivor.PsychicStress += psychicStress;
    survivor.RunicBlightCorruption += corruption;
    
    // Update maximums
    if (survivor.PsychicStress > survivor.MaxPsychicStressReached)
        survivor.MaxPsychicStressReached = survivor.PsychicStress;
    
    if (survivor.RunicBlightCorruption > survivor.MaxCorruptionReached)
        survivor.MaxCorruptionReached = survivor.RunicBlightCorruption;
    
    // Check for death from Corruption/Stress thresholds
    if (survivor.PsychicStress >= 100)
    {
        await _deathController.HandlePlayerDeathAsync(survivor, "Psychic collapse");
        return;
    }
    
    if (survivor.RunicBlightCorruption >= 100)
    {
        await _deathController.HandlePlayerDeathAsync(survivor, "Consumed by Runic Blight");
        return;
    }
    
    await _progressionSystem.SaveCharacterAsync(survivor);
}
```

---

## UI Implementation

### DeathScreenView (Avalonia)

```xml
<UserControl xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             x:Class="RuneAndRust.UI.Views.DeathScreenView">
    
    <Border Background="#1a0000" Padding="40">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center" Spacing="30">
            
            <!-- Death Header -->
            <TextBlock Text="THE SAGA ENDS" 
                      FontSize="48" 
                      FontWeight="Bold" 
                      Foreground="#cc0000" 
                      HorizontalAlignment="Center"/>
            
            <TextBlock x:Name="CauseOfDeathText"
                      FontSize="20" 
                      Foreground="#ff6666" 
                      HorizontalAlignment="Center"
                      TextWrapping="Wrap"
                      MaxWidth="600"/>
            
            <!-- Survivor Info -->
            <StackPanel Spacing="10">
                <TextBlock x:Name="SurvivorNameText" 
                          FontSize="24" 
                          FontWeight="Bold" 
                          Foreground="White" 
                          HorizontalAlignment="Center"/>
                <TextBlock x:Name="SurvivorDetailsText" 
                          FontSize="16" 
                          Foreground="#aaaaaa" 
                          HorizontalAlignment="Center"/>
            </StackPanel>
            
            <!-- Statistics Grid -->
            <Border Background="#000000" BorderBrush="#444444" BorderThickness="2" Padding="30">
                <StackPanel Spacing="20">
                    
                    <!-- Progression Stats -->
                    <TextBlock Text="PROGRESSION" 
                              FontSize="18" 
                              FontWeight="Bold" 
                              Foreground="#ffcc00"/>
                    <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto,Auto,Auto" 
                          HorizontalAlignment="Center">
                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Final Level: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="0" Grid.Column="1" x:Name="FinalLevelText" Foreground="White" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="1" Grid.Column="0" Text="Total Legend Earned: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="1" Grid.Column="1" x:Name="TotalLegendText" Foreground="White" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="2" Grid.Column="0" Text="Legend to Hall of Legends: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="2" Grid.Column="1" x:Name="LegendToHallText" Foreground="#ffcc00" Margin="10,0,0,0" FontWeight="Bold"/>
                        
                        <TextBlock Grid.Row="3" Grid.Column="0" Text="Progression Points Spent: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="3" Grid.Column="1" x:Name="PPSpentText" Foreground="White" Margin="10,0,0,0"/>
                    </Grid>
                    
                    <!-- Trauma Stats -->
                    <TextBlock Text="TRAUMA" 
                              FontSize="18" 
                              FontWeight="Bold" 
                              Foreground="#cc6600" 
                              Margin="0,20,0,0"/>
                    <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto,Auto,Auto" 
                          HorizontalAlignment="Center">
                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Final Psychic Stress: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="0" Grid.Column="1" x:Name="FinalStressText" Foreground="#ff6666" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="1" Grid.Column="0" Text="Final Corruption: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="1" Grid.Column="1" x:Name="FinalCorruptionText" Foreground="#cc00cc" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="2" Grid.Column="0" Text="Max Stress Reached: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="2" Grid.Column="1" x:Name="MaxStressText" Foreground="White" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="3" Grid.Column="0" Text="Max Corruption Reached: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="3" Grid.Column="1" x:Name="MaxCorruptionText" Foreground="White" Margin="10,0,0,0"/>
                    </Grid>
                    
                    <!-- Combat Stats -->
                    <TextBlock Text="COMBAT" 
                              FontSize="18" 
                              FontWeight="Bold" 
                              Foreground="#cc0000" 
                              Margin="0,20,0,0"/>
                    <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto,Auto" 
                          HorizontalAlignment="Center">
                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Undying Slain: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="0" Grid.Column="1" x:Name="KillsText" Foreground="White" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="1" Grid.Column="0" Text="Damage Dealt: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="1" Grid.Column="1" x:Name="DamageDealtText" Foreground="White" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="2" Grid.Column="0" Text="Combats Won: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="2" Grid.Column="1" x:Name="CombatsWonText" Foreground="White" Margin="10,0,0,0"/>
                    </Grid>
                    
                    <!-- Exploration Stats -->
                    <TextBlock Text="EXPLORATION" 
                              FontSize="18" 
                              FontWeight="Bold" 
                              Foreground="#0066cc" 
                              Margin="0,20,0,0"/>
                    <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto,Auto" 
                          HorizontalAlignment="Center">
                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Sectors Explored: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="0" Grid.Column="1" x:Name="SectorsText" Foreground="White" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="1" Grid.Column="0" Text="Rooms Visited: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="1" Grid.Column="1" x:Name="RoomsText" Foreground="White" Margin="10,0,0,0"/>
                        
                        <TextBlock Grid.Row="2" Grid.Column="0" Text="Secrets Found: " Foreground="#aaaaaa"/>
                        <TextBlock Grid.Row="2" Grid.Column="1" x:Name="SecretsText" Foreground="White" Margin="10,0,0,0"/>
                    </Grid>
                    
                    <!-- Playtime -->
                    <Grid ColumnDefinitions="Auto,*" HorizontalAlignment="Center" Margin="0,20,0,0">
                        <TextBlock Grid.Column="0" Text="Playtime: " Foreground="#aaaaaa" FontSize="14"/>
                        <TextBlock Grid.Column="1" x:Name="PlaytimeText" Foreground="White" Margin="10,0,0,0" FontSize="14"/>
                    </Grid>
                    
                </StackPanel>
            </Border>
            
            <!-- Return to Main Menu Button -->
            <Button x:Name="ReturnButton"
                   Content="Return to Main Menu"
                   FontSize="18"
                   Padding="30,15"
                   HorizontalAlignment="Center"
                   Background="#660000"
                   Foreground="White"
                   Cursor="Hand"/>
            
        </StackPanel>
    </Border>
    
</UserControl>
```

### DeathScreenView Code-Behind

```csharp
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace RuneAndRust.UI.Views
{
    public partial class DeathScreenView : UserControl
    {
        private TaskCompletionSource<bool> _confirmationTcs;
        
        public DeathScreenView()
        {
            InitializeComponent();
            [ReturnButton.Click](http://ReturnButton.Click) += OnReturnButtonClick;
        }
        
        public void DisplayDeathScreen(Character survivor, string causeOfDeath, RunStatistics stats)
        {
            // Header
            CauseOfDeathText.Text = causeOfDeath;
            
            // Survivor Info
            SurvivorNameText.Text = [survivor.Name](http://survivor.Name);
            SurvivorDetailsText.Text = $"{stats.Lineage} {stats.Background} • {stats.Archetype} ({stats.Specialization})";
            
            // Progression
            FinalLevelText.Text = stats.FinalLevel.ToString();
            TotalLegendText.Text = stats.TotalLegendEarned.ToString();
            LegendToHallText.Text = $"{stats.LegendToHallOfLegends} (+{stats.LegendToHallOfLegends} to permanent meta-progression)";
            PPSpentText.Text = stats.ProgressionPointsSpent.ToString();
            
            // Trauma
            FinalStressText.Text = stats.FinalPsychicStress.ToString();
            FinalCorruptionText.Text = stats.FinalCorruption.ToString();
            MaxStressText.Text = stats.MaxPsychicStressReached.ToString();
            MaxCorruptionText.Text = stats.MaxCorruptionReached.ToString();
            
            // Combat
            KillsText.Text = stats.TotalKills.ToString();
            DamageDealtText.Text = stats.TotalDamageDealt.ToString();
            CombatsWonText.Text = stats.TotalCombatsWon.ToString();
            
            // Exploration
            SectorsText.Text = stats.SectorsExplored.ToString();
            RoomsText.Text = stats.RoomsVisited.ToString();
            SecretsText.Text = stats.SecretsFound.ToString();
            
            // Playtime
            int hours = stats.PlaytimeMinutes / 60;
            int minutes = stats.PlaytimeMinutes % 60;
            PlaytimeText.Text = $"{hours}h {minutes}m";
        }
        
        public async Task WaitForConfirmationAsync()
        {
            _confirmationTcs = new TaskCompletionSource<bool>();
            await _confirmationTcs.Task;
        }
        
        private void OnReturnButtonClick(object sender, RoutedEventArgs e)
        {
            _confirmationTcs?.SetResult(true);
        }
    }
}
```

---

## Success Criteria

### Functional Requirements

- [ ]  DeathController detects player death from HP loss, Corruption threshold (100+), or Psychic Stress threshold (100+)
- [ ]  Death screen displays all run statistics (progression, trauma, combat, exploration, playtime)
- [ ]  Legend converts to Hall of Legends at 10% rate
- [ ]  Meta-progression system updates (Legend added, death recorded, unlocks checked)
- [ ]  RunEndings table stores permanent death record
- [ ]  Return to Main Menu button transitions cleanly to MainMenuController
- [ ]  Death handling fails gracefully (returns to main menu even on error)

### Integration Requirements

- [ ]  CombatController checks for death after each combat round
- [ ]  ExplorationController checks for death from Corruption/Stress thresholds
- [ ]  ProgressionController checks for death when applying trauma
- [ ]  All death causes properly identified and displayed
- [ ]  Meta-progression unlocks check executes on death

### Quality Requirements

- [ ]  All death scenarios tested (combat death, Corruption, Psychic Stress)
- [ ]  Death screen UI displays correctly with proper Aethelgard terminology
- [ ]  Database writes succeed without errors
- [ ]  Logging captures death event with full context
- [ ]  Return to main menu clears game state properly
- [ ]  No memory leaks from death/restart cycle

---

## Testing Checklist

### Unit Tests

```csharp
[TestClass]
public class DeathControllerTests
{
    [TestMethod]
    public async Task HandlePlayerDeath_CalculatesLegendCorrectly()
    {
        // 1000 Legend earned → 100 to Hall of Legends (10%)
        var survivor = CreateTestSurvivor(totalLegend: 1000);
        
        await _deathController.HandlePlayerDeathAsync(survivor, "Test death");
        
        Assert.AreEqual(100, _metaProgressionSystem.HallOfLegendsBalance);
    }
    
    [TestMethod]
    public async Task IsSurvivorDead_ReturnsTrue_WhenHPZero()
    {
        var survivor = CreateTestSurvivor(hp: 0);
        
        bool isDead = _deathController.IsSurvivorDead(survivor);
        
        Assert.IsTrue(isDead);
    }
    
    [TestMethod]
    public async Task HandlePlayerDeath_TransitionsToDeathState()
    {
        var survivor = CreateTestSurvivor();
        
        await _deathController.HandlePlayerDeathAsync(survivor, "Test");
        
        Assert.AreEqual(GamePhase.Death, _gameStateController.CurrentPhase);
    }
    
    [TestMethod]
    public async Task HandlePlayerDeath_RecordsRunEnding()
    {
        var survivor = CreateTestSurvivor();
        
        await _deathController.HandlePlayerDeathAsync(survivor, "Slain by Draugr");
        
        var runEnding = await _database.GetLatestRunEndingAsync();
        Assert.IsNotNull(runEnding);
        Assert.AreEqual("Death", runEnding.EndingType);
        Assert.AreEqual("Slain by Draugr", runEnding.CauseOfDeath);
    }
}
```

### Integration Tests

- Death from combat damage (HP → 0)
- Death from Corruption threshold (100+)
- Death from Psychic Stress threshold (100+)
- Death screen displays all statistics correctly
- Meta-progression updates correctly
- Return to main menu works

---

## Dependencies

### Prerequisites

- v0.44.4 (CombatController) must detect death
- v0.41 (Meta-Progression) must provide Hall of Legends system
- v0.44.5 (ProgressionController) must detect Corruption/Stress death

### Provides

- DeathController for v0.44.7 (VictoryController) to reference
- RunEndings table for v0.41 statistics display
- Death detection for all controllers

---

## Aethelgard Terminology

**Correct Usage:**

- "Survivor" (not "player character")
- "Undying" / "Jötun-Forged" (not "enemies")
- "Saga" (not "run" or "playthrough" in UI)
- "Legend" (not "experience" or "XP")
- "Hall of Legends" (not "meta-currency")
- "Runic Blight Corruption" (full term, not just "Corruption")
- "Psychic Stress" (not "Sanity" or "Mental Health")
- "Sector" (not "dungeon")

---

## Notes

**10% Legend Conversion Rationale:**

- Prevents excessive meta-progression from single good runs
- Creates meaningful long-term progression arc
- 10 runs needed to earn back Legend from one successful run
- Aligns with roguelike progression pacing

**Death Causes Tracked:**

- Combat (Slain by {EnemyName})
- Corruption threshold (Consumed by Runic Blight)
- Psychic Stress threshold (Psychological collapse)
- Unknown (fallback)

**Meta-Progression Unlocks:**

- Checked automatically on death
- Unlocks displayed on return to main menu (MainMenuController responsibility)
- Hall of Legends balance persists across runs