using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace Lexplosion.Gui.Views.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для InstanceForm.xaml
    /// </summary>
    public partial class InstanceForm : UserControl
    {
        public InstanceForm()
        {
            InitializeComponent();
        }

        private void InstanceLogo_MouseEnter(object sender, MouseEventArgs e) 
        {
            InstanceLogo_Background.Effect = new BlurEffect();
            InstanceLogo_Text.Visibility = Visibility.Visible;
        }

        private void InstanceLogo_MouseLeave(object sender, MouseEventArgs e)
        {
            InstanceLogo_Background.Effect = null;
            InstanceLogo_Text.Visibility = Visibility.Collapsed;
        }
    }
}
