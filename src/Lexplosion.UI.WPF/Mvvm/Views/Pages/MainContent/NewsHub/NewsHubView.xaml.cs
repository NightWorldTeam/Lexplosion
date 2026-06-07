using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.NewsHub
{
    /// <summary>
    /// Логика взаимодействия для NewsHubView.xaml
    /// </summary>
    public partial class NewsHubView : UserControl
    {
        public NewsHubView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                // Check if the vertical scroll position is greater than 0
                if (scrollViewer.VerticalOffset > 0)
                {
                    // Show bottom border
                    HeaderBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                }
                else
                {
                    // Hide all borders
                    HeaderBorder.BorderThickness = new Thickness(0, 0, 0, 0);
                }
            }
        }
    }
}
