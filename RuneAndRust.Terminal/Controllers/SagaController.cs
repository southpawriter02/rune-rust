using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Terminal.Controllers;

/// <summary>
/// Controller for the Saga progression menu ("The Shrine") input handling (v0.4.0c).
/// Manages attribute selection and upgrade actions via Progression Points.
/// </summary>
/// <remarks>See: v0.4.0c (The Shrine) for Saga UI implementation.</remarks>
public class SagaController
{
    private readonly IProgressionService _progression;
    private readonly ILogger<SagaController> _logger;

    /// <summary>
    /// Ordered list of attributes as displayed in the Shrine UI.
    /// </summary>
    private static readonly CharacterAttribute[] AttributeOrder =
    {
        CharacterAttribute.Might,
        CharacterAttribute.Finesse,
        CharacterAttribute.Sturdiness,
        CharacterAttribute.Wits,
        CharacterAttribute.Will
    };

    /// <summary>
    /// Gets or sets the currently selected attribute index (0-4).
    /// </summary>
    public int SelectedIndex { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaController"/> class.
    /// </summary>
    /// <param name="progression">The progression service for attribute upgrades.</param>
    /// <param name="logger">The logger for traceability.</param>
    public SagaController(
        IProgressionService progression,
        ILogger<SagaController> logger)
    {
        _progression = progression;
        _logger = logger;
    }

    /// <summary>
    /// Processes a keypress and returns the resulting game phase.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <param name="character">The character to apply upgrades to.</param>
    /// <returns>The game phase to transition to (SagaMenu to stay, Exploration to exit).</returns>
    public GamePhase HandleInput(ConsoleKey key, Character character)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                    _logger.LogTrace("[Saga UI] Selection moved up to index {Index}", SelectedIndex);
                }
                break;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                if (SelectedIndex < AttributeOrder.Length - 1)
                {
                    SelectedIndex++;
                    _logger.LogTrace("[Saga UI] Selection moved down to index {Index}", SelectedIndex);
                }
                break;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                var attribute = AttributeOrder[SelectedIndex];
                var result = _progression.UpgradeAttribute(character, attribute);
                if (result.Success)
                {
                    _logger.LogInformation("[Saga UI] Successfully upgraded {Attribute}", attribute);
                }
                else
                {
                    _logger.LogDebug("[Saga UI] Upgrade failed: {Message}", result.Message);
                }
                break;

            case ConsoleKey.Escape:
            case ConsoleKey.Q:
                _logger.LogInformation("[Saga UI] Exiting Shrine, returning to Exploration");
                SelectedIndex = 0; // Reset for next entry
                return GamePhase.Exploration;
        }

        return GamePhase.SagaMenu; // Stay in menu
    }

    /// <summary>
    /// Gets the attribute at the specified index.
    /// </summary>
    /// <param name="index">The index (0-4).</param>
    /// <returns>The attribute at that index.</returns>
    public static CharacterAttribute GetAttributeAtIndex(int index)
    {
        if (index < 0 || index >= AttributeOrder.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return AttributeOrder[index];
    }

    /// <summary>
    /// Gets the total number of attributes in the list.
    /// </summary>
    public static int AttributeCount => AttributeOrder.Length;

    /// <summary>
    /// Resets the selection to the first attribute.
    /// Used when re-entering the Shrine.
    /// </summary>
    public void ResetSelection()
    {
        SelectedIndex = 0;
    }
}
