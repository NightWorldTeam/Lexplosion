using System.Linq;
using System;
using System.Net;
using System.Net.Sockets;

namespace Lexplosion.Logic.Network
{
	struct ControlServerData
	{
		public IPEndPoint HandshakeServerPoint;
		public IPEndPoint TurnPoint;
		public IPEndPoint SmpProxyPoint;

		public readonly (string, int)[] StunServers = new (string, int)[]
		{
			new ("stun.l.google.com", 19305),
			new ("stun.night-world.org", 3478),
			new ("stun.webcalldirect.com", 3478)
		};

		public ControlServerData(string serverAddr)
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

			HandshakeServerPoint = new IPEndPoint(ipAddress, 4565);
			TurnPoint = new IPEndPoint(ipAddress, 9765);
			SmpProxyPoint = new IPEndPoint(ipAddress, 4729);
		}
	}
}
