using Lexplosion.Global;
using Lexplosion.Logic.FileSystem.Services;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.Logic.Management.Notifications
{
    public class NotificationsManager
    {
        private readonly INightWorldFileServicesContainer _services;

        internal NotificationsManager(INightWorldFileServicesContainer services)
        {
            _services = services;
        }

        public List<News> GetAllNews(int page, int pageSize)
        {
            var result = new List<News>();
            return result;
        }

        /// <summary>
        /// Возвращает непросмотренные новости.
        /// </summary>
        public List<News> GetUnseenNews()
        {
            var id = _services.DataFilesService.GetLastViewedNewsId();

            GlobalData.LastNewsId = 0; // TODO: временная херь. Потом починить
            if (id >= GlobalData.LastNewsId) return new();

            var news = _services.NwApi.GetUnseenNews(id);

            return news.Select(x => new News(x, _services.DataFilesService, x.Id <= id)).ToList();
        }
    }
}
