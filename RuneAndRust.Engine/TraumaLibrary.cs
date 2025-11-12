using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// Library of trauma definitions for v0.15 Trauma Economy
/// </summary>
public static class TraumaLibrary
{
    /// <summary>
    /// Gets all trauma definitions
    /// </summary>
    public static List<Trauma> GetAllTraumas()
    {
        return new List<Trauma>
        {
            CreateParanoia(),
            CreateAgoraphobia(),
            CreateClaustrophobia(),
            CreateNyctophobia(),
            CreateIsolophobia(),
            CreateSurvivorsGuilt(),
            CreateHypervigilance(),
            CreateDistrust(),
            CreateCompulsiveCounting(),
            CreateHoarding(),
            CreateDissociation(),
            CreateMachineAffinity()
        };
    }

    /// <summary>
    /// Gets a trauma by ID
    /// </summary>
    public static Trauma? GetTrauma(string traumaId)
    {
        return GetAllTraumas().FirstOrDefault(t =>
            t.TraumaId.Equals(traumaId, StringComparison.OrdinalIgnoreCase));
    }

    // Fear Category Traumas

    private static Trauma CreateParanoia()
    {
        return new Trauma
        {
            TraumaId = "paranoia",
            Name = "[PARANOIA]",
            Description = "They're watching. They're always watching.",
            Category = TraumaCategory.Paranoia,
            Severity = TraumaSeverity.Mild,
            ProgressionLevel = 1,
            FlavorText = "You hear something behind you. Nothing there.",
            Effects = new List<TraumaEffect>
            {
                new StressMultiplierEffect
                {
                    Multiplier = 1.2f,
                    TriggerCondition = null // Always active
                },
                new AttributePenaltyEffect
                {
                    AttributeName = "Wits",
                    Penalty = 1,
                    Condition = "social"
                },
                new RestRestrictionEffect
                {
                    RestrictionType = "no_rest_multiple_exits",
                    BlockReason = "[PARANOIA]: Too many exits. Too exposed. You can't rest here."
                }
            }
        };
    }

    private static Trauma CreateAgoraphobia()
    {
        return new Trauma
        {
            TraumaId = "agoraphobia",
            Name = "[AGORAPHOBIA]",
            Description = "The open spaces feel wrong. Exposed. Vulnerable.",
            Category = TraumaCategory.Fear,
            Severity = TraumaSeverity.Mild,
            ProgressionLevel = 1,
            FlavorText = "Too much space. Too much emptiness. You're exposed.",
            Effects = new List<TraumaEffect>
            {
                new PassiveStressEffect
                {
                    StressPerTurn = 2,
                    Condition = "large_room"
                },
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "movement_cost_open",
                    Description = "+1 Movement cost in rooms without cover"
                }
            }
        };
    }

    private static Trauma CreateClaustrophobia()
    {
        return new Trauma
        {
            TraumaId = "claustrophobia",
            Name = "[CLAUSTROPHOBIA]",
            Description = "The walls are too close. Can't breathe. Can't think.",
            Category = TraumaCategory.Fear,
            Severity = TraumaSeverity.Mild,
            ProgressionLevel = 1,
            FlavorText = "The walls press in. Crushing. Suffocating.",
            Effects = new List<TraumaEffect>
            {
                new PassiveStressEffect
                {
                    StressPerTurn = 2,
                    Condition = "small_room"
                },
                new AttributePenaltyEffect
                {
                    AttributeName = "Will",
                    Penalty = 1,
                    Condition = "confined_space"
                }
            }
        };
    }

    private static Trauma CreateNyctophobia()
    {
        return new Trauma
        {
            TraumaId = "nyctophobia",
            Name = "[NYCTOPHOBIA]",
            Description = "The darkness has teeth. Things move in it.",
            Category = TraumaCategory.Fear,
            Severity = TraumaSeverity.Moderate,
            ProgressionLevel = 1,
            FlavorText = "Something shifted in the shadows. You're sure of it.",
            Effects = new List<TraumaEffect>
            {
                new StressMultiplierEffect
                {
                    Multiplier = 1.5f,
                    TriggerCondition = "dim_lighting"
                },
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "must_carry_light",
                    Description = "Must carry light source or gain +1 Stress/turn"
                }
            }
        };
    }

    // Isolation Category Traumas

    private static Trauma CreateIsolophobia()
    {
        return new Trauma
        {
            TraumaId = "isolophobia",
            Name = "[ISOLOPHOBIA]",
            Description = "Alone. Always alone. The silence is deafening.",
            Category = TraumaCategory.Isolation,
            Severity = TraumaSeverity.Mild,
            ProgressionLevel = 1,
            FlavorText = "The emptiness echoes. No one is coming. No one is left.",
            Effects = new List<TraumaEffect>
            {
                new PassiveStressEffect
                {
                    StressPerTurn = 1,
                    Condition = "alone"
                },
                new AttributePenaltyEffect
                {
                    AttributeName = "Will",
                    Penalty = 1,
                    Condition = null // Always active
                },
                new RestRestrictionEffect
                {
                    RestrictionType = "no_recovery_outside_settlement",
                    BlockReason = "[ISOLOPHOBIA]: Can't recover stress unless in a settlement"
                }
            }
        };
    }

    private static Trauma CreateSurvivorsGuilt()
    {
        return new Trauma
        {
            TraumaId = "survivors_guilt",
            Name = "[SURVIVOR'S GUILT]",
            Description = "They died. I lived. Why? Why them?",
            Category = TraumaCategory.Isolation,
            Severity = TraumaSeverity.Moderate,
            ProgressionLevel = 1,
            FlavorText = "You could have saved them. You should have saved them.",
            Effects = new List<TraumaEffect>
            {
                new StressMultiplierEffect
                {
                    Multiplier = 1.3f,
                    TriggerCondition = "ally_takes_damage"
                },
                new AttributePenaltyEffect
                {
                    AttributeName = "All",
                    Penalty = 1,
                    Condition = "companion_low_hp"
                }
            }
        };
    }

    // Paranoia Category Traumas

    private static Trauma CreateHypervigilance()
    {
        return new Trauma
        {
            TraumaId = "hypervigilance",
            Name = "[HYPERVIGILANCE]",
            Description = "Constant alertness. Never relaxed. Never safe.",
            Category = TraumaCategory.Paranoia,
            Severity = TraumaSeverity.Moderate,
            ProgressionLevel = 1,
            FlavorText = "Every sound is a threat. Every shadow is an enemy.",
            Effects = new List<TraumaEffect>
            {
                new RestRestrictionEffect
                {
                    RestrictionType = "reduced_effectiveness",
                    EffectivenessMultiplier = 0.5f,
                    BlockReason = "[HYPERVIGILANCE]: Rest effectiveness reduced to 50%"
                },
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "exhaustion_accumulates_faster",
                    Description = "Exhaustion accumulates faster (-1 to all checks after 8 hours)"
                }
            }
        };
    }

    private static Trauma CreateDistrust()
    {
        return new Trauma
        {
            TraumaId = "distrust",
            Name = "[DISTRUST]",
            Description = "Everyone lies. Everyone has an angle. Trust no one.",
            Category = TraumaCategory.Paranoia,
            Severity = TraumaSeverity.Moderate,
            ProgressionLevel = 1,
            FlavorText = "What do they want? What's their real agenda?",
            Effects = new List<TraumaEffect>
            {
                new AttributePenaltyEffect
                {
                    AttributeName = "Wits",
                    Penalty = 2,
                    Condition = "social"
                },
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "cannot_accept_unfamiliar_quests",
                    Description = "Cannot accept quests from unfamiliar NPCs"
                },
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "shop_prices_increased",
                    Description = "Shop prices +20% (merchants sense hostility)"
                }
            }
        };
    }

    // Obsession Category Traumas

    private static Trauma CreateCompulsiveCounting()
    {
        return new Trauma
        {
            TraumaId = "compulsive_counting",
            Name = "[COMPULSIVE COUNTING]",
            Description = "Count the tiles. Count the rivets. Keep counting. Stay in control.",
            Category = TraumaCategory.Obsession,
            Severity = TraumaSeverity.Mild,
            ProgressionLevel = 1,
            FlavorText = "One, two, three... lose count and lose control...",
            Effects = new List<TraumaEffect>
            {
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "extra_action_entering_rooms",
                    Description = "Must spend 1 extra action entering new rooms (counting ritual)"
                },
                new AttributePenaltyEffect
                {
                    AttributeName = "Wits",
                    Penalty = 1,
                    Condition = "investigation"
                }
            }
        };
    }

    private static Trauma CreateHoarding()
    {
        return new Trauma
        {
            TraumaId = "hoarding",
            Name = "[HOARDING]",
            Description = "Can't throw anything away. Might need it. Always might need it.",
            Category = TraumaCategory.Obsession,
            Severity = TraumaSeverity.Mild,
            ProgressionLevel = 1,
            FlavorText = "What if you need it later? Can't risk it. Keep it all.",
            Effects = new List<TraumaEffect>
            {
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "cannot_drop_common_items",
                    Description = "Cannot drop or sell Common items"
                },
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "inventory_weight_reduced",
                    Description = "Inventory weight limit reduced by 20%"
                }
            }
        };
    }

    // Dissociation Category Traumas

    private static Trauma CreateDissociation()
    {
        return new Trauma
        {
            TraumaId = "dissociation",
            Name = "[DISSOCIATION]",
            Description = "Nothing feels real anymore. Are you real? Am I?",
            Category = TraumaCategory.Dissociation,
            Severity = TraumaSeverity.Severe,
            ProgressionLevel = 1,
            FlavorText = "Everything is distant. Muffled. Like watching through glass.",
            Effects = new List<TraumaEffect>
            {
                new AttributePenaltyEffect
                {
                    AttributeName = "All",
                    Penalty = 1,
                    Condition = null // Always active
                },
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "chance_to_skip_turn",
                    Description = "10% chance per turn to 'skip' (lose awareness)"
                }
            }
        };
    }

    // Corruption Category Traumas

    private static Trauma CreateMachineAffinity()
    {
        return new Trauma
        {
            TraumaId = "machine_affinity",
            Name = "[MACHINE AFFINITY]",
            Description = "The machines make sense. More sense than people ever did.",
            Category = TraumaCategory.Corruption,
            Severity = TraumaSeverity.Severe,
            ProgressionLevel = 1,
            FlavorText = "Human thought is flawed. Biological. Inferior.",
            Effects = new List<TraumaEffect>
            {
                new ImmediateCorruptionEffect
                {
                    CorruptionAmount = 10
                },
                new AttributePenaltyEffect
                {
                    AttributeName = "Wits",
                    Penalty = 2,
                    Condition = "social_non_dvergr"
                },
                new BehaviorRestrictionEffect
                {
                    RestrictionType = "prefers_machine_company",
                    Description = "Preference for machine company over human"
                }
            }
        };
    }

    /// <summary>
    /// Selects a trauma to acquire based on the Breaking Point source
    /// </summary>
    public static Trauma SelectTraumaForSource(string source, int currentCorruption, Random rng)
    {
        // Machine/Corruption-related sources
        if (currentCorruption >= 60 || source.Contains("jotun", StringComparison.OrdinalIgnoreCase) ||
            source.Contains("machine", StringComparison.OrdinalIgnoreCase))
        {
            return CreateMachineAffinity();
        }

        // Darkness-related
        if (source.Contains("darkness", StringComparison.OrdinalIgnoreCase) ||
            source.Contains("dim", StringComparison.OrdinalIgnoreCase))
        {
            return CreateNyctophobia();
        }

        // Ambush/surprise
        if (source.Contains("ambush", StringComparison.OrdinalIgnoreCase) ||
            source.Contains("surprise", StringComparison.OrdinalIgnoreCase))
        {
            return rng.Next(2) == 0 ? CreateParanoia() : CreateHypervigilance();
        }

        // Default: select random fear or paranoia trauma
        var traumas = new[]
        {
            CreateParanoia(),
            CreateAgoraphobia(),
            CreateClaustrophobia(),
            CreateIsolophobia()
        };
        return traumas[rng.Next(traumas.Length)];
    }
}
