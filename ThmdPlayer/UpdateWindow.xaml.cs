using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Thmd;
using Thmd.Logs;
using Thmd.Updates;

namespace ThmdPlayer
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private readonly Updater _updater;
        private readonly StringBuilder _statusLog;
        private string _currentProgressLine;

        public UpdateWindow()
        {
            InitializeComponent();
            _updater = new Updater();
            _statusLog = new StringBuilder("Status: Waiting\n");
            _currentProgressLine = string.Empty;
            SubscribeToUpdaterEvents();
        }

        private void SubscribeToUpdaterEvents()
        {
            _updater.UpdateAvailable += Updater_UpdateAvailable;
            _updater.ProgressChanged += Updater_ProgressChanged;
            _updater.UpdateCompleted += Updater_UpdateCompleted;
            _updater.UpdateFailed += Updater_UpdateFailed;
        }

        private void UnsubscribeFromUpdaterEvents()
        {
            _updater.UpdateAvailable -= Updater_UpdateAvailable;
            _updater.ProgressChanged -= Updater_ProgressChanged;
            _updater.UpdateCompleted -= Updater_UpdateCompleted;
            _updater.UpdateFailed -= Updater_UpdateFailed;
        }

        private async void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            CheckUpdateButton.IsEnabled = false;
            UpdateStatus("Status: Checking for updates...");
            RunUpdateButton.IsEnabled = false;

            try
            {
                if (await _updater.CheckForUpdatesAsync())
                {
                    UpdateStatus($"Status: New version {_updater.LatestVersion} available. Downloading...");
                    await _updater.DownloadUpdateAsync();
                }
                else
                {
                    UpdateStatus("Status: No updates available.");
                    CheckUpdateButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Status: Error checking for updates: {ex.Message}");
                CheckUpdateButton.IsEnabled = true;
            }
        }

        private void RunUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Status: Extracting and replacing application files...");
            RunUpdateButton.IsEnabled = false;
            CheckUpdateButton.IsEnabled = false;

            try
            {
                _updater.ApplyUpdate();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Status: Error applying update: {ex.Message}");
                CheckUpdateButton.IsEnabled = true;
            }
        }

        private void Updater_UpdateAvailable(object sender, UpdateAvailableEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RunUpdateButton.IsEnabled = true;
                UpdateStatus($"Status: New version {e.NewVersion} available.");
            });
            Logger.Log.Log(LogLevel.Info, new[] { "File", "Console" }, $"New version available: {e.NewVersion}");
        }

        private void Updater_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProgressBar.Value = e.ProgressPercentage;
                LabelProgress.Content = $"Download progress: {e.ProgressPercentage}%";
                UpdateProgressLine($"Status: Downloading update... {e.ProgressPercentage}%");
            });
        }

        private void Updater_UpdateCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateStatus("\nStatus: Download completed. Ready to apply update.");
                RunUpdateButton.IsEnabled = true;
                CheckUpdateButton.IsEnabled = true;
            });
            Logger.Log.Log(LogLevel.Info, new[] { "File", "Console" }, "Update download completed.");
        }

        private void Updater_UpdateFailed(object sender, UpdateErrorEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateStatus($"Status: Update failed: {e.Exception.Message}");
                CheckUpdateButton.IsEnabled = true;
                RunUpdateButton.IsEnabled = false;
            });
            Logger.Log.Log(LogLevel.Error, new[] { "File", "Console" }, $"Update failed: {e.Exception.Message}", e.Exception);
        }

        private void UpdateStatus(string message)
        {
            _statusLog.AppendLine(message);
            _currentProgressLine = string.Empty; // Reset progress line for new status
            StatusTextBlock.Text = _statusLog.ToString();
            ScrollToBottom();
        }

        private void UpdateProgressLine(string progressMessage)
        {
            // Remove the previous progress line if it exists
            if (!string.IsNullOrEmpty(_currentProgressLine))
            {
                int lastIndex = _statusLog.ToString().LastIndexOf(_currentProgressLine, StringComparison.Ordinal);
                if (lastIndex >= 0)
                {
                    _statusLog.Remove(lastIndex, _currentProgressLine.Length);
                }
            }

            // Append the new progress line
            _statusLog.Append(progressMessage);
            _currentProgressLine = progressMessage;
            StatusTextBlock.Text = _statusLog.ToString();
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            var scrollViewer = StatusTextBlock.Parent as ScrollViewer;
            scrollViewer?.ScrollToBottom();
        }

        protected override void OnClosed(EventArgs e)
        {
            UnsubscribeFromUpdaterEvents();
            _updater?.Dispose();
            base.OnClosed(e);
        }
    }
}