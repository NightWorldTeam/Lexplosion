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
    /// 

    public struct ImageBuffer
    {
        private readonly List<string> _links;
        public readonly int Count;

        public ImageBuffer(List<string> links)
        {
            _links = links;
            Count = links.Count;
        }

        public string this[int index] 
        {
            get 
            {
                return _links[index];
            }
        }

        public void Clear() 
        {
            _links.Clear();
        }
    }

    public partial class Gallery : UserControl
    {
        private int maxNumberPage;
        private int selectedPage = 0;
        private ImageBuffer buffer;

        public Gallery()
        {
            InitializeComponent();
        }

        public void LoadImages(List<string> uris)
        {
            if (uris == null || uris.Count == 0)
                return;


            buffer = new ImageBuffer(uris);
            var ib = new ImageBrush();
            ib.ImageSource = new BitmapImage(new Uri(buffer[0], UriKind.RelativeOrAbsolute));
            Image.Fill = ib;
            if (buffer.Count > 1)
            {
                NextImageButton.IsEnabled = true;
                PreviousImageButton.IsEnabled = true;
            }
        }

        private void NextImageButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("++");
            if(selectedPage + 1 < maxNumberPage)
            {
                selectedPage++;
                Image.Fill = new ImageBrush(new BitmapImage(new Uri(buffer[selectedPage], UriKind.RelativeOrAbsolute)));
            }
        }

        private void PreviousImageButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("--");
            if (selectedPage > 0)
            {
                selectedPage--;
                Image.Fill = new ImageBrush(new BitmapImage(new Uri(buffer[selectedPage], UriKind.RelativeOrAbsolute)));
            }
        }

        public void Clear() 
        {
            if (buffer.Count > 0) { 
                buffer.Clear();
                Image.Fill = null;
            }
        }

        public void ShowControlParams()
        {
            Console.WriteLine(Image.Width);
            Console.WriteLine(Image.Height);
        }
    }
}
