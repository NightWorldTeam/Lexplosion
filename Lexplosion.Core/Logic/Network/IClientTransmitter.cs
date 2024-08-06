using System;
using System.Net;

namespace Lexplosion.Logic.Network
{
    public delegate void PointHandle(IPEndPoint point);

    interface IClientTransmitter
    {
        void Send(byte[] inputData);

        bool Receive(out byte[] data);

        event PointHandle ClientClosing;

        bool IsConnected { get; }

        void Close();
    }

}
