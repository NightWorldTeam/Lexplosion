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
        private int maxNumberPage;
        private int selectedPage = 0;
        private List<string> uriImages;

        public Gallery()
        {
            InitializeComponent();
        }

        private void NextImageButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("++");
            if(selectedPage + 1 < maxNumberPage)
            {
                selectedPage++;
                Image.Fill = new ImageBrush(new BitmapImage(new Uri(uriImages[selectedPage], UriKind.RelativeOrAbsolute)));
            }
        }

        private void PreviousImageButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("--");
            if (selectedPage > 0)
            {
                selectedPage--;
                Image.Fill = new ImageBrush(new BitmapImage(new Uri(uriImages[selectedPage], UriKind.RelativeOrAbsolute)));
            }
        }

        public void LoadImages(List<string> uris)
        {
            if(uris == null || uris.Count == 0)
                return;

            uriImages = uris;

            maxNumberPage = uris.Count;
            var ib = new ImageBrush();
            ib.ImageSource = new BitmapImage(new Uri(uriImages[0], UriKind.RelativeOrAbsolute));
            Image.Fill = ib;
            if(maxNumberPage > 1)
            {
                NextImageButton.IsEnabled = true;
                PreviousImageButton.IsEnabled = true;
            }
        }

        public void Clear() 
        {
            uriImages.Clear();
            Image.Fill = null;
        }

        public void ShowControlParams()
        {
            Console.WriteLine(Image.Width);
            Console.WriteLine(Image.Height);
        }
    }
}
