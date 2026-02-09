using System.Windows;
using FastPin.ViewModels;

namespace FastPin
{
    /// <summary>
    /// Interaction logic for TagManagementWindow.xaml
    /// </summary>
    public partial class TagManagementWindow : Window
    {
        public TagManagementWindow()
        {
            InitializeComponent();
            DataContext = new TagManagementViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
