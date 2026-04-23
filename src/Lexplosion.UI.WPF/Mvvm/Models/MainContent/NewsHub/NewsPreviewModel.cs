using Lexplosion.UI.WPF.Core.ViewModel;
using System;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub
{
    public sealed class NewsPreviewModel
    {
        public string Title { get; }
        public string Description { get; }
        public DateTime Date { get; }
        public string Author { get; }

        public NewsPreviewModel(string title, string description, DateTime date, string author)
        {
            Title = title;
            Description = description;
            Date = date;
            Author = author;
        }
    }
}
