using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Notifications
{
    public class NotificationsManager
    {
        private readonly DataFilesManager _dataFilesManager;

        internal NotificationsManager(DataFilesManager dataFilesManager)
        {
            _dataFilesManager = dataFilesManager;
        }

        public List<News> GetAllNews(int page, int pageSize)
        {
            var result = new List<News>();
            result.Add(new News(_dataFilesManager)
            {
                Id = 0,
                Content = "ЕБАТЬСЯ ВРЕДНО! СПЕРМА ЯДОВИТА!",
                Summary = "Последние научные исследования"
            });

            return result;
        }

        /// <summary>
        /// Возвращает непросмотренные новости. Null если таких нет
        /// </summary>
        public List<News> GetUnseenNews()
        {
            var id = _dataFilesManager.GetLastViewedNewsId();
            if (id < -1) return null;

            return GetAllNews(0, 0);
        }
    }
}
