using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lexplosion.Logic.Network.WebSockets
{
    /// <summary>
    /// Клиент Веб-сокетов. Эта хуяня нужна только чтобы отправлять данные на WebSocketServer.
    /// Устноавить нормальное вебсокет соединение особо не выйдет, ведь они оба рботают подобно html соединению.
    /// </summary>
    class WebSocketClient : WebSocket
    {
        private Socket _socket;
        private IPEndPoint _host;

        public WebSocketClient(IPEndPoint host)
        {
            _host = host;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect()
        {
            try
            {
                string headers =
                    "GET / HTTP/1.1\r\n" +
                    "Host: " + _host.ToString() + "\r\n" +
                    "Connection: Upgrade\r\n" +
                    "Pragma: no-cache\r\n" +
                    "Cache-Control: no-cache\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Sec-WebSocket-Version: 13\r\n" +
                    "User-Agent: Lexplosion\r\n" +
                    "Accept-Encoding: gzip, deflate, br\r\n" +
                    "Accept-Language: ru-RU,ru;q=0.9\r\n" +
                    "Sec-GPC: 1\r\n" +
                    "Sec-WebSocket-Key: ujaTpK1TTDj3L9lkzFagyA==\r\n" +
                    "Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits\r\n\r\n";

                _socket.Connect(_host);

                byte[] bytes = Encoding.UTF8.GetBytes(headers);
                _socket.Send(bytes);

                byte[] buffer = new byte[10];
                byte flag = 0;
                do
                {
                    // нам нужно /r/n/r/n что бы понять, что заголовки закончились и дальше пойдут данные
                    int lenght = _socket.Receive(buffer);
                    if (lenght > 0)
                    {
                        for (int i = 0; i < lenght; i++)
                        {
                            if ((buffer[i] == 0xd && (flag == 0 || flag == 2)) || (buffer[i] == 0xa && (flag == 1 || flag == 3)))
                            {
                                flag++;
                            }
                            else
                            {
                                flag = 0;
                            }

                            if (flag == 4)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                while (_socket.Available > 0);

            }
            catch { }

            return false;
        }

        public void SendData(string data)
        {
            try
            {
                if (Connect())
                {
                    byte[] frame = EncodeFrame(Encoding.UTF8.GetBytes(data));
                    _socket.Send(frame);

                    _socket.ReceiveTimeout = 5000;
                    byte[] _ = new byte[1];
                    _socket.Receive(_);
                    _socket.Close();
                }
            }
            catch { }

        }
    }
}
