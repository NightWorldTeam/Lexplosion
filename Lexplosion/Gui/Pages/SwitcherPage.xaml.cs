using Lexplosion.Gui.Pages.Instance;
using Lexplosion.Gui.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        private Dictionary<string, ToggleItem> content = new Dictionary<string, ToggleItem>();
        private MainWindow mainWindow;
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
                    Width = content[key].Header.Length * 10,
                    Style = (Style)Application.Current.FindResource("MWCBS"),
                };

                if (flag) 
                { 
                    toggleBtn.IsChecked = flag;
                    flag = false;
                    mainWindow.PagesController(content[key].PageName, ContentFrame, delegate ()
                    {
                        return content[key].Page;
                    });
                }

                toggleBtn.Click += HeaderClick;
                HeaderSwitcher.Children.Add(toggleBtn);
            }
        }


        private void HeaderClick(object sender, RoutedEventArgs e) 
        {
            var btn = (ToggleButton)sender;
            RecheckedToggleBtn(btn);
            mainWindow.PagesController(content[btn.Name].PageName, ContentFrame, delegate ()
            {
                return content[btn.Name].Page;
            });
        }

        private void RecheckedToggleBtn(ToggleButton btn)
        {
            foreach (ToggleButton tb in HeaderSwitcher.Children)
                tb.IsChecked = false;
            btn.IsChecked = true;
        } 
    }
}
