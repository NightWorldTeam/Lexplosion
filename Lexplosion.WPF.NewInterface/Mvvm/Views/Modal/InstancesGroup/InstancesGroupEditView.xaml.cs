using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Modal
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
    }
}
