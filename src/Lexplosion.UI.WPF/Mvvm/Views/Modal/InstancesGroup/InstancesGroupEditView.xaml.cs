using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Modal
{
    /// <summary>
    /// Interaction logic for InstancesGroupEditView.xaml
    /// </summary>
    public partial class InstancesGroupEditView : UserControl
    {
        private InstancesGroupEditViewModel _viewModel;

        public InstancesGroupEditView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            Runtime.DebugWrite("OnDataContextChanged");

            _viewModel = (InstancesGroupEditViewModel)DataContext;
        }

        public void InstancesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            var collection = _viewModel.Model.SelectedInstances;

            foreach (var ic in e.RemovedItems)
            {
                collection.Remove(ic as InstanceClient);
            }

            foreach (var ic in e.AddedItems)
            {
                collection.Add(ic as InstanceClient);
            }
        }

        private void InstancesList_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            var listbox = sender as ListBox;

            var selectedItems = _viewModel.Model.SelectedInstances.ToArray();
            listbox.SelectedItems.Clear();
            _viewModel.Model.SelectedInstances.Clear();
            foreach (var i in selectedItems)
            {
                listbox.SelectedItems.Add(i);
            }
        }
    }
}
