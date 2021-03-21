using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages
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

