using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal.InstanceTransfer;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.Views.Modal.InstanceTransfer
{
    /// <summary>
    /// Логика взаимодействия для InstanceImportView.xaml
    /// </summary>
    public partial class InstanceImportView : UserControl
    {
        private InstanceImportViewModel _importViewModel;

        public InstanceImportView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (_importViewModel != null)
            {
                _importViewModel.Model.ImportProcesses.CollectionChanged -= ImportProcessesChanged;
            }
            _importViewModel = (InstanceImportViewModel)DataContext;

            if (_importViewModel != null)
            {
                _importViewModel.Model.ImportProcesses.CollectionChanged += ImportProcessesChanged;
            }
        }

        private void ImportProcessesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                PageScroll.ScrollToBottom();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            Runtime.DebugWrite($"{border.ActualWidth}x{border.ActualHeight}");
        }


    }
}
