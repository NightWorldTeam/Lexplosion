using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Notifications
{
	public class NotificationsManager
	{
		private readonly INightWorldFileServicesContainer _services;

		public long LatestNewsId { get; internal set; } = 0;

		internal NotificationsManager(INightWorldFileServicesContainer services)
		{
			_services = services;
		}

		public CatalogResult<News> GetAllNews(int page, int pageSize)
		{
			long lastViewedNewsId = _services.DataFilesService.GetLastViewedNewsId();
			var res = _services.NwApi.GetNews();

			var news = res.Select(x => new News(x, _services.DataFilesService, x.Id <= lastViewedNewsId)).ToList();
			return new CatalogResult<News>(news, res.Count);
		}

		/// <summary>
		/// Возвращает непросмотренные новости.
		/// </summary>
		public List<News> GetUnseenNews()
		{
			long id = _services.DataFilesService.GetLastViewedNewsId();
			if (id == LatestNewsId) return new();

			if (id >= LatestNewsId)
			{
				_services.DataFilesService.SaveLastViewedNewsId(LatestNewsId);
				return new();
			}

			var news = _services.NwApi.GetUnseenNews(id);

			return news.Select(x => new News(x, _services.DataFilesService, x.Id <= id)).ToList();
		}
	}
}
