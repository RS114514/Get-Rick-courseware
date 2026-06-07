using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.IO;

namespace USBAutoCopy
{
    public class App : Application
    {
        private TrayIcon _trayIcon;
        private NativeMenu _trayMenu;
        private NativeMenuItem _startItem;
        private NativeMenuItem _stopItem;
        private MainWindow _mainWindow;
        private USBMonitor _monitor;
        private bool _isMonitoring = false;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _mainWindow = new MainWindow(this);
                desktop.MainWindow = _mainWindow;
                
                // Keep app running even when main window is closed/hidden
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                
                // Initialize Tray Icon
                SetupTrayIcon();
                
                // Load saved autostart status
                LoadAutostartConfig();

                // Delay start monitoring to give system time
                var timer = new System.Threading.Timer(_ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (_mainWindow != null)
                        {
                            StartMonitoring();
                        }
                    });
                }, null, 3000, System.Threading.Timeout.Infinite);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void SetupTrayIcon()
        {
            try
            {
                _trayIcon = new TrayIcon();
                
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (!File.Exists(iconPath))
                {
                    iconPath = "app.ico"; // Fallback to current dir
                }

                if (File.Exists(iconPath))
                {
                    using (var stream = File.OpenRead(iconPath))
                    {
                        _trayIcon.Icon = new WindowIcon(stream);
                    }
                }
                
                _trayIcon.ToolTipText = "获取Rick课件";
                _trayIcon.IsVisible = true;

                // Create native context menu
                _trayMenu = new NativeMenu();

                var showItem = new NativeMenuItem("显示主窗口");
                showItem.Click += (s, e) => ShowMainWindow();
                _trayMenu.Add(showItem);

                _trayMenu.Add(new NativeMenuItemSeparator());

                _startItem = new NativeMenuItem("启动监控");
                _startItem.Click += (s, e) => StartMonitoring();
                _trayMenu.Add(_startItem);

                _stopItem = new NativeMenuItem("停止监控") { IsEnabled = false };
                _stopItem.Click += (s, e) => StopMonitoring();
                _trayMenu.Add(_stopItem);

                _trayMenu.Add(new NativeMenuItemSeparator());

                var autoStartItem = new NativeMenuItem("开机自启") { IsChecked = GetAutoStartStatus() };
                autoStartItem.Click += (s, e) =>
                {
                    bool current = GetAutoStartStatus();
                    SetAutoStart(!current);
                    autoStartItem.IsChecked = !current;
                    if (_mainWindow != null)
                    {
                        _mainWindow.UpdateAutoStartCheckbox(!current);
                    }
                };
                _trayMenu.Add(autoStartItem);

                _trayMenu.Add(new NativeMenuItemSeparator());

                var exitItem = new NativeMenuItem("退出");
                exitItem.Click += (s, e) => Exit();
                _trayMenu.Add(exitItem);

                _trayIcon.Menu = _trayMenu;
                _trayIcon.Clicked += (s, e) => ShowMainWindow();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up tray icon: {ex.Message}");
            }
        }

        public void ShowMainWindow()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        }

        public void StartMonitoring()
        {
            if (!_isMonitoring)
            {
                string backupPath = _mainWindow.GetBackupPath();
                if (string.IsNullOrEmpty(backupPath))
                {
                    ShowMainWindow();
                    _mainWindow.AddLog("请先设置课件保存文件夹！");
                    return;
                }

                _monitor = new USBMonitor(backupPath, _mainWindow.AddLog);
                _monitor.Start();
                _isMonitoring = true;
                _mainWindow.SetMonitoringStatus(true);
                UpdateTrayMenuStatus(true);
            }
        }

        public void StopMonitoring()
        {
            if (_isMonitoring && _monitor != null)
            {
                _monitor.Stop();
                _isMonitoring = false;
                _mainWindow.SetMonitoringStatus(false);
                UpdateTrayMenuStatus(false);
            }
        }

        private void UpdateTrayMenuStatus(bool isRunning)
        {
            if (_startItem != null) _startItem.IsEnabled = !isRunning;
            if (_stopItem != null) _stopItem.IsEnabled = isRunning;
        }

        private void LoadAutostartConfig()
        {
            bool enabled = Properties.Settings.Default.AutoStart;
            SetAutoStart(enabled, quiet: true);
        }

        public bool GetAutoStartStatus()
        {
            return Properties.Settings.Default.AutoStart;
        }

        public void SetAutoStart(bool enable, bool quiet = false)
        {
            try
            {
                string plistPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library/LaunchAgents/com.rs.rickcourseware.plist");

                if (enable)
                {
                    string execPath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    string plistContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>com.rs.rickcourseware</string>
    <key>ProgramArguments</key>
    <array>
        <string>{execPath}</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
</dict>
</plist>";
                    string dir = Path.GetDirectoryName(plistPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllText(plistPath, plistContent);
                    if (!quiet) _mainWindow?.AddLog("✓ 已设置开机自启 (macOS LaunchAgent)");
                }
                else
                {
                    if (File.Exists(plistPath))
                    {
                        File.Delete(plistPath);
                    }
                    if (!quiet) _mainWindow?.AddLog("✗ 已取消开机自启");
                }
                Properties.Settings.Default.AutoStart = enable;
            }
            catch (Exception ex)
            {
                if (!quiet)
                {
                    _mainWindow?.AddLog($"设置开机自启失败: {ex.Message}");
                }
            }
        }

        public void Exit()
        {
            StopMonitoring();
            if (_trayIcon != null)
            {
                _trayIcon.Dispose();
            }
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}
