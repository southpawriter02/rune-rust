using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds interaction descriptors from the descriptors-interaction.md specification.
/// Contains 150+ narrative text fragments for interactive objects, examination results,
/// discovery moments, and skill-specific interactions.
/// </summary>
public static class InteractionDescriptorSeeder
{
    private static int _idCounter;

    /// <summary>
    /// Gets all seeded interaction descriptors.
    /// </summary>
    public static IEnumerable<InteractionDescriptor> GetAllDescriptors()
    {
        _idCounter = 0;
        return GetMechanicalObjectDescriptors()
            .Concat(GetContainerDescriptors())
            .Concat(GetWitsSuccessDescriptors())
            .Concat(GetWitsFailureDescriptors())
            .Concat(GetObjectExaminationDescriptors())
            .Concat(GetDiscoveryDescriptors())
            .Concat(GetContainerInteractionDescriptors())
            .Concat(GetSkillSpecificDescriptors())
            .Concat(GetEnvironmentalDescriptors());
    }

    private static Guid NextId() =>
        new Guid($"00000000-0000-0000-0000-{(++_idCounter):D12}");

    #region Mechanical Objects (Section 2.1)

    private static IEnumerable<InteractionDescriptor> GetMechanicalObjectDescriptors()
    {
        // Levers
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Lever", "Active",
            "A lever locked in the up position, mechanism still engaged");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Lever", "Inactive",
            "A lever pulled down, whatever it controlled now dormant");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Lever", "Stuck",
            "A lever frozen by centuries of rust. It would take force to move");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Lever", "Unknown",
            "A lever. No markings. No indication of what it controls");

        // Doors
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Door", "Open",
            "A door left ajar. What passed through didn't bother to close it");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Door", "Closed",
            "A sealed door. The mechanism might still function");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Door", "Locked",
            "The door is sealed. Whatever lock controls it isn't visible");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Door", "Destroyed",
            "A door torn from its hinges. Something wanted through badly");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Door", "Rusted",
            "A door fused shut by rust. This portal hasn't moved in centuries");

        // Terminals (Oracle-Boxes)
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Terminal", "Active",
            "The oracle-box hums with invisible fire, glass face swimming with ghost-light");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Terminal", "Dormant",
            "The screen is dark. But the box still ticks, waiting");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Terminal", "Corrupted",
            "Static crawls across the display. Symbols writhe, half-formed");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Terminal", "Dead",
            "Nothing. The box is cold, its purpose long since ended");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.MechanicalObject, "Terminal", "Glitched",
            "The oracle-box shows things that shouldn't be. Images from when?");
    }

    #endregion

    #region Container Objects (Section 2.2)

    private static IEnumerable<InteractionDescriptor> GetContainerDescriptors()
    {
        // Salvage Crates
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "SalvageCrate", "Intact",
            "A sealed crate, markings still visible. Someone thought the contents worth protecting");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "SalvageCrate", "Damaged",
            "The crate has been breached. Whatever was here may still remain");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "SalvageCrate", "Opened",
            "Already searched. What's left is what wasn't worth taking");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "SalvageCrate", "Trapped",
            "Something about the seal is wrong. This crate was secured for a reason");

        // Corpses
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "Corpse", "Fresh",
            "Blood still wet. This happened recently. Whatever did this might still be here");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "Corpse", "Recent",
            "Days old. The smell is starting. The useful items might remain");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "Corpse", "Old",
            "Bones and dust. This one has been here longer than you've been alive");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "Corpse", "Ancient",
            "Mummified remains. They've been here since before the Glitch");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "Corpse", "Forlorn",
            "A Forlorn, finally still. Whatever humanity remained is gone now");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "Corpse", "Stripped",
            "Already looted. Whoever got here first took what mattered");

        // Resource Nodes
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "ResourceNode", "OreVein",
            "Metal gleams in the stone. Someone has to dig it out");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "ResourceNode", "SalvagePile",
            "Loose components, probably functional. Worth sifting through");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "ResourceNode", "ScrapCluster",
            "Junk, mostly. But junk can become armor. Weapons. Life");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Container, "ResourceNode", "RareFind",
            "Your heart skips. This is the good stuff. Finally");
    }

    #endregion

    #region WITS Check Descriptors (Section 3.1)

    private static IEnumerable<InteractionDescriptor> GetWitsSuccessDescriptors()
    {
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "WitsCheck", "Low",
            "You notice something. There's more here than appears at first glance");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "WitsCheck", "Medium",
            "Details emerge. The mechanism here isn't quite what it seems");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "WitsCheck", "High",
            "Everything becomes clear. You see how this works, what it hides, what it means");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "WitsCheck", "Critical",
            "You understand this object in ways its makers might not have intended");
    }

    private static IEnumerable<InteractionDescriptor> GetWitsFailureDescriptors()
    {
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsFailure, "WitsCheck", "NearMiss",
            "Something... you're missing something. You can feel it");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsFailure, "WitsCheck", "Failure",
            "Nothing stands out. It's just what it appears to be. Or so you think");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsFailure, "WitsCheck", "Bad",
            "You stare at it. Nothing. Just an object");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsFailure, "WitsCheck", "CriticalFail",
            "You have no idea what you're looking at. Worse, you're confident you do");
    }

    #endregion

    #region Object-Specific Examination (Section 3.2)

    private static IEnumerable<InteractionDescriptor> GetObjectExaminationDescriptors()
    {
        // Technology Examination
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Technology", "Function",
            "The mechanism reveals its purpose. Here is where you interact");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Technology", "Status",
            "You assess its condition. Damaged, but repairable. Maybe");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Technology", "Origin",
            "Dvergr-make. The quality is unmistakable, even corrupted");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Technology", "Danger",
            "Something is wrong here. This object is trapped, or worse");

        // Body Examination
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Body", "Cause",
            "The wound tells a story. They didn't die peacefully");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Body", "Identity",
            "Clan markings. You recognize the faction. This was one of yours");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Body", "Loot",
            "They carried something useful. Still do, technically");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Body", "Warning",
            "Whatever killed them left marks. Now you know what to fear");

        // Environment Examination
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Environment", "Hidden",
            "There — a seam where there shouldn't be one. Concealed");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Environment", "Trap",
            "The dust pattern is wrong. Something has been placed here recently");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Environment", "Path",
            "Air moves. Faintly. There's another way through somewhere");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.WitsSuccess, "Environment", "Hazard",
            "The stains on the floor tell you where not to step");
    }

    #endregion

    #region Discovery Descriptors (Section 4)

    private static IEnumerable<InteractionDescriptor> GetDiscoveryDescriptors()
    {
        // Secret Discovery
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Secret", "Minor",
            "You find a hidden compartment. Someone wanted this overlooked");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Secret", "Minor2",
            "Behind the panel — a cache. Small, but valuable");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Secret", "Moderate",
            "A concealed passage. Not on any map you've seen");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Secret", "Moderate2",
            "The wall is false. Beyond it... something worth hiding");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Secret", "Major",
            "A sealed vault. Whatever they hid here, they MEANT to hide it");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Secret", "Major2",
            "No one has been here. No one. Until you");

        // Lore Discovery
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Lore", "DataSlate",
            "A data-slate, still readable. Someone left a record");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Lore", "DataSlate2",
            "Words from the past. The dead speak, if you're willing to listen");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Lore", "Inscription",
            "Runes carved into the wall. Not decoration — instruction. Or warning");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Lore", "Inscription2",
            "Ancient words. You understand some of them. Wish you didn't");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Lore", "Art",
            "A mural, faded but visible. This is what the world looked like, before");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Lore", "Note",
            "Handwriting. Personal. This was meant for someone specific");

        // Danger Discovery
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Danger", "Trap",
            "Your instincts scream. Something here is waiting to kill you");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Danger", "Trap2",
            "The trigger mechanism is barely visible. One more step and...");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Danger", "Ambush",
            "Movement. At the edge of vision. You're not alone");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Danger", "Ambush2",
            "Too quiet. This is a kill-box. You're in it");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Danger", "Hazard",
            "The air is wrong. Toxic. You need to leave or stop breathing");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Discovery, "Danger", "Hazard2",
            "The floor is weak. One more person and it gives way");
    }

    #endregion

    #region Container Interaction Descriptors (Section 5)

    private static IEnumerable<InteractionDescriptor> GetContainerInteractionDescriptors()
    {
        // Opening Actions
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Opening", "Unlock",
            "The lock yields. The contents are yours now");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Opening", "Force",
            "Metal groans, then gives. Brute force has its uses");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Opening", "Bypass",
            "The mechanism clicks. You're in without triggering anything");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Opening", "Fail",
            "It won't budge. Whatever sealed this meant business");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Opening", "TrapTrigger",
            "Click. That wasn't the lock. That was something else");

        // Loot Quality
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Poor",
            "Junk, mostly. But junk is better than nothing");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Poor2",
            "Someone else got here first. Leftovers only");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Average",
            "Usable supplies. Nothing remarkable, but nothing worthless");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Average2",
            "This will keep you alive a little longer. Good enough");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Good",
            "Quality goods. Someone lost this; you gained it");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Good2",
            "Real resources. This trip just paid for itself");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Excellent",
            "Your breath catches. This is worth the bruises");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Excellent2",
            "The good stuff. Finally. Someone is watching over you");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.ContainerInteraction, "Loot", "Jackpot",
            "You've found something IMPORTANT. This changes things");
    }

    #endregion

    #region Skill-Specific Descriptors (Section 6)

    private static IEnumerable<InteractionDescriptor> GetSkillSpecificDescriptors()
    {
        // Crafting Trades
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Trade", "Bodging",
            "You assess the salvage. With the right parts, this could be fixed");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Trade", "Bodging2",
            "Broken, but not useless. Your hands already know what to do");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Trade", "FieldMedicine",
            "Medical supplies. Some expired, some contaminated, some... usable");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Trade", "FieldMedicine2",
            "You inventory what heals and what harms. The line is thin");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Trade", "Runeforging",
            "The inscriptions are degraded but readable. The old strength remains here");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Trade", "Runeforging2",
            "Rune-material. The substrate accepts inscription — if you're careful");

        // Specializations
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Specialization", "JotunReader",
            "The mechanism is Old World — pre-Glitch architecture. You recognize the patterns");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Specialization", "JotunReader2",
            "This is Giant-craft. Original. Unmolested since the Glitch");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Specialization", "RuinStalker",
            "You evaluate the obstacles. Routes of entry, points of exit. Professional assessment");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.SkillSpecific, "Specialization", "RuinStalker2",
            "Trap-lines. Patrol patterns. This place was secured. By whom?");
    }

    #endregion

    #region Environmental Descriptors (Section 7)

    private static IEnumerable<InteractionDescriptor> GetEnvironmentalDescriptors()
    {
        // Climbing/Traversal
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Environmental, "Traversal", "Easy",
            "You pull yourself up. Simple");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Environmental, "Traversal", "Standard",
            "Handholds present themselves. You ascend with care");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Environmental, "Traversal", "Difficult",
            "The climb demands everything. But you make it");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Environmental, "Traversal", "Fail",
            "Your grip fails. Gravity has opinions");

        // Disabling/Repair
        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Environmental, "Repair", "DisableTrap",
            "The mechanism is neutralized. Safe, now");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Environmental, "Repair", "Repair",
            "With improvised tools and stubbornness, you make it work again");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Environmental, "Repair", "Sabotage",
            "It won't function for anyone else now. Your work here is done");

        yield return new InteractionDescriptor(NextId(),
            InteractionCategory.Environmental, "Repair", "Catastrophic",
            "Something breaks. Spectacularly. Permanently");
    }

    #endregion
}
