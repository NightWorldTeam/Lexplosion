using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network
{
    struct ControlServerData
    {
        public IPEndPoint HandshakeServerPoint;
        public IPEndPoint TurnPoint;
        public IPEndPoint SmpProxyPoint;

        public ControlServerData(string serverIp)
        {
            HandshakeServerPoint = new IPEndPoint(IPAddress.Parse(serverIp), 4565);
            TurnPoint = new IPEndPoint(IPAddress.Parse(serverIp), 9765);
            SmpProxyPoint = new IPEndPoint(IPAddress.Parse(serverIp), 4719);
        }
    }
}
