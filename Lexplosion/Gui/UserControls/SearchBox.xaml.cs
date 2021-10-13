using Lexplosion.Gui.Pages.MW;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Логика взаимодействия для SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public static readonly SolidColorBrush MouseDownColor = new SolidColorBrush(Color.FromArgb(255, 44, 153, 194));
        public static readonly SolidColorBrush MouseUpColor = new SolidColorBrush(Color.FromArgb(255, 19, 21, 19));

        private InstanceContainerPage page;
        private bool isForcedIndex = true;

        public string LastRequest = "";
        public int LastSelectedIndex = 2;

        public SearchBox(InstanceContainerPage _page)
        {
            InitializeComponent();
            page = _page;
            SourceBox.SelectedIndex = 1;
        }

        private void SearchProcess()
        {
            if (LastRequest != SearchTextBox.Text || SourceBox.SelectedIndex != LastSelectedIndex)
            {
                this.Dispatcher.Invoke(() =>
                {
                    page.SearchInstances();
                });
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchProcess();
        }

        private void SourceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isForcedIndex)
                isForcedIndex = false;
            else
                SearchProcess();
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // if "Enter" button clicked
            if (e.Key == Key.Return)
            {
                SearchProcess();
                e.Handled = true;
            }
        }
    }
}
