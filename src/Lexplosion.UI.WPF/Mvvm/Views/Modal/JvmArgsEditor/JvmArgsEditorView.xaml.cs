using Lexplosion.UI.WPF.Mvvm.Models.Modal;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using System.Windows;
using System.Windows.Controls;

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
            ArgsDataGrid.ScrollIntoView(entry);
            ArgsDataGrid.SelectedItem = entry;

            var column = ArgsDataGrid.Columns[0];
            ArgsDataGrid.CurrentCell = new DataGridCellInfo(entry, column);

            ArgsDataGrid.Focus();
            ArgsDataGrid.BeginEdit();
        }
    }
}
