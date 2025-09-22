using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Threading;
using System.Linq;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Network.Web;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Messaging;

namespace Lexplosion.Logic.Network
{
	public class ToServer
	{
		private HttpClient _httpClient;
		private ProxyHandler _clientHandler;

		private const string USER_AGENT = "Mozilla/5.0 Lexplosion/1.0.1.1";

		public bool IsMirrorModeToNw { get; private set; } = false;

		internal ToServer()
		{
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ChangeToProxyMode()
		{
			_clientHandler = new ProxyHandler(USER_AGENT);
			var newHttpClient = new HttpClient(_clientHandler);
			newHttpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
			_httpClient = newHttpClient;
		}

		public void AddProxy(Proxy proxy)
		{
			_clientHandler.AddProxy(proxy);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ChangeToMirrorMode()
		{
			if (!IsMirrorModeToNw)
			{
				Runtime.DebugWrite("Enable mirror mode");
				var newHttpClient = new HttpClient(new RedirectToMirrorHandler());
				newHttpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
				_httpClient = newHttpClient;

				IsMirrorModeToNw = true;
			}
		}

		public T ProtectedRequest<T>(string url, int timeout = 0) where T : ProtectedManifest
		{
			Random rnd = new Random();

			string str = rnd.GenerateString(32);
			string str2 = rnd.GenerateString(32);

			using (SHA1 sha = new SHA1Managed())
			{
				string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

				int d = 32 - key.Length;
				for (int i = 0; i < d; i++)
				{
					key += str2[i];
				}

				Dictionary<string, string> data = new Dictionary<string, string>()
				{
					["str"] = str,
					["str2"] = str2,
					["code"] = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord)))
				};

				try
				{
					Runtime.DebugWrite(url);
					string answer = HttpPost(url, data, timeout: timeout);

					if (answer != null && answer != "")
					{
						byte[] IV = Encoding.UTF8.GetBytes(str.Substring(0, 16));
						byte[] decripted = Cryptography.AesDecode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), IV);
						answer = Encoding.UTF8.GetString(decripted);

						T filesData = JsonConvert.DeserializeObject<T>(answer);
						if (filesData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(filesData.str + ":" + LaunсherSettings.secretWord))))
						{
							return filesData;
						}
						else
						{
							return default;
						}
					}
					else
					{
						return default;
					}
				}
				catch (Exception ex)
				{
					Runtime.DebugWrite(ex);
					return default;
				}
			}
		}

		/// <summary>
		/// Защищенный запрос для передачи данных пользователя (регистрация, авторизация).
		/// </summary>
		/// <typeparam name="T">Возвращаемый тип</typeparam>
		/// <param name="url">url</param>
		/// <param name="data">Данные</param>
		/// <param name="errorResult">Если сервер вернул ошибку, то ее код будет помещен сюда. В случае успеха будет null </param>
		/// <returns>Вернет базовое значение при ошибке, поместив код ошибки в errorResult (если он есть).</returns>
		public T ProtectedUserRequest<T>(string url, string data, out string errorResult) where T : ProtectedManifest
		{
			Random rnd = new Random();

			string str = rnd.GenerateString(32);
			string str2 = rnd.GenerateString(32);
			string salt = rnd.GenerateString(32);

			using (SHA1 sha = new SHA1Managed())
			{
				string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

				int d = 32 - key.Length;
				for (int i = 0; i < d; i++)
				{
					key += str2[i];
				}

				data = Convert.ToBase64String(Encoding.UTF8.GetBytes(data)) + ":" + str;
				string planText = Convert.ToBase64String(Encoding.UTF8.GetBytes(data)) + ":" + salt;
				byte[] encrypted = Cryptography.AesEncode(planText, Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));

				var fullData = new Dictionary<string, string>()
				{
					["data"] = Convert.ToBase64String(encrypted),
					["str"] = str,
					["str2"] = str2,
					["code"] = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord)))
				};

				string answer;
				try
				{
					answer = HttpPost(url, fullData);
					Runtime.DebugWrite(answer);

					if (answer == null)
					{
						errorResult = null;
						return default;
					}
					else if (answer.StartsWith("ERROR:")) // если результат является кодом ошибки, то возращаем его не декодируя
					{
						errorResult = answer;
						return default;
					}
					else
					{
						byte[] IV = Encoding.UTF8.GetBytes(str.Substring(0, 16));
						byte[] decripted = Cryptography.AesDecode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), IV);
						answer = Encoding.UTF8.GetString(decripted);
						T answerData = JsonConvert.DeserializeObject<T>(answer);

						if (answerData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(answerData.str + ":" + LaunсherSettings.secretWord))))
						{
							errorResult = null;
							return answerData;
						}
						else
						{
							errorResult = null;
							return default;
						}
					}
				}
				catch
				{
					errorResult = null;
					return default;
				}
			}
		}

		public string HttpPost(string url, IDictionary<string, string> data = null, IDictionary<string, string> headers = null, int timeout = 0)
		{
			return HttpPostAsync(url, data, headers, timeout).Result;
		}

		public async Task<string> HttpPostAsync(string url, IDictionary<string, string> data = null, IDictionary<string, string> headers = null, int timeout = 0)
		{
			Runtime.DebugWrite($"Request url: {url}");

			var httpClient = _httpClient;
			HttpResponseMessage response;
			try
			{
				var content = data != null
						? new FormUrlEncodedContent(data)
						: new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>());

				var request = new HttpRequestMessage(HttpMethod.Post, url)
				{
					Content = content
				};

				request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				AddHeaders(request, headers);

				if (timeout > 0)
				{
					var cts = new CancellationTokenSource();
					cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
					response = await httpClient.SendAsync(request, cts.Token);
				}
				else
				{
					response = await httpClient.SendAsync(request);
				}

				if (!response.IsSuccessStatusCode) return null;

				return await response.Content.ReadAsStringAsync();
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite($"url: {url}, Exception: {ex}, stack trace: {new System.Diagnostics.StackTrace()}");
				return null;
			}
		}

		public string HttpGet(string url, Dictionary<string, string> headers = null, int timeout = 0)
		{
			var task = Task.Run(() => HttpGetAsync(url, headers, timeout));
			task.Wait();
			return task.Result;
		}

		public async Task<string> HttpGetAsync(string url, IDictionary<string, string> headers = null, int timeout = 0)
		{
			Runtime.DebugWrite($"Request url: {url}");

			var httpClient = _httpClient;
			try
			{
				var request = new HttpRequestMessage(HttpMethod.Get, url);

				AddHeaders(request, headers);
				HttpResponseMessage response;

				if (timeout > 0)
				{
					var cts = new CancellationTokenSource();
					cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
					response = await httpClient.SendAsync(request, cts.Token);
				}
				else
				{
					response = await httpClient.SendAsync(request);
				}

				response.EnsureSuccessStatusCode();

				return await response.Content.ReadAsStringAsync();
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite($"url: {url}, Exception: {ex}, stack trace: {new System.Diagnostics.StackTrace()}");
				return null;
			}
		}

		public bool IsHtmlPage(string url)
		{
			var request = HttpWebRequest.Create(url);
			request.Method = "HEAD";

			return (request.GetResponse().ContentType.StartsWith("text/html"));
		}

		public string HttpPostJson(string url, string data, out HttpStatusCode? httpStatus, Dictionary<string, string> headers = null)
		{
			var result = HttpPostJsonAsync(url, data, headers).ConfigureAwait(false).GetAwaiter().GetResult();

			httpStatus = result.Item2;
			return result.Item1;
		}

		public async Task<(string, HttpStatusCode?)> HttpPostJsonAsync(string url, string data, IDictionary<string, string> headers = null)
		{
			Runtime.DebugWrite($"Request url: {url}");
			HttpStatusCode? httpStatus = null;

			var httpClient = _httpClient;
			try
			{
				var request = new HttpRequestMessage(HttpMethod.Post, url)
				{
					Content = new StringContent(data, Encoding.UTF8, "application/json")
				};

				// Добавляем заголовки
				AddHeaders(request, headers);

				var response = await httpClient.SendAsync(request);
				httpStatus = response.StatusCode;
				if (!response.IsSuccessStatusCode) return (null, httpStatus);

				string result = await response.Content.ReadAsStringAsync();

				return (result, httpStatus);
			}
			catch (HttpRequestException ex)
			{
				if (ex.Data.Contains("StatusCode"))
				{
					httpStatus = (HttpStatusCode)ex.Data["StatusCode"];
				}
			}
			catch (WebException ex)
			{
				if (ex.Response is HttpWebResponse httpResponse)
				{
					httpStatus = httpResponse.StatusCode;
				}
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite($"url: {url}, Exception: {ex}, stack trace: {new System.Diagnostics.StackTrace()}");
			}

			return (null, httpStatus);
		}

		public byte[] LoadCertificate(string url)
		{
			var task = Task.Run(() => LoadCertificateAsync(url));
			task.Wait();
			return task.Result;
		}

		public async Task<byte[]> LoadCertificateAsync(string url)
		{
			Runtime.DebugWrite($"Load certificate for {url}");

			try
			{
				X509Certificate2 ServerCertificate = null;

				var handler = new MirrorHttpHandler();

				handler.ValidCertificateHandler += (X509Certificate2 cert) =>
				{
					ServerCertificate = cert;
				};

				using (var httpClient = new HttpClient(handler))
				{
					httpClient.Timeout = TimeSpan.FromMilliseconds(5000);

					var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

					if (ServerCertificate != null)
					{
						byte[] certData = ServerCertificate.Export(X509ContentType.Cert);
						return certData;
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
				return null;
			}
		}

		private void AddHeaders(HttpRequestMessage request, IDictionary<string, string> headers)
		{
			if (headers != null)
			{
				foreach (var header in headers)
				{
					if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
					{
						request.Content.Headers.ContentType = new MediaTypeHeaderValue(header.Value);
					}
					else
					{
						if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
						{
							request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
						}
					}
				}
			}
		}

	}
}
