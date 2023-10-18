using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lexplosion.Logic.Network.TURN
{
    class TurnBridgeClient : IClientTransmitter
    {
        private Socket _socket;
        private IPEndPoint _serverPoint;

        private byte[] _selfTurnId;
        private char _groupPrefix;

        /// <param name="uuid">UUID с которым мы подключаемся к серверу. Не должен быть больше 32-х символов.</param>
        /// <param name="turnGroup">Этот символ будет вставлен перед uuid при подключении к серверу.
        /// Он описывает группу, к которой относится это подключение.
        /// </param>
        public TurnBridgeClient(string uuid, char turnGroup, IPEndPoint controlServerPoint)
        {
            _selfTurnId = Encoding.UTF8.GetBytes(turnGroup + uuid);
            _groupPrefix = turnGroup;

            _serverPoint = controlServerPoint;
        }

        /// <summary>
        /// Выполняет соединение с хостом.
        /// </summary>
        /// <param name="hostUUID">UUID хоста. не должен быть больше 32-х символов.</param>
        /// <returns></returns>
        public bool Connect(string hostUUID)
        {
            byte[] data = new byte[66];
            byte[] bhostUUID = Encoding.UTF8.GetBytes(_groupPrefix + hostUUID);

            Buffer.BlockCopy(_selfTurnId, 0, data, 0, _selfTurnId.Length);
            Buffer.BlockCopy(bhostUUID, 0, data, 33, bhostUUID.Length);

            Runtime.DebugConsoleWrite(Encoding.UTF8.GetString(data));

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(_serverPoint);
            _socket.Send(data);

            Runtime.DebugConsoleWrite("CONNECTED FDHSGFHDFH");
            return true;
        }

        public void Send(byte[] inputData)
        {
            _socket.Send(inputData);
        }

        public bool Receive(out byte[] data)
        {
            try
            {
                _socket.Poll(-1, SelectMode.SelectRead);
                data = new byte[_socket.Available];
                _socket.Receive(data);

                return true;
            }
            catch (Exception e)
            {
                Runtime.DebugConsoleWrite("Turn Receive exception " + e);
                data = new byte[0];
                return false;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _socket.Connected;
            }
        }

        public void Close()
        {
            _socket.Close();
        }

        public event PointHandle ClientClosing;
    }
}
