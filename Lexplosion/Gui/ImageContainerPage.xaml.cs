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

namespace Lexplosion.Gui
{
    /// <summary>
    /// Логика взаимодействия для ImageContainerPage.xaml
    /// </summary>
    public partial class ImageContainerPage : Page
    {
        public ImageContainerPage()
        {
            InitializeComponent();
        }

        private void Arrow_Left_Button(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Arrow Left");
        }

        private void Arrow_Right_Button(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Arrow Right");
        }
    }
}

