using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Modal.InstanceTransfer
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
            //var tmpViewModel = _importViewModel;
            if (_importViewModel != null) 
            {
              _importViewModel.Model.ImportProcesses.CollectionChanged -= ImportProcessesChanged;
            }
            _importViewModel = (InstanceImportViewModel)DataContext;

            //if (tmpViewModel != null) 
            //{
            //    tmpViewModel.Model.ImportProcesses.CollectionChanged -= ImportProcessesChanged;
            //}

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
