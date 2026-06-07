using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.NewsHub;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub
{
    public sealed class NewsHubModel : ObservableObject
    {
        public ObservableCollection<NewsPreviewModel> Items { get; } = [];

        public NewsHubModel(AppCore appCore)
        {
            var openNewsViewerCommand = new RelayCommand((parameter) =>
            {
                NewsPreviewModel model = (parameter as NewsPreviewModel)!;
                var previusViewModel = appCore.NavigationStore.CurrentViewModel;
                var backCommand = new NavigateCommand<ViewModelBase>(appCore.NavigationStore, () => previusViewModel);
                appCore.NavigationStore.CurrentViewModel = new NewsArticleViewModel(backCommand, model);
            });

            ThreadPool.QueueUserWorkItem((obj) =>
            {
                var items = Runtime.ServicesContainer.NotificationsService
                    .GetAllNews(0, 0)
                    .Select(i => new NewsPreviewModel(i.Title, i.Summary, i.Content, i.CreationDate, "No Author", i.BannerUrl ?? "", openNewsViewerCommand));

                appCore.UIThread.Invoke(() =>
                {
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }
                });
            });
        }
    }
}
