using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.ServerProfile;
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

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.ServerProfile
{
    /// <summary>
    /// Interaction logic for ServerProfileOverviewGalleryView.xaml
    /// </summary>
    public partial class InstanceProfileOverviewGalleryView : UserControl
    {
        public InstanceProfileOverviewGalleryView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.Command.Execute(button.CommandParameter);
        }

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (DataContext is ServerProfileOverviewGalleryViewModel vm)
			{
				// Call a safe, parameterless void method on your ViewModel
				vm.Model.Initialize();
			}
		}
    }
}
