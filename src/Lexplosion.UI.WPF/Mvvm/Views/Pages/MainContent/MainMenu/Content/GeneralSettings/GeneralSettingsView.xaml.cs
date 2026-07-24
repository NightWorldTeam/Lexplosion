using System.Windows.Controls;
using System.Windows.Input;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.MainMenu
{
    public partial class GeneralSettingsView : UserControl
    {
        public GeneralSettingsView()
        {
            InitializeComponent();
        }

        private void JvmArgsTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is GeneralSettingsViewModel vm && vm.OpenJvmArgsEditorCommand.CanExecute(null))
                vm.OpenJvmArgsEditorCommand.Execute(null);
        }
    }
}
