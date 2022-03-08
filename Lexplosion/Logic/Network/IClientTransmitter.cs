using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network
{
    interface IClientTransmitter
    {
        void Send(byte[] inputData);

        bool Receive(out byte[] data);

        event PointHandle ClientClosing;

        bool IsConnected { get; }

        void Close();
    }

}
