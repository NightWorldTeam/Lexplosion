using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lexplosion.Logic.Network.TURN
{
    class TurnBridgeClient : IClientTransmitter
    {
        private Socket socket;

        public bool Connect(string selfUUID, string hostUUID)
        {
            byte[] data = new byte[64];
            byte[] bselfUUID = Encoding.UTF8.GetBytes(selfUUID);
            byte[] bhostUUID = Encoding.UTF8.GetBytes(hostUUID);

            for (int i = 0; i < bselfUUID.Length; i++)
            {
                data[i] = bselfUUID[i];
            }

            for (int i = 0; i < bhostUUID.Length; i++)
            {
                data[i + 32] = bhostUUID[i];
            }

            Runtime.DebugWrite(Encoding.UTF8.GetString(data));

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse("194.61.2.176"), 9765));
            socket.Send(data);

            Runtime.DebugWrite("CONNECTED FDHSGFHDFH");
            return true;
        }

        public void Send(byte[] inputData)
        {
            socket.Send(inputData);
        }

        public bool Receive(out byte[] data)
        {
            try
            {
                socket.Poll(-1, SelectMode.SelectRead);
                data = new byte[socket.Available];
                socket.Receive(data);

                return true;
            }
            catch
            {
                data = new byte[0];
                return false;
            }
        }

        public bool IsConnected
        {
            get
            {
                return socket.Connected;
            }
        }

        public void Close()
        {
            socket.Close();
        }

        public event PointHandle ClientClosing;
    }
}
