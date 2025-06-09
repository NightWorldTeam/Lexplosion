using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network.Web
{
	internal class RedirectToMirrorHandler : HttpClientHandler
	{
		private const string MIRROR_CERT_SHA1 = "8DDAD6C33EFAFD9D1F5FB17A322159F1D71F9C73";
		public RedirectToMirrorHandler()
		{
			ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
			{
				if (message.RequestUri.Host.Equals("mirror.night-world.org", StringComparison.OrdinalIgnoreCase))
				{
					return cert != null && cert.Thumbprint?.Equals(MIRROR_CERT_SHA1, StringComparison.OrdinalIgnoreCase) == true;
				}

				return errors == System.Net.Security.SslPolicyErrors.None;
			};
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request.RequestUri.Host.Equals("night-world.org", StringComparison.OrdinalIgnoreCase))
			{
				var builder = new UriBuilder(request.RequestUri)
				{
					Host = "mirror.night-world.org"
				};
				request.RequestUri = builder.Uri;
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}
