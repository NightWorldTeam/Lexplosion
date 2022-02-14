using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public Gallery() // List<string> images
        {
            InitializeComponent();
            //uriImages = images;
            //maxNumberPage = images.Count;
            //Console.WriteLine(images[0]);
        }

        public void ShowControlParams() 
        {
            Console.WriteLine(Image.Width);
            Console.WriteLine(Image.Height);
        }

        private void NextImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPage + 1 != maxNumberPage) 
            {
                selectedPage++;
                var ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(uriImages[selectedPage], UriKind.RelativeOrAbsolute));
                Image.Fill = ib;
            }
        }

        private void PreviousImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPage > 0) 
            {
                selectedPage--;
                var ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(uriImages[selectedPage], UriKind.RelativeOrAbsolute));
                Image.Fill = ib;
            }
        }
    }
}
