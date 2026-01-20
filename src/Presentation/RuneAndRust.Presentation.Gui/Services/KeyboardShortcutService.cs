namespace RuneAndRust.Presentation.Gui.Services;

using Avalonia.Input;
using Serilog;

/// <summary>
/// Implements keyboard shortcut service with context-aware processing.
/// </summary>
public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly Dictionary<string, Action> _actions = new();
    private readonly Dictionary<ShortcutContext, Dictionary<(Key, KeyModifiers), string>> _bindings = new();
    private readonly Dictionary<string, (ShortcutContext Context, Key Key, KeyModifiers Modifiers)> _actionBindings = new();
    private readonly Dictionary<string, string> _actionDisplayNames = new();

    /// <inheritdoc />
    public ShortcutContext CurrentContext { get; set; } = ShortcutContext.Game;

    /// <summary>Creates a new keyboard shortcut service.</summary>
    public KeyboardShortcutService()
    {
        InitializeContexts();
        LoadDefaultBindings();
    }

    private void InitializeContexts()
    {
        foreach (ShortcutContext context in Enum.GetValues<ShortcutContext>())
        {
            _bindings[context] = new Dictionary<(Key, KeyModifiers), string>();
        }
    }

    /// <inheritdoc />
    public bool ProcessKeyDown(Key key, KeyModifiers modifiers)
    {
        // Global bindings always checked first
        if (TryExecuteBinding(ShortcutContext.Global, key, modifiers))
            return true;

        // Then check current context bindings
        if (TryExecuteBinding(CurrentContext, key, modifiers))
            return true;

        return false;
    }

    private bool TryExecuteBinding(ShortcutContext context, Key key, KeyModifiers modifiers)
    {
        if (!_bindings.TryGetValue(context, out var contextBindings))
            return false;

        if (!contextBindings.TryGetValue((key, modifiers), out var actionId))
            return false;

        if (!_actions.TryGetValue(actionId, out var action))
        {
            Log.Warning("Action not registered: {ActionId}", actionId);
            return false;
        }

        Log.Debug("Executing shortcut: {ActionId} ({Key}+{Modifiers}) in context {Context}",
            actionId, key, modifiers, context);

        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing shortcut action: {ActionId}", actionId);
            return false;
        }
    }

    /// <inheritdoc />
    public void RegisterAction(string actionId, Action action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionId);
        ArgumentNullException.ThrowIfNull(action);

        _actions[actionId] = action;
        Log.Debug("Registered action: {ActionId}", actionId);
    }

    /// <inheritdoc />
    public void UnregisterAction(string actionId)
    {
        if (_actions.Remove(actionId))
        {
            Log.Debug("Unregistered action: {ActionId}", actionId);
        }
    }

    /// <inheritdoc />
    public KeyGesture? GetBinding(string actionId)
    {
        return _actionBindings.TryGetValue(actionId, out var binding)
            ? new KeyGesture(binding.Key, binding.Modifiers)
            : null;
    }

    /// <inheritdoc />
    public void SetBinding(string actionId, KeyGesture gesture)
    {
        if (HasConflict(actionId, gesture))
        {
            throw new InvalidOperationException(
                $"Key gesture {gesture} is already bound to another action in this context.");
        }

        // Remove old binding if exists
        if (_actionBindings.TryGetValue(actionId, out var oldBinding))
        {
            _bindings[oldBinding.Context].Remove((oldBinding.Key, oldBinding.Modifiers));
        }

        // Add new binding
        var context = GetContextForAction(actionId);
        _bindings[context][(gesture.Key, gesture.KeyModifiers)] = actionId;
        _actionBindings[actionId] = (context, gesture.Key, gesture.KeyModifiers);

        Log.Information("Binding updated: {ActionId} → {Key}+{Mods}", actionId, gesture.Key, gesture.KeyModifiers);
    }

    /// <inheritdoc />
    public void ResetToDefaults()
    {
        foreach (var context in _bindings.Values)
            context.Clear();

        _actionBindings.Clear();
        LoadDefaultBindings();

        Log.Information("Keybindings reset to defaults");
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, KeyGesture> GetBindingsForContext(ShortcutContext context)
    {
        var result = new Dictionary<string, KeyGesture>();

        if (_bindings.TryGetValue(context, out var contextBindings))
        {
            foreach (var ((key, mods), actionId) in contextBindings)
            {
                result[actionId] = new KeyGesture(key, mods);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public bool HasConflict(string actionId, KeyGesture gesture)
    {
        var context = GetContextForAction(actionId);

        if (!_bindings.TryGetValue(context, out var contextBindings))
            return false;

        if (!contextBindings.TryGetValue((gesture.Key, gesture.KeyModifiers), out var existingAction))
            return false;

        // Not a conflict if it's the same action
        return existingAction != actionId;
    }

    /// <inheritdoc />
    public IReadOnlyList<ShortcutDisplayInfo> GetAllShortcuts()
    {
        var result = new List<ShortcutDisplayInfo>();

        foreach (var (actionId, (context, key, mods)) in _actionBindings)
        {
            var displayName = _actionDisplayNames.GetValueOrDefault(actionId, actionId);
            result.Add(new ShortcutDisplayInfo(actionId, displayName, new KeyGesture(key, mods), context));
        }

        return result.OrderBy(s => s.Context).ThenBy(s => s.DisplayName).ToList();
    }

    private ShortcutContext GetContextForAction(string actionId)
    {
        if (actionId.StartsWith("global-") || IsGlobalAction(actionId))
            return ShortcutContext.Global;
        if (actionId.StartsWith("combat-") || IsCombatAction(actionId))
            return ShortcutContext.Combat;
        if (actionId.StartsWith("dialogue-"))
            return ShortcutContext.Dialogue;

        return ShortcutContext.Game;
    }

    private static bool IsGlobalAction(string actionId) =>
        actionId is "show-help" or "quick-save" or "quick-load" or "close-window";

    private static bool IsCombatAction(string actionId) =>
        actionId is "attack-mode" or "move-mode" or "ability-menu" or "defend"
            or "end-turn" or "next-target" or "previous-target";

    private void LoadDefaultBindings()
    {
        // Global shortcuts
        AddBinding(ShortcutContext.Global, Key.F1, KeyModifiers.None, "show-help", "Help / Shortcuts");
        AddBinding(ShortcutContext.Global, Key.F5, KeyModifiers.None, "quick-save", "Quick Save");
        AddBinding(ShortcutContext.Global, Key.F9, KeyModifiers.None, "quick-load", "Quick Load");
        AddBinding(ShortcutContext.Global, Key.Escape, KeyModifiers.None, "close-window", "Close Window");

        // Game context shortcuts
        AddBinding(ShortcutContext.Game, Key.I, KeyModifiers.None, "toggle-inventory", "Toggle Inventory");
        AddBinding(ShortcutContext.Game, Key.M, KeyModifiers.None, "toggle-map", "Toggle Map");
        AddBinding(ShortcutContext.Game, Key.J, KeyModifiers.None, "toggle-quest-log", "Toggle Quest Log");
        AddBinding(ShortcutContext.Game, Key.C, KeyModifiers.None, "toggle-character", "Toggle Character Sheet");
        AddBinding(ShortcutContext.Game, Key.D1, KeyModifiers.None, "quick-slot-1", "Quick Slot 1");
        AddBinding(ShortcutContext.Game, Key.D2, KeyModifiers.None, "quick-slot-2", "Quick Slot 2");
        AddBinding(ShortcutContext.Game, Key.D3, KeyModifiers.None, "quick-slot-3", "Quick Slot 3");

        // Combat context shortcuts
        AddBinding(ShortcutContext.Combat, Key.A, KeyModifiers.None, "attack-mode", "Attack Mode");
        AddBinding(ShortcutContext.Combat, Key.M, KeyModifiers.None, "move-mode", "Move Mode");
        AddBinding(ShortcutContext.Combat, Key.B, KeyModifiers.None, "ability-menu", "Ability Menu");
        AddBinding(ShortcutContext.Combat, Key.D, KeyModifiers.None, "defend", "Defend");
        AddBinding(ShortcutContext.Combat, Key.Space, KeyModifiers.None, "end-turn", "End Turn");
        AddBinding(ShortcutContext.Combat, Key.Tab, KeyModifiers.None, "next-target", "Next Target");
        AddBinding(ShortcutContext.Combat, Key.Tab, KeyModifiers.Shift, "previous-target", "Previous Target");

        Log.Debug("Default bindings loaded: {Count} bindings", _actionBindings.Count);
    }

    private void AddBinding(ShortcutContext context, Key key, KeyModifiers modifiers,
        string actionId, string displayName)
    {
        _bindings[context][(key, modifiers)] = actionId;
        _actionBindings[actionId] = (context, key, modifiers);
        _actionDisplayNames[actionId] = displayName;
    }
}
