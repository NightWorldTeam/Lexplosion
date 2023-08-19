using Lexplosion.WPF.NewInterface.Controls;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для CatalogView.xaml
    /// </summary>
    public partial class CatalogView : UserControl
    {
        public CatalogView()
        {
            InitializeComponent();
        }

        private void InstanceForm_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var if_ = (InstanceForm)sender;
            Runtime.DebugWrite(if_.ActualWidth);
        }
    }
}
