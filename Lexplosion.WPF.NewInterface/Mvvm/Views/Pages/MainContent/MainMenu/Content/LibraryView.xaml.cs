using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
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

        }
    }
}
