namespace RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Data collected during the character creation wizard.
/// </summary>
public class CharacterCreationData
{
    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected race ID.
    /// </summary>
    public string RaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected class ID.
    /// </summary>
    public string ClassId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the allocated stat values.
    /// </summary>
    public Dictionary<string, int> Stats { get; } = new()
    {
        ["Strength"] = 10,
        ["Dexterity"] = 10,
        ["Constitution"] = 10,
        ["Intelligence"] = 10,
        ["Wisdom"] = 10,
        ["Charisma"] = 10
    };

    /// <summary>
    /// Gets or sets the selected portrait ID.
    /// </summary>
    public string PortraitId { get; set; } = string.Empty;
}
