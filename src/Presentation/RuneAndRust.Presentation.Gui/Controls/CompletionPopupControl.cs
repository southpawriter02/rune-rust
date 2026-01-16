namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System.Collections.Generic;

/// <summary>
/// Displays tab completion suggestions in a popup list.
/// </summary>
public class CompletionPopupControl : TemplatedControl
{
    /// <summary>
    /// Defines the Suggestions property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<string>> SuggestionsProperty =
        AvaloniaProperty.Register<CompletionPopupControl, IEnumerable<string>>(
            nameof(Suggestions),
            defaultValue: Array.Empty<string>());

    /// <summary>
    /// Defines the SelectedIndex property.
    /// </summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<CompletionPopupControl, int>(
            nameof(SelectedIndex),
            defaultValue: -1);

    /// <summary>
    /// Gets or sets the completion suggestions.
    /// </summary>
    public IEnumerable<string> Suggestions
    {
        get => GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected suggestion index.
    /// </summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// Raised when a suggestion is accepted (clicked or double-clicked).
    /// </summary>
    public event EventHandler<string>? SuggestionAccepted;

    private ListBox? _listBox;

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _listBox = e.NameScope.Find<ListBox>("PART_SuggestionList");

        if (_listBox is not null)
        {
            _listBox.DoubleTapped += OnListBoxDoubleTapped;
        }
    }

    private void OnListBoxDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (_listBox?.SelectedItem is string suggestion)
        {
            SuggestionAccepted?.Invoke(this, suggestion);
        }
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Single click on list item
        if (e.ClickCount == 1 && _listBox?.SelectedItem is string suggestion)
        {
            SuggestionAccepted?.Invoke(this, suggestion);
        }
    }
}
