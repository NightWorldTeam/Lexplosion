using Lexplosion.Logic.Management.Addons;
using Lexplosion.UI.WPF.Mvvm.ViewModels.AddonsRepositories;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Lexplosion.UI.WPF.Controls;
using Lexplosion.UI.WPF.Extensions;
using System;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.AddonsRepositories
{
    /// <summary>
    /// Interaction logic for AddonsRepository.xaml
    /// </summary>
    public partial class LexplosionAddonsRepository : UserControl
    {
        DropdownMenu _currentOpenedDropDownMenu;
        private LexplosionAddonsRepositoryViewModel _viewModel;

        public LexplosionAddonsRepository()
        {
            InitializeComponent();
            DataContextChanged += OnRepositoryDataContextChanged;
            Unloaded += (sender, args) => DetachViewModel();
            Loaded += (sender, args) => AttachViewModel(DataContext as LexplosionAddonsRepositoryViewModel);
        }

        private void OnRepositoryDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AttachViewModel(e.NewValue as LexplosionAddonsRepositoryViewModel);
        }

        private void DetachViewModel()
        {
            if (_viewModel != null) _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = null;
        }

        private void AttachViewModel(LexplosionAddonsRepositoryViewModel viewModel)
        {
            if (ReferenceEquals(_viewModel, viewModel)) return;
            DetachViewModel();
            _viewModel = viewModel;
            if (_viewModel != null) _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateRepositoryLayout();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LexplosionAddonsRepositoryViewModel.IsAddonDetailsOpen))
                Dispatcher.BeginInvoke(new System.Action(UpdateRepositoryLayout));
        }

        private void UpdateRepositoryLayout()
        {
            bool detail = _viewModel?.IsAddonDetailsOpen == true;
            FiltersColumn.Width = new GridLength(detail ? 0 : 250);
            FiltersPanel.Visibility = detail ? Visibility.Collapsed : Visibility.Visible;
            RepositoryTabs.Visibility = detail ? Visibility.Collapsed : Visibility.Visible;
            // Keep the existing visibility binding for pending filter changes intact.
            ApplyCategoriesButton.Opacity = detail ? 0 : 1;
            ApplyCategoriesButton.IsHitTestVisible = !detail;
        }

        private void AddonTitle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is InstanceAddon addon)
            {
                _viewModel?.OpenAddonDetails(addon);
                e.Handled = true;
            }
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
            // No opacity / entrance animation around the native WebView2 surface.
            UpdateRepositoryLayout();
        }
    }
}
