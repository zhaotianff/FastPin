using System.Windows;
using FastPin.ViewModels;

namespace FastPin;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.StartClipboardMonitoring();
    }

    private void MainWindow_Closed(object? sender, System.EventArgs e)
    {
        _viewModel.StopClipboardMonitoring();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button)
        {
            _viewModel.SelectedItem = button.DataContext as PinnedItemViewModel;
            _viewModel.DeleteItemCommand.Execute(null);
        }
    }
}
