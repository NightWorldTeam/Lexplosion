using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Modal
{
    /// <summary>
    /// Interaction logic for InstanceGroupFactory.xaml
    /// </summary>
    public partial class InstancesGroupFactoryView : UserControl
    {
        private InstancesGroupFactoryViewModel _viewModel;

        public InstancesGroupFactoryView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            Runtime.DebugWrite("OnDataContextChanged");

            _viewModel = (InstancesGroupFactoryViewModel)DataContext;
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
    }
}
