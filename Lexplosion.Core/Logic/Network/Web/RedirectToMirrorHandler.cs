using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network.Web
{
	internal class RedirectToMirrorHandler : MirrorHttpHandler
	{
		public RedirectToMirrorHandler() : base() { }

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
