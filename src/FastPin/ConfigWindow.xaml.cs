using System;
using System.Windows;
using System.Windows.Controls;
using FastPin.Models;

namespace FastPin
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private AppSettings _settings;

        public ConfigWindow()
        {
            InitializeComponent();
            _settings = AppSettings.Load();
            LoadSettings();
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
    }
}
