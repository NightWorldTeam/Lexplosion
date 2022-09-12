using System;
using System.Windows.Controls;

namespace Lexplosion.Gui.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthView.xaml
    /// </summary>
    public partial class AuthView : UserControl
    {
        public AuthView()
        {
            InitializeComponent();
        }

        private void LoginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var TEST = (TextBox)sender;
            Console.WriteLine("Act: " + TEST.Width);
            Console.WriteLine("Act1: " + TEST.ActualWidth);
        }
    }
}
