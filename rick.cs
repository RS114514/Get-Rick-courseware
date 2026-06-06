// USBAutoCopy.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace USBAutoCopy
{
    public class USBAutoCopy : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private MainForm mainForm;
        private USBMonitor monitor;
        private bool isMonitoring = false;

        public USBAutoCopy()
        {
            // 初始化托盘图标
            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Information,
                Visible = true
            };

            // 创建托盘菜单
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("显示主窗口", null, ShowMainForm);
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("启动监控", null, StartMonitoring);
            trayMenu.Items.Add("停止监控", null, StopMonitoring);
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("开机自启", null, ToggleAutoStart);
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("退出", null, Exit);
            trayIcon.ContextMenuStrip = trayMenu;

            // 双击托盘图标显示主窗口
            trayIcon.DoubleClick += (s, e) => ShowMainForm(null, null);

            // 创建主窗口
            mainForm = new MainForm(this);
            
            // 显示启动提示
            trayIcon.ShowBalloonTip(3000, "获取Rick课件", "程序已启动，正在监控U盘...", ToolTipIcon.Info);
            
            // 自动开始监控
            StartMonitoring(null, null);
        }

        private void ShowMainForm(object sender, EventArgs e)
        {
            mainForm.Show();
            mainForm.WindowState = FormWindowState.Normal;
            mainForm.BringToFront();
        }

        public void StartMonitoring(object sender, EventArgs e)
        {
            if (!isMonitoring)
            {
                string backupPath = mainForm.GetBackupPath();
                if (string.IsNullOrEmpty(backupPath))
                {
                    mainForm.Show();
                    mainForm.AddLog("请先设置课件保存文件夹！");
                    return;
                }

                monitor = new USBMonitor(backupPath, mainForm.AddLog);
                monitor.Start();
                isMonitoring = true;
                mainForm.SetMonitoringStatus(true);
                UpdateTrayMenuStatus(true);
                trayIcon.ShowBalloonTip(1000, "获取Rick课件", "已开始监控U盘", ToolTipIcon.Info);
            }
        }

        public void StopMonitoring(object sender, EventArgs e)
        {
            if (isMonitoring && monitor != null)
            {
                monitor.Stop();
                isMonitoring = false;
                mainForm.SetMonitoringStatus(false);
                UpdateTrayMenuStatus(false);
                trayIcon.ShowBalloonTip(1000, "获取Rick课件", "已停止监控U盘", ToolTipIcon.Info);
            }
        }

        private void UpdateTrayMenuStatus(bool isRunning)
        {
            foreach (ToolStripItem item in trayMenu.Items)
            {
                if (item.Text == "启动监控")
                {
                    item.Enabled = !isRunning;
                }
                else if (item.Text == "停止监控")
                {
                    item.Enabled = isRunning;
                }
            }
        }

        private void ToggleAutoStart(object sender, EventArgs e)
        {
            bool isAutoStart = GetAutoStartStatus();
            SetAutoStart(!isAutoStart);
            
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                menuItem.Checked = !isAutoStart;
            }
            
            mainForm.UpdateAutoStartStatus(!isAutoStart);
            string status = !isAutoStart ? "已开启" : "已关闭";
            trayIcon.ShowBalloonTip(1000, "获取Rick课件", $"开机自启{status}", ToolTipIcon.Info);
        }

        private bool GetAutoStartStatus()
        {
            try
            {
                string appPath = Application.ExecutablePath;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    string value = key?.GetValue("获取Rick课件") as string;
                    return value != null && value.Equals(appPath, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        private void SetAutoStart(bool enable)
        {
            try
            {
                string appPath = Application.ExecutablePath;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (enable)
                    {
                        key?.SetValue("获取Rick课件", appPath);
                        mainForm.AddLog("✓ 已设置开机自启");
                    }
                    else
                    {
                        key?.DeleteValue("获取Rick课件", false);
                        mainForm.AddLog("✗ 已取消开机自启");
                    }
                }
            }
            catch (Exception ex)
            {
                mainForm.AddLog($"设置开机自启失败: {ex.Message}");
                MessageBox.Show($"设置开机自启失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            StopMonitoring(null, null);
            trayIcon.Visible = false;
            Application.Exit();
        }
    }

    public class MainForm : Form
    {
        private TextBox txtBackupPath;
        private Button btnBrowse, btnStart, btnStop;
        private ListBox lstLog;
        private Label lblStatus;
        private ProgressBar progressBar;
        private USBAutoCopy appContext;
        private Label lblDriveInfo;
        private CheckBox chkAutoStart;
        private Button btnClearLog;

        public MainForm(USBAutoCopy context)
        {
            appContext = context;
            InitializeComponent();
            LoadSettings();
            LoadAutoStartStatus();
        }

        private void InitializeComponent()
        {
            this.Text = "获取Rick课件 v1.0";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += MainForm_FormClosing;
            this.Icon = SystemIcons.Information;

            Label lblPath = new Label() 
            { 
                Text = "课件保存文件夹:", 
                Location = new Point(20, 20), 
                Size = new Size(120, 25), 
                Font = new Font("微软雅黑", 10, FontStyle.Bold) 
            };
            
            txtBackupPath = new TextBox() 
            { 
                Location = new Point(150, 18), 
                Size = new Size(400, 25), 
                ReadOnly = true 
            };
            
            btnBrowse = new Button() 
            { 
                Text = "浏览", 
                Location = new Point(560, 17), 
                Size = new Size(80, 30),
                BackColor = Color.LightBlue
            };
            btnBrowse.Click += BtnBrowse_Click;
            
            lblDriveInfo = new Label() 
            { 
                Text = "💡 提示：插入U盘后会自动复制，文件夹格式：日期_盘符_U盘名称", 
                Location = new Point(20, 60), 
                Size = new Size(650, 25),
                ForeColor = Color.Blue,
                Font = new Font("微软雅黑", 9)
            };
            
            chkAutoStart = new CheckBox()
            {
                Text = "开机自动启动",
                Location = new Point(20, 95),
                Size = new Size(120, 25),
                Font = new Font("微软雅黑", 9)
            };
            chkAutoStart.CheckedChanged += ChkAutoStart_CheckedChanged;
            
            btnStart = new Button() 
            { 
                Text = "启动监控", 
                Location = new Point(150, 92), 
                Size = new Size(100, 35), 
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            btnStart.Click += BtnStart_Click;
            
            btnStop = new Button() 
            { 
                Text = "停止监控", 
                Location = new Point(260, 92), 
                Size = new Size(100, 35), 
                BackColor = Color.LightCoral, 
                Enabled = false,
                FlatStyle = FlatStyle.Flat
            };
            btnStop.Click += BtnStop_Click;
            
            lblStatus = new Label() 
            { 
                Text = "状态: 未监控", 
                Location = new Point(380, 100), 
                Size = new Size(150, 25), 
                ForeColor = Color.Red, 
                Font = new Font("微软雅黑", 9, FontStyle.Bold) 
            };
            
            Label lblLog = new Label() 
            { 
                Text = "运行日志:", 
                Location = new Point(20, 140), 
                Size = new Size(80, 25), 
                Font = new Font("微软雅黑", 10, FontStyle.Bold) 
            };
            
            btnClearLog = new Button()
            {
                Text = "清空日志",
                Location = new Point(580, 138),
                Size = new Size(80, 25),
                BackColor = Color.LightGray
            };
            btnClearLog.Click += BtnClearLog_Click;
            
            lstLog = new ListBox() 
            { 
                Location = new Point(20, 170), 
                Size = new Size(640, 350), 
                Font = new Font("Consolas", 9),
                BackColor = Color.Black,
                ForeColor = Color.LightGreen
            };
            
            progressBar = new ProgressBar() 
            { 
                Location = new Point(20, 530), 
                Size = new Size(640, 20), 
                Style = ProgressBarStyle.Marquee, 
                Visible = false 
            };

            this.Controls.AddRange(new Control[] { 
                lblPath, txtBackupPath, btnBrowse, lblDriveInfo, 
                chkAutoStart, btnStart, btnStop, lblStatus, 
                lblLog, btnClearLog, lstLog, progressBar 
            });

            string savedPath = Properties.Settings.Default.BackupPath;
            if (!string.IsNullOrEmpty(savedPath) && Directory.Exists(savedPath))
            {
                txtBackupPath.Text = savedPath;
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择课件保存文件夹";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtBackupPath.Text = dialog.SelectedPath;
                    SaveSettings();
                    AddLog($"📁 设置保存路径: {dialog.SelectedPath}");
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBackupPath.Text))
            {
                MessageBox.Show("请先选择课件保存文件夹！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(txtBackupPath.Text))
            {
                try
                {
                    Directory.CreateDirectory(txtBackupPath.Text);
                    AddLog($"📁 创建保存目录: {txtBackupPath.Text}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法创建保存目录：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            appContext.StartMonitoring(null, null);
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            appContext.StopMonitoring(null, null);
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            lstLog.Items.Clear();
            AddLog("日志已清空");
        }

        private void ChkAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            var method = appContext.GetType().GetMethod("SetAutoStart", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(appContext, new object[] { chkAutoStart.Checked });
            }
        }

        public string GetBackupPath()
        {
            return txtBackupPath.Text;
        }

        public void AddLog(string message)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action<string>(AddLog), message);
                return;
            }

            string time = DateTime.Now.ToString("HH:mm:ss");
            lstLog.Items.Insert(0, $"[{time}] {message}");
            if (lstLog.Items.Count > 500)
                lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
        }

        public void SetMonitoringStatus(bool isMonitoring)
        {
            if (btnStart.InvokeRequired)
            {
                btnStart.Invoke(new Action<bool>(SetMonitoringStatus), isMonitoring);
                return;
            }

            btnStart.Enabled = !isMonitoring;
            btnStop.Enabled = isMonitoring;
            lblStatus.Text = isMonitoring ? "状态: 监控中 ✓" : "状态: 未监控 ✗";
            lblStatus.ForeColor = isMonitoring ? Color.Green : Color.Red;
        }

        public void UpdateAutoStartStatus(bool isEnabled)
        {
            if (chkAutoStart.InvokeRequired)
            {
                chkAutoStart.Invoke(new Action<bool>(UpdateAutoStartStatus), isEnabled);
                return;
            }
            
            chkAutoStart.Checked = isEnabled;
        }

        public void ShowProgress(bool show)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action<bool>(ShowProgress), show);
                return;
            }

            progressBar.Visible = show;
        }

        private void LoadAutoStartStatus()
        {
            try
            {
                string appPath = Application.ExecutablePath;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    string value = key?.GetValue("获取Rick课件") as string;
                    chkAutoStart.Checked = value != null && value.Equals(appPath, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                chkAutoStart.Checked = false;
            }
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.BackupPath = txtBackupPath.Text;
        }

        private void LoadSettings()
        {
            // 目前设置已在InitializeComponent中加载
            // 未来可以在此加载其他设置项
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                AddLog("程序已最小化到托盘，双击图标可重新打开");
            }
        }
    }

    public class USBMonitor
    {
        private string backupPath;
        private Action<string> logCallback;
        private Thread monitorThread;
        private bool isRunning;
        private HashSet<string> processedDevices = new HashSet<string>();
        private readonly object processedDevicesLock = new object();

        public USBMonitor(string path, Action<string> log)
        {
            backupPath = path;
            logCallback = log;
        }

        public void Start()
        {
            isRunning = true;
            monitorThread = new Thread(MonitorLoop);
            monitorThread.IsBackground = true;
            monitorThread.Start();
            logCallback?.Invoke("🔍 监控已启动，等待U盘插入...");
        }

        public void Stop()
        {
            isRunning = false;
            if (monitorThread != null && monitorThread.IsAlive)
            {
                monitorThread.Join(2000);
            }
            logCallback?.Invoke("⏹ 监控已停止");
        }

        private void MonitorLoop()
        {
            var previousDrives = GetRemovableDrives();

            while (isRunning)
            {
                try
                {
                    var currentDrives = GetRemovableDrives();

                    foreach (var drive in currentDrives)
                    {
                        if (!previousDrives.Contains(drive))
                        {
                            bool shouldProcess = false;
                            lock (processedDevicesLock)
                            {
                                if (!processedDevices.Contains(drive))
                                {
                                    shouldProcess = true;
                                }
                            }
                            if (shouldProcess)
                            {
                                ThreadPool.QueueUserWorkItem(_ => ProcessUSB(drive));
                            }
                        }
                    }

                    foreach (var drive in previousDrives)
                    {
                        if (!currentDrives.Contains(drive))
                        {
                            bool shouldRemove = false;
                            lock (processedDevicesLock)
                            {
                                if (processedDevices.Contains(drive))
                                {
                                    processedDevices.Remove(drive);
                                    shouldRemove = true;
                                }
                            }
                            if (shouldRemove)
                            {
                                logCallback?.Invoke($"💾 U盘已移除: {drive}");
                            }
                        }
                    }

                    previousDrives = currentDrives;
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    logCallback?.Invoke($"⚠ 监控错误: {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        private HashSet<string> GetRemovableDrives()
        {
            var drives = new HashSet<string>();
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType == DriveType.Removable && drive.IsReady)
                    {
                        string drivePath = drive.Name.TrimEnd('\\');
                        if (!string.IsNullOrEmpty(drivePath) && Directory.Exists(drivePath))
                        {
                            drives.Add(drivePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"获取驱动器列表错误: {ex.Message}");
            }
            return drives;
        }

        private string GetUSBName(string driveLetter)
        {
            try
            {
                // 确保路径格式正确：转换为驱动器号格式（如 "D:"）
                string cleanPath = driveLetter.TrimEnd('\\', '/');
                if (cleanPath.Length > 0 && cleanPath[cleanPath.Length - 1] != ':')
                    cleanPath = cleanPath + ":";

                DriveInfo drive = new DriveInfo(cleanPath);
                if (!string.IsNullOrEmpty(drive.VolumeLabel))
                {
                    return drive.VolumeLabel;
                }
                
                // 尝试读取根目录第一个文件夹名作为标识
                try
                {
                    string[] dirs = Directory.GetDirectories(cleanPath);
                    foreach (string dir in dirs)
                    {
                        string dirName = Path.GetFileName(dir);
                        if (!string.IsNullOrEmpty(dirName) && dirName.Length <= 20 && 
                            dirName != "System Volume Information" && dirName != "$RECYCLE.BIN")
                        {
                            return dirName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logCallback?.Invoke($"获取U盘目录列表错误: {ex.Message}");
                }
                
                return "未命名U盘";
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"获取U盘名称错误: {ex.Message}");
                return "未知U盘";
            }
        }

        private string SanitizeFolderName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "未命名";
                
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                name = name.Replace(c, '_');
            }
            
            // 移除可能导致问题的字符
            name = name.Replace(":", "_");
            name = name.Replace("\\", "_");
            name = name.Replace("/", "_");
            
            if (name.Length > 30)
                name = name.Substring(0, 30);
                
            return name.Trim();
        }

        private void ProcessUSB(string driveLetter)
        {
            try
            {
                // 确保驱动器路径格式正确：转换为根目录格式（如 "D:\\"）
                string cleanDrive = driveLetter.TrimEnd('\\', '/');
                if (cleanDrive.Length > 0 && cleanDrive[cleanDrive.Length - 1] != ':')
                    cleanDrive = cleanDrive + ":";
                cleanDrive = cleanDrive + "\\";

                logCallback?.Invoke($"🔌 检测到U盘插入: {cleanDrive}");
                
                // 等待U盘完全加载
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(500);
                    if (Directory.Exists(cleanDrive))
                        break;
                }

                if (!Directory.Exists(cleanDrive))
                {
                    logCallback?.Invoke($"⚠ U盘未就绪: {cleanDrive}");
                    return;
                }

                // 获取U盘信息
                string usbName = GetUSBName(cleanDrive);
                string driveLetterOnly = cleanDrive.TrimEnd('\\');
                string sanitizedUSBName = SanitizeFolderName(usbName);
                
                // 创建文件夹名
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string folderName = $"{timestamp}_{driveLetterOnly}_{sanitizedUSBName}";
                
                // 清理文件夹名中的非法字符
                folderName = SanitizeFolderName(folderName);
                
                string targetFolder = Path.Combine(backupPath, folderName);
                
                // 创建目标文件夹
                try
                {
                    Directory.CreateDirectory(targetFolder);
                    logCallback?.Invoke($"📁 创建文件夹: {folderName}");
                    logCallback?.Invoke($"💾 U盘名称: {usbName}");
                    logCallback?.Invoke($"🎯 目标路径: {targetFolder}");
                }
                catch (Exception ex)
                {
                    logCallback?.Invoke($"❌ 创建文件夹失败: {ex.Message}");
                    return;
                }
                
                // 统计并复制文件
                int totalFiles = 0;
                int copiedFiles = 0;
                
                try
                {
                    totalFiles = CountFiles(cleanDrive);
                    logCallback?.Invoke($"📊 共发现 {totalFiles} 个文件，开始复制...");
                    
                    CopyDirectory(cleanDrive, targetFolder, ref copiedFiles, totalFiles);
                    
                    lock (processedDevicesLock)
                    {
                        processedDevices.Add(driveLetter);
                    }
                    
                    logCallback?.Invoke($"✅ 课件获取完成！共复制 {copiedFiles} 个文件");
                }
                catch (Exception ex)
                {
                    logCallback?.Invoke($"❌ 复制过程出错: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"❌ 处理U盘时出错: {ex.Message}");
            }
        }

        private int CountFiles(string path)
        {
            int count = 0;
            try
            {
                if (!Directory.Exists(path))
                    return 0;
                    
                string[] files = Directory.GetFiles(path);
                count += files.Length;
                
                string[] directories = Directory.GetDirectories(path);
                foreach (string dir in directories)
                {
                    string dirName = Path.GetFileName(dir);
                    if (dirName != "System Volume Information" && 
                        dirName != "$RECYCLE.BIN" &&
                        !dirName.StartsWith("."))
                    {
                        count += CountFiles(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"统计文件数错误: {ex.Message}");
            }
            return count;
        }

        private void CopyDirectory(string sourceDir, string destDir, ref int copiedCount, int totalFiles)
        {
            try
            {
                if (!Directory.Exists(sourceDir))
                    return;
                    
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                // 复制文件
                string[] files = Directory.GetFiles(sourceDir);
                foreach (string file in files)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        if (string.IsNullOrEmpty(fileName))
                            continue;
                            
                        string destFile = Path.Combine(destDir, fileName);
                        
                        // 检查文件是否被占用
                        if (IsFileLocked(file))
                        {
                            logCallback?.Invoke($"⚠ 文件被占用，跳过: {fileName}");
                            continue;
                        }
                        
                        // 复制文件
                        File.Copy(file, destFile, true);
                        copiedCount++;
                        
                        if (totalFiles > 0 && copiedCount % 10 == 0)
                        {
                            int percent = (copiedCount * 100 / totalFiles);
                            logCallback?.Invoke($"📥 复制进度: {copiedCount}/{totalFiles} ({percent}%)");
                        }
                    }
                    catch (Exception ex)
                    {
                        logCallback?.Invoke($"⚠ 复制失败 {Path.GetFileName(file)}: {ex.Message}");
                    }
                }

                // 复制子目录
                string[] directories = Directory.GetDirectories(sourceDir);
                foreach (string subDir in directories)
                {
                    string dirName = Path.GetFileName(subDir);
                    
                    if (dirName == "System Volume Information" || 
                        dirName == "$RECYCLE.BIN" || 
                        dirName.StartsWith("."))
                        continue;
                    
                    string destSubDir = Path.Combine(destDir, dirName);
                    CopyDirectory(subDir, destSubDir, ref copiedCount, totalFiles);
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"⚠ 目录复制错误: {ex.Message}");
            }
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return true;
            }
        }
    }

    namespace Properties
    {
        public class Settings
        {
            private static readonly Settings _default = new Settings();
            public static Settings Default => _default;

            private static Dictionary<string, string> settings = new Dictionary<string, string>();
            private static string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RickConfig.ini");

            static Settings()
            {
                Load();
            }

            public string BackupPath
            {
                get => settings.ContainsKey("BackupPath") ? settings["BackupPath"] : "";
                set { settings["BackupPath"] = value; Save(); }
            }
            
            private static void Load()
            {
                if (File.Exists(configFile))
                {
                    try
                    {
                        foreach (var line in File.ReadAllLines(configFile))
                        {
                            var parts = line.Split(new[] { '=' }, 2);
                            if (parts.Length == 2)
                                settings[parts[0]] = parts[1];
                        }
                    }
                    catch { /* 静默失败，配置加载错误 */ }
                }
            }
            
            public static void Save()
            {
                try
                {
                    var lines = new List<string>();
                    foreach (var kvp in settings)
                        lines.Add($"{kvp.Key}={kvp.Value}");
                    File.WriteAllLines(configFile, lines);
                }
                catch { /* 静默失败，配置保存错误 */ }
            }
        }
    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new USBAutoCopy());
        }
    }
}