using Lexplosion.Gui.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lexplosion.Gui.Pages
{
    /// <summary>
    /// Логика взаимодействия для SwitcherPage.xaml
    /// </summary>
    /// 

    public class ToggleItem 
    {
        public readonly string  Header;
        public readonly string PageName;
        public readonly Page Page;
        public ToggleItem(string header, string pageName, Page page) 
        {
            Header = header;
            PageName = pageName;
            Page = page;
        }
    }

    public partial class SwitcherPage : Page
    {
        private readonly Dictionary<string, ToggleItem> content = new Dictionary<string, ToggleItem>();
        private readonly MainWindow mainWindow;
        public SwitcherPage(string pageTitle, Dictionary<string, ToggleItem> content, MainWindow mainWindow)
        {
            InitializeComponent();
            PageTitle.Text = pageTitle;
            this.content = content;
            this.mainWindow = mainWindow;
            FillHeaders();
        }

        private void FillHeaders() 
        {
            var flag = true;
            foreach (var key in content.Keys) 
            {
                var toggleBtn = new ToggleButton()
                {
                    Content = content[key].Header,
                    Name = key,
                    Height = 35,
                    Width = double.NaN - content[key].Header.Length * 20,
                    Style = (Style)Application.Current.FindResource("MWCBS"),
                };

                if (flag) 
                { 
                    toggleBtn.IsChecked = flag;
                    flag = false;
                    ContentFrame.Navigate(content[key].Page);
                }

                toggleBtn.Click += HeaderClick;
                HeaderSwitcher.Children.Add(toggleBtn);
            }
        }


        private void HeaderClick(object sender, RoutedEventArgs e) 
        {
            var btn = (ToggleButton)sender;
            RecheckedToggleBtn(btn);
            ContentFrame.Navigate(content[btn.Name].Page);
        }

        private void RecheckedToggleBtn(ToggleButton btn)
        {
            foreach (ToggleButton tb in HeaderSwitcher.Children)
                tb.IsChecked = false;
            btn.IsChecked = true;
        } 
    }
}
