using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub
{
    public sealed class NewsPreviewModel
    {
        public string Title { get; }
        public string Description { get; }
        public DateTime Date { get; }
        public string Author { get; }
        public ICommand OpenNewsViewerCommand { get; }

        public NewsPreviewModel(string title, string description, DateTime date, string author, ICommand openViewerCommand)
        {
            Title = title;
            Description = description;
            Date = date;
            Author = author;
            OpenNewsViewerCommand = openViewerCommand;
        }
    }
}
