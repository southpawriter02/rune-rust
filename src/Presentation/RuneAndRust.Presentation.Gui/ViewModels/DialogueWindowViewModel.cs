namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Dialogue Window.
/// </summary>
public partial class DialogueWindowViewModel : ViewModelBase
{
    private readonly Action? _closeAction;
    private readonly Dictionary<string, DialogueNode> _nodes = [];
    private DialogueNode? _currentNode;
    private CancellationTokenSource? _typingCts;

    /// <summary>Gets the NPC name.</summary>
    [ObservableProperty] private string _npcName = "Unknown";

    /// <summary>Gets the NPC portrait icon.</summary>
    [ObservableProperty] private string _npcPortrait = "☺";

    /// <summary>Gets the displayed text (animated).</summary>
    [ObservableProperty] private string _displayedText = "";

    /// <summary>Gets the full text.</summary>
    [ObservableProperty] private string _fullText = "";

    /// <summary>Gets whether typing animation is active.</summary>
    [ObservableProperty] private bool _isTyping;

    /// <summary>Gets whether choices are available.</summary>
    [ObservableProperty] private bool _hasChoices;

    /// <summary>Gets the typing speed in milliseconds per character.</summary>
    [ObservableProperty] private int _typingSpeedMs = 30;

    /// <summary>Dialogue choices.</summary>
    public ObservableCollection<DialogueChoiceViewModel> Choices { get; } = [];

    /// <summary>Gets the continue hint text.</summary>
    public string ContinueHint => IsTyping ? "[Click to skip...]" : "[Click to continue...]";

    /// <summary>Whether to show the continue hint.</summary>
    public bool ShowContinueHint => IsTyping || !HasChoices;

    /// <summary>Creates a dialogue window ViewModel.</summary>
    public DialogueWindowViewModel(Action? closeAction = null)
    {
        _closeAction = closeAction;
        LoadSampleDialogue();
    }

    /// <summary>Starts dialogue with an NPC.</summary>
    public async Task StartDialogueAsync(string npcName, string portrait = "☺")
    {
        NpcName = npcName;
        NpcPortrait = portrait;

        if (_nodes.TryGetValue("start", out var startNode))
        {
            await ShowNodeAsync(startNode);
        }

        Log.Information("Started dialogue with {NpcName}", npcName);
    }

    /// <summary>Shows a dialogue node.</summary>
    private async Task ShowNodeAsync(DialogueNode node)
    {
        _currentNode = node;
        FullText = node.Text;
        DisplayedText = "";
        Choices.Clear();
        HasChoices = false;

        await TypeTextAsync(node.Text);

        ShowChoices();
    }

    /// <summary>Types text character by character.</summary>
    private async Task TypeTextAsync(string text)
    {
        IsTyping = true;
        OnPropertyChanged(nameof(ContinueHint));
        OnPropertyChanged(nameof(ShowContinueHint));

        _typingCts = new CancellationTokenSource();

        try
        {
            for (int i = 0; i <= text.Length; i++)
            {
                DisplayedText = text[..i];
                await Task.Delay(TypingSpeedMs, _typingCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            DisplayedText = FullText;
        }

        IsTyping = false;
        OnPropertyChanged(nameof(ContinueHint));
        OnPropertyChanged(nameof(ShowContinueHint));
    }

    /// <summary>Shows available choices for the current node.</summary>
    private void ShowChoices()
    {
        if (_currentNode is null) return;

        Choices.Clear();
        int index = 1;
        foreach (var choice in _currentNode.Choices)
        {
            Choices.Add(new DialogueChoiceViewModel(index++, choice));
        }

        HasChoices = Choices.Count > 0;
        OnPropertyChanged(nameof(ShowContinueHint));
    }

    /// <summary>Handles click on dialogue text.</summary>
    [RelayCommand]
    private async Task ClickText()
    {
        if (IsTyping)
        {
            _typingCts?.Cancel();
            DisplayedText = FullText;
            IsTyping = false;
            ShowChoices();
            Log.Debug("Skipped typing animation");
        }
        else if (!HasChoices && _currentNode?.AutoAdvanceNodeId is not null)
        {
            if (_nodes.TryGetValue(_currentNode.AutoAdvanceNodeId, out var nextNode))
            {
                await ShowNodeAsync(nextNode);
            }
        }
        else if (!HasChoices)
        {
            _closeAction?.Invoke();
        }
    }

    /// <summary>Selects a dialogue choice.</summary>
    [RelayCommand]
    private async Task SelectChoice(DialogueChoiceViewModel choice)
    {
        Log.Information("Selected choice {Index}: {Text}", choice.Index, choice.Text);

        // Handle action
        switch (choice.Action)
        {
            case DialogueAction.AcceptQuest:
                Log.Information("Quest accepted: {QuestId}", choice.ActionData);
                break;
            case DialogueAction.OpenShop:
                Log.Information("Opening shop: {ShopId}", choice.ActionData);
                break;
            case DialogueAction.Leave:
                _closeAction?.Invoke();
                return;
        }

        // Navigate to next node
        if (choice.NextNodeId is not null && _nodes.TryGetValue(choice.NextNodeId, out var nextNode))
        {
            await ShowNodeAsync(nextNode);
        }
        else
        {
            _closeAction?.Invoke();
        }
    }

    /// <summary>Handles key press.</summary>
    public async Task HandleKeyPress(int keyNumber)
    {
        if (keyNumber >= 1 && keyNumber <= 9)
        {
            var choice = Choices.FirstOrDefault(c => c.Index == keyNumber);
            if (choice is not null)
            {
                await SelectChoice(choice);
            }
        }
    }

    private void LoadSampleDialogue()
    {
        _nodes["start"] = new DialogueNode(
            "start",
            "Greetings, traveler! I am Merchant Marcus. Welcome to my humble shop. What brings you to these parts?",
            [
                new DialogueChoiceData("I'm looking for supplies.", "supplies"),
                new DialogueChoiceData("Just passing through.", "passing"),
                new DialogueChoiceData("What quests do you have?", "quests"),
                new DialogueChoiceData("Goodbye.", Action: DialogueAction.Leave)
            ]);

        _nodes["supplies"] = new DialogueNode(
            "supplies",
            "Ah, you've come to the right place! I have the finest goods this side of the mountains. Take a look at my wares!",
            [
                new DialogueChoiceData("Let me see your goods.", Action: DialogueAction.OpenShop, ActionData: "marcus-shop"),
                new DialogueChoiceData("Maybe later.", "goodbye")
            ]);

        _nodes["passing"] = new DialogueNode(
            "passing",
            "Well, safe travels then! But do stop by again if you need anything. These roads can be treacherous.",
            [],
            AutoAdvanceNodeId: "goodbye");

        _nodes["quests"] = new DialogueNode(
            "quests",
            "Quests, you say? Well, there is one matter... Rats have been eating my stock. Clear them out and I'll reward you handsomely!",
            [
                new DialogueChoiceData("I'll help you.", "quest_accept", DialogueAction.AcceptQuest, "rat_problem"),
                new DialogueChoiceData("That's not my problem.", "quest_decline", DialogueAction.DeclineQuest)
            ]);

        _nodes["quest_accept"] = new DialogueNode(
            "quest_accept",
            "Excellent! The rats are in my basement. Be careful, some of them are quite large. Return when you've dealt with them.",
            [new DialogueChoiceData("I'll get right on it.", Action: DialogueAction.Leave)]);

        _nodes["quest_decline"] = new DialogueNode(
            "quest_decline",
            "I understand, adventurer. Not everyone wants to deal with vermin. Let me know if you change your mind.",
            [new DialogueChoiceData("Goodbye.", Action: DialogueAction.Leave)]);

        _nodes["goodbye"] = new DialogueNode(
            "goodbye",
            "Farewell, traveler! May your journey be prosperous!",
            []);

        Log.Information("Loaded sample dialogue with {NodeCount} nodes", _nodes.Count);
    }
}
