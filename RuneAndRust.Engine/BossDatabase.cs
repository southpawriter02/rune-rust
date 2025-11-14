using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.23: Database of boss configurations including phases, abilities, and loot tables
/// </summary>
public static class BossDatabase
{
    /// <summary>
    /// Get phase definitions for a boss
    /// </summary>
    public static List<BossPhaseDefinition> GetBossPhases(EnemyType bossType)
    {
        return bossType switch
        {
            EnemyType.RuinWarden => GetRuinWardenPhases(),
            EnemyType.AethericAberration => GetAethericAberrationPhases(),
            EnemyType.ForlornArchivist => GetForlornArchivistPhases(),
            EnemyType.OmegaSentinel => GetOmegaSentinelPhases(),
            _ => new List<BossPhaseDefinition>()
        };
    }

    /// <summary>
    /// Get boss abilities for a boss
    /// </summary>
    public static List<BossAbility> GetBossAbilities(EnemyType bossType)
    {
        return bossType switch
        {
            EnemyType.RuinWarden => GetRuinWardenAbilities(),
            EnemyType.AethericAberration => GetAethericAberrationAbilities(),
            EnemyType.ForlornArchivist => GetForlornArchivistAbilities(),
            EnemyType.OmegaSentinel => GetOmegaSentinelAbilities(),
            _ => new List<BossAbility>()
        };
    }

    /// <summary>
    /// Get boss loot table
    /// </summary>
    public static BossLootTable GetBossLootTable(EnemyType bossType)
    {
        return bossType switch
        {
            EnemyType.RuinWarden => GetRuinWardenLootTable(),
            EnemyType.AethericAberration => GetAethericAberrationLootTable(),
            EnemyType.ForlornArchivist => GetForlornArchivistLootTable(),
            EnemyType.OmegaSentinel => GetOmegaSentinelLootTable(),
            _ => GetDefaultBossLootTable()
        };
    }

    #region Ruin-Warden Boss Configuration

    private static List<BossPhaseDefinition> GetRuinWardenPhases()
    {
        return new List<BossPhaseDefinition>
        {
            // Phase 1: 100%-75% HP
            new BossPhaseDefinition
            {
                PhaseNumber = 1,
                HPPercentageThreshold = 100,
                TransitionDescription = "The Ruin-Warden initiates combat protocols. Shields online.",
                InvulnerabilityTurns = 0,
                AvailableAbilityIds = new List<string> { "warden_slash", "warden_charge" },
                StatModifiers = new PhaseStatModifiers
                {
                    DamageMultiplier = 1.0,
                    DefenseBonus = 0,
                    RegenerationPerTurn = 0
                }
            },

            // Phase 2: 75%-50% HP
            new BossPhaseDefinition
            {
                PhaseNumber = 2,
                HPPercentageThreshold = 75,
                TransitionDescription = "⚡ The Ruin-Warden's systems overload! Emergency protocols activated!",
                InvulnerabilityTurns = 1,
                AvailableAbilityIds = new List<string> { "warden_slash", "warden_charge", "system_overload" },
                StatModifiers = new PhaseStatModifiers
                {
                    DamageMultiplier = 1.2,
                    DefenseBonus = 2,
                    RegenerationPerTurn = 3
                },
                AddWave = new AddWaveConfig
                {
                    EnemyTypes = new List<EnemyType> { EnemyType.CorruptedServitor, EnemyType.CorruptedServitor },
                    SpawnCounts = new Dictionary<EnemyType, int>
                    {
                        { EnemyType.CorruptedServitor, 2 }
                    },
                    SpawnDescription = "⚠️ The Ruin-Warden calls for reinforcements!",
                    SpawnDelay = 0
                }
            },

            // Phase 3: 50%-0% HP (Desperate)
            new BossPhaseDefinition
            {
                PhaseNumber = 3,
                HPPercentageThreshold = 50,
                TransitionDescription = "💀 The Ruin-Warden enters CRITICAL STATE! All systems overclocked!",
                InvulnerabilityTurns = 2,
                AvailableAbilityIds = new List<string> { "warden_slash", "system_overload", "total_system_failure" },
                StatModifiers = new PhaseStatModifiers
                {
                    DamageMultiplier = 1.5,
                    DefenseBonus = 3,
                    RegenerationPerTurn = 5,
                    BonusActionsPerTurn = 1
                }
            }
        };
    }

    private static List<BossAbility> GetRuinWardenAbilities()
    {
        return new List<BossAbility>
        {
            new BossAbility
            {
                Id = "warden_slash",
                Name = "Electro-Blade Slash",
                Description = "Standard melee attack with electrified blade",
                Type = BossAbilityType.Standard,
                DamageDice = 2,
                DamageBonus = 2,
                ExecuteMessage = "The Ruin-Warden strikes with its electrified blade!"
            },

            new BossAbility
            {
                Id = "warden_charge",
                Name = "Shield Charge",
                Description = "Charge forward with shield raised",
                Type = BossAbilityType.Standard,
                DamageDice = 3,
                DamageBonus = 1,
                StatusEffects = new List<AbilityStatusEffect>
                {
                    new AbilityStatusEffect { StatusName = "Stunned", Duration = 1 }
                },
                ExecuteMessage = "The Ruin-Warden charges forward, shield raised!"
            },

            new BossAbility
            {
                Id = "system_overload",
                Name = "System Overload",
                Description = "Release electrical discharge in all directions",
                Type = BossAbilityType.Telegraphed,
                ChargeTurns = 1,
                ChargeMessage = "⚡ The Ruin-Warden's core begins to glow with building energy!",
                ExecuteMessage = "💥 SYSTEM OVERLOAD! Electrical discharge erupts!",
                DamageDice = 4,
                DamageBonus = 3,
                IsAoE = true,
                CooldownTurns = 3,
                CanBeInterrupted = true
            },

            new BossAbility
            {
                Id = "total_system_failure",
                Name = "Total System Failure",
                Description = "Ultimate attack - massive energy discharge",
                Type = BossAbilityType.Ultimate,
                ChargeTurns = 2,
                ChargeMessage = "🔴 [CRITICAL WARNING] The Ruin-Warden's core is destabilizing!",
                ExecuteMessage = "💀 TOTAL SYSTEM FAILURE! Catastrophic energy release!",
                DamageDice = 6,
                DamageBonus = 5,
                IsAoE = true,
                CooldownTurns = 5,
                TriggersVulnerability = true,
                VulnerabilityDuration = 2,
                CanBeInterrupted = false,
                MinimumHPPercentage = 0,
                MaximumHPPercentage = 50
            }
        };
    }

    private static BossLootTable GetRuinWardenLootTable()
    {
        return new BossLootTable
        {
            BossType = EnemyType.RuinWarden,
            MinimumQuality = QualityTier.ClanForged,
            GuaranteedQuality = QualityTier.ClanForged,
            OptimizedChance = 30,
            LegendaryChance = 15,
            ArtifactChance = 10,
            UniqueArtifacts = new List<UniqueArtifact>
            {
                new UniqueArtifact
                {
                    Id = "warden_electroBlade",
                    Name = "Ruin-Warden's Electro-Blade",
                    Description = "A salvaged blade from the Ruin-Warden, still crackling with residual energy.",
                    Type = ArtifactType.Weapon,
                    WeaponCategory = WeaponCategory.Blade,
                    DamageDice = 3,
                    DamageBonus = 2,
                    SpecialEffect = "Attacks have 25% chance to inflict [Stunned] for 1 turn",
                    Bonuses = new List<EquipmentBonus>
                    {
                        new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 2, Description = "+2 FINESSE" }
                    },
                    DropWeight = 3,
                    MinimumTDR = 0
                },

                new UniqueArtifact
                {
                    Id = "ancient_power_core",
                    Name = "Ancient Power Core",
                    Description = "A still-functioning power core from pre-Glitch technology. Valuable to engineers.",
                    Type = ArtifactType.CraftingMaterial,
                    SpecialEffect = "Legendary crafting component",
                    DropWeight = 2,
                    MinimumTDR = 5
                }
            },
            CurrencyDrop = (300, 800),
            GuaranteedMaterials = new Dictionary<ComponentType, int>
            {
                { ComponentType.AncientCircuitBoard, 1 },
                { ComponentType.StructuralScrap, 3 }
            },
            RareMaterialChances = new Dictionary<ComponentType, int>
            {
                { ComponentType.DvergrAlloyIngot, 40 },
                { ComponentType.CorruptedCrystal, 30 }
            },
            EpicMaterialChances = new Dictionary<ComponentType, int>
            {
                { ComponentType.JotunCoreFragment, 15 }
            },
            TDRScalingFactor = 1.0
        };
    }

    #endregion

    #region Aetheric Aberration Boss Configuration

    private static List<BossPhaseDefinition> GetAethericAberrationPhases()
    {
        return new List<BossPhaseDefinition>
        {
            new BossPhaseDefinition
            {
                PhaseNumber = 1,
                HPPercentageThreshold = 100,
                TransitionDescription = "The Aetheric Aberration materializes, reality warping around it.",
                AvailableAbilityIds = new List<string> { "void_blast", "phase_shift" }
            },

            new BossPhaseDefinition
            {
                PhaseNumber = 2,
                HPPercentageThreshold = 75,
                TransitionDescription = "⚡ The Aberration's form destabilizes! Reality tears open!",
                InvulnerabilityTurns = 1,
                AvailableAbilityIds = new List<string> { "void_blast", "reality_tear", "summon_echoes" },
                StatModifiers = new PhaseStatModifiers { DamageMultiplier = 1.3 }
            },

            new BossPhaseDefinition
            {
                PhaseNumber = 3,
                HPPercentageThreshold = 50,
                TransitionDescription = "💀 The Aberration becomes DESPERATE! The veil between worlds shatters!",
                InvulnerabilityTurns = 2,
                AvailableAbilityIds = new List<string> { "void_blast", "reality_tear", "aetheric_storm" },
                StatModifiers = new PhaseStatModifiers
                {
                    DamageMultiplier = 1.6,
                    RegenerationPerTurn = 4
                }
            }
        };
    }

    private static List<BossAbility> GetAethericAberrationAbilities()
    {
        return new List<BossAbility>
        {
            new BossAbility
            {
                Id = "void_blast",
                Name = "Void Blast",
                Description = "Launch bolt of corrupted aetheric energy",
                Type = BossAbilityType.Standard,
                DamageDice = 3,
                DamageBonus = 2,
                ExecuteMessage = "The Aberration fires a bolt of void energy!"
            },

            new BossAbility
            {
                Id = "phase_shift",
                Name = "Phase Shift",
                Description = "Shift between dimensions, gaining evasion",
                Type = BossAbilityType.Standard,
                ExecuteMessage = "The Aberration flickers out of phase!",
                SpecialEffects = new AbilitySpecialEffects
                {
                    DefenseBonus = 5,
                    DefenseDuration = 2
                }
            },

            new BossAbility
            {
                Id = "reality_tear",
                Name = "Reality Tear",
                Description = "Tear open a rift in reality",
                Type = BossAbilityType.Telegraphed,
                ChargeTurns = 1,
                ChargeMessage = "⚠️ Reality begins to fracture around the Aberration!",
                ExecuteMessage = "💥 A tear in reality rips open!",
                DamageDice = 5,
                DamageBonus = 3,
                IsAoE = true,
                CooldownTurns = 3
            },

            new BossAbility
            {
                Id = "summon_echoes",
                Name = "Summon Echoes",
                Description = "Call forth echoes from the void",
                Type = BossAbilityType.Standard,
                ExecuteMessage = "⚠️ The Aberration summons twisted echoes!",
                SpecialEffects = new AbilitySpecialEffects
                {
                    SummonAdds = new AddWaveConfig
                    {
                        EnemyTypes = new List<EnemyType> { EnemyType.BlightDrone },
                        SpawnCounts = new Dictionary<EnemyType, int> { { EnemyType.BlightDrone, 1 } },
                        SpawnDescription = "An echo materializes from the void!"
                    }
                },
                CooldownTurns = 4
            },

            new BossAbility
            {
                Id = "aetheric_storm",
                Name = "Aetheric Storm",
                Description = "Ultimate - unleash catastrophic storm",
                Type = BossAbilityType.Ultimate,
                ChargeTurns = 2,
                ChargeMessage = "🔴 [CRITICAL] Aetheric energy builds to critical mass!",
                ExecuteMessage = "💀 AETHERIC STORM! Reality itself screams!",
                DamageDice = 7,
                DamageBonus = 4,
                IsAoE = true,
                TriggersVulnerability = true,
                VulnerabilityDuration = 3,
                CooldownTurns = 6,
                MaximumHPPercentage = 50
            }
        };
    }

    private static BossLootTable GetAethericAberrationLootTable()
    {
        return new BossLootTable
        {
            BossType = EnemyType.AethericAberration,
            GuaranteedQuality = QualityTier.ClanForged,
            OptimizedChance = 35,
            LegendaryChance = 20,
            ArtifactChance = 12,
            UniqueArtifacts = new List<UniqueArtifact>
            {
                new UniqueArtifact
                {
                    Id = "void_touched_staff",
                    Name = "Void-Touched Staff",
                    Description = "A staff warped by void energies, pulsing with otherworldly power.",
                    Type = ArtifactType.Weapon,
                    WeaponCategory = WeaponCategory.Staff,
                    DamageDice = 4,
                    DamageBonus = 3,
                    SpecialEffect = "Abilities cost -2 Aether. Gain +2 WILL",
                    Bonuses = new List<EquipmentBonus>
                    {
                        new EquipmentBonus { AttributeName = "WILL", BonusValue = 2, Description = "+2 WILL" }
                    },
                    DropWeight = 3,
                    MinimumTDR = 0
                }
            },
            CurrencyDrop = (400, 900),
            GuaranteedMaterials = new Dictionary<ComponentType, int>
            {
                { ComponentType.AethericDust, 5 },
                { ComponentType.CorruptedCrystal, 2 }
            },
            TDRScalingFactor = 1.2
        };
    }

    #endregion

    #region Forlorn Archivist Boss Configuration

    private static List<BossPhaseDefinition> GetForlornArchivistPhases()
    {
        return new List<BossPhaseDefinition>
        {
            new BossPhaseDefinition
            {
                PhaseNumber = 1,
                HPPercentageThreshold = 100,
                TransitionDescription = "The Forlorn Archivist raises its hands. Psychic whispers fill the air.",
                AvailableAbilityIds = new List<string> { "mind_spike", "psychic_scream" }
            },

            new BossPhaseDefinition
            {
                PhaseNumber = 2,
                HPPercentageThreshold = 75,
                TransitionDescription = "⚡ The Archivist's psychic aura intensifies! The dead stir!",
                InvulnerabilityTurns = 1,
                AvailableAbilityIds = new List<string> { "mind_spike", "psychic_scream", "summon_revenants" },
                StatModifiers = new PhaseStatModifiers { DamageMultiplier = 1.25 }
            },

            new BossPhaseDefinition
            {
                PhaseNumber = 3,
                HPPercentageThreshold = 50,
                TransitionDescription = "💀 MADNESS! The Archivist's mind fractures, releasing psychic storm!",
                InvulnerabilityTurns = 2,
                AvailableAbilityIds = new List<string> { "mass_hysteria", "psychic_storm" },
                StatModifiers = new PhaseStatModifiers
                {
                    DamageMultiplier = 1.5,
                    BonusActionsPerTurn = 1
                }
            }
        };
    }

    private static List<BossAbility> GetForlornArchivistAbilities()
    {
        return new List<BossAbility>
        {
            new BossAbility
            {
                Id = "mind_spike",
                Name = "Mind Spike",
                Description = "Pierce target's mind with psychic attack",
                Type = BossAbilityType.Standard,
                DamageDice = 3,
                DamageBonus = 1,
                ExecuteMessage = "The Archivist lances your mind with psychic energy!"
            },

            new BossAbility
            {
                Id = "psychic_scream",
                Name = "Psychic Scream",
                Description = "Unleash mind-shattering scream",
                Type = BossAbilityType.Standard,
                DamageDice = 2,
                DamageBonus = 2,
                ExecuteMessage = "💀 The Archivist screams! Your mind reels!",
                StatusEffects = new List<AbilityStatusEffect>
                {
                    new AbilityStatusEffect { StatusName = "Disoriented", Duration = 2 }
                }
            },

            new BossAbility
            {
                Id = "summon_revenants",
                Name = "Summon Revenants",
                Description = "Call forth undead servants",
                Type = BossAbilityType.Standard,
                ExecuteMessage = "⚠️ The Archivist calls forth the dead!",
                SpecialEffects = new AbilitySpecialEffects
                {
                    SummonAdds = new AddWaveConfig
                    {
                        EnemyTypes = new List<EnemyType> { EnemyType.CorruptedServitor, EnemyType.CorruptedServitor },
                        SpawnCounts = new Dictionary<EnemyType, int> { { EnemyType.CorruptedServitor, 2 } },
                        SpawnDescription = "Corrupted revenants rise from the ground!"
                    }
                },
                CooldownTurns = 4
            },

            new BossAbility
            {
                Id = "mass_hysteria",
                Name = "Mass Hysteria",
                Description = "Drive all enemies into panic",
                Type = BossAbilityType.Telegraphed,
                ChargeTurns = 1,
                ChargeMessage = "⚠️ Maddening whispers crescendo around the Archivist!",
                ExecuteMessage = "💥 MASS HYSTERIA! Madness overwhelms you!",
                DamageDice = 4,
                DamageBonus = 2,
                IsAoE = true,
                CooldownTurns = 3
            },

            new BossAbility
            {
                Id = "psychic_storm",
                Name = "Psychic Storm",
                Description = "Ultimate - unleash total psychic devastation",
                Type = BossAbilityType.Ultimate,
                ChargeTurns = 2,
                ChargeMessage = "🔴 [CRITICAL] The Archivist's mind fractures completely!",
                ExecuteMessage = "💀 PSYCHIC STORM! Your sanity shatters!",
                DamageDice = 6,
                DamageBonus = 5,
                IsAoE = true,
                TriggersVulnerability = true,
                VulnerabilityDuration = 2,
                CooldownTurns = 5,
                MaximumHPPercentage = 50
            }
        };
    }

    private static BossLootTable GetForlornArchivistLootTable()
    {
        return new BossLootTable
        {
            BossType = EnemyType.ForlornArchivist,
            GuaranteedQuality = QualityTier.ClanForged,
            OptimizedChance = 30,
            LegendaryChance = 18,
            ArtifactChance = 15,
            CurrencyDrop = (350, 850),
            TDRScalingFactor = 1.1
        };
    }

    #endregion

    #region Omega Sentinel Boss Configuration

    private static List<BossPhaseDefinition> GetOmegaSentinelPhases()
    {
        return new List<BossPhaseDefinition>
        {
            new BossPhaseDefinition
            {
                PhaseNumber = 1,
                HPPercentageThreshold = 100,
                TransitionDescription = "The Omega Sentinel powers up. Combat systems online.",
                AvailableAbilityIds = new List<string> { "maul_strike", "seismic_slam" }
            },

            new BossPhaseDefinition
            {
                PhaseNumber = 2,
                HPPercentageThreshold = 75,
                TransitionDescription = "⚡ The Sentinel overcharges! Hydraulics strain!",
                InvulnerabilityTurns = 1,
                AvailableAbilityIds = new List<string> { "overcharged_maul", "seismic_slam", "power_draw" },
                StatModifiers = new PhaseStatModifiers
                {
                    DamageMultiplier = 1.3,
                    DefenseBonus = 3,
                    SoakBonus = 2
                }
            },

            new BossPhaseDefinition
            {
                PhaseNumber = 3,
                HPPercentageThreshold = 50,
                TransitionDescription = "💀 OMEGA PROTOCOLS ACTIVE! Maximum power!",
                InvulnerabilityTurns = 2,
                AvailableAbilityIds = new List<string> { "overcharged_maul", "omega_protocol" },
                StatModifiers = new PhaseStatModifiers
                {
                    DamageMultiplier = 1.6,
                    DefenseBonus = 5,
                    SoakBonus = 3
                }
            }
        };
    }

    private static List<BossAbility> GetOmegaSentinelAbilities()
    {
        return new List<BossAbility>
        {
            new BossAbility
            {
                Id = "maul_strike",
                Name = "Maul Strike",
                Description = "Powerful melee slam",
                Type = BossAbilityType.Standard,
                DamageDice = 3,
                DamageBonus = 3,
                ExecuteMessage = "The Sentinel slams down with massive force!"
            },

            new BossAbility
            {
                Id = "seismic_slam",
                Name = "Seismic Slam",
                Description = "Ground-shaking AoE attack",
                Type = BossAbilityType.Standard,
                DamageDice = 4,
                DamageBonus = 2,
                IsAoE = true,
                ExecuteMessage = "💥 The Sentinel pounds the ground! Shockwaves ripple out!",
                CooldownTurns = 2
            },

            new BossAbility
            {
                Id = "power_draw",
                Name = "Power Draw",
                Description = "Draw power from internal core to heal",
                Type = BossAbilityType.Standard,
                ExecuteMessage = "🔄 The Sentinel draws from its power core!",
                SpecialEffects = new AbilitySpecialEffects
                {
                    HealAmount = 20
                },
                CooldownTurns = 3
            },

            new BossAbility
            {
                Id = "overcharged_maul",
                Name = "Overcharged Maul",
                Description = "Electrically charged devastating strike",
                Type = BossAbilityType.Telegraphed,
                ChargeTurns = 1,
                ChargeMessage = "⚡ The Sentinel charges its maul with crackling energy!",
                ExecuteMessage = "💥 OVERCHARGED STRIKE! Electrical devastation!",
                DamageDice = 5,
                DamageBonus = 4,
                StatusEffects = new List<AbilityStatusEffect>
                {
                    new AbilityStatusEffect { StatusName = "Stunned", Duration = 1 }
                },
                CooldownTurns = 3
            },

            new BossAbility
            {
                Id = "omega_protocol",
                Name = "Omega Protocol",
                Description = "Ultimate - full power assault",
                Type = BossAbilityType.Ultimate,
                ChargeTurns = 2,
                ChargeMessage = "🔴 [OMEGA PROTOCOL] All systems at maximum!",
                ExecuteMessage = "💀 OMEGA PROTOCOL UNLEASHED!",
                DamageDice = 8,
                DamageBonus = 5,
                IsAoE = true,
                TriggersVulnerability = true,
                VulnerabilityDuration = 3,
                CooldownTurns = 6,
                MaximumHPPercentage = 50
            }
        };
    }

    private static BossLootTable GetOmegaSentinelLootTable()
    {
        return new BossLootTable
        {
            BossType = EnemyType.OmegaSentinel,
            GuaranteedQuality = QualityTier.Optimized,
            OptimizedChance = 40,
            LegendaryChance = 25,
            ArtifactChance = 18,
            CurrencyDrop = (500, 1000),
            TDRScalingFactor = 1.3
        };
    }

    #endregion

    private static BossLootTable GetDefaultBossLootTable()
    {
        return new BossLootTable
        {
            GuaranteedQuality = QualityTier.ClanForged,
            OptimizedChance = 30,
            LegendaryChance = 15,
            ArtifactChance = 10,
            CurrencyDrop = (300, 800),
            TDRScalingFactor = 1.0
        };
    }
}
