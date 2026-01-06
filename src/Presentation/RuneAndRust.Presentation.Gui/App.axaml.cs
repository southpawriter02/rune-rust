using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RuneAndRust.Presentation.Gui.Views;

namespace RuneAndRust.Presentation.Gui;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
