using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace USBAutoCopy
{
    public partial class MainWindow : Window
    {
        private App _appContext;
        private System.Threading.Timer _driveRefreshTimer;
        private Dictionary<string, string> _driveMap = new Dictionary<string, string>(); // 显示文本 -> 唯一标识
        private ObservableCollection<string> _logEntries = new ObservableCollection<string>();
        private bool _isExplicitClose = false;
        private HashSet<string> _lastVolumes = new HashSet<string>();
        private readonly object _refreshLock = new object();
        private bool _isRefreshing = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(App context)
        {
            _appContext = context;
            InitializeComponent();
            
            LstLog.ItemsSource = _logEntries;
            
            LoadSettings();
            LoadAutoStartStatus();
            
            // Periodically refresh drive list (every 2 seconds, runs on background thread)
            _driveRefreshTimer = new System.Threading.Timer(_ =>
            {
                RefreshDriveList(force: false);
            }, null, 0, 2000);
            
            Closing += MainWindow_Closing;
        }

        private void LoadSettings()
        {
            string savedPath = Properties.Settings.Default.BackupPath;
            if (!string.IsNullOrEmpty(savedPath))
            {
                TxtBackupPath.Text = savedPath;
            }
        }

        private void LoadAutoStartStatus()
        {
            ChkAutoStart.IsChecked = _appContext.GetAutoStartStatus();
        }

        public void UpdateAutoStartCheckbox(bool isChecked)
        {
            ChkAutoStart.IsChecked = isChecked;
        }

        private async void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "选择课件保存文件夹",
                    AllowMultiple = false
                });

                if (folders != null && folders.Count > 0)
                {
                    string selectedPath = folders[0].Path.LocalPath;
                    TxtBackupPath.Text = selectedPath;
                    Properties.Settings.Default.BackupPath = selectedPath;
                    Properties.Settings.Save();
                    AddLog($"📁 设置保存路径: {selectedPath}");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageBox("错误", $"无法选择文件夹: {ex.Message}");
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtBackupPath.Text))
            {
                await ShowMessageBox("提示", "请先选择课件保存文件夹！");
                return;
            }

            if (!Directory.Exists(TxtBackupPath.Text))
            {
                try
                {
                    Directory.CreateDirectory(TxtBackupPath.Text);
                    AddLog($"📁 创建保存目录: {TxtBackupPath.Text}");
                }
                catch (Exception ex)
                {
                    await ShowMessageBox("错误", $"无法创建保存目录: {ex.Message}");
                    return;
                }
            }

            _appContext.StartMonitoring();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _appContext.StopMonitoring();
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logEntries.Clear();
            AddLog("日志已清空");
        }

        public string GetBackupPath()
        {
            return TxtBackupPath.Text;
        }

        public void AddLog(string message)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                string entry = $"[{time}] {message}";
                _logEntries.Insert(0, entry);
                if (_logEntries.Count > 500)
                    _logEntries.RemoveAt(_logEntries.Count - 1);
                
                try
                {
                    string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                    string logsDir = Path.Combine(exeDir, "logs");
                    Directory.CreateDirectory(logsDir);
                    string logFile = Path.Combine(logsDir, $"{DateTime.Now:yyyyMMdd}.log");
                    File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
                }
                catch { }
            });
        }

        public void SetMonitoringStatus(bool isMonitoring)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                BtnStart.IsEnabled = !isMonitoring;
                BtnStop.IsEnabled = isMonitoring;
                TxtStatus.Text = isMonitoring ? "状态: 监控中 ✓" : "状态: 未监控 ✗";
                TxtStatus.Foreground = isMonitoring ? Avalonia.Media.Brushes.Green : Avalonia.Media.Brushes.Red;
            });
        }

        private void RefreshDriveList(bool force = false)
        {
            lock (_refreshLock)
            {
                if (_isRefreshing) return;
                _isRefreshing = true;
            }

            Task.Run(() =>
            {
                try
                {
                    var currentVolumes = new HashSet<string>();
                    if (Directory.Exists("/Volumes"))
                    {
                        foreach (var dir in Directory.GetDirectories("/Volumes"))
                        {
                            currentVolumes.Add(dir);
                        }
                    }

                    bool changed = false;
                    lock (_refreshLock)
                    {
                        if (currentVolumes.Count != _lastVolumes.Count)
                        {
                            changed = true;
                        }
                        else
                        {
                            foreach (var vol in currentVolumes)
                            {
                                if (!_lastVolumes.Contains(vol))
                                {
                                    changed = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!force && !changed && _driveMap.Count > 0)
                    {
                        return;
                    }

                    var blocked = Properties.Settings.Default.GetBlockedList();
                    var newDriveMap = new Dictionary<string, string>();

                    foreach (var dir in currentVolumes)
                    {
                        if (USBMonitor.IsMacUsbDrive(dir))
                        {
                            string drivePath = dir;
                            string label = Path.GetFileName(dir);
                            string uniqueId = USBMonitor.GetDriveUniqueId(drivePath);
                            bool isBlocked = blocked.Contains(uniqueId);
                            string display = isBlocked ? $"{drivePath} - {label} [已屏蔽]" : $"{drivePath} - {label}";
                            newDriveMap[display] = uniqueId;
                        }
                    }

                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        lock (_refreshLock)
                        {
                            _driveMap = newDriveMap;
                            _lastVolumes = currentVolumes;
                        }

                        string selected = CmbDrives.SelectedItem as string;
                        CmbDrives.Items.Clear();
                        if (newDriveMap.Count == 0)
                        {
                            CmbDrives.Items.Add("（无可移动设备）");
                        }
                        else
                        {
                            foreach (var key in newDriveMap.Keys)
                            {
                                CmbDrives.Items.Add(key);
                            }
                        }

                        if (selected != null && CmbDrives.Items.Contains(selected))
                            CmbDrives.SelectedItem = selected;
                        else if (CmbDrives.Items.Count > 0)
                            CmbDrives.SelectedIndex = 0;
                    });
                }
                catch { }
                finally
                {
                    lock (_refreshLock)
                    {
                        _isRefreshing = false;
                    }
                }
            });
        }

        private async void BtnBlockDrive_Click(object sender, RoutedEventArgs e)
        {
            string display = CmbDrives.SelectedItem as string;
            if (string.IsNullOrEmpty(display) || display == "（无可移动设备）") return;

            if (!_driveMap.TryGetValue(display, out string uniqueId)) return;

            string friendlyName = display.Replace(" [已屏蔽]", "").Trim();

            var blocked = Properties.Settings.Default.GetBlockedList();
            if (blocked.Contains(uniqueId))
            {
                await ShowMessageBox("提示", $"「{friendlyName}」已在屏蔽列表中。");
                return;
            }

            Properties.Settings.Default.AddBlocked(uniqueId);
            AddLog($"🚫 已屏蔽 U 盘: {friendlyName}");
            RefreshDriveList(force: true);
        }

        private async void BtnManageBlock_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BlocklistWindow();
            await dialog.ShowDialog(this);
            RefreshDriveList(force: true);
        }

        private void ChkAutoStart_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = ChkAutoStart.IsChecked ?? false;
            _appContext.SetAutoStart(isChecked);
        }

        private void MainWindow_Closing(object sender, WindowClosingEventArgs e)
        {
            if (!_isExplicitClose)
            {
                e.Cancel = true;
                Hide();
                AddLog("程序已最小化到托盘，单击托盘图标可重新打开");
            }
        }

        public void CloseExplicitly()
        {
            _isExplicitClose = true;
            Close();
        }

        private Task ShowMessageBox(string title, string text)
        {
            return MessageBoxWindow.Show(this, title, text);
        }
    }
}
