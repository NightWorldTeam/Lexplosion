using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.Nightworld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Notifications
{
    public class News : NewsModel
    {
        private static object _fileLoacker = new object();
        private readonly DataFilesManager _dataFilesManager;

        public string? BannerUrl { get; set; } = null;

        [JsonIgnore]
        public bool IsViewed { get; private set; }

        public void MarkAsViewed()
        {
            lock (_fileLoacker)
            {
                var id = _dataFilesManager.GetLastViewedNewsId();
                if (Id > id)
                {
                    _dataFilesManager.SaveLastViewedNewsId(Id);
                }

                IsViewed = true;
            }
        }

        internal News(NewsModel model, DataFilesManager dataFilesManager, bool isViewed)
        {
            _dataFilesManager = dataFilesManager;
            IsViewed = isViewed;

            Id = model.Id;
            Title = model.Title;
            Summary = model.Summary;
            Content = model.Content;
            DateUnix = model.DateUnix;
        }
    }
}
