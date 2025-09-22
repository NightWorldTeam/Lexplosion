using Lexplosion.UI.WPF.Controls;
using Lexplosion.UI.WPF.Extensions;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.AddonsRepositories
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

        private void AddonRepositoryCatalogView_PaginationChanged()
        {
            ScrollViewerExtensions.ScroollToPosAnimated(
                Scroll,
                ScrollViewerExtensions.GetScrollBar(Scroll).Minimum
            );
        }

        private void Grid_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var grid = sender as Grid;
            var da = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            grid.BeginAnimation(OpacityProperty, da);
        }
    }
}
