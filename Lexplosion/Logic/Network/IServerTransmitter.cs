using System.Net;

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
