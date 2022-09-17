using System.Windows.Controls;

namespace Lexplosion.Gui.Views.Pages.FactoryMenu
{
    /// <summary>
    /// Логика взаимодействия для FactoryGeneralView.xaml
    /// </summary>
    public partial class FactoryGeneralView : UserControl
    {
        public FactoryGeneralView()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                scroll.ScrollToBottom();
            }
            catch { }
        }
    }
}
