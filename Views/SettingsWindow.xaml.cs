using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using LearningReminder.Models;
using LearningReminder.Resources;
using LearningReminder.Services;
using Microsoft.Win32;

namespace LearningReminder.Views
{
    /// <summary>
    /// 设置窗口：提醒（免打扰等）、每日目标、数据与隐私（备份/加密/导入导出）、关于与更新。
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private string _latestDownloadUrl = string.Empty;

        /// <summary>构造设置窗口并载入当前设置。</summary>
        public SettingsWindow()
        {
            InitializeComponent();
            LoadFromSettings();
        }

        private void LoadFromSettings()
        {
            AppSettings settings = DataStore.Instance.Data.Settings;

            DndCheck.IsChecked = settings.DndEnabled;
            DndStartBox.Text = settings.DndStart;
            DndEndBox.Text = settings.DndEnd;
            SnoozeBox.Text = settings.SnoozeMinutes.ToString();
            IntervalBox.Text = settings.DefaultIntervalMinutes.ToString();
            AutoCardCheck.IsChecked = settings.AutoShowCheckCard;
            GoalBox.Text = settings.DailyGoalCount.ToString();
            EncryptCheck.IsChecked = settings.EncryptData;
            CheckUpdateOnStartCheck.IsChecked = settings.CheckUpdateOnStart;

            VersionText.Text = string.Format(AppStrings.LabelVersionFormat, UpdateService.CurrentVersion);
            BackupHintText.Text = string.Format(AppStrings.BackupHintFormat, AppConstants.BackupKeepCount);
            DataDirText.Text = string.Format(AppStrings.DataPathLabelFormat, AppPaths.DataDirectory);

            UpdateDndState();
        }

        private void OnDndChanged(object sender, RoutedEventArgs e)
        {
            UpdateDndState();
        }

        private void UpdateDndState()
        {
            bool enabled = DndCheck.IsChecked == true;
            DndStartBox.IsEnabled = enabled;
            DndEndBox.IsEnabled = enabled;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            bool dndEnabled = DndCheck.IsChecked == true;
            string dndStart = DataStore.Instance.Data.Settings.DndStart;
            string dndEnd = DataStore.Instance.Data.Settings.DndEnd;

            if (dndEnabled)
            {
                if (!QuietHours.TryParseRange(DndStartBox.Text, DndEndBox.Text, out _, out _))
                {
                    ShowError(AppStrings.ValidationDndRange);
                    return;
                }

                dndStart = DndStartBox.Text.Trim();
                dndEnd = DndEndBox.Text.Trim();
            }

            if (!TryReadNumber(
                    SnoozeBox.Text,
                    AppConstants.MinSnoozeMinutes,
                    AppConstants.MaxSnoozeMinutes,
                    AppStrings.LabelSnoozeMinutes,
                    out int snooze)
                || !TryReadNumber(
                    IntervalBox.Text,
                    AppConstants.MinIntervalMinutes,
                    AppConstants.MaxIntervalMinutes,
                    AppStrings.LabelDefaultInterval,
                    out int interval)
                || !TryReadNumber(
                    GoalBox.Text,
                    0,
                    AppConstants.MaxDailyGoalCount,
                    AppStrings.LabelDailyGoal,
                    out int goal))
            {
                return;
            }

            AppSettings settings = DataStore.Instance.Data.Settings;
            settings.DndEnabled = dndEnabled;
            settings.DndStart = dndStart;
            settings.DndEnd = dndEnd;
            settings.SnoozeMinutes = snooze;
            settings.DefaultIntervalMinutes = interval;
            settings.AutoShowCheckCard = AutoCardCheck.IsChecked == true;
            settings.DailyGoalCount = goal;
            settings.EncryptData = EncryptCheck.IsChecked == true;
            settings.CheckUpdateOnStart = CheckUpdateOnStartCheck.IsChecked == true;
            DataStore.Instance.SaveAndNotify();

            DialogResult = true;
        }

        private bool TryReadNumber(string text, int min, int max, string field, out int value)
        {
            value = min;
            if (!int.TryParse(text.Trim(), out int parsed) || parsed < min || parsed > max)
            {
                ShowError(string.Format(AppStrings.ValidationNumberRangeFormat, field, min, max));
                return false;
            }

            value = parsed;
            return true;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnOpenDataDirClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = AppPaths.DataDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                FileLogger.Error("打开数据目录失败", ex);
            }
        }

        private void OnBackupNowClick(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.CreateTimestampedBackup(DateTime.Now);
            MessageBox.Show(
                this,
                AppStrings.BackupDone,
                AppStrings.SettingsWindowTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = AppStrings.ExportDialogTitle,
                Filter = AppStrings.DataFileFilter,
                FileName = AppStrings.ExportFileNamePrefix + DateTime.Now.ToString("yyyyMMdd") + ".json"
            };
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                DataStore.Instance.ExportTo(dialog.FileName);
                MessageBox.Show(
                    this,
                    AppStrings.ExportSuccess,
                    AppStrings.SettingsWindowTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                FileLogger.Error("导出数据失败", ex);
                MessageBox.Show(
                    this,
                    AppStrings.ExportFailed,
                    AppStrings.SettingsWindowTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void OnImportClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = AppStrings.ImportDialogTitle,
                Filter = AppStrings.DataFileFilter
            };
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                this,
                AppStrings.ImportConfirmMessage,
                AppStrings.ImportConfirmTitle,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (confirm != MessageBoxResult.OK)
            {
                return;
            }

            try
            {
                DataStore.Instance.ImportFrom(dialog.FileName);
                CheckInService.Instance.ClearAll();
                MessageBox.Show(
                    this,
                    AppStrings.ImportSuccess,
                    AppStrings.SettingsWindowTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 导入后数据整体替换，直接关闭设置让主界面刷新
                DialogResult = true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("导入数据失败", ex);
                MessageBox.Show(
                    this,
                    AppStrings.ImportFailed,
                    AppStrings.SettingsWindowTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async void OnCheckUpdateClick(object sender, RoutedEventArgs e)
        {
            UpdateStatusText.Text = AppStrings.UpdateChecking;
            DownloadLinkButton.Visibility = Visibility.Collapsed;

            UpdateInfo? info = await UpdateService.CheckAsync();
            ApplyUpdateResult(info);
        }

        private void ApplyUpdateResult(UpdateInfo? info)
        {
            if (info == null)
            {
                UpdateStatusText.Text = AppStrings.UpdateFailed;
                return;
            }

            if (UpdateService.IsNewer(info.Version, UpdateService.CurrentVersion))
            {
                UpdateStatusText.Text = string.Format(AppStrings.UpdateAvailableFormat, info.Version);
                _latestDownloadUrl = info.Url;
                DownloadLinkButton.Visibility = Visibility.Visible;
                return;
            }

            UpdateStatusText.Text = string.Format(AppStrings.UpdateLatestFormat, UpdateService.CurrentVersion);
        }

        private void OnGoDownloadClick(object sender, RoutedEventArgs e)
        {
            string url = string.IsNullOrEmpty(_latestDownloadUrl) ? AppConstants.DownloadPageUrl : _latestDownloadUrl;
            LinkLauncher.TryOpen(url);
        }
    }
}