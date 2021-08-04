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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for InstanceData.xaml
    /// </summary>
    public partial class InstanceForm : UserControl
    {
        Color BlockBackground = System.Windows.Media.Color.FromArgb(255, 21, 23, 25);
        public InstanceForm()
        {
            InitializeComponent();
            Background.Color = BlockBackground;
        }
    }
}
