using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using Lexplosion.Global;

namespace Lexplosion.Logic.Network
{
	public class ToServer
	{
		private HttpClient _httpClient;

		public ToServer()
		{
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
		}

		public T ProtectedRequest<T>(string url) where T : ProtectedManifest
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
					string answer = HttpPost(url, data);

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

		public string HttpPost(string url, IDictionary<string, string> data = null, IDictionary<string, string> headers = null)
		{
			return HttpPostAsync(url, data, headers).Result;
		}

		public async Task<string> HttpPostAsync(string url, IDictionary<string, string> data = null, IDictionary<string, string> headers = null)
		{
			try
			{
				var content = data != null
						? new FormUrlEncodedContent(data)
						: new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>());

				var request = new HttpRequestMessage(HttpMethod.Post, url)
				{
					Content = content
				};

				request.Content.Headers.ContentType =
					new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

				AddHeaders(request, headers);

				var response = await _httpClient.SendAsync(request);

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
			try
			{
				string answer;

				WebRequest req = WebRequest.Create(url);
				((HttpWebRequest)req).UserAgent = "Mozilla/5.0";
				if (timeout > 0) req.Timeout = timeout;

				if (headers != null)
				{
					foreach (var header in headers)
					{
						req.Headers.Add(header.Key, header.Value);
					}
				}

				using (WebResponse resp = req.GetResponse())
				{
					using (Stream stream = resp.GetResponseStream())
					{
						using (StreamReader sr = new StreamReader(stream))
						{
							answer = sr.ReadToEnd();
						}
					}
				}

				return answer;
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite($"url: {url}, Exception: {ex}, stack trace: {new System.Diagnostics.StackTrace()}");
				return null;
			}
		}

		public async Task<string> HttpGetAsync(string url, IDictionary<string, string> headers = null, int timeout = 0)
		{
			try
			{
				var request = new HttpRequestMessage(HttpMethod.Get, url);

				AddHeaders(request, headers);
				HttpResponseMessage response;

				if (timeout > 0)
				{
					var cts = new CancellationTokenSource();
					cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
					response = await _httpClient.SendAsync(request, cts.Token);
				}
				else
				{
					response = await _httpClient.SendAsync(request);
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
			HttpStatusCode? httpStatus = null;

			try
			{
				var request = new HttpRequestMessage(HttpMethod.Post, url)
				{
					Content = new StringContent(data, Encoding.UTF8, "application/json")
				};

				// Добавляем заголовки
				AddHeaders(request, headers);

				var response = await _httpClient.SendAsync(request);
				httpStatus = response.StatusCode;
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
