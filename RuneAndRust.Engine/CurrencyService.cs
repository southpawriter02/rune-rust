using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for managing Dvergr Cogs currency system (v0.9)
/// </summary>
public class CurrencyService
{
    private static readonly ILogger _log = Log.ForContext<CurrencyService>();

    /// <summary>
    /// Check if character can afford a given cost
    /// </summary>
    public bool CanAfford(PlayerCharacter character, int cost)
    {
        return character.Currency >= cost;
    }

    /// <summary>
    /// Add currency to character with logging
    /// </summary>
    public void AddCurrency(PlayerCharacter character, int amount, string source)
    {
        if (amount <= 0)
        {
            _log.Warning("Attempted to add non-positive currency: Character={CharacterName}, Amount={Amount}, Source={Source}",
                character.Name, amount, source);
            return;
        }

        character.Currency += amount;

        _log.Information("Currency gained: Character={CharacterName}, Amount={Amount}, Source={Source}, NewTotal={NewTotal}",
            character.Name, amount, source, character.Currency);
    }

    /// <summary>
    /// Spend currency if character can afford it
    /// </summary>
    public bool SpendCurrency(PlayerCharacter character, int amount, string purpose)
    {
        if (amount <= 0)
        {
            _log.Warning("Attempted to spend non-positive currency: Character={CharacterName}, Amount={Amount}, Purpose={Purpose}",
                character.Name, amount, purpose);
            return false;
        }

        if (!CanAfford(character, amount))
        {
            _log.Warning("Insufficient currency: Character={CharacterName}, Required={Required}, Available={Available}, Purpose={Purpose}",
                character.Name, amount, character.Currency, purpose);
            return false;
        }

        character.Currency -= amount;

        _log.Information("Currency spent: Character={CharacterName}, Amount={Amount}, Purpose={Purpose}, Remaining={Remaining}",
            character.Name, amount, purpose, character.Currency);

        return true;
    }

    /// <summary>
    /// Get formatted currency display string
    /// </summary>
    public string GetCurrencyDisplay(int amount)
    {
        return $"{amount} Cogs ⚙";
    }

    /// <summary>
    /// Get formatted currency display for character
    /// </summary>
    public string GetCurrencyDisplay(PlayerCharacter character)
    {
        return GetCurrencyDisplay(character.Currency);
    }
}
