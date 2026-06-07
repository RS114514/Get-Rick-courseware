using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using USBAutoCopy;

namespace RickCourseware.Tests
{
    public class USBMonitorTests
    {
        [Fact]
        public void TestMonitorIgnoresDrivesInsertedBeforeStartup()
        {
            var processedDrives = new List<string>();
            var logs = new List<string>();

            // 初始驱动器集合（在监控启动前已插入的 U 盘，如 D:）
            var drives = new HashSet<string> { "D:" };

            var monitor = new USBMonitor("/tmp/backup", (msg) => logs.Add(msg));
            
            // Mock 驱动器检测：返回当前驱动器集合的快照
            monitor.GetDrivesFunc = () =>
            {
                lock (drives)
                {
                    return new HashSet<string>(drives);
                }
            };
            
            // Mock 处理逻辑：记录哪些 U 盘被处理了，不真正读写文件
            monitor.ProcessUSBAction = (drive) =>
            {
                lock (processedDrives)
                {
                    processedDrives.Add(drive);
                }
            };

            // 启动监控
            monitor.Start();

            // 等待一段时间（让第一个 Loop 周期运行）
            Thread.Sleep(500);

            // 验证：开启前已存在的 U 盘 D: 应该被忽略，不应该被处理
            lock (processedDrives)
            {
                Assert.Empty(processedDrives);
            }

            // 现在模拟插入一个新的 U 盘 E:
            lock (drives)
            {
                drives.Add("E:");
            }

            // 等待下一个 Loop 周期（监视线程周期为 2000 毫秒，我们等待 2500 毫秒以确保其被检测到）
            Thread.Sleep(2500);

            // 验证：新插入的 U 盘 E: 应该被成功检测并处理，而 D: 始终保持未处理状态
            lock (processedDrives)
            {
                Assert.Single(processedDrives);
                Assert.Contains("E:", processedDrives);
                Assert.DoesNotContain("D:", processedDrives);
            }

            // 停止监控
            monitor.Stop();
        }
    }
}
