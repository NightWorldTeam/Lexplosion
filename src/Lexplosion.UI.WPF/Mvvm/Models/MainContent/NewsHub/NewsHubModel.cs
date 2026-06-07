using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.NewsHub;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub
{
    public sealed class NewsHubModel : ObservableObject
    {
        public List<NewsPreviewModel> Items { get; }

        public NewsHubModel(AppCore appCore)
        { 
            var openNewsViewerCommand = new RelayCommand((parameter) =>
            {
                Console.WriteLine("12312312312312312");
                NewsPreviewModel model = (parameter as NewsPreviewModel)!;
                var previusViewModel = appCore.NavigationStore.CurrentViewModel;
                var backCommand = new NavigateCommand<ViewModelBase>(appCore.NavigationStore, () => previusViewModel);
                appCore.NavigationStore.CurrentViewModel = new NewsArticleViewModel(backCommand, model);
            });

            Items = Runtime.ServicesContainer.NwApi.GetNews()
                .Select(i => new NewsPreviewModel(i.Title, i.Summary, i.CreationDate, "No Author", openNewsViewerCommand))
                .ToList();

            for (var i = 0; i < 10; i++)
            {
                Items.Add(

                    new NewsPreviewModel(
                    "Ивент на ПОДПИСКИ",
                    "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                    DateTime.Now.AddDays(-i),
                    "NightWorld", openNewsViewerCommand)
                );
            }
        }
    }
}
