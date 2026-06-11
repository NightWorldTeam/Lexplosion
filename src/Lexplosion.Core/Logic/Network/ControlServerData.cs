using System.Linq;
using System;
using System.Net;
using System.Net.Sockets;

namespace Lexplosion.Logic.Network
{
	struct ControlServerData
	{
		public readonly IPEndPoint HandshakeServerPoint;
		public readonly IPEndPoint TurnPoint;
		public readonly IPEndPoint SmpProxyPoint;

		public const int HandshakeServerPort = 4465;
		public const int TurnServerPort = 9765;
		public const int SmpProxyAPort = 9775;
		public const int SmpProxyBPort = 9776;

		public readonly (string, int)[] StunServers = new (string, int)[]
		{
			new ("stun.l.google.com", 19305),
			new ("stun.night-world.org", 3478),
			new ("stun.webcalldirect.com", 3478)
		};

		/// <param name="serverAddr">IP адрес или домен сервера</param>
		/// <param name="useASmpProxy">Если true, то для SmpProxy будет установлен порт SmpProxyAPort, иначе SmpProxyBPort</param>
		public ControlServerData(string serverAddr, bool useASmpProxy)
		{
			if (!IPAddress.TryParse(serverAddr, out IPAddress ipAddress))
			{
				try
				{
					var addresses = Dns.GetHostEntry(serverAddr).AddressList;
					ipAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
				}
				catch { }

				if (ipAddress == null)
				{
					ipAddress = IPAddress.Parse("83.147.192.203");
				}
			}

			HandshakeServerPoint = new IPEndPoint(ipAddress, 4465);
			TurnPoint = new IPEndPoint(ipAddress, 9765);
			SmpProxyPoint = new IPEndPoint(ipAddress, useASmpProxy ? SmpProxyAPort : SmpProxyBPort);
		}
	}
}
