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

		/// <summary>
		/// Если True то значит id последней новости получить с сервера не удалось 
		/// и значение <see cref="LatestNewsId"/> недействительно
		/// </summary>
		public bool LatestNewsIdUnknown { get; private set; } = false;
		public long LatestNewsId { get; private set; } = 0;

		internal NotificationsManager(INightWorldFileServicesContainer services)
		{
			_services = services;
		}

		/// <summary>
		/// Устанавливает id последней новости в поле <see cref="LatestNewsId"/>. 
		/// Передача null означает что id последней новости определить не удалось,
		/// в этом случае <see cref="LatestNewsIdUnknown"/> будет утсановлен на true
		/// </summary>
		/// <param name="latestNewsId">id последней новости или null если его определить не удалось.</param>
		public void SetLatestNewsId(long? latestNewsId)
		{
			if (latestNewsId == null)
			{
				LatestNewsIdUnknown = true;
				return;
			}

			LatestNewsId = latestNewsId.Value;
		}

		public CatalogResult<News> GetAllNews(int page, int pageSize)
		{
			long lastViewedNewsId = _services.DataFilesService.GetLastViewedNewsId();
			var res = _services.NwApi.GetNews();

			var news = res.Select(x => new News(x, _services.DataFilesService, x.Id <= lastViewedNewsId)).ToList();
			return new CatalogResult<News>(news, res.Count);
		}

		/// <summary>
		/// Возвращает последнюю непросмотренную новость. null - непросмотренных новостей нет
		/// </summary>
		public News? GetLastUnseenNews()
		{
			// если мы не смогли получить айдишник последней новости с сервера, то ничего не делаем
			if (LatestNewsIdUnknown) return null;

			long id = _services.DataFilesService.GetLastViewedNewsId();
			if (id == LatestNewsId) return null;

			if (id >= LatestNewsId)
			{
				_services.DataFilesService.SaveLastViewedNewsId(LatestNewsId);
				return null;
			}

			var news = _services.NwApi.GetUnseenNews(id);

			News? lastNews = null;
			foreach (var item in news)
			{
				if ((lastNews == null || item.Id > lastNews.Id) && item.Id <= LatestNewsId)
				{
					//по сути item.Id <= id тут писать бессмысленно, оно всегда будет false, ибо 
					//NwApi.GetUnseenNews должен возвращать новости, айдишник которых больше чем id
					//но мало ли, пусть будет
					lastNews = new News(item, _services.DataFilesService, item.Id <= id);
				}
			}

			return lastNews;
		}
	}
}
