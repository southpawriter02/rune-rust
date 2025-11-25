using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.21: Full implementation of dialog service with Avalonia window dialogs.
/// Provides confirmation dialogs, message boxes, and custom dialog support.
/// </summary>
public class DialogService : IDialogService
{
    /// <inheritdoc/>
    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var result = false;
        var window = CreateDialogWindow(title, 400, 200);

        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20
        };

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };

        var yesButton = new Button
        {
            Content = "Yes",
            MinWidth = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        yesButton.Click += (s, e) =>
        {
            result = true;
            window.Close();
        };

        var noButton = new Button
        {
            Content = "No",
            MinWidth = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        noButton.Click += (s, e) =>
        {
            result = false;
            window.Close();
        };

        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);
        panel.Children.Add(buttonPanel);

        window.Content = panel;

        await ShowDialogWindowAsync(window);
        return result;
    }

    /// <inheritdoc/>
    public async Task ShowMessageAsync(string title, string message)
    {
        var window = CreateDialogWindow(title, 400, 180);

        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20
        };

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        });

        var okButton = new Button
        {
            Content = "OK",
            MinWidth = 80,
            HorizontalAlignment = HorizontalAlignment.Right,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        okButton.Click += (s, e) => window.Close();

        panel.Children.Add(okButton);
        window.Content = panel;

        await ShowDialogWindowAsync(window);
    }

    /// <inheritdoc/>
    public async Task<T?> ShowDialogAsync<T>(object viewModel)
    {
        // Create a window to host the custom ViewModel
        var window = new Window
        {
            Title = "Dialog",
            Width = 500,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            DataContext = viewModel,
            CanResize = false
        };

        // For custom dialogs, the content is resolved through DataTemplates
        window.Content = new ContentControl
        {
            Content = viewModel
        };

        await ShowDialogWindowAsync(window);

        // Return the result if the ViewModel implements a result pattern
        if (viewModel is IDialogResult<T> dialogResult)
        {
            return dialogResult.Result;
        }

        return default;
    }

    /// <summary>
    /// Shows an error dialog with a red accent.
    /// </summary>
    public async Task ShowErrorAsync(string title, string message)
    {
        var window = CreateDialogWindow(title, 450, 200);

        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        headerPanel.Children.Add(new TextBlock
        {
            Text = "⚠",
            FontSize = 24,
            Foreground = Brushes.Red,
            VerticalAlignment = VerticalAlignment.Center
        });

        headerPanel.Children.Add(new TextBlock
        {
            Text = "Error",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.Red,
            VerticalAlignment = VerticalAlignment.Center
        });

        panel.Children.Add(headerPanel);

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        });

        var okButton = new Button
        {
            Content = "OK",
            MinWidth = 80,
            HorizontalAlignment = HorizontalAlignment.Right,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        okButton.Click += (s, e) => window.Close();

        panel.Children.Add(okButton);
        window.Content = panel;

        await ShowDialogWindowAsync(window);
    }

    /// <summary>
    /// Shows an input dialog and returns the user's input.
    /// </summary>
    public async Task<string?> ShowInputAsync(string title, string prompt, string defaultValue = "")
    {
        string? result = null;
        var window = CreateDialogWindow(title, 400, 180);

        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };

        panel.Children.Add(new TextBlock
        {
            Text = prompt,
            FontSize = 14
        });

        var inputBox = new TextBox
        {
            Text = defaultValue,
            MinHeight = 30
        };
        panel.Children.Add(inputBox);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };

        var okButton = new Button
        {
            Content = "OK",
            MinWidth = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        okButton.Click += (s, e) =>
        {
            result = inputBox.Text;
            window.Close();
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            MinWidth = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        cancelButton.Click += (s, e) =>
        {
            result = null;
            window.Close();
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        panel.Children.Add(buttonPanel);

        window.Content = panel;

        await ShowDialogWindowAsync(window);
        return result;
    }

    /// <summary>
    /// Creates a dialog window with standard styling.
    /// </summary>
    private static Window CreateDialogWindow(string title, double width, double height)
    {
        return new Window
        {
            Title = title,
            Width = width,
            Height = height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            SystemDecorations = SystemDecorations.BorderOnly
        };
    }

    /// <summary>
    /// Shows a dialog window modally.
    /// </summary>
    private static async Task ShowDialogWindowAsync(Window window)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = desktop.MainWindow;
            if (mainWindow != null)
            {
                await window.ShowDialog(mainWindow);
            }
            else
            {
                window.Show();
            }
        }
        else
        {
            window.Show();
        }
    }
}

/// <summary>
/// Interface for ViewModels that return a dialog result.
/// </summary>
public interface IDialogResult<T>
{
    /// <summary>
    /// Gets the dialog result.
    /// </summary>
    T? Result { get; }
}
