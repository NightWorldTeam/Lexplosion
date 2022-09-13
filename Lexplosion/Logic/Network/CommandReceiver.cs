using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    static class CommandReceiver
    {
        #region Events

        private static Action<string> _openModpackPage;
        public static event Action<string> OpenModpackPage
        {
            add => _openModpackPage += value;
            remove => _openModpackPage -= value;
        }

        private static Action<string> _microsoftAuthPassed;
        public static event Action<string> MicrosoftAuthPassed
        {
            add => _microsoftAuthPassed += value;
            remove => _microsoftAuthPassed -= value;
        }

        #endregion

        public static void StartCommandServer()
        {
            if (HttpListener.IsSupported)
            {
                Lexplosion.Run.TaskRun(delegate ()
                {
                    var ws = new WebSocketServer();
                    ws.Run();
                });
            }
        }

        class WebSocketServer
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            private const string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

            public void Run()
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, 54352));
                serverSocket.Listen(1);
                serverSocket.BeginAccept(null, 0, OnAccept, null);
            }

            private void OnAccept(IAsyncResult result)
            {
                Socket client = null;
                byte[] buffer = new byte[1024*1024];

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
                        var test1 = AcceptKey(ref key);
                        var newLine = "\r\n";
                        var response = "HTTP/1.1 101 Switching Protocols" + newLine
                             + "Upgrade: websocket" + newLine
                             + "Connection: Upgrade" + newLine
                             + "Sec-WebSocket-Accept: " + test1 + newLine + newLine;

                        client.Send(System.Text.Encoding.UTF8.GetBytes(response));

                        int byteCount = client.Receive(buffer);

                        byte[] decodedFrame = DecodeFrame(buffer);
                        if (decodedFrame != null)
                        {
                            string text = Encoding.UTF8.GetString(decodedFrame);

                            if (text.Contains("$openModpackPage:"))
                            {
                                if (_openModpackPage != null)
                                {
                                    string modpackId = text.Replace("$openModpackPage:", "");
                                    _openModpackPage(modpackId);
                                    client.Send(EncodeFrame(Encoding.UTF8.GetBytes("OK")));
                                }
                                else
                                {
                                    client.Send(EncodeFrame(Encoding.UTF8.GetBytes("NO_AUTH")));
                                }
                            }
                            else if (text.Contains("$microsoftAuth:"))
                            {
                                string data = text.Replace("$microsoftAuth:", "");
                                _microsoftAuthPassed.Invoke(data);
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    if (serverSocket != null && serverSocket.IsBound)
                    {
                        serverSocket.BeginAccept(null, 0, OnAccept, null);
                    }
                }

                if (client != null)
                {
                    client.Send(new byte[2] { 136, 0 });
                    client.Close();
                }
            }

            private byte[] EncodeFrame(byte[] payload)
            {
                byte[] data = new byte[payload.Length + 2];
                data[0] = 129;
                data[1] = (byte)payload.Length;
                Array.Copy(payload, 0, data, 2, payload.Length);

                return data;
            }

            private byte[] DecodeFrame(byte[] frame)
            {
                //try
                {
                    bool fin = (frame[0] & 0b10000000) != 0,
                    mask = (frame[1] & 0b10000000) != 0;

                    int opcode = frame[0] & 0b00001111,
                        msglen = frame[1] - 128,
                        offset = 2;

                    if (msglen == 126)
                    {
                        msglen = BitConverter.ToUInt16(new byte[] { frame[3], frame[2] }, 0);
                        offset = 4;
                    }
                    else if (msglen == 127)
                    {
                        return null;
                    }

                    if (mask && msglen != 0)
                    {
                        byte[] decoded = new byte[msglen];
                        byte[] masks = new byte[4] { frame[offset], frame[offset + 1], frame[offset + 2], frame[offset + 3] };
                        offset += 4;

                        for (int i = 0; i < msglen; ++i)
                            decoded[i] = (byte)(frame[offset + i] ^ masks[i % 4]);

                        return decoded;
                    }
                    else
                    {
                        return null;
                    }
                }
                //catch
                //{
                //    return null;
                //}
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
}
