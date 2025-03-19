using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для LibraryView.xaml
    /// </summary>
    public partial class LibraryView : System.Windows.Controls.UserControl
    {
        double filterHeight = 0;
        bool _isFilterHidden = false;

        public LibraryView()
        {
            InitializeComponent();
            filterHeight = FiltersControlPanel.ActualHeight;
        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (BackTopButton.TargetScroll == null)
            {
                BackTopButton.TargetScroll = e.OriginalSource as ScrollViewer;
            }
        }
    }
}
