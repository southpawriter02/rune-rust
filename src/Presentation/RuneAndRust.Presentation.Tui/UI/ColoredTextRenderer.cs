using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Represents text with associated color information for rendering.
/// </summary>
/// <remarks>
/// <para>Factory methods provide convenient creation of colored text elements.</para>
/// <para>Renamed from ColoredText to ColoredTextRenderer in v0.13.5a for naming convention alignment.</para>
/// </remarks>
/// <param name="Text">The text content.</param>
/// <param name="Type">The message type for color lookup.</param>
/// <param name="ExplicitColor">Optional explicit color override.</param>
public readonly record struct ColoredTextRenderer(
    string Text,
    MessageType Type,
    ConsoleColor? ExplicitColor = null)
{
    /// <summary>Creates default colored text.</summary>
    public static ColoredTextRenderer Default(string text) => new(text, MessageType.Default);
    
    /// <summary>Creates info colored text.</summary>
    public static ColoredTextRenderer Info(string text) => new(text, MessageType.Info);
    
    /// <summary>Creates warning colored text.</summary>
    public static ColoredTextRenderer Warning(string text) => new(text, MessageType.Warning);
    
    /// <summary>Creates error colored text.</summary>
    public static ColoredTextRenderer Error(string text) => new(text, MessageType.Error);
    
    /// <summary>Creates combat hit colored text.</summary>
    public static ColoredTextRenderer CombatHit(string text) => new(text, MessageType.CombatHit);
    
    /// <summary>Creates combat miss colored text.</summary>
    public static ColoredTextRenderer CombatMiss(string text) => new(text, MessageType.CombatMiss);
    
    /// <summary>Creates combat heal colored text.</summary>
    public static ColoredTextRenderer CombatHeal(string text) => new(text, MessageType.CombatHeal);
    
    /// <summary>Creates combat damage colored text.</summary>
    public static ColoredTextRenderer CombatDamage(string text) => new(text, MessageType.CombatDamage);
    
    /// <summary>Creates critical hit colored text.</summary>
    public static ColoredTextRenderer CombatCritical(string text) => new(text, MessageType.CombatCritical);
    
    /// <summary>Creates common loot colored text.</summary>
    public static ColoredTextRenderer LootCommon(string text) => new(text, MessageType.LootCommon);
    
    /// <summary>Creates uncommon loot colored text.</summary>
    public static ColoredTextRenderer LootUncommon(string text) => new(text, MessageType.LootUncommon);
    
    /// <summary>Creates rare loot colored text.</summary>
    public static ColoredTextRenderer LootRare(string text) => new(text, MessageType.LootRare);
    
    /// <summary>Creates epic loot colored text.</summary>
    public static ColoredTextRenderer LootEpic(string text) => new(text, MessageType.LootEpic);
    
    /// <summary>Creates legendary loot colored text.</summary>
    public static ColoredTextRenderer LootLegendary(string text) => new(text, MessageType.LootLegendary);
    
    /// <summary>Creates dialogue colored text.</summary>
    public static ColoredTextRenderer Dialogue(string text) => new(text, MessageType.Dialogue);
    
    /// <summary>Creates description colored text.</summary>
    public static ColoredTextRenderer Description(string text) => new(text, MessageType.Description);
    
    /// <summary>Creates command colored text.</summary>
    public static ColoredTextRenderer Command(string text) => new(text, MessageType.Command);
    
    /// <summary>Creates success colored text.</summary>
    public static ColoredTextRenderer Success(string text) => new(text, MessageType.Success);
    
    /// <summary>Creates failure colored text.</summary>
    public static ColoredTextRenderer Failure(string text) => new(text, MessageType.Failure);
}

