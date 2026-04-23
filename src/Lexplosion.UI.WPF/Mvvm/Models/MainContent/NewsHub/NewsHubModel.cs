using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub
{
    public sealed class NewsHubModel : ObservableObject
    {
        public List<NewsPreviewModel> Items { get; }

        public NewsHubModel()
        {
            Items = new List<NewsPreviewModel>();

            for (var i = 0; i < 10; i++)
            {
                Items.Add(

                    new NewsPreviewModel(
                    "Ивент на ПОДПИСКИ",
                    "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                    DateTime.Now.AddDays(-i),
                    "NightWorld")
                );
            }
        }
    }
}
