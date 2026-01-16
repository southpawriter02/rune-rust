namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.Services;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for displaying tooltip content.
/// </summary>
public partial class TooltipViewModel : ViewModelBase
{
    private readonly ITooltipService _tooltipService;

    /// <summary>Whether the tooltip is visible.</summary>
    [ObservableProperty] private bool _isVisible;

    /// <summary>The tooltip title.</summary>
    [ObservableProperty] private string _title = "";

    /// <summary>The tooltip subtitle.</summary>
    [ObservableProperty] private string? _subtitle;

    /// <summary>The tooltip description.</summary>
    [ObservableProperty] private string? _description;

    /// <summary>The tooltip footer.</summary>
    [ObservableProperty] private string? _footer;

    /// <summary>Tooltip sections.</summary>
    public ObservableCollection<TooltipSection> Sections { get; } = [];

    /// <summary>Creates a tooltip ViewModel.</summary>
    public TooltipViewModel(ITooltipService tooltipService)
    {
        _tooltipService = tooltipService;
        _tooltipService.OnTooltipChanged += HandleTooltipChanged;
    }

    /// <summary>Creates a tooltip ViewModel with default service.</summary>
    public TooltipViewModel() : this(new TooltipService()) { }

    private void HandleTooltipChanged(TooltipContent? content)
    {
        if (content is null)
        {
            IsVisible = false;
            return;
        }

        Title = content.Title;
        Subtitle = content.Subtitle;
        Description = content.Description;
        Footer = content.Footer;

        Sections.Clear();
        foreach (var section in content.Sections)
            Sections.Add(section);

        IsVisible = true;
    }
}
