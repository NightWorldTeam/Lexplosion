using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Modal.InstanceTransfer
{
    /// <summary>
    /// Логика взаимодействия для InstanceImportView.xaml
    /// </summary>
    public partial class InstanceImportView : UserControl
    {
        public InstanceImportView()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            Runtime.DebugWrite($"{border.ActualWidth}x{border.ActualHeight}");
        }
    }
}
