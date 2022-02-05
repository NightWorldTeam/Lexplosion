using Lexplosion.Gui.Pages.MW;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Логика взаимодействия для Paginator.xaml
    /// </summary>
    public partial class Paginator : UserControl
    {
        private int _pageIndex = 0;
        public (int min, int max) PageLimit = (0, 1638);

        public int PageIndex
        {
            get { return _pageIndex; }
            set
            {
                SelectedPageTextBox.Text = (value + 1).ToString();

                if (_pageIndex < value)
                {
                    if (value > 0) PrevPageButton.Visibility = Visibility.Visible;
                    if (value == PageLimit.max) NextPageButton.Visibility = Visibility.Hidden;
                }
                else if (_pageIndex > value) 
                {
                    if (value == 0) PrevPageButton.Visibility = Visibility.Hidden;
                    if (value == PageLimit.max - 1) NextPageButton.Visibility = Visibility.Visible;
                }

                _pageIndex = value;
                _page.ChangePage();
            }
        }

        private InstanceContainerPage _page;

        public Paginator(InstanceContainerPage page)
        {
            InitializeComponent();
            _page = page;
            SelectedPageTextBox.Text = (PageIndex + 1).ToString();
            PrevPageButton.Visibility = Visibility.Hidden;
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (PageIndex < PageLimit.max) PageIndex++;
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (PageIndex > 0) PageIndex--;
        }

        private void SelectedPageTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // if "Enter" button clicked
            if (e.Key == Key.Return)
            {
                var val = Int32.Parse(SelectedPageTextBox.Text) - 1;
                if (val <= PageLimit.max)
                    PageIndex = val;
                else PageIndex = PageLimit.max - 10;
                e.Handled = true;
            }
        }

        public void ChangePaginatorVisibility(int instancesCount, int pageSize) 
        {
            this.Dispatcher.Invoke(() => {
                if (instancesCount < pageSize) this.Visibility = Visibility.Hidden;
                else this.Visibility = Visibility.Visible;
            });
        }
    }
}
