namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia.Controls;
using Avalonia.Threading;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Panel control displaying the message log.
/// </summary>
/// <remarks>
/// Displays a scrollable history of game messages with:
/// - Timestamp for each message
/// - Color coding based on message type
/// - Auto-scroll with toggle option
/// - Category filtering
/// </remarks>
public partial class MessageLogPanel : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageLogPanel"/> class.
    /// </summary>
    public MessageLogPanel()
    {
        InitializeComponent();
        
        // Subscribe to message additions for auto-scroll
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MessageLogViewModel vm)
        {
            vm.Messages.CollectionChanged += OnMessagesChanged;
        }
    }

    private void OnMessagesChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (DataContext is MessageLogViewModel { AutoScrollEnabled: true })
        {
            Dispatcher.UIThread.Post(() =>
            {
                MessageScrollViewer?.ScrollToEnd();
            });
        }
    }
}
