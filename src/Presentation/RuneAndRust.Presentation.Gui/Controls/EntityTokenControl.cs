namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using RuneAndRust.Domain.Entities;

/// <summary>
/// Displays an entity token on the combat grid.
/// </summary>
public class EntityTokenControl : TemplatedControl
{
    /// <summary>
    /// Defines the Entity property.
    /// </summary>
    public static readonly StyledProperty<Combatant?> EntityProperty =
        AvaloniaProperty.Register<EntityTokenControl, Combatant?>(nameof(Entity));

    /// <summary>
    /// Defines the IsSelected property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<EntityTokenControl, bool>(nameof(IsSelected));

    /// <summary>
    /// Defines the IsCurrentTurn property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCurrentTurnProperty =
        AvaloniaProperty.Register<EntityTokenControl, bool>(nameof(IsCurrentTurn));

    /// <summary>
    /// Defines the ShowGlowEffect property for turn start animation.
    /// </summary>
    public static readonly StyledProperty<bool> ShowGlowEffectProperty =
        AvaloniaProperty.Register<EntityTokenControl, bool>(nameof(ShowGlowEffect));

    /// <summary>
    /// Defines the GlowOpacity property for pulse animation.
    /// </summary>
    public static readonly StyledProperty<double> GlowOpacityProperty =
        AvaloniaProperty.Register<EntityTokenControl, double>(nameof(GlowOpacity), 1.0);

    /// <summary>
    /// Gets or sets the entity this token represents.
    /// </summary>
    public Combatant? Entity
    {
        get => GetValue(EntityProperty);
        set => SetValue(EntityProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this token is selected.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Gets or sets whether it's this entity's turn.
    /// </summary>
    public bool IsCurrentTurn
    {
        get => GetValue(IsCurrentTurnProperty);
        set => SetValue(IsCurrentTurnProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the glow effect is visible.
    /// </summary>
    /// <remarks>Used for turn start highlight animation.</remarks>
    public bool ShowGlowEffect
    {
        get => GetValue(ShowGlowEffectProperty);
        set => SetValue(ShowGlowEffectProperty, value);
    }

    /// <summary>
    /// Gets or sets the glow effect opacity for pulse animation.
    /// </summary>
    /// <value>Value between 0.0 and 1.0.</value>
    public double GlowOpacity
    {
        get => GetValue(GlowOpacityProperty);
        set => SetValue(GlowOpacityProperty, value);
    }

    /// <summary>
    /// Gets the symbol for this entity.
    /// </summary>
    public string TokenSymbol => Entity?.IsPlayer == true
        ? "@"
        : Entity?.DisplayName.FirstOrDefault().ToString() ?? "?";

    /// <summary>
    /// Gets the color for this entity's faction.
    /// </summary>
    public IBrush TokenColor => Entity?.IsPlayer == true
        ? Brushes.LimeGreen
        : Entity?.IsMonster == true
            ? Brushes.OrangeRed
            : Brushes.White;

    /// <summary>
    /// Gets the tooltip text for this entity.
    /// </summary>
    public string Tooltip => Entity is not null
        ? $"{Entity.DisplayName}\nHP: {Entity.CurrentHealth}/{Entity.MaxHealth}"
        : "";

    static EntityTokenControl()
    {
        EntityProperty.Changed.AddClassHandler<EntityTokenControl>((c, _) => c.OnEntityChanged());
        IsCurrentTurnProperty.Changed.AddClassHandler<EntityTokenControl>((c, _) => c.OnTurnChanged());
        ShowGlowEffectProperty.Changed.AddClassHandler<EntityTokenControl>((c, _) => c.OnGlowChanged());
    }

    private void OnEntityChanged()
    {
        Classes.Set("player", Entity?.IsPlayer == true);
        Classes.Set("enemy", Entity?.IsMonster == true);
    }

    private void OnTurnChanged()
    {
        Classes.Set("current-turn", IsCurrentTurn);
    }

    private void OnGlowChanged()
    {
        Classes.Set("glow-active", ShowGlowEffect);
    }
}
