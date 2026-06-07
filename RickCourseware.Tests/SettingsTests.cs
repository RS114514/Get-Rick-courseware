using Xunit;
using System;
using System.IO;
using USBAutoCopy.Properties;

namespace RickCourseware.Tests
{
    public class SettingsTests : IDisposable
    {
        private readonly string _configPath;

        public SettingsTests()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RickConfig.ini");
            // 备份旧配置（若存在）
            if (File.Exists(_configPath))
            {
                File.Move(_configPath, _configPath + ".bak", true);
            }
        }

        public void Dispose()
        {
            // 清理当前测试生成的配置
            if (File.Exists(_configPath))
            {
                File.Delete(_configPath);
            }
            // 恢复原有备份
            if (File.Exists(_configPath + ".bak"))
            {
                File.Move(_configPath + ".bak", _configPath, true);
            }
        }

        [Fact]
        public void TestBackupPathSetting()
        {
            var testPath = @"C:\TestBackupFolder";
            Settings.Default.BackupPath = testPath;

            Assert.Equal(testPath, Settings.Default.BackupPath);
            
            // 检查文件内容是否正确写入
            Assert.True(File.Exists(_configPath));
            var content = File.ReadAllText(_configPath);
            Assert.Contains($"BackupPath={testPath}", content);
        }

        [Fact]
        public void TestAutoStartSetting()
        {
            Settings.Default.AutoStart = true;
            Assert.True(Settings.Default.AutoStart);

            var content = File.ReadAllText(_configPath);
            Assert.Contains("AutoStart=true", content);

            Settings.Default.AutoStart = false;
            Assert.False(Settings.Default.AutoStart);

            content = File.ReadAllText(_configPath);
            Assert.Contains("AutoStart=false", content);
        }

        [Fact]
        public void TestBlockedDrivesList()
        {
            // 重置屏蔽列表
            Settings.Default.BlockedDrives = "";
            Assert.Empty(Settings.Default.GetBlockedList());

            // 添加屏蔽
            Settings.Default.AddBlocked("U_Disk_1");
            Settings.Default.AddBlocked("U_Disk_2");

            var list = Settings.Default.GetBlockedList();
            Assert.Equal(2, list.Count);
            Assert.Contains("U_Disk_1", list);
            Assert.Contains("U_Disk_2", list);

            // 移除屏蔽
            Settings.Default.RemoveBlocked("U_Disk_1");
            list = Settings.Default.GetBlockedList();
            Assert.Single(list);
            Assert.Contains("U_Disk_2", list);
            Assert.DoesNotContain("U_Disk_1", list);
        }
    }
}
