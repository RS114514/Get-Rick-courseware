using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;

namespace USBAutoCopy
{
    public partial class BlocklistWindow : Window
    {
        public BlocklistWindow()
        {
            InitializeComponent();
            RefreshList();
        }

        private void RefreshList()
        {
            var list = new List<BlocklistItem>();
            foreach (var item in Properties.Settings.Default.GetBlockedList())
            {
                string display = item.Contains("|") ? item.Split('|')[0] + $" (UUID/序列号: {item.Split('|')[1]})" : item;
                list.Add(new BlocklistItem(display, item));
            }
            if (list.Count == 0)
            {
                list.Add(new BlocklistItem("(暂无屏蔽记录)", null));
            }
            LstBlocked.ItemsSource = list;
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selected = LstBlocked.SelectedItem as BlocklistItem;
            if (selected == null || selected.UniqueId == null) return;

            Properties.Settings.Default.RemoveBlocked(selected.UniqueId);
            RefreshList();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private class BlocklistItem
        {
            public string Display { get; }
            public string UniqueId { get; }
            public BlocklistItem(string display, string uniqueId) 
            { 
                Display = display; 
                UniqueId = uniqueId; 
            }
            public override string ToString() => Display;
        }
    }
}
