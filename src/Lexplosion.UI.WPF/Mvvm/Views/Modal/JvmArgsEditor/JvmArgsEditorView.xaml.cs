using Lexplosion.UI.WPF.Mvvm.Models.Modal;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Lexplosion.UI.WPF.Mvvm.Views.Modal
{
    public partial class JvmArgsEditorView : UserControl
    {
        public JvmArgsEditorView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is JvmArgsEditorViewModel vm)
            {
                vm.FocusNewEntryRequested += OnFocusNewEntryRequested;
            }
        }

        private void OnFocusNewEntryRequested(JvmArgEntry entry)
        {
            if (entry == null || ArgsDataGrid == null || ArgsDataGrid.Columns.Count == 0)
                return;

            ArgsDataGrid.SelectedItem = entry;
            ArgsDataGrid.ScrollIntoView(entry);

            Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                if (ArgsDataGrid.Columns.Count == 0)
                    return;

                ArgsDataGrid.CurrentCell = new DataGridCellInfo(entry, ArgsDataGrid.Columns[0]);
                ArgsDataGrid.Focus();
                ArgsDataGrid.BeginEdit();
            });
        }
    }
}
