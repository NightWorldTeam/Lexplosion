using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub
{
    public sealed class NewsPreviewModel
    {
        public string Title { get; }
        public string Summary { get; }
        public string Context { get; }
        public DateTime Date { get; }
        public string Author { get; }
        public string BannerUrl { get; }
        public ICommand OpenNewsViewerCommand { get; }

        public NewsPreviewModel(string title, string summary, string context, DateTime date, string author, string bannerUrl, ICommand openViewerCommand)
        {
            Title = title;
            Summary = summary;
            Context = context;
            Date = date;
            Author = author;
            BannerUrl = bannerUrl;
            OpenNewsViewerCommand = openViewerCommand;
        }
    }
}
