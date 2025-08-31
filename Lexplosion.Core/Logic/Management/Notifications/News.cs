using Lexplosion.Logic.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Notifications
{
    public class News
    {
        private static object _fileLoacker = new object();
        private readonly DataFilesManager _dataFilesManager;

        public long Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string? BannerUrl { get; set; } = null;
        public string Content { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Today;

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

        internal News(DataFilesManager dataFilesManager)
        {
            _dataFilesManager = dataFilesManager;
        }
    }
}
