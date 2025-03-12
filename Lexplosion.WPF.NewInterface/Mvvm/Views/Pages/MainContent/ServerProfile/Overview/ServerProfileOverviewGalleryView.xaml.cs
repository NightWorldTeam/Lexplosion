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

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.ServerProfile
{
    /// <summary>
    /// Interaction logic for ServerProfileOverviewGalleryView.xaml
    /// </summary>
    public partial class ServerProfileOverviewGalleryView : UserControl
    {
        public ServerProfileOverviewGalleryView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.Command.Execute(button.CommandParameter);
        }
    }
}
