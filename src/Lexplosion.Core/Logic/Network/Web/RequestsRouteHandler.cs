using Lexplosion.Logic.Network.Web.Models;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network.Web
{
    internal class RequestsRouteHandler : DelegatingHandler
    {
        public RequestsRouteHandler() : base() { }

        private Dictionary<string, DomainRouteData> _routeData = new();
        private List<(double, HttpClient)> _fallbackClients = [];
        private Dictionary<Proxy, HttpClient> _proxies = new();
        private string _defaultUserAgent;

        public RequestsRouteHandler(string userAgent, int maxConnectionsPerServer)
        {
            _defaultUserAgent = userAgent;
            InnerHandler = new HttpClientHandler
            {
                UseProxy = false,
                MaxAutomaticRedirections = maxConnectionsPerServer
            };
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddRouteData(DomainRouteData routeData)
        {
            _routeData[routeData.Domain] = routeData;
        }

        protected async Task<HttpResponseMessage> SendThroughProxy(DomainRouteData routeData, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Exception lastException = null;

            foreach (var proxy in routeData.Proxies)
            {
                try
                {
                    var client = _proxies[proxy];
                    var clonedRequest = await CloneHttpRequestAsync(request);
                    var response = await client.SendAsync(clonedRequest, cancellationToken);

                    return response;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    routeData.ProxyFailed(proxy);
                    Runtime.DebugWrite($"Send with proxy error {request.RequestUri} {ex}");
                }
            }

            Runtime.DebugWrite("All proxies failed");
            throw lastException ?? new HttpRequestException("All proxies failed");
        }

        protected Task<HttpResponseMessage> SendThroughMirror(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Host = "mirror.night-world.org"
            };

            if (request.RequestUri.Host.Equals("night-world.org", StringComparison.OrdinalIgnoreCase))
            {
                request.RequestUri = uriBuilder.Uri;
            }
            else
            {
                uriBuilder.Path = $"/mirror/{request.RequestUri.Host}{uriBuilder.Path}";
                request.RequestUri = uriBuilder.Uri;
            }

            return base.SendAsync(request, cancellationToken);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri == null || !_routeData.TryGetValue(request.RequestUri.Host, out DomainRouteData routeData))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            if (routeData.RouteMethod == DomainRouteMethod.Proxy)
            {
                return await SendThroughProxy(routeData, request, cancellationToken);
            }

            if (routeData.RouteMethod == DomainRouteMethod.Mirror)
            {
                return await SendThroughMirror(request, cancellationToken);
            }

            return await base.SendAsync(request, cancellationToken);

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
    }
}
