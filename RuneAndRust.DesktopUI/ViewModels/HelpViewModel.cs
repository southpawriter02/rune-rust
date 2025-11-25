using ReactiveUI;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.43.20: View model for individual help topic display.
/// Wraps TooltipData with selection state for UI.
/// </summary>
public class HelpTopicViewModel : ViewModelBase
{
    private bool _isSelected;

    /// <summary>
    /// The underlying tooltip data.
    /// </summary>
    public TooltipData Data { get; }

    /// <summary>
    /// Tooltip key.
    /// </summary>
    public string Key => Data.Key;

    /// <summary>
    /// Topic title.
    /// </summary>
    public string Title => Data.Title;

    /// <summary>
    /// Brief description.
    /// </summary>
    public string Description => Data.Description;

    /// <summary>
    /// Detailed help text.
    /// </summary>
    public string? DetailedHelp => Data.DetailedHelp;

    /// <summary>
    /// Topic category.
    /// </summary>
    public string Category => Data.Category;

    /// <summary>
    /// Optional icon.
    /// </summary>
    public string? Icon => Data.Icon;

    /// <summary>
    /// Optional keyboard shortcut.
    /// </summary>
    public string? Shortcut => Data.Shortcut;

    /// <summary>
    /// Whether this is a new feature.
    /// </summary>
    public bool IsNew => Data.IsNew;

    /// <summary>
    /// Whether this topic has detailed help.
    /// </summary>
    public bool HasDetailedHelp => !string.IsNullOrEmpty(DetailedHelp);

    /// <summary>
    /// Whether this topic has a shortcut.
    /// </summary>
    public bool HasShortcut => !string.IsNullOrEmpty(Shortcut);

    /// <summary>
    /// Whether this topic has an icon.
    /// </summary>
    public bool HasIcon => !string.IsNullOrEmpty(Icon);

    /// <summary>
    /// Whether this topic is currently selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    /// <summary>
    /// Creates a new HelpTopicViewModel from tooltip data.
    /// </summary>
    public HelpTopicViewModel(TooltipData data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}

/// <summary>
/// v0.43.20: View model for the help browser.
/// Provides searchable help documentation with category filtering.
/// </summary>
public class HelpViewModel : ViewModelBase
{
    private readonly ITooltipService _tooltipService;
    private readonly INavigationService? _navigationService;

    private string _searchQuery = string.Empty;
    private string _selectedCategory = "All";
    private HelpTopicViewModel? _selectedTopic;
    private int _topicCount;

    /// <summary>
    /// Collection of filtered help topics.
    /// </summary>
    public ObservableCollection<HelpTopicViewModel> HelpTopics { get; } = new();

    /// <summary>
    /// Available categories for filtering.
    /// </summary>
    public ObservableCollection<string> Categories { get; } = new();

    /// <summary>
    /// Current search query.
    /// </summary>
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchQuery, value);
            PerformSearch();
        }
    }

    /// <summary>
    /// Currently selected category filter.
    /// </summary>
    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCategory, value);
            PerformSearch();
        }
    }

    /// <summary>
    /// Currently selected help topic.
    /// </summary>
    public HelpTopicViewModel? SelectedTopic
    {
        get => _selectedTopic;
        set
        {
            // Deselect previous
            if (_selectedTopic != null)
            {
                _selectedTopic.IsSelected = false;
            }

            this.RaiseAndSetIfChanged(ref _selectedTopic, value);

            // Select new
            if (_selectedTopic != null)
            {
                _selectedTopic.IsSelected = true;
            }

            this.RaisePropertyChanged(nameof(HasSelectedTopic));
        }
    }

    /// <summary>
    /// Whether a topic is selected.
    /// </summary>
    public bool HasSelectedTopic => SelectedTopic != null;

    /// <summary>
    /// Number of topics currently displayed.
    /// </summary>
    public int TopicCount
    {
        get => _topicCount;
        private set => this.RaiseAndSetIfChanged(ref _topicCount, value);
    }

    /// <summary>
    /// Command to clear search and filters.
    /// </summary>
    public ICommand ClearSearchCommand { get; }

    /// <summary>
    /// Command to go back.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Creates a new instance for design-time support.
    /// </summary>
    public HelpViewModel()
    {
        _tooltipService = null!;

        ClearSearchCommand = ReactiveCommand.Create(() => { });
        BackCommand = ReactiveCommand.Create(() => { });

        LoadDesignTimeData();
    }

    /// <summary>
    /// Creates a new instance with dependency injection.
    /// </summary>
    public HelpViewModel(
        ITooltipService tooltipService,
        INavigationService? navigationService = null)
    {
        _tooltipService = tooltipService ?? throw new ArgumentNullException(nameof(tooltipService));
        _navigationService = navigationService;

        ClearSearchCommand = ReactiveCommand.Create(ClearSearch);
        BackCommand = ReactiveCommand.Create(GoBack);

        LoadCategories();
        LoadAllTopics();

        Console.WriteLine("[HELP] HelpViewModel initialized");
    }

    /// <summary>
    /// Loads all available categories.
    /// </summary>
    private void LoadCategories()
    {
        Categories.Clear();
        Categories.Add("All");

        foreach (var category in _tooltipService.GetCategories())
        {
            Categories.Add(category);
        }
    }

    /// <summary>
    /// Loads all topics without filtering.
    /// </summary>
    private void LoadAllTopics()
    {
        HelpTopics.Clear();

        var topics = _tooltipService.GetAllTooltips();
        foreach (var topic in topics)
        {
            HelpTopics.Add(new HelpTopicViewModel(topic));
        }

        TopicCount = HelpTopics.Count;
    }

    /// <summary>
    /// Performs search and category filtering.
    /// </summary>
    private void PerformSearch()
    {
        HelpTopics.Clear();

        var results = string.IsNullOrWhiteSpace(SearchQuery)
            ? _tooltipService.GetAllTooltips()
            : _tooltipService.SearchTooltips(SearchQuery);

        // Apply category filter
        if (SelectedCategory != "All")
        {
            results = results.Where(t => t.Category == SelectedCategory);
        }

        foreach (var result in results)
        {
            HelpTopics.Add(new HelpTopicViewModel(result));
        }

        TopicCount = HelpTopics.Count;

        // Auto-select first result if we had a selection
        if (SelectedTopic != null && HelpTopics.Count > 0)
        {
            var previousKey = SelectedTopic.Key;
            var matchingTopic = HelpTopics.FirstOrDefault(t => t.Key == previousKey);
            SelectedTopic = matchingTopic ?? HelpTopics.FirstOrDefault();
        }
    }

    /// <summary>
    /// Clears search query and category filter.
    /// </summary>
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        SelectedCategory = "All";
    }

    /// <summary>
    /// Selects a topic by view model.
    /// </summary>
    public void SelectTopic(HelpTopicViewModel? topic)
    {
        SelectedTopic = topic;
    }

    /// <summary>
    /// Opens help for a specific key.
    /// </summary>
    public void ShowHelpFor(string key)
    {
        SearchQuery = string.Empty;
        SelectedCategory = "All";
        PerformSearch();

        var topic = HelpTopics.FirstOrDefault(t => t.Key == key);
        if (topic != null)
        {
            SelectedTopic = topic;
        }
    }

    /// <summary>
    /// Navigates back to the previous view.
    /// </summary>
    private void GoBack()
    {
        _navigationService?.NavigateBack();
    }

    /// <summary>
    /// Loads design-time sample data.
    /// </summary>
    private void LoadDesignTimeData()
    {
        Categories.Add("All");
        Categories.Add("Combat");
        Categories.Add("Stats");
        Categories.Add("Items");

        HelpTopics.Add(new HelpTopicViewModel(new TooltipData
        {
            Key = "combat.attack",
            Title = "Attack",
            Description = "Deal damage to a target enemy within range.",
            DetailedHelp = "Choose an enemy within your weapon's range and attempt to hit them.",
            Category = "Combat",
            Icon = "\u2694",
            Shortcut = "1"
        }));

        HelpTopics.Add(new HelpTopicViewModel(new TooltipData
        {
            Key = "stat.might",
            Title = "MIGHT",
            Description = "Physical power and melee damage.",
            DetailedHelp = "MIGHT increases your base melee damage, carrying capacity.",
            Category = "Stats",
            Icon = "\U0001F4AA"
        }));

        HelpTopics.Add(new HelpTopicViewModel(new TooltipData
        {
            Key = "item.potion",
            Title = "Health Potion",
            Description = "Restores HP when used.",
            Category = "Items",
            Icon = "\U0001F9EA"
        }));

        TopicCount = HelpTopics.Count;
    }
}
