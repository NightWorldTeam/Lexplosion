using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.NewsHub
{
	public sealed class NewsHubViewModel : ViewModelBase
	{
		public NewsHubModel Model { get; }
		public ICommand BackCommand { get; }


        public NewsHubViewModel(AppCore appCore, ICommand backCommand)
		{
			Model = new NewsHubModel(appCore);
			BackCommand = backCommand;
        }
	}
}
