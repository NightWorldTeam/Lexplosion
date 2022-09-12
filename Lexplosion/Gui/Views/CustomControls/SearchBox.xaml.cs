using System.Windows.Controls;

namespace Lexplosion.Gui.Views.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public SearchBox()
        {
            InitializeComponent();
        }

        private void FiltersItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltersDropdownMenu.IsOpen = false;
        }
    }
}
