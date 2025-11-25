using ReactiveUI;
using RuneAndRust.Core;
using System;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for the character sheet view.
/// Displays all character attributes, derived stats, trauma meters, and progression.
/// Implements v0.43.9: Character Sheet & Stats Display.
/// </summary>
public class CharacterSheetViewModel : ViewModelBase
{
    private PlayerCharacter? _character;

    /// <summary>
    /// Gets or sets the character to display.
    /// </summary>
    public PlayerCharacter? Character
    {
        get => _character;
        set
        {
            this.RaiseAndSetIfChanged(ref _character, value);
            UpdateAllStats();
        }
    }

    /// <summary>
    /// Gets the view title.
    /// </summary>
    public string Title => "Character Sheet";

    #region Character Identity

    /// <summary>
    /// Gets the character's name.
    /// </summary>
    public string CharacterName => Character?.Name ?? "Unknown";

    /// <summary>
    /// Gets the character's class display name.
    /// </summary>
    public string ClassName => Character?.Class.ToString() ?? "None";

    /// <summary>
    /// Gets the character's specialization display name.
    /// </summary>
    public string SpecializationName => FormatSpecialization(Character?.Specialization ?? Specialization.None);

    /// <summary>
    /// Gets the character's archetype name if set.
    /// </summary>
    public string ArchetypeName => Character?.Archetype?.ArchetypeType.ToString() ?? "None";

    #endregion

    #region Core Attributes

    /// <summary>
    /// Gets the MIGHT attribute value.
    /// </summary>
    public int Might => Character?.Attributes?.Might ?? 0;

    /// <summary>
    /// Gets the FINESSE attribute value.
    /// </summary>
    public int Finesse => Character?.Attributes?.Finesse ?? 0;

    /// <summary>
    /// Gets the WITS attribute value.
    /// </summary>
    public int Wits => Character?.Attributes?.Wits ?? 0;

    /// <summary>
    /// Gets the WILL attribute value.
    /// </summary>
    public int Will => Character?.Attributes?.Will ?? 0;

    /// <summary>
    /// Gets the STURDINESS attribute value.
    /// </summary>
    public int Sturdiness => Character?.Attributes?.Sturdiness ?? 0;

    #endregion

    #region Resource Pools

    /// <summary>
    /// Gets the current HP.
    /// </summary>
    public int CurrentHP => Character?.HP ?? 0;

    /// <summary>
    /// Gets the maximum HP.
    /// </summary>
    public int MaxHP => Character?.MaxHP ?? 1;

    /// <summary>
    /// Gets the HP percentage for progress bar.
    /// </summary>
    public double HPPercent => MaxHP > 0 ? (double)CurrentHP / MaxHP : 0;

    /// <summary>
    /// Gets the current Stamina.
    /// </summary>
    public int CurrentStamina => Character?.Stamina ?? 0;

    /// <summary>
    /// Gets the maximum Stamina.
    /// </summary>
    public int MaxStamina => Character?.MaxStamina ?? 1;

    /// <summary>
    /// Gets the Stamina percentage for progress bar.
    /// </summary>
    public double StaminaPercent => MaxStamina > 0 ? (double)CurrentStamina / MaxStamina : 0;

    /// <summary>
    /// Gets the current Aether Pool (Mystic resource).
    /// </summary>
    public int CurrentAP => Character?.AP ?? 0;

    /// <summary>
    /// Gets the maximum Aether Pool.
    /// </summary>
    public int MaxAP => Character?.MaxAP ?? 0;

    /// <summary>
    /// Gets whether the character has Aether Pool (is a Mystic).
    /// </summary>
    public bool HasAetherPool => MaxAP > 0;

    /// <summary>
    /// Gets the current Savagery (Skar-Horde resource).
    /// </summary>
    public int Savagery => Character?.Savagery ?? 0;

    /// <summary>
    /// Gets the maximum Savagery.
    /// </summary>
    public int MaxSavagery => Character?.MaxSavagery ?? 100;

    /// <summary>
    /// Gets whether the character has Savagery resource.
    /// </summary>
    public bool HasSavagery => Character?.Specialization == Specialization.SkarHordeAspirant;

    /// <summary>
    /// Gets the current Righteous Fervor (Iron-Bane resource).
    /// </summary>
    public int RighteousFervor => Character?.RighteousFervor ?? 0;

    /// <summary>
    /// Gets the maximum Righteous Fervor.
    /// </summary>
    public int MaxRighteousFervor => Character?.MaxRighteousFervor ?? 100;

    /// <summary>
    /// Gets whether the character has Righteous Fervor resource.
    /// </summary>
    public bool HasRighteousFervor => Character?.Specialization == Specialization.IronBane;

    /// <summary>
    /// Gets the current Momentum (Strandhogg resource).
    /// </summary>
    public int Momentum => Character?.Momentum ?? 0;

    /// <summary>
    /// Gets the maximum Momentum.
    /// </summary>
    public int MaxMomentum => Character?.MaxMomentum ?? 100;

    /// <summary>
    /// Gets whether the character has Momentum resource.
    /// </summary>
    public bool HasMomentum => Momentum > 0 || Character?.Specialization.ToString().Contains("Strandhogg") == true;

    #endregion

    #region Derived Combat Stats

    /// <summary>
    /// Gets the Speed stat (movement and initiative).
    /// Derived from FINESSE and WITS.
    /// </summary>
    public int Speed => 5 + GetModifier(Finesse) + (GetModifier(Wits) / 2);

    /// <summary>
    /// Gets the Accuracy stat (attack hit chance).
    /// Derived from FINESSE for ranged, MIGHT for melee.
    /// </summary>
    public int Accuracy => Math.Max(GetModifier(Finesse), GetModifier(Might));

    /// <summary>
    /// Gets the Evasion stat (dodge chance).
    /// Derived from FINESSE.
    /// </summary>
    public int Evasion => 10 + GetModifier(Finesse);

    /// <summary>
    /// Gets the Critical Chance percentage.
    /// Derived from WITS.
    /// </summary>
    public int CritChance => 5 + GetModifier(Wits);

    /// <summary>
    /// Gets the Physical Defense stat.
    /// Derived from STURDINESS and equipped armor.
    /// </summary>
    public int PhysicalDefense => 10 + GetModifier(Sturdiness) + GetArmorBonus();

    /// <summary>
    /// Gets the Metaphysical Defense stat.
    /// Derived from WILL.
    /// </summary>
    public int MetaphysicalDefense => 10 + GetModifier(Will);

    /// <summary>
    /// Gets the Attack Power stat.
    /// Derived from MIGHT.
    /// </summary>
    public int AttackPower => Character?.BaseDamage ?? 1 + GetModifier(Might);

    /// <summary>
    /// Gets the Initiative bonus.
    /// Derived from WITS and FINESSE.
    /// </summary>
    public int Initiative => GetModifier(Wits) + (GetModifier(Finesse) / 2);

    #endregion

    #region Trauma Meters

    /// <summary>
    /// Gets the current Psychic Stress level.
    /// </summary>
    public int PsychicStress => Character?.PsychicStress ?? 0;

    /// <summary>
    /// Gets the maximum Psychic Stress before breaking.
    /// </summary>
    public int MaxPsychicStress => 100;

    /// <summary>
    /// Gets the Psychic Stress percentage for progress bar.
    /// </summary>
    public double PsychicStressPercent => (double)PsychicStress / MaxPsychicStress;

    /// <summary>
    /// Gets the Psychic Stress warning level (yellow > 50, red > 75).
    /// </summary>
    public string PsychicStressLevel => PsychicStress switch
    {
        >= 75 => "Critical",
        >= 50 => "High",
        >= 25 => "Moderate",
        _ => "Low"
    };

    /// <summary>
    /// Gets the current Corruption level.
    /// </summary>
    public int Corruption => Character?.Corruption ?? 0;

    /// <summary>
    /// Gets the maximum Corruption before permanent consequences.
    /// </summary>
    public int MaxCorruption => 100;

    /// <summary>
    /// Gets the Corruption percentage for progress bar.
    /// </summary>
    public double CorruptionPercent => (double)Corruption / MaxCorruption;

    /// <summary>
    /// Gets the Corruption warning level.
    /// </summary>
    public string CorruptionLevel => Corruption switch
    {
        >= 75 => "Critical",
        >= 50 => "High",
        >= 25 => "Moderate",
        _ => "Low"
    };

    /// <summary>
    /// Gets the number of permanent traumas.
    /// </summary>
    public int TraumaCount => Character?.Traumas?.Count ?? 0;

    /// <summary>
    /// Gets whether the character has any traumas.
    /// </summary>
    public bool HasTraumas => TraumaCount > 0;

    #endregion

    #region Progression

    /// <summary>
    /// Gets the current Legend level.
    /// </summary>
    public int Legend => Character?.CurrentLegend ?? 0;

    /// <summary>
    /// Gets the current Milestone level.
    /// </summary>
    public int Milestone => Character?.CurrentMilestone ?? 0;

    /// <summary>
    /// Gets the current XP (Legend points toward next milestone).
    /// </summary>
    public int CurrentXP => Character?.CurrentLegend ?? 0;

    /// <summary>
    /// Gets the XP required for the next milestone.
    /// </summary>
    public int XPToNextLevel => Character?.LegendToNextMilestone ?? 100;

    /// <summary>
    /// Gets the XP progress as a percentage.
    /// </summary>
    public double XPProgress => XPToNextLevel > 0 ? (double)CurrentXP / XPToNextLevel : 0;

    /// <summary>
    /// Gets the available Progression Points.
    /// </summary>
    public int ProgressionPoints => Character?.ProgressionPoints ?? 0;

    /// <summary>
    /// Gets the current currency (Dvergr Cogs).
    /// </summary>
    public int Currency => Character?.Currency ?? 0;

    #endregion

    #region Stance System

    /// <summary>
    /// Gets the current stance name.
    /// </summary>
    public string CurrentStanceName => FormatStanceName(Character?.ActiveStance?.Type);

    /// <summary>
    /// Gets the current stance description.
    /// </summary>
    public string CurrentStanceDescription => GetStanceDescription(Character?.ActiveStance);

    /// <summary>
    /// Gets the number of stance shifts remaining this turn.
    /// </summary>
    public int StanceShiftsRemaining => Character?.StanceShiftsRemaining ?? 0;

    #endregion

    #region Equipment Summary

    /// <summary>
    /// Gets the equipped weapon name.
    /// </summary>
    public string EquippedWeaponName => Character?.EquippedWeapon?.Name ?? Character?.WeaponName ?? "Unarmed";

    /// <summary>
    /// Gets the equipped armor name.
    /// </summary>
    public string EquippedArmorName => Character?.EquippedArmor?.Name ?? "Unarmored";

    /// <summary>
    /// Gets the number of abilities the character has.
    /// </summary>
    public int AbilityCount => Character?.Abilities?.Count ?? 0;

    /// <summary>
    /// Gets the inventory item count.
    /// </summary>
    public int InventoryCount => Character?.Inventory?.Count ?? 0;

    /// <summary>
    /// Gets the maximum inventory size.
    /// </summary>
    public int MaxInventorySize => Character?.MaxInventorySize ?? 5;

    #endregion

    /// <summary>
    /// Initializes a new instance of CharacterSheetViewModel.
    /// Creates a demo character for testing.
    /// </summary>
    public CharacterSheetViewModel()
    {
        // Initialize with a demo character for testing/development
        Character = CreateDemoCharacter();
    }

    /// <summary>
    /// Creates a demo character for testing the character sheet.
    /// </summary>
    private static PlayerCharacter CreateDemoCharacter()
    {
        return new PlayerCharacter
        {
            Name = "Ragnar the Bold",
            Class = CharacterClass.Warrior,
            Specialization = Specialization.SkarHordeAspirant,
            Attributes = new Attributes(14, 12, 10, 8, 16),
            HP = 45,
            MaxHP = 60,
            Stamina = 30,
            MaxStamina = 40,
            Savagery = 35,
            MaxSavagery = 100,
            PsychicStress = 28,
            Corruption = 12,
            CurrentLegend = 75,
            CurrentMilestone = 3,
            LegendToNextMilestone = 100,
            ProgressionPoints = 5,
            Currency = 250,
            BaseDamage = 2,
            WeaponName = "Rusted Greataxe",
            ActiveStance = Stance.CreateAggressiveStance()
        };
    }

    /// <summary>
    /// Gets the attribute modifier (D&D-style: (score - 10) / 2).
    /// </summary>
    private static int GetModifier(int attributeValue)
    {
        return (attributeValue - 10) / 2;
    }

    /// <summary>
    /// Gets the armor bonus from equipped armor.
    /// </summary>
    private int GetArmorBonus()
    {
        // TODO: Get actual armor bonus from equipment system
        return Character?.EquippedArmor != null ? 2 : 0;
    }

    /// <summary>
    /// Formats a specialization enum value for display.
    /// </summary>
    private static string FormatSpecialization(Specialization spec)
    {
        if (spec == Specialization.None)
            return "None";

        // Convert PascalCase to spaced words (e.g., SkarHordeAspirant -> Skar-Horde Aspirant)
        var name = spec.ToString();

        // Handle known specializations with proper formatting
        return spec switch
        {
            Specialization.SkarHordeAspirant => "Skar-Horde Aspirant",
            Specialization.IronBane => "Iron-Bane",
            Specialization.AtgeirWielder => "Atgeir Wielder",
            Specialization.BoneSetter => "Bone-Setter",
            Specialization.ScrapTinker => "Scrap Tinker",
            Specialization.JotunReader => "Jötun-Reader",
            Specialization.VardWarden => "Vard-Warden",
            Specialization.RustWitch => "Rust-Witch",
            _ => name
        };
    }

    /// <summary>
    /// Formats a stance type for display.
    /// </summary>
    private static string FormatStanceName(StanceType? stanceType)
    {
        return stanceType switch
        {
            StanceType.Calculated => "Calculated",
            StanceType.Aggressive => "Aggressive",
            StanceType.Defensive => "Defensive",
            StanceType.Evasive => "Evasive",
            _ => "Balanced"
        };
    }

    /// <summary>
    /// Gets a description for the current stance.
    /// </summary>
    private static string GetStanceDescription(Stance? stance)
    {
        if (stance == null)
            return "No stance modifiers active.";

        return stance.Type switch
        {
            StanceType.Aggressive => $"+{stance.FlatDamageBonus} damage, {stance.DefenseModifier} Defense, {stance.WillModifier} WILL checks",
            StanceType.Defensive => $"+{stance.SoakBonus} Soak, +{stance.WillModifier} WILL checks, {(int)((1 - stance.DamageMultiplier) * 100)}% damage penalty",
            StanceType.Evasive => $"+{stance.DefenseBonus} Defense, {(int)((1 - stance.DamageMultiplier) * 100)}% damage penalty",
            _ => "No stance modifiers active."
        };
    }

    /// <summary>
    /// Updates all stat property notifications.
    /// Called when the Character property changes.
    /// </summary>
    private void UpdateAllStats()
    {
        // Identity
        this.RaisePropertyChanged(nameof(CharacterName));
        this.RaisePropertyChanged(nameof(ClassName));
        this.RaisePropertyChanged(nameof(SpecializationName));
        this.RaisePropertyChanged(nameof(ArchetypeName));

        // Core Attributes
        this.RaisePropertyChanged(nameof(Might));
        this.RaisePropertyChanged(nameof(Finesse));
        this.RaisePropertyChanged(nameof(Wits));
        this.RaisePropertyChanged(nameof(Will));
        this.RaisePropertyChanged(nameof(Sturdiness));

        // Resources
        this.RaisePropertyChanged(nameof(CurrentHP));
        this.RaisePropertyChanged(nameof(MaxHP));
        this.RaisePropertyChanged(nameof(HPPercent));
        this.RaisePropertyChanged(nameof(CurrentStamina));
        this.RaisePropertyChanged(nameof(MaxStamina));
        this.RaisePropertyChanged(nameof(StaminaPercent));
        this.RaisePropertyChanged(nameof(CurrentAP));
        this.RaisePropertyChanged(nameof(MaxAP));
        this.RaisePropertyChanged(nameof(HasAetherPool));
        this.RaisePropertyChanged(nameof(Savagery));
        this.RaisePropertyChanged(nameof(MaxSavagery));
        this.RaisePropertyChanged(nameof(HasSavagery));
        this.RaisePropertyChanged(nameof(RighteousFervor));
        this.RaisePropertyChanged(nameof(MaxRighteousFervor));
        this.RaisePropertyChanged(nameof(HasRighteousFervor));
        this.RaisePropertyChanged(nameof(Momentum));
        this.RaisePropertyChanged(nameof(MaxMomentum));
        this.RaisePropertyChanged(nameof(HasMomentum));

        // Derived Stats
        this.RaisePropertyChanged(nameof(Speed));
        this.RaisePropertyChanged(nameof(Accuracy));
        this.RaisePropertyChanged(nameof(Evasion));
        this.RaisePropertyChanged(nameof(CritChance));
        this.RaisePropertyChanged(nameof(PhysicalDefense));
        this.RaisePropertyChanged(nameof(MetaphysicalDefense));
        this.RaisePropertyChanged(nameof(AttackPower));
        this.RaisePropertyChanged(nameof(Initiative));

        // Trauma
        this.RaisePropertyChanged(nameof(PsychicStress));
        this.RaisePropertyChanged(nameof(PsychicStressPercent));
        this.RaisePropertyChanged(nameof(PsychicStressLevel));
        this.RaisePropertyChanged(nameof(Corruption));
        this.RaisePropertyChanged(nameof(CorruptionPercent));
        this.RaisePropertyChanged(nameof(CorruptionLevel));
        this.RaisePropertyChanged(nameof(TraumaCount));
        this.RaisePropertyChanged(nameof(HasTraumas));

        // Progression
        this.RaisePropertyChanged(nameof(Legend));
        this.RaisePropertyChanged(nameof(Milestone));
        this.RaisePropertyChanged(nameof(CurrentXP));
        this.RaisePropertyChanged(nameof(XPToNextLevel));
        this.RaisePropertyChanged(nameof(XPProgress));
        this.RaisePropertyChanged(nameof(ProgressionPoints));
        this.RaisePropertyChanged(nameof(Currency));

        // Stance
        this.RaisePropertyChanged(nameof(CurrentStanceName));
        this.RaisePropertyChanged(nameof(CurrentStanceDescription));
        this.RaisePropertyChanged(nameof(StanceShiftsRemaining));

        // Equipment
        this.RaisePropertyChanged(nameof(EquippedWeaponName));
        this.RaisePropertyChanged(nameof(EquippedArmorName));
        this.RaisePropertyChanged(nameof(AbilityCount));
        this.RaisePropertyChanged(nameof(InventoryCount));
        this.RaisePropertyChanged(nameof(MaxInventorySize));
    }
}
