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
    /// Логика взаимодействия для Gallery.xaml
    /// </summary>
    public partial class Gallery : UserControl
    {
        private int selectedPage = 0;
        private int maxNumberPage;
        private List<string> uriImages;

        public Gallery(List<string> images)
        {
            InitializeComponent();
            uriImages = images;
            maxNumberPage = images.Count;
        }

        private void NextImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPage + 1 != maxNumberPage) 
            {
                selectedPage++;
                var ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(uriImages[selectedPage], UriKind.Relative));
                Image.Fill = ib;
            }
        }

        private void PreviousImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPage > -1) 
            {
                selectedPage--;
                var ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(uriImages[selectedPage], UriKind.Relative));
                Image.Fill = ib;
            }
        }
    }
}
