using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network
{
    public delegate void PointHandle(IPEndPoint ip);

    interface IServerTransmitter
    {
        IPEndPoint Receive(out byte[] data);
        void Send(byte[] inputData, IPEndPoint ip);

        event PointHandle ClientClosing;

        void StopWork();

        bool Close(IPEndPoint point);
    }

}
