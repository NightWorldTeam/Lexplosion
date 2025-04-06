using System.Net;

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
			new ("79.174.92.100", 3478),
			new ("stun.webcalldirect.com", 3478)
		};

		public ControlServerData(string serverIp)
		{
			HandshakeServerPoint = new IPEndPoint(IPAddress.Parse(serverIp), 4565);
			TurnPoint = new IPEndPoint(IPAddress.Parse(serverIp), 9765);
			SmpProxyPoint = new IPEndPoint(IPAddress.Parse(serverIp), 4729);
		}
	}
}
