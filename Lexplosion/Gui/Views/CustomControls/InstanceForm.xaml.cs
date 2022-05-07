using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
