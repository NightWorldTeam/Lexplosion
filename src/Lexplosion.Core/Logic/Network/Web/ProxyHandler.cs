using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network.Web
{
    public class ProxyHandler : DelegatingHandler
    {
        private List<(double, HttpClient)> _fallbackClients = [];
        private string _defaultUserAgent;

        public ProxyHandler(string userAgent)
        {
            _defaultUserAgent = userAgent;
            InnerHandler = new HttpClientHandler
            {
                UseProxy = false
            };
        }

        public void AddProxy(Proxy proxy)
        {
            lock (_fallbackClients)
            {
                var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxy.Url),
                    UseProxy = true
                };

                var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent", _defaultUserAgent);

                var newFallbackClients = new List<(double, HttpClient)>(_fallbackClients);
                newFallbackClients.Add((proxy.CalculatedDelay, client));

                newFallbackClients.Sort(((double, HttpClient) x, (double, HttpClient) y) =>
                {
                    if (x.Item1 < y.Item1) return -1;
                    if (x.Item1 > y.Item1) return 1;
                    return 0;
                });

                _fallbackClients = newFallbackClients;
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri == null)
                return await base.SendAsync(request, cancellationToken);

            var domain = request.RequestUri.Host;
            if (domain != "night-world.org" && domain != "api.curseforge.com" && domain != "curseforge.com")
                return await base.SendAsync(request, cancellationToken);

            HttpResponseMessage lastResponse = null;
            Exception lastException = null;

            List<(double, HttpClient)> clients = _fallbackClients;
            foreach (var clientData in clients)
            {
                try
                {
                    var client = clientData.Item2;
                    var clonedRequest = await CloneHttpRequestAsync(request);
                    lastResponse = await client.SendAsync(clonedRequest, cancellationToken);

                    if (lastResponse.IsSuccessStatusCode)
                        return lastResponse;

                    Runtime.DebugWrite($"send with proxy error {request.RequestUri} {lastResponse.StatusCode}");
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Runtime.DebugWrite($"send with proxy error {request.RequestUri} {ex}");
                }
            }

            Runtime.DebugWrite("All proxies failed");
            throw lastException ?? new HttpRequestException("All proxies failed");
        }

        private async Task<HttpRequestMessage> CloneHttpRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = await CloneHttpContentAsync(request.Content).ConfigureAwait(false),
                Version = request.Version
            };
            foreach (KeyValuePair<string, object> prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        private async Task<HttpContent> CloneHttpContentAsync(HttpContent content)
        {
            if (content == null) return null;

            var ms = new MemoryStream();
            await content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;

            var clone = new StreamContent(ms);
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }
            return clone;
        }

        //var handler = new HttpClientHandler
        //{
        //	ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
        //	{
        //		// 1. Явно выполняем стандартную проверку
        //		bool isChainValid = chain.Build(cert); // Проверяет цепочку и срок действия

        //		// 2. Анализируем ошибки (как это делает .NET)
        //		bool isStandardValid = isChainValid && sslPolicyErrors == SslPolicyErrors.None;

        //		// 3. Кастомная проверка (например, по отпечатку)
        //		bool isCustomValid = cert.GetCertHashString() == "A1B2C3D4E5...";

        //		return isStandardValid || isCustomValid;
        //	}
        //};

    }

}
