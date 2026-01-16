namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Quest Log Window displaying active and completed quests.
/// </summary>
public partial class QuestLogWindow : Window
{
    /// <summary>Creates a new quest log window.</summary>
    public QuestLogWindow()
    {
        InitializeComponent();
        DataContext = new QuestLogWindowViewModel(Close);
    }

    private void OnQuestEntryPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border &&
            DataContext is QuestLogWindowViewModel vm &&
            border.DataContext is QuestEntryViewModel quest)
        {
            vm.SelectQuestCommand.Execute(quest);
        }
    }
}
