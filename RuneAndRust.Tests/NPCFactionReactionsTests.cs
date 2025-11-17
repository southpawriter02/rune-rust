using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Core;
using RuneAndRust.Core.Territory;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.35.3: Test suite for NPCFactionReactions
/// Validates NPC behavior modifications based on territorial control
/// </summary>
[TestFixture]
public class NPCFactionReactionsTests
{
    private NPCFactionReactions _reactions;

    [SetUp]
    public void Setup()
    {
        _reactions = new NPCFactionReactions();
    }

    [Test]
    public void GetFactionModifiedBehavior_SameFaction_ReturnsFriendly()
    {
        // Arrange: Iron-Bane NPC in Iron-Bane controlled sector
        var npc = new NPC
        {
            Id = "iron_bane_guard",
            Name = "Guard Thorvald",
            Faction = FactionType.IronBanes,
            BaseDisposition = 0
        };
        string controllingFaction = "IronBanes";

        // Act
        var modifier = _reactions.GetFactionModifiedBehavior(npc, controllingFaction);

        // Assert
        Assert.That(modifier.HostilityLevel, Is.EqualTo("Friendly"));
        Assert.That(modifier.PriceModifier, Is.EqualTo(0.85)); // 15% discount
        Assert.That(modifier.InformationWillingness, Is.EqualTo("High"));
        Assert.That(modifier.DispositionModifier, Is.EqualTo(20));
    }

    [Test]
    public void GetFactionModifiedBehavior_HostileFaction_ReturnsSuspicious()
    {
        // Arrange: Iron-Bane NPC in God-Sleeper controlled sector
        var npc = new NPC
        {
            Id = "iron_bane_patrol",
            Name = "Patrol Leader Sigrid",
            Faction = FactionType.IronBanes,
            BaseDisposition = 0
        };
        string controllingFaction = "GodSleeperCultists";

        // Act
        var modifier = _reactions.GetFactionModifiedBehavior(npc, controllingFaction);

        // Assert
        Assert.That(modifier.HostilityLevel, Is.EqualTo("Suspicious"));
        Assert.That(modifier.PriceModifier, Is.EqualTo(1.25)); // 25% markup
        Assert.That(modifier.InformationWillingness, Is.EqualTo("Low"));
        Assert.That(modifier.DispositionModifier, Is.EqualTo(-20));
    }

    [Test]
    public void GetFactionModifiedBehavior_NeutralFaction_ReturnsNeutral()
    {
        // Arrange: Rust-Clan NPC in Jötun-Reader controlled sector
        var npc = new NPC
        {
            Id = "rust_trader",
            Name = "Trader Ulf",
            Faction = FactionType.RustClans,
            BaseDisposition = 0
        };
        string controllingFaction = "JotunReaders";

        // Act
        var modifier = _reactions.GetFactionModifiedBehavior(npc, controllingFaction);

        // Assert
        Assert.That(modifier.HostilityLevel, Is.EqualTo("Neutral"));
        Assert.That(modifier.PriceModifier, Is.EqualTo(1.0)); // No change
        Assert.That(modifier.InformationWillingness, Is.EqualTo("Medium"));
        Assert.That(modifier.DispositionModifier, Is.EqualTo(0));
    }

    [Test]
    public void GetFactionModifiedBehavior_IndependentControl_ReturnsNeutral()
    {
        // Arrange: Any NPC in independent sector
        var npc = new NPC
        {
            Id = "settler",
            Name = "Settler Erik",
            Faction = FactionType.Independents,
            BaseDisposition = 0
        };
        string controllingFaction = "Independents";

        // Act
        var modifier = _reactions.GetFactionModifiedBehavior(npc, controllingFaction);

        // Assert
        Assert.That(modifier.HostilityLevel, Is.EqualTo("Neutral"));
        Assert.That(modifier.PriceModifier, Is.EqualTo(1.0));
    }

    [Test]
    public void GetFactionModifiedBehavior_NoControl_ReturnsNeutral()
    {
        // Arrange: NPC in sector with no faction control
        var npc = new NPC
        {
            Id = "wanderer",
            Name = "Wanderer",
            Faction = FactionType.Independents,
            BaseDisposition = 0
        };

        // Act
        var modifier = _reactions.GetFactionModifiedBehavior(npc, null);

        // Assert
        Assert.That(modifier.HostilityLevel, Is.EqualTo("Neutral"));
        Assert.That(modifier.PriceModifier, Is.EqualTo(1.0));
    }

    [Test]
    public void ApplyBehaviorModifier_UpdatesDisposition()
    {
        // Arrange
        var npc = new NPC
        {
            Id = "test_npc",
            Name = "Test NPC",
            BaseDisposition = 10,
            InitialGreeting = "Hello."
        };
        var modifier = new NPCBehaviorModifier
        {
            DispositionModifier = 30,
            GreetingDialogue = "Welcome, friend!"
        };

        // Act
        string greeting = _reactions.ApplyBehaviorModifier(npc, modifier);

        // Assert
        Assert.That(npc.CurrentDisposition, Is.EqualTo(40)); // 10 + 30
        Assert.That(greeting, Is.EqualTo("Welcome, friend!"));
    }

    [Test]
    public void ApplyBehaviorModifier_ClampedToMax100()
    {
        // Arrange
        var npc = new NPC
        {
            Id = "test_npc",
            BaseDisposition = 90
        };
        var modifier = new NPCBehaviorModifier
        {
            DispositionModifier = 50 // Would exceed 100
        };

        // Act
        _reactions.ApplyBehaviorModifier(npc, modifier);

        // Assert
        Assert.That(npc.CurrentDisposition, Is.EqualTo(100)); // Clamped
    }

    [Test]
    public void ApplyBehaviorModifier_ClampedToMinNegative100()
    {
        // Arrange
        var npc = new NPC
        {
            Id = "test_npc",
            BaseDisposition = -80
        };
        var modifier = new NPCBehaviorModifier
        {
            DispositionModifier = -50 // Would go below -100
        };

        // Act
        _reactions.ApplyBehaviorModifier(npc, modifier);

        // Assert
        Assert.That(npc.CurrentDisposition, Is.EqualTo(-100)); // Clamped
    }

    [Test]
    public void GetModifiedPrice_AppliesDiscount()
    {
        // Arrange
        int basePrice = 100;
        var modifier = new NPCBehaviorModifier { PriceModifier = 0.85 };

        // Act
        int modifiedPrice = _reactions.GetModifiedPrice(basePrice, modifier);

        // Assert
        Assert.That(modifiedPrice, Is.EqualTo(85)); // 15% discount
    }

    [Test]
    public void GetModifiedPrice_AppliesMarkup()
    {
        // Arrange
        int basePrice = 100;
        var modifier = new NPCBehaviorModifier { PriceModifier = 1.25 };

        // Act
        int modifiedPrice = _reactions.GetModifiedPrice(basePrice, modifier);

        // Assert
        Assert.That(modifiedPrice, Is.EqualTo(125)); // 25% markup
    }

    [Test]
    public void IsDialogueAvailable_HighWillingness_AllAvailable()
    {
        // Arrange
        var modifier = new NPCBehaviorModifier { InformationWillingness = "High" };

        // Act & Assert
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Rumors"), Is.True);
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Quests"), Is.True);
        Assert.That(_reactions.IsDialogueAvailable(modifier, "FactionInfo"), Is.True);
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Trade"), Is.True);
    }

    [Test]
    public void IsDialogueAvailable_LowWillingness_LimitedOptions()
    {
        // Arrange
        var modifier = new NPCBehaviorModifier { InformationWillingness = "Low" };

        // Act & Assert
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Rumors"), Is.False);
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Quests"), Is.False);
        Assert.That(_reactions.IsDialogueAvailable(modifier, "FactionInfo"), Is.False);
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Trade"), Is.True); // Trade always available
    }

    [Test]
    public void IsDialogueAvailable_MediumWillingness_RumorsAvailable()
    {
        // Arrange
        var modifier = new NPCBehaviorModifier { InformationWillingness = "Medium" };

        // Act & Assert
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Rumors"), Is.True);
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Quests"), Is.False);
        Assert.That(_reactions.IsDialogueAvailable(modifier, "Trade"), Is.True);
    }
}
