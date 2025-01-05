using Lexplosion.WPF.NewInterface.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.AddonsRepositories
{
    /// <summary>
    /// Interaction logic for AddonsRepository.xaml
    /// </summary>
    public partial class LexplosionAddonsRepository : UserControl
    {
        DropdownMenu _currentOpenedDropDownMenu;

        public LexplosionAddonsRepository()
        {
            InitializeComponent();
        }

        private void DropdownMenuButtonItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_currentOpenedDropDownMenu != null)
            {
                _currentOpenedDropDownMenu.IsOpen = false;
            }
        }

        private void DropdownMenuButton_PopupOpenedEvent(DropdownMenu obj)
        {
            _currentOpenedDropDownMenu = obj;
        }

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_currentOpenedDropDownMenu != null)
            {
                _currentOpenedDropDownMenu.IsOpen = false;
            }
        }
    }
}
