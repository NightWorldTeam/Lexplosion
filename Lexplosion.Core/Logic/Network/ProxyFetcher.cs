using Lexplosion.Tools;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using System.Diagnostics;

namespace Lexplosion.Logic.Network
{
	public class ProxyFetcher
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5); // Таймаут проверки

		public static List<Proxy> GetProxies()
		{
			var task = Task.Run(() => GetProxiesAsync());
			task.Wait();
			return task.Result;
		}

		public static async Task<List<Proxy>> GetProxiesAsync()
		{
			var proxies = new List<Proxy>();

			try
			{
				var geonodeUrl = "https://proxylist.geonode.com/api/proxy-list?google=true&limit=120&page=1&sort_by=lastChecked&sort_type=desc";
				var geonodeResponse = await _httpClient.GetStringAsync(geonodeUrl);
				proxies.AddRange(ParseGeonodeProxies(geonodeResponse));

				var proxyListUrl = "https://www.proxy-list.download/api/v1/get?type=http";
				var proxyListResponse = await _httpClient.GetStringAsync(proxyListUrl);
				proxies.AddRange(ParseProxyListDownload(proxyListResponse));

				var freeProxyListUrl = "https://free-proxy-list.net/";
				var freeProxyListResponse = await _httpClient.GetStringAsync(freeProxyListUrl);
				proxies.AddRange(ParseFreeProxyList(freeProxyListResponse));
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite($"Ошибка при получении прокси: {ex.Message}");
			}

			return proxies;
		}

		public static void FindWorkingProxy(List<Proxy> proxies, Action<Proxy> findedWorkingProxy, Action end)
		{
			new Thread(() =>
			{
				Runtime.DebugWrite($"Поиск рабочих прокси среди {proxies.Count} штук");
				int maxProxiesToCheck = proxies.Count < 120 ? proxies.Count : 120;
				int workingProxies = 0;

				var perfomer = new TasksPerfomer(15, maxProxiesToCheck);

				foreach (Proxy proxy in proxies)
				{
					if (maxProxiesToCheck-- <= 0 && workingProxies < 5) break;

					perfomer.ExecuteTask(() =>
					{
						var prx = proxy;
						var res = CheckWorking(prx).Result;
						if (res != -1)
						{
							prx.CalculatedDelay = res;
							findedWorkingProxy(prx);
							workingProxies++;
						}
					});
				}

				perfomer.WaitEnd();

				end();
			}).Start();
		}

		private static async Task<double> CheckWorking(Proxy proxy)
		{
			try
			{
				var proxyUri = new Uri($"http://{proxy.IP}:{proxy.Port}");
				var handler = new HttpClientHandler
				{
					Proxy = new WebProxy(proxyUri),
					UseProxy = true,
				};

				using (var testClient = new HttpClient(handler))
				{
					testClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
					testClient.Timeout = _timeout;

					var startTime = new Stopwatch();
					startTime.Start();
					var response = await testClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, LaunсherSettings.URL.Base));
					startTime.Stop();

					var requestTime = startTime.Elapsed;

					if (response.IsSuccessStatusCode)
					{
						Runtime.DebugWrite($"Найден рабочий прокси: {proxy.IP}:{proxy.Port}");
						return requestTime.TotalMilliseconds;
					}

					Runtime.DebugWrite($"Proxy error {response.StatusCode}");
				}
			}
			catch { }

			return -1;
		}

		private static List<Proxy> ParseFreeProxyList(string htmlResponse)
		{
			var proxies = new List<Proxy>();

			var rows = htmlResponse.Split(new[] { "<tr>", "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var row in rows)
			{
				if (row.Contains("<td>") && row.Contains("</td>"))
				{
					var cols = row.Split(new[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries);
					if (cols.Length >= 7)
					{
						var ip = cols[0].Trim();
						var port = cols[1].Trim();

						if (int.TryParse(port, out var portNum))
						{
							proxies.Add(new Proxy
							{
								IP = ip,
								Port = portNum
							});
						}
					}
				}
			}

			return proxies;
		}

		private static List<Proxy> ParseGeonodeProxies(string jsonResponse)
		{
			var proxies = new List<Proxy>();
			var data = JsonConvert.DeserializeObject<JObject>(jsonResponse);

			if (data?["data"] is JArray proxyList)
			{
				foreach (var proxy in proxyList)
				{
					var ip = proxy["ip"]?.ToString();
					var port = proxy["port"]?.ToString();
					var protocols = proxy["protocols"] as JArray;
					var country = proxy["country"]?.ToString();
					var ping = proxy["latency"]?.Value<int>() ?? -1;

					if (ip != null && int.TryParse(port, out var portNum) && protocols != null)
					{
						foreach (var protocol in protocols)
						{
							proxies.Add(new Proxy
							{
								IP = ip,
								Port = portNum
							});
						}
					}
				}
			}

			return proxies;
		}

		private static List<Proxy> ParseProxyListDownload(string rawText)
		{
			var proxies = new List<Proxy>();
			var lines = rawText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				var parts = line.Split(':');
				if (parts.Length == 2 && int.TryParse(parts[1], out var port))
				{
					proxies.Add(new Proxy
					{
						IP = parts[0],
						Port = port
					});
				}
			}

			return proxies;
		}
	}
}
