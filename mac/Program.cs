using Avalonia;
using System;

namespace USBAutoCopy
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (var mutex = new System.Threading.Mutex(true, "获取Rick课件_SingleInstance_Mac", out bool createdNew))
            {
                if (!createdNew)
                {
                    Console.WriteLine("程序已在运行中！");
                    return;
                }
                
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
