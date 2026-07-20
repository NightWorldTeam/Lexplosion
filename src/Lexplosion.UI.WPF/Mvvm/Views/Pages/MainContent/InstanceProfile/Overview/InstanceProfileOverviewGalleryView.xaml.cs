using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.ServerProfile;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.InstanceProfile
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

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (DataContext is InstanceProfileOverviewGalleryViewModel vm)
			{
				// Call a safe, parameterless void method on your ViewModel
				await vm.Model.InitializeAsync();
			}
		}
	}
}
