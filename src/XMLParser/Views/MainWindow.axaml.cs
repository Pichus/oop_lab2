using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
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
}