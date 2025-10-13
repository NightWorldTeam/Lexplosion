using System.Net;
using System.Net.Sockets;

namespace Lexplosion.Tools
{
    static class SocketExtensions
    {
        public static bool TcpPortIsAvailable(int port)
        {
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    socket.Close();

                    return true;
                }

            }
            catch
            {
                return false;
            }
        }
    }
}
