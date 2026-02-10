using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FastPin.Models;
using FastPin.ViewModels;

namespace FastPin
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private AppSettings _settings;
        private TagManagementViewModel _tagViewModel;

        public ConfigWindow()
        {
            InitializeComponent();
            _settings = AppSettings.Load();
            _tagViewModel = new TagManagementViewModel();
            LoadSettings();
            LoadTags();
            
            // Subscribe to tag list selection change
            TagsListBox.SelectionChanged += TagsListBox_SelectionChanged;
            
            // Subscribe to color text change
            TagColorTextBox.TextChanged += TagColorTextBox_TextChanged;
        }

        private void LoadSettings()
        {
            // Load language
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag.ToString() == _settings.Language)
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }

            // Load database settings
            foreach (ComboBoxItem item in DatabaseTypeComboBox.Items)
            {
                if (item.Tag.ToString() == _settings.DatabaseType)
                {
                    DatabaseTypeComboBox.SelectedItem = item;
                    break;
                }
            }

            // Load MySQL settings
            MySqlServerTextBox.Text = _settings.MySqlServer ?? "localhost";
            MySqlPortTextBox.Text = _settings.MySqlPort?.ToString() ?? "3306";
            MySqlDatabaseTextBox.Text = _settings.MySqlDatabase ?? "fastpin";
            MySqlUsernameTextBox.Text = _settings.MySqlUsername ?? "root";
            MySqlPasswordBox.Password = _settings.MySqlPassword ?? "";
        }

        private void LoadTags()
        {
            TagsListBox.ItemsSource = _tagViewModel.Tags;
        }

        private void TagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagsListBox.SelectedItem is Tag tag)
            {
                TagNameTextBox.Text = tag.Name;
                TagClassTextBox.Text = tag.Class ?? string.Empty;
                TagColorTextBox.Text = tag.Color ?? "#0078D4";
                UpdateColorPreview();
            }
        }

        private void TagColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            try
            {
                var colorText = TagColorTextBox.Text;
                if (!string.IsNullOrWhiteSpace(colorText))
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorText);
                    ColorPreviewBorder.Background = new SolidColorBrush(color);
                }
            }
            catch
            {
                // Invalid color, keep previous
            }
        }

        private void ColorButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string colorHex)
            {
                TagColorTextBox.Text = colorHex;
            }
        }

        private void SaveTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TagNameTextBox.Text))
                {
                    MessageBox.Show(
                        FastPin.Resources.LocalizationService.GetString("PleaseEnterTagName"),
                        FastPin.Resources.LocalizationService.GetString("Error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var colorText = TagColorTextBox.Text;
                
                if (TagsListBox.SelectedItem is Tag selectedTag)
                {
                    // Update existing tag
                    selectedTag.Name = TagNameTextBox.Text;
                    selectedTag.Class = TagClassTextBox.Text;
                    selectedTag.Color = colorText;
                    _tagViewModel.SaveTagCommand.Execute(null);
                }
                else
                {
                    // Create new tag
                    _tagViewModel.EditingTagName = TagNameTextBox.Text;
                    _tagViewModel.EditingTagClass = TagClassTextBox.Text;
                    _tagViewModel.EditingTagColor = colorText;
                    _tagViewModel.SaveTagCommand.Execute(null);
                }

                LoadTags();
                ClearEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{FastPin.Resources.LocalizationService.GetString("Error")}: {ex.Message}",
                    FastPin.Resources.LocalizationService.GetString("Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void NewTag_Click(object sender, RoutedEventArgs e)
        {
            TagsListBox.SelectedItem = null;
            ClearEditor();
        }

        private void ClearEditor()
        {
            TagNameTextBox.Text = string.Empty;
            TagClassTextBox.Text = string.Empty;
            TagColorTextBox.Text = "#0078D4";
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Tag tag)
            {
                var result = MessageBox.Show(
                    $"{FastPin.Resources.LocalizationService.GetString("ConfirmDeleteTag")}: {tag.Name}?",
                    FastPin.Resources.LocalizationService.GetString("Confirm"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _tagViewModel.DeleteTagCommand.Execute(tag);
                    LoadTags();
                    ClearEditor();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save language
                if (LanguageComboBox.SelectedItem is ComboBoxItem languageItem)
                {
                    _settings.Language = languageItem.Tag.ToString() ?? "en-US";
                }

                // Save database type
                if (DatabaseTypeComboBox.SelectedItem is ComboBoxItem dbItem)
                {
                    _settings.DatabaseType = dbItem.Tag.ToString() ?? "SQLite";
                }

                // Save MySQL settings
                _settings.MySqlServer = MySqlServerTextBox.Text;
                if (int.TryParse(MySqlPortTextBox.Text, out int port))
                {
                    _settings.MySqlPort = port;
                }
                _settings.MySqlDatabase = MySqlDatabaseTextBox.Text;
                _settings.MySqlUsername = MySqlUsernameTextBox.Text;
                _settings.MySqlPassword = MySqlPasswordBox.Password;

                _settings.Save();

                MessageBox.Show(
                    FastPin.Resources.LocalizationService.GetString("SettingsSavedMessage"),
                    FastPin.Resources.LocalizationService.GetString("Success"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{FastPin.Resources.LocalizationService.GetString("Error")}: {ex.Message}",
                    FastPin.Resources.LocalizationService.GetString("Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DatabaseTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MySqlSettingsPanel == null)
                return;

            if (DatabaseTypeComboBox.SelectedItem is ComboBoxItem item)
            {
                MySqlSettingsPanel.Visibility = item.Tag.ToString() == "MySQL" 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _tagViewModel?.Dispose();
        }
    }
}
