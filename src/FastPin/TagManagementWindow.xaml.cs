using System.Windows;
using FastPin.ViewModels;

namespace FastPin
{
    /// <summary>
    /// Interaction logic for TagManagementWindow.xaml
    /// </summary>
    public partial class TagManagementWindow : Window
    {
        private readonly TagManagementViewModel _viewModel;

        public TagManagementWindow()
        {
            InitializeComponent();
            _viewModel = new TagManagementViewModel();
            DataContext = _viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _viewModel?.Dispose();
        }
    }
}
