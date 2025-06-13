using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Modal
{
    /// <summary>
    /// Interaction logic for InstanceGroupsConfiguratorView.xaml
    /// </summary>
    public partial class InstanceGroupsConfiguratorView : UserControl
    {
        private InstanceGroupsConfiguratorViewModel _viewModel;

        public InstanceGroupsConfiguratorView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            Runtime.DebugWrite("OnDataContextChanged");

            _viewModel = (InstanceGroupsConfiguratorViewModel)DataContext;
        }

        public void GroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            var collection = _viewModel.Model.SelectedGroups;

            foreach (var ig in e.RemovedItems)
            {
                collection.Remove(ig as InstancesGroup);
            }

            foreach (var ig in e.AddedItems)
            {
                collection.Add(ig as InstancesGroup);
            }
        }

        private void GroupsList_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            var listbox = sender as ListBox;

            var selectedItems = _viewModel.Model.SelectedGroups.ToArray();
            listbox.SelectedItems.Clear();
            _viewModel.Model.SelectedGroups.Clear();
            foreach (var i in selectedItems)
            {
                listbox.SelectedItems.Add(i);
            }
        }
    }
}
