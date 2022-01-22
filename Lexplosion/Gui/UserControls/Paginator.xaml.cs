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
        public int pageIndex = 0;
        public (int min, int max) pageLimit = (0, 1638);

        public int PageIndex
        {
            get { return pageIndex; }
            set
            {
                SelectedPageTextBox.Text = (value + 1).ToString();
                if (pageIndex < value)
                {
                    if (value == pageLimit.max) NextPageButton.Visibility = Visibility.Hidden;
                    if (value > 0) PrevPageButton.Visibility = Visibility.Visible;
                }
                else if (pageIndex > value) 
                {
                    if (value == 0) PrevPageButton.Visibility = Visibility.Hidden;
                    if (value == pageLimit.max - 1) NextPageButton.Visibility = Visibility.Visible;
                }
                pageIndex = value;
                page.ChangePage();
            }
        }

        private InstanceContainerPage page;

        public Paginator(InstanceContainerPage _page)
        {
            InitializeComponent();
            page = _page;
            SelectedPageTextBox.Text = (PageIndex + 1).ToString();
            PrevPageButton.Visibility = Visibility.Hidden;
        }
        
        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (PageIndex < pageLimit.max) PageIndex++;
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
                PageIndex = Int32.Parse(SelectedPageTextBox.Text) - 1;
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
