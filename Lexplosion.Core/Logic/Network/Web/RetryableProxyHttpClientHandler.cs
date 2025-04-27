using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Logic.Objects;
using System.Collections.Concurrent;
using NightWorld.Collections.Concurrent;

namespace Lexplosion.Logic.Network.Web
{
	public class RetryableProxyHttpClientHandler : HttpClientHandler
	{
		private readonly ConcurrentHashSet<string> _proxyUrls = new();

		public RetryableProxyHttpClientHandler()
		{
			UseProxy = true;
			Proxy = null; // Будет устанавливаться динамически
		}

		public void AddProxy(string url)
		{
			_proxyUrls.Add(url);
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request.RequestUri == null)
				return await base.SendAsync(request, cancellationToken);

			var domain = request.RequestUri.Host;
			if (domain != "night-world.org" && domain != "api.curseforge.com" && domain != "curseforge.com")
				return await base.SendAsync(request, cancellationToken);

			HttpResponseMessage? lastResponse = null;
			Exception? lastException = null;

			foreach (string proxyUrl in _proxyUrls)
			{
				try
				{
					this.Proxy = new WebProxy(proxyUrl);
					lastResponse = await base.SendAsync(request, cancellationToken);

					// Если ответ успешный (2xx), возвращаем его
					if (lastResponse.IsSuccessStatusCode)
						return lastResponse;

					// Если прокси вернул ошибку (например, 403), пробуем следующий
					lastException = new HttpRequestException($"Proxy returned error: {lastResponse.StatusCode}");
				}
				catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
				{
					lastException = ex;
				}
			}

			// Если все попытки исчерпаны, выбрасываем исключение
			throw lastException ?? new HttpRequestException("Failed to connect after retries");
		}
	}
}
