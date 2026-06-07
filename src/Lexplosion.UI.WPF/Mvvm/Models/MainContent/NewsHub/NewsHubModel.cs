using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.NewsHub;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
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

                var _httpClient = new HttpClient();

                try
                {
                    var url = $"https://api.modrinth.com/v2/project/NNAgCjsB";
                    var jsonString = _httpClient.GetStringAsync(url).Result;

                    JObject data = JObject.Parse(jsonString);
                    var title = data["title"]?.ToString() ?? "No content";
                    var description = data["description"]?.ToString() ?? "No content";
                    var content = data["body"]?.ToString() ?? "No content";

                    appCore.UIThread.Invoke(() =>
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            Items.Add(

                                new NewsPreviewModel(
                                title,
                                description,
                                content,
                                DateTime.Now.AddDays(-i),
                                "NightWorld",
                                "https://sun9-10.userapi.com/s/v1/ig2/ERZt9ooukow1cW71oD6ccG8bu1Wvixewjg3aOuNniYXnaYXU7nE3qElrehQpFMewL8_KD9zqAULDTJA_A5NKRggB.jpg?quality=95&as=32x18,48x27,72x40,108x60,160x89,240x134,360x201,480x268,540x302,640x358,720x403,1080x604,1180x660&from=bu&u=eY0hDzVZtylJDPNTDrIbCfSMMi-R-detyinFiWICAS8&cs=1180x0",
                                openNewsViewerCommand)
                            );
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching data: {ex.InnerException?.Message ?? ex.Message}");
                }
            });
        }
    }
}
