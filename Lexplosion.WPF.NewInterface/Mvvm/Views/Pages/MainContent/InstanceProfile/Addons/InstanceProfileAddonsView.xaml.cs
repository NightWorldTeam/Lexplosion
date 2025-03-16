using Lexplosion.WPF.NewInterface.Controls;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    /// <summary>
    /// Логика взаимодействия для InstanceProfileAddonsView.xaml
    /// </summary>
    public partial class InstanceProfileAddonsView : UserControl
    {
        private InstanceAddonsContainerViewModel _model;

        public InstanceProfileAddonsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _model = (InstanceAddonsContainerViewModel)DataContext;
            if (_model != null) 
            {
                SetSearchBoxPlaceholder(_model.Model.SelectedSortByParam);
            }
        }

        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            DragDropField.Visibility = System.Windows.Visibility.Visible;
        }

        private void DragDropField_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            DragDropField.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void DragDropField_Drop(object sender, System.Windows.DragEventArgs e)
        {
            var fe = sender as FrameworkElement;
            Console.WriteLine(e.Data);

            fe.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OnAddonsSearchSortParamChangedChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ComboBox).SelectedItem;
            if (selectedItem != null)
                SetSearchBoxPlaceholder(selectedItem.ToString());
        }

        private void SetSearchBoxPlaceholder(string sortParam) 
        {
            var addonType = _model.Model.Type switch
            {
                AddonType.Mods => "Mod",
                AddonType.Resourcepacks => "Resourcepack",
                AddonType.Maps => "Map",
                AddonType.Shaders => "Shader",
                _ => string.Empty
            };

            SearchBox.SetResourceReference(SearchBox.PlaceholderProperty, $"{addonType}{sortParam}");
        }
    }
}
