using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Conditions;
using RuneAndRust.Core.Effects;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with sample dialogue trees for testing.
/// </summary>
/// <remarks>
/// v0.4.2e introduces sample dialogue trees:
/// - npc_old_scavenger: Iron-Banes elder with faction-gated options
/// - npc_kjartan: Dvergr smith with attribute-based dialogue
///
/// All dialogue text must be Domain 4 compliant (no precision measurements).
///
/// See: v0.4.2b (The Lexicon) for Dialogue System design.
/// See: v0.4.2c (The Voice) for DialogueService implementation.
/// </remarks>
public static class DialogueSeeder
{
    /// <summary>
    /// Seeds sample dialogue trees if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        if (await context.DialogueTrees.AnyAsync())
        {
            logger?.LogDebug("[Seeder] Dialogue trees already exist, skipping seed");
            return;
        }

        logger?.LogInformation("[Seeder] Seeding Dialogue Trees...");

        var trees = GetDialogueTrees();

        foreach (var tree in trees)
        {
            await context.DialogueTrees.AddAsync(tree);
        }

        await context.SaveChangesAsync();

        logger?.LogInformation("[Seeder] Seeded {Count} dialogue trees", trees.Count);
    }

    /// <summary>
    /// Gets the sample dialogue tree definitions.
    /// </summary>
    private static List<DialogueTree> GetDialogueTrees()
    {
        return new List<DialogueTree>
        {
            CreateOldScavengerTree(),
            CreateKjartanTree()
        };
    }

    /// <summary>
    /// Creates the Old Scavenger dialogue tree.
    /// An Iron-Banes elder who offers trade and information.
    /// </summary>
    private static DialogueTree CreateOldScavengerTree()
    {
        var treeId = Guid.NewGuid();

        var tree = new DialogueTree
        {
            Id = treeId,
            TreeId = "npc_old_scavenger",
            NpcName = "Old Scavenger",
            NpcTitle = "Iron-Bane Elder",
            RootNodeId = "root",
            AssociatedFaction = FactionType.IronBanes
        };

        // Root greeting node
        var rootNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "root",
            SpeakerName = "Old Scavenger",
            Text = "Ah, another wanderer from the upper ruins. The wind carries strange tidings these days. " +
                   "What brings you to our humble camp?",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "I'm looking for supplies.",
                    NextNodeId = "trade",
                    DisplayOrder = 1
                },
                new DialogueOption
                {
                    Text = "What do you know about the deep passages?",
                    NextNodeId = "rumors",
                    DisplayOrder = 2
                },
                new DialogueOption
                {
                    Text = "[Iron-Banes: Friendly] I heard you might have something special for friends of the clan.",
                    NextNodeId = "special_offer",
                    DisplayOrder = 3,
                    VisibilityMode = OptionVisibility.ShowLocked,
                    Conditions = new List<DialogueCondition>
                    {
                        new ReputationCondition
                        {
                            Faction = FactionType.IronBanes,
                            MinDisposition = Disposition.Friendly
                        }
                    }
                },
                new DialogueOption
                {
                    Text = "Farewell.",
                    NextNodeId = null, // Terminal
                    DisplayOrder = 99
                }
            }
        };

        // Trade node
        var tradeNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "trade",
            SpeakerName = "Old Scavenger",
            Text = "Supplies, eh? We've got rations, rope, torches—the essentials. " +
                   "Nothing fancy, but it'll keep you alive in the dark. " +
                   "Prices are fair for those who deal honestly with the Iron-Banes.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "Show me what you have.",
                    NextNodeId = "trade_complete",
                    DisplayOrder = 1,
                    Effects = new List<DialogueEffect>
                    {
                        new ModifyReputationEffect
                        {
                            Faction = FactionType.IronBanes,
                            Amount = 5
                        }
                    }
                },
                new DialogueOption
                {
                    Text = "I'll think about it.",
                    NextNodeId = "root",
                    DisplayOrder = 2
                }
            }
        };

        // Trade complete node
        var tradeCompleteNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "trade_complete",
            SpeakerName = "Old Scavenger",
            Text = "Good doing business with you. May your path be clear of Blight-touched.",
            IsTerminal = true,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "Thank you. Farewell.",
                    NextNodeId = null,
                    DisplayOrder = 1
                }
            }
        };

        // Rumors node
        var rumorsNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "rumors",
            SpeakerName = "Old Scavenger",
            Text = "*The elder's eyes narrow* The deep passages? You've got more courage than sense, friend. " +
                   "The Dvergr guard those ways jealously, and what lies beyond... " +
                   "Well, let's just say not everyone who goes down comes back up.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "[WITS 6] There must be something valuable down there to warrant such danger.",
                    NextNodeId = "deep_secret",
                    DisplayOrder = 1,
                    VisibilityMode = OptionVisibility.ShowLocked,
                    Conditions = new List<DialogueCondition>
                    {
                        new AttributeCondition
                        {
                            Attribute = CharacterAttribute.Wits,
                            Comparison = ComparisonType.GreaterThanOrEqual,
                            Threshold = 6
                        }
                    }
                },
                new DialogueOption
                {
                    Text = "Thank you for the warning.",
                    NextNodeId = "root",
                    DisplayOrder = 2
                }
            }
        };

        // Deep secret node (hidden knowledge)
        var deepSecretNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "deep_secret",
            SpeakerName = "Old Scavenger",
            Text = "*He leans closer, voice dropping* Sharp one, aren't you? " +
                   "They say there's a forge down there—Pre-Glitch make. The Dvergr found it generations ago. " +
                   "That's where their legendary craft comes from. But getting there... " +
                   "that's another matter entirely.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "How would one gain passage?",
                    NextNodeId = "passage_hint",
                    DisplayOrder = 1
                },
                new DialogueOption
                {
                    Text = "Interesting. I'll remember that.",
                    NextNodeId = "root",
                    DisplayOrder = 2
                }
            }
        };

        // Passage hint node
        var passageHintNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "passage_hint",
            SpeakerName = "Old Scavenger",
            Text = "The Dvergr respect strength and craftsmanship. Bring them something of value— " +
                   "not coin, mind you, but something that shows you understand the old ways. " +
                   "A relic, perhaps. Something that speaks of times before the breaking.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "I'll keep that in mind. Thank you.",
                    NextNodeId = "root",
                    DisplayOrder = 1,
                    Effects = new List<DialogueEffect>
                    {
                        new ModifyReputationEffect
                        {
                            Faction = FactionType.IronBanes,
                            Amount = 10
                        },
                        new SetFlagEffect
                        {
                            FlagKey = "learned_dvergr_hint",
                            Value = true
                        }
                    }
                }
            }
        };

        // Special offer node (faction-gated)
        var specialOfferNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "special_offer",
            SpeakerName = "Old Scavenger",
            Text = "*A rare smile crosses his weathered face* Word travels fast in the camps. " +
                   "You've done right by our people. Yes, I have something set aside— " +
                   "a map of the old service tunnels. Dangerous paths, but faster than the main routes. " +
                   "Consider it yours.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "I'm honored. Thank you.",
                    NextNodeId = "special_complete",
                    DisplayOrder = 1,
                    Effects = new List<DialogueEffect>
                    {
                        new SetFlagEffect
                        {
                            FlagKey = "received_tunnel_map",
                            Value = true
                        }
                    }
                }
            }
        };

        // Special offer complete node
        var specialCompleteNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "special_complete",
            SpeakerName = "Old Scavenger",
            Text = "Use it well. And remember—the Iron-Banes look after their own. " +
                   "May your salvage be plentiful.",
            IsTerminal = true,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "Until we meet again.",
                    NextNodeId = null,
                    DisplayOrder = 1
                }
            }
        };

        tree.Nodes = new List<DialogueNode>
        {
            rootNode,
            tradeNode,
            tradeCompleteNode,
            rumorsNode,
            deepSecretNode,
            passageHintNode,
            specialOfferNode,
            specialCompleteNode
        };

        return tree;
    }

    /// <summary>
    /// Creates the Kjartan dialogue tree.
    /// A Dvergr smith who offers crafting services.
    /// </summary>
    private static DialogueTree CreateKjartanTree()
    {
        var treeId = Guid.NewGuid();

        var tree = new DialogueTree
        {
            Id = treeId,
            TreeId = "npc_kjartan",
            NpcName = "Kjartan",
            NpcTitle = "Dvergr Smith",
            RootNodeId = "root",
            AssociatedFaction = FactionType.Dvergr
        };

        // Root greeting node
        var rootNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "root",
            SpeakerName = "Kjartan",
            Text = "*The stocky figure barely glances up from the anvil* " +
                   "Surface-dweller. State your business quickly—the forge waits for no one.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "I need repairs on my equipment.",
                    NextNodeId = "repairs",
                    DisplayOrder = 1
                },
                new DialogueOption
                {
                    Text = "[MIGHT 7] I've heard Dvergr steel is the finest in all the ruins.",
                    NextNodeId = "flattery_success",
                    DisplayOrder = 2,
                    VisibilityMode = OptionVisibility.ShowLocked,
                    Conditions = new List<DialogueCondition>
                    {
                        new AttributeCondition
                        {
                            Attribute = CharacterAttribute.Might,
                            Comparison = ComparisonType.GreaterThanOrEqual,
                            Threshold = 7
                        }
                    }
                },
                new DialogueOption
                {
                    Text = "[Dvergr: Friendly] Kjartan, I've brought something that might interest you.",
                    NextNodeId = "special_commission",
                    DisplayOrder = 3,
                    VisibilityMode = OptionVisibility.Hidden,
                    Conditions = new List<DialogueCondition>
                    {
                        new ReputationCondition
                        {
                            Faction = FactionType.Dvergr,
                            MinDisposition = Disposition.Friendly
                        }
                    }
                },
                new DialogueOption
                {
                    Text = "I'll leave you to your work.",
                    NextNodeId = null,
                    DisplayOrder = 99
                }
            }
        };

        // Repairs node
        var repairsNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "repairs",
            SpeakerName = "Kjartan",
            Text = "*He sets down his hammer and examines you with shrewd eyes* " +
                   "Repairs, eh? Let's see what damage you've managed to inflict on perfectly good steel. " +
                   "My work isn't cheap, but it'll last longer than you will.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "Fair enough. What's your price?",
                    NextNodeId = "repair_complete",
                    DisplayOrder = 1
                },
                new DialogueOption
                {
                    Text = "I'll manage on my own.",
                    NextNodeId = "root",
                    DisplayOrder = 2
                }
            }
        };

        // Repair complete node
        var repairCompleteNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "repair_complete",
            SpeakerName = "Kjartan",
            Text = "*He takes your equipment with surprising gentleness* " +
                   "Come back when the forge-fires dim. It'll be ready. " +
                   "And try not to damage it so quickly this time.",
            IsTerminal = true,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "Thank you, smith.",
                    NextNodeId = null,
                    DisplayOrder = 1,
                    Effects = new List<DialogueEffect>
                    {
                        new ModifyReputationEffect
                        {
                            Faction = FactionType.Dvergr,
                            Amount = 3
                        }
                    }
                }
            }
        };

        // Flattery success node (for strong characters)
        var flatterySuccessNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "flattery_success",
            SpeakerName = "Kjartan",
            Text = "*For a moment, something like pride flickers in his eyes* " +
                   "You've got the look of one who understands the weight of good steel. " +
                   "Perhaps you're not entirely without merit, surface-dweller. " +
                   "The Dvergr have kept the old ways alive when others forgot.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "Could you teach me about Dvergr smithing?",
                    NextNodeId = "smithing_lore",
                    DisplayOrder = 1
                },
                new DialogueOption
                {
                    Text = "I'd like to commission something special.",
                    NextNodeId = "commission",
                    DisplayOrder = 2
                }
            }
        };

        // Smithing lore node
        var smithingLoreNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "smithing_lore",
            SpeakerName = "Kjartan",
            Text = "*He pauses his work* The secrets of Dvergr craft are not shared lightly. " +
                   "But know this: we do not merely shape metal. We listen to it. " +
                   "Every piece has a voice, a purpose waiting to be revealed. " +
                   "The ancients understood this. We merely remember what others have forgotten.",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "That's a profound way to view craftsmanship.",
                    NextNodeId = "lore_complete",
                    DisplayOrder = 1,
                    Effects = new List<DialogueEffect>
                    {
                        new ModifyReputationEffect
                        {
                            Faction = FactionType.Dvergr,
                            Amount = 10
                        },
                        new SetFlagEffect
                        {
                            FlagKey = "learned_dvergr_philosophy",
                            Value = true
                        }
                    }
                }
            }
        };

        // Lore complete node
        var loreCompleteNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "lore_complete",
            SpeakerName = "Kjartan",
            Text = "*He nods, returning to his anvil* " +
                   "Perhaps there's hope for surface-dwellers yet. " +
                   "Remember what I've told you. The knowledge may serve you well.",
            IsTerminal = true,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "I will. Thank you, Kjartan.",
                    NextNodeId = null,
                    DisplayOrder = 1
                }
            }
        };

        // Commission node
        var commissionNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "commission",
            SpeakerName = "Kjartan",
            Text = "*His eyes narrow appraisingly* A special commission requires special payment. " +
                   "I don't deal in surface coin alone. Bring me materials worthy of the work— " +
                   "salvage from the deep places, untouched by Blight. Then we'll talk.",
            IsTerminal = true,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "I'll see what I can find.",
                    NextNodeId = null,
                    DisplayOrder = 1,
                    Effects = new List<DialogueEffect>
                    {
                        new SetFlagEffect
                        {
                            FlagKey = "kjartan_commission_offered",
                            Value = true
                        }
                    }
                }
            }
        };

        // Special commission node (faction-gated, hidden until qualified)
        var specialCommissionNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "special_commission",
            SpeakerName = "Kjartan",
            Text = "*He actually stops working and faces you directly* " +
                   "You've proven yourself to the Dvergr. That is no small thing. " +
                   "What have you brought me, friend?",
            IsTerminal = false,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "A Pre-Glitch component I found in the deep ruins.",
                    NextNodeId = "special_reward",
                    DisplayOrder = 1
                },
                new DialogueOption
                {
                    Text = "Actually, I wanted to discuss a custom weapon.",
                    NextNodeId = "custom_weapon",
                    DisplayOrder = 2
                }
            }
        };

        // Special reward node
        var specialRewardNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "special_reward",
            SpeakerName = "Kjartan",
            Text = "*His calloused hands tremble slightly as he examines the component* " +
                   "By the ancestors... this is genuine. Unblemished. " +
                   "You've done the Dvergr a great service. Take this blade— " +
                   "I forged it myself, in the old way. May it serve you well.",
            IsTerminal = true,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "I am honored, Kjartan. Truly.",
                    NextNodeId = null,
                    DisplayOrder = 1,
                    Effects = new List<DialogueEffect>
                    {
                        new ModifyReputationEffect
                        {
                            Faction = FactionType.Dvergr,
                            Amount = 25
                        },
                        new SetFlagEffect
                        {
                            FlagKey = "received_dvergr_blade",
                            Value = true
                        }
                    }
                }
            }
        };

        // Custom weapon node
        var customWeaponNode = new DialogueNode
        {
            TreeId = treeId,
            NodeId = "custom_weapon",
            SpeakerName = "Kjartan",
            Text = "*He strokes his beard thoughtfully* " +
                   "A custom piece for a friend of the Dvergr. Yes, this can be done. " +
                   "Tell me what you seek, and I will tell you what it will require.",
            IsTerminal = true,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Text = "Let me think on it and return.",
                    NextNodeId = null,
                    DisplayOrder = 1
                }
            }
        };

        tree.Nodes = new List<DialogueNode>
        {
            rootNode,
            repairsNode,
            repairCompleteNode,
            flatterySuccessNode,
            smithingLoreNode,
            loreCompleteNode,
            commissionNode,
            specialCommissionNode,
            specialRewardNode,
            customWeaponNode
        };

        return tree;
    }
}
