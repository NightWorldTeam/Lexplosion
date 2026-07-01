using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.NewsHub;
using Markdig;
using MarkdownWPF.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.NewsHub
{
    public sealed class NewsArticleModel : ObservableObject
    {
        public MarkdownPipeline HtmlPipeline { get; }

        private string _context;
        public string Context
        {
            get => _context; set
            {
                _context = value;
                OnPropertyChanged();
            }
        }

        public NewsArticleModel(NewsPreviewModel newsModel)
        {
            HtmlPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseWpfHtml()
                .Build();

            Context = newsModel.Context;
        }
    }

    public sealed class NewsArticleViewModel : ViewModelBase
    {
        public NewsArticleModel Model { get; }
        public ICommand BackCommand { get; }

        public NewsArticleViewModel(ICommand backCommand, NewsPreviewModel newsModel)
        {
            Model = new NewsArticleModel(newsModel);
            BackCommand = backCommand;
        }
    }
}
