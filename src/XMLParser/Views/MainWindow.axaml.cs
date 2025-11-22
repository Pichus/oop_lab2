using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using XMLParser.ViewModels;

namespace XMLParser.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            ViewModel?.ConfirmExit.RegisterHandler(async interaction =>
            {
                var result = await ShowExitDialog(interaction.Input);
                interaction.SetOutput(result);
            });
        });
        Closing += OnClosing;
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            e.Cancel = true;

            var ok = await vm.ConfirmExit.Handle("Чи дійсно ви хочете завершити роботу з програмою?");
            if (ok)
            {
                Closing -= OnClosing;
                Close();
            }
        }
    }

    private async Task<bool> ShowExitDialog(string message)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("Exit", message,
                ButtonEnum.YesNo);

        var result = await box.ShowWindowDialogAsync(this);
        return result == ButtonResult.Yes;
    }

    private async void OpenLinkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (GetTopLevel(this) is TopLevel topLevel && ViewModel?.LastHtmlUrl != null)
        {
            var uri = new Uri(ViewModel.LastHtmlUrl);

            var launched = await topLevel.Launcher.LaunchUriAsync(uri);

            if (!launched)
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Could not launch URI.", "Something went wrong, try again");

                await box.ShowAsync();
            }
        }
    }
}