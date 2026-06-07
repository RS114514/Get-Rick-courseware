using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;

namespace USBAutoCopy
{
    public class MessageBoxWindow : Window
    {
        public MessageBoxWindow(string title, string text)
        {
            Title = title;
            Width = 320;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;
            
            var stackPanel = new StackPanel 
            { 
                Margin = new Avalonia.Thickness(20), 
                Spacing = 15,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = text, 
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            
            var btnClose = new Button 
            { 
                Content = "确定", 
                Width = 80, 
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            btnClose.Click += (s, e) => Close();
            
            stackPanel.Children.Add(btnClose);
            Content = stackPanel;
        }

        public static Task Show(Window owner, string title, string text)
        {
            var box = new MessageBoxWindow(title, text);
            return box.ShowDialog(owner);
        }
    }
}
