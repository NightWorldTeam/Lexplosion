using Lexplosion.UI.WPF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.NewsHub
{

    public sealed class NewsItemModel
    {
        public string Title { get; }
        public string Description { get; }
        public DateTime Date { get; }
        public string Author { get; }

        public NewsItemModel(string title, string description, DateTime date, string author)
        {
            Title = title;
            Description = description;
            Date = date;
            Author = author;
        }
    }

    public sealed class NewsHubViewModel : ViewModelBase
    {
        public List<NewsItemModel> Items { get; }

        public NewsHubViewModel()
        {
            Items = new List<NewsItemModel>
        {
            new NewsItemModel(
                "Ивент на ПОДПИСКИ",
                "24 мая будет конкурс среди игроков...",
                DateTime.Now,
                "NightWorld"),

            new NewsItemModel(
                "Обновление лаунчера",
                "Мы улучшили производительность...",
                DateTime.Now.AddDays(-1),
                "NightWorld"),

            new NewsItemModel(
                "Новая функция",
                "Добавлена поддержка модов...",
                DateTime.Now.AddDays(-2),
                "NightWorld"),

            new NewsItemModel(
                "Новая функция",
                "Добавлена поддержка модов...",
                DateTime.Now.AddDays(-3),
                "NightWorld"),

            new NewsItemModel(
                "Новая функция",
                "Добавлена поддержка модов...",
                DateTime.Now.AddDays(-4),
                "NightWorld"),
        };
        }
    }
}
