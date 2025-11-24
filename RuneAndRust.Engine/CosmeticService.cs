using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.41: Service for managing cosmetic customization
/// Handles cosmetic loadouts and application
/// </summary>
public class CosmeticService
{
    private readonly ILogger _logger;
    private readonly CosmeticRepository _cosmeticRepo;

    public CosmeticService(CosmeticRepository cosmeticRepo)
    {
        _logger = Log.ForContext<CosmeticService>();
        _cosmeticRepo = cosmeticRepo;
    }

    #region Cosmetic Management

    /// <summary>
    /// Get all cosmetics with unlock status
    /// </summary>
    public List<Cosmetic> GetAllCosmetics()
    {
        return _cosmeticRepo.GetAll();
    }

    /// <summary>
    /// Get cosmetics by type
    /// </summary>
    public List<Cosmetic> GetCosmeticsByType(CosmeticType type)
    {
        return _cosmeticRepo.GetByType(type);
    }

    /// <summary>
    /// Get unlocked cosmetics for account
    /// </summary>
    public List<Cosmetic> GetUnlockedCosmetics(int accountId)
    {
        return _cosmeticRepo.GetUnlockedCosmetics(accountId);
    }

    /// <summary>
    /// Get unlocked cosmetics by type
    /// </summary>
    public List<Cosmetic> GetUnlockedCosmeticsByType(int accountId, CosmeticType type)
    {
        return _cosmeticRepo.GetUnlockedCosmetics(accountId)
            .Where(c => c.Type == type)
            .ToList();
    }

    /// <summary>
    /// Check if cosmetic is unlocked
    /// </summary>
    public bool IsUnlocked(int accountId, string cosmeticId)
    {
        return _cosmeticRepo.IsUnlocked(accountId, cosmeticId);
    }

    /// <summary>
    /// Unlock cosmetic for account
    /// </summary>
    public void UnlockCosmetic(int accountId, string cosmeticId)
    {
        _logger.Information("Unlocking cosmetic: AccountID={AccountId}, CosmeticID={CosmeticId}",
            accountId, cosmeticId);

        _cosmeticRepo.UnlockCosmetic(accountId, cosmeticId);

        _logger.Information("Cosmetic unlocked successfully");
    }

    #endregion

    #region Loadout Management

    /// <summary>
    /// Get all loadouts for account
    /// </summary>
    public List<CosmeticLoadout> GetLoadouts(int accountId)
    {
        return _cosmeticRepo.GetLoadouts(accountId);
    }

    /// <summary>
    /// Get active loadout for account
    /// </summary>
    public CosmeticLoadout? GetActiveLoadout(int accountId)
    {
        return _cosmeticRepo.GetActiveLoadout(accountId);
    }

    /// <summary>
    /// Create new cosmetic loadout
    /// </summary>
    public int CreateLoadout(CosmeticLoadout loadout)
    {
        _logger.Information("Creating cosmetic loadout: AccountID={AccountId}, Name={Name}",
            loadout.AccountId, loadout.LoadoutName);

        // Validate all cosmetics are unlocked
        ValidateCosmeticsUnlocked(loadout);

        var loadoutId = _cosmeticRepo.CreateLoadout(loadout);

        _logger.Information("Cosmetic loadout created: LoadoutID={LoadoutId}", loadoutId);
        return loadoutId;
    }

    /// <summary>
    /// Update cosmetic loadout
    /// </summary>
    public void UpdateLoadout(CosmeticLoadout loadout)
    {
        _logger.Information("Updating cosmetic loadout: LoadoutID={LoadoutId}", loadout.LoadoutId);

        // Validate all cosmetics are unlocked
        ValidateCosmeticsUnlocked(loadout);

        _cosmeticRepo.UpdateLoadout(loadout);

        _logger.Information("Cosmetic loadout updated");
    }

    /// <summary>
    /// Set active loadout
    /// </summary>
    public void SetActiveLoadout(int accountId, int loadoutId)
    {
        _logger.Information("Setting active loadout: AccountID={AccountId}, LoadoutID={LoadoutId}",
            accountId, loadoutId);

        _cosmeticRepo.SetActiveLoadout(accountId, loadoutId);

        _logger.Information("Active loadout set");
    }

    /// <summary>
    /// Apply cosmetic loadout to character (for display purposes)
    /// </summary>
    public void ApplyLoadoutToCharacter(PlayerCharacter character, CosmeticLoadout loadout)
    {
        _logger.Debug("Applying cosmetic loadout to character: CharacterName={CharacterName}, LoadoutID={LoadoutId}",
            character.Name, loadout.LoadoutId);

        // Future: Apply cosmetic settings to character display
        // This would integrate with UI system to apply:
        // - Title display
        // - Portrait selection
        // - UI theme colors
        // - Ability VFX variants
        // - Combat log formatting

        _logger.Debug("Cosmetic loadout applied to character");
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Validate that all cosmetics in loadout are unlocked
    /// </summary>
    private void ValidateCosmeticsUnlocked(CosmeticLoadout loadout)
    {
        var unlockedCosmetics = _cosmeticRepo.GetUnlockedCosmetics(loadout.AccountId)
            .Select(c => c.CosmeticId)
            .ToHashSet();

        // Check title
        if (!string.IsNullOrEmpty(loadout.SelectedTitle))
        {
            if (!unlockedCosmetics.Contains(loadout.SelectedTitle))
            {
                throw new InvalidOperationException($"Title not unlocked: {loadout.SelectedTitle}");
            }
        }

        // Check portrait
        if (!string.IsNullOrEmpty(loadout.SelectedPortrait))
        {
            if (!unlockedCosmetics.Contains(loadout.SelectedPortrait))
            {
                throw new InvalidOperationException($"Portrait not unlocked: {loadout.SelectedPortrait}");
            }
        }

        // Check UI theme
        if (!string.IsNullOrEmpty(loadout.SelectedUITheme))
        {
            if (!unlockedCosmetics.Contains(loadout.SelectedUITheme))
            {
                throw new InvalidOperationException($"UI theme not unlocked: {loadout.SelectedUITheme}");
            }
        }

        // Check frame
        if (!string.IsNullOrEmpty(loadout.SelectedCharacterFrame))
        {
            if (!unlockedCosmetics.Contains(loadout.SelectedCharacterFrame))
            {
                throw new InvalidOperationException($"Frame not unlocked: {loadout.SelectedCharacterFrame}");
            }
        }

        // Check emblem
        if (!string.IsNullOrEmpty(loadout.SelectedEmblem))
        {
            if (!unlockedCosmetics.Contains(loadout.SelectedEmblem))
            {
                throw new InvalidOperationException($"Emblem not unlocked: {loadout.SelectedEmblem}");
            }
        }

        // Check ability VFX
        foreach (var vfxId in loadout.AbilityVFXOverrides.Values)
        {
            if (!unlockedCosmetics.Contains(vfxId))
            {
                throw new InvalidOperationException($"Ability VFX not unlocked: {vfxId}");
            }
        }

        _logger.Debug("All cosmetics in loadout are unlocked");
    }

    #endregion
}
