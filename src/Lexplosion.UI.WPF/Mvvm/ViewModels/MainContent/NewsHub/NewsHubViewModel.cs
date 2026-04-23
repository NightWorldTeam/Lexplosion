using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.NewsHub
{
	public sealed class NewsHubViewModel : ViewModelBase
	{
		public NewsHubModel Model { get; }

		public NewsHubViewModel()
		{
			Model = new NewsHubModel();
		}
	}
}
