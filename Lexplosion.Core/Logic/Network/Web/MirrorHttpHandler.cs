using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network.Web
{
	internal class MirrorHttpHandler : HttpClientHandler
	{
		private const string MIRROR_CERT_SHA1 = "8DDAD6C33EFAFD9D1F5FB17A322159F1D71F9C73";

		public event Action<X509Certificate2> ValidCertificateHandler;

		public MirrorHttpHandler()
		{
			ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
			{
				if (message.RequestUri.Host.Equals("mirror.night-world.org", StringComparison.OrdinalIgnoreCase))
				{
					if (cert != null && cert.Thumbprint?.Equals(MIRROR_CERT_SHA1, StringComparison.OrdinalIgnoreCase) == true)
					{
						ValidCertificateHandler?.Invoke(new X509Certificate2(cert));
						return true;
					}

					return false;
				}

				if (errors == System.Net.Security.SslPolicyErrors.None && cert != null)
				{
					ValidCertificateHandler?.Invoke(new X509Certificate2(cert));
				}

				return errors == System.Net.Security.SslPolicyErrors.None;
			};
		}
	}
}
