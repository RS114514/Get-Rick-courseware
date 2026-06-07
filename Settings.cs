using System;
using System.Collections.Generic;
using System.IO;

namespace USBAutoCopy
{
    namespace Properties
    {
        public class Settings
        {
            private static readonly Settings _default = new Settings();
            public static Settings Default => _default;

            private static Dictionary<string, string> settings = new Dictionary<string, string>();
            private static string configFile = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "RickConfig.ini");

            static Settings()
            {
                Load();
            }

            public string BackupPath
            {
                get => settings.ContainsKey("BackupPath") ? settings["BackupPath"] : "";
                set { settings["BackupPath"] = value; Save(); }
            }

            public bool AutoStart
            {
                get => settings.ContainsKey("AutoStart") && settings["AutoStart"] == "true";
                set { settings["AutoStart"] = value ? "true" : "false"; Save(); }
            }

            public string BlockedDrives
            {
                get => settings.ContainsKey("BlockedDrives") ? settings["BlockedDrives"] : "";
                set { settings["BlockedDrives"] = value; Save(); }
            }

            public List<string> GetBlockedList()
            {
                var raw = BlockedDrives;
                if (string.IsNullOrEmpty(raw)) return new List<string>();
                return new List<string>(raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }

            public void AddBlocked(string name)
            {
                var list = GetBlockedList();
                if (!list.Contains(name)) { list.Add(name); BlockedDrives = string.Join(",", list); }
            }

            public void RemoveBlocked(string name)
            {
                var list = GetBlockedList();
                list.Remove(name);
                BlockedDrives = string.Join(",", list);
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
}
