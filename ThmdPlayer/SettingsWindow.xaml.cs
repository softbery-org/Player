using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;

namespace ThmdPlayer
{
    public partial class SettingsWindow : Window
    {
        public string SavePath { get; private set; }
        public string VideoQuality { get; private set; }
        public double DefaultVolume { get; private set; }
        public bool AutoPlay { get; private set; }
        public new string FontFamily { get; private set; }
        public new int FontSize { get; private set; }
        public bool FontBold { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Ładowanie przykładowych ustawień (można zastąpić wczytywaniem z pliku lub bazy danych)
            SavePathTextBox.Text = @"C:\Videos";
            VideoQualityComboBox.SelectedIndex = 1; // Domyślnie średnia jakość
            VolumeSlider.Value = 50;
            AutoPlayCheckBox.IsChecked = true;

            // Wypełnianie ComboBox czcionek
            foreach (var font in Fonts.SystemFontFamilies)
            {
                FontFamilyComboBox.Items.Add(font.Source);
            }
            FontFamilyComboBox.SelectedItem = "Arial"; // Domyślna czcionka
            FontSizeTextBox.Text = "14"; // Domyślna wielkość czcionki
            FontBolderCheckBox.IsChecked = false; // Domyślnie bez pogrubienia
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SavePathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Walidacja wielkości czcionki
            if (!int.TryParse(FontSizeTextBox.Text, out int fontSize) || fontSize < 8 || fontSize > 72)
            {
                System.Windows.MessageBox.Show("Proszę podać prawidłową wielkość czcionki (8-72).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Zapis ustawień
            SavePath = SavePathTextBox.Text;
            VideoQuality = (VideoQualityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            DefaultVolume = VolumeSlider.Value;
            AutoPlay = AutoPlayCheckBox.IsChecked ?? false;
            FontFamily = FontFamilyComboBox.SelectedItem?.ToString();
            FontSize = fontSize;
            FontBold = FontBolderCheckBox.IsChecked ?? false;

            // Tutaj można dodać zapis do pliku konfiguracyjnego lub bazy danych
            System.Windows.MessageBox.Show("Ustawienia zapisane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow updateWindow = new UpdateWindow();
            updateWindow.Owner = this;
            updateWindow.ShowDialog();
        }
    }
}