using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Lexplosion.Logic.Network.WebSockets
{
    /// <summary>
    /// Сервер Веб-сокетов, который обрабатывеат команды. 
    /// Работает подобно html серверам - принял запрос, получил данные, дал ответ, закрыл соединение.
    /// </summary>
    class WebSocketServer : WebSocket
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        private const string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public delegate string ReceivedDataDelegate(string text);

        private bool _isClosed = false;

        /// <summary>
        /// Принимает строку - полученные данные. Возвращает тоже строку - данные которые нужно отправить.
        /// </summary>
        public event ReceivedDataDelegate ReceivedData;

        public bool Run(int port)
        {
            try
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                serverSocket.Listen(1);
                serverSocket.BeginAccept(null, 0, OnAccept, null);

                return true;
            }
            catch 
            {
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                _isClosed = true;
                serverSocket.Close();
                serverSocket.Dispose();
            }
            catch { }
        }

        private void OnAccept(IAsyncResult result)
        {
            Socket client = null;
            byte[] buffer = new byte[1024 * 1024];

            try
            {
                string headerResponse = "";
                if (serverSocket != null && serverSocket.IsBound)
                {
                    client = serverSocket.EndAccept(result);
                    var i = client.Receive(buffer);
                    headerResponse = (System.Text.Encoding.UTF8.GetString(buffer)).Substring(0, i);
                }

                if (client != null)
                {
                    var key = headerResponse.Replace("ey:", "`").Split('`')[1].Replace("\r", "").Split('\n')[0].Trim();
                    var acceptKey = AcceptKey(ref key);
                    var newLine = "\r\n";
                    var response = "HTTP/1.1 101 Switching Protocols" + newLine
                         + "Upgrade: websocket" + newLine
                         + "Connection: Upgrade" + newLine
                         + "Sec-WebSocket-Accept: " + acceptKey + newLine + newLine;

                    client.Send(System.Text.Encoding.UTF8.GetBytes(response));

                    int byteCount = client.Receive(buffer);
                    if (byteCount > 0)
                    {
                        byte[] decodedFrame = DecodeFrame(buffer);
                        if (decodedFrame != null)
                        {
                            string text = Encoding.UTF8.GetString(decodedFrame);
                            string data = ReceivedData?.Invoke(text);
                            if (data != null)
                            {
                                client.Send(EncodeFrame(Encoding.UTF8.GetBytes(data)));
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (serverSocket != null && serverSocket.IsBound)
                {
                    if (!_isClosed)
                    {
                        try
                        {
                            serverSocket.BeginAccept(null, 0, OnAccept, null);
                        }
                        catch { }
                    }
                    else
                    {
                        client?.Close();
                    }
                }
            }

            if (client != null)
            {
                try
                {
                    client.Send(new byte[2] { 136, 0 });
                    client.Close();
                }
                catch { }
            }
        }

        private string AcceptKey(ref string key)
        {
            string longKey = key + guid;
            byte[] hashBytes = ComputeHash(longKey);
            return Convert.ToBase64String(hashBytes);
        }

        SHA1 sha1 = SHA1CryptoServiceProvider.Create();
        private byte[] ComputeHash(string str)
        {
            return sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
        }
    }
}
